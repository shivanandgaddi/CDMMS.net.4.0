define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', '../Utility/referenceDataHelper', '../Utility/user'],
    function (composition, app, ko, $, http, activator, mapping, system, reference, user) {
    var BayInternalSpecification = function (data) {
        selfbayInternal = this;
        specChangeArray = [];
        var results = JSON.parse(data);
        
        if(results.BayItnlWdthLst != null && results.BayItnlWdthLst.list.length > 0){
            for (var i = 0 ; i < results.BayItnlWdthLst.list.length;i++ ){
                results.BayItnlWdthLst.list[i].BayItnlWdth.value = Number(results.BayItnlWdthLst.list[i].BayItnlWdth.value).toFixed(3);
                // Ensure there is a leading zero when a fraction like .75 comes across.  should display as 0.75
                if (results.BayItnlWdthLst.list[i].BayItnlWdth.value < 1 && results.BayItnlWdthLst.list[i].BayItnlWdth.value > 0 && results.BayItnlWdthLst.list[i].BayItnlWdth.value.substring(0, 1) == '.') {
                    results.BayItnlWdthLst.list[i].BayItnlWdth.value = '0' + results.BayItnlWdthLst.list[i].BayItnlWdth.value;
                }
            }
        }
        if(results.BayItnlDpthLst != null && results.BayItnlDpthLst.list.length > 0){
            for (var i = 0 ; i < results.BayItnlDpthLst.list.length; i++) {
                results.BayItnlDpthLst.list[i].BayItnlDpth.value = Number(results.BayItnlDpthLst.list[i].BayItnlDpth.value).toFixed(3);
                // Ensure there is a leading zero when a fraction like .75 comes across.  should display as 0.75
                if (results.BayItnlDpthLst.list[i].BayItnlDpth.value < 1 && results.BayItnlDpthLst.list[i].BayItnlDpth.value > 0 && results.BayItnlDpthLst.list[i].BayItnlDpth.value.substring(0, 1) == '.') {
                    results.BayItnlDpthLst.list[i].BayItnlDpth.value = '0' + results.BayItnlDpthLst.list[i].BayItnlDpth.value;
                }
            }
        }

        selfbayInternal.selectedBayIntlSpec = ko.observable();
        selfbayInternal.selectedBayIntlSpec(results);
        selfbayInternal.completedNotSelected = ko.observable(true);
        selfbayInternal.specName = ko.observable('');
        selfbayInternal.enableName = ko.observable(false);
        selfbayInternal.selectedDepth = ko.observableArray('');

        selfbayInternal.duplicateSelectedBayIntlSpecStrghtThru = ko.observable();
        selfbayInternal.duplicateSelectedBayIntlSpecMidPln = ko.observable();
        selfbayInternal.duplicateSelectedBayIntlSpecWllMnt = ko.observable();
        selfbayInternal.duplicateSelectedBayIntlSpecDpthNbr = ko.observable();
        selfbayInternal.duplicateSelectedBayIntlSpecDpth = ko.observable();
        selfbayInternal.duplicateSelectedBayIntlSpecWdthNbr = ko.observable();
        selfbayInternal.duplicateSelectedBayIntlSpecWdth = ko.observable();
        selfbayInternal.duplicateSelectedBayIntlSpecDltd = ko.observable();
        selfbayInternal.duplicateSelectedBayIntlSpecDltd = results.Dltd.bool;

        if (results.BayIntlStrghtThru != null) {
            selfbayInternal.duplicateSelectedBayIntlSpecStrghtThru(results.BayIntlStrghtThru.value);
        }

        if (results.BayIntlMidPln != null) {
            selfbayInternal.duplicateSelectedBayIntlSpecMidPln(results.BayIntlMidPln.value);
        }

        if (results.BayIntlWllMnt != null) {
            selfbayInternal.duplicateSelectedBayIntlSpecWllMnt(results.BayIntlWllMnt.value);
        }
        if (results.BayItnlDpthLst != null) {
            selfbayInternal.duplicateSelectedBayIntlSpecDpthNbr.value = results.BayItnlDpthLst.list.length;
            selfbayInternal.duplicateSelectedBayIntlSpecDpth = selfbayInternal.selectedBayIntlSpec().BayItnlDpthId.value;
        }
        if (results.BayItnlWdthLst != null) {
            selfbayInternal.duplicateSelectedBayIntlSpecWdthNbr.value = results.BayItnlWdthLst.list.length;
            selfbayInternal.duplicateSelectedBayIntlSpecWdth = selfbayInternal.selectedBayIntlSpec().BayItnlWdthId.value;
        }

        if (selfbayInternal.selectedBayIntlSpec().Cmplt.bool == true && selfbayInternal.selectedBayIntlSpec().Prpgtd.enable == true) {
            selfbayInternal.completedNotSelected(false);
        }

        if( selfspec.selectRadioSpec() == 'newSpec'){
            selfbayInternal.selectedBayIntlSpec().MntngPosQty.value = '';
            selfbayInternal.selectedBayIntlSpec().BayIntlStrghtThru.value = "Y";
        }

        if (selfbayInternal.selectedBayIntlSpec().Nm.value != "") {
            selfbayInternal.specName(selfbayInternal.selectedBayIntlSpec().Nm.value);
        } else {
            selfbayInternal.specName('BI-');
        }

        $(document).on('change', '[type=checkbox]', function () {
            if (this.name == "bayintnmcmpt") {
                if (this.checked == false) {
                    document.getElementById('BayIntlPrpgtdChbk').checked = false;
                }
            }
        });
        setTimeout(function () {
            $(document).ready(function () {
                $('input.BayItnlDpthLstCbk').on('change', function () {
                    $('input.BayItnlDpthLstCbk').not(this).prop('checked', false);                  
                    selfbayInternal.selectedBayIntlSpec().BayItnlDpthId.value = $(this).val();                  
                    $("#BayItnlDpthLstTblDgr").hide();
                });
                $('input.BayItnlWdthLstCbk').on('change', function () {
                    $('input.BayItnlWdthLstCbk').not(this).prop('checked', false);
                    selfbayInternal.selectedBayIntlSpec().BayItnlWdthId.value = $(this).val();
                    $("#BayItnlWdthLstTblDgr").hide();
                });
                
            });
        }, 2000);

        $.ajax({
            type: "GET",
            url: 'api/reference/DimUom',          
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: successFunc,
            error: errorFunc,
            context: selfbayInternal
        });
        function successFunc(data, status) {

            if (data == 'no_results') {
                $("#interstitial").hide();
                //app.showMessage("No records found", 'Failed');
            }
            else {
                $("#interstitial").hide();
                selfbayInternal.selectedDepth(JSON.parse(data));
                $('#depthuomnew').val('22');
                $('#widthnewuom').val('22');
            } 
        }
        function errorFunc() {
            $("#interstitial").hide();
            alert('error');
        }
        $("#baydepthnew").hide();
        $("#baywidthnew").hide();
        $('#depthuomnew').val('22');
        $('#widthnewuom').val('22');
    };

    BayInternalSpecification.prototype.onchangeBayIntlCompleted = function () {
        if ($("#BayIntlCmpltChbk").is(':checked')) {
            selfbayInternal.completedNotSelected(false);
        } else {
            selfbayInternal.completedNotSelected(true);
        }
    };

    BayInternalSpecification.prototype.Adddepth = function () {
        $("#baydepthnew").hide();
        $("#baywidthnew").hide();
        $("#baydepthexists").hide();
        $("#baywidthexists").hide();

        if ((document.getElementById('dpthnew').value !== '') && (document.getElementById('depthuomnew').value !== '')) {
            var dpthnew = $('#dpthnew').val();
            var dpthuom = $('#depthuomnew').val();
            var exists = false;

            for (var i = 0; i < selfbayInternal.selectedBayIntlSpec().BayItnlDpthLst.list.length; i++) {
                if (dpthnew === selfbayInternal.selectedBayIntlSpec().BayItnlDpthLst.list[i].BayItnlDpth.value && dpthuom === selfbayInternal.selectedBayIntlSpec().BayItnlDpthLst.list[i].BayItnlDpthUomId.value) {
                    exists = true;
                    break;
                }
            }

            if (exists) {
                $("#baydepthexists").show();
            } else {
                var addJSON = { "dimension": dpthnew, "uom": dpthuom };
                var tablenm = "bay_itnl_dpth";

                addJSON = JSON.stringify(addJSON);

                $.ajax({
                    type: "POST",
                    url: 'api/specn/insertdimension/' + tablenm,
                    data: addJSON,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successFunc,
                    error: errorFunc,
                    context: selfbayInternal
                });

                function successFunc(data, status) {
                    if (data == 'no_results') {
                        $("#interstitial").hide();
                        //app.showMessage("No records found", 'Failed');
                    }
                    else {
                        $("#interstitial").hide();

                        $.ajax({
                            type: "GET",
                            url: 'api/specn/getdimensions/' + tablenm,
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            success: successFunc,
                            error: errorFunc,
                            context: selfbayInternal
                        });

                        function successFunc(data, status) {

                            if (data == 'no_results') {
                                $("#interstitial").hide();
                                //app.showMessage("No records found", 'Failed');
                            }
                            else {
                                $("#interstitial").hide();
                                app.showMessage("Successfully added new bay internal depth!", 'Specification');
                                document.getElementById('dpthnew').value = "";
                                document.getElementById('depthuomnew').value = "";

                                $("#baydepthnew").hide();
                                $("#baywidthnew").hide();
                                $("#baydepthexists").hide();
                                $("#baywidthexists").hide();
                                $('#depthuomnew').val('22');
                                $('#widthnewuom').val('22');
                                var dpetnew = JSON.parse(data);
                                if (dpetnew != null && dpetnew.list.length > 0) {
                                    for (var i = 0 ; i < dpetnew.list.length; i++) {
                                        dpetnew.list[i].BayItnlDpth.value = Number(dpetnew.list[i].BayItnlDpth.value).toFixed(3);
                                    }
                                }
                                selfbayInternal.selectedBayIntlSpec().BayItnlDpthLst.list = dpetnew.list;
                                selfbayInternal.selectedBayIntlSpec(selfbayInternal.selectedBayIntlSpec());
                            }
                        }

                        function errorFunc() {
                            $("#interstitial").hide();
                            alert('error');
                        }
                    }
                }

                function errorFunc() {
                    $("#interstitial").hide();
                    alert('error');
                }
            }
        }
        else {
            $("#baydepthnew").show();
        }
    };

    BayInternalSpecification.prototype.Addwidth = function () {
        $("#baywidthnew").hide();
        $("#baydepthnew").hide();
        $("#baydepthexists").hide();
        $("#baywidthexists").hide();

        if ((document.getElementById('wdthnew').value !== '') && (document.getElementById('widthnewuom').value !== '')) {
            var widthnewadd = $('#wdthnew').val();
            var widthnewdropdwn = $('#widthnewuom').val();

            var exists = false;

            for (var i = 0; i < selfbayInternal.selectedBayIntlSpec().BayItnlWdthLst.list.length; i++) {
                if (widthnewadd === selfbayInternal.selectedBayIntlSpec().BayItnlWdthLst.list[i].BayItnlWdth.value && widthnewdropdwn === selfbayInternal.selectedBayIntlSpec().BayItnlWdthLst.list[i].BayItnlWdthUomId.value) {
                    exists = true;
                    break;
                }
            }

            if (exists) {
                $("#baywidthexists").show();
            } else {
                var addJSON = { "dimension": widthnewadd, "uom": widthnewdropdwn };
                var tablenm = "bay_itnl_wdth";

                addJSON = JSON.stringify(addJSON);

                $.ajax({
                    type: "POST",
                    url: 'api/specn/insertdimension/' + tablenm,
                    data: addJSON,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successFunc,
                    error: errorFunc,
                    context: selfbayInternal
                });
                function successFunc(data, status) {

                    if (data == 'no_results') {
                        $("#interstitial").hide();
                        //app.showMessage("No records found", 'Failed');
                    }
                    else {
                        $("#interstitial").hide();

                        $.ajax({
                            type: "GET",
                            url: 'api/specn/getdimensions/' + tablenm,
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            success: successFunc,
                            error: errorFunc,
                            context: selfbayInternal
                        });
                        function successFunc(data, status) {

                            if (data == 'no_results') {
                                $("#interstitial").hide();
                                //app.showMessage("No records found", 'Failed');
                            }
                            else {
                                $("#interstitial").hide();
                                app.showMessage("Successfully added new bay internal width!", 'Specification');
                                document.getElementById('wdthnew').value = "";
                                document.getElementById('widthnewuom').value = "";

                                $("#baywidthnew").hide();
                                $("#baydepthnew").hide();
                                $("#baydepthexists").hide();
                                $("#baywidthexists").hide();
                                $('#depthuomnew').val('22');
                                $('#widthnewuom').val('22');
                                var widthnew = JSON.parse(data);
                                if (widthnew != null && widthnew.list.length > 0) {
                                    for (var i = 0 ; i < widthnew.list.length; i++) {
                                        widthnew.list[i].BayItnlWdth.value = Number(widthnew.list[i].BayItnlWdth.value).toFixed(3);
                                    }
                                }
                                selfbayInternal.selectedBayIntlSpec().BayItnlWdthLst.list = widthnew.list;
                                selfbayInternal.selectedBayIntlSpec(selfbayInternal.selectedBayIntlSpec());
                            }
                        }
                        function errorFunc() {
                            $("#interstitial").hide();
                            alert('error');
                        }

                    }
                }
                function errorFunc() {
                    $("#interstitial").hide();
                    alert('error');
                }
            }
        }
        else {
            $("#baywidthnew").show();
        }
    }; 
    BayInternalSpecification.prototype.updateBayInternal = function () {
        specChangeArray = [];
        console.log(selfbayInternal.selectedBayIntlSpec());
        $("#interstitial").css("height", "100%");
        $("#interstitial").show();

        var depthcheckboxSelected = false;
        var widthcheckboxSelected = false;
      
        var bayItnlDpthLstCbk = $("#BayItnlDpthLstTbl .BayItnlDpthLstCbk");

        for (var i = 0; i < bayItnlDpthLstCbk.length; i++) {
            if (bayItnlDpthLstCbk[i].checked == true) {
                depthcheckboxSelected = true;
            }
        }

        var bayItnlDpthLstCbk = $("#BayItnlWdthLstTbl .BayItnlWdthLstCbk");

        for (var i = 0; i < bayItnlDpthLstCbk.length; i++) {
            if (bayItnlDpthLstCbk[i].checked == true) {
                widthcheckboxSelected = true;
            }
        }
       
        if (depthcheckboxSelected && widthcheckboxSelected) {
            selfbayInternal.bayInternalCheckBoxCheck();

            selfbayInternal.selectedBayIntlSpec().Nm.value = selfbayInternal.specName();

            var txtCondt = '';
            if (selfspec.selectRadioSpec() == 'existSpec') {
                if (selfbayInternal.duplicateSelectedBayIntlSpecStrghtThru() != selfbayInternal.selectedBayIntlSpec().BayIntlStrghtThru.value) {
                    txtCondt += "Straight Through changed to <b>" + selfbayInternal.selectedBayIntlSpec().BayIntlStrghtThru.value + '</b> from ' + selfbayInternal.duplicateSelectedBayIntlSpecStrghtThru() + "<br/>";

                    var saveJSON = {
                        'tablename': 'bay_itnl', 'columnname': 'strght_thru_ind', 'audittblpkcolnm': 'bay_itnl_id', 'audittblpkcolval': selfbayInternal.selectedBayIntlSpec().id.value, 'auditprnttblpkcolnm': '',
                        'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': selfbayInternal.duplicateSelectedBayIntlSpecStrghtThru(), 'newcolval': selfbayInternal.selectedBayIntlSpec().BayIntlStrghtThru.value, 'cuid': user.cuid,
                        'cmnttxt': user.cuid + ' changed ' + selfbayInternal.specName() + ' Straight Through from ' + selfbayInternal.duplicateSelectedBayIntlSpecStrghtThru() + ' to ' + selfbayInternal.selectedBayIntlSpec().BayIntlStrghtThru.value + ' on ',
                        'materialitemid': 0
                    };
                    specChangeArray.push(saveJSON);
                }
                if (selfbayInternal.duplicateSelectedBayIntlSpecMidPln() != selfbayInternal.selectedBayIntlSpec().BayIntlMidPln.value) {
                    txtCondt += "Mid Plane changed to <b>" + selfbayInternal.selectedBayIntlSpec().BayIntlMidPln.value + '</b> from ' + selfbayInternal.duplicateSelectedBayIntlSpecMidPln() + "<br/>";

                    var saveJSON = {
                        'tablename': 'bay_itnl', 'columnname': 'mid_pln_ind', 'audittblpkcolnm': 'bay_itnl_id', 'audittblpkcolval': selfbayInternal.selectedBayIntlSpec().id.value, 'auditprnttblpkcolnm': '',
                        'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': selfbayInternal.duplicateSelectedBayIntlSpecMidPln(), 'newcolval': selfbayInternal.selectedBayIntlSpec().BayIntlMidPln.value, 'cuid': user.cuid,
                        'cmnttxt': user.cuid + ' changed ' + selfbayInternal.specName() + ' Mid Plane from ' + selfbayInternal.duplicateSelectedBayIntlSpecMidPln() + ' to ' + selfbayInternal.selectedBayIntlSpec().BayIntlMidPln.value + ' on ',
                        'materialitemid': 0
                    };
                    specChangeArray.push(saveJSON);
                }
                if (selfbayInternal.duplicateSelectedBayIntlSpecWllMnt() != selfbayInternal.selectedBayIntlSpec().BayIntlWllMnt.value) {
                    txtCondt += "Wall Mount changed to <b>" + selfbayInternal.selectedBayIntlSpec().BayIntlWllMnt.value + '</b> from ' + selfbayInternal.duplicateSelectedBayIntlSpecWllMnt() + "<br/>";

                    var saveJSON = {
                        'tablename': 'bay_itnl', 'columnname': 'wll_mnt_allow_ind', 'audittblpkcolnm': 'bay_itnl_id', 'audittblpkcolval': selfbayInternal.selectedBayIntlSpec().id.value, 'auditprnttblpkcolnm': '',
                        'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': selfbayInternal.duplicateSelectedBayIntlSpecWllMnt(), 'newcolval': selfbayInternal.selectedBayIntlSpec().BayIntlWllMnt.value, 'cuid': user.cuid,
                        'cmnttxt': user.cuid + ' changed ' + selfbayInternal.specName() + ' Wall Mount from ' + selfbayInternal.duplicateSelectedBayIntlSpecWllMnt() + ' to ' + selfbayInternal.selectedBayIntlSpec().BayIntlWllMnt.value + ' on ',
                        'materialitemid': 0
                    };
                    specChangeArray.push(saveJSON);
                }
                if (selfbayInternal.selectedBayIntlSpec().BayItnlDpthLst.list.length != selfbayInternal.duplicateSelectedBayIntlSpecDpthNbr.value) {
                    txtCondt += "Depth List changed to " + selfbayInternal.selectedBayIntlSpec().BayItnlDpthLst.list.length + " entries from " + selfbayInternal.duplicateSelectedBayIntlSpecDpthNbr.value;
                }
                if (selfbayInternal.duplicateSelectedBayIntlSpecDpth != selfbayInternal.selectedBayIntlSpec().BayItnlDpthId.value) {
                    var oldNumber = GetNumber(selfbayInternal.duplicateSelectedBayIntlSpecDpth, 'D');  // for display and auditing
                    var newNumber = GetNumber(selfbayInternal.selectedBayIntlSpec().BayItnlDpthId.value, 'D');  // for display and auditing
                    txtCondt += "Depth changed to <b>" + Number(newNumber).toFixed(3) + "</b> from " + Number(oldNumber).toFixed(3) + "<br/>";

                    var saveJSON = {
                        'tablename': 'bay_itnl', 'columnname': 'bay_itnl_dpth_id', 'audittblpkcolnm': 'bay_itnl_id', 'audittblpkcolval': selfbayInternal.selectedBayIntlSpec().id.value, 'auditprnttblpkcolnm': '',
                        'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(oldNumber).toFixed(3), 'newcolval': Number(newNumber).toFixed(3), 'cuid': user.cuid,
                        'cmnttxt': user.cuid + ' changed ' + selfbayInternal.specName() + ' Depth from ' + Number(oldNumber).toFixed(3) + ' to ' + Number(newNumber).toFixed(3) + ' on ',
                        'materialitemid': 0
                    };
                    specChangeArray.push(saveJSON);
                }
                if (selfbayInternal.selectedBayIntlSpec().BayItnlWdthLst.list.length != selfbayInternal.duplicateSelectedBayIntlSpecWdthNbr.value) {
                    txtCondt += "Width List changed to " + selfbayInternal.selectedBayIntlSpec().BayItnlWdthLst.list.length + " entries from " + selfbayInternal.duplicateSelectedBayIntlSpecWdthNbr.value;
                }
                if (selfbayInternal.duplicateSelectedBayIntlSpecWdth != selfbayInternal.selectedBayIntlSpec().BayItnlWdthId.value) {
                    var oldNumber = GetNumber(selfbayInternal.duplicateSelectedBayIntlSpecWdth, 'W');  // for display and auditing
                    var newNumber = GetNumber(selfbayInternal.selectedBayIntlSpec().BayItnlWdthId.value, 'W');  // for display and auditing
                    txtCondt += "Width changed to <b>" + Number(newNumber).toFixed(3) + "</b> from " + Number(oldNumber).toFixed(3) + "<br/>";

                    var saveJSON = {
                        'tablename': 'bay_itnl', 'columnname': 'bay_itnl_wdth_id', 'audittblpkcolnm': 'bay_itnl_id', 'audittblpkcolval': selfbayInternal.selectedBayIntlSpec().id.value, 'auditprnttblpkcolnm': '',
                        'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(oldNumber).toFixed(3), 'newcolval': Number(newNumber).toFixed(3), 'cuid': user.cuid,
                        'cmnttxt': user.cuid + ' changed ' + selfbayInternal.specName() + ' Width from ' + Number(oldNumber).toFixed(3) + ' to ' + Number(newNumber).toFixed(3) + ' on ',
                        'materialitemid': 0
                    };
                    specChangeArray.push(saveJSON);
                }
                if (selfbayInternal.selectedBayIntlSpec().Dltd.bool != selfbayInternal.duplicateSelectedBayIntlSpecDltd) {
                    txtCondt += "Unusable changed to <b>" + selfbayInternal.selectedBayIntlSpec().Dltd.bool + '</b> from ' + selfbayInternal.duplicateSelectedBayIntlSpecDltd + "<br/>";

                    var saveJSON = {
                        'tablename': 'bay_itnl', 'columnname': 'del_ind', 'audittblpkcolnm': 'bay_itnl_id', 'audittblpkcolval': selfbayInternal.selectedBayIntlSpec().id.value, 'auditprnttblpkcolnm': '',
                        'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': selfbayInternal.duplicateSelectedBayIntlSpecDltd, 'newcolval': selfbayInternal.selectedBayIntlSpec().Dltd.bool, 'cuid': user.cuid,
                        'cmnttxt': user.cuid + ' changed ' + selfbayInternal.specName() + ' Unusable from ' + selfbayInternal.duplicateSelectedBayIntlSpecDltd + ' to ' + selfbayInternal.selectedBayIntlSpec().Dltd.bool + ' on ',
                        'materialitemid': 0
                    };
                    specChangeArray.push(saveJSON);
                }
            }

            $("#interstitial").hide();

            if (txtCondt.length > 0) {
                app.showMessage(txtCondt, 'Update Confirmation for Bay Internal', ['Ok', 'Cancel']).then(function (dialogResult) {
                    if (dialogResult == 'Ok') {
                        selfbayInternal.saveBayInternalSpec();
                    }
                });
            } else {
                selfbayInternal.saveBayInternalSpec();
            }
        } else {
            $("#interstitial").hide();
            if (!depthcheckboxSelected) {
                $("#BayItnlDpthLstTblDgr").show();
            }
            if (!widthcheckboxSelected) {
                $("#BayItnlWdthLstTblDgr").show();
            }
        }
    };

    BayInternalSpecification.prototype.saveBayInternalSpec = function () {
        var saveJSON = mapping.toJS(selfbayInternal.selectedBayIntlSpec());

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
                selfbayInternal.saveAuditChanges();
                var specWorkToDoId = specResponseOnSuccess.SpecWTD;

                if (specWorkToDoId !== 0) {
                    var specHelper = new reference();

                    specHelper.getSpecificationSendToNdsStatus(specResponseOnSuccess.Id, specWorkToDoId, 'BAY_INTERNAL');
                }

                if (selfspec.selectRadioSpec() == 'newSpec') {
                    selfspec.selectRadioSpec('existSpec');
                    $("#idProductID").val(specResponseOnSuccess.Id);
                    $('#idspectype').val("BAY_INTERNAL");
                    selfspec.Searchspec();
                    $("#interstitial").hide();

                    return app.showMessage('Successfully created specification<br> of type BAY INTERNAL having <b>Id ' + specResponseOnSuccess.Id + '</b>', 'Specification');
                } else {
                    selfspec.updateOnSuccess();
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

            if (err && err.responseJSON && err.responseJSON.ExceptionMessage && err.responseJSON.ExceptionMessage.indexOf('value too large for column "CDMMS_OWNER"."BAY_ITNL"."BAY_ITNL_NM"') > 0) {
                return app.showMessage('Unable to update the specification. The system generated name is greater than 40 characters. Maximum length of the name is 40 characters.', 'Specification');
            } else {
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }
        }
    }

    BayInternalSpecification.prototype.saveAuditChanges = function () {
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

    function GetNumber(id, choice) {
        var stuff = { id: id, choice: choice };
        var returnnumber;
        $.ajax({
            type: "GET",
            url: 'api/specn/getnumberbayinternal/',
            data: stuff,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: getNumberSuccess,
            error: getNumberError,
            async: false
        });
        function getNumberSuccess(response) {
            returnnumber = response;
        }
        function getNumberError(response) {
            returnnumber = '0';
        }
        return returnnumber;
    }

    BayInternalSpecification.prototype.bayInternalCheckBoxCheck = function () {
        if ($("#BayIntlWllMntChbk").is(':checked'))
            selfbayInternal.selectedBayIntlSpec().BayIntlWllMnt.value = "Y";
        else
            selfbayInternal.selectedBayIntlSpec().BayIntlWllMnt.value = "N";

        if ($("#BayIntlStrghtThruChbk").is(':checked'))
            selfbayInternal.selectedBayIntlSpec().BayIntlStrghtThru.value = "Y";
        else
            selfbayInternal.selectedBayIntlSpec().BayIntlStrghtThru.value = "N";

        if ($("#BayIntlMidPlnChbk").is(':checked'))
            selfbayInternal.selectedBayIntlSpec().BayIntlMidPln.value = "Y";
        else
            selfbayInternal.selectedBayIntlSpec().BayIntlMidPln.value = "N";

        if ($("#BayIntlCmpltChbk").is(':checked'))
            selfbayInternal.selectedBayIntlSpec().Cmplt.value = "Y";
        else
            selfbayInternal.selectedBayIntlSpec().Cmplt.value = "N";

        if ($("#BayIntlPrpgtdChbk").is(':checked'))
            selfbayInternal.selectedBayIntlSpec().Prpgtd.value = "Y";
        else
            selfbayInternal.selectedBayIntlSpec().Prpgtd.value = "N";

        if ($("#BayIntlDltdChbk").is(':checked'))
            selfbayInternal.selectedBayIntlSpec().Dltd.value = "Y";
        else
            selfbayInternal.selectedBayIntlSpec().Dltd.value = "N";
    };

    BayInternalSpecification.prototype.NumDecimal = function (mp, event) {
        //self.saveValidation(true);
        var charCode = event.keyCode;
        var name = event.currentTarget.id;
        var value = $('#' + name).val();
        var reqCharAfterdot = 3;

        if (name == "dpthnew" || name == "wdthnew") {
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

    return BayInternalSpecification;
});