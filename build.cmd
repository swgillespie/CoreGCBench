@echo off
dotnet restore
msbuild CoreGCBench.Core.sln /p:Configuration=Debug /p:Platform="Any CPU"
msbuild CoreGCBench.Desktop.sln /p:Configuration=Debug /p:Platform="Any CPU"