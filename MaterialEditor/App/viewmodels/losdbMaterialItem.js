define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system'],
    function (composition, app, ko, $, http, activator, mapping, system) {
        var LOSDBMaterialItem = function (data, flag) {
        selflosdb = this;

        var results = JSON.parse(data);

        selflosdb.mtl = ko.observable();
        selflosdb.srchlosdbresults = ko.observableArray();
        selflosdb.losdbmtl = ko.observableArray();
        selflosdb.electricalItems = ko.observable();
        selflosdb.compatibleClei7Item = ko.observable();
        selflosdb.searchButtonEnabled = ko.observable(false);
        selflosdb.enableModalSave = ko.observable(false);

        if (results.Attributes) {
            selflosdb.mtl = mapping.fromJS(results.Attributes);
        } else {
            selflosdb.mtl = mapping.fromJS(results);
        }

        if (results.ElectricalItems) {
            selflosdb.electricalItems = mapping.fromJS(results.ElectricalItems);
        }

        if (results.CompatibleSevenCLEIItem)
        {
            selflosdb.compatibleClei7Item = mapping.fromJS(results.CompatibleSevenCLEIItem);
        }

        if (flag)
            selflosdb.exists = true;
        else
            selflosdb.exists = false;

        setTimeout(function () {
            $(document).ready(function () {
                $('input.chksearch').on('change', function () {
                    $('input.chksearch').not(this).prop('checked', false);                    
                });              
            });
         }, 2000);
    };

    LOSDBMaterialItem.prototype.enableSearch = function () {
        if (document.getElementById("seachLosdbClei").value !== '' || document.getElementById("seachLosdbCompatible").value !== '' ||
            document.getElementById("seachLosdbPartNum").value !== '' || document.getElementById("seachLosdbClmc").value !== '') {
            selflosdb.searchButtonEnabled(true);
        } else {
            selflosdb.searchButtonEnabled(false);
        }
    };
   
    LOSDBMaterialItem.prototype.searchOpenLosdbMaterial = function () {       
        //var selflosdb = this;
        selflosdb.srchlosdbresults(false);
        $("#interstitial").css("height", "100%");
        $("#interstitial").show();     

        var seachLosdbCompatible = $("#seachLosdbCompatible").val();
        var seachLosdbClei = $("#seachLosdbClei").val();
        var seachLosdbPartNum = $("#seachLosdbPartNum").val();
        var seachLosdbClmc = $("#seachLosdbClmc").val();
        //var seachLosdbDescription = $("#seachLosdbDescription").val();
       
        var searchJSON = {    
            part_no: seachLosdbPartNum,
            clei_cd: seachLosdbClei,  
            //item_desc: seachLosdbDescription,
            compatibleequipmentclei7:seachLosdbCompatible,
            mfr_cd: seachLosdbClmc       
            
        };

        console.log(searchJSON + " search json ");
        console.log(JSON.stringify(searchJSON) + " search json ");

        $.ajax({
            type: "GET",
            url: 'api/material/losdbsearch',
            data: searchJSON,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: successSearch,
            error: errorFunc,
            context: selflosdb,
            async: false
        });
   
        function successSearch(data, status) {
            console.log("Search results: " + data);

            if (data === 'no_results') {
                $("#interstitial").hide();
                $(".NoRecordrp").show();
                setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
            } else {
                var results = JSON.parse(data);

                selflosdb.srchlosdbresults(results);

                $("#interstitial").hide();
            }
        }

        function errorFunc() {
            $("#interstitial").hide();
            return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
        };

        $(document).ready(function () {
            $('input.chksearch').on('change', function () {
                $('input.chksearch').not(this).prop('checked', false);

                $('input.chksearch').each(function () {
                    if ($(this).prop('checked') == true) {
                        selflosdb.enableModalSave(true);

                        return false;
                    }

                    if ($('input.chkassoc').length > 0) {
                        selflosdb.enableModalSave(true);
                    } else {
                        selflosdb.enableModalSave(false);
                    }
                });

                $('input.chkassoc').each(function () {
                    $(this).prop('checked', false);
                });
            });
        });

        $(document).ready(function () {
            $('input.chkassoc').on('change', function () {
                $('input.chksearch').each(function () {
                    $(this).prop('checked', false);
                });

                $('input.chkassoc').each(function () {
                    if ($(this).prop('checked') == true) {
                        selflosdb.enableModalSave(false);

                        return false;
                    } else {
                        selflosdb.enableModalSave(true);

                        return false;
                    }

                    //selflosdb.enableModalSave(false);
                });
            });
        });
    };

    LOSDBMaterialItem.prototype.searchLosdbMaterial = function () {
        $("#interstitial").css("height", "100%");
        $("#interstitial").show();

        selflosdb.srchlosdbresults(false);

        document.getElementById('seachLosdbClei').value = "";
        document.getElementById('seachLosdbCompatible').value = "";
        document.getElementById('seachLosdbPartNum').value = "";
        document.getElementById('seachLosdbClmc').value = "";

        var modal = document.getElementById('searchLosdbMaterialModal');
        var getUrl = 'api/material/assoclosdb/' + selflosdb.mtl.id.value();

        modal.style.display = "block";

        if (selflosdb.exists) {
            $.ajax({
                type: "GET",
                url: getUrl,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: getSuccess,
                error: getError,
                context: selflosdb,
                async: true
            });
        } else {
            $("#interstitial").hide();
        }

        function getSuccess(data, status) {
            console.log("Search results: " + data);

            if (data === 'no_results') {
                $("#interstitial").hide();
            } else {
                var results = JSON.parse(data);

                selflosdb.losdbmtl(results);

                $("#interstitial").hide();
            }

            $(document).ready(function () {
                $('input.chkassoc').on('change', function () {
                    $('input.chksearch').each(function () {
                        $(this).prop('checked', false);
                    });

                    $('input.chkassoc').each(function () {
                        if ($(this).prop('checked') == true) {
                            selflosdb.enableModalSave(false);

                            return false;
                        } else {
                            selflosdb.enableModalSave(true);

                            return false;
                        }

                        //selflosdb.enableModalSave(false);
                    });
                });
            });
        }

        function getError() {
            $("#interstitial").hide();
            console.log('Error in searchLosdbMaterial()');
        };
    };

    LOSDBMaterialItem.prototype.save = function () {
        //var selflosdb = this;
        console.log('Save');
    };

    LOSDBMaterialItem.prototype.saveSearchLosbModal = function (rootContext) {
        var selectedId = document.getElementById('cdmmslosdb').innerHTML;
        var eqptIds = $("#srchlosdbresults .eqptIds");
        var checkBoxes = $("#srchlosdbresults .chksearch");
        var matid = document.getElementById('prodtid').innerHTML;
        var productId = 0;
        var eqptId = 0;
        var saveUrl;

        $("#interstitial").show();
        
        for (var i = 0; i < checkBoxes.length; i++) {
            if (checkBoxes[i].checked == true) {
                productId = checkBoxes[i].value;
                eqptId = eqptIds[i].innerText;

                break;
            }
        }

        saveUrl = 'api/newupdatedparts/associateLTSMtlCd/' + selectedId + '/' + productId + '/' + eqptId;

        if (selflosdb.exists) {
            if (productId == 0)
                saveUrl = saveUrl + '/D';
            else
                saveUrl = saveUrl + '/N';
        }

        $.ajax({
                type: "GET",
                url: saveUrl,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successSearch,
                error: errorFunc,
                context: selflosdb
        });

        function successSearch(data, status) {
            if (data === 'SUCCESS') {
                $("#interstitial").hide();

                app.showMessage("LOSDB material associated successfully");

                $("#searchLosdbMaterialModal").hide();
                
                //self.materialSelectCalllosdb(selectedId);
                selfMtlEdit.materialSelected(selectedId, selfMtlEdit);
            }
            else if (data.indexOf('unique constraint (CDMMS_OWNER.MTRL_ALIAS_VAL_PK) violated') > 0) {
                app.showMessage('Unable to associate this LOSDB part because it is already asssociated to another SAP part.');
                $("#interstitial").hide();
            }
            else {
                app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                $("#interstitial").hide();
            }
        }

        function errorFunc() {
            app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            $("#interstitial").hide();
        };
    };

    LOSDBMaterialItem.prototype.cancelSearchLosbModal = function () {

        var modal = document.getElementById('searchLosdbMaterialModal');
        modal.style.display = "none";
    };

    return LOSDBMaterialItem;
});
