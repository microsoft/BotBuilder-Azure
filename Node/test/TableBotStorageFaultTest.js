var azure = require('../');
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
        let faultyTableClient = new azure.FaultyAzureTableClient(realTableClient, faultSettings);

        let store = new azure.TableBotStorage(options, faultyTableClient);
        
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
        let faultyTableClient = new azure.FaultyAzureTableClient(realTableClient, faultSettings);

        let store = new azure.TableBotStorage(options, faultyTableClient);
        
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
        let faultyTableClient = new azure.FaultyAzureTableClient(realTableClient, faultSettings);

        let store = new azure.TableBotStorage(options, faultyTableClient);
        
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
        let faultyTableClient = new azure.FaultyAzureTableClient(realTableClient, faultSettings);

        let store = new azure.TableBotStorage(options, faultyTableClient);
        
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
        let faultyTableClient = new azure.FaultyAzureTableClient(realTableClient, faultSettings);

        let store = new azure.TableBotStorage(options, faultyTableClient);
        
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