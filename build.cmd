@echo off

set __BuildConfig=Debug
set __Restore=1

:ArgLoop
if "%1" == "" goto ArgsDone

if /i "%1" == "-?"    goto Usage
if /i "%1" == "-h"    goto Usage
if /i "%1" == "-help" goto Usage

if /i "%1" == "debug" (
    set __BuildConfig=Debug
    shift
    goto ArgLoop
)

if /i "%1" == "release" (
    set __BuildConfig=Release
    shift
    goto ArgLoop
)

if /i "%1" == "skiprestore" (
    set __Restore=0
    shift
    goto ArgLoop
)

:ArgsDone

if %__Restore%==1 (
    dotnet restore src\CoreGCBench.Runner\CoreGCBench.Runner.csproj
    .nuget\nuget.exe restore CoreGCBench.Desktop.sln
)

msbuild CoreGCBench.Core.sln /p:Configuration=%__BuildConfig% /p:Platform="Any CPU"
msbuild CoreGCBench.Desktop.sln /p:Configuration=%__BuildConfig% /p:Platform="Any CPU"
dotnet publish src\CoreGCBench.Runner\CoreGCBench.Runner.csproj --configuration %__BuildConfig%
exit /b 0

:Usage
echo build.cmd [debug] [release] [skiprestore] - Builds CoreGCBench
exit /b 1