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
import Consts = require('./Consts');
import zlib = require('zlib');

import { IStorageClient, IHttpResponse } from './IStorageClient';
import { AzureTableClient } from './AzureTableClient';

var azure = require('azure-storage');

export interface IAzureBotStorageOptions {
    /** If true the data will be gzipped prior to writing to storage. */
    gzipData?: boolean;
}

export class AzureBotStorage implements builder.IBotStorage {

    private initializeTableClientPromise: Promise<boolean>;
    private storageClientInitialized: boolean;

    constructor(private options: IAzureBotStorageOptions, private storageClient?: IStorageClient) { }
    
    public client(storageClient: IStorageClient) : this {
        this.storageClient = storageClient;
        return this;
    }

    /** Reads in data from storage. */
    public getData(context: builder.IBotStorageContext, callback: (err: Error, data: builder.IBotStorageData) => void): void {
        // We initialize on every call, but only block on the first call. The reason for this is that we can't run asynchronous initialization in the class ctor
        this.initializeStorageClient().done(() => {
            // Build list of read commands
            var list: any[] = [];
            if (context.userId) {
                // Read userData
                if (context.persistUserData) {
                    list.push({ 
                        partitionKey: context.userId, 
                        rowKey: Consts.Fields.UserDataField,
                        field: Consts.Fields.UserDataField
                    });
                }
                if (context.conversationId) {
                    // Read privateConversationData
                    list.push({
                        partitionKey: context.conversationId, 
                        rowKey: context.userId,
                        field: Consts.Fields.PrivateConversationDataField
                    });
                }
            }
            if (context.persistConversationData && context.conversationId) {
                // Read conversationData
                list.push({ 
                    partitionKey: context.conversationId, 
                    rowKey: Consts.Fields.ConversationDataField,
                    field: Consts.Fields.ConversationDataField
                });
            }

            // Execute reads in parallel
            var data: builder.IBotStorageData = {};
            async.each(list, (entry, cb) => {

                this.storageClient.retrieve(entry.partitionKey, entry.rowKey, function(error: any, entity: any, response: IHttpResponse){
                    if (!error) {
                        if(entity) {
                            let botData = entity.data || {};
                            let isCompressed = entity.isCompressed || false;
                                
                            if (isCompressed) {
                                // Decompress gzipped data
                                zlib.gunzip(new Buffer(botData, Consts.base64), (err, result) => {
                                    if (!err) {
                                        try {
                                            var txt = result.toString();
                                            (<any>data)[entry.field + Consts.hash] = txt;
                                            (<any>data)[entry.field] = txt != null ? JSON.parse(txt) : null;
                                        } catch (e) {
                                            err = e;
                                        }
                                    }
                                    cb(err);
                                });
                            } else {
                                try {
                                    (<any>data)[entry.field + Consts.hash] = botData ? JSON.stringify(botData) : null ;
                                    (<any>data)[entry.field] = botData != null ? botData : null;
                                } catch (e) {
                                    error = e;
                                }
                                cb(error);
                            }
                        } else {
                            (<any>data)[entry.field + Consts.hash] = null;
                            (<any>data)[entry.field] = null;
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
    }
    
    /** Writes out data to storage. */
    public saveData(context: builder.IBotStorageContext, data: builder.IBotStorageData, callback?: (err: Error) => void): void {
       
        // We initialize on every call, but only block on the first call. The reason for this is that we can't run asynchronous initialization in the class ctor
        let promise = this.initializeStorageClient();
        promise.done(() => {

            var list: any[] = [];
            
            function addWrite(field: string, partitionKey: string, rowKey: string, botData: any) {
                let hashKey = field + Consts.hash; 
                let hash = JSON.stringify(botData);
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
                        addWrite(Consts.Fields.UserDataField, context.userId, Consts.Fields.UserDataField, data.userData);
                    }
                    if (context.conversationId) {
                        // Write privateConversationData
                        addWrite(Consts.Fields.PrivateConversationDataField, context.conversationId, context.userId, data.privateConversationData);
                    }
                }
                if (context.persistConversationData && context.conversationId) {
                    // Write conversationData
                    addWrite(Consts.Fields.ConversationDataField, context.conversationId, Consts.Fields.ConversationDataField, data.conversationData);
                }

                // Execute writes in parallel
                async.each(list, (entry, errorCallback) => {
                    if (this.options.gzipData) {
                        zlib.gzip(entry.hash, (err, result) => {
                            if (!err && result.length > Consts.maxDataLength) {
                                err = new Error("Data of " + result.length + " bytes gzipped exceeds the " + Consts.maxDataLength + " byte limit. Can't post to: " + entry.url);
                                (<any>err).code = Consts.ErrorCodes.MessageSize;
                            }
                            if (!err) {
                                //Insert gzipped entry
                                this.storageClient.insertOrReplace(entry.partitionKey, entry.rowKey, result.toString('base64'), true, function(error: any, eTag: any, response: IHttpResponse){
                                    errorCallback(error);
                                });
                            } else {
                                errorCallback(err);
                            }
                        });
                    } else if (entry.hash.length < Consts.maxDataLength) {
                        this.storageClient.insertOrReplace(entry.partitionKey, entry.rowKey, entry.botData, false, function(error: any, eTag: any, response: IHttpResponse){
                            errorCallback(error);
                        });
                    } else {
                        var err = new Error("Data of " + entry.hash.length + " bytes exceeds the " + Consts.maxDataLength + " byte limit. Consider setting connectors gzipData option. Can't post to: " + entry.url);
                        (<any>err).code = Consts.ErrorCodes.MessageSize;
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
                    (<any>err).code = Consts.ErrorCodes.BadMessage;
                    callback(err);
                }
            }
        }, (err) => callback(err));
    }

    private initializeStorageClient(): Promise<boolean>{
        if(!this.initializeTableClientPromise)
        {
            // The first call will trigger the initialization of the table client, which creates the Azure table if it 
            // does not exist. Subsequent calls will not block.
            this.initializeTableClientPromise = new Promise<boolean>((resolve, reject) => {
                this.storageClient.initialize(function(error: any){
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
