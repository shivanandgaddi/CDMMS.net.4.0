define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', 'jquerydatatable', 'jqueryui', 'datablescelledit'],
    function (composition, app, ko, $, http, activator, mapping, system, jquerydatatable, jqueryui, datablescelledit) {
        var DDviewFlowthru = function (data) {
            var self = this;
            var results = JSON.parse(data);
            self.DDF = mapping.fromJS(results);
            self.items = mapping.fromJS(results);
            self.usr = require('Utility/user');
            self.addElement = ko.observable(null);
            self.addedElement = ko.observable();
            self.flowthru_id = ko.observable();
            self.orgfld_nm = ko.observable();
            self.field_desc = ko.observable();
            self.flowthru_ind = ko.observable();
            self.org_sys = ko.observable();
            self.userid = ko.observable();
            self.timestmp = ko.observable();
            self.exists = true;
            self.addDDviewFlowthru = ko.observable();
            self.updateDDviewFlowthru = ko.observable();
            self.selectedElement = ko.observable(null);
        };

        var updateModel = function (object) {
            this.updateflowthruid = object.Attributes.flowthruid.value;
            this.updateoriginatingfieldname = object.Attributes.originatingfieldname.value;
            this.updatefielddescription = object.Attributes.fielddescription.value;
            this.updateflowthruind = object.Attributes.flowthruind.value;
            this.updateoriginatingsystem = object.Attributes.originatingsystem.value;
            this.updateUserid = object.Attributes.lastupdateduserid.value;
            this.updateTimestamp = object.Attributes.lastupdated.value;
        }

        var addModel = function (obj) {
            var attr = {
                Attributes: {
                    flowthruid: {
                        value: ko.observable(obj.updateflowthruid)
                    },
                    originatingfieldname: {
                        value: ko.observable(obj.updateoriginatingfieldname)
                    },
                    fielddescription: {
                        value: ko.observable(obj.updatefielddescription)
                    },
                    flowthruind: {
                        value: ko.observable(obj.updateflowthruind)
                    },
                    originatingsystem: {
                        value: ko.observable(obj.updateoriginatingsystem)
                    },
                    lastupdateduserid: {
                        value: ko.observable(obj.updateUserid)
                    },
                    lastupdated: {
                        value: ko.observable(obj.updateTimestamp)
                    }
                }
            };

            return attr;
        }

        DDviewFlowthru.prototype.Saveadd = function (rootcontext, selectedRow) {
            var saveJSON = { flowthruind: document.getElementById('flowthru_ind').value, originatingfieldname: document.getElementById('orgfld_nm').value, originatingfieldname: document.getElementById('field_desc').value, originatingfieldname: document.getElementById('flowthru_ind').value, originatingfieldname: document.getElementById('org_sys').value, user: rootcontext.usr.cuid };
            $.ajax({
                type: "GET",
                url: 'api/drop/insert',
                data: saveJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: self
            });
            function successFunc(data, status) {
                var results = JSON.parse(data);
            }

            function errorFunc() {
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            }
        }

        DDviewFlowthru.prototype.Save = function (rootcontext, selectedRow) {
            var saveJSON = {
                flowthruid: document.getElementById('flowthru_id').value,
                originatingfieldname: document.getElementById('orgfld_nm').value,
                fielddescription: document.getElementById('field_desc').value,
                flowthruind: document.getElementById('flowthru_ind').value,
                originatingsystem: document.getElementById('org_sys').value,
                user: rootcontext.usr.cuid
            };

            $.ajax({
                type: "GET",
                url: 'api/drop/updationflowthru',
                data: saveJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc1,
                error: errorFunc,
                context: self
            });

            function successFunc1(data, status) {
                var results = JSON.parse(data);
                rootcontext.DDF(results);
                $(".updateSuccessfully").show();
                $(".insertSuccessfully").show();
                setTimeout(function () {
                    $(".insertSuccessfully").hide()
                }, 5000);

            }

            function errorFunc() {
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            }
        }

        DDviewFlowthru.prototype.Update = function (rootContext, selectedrow) {
            rootContext.addElement(false);
            rootContext.selectedElement(new updateModel(selectedrow));
            document.getElementById('updateDDviewFlowthru').scrollIntoView();
        }

        DDviewFlowthru.prototype.Delete = function (rootContext, selectedrow) {
            var url = 'api/drop/' + selectedrow.Attributes.originatingfieldname.value();
            http.get(url).then(function (response) {
                var newresults = JSON.parse(response);
                rootContext.DDF(newresults);
                app.showMessage('Deleted Successfully');
            });
        }

        return DDviewFlowthru;
    });