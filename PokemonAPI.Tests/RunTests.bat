@echo off
:: Script batch pour exécuter les tests de l'API Pokemon
:: Usage: RunTests.bat [TestType]

setlocal enabledelayedexpansion

set TEST_TYPE=%1
if "%TEST_TYPE%"=="" set TEST_TYPE=All

echo.
echo === Tests de l'API Pokemon ===
echo Type de test: %TEST_TYPE%
echo.

:: Vérifier que nous sommes dans le bon répertoire
if not exist "PokemonAPI.Tests.csproj" (
    echo Erreur: Ce script doit etre execute depuis le repertoire PokemonAPI.Tests
    pause
    exit /b 1
)

:: Configuration des commandes selon le type de test
set BASE_CMD=dotnet test

if /i "%TEST_TYPE%"=="All" (
    echo Execution de tous les tests...
    %BASE_CMD%
) else if /i "%TEST_TYPE%"=="Unit" (
    echo Execution des tests unitaires...
    %BASE_CMD% --filter "FullyQualifiedName~Services"
) else if /i "%TEST_TYPE%"=="Integration" (
    echo Execution des tests d'integration...
    %BASE_CMD% --filter "FullyQualifiedName~Controllers"
) else if /i "%TEST_TYPE%"=="Minimal" (
    echo Execution des tests d'API minimale...
    %BASE_CMD% --filter "FullyQualifiedName~MinimalApi"
) else if /i "%TEST_TYPE%"=="Performance" (
    echo Execution des tests de performance...
    %BASE_CMD% --filter "FullyQualifiedName~Performance"
) else if /i "%TEST_TYPE%"=="E2E" (
    echo Execution des tests de bout en bout...
    %BASE_CMD% --filter "FullyQualifiedName~EndToEnd"
) else if /i "%TEST_TYPE%"=="Pokemon" (
    echo Execution des tests Pokemon...
    %BASE_CMD% --filter "FullyQualifiedName~Pokemon"
) else if /i "%TEST_TYPE%"=="Trainer" (
    echo Execution des tests Trainer...
    %BASE_CMD% --filter "FullyQualifiedName~Trainer"
) else if /i "%TEST_TYPE%"=="Battle" (
    echo Execution des tests Battle...
    %BASE_CMD% --filter "FullyQualifiedName~Battle"
) else if /i "%TEST_TYPE%"=="Fast" (
    echo Execution des tests rapides...
    %BASE_CMD% --filter "FullyQualifiedName!~Performance&FullyQualifiedName!~EndToEnd"
) else if /i "%TEST_TYPE%"=="Coverage" (
    echo Execution avec rapport de couverture...
    %BASE_CMD% --collect:"XPlat Code Coverage"
) else (
    echo Type de test non reconnu: %TEST_TYPE%
    echo Types disponibles: All, Unit, Integration, Minimal, Performance, E2E, Pokemon, Trainer, Battle, Fast, Coverage
    pause
    exit /b 1
)

if %ERRORLEVEL% equ 0 (
    echo.
    echo === Tests termines avec succes ===
    echo.
) else (
    echo.
    echo === Certains tests ont echoue ===
    echo.
)

pause
