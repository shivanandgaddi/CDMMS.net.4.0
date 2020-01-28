define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', 'jqueryui', '../Utility/referenceDataHelper', 'durandal/app', 'datablescelledit', 'bootstrapJS', 'plugins/router', '../Utility/user'],
    function (composition, ko, $, http, activator, mapping, system, jqueryui, reference, app, datablescelledit, bootstrapJS, router, user) {
        var ShelfSpecification = function (data) {
            selfshs = this;
            specChangeArray = [];
            var dataResult = data.resp;
            var results = JSON.parse(dataResult);
            selfshs.specification = data.specification;

            results.Dpth.value = Number(results.Dpth.value).toFixed(3);
            results.Hght.value = Number(results.Hght.value).toFixed(3);
            results.Wdth.value = Number(results.Wdth.value).toFixed(3);
            if (results.Shelf != null && results.Shelf.list.length > 0) {
                results.Shelf.list[0].Dpth.value = Number(results.Shelf.list[0].Dpth.value).toFixed(3);
                results.Shelf.list[0].Hght.value = Number(results.Shelf.list[0].Hght.value).toFixed(3);
                results.Shelf.list[0].Wdth.value = Number(results.Shelf.list[0].Wdth.value).toFixed(3);
                // Ensure there is a leading zero when a fraction like .75 comes across.  should display as 0.75
                if (results.Shelf.list[0].Wdth.value < 1 && results.Shelf.list[0].Wdth.value > 0 && results.Shelf.list[0].Wdth.value.substring(0, 1) == '.') {
                    results.Shelf.list[0].Wdth.value = '0' + results.Shelf.list[0].Wdth.value;
                }
                if (results.Shelf.list[0].Hght.value < 1 && results.Shelf.list[0].Hght.value > 0 && results.Shelf.list[0].Hght.value.substring(0, 1) == '.') {
                    results.Shelf.list[0].Hght.value = '0' + results.Shelf.list[0].Hght.value;
                }
                if (results.Shelf.list[0].Dpth.value < 1 && results.Shelf.list[0].Dpth.value > 0 && results.Shelf.list[0].Dpth.value.substring(0, 1) == '.') {
                    results.Shelf.list[0].Dpth.value = '0' + results.Shelf.list[0].Dpth.value;
                }
            }
            else {
                results.Dpth.value = Number(results.Dpth.value).toFixed(3);
                results.Hght.value = Number(results.Hght.value).toFixed(3);
                results.Wdth.value = Number(results.Wdth.value).toFixed(3);
                // Ensure there is a leading zero when a fraction like .75 comes across.  should display as 0.75
                if (results.Wdth.value < 1 && results.Wdth.value > 0 && results.Wdth.value.substring(0, 1) == '.') {
                    results.Wdth.value = '0' + results.Wdth.value;
                }
                if (results.Hght.value < 1 && results.Hght.value > 0 && results.Hght.value.substring(0, 1) == '.') {
                    results.Hght.value = '0' + results.Hght.value;
                }
                if (results.Dpth.value < 1 && results.Dpth.value > 0 && results.Dpth.value.substring(0, 1) == '.') {
                    results.Dpth.value = '0' + results.Dpth.value;
                }
            }
            selfshs.selectedShelfSpec = ko.observable();
            selfshs.selectedShelfSpec(results);
            selfshs.associatemtlblck = ko.observableArray();
            selfshs.searchtblshelf = ko.observableArray();
            selfshs.genericSelected = ko.observable(false);
            selfshs.completedNotSelected = ko.observable(true);
            selfshs.shelfRoleTypeListTbl = ko.observableArray();
            selfshs.enableAssociate = ko.observable(false);
            selfshs.enableModalSave = ko.observable(false);
            selfshs.enableName = ko.observable(true);
            selfshs.enableBackButton = ko.observable(false);
            selfshs.shelfRoleTypeListTbl(selfshs.selectedShelfSpec().SpcnRlTypLst.list);
            selfshs.nongenericBlockview = ko.observable();
            selfshs.specName = ko.observable('');
            selfshs.duplicateSelectedShelfSpecName = ko.observable();

            selfshs.duplicateshelfRoleTypeListTbl = ko.observableArray();
            selfshs.duplicateshelfRoleTypeListTbl = [];
            selfshs.duplicateSelectedShelfSpecDpth = ko.observable();
            selfshs.duplicateSelectedShelfSpecHght = ko.observable();
            selfshs.duplicateSelectedShelfSpecWdth = ko.observable();
            selfshs.duplicateSelectedShelfSpecStrghtThru = ko.observable();
            selfshs.duplicateSelectedShelfSpecMidPln = ko.observable();
            selfshs.duplicateSelectedShelfSpecDltd = ko.observable();
            selfshs.duplicateSelectedShelfSpecDltd = results.Dltd.bool;
            selfshs.duplicateShelfNDSUseTyp = ko.observable()
            selfshs.duplicateShelfNDSUseTyp(selfshs.selectedShelfSpec().ShelfNDSUseTyp.value);

            if (results.SpcnRlTypLst.list != null && results.SpcnRlTypLst.list.length > 0) {
                for (var x = 0; x < results.SpcnRlTypLst.list.length; x++) {
                    selfshs.duplicateshelfRoleTypeListTbl[x] = results.SpcnRlTypLst.list[x].Slctd.bool;
                }
            }

            if (results.Shelf != null && results.Shelf.list.length > 0) {
                selfshs.duplicateSelectedShelfSpecDpth(results.Shelf.list[0].Dpth.value);
                selfshs.duplicateSelectedShelfSpecHght(results.Shelf.list[0].Hght.value);
                selfshs.duplicateSelectedShelfSpecWdth(results.Shelf.list[0].Wdth.value);
            }
            else {
                selfshs.duplicateSelectedShelfSpecDpth(results.Dpth.value);
                selfshs.duplicateSelectedShelfSpecHght(results.Hght.value);
                selfshs.duplicateSelectedShelfSpecWdth(results.Wdth.value);
            }

            if (results.StrghtThru != null) {
                selfshs.duplicateSelectedShelfSpecStrghtThru(results.StrghtThru.value);
            }

            if (results.MidPln != null) {
                selfshs.duplicateSelectedShelfSpecMidPln(results.MidPln.value);
            }

            if (selfshs.selectedShelfSpec().Gnrc.bool) {
                selfshs.genericSelected(true);
                selfshs.enableName(true);
                selfshs.nongenericBlockview(false);

                selfshs.duplicateSelectedShelfSpecDpth(selfshs.selectedShelfSpec().Dpth.value);
                selfshs.duplicateSelectedShelfSpecHght(selfshs.selectedShelfSpec().Hght.value);
                selfshs.duplicateSelectedShelfSpecWdth(selfshs.selectedShelfSpec().Wdth.value);
            } else {
                selfshs.enableAssociate(true);
                selfshs.nongenericBlockview(true);
            }

            if (selfshs.selectedShelfSpec().Nm.value != "") {
                if (selfshs.selectedShelfSpec().Gnrc.bool == true) {
                    selfshs.specName(selfshs.selectedShelfSpec().Nm.value);
                } else {
                    selfshs.specName(selfshs.selectedShelfSpec().RvsnNm.value);
                }
            }

            if (results.Nm != null) {
                selfshs.duplicateSelectedShelfSpecName.value = selfshs.specName();
            }

            selfshs.duplicateSelectedShelfSpecName.value = selfshs.specName();

            var useTypeOptions = new Array();
            for (var x = 0; x < results.SpcnUseTypLst.list.length; x++) {
                useTypeOptions[x] = results.SpcnUseTypLst.list[x].SpcnRlTyp.value
            }
            selfshs.useTypeList = ko.observableArray();
            selfshs.useTypeList(useTypeOptions);

            if (selfshs.selectedShelfSpec().ShelfNDSUseTyp.value === '') {
                selfshs.selectedShelfSpec().ShelfNDSUseTyp.value = 'mit_shelf';  //default
            }

            setTimeout(function () {
                if (document.getElementById('idShelfDimUom') !== null) {
                    if (document.getElementById('idShelfDimUom').value == '') {
                        document.getElementById('idShelfDimUom').value = '22';
                    }
                }
                if (document.getElementById('idDUOM') !== null) {
                    if (document.getElementById('idDUOM').value == '') {
                        document.getElementById('idDUOM').value = '22';
                    }
                }
            }, 5000);
            if (selfshs.selectedShelfSpec().Cmplt.bool && selfshs.selectedShelfSpec().Prpgtd.enable) {
                selfshs.completedNotSelected(false);
            }
            if (selfspec.backToMtlItmId > 0) {
                selfshs.enableBackButton(true);
            }
            if (selfspec.selectRadioSpec() == 'newSpec') {
                selfshs.selectedShelfSpec().SltsRwQty.value = '';
                selfshs.selectedShelfSpec().StrtSltNo.value = '';
                selfshs.selectedShelfSpec().StrghtThru.value = 'Y';
                //selfshs.selectedShelfSpec().RO.enable = true;
                //selfshs.selectedShelfSpec().Gnrc.enable = true;
            }

            if (selfshs.selectedShelfSpec().Shelf !== undefined) {
                if (selfshs.selectedShelfSpec().Shelf.list[0].ElcCurrDrnNrmUom.value == "" && document.getElementById('txtNrmlDrn')) {
                    document.getElementById('txtNrmlDrn').value = "";
                }

                if (selfshs.selectedShelfSpec().Shelf.list[0].ElcCurrDrnNrm.value == "" && document.getElementById('idNrmlDrnUom')) {
                    document.getElementById('idNrmlDrnUom').value = "";
                }

                if (selfshs.selectedShelfSpec().Shelf.list[0].ElcCurrDrnMxUom.value == "" && document.getElementById('txtPkDrn')) {
                    document.getElementById('txtPkDrn').value = "";
                }

                if (selfshs.selectedShelfSpec().Shelf.list[0].ElcCurrDrnMx.value == "" && document.getElementById('idPkDrnUom')) {
                    document.getElementById('idPkDrnUom').value = "";
                }

                if (selfshs.selectedShelfSpec().Shelf.list[0].HtDssptnUom.value == "" && document.getElementById('txtPwr')) {
                    document.getElementById('txtPwr').value = "";
                }

                if (selfshs.selectedShelfSpec().Shelf.list[0].HtDssptn.value == "" && document.getElementById('idPwrUom')) {
                    document.getElementById('idPwrUom').value = "";
                }

                if (selfshs.selectedShelfSpec().Shelf.list[0].Wght.value == "" && document.getElementById('idWtUom')) {
                    document.getElementById('idWtUom').value = "";
                }

                if (selfshs.selectedShelfSpec().Shelf.list[0].WghtUom.value == "" && document.getElementById('txtWt')) {
                    document.getElementById('txtWt').value = "";
                }
                setTimeout(function () {
                    document.getElementById('txtDpth').required = true;
                    document.getElementById('txtHght').required = true;
                    document.getElementById('txtWdth').required = true;
                }, 5000);
            }

            //$(document).on('change', '[type=checkbox]', function () {
            //    if (this.name == "proptIndBay") {
            //        if (this.checked == true) {
            //            selfshs.enableAssociate(true);
            //        }
            //        else {
            //            selfshs.enableAssociate(false);
            //        }
            //    }
            //});

            $(document).on('change', '[type=checkbox]', function () {
                if (this.name == "cmpIndnmshelf") {
                    if (this.checked == false) {
                        document.getElementById('shelfPrpgtdChbk').checked = false;
                    }
                }
            });
        };

        ShelfSpecification.prototype.navigateToMaterial = function () {
            if (document.getElementById('shelfROChbk').checked == true) {
                var url = '#/roNew/' + selfspec.backToMtlItmId;
                router.navigate(url, false);
            }
            else {
                var url = '#/mtlInv/' + selfspec.backToMtlItmId;
                router.navigate(url, false);
            }
        };

        ShelfSpecification.prototype.NumDecimal = function (mp, event) {
            //self.saveValidation(true);
            var charCode = event.keyCode;
            var name = event.currentTarget.id;
            var value = $('#' + name).val();
            var reqCharAfterdot = 3;

            if (name == "txtHght" || name == "txtWdth" || name == "txtDpth" || name == "idHeight" || name == "idHeight" || name == "idWidth") {
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

        ShelfSpecification.prototype.onchangeShelfGeneric = function () {
            if ($("#GenericChbk").is(':checked')) {
                selfshs.genericSelected(true);
                selfshs.selectedShelfSpec().RO.enable = false;
                selfshs.enableAssociate(false);
                selfshs.enableName(true);
                selfshs.nongenericBlockview(false);

                document.getElementById('shelfROChbk').disabled = true;
                document.getElementById("idHeight").required = true;
                document.getElementById("idWidth").required = true;
                document.getElementById("idDepth").required = true;
                document.getElementById("idDUOM").required = true;
                document.getElementById("modelTxt").required = false;
            } else {
                selfshs.genericSelected(false);
                selfshs.selectedShelfSpec().RO.enable = true;
                selfshs.enableAssociate(true);
                selfshs.enableName(true);
                selfshs.nongenericBlockview(true);

                document.getElementById('shelfROChbk').disabled = false;
                document.getElementById("idHeight").required = false;
                document.getElementById("idWidth").required = false;
                document.getElementById("idDepth").required = false;
                document.getElementById("idDUOM").required = false;
                document.getElementById("modelTxt").required = true;
            }
        };

        ShelfSpecification.prototype.onchangeShelfRecordOnly = function () {
            if ($("#shelfROChbk").is(':checked')) {
                selfshs.selectedShelfSpec().Gnrc.enable = false;
                document.getElementById('GenericChbk').disabled = true;
            } else {
                selfshs.selectedShelfSpec().Gnrc.enable = true;
                document.getElementById('GenericChbk').disabled = false;
            }
        };

        ShelfSpecification.prototype.onchangeShelfCompleted = function () {
            if ($("#shelfCmpltChbk").is(':checked')) {
                selfshs.completedNotSelected(false);
            } else {
                selfshs.completedNotSelected(true);
                selfshs.selectedShelfSpec().Prpgtd.bool = false;
            }
        };

        ShelfSpecification.prototype.updateShelfSpec = function () {

            // check for duplicate model name
            var modelname = document.getElementById("modelTxt").value;
            if (selfspec.selectRadioSpec() == 'existSpec') {
                var modelnamecount = selfshs.GetModelNameCount(modelname, selfshs.selectedShelfSpec().Shelf.list[0].SpecId.value);
            }
            else {
                var modelnamecount = selfshs.GetModelNameCount(modelname, 0);
            }
            if (modelnamecount > 0) {
                app.showMessage('The model name ' + modelname + ' already exists on a different Spec.');
                return;
            }

            specChangeArray = [];
            var chkgenrc = false;

            if ($("#GenericChbk").is(':checked')) {
                chkgenrc = true;
            }

            if ((chkgenrc) || (!chkgenrc && selfshs.selectedShelfSpec().Shelf !== undefined)) {
                selfshs.selectedShelfSpec().SpcnRlTypLst.list = selfshs.shelfRoleTypeListTbl();

                var arr = [];
                var priorityNull = '';

                for (var i = 0; i < selfshs.shelfRoleTypeListTbl().length; i++) {
                    if (selfshs.shelfRoleTypeListTbl()[i].PrtyNo.value != 0) {
                        arr.push(selfshs.shelfRoleTypeListTbl()[i].PrtyNo.value)
                    } else {
                        if (selfshs.shelfRoleTypeListTbl()[i].Slctd.bool) {
                            priorityNull += "Cannot have priority <b>'0'</b> for a selected Role: <b>" + selfshs.shelfRoleTypeListTbl()[i].SpcnRlTyp.value + "</b>";
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

                if (duplicatePriority.length > 0) {
                    errorMessage = 'Selected role types ';
                }

                for (var j = 0; j < duplicatePriority.length; j++) {
                    for (var i = 0; i < selfshs.shelfRoleTypeListTbl().length; i++) {
                        if (selfshs.shelfRoleTypeListTbl()[i].PrtyNo.value == duplicatePriority[j]) {
                            errorMessage += selfshs.shelfRoleTypeListTbl()[i].SpcnRlTyp.value + ", ";
                        }
                    }

                    errorMessage = errorMessage.substring(0, errorMessage.length - 2);
                    errorMessage += " may not have the same priority <b>" + duplicatePriority[j] + "</b>.<br/><br/>";
                }


                var valSltsRwQtyLbl = $("#valSltsRwQtyLblId").val();

                if (valSltsRwQtyLbl !== '' && !(valSltsRwQtyLbl > 0 && valSltsRwQtyLbl < 1000)) {
                    $("#valSltsRwQtyLbl").show();
                }
                else if (duplicatePriority.length > 0 || priorityNull.length > 0) {
                    $("#interstitial").hide();

                    errorMessage += priorityNull;

                    $("#roleTypeValidTblDgr").html(errorMessage);
                    $("#roleTypeValidTblDgr").show();
                } else {
                    selfshs.shelfupdateChbk();

                    if (selfshs.selectedShelfSpec().Gnrc.bool == true)
                        selfshs.selectedShelfSpec().Nm.value = selfshs.specName();
                    else
                        selfshs.selectedShelfSpec().RvsnNm.value = selfshs.specName();

                    if (selfshs.selectedShelfSpec().Shelf !== undefined) {
                        selfshs.selectedShelfSpec().Shelf.list[0].ElcCurrDrnNrm.value = $("#txtNrmlDrn").val();
                        selfshs.selectedShelfSpec().Shelf.list[0].ElcCurrDrnMx.value = $("#txtPkDrn").val();
                        selfshs.selectedShelfSpec().Shelf.list[0].HtDssptn.value = $("#txtPwr").val();
                        selfshs.selectedShelfSpec().Shelf.list[0].Wght.value = $("#txtWt").val();
                    } else {
                        selfshs.selectedShelfSpec().Hght.value = $("#idHeight").val();
                        selfshs.selectedShelfSpec().Wdth.value = $("#idWidth").val();
                        selfshs.selectedShelfSpec().Dpth.value = $("#idDepth").val();
                    }

                    selfshs.selectedShelfSpec().SltsRwQty.value = $("#valSltsRwQtyLblId").val();
                    selfshs.selectedShelfSpec().StrtSltNo.value = $("#idStrtSltNo").val();

                    var txtCondt = '';

                    if (selfspec.selectRadioSpec() == 'existSpec' && selfshs.selectedShelfSpec().Shelf) {
                        if (selfshs.selectedShelfSpec().Shelf != null && selfshs.selectedShelfSpec().Shelf.list.length > 0) {
                            if (selfshs.specName() != selfshs.duplicateSelectedShelfSpecName.value) {
                                txtCondt += "Name changed to <b>" + selfshs.specName() + '</b> from ' + selfshs.duplicateSelectedShelfSpecName.value + "<br/>";

                                var saveJSON = {
                                    'tablename': 'shelf_specn', 'columnname': 'shelf_specn_nm', 'audittblpkcolnm': 'shelf_specn_id', 'audittblpkcolval': selfshs.selectedShelfSpec().Shelf.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfshs.duplicateSelectedShelfSpecName.value,
                                    'newcolval': selfshs.specName(),
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Spec Name from ' + selfshs.duplicateSelectedShelfSpecName.value + ' on ', 'materialitemid': selfshs.selectedShelfSpec().Shelf.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }
                            if (Number(selfshs.selectedShelfSpec().Shelf.list[0].Dpth.value).toFixed(3) != Number(selfshs.duplicateSelectedShelfSpecDpth()).toFixed(3)) {
                                txtCondt += "Depth changed to <b>" + Number(selfshs.selectedShelfSpec().Shelf.list[0].Dpth.value).toFixed(3) + '</b> from ' + Number(selfshs.duplicateSelectedShelfSpecDpth()).toFixed(3) + "<br/>";

                                var saveJSON = {
                                    'tablename': 'shelf_specn_revsn_alt', 'columnname': 'dpth_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfshs.selectedShelfSpec().Shelf.list[0].MtrlId.value, 'auditprnttblpkcolnm': '',
                                    'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfshs.duplicateSelectedShelfSpecDpth()).toFixed(3), 'newcolval': Number(selfshs.selectedShelfSpec().Shelf.list[0].Dpth.value).toFixed(3), 'cuid': user.cuid,
                                    'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Depth from ' + Number(selfshs.duplicateSelectedShelfSpecDpth()).toFixed(3) + ' to ' + Number(selfshs.selectedShelfSpec().Shelf.list[0].Dpth.value).toFixed(3) + ' on ',
                                    'materialitemid': selfshs.selectedShelfSpec().Shelf.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (Number(selfshs.selectedShelfSpec().Shelf.list[0].Hght.value).toFixed(3) != Number(selfshs.duplicateSelectedShelfSpecHght()).toFixed(3)) {
                                txtCondt += "Height changed to <b>" + Number(selfshs.selectedShelfSpec().Shelf.list[0].Hght.value).toFixed(3) + '</b> from ' + Number(selfshs.duplicateSelectedShelfSpecHght()).toFixed(3) + "<br/>";

                                var saveJSON = {
                                    'tablename': 'shelf_specn_revsn_alt', 'columnname': 'hgt_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfshs.selectedShelfSpec().Shelf.list[0].MtrlId.value, 'auditprnttblpkcolnm': '',
                                    'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfshs.duplicateSelectedShelfSpecHght()).toFixed(3), 'newcolval': Number(selfshs.selectedShelfSpec().Shelf.list[0].Hght.value).toFixed(3), 'cuid': user.cuid,
                                    'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Height from ' + Number(selfshs.duplicateSelectedShelfSpecHght()).toFixed(3) + ' to ' + Number(selfshs.selectedShelfSpec().Shelf.list[0].Hght.value).toFixed(3) + ' on ',
                                    'materialitemid': selfshs.selectedShelfSpec().Shelf.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (Number(selfshs.selectedShelfSpec().Shelf.list[0].Wdth.value).toFixed(3) != Number(selfshs.duplicateSelectedShelfSpecWdth()).toFixed(3)) {
                                txtCondt += "Width changed to <b>" + Number(selfshs.selectedShelfSpec().Shelf.list[0].Wdth.value).toFixed(3) + '</b> from ' + Number(selfshs.duplicateSelectedShelfSpecWdth()).toFixed(3) + "<br/>";

                                var saveJSON = {
                                    'tablename': 'shelf_specn_revsn_alt', 'columnname': 'wdth_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfshs.selectedShelfSpec().Shelf.list[0].MtrlId.value, 'auditprnttblpkcolnm': '',
                                    'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfshs.duplicateSelectedShelfSpecWdth()).toFixed(3), 'newcolval': Number(selfshs.selectedShelfSpec().Shelf.list[0].Wdth.value).toFixed(3), 'cuid': user.cuid,
                                    'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Width from ' + Number(selfshs.duplicateSelectedShelfSpecWdth()).toFixed(3) + ' to ' + Number(selfshs.selectedShelfSpec().Shelf.list[0].Wdth.value).toFixed(3) + ' on ',
                                    'materialitemid': selfshs.selectedShelfSpec().Shelf.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfshs.duplicateSelectedShelfSpecStrghtThru() != selfshs.selectedShelfSpec().StrghtThru.value) {
                                txtCondt += "Straight Through changed to <b>" + selfshs.selectedShelfSpec().StrghtThru.value + '</b> from ' + selfshs.duplicateSelectedShelfSpecStrghtThru() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'shelf_specn', 'columnname': 'strght_thru_ind', 'audittblpkcolnm': 'shelf_specn_id', 'audittblpkcolval': selfshs.selectedShelfSpec().Shelf.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfshs.duplicateSelectedShelfSpecStrghtThru(),
                                    'newcolval': selfshs.selectedShelfSpec().StrghtThru.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Straight Thru from ' + selfshs.duplicateSelectedShelfSpecStrghtThru() + ' to ' + selfshs.selectedShelfSpec().StrghtThru.value + ' on ', 'materialitemid': selfshs.selectedShelfSpec().Shelf.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfshs.duplicateSelectedShelfSpecMidPln() != selfshs.selectedShelfSpec().MidPln.value) {
                                txtCondt += "Mid Plane changed to <b>" + selfshs.selectedShelfSpec().MidPln.value + '</b> from ' + selfshs.duplicateSelectedShelfSpecMidPln() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'shelf_specn', 'columnname': 'mid_pln_ind', 'audittblpkcolnm': 'shelf_specn_id', 'audittblpkcolval': selfshs.selectedShelfSpec().Shelf.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfshs.duplicateSelectedShelfSpecMidPln(),
                                    'newcolval': selfshs.selectedShelfSpec().MidPln.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Mid Plane from ' + selfshs.duplicateSelectedShelfSpecMidPln() + ' to ' + selfshs.selectedShelfSpec().MidPln.value + ' on ', 'materialitemid': selfshs.selectedShelfSpec().Shelf.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfshs.selectedShelfSpec().Dltd.bool != selfshs.duplicateSelectedShelfSpecDltd) {
                                txtCondt += "Unusable changed to <b>" + selfshs.selectedShelfSpec().Dltd.bool + '</b> from ' + selfshs.duplicateSelectedShelfSpecDltd + "<br/>";

                                var saveJSON = {
                                    'tablename': 'shelf_specn_revsn_alt', 'columnname': 'del_ind', 'audittblpkcolnm': 'shelf_specn_revsn_alt_id', 'audittblpkcolval': selfshs.selectedShelfSpec().Shelf.list[0].SpecRvsnId.value, 'auditprnttblpkcolnm': 'shelf_specn_id', 'auditprnttblpkcolval': selfshs.selectedShelfSpec().Shelf.list[0].SpecId.value, 'actncd': 'C',
                                    'oldcolval': selfshs.duplicateSelectedShelfSpecDltd,
                                    'newcolval': selfshs.selectedShelfSpec().Dltd.bool,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Unusable from ' + selfshs.duplicateSelectedshelfSpecDltd + ' to ' + selfshs.selectedShelfSpec().Dltd.bool + ' on ', 'materialitemid': selfshs.selectedShelfSpec().Shelf.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfshs.duplicateShelfNDSUseTyp() != selfshs.selectedShelfSpec().ShelfNDSUseTyp.value) {
                                txtCondt += "Use Type changed to <b>" + selfshs.selectedShelfSpec().ShelfNDSUseTyp.value + '</b> from ' + selfshs.duplicateShelfNDSUseTyp() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'shelf_specn', 'columnname': 'specn_record_use_typ_id', 'audittblpkcolnm': 'shelf_specn_id', 'audittblpkcolval': selfshs.selectedShelfSpec().Shelf.list[0].SpecId.value, 'auditprnttblpkcolnm': 'shelf_specn_id', 'auditprnttblpkcolval': selfshs.selectedShelfSpec().Shelf.list[0].SpecId.value, 'actncd': 'C',
                                    'oldcolval': selfshs.duplicateShelfNDSUseTyp(),
                                    'newcolval': selfshs.selectedShelfSpec().ShelfNDSUseTyp.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Use Type from ' + selfshs.duplicateShelfNDSUseTyp() + ' to ' + selfshs.selectedShelfSpec().ShelfNDSUseTyp.value + ' on ', 'materialitemid': selfshs.selectedShelfSpec().Shelf.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            // check selected role types for use type differences
                            var didItOnce = false;
                            for (var x = 0; x < selfshs.shelfRoleTypeListTbl().length; x++) {
                                if (selfshs.duplicateshelfRoleTypeListTbl[x] != selfshs.shelfRoleTypeListTbl()[x].Slctd.bool) {
                                    if (selfshs.selectedShelfSpec().ShelfUseTypId.value != selfshs.shelfRoleTypeListTbl()[x].UseTyp.value) {
                                        if (!didItOnce) {
                                            txtCondt += "Use Type changed to <b>" + selfshs.shelfRoleTypeListTbl()[x].UseTyp.value + '</b> from ' + selfshs.selectedShelfSpec().ShelfUseTypId.value + "<br/>";
                                            didItOnce = true;
                                        }
                                    }
                                }
                            }
                            // check to see if none are selected
                            var numberSelected = 0;
                            for (var x = 0; x < selfshs.shelfRoleTypeListTbl().length; x++) {
                                if (selfshs.shelfRoleTypeListTbl()[x].Slctd.bool == true) {
                                    numberSelected++;
                                }
                            }
                            if (numberSelected == 0) {
                                // check to see if selected alias is being unchecked back to the default
                                if (selfshs.selectedShelfSpec().ShelfUseTypId.value.substring(0, 3) != 'mit' && !didItOnce) {
                                    txtCondt += "Use Type changed to <b>mit_shelf</b> from " + selfshs.selectedShelfSpec().ShelfUseTypId.value + "<br/>";
                                }
                            }
                        }
                    } else if (selfspec.selectRadioSpec() == 'existSpec' && selfshs.selectedShelfSpec().Gnrc.bool) {
                        if (selfshs.specName() != selfshs.duplicateSelectedShelfSpecName.value) {
                            txtCondt += "Name changed to <b>" + selfshs.specName() + '</b> from ' + selfshs.duplicateSelectedShelfSpecName.value + "<br/>";

                            var saveJSON = {
                                'tablename': 'shelf_specn', 'columnname': 'shelf_specn_nm', 'audittblpkcolnm': 'shelf_specn_id', 'audittblpkcolval': selfshs.selectedShelfSpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                'oldcolval': selfshs.duplicateSelectedShelfSpecName.value,
                                'newcolval': selfshs.specName(),
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Spec Name from ' + selfshs.duplicateSelectedShelfSpecName.value + ' on ', 'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (Number(selfshs.selectedShelfSpec().Dpth.value).toFixed(3) != Number(selfshs.duplicateSelectedShelfSpecDpth()).toFixed(3)) {
                            txtCondt += "Depth changed to <b>" + Number(selfshs.selectedShelfSpec().Dpth.value).toFixed(3) + '</b> from ' + Number(selfshs.duplicateSelectedShelfSpecDpth()).toFixed(3) + "<br/>";

                            var saveJSON = {
                                'tablename': 'shelf_specn_gnrc', 'columnname': 'dpth_no', 'audittblpkcolnm': 'shelf_specn_id', 'audittblpkcolval': selfshs.selectedShelfSpec().id.value, 'auditprnttblpkcolnm': '',
                                'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfshs.duplicateSelectedShelfSpecDpth()).toFixed(3), 'newcolval': Number(selfshs.selectedShelfSpec().Dpth.value).toFixed(3), 'cuid': user.cuid,
                                'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Depth from ' + Number(selfshs.duplicateSelectedShelfSpecDpth()).toFixed(3) + ' to ' + Number(selfshs.selectedShelfSpec().Dpth.value).toFixed(3) + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }

                        if (Number(selfshs.selectedShelfSpec().Hght.value).toFixed(3) != Number(selfshs.duplicateSelectedShelfSpecHght()).toFixed(3)) {
                            txtCondt += "Height changed to <b>" + Number(selfshs.selectedShelfSpec().Hght.value).toFixed(3) + '</b> from ' + Number(selfshs.duplicateSelectedShelfSpecHght()).toFixed(3) + "<br/>";

                            var saveJSON = {
                                'tablename': 'shelf_specn_gnrc', 'columnname': 'hgt_no', 'audittblpkcolnm': 'shelf_specn_id', 'audittblpkcolval': selfshs.selectedShelfSpec().id.value, 'auditprnttblpkcolnm': '',
                                'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfshs.duplicateSelectedShelfSpecHght()).toFixed(3), 'newcolval': Number(selfshs.selectedShelfSpec().Hght.value).toFixed(3), 'cuid': user.cuid,
                                'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Height from ' + Number(selfshs.duplicateSelectedShelfSpecHght()).toFixed(3) + ' to ' + Number(selfshs.selectedShelfSpec().Hght.value).toFixed(3) + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }

                        if (Number(selfshs.selectedShelfSpec().Wdth.value).toFixed(3) != Number(selfshs.duplicateSelectedShelfSpecWdth()).toFixed(3)) {
                            txtCondt += "Width changed to <b>" + Number(selfshs.selectedShelfSpec().Wdth.value).toFixed(3) + '</b> from ' + Number(selfshs.duplicateSelectedShelfSpecWdth()).toFixed(3) + "<br/>";

                            var saveJSON = {
                                'tablename': 'shelf_specn_gnrc', 'columnname': 'wdth_no', 'audittblpkcolnm': 'shelf_specn_id', 'audittblpkcolval': selfshs.selectedShelfSpec().id.value, 'auditprnttblpkcolnm': '',
                                'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfshs.duplicateSelectedShelfSpecWdth()).toFixed(3), 'newcolval': Number(selfshs.selectedShelfSpec().Wdth.value).toFixed(3), 'cuid': user.cuid,
                                'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Width from ' + Number(selfshs.duplicateSelectedShelfSpecWdth()).toFixed(3) + ' to ' + Number(selfshs.selectedShelfSpec().Wdth.value).toFixed(3) + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }

                        if (selfshs.duplicateSelectedShelfSpecStrghtThru() != selfshs.selectedShelfSpec().StrghtThru.value) {
                            txtCondt += "Straight Through changed to <b>" + selfshs.selectedShelfSpec().StrghtThru.value + '</b> from ' + selfshs.duplicateSelectedShelfSpecStrghtThru() + "<br/>";

                            var saveJSON = {
                                'tablename': 'shelf_specn', 'columnname': 'strght_thru_ind', 'audittblpkcolnm': 'shelf_specn_id', 'audittblpkcolval': selfshs.selectedShelfSpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                'oldcolval': selfshs.duplicateSelectedShelfSpecStrghtThru(),
                                'newcolval': selfshs.selectedShelfSpec().StrghtThru.value,
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Straight Thru from ' + selfshs.duplicateSelectedShelfSpecStrghtThru() + ' to ' + selfshs.selectedShelfSpec().StrghtThru.value + ' on ', 'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }

                        if (selfshs.duplicateSelectedShelfSpecMidPln() != selfshs.selectedShelfSpec().MidPln.value) {
                            txtCondt += "Mid Plane changed to <b>" + selfshs.selectedShelfSpec().MidPln.value + '</b> from ' + selfshs.duplicateSelectedShelfSpecMidPln() + "<br/>";

                            var saveJSON = {
                                'tablename': 'shelf_specn', 'columnname': 'mid_pln_ind', 'audittblpkcolnm': 'shelf_specn_id', 'audittblpkcolval': selfshs.selectedShelfSpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                'oldcolval': selfshs.duplicateSelectedShelfSpecMidPln(),
                                'newcolval': selfshs.selectedShelfSpec().MidPln.value,
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Mid Plane from ' + selfshs.duplicateSelectedShelfSpecMidPln() + ' to ' + selfshs.selectedShelfSpec().MidPln.value + ' on ', 'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }

                        if (selfshs.selectedShelfSpec().Dltd.bool != selfshs.duplicateSelectedShelfSpecDltd) {
                            txtCondt += "Unusable changed to <b>" + selfshs.selectedShelfSpec().Dltd.bool + '</b> from ' + selfshs.duplicateSelectedShelfSpecDltd + "<br/>";

                            var saveJSON = {
                                'tablename': 'shelf_specn_gnrc', 'columnname': 'del_ind', 'audittblpkcolnm': 'shelf_specn_id', 'audittblpkcolval': selfshs.selectedShelfSpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                'oldcolval': selfshs.duplicateSelectedShelfSpecDltd,
                                'newcolval': selfshs.selectedShelfSpec().Dltd.bool,
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Unusable from ' + selfshs.duplicateSelectedshelfSpecDltd + ' to ' + selfshs.selectedShelfSpec().Dltd.bool + ' on ', 'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }

                        if (selfshs.duplicateShelfNDSUseTyp() != selfshs.selectedShelfSpec().ShelfNDSUseTyp.value) {
                            txtCondt += "Use Type changed to <b>" + selfshs.selectedShelfSpec().ShelfNDSUseTyp.value + '</b> from ' + selfshs.duplicateShelfNDSUseTyp() + "<br/>";

                            var saveJSON = {
                                'tablename': 'shelf_specn', 'columnname': 'specn_record_use_typ_id', 'audittblpkcolnm': 'shelf_specn_id', 'audittblpkcolval': selfshs.selectedShelfSpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                'oldcolval': selfshs.duplicateShelfNDSUseTyp(),
                                'newcolval': selfshs.selectedShelfSpec().ShelfNDSUseTyp.value,
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfshs.specName() + ' Use Type from ' + selfshs.duplicateShelfNDSUseTyp() + ' to ' + selfshs.selectedShelfSpec().ShelfNDSUseTyp.value + ' on ', 'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                    }

                    var textlength = txtCondt.length;
                    if (selfshs.selectedShelfSpec().Shelf) {
                        var configNameList = selfshs.getConfigNames(selfshs.selectedShelfSpec().Shelf.list[0].id.value);
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

                    $("#interstitial").hide();

                    if (txtCondt.length > 0 && textlength > 0) {
                        app.showMessage(txtCondt, 'Update Confirmation for Shelf', ['Ok', 'Cancel']).then(function (dialogResult) {
                            if (dialogResult == 'Ok') {
                                selfshs.saveShelfSpec();
                            }
                        });
                    } else {
                        selfshs.saveShelfSpec();
                    }
                }
            } else {
                $("#interstitial").hide();
                return app.showMessage('Please associate a material item to the specification before saving.', 'Specification');
            }
        };

        ShelfSpecification.prototype.saveShelfSpec = function () {
            var saveJSON = mapping.toJS(selfshs.selectedShelfSpec());
            $("#interstitial").show();
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
                    selfshs.saveAuditChanges();
                    var specWorkToDoId = specResponseOnSuccess.SpecWTD;
                    var mtlWorkToDoId = specResponseOnSuccess.MtlWTD;

                    if (specWorkToDoId !== 0) {
                        var specHelper = new reference();

                        specHelper.getSpecificationSendToNdsStatus(specResponseOnSuccess.Id, specWorkToDoId, 'SHELF');
                    }

                    if (mtlWorkToDoId !== 0) {
                        var mtlHelper = new reference();

                        mtlHelper.getSendToNdsStatus(specResponseOnSuccess.MtlItmId, mtlWorkToDoId);
                    }

                    var backMtrlId = selfspec.backToMtlItmId;

                    if (selfspec.selectRadioSpec() == 'newSpec') {
                        selfspec.selectRadioSpec('existSpec');
                        $("#idProductID").val(specResponseOnSuccess.Id);
                        $('#idspectype').val("SHELF");
                        selfspec.Searchspec();
                        selfspec.specificationSelected(specResponseOnSuccess.Id, 'SHELF', selfspec, backMtrlId);

                        $("#interstitial").hide();

                        if (backMtrlId > 0) {
                            selfspec.backToMtlItmId = backMtrlId;
                            selfshs.enableBackButton(true);
                        }

                        return app.showMessage('Successfully updated specification<br> of type SHELF having Id <b>' + specResponseOnSuccess.Id + '</b>', 'Specification');
                    } else {
                        selfspec.selectRadioSpec('existSpec');
                        $("#idProductID").val(specResponseOnSuccess.Id);
                        $('#idspectype').val("SHELF");
                        if (selfshs.specification == true) {
                            selfspec.updateOnSuccess();
                        }
                        $("#interstitial").hide();

                        if (backMtrlId > 0) {
                            selfspec.backToMtlItmId = backMtrlId;
                            selfshs.enableBackButton(true);
                        }

                        return app.showMessage('Successfully Updated specification details', 'Specification');
                    }
                } else {
                    $("#interstitial").hide();
                    //app.showMessage("failure").then(function () {
                    //    return;
                    //});
                }
            }

            function updateError(err) {
                $("#interstitial").hide();

                if (err && err.responseJSON && err.responseJSON.ExceptionMessage && err.responseJSON.ExceptionMessage.indexOf('SHELF_SPECN_GNRC_AK01') > 0) {
                    return app.showMessage('A generic shelf already exists with the given dimensions. Height, width, depth and unit of measure must be unique in order to save.', 'Specification');
                } else
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }

        };

        ShelfSpecification.prototype.saveAuditChanges = function () {
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
                }
            }
            function saveAuditError() {
                // not sure we want to do anything here, so ask client
                //$("#interstitial").hide();
                //return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }
        };

        ShelfSpecification.prototype.GetModelNameCount = function (modelname, id) {
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

        ShelfSpecification.prototype.getConfigNames = function (materialItemID) {
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

        ShelfSpecification.prototype.shelfupdateChbk = function () {
            /* if ($("#GenericChbk").is(':checked'))
                 selfshs.selectedShelfSpec().Gnrc.bool = "Y";
             else
                 selfshs.selectedShelfSpec().Gnrc.bool = "N";*/

            if ($("#shelfROChbk").is(':checked'))
                selfshs.selectedShelfSpec().RO.value = "Y";
            else
                selfshs.selectedShelfSpec().RO.value = "N";

            if ($("#shelfStrghtThruChbk").is(':checked'))
                selfshs.selectedShelfSpec().StrghtThru.value = "Y";
            else
                selfshs.selectedShelfSpec().StrghtThru.value = "N";

            if ($("#shelfMidPlnChbk").is(':checked'))
                selfshs.selectedShelfSpec().MidPln.value = "Y";
            else
                selfshs.selectedShelfSpec().MidPln.value = "N";

            if ($("#shelfNmnlChbk").is(':checked'))
                selfshs.selectedShelfSpec().NdLvlMtrl.value = "Y";
            else
                selfshs.selectedShelfSpec().NdLvlMtrl.value = "N";
        };

        ShelfSpecification.prototype.onchangeSpcnRlTypLst = function (item) {
            var selectedId = item.id.value;
            var selectedUseType = "";

            if (item.Slctd.bool) {
                for (var i = 0; i < selfshs.shelfRoleTypeListTbl().length; i++) {
                    if (selectedId == selfshs.shelfRoleTypeListTbl()[i].id.value) {
                        selfshs.shelfRoleTypeListTbl()[i].Slctd.bool = true;
                        selfshs.shelfRoleTypeListTbl()[i].PrtyNo.value = item.PrtyNo.value;

                        break;
                    }
                }

                selectedUseType = item.UseTyp.value;

            } else {
                for (var i = 0; i < selfshs.shelfRoleTypeListTbl().length; i++) {
                    if (selectedId == selfshs.shelfRoleTypeListTbl()[i].id.value) {
                        selfshs.shelfRoleTypeListTbl()[i].Slctd.bool = false;
                        selfshs.shelfRoleTypeListTbl()[i].PrtyNo.value = 0;
                    } else {
                        if (selfshs.shelfRoleTypeListTbl()[i].Slctd.bool) {
                            selectedUseType = selfshs.shelfRoleTypeListTbl()[i].UseTyp.value;
                        }
                    }
                }

            }

            if (selectedUseType !== "") {
                for (var i = 0; i < selfshs.shelfRoleTypeListTbl().length; i++) {
                    if (selectedUseType == selfshs.shelfRoleTypeListTbl()[i].UseTyp.value) {
                        selfshs.shelfRoleTypeListTbl()[i].Slctd.enable = true;
                    } else {
                        selfshs.shelfRoleTypeListTbl()[i].Slctd.enable = false;
                        selfshs.shelfRoleTypeListTbl()[i].Slctd.bool = false;
                        selfshs.shelfRoleTypeListTbl()[i].PrtyNo.value = 0;
                    }
                }
            } else {
                for (var i = 0; i < selfshs.shelfRoleTypeListTbl().length; i++) {
                    selfshs.shelfRoleTypeListTbl()[i].Slctd.enable = true;
                }
            }

            var temp = selfshs.shelfRoleTypeListTbl();

            selfshs.shelfRoleTypeListTbl([]);
            selfshs.shelfRoleTypeListTbl(temp);

            return true;
            //var selectedId = item.id.value;

            //if (item.Slctd.bool === true) {
            //    for (var i = 0; i < selfshs.shelfRoleTypeListTbl().length; i++) {
            //        if (selectedId == selfshs.shelfRoleTypeListTbl()[i].id.value) {
            //            selfshs.shelfRoleTypeListTbl()[i].Slctd.bool=true;
            //            selfshs.shelfRoleTypeListTbl()[i].PrtyNo.value=item.PrtyNo.value;
            //        }
            //    }

            //    var temp = selfshs.shelfRoleTypeListTbl();

            //    selfshs.shelfRoleTypeListTbl([]);
            //    selfshs.shelfRoleTypeListTbl(temp);
            //} else {

            //    for (var i = 0; i < selfshs.shelfRoleTypeListTbl().length; i++) {
            //        if (selectedId == selfshs.shelfRoleTypeListTbl()[i].id.value) {
            //            selfshs.shelfRoleTypeListTbl()[i].Slctd.bool=false;
            //            selfshs.shelfRoleTypeListTbl()[i].PrtyNo.value=0;
            //        }
            //    }

            //    var temp = selfshs.shelfRoleTypeListTbl();

            //    selfshs.shelfRoleTypeListTbl([]);
            //    selfshs.shelfRoleTypeListTbl(temp);
            //}

            //return true;
        };

        ShelfSpecification.prototype.openShelfAssociatepart = function (model, event) {
            $("#slotAssociateModalpopup").show();
        };

        ShelfSpecification.prototype.closeShelfAssociatepart = function () {
            $("#slotAssociateModalpopup").hide();
        };
        ShelfSpecification.prototype.SaveassociateShelf = function () {
            var chkflag = false;
            var checkBoxes = $("#searchresultshelf .checkshelfpopsearch");
            var ids = $("#searchresultshelf .idshelfs");
            var specid = selfshs.selectedShelfSpec().RvsnId.value;
            var src = "SHELF";
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
                    context: selfshs,
                    async: false
                });
                //$.ajax({
                //    type: "POST",
                //    url: 'api/specn/associatemtl',
                //    data: saveJSON,
                //    contentType: "application/json; charset=utf-8",
                //    dataType: "json",
                //    success: successSearchAssociated,
                //    error: errorFunc,
                //    context: selfshs,
                //    async: false
                //});


            }

            //var checkBoxesassp = $("#associatedmtl .checkasstspopsearch");           
            //var mtlcodeassp = $("#idchkshelf").val();
            //var src1 = "SHELF";
            //var saveJSONdis = {
            //    material_rev_id: mtlcodeassp, source: src1
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
            //        context: selfshs,
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
                    var shelfList = JSON.parse(data);
                    var shelf = { "list": [shelfList] };
                    var name = shelf.list[0].Mfg.value + '-' + shelf.list[0].PrtNo.value;

                    selfshs.selectedShelfSpec().Shelf = shelf;
                    selfshs.selectedShelfSpec().RvsnNm.value = name;
                    selfshs.selectedShelfSpec().Nm.value = name;
                    selfshs.specName(name);

                    if (shelf != null && shelf.list.length > 0) {
                        selfshs.duplicateSelectedShelfSpecDpth(shelf.list[0].Dpth.value);
                        selfshs.duplicateSelectedShelfSpecHght(shelf.list[0].Hght.value);
                        selfshs.duplicateSelectedShelfSpecWdth(shelf.list[0].Wdth.value);
                    }

                    selfshs.selectedShelfSpec(selfshs.selectedShelfSpec());

                    $("#shelfAssociateModalpopup").hide();
                    $("#interstitial").hide();
                }
            }
        };

        ShelfSpecification.prototype.associatepartShelf = function (model, event) {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfshs.searchtblshelf(false);
            selfshs.associatemtlblck(false);
            document.getElementById('idcdmmsshelf').value = "";
            document.getElementById('materialcodeshelf').value = "";
            document.getElementById('partnumbershelf').value = "";
            document.getElementById('clmcshelf').value = "";
            document.getElementById('catlogdsptshelf').value = "";

            var modal = document.getElementById('shelfAssociateModalpopup');
            var btn = document.getElementById("idAsscociateshelf");
            var span = document.getElementsByClassName("close")[0];
            var rvsid = selfshs.selectedShelfSpec().RvsnId.value;
            var srcd = "SHELF";
            var ro = document.getElementById('shelfROChbk').checked ? "Y" : "N";
            var searchJSON = {
                RvsnId: rvsid, source: srcd, isRO: ro
            };
            $.ajax({
                type: "GET",
                url: 'api/specn/getassociatedmtl',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successSearchAssociateddisp,
                error: errorFunc,
                context: selfshs,
                async: false
            });

            function errorFunc() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }

            function successSearchAssociateddisp(data, status) {
                selfshs.enableModalSave(false);
                if (data === 'no_results') {
                    $("#interstitial").hide();
                    //$(".NoRecordrp").show();
                    //setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
                } else {

                    var results = JSON.parse(data);
                    selfshs.associatemtlblck(results);
                    $("#interstitial").hide();
                }

            }
            // When the user clicks the button, open the modal 
            btn.onclick = function () {
                modal.style.display = "block";
            }

            $("#interstitial").hide();
            $("#shelfAssociateModalpopup").show();

            // When the user clicks on <span> (x), close the modal
            span.onclick = function () {
                modal.style.display = "none";
            }
        };

        ShelfSpecification.prototype.Cancelassociateshelf = function (model, event) {
            selfshs.searchtblshelf(false);
            selfshs.associatemtlblck(false);
            document.getElementById('idcdmmsshelf').value = "";
            document.getElementById('materialcodeshelf').value = "";
            document.getElementById('partnumbershelf').value = "";
            document.getElementById('clmcshelf').value = "";
            document.getElementById('catlogdsptshelf').value = "";
            $("#shelfAssociateModalpopup").hide();

        };

        ShelfSpecification.prototype.searchmtlshelf = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfshs.searchtblshelf(false);

            var mtlid = $("#idcdmmsshelf").val();
            var mtlcode = $("#materialcodeshelf").val();
            var partnumb = $("#partnumbershelf").val();
            var clmc = $("#clmcshelf").val();
            var caldsp = $("#catlogdsptshelf").val();
            var ro = document.getElementById('shelfROChbk').checked ? "Y" : "N";
            var src = "SHELF";

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
                    context: selfshs,
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
                        selfshs.searchtblshelf(results);
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
                $('input.checkshelfpopsearch').on('change', function () {
                    $('input.checkshelfpopsearch').not(this).prop('checked', false);

                    $('input.checkshelfpopsearch').each(function () {
                        if ($(this).prop('checked') == true) {
                            selfshs.enableModalSave(true);

                            return false;
                        }

                        selfshs.enableModalSave(false);
                    });

                    $('input.checkasstspopsearch').each(function () {
                        $(this).prop('checked', false);
                    });
                });
            });

            $(document).ready(function () {
                $('input.checkasstspopsearch').on('change', function () {
                    $('input.checkshelfpopsearch').each(function () {
                        $(this).prop('checked', false);
                    });

                    $('input.checkasstspopsearch').each(function () {
                        if ($(this).prop('checked') == true) {
                            selfshs.enableModalSave(false);

                            return false;
                        }

                        selfshs.enableModalSave(false);
                    });
                });
            });
        };

        ShelfSpecification.prototype.exportShelfSpecReport = function () {
            //alert("Hi");

            var mainObj;
            //Excel report generation
            var excel = $JExcel.new("Calibri light 10 #333333");			// Default font

            excel.set({ sheet: 0, value: "Searched Specification List" });   //Add sheet 1 for Specification search details.
            excel.addSheet("Specification Details");                 //Add sheet 2 for selected specification id details 

            //Start writing Searched Material data into sheet 1
            var speclheaders = ["Specification ID", "Name", "Description", "Completed", "Propagated", "Unusable in NDS", "Specification Type", "Specification Class"
            ];							// This array holds the HEADERS text

            var formatHeader = excel.addStyle({ 											// Format for headers
                border: "none,none,none,thin #333333", 										// 		Border for header
                font: "Calibri 12 #1b834c B"
            });

            for (var i = 0; i < speclheaders.length; i++) {									// Loop all the haders
                excel.set(0, i, 0, speclheaders[i], formatHeader);							// Set CELL with header text, using header format
                excel.set(0, i, undefined, "auto");											// Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
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

            //Start writing Selected bay specification data into sheet 2
            var shelfSpecHeaders = ["Generic", "Record Only", "Completed", "Propagated", "Unusable in NDS", "Specification ID", "Name", "Description"
                           , "Slots Per Row Quantity", "Starting Slot No", "Use Type", "Orientation", "Label Name", "Label Position"
                           , "Height", "Width", "Depth", "Dimensions UOM", "Model", "CLEI", "HECI"
                           , "Straight Through", "Mid Plane", "Node Level Material", "Shelf Role Type List"
            ];							// This array holds the HEADERS text


            for (var i = 0; i < shelfSpecHeaders.length; i++) {											//  Loop all the haders
                excel.set(1, i, 0, shelfSpecHeaders[i], formatHeader);									//  Set CELL with header text, using header format
                excel.set(1, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }
            //Writing data into sheet 2 for selected specification
            var i = 0;

            var Gnrc = typeof (selfshs.selectedShelfSpec().Gnrc) === "undefined" ? "" : selfshs.selectedShelfSpec().Gnrc.bool;
            var RO = typeof (selfshs.selectedShelfSpec().RO) === "undefined" ? "" : selfshs.selectedShelfSpec().RO.bool;
            var Cmplt = typeof (selfshs.selectedShelfSpec().Cmplt) === "undefined" ? "" : selfshs.selectedShelfSpec().Cmplt.bool;
            var Prpgtd = typeof (selfshs.selectedShelfSpec().Prpgtd) === "undefined" ? "" : selfshs.selectedShelfSpec().Prpgtd.bool;
            var Dltd = typeof (selfshs.selectedShelfSpec().Dltd) === "undefined" ? "" : selfshs.selectedShelfSpec().Dltd.bool;

            excel.set(1, 0, i + 1, Gnrc);                                                                   // Generic
            excel.set(1, 1, i + 1, RO);                                                                     // Record Only
            excel.set(1, 2, i + 1, Cmplt);                                                                  // Completed
            excel.set(1, 3, i + 1, Prpgtd);                                                                 // Propagated
            excel.set(1, 4, i + 1, Dltd);                                                                   // Unusable in NDS
            excel.set(1, 5, i + 1, selfshs.selectedShelfSpec().id.value);                                   // Specification ID
            excel.set(1, 6, i + 1, selfshs.specName());                                                     // Name
            excel.set(1, 7, i + 1, selfshs.selectedShelfSpec().Desc.value);                                 // Description

            excel.set(1, 8, i + 1, selfshs.selectedShelfSpec().SltsRwQty.value);                            // Slots Per Row Quantity 
            excel.set(1, 9, i + 1, selfshs.selectedShelfSpec().StrtSltNo.value);                            // Starting Slot No     
            excel.set(1, 10, i + 1, selfshs.selectedShelfSpec().ShelfNDSUseTyp.value);                      // Use Type

            var indInt = selfshs.selectedShelfSpec().OrnttnId.options.map(function (img) { return img.value; }).indexOf(selfshs.selectedShelfSpec().OrnttnId.value);
            excel.set(1, 11, i + 1, selfshs.selectedShelfSpec().OrnttnId.options[indInt].text);              // Orientation

            excel.set(1, 12, i + 1, selfshs.selectedShelfSpec().LblNm.value);                                // Label Name 

            var indInt = selfshs.selectedShelfSpec().LblPosId.options.map(function (img) { return img.value; }).indexOf(selfshs.selectedShelfSpec().LblPosId.value);
            excel.set(1, 13, i + 1, selfshs.selectedShelfSpec().LblPosId.options[indInt].text);              // Label Position

            excel.set(1, 14, i + 1, selfshs.selectedShelfSpec().Hght.value);                                 // Height 
            excel.set(1, 15, i + 1, selfshs.selectedShelfSpec().Wdth.value);                                 // Width 
            excel.set(1, 16, i + 1, selfshs.selectedShelfSpec().Dpth.value);                                 // Depth 

            var indInt = selfshs.selectedShelfSpec().DimUom.options.map(function (img) { return img.value; }).indexOf(selfshs.selectedShelfSpec().DimUom.value);
            excel.set(1, 17, i + 1, selfshs.selectedShelfSpec().DimUom.options[indInt].text);                // Dimensions UOM

            excel.set(1, 18, i + 1, selfshs.selectedShelfSpec().Nm.value);                                 // Model 
            excel.set(1, 19, i + 1, $("#modelTxt").text());                                                 // CLEI 
            excel.set(1, 20, i + 1, $("#modelTxt").text());                                                 // HECI 

            excel.set(1, 21, i + 1, selfshs.selectedShelfSpec().StrghtThru.value);                           // Straight Through 
            excel.set(1, 22, i + 1, selfshs.selectedShelfSpec().MidPln.value);                               // Mid Plane 
            excel.set(1, 23, i + 1, selfshs.selectedShelfSpec().NdLvlMtrl.value);                            // Node Level Material
            var cnt = 1;
            for (var i = 0; i < selfshs.shelfRoleTypeListTbl().length; i++) {
                if (selfshs.shelfRoleTypeListTbl()[i].Slctd.bool) {
                    excel.set(1, 24, cnt, selfshs.shelfRoleTypeListTbl()[i].SpcnRlTyp.value);               // Shelf Role Type List        
                    cnt = cnt + 1;
                }
            }

            var i = 0;
            if (selfshs.enableAssociate() == true) {
                //fetch the associated material details
                selfshs.associatepartShelf("", "");
                $("#shelfAssociateModalpopup").hide();
                //Start writing associate details into sheet 3
                excel.addSheet("Search result of Associated Material");                 //Add sheet 3 for selected specification id details 
                var assoSearchcHeaders = ["CDMMS ID", "Material Code", "CLMC", "Part Number", "Catalog/Material Description"];		// This array holds the HEADERS text


                for (var i = 0; i < assoSearchcHeaders.length; i++) {								    //  Loop all the haders               
                    excel.set(2, i, 0, assoSearchcHeaders[i], formatHeader);							//  Set CELL with header text, using header format
                    excel.set(2, i, undefined, "auto");													//  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
                }

                var i = 0;

                if (selfshs.searchtblshelf() != false) {
                    if (selfshs.searchtblshelf().length > 0) {
                        excel.set(2, 0, i + 1, selfshs.searchtblshelf()[i].material_item_id.value);        // CDMMS ID
                        excel.set(2, 1, i + 1, selfshs.searchtblshelf()[i].product_id.value);              // Material Code
                        excel.set(2, 2, i + 1, selfshs.searchtblshelf()[i].mfg_id.value);                  // CLMC
                        excel.set(2, 3, i + 1, selfshs.searchtblshelf()[i].mfg_part_no.value);             // Part Number
                        excel.set(2, 4, i + 1, selfshs.searchtblshelf()[i].item_desc.value);               // Catalog/Material Description
                    }
                }
                //End of writing associate details into sheet 3


                //Start writing associate details into sheet 4
                excel.addSheet("Associated Material");
                var assoSearchcHeaders = ["CDMMS ID", "Material Code", "CLMC", "Part Number", "Catalog/Material Description"];							// This array holds the HEADERS text


                for (var i = 0; i < assoSearchcHeaders.length; i++) {										//  Loop all the haders               
                    excel.set(3, i, 0, assoSearchcHeaders[i], formatHeader);								//  Set CELL with header text, using header format
                    excel.set(3, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
                }

                var i = 0;

                if (selfshs.associatemtlblck() != false) {
                    if (selfshs.associatemtlblck().length > 0) {
                        excel.set(3, 0, i + 1, selfshs.associatemtlblck()[i].material_item_id.value);        // CDMMS ID
                        excel.set(3, 1, i + 1, selfshs.associatemtlblck()[i].product_id.value);              // Material Code
                        excel.set(3, 2, i + 1, selfshs.associatemtlblck()[i].mfg_id.value);                  // CLMC
                        excel.set(3, 3, i + 1, selfshs.associatemtlblck()[i].mfg_part_no.value);             // Part Number
                        excel.set(3, 4, i + 1, selfshs.associatemtlblck()[i].item_desc.value);               // Catalog/Material Description
                    }
                }
                //End of writing associate details into sheet 4
            }
            excel.generate("Report_Shelf_Specification.xlsx");

            ////End of writing shelf specification data into sheet 2
        };
        return ShelfSpecification;
    });