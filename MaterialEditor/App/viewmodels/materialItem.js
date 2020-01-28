define(['durandal/composition', 'plugins/router', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', 'jqueryui', '../Utility/referenceDataHelper', 'durandal/app', 'datablescelledit', 'bootstrapJS', 'jszip', 'fileSaver', 'myexcel'],
    function (composition, router, ko, $, http, activator, mapping, system, jqueryui, reference, app, datablescelledit, bootstrapJS, jszip, fileSaver, myexcel) {
        var MaterialItem = function (data) {
            selfmi = this;
            var results = JSON.parse(data);
            var selectedBox = null;
            selfmi.mtl = mapping.fromJS(results);

            selfmi.viewIspOrOspAttrs = ko.observable("I");
            selfmi.selectedStatus = ko.observable('');
            selfmi.selectedFeatureType = ko.observable('');
            selfmi.initialFeatureType = ko.observable();
            selfmi.selectedLctnPosInd = ko.observable('');
            selfmi.selectedPrtNbrTypCd = ko.observable('');
            selfmi.selectedPrdTyp = ko.observable('');
            selfmi.selectedGgeUnt = ko.observable('');
            selfmi.selectedPosSchm = ko.observable('');
            selfmi.selectedLbrId = ko.observable('');
            selfmi.selectedSetLgthUom = ko.observable('');
            selfmi.selectedMaterialCategory = ko.observable('');
            selfmi.selectedApcl = ko.observable('');
            selfmi.selectedCableType = ko.observable('');
            selfmi.selectedPlugInType = ko.observable('');
            selfmi.saveSRandAM = ko.observable(false);
            /*selfmi.enableheight = ko.observable(false);
            selfmi.enablewidth = ko.observable(false);
            selfmi.enabledepth = ko.observable(false);*/
            selfmi.hlpncheckSearchType = ko.observable("Fuzzy Search");

            selfmi.hlpncdmmsid = ko.observable('');
            selfmi.hlpnmtlcd = ko.observable('');
            selfmi.hlpnpartno = ko.observable('');
            selfmi.hlpnclmc = ko.observable('');
            selfmi.hlpndesc = ko.observable('');
            selfmi.hlpnrecordonly = ko.observable('');

            if (selfmi.mtl.SpecId && selfmi.mtl.SpecId != null && selfmi.mtl.SpecId.value() > 0) {
                selfmi.enableheight = ko.observable(false);
                selfmi.enablewidth = ko.observable(false);
                selfmi.enabledepth = ko.observable(false);
            }
            else {
            selfmi.enableheight = ko.observable(true);
            selfmi.enablewidth = ko.observable(true);
            selfmi.enabledepth = ko.observable(true);
            }

            selfmi.visbleMntposhgt = ko.observable(false);
            selfmi.visbledefaultscheme = ko.observable(false);
            selfmi.visblemntngpltsz = ko.observable(false);

            selfmi.exception = ko.observable();
            selfmi.exceptionData = ko.observableArray();
            selfmi.enableModalCreateRvsn = ko.observable(false);
            selfmi.enableModalSaveRtPrtNbr = ko.observable(false);

            //variablecables
            selfmi.associatetbl = ko.observableArray();
            selfmi.searchtbl = ko.observableArray();
            //variablecables

            //srReplacedBy
            selfmi.searchtblsrd = ko.observableArray();
            selfmi.parentSRd = ko.observable(null);
            //srReplacedBy

            //srReplaces
            selfmi.srsPartstbl = ko.observableArray();
            //srReplaces

            //cpReplacedBy
            selfmi.parentCPd = ko.observable(null);
            //cpReplacedBy

            //cpReplaces
            selfmi.cpsPartstbl = ko.observableArray();
            //cpReplaces

            selfmi.losdbChainedIn = ko.observableArray();

            //revisionParts
            selfmi.revisionPartstbl = ko.observableArray();
            selfmi.searchtblrp = ko.observableArray();
            //revisionParts

            // High level parts
            selfmi.searchHlpn = ko.observableArray();
            selfmi.modalSize = ko.observable(0);
            selfmi.getCITHlpn = ko.observableArray();
            // selfmi.showModalHLPNMulti();
            selfmi.enableHLPNBtn = ko.observable(false);
            selfmi.enableHLPNBtnBoolSave = ko.observable(false);
            // High level parts
            selfmi.specCmdPrompt = ko.observable(false);
            selfmi.specCmdPromptNavigate = ko.observable(false);

            if (selfmi.mtl.id.value() > 0)
                selfmi.exists = true;
            else
                selfmi.exists = false;
            if (selfmi.mtl.LctnPosInd.value())
                selfmi.selectedLctnPosInd(selfmi.mtl.LctnPosInd.value());
            else
                selfmi.selectedLctnPosInd('N');

            if (selfmi.mtl.PrtNbrTypCd.value())
                selfmi.selectedPrtNbrTypCd(selfmi.mtl.PrtNbrTypCd.value());

            if (selfmi.mtl.PrdTyp.value())
                selfmi.selectedPrdTyp(selfmi.mtl.PrdTyp.value());

            if (selfmi.mtl.GgeUnt.value())
                selfmi.selectedGgeUnt(selfmi.mtl.GgeUnt.value());

            if (selfmi.mtl.PosSchm.value())
                selfmi.selectedPosSchm(selfmi.mtl.PosSchm.value());

            if (selfmi.mtl.LbrId.value())
                selfmi.selectedLbrId(selfmi.mtl.LbrId.value());
            else {
                selfmi.mtl.LbrId.value('995');
                selfmi.selectedLbrId(selfmi.mtl.LbrId.value());
            }

            if (selfmi.mtl.Apcl.value())
                selfmi.selectedApcl(selfmi.mtl.Apcl.value());

            if (selfmi.mtl.SetLgthUom.value())
                selfmi.selectedSetLgthUom(selfmi.mtl.SetLgthUom.value());

            if (selfmi.mtl.Stts.value())
                selfmi.selectedStatus(selfmi.mtl.Stts.value());

            if (selfmi.mtl.CblTypId.value())
                selfmi.selectedCableType(selfmi.mtl.CblTypId.value());

            if (selfmi.mtl.PlgInRlTyp.value())
                selfmi.selectedPlugInType(selfmi.mtl.PlgInRlTyp.value());

            if (selfmi.mtl.FtrTyp.value()) {
                for (var i = 0; i < selfmi.mtl.FtrTyp.options().length; i++) {
                    if (selfmi.mtl.FtrTyp.options()[i].text() === 'Variable Length') {
                        selfmi.mtl.FtrTyp.options().splice(i, 1);
                        i--;
                    }
                }

                selfmi.selectedFeatureType(selfmi.mtl.FtrTyp.value());
                selfmi.initialFeatureType(selfmi.mtl.FtrTyp.value());
            }
            else {
                selfmi.initialFeatureType('');
            }

            //if (selfmi.mtl.FtrTyp.value() && selfmi.mtl.CblTypId.value())
            //{
            //    if (selfmi.mtl.FtrTyp.value() === '10') {
            //        if (selfmi.mtl.CblTypId.value() == '3') {
            //            selfmi.selectedSetLgthUom('13');
            //        }
            //    }
            //}

            if (selfmi.mtl.MtlCtgry.value) {
                selfmi.selectedMaterialCategory(selfmi.mtl.MtlCtgry.value());
                if (selfmi.mtl.MtlCtgry.value() == 1) {
                    selfmi.enableHLPNBtn(true);
                } else {
                    selfmi.enableHLPNBtn(false);
                }
            }

            //if (selfmi.mtl.HvSpec.bool) {
            //    self.selectedHvSpec(true);
            //}

            //    if (selfmi.mtl.SpecId.value) {
            //        self.selectedSpecId(selfmi.mtl.SpecId.value);
            //    }
            //    if (selfmi.mtl.SpecNm.value) {
            //        self.selectedSpecName(selfmi.mtl.SpecNm.value);
            //    }

            setTimeout(function () {
                selfmi.checkMtlCtgry(selfmi.selectedMaterialCategory());
                selfmi.checkFtrTyp(selfmi.selectedFeatureType());
                selfmi.checkApcl(selfmi.selectedApcl());
                selfmi.filterLaborIds(selfmi.selectedMaterialCategory(), selfmi.selectedFeatureType(), selfmi.selectedCableType());

                if (selfmi.mtl.FtrTyp.value()) {
                    selfmi.getFeatureTypeLocationType(selfmi.mtl.FtrTyp.value());
                }

                //selfmi.setSpecButtonText();

                // 2020-01-09: mwj - added for "Pop Out" window
                selfmi.adjustForPopOutWindow();
                
            }, 2000);
        };

        MaterialItem.prototype.adjustForPopOutWindow = function () {
            if (window.location.hash.indexOf('id') === -1 ) {
                return;
            }

            var parms = new URLSearchParams(window.location.hash.split('?')[1]);
            selfmi.autoSearchId = parms.get('id');
        }
        MaterialItem.prototype.handleMaterialEditorException = function (err) {
            //var exception = JSON.parse(err.responseJSON.ExceptionMessage);
            var exception = err;

            if (exception.errCd == 101) {
                var span = document.getElementsByClassName("close")[0];
                var modal = document.getElementById('duplicateRtPrtNoModal');

                selfmi.exception = mapping.fromJS(exception);
                selfmi.exceptionData(selfmi.exception.data());

                $("#duplicateRtPrtNoModal").show();

                // When the user clicks on <span> (x), close the modal
                span.onclick = function () {
                    modal.style.display = "none";
                }
            }
        };

        MaterialItem.prototype.saveMaterialAsRevision = function () {
            var rvsn = document.getElementById("idrtprtnbrrvsntxt").value;
            var ids = $("#dupRtPrtNbrTbl .rtMtrlId");

            selfmi.mtl.MtrlId.value(ids[0].innerText);

            this.setWorkingMtlNumberValue('txtHght', selfmi.mtl.Hght);
            this.setWorkingMtlNumberValue('txtWdth', selfmi.mtl.Wdth);
            this.setWorkingMtlNumberValue('txtDpth', selfmi.mtl.Dpth);
            this.setWorkingMtlNumberValue('txtConvRt1', selfmi.mtl.ConvRt1);
            this.setWorkingMtlNumberValue('txtConvRt2', selfmi.mtl.ConvRt2);

            selfmi.mtl.Stts.value(selfmi.selectedStatus());
            selfmi.mtl.MtlCtgry.value(selfmi.selectedMaterialCategory());
            selfmi.mtl.FtrTyp.value(selfmi.selectedFeatureType());
            selfmi.mtl.LctnPosInd.value(selfmi.selectedLctnPosInd());
            selfmi.mtl.LbrId.value(selfmi.selectedLbrId());
            selfmi.mtl.SetLgthUom.value(selfmi.selectedSetLgthUom());
            selfmi.mtl.Apcl.value(selfmi.selectedApcl());
            selfmi.mtl.CblTypId.value(selfmi.selectedCableType());
            selfmi.mtl.PlgInRlTyp.value(selfmi.selectedPlugInType());

            var js = mapping.toJS(selfmi);

            js.mtl.cuid = selfMtlEdit.usr.cuid;

            if (js.mtl.Rvsn) {
                js.mtl.Rvsn.value(rvsn);
            } else {
                js.mtl.Rvsn = { "value": rvsn };
            }

            $.ajax({
                type: "POST",
                url: 'api/revisions/create',
                data: JSON.stringify(js.mtl),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: saveRevisionSuccessful,
                error: saveRevisionError,
                context: selfmi
            });

            function saveRevisionSuccessful(data, status) {
                var id = this.mtl.id.value();
                var getUrl = 'api/material/' + id + '/';
                var workToDoId = data[1];
                var specWorkToDoId = data[2];

                if (workToDoId !== '0') {
                    var helper = new reference();

                    helper.getSendToNdsStatus(id, workToDoId);
                }

                if (specWorkToDoId !== '0') {
                    var specHelper = new reference();
                    var specId = 0;

                    if (this.mtl.SpecRvsnId && this.mtl.SpecRvsnId.value() > 0) {
                        specId = this.mtl.SpecRvsnId.value();
                    } else if (this.mtl.SpecId && this.mtl.SpecId.value() > 0) {
                        specId = this.mtl.SpecId.value();
                    }

                    specHelper.getSpecificationSendToNdsStatus(specId, specWorkToDoId, this.mtl.SpecTyp.value());;
                }

                selfMtlEdit.materialSelected(this.mtl.id.value(), selfMtlEdit);

                return app.showMessage('Changes successfully saved to the database.');
            }

            function saveRevisionError(err) {
                $("#interstitial").hide();

                return app.showMessage('Unable to save changes to the database due to an internal error. If problem persists please contact your system administrator.');
            }
        };

        MaterialItem.prototype.hlpnsearchMethod = function () {
            var radio = document.getElementById('hlpnsearchFuzzy');
            if (radio.checked) {  // Fuzzy Match
                document.getElementById('hlpncdmmslabel').style.color = "black";
                document.getElementById('hlpnprodidlabel').style.color = "black";
                document.getElementById('hlpnpartnumberlabel').style.color = "black";
                document.getElementById('hlpnclmclabel').style.color = "black";
            }
            else {  // Exact Match
                document.getElementById('hlpncdmmslabel').style.color = "red";
                document.getElementById('hlpnprodidlabel').style.color = "red";
                document.getElementById('hlpnpartnumberlabel').style.color = "red";
                document.getElementById('hlpnclmclabel').style.color = "red";
            }
        };

        MaterialItem.prototype.saveMaterialWithRtPrtNo = function () {
            selfmi.mtl.RtPrtNbr.value(document.getElementById("idrtprtnbrtxt").value);

            this.setWorkingMtlNumberValue('txtHght', selfmi.mtl.Hght);
            this.setWorkingMtlNumberValue('txtWdth', selfmi.mtl.Wdth);
            this.setWorkingMtlNumberValue('txtDpth', selfmi.mtl.Dpth);
            this.setWorkingMtlNumberValue('txtConvRt1', selfmi.mtl.ConvRt1);
            this.setWorkingMtlNumberValue('txtConvRt2', selfmi.mtl.ConvRt2);

            selfmi.mtl.Stts.value(selfmi.selectedStatus());
            selfmi.mtl.MtlCtgry.value(selfmi.selectedMaterialCategory());
            selfmi.mtl.FtrTyp.value(selfmi.selectedFeatureType());
            selfmi.mtl.LctnPosInd.value(selfmi.selectedLctnPosInd());
            selfmi.mtl.LbrId.value(selfmi.selectedLbrId());
            selfmi.mtl.SetLgthUom.value(selfmi.selectedSetLgthUom());
            selfmi.mtl.Apcl.value(selfmi.selectedApcl());
            selfmi.mtl.CblTypId.value(selfmi.selectedCableType());
            selfmi.mtl.PlgInRlTyp.value(selfmi.selectedPlugInType());

            var js = mapping.toJS(selfmi);

            js.mtl.cuid = selfMtlEdit.usr.cuid;

            $.ajax({
                type: "POST",
                url: 'api/material/update',
                data: JSON.stringify(js.mtl),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: updateSuccessful,
                error: updateError,
                context: selfmi
            });

            function updateSuccessful(data, status) {
                var id = this.mtl.id.value();
                var getUrl = 'api/material/' + id + '/';
                var workToDoId = data[2];
                var specWorkToDoId = data[3];

                if (workToDoId !== '0') {
                    var helper = new reference();

                    helper.getSendToNdsStatus(id, workToDoId);
                }

                if (specWorkToDoId !== '0') {
                    var specHelper = new reference();
                    var specId = 0;

                    if (this.mtl.SpecRvsnId && this.mtl.SpecRvsnId.value() > 0) {
                        specId = this.mtl.SpecRvsnId.value();
                    } else if (this.mtl.SpecId && this.mtl.SpecId.value() > 0) {
                        specId = this.mtl.SpecId.value();
                    }

                    specHelper.getSpecificationSendToNdsStatus(specId, specWorkToDoId, this.mtl.SpecTyp.value());
                }

                selfMtlEdit.materialSelected(this.mtl.id.value(), selfMtlEdit);

                return app.showMessage('Changes successfully saved to the database.');
            }

            function updateError(err) {
                $("#interstitial").hide();

                if (err.responseJSON.ExceptionType === 'CenturyLink.Network.Engineering.Material.Editor.Utility.MaterialEditorException') {
                    return app.showMessage("Root Part Number exists. Please enter a new value.");
                } else {
                    return app.showMessage('Unable to save changes to the database due to an internal error. If problem persists please contact your system administrator.');
                }
            }
        };

        MaterialItem.prototype.enableCreateRevision = function () {
            if (document.getElementById("idrtprtnbrrvsntxt").value !== '') {
                selfmi.enableModalCreateRvsn(true);
            } else {
                selfmi.enableModalCreateRvsn(false);
            }
        };

        MaterialItem.prototype.enableSaveRtPrtNbr = function () {
            if (document.getElementById("idrtprtnbrtxt").value !== '') {
                selfmi.enableModalSaveRtPrtNbr(true);
            } else {
                selfmi.enableModalSaveRtPrtNbr(false);
            }
        };

        MaterialItem.prototype.setWorkingMtlNumberValue = function (inputName, workingMtlField) {
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

        MaterialItem.prototype.setSpecButtonText = function (ftrTypId) {
            var btn = document.getElementById("specNameNavigateBtn");

            if (btn) {
                if (selfmi.mtl.MtrlId.value() !== '0' && (ftrTypId == 1 || ftrTypId == 2 || ftrTypId == 5 || ftrTypId == 6 || ftrTypId == 7)) {
                    if ((selfmi.mtl.SpecId && selfmi.mtl.SpecId.value() > 0) && selfmi.mtl.FtrTyp.value() == ftrTypId) {
                        btn.innerHTML = "View Specification";
                        btn.disabled = false;
                        selfmi.specCmdPrompt(false);
                    } else if (selfmi.mtl.FtrTyp.value() == ftrTypId) {
                        btn.innerHTML = "Create Specification";
                        btn.disabled = false;
                        selfmi.specCmdPrompt(false);
                    } else {
                        btn.innerHTML = "Create Specification";
                        btn.disabled = false;
                        selfmi.specCmdPrompt(true);
                    }
                } else if (selfmi.mtl.MtrlId.value() === '0' && (ftrTypId == 1 || ftrTypId == 2 || ftrTypId == 5 || ftrTypId == 6 || ftrTypId == 7)) {
                    btn.innerHTML = "Create Specification";
                    btn.disabled = true;
                    selfmi.specCmdPrompt(false);
                } else {
                    btn.innerHTML = "Create Specification";
                    btn.disabled = true;
                    selfmi.specCmdPrompt(false);
                }
            }
        };

        MaterialItem.prototype.changeCableType = function (model, event) {
            selfMtlEdit.saveValidation(true);
            selfmi.selectedCableType(event.target.value);
            selfmi.filterLaborIds(selfmi.selectedMaterialCategory(), selfmi.selectedFeatureType(), selfmi.selectedCableType());

            if ($("#ftrTypDropdown option:selected").text() === 'Bulk') {
                if (($("#cblTypDropdown option:selected").text() === 'Power'
                    || $("#cblTypDropdown option:selected").text() === 'Copper'
                    || $("#cblTypDropdown option:selected").text() === 'Coaxial')) {
                    $("#setLgthUomDropdown").val("13");
                    selfmi.selectedSetLgthUom("13");
                }
                else {
                    $("#setLgthUomDropdown").val("15");
                    selfmi.selectedSetLgthUom("15");
                }
            }
            if ($("#ftrTypDropdown option:selected").text() === 'Connectorized/Set Length') {
                if (($("#cblTypDropdown option:selected").text() === 'Power'
                    || $("#cblTypDropdown option:selected").text() === 'Copper'
                    || $("#cblTypDropdown option:selected").text() === 'Coaxial')) {
                    $("#setLgthUomDropdown").val("13");
                    selfmi.selectedSetLgthUom("13");
                }
                else {
                    $("#setLgthUomDropdown").val("15");
                    selfmi.selectedSetLgthUom("15");
                }
            }
            if ($("#ftrTypDropdown option:selected").text() === 'Cable') {
                if (($("#cblTypDropdown option:selected").text() === 'Power'
                    || $("#cblTypDropdown option:selected").text() === 'Copper'
                    || $("#cblTypDropdown option:selected").text() === 'Coaxial')) {
                    $("#setLgthUomDropdown").val("13");
                    selfmi.selectedSetLgthUom("13");
                }
                else {
                    $("#setLgthUomDropdown").val("15");
                    selfmi.selectedSetLgthUom("15");
                }
            }
        };

        //UI validation
        MaterialItem.prototype.changeMtlCtgry = function (model, event) {
            selfMtlEdit.saveValidation(true);
            selfmi.checkMtlCtgry(event.target.value);
            selfmi.filterLaborIds(selfmi.selectedMaterialCategory(), selfmi.selectedFeatureType(), selfmi.selectedCableType());
        };

        MaterialItem.prototype.checkMtlCtgry = function (mtlCtgryVal) {
            if (mtlCtgryVal == 1 && selfmi.enableHLPNBtn()) {
                // When High Level Part is selected
                $("#hlpnBtn").show();
                $('#hlpnBtn').prop('disabled', false);
            } else if (mtlCtgryVal == 1 && !selfmi.enableHLPNBtn()) {
                $("#hlpnBtn").show();
                $('#hlpnBtn').prop('disabled', false);
                selfmi.enableHLPNBtnBoolSave(true);
            } else {
                $("#hlpnBtn").hide();
            }

            if (document.getElementById("ftrTypDropdown")) {
                if (mtlCtgryVal == "3") {
                    document.getElementById('ftrTypDropdown').disabled = false;
                    //document.getElementById("txtSpecNm").disabled = false;
                    //document.getElementById("txtSpecNm").required = true;
                    //document.getElementById('ftrTypDropdown').value = selfmi.mtl.FtrTyp.value();
                    //selfmi.selectedFeatureType(selfmi.mtl.FtrTyp.value());
                    //selfmi.checkFtrTyp(selfmi.selectedFeatureType());
                }
                else {
                    document.getElementById("txtSpecNm").required = false;
                    document.getElementById("txtSpecNm").disabled = true;
                    document.getElementById("txtSpecNm").value = '';
                    selfmi.mtl.SpecNm.value('');
                    document.getElementById('ftrTypDropdown').disabled = true;
                    document.getElementById('btnassociate').disabled = true;
                    selfmi.selectedFeatureType('');
                    selfmi.checkFtrTyp(selfmi.selectedFeatureType());
                }
            }
        };

        MaterialItem.prototype.getFeatureTypeLocationType = function (ftrTypId) {
            var helper = new reference();
            var results = helper.getFeatureTypeLocatableType(ftrTypId).then(function (results) {
                if (results === null || results === '')
                { }
                else if (results === 'Locatable') {
                    if (document.getElementById("txtSpecNm")) {
                        if (ftrTypId == 1 || ftrTypId == 2 || ftrTypId == 5 || ftrTypId == 6 || ftrTypId == 7) {
                            document.getElementById("txtSpecNm").disabled = true;
                            document.getElementById("txtSpecNm").required = true;
                        } else {
                            document.getElementById("txtSpecNm").disabled = false;
                            document.getElementById("txtSpecNm").required = true;
                        }
                    }
                }
                else if (results === 'Non Locatable') {
                    if (document.getElementById("txtSpecNm")) {
                        document.getElementById("txtSpecNm").required = false;
                        document.getElementById("txtSpecNm").disabled = true;
                        document.getElementById("txtSpecNm").value = '';

                        selfmi.mtl.SpecNm.value('');
                    }
                }
            });
        };

        MaterialItem.prototype.changeFtrTyp = function (model, event) {
            selfMtlEdit.saveValidation(true);
            selfmi.checkFtrTyp(event.target.value);

            selfmi.getFeatureTypeLocationType(event.target.value);
            selfmi.filterLaborIds(selfmi.selectedMaterialCategory(), selfmi.selectedFeatureType(), selfmi.selectedCableType());
            //if ($("#ftrTypDropdown option:selected").text() === 'Bulk') {
            //    selfmi.mtl.SetLgth.value(0);
            //    if (($("#cblTypDropdown option:selected").text() === 'Power'
            //        || $("#cblTypDropdown option:selected").text() === 'Copper'
            //        || $("#cblTypDropdown option:selected").text() === 'Coaxial')) {
            //        $("#setLgthUomDropdown").val("13");
            //    }
            //    else {
            //        $("#setLgthUomDropdown").val("15");
            //    }
            //}
            //if ($("#ftrTypDropdown option:selected").text() === 'Connectorized/Set Length') {
            //    if (($("#cblTypDropdown option:selected").text() === 'Power'
            //        || $("#cblTypDropdown option:selected").text() === 'Copper'
            //        || $("#cblTypDropdown option:selected").text() === 'Coaxial')) {
            //        $("#setLgthUomDropdown").val("13");
            //    }
            //    else {
            //        $("#setLgthUomDropdown").val("15");
            //    }
            //}

            if (selfmi.initialFeatureType() != ""
                || (selfmi.initialFeatureType() == '6' && event.target.value == '5')
                || (selfmi.initialFeatureType() == '5' && event.target.value == '6')) {
                app.showMessage('The Feature Type has changed.  Please note that a Feature Type change has the possibility of affecting a Template in NDS.');
            }
        };

        MaterialItem.prototype.checkFtrTyp = function (ftrTypVal) {
            if (document.getElementById("txtSetLgth")) {
                if (ftrTypVal == "9") {
                    document.getElementById('txtSetLgth').value = selfmi.mtl.SetLgth.value();
                    document.getElementById('setLgthUomDropdown').value = selfmi.mtl.SetLgthUom.value();

                    if (selfmi.mtl.MtrlId.value() !== "0") {
                        document.getElementById('btnassociate').disabled = false;
                    }

                    document.getElementById('txtSetLgth').disabled = false;
                    document.getElementById('setLgthUomDropdown').disabled = false;
                    document.getElementById('cblTypDropdown').disabled = false;
                    document.getElementById('cblTypDropdown').required = true;
                    document.getElementById('plginTypDropdown').disabled = true;
                    document.getElementById('plginTypDropdown').required = false;
                }
                else if (ftrTypVal == "10") {
                    document.getElementById('btnassociate').disabled = true;
                    document.getElementById('txtSetLgth').disabled = true;
                    //document.getElementById('setLgthUomDropdown').disabled = true;
                    document.getElementById('setLgthUomDropdown').disabled = false;  // per Jesse Olave, INC1908748
                    document.getElementById('txtSetLgth').value = 0;
                    document.getElementById('setLgthUomDropdown').value = 15;
                    document.getElementById('cblTypDropdown').disabled = false;
                    document.getElementById('cblTypDropdown').required = true;
                    document.getElementById('plginTypDropdown').disabled = true;
                    document.getElementById('plginTypDropdown').required = false;
                }
                else if (ftrTypVal == "11") {
                    document.getElementById('btnassociate').disabled = true;
                    document.getElementById('txtSetLgth').disabled = true;
                    //document.getElementById('setLgthUomDropdown').disabled = true;
                    document.getElementById('txtSetLgth').value = -1;
                    document.getElementById('setLgthUomDropdown').value = 15;
                    document.getElementById('cblTypDropdown').disabled = false;
                    document.getElementById('cblTypDropdown').required = true;
                    document.getElementById('plginTypDropdown').disabled = true;
                    document.getElementById('plginTypDropdown').required = false;
                }
                else if (ftrTypVal == "30") {
                    document.getElementById('btnassociate').disabled = true;
                    document.getElementById('txtSetLgth').disabled = true;
                    //document.getElementById('setLgthUomDropdown').disabled = true;
                    document.getElementById('setLgthUomDropdown').disabled = false;  // per Jesse Olave, INC1908748
                    document.getElementById('txtSetLgth').value = '';
                    //document.getElementById('setLgthUomDropdown').value = '';
                    document.getElementById('cblTypDropdown').disabled = false;
                    document.getElementById('cblTypDropdown').required = true;
                    document.getElementById('plginTypDropdown').disabled = true;
                    document.getElementById('plginTypDropdown').required = false;
                }
                else if (ftrTypVal == "8") {
                    document.getElementById('btnassociate').disabled = true;
                    document.getElementById('txtSetLgth').disabled = true;
                    document.getElementById('setLgthUomDropdown').disabled = true;
                    document.getElementById('txtSetLgth').value = '';
                    document.getElementById('setLgthUomDropdown').value = '';
                    document.getElementById('cblTypDropdown').disabled = true;
                    document.getElementById('cblTypDropdown').required = false;
                    document.getElementById('plginTypDropdown').disabled = false;
                    document.getElementById('plginTypDropdown').required = true;
                }
                else {
                    document.getElementById('btnassociate').disabled = true;
                    document.getElementById('txtSetLgth').disabled = true;
                    document.getElementById('setLgthUomDropdown').disabled = true;
                    document.getElementById('txtSetLgth').value = '';
                    document.getElementById('setLgthUomDropdown').value = '';
                    document.getElementById('cblTypDropdown').disabled = true;
                    document.getElementById('cblTypDropdown').required = false;
                    document.getElementById('plginTypDropdown').disabled = true;
                    document.getElementById('plginTypDropdown').required = false;
                }

                selfmi.setSpecButtonText(ftrTypVal);
            }
        };

        MaterialItem.prototype.changeApcl = function (model, event) {
            selfMtlEdit.saveValidation(true);
            selfmi.checkApcl(event.target.value);
            return true;
        };

        MaterialItem.prototype.checkApcl = function (apclVal) {
            if (document.getElementById("txtApcl")) {
                //if (apclVal.toUpperCase() === "SR" || selfmi.mtl.RplcsPrtNbr !== undefined || selfmi.mtl.RplcdByPrtNbr !== undefined) {
                if (selfmi.mtl.MtrlId.value() !== "0" && (apclVal.toUpperCase() === "SR" || selfmi.mtl.RplcsPrtNbr !== undefined)) {
                    document.getElementById('btnsr').disabled = false;
                }
                else { document.getElementById('btnsr').disabled = true; }
            }

        };

        MaterialItem.prototype.NumDecimal = function (mp, event) {
            selfMtlEdit.saveValidation(true);
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

        //UI validation

        //UI functions
        MaterialItem.prototype.enableSave = function () {
            selfMtlEdit.saveValidation(true);
        };

        MaterialItem.prototype.getfirstword = function (x) {
            selfMtlEdit.saveValidation(true);
            var helper = require('Utility/helper');
            var eqpCls = helper.deriveEquipmentClass(document.getElementById("txtCtlgDesc").value, document.getElementById("txtEqptCls").value)

            document.getElementById("txtEqptCls").value = eqpCls;
            x.mtl.EqptCls.value(eqpCls);
        };

        //MaterialItem.prototype.checkLocatable = function (model, event) {
        //    $("#interstitial").css("height", "100%");
        //    $("#interstitial").show();

        //    var self = this;
        //    var helper = new reference();
        //    var selectedValue = event.target.value;
        //    var results = helper.getFeatureTypeLocatableType(selectedValue).then(function (results) {
        //        if (results === null || results === '')
        //        { }
        //        else if (results === 'Locatable') {
        //            document.getElementById("txtSpecNm").disabled = false;
        //            document.getElementById("txtSpecNm").required = true;
        //        }
        //        else if (results === 'Non Locatable') {
        //            document.getElementById("txtSpecNm").required = false;
        //            document.getElementById("txtSpecNm").disabled = true;
        //            document.getElementById("txtSpecNm").value = '';
        //            self.mtl.SpecNm.value('');
        //        }

        //        $("#interstitial").hide();
        //    });
        //};

        MaterialItem.prototype.checkEnterVC = function (root, event) {
            if (event.keyCode === 13) {
                console.log("Enter");
                selfmi.searchmtl(root);
            }
            else { return true; }
        };

        MaterialItem.prototype.checkEnterSR = function (root, event) {
            if (event.keyCode === 13) {
                console.log("Enter");
                selfmi.searchmtlSR(root);
            }
            else { return true; }
        };

        MaterialItem.prototype.checkEnterRP = function (root, event) {
            if (event.keyCode === 13) {
                console.log("Enter");
                selfmi.searchmtlRP(root);
            }
            else { return true; }
        };

        MaterialItem.prototype.attached = function (view) {
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
                        selfMtlEdit.workingMaterial().mtl.Mfg.value(selected.label);
                        selfMtlEdit.workingMaterial().mtl.MfgId.value(selected.value);
                        app.showMessage('Changing the CLMC could have downstream impacts on a Template.  Please check NDS if this is going to cause a Template issue.');

                        getDescription(selected.label);
                    }
                },
                select: function (event, ui) {
                    selfMtlEdit.workingMaterial().mtl.Mfg.value(ui.item.label);

                    getDescription(ui.item.label);
                }
            });

            function getDescription(label) {
                var getUrl = 'api/reference/autocomplete/mfgDesc/' + label;

                $.ajax({
                    type: "GET",
                    url: getUrl,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (data) {
                        var obj = JSON.parse(data);

                        if (obj && obj[0] && obj[0].value) {
                            selfMtlEdit.workingMaterial().mtl.MfgDesc.value(obj[0].value)
                        }
                    },
                    //error: getError,
                    context: this
                });
            };
        };
        //UI functions


        MaterialItem.prototype.save = function () {
            var self = this;
            var elements = document.querySelectorAll('#txtAccntCd,#statusDropdown,#ftrTypDropdown,#txtApcl');

            for (var i = elements.length; i--;) {
                elements[i].addEventListener('invalid', function () {
                    this.scrollIntoView(false);
                });
            }

            console.log('Save');
        };

        function errorFunc(data, status) {
            $("#interstitial").hide();
            app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
        };

        MaterialItem.prototype.reset = function () {
            selfmi.searchtblsrd(false);
            selfmi.searchtbl(false);
            selfmi.searchtblrp(false);
            selfmi.searchtblsrd(false);
            selfmi.saveSRandAM(false);
            document.getElementById("idcdmmsidmodal").value = "";
            document.getElementById("materialcodemodal").value = "";
            document.getElementById("partnumbermodal").value = "";
            document.getElementById("clmcmodal").value = "";
            document.getElementById("catlogdsptn").value = "";
            document.getElementById("idcdmmsrp").value = "";
            document.getElementById("materialcoderp").value = "";
            document.getElementById("partnumberrp").value = "";
            document.getElementById("clmcrp").value = "";
            document.getElementById("catlogdsptnrp").value = "";
            document.getElementById("idcdmmssrd").value = "";
            document.getElementById("materialcodesrd").value = "";
            document.getElementById("partnumbersrd").value = "";
            document.getElementById("clmcsrd").value = "";
            document.getElementById("catlogdsptnsrd").value = "";

        };

        //Variable Cable
        MaterialItem.prototype.associatecablepopup = function (model, event) {
            selfmi.reset();
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            var modal = document.getElementById('variableCableModal');
            var btn = document.getElementById("btnassociate");
            var span = document.getElementsByClassName("close")[0];
            var idcdmms = document.getElementById("cdmmsidmain").innerText;
            $.ajax({
                type: "GET",
                url: 'api/variablecable/' + idcdmms,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successAssociate,
                error: errorFunc,
                context: selfmi,
                async: false
            });
            function successAssociate(data, status) {
                if (data === '{}')
                { selfmi.associatetbl(false); }
                else {
                    var results = JSON.parse(data);

                    selfmi.associatetbl(results);
                }
                // When the user clicks the button, open the modal 
                btn.onclick = function () {
                    modal.style.display = "block";
                }
                $("#interstitial").hide();
                $("#variableCableModal").show();

                // When the user clicks on <span> (x), close the modal
                span.onclick = function () {
                    modal.style.display = "none";
                }
            }
        };

        MaterialItem.prototype.searchmtl = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfmi.searchtbl(false);
            var mtlid = document.getElementById("idcdmmsidmodal").value;
            var mtlcode = $("#materialcodemodal").val();
            var partnumb = $("#partnumbermodal").val();
            var clmc = $("#clmcmodal").val();
            var caldsp = $("#catlogdsptn").val();
            var src = "mtlInv";
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
                context: selfmi,
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
                    selfmi.searchtbl(results);

                    $("#interstitial").hide();
                }

            }
            $(document).ready(function () {
                $('input.chkbxsearch').on('change', function () {
                    $('input.chkbxsearch').not(this).prop('checked', false);
                });

            });
        };

        MaterialItem.prototype.savemodalVC = function (rootContext) {
            $("#interstitial").show();
            var SelectedTableRow = ko.observable("");
            var saveop;
            $("#searchresults .chkbxsearch").each(function () {
                if (this.checked == true) {
                    SelectedTableRow(this.value);
                }
            });
            if (SelectedTableRow() == '') {
                $("#associatedcables .chktablasc").each(function () {
                    if (this.checked == true) {
                        SelectedTableRow(this.value);
                    }
                });
                if (SelectedTableRow() == '')
                { SelectedTableRow('0'); }
            }
            var idcdmms = document.getElementById("cdmmsidmain").innerText;
            saveop = [SelectedTableRow()];
            var url = 'api/variablecable/update/mtlInv/' + idcdmms;
            http.post(url, saveop).then(function (response) {
                $("#interstitial").hide();
                console.log(response);
                if (response.indexOf('Success') !== -1) {
                    app.showMessage('Successfully saved');
                    $("#variableCableModal").hide();
                }
                else {
                    $(".saveErrorVC").show();
                    setTimeout(function () { $(".saveErrorVC").hide() }, 5000);
                }
            },
            function (error) {
                $("#interstitial").hide();
                $(".saveErrorVC").show();
                setTimeout(function () { $(".saveErrorVC").hide() }, 5000);
            });
        };

        MaterialItem.prototype.cancelmodalVC = function (modal, event) {
            $("#variableCableModal").hide();
        };
        //Variable Cable

        //SRparts
        MaterialItem.prototype.srpartspopup = function () {
            selfmi.reset();
            selfmi.searchtblsrd(false);
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            var modal;
            var span;
            var apiURL;
            var btn = document.getElementById("btnsr");
            var idcdmms = document.getElementById("cdmmsidmain").innerText;

            apiURL = 'api/replacement/' + idcdmms + '/S';

            if (selfmi.mtl.RplcsPrtNbr !== undefined) {
                modal = document.getElementById('replacesModal');
                span = document.getElementsByClassName("closesr")[0];
            }
            else {
                modal = document.getElementById('replacedByModal');
                span = document.getElementsByClassName("closesrd")[0];
            }


            $.ajax({
                type: "GET",
                url: apiURL,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successAssociatesr,
                error: errorFunc,
                context: selfmi,
                async: false
            });
            function successAssociatesr(data, status) {
                var results = JSON.parse(data);
                if (selfmi.mtl.RplcsPrtNbr !== undefined) {
                    if (data === '{}') { selfmi.srsPartstbl(false); }
                    else { selfmi.srsPartstbl(results); }
                }
                else {
                    if (data === '{}') { selfmi.parentSRd(false); }
                    else { selfmi.parentSRd(results[0]); }
                }

                $("#interstitial").hide();
                // When the user clicks the button, open the modal 
                btn.onclick = function () {
                    modal.style.display = "block";
                }
                modal.style.display = "block";
                // When the user clicks on <span> (x), close the modal
                span.onclick = function () {
                    modal.style.display = "none";
                }
            }
        };

        MaterialItem.prototype.searchmtlSR = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfmi.searchtblsrd(false);
            var maincdmms = document.getElementById("cdmmsidmain").innerText;
            var mtlid = document.getElementById("idcdmmssrd").value;
            var mtlcode = $("#materialcodesrd").val();
            var partnumb = $("#partnumbersrd").val();
            var Clmc = $("#clmcsrd").val();
            var caldsp = $("#catlogdsptnsrd").val();
            var searchJSON = {
                PrdctId: mtlcode, PrtNo: partnumb, MtlDesc: caldsp, clmc: Clmc, cdmmsid: mtlid
            };

            $.ajax({
                type: "GET",
                url: 'api/material/searchall',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successSearch,
                error: errorFunc,
                context: selfmi,
                async: false
            });

            function successSearch(data, status) {
                if (data === 'no_results') {
                    $("#interstitial").hide();
                    $(".NoRecordsrd").show();

                    setTimeout(function () {
                        $(".NoRecordsrd").hide()
                    }, 5000);
                } else {
                    var results = JSON.parse(data);
                    var finalResults = [];

                    for (var i = 0; i < results.length; i++) {
                        if (results[i].Attributes.material_item_id.value !== maincdmms && results[i].Attributes.mtrl_id.value !== '0') {
                            finalResults.push(results[i]);
                            //break;
                        }
                    }

                    if (finalResults.length > 0) {
                        selfmi.searchtblsrd(finalResults);
                        $("#interstitial").hide();
                    }
                    else {
                        $("#interstitial").hide();
                        $(".NoRecordsrd").show();

                        setTimeout(function () {
                            $(".NoRecordsrd").hide()
                        }, 5000);
                    }
                }
            }
            $(document).ready(function () {
                $('input.chkbxsearchsrd').on('change', function () {
                    $('input.chkbxsearchsrd').not(this).prop('checked', false);
                });
            });
        };

        MaterialItem.prototype.savemodalsrs = function (rootContext) {
            var saveop = '';
            var sr = this;
            var checkBoxes = $("#srsPartsTable .chktablsrs");
            var ids = $("#srsPartsTable .idtablsrs");
            var comments = $("#srsPartsTable .cmnttablsrs");
            var removedChildIds = [];
            //  $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            sr.selectedrowslist = ko.observableArray();
            for (var i = 0; i < checkBoxes.length; i++) {
                if (checkBoxes[i].checked == true) {
                    sr.selectedrowslist.push({ id: ids[i].innerText, cmnt: comments[i].value });
                } else {
                    removedChildIds.push(ids[i].innerText);
                }
            }
            saveop = mapping.toJSON(sr.selectedrowslist);
            var idcdmms = document.getElementById("cdmmsidmain").innerText;
            var apiurl = 'api/replacement/update/';
            if (rootContext.mtl.RplcsPrtNbr !== undefined) {
                apiurl = apiurl + 'children/' + idcdmms + '/S';
            }
            else {
                apiurl = apiurl + 'parent/' + idcdmms + '/S';
            }
            http.post(apiurl, saveop).then(function (response) {
                console.log(response);
                if (response === 'success') {
                    $("#replacesModal").hide();
                    selfmi.srpartspopup();
                    //Added to get the Sap material APCL to Active material
                    for (var child = 0; child < removedChildIds.length; child++) {
                        var url = 'api/material/' + removedChildIds[child] + '/';
                        http.get(url + 'sap').then(function (sapresponse) {
                            var sapValue = "";
                            if (sapresponse === "{}") {
                                sapValue = "";
                                $("#interstitial").hide();
                            } else {
                                var sapJs = JSON.parse(sapresponse);
                                sapValue = sapJs.Apcl.value;
                                http.get(url).then(function (activeResponse) {
                                    if (activeResponse === "{}") {
                                        $("#interstitial").hide();
                                    }
                                    else {
                                        var activeResponse = new MaterialItem(activeResponse);
                                        activeResponse.mtl.Apcl.value(sapValue);
                                        var js = mapping.toJS(activeResponse);
                                        js.mtl.cuid = selfMtlEdit.usr.cuid;
                                        $.ajax({
                                            type: "POST",
                                            url: 'api/material/update',
                                            data: JSON.stringify(js.mtl),
                                            contentType: "application/json; charset=utf-8",
                                            dataType: "json",
                                            success: updateSuccessful,
                                            error: updateError
                                        });
                                        function updateSuccessful(data, status) {
                                            //Show msg here.
                                        }
                                        function updateError() {
                                            //Show msg here.
                                        }
                                    }
                                });
                            }
                        });
                    }
                    $("#interstitial").hide();
                }
                else {
                    $("#interstitial").hide();
                    $(".saveErrorSRS").show();
                    setTimeout(function () { $(".saveErrorSRS").hide() }, 5000);
                }
            },
            function (error) {
                $("#interstitial").hide();
                $(".saveErrorSRS").show();
                setTimeout(function () { $(".saveErrorSRS").hide() }, 5000);
            });

        };


        MaterialItem.prototype.saveCloseModalsrd = function (rootContext) {
            selfmi.saveSRandAM(true);
            selfmi.savemodalsrd(rootContext);
        };

        //MaterialItem.prototype.saveBeforesrparts = function () {
        //    self.save('saveSRPartsBefore');
        //};

        MaterialItem.prototype.savemodalsrd = function (rootContext) {
            var sr = this;
            var checkBoxes = $("#searchresultssrd .chkbxsearchsrd");
            var ids = $("#searchresultssrd .idtablsrd");
            var comments = $("#searchresultssrd .cmnttablsrd");

            sr.selectedrowslist = ko.observableArray();

            for (var i = 0; i < checkBoxes.length; i++) {
                if (checkBoxes[i].checked == true) {
                    sr.selectedrowslist.push({ id: ids[i].innerText, cmnt: comments[i].value });
                }
            }

            saveop = mapping.toJSON(sr.selectedrowslist);
            var idcdmms = document.getElementById("cdmmsidmain").innerText;

            //[{"id":"152896", "cmnt": "test"}] 
            var apiurl = 'api/replacement/update/';

            if (rootContext.mtl.RplcsPrtNbr !== undefined) {
                apiurl = apiurl + 'children/' + idcdmms + '/S';
            }
            else {
                apiurl = apiurl + 'parent/' + idcdmms + '/S';
            }


            http.post(apiurl, saveop).then(function (response) {
                console.log(response);
                if (response === 'success') {
                    $("#replacedByModal").hide();
                    $("#interstitial").hide();
                    if (selfmi.saveSRandAM() === true)
                    { selfMtlEdit.save(); }
                    else {
                        selfmi.srpartspopup();
                        //document.getElementById("savemodalsrdbtn").disabled = true;
                    }
                }
                else {
                    $("#interstitial").hide();
                    $(".saveErrorSRD").show();
                    //document.getElementById("savemodalsrdbtn").disabled = false;
                    setTimeout(function () { $(".saveErrorSRD").hide() }, 5000);
                }
            }, function (error) {
                $("#interstitial").hide();
                $(".saveErrorSRD").show();
                //document.getElementById("savemodalsrdbtn").disabled = false;
                setTimeout(function () { $(".saveErrorSRD").hide() }, 5000);
            });


        };

        MaterialItem.prototype.cancelmodalsrs = function (modal, event) {
            $("#replacesModal").hide();
        };

        MaterialItem.prototype.cancelmodalsrd = function (modal, event) {
            document.getElementById('replacedByModal').style.display = "none";
            $("#replacedByModal").hide();
        };
        //SRparts

        //Chaining
        MaterialItem.prototype.cppartspopup = function (obj, event) {
            selfmi.reset();
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            var modal;
            var span;
            var apiURL;
            var cdmmsResults;
            var btn = document.getElementById("btn_chaining");
            var idcdmms = document.getElementById("cdmmsidmain").innerText;
            apiURL = 'api/replacement/' + idcdmms + '/C';

            $.ajax({
                type: "GET",
                url: apiURL,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successAssociateCP,
                error: errorFunc,
                context: selfmi,
                async: false
            });

            function successAssociateCP(data, status) {
                //var results = JSON.parse(data);
                var chainedUrl = 'api/replacement/chained/' + selfmi.mtl.MtrlId.value();

                cdmmsResults = JSON.parse(data);

                $.ajax({
                    type: "GET",
                    url: chainedUrl,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successGetChained,
                    error: errorFunc,
                    context: selfmi,
                    async: false
                });

                //if (results.length > 0) {
                //    if (results[0].Attributes.is_child !== undefined) {
                //        if (results[0].Attributes.is_child.value === 'N') {
                //            modal = document.getElementById('CPreplacedByModal');
                //            span = document.getElementsByClassName("closecpd")[0];
                //            selfmi.parentCPd(results[0]);
                //        }
                //        else {
                //            modal = document.getElementById('CPreplacesModal');
                //            span = document.getElementsByClassName("closecp")[0];
                //            selfmi.cpsPartstbl(results);
                //        }
                //    }
                //    else {
                //        modal = document.getElementById('CPreplacedByModal');
                //        span = document.getElementsByClassName("closecpd")[0];
                //        selfmi.parentCPd(false);
                //    }
                //}
                //else {
                //    modal = document.getElementById('CPreplacedByModal');
                //    span = document.getElementsByClassName("closecpd")[0];
                //    selfmi.parentCPd(false);
                //}

                //$("#interstitial").hide();
                //// When the user clicks the button, open the modal 
                //btn.onclick = function () {
                //    modal.style.display = "block";
                //}

                //modal.style.display = "block";
                //// When the user clicks on <span> (x), close the modal
                //span.onclick = function () {
                //    modal.style.display = "none";
                //}
            }

            function successGetChained(data, status) {
                var chainedResults = JSON.parse(data);

                if (chainedResults.length > 0)
                    selfmi.losdbChainedIn(chainedResults);

                if (cdmmsResults.length > 0) {
                    if (cdmmsResults[0].Attributes.is_child !== undefined) {
                        if (cdmmsResults[0].Attributes.is_child.value === 'N') {
                            modal = document.getElementById('CPreplacedByModal');
                            span = document.getElementsByClassName("closecpd")[0];
                            selfmi.parentCPd(cdmmsResults[0]);
                        }
                        else {
                            modal = document.getElementById('CPreplacesModal');
                            span = document.getElementsByClassName("closecp")[0];
                            selfmi.cpsPartstbl(cdmmsResults);
                        }
                    }
                    else {
                        modal = document.getElementById('CPreplacedByModal');
                        span = document.getElementsByClassName("closecpd")[0];
                        selfmi.parentCPd(false);
                    }
                }
                else {
                    modal = document.getElementById('CPreplacedByModal');
                    span = document.getElementsByClassName("closecpd")[0];
                    selfmi.parentCPd(false);
                }

                $("#interstitial").hide();
                // When the user clicks the button, open the modal 
                btn.onclick = function () {
                    modal.style.display = "block";
                }

                modal.style.display = "block";
                // When the user clicks on <span> (x), close the modal
                span.onclick = function () {
                    modal.style.display = "none";
                }
            }
        };
        //Chaining

        //Revision 
        MaterialItem.prototype.revisionparts = function () {
            selfmi.reset();
            selfmi.revisionPartstbl(false);
            selfmi.searchtblrp(false);
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            var modal = document.getElementById('revisionPartsModal');
            var btn = document.getElementById("btn_revision");
            var span = document.getElementsByClassName("closerp")[0];
            var idcdmms = selfmi.mtl.MtrlId.value();
            var mtlctgy = selfmi.mtl.MtlCtgry.value();
            var callurl = 'api/revisions/' + idcdmms + '/' + mtlctgy;

            if (selfmi.mtl.FtrTyp.value()) {
                callurl = callurl + '/' + selfmi.mtl.FtrTyp.value();
            }

            $.ajax({
                type: "GET",
                url: callurl,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successRevisionParts,
                error: errorFunc,
                context: selfmi,
                async: false
            });
            function successRevisionParts(data, status) {

                if (data === '{}') {
                    selfmi.revisionPartstbl(false);
                }
                else {
                    var results = JSON.parse(data);
                    //if (results.length===1) {
                    //    document.getElementById('deletecp').disabled = true;

                    //}
                    selfmi.revisionPartstbl(results);
                }
                $(document).ready(function () {
                    $('input.chkbx_baservsn').on('change', function () {
                        $('input.chkbx_baservsn').not(this).prop('checked', false);
                    });
                    $('input.chkbx_currvsn').on('change', function () {
                        $('input.chkbx_currvsn').not(this).prop('checked', false);
                    });
                });
                // When the user clicks the button, open the modal 
                btn.onclick = function () {
                    $("#revisionPartsModal").show().draggable();
                    modal.style.display = "block";
                }
                $("#interstitial").hide();
                modal.style.display = "block";
                // When the user clicks on <span> (x), close the modal
                span.onclick = function () {
                    modal.style.display = "none";
                }

                // When the user clicks anywhere outside of the modal, close it
                window.onclick = function (event) {
                    if (event.target == modal) {
                        modal.style.display = "none";
                    }
                }
            }
        };

        MaterialItem.prototype.searchmtlRP = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            selfmi.searchtblrp(false);

            var mtlidRoot = selfmi.mtl.MtrlId.value();
            var mtlid = $("#idcdmmsrp").val();
            var mtlcode = $("#materialcoderp").val();
            var partnumb = $("#partnumberrp").val();
            var Clmc = $("#clmcrp").val();
            var caldsp = $("#catlogdsptnrp").val();
            if (mtlid === '' && mtlcode === '' && partnumb === '' && Clmc === '' && caldsp === '') {
                $("#interstitial").hide();
                app.showMessage('You must enter something to search on!');
                return;
            }
            var searchJSON = {
                product_id: mtlcode, mfg_part_no: partnumb, mat_desc: caldsp, mfg_id: Clmc, material_item_id: mtlid
            };

            $.ajax({
                type: "GET",
                url: 'api/revisions/search/' + mtlidRoot,
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successSearch,
                error: errorFunc,
                context: selfmi,
                async: false
            });

            function successSearch(data, status) {
                if (data === 'no_results') {
                    $("#interstitial").hide();
                    $(".NoRecordrp").show();
                    setTimeout(function () {
                        $(".NoRecordrp").hide()
                    }, 5000);
                } else {
                    var results = JSON.parse(data);
                    selfmi.searchtblrp(results);
                    $("#interstitial").hide();
                }

            }
        };

        MaterialItem.prototype.Delete_RevisionParts = function (row_obj) {
            if (selfmi.revisionPartstbl().length === 1)
            { app.showMessage('Cannot delete the only revision'); return; }
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            var delJSON = mapping.toJS(row_obj);
            delJSON.Attributes.updtd_mtrl_id.value = "0";
            var callurl = 'api/revisions/update';
            delJSON = "[" + JSON.stringify(delJSON) + "]";

            $.ajax({
                type: "POST",
                url: callurl,
                data: delJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successDelete,
                error: errorFunc,
                context: selfmi
            });
            function successDelete(data, status) {
                if (data === 'SUCCESS') {
                    selfmi.revisionparts();
                    $("#interstitial").hide();
                    app.showMessage('Deleted successfully');
                } else {
                    app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                    $("#interstitial").hide();
                }
            }
        };

        MaterialItem.prototype.Add_RevisionParts = function (row_obj) {
            //if (row_obj.Attributes.rvsn.value === '' || row_obj.Attributes.rvsn.value === ' ')
                //return app.showMessage("Please enter a Revision Number");

            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            $.extend(row_obj.Attributes, { curr_mtrl_id: row_obj.Attributes.mtrl_id });
            delete row_obj.Attributes.item_desc;
            delete row_obj.Attributes.mfg_id;
            delete row_obj.Attributes.mfg_part_no;
            delete row_obj.Attributes.mtrl_id;
            delete row_obj.Attributes.product_id;
            delete row_obj.Attributes.rt_part_no;
            $.extend(row_obj.Attributes, { updtd_mtrl_id: selfmi.mtl.MtrlId });

            //var element = row_obj, cart = [];

            //row_obj.Attributes.rvsn = { "value": " ", "bool": false, "enable": false };
            //cart.push(row_obj.Attributes.rvsn);
            //var element = row_obj, cart = [];

            row_obj.Attributes.baseRvsn = { "value": "N", "bool": false, "enable": false };

            //cart.push(row_obj.Attributes.baseRvsn);
            //var element = row_obj, cart = [];

            row_obj.Attributes.currRvsn = { "value": "N", "bool": false, "enable": false };
            //cart.push(row_obj.Attributes.currRvsn);

            var addJSON = mapping.toJS(row_obj);
            var callurl = 'api/revisions/update';

            addJSON = "[" + JSON.stringify(addJSON) + "]";            

            var addJSON = mapping.toJS(row_obj);
            addJSON = "[" + JSON.stringify(addJSON) + "]";;
            var callurl = 'api/revisions/update'
            $.ajax({
                type: "POST",
                url: callurl,
                data: addJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successAdd,
                error: errorFunc,
                context: selfmi
            });

            function successAdd(data, status) {
                if (data === 'SUCCESS') {
                    selfmi.revisionparts();
                    $("#interstitial").hide();
                    app.showMessage('Added successfully');
                } else if (data.indexOf('already exists for the root part number') > 0) {
                    $("#interstitial").hide();
                    app.showMessage(data);
                } else {
                    app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                    $("#interstitial").hide();
                }
            }

        };

        MaterialItem.prototype.savemodalrp = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            var brCheckBoxes = $("#idRevisionPartsrp .chkbx_baservsn");

            for (var i = 0; i < brCheckBoxes.length; i++) {
                if (brCheckBoxes[i].checked == true) {
                    selfmi.revisionPartstbl()[i].Attributes.baseRvsn.value = "Y";
                }
                else {
                    selfmi.revisionPartstbl()[i].Attributes.baseRvsn.value = "N";
                }
            }

            var crCheckBoxes = $("#idRevisionPartsrp .chkbx_currvsn");

            for (var i = 0; i < crCheckBoxes.length; i++) {
                if (crCheckBoxes[i].checked == true) {
                    selfmi.revisionPartstbl()[i].Attributes.currRvsn.value = "Y";
                }
                else {
                    selfmi.revisionPartstbl()[i].Attributes.currRvsn.value = "N";
                }
            }

            var saveJSON = mapping.toJSON(selfmi.revisionPartstbl);
            var callurl = 'api/revisions/update';

            $.ajax({
                type: "POST",
                url: callurl,
                data: saveJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successSaverp,
                error: errorFunc,
                context: selfmi
            });

            function successSaverp(data, status) {
                if (data === 'SUCCESS') {

                    // Check revision number vs spec name
                    for (var i = 0; i < crCheckBoxes.length; i++) {
                        selfmi.revisionPartstbl()[i].Attributes.proposedSpec.value = selfmi.revisionPartstbl()[0].Attributes.mfg_id.value + '-' + selfmi.mtl.RtPrtNbr.value() + selfmi.revisionPartstbl()[i].Attributes.rvsn.value;
                    }

                    callurl = 'api/revisions/updateSpec';

                    for (var i = 0; i < crCheckBoxes.length; i++) {

                        var json = '{"mtrlId": ' + selfmi.revisionPartstbl()[i].Attributes.material_item_id.value + ', "oldSpec": "' + selfmi.revisionPartstbl()[i].Attributes.spec_name.value + '", "newSpec": "' + selfmi.revisionPartstbl()[i].Attributes.proposedSpec.value + '"}';

                        $.ajax({
                            type: "POST",
                            url: callurl,
                            data: json,
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            success: successSaveSpec,
                            error: errorFunc,
                            context: selfmi,
                            async: false
                        });

                        function successSaveSpec(data, status) {
                            if (data === 'SUCCESS') {
                            }
                            else {
                                $("#interstitial").hide();
                                if (data.indexOf('unique constraint (CDMMS_OWNER.MTRL_AK01) violated') > 0) {
                                    return app.showMessage('Unable to update the spec part name.');
                                }
                                else if (data.indexOf('/') > 0) {
                                    if (clmc + '-' + selfmi.mtl.RtPrtNbr.value() != specname) {
                                        for (var i = 0; i < selfmi.revisionPartstbl().length; i++) {
                                            self.revisionPartstbl()[i].Attributes.spec_name.value = clmc + "-" + selfmi.mtl.RtPrtNbr.value();
                                        }
                                        selfmi.revisionPartstbl(self.revisionPartstbl());
                                        this.revisionparts();
                                    }
                                    // anything but empty or success will be a string like specid/worktodoid/spectype so call reference method for the spec update popup
                                    var fields = data.split('/');
                                    var specHelper = new reference();
                                    specHelper.getSpecificationSendToNdsStatus(fields[0], fields[1], fields[2]);
                                }
                                else {
                                    app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                                    $("#interstitial").hide();
                                }
                            }
                        }
                    }

                    selfmi.revisionparts();
                    var specname = selfmi.revisionPartstbl()[0].Attributes.spec_name.value;
                    var clmc = selfmi.revisionPartstbl()[0].Attributes.mfg_id.value;

                    var rtUrl = 'api/revisions/update/rtprtnbr';
                    var json = '{"mtrlId": ' + selfmi.mtl.MtrlId.value() + ', "rtPrtNbr": "' + selfmi.mtl.RtPrtNbr.value() + '", "specNm": "' + specname + '", "clmc": "' + clmc + '"}';

                    $("#interstitial").css("height", "100%");
                    $("#interstitial").show();

                    $.ajax({
                        type: "POST",
                        url: rtUrl,
                        data: json,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: successSaveRootPartNumber,
                        error: errorSaveRootPartNumber,
                        context: selfmi
                    });

                    function successSaveRootPartNumber(data, status) {
                        var self = this;
                        if (data === 'SUCCESS') {
                            if (clmc + '-' + selfmi.mtl.RtPrtNbr.value() != specname) {
                                for (var i = 0; i < selfmi.revisionPartstbl().length; i++) {
                                    self.revisionPartstbl()[i].Attributes.spec_name.value = clmc + "-" + selfmi.mtl.RtPrtNbr.value();
                                }
                                selfmi.mtl.SpecNm.value(clmc + "-" + selfmi.mtl.RtPrtNbr.value());
                                selfmi.revisionPartstbl(self.revisionPartstbl());
                                this.revisionparts();
                            }
                            $("#interstitial").hide();
                            app.showMessage('Saved successfully');
                        } else {
                            $("#interstitial").hide();
                            if (data.indexOf('unique constraint (CDMMS_OWNER.MTRL_AK01) violated') > 0) {
                                return app.showMessage('Unable to update the material root part number. A material already exists with the same root part number.');
                            }
                            else if (data.indexOf('/') > 0) {
                                if (clmc + '-' + selfmi.mtl.RtPrtNbr.value() != specname) {
                                    for (var i = 0; i < selfmi.revisionPartstbl().length; i++) {
                                        self.revisionPartstbl()[i].Attributes.spec_name.value = clmc + "-" + selfmi.mtl.RtPrtNbr.value();
                                    }
                                    selfmi.revisionPartstbl(self.revisionPartstbl());
                                    this.revisionparts();
                                }
                                // anything but empty or success will be a string like specid/worktodoid/spectype so call reference method for the spec update popup
                                var fields = data.split('/');
                                var specHelper = new reference();
                                specHelper.getSpecificationSendToNdsStatus(fields[0], fields[1], fields[2]);

                                app.showMessage('Selected material updates are accepted successfully.', 'Accepted:Success');
                            }
                            else {
                                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                            }
                        }
                    };

                    function errorSaveRootPartNumber(err) {
                        $("#interstitial").hide();

                        if (err && err.responseJSON && err.responseJSON.ExceptionMessage && err.responseJSON.ExceptionMessage.indexOf('unique constraint (CDMMS_OWNER.MTRL_AK01) violated') > 0) {
                            return app.showMessage('Unable to update the material root part number. A material already exists with the same root part number.');
                        } else {
                            return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                        }
                    };
                }
                else {
                    app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                    $("#interstitial").hide();
                }
            }
        };

        MaterialItem.prototype.cancelmodalrp = function (modal, event) {
            $("#revisionPartsModal").hide();
        };

        MaterialItem.prototype.navigateToSpecification = function () {
            if (selfmi.specCmdPrompt()) {
                app.showMessage("Click on 'Ok' to Save Material Info and Navigate to Specification", 'Navigate to Specification', ['Ok', 'Cancel']).then(function (dialogResult) {
                    if (dialogResult == 'Ok') {
                        $("#interstitial").show();
                        selfmi.mtl.SpecTyp.value(($("#ftrTypDropdown option:selected").text()).toUpperCase());
                        selfmi.specCmdPromptNavigate(true);
                        selfMtlEdit.save();
                    }
                });
            } else {
                selfmi.navigateToSpecificationOnSuccess();
            }

        };

        MaterialItem.prototype.navigateToSpecificationOnSuccess = function () {
            var url = '#/spec/' + selfmi.mtl.SpecTyp.value();

            if (selfmi.mtl.SpecRvsnId && selfmi.mtl.SpecRvsnId.value() > 0) {
                url = url + '/' + selfmi.mtl.SpecRvsnId.value();
            } else if (selfmi.mtl.SpecId && selfmi.mtl.SpecId.value() > 0) {
                url = url + '/' + selfmi.mtl.SpecId.value();
            }

            if (selfmi.mtl.id.value())
                url = url + '?mtlid=' + selfmi.mtl.id.value();

            router.navigate(url, false);
        };

        //Revision 
        MaterialItem.prototype.filterLaborIds = function (pMtrlCatId, pFeatTypId, pCablTypId) {
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

                    var sam = selfmi.mtl;
                    sam.LbrId.options = results;
                    selfmi.mtl = sam;
                    //results;
                    $("#interstitial").hide();
                }
            },
        function (error) {
            $("#interstitial").hide();
            return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
        });
        };

        MaterialItem.prototype.showModalHLPN = function () {
            $("#idcdmmsrpHlpn").val('');
            $("#materialcoderpHlpn").val('');
            $("#partnumberrpHlpn").val('');
            $("#clmcrpHlpn").val('');
            $("#catlogdsptnrpHlpn").val('');
            selfmi.searchHlpn(null);
            selfmi.getCITHlpn(null);
            if (selfmi.enableHLPNBtnBoolSave()) {
                app.showMessage("Click on 'Ok' to Save Material Category <br/> to High Level Part", 'Material Category', ['Ok', 'Cancel']).then(function (dialogResult) {
                    if (dialogResult == 'Ok') {
                        $("#interstitial").show();
                        selfMtlEdit.save();
                    }
                });
            } else {
                $("#highLevelPartsModal").show();
                selfmi.getCITModalHLPN(selfmi.mtl.id.value());
            }
        };

        MaterialItem.prototype.cancelModalHLPN = function () {
            $("#idcdmmsrpHlpn").val('');
            $("#materialcoderpHlpn").val('');
            $("#partnumberrpHlpn").val('');
            $("#clmcrpHlpn").val('');
            $("#catlogdsptnrpHlpn").val('');
            $("#ChbkSearchRecordOnly").val('off');
            selfmi.searchHlpn(null);
            selfmi.getCITHlpn(null);
            $("#highLevelPartsModal").hide();
        };

        MaterialItem.prototype.UpdatePlacement = function (data, event) {
            //var selectedValue = event.target.value;
            //var selectedValuePlacement='';
            //var selectedValueId = event.target.id;
            //var refId = '';
            //refId = selectedValueId.substring(0,selectedValueId.lastIndexOf('-ref-'));
            //selectedValueId = selectedValueId.substring(selectedValueId.lastIndexOf('-ref-') + 5);
            //var testX = selectedValue;

            //if (selectedValue != null && selectedValue.length >= 0) {
            //    if (selectedValue.substring(0, 2) == 'F ' || selectedValue.substring(0, 2) == 'R ') {
            //        //selectedValueId = selectedValueId.substring(4, selectedValueId.length);
            //        selectedValuePlacement = selectedValue.substring(0, 1);
            //        selectedValue = selectedValue.substring(4, selectedValue.length);
                   
            //    } else if (selectedValueId.substring(0, 2) == ' -') {
            //        //selectedValueId = selectedValueId.substring(3, selectedValueId.length);
            //        selectedValue = selectedValue.substring(3, selectedValue.length);
            //    } else {

            //    }


            //    for (var i = 0 ; i < selfmi.getCITHlpn().length ; i++) {
            //        if (selfmi.getCITHlpn()[i].part_number.value == selectedValueId && selfmi.getCITHlpn()[i].refDefId.value == refId
            //            && selfmi.getCITHlpn()[i].parent_def_id.value == data.parent_def_id.value) {
            //            selfmi.getCITHlpn()[i].parent_part.value = testX;
            //            for (j in selfmi.getCITHlpn()[i].ht_parts.htparts) {                           
            //                if (testX == selfmi.getCITHlpn()[i].ht_parts.htparts[j]) {
            //                    selfmi.getCITHlpn()[i].parent_id.value = j;
            //                }
            //            }
            //            selfmi.getCITHlpn()[i].placement_front_rear.value = selectedValuePlacement;
            //        }
            //    }
            //}
            //var x = selfmi.getCITHlpn();
            //selfmi.getCITHlpn(null);
            //selfmi.getCITHlpn(x);
        };
        MaterialItem.prototype.totalQuantity = function (data, event) {
            var quanId = event.target.id;
            quanId = quanId.substring(5);

            var quanIdValue = event.target.value;
            $("#quana" + quanId).val(quanIdValue);
            $("#total" + quanId).text(Number($("#quana" + quanId).val()) + Number($("#spare" + quanId).val()));

        };

        MaterialItem.prototype.totalQuantitySpare = function (data, event) {
            var quanId = event.target.id;
            quanId = quanId.substring(5);

            var quanIdValue = event.target.value;
            $("#spare" + quanId).val(quanIdValue);
            $("#total" + quanId).text(Number($("#quana" + quanId).val()) + Number($("#spare" + quanId).val()));

        };

        MaterialItem.prototype.getCITModalHLPN = function (cdmmsId) {
            //alert(selfmi.mtl.id.value());
            $("#interstitial").show();
            var url = 'api/highlevelpart/getCIThlp/' + cdmmsId;
            http.get(url).then(function (response) {

                if (response != 'no_results') {
                    var results = JSON.parse(response);
                    var results123 = JSON.parse(response);

                    for (var i = 0 ; i < results.length ; i++) {
                        if (results[i].placement_front_rear.value.length == 0) {
                            results[i].placement_front_rear.enable = false;
                        } else {
                            results[i].placement_front_rear.enable = true;
                        }
                    }


                    for (var i = 0 ; i < results.length ; i++) {
                        if (results[i].parent_id != null && results[i].parent_id.value.length > 0) {
                            var dup = [];
                            dup.push('wer');
                            for (j in results[i].related_to.liststrings) {
                                for (var c = 0; c < results123.length; c++) {
                                    if (results[c].part_number.value == results[i].related_to.liststrings[j]) {
                                        if (dup.indexOf(results[c].placement_front_rear.value + " - " + results[i].related_to.liststrings[j]) < 0) {
                                            if (results[c].placement_front_rear.value != null && results[c].placement_front_rear.value.length > 0) {

                                                results[i].related_to.liststrings[j] = results[c].placement_front_rear.value + " - " + results[i].related_to.liststrings[j];
                                                // results[i].placement_front_rear.value = results[c].placement_front_rear.value;
                                                dup.push(results[i].related_to.liststrings[j]);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    for (var i = 0 ; i < results.length ; i++) {
                        if (results[i].parent_id != null && results[i].parent_id.value.length > 0) {
                            var dup = [];
                            dup.push('wer');
                            for (j in results[i].ht_parts.htparts) {
                                for (var c = 0; c < results123.length; c++) {
                                    if (results[c].part_number.value == results[i].ht_parts.htparts[j]) {
                                        if (dup.indexOf(results[c].placement_front_rear.value + " - " + results[i].ht_parts.htparts[j]) < 0) {
                                            if (results[c].placement_front_rear.value != null && results[c].placement_front_rear.value.length > 0) {

                                                results[i].ht_parts.htparts[j] = results[c].placement_front_rear.value + " - " + results[i].ht_parts.htparts[j];
                                                dup.push(results[i].ht_parts.htparts[j]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (var b = 0; b < results123.length; b++) {
                        for (var i = 0 ; i < results.length ; i++) {
                            if (results[i].parent_id != null && results[i].parent_id.value.length > 0) {
                                if (results[i].parent_part.value == results[b].part_number.value) {
                                    for (j in results[i].ht_parts.htparts) {
                                        if (results[i].parent_id.value == j) {
                                            var str = results[i].ht_parts.htparts[j];
                                            str = str.substring(0, 1);
                                            if (str == 'F' || str == 'R') {
                                                results[i].placement_front_rear.value = str;
                                                results[i].parent_part.value = results[i].placement_front_rear.value + " - " + results[i].parent_part.value;
                                            }
                                        }
                                    }
                                    results[i].placement_front_rear.enable = false;
                                }
                            }
                        }
                    }

                    results.forEach(function (e, ndx) {
                        e._seqNo = { value: ndx + 1, bool: false, enabled: true };
                    });

                    var hlpncdmmsid = sessionStorage.hlpncdmmsid;
                    if (typeof hlpncdmmsid !== 'undefined') {
                        selfmi.hlpncdmmsid(hlpncdmmsid);
                        document.getElementById('idcdmmsrpHlpn').value = hlpncdmmsid;
                    }

                    var hlpnmtlcd = sessionStorage.hlpnmtlcd;
                    if (typeof hlpnmtlcd !== 'undefined') {
                        selfmi.hlpnmtlcd(hlpnmtlcd);
                        document.getElementById('materialcoderpHlpn').value = hlpnmtlcd;
                    }

                    var hlpnpartno = sessionStorage.hlpnpartno;
                    if (typeof hlpnpartno !== 'undefined') {
                        selfmi.hlpnpartno(hlpnpartno);
                        document.getElementById('partnumberrpHlpn').value = hlpnpartno;
                    }

                    var hlpnclmc = sessionStorage.hlpnclmc;
                    if (typeof hlpnclmc !== 'undefined') {
                        selfmi.hlpnclmc(hlpnclmc);
                        document.getElementById('clmcrpHlpn').value = hlpnclmc;
                    }

                    var hlpndesc = sessionStorage.hlpndesc;
                    if (typeof hlpndesc !== 'undefined') {
                        selfmi.hlpndesc(hlpndesc);
                        document.getElementById('catlogdsptnrpHlpn').value = hlpndesc;
                    }

                    var hlpnrecordonly = sessionStorage.hlpnrecordonly;
                    if (typeof hlpnrecordonly !== 'undefined') {
                        if (hlpnrecordonly === 'on') {
                            selfmi.hlpnrecordonly(true);
                            document.getElementById('ChbkSearchRecordOnly').checked = true;
                        }
                        else {
                            selfmi.hlpnrecordonly(false);
                            document.getElementById('ChbkSearchRecordOnly').checked = false;
                        }
                    }

                    var hlpnexactmatch = sessionStorage.hlpnexactmatch;
                    if (typeof hlpnexactmatch !== 'undefined') {
                        if (hlpnexactmatch === 'Y') {
                            selfmi.hlpncheckSearchType("Exact Search");
                        }
                        else {
                            selfmi.hlpncheckSearchType("Fuzzy Search");
                        }
                    }

                    selfmi.getCITHlpn(results);
                    setTimeout(selfmi.enableDragging, 100);
                    $("#interstitial").hide();
                } else {
                    $("#interstitial").hide();
                }
            },
           function (error) {
               $("#interstitial").hide();
               return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
           });
        };

        MaterialItem.prototype.onHlpClick = function (rec) {
            var sequenceNumber = '' + rec.cntnd_in_seq_no.value;
            var numberDashes = sequenceNumber.split("-").length - 1;
            var toggleAllOff = false;
            for (var i = 0; i < selfmi.getCITHlpn().length; i++) {
                if (selfmi.getCITHlpn()[i].cntnd_in_seq_no.value.length >= (sequenceNumber + '-').length
                    && selfmi.getCITHlpn()[i].cntnd_in_seq_no.value.indexOf(sequenceNumber + '-') === 0
                    && (numberDashes + 1) == (selfmi.getCITHlpn()[i].cntnd_in_seq_no.value).split("-").length-1) {
                    if (selfmi.getCITHlpn()[i].is_visible.bool == true) {
                        selfmi.getCITHlpn()[i].is_visible.bool = false;
                        toggleAllOff = true;
                    }
                    else {
                        selfmi.getCITHlpn()[i].is_visible.bool = true;
                    }
                }
            }
            if (toggleAllOff) {
                for (var i = 0; i < selfmi.getCITHlpn().length; i++) {
                    if (selfmi.getCITHlpn()[i].cntnd_in_seq_no.value.length >= (sequenceNumber + '-').length
                        && selfmi.getCITHlpn()[i].cntnd_in_seq_no.value.indexOf(sequenceNumber + '-') === 0) {
                        selfmi.getCITHlpn()[i].is_visible.bool = false;
                    }
                }
            }
            var x = selfmi.getCITHlpn();
            selfmi.getCITHlpn(null);
            selfmi.getCITHlpn(x);
        }

        MaterialItem.prototype.enableDragging = function () {
            var $pane = $("#contaiedInPartsHlpn tbody");
            var $rows = $pane.find("tr");

            var cloned = {};

            $rows.draggable({
                scroll: true
                , cusor: "move"
                , helper: "clone"
                , handle: ".dragger"
                , start: function (event, ui) {
                    cloned.$tr = $(this);
                    cloned.$helper = $(ui.helper);
                    cloned.$tr.addClass("disabled");
                    cloned.$helper.addClass("dragging");

                    // such a kludge to format this....
                    var $cells = cloned.$tr.find("td");
                    var max = $cells.length;
                    $cells.each(function (ndx, e) {
                        var $e = $(e);
                        var $td = cloned.$helper.find("td:nth-child(" + (ndx + 1) + ")");
                        var css = {};
                        css["border-" + (ndx === 0 ? "left" : "right")] = "1px solid red";
                        if (ndx == 0 || ndx == max - 1) {
                            $td.css(css);
                        }
                        $td.css({ width: $e.width() + 11, "background-color": "orange", "border-top": "1px solid red", "border-bottom": "1px solid red" });
                    });
                }
                , drag: function (event, ui) {
                    var $d = $(this);
                    if (cloned.$tr.data("expanded")) {
                        return false;
                    }
                }

                , stop: function (event, ui) {
                    cloned.$tr.removeClass("disabled");
                    cloned.$helper.removeClass("dragging").remove();
                }
                , cancel: function (event, uit) {
                    cloned.$helper.remove();
                }
            });

            $rows.droppable({
                over: function (event, ui) {
                    var $e = $(this);
                    //console.log("over: %s %s %s", $e.data("cc-def-id"), $e.data("cntnd-in-id"), $e.data("expaned"));
                    $e.addClass("success");
                }
                , out: function (event, ui) {
                    var $e = $(this);
                    //console.log("out: %s %s %s", $e.data("cc-def-id"), $e.data("cntnd-in-id"), $e.data("expaned"));
                    $e.removeClass("success");
                }
                , drop: function (event, ui) {
                    var $t = $(this);
                    $t.removeClass("success");

                    if ($t.data("expanded")) {
                        app.showMessage("Cannot re-order item while expanded");
                        return false;
                    }

                    var currSeqNo = cloned.$tr.data("seq-no");
                    var newSeqNo = $t.data("seq-no");

                    var $moved = cloned.$tr.find("span.seq-no");
                    $moved.text(newSeqNo);

                    selfmi.reOrderList(cloned.$tr, $t);
                }
            });
        }
        MaterialItem.prototype.reOrderList = function ($moved, $target) {
            var list = JSON.parse(JSON.stringify(selfmi.getCITHlpn())); // copy list...
            var newSeqNo = $moved.find("span.seq-no").text();
            var currSeqNo = $moved.data('seq-no');

            //var toBeMovedItemNdx = list.findIndex(function (e) { return e._seqNo.value == currSeqNo; });
            //if (toBeMovedItemNdx === -1) {
            //    app.showMessage("Could not find item to be moved.");
            //    return;
            //}
            //var toBeDisplacedItemNdx = list.findIndex(function (e) { return e._seqNo.value == newSeqNo; });
            //if (toBeDisplacedItemNdx === -1) {
            //    app.showMessage("Could not find displaced item.");
            //    return;
            //}

            var toBeMovedItemNdx = currSeqNo-1;
            var toBeDisplacedItemNdx = newSeqNo - 1;

            var originalFirstSeqNumber = 0;
            var originalLastSeqNumber = 0;
            if (toBeMovedItemNdx < toBeDisplacedItemNdx) {
                originalFirstSeqNumber = list[toBeMovedItemNdx].cntnd_in_seq_no.value;
                originalLastSeqNumber = list[toBeDisplacedItemNdx].cntnd_in_seq_no.value;
            }
            else {
                originalFirstSeqNumber = list[toBeDisplacedItemNdx].cntnd_in_seq_no.value;
                originalLastSeqNumber = list[toBeMovedItemNdx].cntnd_in_seq_no.value;
            }

            var up = (toBeMovedItemNdx < toBeDisplacedItemNdx);

            // take item that is moving out of the list...
            var toBeMovedItem = list.splice(toBeMovedItemNdx, 1)[0];
            // insert item at new location...
            list.splice(toBeDisplacedItemNdx, 0, toBeMovedItem);

            // renumber
            list.forEach(function (e, ndx) {
                e._seqNo.value = ndx + 1;
            });

            var first = true
            var savedFirstIndex = 0;
            var savedFirstSequenceNumber = 0;
            if (toBeMovedItemNdx < toBeDisplacedItemNdx) {
                var startingSeqNumber = list[toBeDisplacedItemNdx].cntnd_in_seq_no.value;
                var firstSeqNumber = list[toBeMovedItemNdx].cntnd_in_seq_no.value;
                var x = 0;
                for (i = toBeMovedItemNdx; i <= toBeDisplacedItemNdx; i++) {
                    if (list[i].expanded.bool == false) {
                        if (first) {
                            savedFirstIndex = i;
                            savedFirstSequenceNumber = startingSeqNumber;
                            list[i].cntnd_in_seq_no.value = 0;
                            this.UpdateSequenceNumber(list[i].refDefId.value,'0');
                            first = false;
                        }
                        else {  // not the first one
                            list[i].cntnd_in_seq_no.value = parseFloat(firstSeqNumber) + parseFloat(x);
                            this.UpdateSequenceNumber(list[i].refDefId.value, list[i].cntnd_in_seq_no.value);
                            x++;
                        }
                    }
                }
                list[savedFirstIndex].cntnd_in_seq_no.value = savedFirstSequenceNumber;
                this.UpdateSequenceNumber(list[savedFirstIndex].refDefId.value, savedFirstSequenceNumber);
            }
            else {
                first = true;
                var startingSeqNumber = list[toBeMovedItemNdx].cntnd_in_seq_no.value;
                var firstSeqNumber = list[toBeDisplacedItemNdx].cntnd_in_seq_no.value;
                var x = 1;
                for (i = toBeMovedItemNdx; i >= toBeDisplacedItemNdx; i--) {
                    if (list[i].expanded.bool == false) {
                        if (first) {
                            savedFirstIndex = i;
                            savedFirstSequenceNumber = firstSeqNumber;
                            list[i].cntnd_in_seq_no.value = 0;
                            this.UpdateSequenceNumber(list[i].refDefId.value, '0');
                            first = false;
                        }
                        else {  // not the first one
                            list[i].cntnd_in_seq_no.value = parseFloat(firstSeqNumber) - parseFloat(x);
                            this.UpdateSequenceNumber(list[i].refDefId.value, list[i].cntnd_in_seq_no.value);
                            x++;
                        }
                    }
                }
                list[savedFirstIndex].cntnd_in_seq_no.value = savedFirstSequenceNumber;
                this.UpdateSequenceNumber(list[savedFirstIndex].refDefId.value, savedFirstSequenceNumber);
            }

            // redraw
            //selfmi.getCITHlpn(list);
            //setTimeout(selfmi.enableDragging, 100);

            // refresh
            var cdmmsID = document.getElementById("cdmmsIdHlpnTxt").innerText;
            selfmi.getCITModalHLPN(cdmmsID);
            selfmi.searchHlpn(null);
        }

        MaterialItem.prototype.UpdateSequenceNumber = function(refDefId, sequenceNumber) {
            var url = "api/highlevelpart/swapsequence/" + refDefId + "/" + sequenceNumber;
            $.ajax({
                type: "POST",
                url: url,
                success: function (data) {
                },
                dataType: "json",
                async: false,
                error: function (data) {
                }
            });
        }
        MaterialItem.prototype.searchModalHLPN = function () {
            var mtlid = $("#idcdmmsrpHlpn").val();
            var mtlcode = $("#materialcoderpHlpn").val();
            var partnumb = $("#partnumberrpHlpn").val();
            var Clmc = $("#clmcrpHlpn").val();
            var caldsp = $("#catlogdsptnrpHlpn").val();
            var recordonly = $("#ChbkSearchRecordOnly").val();

            sessionStorage.setItem('hlpncdmmsid', mtlid);
            sessionStorage.setItem('hlpnmtlcd', mtlcode);
            sessionStorage.setItem('hlpnpartno', partnumb);
            sessionStorage.setItem('hlpnclmc', Clmc);
            sessionStorage.setItem('hlpndesc', caldsp);

            var radio = document.getElementById('hlpnsearchFuzzy');
            var exactMatch = 'Y';

            if (radio.checked) {
                exactMatch = 'N';
            }
            else exactMatch = 'Y';
            sessionStorage.setItem('hlpnexactmatch', exactMatch);

            if (mtlid == '' && mtlcode == '' && partnumb == '' && Clmc == '' && caldsp == '') {
                app.showMessage('You must enter something to search on.');
                return;
            }
            $("#interstitial").show();
            var searchJSON = {
                product_id: mtlcode, mfg_part_no: partnumb, mat_desc: caldsp, mfg_id: Clmc, material_item_id: mtlid
            };
            var searchJSON = {
                PrdctId: mtlcode, PrtNo: partnumb, MtlDesc: caldsp, clmc: Clmc, cdmmsid: mtlid, status: '',
                MaterialCategory: '', CableType: '', FeatureType: '',
                ItemStatus: '', SpecificationName: '', LastUpdate: '', UserID: '', ExactMatch: exactMatch
            };
            var searchUrl = '';
            if ($("#ChbkSearchRecordOnly").is(':checked')) {
                searchUrl = 'api/highlevelpart/search/ro';
                sessionStorage.setItem('hlpnrecordonly', 'on');
            } else {
                searchUrl = 'api/material/searchall/';
                sessionStorage.setItem('hlpnrecordonly', 'off');
            }


            http.get(searchUrl, searchJSON).then(function (response) {
                if (response === 'no_results') {
                    $("#interstitial").hide();
                    $(".NoRecordhlp").show();
                    setTimeout(function () {
                        $(".NoRecordhlp").hide()
                    }, 5000);
                }
                else {

                    var results = JSON.parse(response);

                    // remove all results that have a null material category
                    for (var z = results.length - 1; z >= 0; z--) {
                        if (results[z].Attributes.mtl_ctgry.value == '') {
                            results.splice(z, 1);
                        }
                    }

                    for (var i = 0; i < results.length; i++) {
                        results[i].Attributes.is_selected = { bool: false, value: false };
                        results[i].Attributes.revisions = { bool: false, value: 'Y', options: [{ value: 'Y', text: 'Y' }, { value: 'N', text: 'N' }] };
                    }
                    //var getList = selfmi.getCITHlpn();
                    //for (var x = 0; x < getList.length; x++) {
                    //for (var y = 0; y < results.length; y++) {
                    //    if (getList[x].cin_cdmms_Id.value == results[y].Attributes.material_item_id.value) {
                    //        results.splice(y, 1);
                    //    }
                    //}
                    //}
                    //if (results.length == 0) {
                    //    $(".NoRecordhlp").show();
                    //    setTimeout(function () {
                    //        $(".NoRecordhlp").hide()
                    //    }, 5000);
                    //}
                    selfmi.searchHlpn(results);
                }

                $("#interstitial").hide();
            },
           function (error) {
               selfmi.searchHlpn(null);
               $("#interstitial").hide();
               return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
           });

        };

        MaterialItem.prototype.ensureRackPositionsDontOverlap = function () {
            $(".rack_pos").css("background-color", "white"); // clear any errors...

            var list = selfmi.getCITHlpn().filter(function(e) { 
                return e.rack_pos 
                        && e.rack_pos.value.length > 0 
                        && e.rack_pos.value.toString() !== ""
                        && isNaN(e.rack_pos.value) === false
                ;
            }) || [];

            
            var rackPos = [];
            list.forEach(function(e) { 
                var ndx = parseInt(e.rack_pos.value);
                if (!rackPos[ndx]) {
                    rackPos[ndx] = [];
                }
                rackPos[ndx].push(e);
            });

            var overlaps = rackPos.filter(function (e) { return e.length > 1; }) || [];
            return overlaps;
        }
        MaterialItem.prototype.hiliteOverlappingRackPositions = function (list) {
            list = (list || selfmi.ensureRackPositionsDontOverlap());
            var errors = [];
            list.forEach(function (items) {
                items.forEach(function (e) {
                    var id = e.cntnd_in_seq_no.value;
                    $(".seq" + id).find(".rack_pos").css("background-color", "salmon");
                    errors.push("#"+e.cntnd_in_seq_no.value+": "+e.clmc.value + " " + e.part_number.value+" at rack position "+e.rack_pos.value);
                });
            });
           
            return errors;
        }

        MaterialItem.prototype.onUpdateRackMountPos = function (rec, event) {
            $(event.target).css("background-color", "white"); // clear any errors...

            var pos = rec.rack_pos.value;
            if (pos.length === 0) {
                return;
            }

            var isWholeNum = new RegExp(/^\d+$/);
            if (isNaN(pos) || pos < 0 || pos > 200 || isWholeNum.test(pos) === false ) {
                $(".seq" + rec.cntnd_in_seq_no.value).find(".rack_pos").css("background-color", "salmon");
                return app.showMessage(pos + " is not a valid rack mount position; must be a whole number between 0 and 200.", "Invalid Rack Mount Position: "+pos);
            }

            selfmi.hiliteOverlappingRackPositions();
        }

        MaterialItem.prototype.clearMaterialSearchHLPN = function () {
            document.getElementById('idcdmmsrpHlpn').value = '';
            document.getElementById('materialcoderpHlpn').value = '';
            document.getElementById('partnumberrpHlpn').value = '';
            document.getElementById('clmcrpHlpn').value = '';
            document.getElementById('catlogdsptnrpHlpn').value = '';
            document.getElementById("ChbkSearchRecordOnly").checked = false;
            sessionStorage.setItem('hlpncdmmsid', '');
            sessionStorage.setItem('hlpnmtlcd', '');
            sessionStorage.setItem('hlpnpartno', '');
            sessionStorage.setItem('hlpnclmc', '');
            sessionStorage.setItem('hlpndesc', '');
            sessionStorage.setItem('hlpnrecordonly', 'off');
            selfmi.hlpnrecordonly(false);
            sessionStorage.setItem('hlpnexactmatch', 'N')
            selfmi.hlpncheckSearchType("Fuzzy Search");
        }

        MaterialItem.prototype.toggleStrikeout = function (data, event) {
            var isIE = /*@cc_on!@*/false || !!document.documentMode;
            var isEdge = !isIE && !!window.StyleMedia;
            var x = selfmi.getCITHlpn();
            if (isIE || isEdge) {
                if (data.is_selected.bool == false) {  // yes, false means checked, go figure
                    // add code to unstrike
                    x[data._seqNo.value - 1].added_class.value = '';
                    x[data._seqNo.value - 1].is_selected.bool = true;
                } else {
                    // add code for strikeout
                    x[data._seqNo.value - 1].added_class.value = ' to_be_removed to-be-removed';
                    x[data._seqNo.value - 1].is_selected.bool = false;
                }
            }
            else {
                if (data.is_selected.bool == true) {
                    // add code to unstrike
                    x[data._seqNo.value - 1].added_class.value = '';
                    x[data._seqNo.value - 1].is_selected.bool = true;
                } else {
                    // add code for strikeout
                    x[data._seqNo.value - 1].added_class.value = ' to_be_removed to-be-removed';
                    x[data._seqNo.value - 1].is_selected.bool = false;
                }
            }
            selfmi.getCITHlpn(null);
            selfmi.getCITHlpn(x);
        }

        MaterialItem.prototype.resequenceHlpn = function () {
            $("#interstitial").show();
            var cdmmsID = document.getElementById("cdmmsIdHlpnTxt").innerText;
            var url = "api/highlevelpart/resequence/" + cdmmsID;
            $.ajax({
                type: "POST",
                url: url,
                success: function (data) {
                    selfmi.getCITModalHLPN(cdmmsID);
                    selfmi.searchHlpn(null);
                },
                dataType: "json",
                error: function (data) {
                }
            });
            $("#interstitial").hide();
        }

        MaterialItem.prototype.saveHlpnClose = function () {
            var close = function() { $("#highLevelPartsModal").hide(); }
            selfmi.doSaveHlpn( close );
        };
        MaterialItem.prototype.saveHlpn = function () {
            selfmi.doSaveHlpn();
        }
        MaterialItem.prototype.doSaveHlpn = function(next) {
            $("#interstitial").show();

            next = (next || function () { });

            var errors = []; 
            var selectedhlpnSearchCbk = [];
            var selectedcontainedInCheckHlpn = [];
            var hlpnSearchCbk = $("#searchresultHlpn .hlpnSearchCbk");
            var containedInCheckHlpn = $("#contaiedInPartsHlpn .containedInCheckHlpn");
            var cdmmsId = $("#cdmmsIdHlpnTxt").text();
            var materialCode = $("#materialCodeHlpnTxt").text();
            var parentFeatureType = '';
            var deleteString = '';

            var overlaps = selfmi.hiliteOverlappingRackPositions();
            if (overlaps.length > 0) {
                $("#interstitial").hide();
                // can't save until user fixes...
                return app.showMessage(overlaps.join("<br/>"), "Overlapping Rack Mount Positions Found");
            }
           
            if (selfmi.searchHlpn() != null) {
                for (var i = 0; i < selfmi.searchHlpn().length; i++) {
                    if (selfmi.searchHlpn()[i].Attributes.is_selected.bool == true) {
                        selectedhlpnSearchCbk.push(selfmi.searchHlpn()[i]);
                    }
                }
            }

            var url = 'api/highlevelpart/getCIThlp/' + cdmmsId;
            http.get(url).then(function (response) {
                if (response != 'no_results') {
                    var results = JSON.parse(response);
                    var abc = selfmi.getCITHlpn();
                    var xyz = results;
                    var deletethis = false;
                    var txtCondt = '';

                    for (var i = 0; i < selectedhlpnSearchCbk.length; i++) {
                        txtCondt += "Part Number <b>" + selectedhlpnSearchCbk[i].Attributes.mfg_part_no.value + '</b> is being added<br/>';
                    }

                    for (var i = 0; i < abc.length; i++) {
                        if (abc[i].is_selected.bool == false && abc[i].has_children.bool == true) {
                            deletethis = confirm('Part ' + abc[i].part_number.value + ' will be deleted but has an assigned contained-in part. Do you really want to delete ' + abc[i].part_number.value + '?');
                            if (!deletethis) {
                                abc[i].is_selected.bool = true;  // don't delete this one but continue with anything else
                                deletethis = false;  // set back to false in case there's another one
                            }
                        }

                        // this part has an NDS record in hlp_mtrl_revsn_def_alias_val and has to be deleted by NDS
                        if (abc[i].is_selected.bool == false) {
                            if (abc[i].aliasVal.value != null && abc[i].aliasVal.value != '') {
                                deleteString += 'Part ' + abc[i].part_number.value + ' has been marked for deletion. Deletion will occur once successfully deleted in NDS. When you see the message returned from NDS, please close this HLPN window to refresh the contained-in parts list.  ';
                            }
                        }

                        for (var j = 0; j < xyz.length; j++) {
                            if (xyz[j].cin_cdmms_Id.value == abc[i].cin_cdmms_Id.value) {
                                if (abc[i].parent_part.value != null
                                    && ((xyz[j].parent_part.value != abc[i].parent_part.value) || (abc[i].parent_part.value != null && abc[i].parent_part.value != '' && xyz[i].prnt_hlp_mtrl_revsn_def_id.value == '0'))
                                    && xyz[j].parent_def_id.value == abc[i].parent_def_id.value) {
                                    if (abc[i].parent_part.value == '') {
                                        abc[i].parent_id.value = '';  // allow the user to null this out
                                    }
                                    abc[i].is_updated.bool = true;
                                    var parentpartnumber = abc[i].parent_part.value
                                    for (var index in abc[i].htRefIdParts.htparts) {
                                        if (parentpartnumber == abc[i].htRefIdParts.htparts[index]) {
                                            abc[i].parent_id.value = index;
                                        }
                                    }
                                }
                                if (xyz[j].placement_front_rear.value != abc[i].placement_front_rear.value && xyz[j].parent_def_id.value == abc[i].parent_def_id.value) {
                                    abc[i].is_updated.bool = true;
                                }
                                //if (!abc[i].contained_in_part.bool && (xyz[j].is_revision.value != abc[i].is_revision.value)) {
                                //    abc[i].is_updated.bool = true;
                                //}
                                if (xyz[j].quantity.value != abc[i].quantity.value && xyz[j].parent_def_id.value == abc[i].parent_def_id.value) {
                                    abc[i].is_updated.bool = true;
                                }
                                if (xyz[j].spare_quantity.value != abc[i].spare_quantity.value && xyz[j].parent_def_id.value == abc[i].parent_def_id.value) {
                                    abc[i].is_updated.bool = true;
                                }
                                if (xyz[j].ycoord.value != abc[i].ycoord.value && xyz[j].parent_def_id.value == abc[i].parent_def_id.value) {
                                    abc[i].is_updated.bool = true;
                                }
                                if (xyz[j].xcoord.value != abc[i].xcoord.value && xyz[j].parent_def_id.value == abc[i].parent_def_id.value) {
                                    abc[i].is_updated.bool = true;
                                }
                                if (xyz[j].rack_pos.value != abc[i].rack_pos.value && xyz[j].parent_def_id.value == abc[i].parent_def_id.value) {
                                    abc[i].is_updated.bool = true;
                                }
                                if (xyz[j].label_nm.value != abc[i].label_nm.value && xyz[j].parent_def_id.value == abc[i].parent_def_id.value) {
                                    abc[i].is_updated.bool = true;
                                }
                            }
                        }

                        selectedcontainedInCheckHlpn.push(abc[i]);

                        // Check if this is a change that might affect a common config
                        if (abc[i].comn_cnfg_string.value != '') {
                            if (typeof abc[i].ycoord.value !== "undefined" && typeof xyz[i].ycoord !== "undefined" && abc[i].ycoord.value != xyz[i].ycoord.value) {
                                txtCondt += "EQDES changed to <b>" + xyz[i].ycoord.value + '</b> from ' + abc[i].ycoord.value + ' on part number ' + xyz[i].part_number.value + "<br/>";
                            }

                            if (typeof abc[i].xcoord.value !== "undefined" && typeof xyz[i].xcoord !== "undefined" && abc[i].xcoord.value != xyz[i].xcoord.value) {
                                txtCondt += "HORZ DISP changed to <b>" + xyz[i].xcoord.value + '</b> from ' + abc[i].xcoord.value + ' on part number ' + xyz[i].part_number.value + "<br/>";
                            }

                            if (typeof abc[i].quantity.value !== "undefined" && typeof xyz[i].quantity !== "undefined" && abc[i].quantity.value != xyz[i].quantity.value) {
                                txtCondt += "Quantity changed to <b>" + xyz[i].quantity.value + '</b> from ' + abc[i].quantity.value + ' on part number ' + xyz[i].part_number.value + "<br/>";
                            }

                            if (typeof abc[i].spare_quantity.value !== "undefined" && typeof xyz[i].spare_quantity !== "undefined" && abc[i].spare_quantity.value != xyz[i].spare_quantity.value) {
                                txtCondt += "Spare Quantity changed to <b>" + xyz[i].spare_quantity.value + '</b> from ' + abc[i].spare_quantity.value + ' on part number ' + xyz[i].part_number.value + "<br/>";
                            }

                            if (typeof abc[i].placement_front_rear.value !== "undefined" && typeof xyz[i].placement_front_rear !== "undefined" && abc[i].placement_front_rear.value != xyz[i].placement_front_rear.value) {
                                txtCondt += "Placement (Front/Rear) changed to <b>" + xyz[i].placement_front_rear.value + '</b> from ' + abc[i].placement_front_rear.value + ' on part number ' + xyz[i].part_number.value + "<br/>";
                            }

                            if (typeof xyz[i].is_selected.bool !== "undefined" && abc[i].is_selected.bool == false) {
                                txtCondt += "Part Number <b>" + xyz[i].part_number.value + '</b> is being deleted<br/>';
                            }

                            if (typeof abc[i].parent_part.value !== "undefined" && typeof xyz[i].parent_part !== "undefined" && abc[i].parent_part.value != xyz[i].parent_part.value) {
                                var oldvalue = xyz[i].parent_part.value;
                                var newvalue = abc[i].parent_part.value;
                                if (oldvalue === '') {
                                    oldvalue = '-nothing selected-';
                                }
                                if (newvalue === '') {
                                    newvalue = '-nothing selected-';
                                }
                                txtCondt += "Related To changed to <b>" + newvalue + '</b> from ' + oldvalue + ' on part number ' + xyz[i].part_number.value + "<br/>";
                            }
                        }
                    }

                    for (var i = 0; i < selectedhlpnSearchCbk.length; i++) {
                        selectedhlpnSearchCbk[i].Attributes.cin_cdmms_Id.value = cdmmsId;
                    }
                    //if (deleteString != '' && xyz[0].comn_cnfg_string === '') {
                    if (deleteString != '') {
                        app.showMessage(deleteString);
                    }
                    var newCIT = mapping.toJS(selectedhlpnSearchCbk);
                    var oldCIT = mapping.toJS(selectedcontainedInCheckHlpn);

                    var saveHlpn = { saveCITNew: newCIT, saveCITExisting: oldCIT, cuid: selfMtlEdit.usr.cuid, catalogDescription: selfmi.mtl.CtlgDesc.value() };

                    saveHlpn = JSON.stringify(saveHlpn);

                    var textlength = txtCondt.length;
                    var configNameList = selfmi.getConfigNames(cdmmsId);
                    if (xyz && xyz.length != 0 && xyz[0].comn_cnfg_string) {
                        if (xyz[0].comn_cnfg_string.value.length > 0) {
                            txtCondt += "<br/><br/>The following Common Configs would be affected by a change to this High Level Part:<br/><br/>";
                            txtCondt += '<table style=\"border-spacing:5px;\"><tr><th><b>CCID</b></th><th><b>Name</b></th><th><b>Template Name</b></th></tr>';
                            for (var i = 0; i < configNameList.length; i++) {
                                var fields = configNameList[i].split('^');
                                txtCondt += '<tr><td>' + fields[0] + '</td><td style=\"white-space:nowrap\">' + fields[1] + '</td><td style=\"white-space:nowrap\">' + fields[2] + '</td></tr>';
                            }
                            txtCondt += '</table>';
                        }
                    }

                    if (txtCondt.length > 0 && textlength > 0 && configNameList.length > 0) {
                        app.showMessage(txtCondt, 'Update Confirmation for HLPN', ['Yes', 'No']).then(function (dialogResult) {
                            if (dialogResult == 'Yes') {
                                selfmi.saveHLPNRecords(cdmmsId, saveHlpn, materialCode, next);
                            }
                        });
                    } else {
                        selfmi.saveHLPNRecords(cdmmsId, saveHlpn, materialCode, next);
                    }
                }

                $("#interstitial").hide();
            },
           function (error) {
               $("#interstitial").hide();
               return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
           });
        };

        MaterialItem.prototype.saveHLPNRecords = function (cdmmsId, saveHlpn, materialCode, next) {
            next = (next || function () { });

            $.ajax({
                type: "POST",
                url: 'api/highlevelpart/validate/' + cdmmsId,
                data: saveHlpn,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: updateSuccess,
                error: updateError
            });
            function updateSuccess(response) {
                if (JSON.parse(response) == "success") {
                    $.ajax({
                        type: "POST",
                        url: 'api/highlevelpart/update/' + materialCode,
                        data: saveHlpn,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: updateSuccess123,
                        error: updateError123
                    });
                }

                function updateSuccess123(data) {
                    var response = JSON.parse(data);

                    if (response.wtd_id > 0) {
                        var helper = new reference();

                        helper.getHighLevelPartSendToNdsStatus(cdmmsId, response.wtd_id);
                    }

                    selfmi.getCITModalHLPN(cdmmsId);
                    selfmi.searchHlpn(null);

                    $("#interstitial").hide();
                    app.showMessage('Saved successfully');
                    next();
                }
                function updateError123() {
                    $("#interstitial").hide();
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');

                }
            }
            function updateError() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            }
        };

        MaterialItem.prototype.getConfigNames = function (materialItemID) {
            var material = { 'id': materialItemID }
            var configNameList = [];
            $.ajax({
                type: "GET",
                url: 'api/audit/getnames',
                data: material,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: getNameSuccess,
                error: getNameError,
                async: false
            });
            function getNameSuccess(response) {
                configNameList = JSON.parse(response);
            }
            function getNameError() {
            }
            return configNameList;
        }

        //MaterialItem.prototype.saveHlpnClose = function () {
        //    selfmi.saveHlpn();
        //    $("#highLevelPartsModal").hide();
        //};
        MaterialItem.prototype.cloneHlpn = function () {
            var cloneMaterialCode = $("#cloneInputHLPN").val();
            var childCdmmsId = $("#cdmmsIdHlpnTxt").text();
            var childMaterialCode = $("#materialCodeHlpnTxt").text();
            $("#interstitial").show();

            http.get('api/highlevelpart/validateMtrlCd/' + cloneMaterialCode).then(function (responseValid) {
                var cloneCdmmId = JSON.parse(responseValid);
                if (cloneCdmmId == 'errorMC') {
                    $("#interstitial").hide();
                    $("#cloneHlpnErrorMsg").show();
                    $("#cloneHlpnSuccessMsg").hide();
                    $("#cloneHlpnErrorMsg").html('<strong>Material Code is Invalid !<Strong>');
                    $("#cloneHlpnSuccessMsg").html('');
                } else if (cloneCdmmId == 'errorCDM') {
                    $("#interstitial").hide();
                    $("#cloneHlpnErrorMsg").show();
                    $("#cloneHlpnSuccessMsg").hide();
                    $("#cloneHlpnErrorMsg").html('<strong>Unable to get CDMMS ID!<Strong>');
                    $("#cloneHlpnSuccessMsg").html('');
                } else {
                    http.get('api/highlevelpart/getCIThlp/' + childCdmmsId).then(function (responseCheckContained) {
                        var responseCheckContainedRes = JSON.parse(responseCheckContained);
                        if (false) {
                            $("#cloneHlpnErrorMsg").show();
                            $("#cloneHlpnSuccessMsg").hide();
                            $("#cloneHlpnErrorMsg").html('Error:Material code already has contained in items.');
                            $("#cloneHlpnSuccessMsg").html('');
                            $("#interstitial").hide();
                        } else {
                            http.get('api/highlevelpart/getCIThlp/' + cloneCdmmId).then(function (response) {
                                if (response != 'no_results') {
                                    var cloneGetResults = JSON.parse(response);
                                    var oldCIT = mapping.toJS(cloneGetResults);
                                    var saveHlpn = { cloneCIT: oldCIT };
                                    saveHlpn = JSON.stringify(saveHlpn);
                                    $.ajax({
                                        type: "POST",
                                        url: 'api/highlevelpart/validate/' + childCdmmsId,
                                        data: saveHlpn,
                                        contentType: "application/json; charset=utf-8",
                                        dataType: "json",
                                        success: updateSuccess,
                                        error: updateError
                                    });
                                    function updateSuccess(response) {
                                        if (JSON.parse(response) == "success") {
                                            $.ajax({
                                                type: "POST",
                                                url: 'api/highlevelpart/clone/' + childMaterialCode,
                                                data: saveHlpn,
                                                contentType: "application/json; charset=utf-8",
                                                dataType: "json",
                                                success: updateSuccessonClone,
                                                error: updateErroronClone
                                            });
                                            function updateSuccessonClone(response) {
                                                var response1 = JSON.parse(response);

                                                if (response1.wtd_id > 0) {
                                                    var helper = new reference();

                                                    helper.getHighLevelPartSendToNdsStatus(childCdmmsId, response1.wtd_id);
                                                }

                                                $("#interstitial").hide();
                                                selfmi.getCITModalHLPN(childCdmmsId);
                                                app.showMessage('Cloned successfully');
                                            }
                                            function updateErroronClone() {
                                                $("#interstitial").hide();
                                                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                                            }
                                        } else {
                                            $("#interstitial").hide();
                                            return app.showMessage('Error occured');
                                        }
                                    }
                                    function updateError() {
                                        $("#interstitial").hide();
                                        return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                                    }

                                } else {
                                    $("#interstitial").hide();
                                    return app.showMessage('Error occured');
                                }
                            },
                                           function (error) {
                                               $("#interstitial").hide();
                                               return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                                           });
                        }
                    },
                     function (error) {
                         $("#interstitial").hide();
                         return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                     });

                }
            },
            function (error) {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            });
        };
        //
        //This function is introduce for exporting the Active material record as excel report
        MaterialItem.prototype.exportMaterialReport = function () {

            selfmi.materialreportgeneration("Material_List_report.xlsx", false);
        }

        //
        //This function is introduce for exporting the Active material record as excel report
        MaterialItem.prototype.materialreportgeneration = function (varfilename, varHlpn) {
            var excel = $JExcel.new("Calibri light 10 #333333");			// Default font
            var valAlign = 0;
            var lineCnt = 0;
            excel.set({ sheet: 0, value: "Material List" }); //Adding sheet1 for Material List            
            excel.addSheet("Active Material List"); //Adding sheet2  for Active Material List            

            //-------------------------------------------------------------------------------------------------------
            //Start writing Searched Material data into sheet 1
            var mtlheaders = ["CDMMS ID", "Material Code", "Part Number", "Material Description", "CLMC", "Status", "Material Category", "Feature Type"
                           , "Cable Type", "Item Status", "Specification Name", "Last Update Date", "User Id"
            ];							// This array holds the HEADERS text

            var mtlformatHeader = excel.addStyle({ 															// Format for headers
                border: "none,none,none,thin #333333", 													    // 		Border for header
                font: "Calibri 12 #1b834c B"
            });

            for (var i = 0; i < mtlheaders.length; i++) {																// Loop all the haders
                excel.set(0, i, 0, mtlheaders[i], mtlformatHeader);													// Set CELL with header text, using header format
                excel.set(0, i, undefined, "auto");															// Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }
            var i = 0;

            for (var i = 0; i < selfMtlEdit.MIList().length; i++) {
                excel.set(0, 0, i + 1, selfMtlEdit.MIList()[i].Attributes.material_item_id.value);              // CDMMS ID
                excel.set(0, 1, i + 1, selfMtlEdit.MIList()[i].Attributes.product_id.value);                    // Material Code
                excel.set(0, 2, i + 1, selfMtlEdit.MIList()[i].Attributes.mfg_part_no.value);                   // Part Number
                excel.set(0, 3, i + 1, selfMtlEdit.MIList()[i].Attributes.item_desc.value);                     // Material Description
                excel.set(0, 4, i + 1, selfMtlEdit.MIList()[i].Attributes.mfg_id.value);                        // CLMC
                excel.set(0, 5, i + 1, selfMtlEdit.MIList()[i].Attributes.stts.value);                          // Status
                excel.set(0, 6, i + 1, selfMtlEdit.MIList()[i].Attributes.mtl_ctgry.value);                     // Material Category
                excel.set(0, 7, i + 1, selfMtlEdit.MIList()[i].Attributes.ftr_typ.value);                       // Feature Type
                excel.set(0, 8, i + 1, selfMtlEdit.MIList()[i].Attributes.cbl_typ.value);                       // Cable Type
                excel.set(0, 9, i + 1, selfMtlEdit.MIList()[i].Attributes.item_current_status.value);           // Item Status
                excel.set(0, 10, i + 1, selfMtlEdit.MIList()[i].Attributes.spec_nm.value);                      // Specification Name
                excel.set(0, 11, i + 1, selfMtlEdit.MIList()[i].Attributes.lastupdt.value);                     // Last Update Date
                excel.set(0, 12, i + 1, selfMtlEdit.MIList()[i].Attributes.userid.value);                       // User Id
            }
            //End - writing Searched Material data into sheet 1
            //-------------------------------------------------------------------------------------------------------           
            //-------------------------------------------------------------------------------------------------------
            //Start writing Active material data into sheet 2
            var activeHeader = excel.addStyle({ 															// Format for headers                
                font: "Calibri 16 #1b834c B"
            });
            excel.set(1, 0, 0, "Active material", activeHeader);

            var headers = ["CDMMS ID", "Material Code", "Part Number", "Material Description", "Catalog Description", "Manufacturer CLMC", "Manufacturer Name"
                           , "HECI", "Unit of Measurement", "Material Group", "Height", "Width", "Depth", "Hazard Indicator", "Material Category", "Feature Type"
                           , "Cable Type", "Set Length", "Set Length Unit", "Specification Name", "Plug-In Role Type", "APCL Code", "Status"
                           , "Labor ID", "Last Updated UserID", "Last Updated Date", "Account Code", "Internal Height", "Internal Width", "Internal Depth"
                           , "Amps Drain", "Equipment Weight", "Frame Name", "Number Mounting Spaces", "Voltage Description", "Location Position Indicator"
                           , "Max Equipment Positions", "Gauge", "Gauge Unit", "Equipment Class", "Product Type", "Part Number Type Code"
            ];							// This array holds the HEADERS text

            var formatHeader = excel.addStyle({ 															// Format for headers
                border: "none,thin #333333,thin #333333,thin #333333", 													    // 		Border for header
                font: "Calibri 12 #1b834c B"
            });

            for (var i = 0; i < headers.length; i++) {																// Loop all the haders
                excel.set(1, i, 1, headers[i], formatHeader);													// Set CELL with header text, using header format
                excel.set(1, i, undefined, "auto");															// Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }
            var i = 1;

            excel.set(1, 0, i + 1, selfmi.mtl.id.value());              // CDMMS ID
            excel.set(1, 1, i + 1, selfmi.mtl.PrdctId.value());         // Material Code
            excel.set(1, 2, i + 1, selfmi.mtl.PrtNo.value());           // Part Number
            excel.set(1, 3, i + 1, selfmi.mtl.ItmDesc.value());         // Material Description
            excel.set(1, 4, i + 1, selfmi.mtl.CtlgDesc.value());        // Catalog Description
            excel.set(1, 5, i + 1, selfmi.mtl.Mfg.value());             // Manufacturer CLMC
            excel.set(1, 6, i + 1, selfmi.mtl.MfgDesc.value());         // Manufacturer Name
            excel.set(1, 7, i + 1, selfmi.mtl.HECI.value());            // HECI
            excel.set(1, 8, i + 1, selfmi.mtl.UOM.value());             // Unit of Measurement
            if (selfmi.mtl.CtgryId.value().indexOf('E') > -1) {
                excel.set(1, 9, i + 1, "'" + selfmi.mtl.CtgryId.value() + "'");   // Material Group, if it contains 'E' (excel treat it as exponential)
            }
            else {
                excel.set(1, 9, i + 1, selfmi.mtl.CtgryId.value());
            }
            excel.set(1, 10, i + 1, selfmi.mtl.Hght.value());                                       // Height
            excel.set(1, 11, i + 1, selfmi.mtl.Wdth.value());                                       // Width
            excel.set(1, 12, i + 1, selfmi.mtl.Dpth.value());                                       // Depth
            excel.set(1, 13, i + 1, selfmi.mtl.HzrdInd.bool());                                     // Hazard Indicator
            valAlign = selfmi.selectedMaterialCategory() == "" ? valAlign : selfmi.selectedMaterialCategory();
            excel.set(1, 14, i + 1, selfmi.mtl.MtlCtgry.options()[valAlign].text());                // Material Category
            valAlign = 0;
            valAlign = selfmi.selectedFeatureType() == "" ? valAlign : selfmi.selectedFeatureType();
            excel.set(1, 15, i + 1, selfmi.mtl.FtrTyp.options()[valAlign].text());                  // Feature Type
            valAlign = 0;
            valAlign = selfmi.selectedCableType() == "" ? valAlign : selfmi.selectedCableType();
            excel.set(1, 16, i + 1, selfmi.mtl.CblTypId.options()[valAlign].text());                // Cable Type 
            valAlign = 0;
            excel.set(1, 17, i + 1, selfmi.mtl.SetLgth.value());                                    // Set Length
            //valAlign = selfmi.selectedSetLgthUom() == "" ? valAlign : selfmi.selectedSetLgthUom();
            var ind1 = selfmi.mtl.SetLgthUom.options().map(function (img) { return img.value(); }).indexOf(selfmi.selectedSetLgthUom());
            excel.set(1, 18, i + 1, selfmi.mtl.SetLgthUom.options()[ind1].text());              // Set Length Unit
            valAlign = 0;
            excel.set(1, 19, i + 1, selfmi.mtl.SpecNm.value());                                     // Specification Name
            valAlign = selfmi.selectedPlugInType() == "" ? valAlign : selfmi.selectedPlugInType();
            excel.set(1, 20, i + 1, selfmi.mtl.PlgInRlTyp.options()[valAlign].text());              // Plug-In Role Type
            valAlign = 0;

            excel.set(1, 21, i + 1, selfmi.selectedApcl());                                         // APCL Code
            valAlign = selfmi.selectedStatus() == "" ? valAlign : selfmi.selectedStatus();
            excel.set(1, 22, i + 1, selfmi.mtl.Stts.options()[valAlign].text());                    // Status
            valAlign = 0;
            //valAlign = selfmi.selectedLbrId() == "" ? valAlign : selfmi.selectedLbrId();
            var ind = selfmi.mtl.LbrId.options.map(function (img) { return img.value; }).indexOf(selfmi.selectedLbrId());
            excel.set(1, 23, i + 1, selfmi.mtl.LbrId.options[ind].text);                            // Labor ID
            valAlign = 0;
            excel.set(1, 24, i + 1, selfmi.mtl.LstUid.value());                                     // Last Updated UserID
            excel.set(1, 25, i + 1, selfmi.mtl.LstDt.value());                                      // Last Updated Date
            excel.set(1, 26, i + 1, selfmi.mtl.AccntCd.value());                                    // Account Code
            excel.set(1, 27, i + 1, selfmi.mtl.IntrlHght.value());                                  // Internal Height
            excel.set(1, 28, i + 1, selfmi.mtl.IntrlWdth.value());                                  // Internal Width:
            excel.set(1, 29, i + 1, selfmi.mtl.IntrlDpth.value());                                  // Internal Depth:
            excel.set(1, 30, i + 1, selfmi.mtl.AmpsDrn.value());                                    // Amps Drain

            excel.set(1, 31, i + 1, selfmi.mtl.EqpWght.value());                                    // Equipment Weight
            excel.set(1, 32, i + 1, selfmi.mtl.FrmNm.value());                                      // Frame Name
            excel.set(1, 33, i + 1, selfmi.mtl.NumMtgSpcs.value());                                 // Number Mounting Spaces
            excel.set(1, 34, i + 1, selfmi.mtl.VltDsc.value());                                     // Voltage Description
            valAlign = selfmi.selectedLctnPosInd() == "" ? valAlign : selfmi.selectedLctnPosInd();
            var indInt = selfmi.mtl.LctnPosInd.options().map(function (img) { return img.value(); }).indexOf(valAlign); // find the index of string value, then find the text.
            valAlign = 0;
            excel.set(1, 35, i + 1, selfmi.mtl.LctnPosInd.options()[indInt].text());              // Location Position Indicator
            excel.set(1, 36, i + 1, selfmi.mtl.MxEqpPos.value());                                   // Max Equipment Positions
            excel.set(1, 37, i + 1, selfmi.mtl.Gge.value());                                        // Gauge
            excel.set(1, 38, i + 1, selfmi.selectedGgeUnt());                                       // Gauge Unit
            excel.set(1, 39, i + 1, selfmi.mtl.EqptCls.value());                                    // Equipment Class
            excel.set(1, 40, i + 1, selfmi.selectedPrdTyp());                                       // Product Type
            excel.set(1, 41, i + 1, selfmi.selectedPrtNbrTypCd());                                  // Part Number Type Code            

            // End of active material data writing in sheet 2
            //-------------------------------------------------------------------------------------------------------
            lineCnt = lineCnt + 1;
            // Associate cable
            //-----------------------------------------------------------------------------------------------------------
            selfmi.associatecablepopup("", "");
            $("#variableCableModal").hide();
            excel.set(1, 0, lineCnt + 3, "Associate Cable", activeHeader);                                  // Associate cable Details
            lineCnt = lineCnt + 4;

            var assoCable = ["CDMMS ID", "Material Code", "Part Number", "CLMC", "Description"];							// This array holds the HEADERS text

            var formatHeader = excel.addStyle({ 															// Format for headers
                border: "none,thin #333333,thin #333333,thin #333333", 													    // 		Border for header
                font: "Calibri 12 #1b834c B"
            });

            for (var i = 0; i < assoCable.length; i++) {																// Loop all the haders
                excel.set(1, i, lineCnt, assoCable[i], formatHeader);													// Set CELL with header text, using header format
                excel.set(1, i, undefined, "auto");															// Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }
            var i = lineCnt;

            if (selfmi.associatetbl()) {
                for (var i = 0; i < selfmi.associatetbl().length; i++) {
                    excel.set(1, 0, lineCnt + 1, selfmi.associatetbl()[i].Attributes.material_item_id.value);       // CDMMS ID
                    excel.set(1, 1, lineCnt + 1, selfmi.associatetbl()[i].Attributes.product_id.value);             // Material Code
                    excel.set(1, 2, lineCnt + 1, selfmi.associatetbl()[i].Attributes.mfg_part_no.value);            // Part Number
                    excel.set(1, 3, lineCnt + 1, selfmi.associatetbl()[i].Attributes.mfg_id.value);                 // CLMC
                    excel.set(1, 4, lineCnt + 1, selfmi.associatetbl()[i].Attributes.mat_desc.value);              // Description                 
                    lineCnt = lineCnt + 1;
                }
            }

            //End of Associate cable
            //----------------------------------------------------------------------------------------------------------
            var sapformat = excel.addStyle({ 															// Format for headers                
                font: "Calibri 16 #6495ed B"
            });

            excel.set(1, 0, lineCnt + 3, "SAP Material Details", sapformat);                                  // SAP Material Details
            lineCnt = lineCnt + 4;
            //-------------------------------------------------------------------------------------------------------
            //Start - Sap material excel report generation in sheet 2.

            var sapheaders = ["CDMMS ID", "Material Code", "Part Number", "Material Description", "Additional Description", "Manufacturer CLMC", "Manufacturer Name"
                           , "HECI", "Unit of Measurement", "APCL Code", "Material Group", "Height", "Width", "Depth", "Hazard Indicator", "Date of Created", "Item Status"
            ];							// This array holds the HEADERS text

            var sapformatHeader = excel.addStyle({ 															// Format for headers
                border: "none,thin #333333,thin #333333,thin #333333", 													    // 		Border for header
                font: "Calibri 12 #6495ed B"
            });

            for (var i = 0; i < sapheaders.length; i++) {																// Loop all the haders
                excel.set(1, i, lineCnt, sapheaders[i], sapformatHeader);													// Set CELL with header text, using header format
                excel.set(1, i, undefined, "auto");															// Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }
            var i = lineCnt;

            excel.set(1, 0, i + 1, selfMtlEdit.sapMaterial().mtl.id.value());              // CDMMS ID
            excel.set(1, 1, i + 1, selfMtlEdit.sapMaterial().mtl.PrdctId.value());         // Material Code
            excel.set(1, 2, i + 1, selfMtlEdit.sapMaterial().mtl.PrtNo.value());           // Part Number
            excel.set(1, 3, i + 1, selfMtlEdit.sapMaterial().mtl.ItmDesc.value());         // Material Description
            excel.set(1, 4, i + 1, selfMtlEdit.sapMaterial().mtl.AddtlDesc.value());       // Additional Description
            excel.set(1, 5, i + 1, selfMtlEdit.sapMaterial().mtl.Mfg.value());             // Manufacturer CLMC
            excel.set(1, 6, i + 1, selfMtlEdit.sapMaterial().mtl.MfgDesc.value());         // Manufacturer Name
            excel.set(1, 7, i + 1, selfMtlEdit.sapMaterial().mtl.HECI.value());            // HECI
            excel.set(1, 8, i + 1, selfMtlEdit.sapMaterial().mtl.UOM.value());             // Unit of Measurement

            excel.set(1, 9, i + 1, selfMtlEdit.sapMaterial().mtl.Apcl.value());             // APCL Code
            excel.set(1, 10, i + 1, selfMtlEdit.sapMaterial().mtl.CtgryId.value());         // Material group
            excel.set(1, 11, i + 1, selfMtlEdit.sapMaterial().mtl.Hght.value());            // Height
            excel.set(1, 12, i + 1, selfMtlEdit.sapMaterial().mtl.Wdth.value());            // Width
            excel.set(1, 13, i + 1, selfMtlEdit.sapMaterial().mtl.Dpth.value());            // Depth
            excel.set(1, 14, i + 1, selfMtlEdit.sapMaterial().mtl.HzrdInd.bool());            // Hazard Indicator
            excel.set(1, 15, i + 1, selfMtlEdit.sapMaterial().mtl.DtCreated.value());       // Date of Created
            excel.set(1, 16, i + 1, selfMtlEdit.sapMaterial().mtl.ItemStatus.value());      // Item Status
            //End - Sap material excel report generation in sheet 2.
            //-------------------------------------------------------------------------------------------------------
            var lossformat = excel.addStyle({ 															// Format for headers                
                font: "Calibri 16 #708090 B"
            });
            excel.set(1, 0, lineCnt + 3, "LossDB Material Details", lossformat);                                  // LossDB Material Details
            lineCnt = lineCnt + 4;
            //-------------------------------------------------------------------------------------------------------
            ////Start - lossDB report generation in sheet 2.         

            var lossDbheaders = ["CDMMS ID", "Part Number", "Equipment Description", "Inventory Description", "Vendor", "Vendor Name"
                           , "CLEI", "Compatible Equipment CLEI(7)", "Equipment Catalog Item ID", "Drawing", "Drawing Issue", "Height"
                           , "Width", "Depth", "Weight", "LS or Series", "Ordering Status", "Ordering Code"
                           , "Heat Generation", "Heat Dissipation"
            ];							// This array holds the HEADERS text

            var lossDbformatHeader = excel.addStyle({ 															// Format for headers
                border: "none,thin #333333,thin #333333,thin #333333", 													    // 		Border for header
                font: "Calibri 12 #708090 B"
            });


            for (var i = 0; i < lossDbheaders.length; i++) {																// Loop all the haders
                excel.set(1, i, lineCnt, lossDbheaders[i], lossDbformatHeader);													// Set CELL with header text, using header format
                excel.set(1, i, undefined, "auto");															// Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }

            var i = lineCnt;

            excel.set(1, 0, i + 1, selfMtlEdit.losdbMaterial().mtl.id.value());                 // CDMMS ID
            excel.set(1, 1, i + 1, selfMtlEdit.losdbMaterial().mtl.PrtNo.value());              // Part Number

            var EqptDscr = typeof (selfMtlEdit.losdbMaterial().mtl.EqptDscr) === "undefined" ? "" : selfMtlEdit.losdbMaterial().mtl.EqptDscr.value();
            var MfgNm = typeof (selfMtlEdit.losdbMaterial().mtl.MfgNm) === "undefined" ? "" : selfMtlEdit.losdbMaterial().mtl.MfgNm.value();
            var CLEI = typeof (selfMtlEdit.losdbMaterial().mtl.CLEI) === "undefined" ? "" : selfMtlEdit.losdbMaterial().mtl.CLEI.value();
            var EqptCtlgItmId = typeof (selfMtlEdit.losdbMaterial().mtl.EqptCtlgItmId) === "undefined" ? "" : selfMtlEdit.losdbMaterial().mtl.EqptCtlgItmId.value();
            var Drwg = typeof (selfMtlEdit.losdbMaterial().mtl.Drwg) === "undefined" ? "" : selfMtlEdit.losdbMaterial().mtl.Drwg.value();
            var DrwgIss = typeof (selfMtlEdit.losdbMaterial().mtl.DrwgIss) === "undefined" ? "" : selfMtlEdit.losdbMaterial().mtl.DrwgIss.value();

            excel.set(1, 2, i + 1, EqptDscr);                                                   // Equipment Description
            excel.set(1, 3, i + 1, selfMtlEdit.losdbMaterial().mtl.CtlgDesc.value());           // Inventory Description
            excel.set(1, 4, i + 1, selfMtlEdit.losdbMaterial().mtl.Mfg.value());                // Vendor           
            excel.set(1, 5, i + 1, MfgNm);                                                      // Vendor Name           
            excel.set(1, 6, i + 1, CLEI);                                                       // CLEI
            excel.set(1, 7, i + 1, selfMtlEdit.losdbMaterial().mtl.UOM.value());                // Compatible Equipment CLEI(7)           
            excel.set(1, 8, i + 1, EqptCtlgItmId);                                              // Equipment Catalog Item ID           
            excel.set(1, 9, i + 1, Drwg);                                                       // Drawing           
            excel.set(1, 10, i + 1, DrwgIss);                                                   // Drawing Issue
            excel.set(1, 11, i + 1, selfMtlEdit.losdbMaterial().mtl.Hght.value());            // Height
            excel.set(1, 12, i + 1, selfMtlEdit.losdbMaterial().mtl.Wdth.value());            // Width
            excel.set(1, 13, i + 1, selfMtlEdit.losdbMaterial().mtl.Dpth.value());            // Depth

            var Wght = typeof (selfMtlEdit.losdbMaterial().mtl.Wght) === "undefined" ? "" : selfMtlEdit.losdbMaterial().mtl.Wght.value();
            var LsOrSrs = typeof (selfMtlEdit.losdbMaterial().mtl.LsOrSrs) === "undefined" ? "" : selfMtlEdit.losdbMaterial().mtl.LsOrSrs.value();
            var OrdStat = typeof (selfMtlEdit.losdbMaterial().mtl.OrdStat) === "undefined" ? "" : selfMtlEdit.losdbMaterial().mtl.OrdStat.value();
            var OrdgCd = typeof (selfMtlEdit.losdbMaterial().mtl.OrdgCd) === "undefined" ? "" : selfMtlEdit.losdbMaterial().mtl.OrdgCd.value();
            var HtGntn = typeof (selfMtlEdit.losdbMaterial().mtl.HtGntn) === "undefined" ? "" : selfMtlEdit.losdbMaterial().mtl.HtGntn.value();
            var HtDssptn = typeof (selfMtlEdit.losdbMaterial().mtl.HtDssptn) === "undefined" ? "" : selfMtlEdit.losdbMaterial().mtl.HtDssptn.value();

            excel.set(1, 14, i + 1, Wght);            // Weight
            excel.set(1, 15, i + 1, LsOrSrs);         // LS or Series
            excel.set(1, 16, i + 1, OrdStat);         // Ordering Status
            excel.set(1, 17, i + 1, OrdgCd);          // Ordering Code
            excel.set(1, 18, i + 1, HtGntn);          // Heat Generation
            excel.set(1, 19, i + 1, HtDssptn);        // Heat Dissipation 
            //End - LossDB report generation in sheet 2.
            //-------------------------------------------------------------------------------------------------------

            ////Start - HLPN report generation .       
            if (varHlpn == true) {
                lineCnt = 0;
                excel.addSheet("HLPN Details");                 // HLPN sheet 5
                //excel.addSheet("HLPN Search Result");           // HLPN search details sheet 6
                //excel.addSheet("HLPN Contained-In Details");    // HLPN Contained-In details sheet 7

                var hlpnHeaders = ["HLP CDMMS ID", "HLP Part Number", "HLP CLMC", "HLP Material Code", "HLP Description"];
                var hlpnSearchHeaders = ["CDMMS ID", "Material Code", "CLMC", "Part Number", "Material Category", "Feature Type", "Revisions?"];	// This array holds the HEADERS text for HLPN
                var hlpnHeadersContainedItems = ["Sequence No", "HLPN CDMMS", "CDMMS ID", "Material Code", "CLMC", "Part Number","Spec Name", "Material Category", "Feature Type", "UOM", "Related To",
                                             "Placement (Front/Rear)", "Revisions?", "EQDES", "HORZ DISP", "Rack Mount Pos", "QTY", "Spare QTY", "Total QTY", "Unselect to Delete"];

                var formatHeader = excel.addStyle({ 															// Format for headers
                    border: "none,thin #333333,thin #333333,thin #333333", 										// Border for header
                    font: "Calibri 12 #1b834c B"
                });

                //Writing HLPN data details into sheet 3
                var hlpnHeader = excel.addStyle({ 															// Format for headers                
                    font: "Calibri 16 #1b834c B"
                });
                excel.set(2, 0, lineCnt, "HLPN", hlpnHeader);

                for (var i = 0; i < hlpnHeaders.length; i++) {													// Loop all the haders
                    excel.set(2, i, lineCnt+1, hlpnHeaders[i], formatHeader);											// Set CELL with header text, using header format
                    excel.set(2, i, undefined, "auto");															// Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
                }

                var i = 1;
                lineCnt = i;
                excel.set(2, 0, i + 1, selfmi.mtl.id.value());              // HLP CDMMS ID
                excel.set(2, 1, i + 1, selfmi.mtl.PrtNo.value());           // HLP Part Number
                excel.set(2, 2, i + 1, selfmi.mtl.Mfg.value());             // HLP CLMC
                excel.set(2, 3, i + 1, selfmi.mtl.PrdctId.value());         // HLP Material Code
                excel.set(2, 4, i + 1, selfmi.mtl.CtlgDesc.value());        // HLP Description
                //End of writing data into sheet 3

                // Writing HLPN search data details into sheet 3
                var hlpnSearchformat = excel.addStyle({ 															// Format for headers                
                    font: "Calibri 16 #6495ed B"
                });

                var hlpnSearchHeader = excel.addStyle({ 															// Format for headers
                    border: "none,thin #333333,thin #333333,thin #333333", 										    // Border for header
                    font: "Calibri 12 #6495ed B"
                });

                excel.set(2, 0, i + 3, "HLPN Search Details", hlpnSearchformat);
                lineCnt = 5;

                for (var i = 0; i < hlpnSearchHeaders.length; i++) {											// Loop all the haders
                    excel.set(2, i, lineCnt, hlpnSearchHeaders[i], hlpnSearchHeader);										// Set CELL with header text, using header format
                    excel.set(2, i, undefined, "auto");															// Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
                }

                if (selfmi.searchHlpn() != null) {
                    for (var i = 0; i < selfmi.searchHlpn().length; i++) {

                        excel.set(2, 0, lineCnt + 1, selfmi.searchHlpn()[i].Attributes.material_item_id.value);        // CDMMS ID
                        excel.set(2, 1, lineCnt + 1, selfmi.searchHlpn()[i].Attributes.product_id.value);          // Material Code
                        excel.set(2, 2, lineCnt + 1, selfmi.searchHlpn()[i].Attributes.mfg_id.value);            // CLMC
                        excel.set(2, 3, lineCnt + 1, selfmi.searchHlpn()[i].Attributes.mfg_part_no.value);        // Part Number
                        excel.set(2, 4, lineCnt + 1, selfmi.searchHlpn()[i].Attributes.mtl_ctgry.value);          // Material Category
                        excel.set(2, 5, lineCnt + 1, selfmi.searchHlpn()[i].Attributes.ftr_typ.value);            // Feature Type
                        excel.set(2, 6, lineCnt + 1, selfmi.searchHlpn()[i].Attributes.revisions.value);            // Revisions
                        lineCnt = lineCnt + 1;
                    }
                }

                //End of writing HLPN search data details into sheet 3

                //Start writng HLPN contained items details in sheet 3.
                var hlpnContainFormat = excel.addStyle({ 															// Format for headers                
                    font: "Calibri 16 #708090 B"
                });

                var hlpnContanedHeader = excel.addStyle({ 															// Format for headers
                    border: "none,thin #333333,thin #333333,thin #333333", 										    // Border for header
                    font: "Calibri 12 #708090 B"
                });

                excel.set(2, 0, lineCnt + 3, "HLPN Contained Item Details", hlpnContainFormat);
                lineCnt = lineCnt + 4;
                for (var i = 0; i < hlpnHeadersContainedItems.length; i++) {									// Loop all the haders
                    excel.set(2, i, lineCnt, hlpnHeadersContainedItems[i], hlpnContanedHeader);								// Set CELL with header text, using header format
                    excel.set(2, i, undefined, "auto");															// Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
                }

                if (selfmi.getCITHlpn() != null) {
                    for (var i = 0; i < selfmi.getCITHlpn().length; i++) {
                        excel.set(2, 0, lineCnt + 1, selfmi.getCITHlpn()[i]._seqNo.value);                // Sequence Numnber 
                        excel.set(2, 1, lineCnt + 1, selfmi.mtl.id.value());                              // HLPN CDMMS id 
                        excel.set(2, 2, lineCnt + 1, selfmi.getCITHlpn()[i].cin_cdmms_Id.value);          // CDMMS ID
                        excel.set(2, 3, lineCnt + 1, selfmi.getCITHlpn()[i].material_code.value);         // Material Code
                        excel.set(2, 4, lineCnt + 1, selfmi.getCITHlpn()[i].clmc.value);                  // CLMC
                        excel.set(2, 5, lineCnt + 1, selfmi.getCITHlpn()[i].part_number.value);           // Part Number
                        excel.set(2, 6, lineCnt + 1, selfmi.getCITHlpn()[i].specn_nm.value);              // Spec name
                        excel.set(2, 7, lineCnt + 1, selfmi.getCITHlpn()[i].material_category.value);     // Material Category
                        excel.set(2, 8, lineCnt + 1, selfmi.getCITHlpn()[i].feature_type.value);          // Feature Type
                        excel.set(2, 9, lineCnt + 1, selfmi.getCITHlpn()[i].mtrlUOM.value);               // UOM
                        if (selfmi.getCITHlpn()[i].parent_part.value == null) {
                            excel.set(2, 10, lineCnt + 1, "");                                               //Passing blank for undefined Related To values
                        }
                        else {
                            excel.set(2, 10, lineCnt + 1, selfmi.getCITHlpn()[i].parent_part.value);         // Related To
                        }
                        excel.set(2, 11, lineCnt + 1, selfmi.getCITHlpn()[i].placement_front_rear.value);    // Placement (Front/Rear)
                        excel.set(2, 12, lineCnt + 1, selfmi.getCITHlpn()[i].is_revision.value);             // Revisions?
                        excel.set(2, 13, lineCnt + 1, selfmi.getCITHlpn()[i].ycoord.value);                 // EQDES
                        excel.set(2, 14, lineCnt + 1, selfmi.getCITHlpn()[i].xcoord.value);                 // HORZ Disp
                        excel.set(2, 15, lineCnt + 1, selfmi.getCITHlpn()[i].rack_pos.value);                 // Rack Mount Pos
                        excel.set(2, 16, lineCnt + 1, selfmi.getCITHlpn()[i].quantity.value);               // QTY
                        excel.set(2, 17, lineCnt + 1, selfmi.getCITHlpn()[i].spare_quantity.value);         // Spare QTY
                        excel.set(2, 18, lineCnt + 1, selfmi.getCITHlpn()[i].total_quantity.value);         // Total QTY
                        excel.set(2, 19, lineCnt + 1, selfmi.getCITHlpn()[i].is_selected.bool);             // Unselect to Delete
                        lineCnt = lineCnt + 1;
                    }
                }
                // End of writng HLPN contained items details in sheet 3.

            }
            //End - LossDB report generation in sheet 5.

            excel.generate(varfilename);
        };
        // End of exporting active material record as excel file report

        //This function is introduce for exporting the HLPN record as excel report
        MaterialItem.prototype.exportHlpnReport = function () {
            selfmi.materialreportgeneration("HLPN_report.xlsx", true);
        };
        // End of exporting HLPN record as excel file report
        //
        return MaterialItem;
    });