define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', './nupSearchResults', './nupShowDetails', 'jquerydatatable', 'jqueryui', 'datablescelledit'
], function (composition, app, ko, $, http, activator, mapping, system, nupsr, nupsd, jquerydatatable, jqueryui, datablescelledit) {
    var NewUpdatedParts = function () {
        /*Main page of this view. Input values will be collected to use for search.*/
        self = this;
        self.MaterialCode = ko.observable();
        self.SapPartNo = ko.observable();
        self.SapDescr = ko.observable();
        self.Heci = ko.observable();
        self.isNew = ko.observable(false);
        self.selectedMaterialLink = ko.observable();
        self.nupShowDetails = ko.observable();

        self.shownupShowDetails = ko.observable();

        self.nupSearchResults = ko.observable();
        self.recordtype = ko.observable("");
        self.viewrejected = ko.observable(false);
        self.rejectedcuid = ko.observable('');
        self.usr = require('Utility/user');
        setTimeout(function () {
            $("#startdate").datepicker();
            $("#enddate").datepicker();
        }, 3000);
    };
    NewUpdatedParts.prototype.checkEnter = function (root, event) {
        if (event.keyCode === 13) {
            console.log("Enter");
            self.Search();
        }
        else { return true; }
    };

    NewUpdatedParts.prototype.Search = function () {
        /*On Click Search. Input values used to search the data. It allows wild character on empty or partially entered field.*/
        self.shownupShowDetails(false);
        self.nupSearchResults(false);
        var intHeight = $("body").height();
        $("#interstitial").css("height", intHeight);
        $("#interstitial").show();

        var min = $('#minnumber').val();
        var max = $('#maxnumber').val()
        if (!isNumeric(min) || !isNumeric(max) || min > max || min % 1 != 0 || max % 1 != 0 || min <= 0 || max <= 0)
        {
            min = 1;
            max = 100;
        }

        var searchJSON = {
            mtlcode: self.MaterialCode(), partno: self.SapPartNo(), heci: self.Heci(), descr: self.SapDescr(), isnew: self.isNew(), startdate: $("#startdate").val(), enddate: $("#enddate").val(), recordtype: self.recordtype(), minsearch: min, maxsearch: max, viewrejected: self.viewrejected(), rejectedcuid: self.rejectedcuid()
        };

        if ($("#startdate").val() > $("#enddate").val()) {
            app.showMessage("End date should be greater than Start date.");
            $("#interstitial").hide();
        }
        else {
            $.ajax({
                type: "GET",
                url: 'api/newupdatedparts/search',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: self
            });
            function successFunc(data, status) {
                if (data == 'null') {
                    app.showMessage("No records found", 'Failed');
                    $("#interstitial").hide();
                } else {
                    var nuplist;
                    self.shownupShowDetails(false);
                    nuplist = new nupsr(data);
                    self.nupSearchResults(nuplist);
                    setTimeout(function () {
                        $("#newupdatedparts").DataTable();
                        var elmnt = document.getElementById("nupSearchResultsDiv");
                        elmnt.scrollIntoView();
                        $("#interstitial").hide();
                    }, 1000);                   
                }
            }
            function errorFunc() {
                $("#interstitial").hide();
                alert('error');
            }
        }
    };

    function isNumeric(n) {
        return !isNaN(parseFloat(n)) && isFinite(n);
    }

    return NewUpdatedParts;
});