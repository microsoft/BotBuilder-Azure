"use strict";
var azure = require('azure-storage');
var DEVELOPMENT_CONNECTION_STRING = 'UseDevelopmentStorage=true';
var AzureTableClient = (function () {
    function AzureTableClient(tableName, accountName, accountKey) {
        if (!accountName && !accountKey) {
            this.useDevelopmentStorage = true;
        }
        else if (!accountName || !accountKey) {
            throw Error('Storage account name and account key are mandatory when not using development storage');
        }
        this.accountName = accountName;
        this.accountKey = accountKey;
        this.tableName = tableName;
    }
    AzureTableClient.prototype.initialize = function (callback) {
        var tableService = this.buildTableService();
        tableService.createTableIfNotExists(this.tableName, function (error, result, response) {
            callback(AzureTableClient.getError(error, response));
        });
    };
    AzureTableClient.prototype.insertOrReplace = function (partitionKey, rowKey, data, isCompressed, callback) {
        var tableService = this.buildTableService();
        var entityGenerator = azure.TableUtilities.entityGenerator;
        var entity = {
            PartitionKey: entityGenerator.String(partitionKey),
            RowKey: entityGenerator.String(rowKey),
            Data: entityGenerator.String(data),
            IsCompressed: entityGenerator.Boolean(isCompressed)
        };
        tableService.insertOrReplaceEntity(this.tableName, entity, { checkEtag: false }, function (error, result, response) {
            callback(AzureTableClient.getError(error, response), result, response);
        });
    };
    AzureTableClient.prototype.retrieve = function (partitionKey, rowKey, callback) {
        var tableService = this.buildTableService();
        tableService.retrieveEntity(this.tableName, partitionKey, rowKey, function (error, result, response) {
            callback(AzureTableClient.getError(error, response), result, response);
        });
    };
    AzureTableClient.prototype.buildTableService = function () {
        var tableService = this.useDevelopmentStorage
            ? azure.createTableService(DEVELOPMENT_CONNECTION_STRING)
            : azure.createTableService(this.accountName, this.accountKey);
        return tableService.withFilter(new azure.ExponentialRetryPolicyFilter());
    };
    AzureTableClient.getError = function (error, response) {
        if (!error)
            return null;
        var message = 'Failed to perform the requested operation on Azure Table. Message: ' + error.message + '. Error code: ' + error.code;
        if (response) {
            message += '. Http status code: ';
            message += response.statusCode;
        }
        return new Error(message);
    };
    return AzureTableClient;
}());
exports.AzureTableClient = AzureTableClient;
