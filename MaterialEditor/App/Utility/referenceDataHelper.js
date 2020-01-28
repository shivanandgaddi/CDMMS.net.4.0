define(['plugins/http', 'durandal/system', 'jquery', 'durandal/app'], function (http, system, $, app) {
    var ReferenceDataHelper = function () {
        var self = this;

        self.baseUrl = 'api/reference/';
        self.materialItemId = 0;
        self.workToDoId = 0;
    };

    ReferenceDataHelper.prototype.getData = function (name, parentName, parentValue) {
        var self = this;
        var url = self.baseUrl + name;
        var dfd = $.Deferred();
        var param = null;

        if (parentName && parentValue) {
            param = { parentName: parentValue };
        }

        http.get(url, param).then(function (response) {
            dfd.resolve(JSON.parse(response));
        });

        return dfd.promise();
    };

    ReferenceDataHelper.prototype.getSendToNdsStatus = function (mtlId, workId) {
        var self = this;
        var statusUrl = "api/material/ndsStatus/" + workId;

        setTimeout(function () {
            $.ajax({
                type: "GET",
                url: statusUrl,
                success: function (data) {
                    var response = JSON.parse(data);

                    if (response.status === 'PROCESSING') {
                        //Setup the next poll recursively
                        self.getSendToNdsStatus(mtlId, workId);
                    }
                    else {
                        var message = "Changes made to CDMMS ID " + mtlId + " have been processed by NDS. Status: " + response.status + ".";

                        if (response.notes) {
                            message = message + " Additional Information: " + response.notes;
                        }

                        message = message + " Request ID: " + workId + ".";

                        //alert(message);
                        return app.showMessage(message);
                    }
                },
                dataType: "json"
            });
        }, 15000);
    };

    ReferenceDataHelper.prototype.getHighLevelPartSendToNdsStatus = function (mtlId, workId) {
        var self = this;
        var statusUrl = "api/highlevelpart/ndsStatus/" + workId;

        setTimeout(function () {
            $.ajax({
                type: "GET",
                url: statusUrl,
                success: function (data) {
                    var response = JSON.parse(data);

                    if (response.status === 'PROCESSING') {
                        //Setup the next poll recursively
                        self.getHighLevelPartSendToNdsStatus(mtlId, workId);
                    }
                    else {
                        var message = "Changes made to CDMMS ID " + mtlId + " have been processed by NDS. Status: " + response.status + ".";

                        if ("ERROR" === response.status) {
                            if (response.ndsId > 0) {
                                app.showMessage(response.notes, "Material Editor", ["Yes", "No"]).then(function (dialogResponse) {
                                    if (dialogResponse === "Yes") {
                                        self.saveExistingMacroAssemblyId(mtlId, response.ndsId, workId);
                                    }

                                    return;
                                });
                            } else {
                                if (response.notes) {
                                    message = message + " Additional Information: " + response.notes;
                                }

                                message = message + " Request ID: " + workId + ".";

                                return app.showMessage(message);
                            }
                        } else {
                            if (response.notes) {
                                message = message + " Additional Information: " + response.notes;
                            }

                            message = message + " Request ID: " + workId + ".";

                            return app.showMessage(message);
                        }
                    }
                },
                dataType: "json"
            });
        }, 15000);
    };

    ReferenceDataHelper.prototype.saveExistingMacroAssemblyId = function (mtlItmId, maId, wtdId) {
        var self = this;
        var saveUrl = "api/highlevelpart/updatendsmaid/" + mtlItmId + "/" + maId + "/" + wtdId;

        $.ajax({
            type: "POST",
            url: saveUrl,
            success: function (data) {
                console.log(data);

                if ('Success' === data) {
                    self.getHighLevelPartSendToNdsStatus(mtlItmId, wtdId);
                }
            },
            error: function (data) {
                console.log("Error in ReferenceDataHelper.prototype.saveExistingMacroAssemblyId - " + data);
            },
            dataType: "json"
        });
    };

    ReferenceDataHelper.prototype.getSpecificationSendToNdsStatus = function (specId, workId, specTyp) {
        var self = this;
        var statusUrl = "api/specn/ndsStatus/" + workId;

        setTimeout(function () {
            $.ajax({
                type: "GET",
                url: statusUrl,
                success: function (data) {
                    var response = JSON.parse(data);

                    if (response.status === 'PROCESSING') {
                        //Setup the next poll recursively
                        self.getSpecificationSendToNdsStatus(specId, workId, specTyp);
                    }
                    else {
                        var message = "Changes made to Specification ID " + specId + " have been processed by NDS. Status: " + response.status + ".";

                        if ("ERROR" === response.status) {
                            if (response.ndsId > 0) {
                                app.showMessage(response.notes, "Specification", ["Yes", "No"]).then(function (dialogResponse) {
                                    if (dialogResponse === "Yes") {
                                        self.saveExistingNDSSpecId(specId, response.ndsId, specTyp);
                                    }

                                    return;
                                });
                            } else {
                                if (response.notes) {
                                    message = message + " Additional Information: " + response.notes;
                                }

                                message = message + " Request ID: " + workId + ".";

                                self.saveNDSSpecId(specId, workId, specTyp);

                                return app.showMessage(message, "Specification");
                            }
                        } else {
                            if (response.notes) {
                                message = message + " Additional Information: " + response.notes;
                            }

                            message = message + " Request ID: " + workId + ".";

                            self.saveNDSSpecId(specId, workId, specTyp);

                            return app.showMessage(message, "Specification");
                        }
                    }
                },
                error: function (data) {
                    console.log("Error in ReferenceDataHelper.prototype.getSpecificationSendToNdsStatus - " + data);
                },
                dataType: "json"
            });
        }, 15000);
    };

    ReferenceDataHelper.prototype.saveNDSSpecId = function (specId, workId, specTyp) {
        var saveUrl = "api/specn/getndsspecid/" + specId + "/" + specTyp + "/" + workId;

        $.ajax({
            type: "GET",
            url: saveUrl,
            success: function (data) {
                console.log(data);
            },
            error: function (data) {
                console.log("Error in ReferenceDataHelper.prototype.saveNDSSpecId - " + data);
            },
            dataType: "json"
        });
    };

    ReferenceDataHelper.prototype.saveExistingNDSSpecId = function (specId, ndsSpecId, specTyp) {
        var self = this;
        var saveUrl = "api/specn/updatendsspecid/" + specId + "/" + specTyp + "/" + ndsSpecId;

        $.ajax({
            type: "POST",
            url: saveUrl,
            success: function (data) {
                console.log(data);
                var response = JSON.parse(data);

                if (response.wtd_id > 0) {
                    self.getSpecificationSendToNdsStatus(specId, response.wtd_id, specTyp);
                }
            },
            error: function (data) {
                console.log("Error in ReferenceDataHelper.prototype.saveNDSSpecId - " + data);
            },
            dataType: "json"
        });
    };

    ReferenceDataHelper.prototype.getFeatureTypeLocatableType = function (id) {
        var url = 'api/reference/checkFtrTypIsLocatable/' + id;
        var dfd = $.Deferred();

        http.get(url).then(function (response) {
            dfd.resolve(response);
        },
        function (error) {
            return '';
        });

        return dfd.promise();
    };

    return ReferenceDataHelper;
});