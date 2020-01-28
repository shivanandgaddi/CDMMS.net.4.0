define(['durandal/composition','durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', './viewCategory', 'jquerydatatable', 'jqueryui', 'datablescelledit'],
    function (composition,app, ko, $, http, activator, mapping, system, viewcat, jquerydatatable, jqueryui, datablescelledit) {
        var results;
        var CategoryManagement = function () {
            var self = this;

            self.usr = require('Utility/user');
            self.viewCategory = ko.observable();
        };
        CategoryManagement.prototype.checkEnter = function (root, event) {
            if (event.keyCode === 13) {
                console.log("Enter");
                root.Search();
            }
            else { return true; }
        };

        CategoryManagement.prototype.Search = function () {
            var intHeight = $("body").height();
            $("#interstitial").css("height", intHeight);
            $("#interstitial").show();

            var searchValue = document.getElementById('MaterialGroupId').value;
            var self = this;

            console.log('CategoryManagement.search ' + searchValue);

            if (searchValue != '') {
                $.ajax({
                    type: "GET",
                    url: 'api/category/search',
                    data: { 'val': searchValue },
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successFunc,
                    error: errorFunc,
                    context: self
                });
            }

            function successFunc(data, status) {
                $("#interstitial").hide();
                var self = this;
                var catItem;
                catItem = new viewcat(data);
                self.viewCategory(catItem);
            }

            function errorFunc() {
                alert('error');
            }

            $("#btn_search").click(function () {
                if ($("#MaterialGroupId").val() == '')
                    app.showMessage("Please Enter Material Group ID");
            });
        };

        return CategoryManagement;
    });