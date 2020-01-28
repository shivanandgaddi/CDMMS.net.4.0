define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system',
    './shelfSpecification', './slotAssignment', 'durandal/app', 'datablescelledit', 'bootstrapJS', 'jquerydatatable', 'jqueryui', '../Utility/referenceDataHelper'],
    function (composition, ko, $, http, activator, mapping, system, shelfSpec, slotSpecAssign, app, datablescelledit, bootstrapJS, jquerydatatable, jqueryui, reference) {
        var portAssign = function (data) {
            prtAssig = this;
            var results = JSON.parse(data);
            var specInfo = results["specType"];

            prtAssig.usr = require('Utility/user');
            prtAssig.defId = ko.observable();

            prtAssig.nodeValue = ko.observable('');
            prtAssig.nodeName = ko.observable('');
            prtAssig.cardName = ko.observable('');
            prtAssig.cardValue = ko.observable('');
            prtAssig.startingPortNumber = ko.observable('');
            prtAssig.portassignTbl = ko.observable();
            prtAssig.actioncode = ko.observable();
            prtAssig.addBtnEnable = ko.observable(false);
            prtAssig.smBtnEnable = ko.observable(false);

            prtAssig.drpseqnumber = ko.observableArray(['']);
            prtAssig.drpquantity = ko.observableArray(['']);
            for (var i = 1; i <= 10; i++) {
                prtAssig.drpseqnumber.push(i);
            }


            for (var i = 1; i <= 100; i++) {
                prtAssig.drpquantity.push(i);
            }
            prtAssig.CardAssignList = ko.observableArray([]);
            prtAssig.drpportType = ko.observableArray(['']);
            prtAssig.CardAssignListUpdate = ko.observableArray(['']);
            prtAssig.drpconctrType = ko.observableArray(['']);
            prtAssig.offsetNumber = ko.observable();
            prtAssig.networkPortName = ko.observable();
            prtAssig.hasAssignablePort = ko.observable();
            prtAssig.hasAssignablePort = false;
            prtAssig.hasAssignPort = ko.observable();
            prtAssig.singleRecord = ko.observable(false);

            prtAssig.getConnectorDtls();
            prtAssig.getPortTypeDtls();

            prtAssig.selectedSequenceNum = ko.observable();
            prtAssig.selectedSequenceNumVal = ko.computed(function () {
                return prtAssig.selectedSequenceNum() && prtAssig.selectedSequenceNum();
            });

            prtAssig.selectedQuantity = ko.observable();
            prtAssig.selectedQuantityVal = ko.computed(function () {
                return prtAssig.selectedQuantity() && prtAssig.selectedQuantity();
            });

            prtAssig.selectedConnectorType = ko.observable();
            prtAssig.selectedPortType = ko.observable();
            prtAssig.actioncode = 'INSERT';

            if (specInfo == 'CARD') {
                prtAssig.cardName(results["cardName"]);
                prtAssig.nodeName(results["nodeName"]);

                prtAssig.nodeValue(results["nodeId"]);
                prtAssig.cardValue(results["cardId"]);
                prtAssig.getPortAssignmentDtls();
            }
        };

        var count = 0;

        // getting connector type details from database
        portAssign.prototype.getConnectorDtls = function () {
            var searchJSON = "";
            $.ajax({
                type: "GET",
                url: 'api/specn/GetConnectorTypes/',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: prtAssig
            });
            function successFunc(data, status) {

                if (data == 'no_results') {
                    app.showMessage("No Connector Types found.", 'Port Assignment');

                }
                else {
                    var results = JSON.parse(data);
                    prtAssig.drpconctrType(results);
                }
            }
            function errorFunc() {
                app.showMessage("error", 'Port Assignment');
            }

        };

        // getting port type details from database
        portAssign.prototype.getPortTypeDtls = function () {
            var searchJSON = "";
            $.ajax({
                type: "GET",
                url: 'api/specn/GetPortTypes/0',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFuncPort,
                error: errorFuncPort,
                context: prtAssig
            });
            function successFuncPort(data, status) {

                if (data == 'no_results') {
                    app.showMessage("No Port Types found.", 'Port Assignment');

                }
                else {
                    var results = JSON.parse(data);
                    prtAssig.drpportType(results);
                }
            }
            function errorFuncPort() {
                app.showMessage("error", 'Port Assignment');
            }

        };


        //Adding the data into grid/Table
        portAssign.prototype.addPortdtls = function (selected) {

            //  console.log("selectedId = " + selected.specn_id.value);
            if (prtAssig.cardValue() == "") {
                app.showMessage("Please enter the card id or name");
                return false;
            }
            else if (prtAssig.addvalidations() == false) {
                return false;
            }
            var portSeq = prtAssig.selectedSequenceNumVal();
            var portType = prtAssig.selectedPortType();
            var connType = prtAssig.selectedConnectorType();
            var portTypename = "";
            for (var i = 0; i < prtAssig.drpportType().length; i++) {
                if (portType === prtAssig.drpportType()[i].value) {
                    portTypename = prtAssig.drpportType()[i].text;
                    break;
                }
            }

            var conctrTypename = "";
            for (var i = 0; i < prtAssig.drpconctrType().length; i++) {
                if (connType === prtAssig.drpconctrType()[i].value) {
                    conctrTypename = prtAssig.drpconctrType()[i].text;
                    break;
                }
            }
            var portQty = prtAssig.selectedQuantityVal();
            var portNoOffset = $('#idtxtOffsetnum').val();
            var portName = $('#idtxtnetwnumber').val();
            var hasAssignedPort = prtAssig.hasAssignablePort;

            count = prtAssig.CardAssignList().length + 1;

            var jsonData;
            var json;
            var results;

            count = "Item" + count;
            jsonData = {
                defId: 0,
                portSeqNo: portSeq,
                portTypId: portType,
                portTypNm: portTypename,
                connectorTypId: connType,
                connectorTypNm: conctrTypename,
                portQty: portQty,
                portNoOffset: portNoOffset,
                portName: portName,
                hasAssignedPort: hasAssignedPort
            };

            json = ko.toJSON(jsonData);
            results = JSON.parse(json);
            prtAssig.CardAssignList.push(results);
            prtAssig.addBtnEnable(false);
            prtAssig.smBtnEnable(true);
            prtAssig.resetControls();
        };


        portAssign.prototype.removeCardAssign = function (selected) {
            prtAssig.actioncode = 'DELETE';

            prtAssig.CardAssignListUpdate([]);
            var jsonData = {
                defId: selected.defId,
                portSeqNo: selected.portSeqNo,
                portTypId: 0,
                portTypNm: selected.portTypNm,
                connectorTypId: 0,
                connectorTypNm: selected.connectorTypNm,
                portQty: selected.portQty,
                portNoOffset: selected.portNoOffset,
                portName: selected.portName,
                hasAssignedPort: selected.hasAssignedPort,
            };

            var json = ko.toJSON(jsonData);
            var results = JSON.parse(json);
            prtAssig.CardAssignListUpdate.push(results);

            var jsonUpdate = {
                cardId: prtAssig.cardValue(),
                actioncode: prtAssig.actioncode,
                portDtls: prtAssig.CardAssignListUpdate()
            };

            prtAssig.portassignTbl(jsonUpdate);
            var saveJSON = mapping.toJS(prtAssig.portassignTbl());
            $.ajax({
                type: "POST",
                url: 'api/specn/UpdateCardtoPortAssign/',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: deleteSuccess,
                error: deleteError
            });
            function deleteSuccess(response) {
                $("#interstitial").hide();
                prtAssig.CardAssignList.remove(selected);
                prtAssig.getPortAssignmentDtls();
                prtAssig.resetControls();
                app.showMessage("Deleted Successfully", 'Port Assignment');
            }
            function deleteError(err) {
                $("#interstitial").hide();
                app.showMessage('Error', 'Port Assignment');
            }
        };

        // Modify the card Assignments
        portAssign.prototype.modifyCardAssign = function (selected) {
            $('#btnsavePortdtls').html('MODIFY');
            prtAssig.actioncode = 'UPDATE';
            prtAssig.defId(selected.defId);
            prtAssig.selectedSequenceNum(selected.portSeqNo);
            prtAssig.selectedQuantity(selected.portQty);
            prtAssig.selectedPortType(selected.portTypId);
            prtAssig.selectedConnectorType(selected.connectorTypId);
            prtAssig.hasAssignablePort = selected.hasAssignedPort;
            $('#idtxtOffsetnum').val(selected.portNoOffset);
            $('#idtxtnetwnumber').val(selected.portName);
            prtAssig.addBtnEnable(false);
            prtAssig.smBtnEnable(true);
            prtAssig.singleRecord(true);
        };
        portAssign.prototype.getNodeName = function () {

            //$("#interstitial").css("height", "100%");
            //$("#interstitial").show();
            //var searchJSON = "";
            //var id = $('#idtxtnodename').val();
            //$.ajax({
            //    type: "GET",
            //    url: 'api/specnModel/getNodename/' + id,
            //    data: searchJSON,
            //    contentType: "application/json; charset=utf-8",
            //    dataType: "json",
            //    success: successFunc,
            //    error: errorFunc,
            //    context: prtAssig
            //});
            //function successFunc(data, status) {
            //    if (data == '') {
            //        $("#interstitial").hide();
            //        app.showMessage("Node name not found", 'Shelf Assignment');
            //        return false;
            //    }
            //    else {
            //        $("#interstitial").hide();
            //        $("#idtxtnodename").val(data);
            //        prtAssig.nodeName(data);
            //        return true;
            //    }
            //}
            //function errorFunc(data) {
            //    $("#interstitial").hide();
            //    app.showMessage("error : " + data, 'Shelf Assignment');
            //    return false;
            //}
        };
        portAssign.prototype.getcardName = function (id) {
            //var searchJSON = "";
            //$.ajax({
            //    type: "GET",
            //    url: 'api/specnModel/getCardname/' + id,
            //    data: searchJSON,
            //    contentType: "application/json; charset=utf-8",
            //    dataType: "json",
            //    success: successFunc,
            //    error: errorFunc,
            //    context: prtAssig
            //});
            //function successFunc(data, status) {
            //    if (data == '') {
            //        $("#interstitial").hide();
            //        app.showMessage("Card name not found", 'Shelf Assignment');
            //        return false;
            //    }
            //    else {
            //        $("#interstitial").hide();
            //        $("#idtxtcardname").val(data);
            //        prtAssig.cardName(data);
            //        return true;
            //    }
            //}
            //function errorFunc(data) {
            //    $("#interstitial").hide();
            //    app.showMessage("error : " + data, 'Shelf Assignment');
            //    return false;
            //}
        };
        //get the port assignment details for card
        portAssign.prototype.getPortAssignmentDtls = function (mtl) {


            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            $("#idShelfdetails").hide();
            prtAssig.CardAssignList([]);
            var cardId = prtAssig.cardValue();
            if (prtAssig.getcardName(cardId) == false) {
                return false;
            }

            $.ajax({
                type: "GET",
                url: 'api/specn/getCardtoPortAssign/' + cardId,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: prtAssig
            });
            function successFunc(data, status) {
                if (data == 'no_results' || data == "{}") {
                    $("#interstitial").hide();
                    $('#btnsavePortdtls').html('SAVE');
                    prtAssig.actioncode = 'INSERT';
                    app.showMessage("No records found.", 'Port Assignment');
                    prtAssig.addBtnEnable(true);
                    prtAssig.smBtnEnable(false);
                }
                else {
                    $('#btnsavePortdtls').html('MODIFY');
                    prtAssig.actioncode = 'UPDATE';
                    $("#interstitial").hide();
                    prtAssig.resetControls();
                    var results = JSON.parse(data);
                    prtAssig.CardAssignList(results);
                    prtAssig.addBtnEnable(true);
                    prtAssig.smBtnEnable(false);
                }
            }
            function errorFunc() {
                $("#interstitial").hide();
                app.showMessage("error", 'Shelf Assignment');
            }
        };
        //Button operation selectio
        portAssign.prototype.btnOperations = function (selected) {
            if (prtAssig.CardAssignList().length == 0) {
                app.showMessage("Please add some ports into table.. ");
                return false;
            }
            if ($('#btnsavePortdtls').html() == 'SAVE') {
                prtAssig.savePortdtls();
            }
            else if ($('#btnsavePortdtls').html() == 'MODIFY') {
                prtAssig.modifyportDtls();
            }
            prtAssig.addBtnEnable(true);
            prtAssig.smBtnEnable(true);
        };

        //Modifying the data into grid/Table
        portAssign.prototype.modifyportDtls = function (selected) {
            if (prtAssig.singleRecord() == true) {
                prtAssig.CardAssignListUpdate([]);
                var jsonData;
                var json;
                var results;

                jsonData = {
                    defId: prtAssig.defId,
                    portSeqNo: prtAssig.selectedSequenceNumVal(),
                    portTypId: prtAssig.selectedPortType(),
                    portTypNm: "",
                    connectorTypId: prtAssig.selectedConnectorType(),
                    connectorTypNm: "",
                    portQty: prtAssig.selectedQuantityVal(),
                    portNoOffset: $('#idtxtOffsetnum').val(),
                    portName: $('#idtxtnetwnumber').val(),
                    hasAssignedPort: prtAssig.hasAssignablePort
                };
                json = ko.toJSON(jsonData);
                results = JSON.parse(json);
                prtAssig.CardAssignListUpdate.push(results);


                if (prtAssig.CardAssignListUpdate().length == 0) {
                    app.showMessage("Records not selected for insert", 'Port Assignment');
                    return false;
                }

                var jsonUpdate = {
                    cardId: prtAssig.cardValue(),
                    actioncode: prtAssig.actioncode,
                    portDtls: prtAssig.CardAssignListUpdate()
                };
            }
            else if (prtAssig.singleRecord() == false) {
                var jsonUpdate = {
                    cardId: prtAssig.cardValue(),
                    actioncode: prtAssig.actioncode,
                    portDtls: prtAssig.CardAssignList()
                };
            }

            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            prtAssig.portassignTbl(jsonUpdate);
            var saveJSON = mapping.toJS(prtAssig.portassignTbl());

            $.ajax({
                type: "POST",
                url: 'api/specn/UpdateCardtoPortAssign/',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: insertSuccess,
                error: insertError
            });
            function insertSuccess(response) {
                $("#interstitial").hide();
                prtAssig.getPortAssignmentDtls();
                prtAssig.resetControls();
                app.showMessage("Updated Successfully", 'Port Assignment');
            }
            function insertError(err) {
                $("#interstitial").hide();
                app.showMessage('Error', 'Port Assignment');
            }


        };

        //Adding the data into grid/Table
        portAssign.prototype.savePortdtls = function () {

            if (prtAssig.CardAssignList().length == 0) {
                app.showMessage("Records not selected for insert", 'Port Assignment');
                return false;
            }
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            var jsonUpdate = {
                cardId: prtAssig.cardValue(),
                actioncode: prtAssig.actioncode,
                portDtls: prtAssig.CardAssignList()
            };
            prtAssig.portassignTbl(jsonUpdate);
            var saveJSON = mapping.toJS(prtAssig.portassignTbl());

            $.ajax({
                type: "POST",
                url: 'api/specn/UpdateCardtoPortAssign/',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: insertSuccess,
                error: insertError
            });
            function insertSuccess(response) {
                $("#interstitial").hide();
                app.showMessage("Inserted Successfully", 'Port Assignment');
                prtAssig.resetControls();
            }
            function insertError(err) {
                $("#interstitial").hide();
                app.showMessage('Error', 'Port Assignment');
            }
        };

        portAssign.prototype.onchangeHasAssignable = function (item) {

            if (prtAssig.hasAssignablePort == true || prtAssig.hasAssignablePort == 'Y') {
                prtAssig.hasAssignablePort = 'N';
                return false;
            }
            else {
                prtAssig.hasAssignablePort = 'Y';
                return true;
            }
        };

        portAssign.prototype.addvalidations = function () {
            if (prtAssig.selectedSequenceNumVal() == "") { app.showMessage('Please select the sequence number'); return false; }
            else if (prtAssig.selectedPortType() == "") { app.showMessage('Please select the port type'); return false; }
            else if (prtAssig.selectedConnectorType() == "") { app.showMessage('Please select the connector type'); return false; }
            else if (prtAssig.selectedQuantityVal() == "") { app.showMessage('Please select the quantity type'); return false; }
            else if ($('#idtxtOffsetnum').val() == "") { app.showMessage('Please enter the offset number'); return false; }
            else if ($('#idtxtnetwnumber').val() == "") { app.showMessage('Please enter the network name'); return false; }
            else if (prtAssig.hasAssignablePort == "") { app.showMessage('Please select has assignable port value'); return false; }
            else { return true; }
        };

        portAssign.prototype.resetControls = function () {
            prtAssig.selectedSequenceNum("");
            prtAssig.selectedPortType('');
            prtAssig.selectedConnectorType('');
            prtAssig.selectedQuantity("");
            $('#idtxtOffsetnum').val("");
            $('#idtxtnetwnumber').val("");
            prtAssig.hasAssignablePort = 'N';
        };
        return portAssign;
    });