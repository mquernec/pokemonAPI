# Tests End-to-End Playwright pour l'API Pokemon

Ce dossier contient les tests end-to-end utilisant Playwright pour valider les scénarios d'authentification de l'API Pokemon.

## Structure

- `PlaywrightTestBase.cs` : Classe de base fournissant des utilitaires pour les tests Playwright
- `AuthenticationE2ETests.cs` : Tests d'authentification reproduisant les scénarios du fichier `Auth.http`
- `playwright.config.json` : Configuration Playwright
- `README.md` : Cette documentation

## Prérequis

1. **API Pokemon en cours d'exécution** : L'API doit être démarrée sur `http://localhost:5183`
2. **Playwright installé** : Le package `Microsoft.Playwright.MSTest` est inclus dans le projet
3. **Navigateurs Playwright** : Installer les navigateurs avec `pwsh bin/Debug/net9.0/playwright.ps1 install`

## Installation des navigateurs Playwright

Après avoir compilé le projet, exécutez cette commande depuis le dossier du projet de tests :

```powershell
# Depuis d:\pokemonAPI\Test\
pwsh bin/Debug/net9.0/playwright.ps1 install
```

## Exécution des tests

### Démarrer l'API

Assurez-vous que l'API Pokemon est en cours d'exécution :

```powershell
# Depuis d:\pokemonAPI\Pokemon\controller\
dotnet run
```

L'API doit être accessible sur `http://localhost:5183`

### Exécuter tous les tests E2E

```powershell
# Depuis d:\pokemonAPI\Test\
dotnet test --filter "TestCategory=E2E"
```

### Exécuter les tests d'authentification spécifiquement

```powershell
# Depuis d:\pokemonAPI\Test\
dotnet test --filter "FullyQualifiedName~AuthenticationE2ETests"
```

### Exécuter un test spécifique

```powershell
# Exemple pour le test de login
dotnet test --filter "TestMethod=Login_WithValidCredentials_ShouldReturnTokenAndUserInfo"
```

## Tests inclus

Les tests reproduisent exactement les scénarios du fichier `Auth.http` :

1. **Enregistrement d'utilisateur** (`POST /api/auth/register`)
   - Teste l'enregistrement avec des données valides
   - Gère le cas où l'utilisateur existe déjà (409)

2. **Connexion** (`POST /api/auth/login`)
   - Teste la connexion avec des identifiants valides (admin/admin123)
   - Teste la connexion avec des identifiants invalides
   - Valide la structure du token JWT retourné

3. **Validation de token** (`GET /api/auth/validate`)
   - Teste la validation avec un token valide
   - Teste la validation sans token (401)
   - Teste la validation avec un token invalide (401)

4. **Accès à une ressource protégée** (`GET /api/Trainer`)
   - Teste l'accès avec un token valide
   - Teste l'accès sans token (401)

5. **Flux complet d'authentification**
   - Teste l'ensemble du workflow : enregistrement → connexion → validation → accès ressource

## Configuration

### Variables d'environnement supportées

Vous pouvez surcharger la configuration via les variables d'environnement :

- `POKEMON_API_BASE_URL` : URL de base de l'API (défaut: `http://localhost:5183`)
- `TEST_ADMIN_USERNAME` : Nom d'utilisateur admin (défaut: `admin`)
- `TEST_ADMIN_PASSWORD` : Mot de passe admin (défaut: `admin123`)

### Utilisateurs de test

Les tests utilisent ces comptes par défaut :

- **Admin** : `admin` / `admin123` (doit exister dans l'API)
- **Test User** : `testuser` / `Test@123` / `testuser@example.com` (créé lors des tests)

## Résolution des problèmes

### L'API n'est pas accessible

```
System.Net.Http.HttpRequestException: Connection refused
```

**Solution** : Vérifiez que l'API est démarrée sur le bon port :
```powershell
cd d:\pokemonAPI\Pokemon\controller
dotnet run
```

### Navigateurs Playwright manquants

```
Playwright browser not found
```

**Solution** : Installez les navigateurs :
```powershell
pwsh bin/Debug/net9.0/playwright.ps1 install
```

### Tests qui échouent sur l'authentification

**Solution** : Vérifiez que l'utilisateur `admin` avec le mot de passe `admin123` existe dans votre base de données.

### Timeouts

Si les tests sont lents, vous pouvez ajuster les timeouts dans `playwright.config.json` ou via les variables d'environnement.

## Rapports

Les rapports HTML sont générés dans le dossier `playwright-report/` après l'exécution des tests.

Pour ouvrir le rapport :
```powershell
# Depuis d:\pokemonAPI\Test\
start playwright-report/index.html
```

## Intégration Continue

Ces tests peuvent être intégrés dans une pipeline CI/CD. Assurez-vous que :

1. L'API est démarrée avant les tests
2. Les navigateurs Playwright sont installés dans l'environnement CI
3. Les variables d'environnement sont configurées si nécessaire
