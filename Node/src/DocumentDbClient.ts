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
import { DocumentClient, QueryError, DatabaseMeta, CollectionMeta, RetrievedDocument, QueryIterator, UniqueId, RequestOptions, RequestCallback, NewDocument, Collection, SqlQuerySpec, FeedOptions } from 'documentdb';

export interface IDocumentDbOptions {
    host: string;
    masterKey: string;
    database: string;
    collection: string;
}

export interface IDocDbEntity extends IBotEntity {
    id: string;
}

export class DocumentDbClient implements IStorageClient {

    private client: IDocumentClient;
    private database: DatabaseMeta;
    private collection: CollectionMeta;
    
    constructor(private options: IDocumentDbOptions) { }

    /** Initializes the DocumentDb client */
    public initialize(callback: (error: Error) => void): void {
        let client: any = new DocumentClient(this.options.host, { masterKey: this.options.masterKey });

        // DocumentDb public typings are not correct, so we cast to this interface to have the correct typings
        this.client = <IDocumentClient>client;

        this.getOrCreateDatabase((error: QueryError, database: DatabaseMeta) => {
            if(error) {
                callback(DocumentDbClient.getError(error));
            }
            else {
                this.database = database;
                this.getOrCreateCollection((error: QueryError, collection: CollectionMeta) => {
                    if(error) {
                        callback(DocumentDbClient.getError(error));
                    }
                    else {
                        this.collection = collection;
                        callback(null);
                    }
                })
            }
        });
    }

    /** Inserts or replaces an entity in the table */
    public insertOrReplace(partitionKey: string, rowKey: string, entity: any, isCompressed: boolean, callback: (error: Error, etag: any, response: IHttpResponse) => void): void {

        let docDbEntity: IDocDbEntity = { id: partitionKey + ',' + rowKey, data: entity, isCompressed: isCompressed };

        this.client.upsertDocument(this.collection._self, docDbEntity, {}, (error: QueryError, collection: RetrievedDocument<IDocDbEntity>, responseHeaders: any): void => {
            callback(DocumentDbClient.getError(error), null, responseHeaders);
        });
    }

    /** Retrieves an entity from the table */
    public retrieve(partitionKey: string, rowKey: string, callback: (error: Error, entity: IBotEntity, response: IHttpResponse) => void): void {

        let id = partitionKey + ',' + rowKey;
        let querySpec = {
            query: Consts.DocDbRootQuery,
            parameters: [{
                name: Consts.DocDbIdParam,
                value: id
            }]
        };

        let iterator: QueryIterator<RetrievedDocument<IDocDbEntity>> = this.client.queryDocuments(this.collection._self, querySpec, {});

        iterator.toArray((error: QueryError, result: RetrievedDocument<IDocDbEntity>[], responseHeaders?: any): void => {
            if(error) {
                callback(DocumentDbClient.getError(error), null, null);
            }
            else if(result.length == 0) {
                callback(null, null, null);
            }
            else {
                let document: any = result[0];
                callback(null, <IBotEntity>document, null);
            }
        });
    }

    private static getError(error: QueryError): Error {
        if(!error) return null;
        return new Error('Error Code: ' + error.code + ' Error Body: ' + error.body);
    }

    private getOrCreateDatabase(callback: (error: QueryError, database: DatabaseMeta) => void): void {

        let querySpec = {
            query: Consts.DocDbRootQuery,
            parameters: [{
                name: Consts.DocDbIdParam,
                value: this.options.database
            }]
        };

        this.client.queryDatabases(querySpec).toArray((error: QueryError, result: DatabaseMeta[], responseHeaders?: any) => {
            if(error) {
                callback(error, null);
            }
            else if(result.length == 0) {
                this.client.createDatabase({id: this.options.database}, {}, (error: QueryError, database: DatabaseMeta): void => {
                    if(error) {
                        callback(error, null);
                    }
                    else {
                        callback(null, database);
                    }
                });
            }
            else {
                callback(null, result[0]);
            }
        });
    }

    private getOrCreateCollection(callback: (error: QueryError, collection: CollectionMeta) => void): void {

        let querySpec = {
            query: Consts.DocDbRootQuery,
            parameters: [{
                name: Consts.DocDbIdParam,
                value: this.options.collection
            }]
        };

        this.client.queryCollections(this.database._self, querySpec).toArray((error: QueryError, result: CollectionMeta[], responseHeaders?: any) => {
            if(error) {
                callback(error, null);
            }
            else if(result.length == 0) {
                this.client.createCollection(this.database._self, {id: this.options.collection}, {}, (error: QueryError, collection: CollectionMeta): void => {
                    if(error) {
                        callback(error, null);
                    }
                    else {
                        callback(null, collection);
                    }
                });
            }
            else {
                callback(null, result[0]);
            }
        });
    }
}

// DocumentDb public typings are not correct, so we use this interface to have the correct typings
export interface IDocumentClient {
     createDatabase(body: UniqueId, options: RequestOptions, callback: RequestCallback<DatabaseMeta>): void;
     upsertDocument<TDocument>(collectionSelfLink: string, document: NewDocument<TDocument>, options: RequestOptions, callback: RequestCallback<RetrievedDocument<TDocument>>): void;
     createCollection(databaseLink: string, body: Collection, options: RequestOptions, callback: RequestCallback<CollectionMeta>): void;
     queryDocuments<TDocument>(collectionLink: string, query: string | SqlQuerySpec, options?: FeedOptions): QueryIterator<RetrievedDocument<TDocument>>;
     queryDatabases(query: string | SqlQuerySpec): QueryIterator<DatabaseMeta>;
     queryCollections(databaseLink: string, query: string | SqlQuerySpec): QueryIterator<CollectionMeta>;
}