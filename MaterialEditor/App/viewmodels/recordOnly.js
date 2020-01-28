define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', 'jqueryui', '../Utility/referenceDataHelper', 'durandal/app', 'datablescelledit', 'bootstrapJS', 'plugins/router'],
    function (composition, ko, $, http, activator, mapping, system, jqueryui, reference, app, datablescelledit, bootstrapJS, router) {
        var RecordOnly = function () {
            selfro = this;

            selfro.usr = require('Utility/user');
            selfro.mtl = ko.observable(null);

            selfro.DefaultPositionSchemeDD = ko.observableArray(['BOTTOM', 'FLOOR', 'TOP']);
            selfro.PartNumberTypeCodeDD = ko.observableArray(['ATOMIC', 'CONFIGURATION', 'MANUFACTURERS ASSEMBLY', 'PLUG-IN PACKAGE']);
            selfro.ProdTypeDD = ko.observableArray(['CSYS', 'LOOP', 'MISC', 'POWR', 'RADO', 'SWTC', 'TRAN']);
            selfro.GaugeUnitDD = ko.observableArray(['NOT APPLICABLE', 'CIRCULAR MILL', 'GAUGE']);

            var selectedBox = null;
                        
            selfro.selectNewExistingRadio = ko.observable('existingRO');

            selfro.recordtype = ko.observable("BOTH");
            selfro.searchBy = ko.observable("MtlCd");
            selfro.specCmdPrompt = ko.observable(false);
            selfro.selectedStatus = ko.observable('');
            selfro.selectedFeatureType = ko.observable('');
            selfro.selectedLctnPosInd = ko.observable('');
            selfro.selectedPrtNbrTypCd = ko.observable('');
            selfro.selectedPrdTyp = ko.observable('');
            selfro.selectedGgeUnt = ko.observable('');
            selfro.selectedPosSchm = ko.observable('');
            selfro.selectedLbrId = ko.observable('');
            selfro.selectedSetLgthUom = ko.observable('');
            selfro.selectedMaterialCategory = ko.observable('');
            selfro.selectedApcl = ko.observable('');
            selfro.selectedCableType = ko.observable('');
            selfro.selectedPlugInType = ko.observable('');
            selfro.showMtl = ko.observable(false);

            //variablecables
            selfro.associatetbl = ko.observableArray();
            selfro.searchtbl = ko.observableArray();
            //variablecables

            //srReplacedBy
            selfro.searchtblsrd = ko.observableArray();
            selfro.parentSRd = ko.observable(null);
            //srReplacedBy

            //srReplaces
            selfro.srsPartstbl = ko.observableArray();
            //srReplaces

            //cpReplacedBy
            selfro.parentCPd = ko.observable(null);
            //cpReplacedBy

            //cpReplaces
            selfro.cpsPartstbl = ko.observableArray();
            //cpReplaces

            //revisionParts
            selfro.revisionPartstbl = ko.observableArray();
            selfro.searchtblrp = ko.observableArray();
            //revisionParts
        };

        RecordOnly.prototype.setSpecButtonText = function (ftrTypId) {
            var btn = document.getElementById("specNameNavigateBtn");

            if (btn) {
                if (selfro.mtl.MtrlId.value() !== '0' && (ftrTypId == 1 || ftrTypId == 2 || ftrTypId == 5 || ftrTypId == 6 || ftrTypId == 7)) {
                    if (selfro.mtl.SpecId && selfro.mtl.SpecId.value() > 0 && selfro.mtl.FtrTyp.value() == ftrTypId) {
                        btn.innerHTML = "View Specification";
                        btn.disabled = false;
                        selfro.specCmdPrompt(false);
                    } else if (selfro.mtl.FtrTyp.value() == ftrTypId) {
                        btn.innerHTML = "Create Specification";
                        btn.disabled = false;
                        selfro.specCmdPrompt(false);
                    } else {
                        btn.innerHTML = "Create Specification";
                        btn.disabled = false;
                        selfro.specCmdPrompt(true);
                    }
                } else if (selfro.mtl.MtrlId.value() === '0' && (ftrTypId == 1 || ftrTypId == 2 || ftrTypId == 5 || ftrTypId == 6 || ftrTypId == 7)) {
                    btn.innerHTML = "Create Specification";
                    btn.disabled = false;
                    selfro.specCmdPrompt(false);
                } else {
                    btn.innerHTML = "Create Specification";
                    btn.disabled = true;
                    selfro.specCmdPrompt(false);
                }
            }
        };

        RecordOnly.prototype.navigateToSpecification = function () {
            if (selfro.specCmdPrompt()) {
                app.showMessage("Click on 'Ok' to Save Record Only and Navigate to Specification", 'Navigate to Specification', ['Ok', 'Cancel']).then(function (dialogResult) {
                    if (dialogResult == 'Ok') {
                        selfro.mtl.SpecTyp.value(($("#idFeatureType option:selected").text()).toUpperCase());
                        $("#interstitial").show();
                        selfro.Save();
                    }
                });
            } else {
                selfro.navigateToSpecificationOnSuccess();
            }
            
        };

        RecordOnly.prototype.navigateToSpecificationOnSuccess = function () {
            var url = '#/spec/' + selfro.mtl.SpecTyp.value();

            if (selfro.mtl.SpecId && selfro.mtl.SpecId.value() > 0) {
                url = url + '/' + selfro.mtl.SpecId.value();
            }
            if (selfro.mtl.id.value()) {
                url = url + '?mtlid=' + selfro.mtl.id.value();
            }

            router.navigate(url, false);
        };

        RecordOnly.prototype.activate = function (mtlItmId) {
            console.log(mtlItmId + ", " + mtlItmId);

            if (mtlItmId != null && mtlItmId > 0) {
                selfro.partSelected(mtlItmId, selfro);
               

            }
        };
        RecordOnly.prototype.getEmptyMaterial = function () {
            http.get('api/material/0/ro').then(function (response) {
                console.log(response);

                var results = JSON.parse(response);

                selfro.mtl = mapping.fromJS(results);

                selfro.selectedStatus('');
                selfro.selectedFeatureType('');
                selfro.selectedLctnPosInd('');
                selfro.selectedPrtNbrTypCd('');
                selfro.selectedPrdTyp('');
                selfro.selectedGgeUnt('');
                selfro.mtl.GgeUnt.value('NOT APPLICABLE');
                selfro.selectedPosSchm('');
                selfro.selectedLbrId('');
                selfro.selectedSetLgthUom('');
                selfro.selectedMaterialCategory('');
                selfro.selectedApcl(selfro.mtl.Apcl.value());
                selfro.selectedCableType('');
                selfro.selectedPlugInType('');
                    
                selfro.showMtl(true);

                selfro.setSpecButtonText('');

                selfro.ADD();
            },
            function (error) {
                console.log("Error in RecordOnly.prototype.getEmptyMaterial - " + error);
            });
        };

        RecordOnly.prototype.ADD = function (rootcontext) {
            $("#idsearchBar").hide();
            
            $("#idSave").show();
            $("#addOption").show();
            $("#idUpdate").hide();

            selfro.bindAutoComplete();
        };

        RecordOnly.prototype.searchForPart = function (searchValue, bindingObject) {
            console.log('RecordOnly.searchForPart ' + searchValue);

            $("#addOption").hide();
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            var url = 'api/modifiedpart/search/' + selfro.recordtype();

            return http.get(url, {
                'val': searchValue, 'searchBy': selfro.searchBy()
            })
        };

        RecordOnly.prototype.partSelected = function (selectedId, part) {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            part.showMtl(false);

            console.log("selectedId = " + selectedId);

            var url = 'api/material/' + selectedId + '/ro';

            http.get(url).then(function (response) {
                console.log('response = ' + response);

                var results = JSON.parse(response);

                part.mtl = mapping.fromJS(results);

                if (part.mtl.RtPrtNbr && part.mtl.RtPrtNbr.value() === '') {
                    if (part.mtl.PrtNo && part.mtl.PrtNo.value() !== '') {
                        part.mtl.RtPrtNbr.value(part.mtl.PrtNo.value());
                    }
                }

                if (part.mtl.LctnPosInd.value())
                    part.selectedLctnPosInd(part.mtl.LctnPosInd.value());
                else
                    part.selectedLctnPosInd('N');

                if (part.mtl.PrtNbrTypCd.value())
                    part.selectedPrtNbrTypCd(part.mtl.PrtNbrTypCd.value());

                if (part.mtl.PrdTyp.value())
                    part.selectedPrdTyp(part.mtl.PrdTyp.value());

                if (part.mtl.GgeUnt.value())
                    part.selectedGgeUnt(part.mtl.GgeUnt.value());

                if (part.mtl.PosSchm.value())
                    part.selectedPosSchm(part.mtl.PosSchm.value());

                if (part.mtl.LbrId.value())
                    part.selectedLbrId(part.mtl.LbrId.value());

                if (part.mtl.Apcl.value())
                    part.selectedApcl(part.mtl.Apcl.value());

                if (part.mtl.SetLgthUom.value())
                    part.selectedSetLgthUom(part.mtl.SetLgthUom.value());

                if (part.mtl.Stts.value())
                    part.selectedStatus(part.mtl.Stts.value());

                if (part.mtl.CblTypId.value())
                    part.selectedCableType(part.mtl.CblTypId.value());

                if (part.mtl.PlgInRlTyp.value())
                    part.selectedPlugInType(part.mtl.PlgInRlTyp.value());

                if (part.mtl.FtrTyp.value()) {
                    part.selectedFeatureType(part.mtl.FtrTyp.value());
                }

                if (part.mtl.MtlCtgry.value) {
                    part.selectedMaterialCategory(part.mtl.MtlCtgry.value());
                }

                part.showMtl(true);

                setTimeout(function () {
                    selfro.checkMtlCtgry(selfro.mtl.MtlCtgry.value(), false);
                    //selfro.checkFtrTyp(selfro.selectedFeatureType());
                    selfro.checkApcl(selfro.selectedApcl());
                    selfro.filterLaborIds(selfro.selectedMaterialCategory(), selfro.selectedFeatureType(), selfro.selectedCableType());

                    if (selfro.mtl.FtrTyp.value()) {
                        selfro.getFeatureTypeLocationType(selfro.mtl.FtrTyp.value());
                    }
                    
                    selfro.bindAutoComplete();
                }, 1000);

                $("#addOption").show();
                $("#idSave").hide();
                $("#idUpdate").show();
                $("#interstitial").hide();
            },
            function (error) {
                console.log("Error in RecordOnly.prototype.partSelected - " + error);

                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            });
        };

        RecordOnly.prototype.partSelectedOnload = function () {
            var partValid = selfro.mtl.RtPrtNbr.value();
            var partManValid = selfro.mtl.Mfg.value();

            if (partValid.substring(0, 3) == "USW" && (partValid.indexOf("XXX") != -1)) {
                selfro.mtl.MtlCtgry.value('3');
                selfro.mtl.FtrTyp.value('11');
                selfro.mtl.SetLgth.value('-1');
                selfro.mtl.SetLgthUom.value('13');
                document.getElementById("idMaterialCategory").disabled = false;
                document.getElementById("idFeatureType").disabled = false;
                document.getElementById("idSetLength").disabled = false;
                document.getElementById("idSetLengthUomDD").disabled = false;
                document.getElementById("idSpecificationName").disabled = false;
                document.getElementById("idSpecificationName").required = true;
                document.getElementById("idSpecificationName").value = selfro.mtl.SpecNm.value();

            }
            if (partManValid == "OFSB" && (partValid.indexOf("XXX") != -1)) {
                selfro.mtl.MtlCtgry.value('3');
                selfro.mtl.FtrTyp.value('11');
                selfro.mtl.SetLgth.value('-1');
                selfro.mtl.SetLgthUom.value('15');
                document.getElementById("idMaterialCategory").disabled = false;
                document.getElementById("idFeatureType").disabled = false;
                document.getElementById("idSetLength").disabled = false;
                document.getElementById("idSetLengthUomDD").disabled = false;
                document.getElementById("idSpecificationName").disabled = false;
                document.getElementById("idSpecificationName").required = true;
                document.getElementById("idSpecificationName").value = selfro.mtl.SpecNm.value();
            }

            //this.mtl(selfro.mtl());

            document.getElementById('btnassociate').disabled = false;
        };

        RecordOnly.prototype.changeNewExisting = function (model, event) {
            if (event.target.value === 'newRO') {
                selfro.showMtl(false);
                selfro.getEmptyMaterial();                
            }
            else
                selfro.showMtl(false);

            return true;
        };

        RecordOnly.prototype.checkEnterVCmodifiedPart = function (root, event) {
            if (event.keyCode === 13) {
                console.log("Enter");
                //self.searchmtl(root);
            }
            else { return true; }
        };

        //UI validation
        RecordOnly.prototype.changeMtlCtgry = function (model, event) {
            //self.saveValidation(true);
            selfro.checkMtlCtgry(event.target.value, true);
            selfro.filterLaborIds(selfro.selectedMaterialCategory(), selfro.selectedFeatureType(), selfro.selectedCableType());
        };

        RecordOnly.prototype.checkMtlCtgry = function (mtlCtgryVal, resetSetLgthUom) {

            if (document.getElementById('btnassociate')) {
                document.getElementById('btnassociate').disabled = true;
            }

            if (document.getElementById("idFeatureType")) {
                if (mtlCtgryVal == "3") {
                    document.getElementById('idFeatureType').disabled = false;
                    //document.getElementById("idSpecificationName").disabled = false;
                    //document.getElementById("idSpecificationName").required = true;
                    //document.getElementById('ftrTypDropdown').value = selfro.mtl.FtrTyp.value();
                    //selfro.selectedFeatureType(selfro.mtl.FtrTyp.value());
                    selfro.checkFtrTyp(selfro.selectedFeatureType(), resetSetLgthUom);
                }
                else {
                    document.getElementById("idSpecificationName").required = false;
                    document.getElementById("idSpecificationName").disabled = true;
                    document.getElementById("idSpecificationName").value = '';                    
                    document.getElementById('idFeatureType').disabled = true;
                    document.getElementById('btnassociate').disabled = true;

                    selfro.mtl.SpecNm.value('');
                    selfro.selectedFeatureType('');
                    selfro.checkFtrTyp(selfro.selectedFeatureType(), resetSetLgthUom);
                }
            }
        };

        RecordOnly.prototype.getFeatureTypeLocationType = function (ftrTypId) {
            var helper = new reference();
            var results = helper.getFeatureTypeLocatableType(ftrTypId).then(function (results) {
                if (results === null || results === '')
                { }
                else if (results === 'Locatable') {
                    if (document.getElementById("idSpecificationName")) {
                        if (ftrTypId == 1 || ftrTypId == 2 || ftrTypId == 5 || ftrTypId == 6 || ftrTypId == 7) {
                            document.getElementById("idSpecificationName").disabled = true;
                            document.getElementById("idSpecificationName").required = true;
                        } else {
                            document.getElementById("idSpecificationName").disabled = false;
                            document.getElementById("idSpecificationName").required = true;
                        }
                    }
                }
                else if (results === 'Non Locatable') {
                    if (document.getElementById("idSpecificationName")) {
                        document.getElementById("idSpecificationName").required = false;
                        document.getElementById("idSpecificationName").disabled = true;
                        document.getElementById("idSpecificationName").value = '';

                        selfro.mtl.SpecNm.value('');
                    }
                }
            });
        };

        RecordOnly.prototype.changeCableType = function (model, event) {
            selfro.filterLaborIds(selfro.selectedMaterialCategory(), selfro.selectedFeatureType(), selfro.selectedCableType());
            selfro.checkFtrTyp(selfro.selectedFeatureType(), true);
        };

        RecordOnly.prototype.changeFtrTyp = function (model, event) {
            //selfro.saveValidation(true);
            selfro.checkFtrTyp(event.target.value, true);

            selfro.getFeatureTypeLocationType(event.target.value);
            selfro.filterLaborIds(selfro.selectedMaterialCategory(), selfro.selectedFeatureType(), selfro.selectedCableType());
        };

        RecordOnly.prototype.checkFtrTyp = function (ftrTypVal, resetSetLgthUom) {
            if (document.getElementById("idSetLength")) {
                if (ftrTypVal == "9") {
                    //selfro.mtl.SetLgth.value('');
                    //selfro.mtl.SetLgthUom.value('');

                    document.getElementById('idSetLength').value = selfro.mtl.SetLgth.value();
                    document.getElementById('idSetLengthUomDD').value = selfro.mtl.SetLgthUom.value();
                    selfro.selectedSetLgthUom(selfro.mtl.SetLgthUom.value());

                    document.getElementById('idSetLength').disabled = false;
                    document.getElementById('idSetLengthUomDD').disabled = false;
                    document.getElementById('cblTypDropdown').disabled = false;
                    document.getElementById('plginTypDropdown').disabled = true;
                    document.getElementById('plginTypDropdown').required = false;
                    document.getElementById('btnassociate').disabled = true;
                }
                else if (ftrTypVal == "10") {
                    document.getElementById('btnassociate').disabled = true;
                    document.getElementById('idSetLength').disabled = true;
                    document.getElementById('idSetLengthUomDD').disabled = true;
                    document.getElementById('idSetLength').value = 0;
                    document.getElementById('idSetLengthUomDD').value = 15;
                    document.getElementById('cblTypDropdown').disabled = false;
                    document.getElementById('plginTypDropdown').disabled = true;
                    document.getElementById('plginTypDropdown').required = false;
                    
                    selfro.mtl.SetLgth.value('0');
                    selfro.mtl.SetLgthUom.value('15');
                    selfro.selectedSetLgthUom('15');
                }
                else if (ftrTypVal == "11") {
                    document.getElementById('cblTypDropdown').disabled = false;
                    document.getElementById('plginTypDropdown').disabled = true;
                    document.getElementById('plginTypDropdown').required = false;
                    
                    if (selfro.mtl.MtrlId.value() !== '0') {
                        document.getElementById('btnassociate').disabled = false;
                        document.getElementById('associateCblBtnMsg').style.display = "none";
                        document.getElementById('idClei').disabled = true
                    }
                    else {
                        document.getElementById('associateCblBtnMsg').style.display = "";
                    }

                    document.getElementById('idSetLength').disabled = true;                    
                    document.getElementById('idSetLength').value = -1;
                    document.getElementById('idSetLengthUomDD').disabled = false;

                    if (resetSetLgthUom) {
                        if (selfro.selectedCableType() === '2') {
                            document.getElementById('idSetLengthUomDD').value = 15;
                            selfro.mtl.SetLgthUom.value('15');
                            selfro.selectedSetLgthUom('15');
                        } else {
                            document.getElementById('idSetLengthUomDD').value = 13;
                            selfro.mtl.SetLgthUom.value('13');
                            selfro.selectedSetLgthUom('13');
                        }
                    }

                    selfro.mtl.SetLgth.value('-1');
                }
                else if (ftrTypVal == "30") {
                    selfro.mtl.SetLgth.value('');
                    selfro.mtl.SetLgthUom.value('');
                    selfro.selectedSetLgthUom('');

                    document.getElementById('btnassociate').disabled = true;
                    document.getElementById('idSetLength').disabled = true;
                    document.getElementById('idSetLengthUomDD').disabled = true;
                    document.getElementById('idSetLength').value = '';
                    document.getElementById('idSetLengthUomDD').value = '';
                    document.getElementById('cblTypDropdown').disabled = false;
                    document.getElementById('plginTypDropdown').disabled = true;
                    document.getElementById('plginTypDropdown').required = false;
                }
                else if (ftrTypVal == "8") {
                    selfro.mtl.SetLgth.value('');
                    selfro.mtl.SetLgthUom.value('');
                    selfro.selectedSetLgthUom('');

                    document.getElementById('btnassociate').disabled = true;
                    document.getElementById('idSetLength').disabled = true;
                    document.getElementById('idSetLengthUomDD').disabled = true;
                    document.getElementById('idSetLength').value = '';
                    document.getElementById('idSetLengthUomDD').value = '';
                    document.getElementById('cblTypDropdown').disabled = true;
                    document.getElementById('plginTypDropdown').disabled = false;
                    document.getElementById('plginTypDropdown').required = true;
                }
                else {
                    selfro.mtl.SetLgth.value('');
                    selfro.mtl.SetLgthUom.value('');
                    selfro.selectedSetLgthUom('');
                    selfro.mtl.CblTypId.value('');
                    selfro.mtl.PlgInRlTyp.value('');
                    selfro.selectedCableType('');
                    selfro.selectedPlugInType('');

                    document.getElementById('btnassociate').disabled = true;
                    document.getElementById('idSetLength').disabled = true;
                    document.getElementById('idSetLengthUomDD').disabled = true;
                    document.getElementById('idSetLength').value = '';
                    document.getElementById('idSetLengthUomDD').value = '';
                    document.getElementById('cblTypDropdown').value = '';
                    document.getElementById('plginTypDropdown').value = '';
                    document.getElementById('cblTypDropdown').disabled = true;
                    document.getElementById('plginTypDropdown').disabled = true;
                    document.getElementById('plginTypDropdown').required = false;
                    document.getElementById('associateCblBtnMsg').style.display = "none";
                }

                selfro.setSpecButtonText(ftrTypVal);
            }
        };

        RecordOnly.prototype.changeApcl = function (model, event) {
            //self.saveValidation(true);
            selfro.checkApcl(event.target.value);
            return true;
        };

        RecordOnly.prototype.checkApcl = function (apclVal) {
            if (document.getElementById("txtApcl")) {
                //if (apclVal.toUpperCase() === "SR" || selfro.mtl.RplcsPrtNbr !== undefined || selfro.mtl.RplcdByPrtNbr !== undefined) {
                if (selfro.mtl.MtrlId.value() !== "0" && (apclVal.toUpperCase() === "SR" || selfro.mtl.RplcsPrtNbr !== undefined)) {
                    document.getElementById('btnsr').disabled = false;
                }
                else { document.getElementById('btnsr').disabled = true; }
            }

        };

        RecordOnly.prototype.NumDecimal = function (mp, event) {
            //self.saveValidation(true);
            var charCode = event.keyCode;
            var name = event.currentTarget.id;
            var value = $('#' + name).val();
            var reqCharAfterdot = 2;
            if (name == "txtHght" || name == "txtWdth" || name == "txtDpth") { reqCharAfterdot = 3; }

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

        //UI validation

        //UI functions
        RecordOnly.prototype.enableSave = function () {
            //self.saveValidation(true);
        };

        RecordOnly.prototype.getfirstword = function (x) {
            var helper = require('Utility/helper');
            var eqpCls = helper.deriveEquipmentClass(document.getElementById("idCatalogDescription").value, document.getElementById("idEquipmentClass").value)

            document.getElementById("idEquipmentClass").value = eqpCls;

            x.mtl.EqptCls.value(eqpCls);
        };

        //RecordOnly.prototype.checkLocatable = function (model, event) {
        //    $("#interstitial").css("height", "100%");
        //    $("#interstitial").show();

        //    var self = this;
        //    var helper = new reference();
        //    var selectedValue = event.target.value;
        //    var results = helper.getFeatureTypeLocatableType(selectedValue).then(function (results) {
        //        if (results === null || results === '')
        //        { }
        //        else if (results === 'Locatable') {
        //            document.getElementById("idSpecificationName").disabled = false;
        //            document.getElementById("idSpecificationName").required = true;
        //        }
        //        else if (results === 'Non Locatable') {
        //            document.getElementById("idSpecificationName").required = false;
        //            document.getElementById("idSpecificationName").disabled = true;
        //            document.getElementById("idSpecificationName").value = '';
        //            self.mtl.SpecNm.value('');
        //        }

        //        $("#interstitial").hide();
        //    });
        //};

        RecordOnly.prototype.checkEnterVC = function (root, event) {
            if (event.keyCode === 13) {
                console.log("Enter");
                selfro.searchmtl(root);
            }
            else { return true; }
        };

        RecordOnly.prototype.checkEnterSR = function (root, event) {
            if (event.keyCode === 13) {
                console.log("Enter");
                selfro.searchmtlSR(root);
            }
            else { return true; }
        };

        RecordOnly.prototype.checkEnterRP = function (root, event) {
            if (event.keyCode === 13) {
                console.log("Enter");
                selfro.searchmtlRP(root);
            }
            else { return true; }
        };

        //RecordOnly.prototype.attached = function (view) {
        //    var ac = $("#mfgAutoComplete");

        //    ac.autocomplete({
        //        minLength: 2,
        //        delay: 500,
        //        source: function (request, response) {
        //            var getUrl = 'api/reference/autocomplete/mfg/' + request.term;

        //            $.ajax({
        //                type: "GET",
        //                url: getUrl,
        //                contentType: "application/json; charset=utf-8",
        //                dataType: "json",
        //                success: function (data) {
        //                    response(getSuccessful(data))
        //                },
        //                error: getError,
        //                context: this
        //            });

        //            function getSuccessful(data) {
        //                return JSON.parse(data);
        //            }

        //            function getError() {
        //            }
        //        },
        //        change: function (event, ui) {
        //            var selected = ui.item;

        //            if (!selected) {
        //                $("#mfgAutoComplete").val("");
        //            }
        //            else {
        //                self.workingMaterial().mtl.Mfg.value(selected.label);

        //                getDescription(selected.label);
        //            }
        //        },
        //        select: function (event, ui) {
        //            self.workingMaterial().mtl.Mfg.value(ui.item.label);

        //            getDescription(ui.item.label);
        //        }
        //    });

        //    function getDescription(label) {
        //        var getUrl = 'api/reference/autocomplete/mfgDesc/' + label;

        //        $.ajax({
        //            type: "GET",
        //            url: getUrl,
        //            contentType: "application/json; charset=utf-8",
        //            dataType: "json",
        //            success: function (data) {
        //                var obj = JSON.parse(data);

        //                if (obj && obj[0] && obj[0].value) {
        //                    self.workingMaterial().mtl.MfgDesc.value(obj[0].value)
        //                }
        //            },
        //            //error: getError,
        //            context: this
        //        });
        //    };
        //};
        //UI functions

        RecordOnly.prototype.setWorkingMtlNumberValue = function (inputName, workingMtlField) {
            var input = '#' + inputName;
            var inputObj = $(input);

            if (inputObj && inputObj[0]) {
                if (inputObj[0].value.length === 0) {
                    inputObj[0].value = '0';
                }

                if (inputObj[0].value !== workingMtlField.value()) {
                    workingMtlField.value(inputObj[0].value);
                }
            }
        };

        RecordOnly.prototype.Save = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            if (this.mtl.MfgId.value() == null) {
                $("#interstitial").hide();

                return app.showMessage('Manufacturer CLMC is a required field.');
            }
            else {
                var saveJSON = this.mtl;
                var urlSU;

                selfro.setWorkingMtlNumberValue('idHeight', saveJSON.Hght);
                selfro.setWorkingMtlNumberValue('idWidth', saveJSON.Wdth);
                selfro.setWorkingMtlNumberValue('idDepth', saveJSON.Dpth);

                saveJSON.Stts.value(selfro.selectedStatus());
                saveJSON.MtlCtgry.value(selfro.selectedMaterialCategory());
                saveJSON.FtrTyp.value(selfro.selectedFeatureType());
                saveJSON.LctnPosInd.value(selfro.selectedLctnPosInd());
                saveJSON.LbrId.value(selfro.selectedLbrId());
                saveJSON.SetLgthUom.value(selfro.selectedSetLgthUom());
                saveJSON.Apcl.value(selfro.selectedApcl());
                saveJSON.CblTypId.value(selfro.selectedCableType());
                saveJSON.PlgInRlTyp.value(selfro.selectedPlugInType());

                if (saveJSON.MtlCtgry.value() === '3' && saveJSON.FtrTyp.value() === '') {
                    $("#interstitial").hide();
                    return app.showMessage('Please select a Feature Type.');
                }

                var js = mapping.toJS(saveJSON);

                js.cuid = selfro.usr.cuid;

                var jsString = JSON.stringify(js);

                console.log(jsString);

                //if ($("#idUpdate").is(":visible")) {
                //    urlSU = 'api/modifiedpart/save/update';
                //}
                //else {
                //    urlSU = 'api/modifiedpart/save';
                //}
                urlSU = 'api/material/update';

                $.ajax({
                    type: "POST",
                    url: urlSU,
                    data: jsString,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successFunc,
                    error: errorFunc,
                    context: selfro
                });

                function successFunc(data, status) {
                    $("#interstitial").hide();

                    if (data[0] = "success") {
                        app.showMessage('Record only parts are updated successfully. CDMMSID: ' + data[1], 'Saved');

                        selfro.SEARCH(this);
                        selfro.partSelected(data[1], this);

                        if (data.length >= 3) {
                            var helper = new reference();
                            helper.getSendToNdsStatus(data[1], data[2]);
                        }
                        if (selfro.specCmdPrompt()) {
                            selfro.navigateToSpecificationOnSuccess();
                        }
                    }
                    else if (data[1] == "constraint") {
                        app.showMessage('Entered Material Code is already available in the database.', 'Failed');
                    }
                    else {
                        app.showMessage(data[1], 'Failed');
                    }
                }

                function errorFunc() {
                    $("#interstitial").hide();
                    app.showMessage('Please check inputs again.', 'Failed');
                }
            }
        };

        RecordOnly.prototype.SEARCH = function (rootcontext) {
            $("#idsearchBar").show();
            $("#addOption").hide();
        };

        function errorFunc(data, status) {
            $("#interstitial").hide();
            alert('error');
        };

        RecordOnly.prototype.Reset = function () {
            selfro.associatetbl(false);
            selfro.searchtbl(false);

            document.getElementById("idcdmmsidmodal").value = "";
            document.getElementById("materialcodemodal").value = "";
            document.getElementById("partnumbermodal").value = "";
            document.getElementById("clmcmodal").value = "";
            document.getElementById("catlogdsptn").value = "";
        };

        //Variable Cable
        RecordOnly.prototype.associatecablepopup = function (model, event) {
            var mtlID = event.mtl.MtrlId.value();

            if (mtlID === undefined) {
                mtlID = 0;
            }
            else {
                console.log("hi");
            }

            var clmcpop = event.mtl.Mfg.value();            
            var descpop = document.getElementById("idCatalogDescription").value;            
            var partnopop = document.getElementById("idPartNumber").value;            
            var modal = document.getElementById('myModal');
            var btn = document.getElementById("btnassociate");
            var span = document.getElementsByClassName("close")[0];

            $('#partnumberpopup').text(partnopop);
            $('#descpopup').text(descpop);
            $('#clcmcpopup').text(clmcpop);

            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            $.ajax({
                type: "GET",
                url: 'api/variablecable/' + mtlID,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successAssociate,
                error: errorFunc,
                context: selfro,
                async: false
            });
            function successAssociate(data, status) {
                if (data === '{}') {
                    selfro.associatetbl(false);
                }
                else {
                    var results = JSON.parse(data);
                    selfro.associatetbl(results);
                }

                // When the user clicks the button, open the modal 
                btn.onclick = function () {
                    modal.style.display = "block";
                }

                $("#interstitial").hide();
                $("#myModal").show();

                // When the user clicks on <span> (x), close the modal
                span.onclick = function () {
                    selfro.Reset();
                    modal.style.display = "none";
                }

                // When the user clicks anywhere outside of the modal, close it
                window.onclick = function (event) {
                    if (event.target == modal) {
                        selfro.Reset();
                        modal.style.display = "none";
                    }
                }

                $(document).ready(function () {
                    $(".chkbxsearch").click(function () {
                        selectedBox = this.id;
                        $(".chkbxsearch").each(function () {
                            if (this.id == selectedBox) {
                                this.checked = true;
                            }
                            else {
                                this.checked = false;
                            };
                        });
                    });
                });
            }
        };

        RecordOnly.prototype.searchmtl = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            selfro.searchtbl(false);

            var mtlid = document.getElementById("idcdmmsidmodal").value;
            var mtlcode = $("#materialcodemodal").val();
            var partnumb = $("#partnumbermodal").val();
            var clmc = $("#clmcmodal").val();
            var caldsp = $("#catlogdsptn").val();
            var src = "ro";
            var searchJSON = {
                material_item_id: mtlid, product_id: mtlcode, mfg_part_no: partnumb, mfg_id: clmc, mat_desc: caldsp, source: src
            };

            $.ajax({
                type: "GET",
                url: 'api/variablecable/search',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successSearch,
                error: errorFunc,
                context: selfro,
                async: false
            });

            function successSearch(data, status) {
                if (data === 'no_results') {
                    $("#interstitial").hide();
                    $(".NoRecord").show();
                    setTimeout(function () { $(".NoRecord").hide() }, 5000);
                } else {
                    //document.getElementById("savemodalsrdbtn").disabled = false;
                    var results = JSON.parse(data);

                    selfro.searchtbl(results);

                    $("#interstitial").hide();

                    $("#searchresults .chkbxsearchAll").change(function () {
                        $("#searchresults .chkbxsearch").prop('checked', $(this).prop("checked"));
                    });
                }
            }
        };

        RecordOnly.prototype.saveAssociateCable = function (event) {
            var Selectedrow = ko.observableArray();

            $("#searchresults .chkbxsearch").each(function () {
                if (this.checked == true) {
                    Selectedrow.push(this.value);
                }
            });

            $("#associatedcables .chktblasc").each(function () {
                if (this.checked == true) {
                    Selectedrow.push(this.value);
                }
            });

            var saveop = Selectedrow();
            var mtlID = event.mtl.MtrlId.value();
            var url = 'api/variablecable/update/ro/' + mtlID;

            http.post(url, saveop).then(function (response) {
                console.log(response);

                if (response === 'Success') {
                    app.showMessage('Saved successfully');
                    selfro.Reset();
                    $("#myModal").hide();
                }
                else {
                    $(".errorAssociateCableModal").show();
                    setTimeout(function () { $(".errorAssociateCableModal").hide() }, 5000);
                }
            },
            function (error) {
                $("#interstitial").hide();
                return app.showMessage('error');
            });
        };

        RecordOnly.prototype.cancelmodal = function () {
            selfro.Reset();
            $("#myModal").hide();
        };

        RecordOnly.prototype.bindAutoComplete = function (view) {
            var ac = $("#mfgAutoComplete");

            ac.autocomplete({
                minLength: 2,
                delay: 500,
                source: function (request, response) {
                    var getUrl = 'api/reference/autocomplete/mfg/' + request.term;

                    $.ajax({
                        type: "GET",
                        url: getUrl,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: function (data) {
                            response(getSuccessful(data))
                        },
                        error: getError,
                        context: this
                    });

                    function getSuccessful(data) {
                        return JSON.parse(data);
                    }

                    function getError() {
                    }
                },
                change: function (event, ui) {
                    var selected = ui.item;

                    if (!selected) {
                        $("#mfgAutoComplete").val("");
                    }
                    else {
                        selfro.mtl.Mfg.value(selected.label);
                        selfro.mtl.MfgId.value(selected.value);

                        $("#mfgAutoComplete").val("");
                    }
                },
                select: function (event, ui) {
                    selfro.mtl.Mfg.value(ui.item.label);
                    selfro.mtl.MfgId.value(ui.item.value);
                }
            });
        };

        RecordOnly.prototype.filterLaborIds = function (pMtrlCatId, pFeatTypId, pCablTypId) {
            $("#interstitial").show();

            var searchJSON = {
                pMtrlCatId: pMtrlCatId, pFeatTypId: pFeatTypId, pCablTypId: pCablTypId
            };

            http.get('api/assemblyunit/filterLaborIds', searchJSON).then(function (response) {
                console.log(response);

                if ('no_results' === response) {
                    $("#interstitial").hide();

                    app.showMessage("Labor Id list not found").then(function () {
                        return;
                    });
                } else {
                    var results = JSON.parse(response);

                    selfro.mtl.LbrId.options(results);

                    $("#interstitial").hide();
                }
            },
            function (error) {
                $("#interstitial").hide();

                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            });
        };

        return RecordOnly;
    });