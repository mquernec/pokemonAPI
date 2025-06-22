# Script PowerShell pour exécuter les tests de l'API Pokemon (.NET 9)
# Usage: .\RunTests.ps1 [TestType] [Verbose]

param(
    [string]$TestType = "All",
    [switch]$Verbose = $false,
    [switch]$Coverage = $false,
    [switch]$DisableParallel = $false,
    [switch]$Net9Features = $false
)

Write-Host "=== Tests de l'API Pokemon (.NET 9) ===" -ForegroundColor Green
Write-Host "Type de test: $TestType" -ForegroundColor Yellow

# Vérifier la version de .NET
try {
    $dotnetVersion = dotnet --version
    Write-Host "Version .NET: $dotnetVersion" -ForegroundColor Cyan
    
    if (-not $dotnetVersion.StartsWith("9.")) {
        Write-Host "⚠️  Attention: .NET 9 recommandé pour ce projet" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ .NET CLI non trouvé" -ForegroundColor Red
    exit 1
}

# Configuration des couleurs
$successColor = "Green"
$errorColor = "Red"
$warningColor = "Yellow"
$infoColor = "Cyan"

# Fonction pour exécuter une commande et gérer les erreurs
function Invoke-TestCommand {
    param([string]$Command, [string]$Description)
    
    Write-Host "`n--- $Description ---" -ForegroundColor $infoColor
    Write-Host "Commande: $Command" -ForegroundColor Gray
    
    $startTime = Get-Date
    
    try {
        if ($Verbose) {
            Invoke-Expression "$Command --logger 'console;verbosity=detailed'"
        } else {
            Invoke-Expression $Command
        }
        
        $endTime = Get-Date
        $duration = $endTime - $startTime
        Write-Host "✓ $Description terminé en $($duration.TotalSeconds.ToString('F2')) secondes" -ForegroundColor $successColor
        return $true
    }
    catch {
        Write-Host "✗ Erreur lors de $Description" -ForegroundColor $errorColor
        Write-Host $_.Exception.Message -ForegroundColor $errorColor
        return $false
    }
}

# Vérifier que nous sommes dans le bon répertoire
if (-not (Test-Path "PokemonAPI.Tests.csproj")) {
    Write-Host "Erreur: Ce script doit être exécuté depuis le répertoire PokemonAPI.Tests" -ForegroundColor $errorColor
    exit 1
}

# Commandes de base
$baseCommand = "dotnet test"
if (-not $Parallel) {
    $baseCommand += " --parallel:false"
}

if ($Coverage) {
    $baseCommand += " --collect:'XPlat Code Coverage'"
}

# Configuration des filtres selon le type de test
switch ($TestType.ToLower()) {
    "all" {
        Write-Host "Exécution de tous les tests..." -ForegroundColor $infoColor
        $success = Invoke-TestCommand $baseCommand "Tous les tests"
    }
    
    "unit" {
        Write-Host "Exécution des tests unitaires uniquement..." -ForegroundColor $infoColor
        $filter = "--filter 'FullyQualifiedName~Services'"
        $success = Invoke-TestCommand "$baseCommand $filter" "Tests unitaires (Services)"
    }
    
    "integration" {
        Write-Host "Exécution des tests d'intégration..." -ForegroundColor $infoColor
        $filter = "--filter 'FullyQualifiedName~Controllers'"
        $success = Invoke-TestCommand "$baseCommand $filter" "Tests d'intégration (Controllers)"
    }
    
    "minimal" {
        Write-Host "Exécution des tests d'API minimale..." -ForegroundColor $infoColor
        $filter = "--filter 'FullyQualifiedName~MinimalApi'"
        $success = Invoke-TestCommand "$baseCommand $filter" "Tests API minimale"
    }
    
    "performance" {
        Write-Host "Exécution des tests de performance..." -ForegroundColor $infoColor
        $filter = "--filter 'FullyQualifiedName~Performance'"
        $success = Invoke-TestCommand "$baseCommand $filter" "Tests de performance"
    }
    
    "e2e" {
        Write-Host "Exécution des tests de bout en bout..." -ForegroundColor $infoColor
        $filter = "--filter 'FullyQualifiedName~EndToEnd'"
        $success = Invoke-TestCommand "$baseCommand $filter" "Tests de bout en bout"
    }
    
    "pokemon" {
        Write-Host "Exécution des tests Pokemon uniquement..." -ForegroundColor $infoColor
        $filter = "--filter 'FullyQualifiedName~Pokemon'"
        $success = Invoke-TestCommand "$baseCommand $filter" "Tests Pokemon"
    }
    
    "trainer" {
        Write-Host "Exécution des tests Trainer uniquement..." -ForegroundColor $infoColor
        $filter = "--filter 'FullyQualifiedName~Trainer'"
        $success = Invoke-TestCommand "$baseCommand $filter" "Tests Trainer"
    }
    
    "battle" {
        Write-Host "Exécution des tests Battle uniquement..." -ForegroundColor $infoColor
        $filter = "--filter 'FullyQualifiedName~Battle'"
        $success = Invoke-TestCommand "$baseCommand $filter" "Tests Battle"
    }
    
    "fast" {
        Write-Host "Exécution des tests rapides (exclusion performance et E2E)..." -ForegroundColor $infoColor
        $filter = "--filter 'FullyQualifiedName!~Performance&FullyQualifiedName!~EndToEnd'"
        $success = Invoke-TestCommand "$baseCommand $filter" "Tests rapides"
    }
    
    default {
        Write-Host "Type de test non reconnu: $TestType" -ForegroundColor $errorColor
        Write-Host "Types disponibles: All, Unit, Integration, Minimal, Performance, E2E, Pokemon, Trainer, Battle, Fast" -ForegroundColor $warningColor
        exit 1
    }
}

# Tests spécifiques par catégorie si demandé
if ($TestType.ToLower() -eq "categories") {
    Write-Host "`n=== Exécution par catégories ===" -ForegroundColor Green
    
    $categories = @(
        @{Name="Tests GET"; Filter="--filter 'TestCategory=GET'"},
        @{Name="Tests POST"; Filter="--filter 'TestCategory=POST'"},
        @{Name="Tests PUT"; Filter="--filter 'TestCategory=PUT'"},
        @{Name="Tests DELETE"; Filter="--filter 'TestCategory=DELETE'"},
        @{Name="Tests de validation"; Filter="--filter 'TestCategory=Validation'"}
    )
    
    $allSuccess = $true
    foreach ($category in $categories) {
        $categorySuccess = Invoke-TestCommand "$baseCommand $($category.Filter)" $category.Name
        $allSuccess = $allSuccess -and $categorySuccess
    }
    $success = $allSuccess
}

# Rapport de couverture si demandé
if ($Coverage -and $success) {
    Write-Host "`n=== Génération du rapport de couverture ===" -ForegroundColor Green
    
    # Rechercher le fichier de couverture le plus récent
    $coverageFiles = Get-ChildItem -Recurse -Filter "coverage.cobertura.xml" | Sort-Object LastWriteTime -Descending
    
    if ($coverageFiles.Count -gt 0) {
        $latestCoverage = $coverageFiles[0].FullName
        Write-Host "Fichier de couverture trouvé: $latestCoverage" -ForegroundColor $infoColor
        
        # Installer reportgenerator si pas déjà fait
        try {
            dotnet tool install --global dotnet-reportgenerator-globaltool 2>$null
        } catch {
            # Outil déjà installé
        }
        
        # Générer le rapport HTML
        $reportPath = Join-Path (Get-Location) "TestResults\CoverageReport"
        $reportCommand = "reportgenerator -reports:`"$latestCoverage`" -targetdir:`"$reportPath`" -reporttypes:Html"
        
        try {
            Invoke-Expression $reportCommand
            Write-Host "✓ Rapport de couverture généré: $reportPath\index.html" -ForegroundColor $successColor
            
            # Ouvrir le rapport automatiquement
            $indexFile = Join-Path $reportPath "index.html"
            if (Test-Path $indexFile) {
                Write-Host "Ouverture du rapport de couverture..." -ForegroundColor $infoColor
                Start-Process $indexFile
            }
        } catch {
            Write-Host "✗ Erreur lors de la génération du rapport de couverture" -ForegroundColor $errorColor
        }
    } else {
        Write-Host "Aucun fichier de couverture trouvé" -ForegroundColor $warningColor
    }
}

# Résumé final
Write-Host "`n=== Résumé ===" -ForegroundColor Green
if ($success) {
    Write-Host "✓ Tous les tests ont été exécutés avec succès!" -ForegroundColor $successColor
    exit 0
} else {
    Write-Host "✗ Certains tests ont échoué. Consultez les détails ci-dessus." -ForegroundColor $errorColor
    exit 1
}

# Aide
if ($args -contains "--help" -or $args -contains "-h") {
    Write-Host @"

=== Script de Tests de l'API Pokemon ===

Usage: .\RunTests.ps1 [TestType] [Options]

Types de tests disponibles:
  All          - Tous les tests (défaut)
  Unit         - Tests unitaires uniquement
  Integration  - Tests d'intégration
  Minimal      - Tests d'API minimale
  Performance  - Tests de performance
  E2E          - Tests de bout en bout
  Pokemon      - Tests liés aux Pokemon
  Trainer      - Tests liés aux Trainers
  Battle       - Tests liés aux Battles
  Fast         - Tests rapides (exclut performance et E2E)
  Categories   - Exécution par catégories HTTP

Options:
  -Verbose     - Affichage détaillé
  -Coverage    - Génération du rapport de couverture
  -Parallel:$false - Désactiver l'exécution parallèle

Exemples:
  .\RunTests.ps1                    # Tous les tests
  .\RunTests.ps1 Pokemon -Verbose   # Tests Pokemon avec détails
  .\RunTests.ps1 Performance        # Tests de performance uniquement
  .\RunTests.ps1 All -Coverage      # Tous les tests avec couverture

"@ -ForegroundColor $infoColor
}
