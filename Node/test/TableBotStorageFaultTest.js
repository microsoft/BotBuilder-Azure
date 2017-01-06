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

var azure = require('../');
var faults = require('./lib/FaultyAzureTableClient');
var assert = require('assert');
var builder = require('botbuilder');

describe('TableBotStorageFaultTest', function() {
    it('when initialization fails, an error with the status code and message should be returned', function(done) {
        let options = {
            gzipData: true,
        };
        let faultSettings = {
            shouldFailInsert: false,
            shouldFailInitialize: true,
            shouldFailRetrieve: false,
            error: {
                code: 'TEST_CODE',
                message: 'Failed to create table',
                statusCode: '416'
            },
            response: {
                isSuccessful: false,
                statusCode: '416'
            }
        }

        let realTableClient = new azure.AzureTableClient('testTable1');
        let faultyTableClient = new faults.FaultyAzureTableClient(realTableClient, faultSettings);

        let store = new azure.AzureBotStorage(options, faultyTableClient);
        
        let botStorageContext = {
            userId: 'userId1',
            conversationId: 'convId1',
            persistUserData: true,
            persistConversationData: true
        };

        let botStorageData = {
            userData: '{ Foo: non-null user data }',
            conversationData: '{ Bar: nonnull conversation data }',
            privateConversationData: '{ Baz: non-null private conversation data }'
        }

        store.saveData(botStorageContext, botStorageData, function(error){
            if(error){
                assert.notEqual(-1, error.message.indexOf(faultSettings.response.statusCode));
                assert.notEqual(-1, error.message.indexOf(faultSettings.error.message));
                assert.notEqual(-1, error.message.indexOf(faultSettings.error.code));
                done();
            }
            else{
               assert.fail('The call to saveData should not have succeeded');
               done();
            }
        });    
    });

    it('when saving data fails using gzip, an error with the status code and message should be returned', function(done) {
        let options = {
            gzipData: true,
        };
        let faultSettings = {
            shouldFailInsert: true,
            shouldFailInitialize: false,
            shouldFailRetrieve: false,
            error: {
                code: 'TEST_CODE',
                message: 'Failed to create table',
                statusCode: '416'
            },
            response: {
                isSuccessful: false,
                statusCode: '416'
            }
        }

        let realTableClient = new azure.AzureTableClient('testTable1');
        let faultyTableClient = new faults.FaultyAzureTableClient(realTableClient, faultSettings);

        let store = new azure.AzureBotStorage(options, faultyTableClient);
        
        let botStorageContext = {
            userId: 'userId1',
            conversationId: 'convId1',
            persistUserData: true,
            persistConversationData: true
        };

        let botStorageData = {
            userData: '{ Foo: non-null user data }',
            conversationData: '{ Bar: nonnull conversation data }',
            privateConversationData: '{ Baz: non-null private conversation data }'
        }

        store.saveData(botStorageContext, botStorageData, function(error){
            if(error){
                assert.notEqual(-1, error.message.indexOf(faultSettings.response.statusCode));
                assert.notEqual(-1, error.message.indexOf(faultSettings.error.message));
                assert.notEqual(-1, error.message.indexOf(faultSettings.error.code));
                done();
            }
            else{
               assert.fail('The call to saveData should not have succeeded');
               done();
            }
        });    
    });

    it('when retrieve fails using gzip, an error with the status code and message should be returned', function(done) {
        let options = {
            gzipData: true,
        };
        let faultSettings = {
            shouldFailInsert: false,
            shouldFailInitialize: false,
            shouldFailRetrieve: true,
            error: {
                code: 'TEST_CODE',
                message: 'Failed to create table',
                statusCode: '416'
            },
            response: {
                isSuccessful: false,
                statusCode: '416'
            }
        }

        let realTableClient = new azure.AzureTableClient('testTable1');
        let faultyTableClient = new faults.FaultyAzureTableClient(realTableClient, faultSettings);

        let store = new azure.AzureBotStorage(options, faultyTableClient);
        
        let botStorageContext = {
            userId: 'userId1',
            conversationId: 'convId1',
            persistUserData: true,
            persistConversationData: true
        };

        let botStorageData = {
            userData: '{ Foo: non-null user data }',
            conversationData: '{ Bar: nonnull conversation data }',
            privateConversationData: '{ Baz: non-null private conversation data }'
        }

        store.saveData(botStorageContext, botStorageData, function(error){
            if(error){
                done(error);
            }
            else{
                store.getData(botStorageContext, function(error, data){
                    if(error){
                        assert.notEqual(-1, error.message.indexOf(faultSettings.response.statusCode));
                        assert.notEqual(-1, error.message.indexOf(faultSettings.error.message));
                        assert.notEqual(-1, error.message.indexOf(faultSettings.error.code));
                        done();
                    }
                    else{
                        assert.fail('The call to getData should not have succeeded');
                        done();
                    }
                })
            }
        });    
    });

    it('when saving data fails not using gzip, an error with the status code and message should be returned', function(done) {
        let options = {
            gzipData: false,
        };
        let faultSettings = {
            shouldFailInsert: true,
            shouldFailInitialize: false,
            shouldFailRetrieve: false,
            error: {
                code: 'TEST_CODE',
                message: 'Failed to create table',
                statusCode: '416'
            },
            response: {
                isSuccessful: false,
                statusCode: '416'
            }
        }

        let realTableClient = new azure.AzureTableClient('testTable1');
        let faultyTableClient = new faults.FaultyAzureTableClient(realTableClient, faultSettings);

        let store = new azure.AzureBotStorage(options, faultyTableClient);
        
        let botStorageContext = {
            userId: 'userId1',
            conversationId: 'convId1',
            persistUserData: true,
            persistConversationData: true
        };

        let botStorageData = {
            userData: '{ Foo: non-null user data }',
            conversationData: '{ Bar: nonnull conversation data }',
            privateConversationData: '{ Baz: non-null private conversation data }'
        }

        store.saveData(botStorageContext, botStorageData, function(error){
            if(error){
                assert.notEqual(-1, error.message.indexOf(faultSettings.response.statusCode));
                assert.notEqual(-1, error.message.indexOf(faultSettings.error.message));
                assert.notEqual(-1, error.message.indexOf(faultSettings.error.code));
                done();
            }
            else{
               assert.fail('The call to saveData should not have succeeded');
               done();
            }
        });    
    });

    it('when retrieve fails not using gzip, an error with the status code and message should be returned', function(done) {
        let options = {
            gzipData: false,
        };
        let faultSettings = {
            shouldFailInsert: false,
            shouldFailInitialize: false,
            shouldFailRetrieve: true,
            error: {
                code: 'TEST_CODE',
                message: 'Failed to create table',
                statusCode: '416'
            },
            response: {
                isSuccessful: false,
                statusCode: '416'
            }
        }

        let realTableClient = new azure.AzureTableClient('testTable1');
        let faultyTableClient = new faults.FaultyAzureTableClient(realTableClient, faultSettings);

        let store = new azure.AzureBotStorage(options, faultyTableClient);
        
        let botStorageContext = {
            userId: 'userId1',
            conversationId: 'convId1',
            persistUserData: true,
            persistConversationData: true
        };

        let botStorageData = {
            userData: '{ Foo: non-null user data }',
            conversationData: '{ Bar: nonnull conversation data }',
            privateConversationData: '{ Baz: non-null private conversation data }'
        }

        store.saveData(botStorageContext, botStorageData, function(error){
            if(error){
                done(error);
            }
            else{
                store.getData(botStorageContext, function(error, data){
                    if(error){
                        assert.notEqual(-1, error.message.indexOf(faultSettings.response.statusCode));
                        assert.notEqual(-1, error.message.indexOf(faultSettings.error.message));
                        assert.notEqual(-1, error.message.indexOf(faultSettings.error.code));
                        done();
                    }
                    else{
                        assert.fail('The call to getData should not have succeeded');
                        done();
                    }
                })
            }
        });    
    });
});