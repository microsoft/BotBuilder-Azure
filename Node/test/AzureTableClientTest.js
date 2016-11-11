var azure = require('../');
var assert = require('assert');

describe('AzureTableClient', function() {
    
    it('should write and read a valid entity', function(done) {
        
        var partitionKey = 'pk';
        var rowKey = 'rk';
        var data = 'data';

        var client = new azure.AzureTableClient('testTable1');
        client.initialize(function(error){
            console.log(error);
            if(error){
                done(error);
            }
            else {
                client.insertOrReplace(partitionKey, rowKey, data, function(error, etag, response){
                    if(error){
                        done(error);
                    }
                    else{
                        client.retrieve(partitionKey, rowKey, function(error, entity, response){
                            if(error){
                                done(error);
                            }
                            else{
                                assert.equal(data, entity.Data['_']);
                                done();
                            }
                        });
                    }
                });
            }
        });
    });

    it('should write and replace a valid entity', function(done) {
        
        var partitionKey = 'pk';
        var rowKey = 'rk';
        var data = 'data';
        var data2 = 'data2';

        var client = new azure.AzureTableClient('testTable1');
        client.initialize(function(error){
            console.log(error);
            if(error){
                done(error);
            }
            else {
                client.insertOrReplace(partitionKey, rowKey, data, function(error, etag, response){
                    if(error){
                        done(error);
                    }
                    else{
                        client.insertOrReplace(partitionKey, rowKey, data2, function(error, etag, response){
                            if(error){
                                done(error);
                            }
                            else{
                                client.retrieve(partitionKey, rowKey, function(error, entity, response){
                                    if(error){
                                        done(error);
                                    }
                                    else{
                                        assert.equal(data2, entity.Data['_']);
                                        done();
                                    }
                                });
                            }
                        });
                    }
                });
            }
        });
    });
});