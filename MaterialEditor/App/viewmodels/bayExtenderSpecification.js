define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/app', 'durandal/system', 'jqueryui', '../Utility/referenceDataHelper', 'bootstrapJS', 'plugins/router', '../Utility/user'],
    function (composition, ko, $, http, activator, mapping, app, system, jqueryui, reference, bootstrapJS, router, user) {
        var BayExtenderSpecification = function (data) {
            selfbayextender = this;
            specChangeArray = [];
            var results = JSON.parse(data);

            if (results.BayItnlWdthLst != null && results.BayItnlWdthLst.list.length > 0) {
                for (var i = 0 ; i < results.BayItnlWdthLst.list.length; i++) {
                    results.BayItnlWdthLst.list[i].BayItnlWdth.value = Number(results.BayItnlWdthLst.list[i].BayItnlWdth.value).toFixed(3);
                    // Ensure there is a leading zero when a fraction like .75 comes across.  should display as 0.75
                    if (results.BayItnlWdthLst.list[i].BayItnlWdth.value < 1 && results.BayItnlWdthLst.list[i].BayItnlWdth.value > 0 && results.BayItnlWdthLst.list[i].BayItnlWdth.value.substring(0, 1) == '.') {
                        results.BayItnlWdthLst.list[i].BayItnlWdth.value = '0' + results.BayItnlWdthLst.list[i].BayItnlWdth.value;
                    }
                }
            }
            if (results.BayItnlDpthLst != null && results.BayItnlDpthLst.list.length > 0) {
                for (var i = 0 ; i < results.BayItnlDpthLst.list.length; i++) {
                    results.BayItnlDpthLst.list[i].BayItnlDpth.value = Number(results.BayItnlDpthLst.list[i].BayItnlDpth.value).toFixed(3);
                    results.BayItnlDpthLst.list[i].BayItnlDpth.value = Number(results.BayItnlDpthLst.list[i].BayItnlDpth.value).toFixed(3);
                    // Ensure there is a leading zero when a fraction like .75 comes across.  should display as 0.75
                    if (results.BayItnlDpthLst.list[i].BayItnlDpth.value < 1 && results.BayItnlDpthLst.list[i].BayItnlDpth.value > 0 && results.BayItnlDpthLst.list[i].BayItnlDpth.value.substring(0, 1) == '.') {
                        results.BayItnlDpthLst.list[i].BayItnlDpth.value = '0' + results.BayItnlDpthLst.list[i].BayItnlDpth.value;
                    }
                }
            }
            if (results.BayItnlHghtLst != null && results.BayItnlHghtLst.list.length > 0) {
                for (var i = 0 ; i < results.BayItnlHghtLst.list.length; i++) {
                    results.BayItnlHghtLst.list[i].BayItnlHght.value = Number(results.BayItnlHghtLst.list[i].BayItnlHght.value).toFixed(3);
                    results.BayItnlHghtLst.list[i].BayItnlHght.value = Number(results.BayItnlHghtLst.list[i].BayItnlHght.value).toFixed(3);
                    // Ensure there is a leading zero when a fraction like .75 comes across.  should display as 0.75
                    if (results.BayItnlHghtLst.list[i].BayItnlHght.value < 1 && results.BayItnlHghtLst.list[i].BayItnlHght.value > 0 && results.BayItnlHghtLst.list[i].BayItnlHght.value.substring(0, 1) == '.') {
                        results.BayItnlHghtLst.list[i].BayItnlHght.value = '0' + results.BayItnlHghtLst.list[i].BayItnlHght.value;
                    }
                }
            }            

            selfbayextender.selectedBayExtenderSpec = ko.observable();
            selfbayextender.selectedBayExtenderSpec(results);
            selfbayextender.associatemtlblck = ko.observableArray();
            selfbayextender.searchtblbayextd = ko.observableArray();
            selfbayextender.completedNotSelected = ko.observable(true);
            selfbayextender.enableBackButton = ko.observable(false);
            selfbayextender.enableName = ko.observable(true);
            selfbayextender.enableAssociate = ko.observable(false);
            selfbayextender.enableModalSave = ko.observable(false);
            selfbayextender.specName = ko.observable('');
            selfbayextender.nongenericBlockview = ko.observable();
            selfbayextender.selectedDepth = ko.observableArray('');
            selfbayextender.duplicateSelectedPartNbr = ko.observable();

            selfbayextender.duplicateSelectedShelfSpecDpthNbr = ko.observable();
            selfbayextender.duplicateSelectedShelfSpecDpth = ko.observable();
            selfbayextender.duplicateSelectedShelfSpecHghtNbr = ko.observable();
            selfbayextender.duplicateSelectedShelfSpecHght = ko.observable();
            selfbayextender.duplicateSelectedShelfSpecWdthNbr = ko.observable();
            selfbayextender.duplicateSelectedShelfSpecWdth = ko.observable();
            selfbayextender.duplicateSelectedShelfSpecName = ko.observable();
            selfbayextender.duplicateSelectedShelfSpecModel = ko.observable();
            selfbayextender.duplicateSelectedShelfSpecDltd = ko.observable();
            selfbayextender.duplicateSelectedBayExSpecDltd = results.Dltd.bool;

            if (results.BayItnlDpthLst != null) {
                selfbayextender.duplicateSelectedShelfSpecDpthNbr.value = results.BayItnlDpthLst.list.length;
                selfbayextender.duplicateSelectedShelfSpecDpth = selfbayextender.selectedBayExtenderSpec().BayItnlDpthId.value;
            }
            if (results.BayItnlHghtLst != null) {
                selfbayextender.duplicateSelectedShelfSpecHghtNbr.value = results.BayItnlHghtLst.list.length;
                selfbayextender.duplicateSelectedShelfSpecHght = selfbayextender.selectedBayExtenderSpec().BayItnlHghtId.value;
            }
            if (results.BayItnlWdthLst != null) {
                selfbayextender.duplicateSelectedShelfSpecWdthNbr.value = results.BayItnlWdthLst.list.length;
                selfbayextender.duplicateSelectedShelfSpecWdth = selfbayextender.selectedBayExtenderSpec().BayItnlWdthId.value;
            }

            if (selfbayextender.selectedBayExtenderSpec().Nm.value != "") {
                if (selfbayextender.selectedBayExtenderSpec().Gnrc.bool == true) {
                    selfbayextender.specName(selfbayextender.selectedBayExtenderSpec().Nm.value);
                } else {
                    selfbayextender.specName(selfbayextender.selectedBayExtenderSpec().RvsnNm.value);
                }
            }

            if (results.Nm != null) {
                selfbayextender.duplicateSelectedShelfSpecName.value = selfbayextender.specName();
                selfbayextender.duplicateSelectedShelfSpecModel.value = results.Nm.value;
            }
      
            if (selfbayextender.selectedBayExtenderSpec().Gnrc.bool == false) {
                selfbayextender.enableAssociate(true);
                selfbayextender.nongenericBlockview(true);
            } else {
                selfbayextender.enableName(true);
                selfbayextender.nongenericBlockview(false);
            }

            if (selfspec.backToMtlItmId > 0) {
                selfbayextender.enableBackButton(true);
            }

            if (selfbayextender.selectedBayExtenderSpec().Cmplt.bool && selfbayextender.selectedBayExtenderSpec().Prpgtd.enable) {
                selfbayextender.completedNotSelected(false);
            }

            $(document).on('change', '[type=checkbox]', function () {
                if (this.name == "cmplindbayextd") {
                    if (this.checked == false) {
                        document.getElementById('proptIndBayextd').checked = false;
                    }                   
                }
            });
          
            $(document).on('change', '[type=checkbox]', function () {
                if (this.name == "gnrcIndbayextd") {
                    if (this.checked == true) {
                        document.getElementById('rcrdlyBayextd').disabled = true;
                        selfbayextender.enableAssociate(false);                        
                    }
                    else {
                        document.getElementById('rcrdlyBayextd').disabled = false;
                        selfbayextender.enableAssociate(true);                       
                    }
                }
            });

            $(document).on('change', '[type=checkbox]', function () {
                if (this.name == "rcrdolyBayextd") {
                    if (this.checked == true) {
                        document.getElementById('gnrcIndbayextd').disabled = true;
                    }
                    else {
                        document.getElementById('gnrcIndbayextd').disabled = false;
                    }
                }
            });

            setTimeout(function () {
                $(document).ready(function () {
                    $('input.BayItnlDpthLstCbk').on('change', function () {
                        $('input.BayItnlDpthLstCbk').not(this).prop('checked', false);
                        selfbayextender.selectedBayExtenderSpec().BayItnlDpthId.value = $(this).val();
                        $("#BayItnlDpthLstTblDgr").hide();
                    });

                    $('input.BayItnlWdthLstCbk').on('change', function () {
                        $('input.BayItnlWdthLstCbk').not(this).prop('checked', false);
                        selfbayextender.selectedBayExtenderSpec().BayItnlWdthId.value = $(this).val();
                        $("#BayItnlWdthLstTblDgr").hide();
                    });

                    $('input.BayItnlHghtLstCbk').on('change', function () {
                        $('input.BayItnlHghtLstCbk').not(this).prop('checked', false);
                        selfbayextender.selectedBayExtenderSpec().BayItnlHghtId.value = $(this).val();
                        $("#BayItnlHghtLstTblDgr").hide();
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
                context: selfbayextender
            });
            function successFunc(data, status) {

                if (data == 'no_results') {
                    $("#interstitial").hide();
                    //app.showMessage("No records found", 'Failed');
                }
                else {
                    $("#interstitial").hide();
                    selfbayextender.selectedDepth(JSON.parse(data));
                    $('#depthuomnew').val('22');
                    $('#widthnewuom').val('22');
                    $('#hegthnewuom').val('22');
                }
            }
            function errorFunc() {
                $("#interstitial").hide();
                //alert('error');
            }
            $("#baydepthnew").hide(); 
            $("#baywidthnew").hide();
            $("#bayheightnew").hide();
            $('#depthuomnew').val('22');
            $('#widthnewuom').val('22');
            $('#hegthnewuom').val('22');
        };

        BayExtenderSpecification.prototype.onchangeCompleted = function () {
            if ($("#cmpIndBayextd").is(':checked')) {
                selfbayextender.completedNotSelected(false);
            } else {
                selfbayextender.completedNotSelected(true);
                selfbayextender.selectedBayExtenderSpec().Prpgtd.bool = false;
            }
        };

        BayExtenderSpecification.prototype.onchangebayGeneric = function () {
            if ($("#gnrcIndbayextd").is(':checked')) {              
                selfbayextender.selectedBayExtenderSpec().RO.enable = false;
                selfbayextender.enableName(true);
                selfbayextender.nongenericBlockview(false);
                document.getElementById("modelTxt").required = false;
            } else {              
                selfbayextender.selectedBayExtenderSpec().RO.enable = true;
                selfbayextender.enableName(true);
                selfbayextender.nongenericBlockview(true);
                document.getElementById("modelTxt").required = true;
            }
        };
        BayExtenderSpecification.prototype.Adddepth = function () {
            $("#baydepthnew").hide();
            $("#baywidthnew").hide();
            $("#bayheightnew").hide();
            $("#baydepthexists").hide();
            $("#baywidthexists").hide(); 
            $("#bayheightexists").hide();

            if ((document.getElementById('dpthnew').value !== '') && (document.getElementById('depthuomnew').value !== '')) {
                var dpthnew = $('#dpthnew').val();
                var dpthuom = $('#depthuomnew').val();
                var exists = false;

                for (var i = 0; i < selfbayextender.selectedBayExtenderSpec().BayItnlDpthLst.list.length; i++) {
                    if (dpthnew === selfbayextender.selectedBayExtenderSpec().BayItnlDpthLst.list[i].BayItnlDpth.value && dpthuom === selfbayextender.selectedBayExtenderSpec().BayItnlDpthLst.list[i].BayItnlDpthUomId.value) {
                        exists = true;
                        break;
                    }
                }

                if (exists) {
                    $("#baydepthexists").show();
                } else {
                    var addJSON = { "dimension": dpthnew, "uom": dpthuom };
                    var tablenm = "bay_extndr_intl_dpth";

                    addJSON = JSON.stringify(addJSON);

                    $.ajax({
                        type: "POST",
                        url: 'api/specn/insertdimension/' + tablenm,
                        data: addJSON,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: successFunc,
                        error: errorFunc,
                        context: selfbayextender
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
                                context: selfbayextender
                            });

                            function successFunc(data, status) {

                                if (data == 'no_results') {
                                    $("#interstitial").hide();
                                    //app.showMessage("No records found", 'Failed');
                                }
                                else {
                                    $("#interstitial").hide();
                                    app.showMessage("Successfully added new bay External depth!", 'Specification');
                                    document.getElementById('dpthnew').value = "";
                                    document.getElementById('depthuomnew').value = "";

                                    $("#baydepthnew").hide();
                                    $("#baywidthnew").hide();
                                    $("#bayheightnew").hide();
                                    $("#baydepthexists").hide();
                                    $("#baywidthexists").hide();
                                    $("#bayheightexists").hide();
                                    $('#depthuomnew').val('22');
                                    $('#widthnewuom').val('22');
                                    $('#hegthnewuom').val('22');
                                    var dpetnew = JSON.parse(data);
                                    if (dpetnew != null && dpetnew.list.length > 0) {
                                        for (var i = 0 ; i < dpetnew.list.length; i++) {
                                            dpetnew.list[i].BayItnlDpth.value = Number(dpetnew.list[i].BayItnlDpth.value).toFixed(3);
                                        }
                                    }
                                    selfbayextender.selectedBayExtenderSpec().BayItnlDpthLst.list = dpetnew.list;
                                    selfbayextender.selectedBayExtenderSpec(selfbayextender.selectedBayExtenderSpec());
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
        BayExtenderSpecification.prototype.Addwidth = function () {
            $("#baydepthnew").hide();
            $("#baywidthnew").hide();
            $("#bayheightnew").hide();
            $("#baydepthexists").hide();
            $("#baywidthexists").hide();
            $("#bayheightexists").hide();

            if ((document.getElementById('wdthnew').value !== '') && (document.getElementById('widthnewuom').value !== '')) {
                var widthnewadd = $('#wdthnew').val();
                var widthnewdropdwn = $('#widthnewuom').val();

                var exists = false;

                for (var i = 0; i < selfbayextender.selectedBayExtenderSpec().BayItnlWdthLst.list.length; i++) {
                    if (widthnewadd === selfbayextender.selectedBayExtenderSpec().BayItnlWdthLst.list[i].BayItnlWdth.value && widthnewdropdwn === selfbayextender.selectedBayExtenderSpec().BayItnlWdthLst.list[i].BayItnlWdthUomId.value) {
                        exists = true;
                        break;
                    }
                }

                if (exists) {
                    $("#baywidthexists").show();
                } else {
                    var addJSON = { "dimension": widthnewadd, "uom": widthnewdropdwn };
                    var tablenm = "bay_extndr_intl_wdth";

                    addJSON = JSON.stringify(addJSON);

                    $.ajax({
                        type: "POST",
                        url: 'api/specn/insertdimension/' + tablenm,
                        data: addJSON,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: successFunc,
                        error: errorFunc,
                        context: selfbayextender
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
                                context: selfbayextender
                            });
                            function successFunc(data, status) {

                                if (data == 'no_results') {
                                    $("#interstitial").hide();
                                    //app.showMessage("No records found", 'Failed');
                                }
                                else {
                                    $("#interstitial").hide();
                                    app.showMessage("Successfully added new bay extender width!", 'Specification');
                                    document.getElementById('wdthnew').value = "";
                                    document.getElementById('widthnewuom').value = "";

                                    $("#baydepthnew").hide();
                                    $("#baywidthnew").hide();
                                    $("#bayheightnew").hide();
                                    $("#baydepthexists").hide();
                                    $("#baywidthexists").hide();
                                    $("#bayheightexists").hide();
                                    $('#depthuomnew').val('22');
                                    $('#widthnewuom').val('22');
                                    $('#hegthnewuom').val('22');
                                    var widthnew = JSON.parse(data);
                                    if (widthnew != null && widthnew.list.length > 0) {
                                        for (var i = 0 ; i < widthnew.list.length; i++) {
                                            widthnew.list[i].BayItnlWdth.value = Number(widthnew.list[i].BayItnlWdth.value).toFixed(3);
                                        }
                                    }
                                    selfbayextender.selectedBayExtenderSpec().BayItnlWdthLst.list = widthnew.list;
                                    selfbayextender.selectedBayExtenderSpec(selfbayextender.selectedBayExtenderSpec());
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
        BayExtenderSpecification.prototype.Addheight = function () {
            $("#baydepthnew").hide();
            $("#baywidthnew").hide();
            $("#bayheightnew").hide();
            $("#baydepthexists").hide();
            $("#baywidthexists").hide();
            $("#bayheightexists").hide();

            if ((document.getElementById('hgthnew').value !== '') && (document.getElementById('hegthnewuom').value !== '')) {
                var hgtnewadd = $('#hgthnew').val();
                var hgtnewdropdwn = $('#hegthnewuom').val();

                var exists = false;

                for (var i = 0; i < selfbayextender.selectedBayExtenderSpec().BayItnlHghtLst.list.length; i++) {
                    if (hgtnewadd === selfbayextender.selectedBayExtenderSpec().BayItnlHghtLst.list[i].BayItnlHght.value && hgtnewdropdwn === selfbayextender.selectedBayExtenderSpec().BayItnlHghtLst.list[i].BayItnlHghtUomId.value) {
                        exists = true;
                        break;
                    }
                }

                if (exists) {
                    $("#bayheightexists").show();
                } else {
                    var addJSON = { "dimension": hgtnewadd, "uom": hgtnewdropdwn };
                    var tablenm = "bay_extndr_intl_hgt";

                    addJSON = JSON.stringify(addJSON);

                    $.ajax({
                        type: "POST",
                        url: 'api/specn/insertdimension/' + tablenm,
                        data: addJSON,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: successFunc,
                        error: errorFunc,
                        context: selfbayextender
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
                                context: selfbayextender
                            });
                            function successFunc(data, status) {

                                if (data == 'no_results') {
                                    $("#interstitial").hide();
                                    //app.showMessage("No records found", 'Failed');
                                }
                                else {
                                    $("#interstitial").hide();
                                    app.showMessage("Successfully added new bay extender height!", 'Specification');
                                    document.getElementById('hgthnew').value = "";
                                    document.getElementById('hegthnewuom').value = "";

                                    $("#baydepthnew").hide();
                                    $("#baywidthnew").hide();
                                    $("#bayheightnew").hide();
                                    $("#baydepthexists").hide();
                                    $("#baywidthexists").hide();
                                    $("#bayheightexists").hide();
                                    $('#depthuomnew').val('22');
                                    $('#widthnewuom').val('22');
                                    $('#hegthnewuom').val('22');
                                    var hgtnew = JSON.parse(data);
                                    if (hgtnew != null && hgtnew.list.length > 0) {
                                        for (var i = 0 ; i < hgtnew.list.length; i++) {
                                            hgtnew.list[i].BayItnlHght.value = Number(hgtnew.list[i].BayItnlHght.value).toFixed(3);
                                        }
                                    }
                                    selfbayextender.selectedBayExtenderSpec().BayItnlHghtLst.list = hgtnew.list;
                                    selfbayextender.selectedBayExtenderSpec(selfbayextender.selectedBayExtenderSpec());
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
                $("#bayheightnew").show();
            }
        };


        BayExtenderSpecification.prototype.navigateToMaterial = function () {
            if (document.getElementById('rcrdlyBayextd').checked == true) {
                var url = '#/roNew/' + selfspec.backToMtlItmId;
                router.navigate(url, false);
            }
            else {
                var url = '#/mtlInv/' + selfspec.backToMtlItmId;
                router.navigate(url, false);
            }
        };

        BayExtenderSpecification.prototype.NumDecimal = function (mp, event) {
            //self.saveValidation(true);
            var charCode = event.keyCode;
            var name = event.currentTarget.id;
            var value = $('#' + name).val();
            var reqCharAfterdot = 3;

            if (name == "xtnldpthbay" || name == "xhghtbay" || name == "xtnwdthbay") {
                reqCharAfterdot = 3;
            }

            if (charCode > 31 && (charCode < 48 || charCode > 57) && !(charCode == 46 || charCode == 8)) {
                return false;
            } else {
                var len = value.length;
                var index = value.indexOf('.');

                if (index > 0 && charCode == 46) {
                    return false;
                }

                if (index == 0 || index > 0) {
                    var CharAfterdot = (len + 1) - index;

                    if (CharAfterdot > reqCharAfterdot + 1) {
                        return false;
                    }
                }
            }

            return true;
        };

        BayExtenderSpecification.prototype.SaveBayExtender = function () {

            // check for duplicate model name
            var modelname = document.getElementById("modelTxt").value;
            if (selfspec.selectRadioSpec() == 'existSpec') {
                var modelnamecount = selfbayextender.GetModelNameCount(modelname, selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].SpecId.value);
            }
            else {
                var modelnamecount = selfbayextender.GetModelNameCount(modelname, 0);
            }
            if (modelnamecount > 0) {
                app.showMessage('The model name ' + modelname + ' already exists on a different Spec.');
                return;
            }

            specChangeArray = [];

            console.log(selfbayextender.selectedBayExtenderSpec());
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            var depthcheckboxSelected = false;
            var widthcheckboxSelected = false;
            var heightcheckboxSelected = false;      
            var bayItnlDpthLstCbk = $("#BayItnlDpthLstTbl .BayItnlDpthLstCbk");
            var bayItnlWdthLstCbk = $("#BayItnlWdthLstTbl .BayItnlWdthLstCbk");
            var bayItnlHghtLstCbk = $("#BayItnlHgtLstTbl .BayItnlHghtLstCbk");

            for (var i = 0; i < bayItnlDpthLstCbk.length; i++) {
                if (bayItnlDpthLstCbk[i].checked == true) {
                    depthcheckboxSelected = true;

                    if (selfbayextender.selectedBayExtenderSpec().BayExtndr) {
                        selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].IntlDpthId.value = selfbayextender.selectedBayExtenderSpec().BayItnlDpthLst.list[i].BayItnlDpthId.value;
                    }

                    break;
                }
            }            

            for (var i = 0; i < bayItnlWdthLstCbk.length; i++) {
                if (bayItnlWdthLstCbk[i].checked == true) {
                    widthcheckboxSelected = true;

                    if (selfbayextender.selectedBayExtenderSpec().BayExtndr) {
                        selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].IntlWdthId.value = selfbayextender.selectedBayExtenderSpec().BayItnlWdthLst.list[i].BayItnlWdthId.value;
                    }

                    break;
                }
            }            

            for (var i = 0; i < bayItnlHghtLstCbk.length; i++) {
                if (bayItnlHghtLstCbk[i].checked == true) {
                    heightcheckboxSelected = true;

                    if (selfbayextender.selectedBayExtenderSpec().BayExtndr) {
                        selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].IntlHghtId.value = selfbayextender.selectedBayExtenderSpec().BayItnlHghtLst.list[i].BayItnlHghtId.value;
                    }

                    break;
                }
            }
       
            if (depthcheckboxSelected && widthcheckboxSelected && heightcheckboxSelected) {
                var name = document.getElementById("idbayextdspecnm").value;
                var chkgenrc = false;

                if ($("#gnrcIndbayextd").is(':checked')) {
                    chkgenrc = true;
                }

                if ((chkgenrc) || (!chkgenrc && selfbayextender.selectedBayExtenderSpec().BayExtndr !== undefined)) {
                    if (selfbayextender.selectedBayExtenderSpec().Gnrc.bool == true) {
                        selfbayextender.selectedBayExtenderSpec().Nm.value = selfbayextender.specName();
                    } else {
                        selfbayextender.selectedBayExtenderSpec().RvsnNm.value = selfbayextender.specName();
                    }

                    var txtCondt = '';

                    if (selfbayextender.selectedBayExtenderSpec() != null && !chkgenrc) {
                        if (selfbayextender.specName() != selfbayextender.duplicateSelectedShelfSpecName.value) {
                            txtCondt += "Name changed to <b>" + selfbayextender.specName() + '</b> from ' + selfbayextender.duplicateSelectedShelfSpecName.value + "<br/>";

                            var saveJSON = {
                                'tablename': 'bay_extndr_specn', 'columnname': 'bay_extndr_specn_nm', 'audittblpkcolnm': 'bay_extndr_specn_id', 'audittblpkcolval': selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                'oldcolval': selfbayextender.duplicateSelectedShelfSpecName.value,
                                'newcolval': selfbayextender.specName(),
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbayextender.specName() + ' Spec Name from ' + selfbayextender.duplicateSelectedShelfSpecName.value + ' on ',
                                'materialitemid': selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].id.value
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (selfbayextender.selectedBayExtenderSpec().BayItnlDpthLst.list.length != selfbayextender.duplicateSelectedShelfSpecDpthNbr.value) {
                            txtCondt += "Depth List changed to <b>" + selfbayextender.selectedBayExtenderSpec().BayItnlDpthLst.list.length + "</b> entries from " + selfbayextender.duplicateSelectedShelfSpecDpthNbr.value + "<br/>";
                        }
                        if (selfbayextender.duplicateSelectedShelfSpecDpth != selfbayextender.selectedBayExtenderSpec().BayItnlDpthId.value) {
                            var oldNumber = GetNumber(selfbayextender.duplicateSelectedShelfSpecDpth, 'D');  // for display and auditing
                            var newNumber = GetNumber(selfbayextender.selectedBayExtenderSpec().BayItnlDpthId.value, 'D');  // for display and auditing
                            txtCondt += "Depth changed to <b>" + Number(newNumber).toFixed(3) + "</b> from " + Number(oldNumber).toFixed(3) + "<br/>";

                            var saveJSON = {
                                'tablename': 'rme_bay_extndr_mtrl', 'columnname': 'itnl_dpth_id', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].MtrlId.value, 'auditprnttblpkcolnm': '',
                                'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(oldNumber).toFixed(3), 'newcolval': Number(newNumber).toFixed(3), 'cuid': user.cuid,
                                'cmnttxt': user.cuid + ' changed ' + selfbayextender.specName() + ' Depth from ' + Number(oldNumber).toFixed(3) + ' to ' + Number(newNumber).toFixed(3) + ' on ',
                                'materialitemid': selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].id.value
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (selfbayextender.selectedBayExtenderSpec().BayItnlWdthLst.list.length != selfbayextender.duplicateSelectedShelfSpecWdthNbr.value) {
                            txtCondt += "Width List changed to <b>" + selfbayextender.selectedBayExtenderSpec().BayItnlWdthLst.list.length + "</b> entries from " + selfbayextender.duplicateSelectedShelfSpecWdthNbr.value + "<br/>";
                        }
                        if (selfbayextender.duplicateSelectedShelfSpecWdth != selfbayextender.selectedBayExtenderSpec().BayItnlWdthId.value) {
                            var oldNumber = GetNumber(selfbayextender.duplicateSelectedShelfSpecWdth, 'W');  // for display and auditing
                            var newNumber = GetNumber(selfbayextender.selectedBayExtenderSpec().BayItnlWdthId.value, 'W');  // for display and auditing
                            txtCondt += "Width changed to <b>" + Number(newNumber).toFixed(3) + "</b> from " + Number(oldNumber).toFixed(3) + "<br/>";

                            var saveJSON = {
                                'tablename': 'rme_bay_extndr_mtrl', 'columnname': 'itnl_wdth_id', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].MtrlId.value, 'auditprnttblpkcolnm': '',
                                'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(oldNumber).toFixed(3), 'newcolval': Number(newNumber).toFixed(3), 'cuid': user.cuid,
                                'cmnttxt': user.cuid + ' changed ' + selfbayextender.specName() + ' Width from ' + Number(oldNumber).toFixed(3) + ' to ' + Number(newNumber).toFixed(3) + ' on ',
                                'materialitemid': selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].id.value
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (selfbayextender.selectedBayExtenderSpec().BayItnlHghtLst.list.length != selfbayextender.duplicateSelectedShelfSpecHghtNbr.value) {
                            txtCondt += "Height List changed to <b>" + selfbayextender.selectedBayExtenderSpec().BayItnlHghtLst.list.length + "</b> entries from " + selfbayextender.duplicateSelectedShelfSpecHghtNbr.value + "<br/>";
                        }
                        if (selfbayextender.duplicateSelectedShelfSpecHght != selfbayextender.selectedBayExtenderSpec().BayItnlHghtId.value) {
                            var oldNumber = GetNumber(selfbayextender.duplicateSelectedShelfSpecHght, 'H');  // for display and auditing
                            var newNumber = GetNumber(selfbayextender.selectedBayExtenderSpec().BayItnlHghtId.value, 'H');  // for display and auditing
                            txtCondt += "Height changed to <b>" + Number(newNumber).toFixed(3) + "</b> from " + Number(oldNumber).toFixed(3) + "<br/>";

                            var saveJSON = {
                                'tablename': 'rme_bay_extndr_mtrl', 'columnname': 'itnl_hgt_id', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].MtrlId.value, 'auditprnttblpkcolnm': '',
                                'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(oldNumber).toFixed(3), 'newcolval': Number(newNumber).toFixed(3), 'cuid': user.cuid,
                                'cmnttxt': user.cuid + ' changed ' + selfbayextender.specName() + ' Height from ' + Number(oldNumber).toFixed(3) + ' to ' + Number(newNumber).toFixed(3) + ' on ',
                                'materialitemid': selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].id.value
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (selfbayextender.selectedBayExtenderSpec().Dltd.bool != selfbayextender.duplicateSelectedBayExSpecDltd) {
                            txtCondt += "Unusable changed to <b>" + selfbayextender.selectedBayExtenderSpec().Dltd.bool + '</b> from ' + selfbayextender.duplicateSelectedBayExSpecDltd + "<br/>";

                            var saveJSON = {
                                'tablename': 'bay_extndr_specn_revsn_alt', 'columnname': 'del_ind', 'audittblpkcolnm': 'bay_extndr_specn_revsn_alt_id', 'audittblpkcolval': selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].SpecRvsnId.value, 'auditprnttblpkcolnm': 'bay_extndr_specn_id', 'auditprnttblpkcolval': selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].SpecId.value, 'actncd': 'C',
                                'oldcolval': selfbayextender.duplicateSelectedBayExSpecDltd,
                                'newcolval': selfbayextender.selectedBayExtenderSpec().Dltd.bool,
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbayextender.specName() + ' Unusable from ' + selfbayextender.duplicateSelectedBayExSpecDltd + ' to ' + selfbayextender.selectedBayExtenderSpec().Dltd.bool + ' on ',
                                'materialitemid': selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].id.value
                            };
                            specChangeArray.push(saveJSON);
                        }
                    }
                    else if (chkgenrc) {
                        if (selfbayextender.specName() != selfbayextender.duplicateSelectedShelfSpecName.value) {
                            txtCondt += "Name changed to <b>" + selfbayextender.specName() + '</b> from ' + selfbayextender.duplicateSelectedShelfSpecName.value + "<br/>";

                            var saveJSON = {
                                'tablename': 'bay_extndr_specn', 'columnname': 'bay_extndr_specn_nm', 'audittblpkcolnm': 'bay_extndr_specn_id', 'audittblpkcolval': selfbayextender.selectedBayExtenderSpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                'oldcolval': selfbayextender.duplicateSelectedShelfSpecName.value,
                                'newcolval': selfbayextender.specName(),
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbayextender.specName() + ' Spec Name from ' + selfbayextender.duplicateSelectedShelfSpecName.value + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (selfbayextender.selectedBayExtenderSpec().BayItnlDpthLst.list.length != selfbayextender.duplicateSelectedShelfSpecDpthNbr.value) {
                            txtCondt += "Depth List changed to <b>" + selfbayextender.selectedBayExtenderSpec().BayItnlDpthLst.list.length + "</b> entries from " + selfbayextender.duplicateSelectedShelfSpecDpthNbr.value + "<br/>";
                        }
                        if (selfbayextender.duplicateSelectedShelfSpecDpth != selfbayextender.selectedBayExtenderSpec().BayItnlDpthId.value) {
                            var oldNumber = GetNumber(selfbayextender.duplicateSelectedShelfSpecDpth, 'D');  // for display and auditing
                            var newNumber = GetNumber(selfbayextender.selectedBayExtenderSpec().BayItnlDpthId.value, 'D');  // for display and auditing
                            txtCondt += "Depth changed to <b>" + Number(newNumber).toFixed(3) + "</b> from " + Number(oldNumber).toFixed(3) + "<br/>";

                            var saveJSON = {
                                'tablename': 'bay_extndr_specn_gnrc', 'columnname': 'itnl_dpth_id', 'audittblpkcolnm': 'bay_extndr_specn_id', 'audittblpkcolval': selfbayextender.selectedBayExtenderSpec().id.value, 'auditprnttblpkcolnm': '',
                                'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(oldNumber).toFixed(3), 'newcolval': Number(newNumber).toFixed(3), 'cuid': user.cuid,
                                'cmnttxt': user.cuid + ' changed ' + selfbayextender.specName() + ' Depth from ' + Number(oldNumber).toFixed(3) + ' to ' + Number(newNumber).toFixed(3) + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (selfbayextender.selectedBayExtenderSpec().BayItnlWdthLst.list.length != selfbayextender.duplicateSelectedShelfSpecWdthNbr.value) {
                            txtCondt += "Width List changed to <b>" + selfbayextender.selectedBayExtenderSpec().BayItnlWdthLst.list.length + "</b> entries from " + selfbayextender.duplicateSelectedShelfSpecWdthNbr.value + "<br/>";
                        }
                        if (selfbayextender.duplicateSelectedShelfSpecWdth != selfbayextender.selectedBayExtenderSpec().BayItnlWdthId.value) {
                            var oldNumber = GetNumber(selfbayextender.duplicateSelectedShelfSpecWdth, 'W');  // for display and auditing
                            var newNumber = GetNumber(selfbayextender.selectedBayExtenderSpec().BayItnlWdthId.value, 'W');  // for display and auditing
                            txtCondt += "Width changed to <b>" + Number(newNumber).toFixed(3) + "</b> from " + Number(oldNumber).toFixed(3) + "<br/>";

                            var saveJSON = {
                                'tablename': 'bay_extndr_specn_gnrc', 'columnname': 'itnl_wdth_id', 'audittblpkcolnm': 'bay_extndr_specn_id', 'audittblpkcolval': selfbayextender.selectedBayExtenderSpec().id.value, 'auditprnttblpkcolnm': '',
                                'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(oldNumber).toFixed(3), 'newcolval': Number(newNumber).toFixed(3), 'cuid': user.cuid,
                                'cmnttxt': user.cuid + ' changed ' + selfbayextender.specName() + ' Width from ' + Number(oldNumber).toFixed(3) + ' to ' + Number(newNumber).toFixed(3) + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (selfbayextender.selectedBayExtenderSpec().BayItnlHghtLst.list.length != selfbayextender.duplicateSelectedShelfSpecHghtNbr.value) {
                            txtCondt += "Height List changed to <b>" + selfbayextender.selectedBayExtenderSpec().BayItnlHghtLst.list.length + "</b> entries from " + selfbayextender.duplicateSelectedShelfSpecHghtNbr.value + "<br/>";
                        }
                        if (selfbayextender.duplicateSelectedShelfSpecHght != selfbayextender.selectedBayExtenderSpec().BayItnlHghtId.value) {
                            var oldNumber = GetNumber(selfbayextender.duplicateSelectedShelfSpecHght, 'H');  // for display and auditing
                            var newNumber = GetNumber(selfbayextender.selectedBayExtenderSpec().BayItnlHghtId.value, 'H');  // for display and auditing
                            txtCondt += "Height changed to <b>" + Number(newNumber).toFixed(3) + "</b> from " + Number(oldNumber).toFixed(3) + "<br/>";

                            var saveJSON = {
                                'tablename': 'bay_extndr_specn_gnrc', 'columnname': 'itnl_hgt_id', 'audittblpkcolnm': 'bay_extndr_specn_id', 'audittblpkcolval': selfbayextender.selectedBayExtenderSpec().id.value, 'auditprnttblpkcolnm': '',
                                'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(oldNumber).toFixed(3), 'newcolval': Number(newNumber).toFixed(3), 'cuid': user.cuid,
                                'cmnttxt': user.cuid + ' changed ' + selfbayextender.specName() + ' Height from ' + Number(oldNumber).toFixed(3) + ' to ' + Number(newNumber).toFixed(3) + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (selfbayextender.selectedBayExtenderSpec().Dltd.bool != selfbayextender.duplicateSelectedBayExSpecDltd) {
                            txtCondt += "Unusable changed to <b>" + selfbayextender.selectedBayExtenderSpec().Dltd.bool + '</b> from ' + selfbayextender.duplicateSelectedBayExSpecDltd + "<br/>";

                            var saveJSON = {
                                'tablename': 'bay_extndr_specn_gnrc', 'columnname': 'del_ind', 'audittblpkcolnm': 'bay_extndr_specn_id', 'audittblpkcolval': selfbayextender.selectedBayExtenderSpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                'oldcolval': selfbayextender.duplicateSelectedBayExSpecDltd,
                                'newcolval': selfbayextender.selectedBayExtenderSpec().Dltd.bool,
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfbayextender.specName() + ' Unusable from ' + selfbayextender.duplicateSelectedBayExSpecDltd + ' to ' + selfbayextender.selectedBayExtenderSpec().Dltd.bool + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                    }

                    var textlength = txtCondt.length;
                    if (selfbayextender.selectedBayExtenderSpec().BayExtndr) {
                        var configNameList = selfbayextender.getConfigNames(selfbayextender.selectedBayExtenderSpec().BayExtndr.list[0].id.value);
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
                        app.showMessage(txtCondt, 'Update Confirmation for Bay Extender', ['Ok', 'Cancel']).then(function (dialogResult) {
                            if (dialogResult == 'Ok') {
                                selfbayextender.saveBayExSpec();
                            }
                        });
                    } else {
                        selfbayextender.saveBayExSpec();
                    }
                    
                }
                else {
                    $("#interstitial").hide();

                    if (!chkgenrc)
                        return app.showMessage('Please associate a material item to the specification before saving.', 'Specification');
                }
            }
            else {
                $("#interstitial").hide();

                if (!depthcheckboxSelected) {
                    $("#BayItnlDpthLstTblDgr").show();
                }

                if (!widthcheckboxSelected) {
                    $("#BayItnlWdthLstTblDgr").show();
                }

                if (!heightcheckboxSelected) {
                    $("#BayItnlHghtLstTblDgr").show();
                }
            }
        };

        BayExtenderSpecification.prototype.saveBayExSpec = function () {
            if (selfbayextender.selectedBayExtenderSpec().Gnrc.bool == true) {
                selfbayextender.selectedBayExtenderSpec().Nm.value = selfbayextender.specName();
            }
            else {
                selfbayextender.selectedBayExtenderSpec().RvsnNm.value = selfbayextender.specName();
            }
            
            var saveJSON1 = mapping.toJS(selfbayextender.selectedBayExtenderSpec());

            $.ajax({
                type: "POST",
                url: 'api/specn/update/',
                data: JSON.stringify(saveJSON1),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: updateSuccess,
                error: updateError
            });

            function updateSuccess(response) {
                var specResponseOnSuccess = JSON.parse(response);

                if ("Success" == specResponseOnSuccess.Status) {
                    selfbayextender.saveAuditChanges();
                    var specWorkToDoId = specResponseOnSuccess.SpecWTD;
                    var mtlWorkToDoId = specResponseOnSuccess.MtlWTD;

                    if (specWorkToDoId !== 0) {
                        var specHelper = new reference();

                        specHelper.getSpecificationSendToNdsStatus(specResponseOnSuccess.Id, specWorkToDoId, 'BAY_EXTENDER');
                    }

                    if (mtlWorkToDoId !== 0) {
                        var mtlHelper = new reference();

                        mtlHelper.getSendToNdsStatus(specResponseOnSuccess.MtlItmId, mtlWorkToDoId);
                    }
                    var backMtrlId = selfspec.backToMtlItmId;
                    if (selfspec.selectRadioSpec() == 'newSpec') {
                        selfspec.selectRadioSpec('existSpec');
                        $("#idProductID").val(specResponseOnSuccess.Id);
                        $('#idspectype').val("BAY_EXTENDER");
                        selfspec.Searchspec();
                        selfspec.specificationSelected(specResponseOnSuccess.Id, 'BAY_EXTENDER', selfspec, backMtrlId);
                        if (backMtrlId > 0) {
                            selfspec.backToMtlItmId = backMtrlId;
                            selfbayextender.enableBackButton(true);
                        }
                        $("#interstitial").hide();
                        return app.showMessage('Successfully updated specification of type Bay Extender<br><b> having Id ' + specResponseOnSuccess.Id + '</b>', 'Specification');
                    } else {
                        selfspec.selectRadioSpec('existSpec');
                        $("#idProductID").val(specResponseOnSuccess.Id);
                        $('#idspectype').val("BAY_EXTENDER");
                        selfspec.updateOnSuccess();
                        if (backMtrlId > 0) {
                            selfspec.backToMtlItmId = backMtrlId;
                            selfbayextender.enableBackButton(true);
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
        }

        BayExtenderSpecification.prototype.saveAuditChanges = function () {
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

        BayExtenderSpecification.prototype.GetModelNameCount = function (modelname, id) {
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

        BayExtenderSpecification.prototype.getConfigNames = function (materialItemID) {
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

        function GetNumber(id, choice) {
            var stuff = { id: id, choice: choice };
            var returnnumber;
            $.ajax({
                type: "GET",
                url: 'api/specn/getnumber/',
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
      
        BayExtenderSpecification.prototype.associatepartBayExtender = function (model, event) {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            selfbayextender.searchtblbayextd(false);
            selfbayextender.associatemtlblck(false);

            document.getElementById('idcdmmsbayextd').value = "";
            document.getElementById('materialcodebayextd').value = "";
            document.getElementById('partnumberbayextd').value = "";
            document.getElementById('clmcbayextd').value = "";
            document.getElementById('catlogdsptbayextd').value = "";
      
            var modal = document.getElementById('bayextdModalpopup');
            var btn = document.getElementById("idAsscociatebayextd");
            var span = document.getElementsByClassName("close")[0];
            var rvsid = selfbayextender.selectedBayExtenderSpec().RvsnId.value;
            var ro = document.getElementById('rcrdlyBayextd').checked ? "Y" : "N";
            var srcd = "BAY_EXTENDER";
            var searchJSON = { RvsnId: rvsid, source: srcd, isRO: ro };

            $.ajax({
                type: "GET",
                url: 'api/specn/getassociatedmtl',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successSearchAssociateddisp,
                error: errorFunc,
                context: selfbayextender,
                async: false
            });

            function errorFunc() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }


            function successSearchAssociateddisp(data, status) {
                selfbayextender.enableModalSave(false);

                if (data === 'no_results') {
                    $("#interstitial").hide();
                   
                } else {
                    var results = JSON.parse(data);
                    selfbayextender.associatemtlblck(results);
                    $("#interstitial").hide();
                }
            }

            // When the user clicks the button, open the modal 
            btn.onclick = function () {
                modal.style.display = "block";
            }

            $("#interstitial").hide();
            $("#bayextdModalpopup").show();

            // When the user clicks on <span> (x), close the modal
            span.onclick = function () {
                modal.style.display = "none";
            }
        };

        BayExtenderSpecification.prototype.CancelAssociateBayextd = function (model, event) {
            selfbayextender.searchtblbayextd(false);
            selfbayextender.associatemtlblck(false);
            document.getElementById('idcdmmsbayextd').value = "";
            document.getElementById('materialcodebayextd').value = "";
            document.getElementById('partnumberbayextd').value = "";
            document.getElementById('clmcbayextd').value = "";
            document.getElementById('catlogdsptbayextd').value = "";
            $("#bayextdModalpopup").hide();
        };

        BayExtenderSpecification.prototype.searchmtlbayextd = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfbayextender.searchtblbayextd(false);
       
            var mtlid = $("#idcdmmsbayextd").val();
            var mtlcode = $("#materialcodebayextd").val();
            var partnumb = $("#partnumberbayextd").val();
            var clmc = $("#clmcbayextd").val();
            var caldsp = $("#catlogdsptbayextd").val();
            var ro = document.getElementById('rcrdlyBayextd').checked ? "Y" : "N";
            var src = "BAY_EXTENDER";

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
                    context: selfbayextender,
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
                        selfbayextender.searchtblbayextd(results);
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
                        if($(this).prop('checked') == true) {
                            selfbayextender.enableModalSave(true);

                            return false;
                        }

                        selfbay.enableModalSave(false);
                    });

                    $('input.checkasstspopsearch').each(function ()
                    {
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
                            selfbayextender.enableModalSave(false);

                            return false;
                        }

                        selfbayextender.enableModalSave(false);
                    });
                });
            });
        };

        BayExtenderSpecification.prototype.SaveAssociateBayextd = function () {
            var chkflag = false;
            var checkBoxes = $("#searchresultbayextd .chkbxsearch");
            var ids = $("#searchresultbayextd .idbayes");
            var specid = selfbayextender.selectedBayExtenderSpec().RvsnId.value;
            var src = "BAY_EXTENDER";
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
                    context: selfbayextender,
                    async: false
                });

            }

            function errorFunc() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }

            function successSearchAssociated(data, status) {
                if (data === ' ') {
                    $("#interstitial").hide();                   
                } else {
                    var bayextdList = JSON.parse(data);
                    var bayextd = { "list": [bayextdList] };
                    var name = bayextd.list[0].Mfg.value + '-' + bayextd.list[0].PrtNo.value;

                    selfbayextender.specName(name);
                    selfbayextender.selectedBayExtenderSpec().BayExtndr = bayextd;
                    selfbayextender.selectedBayExtenderSpec().RvsnNm.value = name;
                    selfbayextender.selectedBayExtenderSpec().Nm.value = name;

                    selfbayextender.selectedBayExtenderSpec(selfbayextender.selectedBayExtenderSpec());

                    $("#bayextdModalpopup").hide();
                    $("#interstitial").hide();
                }
            }
        };

        BayExtenderSpecification.prototype.NumDecimal = function (mp, event) {
            //self.saveValidation(true);
            var charCode = event.keyCode;
            var name = event.currentTarget.id;
            var value = $('#' + name).val();
            var reqCharAfterdot = 3;

            if (name == "hgthnew" || name == "wdthnew" || name == "dpthnew") {
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

                if(value == '.' && len == 1) {
                    $('#' + name).val('0' + value);
                }
            }

            return true;
        };

        return BayExtenderSpecification;
    });