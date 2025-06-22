# Tests de l'API Pokemon

Ce projet contient une suite complète de tests pour l'API Pokemon, couvrant tous les aspects des fonctionnalités développées.

## Structure des Tests

### 📁 Infrastructure
- **CustomWebApplicationFactory** : Factory personnalisée pour créer des instances de test de l'application
- **BaseApiTest** : Classe de base avec des méthodes utilitaires communes pour tous les tests d'API

### 📁 Models
- **TestModels** : Modèles Pokemon, Trainer, Battle pour les tests
- **RequestModels** : DTOs pour les requêtes de test (Create, Update, etc.)

### 📁 Controllers
Tests d'intégration pour tous les contrôleurs :

#### PokemonControllerTests
- ✅ Tests GET (GetAll, GetById, GetByName, GetByType, GetByLevel, GetByAbility)
- ✅ Tests POST (CreatePokemon avec validation)
- ✅ Tests PUT (UpdatePokemon)
- ✅ Tests PATCH (LevelUp, ChangeAbility)
- ✅ Tests DELETE (DeletePokemon)
- ✅ Tests de validation des données

#### TrainerControllerTests
- ✅ Tests GET (GetAll, GetById, GetByName, GetByRegion, GetByAge)
- ✅ Tests POST (CreateTrainer, AssignPokemon)
- ✅ Tests PUT (UpdateTrainer)
- ✅ Tests DELETE (DeleteTrainer, RemovePokemon)
- ✅ Tests de logique métier (limite d'équipe, etc.)

#### BattleControllerTests
- ✅ Tests GET (GetAll, GetById, GetRecent, GetByTrainer, GetStatistics)
- ✅ Tests POST (CreateBattle, StartBattle)
- ✅ Tests PATCH (DeclareWinner)
- ✅ Tests DELETE (DeleteBattle)
- ✅ Tests de validation des combats

### 📁 MinimalApi
Tests pour les APIs minimales :

#### MinimalPokemonApiTests
- ✅ Tests des endpoints minimaux (/pokemons, /pokemons/{name}, etc.)
- ✅ Tests de validation des paramètres
- ✅ Tests de performance
- ✅ Tests des en-têtes et content-type

### 📁 Services
Tests unitaires pour les services :

#### PokemonServiceTests
- ✅ Tests avec mocks pour tous les services Pokemon
- ✅ Tests de validation des données
- ✅ Tests des exceptions et cas d'erreur

### 📁 EndToEnd
Tests de bout en bout pour les scénarios complets :

#### PokemonApiEndToEndTests
- ✅ Scénario complet : Créer dresseur → Créer Pokemon → Assigner
- ✅ Scénario de combat entre dresseurs
- ✅ Scénario d'évolution de Pokemon
- ✅ Scénarios de recherche et filtrage
- ✅ Tests de robustesse

### 📁 Performance
Tests de performance et de charge :

#### PerformanceTests
- ✅ Tests de temps de réponse
- ✅ Tests de charge concurrente
- ✅ Tests de stress avec volume élevé
- ✅ Tests de taille de réponse

## Types de Tests Couverts

### 🧪 Tests Fonctionnels
- Tous les endpoints CRUD (Create, Read, Update, Delete)
- Recherche et filtrage par différents critères
- Opérations spécialisées (level-up, change ability, etc.)
- Gestion des combats et dresseurs

### ✅ Tests de Validation
- Validation des données d'entrée
- Contraintes de longueur (noms Pokemon 5-10 caractères)
- Validation des plages de valeurs (âge, niveau, etc.)
- Validation de la logique métier

### 🚫 Tests d'Erreur
- Codes de statut HTTP appropriés
- Gestion des IDs inexistants
- Validation des données invalides
- Tests d'authentification/autorisation

### ⚡ Tests de Performance
- Temps de réponse acceptables
- Gestion de la charge concurrente
- Tests de stress avec volume élevé
- Optimisation des requêtes de recherche

### 🔗 Tests d'Intégration
- Communication entre contrôleurs et services
- Tests des APIs minimales
- Scénarios de bout en bout
- Tests avec base de données (si applicable)

## Comment Exécuter les Tests

### Prérequis
```bash
# Restaurer les packages NuGet
dotnet restore
```

### Exécuter tous les tests
```bash
dotnet test
```

### Exécuter des tests spécifiques
```bash
# Tests d'un seul contrôleur
dotnet test --filter "FullyQualifiedName~PokemonControllerTests"

# Tests de performance uniquement
dotnet test --filter "FullyQualifiedName~PerformanceTests"

# Tests d'un endpoint spécifique
dotnet test --filter "TestCategory=Pokemon&TestCategory=GET"
```

### Générer un rapport de couverture
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Configuration pour Différents Environnements

### Tests avec Authentification
Si votre API nécessite une authentification :
1. Modifiez `CustomWebApplicationFactory` pour désactiver l'auth en test
2. Ou ajoutez des tokens de test valides dans `BaseApiTest`

### Tests avec Base de Données
Pour tester avec une vraie base de données :
1. Configurez une base de test dans `CustomWebApplicationFactory`
2. Ajoutez un setup/cleanup des données de test
3. Utilisez des transactions pour isoler les tests

### Variables d'Environnement
```bash
# URL de l'API de test
export TEST_API_URL="https://localhost:5001"

# Niveau de logging pour les tests
export LOG_LEVEL="Warning"
```

## Métriques de Qualité

### Couverture de Code Visée
- **Contrôleurs** : 95%+ de couverture
- **Services** : 90%+ de couverture
- **Modèles** : 80%+ de couverture

### Critères de Performance
- **Temps de réponse simple** : < 1 seconde
- **Temps de réponse recherche** : < 3 secondes
- **Charge concurrente** : 50 requêtes simultanées
- **Taux de succès sous charge** : > 95%

### Standards de Test
- Tous les endpoints publics testés
- Tous les cas d'erreur couverts
- Tests de validation pour tous les DTOs
- Scénarios de bout en bout pour les workflows principaux

## Maintenance et Évolution

### Ajouter de Nouveaux Tests
1. Respecter la structure de dossiers existante
2. Hériter de `BaseApiTest` pour les tests d'API
3. Utiliser `FluentAssertions` pour les assertions
4. Documenter les nouveaux scénarios de test

### Mise à Jour après Changements d'API
1. Mettre à jour les modèles de test si nécessaire
2. Ajouter des tests pour les nouveaux endpoints
3. Modifier les tests existants si les contrats changent
4. Vérifier que tous les tests passent

## Troubleshooting

### Problèmes Courants
1. **Tests qui échouent avec 401 Unauthorized** : Vérifier la configuration d'authentification
2. **Timeouts** : Augmenter les limites de temps ou optimiser les requêtes
3. **Tests flaky** : Vérifier l'isolation des données de test
4. **Problèmes de concurrence** : Utiliser des données uniques par test

### Debug des Tests
```bash
# Exécuter avec logs détaillés
dotnet test --logger "console;verbosity=detailed"

# Debug d'un test spécifique
dotnet test --filter "GetPokemonById_WithValidId_ShouldReturnPokemon" --logger "console;verbosity=detailed"
```

---

Cette suite de tests garantit la qualité et la fiabilité de l'API Pokemon dans tous les scénarios d'utilisation.
