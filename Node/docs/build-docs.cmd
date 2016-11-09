@ECHO OFF
ECHO [COMPILING DOCS]
typedoc --includeDeclarations --module amd --out doc ..\src\botbuilder-azure.d.ts --theme .\botframework --hideGenerator --name "Bot Builder for Azure Reference Library" --readme none
