# Guide d'utilisation des ActionFilters de Monitoring

Les ActionFilters permettent d'appliquer le monitoring et la traçabilité au niveau des actions individuelles ou des contrôleurs entiers, contrairement aux middlewares qui s'appliquent à toutes les requêtes.

## 📋 ActionFilters Disponibles

### 1. ExecutionTimeActionFilter / ExecutionTimeAttribute
Trace les temps d'exécution des actions avec des logs détaillés.

**Utilisation avec injection de dépendance :**
```csharp
[ServiceFilter(typeof(ExecutionTimeActionFilter))]
public class PokemonController : ControllerBase
{
    // Toutes les actions du contrôleur seront tracées
}
```

**Utilisation comme attribut simple :**
```csharp
[ExecutionTime] // Plus simple, pas d'injection de dépendance nécessaire
public async Task<ActionResult<Pokemon>> GetPokemon(int id)
{
    // Action tracée
}
```

### 2. ActionStatisticsFilter
Collecte des statistiques détaillées sur les actions.

```csharp
[ServiceFilter(typeof(ActionStatisticsFilter))]
public class TrainerController : ControllerBase
{
    // Collecte des statistiques pour toutes les actions
}
```

### 3. MonitorPerformanceAttribute
Attribut simple pour marquer les actions importantes à surveiller.

```csharp
[MonitorPerformance]
public async Task<ActionResult> StartBattle(int trainerId1, int trainerId2)
{
    // Action marquée comme importante à surveiller
}

[MonitorPerformance(ActionName = "CriticalOperation")]
public async Task<ActionResult> ComplexOperation()
{
    // Action avec nom personnalisé
}
```

## 🔧 Configuration et Enregistrement

### Dans Program.cs

```csharp
// Enregistrer les filters avec injection de dépendance
builder.Services.AddScoped<ExecutionTimeActionFilter>();
builder.Services.AddScoped<ActionStatisticsFilter>();

// Ou enregistrer globalement
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExecutionTimeActionFilter>();
    options.Filters.Add<ActionStatisticsFilter>();
});
```

## 🎯 Exemples d'Application

### Au niveau du contrôleur (toutes les actions)
```csharp
[ServiceFilter(typeof(ExecutionTimeActionFilter))]
[ServiceFilter(typeof(ActionStatisticsFilter))]
public class PokemonController : ControllerBase
{
    // Toutes les actions héritent du monitoring
}
```

### Au niveau d'une action spécifique
```csharp
public class PokemonController : ControllerBase
{
    [ExecutionTime]
    [MonitorPerformance]
    public async Task<ActionResult<Pokemon>> GetPokemon(int id)
    {
        // Action spécifiquement monitorée
    }

    // Cette action n'est pas monitorée
    public async Task<ActionResult<List<Pokemon>>> GetAllPokemon()
    {
        // ...
    }
}
```

### Combinaison d'attributs
```csharp
[ExecutionTime]
[ServiceFilter(typeof(ActionStatisticsFilter))]
[MonitorPerformance(ActionName = "CriticalBattleOperation")]
public async Task<ActionResult> StartBattle(BattleRequest request)
{
    // Action avec monitoring complet
}
```

## 📊 Sortie Console

### ExecutionTimeActionFilter
```
[2024-01-15 14:30:15.123] 🎯 ACTION DÉBUT - PokemonController.GetPokemon
  🌐 GET /api/pokemon/5
  📱 User-Agent: PostmanRuntime/7.32.2
  🔗 IP: ::1
  📋 Paramètres:
     • id: 5

[2024-01-15 14:30:15.158] ✅ ACTION FIN - PokemonController.GetPokemon
  ⏱️  Temps d'exécution: 35 ms
  📊 Status: 200 Success
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

### ActionStatisticsFilter
```
🎯 ═══════════════ STATISTIQUES ACTIONS ═══════════════
📊 Total des actions: 25
⏰ Mise à jour: 14:30:45
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Action                         Nb   Moy    Min   Max   Err  Taux
────────────────────────────── ──── ────── ───── ───── ──── ──────
PokemonController.GetPokemon   8    42ms   15ms  89ms  0    100.0%
TrainerController.GetTrainer   5    28ms   12ms  45ms  1    80.0%
BattleController.StartBattle   3    156ms  98ms  234ms 0    100.0%
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## 🔍 Accès aux Statistiques

### Via l'endpoint du MonitoringController
```http
GET /api/monitoring/action-statistics
```

### Via la méthode statique
```csharp
var stats = ActionStatisticsFilter.GetStatisticsData();
```

## ⚠️ Considérations

### Différences avec les Middlewares
- **Middlewares** : S'appliquent à toutes les requêtes, y compris les fichiers statiques
- **ActionFilters** : S'appliquent uniquement aux actions des contrôleurs

### Performance
- Les ActionFilters ont une granularité plus fine
- Possibilité d'activer/désactiver le monitoring par action
- Moins d'impact global sur les performances

### Flexibilité
- Combinaison possible avec les middlewares
- Application sélective selon les besoins
- Personnalisation par action ou contrôleur

## 🚀 Exemples d'Utilisation Avancée

### Monitoring conditionnel
```csharp
[ExecutionTime]
public async Task<ActionResult> ExpensiveOperation()
{
    if (ShouldMonitor())
    {
        // Monitoring actif
    }
}
```

### Avec des paramètres personnalisés
```csharp
[MonitorPerformance(ActionName = "CriticalPaymentProcess")]
[ExecutionTime]
public async Task<ActionResult> ProcessPayment(PaymentRequest request)
{
    // Monitoring avec nom personnalisé
}
```
