define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system',
    './shelfSpecification', './slotAssignment', 'durandal/app', 'datablescelledit', 'bootstrapJS', 'jquerydatatable', 'jqueryui', '../Utility/referenceDataHelper'],
    function (composition, ko, $, http, activator, mapping, system, shelfSpec, slotSpecAssign, app, datablescelledit, bootstrapJS, jquerydatatable, jqueryui, reference) {
        var shelfModel = function (data) {
            selfspec = this;

            //var results = JSON.parse(data);
            //var nodeId = results["nodeid"];

            selfspec.usr = require('Utility/user');
            selfspec.showModalHLPNMultiBool = ko.observable(true);
            selfspec.shelfassignTbl = ko.observable();

            selfspec.shelfSpecification = ko.observable();
            selfspec.slotAssignment = ko.observable();
            selfspec.node_value = ko.observable('');
            selfspec.node_name = ko.observable('');
            selfspec.sequence_value = ko.observable('');
            selfspec.nodeStartno = ko.observable();
            selfspec.backToMtlItmId = 0;
            selfspec.idShelfList = ko.observable();
            selfspec.SpecList = ko.observableArray();
            selfspec.ShelfList = ko.observableArray();
            selfspec.ShelfListOriginal = ko.observableArray();
            selfspec.ShelfListModified = ko.observableArray();
            selfspec.shelfAliasList = ko.observableArray();
            selfspec.selectedoption = ko.observable(1);
            selfspec.nodeStartno = selfspec.selectedoption;

            if (typeof ko.unwrap(self.navigateMaterialToSpec) !== 'undefined') {
                if (self.navigateMaterialToSpec() == 'navigateMaterialToSpecNew') {
                    selfspec.selectRadioSpec = ko.observable('newSpec');
                } else {
                    selfspec.selectRadioSpec = ko.observable('existSpec');
                }
            } else {
                selfspec.selectRadioSpec = ko.observable('existSpec');
            }

            selfspec.selectedNewSpecification = ko.observable('');
            selfspec.statusSpecs = ko.observableArray(['', 'Completed', 'Propagated', 'Deleted']);
            selfspec.availableStartno = ko.observableArray(['1', '0'])

            selfspec.existingSelectedIdCurrent = ko.observable();
            selfspec.existingSelectedspecTypeCurrent = ko.observable();

            selfspec.existedData = ko.observable(false);
            selfspec.actioncode = ko.observable('');
            selfspec.selectedShelfId = ko.observable();
            selfspec.selectedShelfName = ko.observable();
            selfspec.node_name = ko.observable();
            selfspec.node_name("Node Name1");
            selfspec.selectedShelfId('');
            selfspec.selectedShelfName('');
            selfspec.enableSaveButton = ko.observable(false);
            selfspec.enableUpdateButton = ko.observable(false);

        };

        shelfModel.prototype.getNodeName = function (id) {
            var searchJSON = "";
            $.ajax({
                type: "GET",
                url: 'api/specnModel/getNodename/' + id,
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: selfspec
            });
            function successFunc(data, status) {
                if (data == '') {
                    $("#interstitial").hide();
                    app.showMessage("Node name not found", 'Shelf Assignment');
                    return false;
                }
                else {                    
                    $("#interstitial").hide();
                    $("#idNode").val(data);
                    selfspec.node_name(data);
                    return true;
                }
            }
            function errorFunc(data) {
                $("#interstitial").hide();
                app.showMessage("error : " + data, 'Shelf Assignment');
                return false;
            }
        };


        var count = 0;

        shelfModel.prototype.searchForShelfDtl = function (mtl) {

            if (selfspec.getNodeName(mtl.node_value()) == false) {
                return false;
            }

            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            var nodeid = mtl.node_value();  //251
            var nodeName = "%";
            $("#idShelfdetails").hide();
            selfspec.ShelfList([]);

            var searchJSON = {
                id: nodeid, name: nodeName
            };

            $.ajax({
                type: "GET",
                url: 'api/specnModel/search',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: selfspec
            });
            function successFunc(data, status) {

                if (data == 'no_results') {
                    $("#interstitial").hide();
                    app.showMessage("No records found. Please add the shelf details for this node.", 'Shelf Assignment');
                    selfspec.existedData(false);
                }
                else {
                    selfspec.enableSaveButton(false);
                    selfspec.enableUpdateButton(true);
                    selfspec.actioncode = 'update';
                    var results = JSON.parse(data);
                    selfspec.ShelfList(results);
                    selfspec.ShelfListOriginal(results);
                    selfspec.existedData(true);
                    $("#interstitial").hide();
                }
            }
            function errorFunc() {
                $("#interstitial").hide();
                app.showMessage("error", 'Shelf Assignment');
            }
        };

        shelfModel.prototype.getShelfAlias = function () {

            // var spanAlias = document.getElementById('idShlfAliasclose');
            // var Aliasdtls = document.getElementById('divShlfAliasdtls');

            if (selfspec.node_value() == "") {
                $("#idNode").attr('placeholder', 'Please enter node name..');
                $("#idNode").focus().select();
                return false;
            }

            if (selfspec.selectedShelfId() == '') {
                app.showMessage("Please select a shelf for Alias details", 'Shelf Assignment').then(function () {
                    return;
                });
                return false;
            }

            var selectedId = selfspec.selectedShelfId();
            var specType = "SHELF";
            // selectedId = 251;
            var url = 'api/specn/getAlias/' + selectedId + '/' + specType;
            //alert(url);

            if ($("#btnShfDisplayAlias").html() == "Hide DisplayAlias") {
                //alert('inside toggle func');
                $("#btnShfDisplayAlias").html('DisplayAlias');
                selfspec.shelfAliasList(null);
                $("#divShfAliasDtls").hide();
            }
            else {

                $("#divShfAliasDtls").show();

                http.get(url).then(function (response) {
                    console.log(response);
                    if (response === "{}") {
                        $("#interstitial").hide();
                        app.showMessage("No results found for Alias", 'Shelf Assignment').then(function () {
                            $("#divShfAliasDtls").hide();
                            return;
                        });
                    } else {
                        var results = JSON.parse(response);
                        //alert(results);
                        var results = JSON.parse(response);
                        selfspec.shelfAliasList(results);
                        $("#btnShfDisplayAlias").html('Hide DisplayAlias');
                    }
                });
            }

        };

        //popup for Adding shelf specification..        
        shelfModel.prototype.addShelf = function (data) {

            if (selfspec.node_value() == "") {
                //document.getElementById("idNode").focus();
                //app.showMessage("Please enter node name..");
                $("#idNode").focus().select();
                $("#idNode").attr('placeholder', 'Please enter node name..');
                return;
            }

            $("#idSpecsearchlist").hide();
            var intHeight = $("body").height();
            $("#interstitial").css("height", intHeight);
            $("#interstitial").show();
            var span = document.getElementById('idShelfclose');
            var modal = document.getElementById('idSpecsearchlist');
            var nodeid = "%";
            var Name = "%";
            var Description = "%";
            var Status = "%";
            var spectype = 'SHELF';
            var SpecClass = "%";

            if (selfspec.existedData() == false) {
                selfspec.enableSaveButton(true);
                selfspec.enableUpdateButton(false);
            }

            var searchJSON = {
                id: nodeid, name: Name, desc: Description, status: Status, specnType: spectype, class: SpecClass
            };

            $.ajax({
                type: "GET",
                url: 'api/specn/search',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: selfspec
            });
            function successFunc(data, status) {

                if (data == 'no_results') {
                    $("#interstitial").hide();
                    app.showMessage("No records found...", 'Shelf Assignment');
                }
                else {

                    $("#idSpecsearchlist").show();
                    modal.style.display = "block";
                    selfspec.SpecList([]);
                    var shelf_table = $('#idSpecList').DataTable();
                    shelf_table.clear();
                    var results = JSON.parse(data);
                    selfspec.SpecList(results);
                    setTimeout(function () { $("#idSpecList").DataTable(); }, 1000);
                    $("#interstitial").hide();
                }
            }
            function errorFunc() {
                $("#interstitial").hide();
                alert('error');
            }

            span.onclick = function () {
                modal.style.display = "none";
                selfspec.SpecList([]);
            }
        };


        shelfModel.prototype.specificationSelected = function (selectedId, specType, stl, mtlItmId) {

            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            if (selectedId > 0) {
                selfspec.existingSelectedIdCurrent(selectedId);
                selfspec.existingSelectedspecTypeCurrent(specType);
            }

            if (mtlItmId && mtlItmId > 0) {
                selfspec.redirectMtlItmId = mtlItmId;
            } else {
                selfspec.redirectMtlItmId = 0;
            }

            var url = 'api/specn/' + selectedId + '/' + specType;

            http.get(url).then(function (response) {
                if (response === "{}") {
                    $("#interstitial").hide();
                }
                else {
                    $("#interstitial").hide();
                    $("#idShelfdetails").show();
                    // Adding this json format for identifying the screen flow 
                    // through specification/assignment module
                    var jsonres = {
                        resp: response,
                        specification: false
                    }
                    stl.shelfSpecification(new shelfSpec(jsonres));
                }
            });
        };

        shelfModel.prototype.specificationSelectCall = function (selected) {
            console.log("selectedId = " + selected.specn_id.value);
            var selectedId = selected.specn_id.value;
            var specType = selected.enumSpecTyp.value;

            count = selfspec.ShelfList().length + 1;

            var jsonData;
            var json;
            var results;

            if (selfspec.existedData() == true) {
                selfspec.actioncode = 'update';
                selfspec.enableUpdateButton(true);
            }
            else {
                selfspec.actioncode = 'insert';
                selfspec.enableSaveButton(true);
                selfspec.shelfStartno();
            }
            count = "Item" + count;
            jsonData = {
                specn_shlvsdef_id: count,
                node_Id: selfspec.node_value(),
                specn_id: selected.specn_id.value,
                shelf_nm: selected.specn_nm.value,
                seq_num: '',
                shelf_qty: '',
                shelf_no_offset: '',
                extra_shelves: 'N',
            };

            json = ko.toJSON(jsonData);
            results = JSON.parse(json);
            selfspec.ShelfList.push(results);
        };

        shelfModel.prototype.selectedShelf = function (selected) {
            // hide the alias panel and change button text, so that user can click Display button to view alias details for selected spec 
            $("#btnShfDisplayAlias").html('DisplayAlias');
            $("#divShfAliasDtls").hide();
            // end
            $("#interstitial").css("height", "100%");
            $("#idShelfdetails").show();
            var specId = selected.specn_id;
            var specType = "SHELF";
            selfspec.selectedShelfId(specId);
            selfspec.selectedShelfName(selected.shelf_nm);
            selfspec.backToMtlItmId = 0;
            selfspec.specificationSelected(specId, specType, selfspec, 0);
        };


        shelfModel.prototype.removeShelfspec = function (selected) {
            selfspec.shelfSpecification(false);

            if (selected.specn_shlvsdef_id.indexOf("Item") >= 0) {
                selfspec.ShelfList.remove(selected);
                app.showMessage("Removed Successfully", 'Shelf Assignment');
                return;
            }


            var jsonData = {
                specn_shlvsdef_id: selected.specn_shlvsdef_id,
                node_Id: selfspec.node_value(),
                specn_id: selected.specn_id,
                shelf_nm: selected.shelf_nm,
                seq_num: selected.seq_num,
                shelf_qty: selected.shelf_qty,
                shelf_no_offset: selected.shelf_no_offset,
                extra_shelves: selected.extra_shelves,
            };

            var json = ko.toJSON(jsonData);
            var results = JSON.parse(json);

            selfspec.ShelfListTmp = ko.observableArray();
            selfspec.ShelfListTmp.push(results);

            $("#interstitial").show();
            selfspec.actioncode = 'delete';
            var jsonUpdate = {
                shelfStnum: selfspec.nodeStartno,
                actioncode: selfspec.actioncode,
                ShelfDtls: selfspec.ShelfListTmp()
            };
            selfspec.shelfassignTbl(jsonUpdate);
            var saveJSON = mapping.toJS(selfspec.shelfassignTbl());

            $.ajax({
                type: "POST",
                url: 'api/specnModel/update/',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: deleteSuccess,
                error: deleteError
            });
            function deleteSuccess(response) {
                $("#interstitial").hide();
                app.showMessage(response, 'Shelf Assignment');
                selfspec.ShelfList.remove(selected);
                selfspec.calculateSequence();
            }
            function deleteError(err) {
                $("#interstitial").hide();
                app.showMessage('Error', 'Shelf Assignment');
            }

        };

        shelfModel.prototype.calculateAll = function (item) {
            selfspec.shelfSpecification(false);
            var selectId = item.specn_shlvsdef_id;
            var indexid = parseInt(0);
            var shlfQty = item.shelf_qty;
            for (var i = 0; i < selfspec.ShelfList().length; i++) {
                if (selectId == selfspec.ShelfList()[i].specn_shlvsdef_id) {
                    indexid = i;
                    break;
                }
            }

            var resval = 0;
            var seqVal = parseInt(selfspec.ShelfList()[indexid].seq_num);
            if (parseInt(indexid) != parseInt(selfspec.ShelfList().length - 1)) {
                resval = parseInt(shlfQty) + parseInt(seqVal);
                selfspec.ShelfList()[parseInt(indexid) + 1].seq_num = parseInt(resval);
                selfspec.ShelfListTmp = ko.observableArray();
                selfspec.ShelfListTmp(selfspec.ShelfList());
                selfspec.ShelfList([]);
                selfspec.ShelfList(selfspec.ShelfListTmp());
            }
            return true;
        };

        shelfModel.prototype.calculateSequence = function (item) {
            selfspec.ShelfList()[0].seq_num = parseInt(selfspec.selectedoption());
            for (var i = 0; i < selfspec.ShelfList().length; i++) {
                selfspec.calculateAll(selfspec.ShelfList()[i]);
            }

            return true;
        };

        shelfModel.prototype.shelfStartno = function () {
            var startSeq = parseInt(selfspec.selectedoption());

            if (selfspec.ShelfList().length > 0) {
                if (startSeq == 0) {
                    selfspec.selectedoption(1);
                    app.showMessage("Shelf start number should not be 0.", 'Shelf Assignment');
                    return false;
                }

                selfspec.calculateSequence();
                selfspec.nodeStartno = parseInt(startSeq);
            }
        };

        shelfModel.prototype.onchangeExtraShelvs = function (item) {
            var selectedId = item.specn_shlvsdef_id;
            if (item.extra_shelves == 'Y' || item.extra_shelves == true) {
                for (var i = 0; i < selfspec.ShelfList().length; i++) {
                    if (selectedId == selfspec.ShelfList()[i].specn_shlvsdef_id) {
                        selfspec.ShelfList()[i].extra_shelves = false;
                        break;
                    }
                }

            } else {
                for (var i = 0; i < selfspec.ShelfList().length; i++) {
                    if (selectedId == selfspec.ShelfList()[i].specn_shlvsdef_id) {
                        selfspec.ShelfList()[i].extra_shelves = true;
                        break;
                    }
                }

            }

            return true;
        };

        shelfModel.prototype.grdrDataValidation = function () {
            for (var j = 0; j < selfspec.ShelfList().length; j++) {
                if (selfspec.ShelfList()[j].specn_shlvsdef_id == "") {
                    app.showMessage("Unique constraint id is empty");
                    return false;
                }
                else if (selfspec.ShelfList()[j].seq_num == "") {
                    app.showMessage("Please enter the sequence number for shelf name :" + selfspec.ShelfList()[j].shelf_nm);
                    return false;
                }
                else if (selfspec.ShelfList()[j].shelf_qty == "") {
                    app.showMessage("Please enter the quantity for shelf name :" + selfspec.ShelfList()[j].shelf_nm);
                    return false;
                }
                else if (selfspec.ShelfList()[j].shelf_no_offset == "") {
                    app.showMessage("Please enter the shelf offset number for shelf name :" + selfspec.ShelfList()[j].shelf_nm);
                    return false;
                }
                else if (selfspec.ShelfList()[j].extra_shelves == "") {
                    app.showMessage("Please check the extra shelve required or not for shelf name :" + selfspec.ShelfList()[j].shelf_nm);
                    return false;
                }
            }
        };

        shelfModel.prototype.saveShelf = function () {
            if (selfspec.ShelfList().length == 0) {
                app.showMessage("Records not selected for insert", 'Shelf Assignment');
                return false;
            }
            else if (selfspec.grdrDataValidation() == false) {
                return false;
            }
            //selfspec.filterData();

            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfspec.actioncode = 'update';
            var jsonUpdate = {
                shelfStnum: selfspec.nodeStartno,
                actioncode: selfspec.actioncode,
                ShelfDtls: selfspec.ShelfList()
            };
            selfspec.shelfassignTbl(jsonUpdate);
            var saveJSON = mapping.toJS(selfspec.shelfassignTbl());

            $.ajax({
                type: "POST",
                url: 'api/specnModel/update/',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: insertSuccess,
                error: insertError
            });
            function insertSuccess(response) {
                $("#interstitial").hide();
                app.showMessage(response, 'Shelf Assignment');
                selfspec.enableSaveButton(false);
                selfspec.reset();
            }
            function insertError(err) {
                $("#interstitial").hide();
                app.showMessage('Error', 'Shelf Assignment');
            }
        };

        shelfModel.prototype.updateShelf = function () {
            if (selfspec.ShelfList().length == 0) {
                app.showMessage("Records not selected for update", 'Shelf Assignment');
                return false;
            }
            else if (selfspec.grdrDataValidation() == false) {
                return false;
            }
            //selfspec.filterData();

            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfspec.actioncode = 'update';
            var jsonUpdate = {
                shelfStnum: selfspec.nodeStartno,
                actioncode: selfspec.actioncode,
                ShelfDtls: selfspec.ShelfList()
            };
            selfspec.shelfassignTbl(jsonUpdate);
            var saveJSON = mapping.toJS(selfspec.shelfassignTbl());

            $.ajax({
                type: "POST",
                url: 'api/specnModel/update/',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: updateSuccess,
                error: updateError
            });
            function updateSuccess(response) {
                $("#interstitial").hide();
                selfspec.reset();
                app.showMessage(response, 'Shelf Assignment');
                selfspec.enableUpdateButton(false);
            }
            function updateError(err) {
                $("#interstitial").hide();
                app.showMessage('Error', 'Shelf Assignment');
            }
        };

        //End of popup display

        shelfModel.prototype.reset = function () {
            selfspec.SpecList([]);
            selfspec.ShelfList([]);
            selfspec.node_value('');
            selfspec.slotAssignment(false);
            selfspec.shelfSpecification(false);

        };


        shelfModel.prototype.getSlotdtls = function () {
            selfspec.shelfSpecification(false);
            console.log("Shelf Id selectd : " + selfspec.selectedShelfId());
            if (selfspec.selectedShelfId() == "") {
                app.showMessage('Please select shelf from the table..');
                return false;
            }
            $("#idShelfdetails").hide();
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            var request = {
                nodeId: selfspec.node_value(),
                nodeName: selfspec.node_name(),
                specType: 'SHELF',
                shelfId: selfspec.selectedShelfId(),
                shelfName: selfspec.selectedShelfName,
                specType: 'SHELF'
            };
            var requestJson = ko.toJSON(request);
            $("#interstitial").hide();
            $("#idShelfdetails").show();
            selfspec.slotAssignment(new slotSpecAssign(requestJson));

        };
        shelfModel.prototype.scrollToSpecDetails = function () {
            setTimeout(function () {
                var elmnt = null;

                if (selfspec.selectRadioSpec() == 'newSpec') {
                    elmnt = document.getElementById("idSpecsearchlist");

                    elmnt.scrollIntoView(false);
                } else {
                    elmnt = document.getElementById("scrollAnchor");

                    elmnt.scrollIntoView(true);
                }

                $("#interstitial").hide();
            }, 1000);
        };


        return shelfModel;
    });