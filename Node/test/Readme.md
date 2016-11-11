#BotBuilder Node unit tests - Run instructions

## 1. Open the test project in Visual Studio Code
## 2. Start Azure Storage Emulator
## 3. In the file launch.json, under .vscode, edit the "program" attribute to run the test you want to execute. Example:

`"program": "${workspaceRoot}/TableBotStorageFaultTest.js"`

## 4. In the Visual Studio Code interactive terminal, run the following command, specifying the same test file from step 3

`mocha --debug-brk TableBotStorageFaultTest.js`

## 5. Open the debug view in Visual Studio Code
## 6. Make sure the debugger is configured to 'Attach to process'
## 7. Set breakpoints if needed and happy debugging!