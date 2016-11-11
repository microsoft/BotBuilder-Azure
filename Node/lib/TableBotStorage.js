"use strict";
var async = require('async');
var Promise = require('promise');
var zlib = require('zlib');
var AzureTableClient_1 = require('./AzureTableClient');
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
var TableBotStorage = (function () {
    function TableBotStorage(options, tableClient) {
        this.options = options;
        this.botStorageTableName = TABLE_NAME;
        this.settings = options;
        if (!tableClient) {
            this.azureTableClient = new AzureTableClient_1.AzureTableClient(this.botStorageTableName, options.accountName, options.accountKey);
        }
        else {
            this.azureTableClient = tableClient;
        }
    }
    TableBotStorage.prototype.getData = function (context, callback) {
        var _this = this;
        try {
            var promise = this.initializeTableClient();
            promise.done(function () {
                var list = [];
                if (context.userId) {
                    if (context.persistUserData) {
                        list.push({
                            partitionKey: context.userId,
                            rowKey: USER_DATA_FIELD,
                            field: USER_DATA_FIELD
                        });
                    }
                    if (context.conversationId) {
                        list.push({
                            partitionKey: context.conversationId,
                            rowKey: context.userId,
                            field: PRIVATE_CONVERSATION_DATA_FIELD
                        });
                    }
                }
                if (context.persistConversationData && context.conversationId) {
                    list.push({
                        partitionKey: context.conversationId,
                        rowKey: CONVERSATION_DATA_FIELD,
                        field: CONVERSATION_DATA_FIELD
                    });
                }
                var data = {};
                async.each(list, function (entry, cb) {
                    _this.azureTableClient.retrieve(entry.partitionKey, entry.rowKey, function (error, entity, response) {
                        if (!error) {
                            var botData = entity.Data['_'] ? entity.Data['_'] : {};
                            var isCompressed = entity.IsCompressed['_'];
                            if (isCompressed) {
                                zlib.gunzip(new Buffer(botData, BASE_64), function (err, result) {
                                    if (!err) {
                                        try {
                                            var txt = result.toString();
                                            data[entry.field + HASH] = txt;
                                            data[entry.field] = JSON.parse(txt);
                                        }
                                        catch (e) {
                                            err = e;
                                        }
                                    }
                                    cb(err);
                                });
                            }
                            else {
                                try {
                                    data[entry.field + HASH] = JSON.stringify(botData);
                                    data[entry.field] = botData;
                                }
                                catch (e) {
                                    error = e;
                                }
                                cb(error);
                            }
                        }
                        else {
                            cb(error);
                        }
                    });
                }, function (err) {
                    if (!err) {
                        callback(null, data);
                    }
                    else {
                        var m = err.toString();
                        callback(err instanceof Error ? err : new Error(m), null);
                    }
                });
            }, function (err) { return callback(err, null); });
        }
        catch (e) {
            callback(e instanceof Error ? e : new Error(e.toString()), null);
        }
    };
    TableBotStorage.prototype.saveData = function (context, data, callback) {
        var _this = this;
        var promise = this.initializeTableClient();
        promise.done(function () {
            var list = [];
            function addWrite(field, partitionKey, rowKey, botData) {
                var hashKey = field + HASH;
                var hash = JSON.stringify(botData);
                if (!data[hashKey] || hash !== data[hashKey]) {
                    data[hashKey] = hash;
                    list.push({ field: field, partitionKey: partitionKey, rowKey: rowKey, botData: botData, hash: hash });
                }
            }
            try {
                if (context.userId) {
                    if (context.persistUserData) {
                        addWrite(USER_DATA_FIELD, context.userId, USER_DATA_FIELD, data.userData || {});
                    }
                    if (context.conversationId) {
                        addWrite(PRIVATE_CONVERSATION_DATA_FIELD, context.conversationId, context.userId, data.privateConversationData || {});
                    }
                }
                if (context.persistConversationData && context.conversationId) {
                    addWrite(PRIVATE_CONVERSATION_DATA_FIELD, context.conversationId, PRIVATE_CONVERSATION_DATA_FIELD, data.conversationData || {});
                }
                async.each(list, function (entry, errorCallback) {
                    if (_this.settings.gzipData) {
                        zlib.gzip(entry.hash, function (err, result) {
                            if (!err && result.length > MAX_DATA_LENGTH) {
                                err = new Error("Data of " + result.length + " bytes gzipped exceeds the " + MAX_DATA_LENGTH + " byte limit. Can't post to: " + entry.url);
                                err.code = ERROR_MESSAGE_SIZE;
                            }
                            if (!err) {
                                _this.azureTableClient.insertOrReplace(entry.partitionKey, entry.rowKey, result.toString('base64'), true, function (error, eTag, response) {
                                    errorCallback(error);
                                });
                            }
                            else {
                                errorCallback(err);
                            }
                        });
                    }
                    else if (entry.hash.length < MAX_DATA_LENGTH) {
                        _this.azureTableClient.insertOrReplace(entry.partitionKey, entry.rowKey, entry.botData, false, function (error, eTag, response) {
                            errorCallback(error);
                        });
                    }
                    else {
                        var err = new Error("Data of " + entry.hash.length + " bytes exceeds the " + MAX_DATA_LENGTH + " byte limit. Consider setting connectors gzipData option. Can't post to: " + entry.url);
                        err.code = ERROR_MESSAGE_SIZE;
                        errorCallback(err);
                    }
                }, function (err) {
                    if (callback) {
                        if (!err) {
                            callback(null);
                        }
                        else {
                            var m = err.toString();
                            callback(err instanceof Error ? err : new Error(m));
                        }
                    }
                });
            }
            catch (e) {
                if (callback) {
                    var err = e instanceof Error ? e : new Error(e.toString());
                    err.code = ERROR_BAD_MESSAGE;
                    callback(err);
                }
            }
        }, function (err) { return callback(err); });
    };
    TableBotStorage.prototype.initializeTableClient = function () {
        var _this = this;
        if (!this.initializeTableClientPromise) {
            this.initializeTableClientPromise = new Promise(function (resolve, reject) {
                _this.azureTableClient.initialize(function (error) {
                    if (error) {
                        reject(new Error('Failed to initialize azure table client. Error: ' + error.toString()));
                    }
                    else {
                        resolve(true);
                    }
                });
            });
        }
        return this.initializeTableClientPromise;
    };
    return TableBotStorage;
}());
exports.TableBotStorage = TableBotStorage;
