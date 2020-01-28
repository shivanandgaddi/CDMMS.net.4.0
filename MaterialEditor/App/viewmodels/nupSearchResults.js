define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', './newUpdatedParts', './nupShowDetails', 'jquerydatatable', 'jqueryui', 'datablescelledit', '../Utility/referenceDataHelper'],
    function (composition, app, ko, $, http, activator, mapping, system, nupup, nupsd, jquerydatatable, jqueryui, datablescelledit, reference) {

        var NUPSearchResults = function (data) {
            /* Initiates and mapped the values into table on search event.*/
            selfsr = this;
            var results = JSON.parse(data);
            selfsr.NupSearchlist = mapping.fromJS(results);

            selfsr.NupPossibleMatchList = ko.observableArray();
            selfsr.CopyMaterialAccept = ko.observable();
            
            selfsr.nupItem = ko.observable();

            selfsr.showsap = ko.observable(false);
            selfsr.showlosdb = ko.observable(false);
            selfsr.mike = ko.observable();

            selfsr.PossiblematchLosdb = ko.observable();
            selfsr.posblematchlosdb = ko.observable();
            selfsr.posblematchsap = ko.observable();
            selfsr.PossiblematchSap = ko.observable();
            selfsr.MakeCurrentRev = ko.observable(false);

            //selfsr.PossibleRevMatchList = ko.observableArray('');
            selfsr.nupShowPossibleMatches = ko.observable();

            selfsr.nupShowDetailslosdb = ko.observable();
            selfsr.shownupShowDetailslosdb = ko.observable();
            $('#newupdatedparts').DataTable();
            selfsr.exists = true;
        };

        NUPSearchResults.prototype.MaterialLink = function (selectedMaterialLink) {
            /* Below function will be called on show details to show the detailed updates from catalog staging*/
            var dataResponse = [];
            var constantNull = '{"id": {"value": -1}}';
            if (selectedMaterialLink.Attributes.type.value() == "SAP") {
                var url = 'api/newupdatedparts/' + selectedMaterialLink.Attributes.materialcode.value() + '/' + self.recordtype();
                var intHeight = $("body").height();
                $("#interstitial").css("height", intHeight);
                $("#interstitial").show();
                //var nupItem;

                http.get(url).then(function (response) {
                    url = 'api/newupdatedparts/possiblelosdb/' + selectedMaterialLink.Attributes.materialcode.value();
                    if (response === "{}")
                        dataResponse[0] = constantNull;
                    else
                        dataResponse[0] = response;
                    http.get(url).then(function (responsePossible) {
                        if (responsePossible === "{}")
                            dataResponse[1] = constantNull;
                        else
                            dataResponse[1] = responsePossible;

                        selfsr.nupItem = new nupsd(dataResponse, selectedMaterialLink, selectedMaterialLink.Attributes.materialcode.value(), self.usr.cuid, self.recordtype(), 'SAP', self.isNew());
                        self.shownupShowDetails(true)
                        self.nupShowDetails(selfsr.nupItem).PossiblematchLosdb;
                        setTimeout(function () {
                            var elmnt = document.getElementById("nupShowDetailsDiv");
                            elmnt.scrollIntoView();
                            $("#interstitial").hide();
                        }, 1000);
                    });
                });
            }
            else if (selectedMaterialLink.Attributes.type.value() == "LOSDB") {

                var intHeight = $("body").height();
                $("#interstitial").css("height", intHeight);
                $("#interstitial").show();

                var url = 'api/newupdatedparts/losdb/' + selectedMaterialLink.Attributes.materialcode.value();
                http.get(url).then(function (response) {
                    if (response === "{}" || response === "null")
                        dataResponse[0] = constantNull;
                    else
                        dataResponse[0] = response;
                    url = 'api/newupdatedparts/possiblesap/' + selectedMaterialLink.Attributes.materialcode.value();
                    http.get(url).then(function (responsePossible) {
                        if (responsePossible === "{}" || responsePossible == "null")
                            dataResponse[1] = constantNull;
                        else
                            dataResponse[1] = responsePossible;
                        url = 'api/newupdatedparts/revision/' + selectedMaterialLink.Attributes.materialcode.value();
                        http.get(url).then(function (responseRevision) {
                            if (responseRevision === "{}" || responseRevision == "null")
                                dataResponse[2] = constantNull;
                            else
                                dataResponse[2] = responseRevision;

                            nupItem = new nupsd(dataResponse, selectedMaterialLink, selectedMaterialLink.Attributes.materialcode.value(), self.usr.cuid, self.recordtype(), 'LOSDB', self.isNew());
                            self.shownupShowDetails(true);
                            self.nupShowDetails(nupItem);
                            setTimeout(function () {                            
                                var elmnt = document.getElementById("nupShowDetailsDiv");
                                elmnt.scrollIntoView();
                                $("#interstitial").hide();
                            }, 1000);
                        });
                    });
                });
            }
        };
        NUPSearchResults.prototype.MaterialReject = function (selectedMaterialReject) {
            /* On click Reject. Selected material updates will be reject and catalog staging updates will not flow through*/
            var url = 'api/newupdatedparts/reject/' + selectedMaterialReject.Attributes.materialcode.value() + '/' + self.usr.cuid + '/' + self.recordtype();
            var intHeight = $("body").height();

            $("#interstitial").css("height", intHeight);
            $("#interstitial").show();

            http.get(url).then(function (response) {
                var nuprej;

                if (response === "") {
                    nuprej = new nupsd('{"id": {"value": -1}}', '{"id": {"value": -1}}', null);
                    app.showMessage("Updates failed. Please check");
                }
                else {
                    app.showMessage('Selected material updates are rejected successfully.', 'Rejected:Success');
                }

                self.Search();
                $("#interstitial").hide();
            },
            function (error) {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            });
        }

        NUPSearchResults.prototype.cancelPossibleMatchModal = function () {

            var modal = document.getElementById('nupPossibleMatchesModal');
            modal.style.display = "none";
        };

        NUPSearchResults.prototype.MaterialAccept = function (selectedMaterialAccept, rootContext) {
            /* Look first to see if this part number is a root part number being used already */
            var intHeight = $("body").height();

            $("#interstitial").css("height", intHeight);
            $("#interstitial").show();

            selfsr.CopyMaterialAccept(selectedMaterialAccept);

            var searchJSON = {
                mfg_part_no: selectedMaterialAccept.Attributes.mfg_part_no.value(),
                clmc: selectedMaterialAccept.Attributes.clmc.value()
            };
            $.ajax({
                type: "GET",
                url: 'api/newupdatedparts/partialcheck',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successSearch,
                error: errorFunc,
                context: selfsr,
                async: false
            });

            function successSearch(data, status) {
                if (data === null) {
                    $("#interstitial").hide();
                    $(".NoRecordrp").show();
                    setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
                } else {
                    var results = JSON.parse(data);
                    if (results.length >= 1) {  // possible revision from part matching
                        selfsr.NupPossibleMatchList(mapping.fromJS(results)());
                        var modal = document.getElementById('nupPossibleMatchesModal');
                        modal.style.display = "block";
                        $("#interstitial").hide();
                    }
                    else {  // proceed as usual
                        finishAccept(selectedMaterialAccept);
                    }
                }
            }
            function errorFunc() {
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            };

            $("#interstitial").hide();
            return;
        };

        function finishAccept(selectedMaterialAccept) {
            if (selectedMaterialAccept.Attributes.type.value() == "SAP") {
                // Check to see if there is any LOSDB material potentially here and checked.  If there is, need to get the losdb prodId and eqptCtlgId
                var checkedcount = 0;
                var losdbProductID = '0';
                var iesEqptProdID = '0';
                if (selfsr.nupItem.PossiblematchLosdb != null && selfsr.nupItem.PossiblematchLosdb() != null) {
                    for (var i = 0; i < selfsr.nupItem.PossiblematchLosdb().length; i++) {
                        if (selfsr.nupItem.PossiblematchLosdb()[i].Attributes.checkmatched.value() == true) {
                            losdbProductID = selfsr.nupItem.PossiblematchLosdb()[i].Attributes.prodt_id.value();
                            iesEqptProdID = selfsr.nupItem.PossiblematchLosdb()[i].Attributes.ies_eqpt_ctlg_item_id.value();
                            checkedcount++;
                        }
                    }
                    if (checkedcount > 1) {
                        alert('Select one LOSDB part to associate to an SAP part.');
                        return;
                    }
                }

                $("#interstitial").css("height", intHeight);
                $("#interstitial").show();

                /* On click Accept. Selected material updates will be accepted and catalog staging updates will flow through item SAP*/
                var url = 'api/newupdatedparts/accept/' + selectedMaterialAccept.Attributes.materialcode.value() + '/' + self.usr.cuid + '/' +
                    self.isNew() + '/' + losdbProductID + '/' + iesEqptProdID + '/' + selectedMaterialAccept.Attributes.clmc.value() + '/' + self.recordtype();
                var intHeight = $("body").height();
                var jsonBody = '{"mtlDsc": "' + selectedMaterialAccept.Attributes.mtl_desc.value() + '"}';

                http.post(url, jsonBody).then(function (response) {
                    if (response === "") {
                        app.showMessage("Unable to process your request due to an internal error. If problem persists please contact your system administrator.");
                    }
                    else if (response === 'success'){
                        app.showMessage('Selected material updates are accepted successfully.', 'Accepted:Success');
                    }
                    else {
                        // anything but empty or success will be a string like specid/worktodoid/spectype so call reference method for the spec update popup
                        var fields = response.split('/');
                        var specHelper = new reference();
                        specHelper.getSpecificationSendToNdsStatus(fields[0], fields[1], fields[2]);

                        app.showMessage('Selected material updates are accepted successfully.', 'Accepted:Success');
                    }

                    self.Search();

                    $("#interstitial").hide();
                },
                function (error) {
                    $("#interstitial").hide();
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                });
            }
            else if (selectedMaterialAccept.Attributes.type.value() == "LOSDB") {
                /* On click Accept. Selected material updates will be accepted and catalog staging updates will flow through item SAP*/
                var url = 'api/newupdatedparts/acceptlosdb/' + selectedMaterialAccept.Attributes.auditdaid.value() + ' / ' + selectedMaterialAccept.Attributes.usraprvtxt.value() + ' / ' + selectedMaterialAccept.Attributes.usraprvtmstp.value() + ' / ' + selectedMaterialAccept.Attributes.usraprvind.value()
                    + '/' + selectedMaterialAccept.Attributes.AuditTablePkColumnName.value() + '/' + selectedMaterialAccept.Attributes.CDMMSColumnName.value() + '/' + selectedMaterialAccept.Attributes.NewColumnValue.value()
                + '/' + selectedMaterialAccept.Attributes.CDMMSTableName.value() + '/' + selectedMaterialAccept.Attributes.losdbprodid.value() + '/' + selectedMaterialAccept.Attributes.AuditTablePkColumnValue.value()
                + '/' + selectedMaterialAccept.Attributes.AuditParentTablePKColumnName.value() + '/' + selectedMaterialAccept.Attributes.AuditParentTablePkColumnValue.value();
                var intHeight = $("body").height();

                $("#interstitial").css("height", intHeight);
                $("#interstitial").show();

                http.get(url).then(function (response) {
                    var nuprej;

                    if (response === "") {
                        nuprej = new nupsd('{"id": {"value": -1}}', '{"id": {"value": -1}}', null);
                        app.showMessage("Updates failed. Please check");
                    }
                    else {
                        app.showMessage('Selected material updates are accepted successfully.', 'Accepted:Success');
                    }

                    self.Search('no show');

                    $("#interstitial").hide();
                },
                function (error) {
                    $("#interstitial").hide();
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                });
            }
        }

        NUPSearchResults.prototype.saveRevisionModal = function (rootContext) {
            var newRevNo = document.getElementById("newRevNo").value;
            if (newRevNo === '') {
                alert('You must enter a New Revision Number to save a new revision.')
            }
            else {
                // count checked
                var numberchecked = 0;
                var checkedrow = 0;
                for (var i = 0; i < selfsr.NupPossibleMatchList().length; i++) {
                    if (selfsr.NupPossibleMatchList()[i].CheckedRow() == true)
                    {
                        numberchecked++;
                        checkedrow = i;
                    }
                }
                if (numberchecked == 0) {
                    alert('You must check one material to create the revision for.');
                }
                if (numberchecked > 1) {
                    alert('You must check only one material to create the revision for.');
                }
                if (numberchecked == 1) {
                    // Create record in material_item and then insert revision
                    finishAccept(selfsr.CopyMaterialAccept());

                    // Get the material_item_id.  this is needed for rme_****_mtrl_revsn table
                    var searchJSON = {
                        productid: selfsr.CopyMaterialAccept().Attributes.materialcode.value()
                    };
                    $.ajax({
                        type: "GET",
                        url: 'api/newupdatedparts/getmaterialid',
                        data: searchJSON,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: successSearchMaterialID,
                        error: errorFuncMaterialID,
                        context: selfsr,
                        async: false
                    });
                    function successSearchMaterialID(data, status) {
                        if (data === null) {
                            $("#interstitial").hide();
                            $(".NoRecordrp").show();
                            setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
                        } else {
                            var results = JSON.parse(data);
                            var materialItemID = results;
                            if (materialItemID != null)
                            {
                                // insert revision
                                var currentRev = 'N';
                                if (selfsr.MakeCurrentRev) {
                                    currentRev = 'Y';
                                }
                                
                                var searchJSON = {
                                    revisiontablename: selfsr.NupPossibleMatchList()[checkedrow].CDMMSRevisionTableName(),
                                    materialitemid: materialItemID,
                                    materialid: selfsr.NupPossibleMatchList()[checkedrow].MaterialID(),
                                    revisionnumber: newRevNo,
                                    materialcode: selfsr.CopyMaterialAccept().Attributes.materialcode.value(),
                                    baserevind: 'N',
                                    currentrevind: currentRev,
                                    retrevind: 'N'
                                };
                                $.ajax({
                                    type: "GET",
                                    url: 'api/newupdatedparts/insertgenericrevision',
                                    data: searchJSON,
                                    contentType: "application/json; charset=utf-8",
                                    dataType: "json",
                                    success: successInsertRevision,
                                    error: errorInsertRevison,
                                    context: selfsr,
                                    async: false
                                });
                                function successInsertRevision(data, status) {
                                    if (data === null) {
                                        $("#interstitial").hide();
                                        $(".NoRecordrp").show();
                                        setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
                                    } else {
                                        var results = JSON.parse(data);
                                        if (results === 'SUCCESS') {
                                            app.showMessage('Revision create successful.', 'Accepted:Success');
                                        }
                                        else {
                                            app.showMessage('Revision create not successful.');
                                        }
                                    }
                                }
                                function errorInsertRevison() {
                                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                                };
                            }
                            else {
                                // something went wrong, shouldn't be here
                            }
                        }
                    }
                    function errorFuncMaterialID() {
                        return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                    };
                }
            }
            return;
        }

        NUPSearchResults.prototype.continueModal = function (rootContext) {
            finishAccept(selfsr.CopyMaterialAccept());
        }

        return NUPSearchResults;
    });
