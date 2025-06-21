using Microsoft.AspNetCore.Mvc;
using controller.Models.Auth;
using controller.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace controller.Controllers
{
    /// <summary>
    /// Contrôleur d'authentification pour la gestion des tokens JWT
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService, 
            IJwtService jwtService,
            ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur
        /// </summary>
        /// <param name="request">Données d'enregistrement</param>
        /// <returns>Réponse d'authentification avec token</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Tentative d'enregistrement pour l'utilisateur: {Username}", request.Username);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Données d'enregistrement invalides pour: {Username}", request.Username);
                    return BadRequest(new { 
                        Message = "Données invalides", 
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
                    });
                }

                var result = await _authService.RegisterAsync(request);
                if (result == null)
                {
                    _logger.LogWarning("Échec de l'enregistrement pour l'utilisateur: {Username}", request.Username);
                    return Conflict(new { Message = "L'utilisateur existe déjà" });
                }

                _logger.LogInformation("Utilisateur enregistré avec succès: {Username}", request.Username);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erreur de validation lors de l'enregistrement: {Username}", request.Username);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de l'enregistrement: {Username}", request.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { Message = "Erreur interne du serveur" });
            }
        }

        /// <summary>
        /// Authentifie un utilisateur existant
        /// </summary>
        /// <param name="request">Données de connexion</param>
        /// <returns>Réponse d'authentification avec token</returns>
        [HttpPost("login")]
        [AllowAnonymous] 
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Tentative de connexion pour l'utilisateur: {Username}", request.Username);
                _logger.LogDebug("Données reçues: Username={Username}, Password={Password}", request.Username, request.Password);
                if (request == null)
                {
                    _logger.LogError("La requête est null");
                    return BadRequest(new { Message = "La requête est invalide" });
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Données de connexion invalides pour: {Username}", request.Username);
                    return BadRequest(new { 
                        Message = "Données invalides", 
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    _logger.LogError("Le nom d'utilisateur ou le mot de passe est manquant");
                    return BadRequest(new { Message = "Le nom d'utilisateur et le mot de passe sont requis" });
                }

                var result = await _authService.LoginAsync(request);
                if (result == null)
                {
                    _logger.LogWarning("Échec de la connexion pour l'utilisateur: {Username}", request.Username);
                    return Unauthorized(new { Message = "Nom d'utilisateur ou mot de passe incorrect" });
                }

                _logger.LogInformation("Utilisateur connecté avec succès: {Username}", request.Username);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erreur de validation lors de la connexion: {Username}", request.Username);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de la connexion: {Username}", request.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { Message = "Erreur interne du serveur" });
            }
        }

        /// <summary>
        /// Valide un token JWT (endpoint protégé pour tester l'authentification)
        /// </summary>
        /// <returns>Informations sur l'utilisateur authentifié</returns>
        [HttpGet("validate")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public IActionResult ValidateToken()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                _logger.LogInformation("Token validé pour l'utilisateur: {Username}", username);

                return Ok(new
                {
                    Message = "Token valide",
                    User = new
                    {
                        Id = userId,
                        Username = username,
                        Email = email,
                        Role = role
                    },
                    Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation du token");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { Message = "Erreur interne du serveur" });
            }
        }

        /// <summary>
        /// Refresh d'un token JWT (endpoint protégé)
        /// </summary>
        /// <returns>Nouveau token JWT</returns>
        [HttpPost("refresh")]
        [Authorize]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Informations utilisateur manquantes lors du refresh du token");
                    return Unauthorized(new { Message = "Token invalide" });
                }

                // Créer un nouvel utilisateur temporaire pour régénérer le token
                var user = new User
                {
                    Id = userId,
                    Username = username,
                    Email = email ?? "",
                    Role = role ?? "User"
                };

                var newToken = _jwtService.GenerateToken(user);
                var response = new AuthResponse
                {
                    Token = newToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtService.GetTokenExpiryMinutes()),
                    User = user
                };

                _logger.LogInformation("Token rafraîchi avec succès pour l'utilisateur: {Username}", username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du refresh du token");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { Message = "Erreur interne du serveur" });
            }
        }

        /// <summary>
        /// Obtient la liste de tous les utilisateurs (Admin uniquement)
        /// </summary>
        /// <returns>Liste des utilisateurs</returns>
        [HttpGet("users")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                var publicUsers = users.Select(u => u.ToPublic());
                
                _logger.LogInformation("Liste des utilisateurs récupérée par l'administrateur");
                return Ok(publicUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la liste des utilisateurs");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { Message = "Erreur interne du serveur" });
            }
        }

        /// <summary>
        /// Met à jour le rôle d'un utilisateur (Admin uniquement)
        /// </summary>
        /// <param name="userId">ID de l'utilisateur</param>
        /// <param name="request">Nouveau rôle</param>
        /// <returns>Résultat de la mise à jour</returns>
        [HttpPut("users/{userId}/role")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] UpdateRoleRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        Message = "Données invalides", 
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
                    });
                }

                var result = await _authService.UpdateUserRoleAsync(userId, request.NewRole);
                if (!result)
                {
                    return NotFound(new { Message = "Utilisateur non trouvé" });
                }

                _logger.LogInformation("Rôle mis à jour pour l'utilisateur {UserId}: {NewRole}", userId, request.NewRole);
                return Ok(new { Message = "Rôle mis à jour avec succès" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erreur de validation lors de la mise à jour du rôle");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du rôle pour l'utilisateur {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { Message = "Erreur interne du serveur" });            }
        }
    }
}



namespace controller.Models.Auth
{
    /// <summary>
    /// Modèle pour la mise à jour du rôle d'un utilisateur
    /// </summary>
    public class UpdateRoleRequest
    {
        [Required(ErrorMessage = "Le nouveau rôle est requis")]
        [RegularExpression("^(User|Trainer|Admin)$", ErrorMessage = "Le rôle doit être User, Trainer ou Admin")]
        public string NewRole { get; set; } = string.Empty;
    }
}
