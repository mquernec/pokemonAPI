# Guide d'Utilisation des Tests - API Pokemon

## 🚀 Démarrage Rapide

### Installation
```bash
cd d:\pokemonAPI\PokemonAPI.Tests
dotnet restore
```

### Exécution de Base
```bash
# Tous les tests
dotnet test

# Tests rapides (sans performance)
.\RunTests.ps1 Fast

# Tests avec couverture
.\RunTests.ps1 All -Coverage
```

## 📋 Types de Tests Disponibles

### Tests par Composant
- **Pokemon** : Tests des APIs Pokemon (CRUD, recherche, évolution)
- **Trainer** : Tests des APIs Dresseur (gestion équipe, combats)
- **Battle** : Tests des APIs Combat (création, résultats, historique)

### Tests par Niveau
- **Unit** : Tests unitaires des services avec mocks
- **Integration** : Tests d'intégration des contrôleurs
- **E2E** : Tests de bout en bout avec scénarios complets
- **Performance** : Tests de charge et de temps de réponse

### Tests par Type d'API
- **Minimal** : Tests des APIs minimales
- **Controllers** : Tests des contrôleurs MVC

## 🎯 Scénarios de Test Couverts

### 1. CRUD Complet
```
✅ Créer un Pokemon/Dresseur/Combat
✅ Lire par ID, nom, critères de recherche
✅ Mettre à jour les propriétés
✅ Supprimer des entités
```

### 2. Logique Métier
```
✅ Évolution Pokemon (level-up, changement capacité)
✅ Gestion équipe dresseur (limite 6 Pokemon)
✅ Combat entre dresseurs
✅ Validation des données (longueur nom, âge, etc.)
```

### 3. Recherche et Filtrage
```
✅ Pokemon par type/niveau/capacité
✅ Dresseurs par région/âge
✅ Combats par localisation/date
✅ Statistiques et rapports
```

### 4. Performance et Robustesse
```
✅ Temps de réponse < 2 secondes
✅ Gestion de 50+ requêtes simultanées
✅ Tests de stress avec volume élevé
✅ Validation taille des réponses
```

## 🛠️ Commandes Utiles

### Tests Ciblés
```bash
# Tests d'un contrôleur spécifique
dotnet test --filter "PokemonControllerTests"

# Tests d'une méthode HTTP
dotnet test --filter "TestCategory=GET"

# Tests excluant la performance
dotnet test --filter "FullyQualifiedName!~Performance"
```

### Avec Scripts
```powershell
# PowerShell
.\RunTests.ps1 Pokemon -Verbose    # Tests Pokemon détaillés
.\RunTests.ps1 Performance         # Tests performance uniquement
.\RunTests.ps1 All -Coverage      # Tous tests + couverture

# Batch Windows
RunTests.bat Integration          # Tests d'intégration
RunTests.bat Fast                 # Tests rapides
```

### Diagnostic et Debug
```bash
# Logs détaillés
dotnet test --logger "console;verbosity=detailed"

# Test spécifique avec debug
dotnet test --filter "GetPokemonById_WithValidId_ShouldReturnPokemon" --logger "console;verbosity=detailed"

# Avec timeout personnalisé
dotnet test --blame-hang-timeout 5m
```

## 📊 Rapport de Couverture

### Génération
```bash
# Avec collecte de couverture
dotnet test --collect:"XPlat Code Coverage"

# Génération rapport HTML
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:Html
```

### Ouverture
Le script PowerShell ouvre automatiquement le rapport dans le navigateur.

## ⚙️ Configuration

### Paramètres de Test
Modifiez `appsettings.test.json` pour :
- URLs d'API de test
- Timeouts et seuils de performance
- Configuration d'authentification
- Paramètres de base de données de test

### Variables d'Environnement
```bash
set TEST_API_URL=https://localhost:5001
set LOG_LEVEL=Warning
set USE_IN_MEMORY_DB=true
```

## 🔧 Résolution de Problèmes

### Problèmes Courants

#### Tests qui échouent avec 401 Unauthorized
```bash
# Solution 1: Désactiver l'auth en test
# Modifier CustomWebApplicationFactory.cs

# Solution 2: Ajouter token de test
# Configurer dans appsettings.test.json
```

#### Timeouts
```bash
# Augmenter les timeouts
dotnet test --blame-hang-timeout 10m

# Ou modifier les seuils dans appsettings.test.json
```

#### Tests flaky (intermittents)
```bash
# Exécuter plusieurs fois
for i in {1..5}; do dotnet test; done

# Ou désactiver parallélisme
dotnet test --parallel:false
```

#### Problèmes de base de données
```bash
# Utiliser base en mémoire
# Vérifier configuration dans appsettings.test.json

# Ou nettoyer la base de test
# Ajouter cleanup dans TestInitialize
```

### Debug Avancé

#### Attacher le debugger
```bash
# Lancer en mode debug
dotnet test --logger "console;verbosity=detailed" --diag:TestResults/log.txt
```

#### Analyser les logs
```bash
# Vérifier les logs de test
type TestResults/log.txt | findstr "ERROR"

# Ou avec PowerShell
Get-Content TestResults/log.txt | Select-String "ERROR"
```

## 📈 Métriques de Qualité

### Objectifs de Couverture
- **Contrôleurs** : 95%+
- **Services** : 90%+
- **Modèles** : 80%+

### Standards de Performance
- **Réponse simple** : < 1 seconde
- **Recherche complexe** : < 3 secondes
- **Charge concurrente** : 50 requêtes simultanées
- **Taux de succès** : > 95% sous charge

### Critères de Validation
- Tous les endpoints testés
- Tous les codes d'erreur couverts
- Validation de toutes les entrées
- Scénarios métier complets

## 🔄 Intégration Continue

### GitHub Actions
```yaml
name: Tests API Pokemon
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - uses: actions/setup-dotnet@v3
    - run: dotnet restore
    - run: dotnet test --collect:"XPlat Code Coverage"
    - uses: codecov/codecov-action@v3
```

### Azure DevOps
```yaml
trigger:
- main

pool:
  vmImage: 'windows-latest'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Run Tests'
  inputs:
    command: 'test'
    arguments: '--collect:"XPlat Code Coverage"'
```

## 📝 Maintenance

### Ajout de Nouveaux Tests
1. Créer dans le bon dossier (`Controllers/`, `Services/`, etc.)
2. Hériter de `BaseApiTest` pour les tests d'API
3. Utiliser les modèles existants dans `Models/`
4. Documenter les nouveaux scénarios

### Mise à Jour après Changements
1. Mettre à jour les DTOs si nécessaire
2. Ajouter tests pour nouveaux endpoints
3. Modifier tests existants si contrats changent
4. Vérifier couverture et performance

### Nettoyage Périodique
```bash
# Nettoyer les résultats de test
Remove-Item TestResults -Recurse -Force

# Restaurer packages
dotnet restore

# Rebuild complet
dotnet clean && dotnet build
```

---

Ce guide devrait vous permettre d'utiliser efficacement la suite de tests pour l'API Pokemon ! 🎮
