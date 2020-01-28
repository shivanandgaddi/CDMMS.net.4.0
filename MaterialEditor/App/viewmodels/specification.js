define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system',
    './nodeSpecification', './baySpecification', './bayInternalSpecification', './pluginSpecification', './shelfSpecification', './slotSpecification', './cardSpecification', './bayExtenderSpecification', './portSpecification', 'jquerydatatable', 'jqueryui', 'datablescelledit',
    '../Utility/referenceDataHelper'],
    function (composition, app, ko, $, http, activator, mapping, system, nodeSpec, baySpec, bayInternalSpec, pluginSpec, shelfSpec, slotSpec, cardSpec, bayExtenderSpec, portSpec, jquerydatatable, jqueryui, datablescelledit, reference) {
        var Specification = function () {
            selfspec = this;
            selfspec.usr = require('Utility/user');
            selfspec.isRedirect = false;
            selfspec.backToMtlItmId = 0;

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
            selfspec.specificationTypes = ko.observableArray([{ value: '', text: '' }, { value: 'BAY', text: 'Bay' }, { value: 'BAY_EXTENDER', text: 'Bay Extender' }, { value: 'BAY_INTERNAL', text: 'Bay Internal' }, { value: 'CARD', text: 'Card' },
            { value: 'NODE', text: 'Node' }, { value: 'PLUG_IN', text: 'Plug-In' }, { value: 'PORT', text: 'Port' }, { value: 'SHELF', text: 'Shelf' }, { value: 'SLOT', text: 'Slot' }]);

            selfspec.newspecificationTypes = ko.observableArray([{ value: '', text: '' }, { value: 'BAY', text: 'Bay' }, { value: 'BAY_EXTENDER', text: 'Bay Extender' }, { value: 'BAY_INTERNAL', text: 'Bay Internal' }, { value: 'CARD', text: 'Card' },
            { value: 'PLUG_IN', text: 'Plug-In' }, { value: 'PORT', text: 'Port' }, { value: 'SHELF', text: 'Shelf' }, { value: 'SLOT', text: 'Slot' }]);

            selfspec.SpecList = ko.observableArray();
            selfspec.baySpecification = ko.observable();
            selfspec.bayExtenderSpecification = ko.observable();
            selfspec.nodeSpecification = ko.observable();
            selfspec.bayInternalSpecification = ko.observable();
            selfspec.pluginSpecification = ko.observable();
            selfspec.shelfSpecification = ko.observable();
            selfspec.slotSpecification = ko.observable();
            selfspec.cardSpecification = ko.observable();
            selfspec.portSpecification = ko.observable();
            selfspec.existingSelectedIdCurrent = ko.observable();
            selfspec.existingSelectedspecTypeCurrent = ko.observable();

            $(document).ready(function () {
                $(window).scroll(function () {
                    if ($(this).scrollTop() > 100) {
                        $('.scrollup').fadeIn();
                    } else {
                        $('.scrollup').fadeOut();
                    }
                });
                $('.scrollup').click(function () {
                    $("html, body").animate({
                        scrollTop: 0
                    }, 600);
                    return false;
                });

                selfspec.attemptAutoSeach();
            });
        };

        Specification.prototype.attemptAutoSeach = function () {
            selfspec.autoSearchId = null;

            var showSearchForm = function () {
                $("#navbarContainer").show();
                $("#navigareRecordHide").show();
                $(".viewContainerPadding").css('padding-top', 150);

                // 
                // show search form and results form here...
                // 
                $("#PANEL_EXISTING_NEW_BUTTONS").show();
                $("#PANEL_SEARCH").show();
                $("#idSpecsearchlist").show();
                $("html, body").animate({ scrollTop: 0 }, 0);

                window.document.title = "Specifications | Material Editor";
                $(this).hide();
            };

            if (window.location.hash.indexOf('?') >= 0) {
                var parms = new URLSearchParams(window.location.hash.split('?')[1]);
                selfspec.autoSearchId = parms.get('id');
                console.log(selfspec.autoSearchId);
            }

            if (!selfspec.autoSearchId) {
                return;
            }

            $("#navbarContainer").hide();
            $("#navigareRecordHide").hide();
            $(".viewContainerPadding").css('padding-top', 10);

            var timer = null;
            var setupRestoreSearchButton = function () {
                selfspec.$btnRESTORE_SEARCH = $(".btnRESTORE_SEARCH");
                if (selfspec.$btnRESTORE_SEARCH.length === 0) {
                    return;
                }
                clearInterval(timer);
                timer = null;
                selfspec.$btnRESTORE_SEARCH.unbind('click').click(showSearchForm).show();
            }
            setTimeout(function () {
                $('#idProductID').val(selfspec.autoSearchId);
                $('#idExactSearchId').val("Y");
                // do search.. hopefully it'll find just one!, replace with exact search later...
                selfspec.Searchspec(function () {
                    $("#PANEL_EXISTING_NEW_BUTTONS").hide();
                    $("#PANEL_SEARCH").hide();
                    $("#idSpecsearchlist").hide();
                    var specName = selfspec.SpecList()[0].specn_nm.value;
                    window.document.title = 'CDMMS | Spec: ' + specName + ' (#' + selfspec.autoSearchId + ')';
                });

                timer = setInterval(setupRestoreSearchButton, 250);
            }, 1000);
        }
        Specification.prototype.activate = function (specType, specId, query) {
            console.log(specType + ", " + specId);

            if (specType != null) {
                var mtlItmId = query.mtlid;

                selfspec.isRedirect = true;
                selfspec.backToMtlItmId = mtlItmId;

                if (specId == null) {
                    selfspec.selectRadioSpec('newSpec');
                    selfspec.selectedNewSpecification(specType);
                    selfspec.specificationSelected(0, specType, selfspec, mtlItmId);
                } else {
                    selfspec.specificationSelected(specId, specType, selfspec, mtlItmId);
                }
            } else {
                selfspec.backToMtlItmId = 0;
            }
        };

        Specification.prototype.specificationSelectCall = function (selected) {
            console.log("selectedId = " + selected.specn_id.value);

            document.getElementById("idSpecificationdetails").style.visibility = "visible";

            var selectedId = selected.specn_id.value;
            var specType = selected.enumSpecTyp.value;

            selfspec.backToMtlItmId = 0;

            selfspec.specificationSelected(selectedId, specType, selfspec, 0);
        };

        Specification.prototype.setDimUomValue = function (dimuom) {
            if (dimuom && dimuom.options) {
                for (var i = 0; i < dimuom.options.length; i++) {
                    if (dimuom.value === dimuom.options[i].value) {
                        dimuom.value = dimuom.options[i].text;
                        break;
                    }
                }
            }
        };

        Specification.prototype.specificationSelected = function (selectedId, specType, stl, mtlItmId, next) {
            next = (next || function () { });
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            if (specType == 'PLUG_IN' || specType == 'PLUG-IN') {
                specType = 'PLUG_IN';
            }
            var url = 'api/specn/' + selectedId + '/' + specType;

            if (selectedId > 0) {
                selfspec.existingSelectedIdCurrent(selectedId);
                selfspec.existingSelectedspecTypeCurrent(specType);
            }

            if (mtlItmId && mtlItmId > 0) {
                selfspec.redirectMtlItmId = mtlItmId;
            } else {
                selfspec.redirectMtlItmId = 0;
            }
          
            if (specType == 'BAY') {
                http.get(url).then(function (response) {
                    if (response === "{}") {
                        $("#interstitial").hide();
                    }
                    else {
                        selfspec.HideEverythingElseFirst(stl);
                        stl.baySpecification(new baySpec(response));

                        if (selfspec.redirectMtlItmId && selfspec.redirectMtlItmId > 0) {
                            http.get('api/material/' + selfspec.redirectMtlItmId).then(function (response) {
                                if (response === "{}") {
                                    $("#interstitial").hide();
                                } else {
                                    var bayList = JSON.parse(response);
                                    var bay = { "list": [bayList] };
                                    var name = bay.list[0].Mfg.value + '-' + bay.list[0].PrtNo.value;
                                    var catalogDesc = bay.list[0].CtlgDesc.value;
                                    var itemDesc = '';

                                    if (bay.list[0].ItemDesc != null) {
                                        itemDesc = bay.list[0].ItmDesc.value;
                                    }

                                    if (stl.baySpecification().selectedBaySpec().Desc.value == '') {
                                        if (catalogDesc !== '') {
                                            stl.baySpecification().selectedBaySpec().Desc.value = catalogDesc;
                                        }
                                        else {
                                            stl.baySpecification().selectedBaySpec().Desc.value = itemDesc;
                                        }
                                    }

                                    stl.setDimUomValue(bay.list[0].DimUom);

                                    stl.baySpecification().selectedBaySpec().Bay = bay;

                                    if (selectedId == '0') {
                                        stl.baySpecification().selectedBaySpec().Nm.value = name;
                                        stl.baySpecification().selectedBaySpec().RvsnNm.value = name;

                                        stl.baySpecification().specName(name);
                                    }

                                    if (bay.list[0].HECI) {
                                        stl.baySpecification().selectedBaySpec().Heci.value = bay.list[0].HECI.value;
                                    }
                                    if (bay.list[0].CLEI) {
                                        stl.baySpecification().selectedBaySpec().Clei.value = bay.list[0].CLEI.value;
                                    }
                                    stl.baySpecification().selectedBaySpec().Gnrc.enable = false;
                                    stl.baySpecification().selectedBaySpec().RO.enable = false;
                                    stl.baySpecification().selectedBaySpec().RO.bool = bay.list[0].RO.bool;
                                    stl.baySpecification().enableAssociate(false);

                                    stl.baySpecification().selectedBaySpec(stl.baySpecification().selectedBaySpec());
                                }
                                next();
                            });
                        } else {
                            selfspec.scrollToSpecDetails();
                            next();
                        }
                    }

                    if (selfspec.isRedirect && selectedId === 0) {
                        $("#newSpecResetVal").val(specType)
                    }
                });
            }
            else if (specType == 'BAY_INTERNAL') {
                http.get(url).then(function (response) {
                    if (response === "{}") {
                        $("#interstitial").hide();
                    }
                    else {
                        selfspec.HideEverythingElseFirst(stl);
                        stl.bayInternalSpecification(new bayInternalSpec(response));
                        selfspec.scrollToSpecDetails();
                    }
                    next();
                });
            }
            else if (specType == 'CARD') {
                http.get(url).then(function (response) {

                    if (response === "{}") {
                        $("#interstitial").hide();
                    }
                    else {
                        // Adding this json format for identifying the screen flow 
                        // through specification/assignment module
                        var jsonres = {
                            resp: response,
                            specification: true
                        }
                        selfspec.HideEverythingElseFirst(stl);
                        stl.cardSpecification(new cardSpec(jsonres));

                        if (selfspec.redirectMtlItmId && selfspec.redirectMtlItmId > 0) {
                            http.get('api/material/' + selfspec.redirectMtlItmId).then(function (response) {
                                if (response === "{}") {
                                    $("#interstitial").hide();
                                } else {
                                    var cardList = JSON.parse(response);
                                    var card = { "list": [cardList] };
                                    var name = card.list[0].Mfg.value + '-' + card.list[0].PrtNo.value;
                                    var catalogDesc = card.list[0].CtlgDesc.value;
                                    var ItemDesc = '';

                                    if (card.list[0].ItmDesc != null) {
                                        itemDesc = card.list[0].ItmDesc.value;
                                    }

                                    if (stl.cardSpecification().selectedCardSpec().Desc.value == '') {
                                        if (catalogDesc !== '') {
                                            stl.cardSpecification().selectedCardSpec().Desc.value = catalogDesc;
                                        }
                                        else {
                                            stl.cardSpecification().selectedCardSpec().Desc.value = itemDesc;
                                        }
                                    }

                                    stl.setDimUomValue(card.list[0].DimUom);

                                    stl.cardSpecification().selectedCardSpec().Card = card;

                                    if (selectedId == '0') {
                                        stl.cardSpecification().selectedCardSpec().Nm.value = name;
                                        stl.cardSpecification().selectedCardSpec().RvsnNm.value = name;
                                    }

                                    if (stl.cardSpecification().selectedCardSpec().Card.list[0].Wght && stl.cardSpecification().selectedCardSpec().Card.list[0].Wght.value !== '') {
                                        stl.cardSpecification().selectedCardSpec().Card.list[0].Wght.value = selfspec.GetWeightConversion(stl.cardSpecification().selectedCardSpec().Card.list[0].Wght.value, stl.cardSpecification().selectedCardSpec().Card.list[0].WghtUom.value)
                                    }

                                    if (card.list[0].HECI) {
                                        stl.cardSpecification().selectedCardSpec().Heci.value = card.list[0].HECI.value;
                                    }
                                    if (card.list[0].CLEI) {
                                        stl.cardSpecification().selectedCardSpec().Clei.value = card.list[0].CLEI.value;
                                    }
                                    stl.cardSpecification().selectedCardSpec().RO.enable = false;
                                    stl.cardSpecification().selectedCardSpec().RO.bool = card.list[0].RO.bool;
                                    stl.cardSpecification().enableAssociate(false);

                                    stl.cardSpecification().selectedCardSpec(stl.cardSpecification().selectedCardSpec());
                                }
                                next();
                            });
                        } else {
                            selfspec.scrollToSpecDetails();
                            next();
                        }

                        if (selfspec.isRedirect && selectedId === 0) {
                            $("#newSpecResetVal").val(specType)
                        }
                    }
                });
            }
            else if (specType == 'NODE') {
                http.get(url).then(function (response) {

                    if (response === "{}") {
                        $("#interstitial").hide();
                    }
                    else {
                        selfspec.HideEverythingElseFirst(stl);
                        stl.nodeSpecification(new nodeSpec(response));

                        if (selfspec.redirectMtlItmId && selfspec.redirectMtlItmId > 0) {
                            http.get('api/material/' + selfspec.redirectMtlItmId).then(function (response) {
                                if (response === "{}") {
                                    $("#interstitial").hide();
                                } else {
                                    var nodeList = JSON.parse(response);
                                    var node = { "list": [nodeList] };
                                    var name = node.list[0].Mfg.value + '-' + node.list[0].PrtNo.value;
                                    var catalogDesc = node.list[0].CtlgDesc.value;
                                    var itemDesc = '';

                                    if (node.list[0].ItmDesc != null) {
                                        itemDesc = node.list[0].ItmDesc.value;
                                    }

                                    if (stl.nodeSpecification().selectedNodeSpec().Desc.value == '') {
                                        if (catalogDesc !== '') {
                                            stl.nodeSpecification().selectedNodeSpec().Desc.value = catalogDesc;
                                        }
                                        else {
                                            stl.nodeSpecification().selectedNodeSpec().Desc.value = itemDesc;
                                        }
                                    }

                                    if (node.list[0].Rvsn) {
                                        name = name + " " + node.list[0].Rvsn.value;
                                    }

                                    stl.setDimUomValue(node.list[0].DimUom);

                                    stl.nodeSpecification().selectedNodeSpec().Node = node;

                                    if (selectedId == '0') {
                                        stl.nodeSpecification().selectedNodeSpec().Nm.value = name;
                                        stl.nodeSpecification().selectedNodeSpec().RvsnNm.value = name;
                                        stl.nodeSpecification().specName(name);
                                    }

                                    // ensure node has default 'A'.  DE74636
                                    if (stl.nodeSpecification().selectedNodeSpec().NdeFrmtCd.value === '') {
                                        stl.nodeSpecification().selectedNodeSpec().NdeFrmtCd.value = 'A';
                                    }

                                    stl.nodeSpecification().selectedNodeSpec().Gnrc.enable = false;
                                    stl.nodeSpecification().selectedNodeSpec().RO.enable = false;
                                    stl.nodeSpecification().selectedNodeSpec().RO.bool = node.list[0].RO.bool;
                                    stl.nodeSpecification().enableAssociate(false);

                                    stl.nodeSpecification().selectedNodeSpec(stl.nodeSpecification().selectedNodeSpec());
                                }
                                next();
                            });
                        } else {
                            selfspec.scrollToSpecDetails();
                            next();
                        }

                        if (selfspec.isRedirect && selectedId === 0) {
                            $("#newSpecResetVal").val(specType)
                        }
                    }

                });
            }
            else if (specType == 'PLUG_IN') {
                http.get(url).then(function (response) {
                    if (response === "{}") {
                        $("#interstitial").hide();
                    }
                    else {
                        // Adding this json format for identifying the screen flow 
                        // through specification/assignment module                     
                        var jsonres = {
                            resp: response,
                            specification: true
                        }
                        selfspec.HideEverythingElseFirst(stl);
                        stl.pluginSpecification(new pluginSpec(jsonres));
                        selfspec.scrollToSpecDetails();
                    }
                    next();
                });
            }
            else if (specType == 'SHELF') {
                http.get(url).then(function (response) {
                    if (response === "{}") {
                        $("#interstitial").hide();
                    }
                    else {
                        // Adding this json format for identifying the screen flow 
                        // through specification/assignment module
                        var jsonres = {
                            resp: response,
                            specification: true
                        }
                        selfspec.HideEverythingElseFirst(stl);
                        stl.shelfSpecification(new shelfSpec(jsonres));

                        if (selfspec.redirectMtlItmId && selfspec.redirectMtlItmId > 0) {
                            http.get('api/material/' + selfspec.redirectMtlItmId).then(function (response) {
                                if (response === "{}") {
                                    $("#interstitial").hide();
                                } else {
                                    var shelfList = JSON.parse(response);
                                    var shelf = { "list": [shelfList] };
                                    var name = shelf.list[0].Mfg.value + '-' + shelf.list[0].PrtNo.value;
                                    var catalogDesc = shelf.list[0].CtlgDesc.value;
                                    var itemDesc = '';

                                    if (shelf.list[0].ItmDesc != null) {
                                        itemDesc = shelf.list[0].ItmDesc.value;
                                    }

                                    if (stl.shelfSpecification().selectedShelfSpec().Desc.value == '') {
                                        if (catalogDesc !== '') {
                                            stl.shelfSpecification().selectedShelfSpec().Desc.value = catalogDesc;
                                        }
                                        else {
                                            stl.shelfSpecification().selectedShelfSpec().Desc.value = itemDesc;
                                        }
                                    }

                                    stl.setDimUomValue(shelf.list[0].DimUom);

                                    stl.shelfSpecification().selectedShelfSpec().Shelf = shelf;

                                    if (selectedId == '0') {
                                        stl.shelfSpecification().selectedShelfSpec().Nm.value = name;
                                        stl.shelfSpecification().selectedShelfSpec().RvsnNm.value = name;
                                        stl.shelfSpecification().specName(name);
                                    }

                                    if (shelf.list[0].HECI) {
                                        stl.shelfSpecification().selectedShelfSpec().Heci.value = shelf.list[0].HECI.value;
                                    }
                                    if (shelf.list[0].CLEI) {
                                        stl.shelfSpecification().selectedShelfSpec().Clei.value = shelf.list[0].CLEI.value;
                                    }
                                    stl.shelfSpecification().selectedShelfSpec().Gnrc.enable = false;
                                    stl.shelfSpecification().selectedShelfSpec().RO.enable = false;
                                    stl.shelfSpecification().selectedShelfSpec().RO.bool = shelf.list[0].RO.bool;
                                    stl.shelfSpecification().enableAssociate(false);

                                    stl.shelfSpecification().selectedShelfSpec(stl.shelfSpecification().selectedShelfSpec());
                                }

                                next();
                            });
                        } else {
                            selfspec.scrollToSpecDetails();
                            next();
                        }

                        if (selfspec.isRedirect && selectedId === 0) {
                            $("#newSpecResetVal").val(specType)
                        }
                    }
                });
            }
            else if (specType == 'SLOT') {
                http.get(url).then(function (response) {
                    if (response === "{}") {
                        $("#interstitial").hide();
                    }
                    else {
                        // Adding this json format for identifying the screen flow 
                        // through specification/assignment module
                        var jsonres = {
                            resp: response,
                            specification: true
                        }
                        selfspec.HideEverythingElseFirst(stl);
                        stl.slotSpecification(new slotSpec(jsonres));
                        selfspec.scrollToSpecDetails();

                    }
                    next();
                });
            }
            else if (specType == 'PORT') {
                http.get(url).then(function (response) {
                    if (response === "{}") {
                        $("#interstitial").hide();
                    }
                    else {
                        // Adding this json format for identifying the screen flow 
                        // through specification/assignment module
                        var jsonres = {
                            resp: response,
                            specification: true
                        }
                        selfspec.HideEverythingElseFirst(stl);
                        stl.portSpecification(new portSpec(jsonres));
                        selfspec.scrollToSpecDetails();

                    }
                    next();
                });
            }
            else if (specType == 'BAY_EXTENDER') {
                http.get(url).then(function (response) {

                    if (response === "{}") {
                        $("#interstitial").hide();
                    }
                    else {
                        selfspec.HideEverythingElseFirst(stl);
                        stl.bayExtenderSpecification(new bayExtenderSpec(response));

                        if (selfspec.redirectMtlItmId && selfspec.redirectMtlItmId > 0) {
                            http.get('api/material/' + selfspec.redirectMtlItmId).then(function (response) {
                                if (response === "{}") {
                                    $("#interstitial").hide();
                                } else {
                                    var bayExtendList = JSON.parse(response);
                                    var bayExtend = { "list": [bayExtendList] };
                                    var name = bayExtend.list[0].Mfg.value + '-' + bayExtend.list[0].PrtNo.value;
                                    var catalogDesc = bayExtend.list[0].CtlgDesc.value;
                                    var itemDesc = '';

                                    if (bayExtend.list[0].ItmDesc) {
                                        itemDesc = bayExtend.list[0].ItmDesc.value;
                                    }

                                    if (stl.bayExtenderSpecification().selectedBayExtenderSpec().Desc.value == '') {
                                        if (catalogDesc !== '') {
                                            stl.bayExtenderSpecification().selectedBayExtenderSpec().Desc.value = catalogDesc;
                                        }
                                        else {
                                            stl.bayExtenderSpecification().selectedBayExtenderSpec().Desc.value = itemDesc;
                                        }
                                    }

                                    stl.setDimUomValue(bayExtend.list[0].DimUom);

                                    stl.bayExtenderSpecification().selectedBayExtenderSpec().BayExtndr = bayExtend;

                                    if (selectedId == '0') {
                                        stl.bayExtenderSpecification().selectedBayExtenderSpec().Nm.value = name;
                                        stl.bayExtenderSpecification().selectedBayExtenderSpec().RvsnNm.value = name;
                                        stl.bayExtenderSpecification().specName(name);
                                    }

                                    stl.bayExtenderSpecification().selectedBayExtenderSpec().RO.enable = false;
                                    stl.bayExtenderSpecification().selectedBayExtenderSpec().RO.bool = bayExtend.list[0].RO.bool;
                                    stl.bayExtenderSpecification().enableAssociate(false);

                                    stl.bayExtenderSpecification().selectedBayExtenderSpec(stl.bayExtenderSpecification().selectedBayExtenderSpec());
                                }
                                next();
                            });
                        } else {
                            selfspec.scrollToSpecDetails();
                            next();
                        }

                        if (selfspec.isRedirect && selectedId === 0) {
                            $("#newSpecResetVal").val(specType)
                        }
                    }
                });
            }

        };

        Specification.prototype.HideEverythingElseFirst = function (stl) {
            stl.baySpecification(null);
            stl.bayInternalSpecification(null);
            stl.cardSpecification(null);
            stl.nodeSpecification(null);
            stl.pluginSpecification(null);
            stl.shelfSpecification(null);
            stl.slotSpecification(null);
            stl.bayExtenderSpecification(null);
            stl.portSpecification(null);
        }

        Specification.prototype.selectNewSpecification = function (model, event) {
            selfspec.reset();
            selfspec.selectedNewSpecification(event.target.value);

            if (selfspec.selectedNewSpecification().length > 0) {
                selfspec.specificationSelected(0, selfspec.selectedNewSpecification(), selfspec, 0);
            }
        };

        Specification.prototype.reset = function () {
            selfspec.baySpecification(null);
            selfspec.nodeSpecification(null);
            selfspec.bayInternalSpecification(null);
            selfspec.pluginSpecification(null);
            selfspec.shelfSpecification(null);
            selfspec.slotSpecification(null);
            selfspec.cardSpecification(null);
            selfspec.bayExtenderSpecification(null);
            selfspec.portSpecification(null);
            selfspec.backToMtlItmId = 0;
        };

        Specification.prototype.ClearSpecSearch = function () {
            document.getElementById('idProductID').value = '';
            document.getElementById('idName').value = '';
            document.getElementById('idDesc').value = '';
            document.getElementById('idstatuspec').selectedIndex = 0;
            document.getElementById('idspectype').selectedIndex = 0;
            document.getElementById('idType').selectedIndex = 0;
            document.getElementById('idModelName').value = '';
            document.getElementById('idMaterialCode').value = '';
        }

        Specification.prototype.Searchspec = function (next) {

            next = (next || function () { })

            selfspec.SpecList(false);
            selfspec.reset();

            var intHeight = $("body").height();
            $("#interstitial").css("height", intHeight);
            $("#interstitial").show();
            var prodid = $("#idProductID").val();
            var Name = $("#idName").val();
            var Description = $("#idDesc").val();
            var Status = $('#idstatuspec').find("option:selected").val();
            var spectype = $('#idspectype').find("option:selected").val();
            var SpecClass = $('#idType').val();
            var modelName = $('#idModelName').val();
            var materialCode = $('#idMaterialCode').val();
            var exactSearch = $('#idExactSearchId').val();
            var route = 'api/specn/search';            

            var searchJSON = {
                id: prodid, name: Name, desc: Description, status: Status, specnType: spectype, class: SpecClass, modelname: modelName, materialcode: materialCode
            };

            if (searchJSON.id === undefined) {
                return;
            }
            if (exactSearch == "Y") {
                route = 'api/specn/search/exact';
            }

            $.ajax({
                type: "GET",
                url: route,
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
                    app.showMessage("No records found", 'Failed');
                }
                else {
                    var results = JSON.parse(data);
                    selfspec.SpecList(results);
                    //setTimeout(function () { $("#idSpecList").DataTable(); }, 1000);

                    $("#interstitial").hide();
                    var elmnt = document.getElementById("idType");
                    elmnt.scrollIntoView();

                    if (results[0].specTyp.value === 'BAY EXTENDER') {
                        if (results.length == 1) {  // pre-load the result if there's only one
                            selfspec.specificationSelected(results[0].specn_id.value, 'BAY_EXTENDER', selfspec, 0, next);
                        }
                        else {
                            next();
                        }
                    }
                    else {
                        if (results.length == 1) {  // pre-load the result if there's only one
                            selfspec.specificationSelected(results[0].specn_id.value, results[0].specTyp.value, selfspec, 0, next);
                        }
                        else {
                            next();
                        }
                    }
                }
            }
            function errorFunc() {
                $("#interstitial").hide();
                alert('error');
            }
        };

        Specification.prototype.NumDecimal = function (mp, event) {
            var charCode = event.keyCode;
            var name = event.currentTarget.id;
            var value = $('#' + name).val();
            var reqCharAfterdot = 3;

            if (charCode > 31 && (charCode < 48 || charCode > 57) && !(charCode == 46 || charCode == 8)) {
                return false;
            }
            else {
                var len = value.length;
                var index = value.indexOf('.');

                if (index > -1 && charCode == 46) {
                    return false;
                }
                if (index == 0 || index > 0) {
                    var CharAfterdot = (len + 1) - index;

                    if (CharAfterdot > reqCharAfterdot + 1) {
                        return false;
                    }
                }

                if (value == '.' && len == 1) {
                    $('#' + name).val('0' + value);
                }
            }

            return true;
        };

        Specification.prototype.onchangeRadioSpec = function () {
            selfspec.reset();
            selfspec.SpecList([]);
            $("#newSpecResetVal").val('');
            $("#idProductID").val('');
            $("#idName").val('');
            $("#idDesc").val('');
            $('#idstatuspec').val('');
            $('#idspectype').val('');
            $('#idType').val('');
        };

        Specification.prototype.updateOnSuccess = function () {
            selfspec.Searchspec();
            selfspec.specificationSelected(selfspec.existingSelectedIdCurrent(), selfspec.existingSelectedspecTypeCurrent(), selfspec, 0);
        };

        Specification.prototype.scrollToSpecDetails = function () {
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

        Specification.prototype.GetWeightConversion = function (weight, uomCd) {
            var spec = { 'weight': weight, 'uomcd': uomCd }
            var newWeight = '';
            $.ajax({
                type: "GET",
                url: 'api/specn/weightconversion',
                data: spec,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: getModelWeightSuccess,
                error: getModelWeightError,
                async: false
            });
            function getModelWeightSuccess(response) {
                var conversion = response;
                newWeight = (weight * conversion).toFixed(3);
            }
            function getModelWeightError() {
            }
            return newWeight;
        }

        return Specification;
    })
