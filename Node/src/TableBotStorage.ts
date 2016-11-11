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

import * as builder from 'botbuilder';
import * as async from 'async';
import * as Promise from 'promise';

import zlib = require('zlib');

import { IAzureTableClient } from './AzureTableClient';
import { AzureTableClient } from './AzureTableClient';
import { IHttpResponse } from './AzureTableClient';

var azure = require('azure-storage');

var MAX_DATA_LENGTH = 65000;
var USER_DATA_FIELD = 'userData';
var CONVERSATION_DATA_FIELD = 'conversationData';
var PRIVATE_CONVERSATION_DATA_FIELD = 'privateConversationData';
var TABLE_NAME = 'BotStore';
var HASH = 'Hash';
var BASE_64 = 'base64';
var ERROR_MESSAGE_SIZE = 'EMSGSIZE';
var ERROR_BAD_MESSAGE = 'EBADMSG';

export interface ITableBotStorageOptions {
    /** If true the data will be gzipped prior to writing to storage. */
    gzipData?: boolean;
    /** Storage account name used to persist bot data. */
    accountName: string;
    /** Storage account key used to persist bot data. */
    accountKey: string;   
}

export class TableBotStorage implements builder.IBotStorage {

    private readonly azureTableClient: IAzureTableClient;
    private readonly botStorageTableName: string = TABLE_NAME;
    private readonly settings: ITableBotStorageOptions;

    private initializeTableClientPromise: Promise.IThenable<boolean>;
    private tableClientInitialized: boolean;

    constructor(private options: ITableBotStorageOptions, tableClient?: IAzureTableClient) {

        this.settings = options;

        if(!tableClient){
            // If no client was injected, use the default implementation
            this.azureTableClient = new AzureTableClient(this.botStorageTableName, options.accountName, options.accountKey);
        }
        else {
            // A table client was injected, use it as underlying store
            this.azureTableClient = tableClient;
        }
    }

    /** Reads in data from storage. */
    public getData(context: builder.IBotStorageContext, callback: (err: Error, data: builder.IBotStorageData) => void): void {
        try {
            // We initialize on every call, but only block on the first call. The reason for this is that we can't run asynchronous initialization in the class ctor
            let promise = this.initializeTableClient();
            promise.done(() => {
                // Build list of read commands
                var list: any[] = [];
                if (context.userId) {
                    // Read userData
                    if (context.persistUserData) {
                        list.push({ 
                            partitionKey: context.userId, 
                            rowKey: USER_DATA_FIELD,
                            field: USER_DATA_FIELD
                        });
                    }
                    if (context.conversationId) {
                        // Read privateConversationData
                        list.push({
                            partitionKey: context.conversationId, 
                            rowKey: context.userId,
                            field: PRIVATE_CONVERSATION_DATA_FIELD
                        });
                    }
                }
                if (context.persistConversationData && context.conversationId) {
                    // Read conversationData
                    list.push({ 
                        partitionKey: context.conversationId, 
                        rowKey: CONVERSATION_DATA_FIELD,
                        field: CONVERSATION_DATA_FIELD
                    });
                }

                // Execute reads in parallel
                var data: builder.IBotStorageData = {};
                async.each(list, (entry, cb) => {

                    this.azureTableClient.retrieve(entry.partitionKey, entry.rowKey, function(error: any, entity: any, response: IHttpResponse){
                        if (!error) {
                            let botData = entity.Data['_'] ? entity.Data['_'] : {};
                            let isCompressed = entity.IsCompressed['_'];
                             
                            if (isCompressed) {
                                // Decompress gzipped data
                                zlib.gunzip(new Buffer(botData, BASE_64), (err, result) => {
                                    if (!err) {
                                        try {
                                            var txt = result.toString();
                                            (<any>data)[entry.field + HASH] = txt;
                                            (<any>data)[entry.field] = JSON.parse(txt);
                                        } catch (e) {
                                            err = e;
                                        }
                                    }
                                    cb(err);
                                });
                            } else {
                                try {
                                    (<any>data)[entry.field + HASH] = JSON.stringify(botData);
                                    (<any>data)[entry.field] = botData;
                                } catch (e) {
                                    error = e;
                                }
                                cb(error);
                            }
                        } else {
                            cb(error);
                        }
                    });
                }, (err) => {
                    if (!err) {
                        callback(null, data);
                    } else {
                        var m = err.toString();
                        callback(err instanceof Error ? err : new Error(m), null);
                    }
                });
            }, (err) => callback(err, null));
        } catch (e) {
            callback(e instanceof Error ? e : new Error(e.toString()), null);
        }
    }
    
