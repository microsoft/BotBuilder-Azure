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
import * as builderazure from '../../';

export class FaultyAzureTableClient implements builderazure.IAzureTableClient {

    private readonly tableClient: builderazure.IAzureTableClient;
    private readonly faultSettings: IFaultSettings;

    constructor(client: builderazure.IAzureTableClient, faultSettings: IFaultSettings) {
        this.tableClient = client;
        this.faultSettings = faultSettings;
    }

    public initialize(callback: (error: Error) => void): void {
        
        if(this.faultSettings.shouldFailInitialize){
            callback((<any>builderazure.AzureTableClient).getError(this.faultSettings.error, this.faultSettings.response));
        } 
        else {
            this.tableClient.initialize(callback);
        }
    }

    public insertOrReplace(partitionKey: string, rowKey: string, data: string, isCompressed: boolean, callback: (error: Error, etag: any, response: builderazure.IHttpResponse) => void): void {
        
        if(this.faultSettings.shouldFailInsert){
            callback((<any>builderazure.AzureTableClient).getError(this.faultSettings.error, this.faultSettings.response), null, this.faultSettings.response);
        } 
        else {
            this.tableClient.insertOrReplace(partitionKey, rowKey, data, isCompressed, callback);
        }
    }

    public retrieve(partitionKey: string, rowKey: string, callback: (error: Error, entity: any, response: builderazure.IHttpResponse) => void): void {
        
        if(this.faultSettings.shouldFailRetrieve){
            callback((<any>builderazure.AzureTableClient).getError(this.faultSettings.error, this.faultSettings.response), null, this.faultSettings.response);
        } 
        else {
            this.tableClient.retrieve(partitionKey, rowKey, callback);
        }
    }
}

export interface IFaultSettings {
    shouldFailInsert: boolean;
    shouldFailInitialize: boolean;
    shouldFailRetrieve: boolean;
    error: builderazure.IStorageError;
    response: builderazure.IHttpResponse;
}