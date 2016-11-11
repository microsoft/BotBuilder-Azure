"use strict";
var builderazure = require('../../');
var FaultyAzureTableClient = (function () {
    function FaultyAzureTableClient(client, faultSettings) {
        this.tableClient = client;
        this.faultSettings = faultSettings;
    }
    FaultyAzureTableClient.prototype.initialize = function (callback) {
        if (this.faultSettings.shouldFailInitialize) {
            callback(builderazure.AzureTableClient.getError(this.faultSettings.error, this.faultSettings.response));
        }
        else {
            this.tableClient.initialize(callback);
        }
    };
    FaultyAzureTableClient.prototype.insertOrReplace = function (partitionKey, rowKey, data, isCompressed, callback) {
        if (this.faultSettings.shouldFailInsert) {
            callback(builderazure.AzureTableClient.getError(this.faultSettings.error, this.faultSettings.response), null, this.faultSettings.response);
        }
        else {
            this.tableClient.insertOrReplace(partitionKey, rowKey, data, isCompressed, callback);
        }
    };
    FaultyAzureTableClient.prototype.retrieve = function (partitionKey, rowKey, callback) {
        if (this.faultSettings.shouldFailRetrieve) {
            callback(builderazure.AzureTableClient.getError(this.faultSettings.error, this.faultSettings.response), null, this.faultSettings.response);
        }
        else {
            this.tableClient.retrieve(partitionKey, rowKey, callback);
        }
    };
    return FaultyAzureTableClient;
}());
exports.FaultyAzureTableClient = FaultyAzureTableClient;
