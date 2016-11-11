var azure = require('../');
var assert = require('assert');
var builder = require('botbuilder');

describe('TableBotStorageTest', function() {
    it('data should be properly written and read from azure table when requesting gzip, requesting to store user and conversation data, and providing conversation and user ids', function(done) {
        let options = {
            gzipData: true,
        };

        let store = new azure.TableBotStorage(options);
        
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
                        done(error);
                    }
                    else{
                        assert.equal(data.userData, botStorageData.userData);
                        assert.equal(data.conversationData, botStorageData.conversationData);
                        assert.equal(data.privateConversationData, botStorageData.privateConversationData);
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

        let store = new azure.TableBotStorage(options);
        
        let botStorageContext = {
            userId: 'userId2',
            conversationId: 'convId2',
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
                        done(error);
                    }
                    else{
                        assert.equal(data.userData, botStorageData.userData);
                        assert.equal(data.conversationData, botStorageData.conversationData);
                        assert.equal(data.privateConversationData, botStorageData.privateConversationData);
                        done();
                    }
                })
            }
        });    
    });
});