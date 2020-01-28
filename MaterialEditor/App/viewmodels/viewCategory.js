define(['durandal/composition','durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', 'jquerydatatable', 'jqueryui', 'datablescelledit'],
    function (composition,app, ko, $, http, activator, mapping, system, jquerydatatable, jqueryui, datablescelledit) {

        var viewCategory = function (data) {
            var self = this;
            var selectedAppName;
        self.CAM = ko.observableArray();       
        var results = JSON.parse(data);
        self.CAM = mapping.fromJS(results);
        self.items = mapping.fromJS(results);
        self.usr = require('Utility/user');
        self.addElement = ko.observable(null);
        self.addedElement = ko.observable();

        self.mtlgrpid = ko.observable();
        self.app_nm = ko.observable();       
        self.enddate = ko.observable();
        self.startdate = ko.observable();

        self.exists = true;
        self.addCategory = ko.observable();
        self.updateCategory = ko.observable();
        self.selectedElement = ko.observable(null);
        self.filterOption = ko.observable("All");
        self.filterOption.subscribe(function (newVal) {
            self.Filter(self);
        });


        };

        var updateModel = function (object) {
            this.updateMtlGrpId = object.Attributes.mtlgrpid.value;
            this.updateAppnm = object.Attributes.Appnm.value;         
            this.updateeffenddt = object.Attributes.effenddt.value;
            this.updateeffsrtdt = object.Attributes.effsrtdt.value;;
        }

        var addModel = function (obj) {
            var attr = {
                Attributes: {
                    mtlgrpid: {
                        value: ko.observable(obj.addMtlGrpId)
                    },
                    Appnm: {
                        value: ko.observable(obj.updateAppnm)
                    },
                    effsrtdt: {
                        value: ko.observable(obj.updateeffsrtdt)
                    },
                    effenddt: {
                        value: ko.observable(obj.updateeffenddt)
                    }
                }
            };
            return attr;
        }

        viewCategory.prototype.Saveadd = function (rootcontext, selectedRow)
        {
            var intHeight = $("body").height();
            $("#interstitial").css("height", intHeight);
            $("#interstitial").show();

            var saveJSON = { mtlgrp: document.getElementById('mtlgrpid1').value, appname: document.getElementById('app_nm1').value, startdate: document.getElementById('startdate1').value, enddate: document.getElementById('enddate1').value, user: rootcontext.usr.cuid };

            $.ajax({
                type: "GET",
                url: 'api/category/insert',
                data: saveJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: self
            });

            function successFunc(data, status) {
                $("#interstitial").hide();
                var results = JSON.parse(data);
                rootcontext.CAM(results);
                $(".insertSuccessfully").show();
                setTimeout(function () { $(".insertSuccessfully").hide() }, 5000);


            }

            function errorFunc() {
                alert('error');
            }

        }

        viewCategory.prototype.Save = function (rootcontext, selectedRow)
        {
            var intHeight = $("body").height();
            $("#interstitial").css("height", intHeight);
            $("#interstitial").show();

            var saveJSON = { mtlgrp: document.getElementById('mtlgrpid').value, appnameold: rootcontext.selectedAppName, appname: document.getElementById('app_nm').value, startdate: document.getElementById('startdate').value, enddate: document.getElementById('enddate').value, user: rootcontext.usr.cuid };
           
            $.ajax({
                type: "GET",
                url: 'api/category/updation',
                data: saveJSON ,              
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: self
            });

            function successFunc(data, status) {
                $("#interstitial").hide();
                var results = JSON.parse(data);
                rootcontext.CAM(results);                
                $(".updatedSuccessfully").show();
                setTimeout(function () { $(".updatedSuccessfully").hide() }, 5000);

            }

            function errorFunc() {               
                alert('error');            }

        }

        viewCategory.prototype.Add = function (rootContext, selectedRow) {
           // rootContext.selectedElement(null);
            rootContext.addElement(new addModel(selectedRow));
        };

        viewCategory.prototype.ShowAdd = function (rootContext, selectedRow) {
            rootContext.selectedElement(null);
            rootContext.addedElement(new addModel({}));
            rootContext.addElement(true);
            setTimeout(function () {
                $("#startdate1").datepicker();
                $("#enddate1").datepicker();
            }, 3000);
            document.getElementById('addCategory').scrollIntoView();
            
        };

      
        viewCategory.prototype.Update = function (rootContext, selectedrow)
        {
            rootContext.addElement(false);
            rootContext.selectedElement(new updateModel(selectedrow));
            setTimeout(function () {
                $("#startdate").datepicker();
                $("#enddate").datepicker();
            }, 3000);
            document.getElementById('updateCategory').scrollIntoView();
            rootContext.selectedAppName = $("#app_nm_hd").val();

        }
       

        viewCategory.prototype.Delete = function (rootContext, selectedrow) {
            var intHeight = $("body").height();
            $("#interstitial").css("height", intHeight);
            $("#interstitial").show();

            var url = 'api/category/' + selectedrow.Attributes.mtlgrpid.value();

          http.get(url).then(function (response) {
            $("#interstitial").hide();
            var newresults = JSON.parse(response);
            rootContext.CAM(newresults);
            $(".deleteSuccessfully").show();
            setTimeout(function () { $(".deleteSuccessfully").hide() }, 5000);
           

         });


        }

        viewCategory.prototype.Filter = function (root) {
            var filter = this.filterOption().toLowerCase();
            var filteredList
            if (filter == "all") {
                filteredList = root.items();
            } else {
                filteredList = ko.utils.arrayFilter(this.items(), function (item) {
                    return stringStartsWith(item.Attributes.approvedstatus.value().toLowerCase(), filter);
                });
            }
            this.CAM(filteredList);
        };
   
        var stringStartsWith = function (string, startsWith) {
            string = string || "";
            if (startsWith == string)
                return true;
            return false;
        };
        //function pagination() {
        //    var table;
        //    table = $('#categorymanagement').DataTable();

        //}
    return viewCategory;

});