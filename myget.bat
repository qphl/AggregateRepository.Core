@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

REM Build
call "%msbuild%" src\cr-aggregaterepository.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
if not "%errorlevel%"=="0" goto failure

REM Unit tests
call %nuget% install NUnit.Runners -Version 2.6.2 -OutputDirectory packages
packages\NUnit.Runners.2.6.2\tools\nunit-console.exe /config:%config% /framework:net-4.5 src\tests\bin\%config%\CR.AggregateRepository.Tests.dll
if not "%errorlevel%"=="0" goto failure

REM Package
mkdir Build
cmd /c %nuget% pack "src\core\core.csproj" -symbols -o Build -p Configuration=%config%
if not "%errorlevel%"=="0" goto failure

cmd /c %nuget% pack "src\persistance.memory\persistance.memory.csproj" -symbols -o Build -p Configuration=%config%
if not "%errorlevel%"=="0" goto failure

cmd /c %nuget% pack "src\persistance.eventstore\persistance.eventstore.csproj" -symbols -o Build -p Configuration=%config%
if not "%errorlevel%"=="0" goto failure

:success
exit 0

:failure
exit -1