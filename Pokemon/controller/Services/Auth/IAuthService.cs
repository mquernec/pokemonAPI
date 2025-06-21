using controller.Models.Auth;

namespace controller.Services.Auth
{
    /// <summary>
    /// Interface pour le service d'authentification
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Enregistre un nouvel utilisateur
        /// </summary>
        /// <param name="request">Données d'enregistrement</param>
        /// <returns>Réponse d'authentification avec token</returns>
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Authentifie un utilisateur
        /// </summary>
        /// <param name="request">Données de connexion</param>
        /// <returns>Réponse d'authentification avec token</returns>
        Task<AuthResponse?> LoginAsync(LoginRequest request);

        /// <summary>
        /// Vérifie si un utilisateur existe
        /// </summary>
        /// <param name="username">Nom d'utilisateur</param>
        /// <returns>True si l'utilisateur existe</returns>
        Task<bool> UserExistsAsync(string username);

        /// <summary>
        /// Obtient un utilisateur par son nom d'utilisateur
        /// </summary>
        /// <param name="username">Nom d'utilisateur</param>
        /// <returns>Utilisateur ou null si non trouvé</returns>
        Task<User?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Obtient un utilisateur par son ID
        /// </summary>
        /// <param name="userId">ID de l'utilisateur</param>
        /// <returns>Utilisateur ou null si non trouvé</returns>
        Task<User?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Valide les informations d'identification d'un utilisateur
        /// </summary>
        /// <param name="username">Nom d'utilisateur</param>
        /// <param name="password">Mot de passe</param>
        /// <returns>True si les informations sont valides</returns>
        Task<bool> ValidateCredentialsAsync(string username, string password);

        /// <summary>
        /// Change le mot de passe d'un utilisateur
        /// </summary>
        /// <param name="userId">ID de l'utilisateur</param>
        /// <param name="oldPassword">Ancien mot de passe</param>
        /// <param name="newPassword">Nouveau mot de passe</param>
        /// <returns>True si le changement a réussi</returns>
        Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword);

        /// <summary>
        /// Met à jour le rôle d'un utilisateur
        /// </summary>
        /// <param name="userId">ID de l'utilisateur</param>
        /// <param name="newRole">Nouveau rôle</param>
        /// <returns>True si la mise à jour a réussi</returns>
        Task<bool> UpdateUserRoleAsync(string userId, string newRole);

        /// <summary>
        /// Obtient la liste de tous les utilisateurs (pour les administrateurs)
        /// </summary>
        /// <returns>Liste des utilisateurs</returns>
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}
