"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var tedious_1 = require("tedious");
var AzureSqlClient = (function () {
    function AzureSqlClient(options) {
        this.options = options;
    }
    AzureSqlClient.prototype.initialize = function (callback) {
        var _this = this;
        var client = new tedious_1.Connection(this.options);
        client.on('connect', function (error) {
            if (error) {
                callback(AzureSqlClient.getError(error));
            }
            else {
                var checkTableRequest = new tedious_1.Request("IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.Tables WHERE TABLE_NAME = N'" + _this.options.options.table + "') BEGIN SELECT TOP 1 * FROM " + _this.options.options.table + " END", function (error, rowCount, rows) {
                    if (error) {
                        client.close();
                        callback(AzureSqlClient.getError(error));
                    }
                    else if (!rowCount) {
                        var createTableRequest = new tedious_1.Request("CREATE TABLE " + _this.options.options.table + " (id NVARCHAR(200), data NVARCHAR(1000), isCompressed BIT)", function (error, rowCount, rows) {
                            client.close();
                            callback(AzureSqlClient.getError(error));
                        });
                        client.execSql(createTableRequest);
                    }
                    else {
                        client.close();
                        callback(null);
                    }
                });
                client.execSql(checkTableRequest);
            }
        });
    };
    AzureSqlClient.prototype.insertOrReplace = function (partitionKey, rowKey, entity, isCompressed, callback) {
        var _this = this;
        var client = new tedious_1.Connection(this.options);
        client.on('connect', function (error) {
            if (error) {
                callback(AzureSqlClient.getError(error), null, null);
            }
            else {
                var getRequest = new tedious_1.Request("SELECT TOP 1 * FROM " + _this.options.options.table + " WHERE id=@id", function (err, rowCount, rows) {
                    if (err) {
                        client.close();
                        callback(AzureSqlClient.getError(err), null, null);
                    }
                    else {
                        if (rowCount) {
                            var updateRequest = new tedious_1.Request("UPDATE " + _this.options.options.table + " SET data=@data, isCompressed=@isCompressed WHERE id=@id", function (error, rowCount, rows) {
                                if (error) {
                                    client.close();
                                    callback(AzureSqlClient.getError(error), null, null);
                                }
                                else {
                                    client.close();
                                    callback(null, rows[0], rows[0]);
                                }
                            });
                            AzureSqlClient.addParameters(updateRequest, completeId_1, stringifiedEntity_1, isCompressed);
                            client.execSql(updateRequest);
                        }
                        else {
                            var insertRequest = new tedious_1.Request("INSERT INTO " + _this.options.options.table + " (id, data, isCompressed) VALUES (@id, @data, @isCompressed)", function (error, rowCount, rows) {
                                if (error) {
                                    client.close();
                                    callback(AzureSqlClient.getError(error), null, null);
                                }
                                else {
                                    client.close();
                                    callback(null, rows[0], rows[0]);
                                }
                            });
                            AzureSqlClient.addParameters(insertRequest, completeId_1, stringifiedEntity_1, isCompressed);
                            client.execSql(insertRequest);
                        }
                    }
                });
                var completeId_1 = partitionKey + ',' + rowKey;
                var stringifiedEntity_1 = JSON.stringify(entity);
                AzureSqlClient.addParameters(getRequest, completeId_1, stringifiedEntity_1, isCompressed);
                client.execSql(getRequest);
            }
        });
    };
    AzureSqlClient.prototype.retrieve = function (partitionKey, rowKey, callback) {
        var _this = this;
        var client = new tedious_1.Connection(this.options);
        client.on('connect', function (error) {
            if (error) {
                callback(AzureSqlClient.getError(error), null, null);
            }
            else {
                var request = new tedious_1.Request("SELECT TOP 1 * FROM " + _this.options.options.table + " WHERE id=@id", function (err, rowCount, rows) {
                    if (err) {
                        client.close();
                        callback(AzureSqlClient.getError(err), null, null);
                    }
                    else if (!rowCount) {
                        client.close();
                        callback(null, null, null);
                    }
                    else {
                        client.close();
                        var row = rows[0];
                        callback(null, row, rows[0]);
                    }
                });
                var id = partitionKey + ',' + rowKey;
                AzureSqlClient.addParameters(request, id);
                client.execSql(request);
            }
        });
    };
    AzureSqlClient.getError = function (error) {
        if (!error)
            return null;
        return new Error('Error Code: ' + error.code + ' Error Message: ' + error.message);
    };
    AzureSqlClient.addParameters = function (request, id, data, isCompressed) {
        request.addParameter('id', tedious_1.TYPES.NVarChar, id);
        request.addParameter('data', tedious_1.TYPES.NVarChar, data);
        request.addParameter('isCompressed', tedious_1.TYPES.Bit, isCompressed);
    };
    return AzureSqlClient;
}());
exports.AzureSqlClient = AzureSqlClient;
