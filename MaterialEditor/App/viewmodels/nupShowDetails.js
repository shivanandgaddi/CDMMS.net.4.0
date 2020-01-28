define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system'], function (composition, app, ko, $, http, activator, mapping, system) {
    var NUPShowDetails = function (data, selectedMaterialLink, materialCode, userID, recordType, materialType, isNew) {
        /*On Click show details. Selected row material updates will be shown here.It includes all the updates required for review.*/
        var self = this;
        self.losdbmtlid = ko.observable(selectedMaterialLink.Attributes.materialcode.value());
        self.selectedMaterialCode = ko.observable(materialCode);
        self.selectedUserID = ko.observable(userID);
        self.selectedRecordType = ko.observable(recordType);
        self.selectedMaterialType = ko.observable(materialType);
        self.selectedIsNew = ko.observable(isNew);
        self.showsap = ko.observable(false);
        self.showlosdb = ko.observable(false);

        self.NupShowlist = ko.observable();
        self.PossiblematchLosdb = ko.observableArray();
        self.NupShowlistlosdb = ko.observable();
        self.PossiblematchSap = ko.observable();
        self.Addrevision = ko.observable();
        self.savedPossibleMatches = ko.observable();

        if (selectedMaterialLink.Attributes.type.value() == "SAP") {
            self.showsap = ko.observable(true);
            self.showlosdb = ko.observable(false);
            self.NupShowlist = mapping.fromJS(JSON.parse(data[0]));
            self.PossiblematchLosdb = mapping.fromJS(JSON.parse(data[1]));
            if (self.PossiblematchLosdb() != null) {
                for (var i = 0; i < self.PossiblematchLosdb().length; i++) {
                    if (self.PossiblematchLosdb()[i].Attributes.checkmatched.value() === 'false') {
                        self.PossiblematchLosdb()[i].Attributes.checkmatched.value(false);
                    }
                    else {
                        self.PossiblematchLosdb()[i].Attributes.checkmatched.value(true);
                    }
                }
                if (self.PossiblematchLosdb().length == 1) {  // go ahead and pre-check this if it's just one result
                    self.PossiblematchLosdb()[0].Attributes.checkmatched.value(true);
                }
            }
        }
        else if (selectedMaterialLink.Attributes.type.value() == "LOSDB") {
            self.showlosdb(true);
            self.showsap(false);
            self.NupShowlistlosdb = mapping.fromJS(JSON.parse(data[0]));
            self.PossiblematchSap = mapping.fromJS(JSON.parse(data[1]));
            self.Addrevision = mapping.fromJS(JSON.parse(data[2]));
        }
        self.exists = true;
    };

    

    NUPShowDetails.prototype.clickPossibleMatch = function (item) {
        //$(function () {
        //    // when any checkbox is checked
        //    $(':checkbox').click(function () {
        //        // uncheck any other checkboxes except this one
        //        $(':checkbox').not($(this)).prop('checked', false);
        //    });
        //})
    }

    $('input.example').on('change', function () {
        $('input.example').not(this).prop('checked', false);
    });

    NUPShowDetails.prototype.Associatesaptolosdb = function () {
        $('input.posblemtchchksap').on('change', function () {
            $('input.posblemtchchksap').not(this).prop('checked', false);
        });
        $('input.possiblematchchklosdb').on('change', function () {
            $('input.possiblematchchklosdb').not(this).prop('checked', false);
        });

        var self = this;
        var checkedcount = 0;
        var losdbProductID = '';
        for (var i = 0; i < self.PossiblematchLosdb().length; i++) {
            if (self.PossiblematchLosdb()[i].Attributes.checkmatched.value() == true) {
                losdbProductID = self.PossiblematchLosdb()[i].Attributes.prodt_id.value();
                iesEqptProdID = self.PossiblematchLosdb()[i].Attributes.ies_eqpt_ctlg_item_id.value();
                checkedcount++;
            }
        }
        if (checkedcount > 1 || checkedcount < 1) {
            alert('Select one LOSDB part to associate to an SAP part.');
            return;
        }

        if (self.selectedMaterialType() == "SAP") {
            // On click Accept. Selected material updates will be accepted and catalog staging updates will flow through item SAP
            var url = 'api/newupdatedparts/accept/' + self.selectedMaterialCode() + '/' + self.selectedUserID() + '/' + self.selectedIsNew() + '/' + losdbProductID + '/' + iesEqptProdID + '/' + self.selectedRecordType();
            var intHeight = $("body").height();

            $("#interstitial").css("height", intHeight);
            $("#interstitial").show();

            http.get(url).then(function (response) {
                var nuprej;

                if (response === "") {
                    nuprej = new nupsd('{"id": {"value": -1}}', '{"id": {"value": -1}}', null);
                    app.showMessage("Selected SAP material with LOSDB association failed to accept.");
                }
                else {
                    app.showMessage('Selected SAP material with LOSDB association has been accepted successfully.', 'Accepted:Success');
                }

                //TO DO get this to work somehow or at least tear down the 
                //self.Search();

                $("#interstitial").hide();
            },
                function (error) {
                    $("#interstitial").hide();
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                });
        }
        else if (self.selectedMaterialType.value() == "LOSDB") {
        }

        //$.ajax({
        //    type: "GET",
        //    url: 'api/newupdatedparts/associateSTL/' + self.losdbmtlid + '/' + $('#prodt_id').val(),
        //    contentType: "application/json; charset=utf-8",
        //    dataType: "json",
        //    success: successFunc,
        //    error: errorFunc,
        //    context: self
        //});

        //function successFunc(data, status) {

        //    if (data === 'SUCCESS') {
        //        app.showMessage('Updated successfully');
        //    } else {
        //        app.showMessage('Error', 'Try Again.!');

        //    }

        //}

        //function errorFunc() {
        //    alert('error');
        //}
    };

    NUPShowDetails.prototype.Associatelosdbtosap = function () {
        $('input.posblemtchchksap').on('change', function () {
            $('input.posblemtchchksap').not(this).prop('checked', false);

        });
        $('input.possiblematchchklosdb').on('change', function () {
            $('input.possiblematchchklosdb').not(this).prop('checked', false);

        });

        $.ajax({
            type: "GET",
            url: 'api/newupdatedparts/associateLTS/' + self.losdbmtlid + '/' + $('#prodt_id').val() + '/' + $('#ies_eqpt_ctlg_item_id').val(),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: successFunc,
            error: errorFunc,
            context: self
        });

        function successFunc(data, status) {

            if (data === 'SUCCESS') {
                app.showMessage('Updated successfully');
            } else {
                app.showMessage('Error', 'Try Again.!');

            }

        }

        function errorFunc() {
            alert('error');
        }
    };

    NUPShowDetails.prototype.Insertrevision = function () {

        $.ajax({
            type: "GET",
            url: 'api/newupdatedparts/insertrevision/' + self.losdbmtlid,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: successFunc,
            error: errorFunc,
            context: self
        });

        function successFunc(data, status) {

            if (data === 'SUCCESS') {
                app.showMessage('Inserted successfully');
            } else {
                app.showMessage('Error', 'Try Again.!');

            }

        }

        function errorFunc() {
            alert('error');
        }
    };
    return NUPShowDetails;
});
