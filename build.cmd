@echo off
dotnet restore src\CoreGCBench.Runner\CoreGCBench.Runner.csproj
.nuget\nuget.exe restore CoreGCBench.Desktop.sln
msbuild CoreGCBench.Core.sln /p:Configuration=Debug /p:Platform="Any CPU"
msbuild CoreGCBench.Desktop.sln /p:Configuration=Debug /p:Platform="Any CPU"