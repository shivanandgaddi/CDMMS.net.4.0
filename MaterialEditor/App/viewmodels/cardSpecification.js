define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', './cardSpecification', 'durandal/app', '../Utility/referenceDataHelper', 'bootstrapJS', 'plugins/router', '../Utility/user'],
    function (composition, ko, $, http, activator, mapping, system, cardSpec, app, reference, bootstrapJS, router, user) {

        var CardSpecification = function (data) {
            selfcard = this;

            specChangeArray = [];
            var dataResult = data.resp;
            var results = JSON.parse(dataResult);
            selfcard.specification = data.specification;

            selfcard.selectedCardSpec = ko.observable();
            if (results.Card != null && results.Card.list.length > 0) {
                results.Card.list[0].Dpth.value = Number(results.Card.list[0].Dpth.value).toFixed(3);
                results.Card.list[0].Hght.value = Number(results.Card.list[0].Hght.value).toFixed(3);
                results.Card.list[0].Wdth.value = Number(results.Card.list[0].Wdth.value).toFixed(3);
                // Ensure there is a leading zero when a fraction like .75 comes across.  should display as 0.75
                if (results.Card.list[0].Wdth.value < 1 && results.Card.list[0].Wdth.value > 0 && results.Card.list[0].Wdth.value.substring(0, 1) == '.') {
                    results.Card.list[0].Wdth.value = '0' + results.Card.list[0].Wdth.value;
                }
                if (results.Card.list[0].Hght.value < 1 && results.Card.list[0].Hght.value > 0 && results.Card.list[0].Hght.value.substring(0, 1) == '.') {
                    results.Card.list[0].Hght.value = '0' + results.Card.list[0].Hght.value;
                }
                if (results.Card.list[0].Dpth.value < 1 && results.Card.list[0].Dpth.value > 0 && results.Card.list[0].Dpth.value.substring(0, 1) == '.') {
                    results.Card.list[0].Dpth.value = '0' + results.Card.list[0].Dpth.value;
                }
            }
            selfcard.selectedCardSpec(results);
            selfcard.duplicatecardRoleTypeListTbl = ko.observableArray();
            selfcard.duplicatecardRoleTypeListTbl = [];
            selfcard.duplicateSelectedCardSpecName = ko.observable();
            selfcard.duplicateSelectedCardSpecName = results.RvsnNm.value;
            selfcard.duplicateSelectedCardSpecDpth = ko.observable();
            selfcard.duplicateSelectedCardSpecHght = ko.observable();
            selfcard.duplicateSelectedCardSpecWdth = ko.observable();
            selfcard.duplicateSelectedCardSpecDltd = ko.observable();
            selfcard.duplicateSelectedCardSpecDltd = results.Dltd.bool;
            selfcard.duplicateCardNDSUseTyp = ko.observable()
            selfcard.duplicateCardNDSUseTyp(selfcard.selectedCardSpec().CardNDSUseTyp.value);
            selfcard.navFromdeviceModel = ko.observable();
            selfcard.navFromdeviceModel(false);

            if (results.SpcnRlTypLst.list != null && results.SpcnRlTypLst.list.length > 0) {
                for (var x = 0; x < results.SpcnRlTypLst.list.length; x++) {
                    selfcard.duplicatecardRoleTypeListTbl[x] = results.SpcnRlTypLst.list[x].Slctd.bool;
                }
            }

            selfcard.navFromdeviceModel = false;
            if (results.Card != null && results.Card.list.length > 0) {
                selfcard.duplicateSelectedCardSpecDpth(results.Card.list[0].Dpth.value);
                selfcard.duplicateSelectedCardSpecHght(results.Card.list[0].Hght.value);
                selfcard.duplicateSelectedCardSpecWdth(results.Card.list[0].Wdth.value);
            }
            selfcard.duplicateSelectedCardSpecStrghtThru = ko.observable(results.StrghtThru.value);

            selfcard.associatemtlblck = ko.observableArray();
            selfcard.searchtblcard = ko.observableArray();
            selfcard.specnlistdetails = ko.observableArray();
            selfcard.consumptionlistdetails = ko.observableArray();
            selfcard.specnlistdetails(selfcard.selectedCardSpec().SpcnRlTypLst.list);
            selfcard.consumptionlistdetails(selfcard.selectedCardSpec().SltCnsmptnLst.list);
            selfcard.enableName = ko.observable(true);
            selfcard.enableAssociate = ko.observable(true);
            selfcard.enableModalSave = ko.observable(false);
            selfcard.enableBackButton = ko.observable(false);
            selfcard.slotListdetails = ko.observableArray('');
            selfcard.ManageSlotslist = ko.observableArray('');
            selfcard.portsListdetails = ko.observableArray('');
            selfcard.RotationAngle = ko.observableArray();
            selfcard.selectedRotatnAngleId = ko.observable();
            selfcard.AddJsonres = ko.observable('');
            selfcard.rotationAnglist();
            selfcard.actionCode = ko.observable('');
            selfcard.sltQnty = ko.observable('');
            selfcard.sltSeq = ko.observable('');
            selfcard.sltDefId = ko.observable('');

            $("#hasSlotDiv").hide();
            $("#hasPortDiv").hide();

            if (selfcard.selectedCardSpec().Slts.bool == true) {
                $("#hasSlotDiv").show();
                selfcard.onLoadHasSlotDtls();
            }

            if (selfcard.selectedCardSpec().Prts.bool == true) {
                $("#hasPortDiv").show();
                selfcard.onLoadHasPortsDtls();
            }

            selfcard.statusSpecs = ko.observableArray(['', 'Completed', 'Propagated', 'Deleted']);
            selfcard.portRole = ko.observableArray(['', 'Connection', 'Management', 'Miscellaneous']);
            selfcard.roleSelect = ko.observable('');
            selfcard.cardSlotSequence = ko.observableArray('');
            selfcard.status = ko.observable('');
            selfcard.cardSpecification = ko.observable();
            selfcard.cardPortTypeList = ko.observableArray('');
            selfcard.cardCnctrListDtls = ko.observableArray('');
            selfcard.portDefId = ko.observable('');
            selfcard.portDefId = 0;
            selfcard.ConfigPortCountlist = ko.observableArray('');
            selfcard.duplicateSelectedCardSpecName = ko.observable();
            selfcard.ConfigCardPortList = ko.observableArray('');
            selfcard.DeleteConfigCardPortList = [];
            selfcard.FrontRearTypes = ko.observableArray(['', 'F', 'R']);
            selfcard.selectedCardPortDef = ko.observable();
            $(document).ready(function () {
                $('#tblslotList').DataTable({
                    "pagingType": "simple"// false to disable pagination (or any other option)
                });
            });
            //selfcard.disablepropt = ko.observable(false);
            //selfcard.disablecmplt = ko.observable(false);
            //selfcard.disabledlt = ko.observable(false);
            //selfcard.intmcheckblock = ko.observable(false);
            selfcard.completedNotSelected = ko.observable(true);
            //selfcard.intmchk = ko.observable(false);
            //selfcard.rcrdolychk = ko.observable(false);

            //if (selfcard.selectedCardSpec.Itrm.bool())
            //{
            //    selfcard.intmcheckblock(true);
            //}
            //else
            //{
            //    selfcard.intmcheckblock(false);  
            //}           
            if (selfcard.selectedCardSpec().Card && selfcard.selectedCardSpec().Card.list[0].CtlgDesc == undefined) {
                selfcard.selectedCardSpec().Card.list[0].CtlgDesc = { "value": "" };
            }

            if (selfspec.selectRadioSpec() == 'newSpec') {
                selfcard.selectedCardSpec().Wdth.value = '';
                selfcard.selectedCardSpec().Dpth.value = '';
                selfcard.selectedCardSpec().Hght.value = '';
            }
            if (selfspec.backToMtlItmId > 0) {
                selfcard.enableBackButton(true);
            }
            //if (selfcard.selectedCardSpec().RO.bool == true)
            //{
            //    selfcard.rcrdolychk(false);
            //    selfcard.intmchk(false);
            //}


            //if (selfcard.selectedCardSpec().Prts.bool) {
            //    selfcard.enableAssociate(true);           
            //}

            if (selfcard.selectedCardSpec().Cmplt.bool && selfcard.selectedCardSpec().Prpgtd.enable) {
                selfcard.completedNotSelected(false);
            }

            //if (selfcard.selectedCardSpec().Prpgtd.bool == true) {

            //    selfcard.disablepropt(false);
            //}
            //else {
            //    selfcard.disablepropt(true);
            //}
            //if (selfcard.selectedCardSpec().Cmplt.bool == true) {
            //    selfcard.disablecmplt(false);
            //}
            //else {
            //    selfcard.disablecmplt(true);

            //}
            //if (selfcard.selectedCardSpec().Dltd.bool == true) {
            //    selfcard.disabledlt(false);
            //}
            //else {
            //    selfcard.disabledlt(true);

            //}

            var useTypeOptions = new Array();
            for (var x = 0; x < results.SpcnUseTypLst.list.length; x++) {
                useTypeOptions[x] = results.SpcnUseTypLst.list[x].SpcnRlTyp.value
            }
            selfcard.useTypeList = ko.observableArray();
            selfcard.useTypeList(useTypeOptions);

            if (selfcard.selectedCardSpec().CardNDSUseTyp.value === '') {
                selfcard.selectedCardSpec().CardNDSUseTyp.value = 'mit_card';  //default
            }

            setTimeout(function () {
                if (document.getElementById('idCardDimUom') !== null) {
                    if (document.getElementById('idCardDimUom').value == '') {
                        document.getElementById('idCardDimUom').value = '22';
                    }
                }
            }, 5000);

            setTimeout(function () {
                $(document).ready(function () {
                    $('input.slttblChkbslt').on('change', function () {
                        $('input.slttblChkbslt').not(this).prop('checked', false);
                        console.log('SltCnsmptnId = ' + selfcard.selectedCardSpec().SltCnsmptnId.value);
                        selfcard.selectedCardSpec().SltCnsmptnId.value = $(this).val();
                        console.log('SltCnsmptnId = ' + selfcard.selectedCardSpec().SltCnsmptnId.value);
                        $("#sltconsumptionselnt").hide();
                    });
                });
            }, 2000);
            $(document).on('change', '[type=checkbox]', function () {
                if (this.name == "cmpindnmcrd") {
                    if (this.checked == false) {
                        document.getElementById('proptIndcard').checked = false;
                    }
                }
            });
            //$(document).on('change', '[type=checkbox]', function () {
            //    if (this.name == "intmcardchk") {
            //        if (this.checked == true) {
            //            selfcard.intmcheckblock(true);
            //            document.getElementById('rcrolyCard').disabled = true;
            //        }
            //        else {
            //            selfcard.intmcheckblock(false);
            //            document.getElementById('rcrolyCard').disabled = false;
            //        }
            //    }
            //});

            //$(document).on('change', '[type=checkbox]', function () {
            //    if (this.name == "proptIndBay") {
            //        if (this.checked == true) {
            //            selfcard.enableAssociate(true);
            //        }
            //        else {
            //            selfcard.enableAssociate(false);
            //        }
            //    }
            //});

            //$(document).on('change', '[type=checkbox]', function () {
            //    if (this.name == "rcrdcardchk") {
            //        if (this.checked == true) {
            //            selfcard.intmcheckblock(false);
            //            document.getElementById('intmCard').disabled = true;
            //        }
            //        else {                    
            //            document.getElementById('intmCard').disabled = false;
            //        }
            //    }
            //});

            //if (selfcard.selectedCardSpec().Card !== undefined) {
            //    if (selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnNrmUom.value == "" && document.getElementById('txtNrmlDrn')) {
            //        document.getElementById('txtNrmlDrn').value = "";
            //    }

            //    if (selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnNrm.value == "" && document.getElementById('idNrmlDrnUom')) {
            //        document.getElementById('idNrmlDrnUom').value = "";
            //    }

            //    if (selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnMxUom.value == "" && document.getElementById('txtPkDrn')) {
            //        document.getElementById('txtPkDrn').value = "";
            //    }

            //    if (selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnMx.value == "" && document.getElementById('idPkDrnUom')) {
            //        document.getElementById('idPkDrnUom').value = "";
            //    }

            //    if (selfcard.selectedCardSpec().Card.list[0].HtDssptnUom.value == "" && document.getElementById('txtPwr')) {
            //        document.getElementById('txtPwr').value = "";
            //    }

            //    if (selfcard.selectedCardSpec().Card.list[0].HtDssptn.value == "" && document.getElementById('idPwrUom')) {
            //        document.getElementById('idPwrUom').value = "";
            //    }

            //    if (selfcard.selectedCardSpec().Card.list[0].Wght.value == "" && document.getElementById('idWtUom')) {
            //        document.getElementById('idWtUom').value = "";
            //    }

            //    if (selfcard.selectedCardSpec().Card.list[0].WghtUom.value == "" && document.getElementById('txtWt')) {
            //        document.getElementById('txtWt').value = "";
            //    }
            //}

            setTimeout(function () {
                if (selfcard.selectedCardSpec().Card !== undefined) {
                    document.getElementById('txtDpth').required = true;
                    document.getElementById('txtHght').required = true;
                    document.getElementById('txtWdth').required = true;
                }

            }, 5000);

        };

        CardSpecification.prototype.onchangeCompleted = function () {
            if ($("#cmpIndcard").is(':checked')) {
                selfcard.completedNotSelected(false);
            } else {
                selfcard.completedNotSelected(true);
                selfcard.selectedCardSpec().Prpgtd.bool = false;
            }
        };
        CardSpecification.prototype.navigateToMaterial = function () {
            if (document.getElementById('rcrolyCard').checked == true) {
                var url = '#/roNew/' + selfspec.backToMtlItmId;
                router.navigate(url, false);
            }
            else {
                var url = '#/mtlInv/' + selfspec.backToMtlItmId;
                router.navigate(url, false);
            }


        };
        //CardSpecification.prototype.onchangecardInterim = function () {
        //    if ($("#intmCard").is(':checked')) {
        //        selfcard.intmcheckblock(true);
        //        selfcard.selectedCardSpec.RO.enable(false);
        //        document.getElementById("idbaydptno").required = true;
        //        document.getElementById("idwdth").required = true;
        //        document.getElementById("idhetNo").required = true;
        //        document.getElementById("idUOmtyp").required = true;
        //    } else {
        //        selfcard.intmcheckblock(false);
        //        selfcard.selectedCardSpec.RO.enable(true);
        //        document.getElementById("idbaydptno").required = false;
        //        document.getElementById("idwdth").required = false;
        //        document.getElementById("idhetNo").required = false;
        //        document.getElementById("idUOmtyp").required = false;
        //    }

        //};

        CardSpecification.prototype.NumDecimal = function (mp, event) {
            //self.saveValidation(true);
            var charCode = event.keyCode;
            var name = event.currentTarget.id;
            var value = $('#' + name).val();
            var reqCharAfterdot = 3;

            if (name == "txtHght" || name == "txtWdth" || name == "txtDpth") {
                reqCharAfterdot = 3;
            }

            if (charCode > 31 && (charCode < 48 || charCode > 57) && !(charCode == 46 || charCode == 8)) {
                return false;
            } else {
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

        CardSpecification.prototype.SaveCard = function () {
            $("#interstitial").show();
            specChangeArray = [];

            if ((selfcard.selectedCardSpec().Card !== undefined)) {

                // check for duplicate model name
                var modelname = document.getElementById("modelcard").value;
                if (selfspec.selectRadioSpec() == 'existSpec') {
                    var modelnamecount = selfcard.GetModelNameCount(modelname, selfcard.selectedCardSpec().Card.list[0].SpecId.value);
                }
                else {
                    var modelnamecount = selfcard.GetModelNameCount(modelname, 0);
                }
                if (modelnamecount > 0) {
                    app.showMessage('The model name ' + modelname + ' already exists on a different Spec.');
                    return;
                }

                selfcard.selectedCardSpec().SpcnRlTypLst.list = selfcard.specnlistdetails();

                var arr = [];
                var priorityNull = '';

                selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnNrm.value = $("#txtNrmlDrn").val();
                selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnMx.value = $("#txtPkDrn").val();
                selfcard.selectedCardSpec().Card.list[0].HtDssptn.value = $("#txtPwr").val();
                selfcard.selectedCardSpec().Card.list[0].Wght.value = $("#txtWt").val();

                for (var i = 0; i < selfcard.specnlistdetails().length; i++) {
                    if (selfcard.specnlistdetails()[i].PrtyNo.value != 0) {
                        arr.push(selfcard.specnlistdetails()[i].PrtyNo.value)
                    } else {
                        if (selfcard.specnlistdetails()[i].Slctd.bool) {
                            priorityNull += "Cannot have priority <b>'0'</b> for a selected Role " + selfcard.specnlistdetails()[i].SpcnRlTyp.value;
                        }
                    }
                }

                var sorted_arr = arr.slice().sort();

                var duplicatePriority = [];

                for (var i = 0; i < sorted_arr.length - 1; i++) {
                    if (sorted_arr[i + 1] == sorted_arr[i]) {
                        if (duplicatePriority.indexOf(sorted_arr[i]) < 0) {
                            duplicatePriority.push(sorted_arr[i]);
                        }
                    }
                }

                var errorMessage = '';

                for (var j = 0; j < duplicatePriority.length; j++) {
                    for (var i = 0; i < selfcard.specnlistdetails().length; i++) {
                        if (selfcard.specnlistdetails()[i].PrtyNo.value == duplicatePriority[j]) {
                            errorMessage += selfcard.specnlistdetails()[i].SpcnRlTyp.value + ", ";
                        }
                    }

                    errorMessage = errorMessage.substring(0, errorMessage.length - 2);
                    errorMessage += " having same priority <strong>" + duplicatePriority[j] + "</strong><br/><br/>";
                }

                console.log(duplicatePriority);

                if (duplicatePriority.length > 0 || priorityNull.length > 0) {
                    //$("#interstitial").hide();
                    errorMessage += priorityNull;
                    $("#roleTypeValidTblDgr").html(errorMessage);
                    $("#roleTypeValidTblDgr").show();
                }
                else {
                    var slotselected = false;
                    var slotselectedchk = $("#slotconsumptnList .slttblChkbslt");

                    for (var i = 0; i < slotselectedchk.length; i++) {
                        if (slotselectedchk[i].checked == true) {
                            slotselected = true;
                        }
                    }

                    var txtCondt = '';
                    selfcard.updateChbkCheck();
                    if (selfspec.selectRadioSpec() == 'existSpec') {
                        if (selfcard.selectedCardSpec().Card != null && selfcard.selectedCardSpec().Card.list.length > 0) {
                            if (selfcard.selectedCardSpec().RvsnNm.value != selfcard.duplicateSelectedCardSpecName) {
                                txtCondt += "Name changed to <b>" + selfcard.selectedCardSpec().RvsnNm.value + '</b> from ' + selfcard.duplicateSelectedCardSpecName + "<br/>";

                                var saveJSON = {
                                    'tablename': 'card_specn', 'columnname': 'card_specn_nm', 'audittblpkcolnm': 'card_specn_id', 'audittblpkcolval': selfcard.selectedCardSpec().Card.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfcard.duplicateSelectedCardSpecName,
                                    'newcolval': selfcard.selectedCardSpec().RvsnNm.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfcard.selectedCardSpec().RvsnNm.value + ' Spec Name from ' + selfcard.duplicateSelectedCardSpecName + ' on ', 'materialitemid': selfcard.selectedCardSpec().Card.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }
                            if (Number(selfcard.selectedCardSpec().Card.list[0].Dpth.value).toFixed(3) != Number(selfcard.duplicateSelectedCardSpecDpth()).toFixed(3)) {
                                txtCondt += "Depth changed to <b>" + Number(selfcard.selectedCardSpec().Card.list[0].Dpth.value).toFixed(3) + '</b> from ' + Number(selfcard.duplicateSelectedCardSpecDpth()).toFixed(3) + "<br/>";

                                var saveJSON = {
                                    'tablename': 'rme_card_mtrl', 'columnname': 'dpth_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfcard.selectedCardSpec().Card.list[0].MtrlId.value, 'auditprnttblpkcolnm': '',
                                    'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfcard.duplicateSelectedCardSpecDpth()).toFixed(3), 'newcolval': Number(selfcard.selectedCardSpec().Card.list[0].Dpth.value).toFixed(3), 'cuid': user.cuid,
                                    'cmnttxt': user.cuid + ' changed ' + selfcard.selectedCardSpec().RvsnNm.value + ' Depth from ' + Number(selfcard.duplicateSelectedCardSpecDpth()).toFixed(3) + ' to ' + Number(selfcard.selectedCardSpec().Card.list[0].Dpth.value).toFixed(3) + ' on ',
                                    'materialitemid': selfcard.selectedCardSpec().Card.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }
                            if (Number(selfcard.selectedCardSpec().Card.list[0].Hght.value).toFixed(3) != Number(selfcard.duplicateSelectedCardSpecHght()).toFixed(3)) {
                                txtCondt += "Height changed to <b>" + Number(selfcard.selectedCardSpec().Card.list[0].Hght.value).toFixed(3) + '</b> from ' + Number(selfcard.duplicateSelectedCardSpecHght()).toFixed(3) + "<br/>";

                                var saveJSON = {
                                    'tablename': 'rme_card_mtrl', 'columnname': 'hgt_no', 'audittblpkcolval': selfcard.selectedCardSpec().Card.list[0].MtrlId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': Number(selfcard.duplicateSelectedCardSpecHght()).toFixed(3),
                                    'newcolval': Number(selfcard.selectedCardSpec().Card.list[0].Hght.value).toFixed(3),
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfcard.selectedCardSpec().RvsnNm.value + ' Height from ' + Number(selfcard.duplicateSelectedCardSpecHght()).toFixed(3) + ' to ' + Number(selfcard.selectedCardSpec().Card.list[0].Hght.value).toFixed(3) + ' on ', 'materialitemid': selfcard.selectedCardSpec().Card.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }
                            if (Number(selfcard.selectedCardSpec().Card.list[0].Wdth.value).toFixed(3) != Number(selfcard.duplicateSelectedCardSpecWdth()).toFixed(3)) {
                                txtCondt += "Width changed to <b>" + Number(selfcard.selectedCardSpec().Card.list[0].Wdth.value).toFixed(3) + '</b> from ' + Number(selfcard.duplicateSelectedCardSpecWdth()).toFixed(3) + "<br/>";

                                var saveJSON = {
                                    'tablename': 'rme_card_mtrl', 'columnname': 'wdth_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfcard.selectedCardSpec().Card.list[0].MtrlId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': Number(selfcard.duplicateSelectedCardSpecWdth()).toFixed(3),
                                    'newcolval': Number(selfcard.selectedCardSpec().Card.list[0].Wdth.value).toFixed(3),
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfcard.selectedCardSpec().RvsnNm.value + ' Width from ' + Number(selfcard.duplicateSelectedCardSpecWdth()).toFixed(3) + ' to ' + Number(selfcard.selectedCardSpec().Card.list[0].Wdth.value).toFixed(3) + ' on ', 'materialitemid': selfcard.selectedCardSpec().Card.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }
                            if (selfcard.selectedCardSpec().StrghtThru.value != selfcard.duplicateSelectedCardSpecStrghtThru()) {
                                txtCondt += "Straight Through changed to <b>" + selfcard.selectedCardSpec().StrghtThru.value + '</b> from ' + selfcard.duplicateSelectedCardSpecStrghtThru() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'card_specn', 'columnname': 'strght_thru_ind', 'audittblpkcolnm': 'card_specn_id', 'audittblpkcolval': selfcard.selectedCardSpec().Card.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfcard.duplicateSelectedCardSpecStrghtThru(),
                                    'newcolval': selfcard.selectedCardSpec().StrghtThru.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfcard.selectedCardSpec().RvsnNm.value + ' Straight Thru from ' + selfcard.duplicateSelectedCardSpecStrghtThru() + ' to ' + selfcard.selectedCardSpec().StrghtThru.value + ' on ', 'materialitemid': selfcard.selectedCardSpec().Card.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }
                            if (selfcard.selectedCardSpec().Dltd.bool != selfcard.duplicateSelectedCardSpecDltd) {
                                txtCondt += "Unusable changed to <b>" + selfcard.selectedCardSpec().Dltd.bool + '</b> from ' + selfcard.duplicateSelectedCardSpecDltd + "<br/>";

                                var saveJSON = {
                                    'tablename': 'card_specn_revsn_alt', 'columnname': 'del_ind', 'audittblpkcolnm': 'card_specn_revsn_alt_id', 'audittblpkcolval': selfcard.selectedCardSpec().Card.list[0].SpecRvsnId.value, 'auditprnttblpkcolnm': 'card_specn_id', 'auditprnttblpkcolval': selfcard.selectedCardSpec().Card.list[0].SpecId.value, 'actncd': 'C',
                                    'oldcolval': selfcard.duplicateSelectedCardSpecDltd,
                                    'newcolval': selfcard.selectedCardSpec().Dltd.bool,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfcard.selectedCardSpec().RvsnNm.value + ' Unusable from ' + selfcard.duplicateSelectedCardSpecDltd + ' to ' + selfcard.selectedCardSpec().Dltd.bool + ' on ', 'materialitemid': selfcard.selectedCardSpec().Card.list[0].id.value

                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfcard.duplicateCardNDSUseTyp() != selfcard.selectedCardSpec().CardNDSUseTyp.value) {
                                txtCondt += "Use Type changed to <b>" + selfcard.selectedCardSpec().CardNDSUseTyp.value + '</b> from ' + selfcard.duplicateCardNDSUseTyp() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'card_specn', 'columnname': 'specn_record_use_typ_id', 'audittblpkcolnm': 'card_specn_id', 'audittblpkcolval': selfcard.selectedCardSpec().Card.list[0].SpecId.value, 'auditprnttblpkcolnm': 'card_specn_id', 'auditprnttblpkcolval': selfcard.selectedCardSpec().Card.list[0].SpecId.value, 'actncd': 'C',
                                    'oldcolval': selfcard.duplicateCardNDSUseTyp(),
                                    'newcolval': selfcard.selectedCardSpec().CardNDSUseTyp.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfcard.selectedCardSpec().RvsnNm.value + ' Use Type from ' + selfcard.duplicateCardNDSUseTyp() + ' to ' + selfcard.selectedCardSpec().CardNDSUseTyp.value + ' on ', 'materialitemid': selfcard.selectedCardSpec().Card.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            // check selected role types for use type differences
                            var didItOnce = false;
                            for (var x = 0; x < selfcard.specnlistdetails().length; x++) {
                                if (selfcard.duplicatecardRoleTypeListTbl[x] != selfcard.specnlistdetails()[x].Slctd.bool) {
                                    if (selfcard.selectedCardSpec().CardUseTypId.value != selfcard.specnlistdetails()[x].UseTyp.value) {
                                        if (!didItOnce) {
                                            txtCondt += "Use Type changed to <b>" + selfcard.specnlistdetails()[x].UseTyp.value + '</b> from ' + selfcard.selectedCardSpec().CardUseTypId.value + "<br/>";
                                            didItOnce = true;
                                        }
                                    }
                                }
                            }
                            // check to see if none are selected
                            var numberSelected = 0;
                            for (var x = 0; x < selfcard.specnlistdetails().length; x++) {
                                if (selfcard.specnlistdetails()[x].Slctd.bool == true) {
                                    numberSelected++;
                                }
                            }
                            if (numberSelected == 0) {
                                // check to see if selected alias is being unchecked back to the default
                                if (selfcard.selectedCardSpec().CardUseTypId.value != 'mit_card' && !didItOnce) {
                                    txtCondt += "Use Type changed to <b>mit_shelf</b> from " + selfcard.selectedCardSpec().CardUseTypId.value + "<br/>";
                                }
                            }
                        }
                    }

                    var textlength = txtCondt.length;
                    if (selfcard.selectedCardSpec().Card) {
                        var configNameList = selfcard.getConfigNames(selfcard.selectedCardSpec().Card.list[0].id.value);
                        if (configNameList.length > 0) {

                            txtCondt += "<br/><br/>The following Common Configs would be affected by a change to this spec:<br/><br/>";
                            txtCondt += '<table style=\"border-spacing:5px;\"><tr><th><b>CCID</b></th><th><b>Name</b></th><th><b>Template Name</b></th></tr>';
                            for (var i = 0; i < configNameList.length; i++) {
                                var fields = configNameList[i].split('^');
                                txtCondt += '<tr><td>' + fields[0] + '</td><td style=\"white-space:nowrap\">' + fields[1] + '</td><td style=\"white-space:nowrap\">' + fields[2] + '</td></tr>';
                                //txtCondt += configNameList[i] + "<br/>";
                            }
                            txtCondt += '</table>';
                        }
                    }

                    if (txtCondt.length > 0 && textlength > 0) {
                        app.showMessage(txtCondt, 'Update Confirmation for Card', ['Yes', 'No']).then(function (dialogResult) {
                            if (dialogResult == 'Yes') {
                                selfcard.saveCardSpec(slotselected);
                            }
                        });
                    } else {
                        selfcard.saveCardSpec(slotselected);
                    }
                }
            } else {
                $("#interstitial").hide();
                return app.showMessage('Please associate a material item to the specification before saving.', 'Specification');
            }

            $("#interstitial").hide();
        };

        CardSpecification.prototype.saveCardSpec = function (slotselected) {
            if (slotselected) {

                var saveJSON = mapping.toJS(selfcard.selectedCardSpec());

                $.ajax({
                    type: "POST",
                    url: 'api/specn/update/',
                    data: JSON.stringify(saveJSON),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: updateSuccess,
                    error: updateError
                });

                function updateSuccess(response) {
                    var specResponseOnSuccess = JSON.parse(response);

                    if ("Success" == specResponseOnSuccess.Status) {
                        selfcard.saveAuditChanges();
                        var specWorkToDoId = specResponseOnSuccess.SpecWTD;
                        var mtlWorkToDoId = specResponseOnSuccess.MtlWTD;

                        if (specWorkToDoId !== 0) {
                            var specHelper = new reference();

                            specHelper.getSpecificationSendToNdsStatus(specResponseOnSuccess.Id, specWorkToDoId, 'CARD');
                        }

                        if (mtlWorkToDoId !== 0) {
                            var mtlHelper = new reference();

                            mtlHelper.getSendToNdsStatus(specResponseOnSuccess.MtlItmId, mtlWorkToDoId);
                        }

                        var backMtrlId = selfspec.backToMtlItmId;
                        if (selfspec.selectRadioSpec() == 'newSpec') {
                            selfspec.selectRadioSpec('existSpec');
                            $("#idProductID").val(specResponseOnSuccess.Id);
                            $('#idspectype').val("CARD");
                            // selfspec.Searchspec();
                            selfspec.specificationSelected(specResponseOnSuccess.Id, 'CARD', selfspec, backMtrlId);
                        } else {
                            selfspec.selectRadioSpec('existSpec');
                            $("#idProductID").val(specResponseOnSuccess.Id);
                            $('#idspectype').val("CARD");
                            if (selfcard.specification == true) {
                                selfspec.updateOnSuccess();
                            }
                        }
                        if (backMtrlId > 0) {
                            selfspec.backToMtlItmId = backMtrlId;
                            selfcard.enableBackButton(true);
                        }

                        $("#interstitial").hide();

                        return app.showMessage('Successfully updated specification of type CARD<br><b> having Id ' + specResponseOnSuccess.Id + '</b>', 'Specification');
                    } else {
                        $("#interstitial").hide();
                        //app.showMessage("failure").then(function () {
                        //    return;
                        //});
                    }
                }
                function updateError() {
                    $("#interstitial").hide();
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
                }
            }
            else {
                $("#interstitial").hide();
                if (!slotselected) {
                    $("#sltconsumptionselnt").show();
                }

            }
        };

        CardSpecification.prototype.saveAuditChanges = function () {
            $.ajax({
                type: "GET",
                url: 'api/audit/save',
                data: JSON.stringify(specChangeArray),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: saveAuditSuccess,
                error: saveAuditError
            });
            function saveAuditSuccess(response) {
                var specResponseOnSuccess = JSON.parse(response);
                if ("success" == specResponseOnSuccess) {
                    // probably don't want the user seeing a go-zillion popups
                    var mike = 'this is good';
                }
            }
            function saveAuditError() {
                // not sure we want to do anything here, so ask client
                //$("#interstitial").hide();
                //return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }
        };

        CardSpecification.prototype.GetModelNameCount = function (modelname, id) {
            var spec = { 'modelname': modelname, 'id': id }
            var modelnamecount = 0;
            $.ajax({
                type: "GET",
                url: 'api/specn/modelcount',
                data: spec,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: saveModelCountSuccess,
                error: saveModelCountError,
                async: false
            });
            function saveModelCountSuccess(response) {
                modelnamecount = JSON.parse(response);
            }
            function saveModelCountError() {
            }
            return modelnamecount;
        }

        CardSpecification.prototype.getConfigNames = function (materialItemID) {
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

        CardSpecification.prototype.onchangeSpcnRlTypLst = function (item) {
            var selectedId = item.id.value;
            var selectedUseType = "";

            if (item.Slctd.bool) {
                for (var i = 0; i < selfcard.specnlistdetails().length; i++) {
                    if (selectedId == selfcard.specnlistdetails()[i].id.value) {
                        selfcard.specnlistdetails()[i].Slctd.bool = true;
                        selfcard.specnlistdetails()[i].PrtyNo.value = item.PrtyNo.value;

                        break;
                    }
                }

                selectedUseType = item.UseTyp.value;

                //var temp = selfcard.specnlistdetails();

                //selfcard.specnlistdetails([]);
                //selfcard.specnlistdetails(temp);
            } else {
                for (var i = 0; i < selfcard.specnlistdetails().length; i++) {
                    if (selectedId == selfcard.specnlistdetails()[i].id.value) {
                        selfcard.specnlistdetails()[i].Slctd.bool = false;
                        selfcard.specnlistdetails()[i].PrtyNo.value = 0;
                    } else {
                        if (selfcard.specnlistdetails()[i].Slctd.bool) {
                            selectedUseType = selfcard.specnlistdetails()[i].UseTyp.value;
                        }
                    }
                }

                //var temp = selfcard.specnlistdetails();

                //selfcard.specnlistdetails([]);
                //selfcard.specnlistdetails(temp);
            }

            if (selectedUseType !== "") {
                for (var i = 0; i < selfcard.specnlistdetails().length; i++) {
                    if (selectedUseType == selfcard.specnlistdetails()[i].UseTyp.value) {
                        selfcard.specnlistdetails()[i].Slctd.enable = true;
                    } else {
                        selfcard.specnlistdetails()[i].Slctd.enable = false;
                        selfcard.specnlistdetails()[i].Slctd.bool = false;
                        selfcard.specnlistdetails()[i].PrtyNo.value = 0;
                    }
                }
            } else {
                for (var i = 0; i < selfcard.specnlistdetails().length; i++) {
                    selfcard.specnlistdetails()[i].Slctd.enable = true;
                }
            }

            var temp = selfcard.specnlistdetails();

            selfcard.specnlistdetails([]);
            selfcard.specnlistdetails(temp);

            return true;
        };

        CardSpecification.prototype.associatepartCard = function (model, event) {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfcard.searchtblcard(false);
            selfcard.associatemtlblck(false);
            document.getElementById('idcdmmscard').value = "";
            document.getElementById('materialcodecard').value = "";
            document.getElementById('partnumbercard').value = "";
            document.getElementById('clmccard').value = "";
            document.getElementById('catlogdsptcard').value = "";

            var modal = document.getElementById('cardModalpopup');
            var btn = document.getElementById("idAsscociatecard");
            var span = document.getElementsByClassName("close")[0];
            var rvsid = selfcard.selectedCardSpec().RvsnId.value;
            var srcd = "CARD";
            var ro = document.getElementById('rcrolyCard').checked ? "Y" : "N";
            var searchJSON = { RvsnId: rvsid, source: srcd, isRO: ro };

            $.ajax({
                type: "GET",
                url: 'api/specn/getassociatedmtl',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successSearchAssociateddisp,
                error: errorFunc,
                context: selfcard,
                async: false
            });

            function errorFunc() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }

            function successSearchAssociateddisp(data, status) {
                selfcard.enableModalSave(false);
                if (data === 'no_results') {
                    $("#interstitial").hide();
                    //$(".NoRecordrp").show();
                    //setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
                } else {

                    var results = JSON.parse(data);
                    selfcard.associatemtlblck(results);
                    $("#interstitial").hide();
                }

            }
            // When the user clicks the button, open the modal 
            btn.onclick = function () {
                modal.style.display = "block";
            }

            $("#interstitial").hide();
            $("#cardModalpopup").show();

            // When the user clicks on <span> (x), close the modal
            span.onclick = function () {
                modal.style.display = "none";
            }
        };

        CardSpecification.prototype.CancelassociateCard = function (model, event) {
            selfcard.searchtblcard(false);
            selfcard.associatemtlblck(false);
            document.getElementById('idcdmmscard').value = "";
            document.getElementById('materialcodecard').value = "";
            document.getElementById('partnumbercard').value = "";
            document.getElementById('clmccard').value = "";
            document.getElementById('catlogdsptcard').value = "";
            $("#cardModalpopup").hide();

        };

        CardSpecification.prototype.searchmtlcard = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfcard.searchtblcard(false);

            var mtlid = $("#idcdmmscard").val();
            var mtlcode = $("#materialcodecard").val();
            var partnumb = $("#partnumbercard").val();
            var clmc = $("#clmccard").val();
            var caldsp = $("#catlogdsptcard").val();
            var ro = document.getElementById('rcrolyCard').checked ? "Y" : "N";
            var src = "CARD";

            if (mtlid.length > 0 || mtlcode.length > 0 || partnumb.length > 0 || clmc.length > 0 || caldsp.length > 0) {
                var searchJSON = {
                    material_item_id: mtlid, product_id: mtlcode, mfg_part_no: partnumb, mfg_id: clmc, mat_desc: caldsp, source: src, isRo: ro
                };

                $.ajax({
                    type: "GET",
                    url: 'api/specn/searchmtl',
                    data: searchJSON,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successSearchAssociated,
                    error: errorFunc,
                    context: selfcard,
                    async: false
                });

                function errorFunc() {
                    $("#interstitial").hide();
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
                }

                function successSearchAssociated(data, status) {
                    if (data === 'no_results') {
                        $("#interstitial").hide();
                        $(".NoRecordrp").show();
                        setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
                    } else {

                        var results = JSON.parse(data);
                        selfcard.searchtblcard(results);

                        $("#interstitial").hide();
                    }
                }
            }
            else {
                $(".Noinput").show();
                setTimeout(function () { $(".Noinput").hide() }, 5000);
                $("#interstitial").hide();
            }
            $(document).ready(function () {
                $('input.checkcardpopsearch').on('change', function () {
                    $('input.checkcardpopsearch').not(this).prop('checked', false);

                    $('input.checkcardpopsearch').each(function () {
                        if ($(this).prop('checked') == true) {
                            selfcard.enableModalSave(true);

                            return false;
                        }

                        selfcard.enableModalSave(false);
                    });

                    $('input.checkasstspopsearch').each(function () {
                        $(this).prop('checked', false);
                    });
                });
            });

            $(document).ready(function () {
                $('input.checkasstspopsearch').on('change', function () {
                    $('input.checkcardpopsearch').each(function () {
                        $(this).prop('checked', false);
                    });

                    $('input.checkasstspopsearch').each(function () {
                        if ($(this).prop('checked') == true) {
                            selfcard.enableModalSave(false);

                            return false;
                        }

                        selfcard.enableModalSave(false);
                    });
                });
            });

        };
        CardSpecification.prototype.updateChbkCheck = function () {

            if ($("#haslotCard").is(':checked'))
                selfcard.selectedCardSpec().Prts.value = "Y";
            else
                selfcard.selectedCardSpec().Prts.value = "N";

            if ($("#hasportCard").is(':checked'))
                selfcard.selectedCardSpec().Slts.value = "Y";
            else
                selfcard.selectedCardSpec().Slts.value = "N";
            if ($("#strgtInd").is(':checked'))
                selfcard.selectedCardSpec().StrghtThru.value = "Y";
            else
                selfcard.selectedCardSpec().StrghtThru.value = "N";

        };

        CardSpecification.prototype.SaveassociateCard = function () {
            var chkflag = false;
            var checkBoxes = $("#searchresultcard .checkcardpopsearch");
            var ids = $("#searchresultcard .idcards");
            var specid = selfcard.selectedCardSpec().RvsnId.value;
            var src = "CARD";
            var saveJSON = {};

            for (var i = 0; i < checkBoxes.length; i++) {
                if (checkBoxes[i].checked == true) {
                    saveJSON = { material_rev_id: ids[i].innerText, specn_id: specid, source: src };
                    chkflag = true;

                    break;
                }
            }

            if (chkflag === true) {
                saveJSON = JSON.stringify(saveJSON);

                $.ajax({
                    type: "GET",
                    url: 'api/material/' + ids[i].innerText,
                    data: saveJSON,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successSearchAssociated,
                    error: errorFunc,
                    context: selfcard,
                    async: false
                });

            }

            //var checkBoxesassp = $("#associatedmtl .checkasstspopsearch");
            //var mtlcodedis = $("#idchkncard").val();
            //var src1 = "NODE";
            //var saveJSONdis = {
            //    material_rev_id: mtlcodedis, source: src1
            //};
            //var chkflagdis = false;
            //for (var i = 0; i < checkBoxesassp.length; i++) {
            //    if (checkBoxesassp[i].checked == false) {
            //        chkflagdis = true;
            //    }

            //}
            //if (chkflagdis === true) {
            //    $.ajax({
            //        type: "GET",
            //        url: 'api/specn/disassociatepart',
            //        data: saveJSONdis,
            //        contentType: "application/json; charset=utf-8",
            //        dataType: "json",
            //        success: successSearchAssociated,
            //        error: errorFunc,
            //        context: selfcard,
            //        async: false
            //    });
            //}
            function errorFunc() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }

            function successSearchAssociated(data, status) {
                if (data === ' ') {
                    $("#interstitial").hide();
                    //return app.showMessage('Failure');
                } else {
                    var cardList = JSON.parse(data);
                    var card = { "list": [cardList] };
                    var name = card.list[0].Mfg.value + '-' + card.list[0].PrtNo.value;

                    if (card != null && card.list.length > 0) {
                        selfcard.duplicateSelectedCardSpecDpth(card.list[0].Dpth.value);
                        selfcard.duplicateSelectedCardSpecHght(card.list[0].Hght.value);
                        selfcard.duplicateSelectedCardSpecWdth(card.list[0].Wdth.value);
                    }

                    selfcard.selectedCardSpec().Card = card;
                    selfcard.selectedCardSpec().Nm.value = name;
                    selfcard.selectedCardSpec().RvsnNm.value = name;

                    selfcard.selectedCardSpec(selfcard.selectedCardSpec());

                    $("#cardModalpopup").hide();
                    //return app.showMessage('Success!');
                    $("#interstitial").hide();
                }
            }
        };
        //**********************************************************************
        // Author: shivanand Gaddi
        // Written date : 03/12/2019
        // Description : card has slot checkbox clicked design started.
        // *********************************************************************
        //This mthod written for loading slot details for selected card.
        CardSpecification.prototype.onLoadHasSlotDtls = function () {
            if (selfcard.selectedCardSpec().Slts.bool == false) {
                return false;
            }

            $("#interstitial").show();

            var cardSpecId = selfcard.selectedCardSpec().id.value;

            $.ajax({
                type: "GET",
                url: 'api/specn/getCardhasSlotDtls/' + cardSpecId,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: getSlotSuccessFunc,
                error: getSlotErrorFunc,
                context: selfcard
            });
            function getSlotSuccessFunc(data, status) {

                if (data == "[]" || data == "{}") {
                    $("#interstitial").hide();
                    app.showMessage("No ports found for card id : " + cardSpecId, 'Failed');
                }
                else {
                    $("#interstitial").hide();
                    var resp = JSON.parse(data);
                    selfcard.slotListdetails(['']);
                    selfcard.slotListdetails(JSON.parse(data));
                }
            }
            function getSlotErrorFunc() {
                $("#interstitial").hide();
                alert('error');
            }
        };
        // End of loading slot details.
        //This method written for 'hasSlot' checkbox check/uncheck operations
        CardSpecification.prototype.onchkSlots = function (item, event) {
            // mwj: if the check box is clicked then item.selectedSpec().Slts.bool would get toggled
            var checked = $("#haslotCard").is(":checked");
            if (checked == false) {
                var varok = confirm("Are you going to uncheck 'hasslot' checkbox for this card");
                if (varok == true) {
                    if (selfcard.slotListdetails().length > 0) {
                        app.showMessage("Please delete all the slots from the below slot window, by uncheck 'Unselect to Remove' checkbox and save it.", "Warning");
                        checked = true;
                        document.getElementById("haslotCard").checked = true;
                    }
                    else {
                        app.showMessage("This card does not have any slot details.", "Message");
                    }
                }
                else {
                    checked = true;
                    document.getElementById("haslotCard").checked = true;
                }
            }
            item.selectedCardSpec().Slts.bool = checked;
            $("#hasSlotDiv").toggle(checked === true);
        };
        //End of 'hasSlot' checkbox check/uncheck operations

        //This method written for popup slot sequence window.
        CardSpecification.prototype.onClickEditbtn = function (item) {
            $("#interstitial").show();
            var spanSlot = document.getElementsByClassName("close")[1];
            spanSlot.onclick = function () {
                $("#slotSeqDtls").hide();
            }
            var slotQuantity = item.SlotQuantity;
            var slotDefId = item.cardSlotDefId;
            var SlotType = item.SlotType;
            SlotType = SlotType.replace("/", "-");
            var SlotSpecId = item.cardSlotSpecId;

            $.ajax({
                type: "GET",
                url: 'api/specn/getCardSpecnWithSlots/' + slotQuantity + '/' + slotDefId + '/' + SlotType + '/' + SlotSpecId + '/',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: selfcard
            });
            function successFunc(data, status) {

                if (data == "[]" || data == "{}") {
                    $("#interstitial").hide();
                    app.showMessage("No records found", 'Failed');
                }
                else {
                    $("#interstitial").hide();
                    var resp = JSON.parse(data);

                    var $dlg = $("#slotSeqDtls").show();

                    // mwj: added some cosmetics to display the Slot Type on the edit dialog box
                    // in the HTML removed edit box for Slot and replaced it with span with SlotNo instead of SlotType
                    $dlg.find(".slot-seq").text(item.SlotSequence);
                    $dlg.find(".slot-type").text(item.SlotType);
                    selfcard.ManageSlotslist(JSON.parse(data));
                }
            }
            function errorFunc() {
                $("#interstitial").hide();
                //alert('error');
            }
        };
        // End of showinf slot sequence popup window.

        //This method written for validate slot quantity, new slot quantity should not be less than current slot quantity.
        CardSpecification.prototype.getCardSlotQuantity = function (item) {
            if (item.SlotQuantity == "") {
                app.showMessage("Slot quantity should not be empty", 'Info');
                return;
            }
            $("#interstitial").show();
            var Jsondata = {
                slotDefId: item.cardSlotDefId,
                cardSlotQntity: item.SlotQuantity,
            };

            selfcard.AddJsonres(Jsondata);
            var saveJSON = mapping.toJS(selfcard.AddJsonres());

            $.ajax({
                type: "POST",
                url: 'api/specn/getSlotQuantity/',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: getQntySuccess,
                error: getQntyError
            });

            function getQntySuccess(data, status) {

                if (data == 'no_results') {
                    $("#interstitial").hide();
                    //app.showMessage("No records found", 'Failed');
                }
                else {
                    $("#interstitial").hide();
                    app.showMessage(data, 'SUCCESS');

                    if (data.indexOf("delete") != -1) {
                        var res = data.split("to");
                        var qtyVal = res[0].replace(/\D/g, '');
                        var $tbl = $("#tblslotList");
                        var $row = $tbl.find("#" + item.GUID);
                        var $cb = $row.find("#idSloqntity");
                        $cb.val(qtyVal);
                        item.SlotQuantity = qtyVal;
                        selfcard.onClickEditbtn(item);
                    }
                }
            }
            function getQntyError() {
                $("#interstitial").hide();
                app.showMessage(data, 'FAILED');
            }
        };
        //End of validating slot quantity.

        //This method written for EQDES validation
        CardSpecification.prototype.validateEQDES = function (item) {
            var $tbl = $("#slotList");
            var $row = $tbl.find("#" + item.GUID);
            var $cb = $row.find("#idSlotEqdes");
            console.log($cb);
            $cb.focus();
            selfcard.CardSltsvalidation('EQDES', item.EQDES);
            return;
        };
        //End of EQDES validations.

        //This method written for slot popup window control validations.
        CardSpecification.prototype.CardSltsvalidation = function (req_Type, req_Val) {
            $("#interstitial").show();

            var Jsondata = {
                reqType: req_Type,
                validationVal: req_Val
            };

            selfcard.AddJsonres(Jsondata);
            var saveJSON = mapping.toJS(selfcard.AddJsonres());

            $.ajax({
                type: "POST",
                url: 'api/specn/cardSlotSeqvalidations/',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: validateSuccess,
                error: validateError
            });

            function validateSuccess(data, status) {

                if (data == 'no_results' || data == '') {
                    $("#interstitial").hide();
                    app.showMessage("No records found", 'Failed');
                }
                else {
                    $("#interstitial").hide();
                    if (data.indexOf("already") != -1) {
                        document.getElementById("saveSlotEdit").disabled = true;
                    }
                    else {
                        document.getElementById("saveSlotEdit").disabled = false;
                    }
                    app.showMessage(data, 'EQDES Validation Result');

                }
            }
            function validateError(data) {
                $("#interstitial").hide();
                app.showMessage(data, 'FAILURE');
            }
        };
        //End of slot popup window control validations.

        //This method written for save slot sequence details.
        CardSpecification.prototype.onClickSaveSlotSeq = function () {
            $("#interstitial").show();
            var card = selfcard.selectedCardSpec();

            var Jsondata = {
                cardId: card.id.value,
                slotDtls: selfcard.slotListdetails()
            };

            selfcard.AddJsonres(Jsondata);
            var saveJSON = mapping.toJS(selfcard.AddJsonres());
            selfcard.slotListdetails(['']);
            $.ajax({
                type: "POST",
                //url: 'api/specn/insertSlotmanageDtls/',
                url: 'api/specn/saveCardSlotsDtls/',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: saveCardSltSuccess,
                error: saveCardSltError
            });

            function saveCardSltSuccess(data, status) {

                if (data == 'no_results') {
                    $("#interstitial").hide();
                    //app.showMessage("No records found", 'Failed');
                }
                else {
                    $("#interstitial").hide();
                    $("#slotSeqDtls").hide();
                    var results = JSON.parse(data);
                    selfcard.slotListdetails([]);
                    selfcard.slotListdetails(results);

                    app.showMessage('Slot Details Saved Successfully', 'RESPONSE');

                }
            }
            function saveCardSltError(data) {
                $("#interstitial").hide();
                app.showMessage(data, 'FAILURE');
            }
        };
        //End of add/modify slot sequence details.

        // This method written for binding the rotation angle details into dropdown.
        CardSpecification.prototype.rotationAnglist = function () {
            $.ajax({
                type: "GET",
                url: 'api/reference/RotationAng',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: selfcard
            });
            function successFunc(data, status) {

                if (data == 'no_results') {
                    $("#interstitial").hide();
                    //app.showMessage("No records found", 'Failed');
                }
                else {
                    selfcard.RotationAngle(JSON.parse(data));
                }
            }
            function errorFunc() {
                $("#interstitial").hide();
            }
        };
        //end of binding rotation angle details

        // This method written for searching slot type details
        CardSpecification.prototype.Searchspec = function () {
            $("#interstitial").show();

            var Name = $("#idSpecName").val();
            var Description = $("#idSpecDesc").val();
            var RevName = $("#idrevName").val();
            var Status = selfcard.status();
            var searchJSON = {
                specname: Name, specdesc: Description, revname: RevName, status: Status
            };

            selfcard.AddJsonres(searchJSON);
            var saveJSON = mapping.toJS(selfcard.AddJsonres());
            $.ajax({
                type: "POST",
                url: 'api/specn/getcardslotseqdtls',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: selfcard
            });
            function successFunc(data, status) {

                if (data == null) {
                    $("#interstitial").hide();
                    app.showMessage("No records found", 'Failed');
                }
                else {
                    var results = JSON.parse(data);
                    selfcard.cardSlotSequence(results);
                    $("#interstitial").hide();
                }
            }
            function errorFunc() {
                $("#interstitial").hide();
                app.showMessage("Error", 'Failed');
            }
        };
        // end of searcing slot type details

        // This method written for save the modified or newlly added slot sequence details.
        CardSpecification.prototype.saveManageSlots = function () {
            $("#interstitial").show();

            // mwj: added some preliminary validation before saving.
            var validate = function (list) {
                var errors = [];
                var len = list.length;
                for (var i = 0; i < len; i++) {
                    var item = list[i];

                    // check numbers
                    ["EQDES", "HorzDisp"].forEach(function (field) {
                        var val = item[field];
                        if (val.length > 0 && (isNaN(val) || val < 0)) {
                            errors.push("* Invalid " + field + " \"" + val + "\" for Slot #" + item.SlotSlotNo + "; must blank or a positive number");
                        }
                    });
                }

                return errors;
            }

            var details = selfcard.ManageSlotslist();
            var errors = validate(details);
            if (errors.length > 0) {
                $("#interstitial").hide();
                app.showMessage(errors.join("<br>"), "Cannot Save Slot Configuration");
                return;
            }

            var Jsondata = {
                slotMngDtls: details
            };

            selfcard.AddJsonres(Jsondata);
            var saveJSON = mapping.toJS(selfcard.AddJsonres());

            $.ajax({
                type: "POST",
                //url: 'api/specn/insertSlotmanageDtls/',
                url: 'api/specn/saveCardSpecnSlots/',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: saveSuccess,
                error: saveError
            });

            function saveSuccess(data, status) {

                if (data == 'no_results') {
                    $("#interstitial").hide();
                    //app.showMessage("No records found", 'Failed');
                }
                else {
                    $("#interstitial").hide();
                    $("#slotSeqDtls").hide();
                    selfcard.onLoadHasSlotDtls();
                    app.showMessage(data, 'RESPONSE');

                }
            }
            function saveError(data) {
                $("#interstitial").hide();
                app.showMessage(data, 'FAILURE');
            }
        };
        // end of inserting/modifying slot sequence details.

        // This method written for closing slot type popup window.
        CardSpecification.prototype.closeManageSlots = function () {
            $("#slotSeqDtls").hide();
        };
        // end of closing slot type popup window.

        // This method written for updating the current selected slot for particular
        // slot sequence number.
        CardSpecification.prototype.onClickUpdSlotSeq = function (item) {
            selfcard.resetSlotDtls();
            var spanSlot = document.getElementById("idCloseSlotSelect");
            spanSlot.onclick = function () {
                $("#cardSlotSelect").hide();
            }
            $("#cardSlotSelect").show();
            selfcard.actionCode = 'UPDATE';
            selfcard.sltQnty = item.SlotQuantity;
            selfcard.sltSeq = item.SlotSequence;
            selfcard.sltDefId = item.cardSlotDefId;
        };
        //End of updating slot type.

        //This method used for adding slot type for particular card.
        CardSpecification.prototype.onClickAddSlotSeq = function () {
            selfcard.resetSlotDtls();
            var spanSlot = document.getElementById("idCloseSlotSelect");
            spanSlot.onclick = function () {
                $("#cardSlotSelect").hide();
            }
            $("#cardSlotSelect").show();
            selfcard.actionCode = 'INSERT';
        };
        // end of adding slot type for particular card.

        // This method written for delete functionality after uncheck on the table checkbox(unselect to delete).
        CardSpecification.prototype.onMarkForDeletion = function (item, event) {
            if (item.SlotSlotId == 0) {
                app.showMessage("This slot hasn't been created yet; so it cannot be removed at this time.")
                return false; // can't mark a new slot for deletion...
            }

            var $cb = $(event.target);
            var $tr = $cb.parents("tr:first");

            item.actionCode = ($cb.is(":checked") ? "UPDATE" : "DELETE");

            $tr.removeClass("remove");
            if (item.actionCode === "DELETE") {
                $tr.addClass("remove");
            }
            return true;
        };
        //End of checkbox delete method.

        //This method written for clearing slot serach details
        CardSpecification.prototype.resetSlotDtls = function () {
            $("#idSpecName").val("");
            $("#idSpecDesc").val("");
            $("#idrevName").val("");
            $('#idstatuspec').find("option:selected").val("");
            selfcard.cardSlotSequence([]);
        };
        //End of clearing slot search details.

        // This method written for deleting the each slot sequence details, using checkbox in the popup window.
        CardSpecification.prototype.onchkRemoveSltDef = function (item, event) {
            //if (item.UnSelectToRemove) {
            //	return false;
            //}

            var $cb = $(event.target);
            var $tr = $cb.parents("tr:first");
            item.UnSelectToRemove = $cb.is(":checked");
            $tr.removeClass("remove");
            if (item.UnSelectToRemove == false) {
                $tr.addClass("remove");
            }
            return true;

        };
        //End of deleting slot sequence details.

        // This method used for selecting slot type and add/modify it.
        CardSpecification.prototype.onClickSelectSlot = function (item) {
            var varok = confirm("You selected slot type - " + item.SLOT_DSC);
            if (varok == false) {
                return false;
            }
            console.log("selected slot type: " + JSON.stringify(item));
            //---------------------------------------
            $("#interstitial").show();

            var card = selfcard.selectedCardSpec();

            if (selfcard.actionCode == 'INSERT') {
                var Jsondata = {
                    cardId: card.id.value,
                    slotSpecId: item.SLOT_SPECN_ID,
                    seqNum: 0,
                    SlotQty: 1,
                    SlotDefid: 0,
                    actionCode: selfcard.actionCode
                };
            }
            else {
                var Jsondata = {
                    cardId: card.id.value,
                    slotSpecId: item.SLOT_SPECN_ID,
                    seqNum: selfcard.sltSeq,
                    SlotQty: selfcard.sltQnty,
                    SlotDefid: selfcard.sltDefId,
                    actionCode: selfcard.actionCode
                };
            }
            selfcard.AddJsonres(Jsondata);
            var saveJSON = mapping.toJS(selfcard.AddJsonres());

            $.ajax({
                type: "POST",
                url: 'api/specn/insertSelectedCardSlot/',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: saveSuccess,
                error: saveError
            });

            function saveSuccess(data, status) {

                if (data == null) {
                    $("#interstitial").hide();
                    app.showMessage("No records found", 'Failed');
                }
                else {
                    var results = JSON.parse(data);
                    $("#interstitial").hide();

                    selfcard.onLoadHasSlotDtls();

                    if (selfcard.actionCode == 'INSERT') {
                        app.showMessage('Slot Inserted Successfully', 'SUCCESS');
                    }
                    else if (selfcard.actionCode == 'UPDATE') {
                        app.showMessage('Slot Updated Successfully', 'SUCCESS');
                    }
                    $("#cardSlotSelect").hide();
                    selfcard.actionCode = "";
                }
            }
            function saveError(data) {
                $("#interstitial").hide();
                app.showMessage('Error during inserting slot details', 'FAILURE');
            }
            //-----------------------------
        };
        // End of selecting slot type and add/modify it.

        // End of card has slot checkbox clicked design .
        //**********************************************************************

        //
        // card has port checkbox clicked design started.
        //
        CardSpecification.prototype.onLoadHasPortsDtls = function () {

            $("#interstitial").show();

            var cardSpecId = selfcard.selectedCardSpec().id.value;

            $.ajax({
                type: "GET",
                url: 'api/specn/getCardhasPortDtls/' + cardSpecId,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: getPartsSuccessFunc,
                error: getPartsErrorFunc,
                context: selfcard
            });
            function getPartsSuccessFunc(data, status) {

                if (data == "[]" || data == "{}") {
                    $("#interstitial").hide();
                    app.showMessage("No ports found for card id : " + cardSpecId, 'Failed');
                }
                else {
                    $("#interstitial").hide();
                    var resp = JSON.parse(data);
                    selfcard.portsListdetails(JSON.parse(data));
                }
            }
            function getPartsErrorFunc() {
                $("#interstitial").hide();
                $("#hasPortDiv").hide();
                alert('error');
            }
        };

        CardSpecification.prototype.onClickPortType = function (item) {
            $("#interstitial").show();
            var spanSlot = document.getElementById("idcardPortSelect");
            spanSlot.onclick = function () {
                $("#cardPortSelect").hide();
                selfcard.cardPortTypeList([]);
            }

            $('#cardPortSelect').show();
            $("#interstitial").hide();

            selfcard.portDefId = item.portDefId;

        };

        CardSpecification.prototype.onClickConctrType = function (item) {
            $("#interstitial").show();
            selfcard.ClearCnctr();
            $('#PortConctrTypeSelect').show();
            $("#interstitial").hide();

            var spanSlot = document.getElementById("idcardPortCnctrSelect");
            spanSlot.onclick = function () {
                $("#PortConctrTypeSelect").hide();
            }

            selfcard.portDefId = item.portDefId;

        };
        CardSpecification.prototype.onchkPort = function (item) {

            var checked = $("#hasportCard").is(":checked");
            item.selectedCardSpec().Prts.bool = checked;
            $("#hasPortDiv").toggle(checked === true);
        };
        CardSpecification.prototype.SearchPortType = function () {
            $("#idPortType").val("");
            $("#idPortDesc").val("");
            selfcard.cardPortTypeList(['']);
            document.getElementById('chkDualPorts').checked = false;
            document.getElementById('chkTranmRate').checked = false;
            document.getElementById('chkAssPorts').checked = false;
        };

        CardSpecification.prototype.SearchPortType = function () {
            $("#interstitial").show();
            selfcard.cardPortTypeList(['']);
            var PortType = $("#idPortType").val();
            var PortRole = selfcard.roleSelect();
            var PortDesc = $("#idPortDesc").val();
            var dualPort = document.getElementById('chkDualPorts').checked == true ? 'Y' : 'N';
            var Transrate = document.getElementById('chkTranmRate').checked == true ? 'Y' : 'N';
            var AssignPorts = document.getElementById('chkAssPorts').checked == true ? 'Y' : 'N';

            var searchJSON = {
                PortType: PortType, PortRole: PortRole, PortDesc: PortDesc, dualPort: dualPort, Transrate: Transrate, AssignPorts: AssignPorts
            };

            selfcard.AddJsonres(searchJSON);
            var saveJSON = mapping.toJS(selfcard.AddJsonres());
            $.ajax({
                type: "POST",
                url: 'api/specn/getcardPortTypeDtls',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: selfcard
            });
            function successFunc(data, status) {

                if (data == null) {
                    $("#interstitial").hide();
                    app.showMessage("No records found", 'Failed');
                }
                else {
                    var results = JSON.parse(data);
                    selfcard.cardPortTypeList(results);
                    $("#interstitial").hide();
                }
            }
            function errorFunc() {
                $("#interstitial").hide();
                app.showMessage("Error", 'Failed');
            }
        };

        CardSpecification.prototype.SearchCnctr = function () {
            $("#interstitial").show();
            selfcard.cardCnctrListDtls(['']);
            var cnctrName = $("#idCnctrName").val();
            var cnctrAliases = $("#idAliases").val();
            var cnctrDesc = $("#idCnctrDesc").val();

            var searchJSON = {
                cnctrName: cnctrName, cnctrAliases: cnctrAliases, cnctrDesc: cnctrDesc
            };

            selfcard.AddJsonres(searchJSON);
            var saveJSON = mapping.toJS(selfcard.AddJsonres());
            $.ajax({
                type: "POST",
                url: 'api/specn/getcardCnctrTypeDtls',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: selfcard
            });
            function successFunc(data, status) {

                if (data == null) {
                    $("#interstitial").hide();
                    app.showMessage("No records found", 'Failed');
                }
                else {
                    var results = JSON.parse(data);
                    selfcard.cardCnctrListDtls(results);
                    $("#interstitial").hide();
                }
            }
            function errorFunc() {
                $("#interstitial").hide();
                app.showMessage("Error", 'Failed');
            }
        };

        CardSpecification.prototype.ClearCnctr = function () {
            $("#idCnctrName").val("");
            $("#idAliases").val("");
            $("#idCnctrDesc").val("");
            selfcard.cardCnctrListDtls(['']);
        };


        CardSpecification.prototype.onClickSelectPortType = function (item) {
            var portDefId = selfcard.portDefId;
            var portTypeid = item.PortTypeId;
            var cnctrTypeId = 0;
            var TypeName = 'PortType';
            selfcard.updatePortDtls(portDefId, portTypeid, cnctrTypeId, TypeName);
        };

        CardSpecification.prototype.onClickSelectCnctr = function (item) {
            var portDefId = selfcard.portDefId;
            var portTypeid = 0;
            var cnctrTypeId = item.cnctrTypeId;
            var TypeName = 'CnctrType';
            selfcard.updatePortDtls(portDefId, portTypeid, cnctrTypeId, TypeName);
        };

        CardSpecification.prototype.updatePortDtls = function (portDefId, portTypeid, cnctrTypeId, TypeName) {

            var searchJSON = {
                portDefId: portDefId, portTypeId: portTypeid, cnctrTypeId: cnctrTypeId, TypeName: TypeName
            };

            selfcard.AddJsonres(searchJSON);
            var saveJSON = mapping.toJS(selfcard.AddJsonres());
            $.ajax({
                type: "POST",
                url: 'api/specn/updateportDtls',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successUpdFunc,
                error: errorUpdFunc,
                context: selfcard
            });
            function successUpdFunc(data, status) {

                if (data == null) {
                    $("#interstitial").hide();
                    app.showMessage(data, 'Failed');
                }
                else {
                    //var results = JSON.parse(data);
                    app.showMessage(data, 'Success');
                    $("#interstitial").hide();
                }
            }
            function errorUpdFunc(data) {
                $("#interstitial").hide();
                app.showMessage(data, 'Failed');
            }
        };

        CardSpecification.prototype.onClickPortCount = function (item) {

            var spanSlot = document.getElementById("idClosePortCount");
            spanSlot.onclick = function () {
                $("#portCountConfigure").hide();
            }
            $("#portCountConfigure").show();
        };


        CardSpecification.prototype.onClickPortAssign = function (item) {


            var spanSlot = document.getElementById("idClosePortAssn");
            spanSlot.onclick = function () {
                $("#portAssignConfigure").hide();
            }
            $("#portAssignConfigure").show();
        };

        //----------- AB48512 Port Configure chaanges starts-------------------------------//

        CardSpecification.prototype.closeConfigurePorts = function () {
            //Remove all records from DeleteArray
            selfcard.DeleteConfigCardPortList = [];
            $("#ConfigCardPorts").hide();
        };

        CardSpecification.prototype.onClickConfigurePortbtn = function (item) {
            $("#interstitial").show();
            var portDefId = item.portDefId;
            // Get Port Seq Details
            $.ajax({
                type: "GET",
                url: 'api/specn/GetCardPortsSeqDtls/' + portDefId + '/',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data, status) {
                    selfcard.selectedCardPortDef(JSON.parse(data));
                    $("#idConfigPortOffset").val(selfcard.selectedCardPortDef().portOffsetVal);
                },
                context: selfcard
            });
            //Populate grid with port configuration details
            PopulatePortConfigGrid(portDefId);

        };
        PopulatePortConfigGrid = function (portDefId) {
            $("#interstitial").show();
            $.ajax({
                type: "GET",
                url: 'api/specn/GetCardPortsConfigDtls/' + portDefId + '/',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                context: selfcard
            });
            function successFunc(data, status) {
                selfcard.rotationAnglist();
                selfcard.ConfigCardPortList(JSON.parse(data));
                selfcard.DeleteConfigCardPortList = [];
                $("#interstitial").hide();
                $("#ConfigCardPorts").show();
            }
        };

        CardSpecification.prototype.onchkRemovePortConfig = function (rec, event) {
            var $cb = $(event.target);
            var shouldRemove = ($cb.is(":checked") === true);

            var cssToBeRemoved = "remove";
            rec._isDeleted = shouldRemove;
            //Find the row
            var $tr = $cb.parents("tr:first");
            if (shouldRemove === false) {
                // If delete is unchecked, delete from DeleteArray if present and 
                //..remove stlyes for "Marked for delete"    	       
                $tr.removeClass(cssToBeRemoved);
                selfcard.DeleteConfigCardPortList.remove(function (e) { return e._guid === rec._guid; });
                //exit function
                return true;
            }
            else {
                $tr.addClass(cssToBeRemoved);
                $tr.css({ 'color': 'darkred' });
            }
            // If checked for remove, add to DeleteArray and update the row style to "Marked for delete"
            if (rec.CardPortDetailsId > 0) {
                if (selfcard.DeleteConfigCardPortList.findIndex(function (e) { return e._guid === rec._guid; }) === -1) {
                    selfcard.DeleteConfigCardPortList.push(rec);
                }
            }
            return true;
        };

        CardSpecification.prototype.autoFillConfigurePortNo = function () {
            //Get Port Offset value and starting point
            var portOffsetVal = $("#idConfigPortOffset").val();
            var portStatingNo = selfcard.selectedCardPortDef().startingPortNo;
            var $ui = $("#uiCardPortConfigItemList");
            //Reset port no based on new offset value
            for (let i = 0; i < selfcard.ConfigCardPortList().length; i++) {

                //Find the port number input field and update the value
                var $tr = $ui.find("tr#port" + selfcard.ConfigCardPortList()[i]._guid);
                var $portNo = $tr.find("#idPortNo");
                var ptNo = Number(portStatingNo) + Number(portOffsetVal) + Number(i);
                $portNo.val(ptNo);
                //bind to selfcard port list array separately as two way bind does not work 
                selfcard.ConfigCardPortList()[i].PortNo = ptNo;
            }

        };

        CardSpecification.prototype.saveConfigurePorts = function () {
            $("#interstitial").show();
            // Get Port DefenitionId to reload the port config grid from database
            var portDefId = selfcard.selectedCardPortDef().portDefId;
            // Get table body to update row wise status  
            var $ui = $("#uiCardPortConfigItemList");
            var fail = false;
            var failcnt = 0;
            //debugger
            // Prepare save array. Records marked to delete will be excluded from insert/update
            var SaveArray = selfcard.ConfigCardPortList().filter(function (item) {
                // Confiuration to be saved only if it is not marked to delete and both Port No and Label are given
                return (item._isDeleted === false && item.PortNo !== 0 && item.PortNo !== null && item.Label !== null && item.Label !== "");
            });
            var totalCnt = selfcard.DeleteConfigCardPortList.length + SaveArray.length;
            if (totalCnt > 0) {
                UpdateDatabase();
                if (!fail) {
                    selfcard.DeleteConfigCardPortList = [];
                }
            }
            else {
                $("#interstitial").hide();
                return true;
            }
            function UpdateDatabase() {
                DeleteCheckedPortConfigRecords();
            }

            function DeleteCheckedPortConfigRecords() {
                if (selfcard.DeleteConfigCardPortList.length > 0) {
                    var JsonDeletedata = {
                        portconfigDeleteList: selfcard.DeleteConfigCardPortList
                    };
                    selfcard.AddJsonres(JsonDeletedata);
                    var saveJSON = mapping.toJS(selfcard.AddJsonres());
                    return $.ajax({
                        type: "POST",
                        url: 'api/specn/DeleteCardPortsConfigurations/',
                        data: JSON.stringify(saveJSON),
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: saveDeleteSuccess,
                        error: saveError
                    });
                }
                else {
                    SavePortConfigRecords();
                }
                function saveDeleteSuccess(data, status) {
                    saveSuccess(data, status);
                    SavePortConfigRecords();
                }
            }
            function SavePortConfigRecords() {
                if (SaveArray.length > 0) {
                    var Jsondata = {
                        portconfigDtls: SaveArray
                    };
                    selfcard.AddJsonres(Jsondata);
                    var saveJSON = mapping.toJS(selfcard.AddJsonres());
                    return $.ajax({
                        type: "POST",
                        url: 'api/specn/SaveCardPortsConfigurations/',
                        data: JSON.stringify(saveJSON),
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: saveUpdateSuccess,
                        error: saveError
                    });
                }
                else {
                    PopulatePortConfigGrid(portDefId);
                }
                function saveUpdateSuccess(data, status) {
                    saveSuccess(data, status);
                    // If atleast one records is failed to save, show the alert message..
                    // ..and stay in edit page without refreshing page 
                    if (fail) {
                        $("#interstitial").hide();
                        app.showMessage(failcnt + " Row/s failed to save", 'Failed');
                    }
                    else {
                        $("#interstitial").hide();
                        app.showMessage("Saved Successfully", 'Success');
                        // On success refresh page
                        PopulatePortConfigGrid(portDefId);
                    }
                }

            }
            function saveError(data) {
                $("#interstitial").hide();
                app.showMessage(data, 'FAILURE');
            }

            function saveSuccess(data, status) {
                var responseArray = data.split(";");
                // Loop through each row status and update table row with Success/Fail image
                responseArray.forEach(function (response) {
                    var stats = response.split(":");
                    if (stats.length > 1) {
                        var _guid = stats[0];
                        // Find the row and element for status update
                        //var $tr = $ui.find("tr#port" + _guid);
                        var $tri = $ui.find("tr#port" + _guid).find("#idStatus");
                        if (stats[2].toUpperCase() === "SUCCESS") {
                            if (stats[1].toUpperCase() === "INSERT") {
                                // the following "for" statement works with IE11 whereas the findIndex does not
                                for (let i = 0; i < selfcard.ConfigCardPortList().length; i++) {
                                    if (selfcard.ConfigCardPortList()[i]._guid === guid) {
                                        index = i;
                                        break;
                                    }
                                }
                                //index = selfcard.ConfigCardPortList().findIndex(x => x._guid === _guid);
                                selfcard.ConfigCardPortList()[index].CardPortDetailsId = stats[3];
                            }
                            // On success update with Check icon
                            $tri.removeClass().addClass("fa fa-check text-success");
                            if ($tri.data('ui-tooltip')) {
                                $tri.tooltip('destroy');
                            }
                            $tri.attr('title', stats[2]).tooltip();
                        }
                        else {
                            // On failure update with Exclamation icon
                            $tri.removeClass().addClass("fa fa-exclamation-triangle text-danger");
                            if ($tri.data('ui-tooltip')) {
                                $tri.tooltip('destroy');
                            }
                            $tri.attr('title', stats[3]).tooltip();
                            fail = true;
                            failcnt++;
                        }
                    }
                });
            }

        };
        //-------------- AB48512 Port Configure chaanges ends ------------    	//

        //Export data as excel report format
        CardSpecification.prototype.exportCardSpecReport = function () {
            //alert("Hi");

            var mainObj;
            //Excel report generation
            var excel = $JExcel.new("Calibri light 10 #333333");			// Default font

            excel.set({ sheet: 0, value: "Searched Specification List" });   //Add sheet 1 for Specification search details.
            excel.addSheet("Specification Details");                 //Add sheet 2 for selected specification id details 

            //Start writing Searched Material data into sheet 1
            var speclheaders = ["Specification ID", "Name", "Description", "Completed", "Propagated", "Unusable in NDS", "Specification Type", "Specification Class"
            ];							// This array holds the HEADERS text

            var formatHeader = excel.addStyle({ 															// Format for headers
                border: "none,none,none,thin #333333", 													    // 		Border for header
                font: "Calibri 12 #1b834c B"
            });

            for (var i = 0; i < speclheaders.length; i++) {																// Loop all the haders
                excel.set(0, i, 0, speclheaders[i], formatHeader);													// Set CELL with header text, using header format
                excel.set(0, i, undefined, "auto");															// Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }
            console.log("start calling table text");

            var table = $("#idSpecList tbody");

            table.find('tr').each(function (i) {
                var $tds = $(this).find('td').find('span');
                // i = i - 1;
                excel.set(0, 0, i + 1, $tds.eq(0).text());              // Specification ID
                excel.set(0, 1, i + 1, $tds.eq(1).text());              // Name
                excel.set(0, 2, i + 1, $tds.eq(2).text());              // Description
                excel.set(0, 3, i + 1, $tds.eq(3).text());              // Completed
                excel.set(0, 4, i + 1, $tds.eq(4).text());              // Propagated
                excel.set(0, 5, i + 1, $tds.eq(5).text());              // Unusable in NDS
                excel.set(0, 6, i + 1, $tds.eq(6).text());              // Specification Type
                excel.set(0, 7, i + 1, $tds.eq(7).text());              // Specification Class
                i = i + 1;
            });

            console.log("end of calling table text");

            //Start writing Searched Material data into sheet 1
            var cardSpecHeaders = ["Record Only", "Completed", "Propagated", "Unusable in NDS", "Specification ID", "Name", "Description", "Model"
                           , "CLEI", "HECI", "Use Type", "Manufacturer", "Manufacturer Name", "Part Number", "Material Code", "Material Description"
                           , "Depth", "Height", "Width", "Dimensions Unit of Measurement", "Normal Current Drain", "Unit of Measurement"
                           , "Peak Current Drain", "Unit of Measurement", "Power", "Unit of Measurement", "Card Weight", "Unit of Measurement"
                           , "Has Slots", "Has Ports", "Straight Through",

            ];							// This array holds the HEADERS text
          

            for (var i = 0; i < cardSpecHeaders.length; i++) {											//  Loop all the haders
                //alert(JSON.parse(selfspec.reptDtls[i]).key);
                excel.set(1, i, 0, cardSpecHeaders[i], formatHeader);									//  Set CELL with header text, using header format
                excel.set(1, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }
            //Writing data into sheet 2 for selected specification
            var i = 0;

            var RO = typeof (selfcard.selectedCardSpec().Card.list[0].RO) === "undefined" ? "" : selfcard.selectedCardSpec().Card.list[0].RO.bool;
            var Cmplt = typeof (selfcard.selectedCardSpec().Card.list[0].Cmplt) === "undefined" ? "" : selfcard.selectedCardSpec().Card.list[0].Cmplt.bool;
            var Prpgtd = typeof (selfcard.selectedCardSpec().Card.list[0].Prpgtd) === "undefined" ? "" : selfcard.selectedCardSpec().Card.list[0].Prpgtd.bool;
            var Dltd = typeof (selfcard.selectedCardSpec().Card.list[0].Dltd) === "undefined" ? "" : selfcard.selectedCardSpec().Card.list[0].Dltd.bool;

            excel.set(1, 0, i + 1, RO);                                                                 // Record Only
            excel.set(1, 1, i + 1, Cmplt);                                                              // Completed
            excel.set(1, 2, i + 1, Prpgtd);                                                             // Propagated
            excel.set(1, 3, i + 1, Dltd);                                                               // Unusable in NDS
            excel.set(1, 4, i + 1, selfcard.selectedCardSpec().Card.list[0].id.value);                  // Specification ID
            excel.set(1, 5, i + 1, selfcard.selectedCardSpec().RvsnNm.value);                           // Name
            excel.set(1, 6, i + 1, selfcard.selectedCardSpec().Desc.value);                // Description
            excel.set(1, 7, i + 1, selfcard.selectedCardSpec().Nm.value);                  // Model
            excel.set(1, 8, i + 1, $("#modelTxt").text());                                                // CLEI   
            excel.set(1, 9, i + 1, $("#modelTxt").text());                                                // HECI
            excel.set(1, 10, i + 1, selfcard.selectedCardSpec().CardUseTypId.value);       // Use Type
            excel.set(1, 11, i + 1, selfcard.selectedCardSpec().Card.list[0].Mfg.value);                // Manufacturer
            excel.set(1, 12, i + 1, selfcard.selectedCardSpec().Card.list[0].MfgNm.value);              // Manufacturer Name

            excel.set(1, 13, i + 1, selfcard.selectedCardSpec().Card.list[0].PrtNo.value);              // Part Number
            excel.set(1, 14, i + 1, selfcard.selectedCardSpec().Card.list[0].PrdctId.value);            // Material Code
            excel.set(1, 15, i + 1, selfcard.selectedCardSpec().Card.list[0].ItmDesc.value);            // Material Description
            excel.set(1, 16, i + 1, selfcard.selectedCardSpec().Card.list[0].Dpth.value);               // Depth
            excel.set(1, 17, i + 1, selfcard.selectedCardSpec().Card.list[0].Hght.value);               // Height
            excel.set(1, 18, i + 1, selfcard.selectedCardSpec().Card.list[0].Wdth.value);               // Width

            var indInt = selfcard.selectedCardSpec().Card.list[0].DimUom.options.map(function (img) { return img.value; }).indexOf(selfcard.selectedCardSpec().Card.list[0].DimUom.value);

            excel.set(1, 19, i + 1, selfcard.selectedCardSpec().Card.list[0].DimUom.options[indInt].text);             // Dimensions Unit of Measurement
            excel.set(1, 20, i + 1, selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnNrm.value);      // Normal Current Drain

            var indInt = selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnNrmUom.options.map(function (img) { return img.value; }).indexOf(selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnNrmUom.value);
            excel.set(1, 21, i + 1, selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnNrmUom.options[indInt].text);   // Unit of Measurement
            excel.set(1, 22, i + 1, selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnMx.value);       // Peak Current Drain

            var indInt = selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnMxUom.options.map(function (img) { return img.value; }).indexOf(selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnMxUom.value);
            excel.set(1, 23, i + 1, selfcard.selectedCardSpec().Card.list[0].ElcCurrDrnMxUom.options[indInt].text);    // Unit of Measurement
            excel.set(1, 24, i + 1, selfcard.selectedCardSpec().Card.list[0].HtDssptn.value);           // Power

            var indInt = selfcard.selectedCardSpec().Card.list[0].HtDssptnUom.options.map(function (img) { return img.value; }).indexOf(selfcard.selectedCardSpec().Card.list[0].HtDssptnUom.value);
            excel.set(1, 25, i + 1, selfcard.selectedCardSpec().Card.list[0].HtDssptnUom.options[indInt].text);        // Unit of Measurement
            excel.set(1, 26, i + 1, selfcard.selectedCardSpec().Card.list[0].Wght.value);               // Card Weight

            var indInt = selfcard.selectedCardSpec().Card.list[0].WghtUom.options.map(function (img) { return img.value; }).indexOf(selfcard.selectedCardSpec().Card.list[0].WghtUom.value);
            excel.set(1, 27, i + 1, selfcard.selectedCardSpec().Card.list[0].WghtUom.options[indInt].text);            // Unit of Measurement

            var Slts = typeof (selfcard.selectedCardSpec().Slts.bool) === "undefined" ? "" : selfcard.selectedCardSpec().Slts.bool;
            var Prts = typeof (selfcard.selectedCardSpec().Prts.bool) === "undefined" ? "" : selfcard.selectedCardSpec().Prts.bool;
            excel.set(1, 28, i + 1, Slts);                                                              // Has Slots
            excel.set(1, 29, i + 1, Prts);                                                              // Has Ports
            excel.set(1, 30, i + 1, selfcard.selectedCardSpec().StrghtThru.value);         // Straight Through

            //fetch the associated material details
            selfcard.associatepartCard();
            $("#cardModalpopup").hide();
            //Start writing associate details into sheet 3
            excel.addSheet("Search result of Associated Material");                 //Add sheet 2 for selected specification id details 
            var assoSearchcHeaders = ["CDMMS ID", "Material Code", "CLMC", "Part Number", "Catalog/Material Description"];							// This array holds the HEADERS text
                       

            for (var i = 0; i < assoSearchcHeaders.length; i++) {											//  Loop all the haders               
                excel.set(2, i, 0, assoSearchcHeaders[i], formatHeader);									//  Set CELL with header text, using header format
                excel.set(2, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }
            
            var i = 0;

            if (selfcard.searchtblcard()!= false)
            {
                if(selfcard.searchtblcard().length >0)
                {
                    excel.set(2, 0, i + 1, selfcard.searchtblcard()[i].material_item_id.value);        // CDMMS ID
                    excel.set(2, 1, i + 1, selfcard.searchtblcard()[i].product_id.value);              // Material Code
                    excel.set(2, 2, i + 1, selfcard.searchtblcard()[i].mfg_id.value);                  // CLMC
                    excel.set(2, 3, i + 1, selfcard.searchtblcard()[i].mfg_part_no.value);             // Part Number
                    excel.set(2, 4, i + 1, selfcard.searchtblcard()[i].item_desc.value);               // Catalog/Material Description
                }
            }
            //End of writing associate details into sheet 3

            //Start writing associate details into sheet 4
            excel.addSheet("Associated Material");                 //Add sheet 2 for selected specification id details 
            var assoSearchcHeaders = ["CDMMS ID", "Material Code", "CLMC", "Part Number", "Catalog/Material Description"];							// This array holds the HEADERS text


            for (var i = 0; i < assoSearchcHeaders.length; i++) {											//  Loop all the haders               
                excel.set(3, i, 0, assoSearchcHeaders[i], formatHeader);									//  Set CELL with header text, using header format
                excel.set(3, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }
           
            var i = 0;

            if (selfcard.associatemtlblck() != false) {
                if (selfcard.associatemtlblck().length > 0) {
                    excel.set(3, 0, i + 1, selfcard.associatemtlblck()[i].material_item_id.value);        // CDMMS ID
                    excel.set(3, 1, i + 1, selfcard.associatemtlblck()[i].product_id.value);              // Material Code
                    excel.set(3, 2, i + 1, selfcard.associatemtlblck()[i].mfg_id.value);                  // CLMC
                    excel.set(3, 3, i + 1, selfcard.associatemtlblck()[i].mfg_part_no.value);             // Part Number
                    excel.set(3, 4, i + 1, selfcard.associatemtlblck()[i].item_desc.value);               // Catalog/Material Description
                }
            }
            //End of writing associate details into sheet 4
            excel.generate("Report_Card_Specification.xlsx");
        };

        //End of report exporting
        // card has port checkbox clicked design end.
        //
        return CardSpecification;
    });