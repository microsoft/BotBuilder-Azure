@echo off
echo *** Building Microsoft.Bot.Builder.Azure
setlocal
setlocal enabledelayedexpansion
setlocal enableextensions
set errorlevel=0
mkdir ..\nuget
erase /s ..\nuget\Microsoft.Bot.Builder.Azure*nupkg
msbuild /property:Configuration=release Microsoft.Bot.Builder.Azure.csproj 
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.dll).FileVersionInfo.FileVersion"') do set builder=%%v
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.History.dll).FileVersionInfo.FileVersion"') do set history=%%v
for /f %%v in ('powershell -noprofile "(Get-Command .\bin\release\Microsoft.Bot.Builder.Azure.dll).FileVersionInfo.FileVersion"') do set version=%%v
..\..\packages\NuGet.CommandLine.3.4.3\tools\NuGet.exe pack Microsoft.Bot.Builder.Azure.nuspec -symbols -properties version=%version%;history=%history%;builder=%builder% -OutputDirectory ..\nuget
echo *** Finished building Microsoft.Bot.Builder.Azure

