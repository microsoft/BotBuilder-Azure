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
var assert = require('assert');
var builder = require('botbuilder');

describe('AzureBotStorageTest', function() {
    it('data should be properly written and read from azure table when requesting gzip, requesting to store user and conversation data, and providing conversation and user ids', function(done) {
        let options = {
            gzipData: true,
        };

        let store = new azure.AzureBotStorage(options)
            .client(new azure.AzureTableClient('testTable1'));
        
        let botStorageContext = {
            userId: 'userId1',
            conversationId: 'convId1',
            persistUserData: true,
            persistConversationData: true
        };

        let botStorageData = {
            userData: { Foo: 'non-null user data' },
            conversationData: { Bar: 'nonnull conversation data' },
            privateConversationData: { Baz: 'non-null private conversation data'}
        }

        store.saveData(botStorageContext, botStorageData, function(error){
            if(error){
                done(error);
            }
            else{
                store.getData(botStorageContext, function(error, data){
                    if(error){
                        done(error);
                    }
                    else{
                        assert.deepEqual(data.userData, botStorageData.userData);
                        assert.deepEqual(data.conversationData, botStorageData.conversationData);
                        assert.deepEqual(data.privateConversationData, botStorageData.privateConversationData);
                        done();
                    }
                })
            }
        });    
    });

    it('data should be properly written and read from azure table when not requesting gzip, requesting to store user and conversation data, and providing conversation and user ids', function(done) {
        let options = {
            gzipData: false,
        };

        let store = new azure.AzureBotStorage(options)
            .client(new azure.AzureTableClient('testTable1'));
        
        let botStorageContext = {
            userId: 'userId2',
            conversationId: 'convId2',
            persistUserData: true,
            persistConversationData: true
        };

        let botStorageData = {
            userData: { Foo: 'non-null user data' },
            conversationData: { Bar: 'nonnull conversation data' },
            privateConversationData: { Baz: 'non-null private conversation data'}
        }

        store.saveData(botStorageContext, botStorageData, function(error){
            if(error){
                done(error);
            }
            else{
                store.getData(botStorageContext, function(error, data){
                    if(error){
                        done(error);
                    }
                    else{
                        assert.deepEqual(data.userData, botStorageData.userData);
                        assert.deepEqual(data.conversationData, botStorageData.conversationData);
                        assert.deepEqual(data.privateConversationData, botStorageData.privateConversationData);
                        done();
                    }
                })
            }
        });    
    });

    it('when retrieving data for a new user with no data in store, and then setting data, the second retrieve should return the newly added data', function(done) {
        let options = {
            gzipData: false,
        };

        let store = new azure.AzureBotStorage(options, new azure.AzureTableClient('testTable1'));
        
        let botStorageContext = {
            userId: Math.floor((Math.random() * 100000000) + 1).toString(),
            conversationId: Math.floor((Math.random() * 100000000) + 1).toString(),
            persistUserData: true,
            persistConversationData: true
        };

        let botStorageData = {
            userData: { Foo: 'non-null user data' },
            conversationData: { Bar: 'nonnull conversation data' },
            privateConversationData: { Baz: 'non-null private conversation data'}
        }

        store.getData(botStorageContext, function(error, data){
            if(error){
                done(error);
            }
            else{
                assert.deepEqual(data.userData, null);
                assert.deepEqual(data.conversationData, null);
                assert.deepEqual(data.privateConversationData, null);
                store.saveData(botStorageContext, botStorageData, function(error){
                    if(error){
                        done(error);
                    }
                    else{
                        store.getData(botStorageContext, function(error, data){
                            if(error){
                                done(error);
                            }
                            else{
                                assert.deepEqual(data.userData, botStorageData.userData);
                                assert.deepEqual(data.conversationData, botStorageData.conversationData);
                                assert.deepEqual(data.privateConversationData, botStorageData.privateConversationData);
                                done();
                            }
                        })
                    }
                });    
            }
        })   
    });

    it('getData should work fine and return null value when there is no past data for a user or conversation', function(done) {
        let options = {
            gzipData: false,
        };

        let store = new azure.AzureBotStorage(options, new azure.AzureTableClient('testTable1'));
        
        let botStorageContext = {
            userId: Math.floor((Math.random() * 100000000) + 1).toString(),
            conversationId: Math.floor((Math.random() * 100000000) + 1).toString(),
            persistUserData: true,
            persistConversationData: true
        };

        let botStorageData = {
            userData: { Foo: 'non-null user data' },
            conversationData: { Bar: 'nonnull conversation data' },
            privateConversationData: { Baz: 'non-null private conversation data'}
        }

        store.getData(botStorageContext, function(error, data){
            if(error){
                done(error);
            }
            else{
                assert.deepEqual(data.userData, null);
                assert.deepEqual(data.conversationData, null);
                assert.deepEqual(data.privateConversationData, null);
                done();
            }
        })   
    });
});