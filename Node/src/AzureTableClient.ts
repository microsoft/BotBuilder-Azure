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

import { IStorageError, IStorageClient, IHttpResponse, IBotEntity } from './IStorageClient';

import * as builder from 'botbuilder';
import * as async from 'async';
import Consts = require('./Consts');

var azure = require('azure-storage');

export interface IBotTableEntity extends IBotEntity {
    partitionKey: string;
    rowKey: string;
}

export class AzureTableClient implements IStorageClient {

    private readonly connectionString: string;
    private readonly accountName: string;
    private readonly accountKey: string;
    private readonly tableName: string;
    private readonly useDevelopmentStorage: boolean;

    constructor(tableName: string, accountName?: string, accountKey?: string) {
        
        // Development storage is used if no accountName and key are provided
        if(!accountName && !accountKey){
            this.useDevelopmentStorage = true;
        }
        // If only account name is provided, we assume it is a full connection string
        else if(accountName && !accountKey){
            this.connectionString = accountName;
        }

        // If no account info is provided, we error out
        else if(!accountName || !accountKey){
            throw Error('Storage account name and account key are mandatory when not using development storage');
        }

        this.accountName = accountName;
        this.accountKey = accountKey;
        this.tableName = tableName;
    }

    /** Initializes the Azure Table client */
    public initialize(callback: (error: Error) => void): void {
        let tableService = this.buildTableService();

        tableService.createTableIfNotExists(this.tableName, function(error : IStorageError, result: any, response: IHttpResponse) {
            callback(AzureTableClient.getError(error, response));
        });
    }

    /** Inserts or replaces an entity in the table */
    public insertOrReplace(partitionKey: string, rowKey: string, data: any, isCompressed: boolean, callback: (error: Error, etag: any, response: IHttpResponse) => void): void {
        let tableService = this.buildTableService();

        let entityGenerator = azure.TableUtilities.entityGenerator;

        let entity = {
            PartitionKey: entityGenerator.String(partitionKey),
            RowKey: entityGenerator.String(rowKey),
            Data: entityGenerator.String((data instanceof String) ? data : JSON.stringify(data)),
            IsCompressed: entityGenerator.Boolean(isCompressed)
        };
 
        tableService.insertOrReplaceEntity(this.tableName, entity, { checkEtag: false }, function(error: IStorageError, result: any, response: IHttpResponse){
            callback(AzureTableClient.getError(error, response), result, response);
        });
    }

    /** Retrieves an entity from the table */
    public retrieve(partitionKey: string, rowKey: string, callback: (error: Error, entity: IBotEntity, response: IHttpResponse) => void): void {
        let tableService = this.buildTableService();

        tableService.retrieveEntity(this.tableName, partitionKey, rowKey, function(error: IStorageError, result: any, response: IHttpResponse){
            //404 on retrieve means the entity does not exist. Just return null
            if(response.statusCode == Consts.HttpStatusCodes.NotFound){
                callback(null, null, response);
            } 
            else{
                callback(AzureTableClient.getError(error, response), AzureTableClient.toBotEntity(result), response);
            }            
        });
    }

    private static toBotEntity(tableResult: any): IBotTableEntity {
        if(!tableResult) {
            return null;
        }
        let entity: IBotTableEntity = {
            data: {},
            isCompressed: tableResult.IsCompressed['_'] || false,
            rowKey: tableResult.RowKey['_'] || '',
            partitionKey: tableResult.PartitionKey['_'] || ''
        };

        if(tableResult.Data['_'] && entity.isCompressed) {
            entity.data = tableResult.Data['_'];
        }
        else if(tableResult.Data['_'] && !entity.isCompressed) {
            entity.data = JSON.parse(tableResult.Data['_']);
        }

        return entity;
    }

    private buildTableService(): any {
        
        let tableService = null;

        // Dev Storage
        if (this.useDevelopmentStorage) {
            tableService = azure.createTableService(Consts.developmentConnectionString) 
        }
        // Connection string provided
        else if (this.connectionString) {
            tableService = azure.createTableService(this.connectionString)
        }
        // Account name / key
        else {
            tableService = azure.createTableService(this.accountName, this.accountKey);
        }

        return tableService.withFilter(new azure.ExponentialRetryPolicyFilter());
    }

    private static getError(error: IStorageError, response: IHttpResponse): Error {
        if(!error) return null;

        let message: string = 'Failed to perform the requested operation on Azure Table. Message: ' + error.message + '. Error code: ' + error.code;
        if(response) {
            message += '. Http status code: ';
            message += response.statusCode;
        }
        return new Error(message);
    }
}