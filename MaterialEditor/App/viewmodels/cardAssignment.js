define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system',
    './cardSpecification', './slotAssignment', './pluginAssociations', './portAssignment', 'durandal/app', 'datablescelledit', 'bootstrapJS', 'jquerydatatable', 'jqueryui', '../Utility/referenceDataHelper'],
    function (composition, ko, $, http, activator, mapping, system, cardSpec, slotSpecAssign, plgSpecAssoc, portAssisg, app, datablescelledit, bootstrapJS, jquerydatatable, jqueryui, reference) {
        var CardModel = function () {
            selfspec = this;

            selfspec.usr = require('Utility/user');
            selfspec.showModalHLPNMultiBool = ko.observable(true);
            selfspec.selfspecTbl = ko.observable();
            selfspec.cardSpecification = ko.observable();
            selfspec.slotAssignment = ko.observable();
            selfspec.pluginAssociations = ko.observable();
            selfspec.portAssignment = ko.observable();
            selfspec.node_value = ko.observable('');
            selfspec.node_name = ko.observable(''); // value need to populated accordingly and pass to the other pages 
            selfspec.sequence_value = ko.observable('');
            selfspec.nodeStartno = ko.observable();
            selfspec.backToMtlItmId = 0;
            selfspec.idCardList = ko.observable();
            selfspec.SpecList = ko.observableArray();
            selfspec.CardList = ko.observableArray();
            selfspec.CardAliasList = ko.observableArray();
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
            selfspec.typeSpecs = ko.observableArray(['', 'Record Only', 'Generic']);
            selfspec.availableStartno = ko.observableArray(['1', '0'])

            selfspec.existingSelectedIdCurrent = ko.observable();
            selfspec.existingSelectedspecTypeCurrent = ko.observable();

            selfspec.existedData = ko.observable(false);
            selfspec.actioncode = ko.observable('');
            selfspec.selectedCardId = ko.observable();
            selfspec.selectedCardId = "";
            selfspec.selectedCardName = ko.observable();
            selfspec.selectedCardName = "";
            selfspec.enableSaveButton = ko.observable(false);
            selfspec.enableUpdateButton = ko.observable(false);

        };

        CardModel.prototype.getNodeName = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            var searchJSON = "";
            var nodeid = $("#idNode").val();
            $.ajax({
                type: "GET",
                url: 'api/specnModel/getNodename/' + nodeid,
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

        CardModel.prototype.getCardAlias = function () {

            if (selfspec.node_value() == "") {
                $("#idNode").attr('placeholder', 'Please enter node name..');
                $("#idNode").focus().select();
                return false;
            }

            if ($("#btnCrdDisplayAlias").html() == "Hide DisplayAlias") {
                //alert('inside toggle func');
                $("#btnCrdDisplayAlias").html('DisplayAlias');
                $("#divCrdAliasdtls").hide();
            }
            else {

                var selectedId = selfspec.selectedCardId;
                //var specType = "SHELF"; // CARD

                var url = 'api/specn/getAlias/' + selectedId + '/' + selfspec.specType;
                //alert(url);

                var intHeight = $("body").height();
                $("#interstitial").css("height", intHeight);
                $("#interstitial").show()

                http.get(url).then(function (response) {
                    //console.log(response);
                    if (response === "{}") {
                        $("#interstitial").hide();
                        app.showMessage("No results found for Alias", 'Card Assignment').then(function () {
                            $("#divCrdAliasdtls").hide();
                            return;
                        });
                    } else {
                        $("#interstitial").hide();
                        $("#divCrdAliasdtls").show();
                        var results = JSON.parse(response);
                        selfspec.CardAliasList(results);
                        $("#btnCrdDisplayAlias").html('Hide DisplayAlias');
                    }
                });
            }


        };

        CardModel.prototype.addCard = function (data) {
            var span = document.getElementById('idCardclose');
            var modal = document.getElementById('idSpecsearchlist');
            $("#idSpecsearchlist").show();
            modal.style.display = "block";
            selfspec.SpecList([]);

            span.onclick = function () {
                modal.style.display = "none";
                selfspec.SpecList([]);
            }
            $("#idProductID").val('');
            $("#idName").val('');
            $("#idDesc").val('');
            $('#idType').val('');
        };

        //popup for Adding Card specification..        
        CardModel.prototype.Searchspec = function (data) {

            //if (selfspec.node_value() == "") {
            //    //document.getElementById("idNode").focus();
            //    //app.showMessage("Please enter node name..");
            //    $("#idNode").focus().select();
            //    $("#idNode").attr('placeholder', 'Please enter node name..');
            //    return;
            //}

            var intHeight = $("body").height();
            $("#interstitial").css("height", intHeight);
            $("#interstitial").show();


            var prodid = $("#idProductID").val();
            var Name = $("#idName").val();
            var Description = $("#idDesc").val();
            var Status = $('#idstatuspec').find("option:selected").val();
            var spectype = 'CARD';
            var SpecClass = $('#idType').val();

            if (selfspec.existedData() == false) {
                selfspec.enableSaveButton(true);
                selfspec.enableUpdateButton(false);
            }
            var Card_table = $('#idSpecList').DataTable();

            var searchJSON = {
                id: prodid, name: Name, desc: Description, status: Status, specnType: spectype, class: SpecClass
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
                    Card_table.clear();
                    selfspec.SpecList([]);
                    app.showMessage("No records found...", 'Card Assignment');
                }
                else {
                    Card_table.clear();
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
        };


        CardModel.prototype.specificationSelected = function (selectedId, specType, stl, mtlItmId) {

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
                    $("#idCarddetails").show();
                    // Adding this json format for identifying the screen flow 
                    // through specification/assignment module
                    var jsonres = {
                        resp: response,
                        specification: false
                    }
                    stl.cardSpecification(new cardSpec(jsonres));
                }
            });
        };

        CardModel.prototype.specificationSelectCall = function (selected) {
            console.log("selectedId = " + selected.specn_id.value);
            var selectedId = selected.specn_id.value;
            var specType = selected.enumSpecTyp.value;

            count = selfspec.CardList().length + 1;

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
                selfspec.CardStartno();
            }
            count = "Item" + count;
            jsonData = {
                specn_shlvsdef_id: count,
                node_Id: selfspec.node_value(),
                specn_id: selected.specn_id.value,
                Card_nm: selected.specn_nm.value,
                seq_num: '',
                Card_qty: '',
                Card_no_offset: '',
                extra_shelves: false,
            };

            json = ko.toJSON(jsonData);
            results = JSON.parse(json);
            selfspec.CardList.push(results);
        };

        CardModel.prototype.selectedCard = function (selected) {
            $("#interstitial").css("height", "100%");
            $("#idCarddetails").show();
            //var specId = selected.specn_id;
            var specId = selected.specn_id.value;
            selfspec.specType = "CARD";
            // var specType = "CARD";
            selfspec.selectedCardId = specId;
            selfspec.selectedCardName = selected.specn_nm.value;
            //alert(selfspec.selectedCardName);
            selfspec.backToMtlItmId = 0;
            selfspec.specificationSelected(specId, selfspec.specType, selfspec, 0);

            $("#idSpecsearchlist").hide();
            selfspec.SpecList([]);
            $("#idProductID").val('');
            $("#idName").val('');
            $("#idDesc").val('');
            $('#idType').val('');
        };


        //End of popup display

        CardModel.prototype.reset = function () {
            selfspec.SpecList([]);
            selfspec.CardList([]);
            selfspec.node_value('');
            selfspec.slotAssignment(null);
            selfspec.cardSpecification(null);
            selfspec.pluginAssociations(null);
            selfspec.portAssignment(null);
        };


        CardModel.prototype.getSlotdtls = function () {
            selfspec.cardSpecification(false);
            selfspec.pluginAssociations(false);
            selfspec.portAssignment(false);
            console.log("Card Id selectd : " + selfspec.selectedCardId);
            if (selfspec.node_value() == "") {
                app.showMessage('Please enter node id..');
                return false;
            }
            if (selfspec.selectedCardId == "") {
                app.showMessage('Please select card from search button..');
                return false;
            }

            $("#idCarddetails").hide();
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            // var selectedId = 89;
            // var specType = 'CARD';
            //alert('selected Card Name :'+ selfspec.selectedCardName);

            var request = {
                nodeId: selfspec.node_value(),
                nodeName: selfspec.node_value(),
                cardId: selfspec.selectedCardId,
                cardName: selfspec.selectedCardName,
                height: selfcard.selectedCardSpec().Card.list[0].Hght.value,
                width: selfcard.selectedCardSpec().Card.list[0].Wdth.value,
                depth: selfcard.selectedCardSpec().Card.list[0].Dpth.value,
                uom: selfcard.selectedCardSpec().Card.list[0].DimUom.value,
                specType: 'CARD'
            };

            var requestJson = ko.toJSON(request);
            $("#interstitial").hide();
            $("#idCarddetails").show();
            selfspec.slotAssignment(new slotSpecAssign(requestJson));
        };

        CardModel.prototype.scrollToSpecDetails = function () {
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

        CardModel.prototype.getPlugindtls = function () {
            selfspec.cardSpecification(false);
            selfspec.slotAssignment(false);
            selfspec.portAssignment(false);
            console.log("Card Id selectd : " + selfspec.selectedCardId);

            if (selfspec.selectedCardId == "") {
                app.showMessage('Please select card from search button..');
                return false;
            }

            $("#idCarddetails").hide();
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            //alert('selected Card Name :'+ selfspec.selectedCardName);

            var request = {
                nodeId: selfspec.node_value(),
                nodeName: selfspec.node_name(),
                cardId: selfspec.selectedCardId,
                cardName: selfspec.selectedCardName,
                specType: 'CARD'
            };
            var requestJson = ko.toJSON(request);
            $("#interstitial").hide();
            $("#idCarddetails").show();
            selfspec.pluginAssociations(new plgSpecAssoc(requestJson));
        };

        CardModel.prototype.getPortdtls = function () {
            selfspec.cardSpecification(false);
            selfspec.slotAssignment(false);
            selfspec.pluginAssociations(false);

            console.log("Card Id selectd : " + selfspec.selectedCardId);

            if (selfspec.selectedCardId == "") {
                app.showMessage('Please select card from search button..');
                return false;
            }

            $("#idCarddetails").hide();
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            //alert('selected Card Name :'+ selfspec.selectedCardName);

            var request = {
                nodeId: selfspec.node_value(),
                nodeName: selfspec.node_name(),
                cardId: selfspec.selectedCardId,
                cardName: selfspec.selectedCardName,
                specType: 'CARD'
            };
            var requestJson = ko.toJSON(request);
            $("#interstitial").hide();
            $("#idCarddetails").show();
            selfspec.portAssignment(new portAssisg(requestJson));
        };

        return CardModel;
    });