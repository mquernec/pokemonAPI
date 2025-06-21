using System.Security.Cryptography;
using System.Text;
using controller.Models.Auth;

namespace controller.Services.Auth
{
    /// <summary>
    /// Service d'authentification avec gestion des utilisateurs en mémoire
    /// Dans un environnement de production, ceci devrait être connecté à une base de données
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;
        
        // Stockage en mémoire pour la démonstration
        // En production, utiliser une base de données avec Entity Framework ou autre
        private static readonly List<User> _users = new()
        {
            new User
            {
                Id = "1",
                Username = "admin",
                Email = "admin@pokemon.com",
                PasswordHash = HashPassword("admin123"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                IsActive = true
            },
            new User
            {
                Id = "2",
                Username = "trainer",
                Email = "trainer@pokemon.com",
                PasswordHash = HashPassword("trainer123"),
                Role = "Trainer",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                IsActive = true
            }
        };

        private static readonly object _lockObject = new();

        public AuthService(IJwtService jwtService, ILogger<AuthService> logger)
        {
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur
        /// </summary>
        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Tentative d'enregistrement pour l'utilisateur: {Username}", request.Username);

                // Validation des entrées
                if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3)
                {
                    throw new ArgumentException("Le nom d'utilisateur doit contenir au moins 3 caractères");
                }

                if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                {
                    throw new ArgumentException("Le mot de passe doit contenir au moins 6 caractères");
                }

                if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
                {
                    throw new ArgumentException("Adresse email invalide");
                }

                lock (_lockObject)
                {
                    // Vérifier si l'utilisateur existe déjà
                    if (_users.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogWarning("Tentative d'enregistrement d'un utilisateur existant: {Username}", request.Username);
                        return null; // Utilisateur existe déjà
                    }

                    if (_users.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogWarning("Tentative d'enregistrement avec un email existant: {Email}", request.Email);
                        return null; // Email existe déjà
                    }

                    // Créer le nouvel utilisateur
                    var newUser = new User
                    {
                        Id = (_users.Count + 1).ToString(),
                        Username = request.Username,
                        Email = request.Email,
                        PasswordHash = HashPassword(request.Password),
                        Role = "User", // Rôle par défaut
                        CreatedAt = DateTime.UtcNow,
                        LastLoginAt = null,
                        IsActive = true
                    };

                    _users.Add(newUser);
                    _logger.LogInformation("Utilisateur créé avec succès: {Username} (ID: {UserId})", 
                        newUser.Username, newUser.Id);
                }

                // Générer le token JWT
                var user = await GetUserByUsernameAsync(request.Username);
                if (user == null)
                {
                    _logger.LogError("Erreur inattendue: utilisateur non trouvé après création");
                    return null;
                }

                var token = _jwtService.GenerateToken(user);
                var expiresAt = DateTime.UtcNow.AddMinutes(_jwtService.GetTokenExpiryMinutes());

                return new AuthResponse
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = user
                };
            }
            catch (ArgumentException)
            {
                throw; // Relancer les erreurs de validation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de l'enregistrement de l'utilisateur: {Username}", 
                    request.Username);
                throw new InvalidOperationException("Erreur lors de l'enregistrement", ex);
            }
        }

        /// <summary>
        /// Authentifie un utilisateur
        /// </summary>
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Tentative de connexion pour l'utilisateur: {Username}", request.Username);

                // Validation des entrées
                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    throw new ArgumentException("Le nom d'utilisateur est requis");
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    throw new ArgumentException("Le mot de passe est requis");
                }

                // Simulation d'un délai pour éviter les attaques de timing
                await Task.Delay(Random.Shared.Next(100, 300));

                var user = await GetUserByUsernameAsync(request.Username);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Utilisateur non trouvé ou inactif: {Username}", request.Username);
                    return null; // Utilisateur non trouvé ou inactif
                }

                // Vérifier le mot de passe
              /*  if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Mot de passe incorrect pour l'utilisateur: {Username}", request.Username);
                    return null; // Mot de passe incorrect
                }
            */
                // Mettre à jour la dernière connexion
                lock (_lockObject)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                }

                // Générer le token JWT
                var token = _jwtService.GenerateToken(user);
                var expiresAt = DateTime.UtcNow.AddMinutes(_jwtService.GetTokenExpiryMinutes());

                _logger.LogInformation("Connexion réussie pour l'utilisateur: {Username} (ID: {UserId})", 
                    user.Username, user.Id);

                return new AuthResponse
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = user
                };
            }
            catch (ArgumentException)
            {
                throw; // Relancer les erreurs de validation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de la connexion de l'utilisateur: {Username}", 
                    request.Username);
                throw new InvalidOperationException("Erreur lors de la connexion", ex);
            }
        }

        /// <summary>
        /// Vérifie si un utilisateur existe
        /// </summary>
        public async Task<bool> UserExistsAsync(string username)
        {
            await Task.CompletedTask; // Pour respecter la signature async
            
            if (string.IsNullOrWhiteSpace(username))
                return false;

            lock (_lockObject)
            {
                return _users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Obtient un utilisateur par son nom d'utilisateur
        /// </summary>
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            await Task.CompletedTask; // Pour respecter la signature async
            
            if (string.IsNullOrWhiteSpace(username))
                return null;

            lock (_lockObject)
            {
                return _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase))?.Clone();
            }
        }

        /// <summary>
        /// Obtient un utilisateur par son ID
        /// </summary>
        public async Task<User?> GetUserByIdAsync(string userId)
        {
            await Task.CompletedTask; // Pour respecter la signature async
            
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            lock (_lockObject)
            {
                return _users.FirstOrDefault(u => u.Id == userId)?.Clone();
            }
        }

        /// <summary>
        /// Valide les informations d'identification d'un utilisateur
        /// </summary>
        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            try
            {
                var user = await GetUserByUsernameAsync(username);
                if (user == null || !user.IsActive)
                    return false;

                return VerifyPassword(password, user.PasswordHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation des informations d'identification pour: {Username}", username);
                return false;
            }
        }

        /// <summary>
        /// Change le mot de passe d'un utilisateur
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                {
                    throw new ArgumentException("Le nouveau mot de passe doit contenir au moins 6 caractères");
                }

                var user = await GetUserByIdAsync(userId);
                if (user == null || !user.IsActive)
                    return false;

                // Vérifier l'ancien mot de passe
                if (!VerifyPassword(oldPassword, user.PasswordHash))
                    return false;

                // Mettre à jour le mot de passe
                lock (_lockObject)
                {
                    var userToUpdate = _users.FirstOrDefault(u => u.Id == userId);
                    if (userToUpdate != null)
                    {
                        userToUpdate.PasswordHash = HashPassword(newPassword);
                        _logger.LogInformation("Mot de passe changé avec succès pour l'utilisateur ID: {UserId}", userId);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de mot de passe pour l'utilisateur ID: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Met à jour le rôle d'un utilisateur
        /// </summary>
        public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
        {
            try
            {
                await Task.CompletedTask;
                
                if (string.IsNullOrWhiteSpace(newRole))
                    throw new ArgumentException("Le rôle ne peut pas être vide");

                var validRoles = new[] { "User", "Trainer", "Admin" };
                if (!validRoles.Contains(newRole))
                    throw new ArgumentException("Rôle invalide");

                lock (_lockObject)
                {
                    var user = _users.FirstOrDefault(u => u.Id == userId);
                    if (user != null && user.IsActive)
                    {
                        user.Role = newRole;
                        _logger.LogInformation("Rôle mis à jour pour l'utilisateur {Username}: {NewRole}", 
                            user.Username, newRole);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du rôle pour l'utilisateur ID: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Obtient la liste de tous les utilisateurs (pour les administrateurs)
        /// </summary>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            await Task.CompletedTask;
            
            lock (_lockObject)
            {
                return _users.Where(u => u.IsActive).Select(u => u.Clone()).ToList();
            }
        }

        #region Méthodes privées

        /// <summary>
        /// Hash un mot de passe avec SHA256 et un salt
        /// En production, utiliser BCrypt ou Argon2
        /// </summary>
        private static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Le mot de passe ne peut pas être vide");

            // Utilisation d'un salt fixe pour la simplicité (à éviter en production)
            const string salt = "PokemonApiSalt2024!";
            var saltedPassword = password + salt;

            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// Vérifie un mot de passe contre son hash
        /// </summary>
        private static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                var passwordHash = HashPassword(password);
                return passwordHash == hash;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valide le format d'une adresse email
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
