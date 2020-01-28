define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/app', 'durandal/system', 'jqueryui', '../Utility/referenceDataHelper', 'bootstrapJS', 'datablescelledit', 'plugins/router', '../Utility/user'],
    function (composition, ko, $, http, activator, mapping, app, system, jqueryui, reference, bootstrapJS, datablescelledit, router, user) {
        var BaySpecification = function (data) {
            selfbay = this;
            specChangeArray = [];
            selfbay.Bayinterlsdetails = ko.observableArray();
            var results = JSON.parse(data);
            selfbay.selectedBaySpec = ko.observable();
            if (results.Bay != null && results.Bay.list.length > 0) {
                results.Bay.list[0].Dpth.value = Number(results.Bay.list[0].Dpth.value).toFixed(3);
                results.Bay.list[0].Hght.value = Number(results.Bay.list[0].Hght.value).toFixed(3);
                results.Bay.list[0].Wdth.value = Number(results.Bay.list[0].Wdth.value).toFixed(3);
                // Ensure there is a leading zero when a fraction like .75 comes across.  should display as 0.75
                if (results.Bay.list[0].Wdth.value < 1 && results.Bay.list[0].Wdth.value > 0 && results.Bay.list[0].Wdth.value.substring(0, 1) == '.') {
                    results.Bay.list[0].Wdth.value = '0' + results.Bay.list[0].Wdth.value;
                }
                if (results.Bay.list[0].Hght.value < 1 && results.Bay.list[0].Hght.value > 0 && results.Bay.list[0].Hght.value.substring(0, 1) == '.') {
                    results.Bay.list[0].Hght.value = '0' + results.Bay.list[0].Hght.value;
                }
                if (results.Bay.list[0].Dpth.value < 1 && results.Bay.list[0].Dpth.value > 0 && results.Bay.list[0].Dpth.value.substring(0, 1) == '.') {
                    results.Bay.list[0].Dpth.value = '0' + results.Bay.list[0].Dpth.value;
                }
            }
            else {
                if (results.XtnlDpth) {
                    results.XtnlDpth.value = Number(results.XtnlDpth.value).toFixed(3);
                }
                if (results.XtnlHgt) {
                    results.XtnlHgt.value = Number(results.XtnlHgt.value).toFixed(3);
                }
                if (results.XtnlWdth) {
                    results.XtnlWdth.value = Number(results.XtnlWdth.value).toFixed(3);
                }
                // Ensure there is a leading zero when a fraction like .75 comes across.  should display as 0.75
                if (results.XtnlDpth && results.XtnlDpth.value < 1 && results.XtnlDpth.value > 0 && results.XtnlDpth.value.substring(0, 1) == '.') {
                    results.XtnlDpth.value = '0' + results.XtnlDpth.value;
                }
                if (results.XtnlHgt && results.XtnlHgt.value < 1 && results.XtnlHgt.value > 0 && results.XtnlHgt.value.substring(0, 1) == '.') {
                    results.XtnlHgt.value = '0' + results.XtnlHgt.value;
                }
                if (results.XtnlWdth && results.XtnlWdth.value < 1 && results.XtnlWdth.value > 0 && results.XtnlWdth.value.substring(0, 1) == '.') {
                    results.XtnlWdth.value = '0' + results.XtnlWdth.value;
                }
            }
            selfbay.selectedBaySpec(results);
            selfbay.associatemtlblck = ko.observableArray();
            selfbay.searchtblbay = ko.observableArray();
            selfbay.bayRoletype = ko.observable();
            selfbay.selectXtnlDimUom = ko.observable('');
            selfbay.selectedDimUom = ko.observable('');
            selfbay.selectedBayrolTyp = ko.observable('');
            selfbay.selectedMxWghtUom = ko.observable('');
            selfbay.selectedMntngPosDistId = ko.observable('');
            selfbay.selectedStrtLbl = ko.observable(selfbay.selectedBaySpec().StrtLbl.value);
            selfbay.completedNotSelected = ko.observable(true);
            selfbay.genericBlockview = ko.observable();
            selfbay.nongenericBlockview = ko.observable();
            selfbay.enableName = ko.observable(false);
            selfbay.enableAssociate = ko.observable(false);
            selfbay.enableModalSave = ko.observable(false);
            selfbay.enableBackButton = ko.observable(false);
            selfbay.enablebayintbl = ko.observable();
            selfbay.specName = ko.observable('');
            selfbay.bayheightuom = ko.observable('in');
            selfbay.duplicateSelectedBaySpecDpth = ko.observable();
            selfbay.duplicateSelectedBaySpecHght = ko.observable();
            selfbay.duplicateSelectedBaySpecWdth = ko.observable();
            selfbay.duplicateSelectedBaySpecDltd = ko.observable();
            selfbay.duplicateSelectedBaySpecDltd = results.Dltd.bool;

            if (results.Bay != null && results.Bay.list.length > 0) {
                selfbay.duplicateSelectedBaySpecDpth(results.Bay.list[0].Dpth.value);
                selfbay.duplicateSelectedBaySpecHght(results.Bay.list[0].Hght.value);
                selfbay.duplicateSelectedBaySpecWdth(results.Bay.list[0].Wdth.value);
            }
            else {
                if (results.XtnlDpth) {
                    selfbay.duplicateSelectedBaySpecDpth(results.XtnlDpth.value);
                }
                else {
                    selfbay.duplicateSelectedBaySpecDpth('');
                }
                if (results.XtnlHgt) {
                    selfbay.duplicateSelectedBaySpecHght(results.XtnlHgt.value);
                }
                else {
                    selfbay.duplicateSelectedBaySpecHght('');
                }
                if (results.XtnlWdth) {
                    selfbay.duplicateSelectedBaySpecWdth(results.XtnlWdth.value);
                }
                else {
                    selfbay.duplicateSelectedBaySpecWdth('');
                }
            }

            selfbay.duplicateSelectedBaySpecStrghtThru = ko.observable(results.StrghtThru.value);
            selfbay.duplicateSelectedBaySpecMidPln = ko.observable(results.MidPln.value);
            selfbay.duplicateSelectedBaySpecWllMnt = ko.observable(results.WllMnt.value);
            selfbay.duplicateSelectedBaySpecName = ko.observable();
            selfbay.duplicateSelectedBaySpecModel = ko.observable();

            if (selfspec.selectRadioSpec() == 'newSpec') {
                selfbay.selectedStrtLbl('B');
                selfbay.selectedBaySpec().BayRlTypId.value = 2;
                selfbay.selectedBaySpec().MntngPosOfst.value = '';
                selfbay.selectedBaySpec().MxWght.value = '';
                selfbay.selectedBaySpec().XtnlDpth.value = '';
                selfbay.selectedBaySpec().XtnlHgt.value = '';
                selfbay.selectedBaySpec().XtnlWdth.value = '';
                selfbay.selectedBaySpec().StrghtThru.value = "Y";
            }

            if (selfbay.selectedBaySpec().Bay && selfbay.selectedBaySpec().Bay.list[0].CtlgDesc == undefined) {
                selfbay.selectedBaySpec().Bay.list[0].CtlgDesc = { "value": "" };
            }

            if (selfbay.selectedBaySpec().DimUom.value != "") {
                selfbay.selectedDimUom(selfbay.selectedBaySpec().DimUom.value);
            }

            if (selfbay.selectedBaySpec().Nm.value != "") {
                if (selfbay.selectedBaySpec().Gnrc.bool == true) {
                    selfbay.specName(selfbay.selectedBaySpec().Nm.value);
                } else {
                    selfbay.specName(selfbay.selectedBaySpec().RvsnNm.value);
                }
            } else if (selfbay.selectedBaySpec().Gnrc.bool == true) {
                selfbay.specName('BA-');
            }

            if (results.Nm != null) {
                selfbay.duplicateSelectedBaySpecName.value = selfbay.specName();
                selfbay.duplicateSelectedBaySpecModel.value = results.Nm.value;
            }

            if (selfbay.selectedBaySpec().Gnrc.bool == false) {
                selfbay.enableAssociate(true);
                selfbay.enableName(true);
            }
            //else {
            //    selfbay.enableName(true);
            //}

            if (selfbay.selectedBaySpec().Cmplt.bool && selfbay.selectedBaySpec().Prpgtd.enable) {
                selfbay.completedNotSelected(false);
            }

            if (selfbay.selectedBaySpec().Gnrc.bool == true) {
                selfbay.genericBlockview(true);
                selfbay.nongenericBlockview(false);

                if (selfbay.selectedBaySpec().Dpth && selfbay.selectedBaySpec().Dpth.value)
                    selfbay.duplicateSelectedBaySpecDpth(selfbay.selectedBaySpec().Dpth.value);

                if (selfbay.selectedBaySpec().Hght && selfbay.selectedBaySpec().Hght.value)
                    selfbay.duplicateSelectedBaySpecHght(selfbay.selectedBaySpec().Hght.value);

                if (selfbay.selectedBaySpec().Wdth && selfbay.selectedBaySpec().Wdth.value)
                    selfbay.duplicateSelectedBaySpecWdth(selfbay.selectedBaySpec().Wdth.value);
            }
            else {
                selfbay.genericBlockview(false);
                selfbay.nongenericBlockview(true);
            }

            if (selfbay.selectedBaySpec().BayRlTypId.value == 4 || selfbay.selectedBaySpec().BayRlTypId.value == 5) {
                selfbay.enablebayintbl(false);
                $('input.baytblChkb').not(this).prop('checked', false);
                selfbay.selectedBaySpec().BayItnlId.value = 0;
                $("#BayItnlLstTblDgr").hide();
            }
            else {
                selfbay.enablebayintbl(true);
            }

            if (selfspec.backToMtlItmId > 0) {
                selfbay.enableBackButton(true);
            }

            selfbay.selectXtnlDimUom = selfbay.selectedBaySpec().XtnlDimUom.value;
            selfbay.selectedBayrolTyp = selfbay.selectedBaySpec().BayRlTypId.value;
            selfbay.selectedMxWghtUom = selfbay.selectedBaySpec().MxWghtUom.value;

            if (results.BayItnlLst != undefined && results.BayItnlLst.list != undefined) {
                selfbay.Bayinterlsdetails(results.BayItnlLst.list);
            }

            setTimeout(function () {
                $(document).ready(function () {
                    var bayTable = $('#bayinternalTbl').DataTable({
                        paging: false,
                        initComplete: function () {
                            this.api().columns().every(function () {
                                var column = this;
                                var val = column[0][0];
                                if (val === 3 || val === 5 || val === 7 || val === 11 || val === 9 || val === 10 || val === 12 || val == 13) {
                                    var select = $('<select style="color:black; font-weight:normal;"><option value=""></option></select>')
                                        .appendTo($(column.header()))
                                        .on('change', function (event) {
                                            if (event.stopPropagation) {
                                                event.stopPropagation();   // W3C model
                                            } else {
                                                event.cancelBubble = true; // IE model
                                            }
                                            var val = $.fn.dataTable.util.escapeRegex(
                                                $(this).val()
                                            );

                                            column
                                                .search(val ? '^' + val + '$' : '', true, false)
                                                .draw();
                                        });

                                    column.data().unique().sort().each(function (d, j) {
                                        select.append('<option >' + d + ' </option>');

                                    });
                                    //$('#dd1').children('option[value=""]').text('All');
                                }
                            });
                        }
                    });

                    //$('#bayinternalTbl tfoot th').each(function () {
                    //    var title = $(this).text();
                    //    $(this).html('<select  class="form-control input-md " id="dd1"><option value=""></option></select>');

                    //});                
                    // Apply the search

                    //var that = this;
                    //$('input', this.footer()).on('keyup change', function () {
                    //    if (that.search() !== this.value) {
                    //        that
                    //            .search(this.value)
                    //            .draw();
                    //    }

                    //});

                    $('input.baytblChkb').on('change', function () {
                        $('input.baytblChkb').not(this).prop('checked', false);

                        if (selfbay.selectedBaySpec().BayRlTypId.value == 4 || selfbay.selectedBaySpec().BayRlTypId.value == 5) {
                            selfbay.selectedBaySpec().BayItnlId.value = 0;
                        }
                        else {
                            selfbay.selectedBaySpec().BayItnlId.value = $(this).val();
                        }

                        $("#BayItnlLstTblDgr").hide();

                    });

                    $('#idbayrltyp').on('change', function () {
                        var ddltyp = $('#idbayrltyp').val();
                        // if distribution frame, make variable height and width true
                        if (ddltyp == 4) {

                        }
                        if (ddltyp == 4 || ddltyp == 5) {
                            selfbay.enablebayintbl(false);
                            $('input.baytblChkb').not(this).prop('checked', false);
                            selfbay.selectedBaySpec().BayItnlId.value = 0;
                            $("#BayItnlLstTblDgr").hide();
                        }
                        else {
                            selfbay.enablebayintbl(true);
                            $('input.baytblChkb').on('change', function () {
                                $('input.baytblChkb').not(this).prop('checked', false);
                                selfbay.selectedBaySpec().BayItnlId.value = $(this).val();
                                $("#BayItnlLstTblDgr").hide();
                            });
                        }

                    });
                });

                selfbay.onchangebayGeneric();



            }, 2000);
            setTimeout(function () {
                if (document.getElementById('idBayDimUom') !== null) {
                    if (document.getElementById('idBayDimUom').value == '') {
                        document.getElementById('idBayDimUom').value = '22';
                    }
                }
                if (document.getElementById('idXDimuom') !== null) {

                    if (document.getElementById('idXDimuom').value == '') {
                        document.getElementById('idXDimuom').value = '22';
                    }
                }
                if (document.getElementById('idDimuom') !== null) {

                    if (document.getElementById('idDimuom').value == '') {
                        document.getElementById('idDimuom').value = '22';
                    }
                }
            }, 5000);
            if (selfbay.selectedBaySpec().MntngPosOfst.value == "" && document.getElementById('idDimuom')) {
                document.getElementById('idDimuom').value = "";
            }

            if (selfbay.selectedBaySpec().DimUom.value == "" && document.getElementById('mtngPstOffbay')) {
                document.getElementById('mtngPstOffbay').value = "";
            }

            if (selfbay.selectedBaySpec().MxWght.value == "" && document.getElementById('idMaxwidthuom')) {
                document.getElementById('idMaxwidthuom').value = "";
            }

            if (selfbay.selectedBaySpec().MxWghtUom.value == "" && document.getElementById('maxCapNo')) {
                document.getElementById('maxCapNo').value = "";
            }

            if (selfbay.selectedBaySpec().Bay !== undefined) {
                if (selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnNrmUom.value == "" && document.getElementById('txtNrmlDrn')) {
                    document.getElementById('txtNrmlDrn').value = "";
                }

                if (selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnNrm.value == "" && document.getElementById('idNrmlDrnUom')) {
                    document.getElementById('idNrmlDrnUom').value = "";
                }

                if (selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnMxUom.value == "" && document.getElementById('txtPkDrn')) {
                    document.getElementById('txtPkDrn').value = "";
                }

                if (selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnMx.value == "" && document.getElementById('idPkDrnUom')) {
                    document.getElementById('idPkDrnUom').value = "";
                }

                if (selfbay.selectedBaySpec().Bay.list[0].HtDssptnUom.value == "" && document.getElementById('txtPwr')) {
                    document.getElementById('txtPwr').value = "";
                }

                if (selfbay.selectedBaySpec().Bay.list[0].HtDssptn.value == "" && document.getElementById('idPwrUom')) {
                    document.getElementById('idPwrUom').value = "";
                }

                if (selfbay.selectedBaySpec().Bay.list[0].HtGntnUom.value == "" && document.getElementById('txtHtGntn')) {
                    document.getElementById('txtHtGntn').value = "";
                }

                if (selfbay.selectedBaySpec().Bay.list[0].HtGntn.value == "" && document.getElementById('idHtGntnUom')) {
                    document.getElementById('idHtGntnUom').value = "";
                }

                if (selfbay.selectedBaySpec().Bay.list[0].Wght.value == "" && document.getElementById('idWtUom')) {
                    document.getElementById('idWtUom').value = "";
                }

                if (selfbay.selectedBaySpec().Bay.list[0].WghtUom.value == "" && document.getElementById('txtWt')) {
                    document.getElementById('txtWt').value = "";
                }
                setTimeout(function () {
                    document.getElementById('txtDpth').required = true;
                    document.getElementById('txtHght').required = true;
                    document.getElementById('txtWdth').required = true;
                }, 5000);

            }


            //$(document).on('change', '[type=checkbox]', function () {
            //    if (this.name == "gnrcIndbay") {
            //        if (this.checked == true) {                  
            //            document.getElementById('rcrdlyBay').disabled = true;
            //        }
            //        else {                    
            //            document.getElementById('rcrdlyBay').disabled = false;
            //        }
            //    }
            //});

            $(document).on('change', '[type=checkbox]', function () {
                //if (this.name == "proptIndBay") {
                //    if (this.checked == true && document.getElementById('gnrcInd').checked == false) {
                //        selfbay.enableAssociate(true);
                //    }
                //    else {
                //        selfbay.enableAssociate(false);
                //    }
                //} else

                if (this.name == "gnrcIndbay") {
                    if (this.checked == true) {
                        document.getElementById('rcrdlyBay').disabled = true;
                        selfbay.enableAssociate(false);
                        selfbay.enableName(false);
                    }
                    else {
                        document.getElementById('rcrdlyBay').disabled = false;

                        //if (document.getElementById('proptIndBay').checked == true) {
                        selfbay.enableAssociate(true);
                        selfbay.enableName(true);
                        //}
                    }
                } else if (this.name == "rcrdolyBay") {
                    if (this.checked == true) {
                        document.getElementById('gnrcInd').disabled = true;
                    }
                    else {
                        document.getElementById('gnrcInd').disabled = false;
                    }
                }
            });

            //$(document).on('change', '[type=checkbox]', function () {
            //    if (this.name == "rcrdolyBay") {
            //        if (this.checked == true) {
            //            document.getElementById('gnrcInd').disabled = true;
            //        }
            //        else {
            //            document.getElementById('gnrcInd').disabled = false;
            //        }
            //    }
            //});

            $(document).on('change', '[type=checkbox]', function () {
                if (this.name == "cmpindnm") {
                    if (this.checked == false) {
                        document.getElementById('proptIndBay').checked = false;
                    }
                }
            });
        };

        BaySpecification.prototype.onchangeCompleted = function () {
            if ($("#cmpIndBay").is(':checked')) {
                selfbay.completedNotSelected(false);
            } else {
                selfbay.completedNotSelected(true);
                selfbay.selectedBaySpec().Prpgtd.bool = false;
            }
        };

        BaySpecification.prototype.onchangebayGeneric = function () {
            if ($("#gnrcInd").is(':checked')) {
                selfbay.genericBlockview(true);
                selfbay.nongenericBlockview(false);
                selfbay.selectedBaySpec().RO.enable = false;

                document.getElementById("xtnldpthbay").required = true;
                document.getElementById("xhghtbay").required = true;
                document.getElementById("xtnwdthbay").required = true;
                document.getElementById("idXDimuom").required = true;
                document.getElementById("maxCapNo").required = false;
                document.getElementById("idMaxwidthuom").required = false;
                document.getElementById("modelTxt").required = false;
            } else {
                selfbay.genericBlockview(false);
                selfbay.nongenericBlockview(true);
                selfbay.selectedBaySpec().RO.enable = true;

                document.getElementById("xtnldpthbay").required = false;
                document.getElementById("xhghtbay").required = false;
                document.getElementById("xtnwdthbay").required = false;
                document.getElementById("idXDimuom").required = false;
                document.getElementById("modelTxt").required = true;
                //document.getElementById("maxCapNo").required = true;
                //document.getElementById("idMaxwidthuom").required = true;            
            }
        };

        BaySpecification.prototype.reset = function () {
        };

        BaySpecification.prototype.navigateToMaterial = function () {
            if (document.getElementById('rcrdlyBay').checked == true) {
                var url = '#/roNew/' + selfspec.backToMtlItmId;
                router.navigate(url, false);
            }
            else {
                var url = '#/mtlInv/' + selfspec.backToMtlItmId;
                router.navigate(url, false);


            }

        };

        //BaySpecification.prototype.activate = function (mtrlItmId) {
        //    console.log(specType + ", " + specId);

        //    //if (specType != null) {
        //    //    var mtlItmId = query.mtlid;

        //    //    selfspec.isRedirect = true;
        //    //    selfspec.backToMtlItmId = mtlItmId

        //    //    if (specId == null) {
        //    //        selfspec.selectRadioSpec('newSpec')
        //    //        selfspec.selectedNewSpecification(specType);
        //    //        selfspec.specificationSelected(0, specType, selfspec, mtlItmId);
        //    //    } else {
        //    //        selfspec.specificationSelected(specId, specType, selfspec, 0);
        //    //    }
        //    //}
        //};

        BaySpecification.prototype.NumDecimal = function (mp, event) {
            //self.saveValidation(true);
            var charCode = event.keyCode;
            var name = event.currentTarget.id;
            var value = $('#' + name).val();
            var reqCharAfterdot = 3;

            if (name == "txtHght" || name == "txtWdth" || name == "txtDpth" || name == "xtnldpthbay" || name == "xhghtbay" || name == "xtnwdthbay") {
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

        BaySpecification.prototype.SaveBay = function () {

            // check for duplicate model name
            var modelname = document.getElementById("modelTxt").value;
            if (selfspec.selectRadioSpec() == 'existSpec') {
                var modelnamecount = selfbay.GetModelNameCount(modelname, selfbay.selectedBaySpec().Bay.list[0].SpecId.value);
            }
            else {
                var modelnamecount = selfbay.GetModelNameCount(modelname, 0);
            }
            if (modelnamecount > 0) {
                app.showMessage('The model name ' + modelname + ' already exists on a different Spec.');
                return;
            }

            specChangeArray = [];
            var name = document.getElementById("idbayspecnm").value;
            var chkgenrc = false;

            if ($("#gnrcInd").is(':checked')) {
                chkgenrc = true;
            }

            selfbay.selectedBaySpec().MxWght.value = $("#maxCapNo").val();
            selfbay.selectedBaySpec().MntngPosOfst.value = $("#mtngPstOffbay").val();

            if (selfbay.selectedBaySpec().Bay !== undefined) {
                selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnNrm.value = $("#txtNrmlDrn").val();
                selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnMx.value = $("#txtPkDrn").val();
                selfbay.selectedBaySpec().Bay.list[0].HtDssptn.value = $("#txtPwr").val();
                selfbay.selectedBaySpec().Bay.list[0].HtGntn.value = $("#txtHtGntn").val();
                selfbay.selectedBaySpec().Bay.list[0].Wght.value = $("#txtWt").val();
            }

            if ((chkgenrc) || (!chkgenrc && selfbay.selectedBaySpec().Bay !== undefined)) {
                if (document.getElementById('idbayrltyp').value == 4 || document.getElementById('idbayrltyp').value == 5) {
                    if ($("#midInd").is(':checked'))
                        selfbay.selectedBaySpec().MidPln.value = "Y";
                    else
                        selfbay.selectedBaySpec().MidPln.value = "N";

                    if ($("#wllmtInd").is(':checked'))
                        selfbay.selectedBaySpec().WllMnt.value = "Y";
                    else
                        selfbay.selectedBaySpec().WllMnt.value = "N";

                    if ($("#strgtInd").is(':checked'))
                        selfbay.selectedBaySpec().StrghtThru.value = "Y";
                    else
                        selfbay.selectedBaySpec().StrghtThru.value = "N";

                    if ($("#dualInd").is(':checked'))
                        selfbay.selectedBaySpec().DualSd.value = "Y";
                    else
                        selfbay.selectedBaySpec().DualSd.value = "N";

                    selfbay.selectedBaySpec().StrtLbl.value = selfbay.selectedStrtLbl();

                    if (selfbay.selectedBaySpec().Gnrc.bool == true)
                        selfbay.selectedBaySpec().Nm.value = selfbay.specName();
                    else
                        selfbay.selectedBaySpec().RvsnNm.value = selfbay.specName();

                    var txtCondt = '';
                    if (selfspec.selectRadioSpec() == 'existSpec') {
                        if (selfbay.selectedBaySpec().Bay != null && selfbay.selectedBaySpec().Bay.list.length > 0) {

                            if (selfbay.specName() != selfbay.duplicateSelectedBaySpecName.value) {
                                txtCondt += "Name changed to <b>" + selfbay.specName() + '</b> from ' + selfbay.duplicateSelectedBaySpecName.value + "<br/>";

                                var saveJSON = {
                                    'tablename': 'bay_specn', 'columnname': 'bay_specn_nm', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfbay.duplicateSelectedBaySpecName.value,
                                    'newcolval': selfbay.specName(),
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Spec Name from ' + selfbay.duplicateSelectedBaySpecName.value + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (Number(selfbay.selectedBaySpec().Bay.list[0].Dpth.value).toFixed(3) != Number(selfbay.duplicateSelectedBaySpecDpth()).toFixed(3)) {
                                txtCondt += "Depth changed to <b>" + Number(selfbay.selectedBaySpec().Bay.list[0].Dpth.value).toFixed(3) + '</b> from ' + Number(selfbay.duplicateSelectedBaySpecDpth()).toFixed(3) + "<br/>";

                                var saveJSON = {
                                    'tablename': 'rme_bay_mtrl', 'columnname': 'xtnl_dpth_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].MtrlId.value, 'auditprnttblpkcolnm': '',
                                    'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfbay.duplicateSelectedBaySpecDpth()).toFixed(3), 'newcolval': Number(selfbay.selectedBaySpec().Bay.list[0].Dpth.value).toFixed(3), 'cuid': user.cuid,
                                    'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Depth from ' + Number(selfbay.duplicateSelectedBaySpecDpth()).toFixed(3) + ' to ' + Number(selfbay.selectedBaySpec().Bay.list[0].Dpth.value).toFixed(3) + ' on ',
                                    'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (Number(selfbay.selectedBaySpec().Bay.list[0].Hght.value).toFixed(3) != Number(selfbay.duplicateSelectedBaySpecHght()).toFixed(3)) {
                                txtCondt += "Height changed to <b>" + Number(selfbay.selectedBaySpec().Bay.list[0].Hght.value).toFixed(3) + '</b> from ' + Number(selfbay.duplicateSelectedBaySpecHght()).toFixed(3) + "<br/>";

                                var saveJSON = {
                                    'tablename': 'rme_bay_mtrl', 'columnname': 'xtnl_hgt_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].MtrlId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': Number(selfbay.duplicateSelectedBaySpecHght()).toFixed(3),
                                    'newcolval': Number(selfbay.selectedBaySpec().Bay.list[0].Hght.value).toFixed(3),
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Height from ' + Number(selfbay.duplicateSelectedBaySpecHght()).toFixed(3) + ' to ' + Number(selfbay.selectedBaySpec().Bay.list[0].Hght.value).toFixed(3) + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (Number(selfbay.selectedBaySpec().Bay.list[0].Wdth.value).toFixed(3) != Number(selfbay.duplicateSelectedBaySpecWdth()).toFixed(3)) {
                                txtCondt += "Width changed to <b>" + Number(selfbay.selectedBaySpec().Bay.list[0].Wdth.value).toFixed(3) + '</b> from ' + Number(selfbay.duplicateSelectedBaySpecWdth()).toFixed(3) + "<br/>";

                                var saveJSON = {
                                    'tablename': 'rme_bay_mtrl', 'columnname': 'xtnl_wdth_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].MtrlId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': Number(selfbay.duplicateSelectedBaySpecWdth()).toFixed(3),
                                    'newcolval': Number(selfbay.selectedBaySpec().Bay.list[0].Wdth.value).toFixed(3),
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Width from ' + Number(selfbay.duplicateSelectedBaySpecWdth()).toFixed(3) + ' to ' + Number(selfbay.selectedBaySpec().Bay.list[0].Wdth.value).toFixed(3) + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfbay.duplicateSelectedBaySpecStrghtThru() != selfbay.selectedBaySpec().StrghtThru.value) {
                                txtCondt += "Straight Through changed to <b>" + selfbay.selectedBaySpec().StrghtThru.value + '</b> from ' + selfbay.duplicateSelectedBaySpecStrghtThru() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'bay_specn', 'columnname': 'strght_thru_ind', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfbay.duplicateSelectedBaySpecStrghtThru(),
                                    'newcolval': selfbay.selectedBaySpec().StrghtThru.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Straight Thru from ' + selfbay.duplicateSelectedBaySpecStrghtThru() + ' to ' + selfbay.selectedBaySpec().StrghtThru.value + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfbay.duplicateSelectedBaySpecMidPln() != selfbay.selectedBaySpec().MidPln.value) {
                                txtCondt += "Mid plane changed to <b>" + selfbay.selectedBaySpec().MidPln.value + '</b> from ' + selfbay.duplicateSelectedBaySpecMidPln() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'bay_specn', 'columnname': 'mid_pln_ind', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfbay.duplicateSelectedBaySpecMidPln(),
                                    'newcolval': selfbay.selectedBaySpec().MidPln.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Mid Plane from ' + selfbay.duplicateSelectedBaySpecMidPln() + ' to ' + selfbay.selectedBaySpec().MidPln.value + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfbay.duplicateSelectedBaySpecWllMnt() != selfbay.selectedBaySpec().WllMnt.value) {
                                txtCondt += "Wall Mount changed to <b>" + selfbay.selectedBaySpec().WllMnt.value + '</b> from ' + selfbay.duplicateSelectedBaySpecWllMnt() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'bay_specn', 'columnname': 'wll_mnt_allow_ind', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfbay.duplicateSelectedBaySpecWllMnt(),
                                    'newcolval': selfbay.selectedBaySpec().WllMnt.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Wall Mount from ' + selfbay.duplicateSelectedBaySpecWllMnt() + ' to ' + selfbay.selectedBaySpec().WllMnt.value + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }
                            if (selfbay.selectedBaySpec().Dltd.bool != selfbay.duplicateSelectedBaySpecDltd) {
                                txtCondt += "Unusable changed to <b>" + selfbay.selectedBaySpec().Dltd.bool + '</b> from ' + selfbay.duplicateSelectedBaySpecDltd + "<br/>";

                                var saveJSON = {
                                    'tablename': 'bay_specn_revsn_alt', 'columnname': 'del_ind', 'audittblpkcolnm': 'bay_specn_revsn_alt_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecRvsnId.value, 'auditprnttblpkcolnm': 'bay_specn_id', 'auditprnttblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'actncd': 'C',
                                    'oldcolval': selfbay.duplicateSelectedBaySpecDltd,
                                    'newcolval': selfbay.selectedBaySpec().Dltd.bool,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Unusable from ' + selfbay.duplicateSelectedBaySpecDltd + ' to ' + selfbay.selectedBaySpec().Dltd.bool + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }
                        } else if (selfbay.selectedBaySpec().Gnrc.bool) {
                            if (selfbay.selectedBaySpec().XtnlDpth.value != selfbay.duplicateSelectedBaySpecDpth()) {
                                txtCondt += "Depth changed to <b>" + selfbay.selectedBaySpec().XtnlDpth.value + '</b> from ' + selfbay.duplicateSelectedBaySpecDpth() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'bay_specn_gnrc', 'columnname': 'xtnl_dpth_no', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfbay.duplicateSelectedBaySpecDpth(),
                                    'newcolval': selfbay.selectedBaySpec().XtnlDpth.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Depth from ' + selfbay.duplicateSelectedBaySpecDpth() + ' to ' + selfbay.selectedBaySpec().XtnlDpth.value + ' on ',
                                    'materialitemid': '0'
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfbay.selectedBaySpec().XtnlHgt.value != selfbay.duplicateSelectedBaySpecHght()) {
                                txtCondt += "Height changed to <b>" + selfbay.selectedBaySpec().XtnlHgt.value + '</b> from ' + selfbay.duplicateSelectedBaySpecHght() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'bay_specn_gnrc', 'columnname': 'xtnl_hgt_no', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfbay.duplicateSelectedBaySpecHght(),
                                    'newcolval': selfbay.selectedBaySpec().XtnlHgt.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Height from ' + selfbay.duplicateSelectedBaySpecHght() + ' to ' + selfbay.selectedBaySpec().XtnlHgt.value + ' on ',
                                    'materialitemid': '0'
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfbay.selectedBaySpec().XtnlWdth.value != selfbay.duplicateSelectedBaySpecWdth()) {
                                txtCondt += "Width changed to <b>" + selfbay.selectedBaySpec().XtnlWdth.value + '</b> from ' + selfbay.duplicateSelectedBaySpecWdth() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'bay_specn_gnrc', 'columnname': 'xtnl_wdth_no', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfbay.duplicateSelectedBaySpecWdth(),
                                    'newcolval': selfbay.selectedBaySpec().XtnlWdth.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Width from ' + selfbay.duplicateSelectedBaySpecWdth() + ' to ' + selfbay.selectedBaySpec().XtnlWdth.value + ' on ',
                                    'materialitemid': '0'
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfbay.duplicateSelectedBaySpecStrghtThru() != selfbay.selectedBaySpec().StrghtThru.value) {
                                txtCondt += "Straight Through changed to <b>" + selfbay.selectedBaySpec().StrghtThru.value + '</b> from ' + selfbay.duplicateSelectedBaySpecStrghtThru() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'bay_specn', 'columnname': 'strght_thru_ind', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfbay.duplicateSelectedBaySpecStrghtThru(),
                                    'newcolval': selfbay.selectedBaySpec().StrghtThru.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Straight Thru from ' + selfbay.duplicateSelectedBaySpecStrghtThru() + ' to ' + selfbay.selectedBaySpec().StrghtThru.value + ' on ',
                                    'materialitemid': '0'
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfbay.duplicateSelectedBaySpecMidPln() != selfbay.selectedBaySpec().MidPln.value) {
                                txtCondt += "Mid plane changed to <b>" + selfbay.selectedBaySpec().MidPln.value + '</b> from ' + selfbay.duplicateSelectedBaySpecMidPln() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'bay_specn', 'columnname': 'mid_pln_ind', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfbay.duplicateSelectedBaySpecMidPln(),
                                    'newcolval': selfbay.selectedBaySpec().MidPln.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Mid Plane from ' + selfbay.duplicateSelectedBaySpecMidPln() + ' to ' + selfbay.selectedBaySpec().MidPln.value + ' on ',
                                    'materialitemid': '0'
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfbay.duplicateSelectedBaySpecWllMnt() != selfbay.selectedBaySpec().WllMnt.value) {
                                txtCondt += "Wall Mount changed to <b>" + selfbay.selectedBaySpec().WllMnt.value + '</b> from ' + selfbay.duplicateSelectedBaySpecWllMnt() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'bay_specn', 'columnname': 'wll_mnt_allow_ind', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfbay.duplicateSelectedBaySpecWllMnt(),
                                    'newcolval': selfbay.selectedBaySpec().WllMnt.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Wall Mount from ' + selfbay.duplicateSelectedBaySpecWllMnt() + ' to ' + selfbay.selectedBaySpec().WllMnt.value + ' on ',
                                    'materialitemid': '0'
                                };
                                specChangeArray.push(saveJSON);
                            }
                            if (selfbay.selectedBaySpec().Dltd.bool != selfbay.duplicateSelectedBaySpecDltd) {
                                txtCondt += "Unusable changed to <b>" + selfbay.selectedBaySpec().Dltd.bool + '</b> from ' + selfbay.duplicateSelectedBaySpecDltd + "<br/>";

                                var saveJSON = {
                                    'tablename': 'bayspecn_gnrc', 'columnname': 'del_ind', 'audittblpkcolnm': 'bay_specn_revsn_alt_id', 'audittblpkcolval': selfbay.selectedBaySpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfbay.duplicateSelectedBaySpecDltd,
                                    'newcolval': selfbay.selectedBaySpec().Dltd.bool,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Unusable from ' + selfbay.duplicateSelectedBaySpecDltd + ' to ' + selfbay.selectedBaySpec().Dltd.bool + ' on ',
                                    'materialitemid': '0'
                                };
                                specChangeArray.push(saveJSON);
                            }
                        }
                    }

                    var textlength = txtCondt.length;
                    if (selfbay.selectedBaySpec().Bay) {
                        var configNameList = selfbay.getConfigNames(selfbay.selectedBaySpec().Bay.list[0].id.value);
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
                        app.showMessage(txtCondt, 'Update Confirmation for Bay', ['Ok', 'Cancel']).then(function (dialogResult) {
                            if (dialogResult == 'Ok') {
                                selfbay.saveBaySpec();
                            }
                        });
                    } else {
                        selfbay.saveBaySpec();
                    }
                }
                else {
                    console.log(selfbay.selectedBaySpec());
                    var bayintsectedchk = false;
                    var bayItnltblchk = $("#bayinternalTbl .baytblChkb");

                    for (var i = 0; i < bayItnltblchk.length; i++) {
                        if (bayItnltblchk[i].checked == true) {
                            bayintsectedchk = true;
                        }
                    }

                    if (bayintsectedchk) {

                        if ($("#midInd").is(':checked'))
                            selfbay.selectedBaySpec().MidPln.value = "Y";
                        else
                            selfbay.selectedBaySpec().MidPln.value = "N";

                        if ($("#wllmtInd").is(':checked'))
                            selfbay.selectedBaySpec().WllMnt.value = "Y";
                        else
                            selfbay.selectedBaySpec().WllMnt.value = "N";

                        if ($("#strgtInd").is(':checked'))
                            selfbay.selectedBaySpec().StrghtThru.value = "Y";
                        else
                            selfbay.selectedBaySpec().StrghtThru.value = "N";

                        if ($("#dualInd").is(':checked'))
                            selfbay.selectedBaySpec().DualSd.value = "Y";
                        else
                            selfbay.selectedBaySpec().DualSd.value = "N";

                        if (selfbay.selectedBaySpec().Gnrc.bool == true)
                            selfbay.selectedBaySpec().Nm.value = selfbay.specName();
                        else
                            selfbay.selectedBaySpec().RvsnNm.value = selfbay.specName();

                        selfbay.selectedBaySpec().StrtLbl.value = selfbay.selectedStrtLbl();

                        var txtCondt = '';
                        if (selfspec.selectRadioSpec() == 'existSpec') {
                            if (selfbay.selectedBaySpec().Bay != null && selfbay.selectedBaySpec().Bay.list.length > 0) {
                                if (selfbay.specName() != selfbay.duplicateSelectedBaySpecName.value) {

                                    txtCondt += "Name changed to <b>" + selfbay.specName() + '</b> from ' + selfbay.duplicateSelectedBaySpecName.value + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'bay_specn', 'columnname': 'bay_specn_nm', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                        'oldcolval': selfbay.duplicateSelectedBaySpecName.value,
                                        'newcolval': selfbay.specName(),
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Spec Name from ' + selfbay.duplicateSelectedBaySpecName.value + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                    };
                                    specChangeArray.push(saveJSON);
                                }

                                if (Number(selfbay.selectedBaySpec().Bay.list[0].Dpth.value).toFixed(3) != Number(selfbay.duplicateSelectedBaySpecDpth()).toFixed(3)) {
                                    txtCondt += "Depth changed to <b>" + Number(selfbay.selectedBaySpec().Bay.list[0].Dpth.value).toFixed(3) + '</b> from ' + Number(selfbay.duplicateSelectedBaySpecDpth()).toFixed(3) + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'rme_bay_mtrl', 'columnname': 'xtnl_dpth_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].MtrlId.value, 'auditprnttblpkcolnm': '',
                                        'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfbay.duplicateSelectedBaySpecDpth()).toFixed(3), 'newcolval': Number(selfbay.selectedBaySpec().Bay.list[0].Dpth.value).toFixed(3), 'cuid': user.cuid,
                                        'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Depth from ' + Number(selfbay.duplicateSelectedBaySpecDpth()).toFixed(3) + ' to ' + Number(selfbay.selectedBaySpec().Bay.list[0].Dpth.value).toFixed(3) + ' on ',
                                        'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                    };
                                    specChangeArray.push(saveJSON);
                                }

                                if (Number(selfbay.selectedBaySpec().Bay.list[0].Hght.value).toFixed(3) != Number(selfbay.duplicateSelectedBaySpecHght()).toFixed(3)) {
                                    txtCondt += "Height changed to <b>" + Number(selfbay.selectedBaySpec().Bay.list[0].Hght.value).toFixed(3) + '</b> from ' + Number(selfbay.duplicateSelectedBaySpecHght()).toFixed(3) + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'rme_bay_mtrl', 'columnname': 'xtnl_hgt_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].MtrlId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                        'oldcolval': Number(selfbay.duplicateSelectedBaySpecHght()).toFixed(3),
                                        'newcolval': Number(selfbay.selectedBaySpec().Bay.list[0].Hght.value).toFixed(3),
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Height from ' + Number(selfbay.duplicateSelectedBaySpecHght()).toFixed(3) + ' to ' + Number(selfbay.selectedBaySpec().Bay.list[0].Hght.value).toFixed(3) + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                    };
                                    specChangeArray.push(saveJSON);
                                }

                                if (Number(selfbay.selectedBaySpec().Bay.list[0].Wdth.value).toFixed(3) != Number(selfbay.duplicateSelectedBaySpecWdth()).toFixed(3)) {
                                    txtCondt += "Width changed to <b>" + Number(selfbay.selectedBaySpec().Bay.list[0].Wdth.value).toFixed(3) + '</b> from ' + Number(selfbay.duplicateSelectedBaySpecWdth()).toFixed(3) + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'rme_bay_mtrl', 'columnname': 'xtnl_wdth_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].MtrlId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                        'oldcolval': Number(selfbay.duplicateSelectedBaySpecWdth()).toFixed(3),
                                        'newcolval': Number(selfbay.selectedBaySpec().Bay.list[0].Wdth.value).toFixed(3),
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Width from ' + Number(selfbay.duplicateSelectedBaySpecWdth()).toFixed(3) + ' to ' + Number(selfbay.selectedBaySpec().Bay.list[0].Wdth.value).toFixed(3) + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                    };
                                    specChangeArray.push(saveJSON);
                                }

                                if (selfbay.duplicateSelectedBaySpecStrghtThru() != selfbay.selectedBaySpec().StrghtThru.value) {
                                    txtCondt += "Straight Through changed to <b>" + selfbay.selectedBaySpec().StrghtThru.value + '</b> from ' + selfbay.duplicateSelectedBaySpecStrghtThru() + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'bay_specn', 'columnname': 'strght_thru_ind', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                        'oldcolval': selfbay.duplicateSelectedBaySpecStrghtThru(),
                                        'newcolval': selfbay.selectedBaySpec().StrghtThru.value,
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Straight Thru from ' + selfbay.duplicateSelectedBaySpecStrghtThru() + ' to ' + selfbay.selectedBaySpec().StrghtThru.value + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                    };
                                    specChangeArray.push(saveJSON);
                                }

                                if (selfbay.duplicateSelectedBaySpecMidPln() != selfbay.selectedBaySpec().MidPln.value) {
                                    txtCondt += "Mid plane changed to <b>" + selfbay.selectedBaySpec().MidPln.value + '</b> from ' + selfbay.duplicateSelectedBaySpecMidPln() + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'bay_specn', 'columnname': 'mid_pln_ind', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                        'oldcolval': selfbay.duplicateSelectedBaySpecMidPln(),
                                        'newcolval': selfbay.selectedBaySpec().MidPln.value,
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Mid Plane from ' + selfbay.duplicateSelectedBaySpecMidPln() + ' to ' + selfbay.selectedBaySpec().MidPln.value + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                    };
                                    specChangeArray.push(saveJSON);
                                }

                                if (selfbay.duplicateSelectedBaySpecWllMnt() != selfbay.selectedBaySpec().WllMnt.value) {
                                    txtCondt += "Wall Mount changed to <b>" + selfbay.selectedBaySpec().WllMnt.value + '</b> from ' + selfbay.duplicateSelectedBaySpecWllMnt() + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'bay_specn', 'columnname': 'wll_mnt_allow_ind', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                        'oldcolval': selfbay.duplicateSelectedBaySpecWllMnt(),
                                        'newcolval': selfbay.selectedBaySpec().WllMnt.value,
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Wall Mount from ' + selfbay.duplicateSelectedBaySpecWllMnt() + ' to ' + selfbay.selectedBaySpec().WllMnt.value + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                    };
                                    specChangeArray.push(saveJSON);
                                }
                                if (selfbay.selectedBaySpec().Dltd.bool != selfbay.duplicateSelectedBaySpecDltd) {
                                    txtCondt += "Unusable changed to <b>" + selfbay.selectedBaySpec().Dltd.bool + '</b> from ' + selfbay.duplicateSelectedBaySpecDltd + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'bay_specn_revsn_alt', 'columnname': 'del_ind', 'audittblpkcolnm': 'bay_specn_revsn_alt_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecRvsnId.value, 'auditprnttblpkcolnm': 'bay_specn_id', 'auditprnttblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'actncd': 'C',
                                        'oldcolval': selfbay.duplicateSelectedBaySpecDltd,
                                        'newcolval': selfbay.selectedBaySpec().Dltd.bool,
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Unusable from ' + selfbay.duplicateSelectedBaySpecDltd + ' to ' + selfbay.selectedBaySpec().Dltd.bool + ' on ', 'materialitemid': selfbay.selectedBaySpec().Bay.list[0].id.value
                                    };
                                    specChangeArray.push(saveJSON);
                                }
                            } else if (selfbay.selectedBaySpec().Gnrc.bool) {
                                if (selfbay.selectedBaySpec().XtnlDpth.value != selfbay.duplicateSelectedBaySpecDpth()) {
                                    txtCondt += "Depth changed to <b>" + selfbay.selectedBaySpec().XtnlDpth.value + '</b> from ' + selfbay.duplicateSelectedBaySpecDpth() + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'bay_specn_gnrc', 'columnname': 'xtnl_dpth_no', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                        'oldcolval': selfbay.duplicateSelectedBaySpecDpth(),
                                        'newcolval': selfbay.selectedBaySpec().XtnlDpth.value,
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Depth from ' + selfbay.duplicateSelectedBaySpecDpth() + ' to ' + selfbay.selectedBaySpec().XtnlDpth.value + ' on ',
                                        'materialitemid': '0'
                                    };
                                    specChangeArray.push(saveJSON);
                                }

                                if (selfbay.selectedBaySpec().XtnlHgt.value != selfbay.duplicateSelectedBaySpecHght()) {
                                    txtCondt += "Height changed to <b>" + selfbay.selectedBaySpec().XtnlHgt.value + '</b> from ' + selfbay.duplicateSelectedBaySpecHght() + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'bay_specn_gnrc', 'columnname': 'xtnl_hgt_no', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                        'oldcolval': selfbay.duplicateSelectedBaySpecHght(),
                                        'newcolval': selfbay.selectedBaySpec().XtnlHgt.value,
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Height from ' + selfbay.duplicateSelectedBaySpecHght() + ' to ' + selfbay.selectedBaySpec().XtnlHgt.value + ' on ',
                                        'materialitemid': '0'
                                    };
                                    specChangeArray.push(saveJSON);
                                }

                                if (selfbay.selectedBaySpec().XtnlWdth.value != selfbay.duplicateSelectedBaySpecWdth()) {
                                    txtCondt += "Width changed to <b>" + selfbay.selectedBaySpec().XtnlWdth.value + '</b> from ' + selfbay.duplicateSelectedBaySpecWdth() + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'bay_specn_gnrc', 'columnname': 'xtnl_wdth_no', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                        'oldcolval': selfbay.duplicateSelectedBaySpecWdth(),
                                        'newcolval': selfbay.selectedBaySpec().XtnlWdth.value,
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Width from ' + selfbay.duplicateSelectedBaySpecWdth() + ' to ' + selfbay.selectedBaySpec().XtnlWdth.value + ' on ',
                                        'materialitemid': '0'
                                    };
                                    specChangeArray.push(saveJSON);
                                }

                                if (selfbay.duplicateSelectedBaySpecStrghtThru() != selfbay.selectedBaySpec().StrghtThru.value) {
                                    txtCondt += "Straight Through changed to <b>" + selfbay.selectedBaySpec().StrghtThru.value + '</b> from ' + selfbay.duplicateSelectedBaySpecStrghtThru() + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'bay_specn', 'columnname': 'strght_thru_ind', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                        'oldcolval': selfbay.duplicateSelectedBaySpecStrghtThru(),
                                        'newcolval': selfbay.selectedBaySpec().StrghtThru.value,
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Straight Thru from ' + selfbay.duplicateSelectedBaySpecStrghtThru() + ' to ' + selfbay.selectedBaySpec().StrghtThru.value + ' on ',
                                        'materialitemid': '0'
                                    };
                                    specChangeArray.push(saveJSON);
                                }

                                if (selfbay.duplicateSelectedBaySpecMidPln() != selfbay.selectedBaySpec().MidPln.value) {
                                    txtCondt += "Mid plane changed to <b>" + selfbay.selectedBaySpec().MidPln.value + '</b> from ' + selfbay.duplicateSelectedBaySpecMidPln() + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'bay_specn', 'columnname': 'mid_pln_ind', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                        'oldcolval': selfbay.duplicateSelectedBaySpecMidPln(),
                                        'newcolval': selfbay.selectedBaySpec().MidPln.value,
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Mid Plane from ' + selfbay.duplicateSelectedBaySpecMidPln() + ' to ' + selfbay.selectedBaySpec().MidPln.value + ' on ',
                                        'materialitemid': '0'
                                    };
                                    specChangeArray.push(saveJSON);
                                }

                                if (selfbay.duplicateSelectedBaySpecWllMnt() != selfbay.selectedBaySpec().WllMnt.value) {
                                    txtCondt += "Wall Mount changed to <b>" + selfbay.selectedBaySpec().WllMnt.value + '</b> from ' + selfbay.duplicateSelectedBaySpecWllMnt() + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'bay_specn', 'columnname': 'wll_mnt_allow_ind', 'audittblpkcolnm': 'bay_specn_id', 'audittblpkcolval': selfbay.selectedBaySpec().Bay.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                        'oldcolval': selfbay.duplicateSelectedBaySpecWllMnt(),
                                        'newcolval': selfbay.selectedBaySpec().WllMnt.value,
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Wall Mount from ' + selfbay.duplicateSelectedBaySpecWllMnt() + ' to ' + selfbay.selectedBaySpec().WllMnt.value + ' on ',
                                        'materialitemid': '0'
                                    };
                                    specChangeArray.push(saveJSON);
                                }
                                if (selfbay.selectedBaySpec().Dltd.bool != selfbay.duplicateSelectedBaySpecDltd) {
                                    txtCondt += "Unusable changed to <b>" + selfbay.selectedBaySpec().Dltd.bool + '</b> from ' + selfbay.duplicateSelectedBaySpecDltd + "<br/>";

                                    var saveJSON = {
                                        'tablename': 'bayspecn_gnrc', 'columnname': 'del_ind', 'audittblpkcolnm': 'bay_specn_revsn_alt_id', 'audittblpkcolval': selfbay.selectedBaySpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                        'oldcolval': selfbay.duplicateSelectedBaySpecDltd,
                                        'newcolval': selfbay.selectedBaySpec().Dltd.bool,
                                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbay.specName() + ' Unusable from ' + selfbay.duplicateSelectedBaySpecDltd + ' to ' + selfbay.selectedBaySpec().Dltd.bool + ' on ',
                                        'materialitemid': '0'
                                    };
                                    specChangeArray.push(saveJSON);
                                }
                            }
                        }

                        var textlength = txtCondt.length;
                        if (selfbay.selectedBaySpec().Bay) {
                            var configNameList = selfbay.getConfigNames(selfbay.selectedBaySpec().Bay.list[0].id.value);
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
                            app.showMessage(txtCondt, 'Update Confirmation for Bay', ['Ok', 'Cancel']).then(function (dialogResult) {
                                if (dialogResult == 'Ok') {
                                    selfbay.saveBaySpec();
                                }
                            });
                        } else {
                            selfbay.saveBaySpec();
                        }
                    }
                    else {
                        $("#interstitial").hide();

                        if (!bayintsectedchk) {
                            $("#BayItnlLstTblDgr").show();
                        }
                    }
                }
            } else {
                $("#interstitial").hide();

                if (!chkgenrc)
                    return app.showMessage('Please associate a material item to the specification before saving.', 'Specification');
            }
        };

        BaySpecification.prototype.saveBaySpec = function () {
            var saveJSON = mapping.toJS(selfbay.selectedBaySpec());

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
                    selfbay.saveAuditChanges();
                    var specWorkToDoId = specResponseOnSuccess.SpecWTD;
                    var mtlWorkToDoId = specResponseOnSuccess.MtlWTD;

                    if (specWorkToDoId !== 0) {
                        var specHelper = new reference();

                        specHelper.getSpecificationSendToNdsStatus(specResponseOnSuccess.Id, specWorkToDoId, 'BAY');
                    }

                    if (mtlWorkToDoId !== 0) {
                        var mtlHelper = new reference();

                        mtlHelper.getSendToNdsStatus(specResponseOnSuccess.MtlItmId, mtlWorkToDoId);
                    }

                    var backMtrlId = selfspec.backToMtlItmId;
                    if (selfspec.selectRadioSpec() == 'newSpec') {
                        selfspec.selectRadioSpec('existSpec');
                        $("#idProductID").val(specResponseOnSuccess.Id);
                        $('#idspectype').val("BAY");
                        selfspec.Searchspec();
                        selfspec.specificationSelected(specResponseOnSuccess.Id, 'BAY', selfspec, backMtrlId);
                        if (backMtrlId > 0) {
                            selfspec.backToMtlItmId = backMtrlId;
                            selfbay.enableBackButton(true);
                        }
                        $("#interstitial").hide();

                        return app.showMessage('Successfully updated specification of type BAY<br><b> having Id ' + specResponseOnSuccess.Id + '</b>', 'Specification');
                    } else {
                        selfspec.selectRadioSpec('existSpec');
                        $("#idProductID").val(specResponseOnSuccess.Id);
                        $('#idspectype').val("BAY");
                        selfspec.updateOnSuccess();
                        if (backMtrlId > 0) {
                            selfspec.backToMtlItmId = backMtrlId;
                            selfbay.enableBackButton(true);
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

            function updateError(err) {
                $("#interstitial").hide();

                if (err && err.responseJSON && err.responseJSON.ExceptionMessage && err.responseJSON.ExceptionMessage.indexOf('BAY_SPECN_GNRC_AK01') > 0) {
                    return app.showMessage('A generic bay already exists with the given external dimensions. External height, width, depth and unit of measure must be unique in order to save.', 'Specification');
                } else if (err && err.responseJSON && err.responseJSON.ExceptionMessage && err.responseJSON.ExceptionMessage.indexOf('The system generated name is greater than 40 characters') > 0) {
                    return app.showMessage('Unable to update the specification. The system generated name is greater than 40 characters. Maximum length of the name is 40 characters.', 'Specification');
                } else
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }
        };

        BaySpecification.prototype.saveAuditChanges = function () {
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

        BaySpecification.prototype.GetModelNameCount = function (modelname, id) {
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

        BaySpecification.prototype.getConfigNames = function (materialItemID) {
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

        BaySpecification.prototype.formSpecName = function (bay, event) {
            var charCode = event.keyCode;
            var name = event.currentTarget.id;
            var value = $('#' + name).val();
            var reqCharAfterDot = 2;

            if (name == "txtHght" || name == "txtWdth" || name == "txtDpth") {
                reqCharAfterDot = 3;
            }

            if (charCode > 31 && (charCode < 48 || charCode > 57) && !(charCode == 46 || charCode == 8)) {
                return false;
            }
            else {
                var len = value.length;
                var index = value.indexOf('.');

                if (index > 0 && charCode == 46) {
                    return false;
                }

                if (index > 0) {
                    var charAfterdot = (len + 1) - index;

                    if (charAfterdot > reqCharAfterDot + 1) {
                        return false;
                    }
                }
            }

            return true;
        };

        BaySpecification.prototype.associatepartBay = function (model, event) {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            selfbay.searchtblbay(false);
            selfbay.associatemtlblck(false);

            document.getElementById('idcdmmsbay').value = "";
            document.getElementById('materialcodebay').value = "";
            document.getElementById('partnumberbay').value = "";
            document.getElementById('clmcbay').value = "";
            document.getElementById('catlogdsptbay').value = "";

            var modal = document.getElementById('bayModalpopup');
            var btn = document.getElementById("idAsscociate");
            var span = document.getElementsByClassName("close")[0];
            var rvsid = selfbay.selectedBaySpec().RvsnId.value;
            var ro = document.getElementById('rcrdlyBay').checked ? "Y" : "N";
            var srcd = "BAY";
            var searchJSON = { RvsnId: rvsid, source: srcd, isRO: ro };

            $.ajax({
                type: "GET",
                url: 'api/specn/getassociatedmtl',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successSearchAssociateddisp,
                error: errorFunc,
                context: selfbay,
                async: false
            });

            function errorFunc() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }

            function successSearchAssociateddisp(data, status) {
                selfbay.enableModalSave(false);

                if (data === 'no_results') {
                    $("#interstitial").hide();
                    //$(".NoRecordrp").show();
                    //setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
                } else {
                    var results = JSON.parse(data);

                    selfbay.associatemtlblck(results);
                    $("#interstitial").hide();
                }
            }

            // When the user clicks the button, open the modal 
            btn.onclick = function () {
                modal.style.display = "block";
            }

            $("#interstitial").hide();
            $("#bayModalpopup").show();

            // When the user clicks on <span> (x), close the modal
            span.onclick = function () {
                modal.style.display = "none";
            }
        };

        BaySpecification.prototype.CancelAssociateBay = function (model, event) {
            selfbay.searchtblbay(false);
            selfbay.associatemtlblck(false);
            document.getElementById('idcdmmsbay').value = "";
            document.getElementById('materialcodebay').value = "";
            document.getElementById('partnumberbay').value = "";
            document.getElementById('clmcbay').value = "";
            document.getElementById('catlogdsptbay').value = "";
            $("#bayModalpopup").hide();
        };

        BaySpecification.prototype.searchmtlbay = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfbay.searchtblbay(false);

            var mtlid = $("#idcdmmsbay").val();
            var mtlcode = $("#materialcodebay").val();
            var partnumb = $("#partnumberbay").val();
            var clmc = $("#clmcbay").val();
            var caldsp = $("#catlogdsptbay").val();
            var ro = document.getElementById('rcrdlyBay').checked ? "Y" : "N";
            var src = "BAY";

            if (mtlid.length > 0 || mtlcode.length > 0 || partnumb.length > 0 || clmc.length > 0 || caldsp.length > 0) {
                var searchJSON = { material_item_id: mtlid, product_id: mtlcode, mfg_part_no: partnumb, mfg_id: clmc, mat_desc: caldsp, source: src, isRo: ro };

                $.ajax({
                    type: "GET",
                    url: 'api/specn/searchmtl',
                    data: searchJSON,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successSearchAssociated,
                    error: errorFunc,
                    context: selfbay,
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
                        selfbay.searchtblbay(results);
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
                $('input.chkbxsearch').on('change', function () {
                    $('input.chkbxsearch').not(this).prop('checked', false);

                    $('input.chkbxsearch').each(function () {
                        if ($(this).prop('checked') == true) {
                            selfbay.enableModalSave(true);

                            return false;
                        }

                        selfbay.enableModalSave(false);
                    });

                    $('input.checkasstspopsearch').each(function () {
                        $(this).prop('checked', false);
                    });
                });
            });

            $(document).ready(function () {
                $('input.checkasstspopsearch').on('change', function () {
                    $('input.chkbxsearch').each(function () {
                        $(this).prop('checked', false);
                    });

                    $('input.checkasstspopsearch').each(function () {
                        if ($(this).prop('checked') == true) {
                            selfbay.enableModalSave(false);

                            return false;
                        }

                        selfbay.enableModalSave(false);
                    });
                });
            });
        };

        BaySpecification.prototype.SaveAssociateBay = function () {
            var chkflag = false;
            var checkBoxes = $("#searchresultbay .chkbxsearch");
            var ids = $("#searchresultbay .idbays");
            var specid = selfbay.selectedBaySpec().RvsnId.value;
            var src = "BAY";
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
                    context: selfbay,
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
                //    context: selfbay,
                //    async: false
                //});
            }

            //var checkBoxesdis = $("#associatedmtl .checkasstspopsearch");
            //var mtlcode = $("#idchknbay").val();
            //var saveJSONde = {
            //    material_rev_id: mtlcode, source: src
            //};
            //var chkflagdis = false;
            //for (var i = 0; i < checkBoxesdis.length; i++) {
            //    if (checkBoxesdis[i].checked == false) {

            //        chkflagdis = true;
            //    }
            //}
            //if (chkflagdis === true) {
            //    $.ajax({
            //        type: "GET",
            //        url: 'api/specn/disassociatepart',
            //        data: saveJSONde,
            //        contentType: "application/json; charset=utf-8",
            //        dataType: "json",
            //        success: successSearchAssociated,
            //        error: errorFunc,
            //        context: selfbay,
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
                    var bayList = JSON.parse(data);
                    var bay = { "list": [bayList] };
                    var name = bay.list[0].Mfg.value + '-' + bay.list[0].PrtNo.value;

                    if (bay != null && bay.list.length > 0) {
                        selfbay.duplicateSelectedBaySpecDpth(bay.list[0].Dpth.value);
                        selfbay.duplicateSelectedBaySpecHght(bay.list[0].Hght.value);
                        selfbay.duplicateSelectedBaySpecWdth(bay.list[0].Wdth.value);
                    }

                    selfbay.specName(name);
                    selfbay.selectedBaySpec().Bay = bay;
                    selfbay.selectedBaySpec().RvsnNm.value = name;
                    selfbay.selectedBaySpec().Nm.value = name;

                    selfbay.selectedBaySpec(selfbay.selectedBaySpec());

                    $("#bayModalpopup").hide();
                    //return app.showMessage('Success!');
                    $("#interstitial").hide();
                }
            }
        };

        BaySpecification.prototype.exportBaySpecReport = function () {
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
            var baySpecHeaders = ["Generic", "Record Only", "Completed", "Propagated", "Unusable in NDS", "Specification ID", "Name", "Description"
                           , "Role Type", "Use Type", "Mounting Position Offset", "Mounting Position UOM", "Max Weight Capacity", "Max Weight UOM", "Model"
                           , "CLEI", "HECI", "Label Start Position", "Manufacturer", "Manufacturer Name", "Part Number", "Material Code", "Material Description"
                           , "Depth", "Height", "Width", "Dimensions Unit of Measurement", "Normal Current Drain", "Unit of Measurement"
                           , "Peak Current Drain", "Unit of Measurement", "Power", "Unit of Measurement", "Planned Heat Generation", "Unit of Measurement", "Bay Weight"
                           , "Unit of Measurement", "Dual SD", "Straight Through", "Wall Mount", "Mid Plane"

            ];							// This array holds the HEADERS text


            for (var i = 0; i < baySpecHeaders.length; i++) {											//  Loop all the haders
                //alert(JSON.parse(selfspec.reptDtls[i]).key);
                excel.set(1, i, 0, baySpecHeaders[i], formatHeader);									//  Set CELL with header text, using header format
                excel.set(1, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }
            //Writing data into sheet 2 for selected specification
            var i = 0;

            var Gnrc = typeof (selfbay.selectedBaySpec().Gnrc) === "undefined" ? "" : selfbay.selectedBaySpec().Gnrc.bool;
            var RO = typeof (selfbay.selectedBaySpec().RO) === "undefined" ? "" : selfbay.selectedBaySpec().RO.bool;
            var Cmplt = typeof (selfbay.selectedBaySpec().Cmplt) === "undefined" ? "" : selfbay.selectedBaySpec().Cmplt.bool;
            var Prpgtd = typeof (selfbay.selectedBaySpec().Prpgtd) === "undefined" ? "" : selfbay.selectedBaySpec().Prpgtd.bool;
            var Dltd = typeof (selfbay.selectedBaySpec().Dltd) === "undefined" ? "" : selfbay.selectedBaySpec().Dltd.bool;

            excel.set(1, 0, i + 1, Gnrc);                                                               // Generic
            excel.set(1, 1, i + 1, RO);                                                                 // Record Only
            excel.set(1, 2, i + 1, Cmplt);                                                              // Completed
            excel.set(1, 3, i + 1, Prpgtd);                                                             // Propagated
            excel.set(1, 4, i + 1, Dltd);                                                               // Unusable in NDS
            excel.set(1, 5, i + 1, selfbay.selectedBaySpec().id.value);                                 // Specification ID
            excel.set(1, 6, i + 1, selfbay.specName());                                                 // Name
            excel.set(1, 7, i + 1, selfbay.selectedBaySpec().Desc.value);                               // Description

            var indInt = selfbay.selectedBaySpec().BayRlTypId.options.map(function (img) { return img.value; }).indexOf(selfbay.selectedBaySpec().BayRlTypId.value);
            excel.set(1, 8, i + 1, selfbay.selectedBaySpec().BayRlTypId.options[indInt].text);          // Role Type
            excel.set(1, 9, i + 1, selfbay.selectedBaySpec().BayUseTypId.value);                        // Use Type
            excel.set(1, 10, i + 1, selfbay.selectedBaySpec().MntngPosOfst.value);                      // Mounting Position Offset
            var indInt = selfbay.selectedBaySpec().DimUom.options.map(function (img) { return img.value; }).indexOf(selfbay.selectedBaySpec().DimUom.value);
            excel.set(1, 11, i + 1, selfbay.selectedBaySpec().DimUom.options[indInt].text);             // Mounting Position UOM

            excel.set(1, 12, i + 1, selfbay.selectedBaySpec().MxWght.value);                            // Max Weight Capacity

            var indInt = selfbay.selectedBaySpec().MxWghtUom.options.map(function (img) { return img.value; }).indexOf(selfbay.selectedBaySpec().MxWghtUom.value);
            excel.set(1, 13, i + 1, selfbay.selectedBaySpec().MxWghtUom.options[indInt].text);          // Max Weight UOM

            excel.set(1, 14, i + 1, selfbay.selectedBaySpec().Nm.value);                                 // Model
            excel.set(1, 15, i + 1, $("#cleitext").text());                                              // CLEI   
            excel.set(1, 16, i + 1, $("#hecitext").text());                                              // HECI
            var lblPos = selfbay.selectedStrtLbl() === "B" ? "Bottom" : "Top";
            excel.set(1, 17, i + 1, lblPos);                                                            // Label Start Position
            excel.set(1, 18, i + 1, selfbay.selectedBaySpec().Bay.list[0].Mfg.value);                   // Manufacturer
            excel.set(1, 19, i + 1, selfbay.selectedBaySpec().Bay.list[0].MfgNm.value);                 // Manufacturer Name

            excel.set(1, 20, i + 1, selfbay.selectedBaySpec().Bay.list[0].PrtNo.value);                 // Part Number
            excel.set(1, 21, i + 1, selfbay.selectedBaySpec().Bay.list[0].PrdctId.value);               // Material Code
            excel.set(1, 22, i + 1, selfbay.selectedBaySpec().Bay.list[0].ItmDesc.value);               // Material Description
            excel.set(1, 23, i + 1, selfbay.selectedBaySpec().Bay.list[0].Dpth.value);                  // Depth
            excel.set(1, 24, i + 1, selfbay.selectedBaySpec().Bay.list[0].Hght.value);                  // Height
            excel.set(1, 25, i + 1, selfbay.selectedBaySpec().Bay.list[0].Wdth.value);                  // Width

            var indInt = selfbay.selectedBaySpec().Bay.list[0].DimUom.options.map(function (img) { return img.value; }).indexOf(selfbay.selectedBaySpec().Bay.list[0].DimUom.value);

            excel.set(1, 26, i + 1, selfbay.selectedBaySpec().Bay.list[0].DimUom.options[indInt].text); // Dimensions Unit of Measurement
            excel.set(1, 27, i + 1, selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnNrm.value);         // Normal Current Drain

            var indInt = selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnNrmUom.options.map(function (img) { return img.value; }).indexOf(selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnNrmUom.value);
            excel.set(1, 28, i + 1, selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnNrmUom.options[indInt].text);   // Unit of Measurement
            excel.set(1, 29, i + 1, selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnMx.value);                      // Peak Current Drain

            var indInt = selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnMxUom.options.map(function (img) { return img.value; }).indexOf(selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnMxUom.value);
            excel.set(1, 30, i + 1, selfbay.selectedBaySpec().Bay.list[0].ElcCurrDrnMxUom.options[indInt].text);    // Unit of Measurement
            excel.set(1, 31, i + 1, selfbay.selectedBaySpec().Bay.list[0].HtDssptn.value);                          // Power

            var indInt = selfbay.selectedBaySpec().Bay.list[0].HtDssptnUom.options.map(function (img) { return img.value; }).indexOf(selfbay.selectedBaySpec().Bay.list[0].HtDssptnUom.value);
            excel.set(1, 32, i + 1, selfbay.selectedBaySpec().Bay.list[0].HtDssptnUom.options[indInt].text);        // Unit of Measurement

            excel.set(1, 33, i + 1, selfbay.selectedBaySpec().Bay.list[0].HtGntn.value);                            // Planned Heat Generation 
            var indInt = selfbay.selectedBaySpec().Bay.list[0].HtGntnUom.options.map(function (img) { return img.value; }).indexOf(selfbay.selectedBaySpec().Bay.list[0].HtGntnUom.value);
            excel.set(1, 34, i + 1, selfbay.selectedBaySpec().Bay.list[0].HtGntnUom.options[indInt].text);        // Unit of Measurement   

            excel.set(1, 35, i + 1, selfbay.selectedBaySpec().Bay.list[0].Wght.value);                              // Bay Weight
            var indInt = selfbay.selectedBaySpec().Bay.list[0].WghtUom.options.map(function (img) { return img.value; }).indexOf(selfbay.selectedBaySpec().Bay.list[0].WghtUom.value);
            excel.set(1, 36, i + 1, selfbay.selectedBaySpec().Bay.list[0].WghtUom.options[indInt].text);            // Unit of Measurement

            excel.set(1, 37, i + 1, selfbay.selectedBaySpec().DualSd.value);                                        // Dual SD         
            excel.set(1, 38, i + 1, selfbay.selectedBaySpec().StrghtThru.value);                                    // Straight Through
            excel.set(1, 39, i + 1, selfbay.selectedBaySpec().WllMnt.value);                                        // Wall Mount     
            excel.set(1, 40, i + 1, selfbay.selectedBaySpec().MidPln.value);                                        // Mid Plane


            //End of writing bay specification data into sheet 2

            // Start writng Associtaed Internal Bay specifcation in sheet 3

            excel.addSheet("Bay Internal Specifications");                              //Add sheet 3 for selected specification id details 


            var assoclheaders = ["Name", "Description", "Height", "Height UOM", "Width", "Width UOM", "Depth", "Depth UOM"
                                , "Mounting Position Quantity", "Mounting Position Distance", "Wall Mount", "Straight Through"
                                , "Mid Plane", "Nominal"
            ];							// This array holds the HEADERS text
   
            for (var i = 0; i < assoclheaders.length; i++) {									// Loop all the haders
                excel.set(2, i, 0, assoclheaders[i], formatHeader);							// Set CELL with header text, using header format
                excel.set(2, i, undefined, "auto");											// Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }

            if (selfbay.Bayinterlsdetails() != false) {
                if (selfbay.Bayinterlsdetails().length > 0) {
                    for (var i = 0; i < selfbay.Bayinterlsdetails().length; i++) {
                        excel.set(2, 0, i + 1, selfbay.Bayinterlsdetails()[i].Nm.value);                // Name
                        excel.set(2, 1, i + 1, selfbay.Bayinterlsdetails()[i].Desc.value);              // Description
                        excel.set(2, 2, i + 1, selfbay.Bayinterlsdetails()[i].BayItnlHght.value);       // Height
                        excel.set(2, 3, i + 1, selfbay.Bayinterlsdetails()[i].BayItnlHghtUom.value);    // Height UOM
                        excel.set(2, 4, i + 1, selfbay.Bayinterlsdetails()[i].BayItnlWdth.value);       // Width
                        excel.set(2, 5, i + 1, selfbay.Bayinterlsdetails()[i].BayItnlWdthUom.value);    // Width UOM
                        excel.set(2, 6, i + 1, selfbay.Bayinterlsdetails()[i].BayItnlDpth.value);       // Depth
                        excel.set(2, 7, i + 1, selfbay.Bayinterlsdetails()[i].BayItnlDpthUom.value);    // Depth UOM
                        excel.set(2, 8, i + 1, selfbay.Bayinterlsdetails()[i].MntngPosQty.value);       // Mounting Position Quantity
                        excel.set(2, 9, i + 1, selfbay.Bayinterlsdetails()[i].MntngPosDist.value);      // Mounting Position Distance
                        excel.set(2, 10, i + 1, selfbay.Bayinterlsdetails()[i].BayIntlWllMnt.value);    // Wall Mount
                        excel.set(2, 11, i + 1, selfbay.Bayinterlsdetails()[i].BayIntlStrghtThru.value);// Straight Through
                        excel.set(2, 12, i + 1, selfbay.Bayinterlsdetails()[i].BayIntlMidPln.value);    // Mid Plane
                        excel.set(2, 13, i + 1, selfbay.Bayinterlsdetails()[i].Nmnl.value);             // Nominal
                    }
                }
            }

            // End of writng Associtaed Internal Bay specifcation in sheet 3

            //fetch the associated material details
            selfbay.associatepartBay();
            $("#bayModalpopup").hide();
            //Start writing associate details into sheet 4
            excel.addSheet("Search result of Associated Material");                 //Add sheet 2 for selected specification id details 
            var assoSearchcHeaders = ["CDMMS ID", "Material Code", "CLMC", "Part Number", "Catalog/Material Description"];							// This array holds the HEADERS text


            for (var i = 0; i < assoSearchcHeaders.length; i++) {											//  Loop all the haders               
                excel.set(3, i, 0, assoSearchcHeaders[i], formatHeader);									//  Set CELL with header text, using header format
                excel.set(3, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }

            var i = 0;

            if (selfbay.searchtblbay() != false) {
                if (selfbay.searchtblbay().length > 0) {
                    excel.set(3, 0, i + 1, selfbay.searchtblbay()[i].material_item_id.value);        // CDMMS ID
                    excel.set(3, 1, i + 1, selfbay.searchtblbay()[i].product_id.value);              // Material Code
                    excel.set(3, 2, i + 1, selfbay.searchtblbay()[i].mfg_id.value);                  // CLMC
                    excel.set(3, 3, i + 1, selfbay.searchtblbay()[i].mfg_part_no.value);             // Part Number
                    excel.set(3, 4, i + 1, selfbay.searchtblbay()[i].item_desc.value);               // Catalog/Material Description
                }
            }
            //End of writing associate details into sheet 4

            //Start writing associate details into sheet 5
            excel.addSheet("Associated Material");                 
            var assoSearchcHeaders = ["CDMMS ID", "Material Code", "CLMC", "Part Number", "Catalog/Material Description"];							// This array holds the HEADERS text


            for (var i = 0; i < assoSearchcHeaders.length; i++) {											//  Loop all the haders               
                excel.set(4, i, 0, assoSearchcHeaders[i], formatHeader);									//  Set CELL with header text, using header format
                excel.set(4, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }

            var i = 0;

            if (selfbay.associatemtlblck() != false) {
                if (selfbay.associatemtlblck().length > 0) {
                    excel.set(4, 0, i + 1, selfbay.associatemtlblck()[i].material_item_id.value);        // CDMMS ID
                    excel.set(4, 1, i + 1, selfbay.associatemtlblck()[i].product_id.value);              // Material Code
                    excel.set(4, 2, i + 1, selfbay.associatemtlblck()[i].mfg_id.value);                  // CLMC
                    excel.set(4, 3, i + 1, selfbay.associatemtlblck()[i].mfg_part_no.value);             // Part Number
                    excel.set(4, 4, i + 1, selfbay.associatemtlblck()[i].item_desc.value);               // Catalog/Material Description
                }
            }
            //End of writing associate details into sheet 5

            excel.generate("Report_Bay_Specification.xlsx");

        };

        return BaySpecification;
    });