    /** Writes out data to storage. */
    public saveData(context: builder.IBotStorageContext, data: builder.IBotStorageData, callback?: (err: Error) => void): void {
       
        // We initialize on every call, but only block on the first call. The reason for this is that we can't run asynchronous initialization in the class ctor
        let promise = this.initializeTableClient();
        promise.done(() => {

            var list: any[] = [];
            
            function addWrite(field: string, partitionKey: string, rowKey: string, botData: any) {
                var hashKey = field + HASH; 
                var hash = JSON.stringify(botData);
                if (!(<any>data)[hashKey] || hash !== (<any>data)[hashKey]) {
                    (<any>data)[hashKey] = hash;
                    list.push({ field: field, partitionKey: partitionKey, rowKey: rowKey, botData: botData, hash: hash });
                }
            }

            try {
                // Build list of write commands
                if (context.userId) {
                    if (context.persistUserData) {
                        // Write userData
                        addWrite(USER_DATA_FIELD, context.userId, USER_DATA_FIELD, data.userData || {});
                    }
                    if (context.conversationId) {
                        // Write privateConversationData
                        addWrite(PRIVATE_CONVERSATION_DATA_FIELD, context.conversationId, context.userId, data.privateConversationData || {});
                    }
                }
                if (context.persistConversationData && context.conversationId) {
                    // Write conversationData
                    addWrite(PRIVATE_CONVERSATION_DATA_FIELD, context.conversationId, PRIVATE_CONVERSATION_DATA_FIELD, data.conversationData || {});
                }

                // Execute writes in parallel
                async.each(list, (entry, errorCallback) => {
                    if (this.settings.gzipData) {
                        zlib.gzip(entry.hash, (err, result) => {
                            if (!err && result.length > MAX_DATA_LENGTH) {
                                err = new Error("Data of " + result.length + " bytes gzipped exceeds the " + MAX_DATA_LENGTH + " byte limit. Can't post to: " + entry.url);
                                (<any>err).code = ERROR_MESSAGE_SIZE;
                            }
                            if (!err) {
                                //Insert gzipped entry
                                this.azureTableClient.insertOrReplace(entry.partitionKey, entry.rowKey, result.toString('base64'), true, function(error: any, eTag: any, response: IHttpResponse){
                                    errorCallback(error);
                                });
                            } else {
                                errorCallback(err);
                            }
                        });
                    } else if (entry.hash.length < MAX_DATA_LENGTH) {
                        this.azureTableClient.insertOrReplace(entry.partitionKey, entry.rowKey, entry.botData, false, function(error: any, eTag: any, response: IHttpResponse){
                            errorCallback(error);
                        });
                    } else {
                        var err = new Error("Data of " + entry.hash.length + " bytes exceeds the " + MAX_DATA_LENGTH + " byte limit. Consider setting connectors gzipData option. Can't post to: " + entry.url);
                        (<any>err).code = ERROR_MESSAGE_SIZE;
                        errorCallback(err);
                    }
                }, (err) => {
                    if (callback) {
                        if (!err) {
                            callback(null);
                        } else {
                            var m = err.toString();
                            callback(err instanceof Error ? err : new Error(m));
                        }
                    }
                });
            } catch (e) {
                if (callback) {
                    var err = e instanceof Error ? e : new Error(e.toString());
                    (<any>err).code = ERROR_BAD_MESSAGE;
                    callback(err);
                }
            }
        }, (err) => callback(err));
    }

    private initializeTableClient(): Promise.IThenable<boolean>{
        if(!this.initializeTableClientPromise)
        {
            // The first call will trigger the initialization of the table client, which creates the Azure table if it 
            // does not exist. Subsequent calls will not block.
            this.initializeTableClientPromise = new Promise<boolean>((resolve, reject) => {
                this.azureTableClient.initialize(function(error: any){
                    if(error){
                        reject(new Error('Failed to initialize azure table client. Error: ' + error.toString()))
                    }
                    else{
                        resolve(true);
                    }
                });
            });
        }
        return this.initializeTableClientPromise;
    }
}