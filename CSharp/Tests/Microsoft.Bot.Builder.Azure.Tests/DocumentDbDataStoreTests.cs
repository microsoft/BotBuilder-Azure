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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    public class DocumentDbDataStoreTests : BaseDataStoreTests
    {
        //This test requires the DocDbEmulator to be installed and started. 
        //Reference: https://docs.microsoft.com/en-us/azure/documentdb/documentdb-nosql-local-emulator
        
        //Fixed docDb emulator local Uri.
        private static readonly Uri docDbEmulatorUri = new Uri("https://localhost:8081");

        //Fixed docDb emulator key
        private static readonly string docDbEmulatorKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private DocumentDbDeleter docDbDeleter;

        public override IBotDataStore<BotData> GetTestCaseDataStore()
        {
            return new DocumentDbBotDataStore(docDbEmulatorUri, docDbEmulatorKey);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            if (docDbDeleter == null)
            {
                docDbDeleter = new DocumentDbDeleter(docDbEmulatorUri, docDbEmulatorKey);
            }

            docDbDeleter.DeleteDatabaseIfExists().Wait();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            docDbDeleter.DeleteDatabaseIfExists().Wait();
        }

        [TestMethod]
        public new async Task SetGet()
        {
            await base.SetGet();
        }

        [TestMethod]
        public new async Task SaveSemantics()
        {
            await base.SaveSemantics();
        }

        [TestMethod]
        public new async Task GetUnknownAddress()
        {
            await base.GetUnknownAddress();
        }
    }
}

