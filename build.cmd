@echo off

SET VERSION=0.0.0
IF NOT [%1]==[] (set VERSION=%1)

SET TAG=0.0.0
IF NOT [%2]==[] (set TAG=%2)
SET TAG=%TAG:tags/=%

dotnet test .\src\AggregateRepository.Core.Tests\AggregateRepository.Core.Tests.csproj
if %errorlevel% neq 0 exit /b %errorlevel%

dotnet pack .\src\AggregateRepository.Core\AggregateRepository.Core.csproj -o .\dist -p:Version="%VERSION%" -p:PackageVersion="%VERSION%" -p:Tag="%TAG%" -c Release