define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', '../Utility/user', '../Utility/referenceDataHelper'],
    function (composition, app, ko, $, http, activator, mapping, system, user, reference) {
        var slotSpecification = function (data) {
            slotSpec = this;
            specChangeArray = [];
            var dataResult = data.resp;
            var results = JSON.parse(dataResult);
            slotSpec.specification = data.specification;

            slotSpec.selectedSlotSpec = ko.observable();
            results.Dpth.value = Number(results.Dpth.value).toFixed(3);
            results.Hght.value = Number(results.Hght.value).toFixed(3);
            results.Wdth.value = Number(results.Wdth.value).toFixed(3);
            // Ensure there is a leading zero when a fraction like .75 comes across.  should display as 0.75
            if (results.Dpth.value < 1 && results.Dpth.value > 0 && results.Dpth.value.substring(0, 1) == '.') {
                results.Dpth.value = '0' + results.Dpth.value;
            }
            if (results.Hght.value < 1 && results.Hght.value > 0 && results.Hght.value.substring(0, 1) == '.') {
                results.Hght.value = '0' + results.Hght.value;
            }
            if (results.Wdth.value < 1 && results.Wdth.value > 0 && results.Wdth.value.substring(0, 1) == '.') {
                results.Wdth.value = '0' + results.Wdth.value;
            }

            slotSpec.selectedSlotSpec(results);
            slotSpec.slotRoleTypeListTbl = ko.observableArray();
            slotSpec.slotRoleTypeListTbl(slotSpec.selectedSlotSpec().SpcnRlTypLst.list);
            slotSpec.completedNotSelected = ko.observable(true);
            slotSpec.duplicateSelectedCardSpecDpth = ko.observable(results.Dpth.value);
            slotSpec.duplicateSelectedCardSpecHght = ko.observable(results.Hght.value);
            slotSpec.duplicateSelectedCardSpecWdth = ko.observable(results.Wdth.value);
            slotSpec.duplicateSelectedCardSpecStrghtThru = ko.observable(results.StrghtThru.value);
            slotSpec.duplicateSelectedSlotSpecDltd = ko.observable();
            slotSpec.duplicateSelectedSlotSpecDltd = results.Dltd.bool;
            slotSpec.duplicateSelectedSlotSpecName = ko.observable();
            slotSpec.duplicateSelectedSlotSpecName.value = results.RvsnNm.value;

            setTimeout(function () {
                $(document).ready(function () {
                    $('input.SltCnsmptnIdCbk').on('change', function () {
                        $('input.SltCnsmptnIdCbk').not(this).prop('checked', false);
                        slotSpec.selectedSlotSpec().SltCnsmptnId.value = $(this).val();
                        $("#slotCmptnValidTblDgr").hide();
                    });
                });
            }, 2000);
            $(document).on('change', '[type=checkbox]', function () {
                if (this.name == "cmplIndnmslot") {
                    if (this.checked == false) {
                        document.getElementById('prpgIndslot').checked = false;
                    }
                }
            });
            if (slotSpec.selectedSlotSpec().Cmplt.bool == true && slotSpec.selectedSlotSpec().Prpgtd.enable == true) {
                slotSpec.completedNotSelected(false);
            }

            if (selfspec.selectRadioSpec() == 'newSpec') {
                slotSpec.selectedSlotSpec().Wdth.value = '';
                slotSpec.selectedSlotSpec().Dpth.value = '';
                slotSpec.selectedSlotSpec().Hght.value = '';
            }
            setTimeout(function () {
                if (document.getElementById('iddimuom') !== null) {
                    if (document.getElementById('iddimuom').value == '') {
                        document.getElementById('iddimuom').value = '22';
                    }
                }
            }, 5000);
        };

        slotSpecification.prototype.onchangeCompleted = function () {
            if ($("#completedChkBox").is(':checked')) {
                slotSpec.completedNotSelected(false);
            } else {
                slotSpec.completedNotSelected(true);
                slotSpec.selectedSlotSpec().Prpgtd.bool = false;
            }
        };

        slotSpecification.prototype.NumDecimal = function (mp, event) {
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

        slotSpecification.prototype.updateSlotSpec = function () {
            $("#interstitial").show();
            specChangeArray = [];
            slotSpec.selectedSlotSpec().SpcnRlTypLst.list = slotSpec.slotRoleTypeListTbl();

            var arr = [];
            var priorityNull = '';
            for (var i = 0; i < slotSpec.slotRoleTypeListTbl().length; i++) {
                if (slotSpec.slotRoleTypeListTbl()[i].PrtyNo.value != 0) {
                    arr.push(slotSpec.slotRoleTypeListTbl()[i].PrtyNo.value)
                } else {
                    if (slotSpec.slotRoleTypeListTbl()[i].Slctd.bool) {
                        priorityNull += "Cannot have priority <b>'0'</b> for a selected Role <b>" + slotSpec.slotRoleTypeListTbl()[i].SpcnRlTyp.value + "</b><br/>";
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
                for (var i = 0; i < slotSpec.slotRoleTypeListTbl().length; i++) {
                    if (slotSpec.slotRoleTypeListTbl()[i].PrtyNo.value == duplicatePriority[j]) {
                        errorMessage += "<b>" + slotSpec.slotRoleTypeListTbl()[i].SpcnRlTyp.value + "</b>,";
                    }
                }
                errorMessage = errorMessage.substring(0, errorMessage.length - 1);
                errorMessage += " having same priority <b>" + duplicatePriority[j] + "</b><br/><br/>";
            }

            var slotRoleTypeTbl = false;
            var slotLstCbk = $("#SltCnsmptnIdCbkTbl .SltCnsmptnIdCbk");
            for (var i = 0; i < slotLstCbk.length; i++) {
                if (slotLstCbk[i].checked == true) {
                    slotRoleTypeTbl = true;

                }
            }
            if (duplicatePriority.length > 0 || priorityNull.length > 0) {
                $("#interstitial").hide();
                errorMessage += priorityNull;
                $("#slotRoleTypeValidTblDgr").html(errorMessage);
                $("#slotRoleTypeValidTblDgr").show();
            } else if (!slotRoleTypeTbl) {
                $("#interstitial").hide();
                $("#slotCmptnValidTblDgr").show();
            } else {
                slotSpec.slotupdateChbkCheck();

                var txtCondt = '';
                if (selfspec.selectRadioSpec() == 'existSpec') {
                    if (slotSpec.selectedSlotSpec().RvsnNm.value != slotSpec.duplicateSelectedSlotSpecName.value) {
                        txtCondt += "Name changed to <b>" + slotSpec.selectedSlotSpec().RvsnNm.value + '</b> from ' + slotSpec.duplicateSelectedSlotSpecName.value + "<br/>";

                        var saveJSON = {
                            'tablename': 'slot_specn', 'columnname': 'slot_specn_revsn_nm', 'audittblpkcolnm': 'slot_specn_id', 'audittblpkcolval': slotSpec.selectedSlotSpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                            'oldcolval': slotSpec.duplicateSelectedSlotSpecName.value,
                            'newcolval': slotSpec.selectedSlotSpec().RvsnNm.value,
                            'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + slotSpec.selectedSlotSpec().RvsnNm.value + ' Spec Name from ' + slotSpec.duplicateSelectedSlotSpecName.value + ' on ', 'materialitemid': 0
                        };
                        specChangeArray.push(saveJSON);
                    }

                    if (Number(slotSpec.selectedSlotSpec().Dpth.value).toFixed(3) != Number(slotSpec.duplicateSelectedCardSpecDpth()).toFixed(3)) {
                        txtCondt += "Depth changed to <b>" + Number(slotSpec.selectedSlotSpec().Dpth.value).toFixed(3) + '</b> from ' + Number(slotSpec.duplicateSelectedCardSpecDpth()).toFixed(3) + "<br/>";

                        var saveJSON = {
                            'tablename': 'slot_specn', 'columnname': 'dpth_no', 'audittblpkcolnm': 'slot_specn_id', 'audittblpkcolval': slotSpec.selectedSlotSpec().id.value, 'auditprnttblpkcolnm': '',
                            'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(slotSpec.duplicateSelectedCardSpecDpth()).toFixed(3), 'newcolval': Number(slotSpec.selectedSlotSpec().Dpth.value).toFixed(3), 'cuid': user.cuid,
                            'cmnttxt': user.cuid + ' changed ' + slotSpec.selectedSlotSpec().RvsnNm.value + ' Depth from ' + Number(slotSpec.duplicateSelectedCardSpecDpth()).toFixed(3) + ' to ' + Number(slotSpec.selectedSlotSpec().Dpth.value).toFixed(3) + ' on ',
                            'materialitemid': 0
                        };
                        specChangeArray.push(saveJSON);
                    }

                    if (Number(slotSpec.selectedSlotSpec().Hght.value).toFixed(3) != Number(slotSpec.duplicateSelectedCardSpecHght()).toFixed(3)) {
                        txtCondt += "Height changed to <b>" + Number(slotSpec.selectedSlotSpec().Hght.value).toFixed(3) + '</b> from ' + Number(slotSpec.duplicateSelectedCardSpecHght()).toFixed(3) + "<br/>";

                        var saveJSON = {
                            'tablename': 'slot_specn', 'columnname': 'hgt_no', 'audittblpkcolnm': 'slot_specn_id', 'audittblpkcolval': slotSpec.selectedSlotSpec().id.value, 'auditprnttblpkcolnm': '',
                            'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(slotSpec.duplicateSelectedCardSpecHght()).toFixed(3), 'newcolval': Number(slotSpec.selectedSlotSpec().Hght.value).toFixed(3), 'cuid': user.cuid,
                            'cmnttxt': user.cuid + ' changed ' + slotSpec.selectedSlotSpec().RvsnNm.value + ' Height from ' + Number(slotSpec.duplicateSelectedCardSpecHght()).toFixed(3) + ' to ' + Number(slotSpec.selectedSlotSpec().Hght.value).toFixed(3) + ' on ',
                            'materialitemid': 0
                        };
                        specChangeArray.push(saveJSON);
                    }

                    if (Number(slotSpec.selectedSlotSpec().Wdth.value).toFixed(3) != Number(slotSpec.duplicateSelectedCardSpecWdth()).toFixed(3)) {
                        txtCondt += "Width changed to <b>" + Number(slotSpec.selectedSlotSpec().Wdth.value).toFixed(3) + '</b> from ' + Number(slotSpec.duplicateSelectedCardSpecWdth()).toFixed(3) + "<br/>";

                        var saveJSON = {
                            'tablename': 'slot_specn', 'columnname': 'wdth_no', 'audittblpkcolnm': 'slot_specn_id', 'audittblpkcolval': slotSpec.selectedSlotSpec().id.value, 'auditprnttblpkcolnm': '',
                            'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(slotSpec.duplicateSelectedCardSpecWdth()).toFixed(3), 'newcolval': Number(slotSpec.selectedSlotSpec().Wdth.value).toFixed(3), 'cuid': user.cuid,
                            'cmnttxt': user.cuid + ' changed ' + slotSpec.selectedSlotSpec().RvsnNm.value + ' Width from ' + Number(slotSpec.duplicateSelectedCardSpecWdth()).toFixed(3) + ' to ' + Number(slotSpec.selectedSlotSpec().Wdth.value).toFixed(3) + ' on ',
                            'materialitemid': 0
                        };
                        specChangeArray.push(saveJSON);
                    }

                    if (slotSpec.duplicateSelectedCardSpecStrghtThru() != slotSpec.selectedSlotSpec().StrghtThru.value) {
                        txtCondt += "Straight Through changed to <b>" + slotSpec.selectedSlotSpec().StrghtThru.value + '</b> from ' + slotSpec.duplicateSelectedCardSpecStrghtThru() + "<br/>";

                        var saveJSON = {
                            'tablename': 'slot_specn', 'columnname': 'strght_thru_ind', 'audittblpkcolnm': 'slot_specn_id', 'audittblpkcolval': slotSpec.selectedSlotSpec().id.value, 'auditprnttblpkcolnm': '',
                            'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': slotSpec.duplicateSelectedCardSpecStrghtThru(), 'newcolval': slotSpec.selectedSlotSpec().StrghtThru.value, 'cuid': user.cuid,
                            'cmnttxt': user.cuid + ' changed ' + slotSpec.selectedSlotSpec().RvsnNm.value + ' Straight Thru from ' + slotSpec.duplicateSelectedCardSpecStrghtThru() + ' to ' + slotSpec.selectedSlotSpec().StrghtThru.value + ' on ',
                            'materialitemid': 0
                        };
                        specChangeArray.push(saveJSON);
                    }

                    if (slotSpec.selectedSlotSpec().Dltd.bool != slotSpec.duplicateSelectedSlotSpecDltd) {
                        txtCondt += "Unusable changed to <b>" + slotSpec.selectedSlotSpec().Dltd.bool + '</b> from ' + slotSpec.duplicateSelectedSlotSpecDltd + "<br/>";

                        var saveJSON = {
                            'tablename': 'slot_specn', 'columnname': 'del_ind', 'audittblpkcolnm': 'slot_specn_id', 'audittblpkcolval': slotSpec.selectedSlotSpec().id.value, 'auditprnttblpkcolnm': '',
                            'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': slotSpec.duplicateSelectedSlotSpecDltd, 'newcolval': slotSpec.selectedSlotSpec().Dltd.bool, 'cuid': user.cuid,
                            'cmnttxt': user.cuid + ' changed ' + slotSpec.selectedSlotSpec().RvsnNm.value + ' Unusable from ' + slotSpec.duplicateSelectedSlotSpecDltd + ' to ' + slotSpec.selectedSlotSpec().Dltd.bool + ' on ',
                            'materialitemid': 0
                        };
                        specChangeArray.push(saveJSON);
                    }
                }
                $("#interstitial").hide();
                if (txtCondt.length > 0) {
                    app.showMessage(txtCondt, 'Update Confirmation for Slot', ['Yes', 'No']).then(function (dialogResult) {
                        if (dialogResult == 'Yes') {
                            slotSpec.saveSlotSpec();
                        }
                    });
                } else {
                    slotSpec.saveSlotSpec();
                }
            }
        };

        slotSpecification.prototype.saveSlotSpec = function () {
            $("#interstitial").show();
            var saveJSON = mapping.toJS(slotSpec.selectedSlotSpec());
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
                    slotSpec.saveAuditChanges();
                    //******** Send to NDS *************//
                    var specWorkToDoId = specResponseOnSuccess.SpecWTD;
                    var mtlWorkToDoId = specResponseOnSuccess.MtlWTD;

                    if (specWorkToDoId !== 0) {
                        var specHelper = new reference();

                        specHelper.getSpecificationSendToNdsStatus(specResponseOnSuccess.Id, specWorkToDoId, 'SLOT');
                    }

                    if (mtlWorkToDoId !== 0) {
                        var mtlHelper = new reference();

                        mtlHelper.getSendToNdsStatus(specResponseOnSuccess.MtlItmId, mtlWorkToDoId);
                    }

                    //**********************************//

                    if (selfspec.selectRadioSpec() == 'newSpec') {
                        selfspec.selectRadioSpec('existSpec');
                        $("#idProductID").val(specResponseOnSuccess.Id);
                        $('#idspectype').val("SLOT");
                        selfspec.Searchspec();
                        $("#interstitial").hide();
                        return app.showMessage('Successfully updated specification <br> of type SLOT having <b>Id ' + specResponseOnSuccess.Id + '</b>', 'Specification');
                    } else {
                        if (slotSpec.specification == true) {
                            selfspec.updateOnSuccess();
                        }
                        $("#interstitial").hide();
                        return app.showMessage('Successfully Updated specification details', 'Specification');
                    }

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
        };

        slotSpecification.prototype.saveAuditChanges = function () {
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

        slotSpecification.prototype.slotupdateChbkCheck = function () {

            if ($("#SlotStrghtThruChbk").is(':checked'))
                slotSpec.selectedSlotSpec().StrghtThru.value = "Y";
            else
                slotSpec.selectedSlotSpec().StrghtThru.value = "N";

            if ($("#SbSltChbk").is(':checked'))
                slotSpec.selectedSlotSpec().SbSlt.value = "Y";
            else
                slotSpec.selectedSlotSpec().SbSlt.value = "N";


        };
        slotSpecification.prototype.slotOnchangeSpcnRlTypLst = function (item) {
            var selectedId = item.id.value;
            if (item.Slctd.bool === true) {
                for (var i = 0; i < slotSpec.slotRoleTypeListTbl().length; i++) {
                    if (selectedId == slotSpec.slotRoleTypeListTbl()[i].id.value) {
                        slotSpec.slotRoleTypeListTbl()[i].Slctd.bool = true;
                        slotSpec.slotRoleTypeListTbl()[i].PrtyNo.value = item.PrtyNo.value;
                    }
                }
                var temp = slotSpec.slotRoleTypeListTbl();
                slotSpec.slotRoleTypeListTbl([]);
                slotSpec.slotRoleTypeListTbl(temp);
            } else {

                for (var i = 0; i < slotSpec.slotRoleTypeListTbl().length; i++) {
                    if (selectedId == slotSpec.slotRoleTypeListTbl()[i].id.value) {
                        slotSpec.slotRoleTypeListTbl()[i].Slctd.bool = false;
                        slotSpec.slotRoleTypeListTbl()[i].PrtyNo.value = 0;
                    }
                }
                var temp = slotSpec.slotRoleTypeListTbl();
                slotSpec.slotRoleTypeListTbl([]);
                slotSpec.slotRoleTypeListTbl(temp);
            }
            return true;
        };

        slotSpecification.prototype.exportSlotSpecReport = function () {
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
            var slotSpecHeaders = ["Completed", "Propagated", "Unusable in NDS", "Specification ID", "Name", "Description"
                           , "Use Type", "Height", "Width", "Depth", "Unit of Measure", "Model"
                           , "Straight Through", "Sub Slot"
            ];							// This array holds the HEADERS text


            for (var i = 0; i < slotSpecHeaders.length; i++) {											//  Loop all the haders
                excel.set(1, i, 0, slotSpecHeaders[i], formatHeader);									//  Set CELL with header text, using header format
                excel.set(1, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }
            //Writing data into sheet 2 for selected specification
            var i = 0;

            var Cmplt = typeof (slotSpec.selectedSlotSpec().Cmplt) === "undefined" ? "" : slotSpec.selectedSlotSpec().Cmplt.bool;
            var Prpgtd = typeof (slotSpec.selectedSlotSpec().Prpgtd) === "undefined" ? "" : slotSpec.selectedSlotSpec().Prpgtd.bool;
            var Dltd = typeof (slotSpec.selectedSlotSpec().Dltd) === "undefined" ? "" : slotSpec.selectedSlotSpec().Dltd.bool;

            excel.set(1, 0, i + 1, Cmplt);                                                                  // Completed
            excel.set(1, 1, i + 1, Prpgtd);                                                                 // Propagated
            excel.set(1, 2, i + 1, Dltd);                                                                   // Unusable in NDS
            excel.set(1, 3, i + 1, slotSpec.selectedSlotSpec().id.value);                                   // Specification ID
            excel.set(1, 4, i + 1, slotSpec.selectedSlotSpec().RvsnNm.value);                                                     // Name
            excel.set(1, 5, i + 1, slotSpec.selectedSlotSpec().Desc.value);                                 // Description

            excel.set(1, 6, i + 1, slotSpec.selectedSlotSpec().SlotUseTypId.value);                         // Use Type
            excel.set(1, 7, i + 1, slotSpec.selectedSlotSpec().Hght.value);                                 // Height 
            excel.set(1, 8, i + 1, slotSpec.selectedSlotSpec().Wdth.value);                                 // Width 
            excel.set(1, 9, i + 1, slotSpec.selectedSlotSpec().Dpth.value);                                 // Depth 

            var indInt = slotSpec.selectedSlotSpec().DimUom.options.map(function (img) { return img.value; }).indexOf(slotSpec.selectedSlotSpec().DimUom.value);
            excel.set(1, 10, i + 1, slotSpec.selectedSlotSpec().DimUom.options[indInt].text);                   // Dimensions UOM
            excel.set(1, 11, i + 1, slotSpec.selectedSlotSpec().Nm.value);                                      // Model       
            excel.set(1, 12, i + 1, slotSpec.selectedSlotSpec().StrghtThru.value);                              // Straight Through 
            excel.set(1, 13, i + 1, slotSpec.selectedSlotSpec().SbSlt.value);                                   // Sub Slot 
            //End of writing data in sheet 2
            //var cnt = 1;
            //for (var i = 0; i < slotSpec.selectedSlotSpec().SltCnsmptnLst.list.length; i++) {
            //    if (slotSpec.selectedSlotSpec().SltCnsmptnLst.list[i].Slctd.bool) {
            //        excel.set(1, 24, cnt, slotSpec.selectedSlotSpec().SltCnsmptnLst.list[i].SpcnRlTyp.value);               // Shelf Role Type List        
            //        cnt = cnt + 1;
            //    }
            //}

            //Start writing slot consumption list into sheet 3
            excel.addSheet("Slot Consumption list");                 //Add sheet 2 for selected specification id details 
            var slotConsmcHeaders = ["Select", "Quantity", "Type", "Priority Number"];						// This array holds the HEADERS text


            for (var i = 0; i < slotConsmcHeaders.length; i++) {											//  Loop all the haders               
                excel.set(2, i, 0, slotConsmcHeaders[i], formatHeader);									//  Set CELL with header text, using header format
                excel.set(2, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }

            var i = 0;


            if (slotSpec.selectedSlotSpec().SltCnsmptnLst.list.length > 0) {
                for (var i = 0; i < slotSpec.selectedSlotSpec().SltCnsmptnLst.list.length; i++) {
                    if (slotSpec.selectedSlotSpec().SltCnsmptnLst.list[i].id.value == slotSpec.selectedSlotSpec().SltCnsmptnId.value) {
                        excel.set(2, 0, i + 1, "Checked");        // Select
                        excel.set(2, 1, i + 1, slotSpec.selectedSlotSpec().SltCnsmptnLst.list[i].SltCnsmptnQty.value);              // Quantity                
                        excel.set(2, 2, i + 1, slotSpec.selectedSlotSpec().SltCnsmptnLst.list[i].SltCnsmptnTyp.value);             // Type
                        excel.set(2, 3, i + 1, slotSpec.selectedSlotSpec().SltCnsmptnLst.list[i].PrtyNo.value);               // priority number
                    }
                }
            }

            //End of writing slot consumption list  into sheet 3

            //Start writing Specification Role Type details into sheet 4
            excel.addSheet("Specification Role Type List");
            var specroleTypeHeaders = ["Select", "Type", "Priority Number"];							    // This array holds the HEADERS text


            for (var i = 0; i < specroleTypeHeaders.length; i++) {									    //  Loop all the haders               
                excel.set(3, i, 0, specroleTypeHeaders[i], formatHeader);								//  Set CELL with header text, using header format
                excel.set(3, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }

            var i = 0;


            if (slotSpec.slotRoleTypeListTbl().length > 0) {
                for (var i = 0; i < slotSpec.slotRoleTypeListTbl().length; i++) {
                    if (slotSpec.slotRoleTypeListTbl()[i].Slctd.bool == true) {
                        excel.set(3, 0, i + 1, "Checked");        // Select
                        excel.set(3, 1, i + 1, slotSpec.slotRoleTypeListTbl()[i].SpcnRlTyp.value);              // Type
                        excel.set(3, 2, i + 1, slotSpec.slotRoleTypeListTbl()[i].PrtyNo.value);                  // Priority Number
                    }
                }
            }

            //End of writing Specification Role Type details into sheet 4

            excel.generate("Report_Slot_Specification.xlsx");
        };

        return slotSpecification;
    });