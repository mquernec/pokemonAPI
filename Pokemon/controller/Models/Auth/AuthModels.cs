using System.ComponentModel.DataAnnotations;

namespace controller.Models.Auth
{
    /// <summary>
    /// Modèle pour la requête de connexion
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Le nom d'utilisateur doit contenir entre 3 et 50 caractères")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modèle pour la requête d'inscription
    /// </summary>
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Le nom d'utilisateur doit contenir entre 3 et 50 caractères")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
        [Compare("Password", ErrorMessage = "La confirmation du mot de passe ne correspond pas")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modèle pour la réponse d'authentification
    /// </summary>
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public User User { get; set; } = new User();
    }

    /// <summary>
    /// Modèle utilisateur pour l'authentification
    /// </summary>
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Crée une copie de l'utilisateur sans le hash du mot de passe (pour la sécurité)
        /// </summary>
        /// <returns>Clone de l'utilisateur sans données sensibles</returns>
        public User Clone()
        {
            return new User
            {
                Id = this.Id,
                Username = this.Username,
                Email = this.Email,
                Role = this.Role,
                CreatedAt = this.CreatedAt,
                LastLoginAt = this.LastLoginAt,
                IsActive = this.IsActive,
                PasswordHash = "" // On ne copie jamais le hash du mot de passe
            };
        }

        /// <summary>
        /// Crée une version publique de l'utilisateur (sans données sensibles)
        /// </summary>
        /// <returns>Version publique de l'utilisateur</returns>
        public object ToPublic()
        {
            return new
            {
                Id = this.Id,
                Username = this.Username,
                Email = this.Email,
                Role = this.Role,
                CreatedAt = this.CreatedAt,
                LastLoginAt = this.LastLoginAt,
                IsActive = this.IsActive
            };
        }
    }

    /// <summary>
    /// Configuration JWT depuis appsettings.json
    /// </summary>
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryInMinutes { get; set; } = 60;
        public int RefreshTokenExpiryInDays { get; set; } = 7;
    }
}
