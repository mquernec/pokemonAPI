using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using controller.Models.Auth;
using Microsoft.Extensions.Options;

namespace controller.Services.Auth
{
    /// <summary>
    /// Service pour la gestion des tokens JWT
    /// Implémente les bonnes pratiques de sécurité pour la génération et validation des tokens
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtService> _logger;
        private readonly byte[] _key;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
        {
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Validation de la clé secrète
            if (string.IsNullOrEmpty(_jwtSettings.SecretKey) || _jwtSettings.SecretKey.Length < 32)
            {
                throw new InvalidOperationException("La clé secrète JWT doit contenir au moins 32 caractères");
            }

            _key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            
            // Configuration des paramètres de validation du token
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ClockSkew = TimeSpan.Zero // Pas de tolérance de temps
            };

            _logger.LogInformation("Service JWT initialisé avec succès");
        }

        /// <summary>
        /// Génère un token JWT pour un utilisateur
        /// </summary>
        public string GenerateToken(User user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(_key);
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Claims standards et personnalisés
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new(ClaimTypes.Name, user.Username),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Role, user.Role),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Token ID unique
                    new(JwtRegisteredClaimNames.Iat, 
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                        ClaimValueTypes.Integer64), // Issued at
                    new(JwtRegisteredClaimNames.Sub, user.Id) // Subject
                };

                var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = expiration,
                    Issuer = _jwtSettings.Issuer,
                    Audience = _jwtSettings.Audience,
                    SigningCredentials = credentials,
                    NotBefore = DateTime.UtcNow // Token valide immédiatement
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("Token JWT généré pour l'utilisateur {Username} (ID: {UserId})", 
                    user.Username, user.Id);

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération du token JWT pour l'utilisateur {Username}", 
                    user?.Username ?? "Unknown");
                throw new InvalidOperationException("Erreur lors de la génération du token", ex);
            }
        }

        /// <summary>
        /// Valide un token JWT et retourne les claims
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Tentative de validation d'un token vide ou null");
                    return null;
                }

                var tokenHandler = new JwtSecurityTokenHandler();

                // Vérification du format du token
                if (!tokenHandler.CanReadToken(token))
                {
                    _logger.LogWarning("Format de token JWT invalide");
                    return null;
                }

                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out SecurityToken validatedToken);

                // Vérification supplémentaire du type de token
                if (validatedToken is not JwtSecurityToken jwtToken || 
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("Algorithme de signature du token invalide");
                    return null;
                }

                var username = principal.FindFirst(ClaimTypes.Name)?.Value;
                _logger.LogDebug("Token JWT validé avec succès pour l'utilisateur: {Username}", username);

                return principal;
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning("Token JWT expiré: {Message}", ex.Message);
                return null;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning("Signature du token JWT invalide: {Message}", ex.Message);
                return null;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("Token JWT invalide: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de la validation du token JWT");
                return null;
            }
        }

        /// <summary>
        /// Extrait l'ID utilisateur d'un token JWT
        /// </summary>
        public string? GetUserIdFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'extraction de l'ID utilisateur du token");
                return null;
            }
        }

        /// <summary>
        /// Extrait le nom d'utilisateur d'un token JWT
        /// </summary>
        public string? GetUsernameFromToken(string token)
        {
            try
            {
                var principal = ValidateToken(token);
                return principal?.FindFirst(ClaimTypes.Name)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'extraction du nom d'utilisateur du token");
                return null;
            }
        }

        /// <summary>
        /// Obtient la durée d'expiration des tokens en minutes
        /// </summary>
        public int GetTokenExpiryMinutes()
        {
            return _jwtSettings.ExpiryInMinutes;
        }

        /// <summary>
        /// Vérifie si un token est expiré
        /// </summary>
        public bool IsTokenExpired(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return true;

                var tokenHandler = new JwtSecurityTokenHandler();
                
                if (!tokenHandler.CanReadToken(token))
                    return true;

                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo <= DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de l'expiration du token");
                return true; // En cas d'erreur, considérer comme expiré pour la sécurité
            }
        }
    }
}
