define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', 'jqueryui', '../Utility/referenceDataHelper', 'durandal/app', 'datablescelledit', 'bootstrapJS', '../Utility/user'],
    function (composition, ko, $, http, activator, mapping, system, jqueryui, reference, app, datablescelledit, bootstrapJS, user) {
        var PlugInSpecification = function (data) {
            selfpis = this;
            specChangeArray = [];
            var dataResult = data.resp;
            var results = JSON.parse(dataResult);
            selfpis.specification = data.specification;

            selfpis.selectedPlugInSpec = ko.observable();
            if (results.Plugin != null && results.Plugin.list.length > 0) {
                results.Plugin.list[0].Dpth.value = Number(results.Plugin.list[0].Dpth.value).toFixed(3);
                results.Plugin.list[0].Hght.value = Number(results.Plugin.list[0].Hght.value).toFixed(3);
                results.Plugin.list[0].Wdth.value = Number(results.Plugin.list[0].Wdth.value).toFixed(3);
                // Ensure there is a leading zero when a fraction like .75 comes across.  should display as 0.75
                if (results.Plugin.list[0].Wdth.value < 1 && results.Plugin.list[0].Wdth.value > 0 && results.Plugin.list[0].Wdth.value.substring(0, 1) == '.') {
                    results.Plugin.list[0].Wdth.value = '0' + results.Plugin.list[0].Wdth.value;
                }
                if (results.Plugin.list[0].Hght.value < 1 && results.Plugin.list[0].Hght.value > 0 && results.Plugin.list[0].Hght.value.substring(0, 1) == '.') {
                    results.Plugin.list[0].Hght.value = '0' + results.Plugin.list[0].Hght.value;
                }
                if (results.Plugin.list[0].Dpth.value < 1 && results.Plugin.list[0].Dpth.value > 0 && results.Plugin.list[0].Dpth.value.substring(0, 1) == '.') {
                    results.Plugin.list[0].Dpth.value = '0' + results.Plugin.list[0].Dpth.value;
                }
            }
            selfpis.duplicateSelectedPlgInSpecName = ko.observable();
            if (results.Nm != null) {
                selfpis.duplicateSelectedPlgInSpecName.value = results.Nm.value;
            }
            selfpis.selectedPlugInSpec(results);
            selfpis.searchtblplugin = ko.observableArray();
            selfpis.associatemtlblck = ko.observableArray();
            selfpis.transmissionRatelist = ko.observableArray();
            selfpis.transmissionRatelistTmp = ko.observableArray();
            selfpis.BiDrctnl = ko.observable();
            selfpis.BiDrctnl(selfpis.selectedPlugInSpec().BiDrctnl.value);
            if (selfpis.BiDrctnl() == "Y") {
                $('#divRecTrawave').show();   // No biderectional flow
                $('#divWavelength').hide();
            }
            else {
                $('#divRecTrawave').hide(); // There is an biderectional flow
                $('#divWavelength').show();
            }
            selfpis.transmissionRatelistTmp(selfpis.selectedPlugInSpec().XmsnRt.options);


            var tempTransArr = new Array();
            tempTransArr = selfpis.selectedPlugInSpec().TransmissionRateLst.value.split(",");

            for (var i = 1; i < selfpis.transmissionRatelistTmp().length; i++) {
                //console.log(selfpis.transmissionRatelistTmp()[i].value);
                var traval = selfpis.transmissionRatelistTmp()[i].value;
                var checkedTransVal = false;
                if (tempTransArr.indexOf(traval) >= 0) {
                    checkedTransVal = 'Y';
                }
                else {
                    checkedTransVal = 'N';
                }
                var jsonData = {
                    value: traval,
                    text: selfpis.transmissionRatelistTmp()[i].text,
                    checkVal: checkedTransVal
                }
                var resChk = ko.toJS(jsonData);
                selfpis.transmissionRatelist.push(resChk);
            }

            selfpis.duplicateSelectedPluginSpecName = ko.observable();
            selfpis.duplicateSelectedPluginSpecName.value = selfpis.selectedPlugInSpec().Nm.value;
            setTimeout(function () {
                $(document).ready(function () {
                    $('input.PlgInRlTypIdCbk').on('change', function () {
                        $('input.PlgInRlTypIdCbk').not(this).prop('checked', false);
                        selfpis.selectedPlugInSpec().PlgInRlTypId.value = $(this).val();
                        $("#plugInLstTblDgr").hide();
                    });
                });
            }, 2000);

            if (selfspec.selectRadioSpec() == 'newSpec') {
                selfpis.selectedPlugInSpec().ChnlNo.value = '';
                selfpis.selectedPlugInSpec().HiTmp.value = '';
                selfpis.selectedPlugInSpec().LoTmp.value = '';
                selfpis.selectedPlugInSpec().MxLiteXmsn.value = '';
            }
            if (selfpis.selectedPlugInSpec().BiDrctnl.value == "N") {
                $('#divRecTrawave').hide();
                $('#divWavelength').show();
            } else {
                $('#divRecTrawave').show();
                $('#divWavelength').hide();
            }

            selfpis.chkTransmit = ko.observable();
            selfpis.chkTransmit = selfpis.selectedPlugInSpec().XmsnRt.value;
            selfpis.chkTransmitList = ko.observableArray();
            selfpis.chkTransmitList(['']);
        };

        PlugInSpecification.prototype.updatePluginSpec = function () {
            specChangeArray = [];
            console.log(selfpis.selectedPlugInSpec());

            selfpis.updatePluginSpecCheckboxes();

            var pluginRoleTypeTbl = false;
            selfpis.selectedPlugInSpec().Transrate = "";
            var bayItnlDpthLstCbk = $("#pluginRoleTypeTbl .PlgInRlTypIdCbk");
            //Push all the checked value into seperate list
            selfpis.chkTransmitList(['']);
            for (var i = 0; i < selfpis.transmissionRatelist().length; i++) {
                if (selfpis.transmissionRatelist()[i].checkVal == 'Y') {
                    selfpis.chkTransmitList.push(selfpis.transmissionRatelist()[i].value);
                }
            }

            selfpis.selectedPlugInSpec().HiTmp.value = $("#hiTmpTxt").val();
            selfpis.selectedPlugInSpec().LoTmp.value = $("#loTmpTxt").val();
            selfpis.selectedPlugInSpec().ChnlNo.value = $("#chnlNoTxt").val();
            selfpis.selectedPlugInSpec().XmsnRt.options = selfpis.chkTransmitList();
            selfpis.selectedPlugInSpec().Transrate = selfpis.chkTransmitList();
            //for (var i = 0; i < bayItnlDpthLstCbk.length; i++) {
            //    if (bayItnlDpthLstCbk[i].checked == true) {
            //        pluginRoleTypeTbl = true;
            //    }
            //}           

            //if (pluginRoleTypeTbl) {
            //    var txtCondt = '';

            //    if (selfpis.selectedPlugInSpec() != null) {
            //        if (selfpis.selectedPlugInSpec().Nm.value != selfpis.duplicateSelectedPluginSpecName.value) {
            //            txtCondt += "Name changed to <b>" + selfpis.selectedPlugInSpec().Nm.value + '</b> from ' + selfpis.duplicateSelectedPluginSpecName.value + "<br/>";
            //        }
            //    }

            var saveJSON = {
                'tablename': 'plg_in_specn', 'columnname': 'plg_in_specn_nm', 'audittblpkcolnm': 'plg_in_specn_id', 'audittblpkcolval': selfpis.selectedPlugInSpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                'oldcolval': selfpis.duplicateSelectedPlgInSpecName.value,
                'newcolval': selfpis.selectedPlugInSpec().Nm.value,
                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfpis.selectedPlugInSpec().Nm.value + ' Spec Name from ' + selfpis.duplicateSelectedPlgInSpecName.value + ' on ', 'materialitemid': selfpis.selectedPlugInSpec().RvsnId.value
            };
            specChangeArray.push(saveJSON);

            //    $("#interstitial").hide();

            //var configNameList = selfpis.getConfigNames(selfpis.selectedPlugInSpec().RvsnId.value);
            //if (configNameList.length > 0) {
            //    txtCondt += "<br/><br/>The following Common Configs would be affected by a change to this spec:<br/>"
            //    for (var i = 0; i < configNameList.length; i++) {
            //        txtCondt += configNameList[i] + "<br/>";
            //    }
            //}
            //    if (txtCondt.length > 0) {
            //        app.showMessage(txtCondt, 'Update Confirmation for Plug-In', ['Ok', 'Cancel']).then(function (dialogResult) {
            //            if (dialogResult == 'Ok') {
            selfpis.savePlugInExSpec();
            //            }
            //        });
            //    } else {
            //        selfpis.savePlugInExSpec();
            //    }
            //} else {
            $("#interstitial").show();
            $("#plugInLstTblDgr").show();
            //}
        };

        PlugInSpecification.prototype.savePlugInExSpec = function () {
            var saveJSON = mapping.toJS(selfpis.selectedPlugInSpec());

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
                    selfpis.saveAuditChanges();
                    if (selfspec.selectRadioSpec() == 'newSpec') {
                        selfspec.selectRadioSpec('existSpec');
                        $("#idProductID").val(specResponseOnSuccess.Id);
                        $('#idspectype').val("PLUG_IN");
                        //selfspec.Searchspec();
                        $("#interstitial").hide();
                        return app.showMessage('Successfully updated specification<br> of type PLUG IN having <b>Id ' + specResponseOnSuccess.Id + '</b>', 'Specification');
                    } else {
                        if (selfpis.specification == true) {
                            //selfspec.updateOnSuccess();
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

        PlugInSpecification.prototype.saveAuditChanges = function () {
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

        PlugInSpecification.prototype.getConfigNames = function (materialItemID) {
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

        PlugInSpecification.prototype.CancelassociatePlugin = function (model, event) {
            selfpis.searchtblplugin(false);
            selfpis.associatemtlblck(false);
            document.getElementById('idcdmmsplugin').value = "";
            document.getElementById('materialcodeplugin').value = "";
            document.getElementById('partnumberplugin').value = "";
            document.getElementById('clmcplugin').value = "";
            document.getElementById('catlogdsptplugin').value = "";
            $("#pluginModalpopup").hide();
        };

        PlugInSpecification.prototype.associatepartPlugin = function (model, event) {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfpis.searchtblplugin(false);
            selfpis.associatemtlblck(false);
            document.getElementById('idcdmmsplugin').value = "";
            document.getElementById('materialcodeplugin').value = "";
            document.getElementById('partnumberplugin').value = "";
            document.getElementById('clmcplugin').value = "";
            document.getElementById('catlogdsptplugin').value = "";

            var modal = document.getElementById('pluginModalpopup');
            var btn = document.getElementById("idAsscociateplugin");
            var span = document.getElementsByClassName("close")[0];
            var rvsid = selfpis.selectedPlugInSpec().RvsnId.value;
            var srcd = "PLUG_IN";
            var ro = "N";
            var searchJSON = {
                RvsnId: rvsid, source: srcd, isRO: ro, specId: selfpis.selectedPlugInSpec().id.value
            };

            $.ajax({
                type: "GET",
                url: 'api/specn/getassociatedmtl',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successSearchAssociateddisp,
                error: errorFunc,
                context: selfpis,
                async: false
            });

            function errorFunc() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            }

            function successSearchAssociateddisp(data, status) {
                if (data === 'no_results') {
                    $("#interstitial").hide();
                    $(".NoRecordrp").show();
                    setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
                } else {

                    var results = JSON.parse(data);
                    selfpis.associatemtlblck(results);
                    $("#interstitial").hide();
                }

            }
            // When the user clicks the button, open the modal 
            btn.onclick = function () {
                modal.style.display = "block";
            }

            $("#interstitial").hide();
            $("#pluginModalpopup").show();

            // When the user clicks on <span> (x), close the modal
            span.onclick = function () {
                modal.style.display = "none";
            }
        };

        PlugInSpecification.prototype.searchmtlplugin = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfpis.searchtblplugin(false);

            var mtlid = $("#idcdmmsplugin").val();
            var mtlcode = $("#materialcodeplugin").val();
            var partnumb = $("#partnumberplugin").val();
            var clmc = $("#clmcplugin").val();
            var caldsp = $("#catlogdsptplugin").val();
            var src = "PLUG_IN";

            if (mtlid.length > 0 || mtlcode.length > 0 || partnumb.length > 0 || clmc.length > 0 || caldsp.length > 0) {
                var searchJSON = {
                    material_item_id: mtlid, product_id: mtlcode, mfg_part_no: partnumb, mfg_id: clmc, mat_desc: caldsp, source: src, RoleType: '', clei: '', isRo: ''
                };

                $.ajax({
                    type: "GET",
                    url: 'api/specn/searchmtl',
                    data: searchJSON,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successSearchAssociated,
                    error: errorFunc,
                    context: selfpis,
                    async: false
                });

                function errorFunc() {
                    $("#interstitial").hide();
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                }

                function successSearchAssociated(data, status) {
                    if (data === 'no_results') {
                        $("#interstitial").hide();
                        $(".NoRecordrp").show();
                        setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
                    } else {

                        var results = JSON.parse(data);
                        selfpis.searchtblplugin(results);
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
                $('input.checkpluginpopsearch').on('change', function () {
                    $('input.checkpluginpopsearch').not(this).prop('checked', false);
                    $('input.checkpluginpopsearch').each(function () {
                        if ($(this).prop('checked') == true) {
                            return false;
                        }
                        else {
                            return true;
                        }
                    });

                });
            });

            $(document).ready(function () {
                $('input.checkasstspopsearch').on('change', function () {
                    $('input.checkasstspopsearch').not(this).prop('checked', true);
                    $('input.checkasstspopsearch').each(function () {
                        if ($(this).prop('checked') == false) {
                            return false;
                        }
                    });

                });
            });
        };

        PlugInSpecification.prototype.NumDecimal = function (mp, event) {
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

        PlugInSpecification.prototype.updatePluginSpecCheckboxes = function () {
            if ($("#VarWvlgthCbk").is(':checked'))
                selfpis.selectedPlugInSpec().VarWvlgth.value = "Y";
            else
                selfpis.selectedPlugInSpec().VarWvlgth.value = "N";

            if ($("#BiDrctnlCbk").is(':checked'))
                selfpis.selectedPlugInSpec().BiDrctnl.value = "Y";
            else
                selfpis.selectedPlugInSpec().BiDrctnl.value = "N";

            if ($("#MultFxWvlgthCbk").is(':checked'))
                selfpis.selectedPlugInSpec().MultFxWvlgth.value = "Y";
            else
                selfpis.selectedPlugInSpec().MultFxWvlgth.value = "N";
        };

        PlugInSpecification.prototype.onchkAssociteid = function (item) {
            var selectedval = item.product_id.value;
            if ($("#idchkplugin").prop('checked') == true) {
                $("#idchkplugin").val("");
                return false;
            }
            else {
                $("#idchkplugin").val(selectedval);
                return true;
            }
        };

        PlugInSpecification.prototype.onchkDeAssociteid = function (item) {
            var selectedval = item.product_id.value;
            if ($("#idchkplugdis").prop('checked') == false) {
                $("#idchkplugdis").val(selectedval);
                return true;
            }
            else {
                $("#idchkplugdis").val("");
                return false;
            }
        };

        PlugInSpecification.prototype.SaveassociatePlugin = function () {

            if ($("#idchkplugin").val() == "") {
                app.showMessage('Please select associte id');
                return false;
            }
            var chkflag = false;
            var checkBoxes = $("#searchresultplugin .checkpluginpopsearch");
            var specid = selfpis.selectedPlugInSpec().id.value;
            var src = "PLUG_IN";
            var mtlcode = $("#idchkplugin").val();
            var saveJSON = {
                material_rev_id: mtlcode, specn_id: specid, Rev_id: selfpis.selectedPlugInSpec().RvsnId.value, source: src
            };
            for (var i = 0; i < checkBoxes.length; i++) {
                if (checkBoxes[i].checked == true) {

                    chkflag = true;
                }
            }
            if (chkflag === true) {
                $.ajax({
                    type: "GET",
                    url: 'api/specn/associatepart',
                    data: saveJSON,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successSearchAssociated,
                    error: errorFunc,
                    context: selfpis,
                    async: false
                });

            }
            var checkBoxesassp = $("#associatedmtl .checkasstspopsearch");
            var mtlcodedis = $("#idchkplugdis").val();
            var src1 = "PLUG_IN";
            var saveJSONdis = {
                material_rev_id: mtlcodedis, source: src1
            };
            var chkflagdis = false;
            for (var i = 0; i < checkBoxesassp.length; i++) {
                if (checkBoxesassp[i].checked == false) {
                    chkflagdis = true;
                }

            }
            if (chkflagdis === true) {
                $.ajax({
                    type: "GET",
                    url: 'api/specn/disassociatepart',
                    data: saveJSONdis,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successSearchAssociated,
                    error: errorFunc,
                    context: selfpis,
                    async: false
                });
            }
            function errorFunc() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            }

            function successSearchAssociated(data, status) {
                if (data === ' ') {
                    $("#interstitial").hide();
                    return app.showMessage('Failure');
                } else {
                    $("#pluginModalpopup").hide();
                    $("#interstitial").hide();
                    return app.showMessage('Success');
                }
            }
        };


        PlugInSpecification.prototype.onchkTransmitrate = function (item) {
            var itemChk;
            var retrunChk;

            if (item.checkVal == 'Y') {
                itemChk = 'N';
                retrunChk = false;
            }
            else {
                itemChk = 'Y';
                retrunChk = true;
            }
            var selectId = item.value;
            for (var i = 0; i < selfpis.transmissionRatelist().length; i++) {
                if (selectId == selfpis.transmissionRatelist()[i].value) {
                    selfpis.transmissionRatelist()[i].checkVal = itemChk;
                    break;
                }
            }

            selfpis.transmissionRatelistTemp = ko.observableArray();
            selfpis.transmissionRatelistTemp(selfpis.transmissionRatelist());
            selfpis.transmissionRatelist([]);
            selfpis.transmissionRatelist(selfpis.transmissionRatelistTemp());

            return retrunChk;
        };

        PlugInSpecification.prototype.onchkBidirectional = function (item) {
            var selectedval = item.selectedPlugInSpec().BiDrctnl.value;
            item.selectedPlugInSpec().BiDrctnl.value = "";
            if (selectedval == 'Y') {
                item.selectedPlugInSpec().BiDrctnl.value = "N";
                selfpis.BiDrctnl('N');
                $('#divRecTrawave').hide();   // No biderectional flow
                $('#divWavelength').show();
                return false;
            }
            else {
                $('#divRecTrawave').show(); // There is an biderectional flow
                $('#divWavelength').hide();
                item.selectedPlugInSpec().BiDrctnl.value = "Y";
                selfpis.BiDrctnl('Y');
                return true;
            }

        };

        PlugInSpecification.prototype.exportPlgInSpecReport = function () {
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
            var plgnSpecHeaders = ["Specification ID", "Specification Name", "Function Code Description"
                           , "Channel No", "Form Factor", "Use Type", "PlugIn Role Type Id", "Transmission Media", "Connector Type"
                           , "Wavelength", "High Temperature (Celsius)", "Low Temperature (Celsius)", "Transmission Distance"
                           , "Distance UOM", "Transmission Rate", "Bi Directional", "Multi Functional Wavelength"
                           , "Variable Wavelength"

            ];							// This array holds the HEADERS text


            for (var i = 0; i < plgnSpecHeaders.length; i++) {											//  Loop all the haders
                //alert(JSON.parse(selfspec.reptDtls[i]).key);
                excel.set(1, i, 0, plgnSpecHeaders[i], formatHeader);									//  Set CELL with header text, using header format
                excel.set(1, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }
            //Writing data into sheet 2 for selected specification
            var i = 0;

            excel.set(1, 0, i + 1, selfpis.selectedPlugInSpec().id.value);                                  // Specification ID
            excel.set(1, 1, i + 1, selfpis.selectedPlugInSpec().Nm.value);                                 // Specification Name
            excel.set(1, 2, i + 1, selfpis.selectedPlugInSpec().FnctnCd.value);                             // Function Code  Description

            excel.set(1, 3, i + 1, selfpis.selectedPlugInSpec().ChnlNo.value);                              // Channel No
            excel.set(1, 4, i + 1, selfpis.selectedPlugInSpec().FrmFctr.value);                             // Form Factor
            excel.set(1, 5, i + 1, selfpis.selectedPlugInSpec().PluginUseTypId.value);                      // Use Type          
            excel.set(1, 6, i + 1, selfpis.selectedPlugInSpec().PlgInRlTypId.value);                        // PlugIn Role Type Id

            var indInt = selfpis.selectedPlugInSpec().XmsnMed.options.map(function (img) { return img.value; }).indexOf(selfpis.selectedPlugInSpec().XmsnMed.value);
            excel.set(1, 7, i + 1, selfpis.selectedPlugInSpec().XmsnMed.options[indInt].text);              // Transmission Media

            var indInt = selfpis.selectedPlugInSpec().CnctrTyp.options.map(function (img) { return img.value; }).indexOf(selfpis.selectedPlugInSpec().CnctrTyp.value);
            excel.set(1, 8, i + 1, selfpis.selectedPlugInSpec().CnctrTyp.options[indInt].text);             // Connector Type

            var indInt = selfpis.selectedPlugInSpec().Wvlgth.options.map(function (img) { return img.value; }).indexOf(selfpis.selectedPlugInSpec().Wvlgth.value);
            excel.set(1, 9, i + 1, selfpis.selectedPlugInSpec().Wvlgth.options[indInt].text);               // Wavelength           
            excel.set(1, 10, i + 1, selfpis.selectedPlugInSpec().HiTmp.value);                              // High Temperature (Celsius)             
            excel.set(1, 11, i + 1, selfpis.selectedPlugInSpec().LoTmp.value);                              // Low Temperature (Celsius)
            excel.set(1, 12, i + 1, selfpis.selectedPlugInSpec().MxLiteXmsn.value);                         // Transmission Distance

            var indInt = selfpis.selectedPlugInSpec().DistUom.options.map(function (img) { return img.value; }).indexOf(selfpis.selectedPlugInSpec().DistUom.value);
            excel.set(1, 13, i + 1, selfpis.selectedPlugInSpec().DistUom.options[indInt].text);             // Distance UOM
            var cnt = 1;
            for (var i = 0; i < selfpis.transmissionRatelist().length; i++) {
                if (selfpis.transmissionRatelist()[i].checkVal == "Y") {
                    excel.set(1, 14, cnt, selfpis.transmissionRatelist()[i].text);                        // Transmission Rate        
                    cnt = cnt + 1;
                }
            }
            var i = 0
            excel.set(1, 15, i + 1, selfpis.selectedPlugInSpec().BiDrctnl.value);                           // Bi Directional
            excel.set(1, 16, i + 1, selfpis.selectedPlugInSpec().MultFxWvlgth.value);                       // Multi Functional Wavelength
            excel.set(1, 17, i + 1, selfpis.selectedPlugInSpec().VarWvlgth.value);                          // Variable Wavelength
            ////End of writing node specification data into sheet 2

            //fetch the associated material details
            selfpis.associatepartPlugin("", "");
            $("#pluginModalpopup").hide();
            //Start writing associate details into sheet 3
            var lineCnt = 0;
            excel.addSheet("Associated Material");                 //Add sheet 3 for selected specification id details 
            //var assoSearchcHeaders = ["CDMMS ID", "Material Code", "CLMC", "Part Number", "Catalog/Material Description"];							// This array holds the HEADERS text
            //var subformat = excel.addStyle({ 															// Format for headers                
            //    font: "Calibri 16 #6495ed B"
            //});

            //var formatHeader = excel.addStyle({ 															// Format for headers
            //    border: "none,thin #333333,thin #333333,thin #333333", 													    // 		Border for header
            //    font: "Calibri 12 #6495ed B"
            //});

            //excel.set(2, 0, lineCnt, "Searched Associate Material", subformat);
            //lineCnt = lineCnt + 1;

            //for (var i = 0; i < assoSearchcHeaders.length; i++) {											//  Loop all the haders               
            //    excel.set(2, i, lineCnt, assoSearchcHeaders[i], formatHeader);									//  Set CELL with header text, using header format
            //    excel.set(2, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            //}

            //if (selfpis.searchtblplugin() != false) {
            //    if (selfpis.searchtblplugin().length > 0) {
            //        for (var i = 0; i < selfpis.searchtblplugin().length; i++) {
            //            excel.set(2, 0, lineCnt + 1, selfpis.searchtblplugin()[i].material_item_id.value);        // CDMMS ID
            //            excel.set(2, 1, lineCnt + 1, selfpis.searchtblplugin()[i].product_id.value);              // Material Code
            //            excel.set(2, 2, lineCnt + 1, selfpis.searchtblplugin()[i].mfg_id.value);                  // CLMC
            //            excel.set(2, 3, lineCnt + 1, selfpis.searchtblplugin()[i].mfg_part_no.value);             // Part Number
            //            excel.set(2, 4, lineCnt + 1, selfpis.searchtblplugin()[i].item_desc.value);               // Catalog/Material Description
            //            lineCnt = lineCnt + 1;
            //        }
            //    }
            //}
            //lineCnt = lineCnt + 2;
            //End of writing associate details into sheet 3

            //Start writing associate details into sheet 3

            var assoSearchcHeaders = ["CDMMS ID", "Material Code", "CLMC", "Part Number", "Catalog/Material Description"];							// This array holds the HEADERS text

            var assocformat = excel.addStyle({ 															// Format for headers                
                font: "Calibri 16 #708090 B"
            });

            var assoformatHeader = excel.addStyle({ 															// Format for headers
                border: "none,thin #333333,thin #333333,thin #333333", 													    // 		Border for header
                font: "Calibri 12 #708090 B"
            });

            excel.set(2, 0, lineCnt, "Associated Materials", assocformat);
            lineCnt = lineCnt + 1;

            for (var i = 0; i < assoSearchcHeaders.length; i++) {											//  Loop all the haders               
                excel.set(2, i, lineCnt, assoSearchcHeaders[i], assoformatHeader);									//  Set CELL with header text, using header format
                excel.set(2, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }

            var i = 0;

            if (selfpis.associatemtlblck() != false) {
                if (selfpis.associatemtlblck().length > 0) {
                    for (var i = 0; i < selfpis.associatemtlblck().length; i++) {
                        excel.set(2, 0, lineCnt + 1, selfpis.associatemtlblck()[i].material_item_id.value);        // CDMMS ID
                        excel.set(2, 1, lineCnt + 1, selfpis.associatemtlblck()[i].product_id.value);              // Material Code
                        excel.set(2, 2, lineCnt + 1, selfpis.associatemtlblck()[i].mfg_id.value);                  // CLMC
                        excel.set(2, 3, lineCnt + 1, selfpis.associatemtlblck()[i].mfg_part_no.value);             // Part Number
                        excel.set(2, 4, lineCnt + 1, selfpis.associatemtlblck()[i].item_desc.value);               // Catalog/Material Description
                        lineCnt = lineCnt + 1;
                    }
                }
            }
            //End of writing associate details into sheet 3

            excel.generate("Report_Plugin_Specification.xlsx");
        };

        return PlugInSpecification;

    });