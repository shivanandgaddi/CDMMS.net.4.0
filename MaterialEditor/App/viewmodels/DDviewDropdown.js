define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', 'jquerydatatable', 'jqueryui', 'datablescelledit'],
    function (composition, app, ko, $, http, activator, mapping, system, jquerydatatable, jqueryui, datablescelledit) {

        var DDviewDropdown = function (data) {
            var self = this;
            var results = JSON.parse(data);
            self.DDT = mapping.fromJS(results);
            self.items = mapping.fromJS(results);
            self.DDF = ko.observable(false);
            self.addElement = ko.observable(null);
            self.addedElement = ko.observable();
            shouldShowMessage: ko.observable(true);
            self.mtl_id = ko.observable();
            self.app_nm = ko.observable();
            self.eff_dt = ko.observable();
            self.end_dt = ko.observable();
            self.userID = ko.observable();
            self.timestmp = ko.observable();
            self.usr = require('Utility/user');
            self.exists = true;
            self.addDropdown = ko.observable();
            self.updateDropdown = ko.observable();
            self.selectedElement = ko.observable(null);
        };

        var updateModel = function (object) {
            this.updatematerialID = object.Attributes.materialid.value;
            this.updateAppnm = object.Attributes.appname.value;
            this.updateEffdate = object.Attributes.effectivedate.value;
            this.updatEenddt = object.Attributes.enddate.value;
            this.updateUserid = object.Attributes.lastupdateduserid.value;
            this.updateTimestamp = object.Attributes.lastupdated.value;

        }

        var addModel = function (obj) {
            var attr = {
                Attributes: {
                    materialid: {
                        value: ko.observable(obj.updatematerialID)
                    },
                    appname: {
                        value: ko.observable(obj.updateAppnm)
                    },
                    effectivedate: {
                        value: ko.observable(obj.updateEffdate)
                    },
                    enddate: {
                        value: ko.observable(obj.updatEenddt)
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

        DDviewDropdown.prototype.Saveadd = function (rootcontext, selectedRow) {

            var saveJSON = { appname: document.getElementById('app_nm1').value, effectivedate: document.getElementById('eff_dt1').value, enddate: document.getElementById('end_dt1').value, user: rootcontext.usr.cuid };

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
                //rootcontext.CAM.push(results);
                $(".insertSuccessfully").show();
                setTimeout(function () { $(".insertSuccessfully").hide() }, 5000);
            }

            function errorFunc() {
                alert('error');
            }

        }

        DDviewDropdown.prototype.Save = function (rootcontext, selectedRow) {

            var saveJSON = { materialid: document.getElementById('mtl_id').value, appname: document.getElementById('app_nm').value, startdate: document.getElementById('eff_dt').value, enddate: document.getElementById('end_dt').value, user: rootcontext.usr.cuid };

            $.ajax({
                type: "GET",
                url: 'api/drop/updation',
                data: saveJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: self
            });

            function successFunc(data, status) {

                var results = JSON.parse(data);
                rootcontext.DDT(results);
                $(".updateSuccessfully").show();
                setTimeout(function () { $(".updateSuccessfully").hide() }, 5000);

            }

            function errorFunc() {
                alert('error');
            }

        }

        DDviewDropdown.prototype.Add = function (rootContext, selectedRow) {
            // rootContext.selectedElement(null);
            rootContext.addElement(new addModel(selectedRow));
        };

        DDviewDropdown.prototype.ShowAdd = function (rootContext, selectedRow) {
            rootContext.selectedElement(null);
            rootContext.addedElement(new addModel({}));
            rootContext.addElement(true);
            setTimeout(function () {
                $("#eff_dt1").datepicker();
                $("#end_dt1").datepicker();
            }, 300);
            document.getElementById('addDropdown').scrollIntoView();

        };

        DDviewDropdown.prototype.Update = function (rootContext, selectedrow) {
            rootContext.addElement(false);
            rootContext.selectedElement(new updateModel(selectedrow));
            setTimeout(function () {
                $("#eff_dt").datepicker();
                $("#end_dt").datepicker();
            }, 300);
            document.getElementById('updateDropdown').scrollIntoView();

        }

        DDviewDropdown.prototype.Delete = function (rootContext, selectedrow) {
            var url = 'api/drop/' + selectedrow.Attributes.appname.value();
            http.get(url).then(function (response) {
                var newresults = JSON.parse(response);
                rootContext.DDT(newresults);


                $(".deleteSuccessfully").show();
                setTimeout(function () { $(".deleteSuccessfully").hide() }, 5000);

            });


        }

        return DDviewDropdown;

    });