define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', './slotSpecification'],
    function (composition,app, ko, $, http, activator, mapping, system, slotSpec1) {
    var slotAssignment = function (data) {
        slotAssig = this;
        var results = JSON.parse(data);
        slotAssig.shelforCardName = ko.observable();
        slotAssig.specType = ko.observable();
        slotAssig.specTypeHTML = ko.observable();
        

        //alert(results);
        var specInfo = results["specType"];
        var specInfoHTML = ""; // this varaiable is only for HTML display, dynamically for Shelf Name/Card Name 
        //alert(specInfo);
        if (specInfo == 'CARD') {
            slotAssig.slotID = ""; // added of validation check.
            slotAssig.cardID = results["cardId"];
            slotAssig.cardName = results["cardName"];
            slotAssig.nodeName = results["nodeName"];
            slotAssig.nodeID = results["nodeId"];
            $('#lblShCrdName').html('Card Name');
            specInfoHTML = "Card";
            slotAssig.selectedCardheight = ko.observable();
            slotAssig.selectedCardwidth = ko.observable();
            slotAssig.selectedCarddepth = ko.observable();
            slotAssig.selectedCarduom = ko.observable();

            // added for storing selected slot details.
            slotAssig.selectedSlotwidth = ko.observable();
            slotAssig.selectedSlotheight = ko.observable();
            slotAssig.selectedSlotdepth = ko.observable();
            slotAssig.selectedSlotuom = ko.observable();

            slotAssig.selectedCardheight(results["height"]);
            slotAssig.selectedCardwidth(results["width"]);
            slotAssig.selectedCarddepth(results["depth"]);
            slotAssig.selectedCarduom(results["uom"]);
            // alert(slotAssig.selectedCardheight());
            slotAssig.shelforCardName(slotAssig.cardName);
            slotAssig.specType(specInfo);
            slotAssig.specTypeHTML(specInfoHTML);
        }
        else {
            //alert("inside Shelf section");
            slotAssig.shelfID = results["shelfId"];
            //alert(slotAssig.shelfID);
            slotAssig.shelfName = results["shelfName"];
            slotAssig.nodeName = results["nodeName"];
            slotAssig.nodeID = results["nodeId"];
            specInfoHTML = "Shelf";
            slotAssig.shelforCardName(slotAssig.shelfName);
            //specInfo = "SLOT";
            slotAssig.specType(specInfo);
            slotAssig.specTypeHTML(specInfoHTML);
        }

        // alert('inside slotAssignment');
        slotAssig.selectedSlotSpec = ko.observable();
        slotAssig.completedNotSelected = ko.observable(true);
        slotAssig.slotSpecification = ko.observable();
        $('#idSlotAssignmentdetails').show();
        //added for testing by rxjohn
        slotAssig.specName = ko.observable();
        slotAssig.specName(slotAssig.nodeName);
        // slotAssig.shelforCardName(slotAssig.shelfName);
        slotAssig.slotAliasList = ko.observableArray();
        slotAssig.slotAssignList = ko.observableArray();
        slotAssig.cardCompartment = ko.observableArray();
        // added for insert, update and delete functionality.
        selfspec.slotAssignTbl = ko.observable();
        slotAssig.SlotSpecList = ko.observableArray();
        slotAssig.slotStartno = ko.observable();
        slotAssig.actionCode = ko.observable();
        slotAssig.shelfSpecId = ko.observable();
       
        //alert(slotAssig.specType());
        slotAssig.getSlotsAssigDtls();
        // added for qualified slots functionality
        slotAssig.qualifiedSlotsforCard = ko.observableArray();
        // added for storing Card Position values
        slotAssig.cardPositionTypes = ko.observableArray();
        //slotAssig.selectedSlotId = ko.observable();
        slotAssig.selectedSlotId = '';
    };

        
    /* added by roshan for slot Alias Display */
    slotAssignment.prototype.getSlotAlias = function () {
               
        if (slotAssig.selectedSlotId == '') {
            app.showMessage("Please select a slot for Alias details", 'Slot Assignment').then(function () {
                return;
            });
            return false;
        }

        if ($("#btnSltDisplayAlias").html() == "Hide DisplayAlias")
        {
            //alert('inside toggle func');
            $("#btnSltDisplayAlias").html('DisplayAlias');
            slotAssig.slotAliasList(null);
            $("#divSltAliasdtls").hide();
        }
        else
        {
            // this ID can be slot ID or Card accordingly to the screen flow.
            var selectedSCId;
            var specType;

            if (slotAssig.specType() == 'SHELF')
            {
                selectedSCId = slotAssig.selectedSlotId;
                specType = 'SLOT'; // we need to query API method with 'SLOT' for getting slot alias details.
            }
            else if (slotAssig.specType() == 'CARD')
            {
                selectedSCId = slotAssig.cardID;
                specType = slotAssig.specType();
            }
                        
            //alert(specType);
            //alert(selectedSCId);
            var url = 'api/specn/getAlias/' + selectedSCId + '/' + specType;

            alert(url, 'inside getSlotAlias');

            http.get(url).then(function (response) {
                    console.log(response);
                    if (response === "{}") {
                        $("#interstitial").hide();
                        app.showMessage("No results found for Alias").then(function () {
                            $("#divSltAliasdtls").hide();
                            return;
                        });
                    } else {
                        $("#divSltAliasdtls").show();
                        // var jsonObj = ko.toJSON(response);
                        var results = JSON.parse(response);
                        alert(results);
                        slotAssig.slotAliasList(results);
                        $("#btnSltDisplayAlias").html('Hide DisplayAlias');

                    }
           });

        }
    }

     /* added by roshan for showing slot Assignment details to shelf */
    slotAssignment.prototype.getSlotsAssigDtls = function () {

        /* var selectedId = "253";
        var newselected = slotAssig.nodeID; */

        // this ID can be shelf ID or Card accordingly the screen flow.
        var selectedSCId;
        var specType;
        //alert('inside getSlotsAssigDtls func');
        //alert(slotAssig.specType());
        //alert(slotAssig.shelfID);

        if (slotAssig.specType() == 'SHELF') {
            selectedSCId = slotAssig.shelfID;

            //alert(selectedSCId);

            var url = 'api/specn/getSlotAssign/' + selectedSCId;

            //alert(url, 'inside getSlotsAssigDtls');

            $("#divSlotAssignDtls").show();

            http.get(url).then(function (response) {
                console.log(response);
                if (response === "{}") {
                    $("#interstitial").hide();
                    app.showMessage("No results found").then(function () {
                        return;
                    });
                } else {

                    var results = JSON.parse(response);
                    //alert(results);
                    slotAssig.slotAssignList(results);

                }
            });
            
        }
        else if (slotAssig.specType() == 'CARD') {
            selectedSCId = slotAssig.cardID;
            //alert('Inside getSlotsAssigDtls for Card');
            //alert(selectedSCId);

            var url = 'api/specn/getCardtoSlotAssign/' + selectedSCId;

            //alert(url, 'inside getSlotsAssigDtls');

            $("#divSlotAssignDtls").show();

            http.get(url).then(function (response) {
                console.log(response);
                if (response === "{}") {
                    $("#interstitial").hide();
                    app.showMessage("No results found").then(function () {
                        return;
                    });
                } else {

                    var results = JSON.parse(response);
                    //alert(results);
                    slotAssig.slotAssignList(results);

                }
            });
            
        }
        
        

      
    }

    /* added by roshan for updating slot Assignment details to DB */
    slotAssignment.prototype.updateSlotAssign = function () {
        // rxjohn: validate whether all required text box is not emtpy 
        if (slotAssig.ValidateforReq() != false) {

            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            alert('Inside updateSlotAssign');
            var jsonUpdate;
            //alert(slotAssig.specType());
            if (slotAssig.specType() == 'SHELF') {
                slotAssig.actionCode('UPDATE');
                jsonUpdate = {
                    actioncode: slotAssig.actionCode,
                    shelfId: slotAssig.shelfID,
                    slotDtls: slotAssig.slotAssignList()
                };
                //alert(jsonUpdate);
                //slotAssig.slotAssignTbl(jsonUpdate);
                var saveJSON = mapping.toJS(jsonUpdate);


                $.ajax({
                    type: "POST",
                    url: 'api/specn/updateSlotAssign/',
                    data: JSON.stringify(saveJSON),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: updateSuccess,
                    error: updateError
                });
            }
            else if (slotAssig.specType() == 'CARD') {
                slotAssig.actionCode('UPDATE');
                jsonUpdate = {
                    actioncode: slotAssig.actionCode,
                    cardId: slotAssig.cardID,
                    slotDtls: slotAssig.slotAssignList()
                };

                var saveJSON = mapping.toJS(jsonUpdate);

                $.ajax({
                    type: "POST",
                    url: 'api/specn/UpdateCardtoSlotAssign/',
                    data: JSON.stringify(saveJSON),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: updateSuccess,
                    error: updateError
                });
            }

            function updateSuccess(response) {
                $("#interstitial").hide();
                // slotAssig.reset();
                //alert(response.value);
                app.showMessage(response);


            }
            function updateError(err) {
                $("#interstitial").hide();
                app.showMessage('Error');
            }
            // reload the slot details to collection
            slotAssignment.prototype.getSlotsAssigDtls();
        }
    };
    
    // added for selected Slot Specification display.
    slotAssignment.prototype.selectedSlot = function (select) {
        $("#btnSltDisplayAlias").html('DisplayAlias');
        slotAssig.slotAliasList(null);
        $("#divSltAliasdtls").hide();

        // alert('inside selectedSlot');
        var specType = 'SLOT'; // specType is given as "SLOT" for getting spec details of selected slot.
        var selectedId = select.slotSpecID;
        slotAssig.selectedSlotId = selectedId;
              
        var url = 'api/specn/' + selectedId + '/' + specType;
        //alert(url);
        //if (selectedId == null) {

        $("#interstitial").show();
        http.get(url).then(function (response) {
            if (response === "{}") {
                $("#interstitial").hide();
            }
            else {
                $("#interstitial").hide();
                $("#idSlotAssignmentdetails").show();
                //slotAssig.shelfSpecification(new shelfSpec(response));
                //alert(response);
                var jsonres = {
                    resp: response,
                    specification: false
                }
                slotAssig.slotSpecification(new slotSpec1(jsonres));
            }
        });
        /*}
        else
        { 
            $("#interstitial").hide();
            $("#idSlotAssignmentdetails").show();
            //slotAssig.shelfSpecification(new shelfSpec(response));
            alert(resp)
            slotAssig.slotSpecification(new slotSpec1(response));
        } */
    };

    slotAssignment.prototype.creatNewSlot = function () {
        //alert('inside creatNewSlot');
        var modal = document.getElementById('idQualifiedSlots');
        var specType = 'SLOT';
        var selectedId;
        selectedId = 0;
        //alert(selectedId)
        var url = 'api/specn/' + selectedId + '/' + specType;

        $("#interstitial").show();
        http.get(url).then(function (response) {
            if (response === "{}") {
                $("#interstitial").hide();
            }
            else {
                $("#interstitial").hide();
                $("#idSlotAssignmentdetails").show();
                var value = 0;
                // this varaible is used for converting Milli meter (1mm) to Inch value, Card use to have value in Inches
                var millimetreToInch = 0.0; 
                if (slotAssig.selectedCarduom() == 22)
                {
                    millimetreToInch = 0.039;
                }
                //alert(response);
                // in the below code we are including a key to inform that, it is not coming through specification. 
                var jsonres = {
                    resp: response,
                    specification: false
                }
                // make the required changes for response object to populate card Height, Width and Depth values
                
                slotAssig.slotSpecification(new slotSpec1(jsonres));
                if (slotAssig.slotSpecification().selectedSlotSpec().Dpth.value == 0) {
                    //alert('if condition');
                    value = parseFloat(slotAssig.selectedCarddepth()) + millimetreToInch;
                    slotAssig.slotSpecification().selectedSlotSpec().Dpth.value = value.toFixed(3);
                }
                if (slotAssig.slotSpecification().selectedSlotSpec().Wdth.value == 0) {
                    value = parseFloat(slotAssig.selectedCardwidth()) + millimetreToInch;
                    slotAssig.slotSpecification().selectedSlotSpec().Wdth.value = value.toFixed(3);
                }
                if (slotAssig.slotSpecification().selectedSlotSpec().Hght.value == 0) {
                    value = parseFloat(slotAssig.selectedCardheight()) + millimetreToInch;
                    slotAssig.slotSpecification().selectedSlotSpec().Hght.value = value.toFixed(3);
                }
                modal.style.display = "none";
            }
        });

    };

    // This function to delete slot assignment to shelf.
    slotAssignment.prototype.removeShelfSlot = function (selected) {
        //alert(selected.defId);
        var action = 'DELETE';
        var slotdefId = 0;
        var url;


        if (slotAssig.specType() == 'SHELF') {

            if (selected.defId.indexOf("NewItem") != -1)
                slotdefId = 0;
            else
                slotdefId = parseInt(selected.defId);

            if (slotdefId != 0) {
                // if slotdefId greater than zero, then only we need to invoke controller method.

                url = 'api/specn/deleteShelfSlotAssign/' + slotdefId + '/' + action;
                //alert(url);

                http.get(url).then(function (response) {
                    if (response === "{}") {
                        //alert(response);
                        $("#interstitial").hide();
                    }
                    else {
                        //alert(response);
                        $("#interstitial").hide();
                        $("#idSlotAssignmentdetails").show();
                        slotAssig.slotAssignList.remove(selected);
                        slotAssig.calculateSequence();
                        app.showMessage(response);
                    }
                });
            }
            else
            {
                // if slotdefId is zero, then we don't have that record exist in DB table, just remove from slotAssignList object in client
                slotAssig.slotAssignList.remove(selected);
                slotAssig.calculateSequence();
                app.showMessage("Delete Successful");
            }

        }
        else if (slotAssig.specType() == 'CARD') {

            if (selected.defId.indexOf("NewItem") != -1)
                slotdefId = 0;
            else
                slotdefId = parseInt(selected.defId);

            //alert('inside delete for card');
            
            if (slotdefId != 0) {

                url = 'api/specn/deleteCardSlotAssign/' + slotdefId + '/' + action;
                //alert(url);

                http.get(url).then(function (response) {
                    if (response === "{}") {
                        //alert(response);
                        $("#interstitial").hide();
                    }
                    else {
                        //alert(response);
                        $("#interstitial").hide();
                        $("#idSlotAssignmentdetails").show();
                        slotAssig.slotAssignList.remove(selected);
                        slotAssig.calculateSequence();
                        app.showMessage(response);
                    }
                });
            }
            else {
                // if slotdefId is zero, then we don't have that record exist in DB table, just remove from slotAssignList object in client
                slotAssig.slotAssignList.remove(selected);
                slotAssig.calculateSequence();
                app.showMessage("Delete Successful");
            }
        }
    };


    slotAssignment.prototype.calculateAll = function (item) {
        slotAssig.slotSpecification(false);
        var selectId = item.defId;
        var indexid = parseInt(0);
        var shlfQty = item.seqQty;

        for (var i = 0; i < slotAssig.slotAssignList().length; i++) {
            if (selectId == slotAssig.slotAssignList()[i].defId) {
                indexid = i;
                break;
            }
        }
        // alert(selectId);
        var resval = 0;
        var seqVal = parseInt(slotAssig.slotAssignList()[indexid].seqNo);
        if (parseInt(indexid) != parseInt(slotAssig.slotAssignList().length - 1)) {
            resval = parseInt(shlfQty) + parseInt(seqVal);
            
            slotAssig.slotAssignList()[parseInt(indexid) + 1].seqNo = parseInt(resval);
            // slotAssig.slotAssignList()[0].seqNo = parseInt(1); commented by rxjohn
            slotAssig.slotAssignListTmp = ko.observableArray();
            slotAssig.slotAssignListTmp(slotAssig.slotAssignList());
            slotAssig.slotAssignList([]);
            slotAssig.slotAssignList(slotAssig.slotAssignListTmp());
        }
        return true;
    };

    slotAssignment.prototype.calculateSequence = function () {
        slotAssig.slotAssignList()[0].seqNo = 1;
        for (var i = 0; i < slotAssig.slotAssignList().length-1; i++) {
            slotAssig.calculateAll(slotAssig.slotAssignList()[i]);
        }

        return true;
    };
    
     // added for pop up screen to select slots in slot assignment screen
    slotAssignment.prototype.specificationSelectCall = function (selected) {
        console.log("selectedId = " + selected.specn_id.value);
        var selectedId = selected.specn_id.value;
        var specType = selected.enumSpecTyp.value;
        var count = slotAssig.slotAssignList().length + 1;

        //slotAssig.specType(specType);
        var jsonData;
        var json;
        var results;
        $("#idSlotAssignDtls").show();
        $("#divSlotAssignDtls").show();
        count = "NewItem" + count; //get unique id for each row.
        jsonData = {
            defId: count,
            slotSpecID: selected.specn_id.value,
            slotSpecNm: selected.specn_nm.value,
            seqNo: '',
            seqQty: ''
        };

        json = ko.toJSON(jsonData);
        results = JSON.parse(json);
        slotAssig.slotAssignList.push(results);
    };
    
    //popup for Adding shelf specification..        
    slotAssignment.prototype.addSlot = function (data) {

        $("#idSlotsearchlist").hide();
        var intHeight = $("body").height();
        $("#interstitial").css("height", intHeight);
        $("#interstitial").show();
        var span = document.getElementById('idSlotclose');
        var modal = document.getElementById('idSlotsearchlist');
        var nodeid = "%";
        var Name = "%";
        var Description = "%";
        var Status = "%";
        var spectype='SLOT';
        var SpecClass = "%";
               
               
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
                app.showMessage("No records found...", 'Failed');
            }
            else {
                //alert(data);
                $("#idSlotsearchlist").show();
                modal.style.display = "block";
                slotAssig.SlotSpecList([]);
               
                //var shelf_table = $('#idSpecList').DataTable();
                //shelf_table.clear();
                var results = JSON.parse(data);
                slotAssig.SlotSpecList(results);
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
            slotAssig.SlotSpecList([]);
        }
    };
    
        //popup for selecting qualified slots for selected card        
    slotAssignment.prototype.addQualifiedSlot = function (data) {
        var depth = 0;
        var height = 0;
        var width = 0;

        // added for populating card position to combo box.
        $.ajax({
            type: "GET",
            url: 'api/specn/GetCardPositionTypes/',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: successForCardPos,
            error: errorCardPos,
            context: selfspec
        });

        function successForCardPos(data, status) {

            //alert('success for GetCardPositionTypes method');
            if (data == 'no_results') {
                $("#interstitial").hide();
                app.showMessage("No records found...", 'Failed');
            }
            else {
                //alert(data);
                slotAssig.cardPositionTypes([]);

                //var shelf_table = $('#idSpecList').DataTable();
                //shelf_table.clear();
                var results = JSON.parse(data);
                //alert('after card Position loading');
                slotAssig.cardPositionTypes(results);

            }
        }

        function errorCardPos() {
            $("#interstitial").hide();
            alert('error on GetCardPositionTypes');
        }

        //alert('inside addQualifiedSlot');
        $("#idSlotsearchlist").hide();
        var intHeight = $("body").height();
        $("#interstitial").css("height", intHeight);
        $("#interstitial").show();
        var span = document.getElementById('idQualifiedSlotsclose');
        var modal = document.getElementById('idQualifiedSlots');
        var searchJSON = {};
        depth = slotAssig.selectedCarddepth();
        height = slotAssig.selectedCardheight();
        width = slotAssig.selectedCardwidth();

        $.ajax({
            type: "GET",
            url: 'api/specn/getQualifiedSlots/' + depth + '/' + height + '/' + width + '/',
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
                app.showMessage("No records found...", 'Failed');
            }
            else {
                //alert(data);
                $("#idQualifiedSlots").show();
                modal.style.display = "block";
                slotAssig.qualifiedSlotsforCard([]);

                //var shelf_table = $('#idSpecList').DataTable();
                //shelf_table.clear();
                var results = JSON.parse(data);
                slotAssig.qualifiedSlotsforCard(results);
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
            slotAssig.qualifiedSlotsforCard([]);
        }
        
    };
    
    /* for updating card-qualified slot Assignment details to DB */
    slotAssignment.prototype.addCard = function () {
        //alert('inside addCard');
        if (slotAssig.slotID == "") {
            app.showMessage('Please selected a Slot for adding');
            return;
        }
        $("#interstitial").css("height", "100%");
        $("#interstitial").show();
        var modal = document.getElementById('idQualifiedSlots');

        //alert('Inside updateSlotAssign');
        var jsonUpdate;
        //alert(slotAssig.specType());
        
        
        //alert(slotAssig.cardID);
        //alert(slotAssig.slotID);
        jsonUpdate = {
            actioncode: 'INSERT',
            cardId: slotAssig.cardID,
            slotId: slotAssig.slotID,
            cardStartPos: slotAssig.cardPos,
            cardheight: slotAssig.selectedCardheight(),
            cardDepth: slotAssig.selectedCardwidth(),
            cardWidth: slotAssig.selectedCarddepth()
        };
        //alert(jsonUpdate);
        //slotAssig.slotAssignTbl(jsonUpdate);
        var saveJSON = mapping.toJS(jsonUpdate);


        $.ajax({
            type: "POST",
            url: 'api/specn/UpdateCardtoQualifiedSlotAssign/',
            data: JSON.stringify(saveJSON),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: updateSuccess,
            error: updateError
        });

        function updateSuccess(response) {
            $("#interstitial").hide();
            // slotAssig.reset();
            //alert(response.value);
            app.showMessage(response);
            modal.style.display = "none";
        }

        function updateError(err) {
            $("#interstitial").hide();
            app.showMessage('Error');
        }
    };

    slotAssignment.prototype.selectSlotSpecification = function (model, event) {
        //alert(event.target.value);
        slotAssig.slotID = event.target.value;
        for (var i = 0; i < slotAssig.qualifiedSlotsforCard().length; i++) {
            if (slotAssig.qualifiedSlotsforCard()[i].slotSpecID == event.target.value) {
                //alert('inside matched slot' + slotAssig.qualifiedSlotsforCard()[i].slotSpecID + '=' + event.target.value);
                slotAssig.selectedSlotwidth(slotAssig.qualifiedSlotsforCard()[i].Wdth);
                slotAssig.selectedSlotheight(slotAssig.qualifiedSlotsforCard()[i].Hght);
                slotAssig.selectedSlotdepth(slotAssig.qualifiedSlotsforCard()[i].Dpth);
                slotAssig.selectedSlotuom(slotAssig.qualifiedSlotsforCard()[i].DimUom);
                break;
            }
        }
    };

    // added for card position combox slection changes.
    slotAssignment.prototype.selectCardPosition = function (model, event) {
        //alert(event.target.value);
        slotAssig.cardPos = event.target.value;
    };
    
    slotAssignment.prototype.updateOnSuccess = function () {
        //selfspec.Searchspec();
        //selfspec.specificationSelected(selfspec.existingSelectedIdCurrent(), selfspec.existingSelectedspecTypeCurrent(), selfspec, 0);
        alert('updated success from Slot Assignment');
    };

    slotAssignment.prototype.ValidateforReq = function () {
        //alert('inside ValidateforReq');
        var count = 0;
        $("#idSlotAssignDtls.table.table-striped input").each(function () {
            //alert($.trim($(this).val()));
            if ($.trim($(this).val()) == '') {
                //alert(this.value);
                count++;
                //alert('about change css');
                $(this).css({
                    "border": "1px solid red",
                    "background": "#FFCECE"
                });
                
            }
            
        });

        if (count > 0)
        {
            app.showMessage("Please input missing fields");
            return false;
        }
        else
        {
           return true;
        }
        
    };

    return slotAssignment;
});