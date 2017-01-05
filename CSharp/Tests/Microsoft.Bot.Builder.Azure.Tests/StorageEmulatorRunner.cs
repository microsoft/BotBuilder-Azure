// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    /// <summary>
    /// Starts and stops Azure Storage Emulator for use in unit or integration tests
    /// </summary>
    internal static class StorageEmulatorRunner
    {
        //Azure storage emulator process name varies by version and architecture, taking one of the names below
        private const string EmulatorProcessNameV1 = "AZURES~1";
        private const string EmulatorProcessNameV2 = "AzureStorageEmulator";

        private const string EmulatorExecutableFileName = "AzureStorageEmulator.exe";
        private const string AzureSdkSubDirectory = @"{0}\Microsoft SDKs\Azure\Storage Emulator";

        private static bool isRunning = false;
        private static bool emulatorWasPreviouslyRunning = false; 

        /// <summary>
        /// Starts Azure Storage Emulator if it has not been started already
        /// </summary>
        public static void Start()
        {
            if (isRunning)
            {
                return;
            }

            Process[] processes = Process.GetProcesses();
            if(processes.Any(p => IsStorageEmulator(p)))
            {
                isRunning = true;
                return;
            }

            var azureSdkDirectory = string.Format(AzureSdkSubDirectory, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
            var executableFullFilePath = Path.Combine(azureSdkDirectory, EmulatorExecutableFileName);
            
            if (!File.Exists(executableFullFilePath))
            {
                throw new FileNotFoundException($"Failed to find Azure Storage Emulator at {executableFullFilePath}. Make sure Azure Storage Emulator is installed");
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = executableFullFilePath,
                Arguments = "start"
            };

            using (Process emulatorProcess = Process.Start(processStartInfo))
            {
                emulatorProcess.WaitForExit();
                emulatorWasPreviouslyRunning = false;
                isRunning = true;
            }
        }

        /// <summary>
        /// Stops Azure Storage Emulator
        /// </summary>
        public static void Stop()
        {
            Process[] azureStorageProcesses = Process.GetProcesses();
            var emulatorProcess = azureStorageProcesses.SingleOrDefault(p => IsStorageEmulator(p));

            // If the emulator is running and we were the ones that started it, we stop the process
            if (emulatorProcess != null && !emulatorWasPreviouslyRunning)
            {
                emulatorProcess.Kill();
                isRunning = false;
            }
        }

        private static bool IsStorageEmulator(Process p)
        {
            return p.ProcessName.StartsWith(EmulatorProcessNameV1, StringComparison.InvariantCultureIgnoreCase) 
                || p.ProcessName.StartsWith(EmulatorProcessNameV2, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
