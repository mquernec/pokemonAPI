using controller.Models.Auth;

namespace controller.Services.Auth
{
    /// <summary>
    /// Interface pour le service de gestion des tokens JWT
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Génère un token JWT pour un utilisateur
        /// </summary>
        /// <param name="user">Utilisateur pour lequel générer le token</param>
        /// <returns>Token JWT</returns>
        string GenerateToken(User user);

        /// <summary>
        /// Valide un token JWT
        /// </summary>
        /// <param name="token">Token à valider</param>
        /// <returns>Claims principal si valide, null sinon</returns>
        System.Security.Claims.ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Extrait l'ID utilisateur d'un token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>ID utilisateur ou null si invalide</returns>
        string? GetUserIdFromToken(string token);

        /// <summary>
        /// Extrait le nom d'utilisateur d'un token JWT
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>Nom d'utilisateur ou null si invalide</returns>
        string? GetUsernameFromToken(string token);

        /// <summary>
        /// Obtient la durée d'expiration des tokens en minutes
        /// </summary>
        /// <returns>Durée en minutes</returns>
        int GetTokenExpiryMinutes();

        /// <summary>
        /// Vérifie si un token est expiré
        /// </summary>
        /// <param name="token">Token à vérifier</param>
        /// <returns>True si expiré, false sinon</returns>
        bool IsTokenExpired(string token);
    }
}
