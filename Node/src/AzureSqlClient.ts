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
import Consts = require('./Consts');
import { Request, Connection, TYPES, ConnectionOptions, ConnectionConfig } from "tedious";

/**
 * IAzureSqlConfiguration extends ConnectionConfig to take IAzureSqlOptions
 */
export interface IAzureSqlConfiguration extends ConnectionConfig {
    /**
     * IAzureSqlOptions which extends ConnectionOptions.
     * Includes "table" parameter
     */
    options: IAzureSqlOptions;
    /**
     * Flag to set if user wishes BotBuilder-Azure to create specified table if it doesn't exist.
     * By default is set to false.
     */
    enforceTable: boolean;
}

/**
 * IAzureSqlOptions extends ConnectionOptions to include "table" and custom note for "encrypt".
 */
export interface IAzureSqlOptions extends ConnectionOptions {
    /**
     * Table name must be included.
     */
    table: string;
    /**
     * "encrypt" MUST be set to true to work with Azure SQL
     */
    encrypt?: boolean; 
}

export class AzureSqlClient implements IStorageClient {

    constructor(private options: IAzureSqlConfiguration) {
        if (typeof options.enforceTable == 'boolean') {
            this.options.enforceTable = options.enforceTable;
        } else {
            this.options.enforceTable = false;
        }
    }

    /** Initializes the SQL Server client */
    public initialize(callback: (error: any) => void): void {

        let client = new Connection(this.options);
        client.on('connect', (error: any): void => {
            if (error) {
                callback(AzureSqlClient.getError(error));
            } else {
                let checkTableRequest = new Request(`IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.Tables WHERE TABLE_NAME = N'${this.options.options.table}') BEGIN SELECT TOP 1 * FROM ${this.options.options.table} END`,
                    (error: Error, rowCount: number, rows: any[]) => {
                        if (error) {
                            client.close();
                            callback(AzureSqlClient.getError(error));
                        } else if (!rowCount) {
                            if (!this.options.enforceTable) {
                                let error = new Error(`Table "${this.options.options.table}" has not been found. Please create your Table before connecting your bot to it or set "enforceTable" to true in your AzureSqlClient configuration to create the table if it does not exist.`);
                                client.close();
                                callback(AzureSqlClient.getError(error));
                            } else {
                                let createTableRequest = new Request(`CREATE TABLE ${this.options.options.table} (id NVARCHAR(200), data NVARCHAR(1000), isCompressed BIT)`,
                                (error: Error, rowCount: number, rows: any[]) => {
                                    client.close();
                                    callback(AzureSqlClient.getError(error));
                                });
                                client.execSql(createTableRequest);
                            }
                        } else {
                            client.close();
                            callback(null);
                        } 
                    })
                client.execSql(checkTableRequest);
            }
        });
    }

    /** Inserts or replaces an entity in the table */
    public insertOrReplace(partitionKey: string, rowKey: string, entity: any, isCompressed: boolean, callback: (error: any, etag: any, response: IHttpResponse) => void): void {
        
        let client = new Connection(this.options); 
        client.on('connect', (error: Error) => {
            if (error) {
                callback(AzureSqlClient.getError(error), null, null);
            } else {
                let getRequest = new Request(`SELECT TOP 1 * FROM ${this.options.options.table} WHERE id=@id`,
                (err: Error, rowCount: number, rows: any[]) => {
                    if (err) {
                        client.close();
                        callback(AzureSqlClient.getError(err), null, null);
                    } else {
                        if (rowCount) {
                            let updateRequest = new Request(`UPDATE ${this.options.options.table} SET data=@data, isCompressed=@isCompressed WHERE id=@id`,
                            (error: Error, rowCount: number, rows: any[]) => {
                                if (error) {
                                    client.close();
                                    callback(AzureSqlClient.getError(error), null, null);
                                } else {
                                    client.close();
                                    callback(null, rows[0], rows[0]);
                                }
                            });
                            AzureSqlClient.addParameters(updateRequest, completeId, stringifiedEntity, isCompressed);
                            client.execSql(updateRequest);
                        } else {
                            let insertRequest = new Request(`INSERT INTO ${this.options.options.table} (id, data, isCompressed) VALUES (@id, @data, @isCompressed)`, 
                            (error: Error, rowCount: number, rows: any[]) => {
                                if (error) {
                                    client.close();
                                    callback(AzureSqlClient.getError(error), null, null);
                                } else {
                                    client.close();
                                    callback(null, rows[0], rows[0]);
                                }
                            });
                            AzureSqlClient.addParameters(insertRequest, completeId, stringifiedEntity, isCompressed);
                            client.execSql(insertRequest);
                        }
                    }
                });
                let completeId: string = partitionKey + ',' + rowKey;
                let stringifiedEntity: string = JSON.stringify(entity);
                AzureSqlClient.addParameters(getRequest, completeId, stringifiedEntity, isCompressed);
                client.execSql(getRequest);
            }
        });
    }

    /** Retrieves an entity from the table */
    public retrieve(partitionKey: string, rowKey: string, callback: (error: any, entity: IBotEntity, response: IHttpResponse) => void): void {

        let client = new Connection(this.options);
        client.on('connect', (error: Error): void => {
            if (error) {
                callback(AzureSqlClient.getError(error), null, null);
            } else {
                let request = new Request(`SELECT TOP 1 * FROM ${this.options.options.table} WHERE id=@id`, 
                (err: Error, rowCount: number, rows: any[]) => {
                    if (err) {
                        client.close();
                        callback(AzureSqlClient.getError(err), null, null);
                    } else if (!rowCount) {
                        client.close();
                        callback(null, null, null);
                    } else {
                        client.close();
                        let row: any = rows[0];
                        callback(null, <IBotEntity>row, rows[0]);
                    }
                });
                let id = partitionKey + ',' + rowKey;
                AzureSqlClient.addParameters(request, id);
                client.execSql(request);
            }
        });
    }
    
    private static getError(error: any): Error {
        if(!error) return null;
        return new Error('Error Code: ' + error.code + ' Error Message: ' + error.message);
    }
    
    private static addParameters (request: Request, id: string, data?: string, isCompressed?: boolean): void {
        request.addParameter('id', TYPES.NVarChar, id); 
        request.addParameter('data', TYPES.NVarChar, data);
        request.addParameter('isCompressed', TYPES.Bit, isCompressed);
    }
}