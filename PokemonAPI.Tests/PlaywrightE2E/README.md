# Installation et configuration des tests Playwright E2E

## Prérequis

1. .NET 9 SDK installé
2. API Pokemon en cours d'exécution sur http://localhost:5183

## Installation de Playwright

```bash
# Restaurer les packages NuGet
dotnet restore

# Installer les navigateurs Playwright
pwsh bin/Debug/net9.0/playwright.ps1 install

# Ou sur Linux/Mac
./bin/Debug/net9.0/playwright.sh install
```

## Exécution des tests E2E

### Tous les tests Playwright
```bash
dotnet test --filter "TestCategory=E2E"
```

### Tests d'authentification uniquement
```bash
dotnet test --filter "TestCategory=Authentication"
```

### Tests d'intégration complets
```bash
dotnet test --filter "TestCategory=Integration"
```

### Avec verbosité pour le débogage
```bash
dotnet test --filter "TestCategory=E2E" --logger "console;verbosity=detailed"
```

## Configuration

- **Base URL**: http://localhost:5183
- **Timeout**: 30 secondes
- **Retry**: 2 tentatives en cas d'échec
- **Screenshots**: Uniquement en cas d'échec
- **Videos**: Conservées en cas d'échec

## Tests disponibles

### AuthenticationE2ETests
- `Register_WithValidCredentials_ShouldSucceed`: Test d'enregistrement
- `Login_WithAdminCredentials_ShouldReturnToken`: Test de connexion admin
- `Login_WithTestUserCredentials_ShouldReturnToken`: Test de connexion utilisateur
- `ValidateAuth_WithValidToken_ShouldSucceed`: Test de validation de token
- `AccessProtectedResource_WithValidToken_ShouldSucceed`: Test d'accès ressource protégée
- `AccessProtectedResource_WithoutToken_ShouldFail`: Test d'accès sans token
- `AccessProtectedResource_WithInvalidToken_ShouldFail`: Test d'accès token invalide
- `Login_WithInvalidCredentials_ShouldFail`: Test de connexion échouée
- `CompleteAuthFlow_RegisterLoginValidateAccess_ShouldSucceed`: Test de flux complet
- `Authentication_ShouldRespondQuickly`: Test de performance

## Correspondance avec Auth.http

| Test Playwright | Requête Auth.http |
|-----------------|-------------------|
| `Register_WithValidCredentials_ShouldSucceed` | `POST /api/auth/register` |
| `Login_WithAdminCredentials_ShouldReturnToken` | `POST /api/auth/login` |
| `ValidateAuth_WithValidToken_ShouldSucceed` | `GET /api/auth/validate` |
| `AccessProtectedResource_WithValidToken_ShouldSucceed` | `GET /api/Trainer` |

## Débogage

Pour déboguer les tests Playwright :

1. Activer le mode headed :
```bash
$env:HEADED = "true"
dotnet test --filter "TestCategory=E2E"
```

2. Ralentir l'exécution :
```bash
$env:SLOWMO = "1000"
dotnet test --filter "TestCategory=E2E"
```

3. Voir les traces :
```bash
pwsh bin/Debug/net9.0/playwright.ps1 show-trace test-results/trace.zip
```

## Résolution des problèmes

### API non accessible
Si vous obtenez l'erreur "API is not accessible", vérifiez que :
1. L'API Pokemon fonctionne sur http://localhost:5183
2. Les endpoints d'authentification sont configurés
3. Aucun firewall ne bloque la connexion

### Tests de timeout
Si les tests échouent en timeout :
1. Augmentez le timeout dans playwright.config.json
2. Vérifiez la performance de l'API
3. Assurez-vous que la base de données répond correctement
