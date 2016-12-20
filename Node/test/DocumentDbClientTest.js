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

describe('DocumentDbClient', function() {
    
    it('should write and read a valid entity', function(done) {
        
        var partitionKey = 'pk';
        var rowKey = 'rk';
        var data = { field1: 'data', field2: 3};

        const options = {
            host: 'https://localhost:8081',
            masterKey: 'C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==',
            database: 'testdb',
            collection: 'testcollection'
        };
        var client = new azure.DocumentDbClient(options);

        client.initialize(function(error){
             console.log(error);
            if(error){
                done(error);
            }
            else {
                client.insertOrReplace(partitionKey, rowKey, data, false, function(error, etag, response){
                    if(error){
                        done(error);
                    }
                    else{
                        client.retrieve(partitionKey, rowKey, function(error, entity, response){
                            if(error){
                                done(error);
                            }
                            else{
                                assert.deepEqual(data, entity.data);
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
        var data = { field1: 'data', field2: 3};
        var data2 = { field1: 'data2', field2: 3};

        var client = new azure.AzureTableClient('testTable1');
        client.initialize(function(error){
            console.log(error);
            if(error){
                done(error);
            }
            else {
                client.insertOrReplace(partitionKey, rowKey, data, false, function(error, etag, response){
                    if(error){
                        done(error);
                    }
                    else{
                        client.insertOrReplace(partitionKey, rowKey, data2, false, function(error, etag, response){
                            if(error){
                                done(error);
                            }
                            else{
                                client.retrieve(partitionKey, rowKey, function(error, entity, response){
                                    if(error){
                                        done(error);
                                    }
                                    else{
                                        assert.deepEqual(data2, entity.data);
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

    it('should return null when retrieving a non-existing entity', function(done) {
        
        var partitionKey = Math.floor((Math.random() * 100000000) + 1).toString();
        var rowKey = 'rk';

        var client = new azure.AzureTableClient('testTable1');
        client.initialize(function(error){
            console.log(error);
            if(error){
                done(error);
            }
            else {
                 client.retrieve(partitionKey, rowKey, function(error, entity, response){
                    if(error){
                        done(error);
                    }
                    else{
                        assert.equal(null, entity);
                        done();
                    }
                });
            }
        });
    });
});