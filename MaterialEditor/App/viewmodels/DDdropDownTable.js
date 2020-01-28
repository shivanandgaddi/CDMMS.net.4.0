define(['durandal/composition','durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', './DDviewDropdown', './DDviewFlowthru', 'jquerydatatable', 'jqueryui', 'datablescelledit'],
    function (composition,app, ko, $, http, activator, mapping, system, viewdrop, viewflow, jquerydatatable, jqueryui, datablescelledit) {
        var DDdropDownTable = function () {
            self = this;
            self.usr = require('Utility/user');
            self.tableOptions = ko.observableArray(['MATERIAL_TYPE_CD', 'MATERIAL_FLOW_THRU']);
            self.DDviewDropdown = ko.observable();
            self.DDviewFlowthru = ko.observable();
            self.showDDT = ko.observable(false);
            self.showDDF = ko.observable(false);
            self.filterOption = ko.observable("All");
            self.filterOption1 = ko.observable("All");
            self.filterOption.subscribe(function (newVal) {
                self.Filter(self);
            });
            self.filterOption1.subscribe(function (newVal) {
                self.Filter1(self);
            });
        };

        DDdropDownTable.prototype.Filter = function (root, selectItem) {
            console.log(self.filterOption());
            self.selectdropdown(root, self.filterOption())
        };

        DDdropDownTable.prototype.Filter1 = function (root, selectItem) {
            console.log(self.filterOption1());
            self.selectdropdown(root, self.filterOption1())
        };
        
        DDdropDownTable.prototype.selectdropdown = function (rootContext, selectItem) {
            var intHeight = $("body").height();
            $("#interstitial").css("height", intHeight);
            $("#interstitial").show();
            var urldd;
            var searchJSON = { selecttable: $("#selectedtable").val() };
            console.log(searchJSON);
            var selectedOpt = $("#selectedtable option:selected").text();
            if (selectedOpt == '--Select a table--') { app.showMessage("Please select the valid table and search."); $("#interstitial").hide(); }
            else if (selectedOpt == 'MATERIAL_TYPE_CD') {
                document.getElementById("filterCategorytypeCD").style.visibility = "visible";
                document.getElementById("filterCategoryflowthru").style.visibility = "hidden";
                if (selectItem == 'Approved') {
                    urldd = 'api/drop/materialtypecd/Approved';
                }
                else if (selectItem == 'Unapproved') {
                    urldd = 'api/drop/materialtypecd/Unapproved';
                }
                else {
                    urldd = 'api/drop/materialtypecd';
                }
                $.ajax({
                    type: "GET",
                    url: urldd,
                    data: searchJSON,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successFunc,
                    error: errorFunc,
                    context: self
                });
            }
            else if (selectedOpt == 'MATERIAL_FLOW_THRU') {
                document.getElementById("filterCategorytypeCD").style.visibility = "hidden";
                document.getElementById("filterCategoryflowthru").style.visibility = "visible";
                if (selectItem == 'Y') {
                    urldd = 'api/drop/materialflowthru/Y';
                }
                else if (selectItem == 'N') {
                    urldd = 'api/drop/materialflowthru/N';
                }
                else {
                    urldd = 'api/drop/materialflowthru';
                }
                $.ajax({
                    type: "GET",
                    url: urldd,
                    data: searchJSON,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successFunc1,
                    error: errorFunc,
                    context: self
                });
            }
            function successFunc(data, status) {
                $("#interstitial").hide();
                var results = JSON.parse(data);
                var dropItem;
                self.showDDF(false);
                self.showDDT(true);
                dropItem = new viewdrop(data);
                self.DDviewDropdown(dropItem);
                setTimeout(function () { $("#DDviewDropdown").DataTable(); }, 3000);
            }
            function successFunc1(data, status) {
                $("#interstitial").hide();
                var results = JSON.parse(data);
                var dropItem1;
                self.showDDT(false);
                self.showDDF(true);
                dropItem1 = new viewflow(data);
                self.DDviewFlowthru(dropItem1);
                setTimeout(function () { $("#idDDviewFlowthru").DataTable(); }, 3000);
            }
            function errorFunc() {
                $("#interstitial").hide();
                alert('error');
            }
        };
        return DDdropDownTable;
    });





