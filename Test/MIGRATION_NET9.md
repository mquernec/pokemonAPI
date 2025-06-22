# Migration vers .NET 9 et ajout des tests E2E Playwright

## Résumé

Ce document décrit la migration du projet de tests vers .NET 9 et l'ajout de tests end-to-end Playwright qui reproduisent les scénarios d'authentification du fichier `Auth.http`.

## Changements effectués

### 1. Migration vers .NET 9

- **Framework cible** : Mise à jour de `net8.0` vers `net9.0`
- **Packages NuGet** : Mise à jour vers les versions compatibles .NET 9
- **Nouvelles fonctionnalités** : Ajout des packages pour les fonctionnalités .NET 9

### 2. Ajout des tests Playwright E2E

#### Structure ajoutée

```
Test/
├── PlaywrightE2E/
│   ├── PlaywrightTestBase.cs      # Classe de base pour les tests Playwright
│   ├── AuthenticationE2ETests.cs  # Tests d'authentification E2E
│   ├── playwright.config.json     # Configuration Playwright
│   └── README.md                   # Documentation des tests E2E
├── Test.csproj                     # Projet mis à jour avec .NET 9 et Playwright
└── MIGRATION_NET9.md               # Ce document
```

#### Tests d'authentification E2E

Les tests reproduisent exactement les scénarios du fichier `Auth.http` :

1. **Enregistrement d'utilisateur** (`POST /api/auth/register`)
   - Teste avec les données : `testuser` / `Test@123` / `testuser@example.com`
   - Gère le cas où l'utilisateur existe déjà (409)

2. **Connexion** (`POST /api/auth/login`)
   - Teste avec les identifiants admin : `admin` / `admin123`
   - Valide la structure du token JWT retourné
   - Teste les identifiants invalides (401)

3. **Validation de token** (`GET /api/auth/validate`)
   - Teste avec un token valide
   - Teste sans token (401)
   - Teste avec un token invalide (401)

4. **Accès à une ressource protégée** (`GET /api/Trainer`)
   - Teste l'accès avec un token valide
   - Teste l'accès sans token (401)

5. **Flux complet d'authentification**
   - Teste l'ensemble du workflow de bout en bout

#### Configuration Playwright

- **Timeouts** : 30 secondes par défaut
- **Retries** : 2 tentatives en cas d'échec
- **Rapports** : HTML et liste
- **Base URL** : `http://localhost:5183`

## Prérequis pour l'exécution

### 1. API en cours d'exécution

L'API Pokemon doit être démarrée sur `http://localhost:5183` :

```powershell
# Depuis le dossier controller
cd d:\pokemonAPI\Pokemon\controller
dotnet run
```

### 2. Navigateurs Playwright installés

Les navigateurs doivent être installés (déjà fait) :

```powershell
# Depuis d:\pokemonAPI\Test\
pwsh bin/Debug/net9.0/playwright.ps1 install
```

### 3. Utilisateur admin configuré

L'utilisateur `admin` avec le mot de passe `admin123` doit exister dans la base de données.

## Commandes d'exécution

### Compiler le projet

```powershell
cd d:\pokemonAPI\Test
dotnet build
```

### Exécuter tous les tests

```powershell
cd d:\pokemonAPI\Test
dotnet test
```

### Exécuter uniquement les tests E2E Playwright

```powershell
cd d:\pokemonAPI\Test
dotnet test --filter "FullyQualifiedName~AuthenticationE2ETests"
```

### Exécuter un test spécifique

```powershell
cd d:\pokemonAPI\Test
dotnet test --filter "TestMethod=Login_WithValidCredentials_ShouldReturnTokenAndUserInfo"
```

### Lister tous les tests disponibles

```powershell
cd d:\pokemonAPI\Test
dotnet test --list-tests
```

## Tests disponibles

1. `RegisterUser_ShouldReturnSuccess`
2. `Login_WithValidCredentials_ShouldReturnTokenAndUserInfo`
3. `Login_WithInvalidCredentials_ShouldReturnUnauthorized`
4. `ValidateToken_WithValidToken_ShouldReturnUserInfo`
5. `ValidateToken_WithoutToken_ShouldReturnUnauthorized`
6. `ValidateToken_WithInvalidToken_ShouldReturnUnauthorized`
7. `AccessProtectedResource_WithValidToken_ShouldReturnData`
8. `AccessProtectedResource_WithoutToken_ShouldReturnUnauthorized`
9. `CompleteAuthenticationFlow_ShouldWorkEndToEnd`

## Dépannage

### L'API n'est pas accessible

```
System.Net.Http.HttpRequestException: Connection refused
```

**Solution** : Vérifiez que l'API est démarrée sur le bon port.

### Navigateurs Playwright manquants

```
Playwright browser not found
```

**Solution** : Réinstallez les navigateurs :
```powershell
pwsh bin/Debug/net9.0/playwright.ps1 install
```

### Tests d'authentification qui échouent

**Solution** : Vérifiez que l'utilisateur `admin` avec le mot de passe `admin123` existe dans votre base de données.

## Avantages de cette approche

1. **Tests E2E réalistes** : Les tests reproduisent exactement les scénarios d'utilisation réelle
2. **Couverture complète** : Tous les endpoints d'authentification sont testés
3. **Validation complète** : Tests des cas de succès et d'erreur
4. **Facilité de maintenance** : Code structuré et documenté
5. **Intégration CI/CD** : Prêt pour l'intégration continue

## Migration réussie

✅ **Framework** : .NET 9  
✅ **Compilation** : Sans erreurs  
✅ **Tests Playwright** : Configurés et prêts  
✅ **Documentation** : Complète  
✅ **Navigateurs** : Installés  

La migration vers .NET 9 et l'ajout des tests E2E Playwright sont terminés avec succès.
