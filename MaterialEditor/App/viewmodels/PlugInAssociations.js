
define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', './pluginSpecification', 'durandal/system', 'jqueryui', '../Utility/referenceDataHelper', 'durandal/app', 'datablescelledit', 'bootstrapJS'],
    function (composition, ko, $, http, activator, mapping, pluginSpec, system, jqueryui, reference, app, datablescelledit, bootstrapJS) {
        var PlugInAssociations = function (data) {
            pluginAssoc = this;
            var results = JSON.parse(data);
            var specInfo = results["specType"];
            console.log(results);
            pluginAssoc.usr = require('Utility/user');

            pluginAssoc.AddJsonres = ko.observable();
            pluginAssoc.pluginSpecification = ko.observable();
            pluginAssoc.searchtblplugin = ko.observableArray();
            pluginAssoc.drpRoleType = ko.observableArray();
            pluginAssoc.addedPlugIn = ko.observableArray();
            pluginAssoc.SelectedPlugInArr = ko.observableArray();
            pluginAssoc.selectedRoleType = ko.observable();
            pluginAssoc.txtNodevalue = ko.observable("");
            pluginAssoc.txtNodename = ko.observable("");
            pluginAssoc.txtCardvalue = ko.observable("");
            pluginAssoc.txtCardname = ko.observable("");
            pluginAssoc.actioncode = ko.observable();
            pluginAssoc.actioncode = "INSERT";
            pluginAssoc.getpluginRoleTypes();


            if (specInfo == 'CARD') {
                pluginAssoc.txtNodevalue(results["nodeId"]);
                pluginAssoc.txtCardvalue(results["cardId"]);
                pluginAssoc.txtNodename(results["nodeName"]);
                pluginAssoc.txtCardname(results["cardName"]);

                pluginAssoc.addmtlplugin(results["cardId"]);
            }

            if (typeof ko.unwrap(self.navigateMaterialToSpec) !== 'undefined') {
                if (pluginAssoc.navigateMaterialToSpec() == 'navigateMaterialToSpecNew') {
                    pluginAssoc.selectRadioSpec = ko.observable('newSpec');
                } else {
                    pluginAssoc.selectRadioSpec = ko.observable('existSpec');
                }
            } else {
                pluginAssoc.selectRadioSpec = ko.observable('existSpec');
            }

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
            });
        };

        PlugInAssociations.prototype.getNodeName = function () {

            //    $("#interstitial").css("height", "100%");
            //    $("#interstitial").show();
            //    var searchJSON = "";
            //    var id = $('#txtNodeId').val();
            //    $.ajax({
            //        type: "GET",
            //        url: 'api/specnModel/getNodename/' + id,
            //        data: searchJSON,
            //        contentType: "application/json; charset=utf-8",
            //        dataType: "json",
            //        success: successFunc,
            //        error: errorFunc,
            //        context: pluginAssoc
            //    });
            //    function successFunc(data, status) {
            //        if (data == '') {
            //            $("#interstitial").hide();
            //            app.showMessage("Node name not found", 'Shelf Assignment');
            //            return false;
            //        }
            //        else {
            //            $("#interstitial").hide();
            //            $("#txtNodeId").val(data);
            //            pluginAssoc.txtNodename(data);
            //            return true;
            //        }
            //    }
            //    function errorFunc(data) {
            //        $("#interstitial").hide();
            //        app.showMessage("error : " + data, 'Shelf Assignment');
            //        return false;
            //    }
        };
        PlugInAssociations.prototype.getcardName = function (id) {
            //    var searchJSON = "";
            //    $.ajax({
            //        type: "GET",
            //        url: 'api/specnModel/getCardname/' + id,
            //        data: searchJSON,
            //        contentType: "application/json; charset=utf-8",
            //        dataType: "json",
            //        success: successFunc,
            //        error: errorFunc,
            //        context: pluginAssoc
            //    });
            //    function successFunc(data, status) {
            //        if (data == '') {
            //            $("#interstitial").hide();
            //            app.showMessage("Card name not found", 'Shelf Assignment');
            //            return false;
            //        }
            //        else {
            //            $("#interstitial").hide();
            //            $("#txtCardId").val(data);
            //            pluginAssoc.txtCardname(data);
            //            return true;
            //        }
            //    }
            //    function errorFunc(data) {
            //        $("#interstitial").hide();
            //        app.showMessage("error : " + data, 'Shelf Assignment');
            //        return false;
            //    }
        };

        PlugInAssociations.prototype.getpluginRoleTypes = function () {
            var searchJSON = "";
            $.ajax({
                type: "GET",
                url: 'api/specn/getPlugInroleTypes',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successPlugInRoletypes,
                error: errorFunc,
                context: pluginAssoc
            });

            function errorFunc() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            }

            function successPlugInRoletypes(data, status) {
                if (data === 'no_results') {
                    $("#interstitial").hide();
                    setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
                } else {

                    var results = JSON.parse(data);
                    pluginAssoc.drpRoleType(results);
                    $("#interstitial").hide();
                }

            }
        };

        PlugInAssociations.prototype.searchmtlplugin = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            // pluginAssoc.searchtblplugin(false);
            pluginAssoc.searchtblplugin([]);
            var mtlid = $("#idcdmmsplugin").val();
            var mtlcode = $("#materialcodeplugin").val();
            var partnumb = $("#partnumberplugin").val();
            var clmc = $("#clmcplugin").val();
            var cleiId = $("#cleiplugin").val();
            var caldsp = "";
            var src = "PLUG_IN";
            var roleid = pluginAssoc.selectedRoleType();
            if (mtlid.length > 0 || mtlcode.length > 0 || partnumb.length > 0 || clmc.length > 0 || caldsp.length > 0 || roleid != "" || cleiId.length > 0) {
                var searchJSON = {
                    material_item_id: mtlid, product_id: mtlcode, mfg_part_no: partnumb, mfg_id: clmc, mat_desc: caldsp, source: src, RoleType: roleid, clei: cleiId
                };

                $.ajax({
                    type: "GET",
                    url: 'api/specn/searchmtl',
                    data: searchJSON,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successSearchAssociated,
                    error: errorFunc,
                    context: pluginAssoc,
                    async: false
                });

                function errorFunc() {
                    $("#interstitial").hide();
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                }

                function successSearchAssociated(data, status) {
                    if (data === 'no_results' || data == '{}') {
                        $("#interstitial").hide();
                        $(".NoRecordrp").show();
                        app.showMessage('No Records Found');
                        setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
                    } else {
                        var results = JSON.parse(data);
                        pluginAssoc.searchtblplugin(results);
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
        };


        PlugInAssociations.prototype.addmtlplugin = function () {
            $("#interstitial").css({ "height": "100%" });
            $("#interstitial").show();
            pluginAssoc.addedPlugIn([]);
            if (pluginAssoc.txtCardvalue() == "") {
                alert("Please provide the card id");
                $('#txtCardId').focus();
                return false;
            }
            var searchJSON = "";
            var cardid = pluginAssoc.txtCardvalue();

            //if (pluginAssoc.getcardName(cardid) == false && specInfo != 'CARD') {
            //    return false;
            //}
            $.ajax({
                type: "GET",
                url: 'api/specn/getCardtoPluginAssign/' + cardid,
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successPlugInRoletypes,
                error: errorFunc,
                context: pluginAssoc
            });

            function errorFunc() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            }

            function successPlugInRoletypes(data, status) {
                $("#interstitial").hide();
                if (data === 'no_results' || data == '{}') {
                    app.showMessage("No Records Found");
                } else {
                    setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
                    var results = JSON.parse(data);
                    pluginAssoc.addedPlugIn(results);
                }

                //$(document).ready(function () {
                //    $('input.checkpluginAddsearch').on('change', function () {
                //        $('input.checkpluginAddsearch').not(this).prop('checked', false);
                //        $('input.checkpluginAddsearch').each(function () {
                //            if ($(this).prop('checked') == true) {
                //                alert($(this).val());
                //                return false;
                //            }
                //        });
                //    });
                //});
            }
        };

        PlugInAssociations.prototype.selectedPlugIn = function (item) {
            $("#interstitial").show();
            var selectedId = item.pluginSpecId;
            var specType = 'PLUG_IN';
            var url = 'api/specn/' + selectedId + '/' + specType;

            http.get(url).then(function (response) {
                if (response === "{}") {
                    $("#interstitial").hide();
                    app.showMessage("No record found");
                }
                else {
                    $('#idSpecificationdetails').show();
                    $("#interstitial").hide();
                    var jsonres = {
                        resp: response,
                        specification: true
                    }
                    pluginAssoc.pluginSpecification(new pluginSpec(jsonres));
                }
            });
        };

        PlugInAssociations.prototype.onchkAssociteid = function (item) {
            pluginAssoc.SelectedPlugInArr([]);
            var pluginSpec_Id = 0;
            //alert(" item.plugIn_id.value   " + item.plugIn_id.value + " pluginAssoc.selectedRoleType() : " + pluginAssoc.selectedRoleType());
            if (item.plugIn_id.value === null || item.plugIn_id.value == "") {
                pluginSpec_Id = 0;
            }
            else {
                pluginSpec_Id = parseInt(item.plugIn_id.value);
            }          

            var pluginRoleType = 0;
            if (pluginAssoc.selectedRoleType() === null || pluginAssoc.selectedRoleType() == "") {
                pluginRoleType = 0;
            }
            else {
                pluginRoleType = pluginAssoc.selectedRoleType();
            }
            if (pluginSpec_Id == 0)
            {
                app.showMessage("Plug in specification id not found for this material");
                return false;
            }
            if (pluginRoleType == 0) {
                app.showMessage("Role type not found for this material");
                return false;
            }

            var jsonData = {
                defId: 0,       //passing it as '0' due to integer datatype
                crdSpecId: pluginAssoc.txtCardvalue(),
                pluginSpecId: pluginSpec_Id,
                pluginSpecNm: "",
                pluginRoleTy: pluginRoleType,
                pluginCntrTy: "",
                partNo: item.mfg_part_no.value,
                cleiCd: "",
            };

            var json = ko.toJSON(jsonData);
            var results = JSON.parse(json);
            pluginAssoc.SelectedPlugInArr.push(results);

            return true;

        }

        PlugInAssociations.prototype.AddPluginAssign = function () {

            if (pluginAssoc.SelectedPlugInArr().length == 0) {
                app.showMessage("Please select the material from table using checkbox");
                return false;
            }

            $("#interstitial").css({ "height": "100%" });
            $("#interstitial").show();
            pluginAssoc.actioncode = "INSERT";
            var Jsondata = {
                actioncode: pluginAssoc.actioncode,
                cardId: pluginAssoc.txtCardvalue(),
                plginDtls: pluginAssoc.SelectedPlugInArr()
            };

            pluginAssoc.AddJsonres(Jsondata);
            var saveJSON = mapping.toJS(pluginAssoc.AddJsonres());

            $.ajax({
                type: "POST",
                url: 'api/specn/UpdateCardtoPluginAssign',
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successPlugInAdded,
                error: errorFunc,
                context: pluginAssoc
            });

            function errorFunc() {
                $("#interstitial").hide();
                pluginAssoc.addmtlplugin();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            }

            function successPlugInAdded(data, status) {
                $("#interstitial").hide();
                if (data === 'no_results') {
                    $("#interstitial").hide();
                    $(".NoRecordAdd").html("No Record Found");
                    setTimeout(function () { $(".NoRecordAdd").hide() }, 5000);
                } else {
                    $(".NoRecordAdd").html("Added the record!");
                    setTimeout(function () { $(".NoRecordAdd").show() }, 5000);
                    $("#interstitial").hide();
                    app.showMessage("Added Successfully");
                    pluginAssoc.addmtlplugin();
                }
            }
        };
        return PlugInAssociations;

    });