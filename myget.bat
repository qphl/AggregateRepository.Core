@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

REM Build
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild src\cr-aggregaterepository.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
if not "%errorlevel%"=="0" goto failure

REM Unit tests
"%GallioEcho%" src\tests\bin\%config%\CR.AggregateRepository.Tests.dll
if not "%errorlevel%"=="0" goto failure

REM Package
mkdir Build
cmd /c %nuget% pack "src\core\core.csproj" -symbols -o Build -p Configuration=%config%
if not "%errorlevel%"=="0" goto failure

cmd /c %nuget% pack "src\persistance.memory\persistance.memory.csproj" -symbols -o Build -p Configuration=%config%
if not "%errorlevel%"=="0" goto failure

cmd /c %nuget% pack "src\persistance.ravendb\persistance.eventstore.csproj" -symbols -o Build -p Configuration=%config%
if not "%errorlevel%"=="0" goto failure

cmd /c %nuget% pack "src\persistance.applicationstate\persistance.applicationstate.csproj" -symbols -o Build -p Configuration=%config%
if not "%errorlevel%"=="0" goto failure

:success
exit 0

:failure
exit -1