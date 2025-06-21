# 📊 Middleware de Tracing et Monitoring

Ce projet inclut des middlewares avancés pour tracer les temps d'exécution et monitorer les performances de l'API Pokémon.

## 🚀 Fonctionnalités

### ⏱️ ExecutionTimeMiddleware
- **Trace chaque requête** avec temps de début et fin
- **Mesure le temps d'exécution** en millisecondes
- **Affiche les détails** : méthode HTTP, chemin, User-Agent, IP
- **Alertes automatiques** pour les requêtes lentes (>1000ms)
- **Gestion des erreurs** avec capture des exceptions

### 📈 RequestStatisticsMiddleware  
- **Statistiques en temps réel** par endpoint
- **Compteurs de requêtes** et calculs de moyennes
- **Temps min/max/moyen** par endpoint
- **Taux de succès** et comptage des erreurs
- **Affichage automatique** toutes les 10 requêtes

### 🖥️ MonitoringController
Endpoints dédiés au monitoring :

#### `GET /api/monitoring/statistics`
Affiche les statistiques détaillées dans la console

#### `GET /api/monitoring/test/{delay}`
Test de performance avec délai configurable (0-5000ms)

#### `GET /api/monitoring/test-error/{type}`
Génère des erreurs de test :
- `400` ou `bad` : Bad Request
- `404` ou `notfound` : Not Found  
- `500` ou `server` : Server Error
- `exception` : Exception non gérée

#### `GET /api/monitoring/info`
Informations sur le serveur et l'application

## 📝 Exemple de sortie

```
[2025-06-19 14:30:15.123] 🚀 DÉBUT - GET /api/pokemon
  📱 User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64)
  🌐 IP: 127.0.0.1

[2025-06-19 14:30:15.456] ✅ FIN - GET /api/pokemon
  ⏱️  Temps d'exécution: 333 ms
  📊 Status: 200 Success
  ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🏆 ═══════════════ STATISTIQUES EN TEMPS RÉEL ═══════════════
📊 Total des requêtes: 25
⏰ Mise à jour: 14:30:20
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Endpoint                            Nb    Moy    Min    Max    Err   Taux  
─────────────────────────────────── ───── ────── ────── ────── ───── ──────
GET /api/pokemon                    8     245ms  150ms  400ms  0     100.0%
GET /api/trainer                    5     180ms  120ms  250ms  1     80.0%
POST /api/battle                    3     320ms  280ms  380ms  0     100.0%
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## 🎯 Tests recommandés

1. **Test de performance basique :**
   ```
   GET /api/monitoring/test/500
   ```

2. **Test de requête lente :**
   ```
   GET /api/monitoring/test/2000
   ```

3. **Test d'erreur :**
   ```
   GET /api/monitoring/test-error/500
   ```

4. **Affichage des stats :**
   ```
   GET /api/monitoring/statistics
   ```

5. **Test avec plusieurs endpoints :**
   ```bash
   # Faites plusieurs appels à différents endpoints
   GET /api/pokemon
   GET /api/trainer
   POST /api/battle
   GET /api/monitoring/statistics
   ```

## ⚙️ Configuration

Les middlewares sont automatiquement configurés dans `Program.cs` :

```csharp
app.UseExecutionTimeTracing();
app.UseRequestStatistics();
```

Les middlewares sont placés en début de pipeline pour capturer toutes les requêtes.

## 🔧 Personnalisation

### Modifier les seuils d'alerte :
Dans `ExecutionTimeMiddleware.cs`, ligne 67 :
```csharp
if (responseTime > 1000) // Changer 1000 pour un autre seuil
```

### Modifier la fréquence d'affichage :
Dans `RequestStatisticsMiddleware.cs`, ligne 41 :
```csharp
if (_totalRequests % 10 == 0 || ...) // Changer 10 pour autre fréquence
```

## 📊 Métriques collectées

- **Temps d'exécution** par requête
- **Nombre total** de requêtes
- **Temps moyen/min/max** par endpoint
- **Taux de succès** par endpoint
- **Comptage des erreurs**
- **Informations de la requête** (IP, User-Agent)
- **Codes de statut HTTP**

Le système fournit une visibilité complète sur les performances de votre API Pokémon ! 🚀
