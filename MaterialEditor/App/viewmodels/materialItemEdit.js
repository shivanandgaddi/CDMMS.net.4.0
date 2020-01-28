define(['durandal/composition', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system',
    './materialItem', './sapMaterialItem', './losdbMaterialItem', 'durandal/app', '../Utility/referenceDataHelper', 'datablescelledit','bootstrapJS'],
    function (composition, ko, $, http, activator, mapping, system, mi, sapMi, losdbMi, app, reference, datablescelledit, bootstrapJS) {
        var MaterialItemEdit = function () {
            selfMtlEdit = this;

            selfMtlEdit.usr = require('Utility/user');
            selfMtlEdit.MIList = ko.observableArray();
            selfMtlEdit.workingMaterial = ko.observable();
            selfMtlEdit.sapMaterial = ko.observable();
            selfMtlEdit.losdbMaterial = ko.observable();
            selfMtlEdit.selectedStatus = ko.observable('');
            selfMtlEdit.selectedFeatureType = ko.observable('');
            selfMtlEdit.selectedCableType = ko.observable('');
            selfMtlEdit.selectedMtlCtgry = ko.observable('');
            selfMtlEdit.mtlCtgryOptions = ko.observable('');
            selfMtlEdit.featureTypeOptions = ko.observable('');
            selfMtlEdit.cableTypeOptions = ko.observable('');
            selfMtlEdit.statusOptions = ko.observable('');
            //selfMtlEdit.selectedStatus = ko.observable('');
            selfMtlEdit.saveValidation = ko.observable(false);
            selfMtlEdit.showModalHLPNMultiBool = ko.observable(true);
            selfMtlEdit.enableModalSaveRtPrtNbrBtn = ko.observable(false);
            selfMtlEdit.verifyRootPartNumber = false;
            selfMtlEdit.checkSearchType = ko.observable("Fuzzy Search");
            selfMtlEdit.workingCategory = ko.observable('');
            selfMtlEdit.workingFeature = ko.observable('');

            selfMtlEdit.cdmmsid = ko.observable('');
            var cdmmsid = sessionStorage.cdmmsid;
            if (typeof cdmmsid !== 'undefined') {
                selfMtlEdit.cdmmsid(cdmmsid);
            }

            selfMtlEdit.mtlcd = ko.observable('');
            var mtlcd = sessionStorage.mtlcd;
            if (typeof mtlcd !== 'undefined') {
                selfMtlEdit.mtlcd(mtlcd);
            }

            selfMtlEdit.partno = ko.observable('');
            var partno = sessionStorage.partno;
            if (typeof partno !== 'undefined') {
                selfMtlEdit.partno(partno);
            }

            selfMtlEdit.desc = ko.observable('');
            var desc = sessionStorage.desc;
            if (typeof desc !== 'undefined') {
                selfMtlEdit.desc(desc);
            }

            selfMtlEdit.clmc = ko.observable('');
            var clmc = sessionStorage.clmc;
            if (typeof clmc !== 'undefined') {
                selfMtlEdit.clmc(clmc);
            }

            selfMtlEdit.specname = ko.observable('');
            var specname = sessionStorage.specname;
            if (typeof specname !== 'undefined') {
                selfMtlEdit.specname(specname);
            }

            selfMtlEdit.cuid = ko.observable('');
            var cuid = sessionStorage.cuid;
            if (typeof cuid !== 'undefined') {
                selfMtlEdit.cuid(cuid);
            }

            selfMtlEdit.heciclei = ko.observable('');
            var heciclei = sessionStorage.heciclei;
            if (typeof heciclei !== 'undefined') {
                selfMtlEdit.heciclei(heciclei);
            }

            selfMtlEdit.startdt = ko.observable('');
            var startdt = sessionStorage.startdt;
            if (typeof startdt !== 'undefined') {
                selfMtlEdit.startdt(startdt);
            }

            selfMtlEdit.enddt = ko.observable('');
            var enddt = sessionStorage.enddt;
            if (typeof enddt !== 'undefined') {
                selfMtlEdit.enddt(enddt);
            }

            selfMtlEdit.itemStatus = ko.observable(false);
            var itemStatus = sessionStorage.itemStatus;
            if (typeof itemStatus !== 'undefined') {
                if (itemStatus === 'true') {
                    selfMtlEdit.itemStatus(true);
                }
                else selfMtlEdit.itemStatus(false);
            }

            var exactMatch = sessionStorage.exactMatch;
            if (typeof exactMatch !== 'undefined') {
                if (exactMatch === 'Y') {
                    selfMtlEdit.checkSearchType("Exact Search");
                }
                else {
                    selfMtlEdit.checkSearchType("Fuzzy Search");
                }
            }
                
            setTimeout(function () {
                $("#startdate").datepicker();
                $("#enddate").datepicker();
            }, 3000);

            var helper = new reference();

            helper.getData('FtrTyp').then(function (data) {
                try {
                    for (var ftrVal in data) {
                        if (data[ftrVal].text === 'Variable Length')
                            data.splice(ftrVal, 1);                             
                    }

                    selfMtlEdit.featureTypeOptions(data);
                    var feattype = sessionStorage.feattype;
                    if (typeof feattype !== 'undefined') {
                        selfMtlEdit.selectedFeatureType('' + feattype);
                    }
                } catch (error) { }
            });

            helper.getData('CblTypId').then(function (data) {
                try {
                    selfMtlEdit.cableTypeOptions(data);
                    var cabletype = sessionStorage.cabletype;
                    if (typeof cabletype !== 'undefined') {
                        selfMtlEdit.selectedCableType('' + cabletype);
                    }
                } catch (error) { }
            });

            helper.getData('Stts').then(function (data) {
                try {
                    selfMtlEdit.statusOptions(data);
                    var stts = sessionStorage.stts;
                    if (typeof stts !== 'undefined') {
                        selfMtlEdit.selectedStatus('' + stts);
                    }
                } catch (error) { }
            });

            helper.getData('MtlCtgry').then(function (data) {
                try {
                    selfMtlEdit.mtlCtgryOptions(data);
                    var mtlcat = sessionStorage.mtlcat;
                    if (typeof mtlcat !== 'undefined') {
                        selfMtlEdit.selectedMtlCtgry('' + mtlcat);
                    }
                } catch (error) { }
            });

            selfMtlEdit.attemptAutoSearch();
        };

        MaterialItemEdit.prototype.attemptAutoSearch = function () {
            selfMtlEdit.autoSearchId = null;
            if (window.location.hash.indexOf('?') === -1 ) {
                return;
            }
            var parms = new URLSearchParams(window.location.hash.split('?')[1]);
            selfMtlEdit.autoSearchId = parms.get('id');

            if ( !selfMtlEdit.autoSearchId ) {
                return;
            }
            
            var searchWhenReady = function () {
                
                if (!document.getElementById('searchFuzzy')) {
                    setTimeout(searchWhenReady, 500);
                    return;
                }

                $("#navbarContainer").hide();
                $("#navigareRecordHide").hide();
                $(".viewContainerPadding").css('padding-top', 10);

                selfMtlEdit.clearMaterialSearch();

                $("#idMaterialSearch").hide();
                $("#idMaterialList").hide();
                $("#idMaterialheader").show();

                
                $('#idcdmmsid').val(selfMtlEdit.autoSearchId);
                $("#searchExact").prop('checked',true);
                selfMtlEdit.searchMethod();
            
                $(".btnSHOW_SEARCH_PANEL").unbind('click').click(function() { 
                    $("#navbarContainer").show();
                    $("#navigareRecordHide").show();
                    $(".viewContainerPadding").css('padding-top', 150);
            
                    $("#idMaterialSearch").show();
                    $("#idMaterialList").show();

                    window.document.title = "Material Inventory | Material Editor";
                    $(this).hide();
                }).show();

                selfMtlEdit.searchForMaterial(selfMtlEdit, function() { 
                    selfMtlEdit.updateWindowTitle();
                });
            };

            setTimeout(searchWhenReady, 500);
        }
        MaterialItemEdit.prototype.updateWindowTitle = function () {
            var title = 'CDMMS: #'+selfMtlEdit.autoSearchId;
            window.document.title = title;
            var timer = null;
            timer = setInterval(function() { 
                var mt = selfMtlEdit.workingMaterial();
                
                if (!mt || !mt.mtl) {
                    return;
                }

                var mtl = mt.mtl;
                window.document.title = 'CDMMS | Material: '+mtl.ItmDesc.value()+ ' (#'+mtl.id.value()+')';
                clearInterval(timer);
            },250);
        }
        MaterialItemEdit.prototype.activate = function (mtlItmId) {
            console.log(mtlItmId + ", " + mtlItmId);

            if (mtlItmId != null && mtlItmId > 0) {               
                selfMtlEdit.materialSelected(mtlItmId, selfMtlEdit);
                setTimeout(function () {
                    document.getElementById("idMaterialheader").style.visibility = "visible";                    
                }, 2000);
                
            }
        };

        MaterialItemEdit.prototype.checkEnter = function (root, event) {
            if (event.keyCode === 13) {
                console.log("Enter");
                selfMtlEdit.searchForMaterial(root);
            }
            else { return true; }
        };

        MaterialItemEdit.prototype.searchMethod = function () {
            var radio = document.getElementById('searchFuzzy');
            if (radio.checked) {  // Fuzzy Match
                document.getElementById('cdmmslabel').style.color = "black";
                document.getElementById('prodidlabel').style.color = "black";
                document.getElementById('partnumberlabel').style.color = "black";
                document.getElementById('useridlabel').style.color = "black";
                document.getElementById('clmclabel').style.color = "black";
            }
            else {  // Exact Match
                document.getElementById('cdmmslabel').style.color = "red";
                document.getElementById('prodidlabel').style.color = "red";
                document.getElementById('partnumberlabel').style.color = "red";
                document.getElementById('useridlabel').style.color = "red";
                document.getElementById('clmclabel').style.color = "red";
            }
        };

        MaterialItemEdit.prototype.Reset = function () {
            selfMtlEdit.MIList(false);
            selfMtlEdit.workingMaterial(false);
            selfMtlEdit.sapMaterial(false);
            selfMtlEdit.losdbMaterial(false);
            selfMtlEdit.saveValidation(false);
            document.getElementById("idMaterialheader").style.visibility = "hidden";

        };

        MaterialItemEdit.prototype.clearMaterialSearch = function () {
            document.getElementById('idPrdctId').value = '';
            document.getElementById('idPrtNo').value = '';
            document.getElementById('idMtlDesc').value = '';
            document.getElementById('idclmc').value = '';
            document.getElementById('idcdmmsid').value = '';
            document.getElementById('statusSearchDropdown').selectedIndex = 0;
            document.getElementById('mtlCtgrySearchDropdown').selectedIndex = 0;
            document.getElementById('ftrTypSearchDropdown').selectedIndex = 0;
            document.getElementById('idSpecificationName').value = '';
            document.getElementById('cabltypSearchdrpdown').selectedIndex = 0;
            document.getElementById('idHeciClei').value = '';
            document.getElementById('userid').value = '';
            document.getElementById('startdate').value = '';
            document.getElementById('enddate').value = '';
            document.getElementById('idItemStatusCheckbox').checked = false;
            sessionStorage.setItem('cdmmsid', '');
            sessionStorage.setItem('mtlcd', '');
            sessionStorage.setItem('partno', '');
            sessionStorage.setItem('desc', '');
            sessionStorage.setItem('clmc', '');
            sessionStorage.setItem('specname', '');
            sessionStorage.setItem('cuid', '');
            sessionStorage.setItem('heciclei', '');
            sessionStorage.setItem('startdt', '');
            sessionStorage.setItem('enddt', '');
            sessionStorage.setItem('stts', '');
            selfMtlEdit.selectedStatus('');
            sessionStorage.setItem('mtlcat', '');
            selfMtlEdit.selectedMtlCtgry('');
            sessionStorage.setItem('feattype', '');
            selfMtlEdit.selectedFeatureType('');
            sessionStorage.setItem('cabletype', '');
            selfMtlEdit.selectedCableType('');
            sessionStorage.setItem('itemStatus', false);
            selfMtlEdit.itemStatus('');
            sessionStorage.setItem('exactMatch', 'N')
            selfMtlEdit.checkSearchType("Fuzzy Search");
        }

        MaterialItemEdit.prototype.searchForMaterial = function (mtl, next) {
            next = (next||function() { });

            selfMtlEdit.Reset();
            selfMtlEdit.showModalHLPNMulti();
            var standaloneCleiSearch = 'N';
            var productId = $("#idPrdctId").val();
            var partNo = $("#idPrtNo").val();
            var mtlDesc = $("#idMtlDesc").val();
            var mfg = $("#idclmc").val();
            var stts = selfMtlEdit.selectedStatus();
            var featureType = selfMtlEdit.selectedFeatureType();
            var cableType = selfMtlEdit.selectedCableType();
            var materialCategory = selfMtlEdit.selectedMtlCtgry();
            var itemStatus = $("#idItemStatus").val();
            var specName = $("#idSpecificationName").val();
            var mtlItemId = $("#idcdmmsid").val();

            sessionStorage.setItem('cdmmsid', mtlItemId);
            sessionStorage.setItem('mtlcd', productId);
            sessionStorage.setItem('partno', partNo);
            sessionStorage.setItem('desc', mtlDesc);
            sessionStorage.setItem('clmc', mfg);
            sessionStorage.setItem('stts', stts);
            sessionStorage.setItem('mtlcat', materialCategory);
            sessionStorage.setItem('feattype', featureType);
            sessionStorage.setItem('cabletype', cableType);
            sessionStorage.setItem('specname', specName);

            var startdt = $("#startdate").val();
            sessionStorage.setItem('startdt', startdt);
            var enddt = $("#enddate").val();
            sessionStorage.setItem('enddt', enddt);
            var userid = $("#userid").val();
            sessionStorage.setItem('cuid', userid);
            var heciclei = $('#idHeciClei').val();
            sessionStorage.setItem('heciclei', heciclei);
            var radio = document.getElementById('searchFuzzy');
            var exactMatch = 'Y';
            var itemStatus = '';
            var itemStatusCheckbox = document.getElementById("idItemStatusCheckbox").checked;
            sessionStorage.setItem('itemStatus', itemStatusCheckbox)
            if (itemStatusCheckbox) {
                itemStatus = 'X';
            }

            if (radio.checked) {
                exactMatch = 'N';
            }
            else exactMatch = 'Y';
            sessionStorage.setItem('exactMatch', exactMatch);

            if (startdt !== '' && enddt === '') {
                app.showMessage('You need to have an end date paired with a start date.');
                return;
            }
            if (startdt === '' && enddt !== '') {
                app.showMessage('You need to have a start date paired with an end date.');
                return;
            }

            if (productId == '' && partNo == '' && mtlDesc == '' && mfg == '' && stts == '' && featureType == ''
                && cableType == '' && materialCategory == '' && itemStatus == '' && specName == '' && mtlItemId == ''
                && userid == '' && startdt == '' && enddt == '') {
                standaloneCleiSearch = 'Y';
            }

            var showMessage = false;
            var searchJSON = {
                PrdctId: productId, PrtNo: partNo, MtlDesc: mtlDesc, clmc: mfg, cdmmsid: mtlItemId, status: stts, MaterialCategory: materialCategory, CableType: cableType, FeatureType: featureType, ItemStatus: itemStatus, SpecificationName: specName, UserID: userid, HeciClei: heciclei, StandaloneCleiSearch: standaloneCleiSearch, ExactMatch: exactMatch, StartDate: startdt, EndDate: enddt
            };

            if (standaloneCleiSearch == 'Y' && heciclei == '') {
                showMessage = true;
            }

            if (productId !== "") {
                if (partNo === "" && mtlDesc === "" && mfg === "" && (stts === "" || stts === undefined) && (cableType === "" || cableType === undefined) && (featureType === "" || featureType === undefined) && itemStatus === "" && specName === "" && mtlItemId === "" && userid === "" && startdt == '' && enddt == '') {
                    if (productId.length <= 2)
                        showMessage = true;
                }
            }
            else if (itemStatus === 'x' || itemStatus === 'X') {
                if (partNo === "" && mtlDesc === "" && mfg === "" && (stts === "" || stts === undefined) && (cableType === "" || cableType === undefined) && (featureType === "" || featureType === undefined) && productId === "" && specName === "" && mtlItemId === "" && userid === "" && startdt === '' && enddt === '') {
                    showMessage = true;
                }
            } else if (partNo === "" && mtlDesc === "" && mfg === "" && stts === "3" && (cableType === "" || cableType === undefined) && (featureType === "" || featureType === undefined) && itemStatus === "" && specName === "" && mtlItemId === "" && startdt === '' && enddt === '') {
                showMessage = true;
            }

            if (showMessage) {
                app.showMessage("Please enter additional search criteria.").then(function () {
                    return;
                });
            }
            else {
                $("#interstitial").css("height", "100%");
                $("#interstitial").show();

                http.get('api/material/searchall/', searchJSON).then(function (response) {
                    console.log(response);
                    if ('no_results' === response) {
                        $("#interstitial").hide();
                        app.showMessage("No results found").then(function () {
                            return;
                        });
                    } else {
                        var results = JSON.parse(response);
                        mtl.MIList(results);
                        setTimeout(function () {
                            document.getElementById('idMaterialList').scrollIntoView();
                            $("#interstitial").hide();
                        }, 1000);
                        if (results.length == 1) {
                            selfMtlEdit.materialSelectCall(results[0], next);
                        }
                    }
                },
                function (error) {
                    $("#interstitial").hide();
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                })
                .always(next)
            }
        };

        //MaterialItemEdit.prototype.searchForMaterial = function (searchValue, bindingObject) {
        //    console.log('MaterialItemEdit.searchForMaterial ' + searchValue);
        //    var searchBy = $("#searchByDropdown")[0].value;
        //    $("#interstitial").css("height", "100%");
        //    $("#interstitial").show();
        //    return http.get('api/material/search/' + searchBy, { 'val': searchValue })
        //};
        MaterialItemEdit.prototype.materialSelectCall = function (selected) {
            selfMtlEdit.saveValidation(false);
            console.log("selectedId = " + selected.Attributes.material_item_id.value);
            document.getElementById("idMaterialheader").style.visibility = "visible";
            var selectedId = selected.Attributes.material_item_id.value;
            selfMtlEdit.materialSelected(selectedId, selfMtlEdit);

        };

        MaterialItemEdit.prototype.materialSelectCalllosdb = function (selectedId) {
           var url = 'api/material/' +selectedId + '/';          

            http.get(url + 'losdb').then(function (response) {
                console.log('response = ' + response);
                var losdbItem;              

                if (response === "{}") {
                    losdbItem = new losdbMi('{"id": {"value": -1}}', false);
                    }
                else {
                   
                    losdbItem = new losdbMi(response, true);
                    }

                selfMtlEdit.losdbMaterial(losdbItem);
                
                },
            function (error) {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            });

            };

        MaterialItemEdit.prototype.materialSelected = function (selectedId, mtl) {
            console.log("selectedId = " + selectedId);
            var url = 'api/material/' + selectedId + '/';

            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            mtl.retrievedCount = 0;

            http.get(url + 'sap').then(function (response) {
                console.log('response = ' + response);
                var sapItem;

                mtl.retrievedCount = mtl.retrievedCount + 1;

                if (response === "{}") {
                    sapItem = new sapMi('{"id": {"value": -1}}');
                }
                else {
                    sapItem = new sapMi(response);
                }

                mtl.sapMaterial(sapItem);
               
                if (mtl.retrievedCount === 2) {
                    selfMtlEdit.scrollToViewMaterialheader();
                }
            },
            function (error) {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            });

            var checkLosbResponse = false;

            http.get(url + 'losdb').then(function (response) {
                console.log('response = ' + response);
                var losdbItem;

                mtl.retrievedCount = mtl.retrievedCount + 1;

                if (response === "{}") {
                    losdbItem = new losdbMi('{"id": {"value": -1}}', false);
                }
                else {
                    checkLosbResponse = true;
                    losdbItem = new losdbMi(response, true);
                }

                mtl.losdbMaterial(losdbItem);

                if (mtl.retrievedCount === 2) {
                    selfMtlEdit.scrollToViewMaterialheader();
                }
            },
            function (error) {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            });

            http.get(url).then(function (response) {
                console.log('response = ' + response);
                var activeMtl;

                mtl.retrievedCount = mtl.retrievedCount + 1;

                if (response === "{}") {
                    activeMtl = new mi('{"id": {"value": -1}}');
                }
                else {
                    activeMtl = new mi(response);

                    if (checkLosbResponse == false) {
                        var losdbItem = new losdbMi(response, false);

                        mtl.losdbMaterial(losdbItem);
                    } else {
                        if (mtl.losdbMaterial().mtl && !mtl.losdbMaterial().mtl.PrdctId) {
                            mtl.losdbMaterial().mtl.PrdctId = activeMtl.mtl.PrdctId;
                        }

                        if (mtl.losdbMaterial().mtl && !mtl.losdbMaterial().mtl.PrtNo) {
                            mtl.losdbMaterial().mtl.PrtNo = activeMtl.mtl.PrtNo;
                        }

                        if (mtl.losdbMaterial().mtl && !mtl.losdbMaterial().mtl.ItmDesc) {
                            mtl.losdbMaterial().mtl.ItmDesc = activeMtl.mtl.ItmDesc;
                        }

                        if (mtl.losdbMaterial().mtl && !mtl.losdbMaterial().mtl.Mfg) {
                            mtl.losdbMaterial().mtl.Mfg = activeMtl.mtl.Mfg;
                        }

                        if (mtl.losdbMaterial().mtl && !mtl.losdbMaterial().mtl.MfgDesc) {
                            mtl.losdbMaterial().mtl.MfgDesc = activeMtl.mtl.MfgDesc;
                        }

                        if (mtl.losdbMaterial().mtl && !mtl.losdbMaterial().mtl.HECI) {
                            mtl.losdbMaterial().mtl.HECI = activeMtl.mtl.HECI;
                        }

                        if (mtl.losdbMaterial().mtl && !mtl.losdbMaterial().mtl.MtrlId) {
                            mtl.losdbMaterial().mtl.MtrlId = activeMtl.mtl.MtrlId;
                        }

                        mtl.losdbMaterial(mtl.losdbMaterial());
                    }
                }

                mtl.workingMaterial(activeMtl);

                if (mtl.retrievedCount === 2) {
                    selfMtlEdit.scrollToViewMaterialheader();
                }
            },
            function (error) {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            });
        };

        MaterialItemEdit.prototype.get = function () {
            //var self = this;
            var item = new mi(99);
            var sapItem = new sapMi(99);
            var losdbItem = new losdbMi(99);

            console.log('Get');

            selfMtlEdit.workingMaterial(item);
            selfMtlEdit.sapMaterial(sapItem);
            selfMtlEdit.losdbMaterial(losdbItem);
        };

        MaterialItemEdit.prototype.enableSaveRtPrtNbrBtn = function () {
            if (document.getElementById("identerrtprtnbrtxt").value !== '') {
                selfMtlEdit.enableModalSaveRtPrtNbrBtn(true);
            } else {
                selfMtlEdit.enableModalSaveRtPrtNbrBtn(false);
            }
        };

        MaterialItemEdit.prototype.cancelEnterRtPrtNbrModal = function () {
            $("#enterRtPrtNoModal").hide();
        };

        MaterialItemEdit.prototype.saveMaterialWithUpdatedRtPrtNo = function () {
            selfMtlEdit.workingMaterial().mtl.RtPrtNbr.value(document.getElementById("identerrtprtnbrtxt").value);

            $("#interstitial").show();

            this.checkRootPartNumber(selfMtlEdit.workingMaterial());
        };

        MaterialItemEdit.prototype.checkRootPartNumber = function (workingMtl) {
            var part = { prtNbr: workingMtl.mtl.RtPrtNbr.value(), mfrId: workingMtl.mtl.MfgId.value() };

            $.ajax({
                type: "POST",
                url: 'api/material/rtprtnbrexists',
                data: JSON.stringify(part),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: checkResults,
                error: existsError,
                context: workingMtl
            });

            function checkResults(data, status) {
                if (data === "Y") {
                    var modal = document.getElementById('enterRtPrtNoModal');

                    $("#interstitial").hide();

                    $("#enterRtPrtNoModal").show();
                } else {
                    $("#enterRtPrtNoModal").hide();

                    selfMtlEdit.saveMaterial(this);
                }
            }

            function existsError(err) {
                $("#interstitial").hide();

                return app.showMessage('Unable to save changes to the database due to an internal error. If problem persists please contact your system administrator.');
            }
        };

        MaterialItemEdit.prototype.save = function () {
            var workingMtl = this.workingMaterial();
            var hasError = false;

            // check if material category or feature type is changed
            var workingMaterialCategory = workingMtl.mtl.MtlCtgry.value();
            var selectedMaterialCategory = workingMtl.selectedMaterialCategory();
            var workingFeatureType = workingMtl.mtl.FtrTyp.value();
            var selectedFeatureType = workingMtl.selectedFeatureType();
            var cdmmsId = workingMtl.mtl.id.value();

            if (workingMtl.mtl.MtlCtgry.value() === '') {
                workingMaterialCategory = '0';
            }
            if (selectedMaterialCategory === '') {
                selectedMaterialCategory = '0';
            }
            if (workingMtl.mtl.FtrTyp.value() === '') {
                workingFeatureType = '0';
            }
            if (selectedFeatureType === '') {
                selectedFeatureType = '0';
            }

            if (workingMaterialCategory == 1 && selectedMaterialCategory != 1) {
                var count = selfMtlEdit.GetNumberContainedIn(cdmmsId);
                if (count > 0) {
                    return app.showMessage('This HLPN has ' + count + ' contained-in parts and does not qualify for a Category change unless the contained-in parts are deleted first.');
                }
            }
            if (workingMaterialCategory != '0' &&
                (workingMaterialCategory != selectedMaterialCategory
                || workingFeatureType != selectedFeatureType)) {
                // check if this material is contained-in to HLPN or Common Config
                var check = { materialcategoryid: workingMaterialCategory, featuretypeid: workingFeatureType };
                check = JSON.stringify(check);

                $.ajax({
                    type: "POST",
                    url: 'api/highlevelpart/checkcontainedin/' + cdmmsId,
                    data: check,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: updateSuccess,
                    error: checkError,
                    async: false
                });
                function updateSuccess(response) {
                    var data = JSON.parse(response);
                    if (data.length && data.length > 0) {
                        hasError = true;
                        var display = 'The following list is where this part is contained-in.<br/>Changing the Category or Feature Type will cause a failure.<br/><br/>';
                        var hasCommonConfig = false;
                        if (data[0].indexOf('Common Config') !== -1) {
                            hasCommonConfig = true;
                        }
                        var i = 0;
                        for (var i = 0; i < data.length; i++) {
                            if (hasCommonConfig && data[i].indexOf('Common Config') === -1 && data[i].indexOf('HLPN') !== -1) {
                                display = display + "<br/>";
                                hasCommonConfig = false;
                            }
                            display = display + data[i] + "<br/>";
                        }
                        app.showMessage(display, "Material Editor", ["Close"], true, { style: { width: "1000px" } });
                        return;
                    }
                }
                function checkError() {
                    $("#interstitial").hide();
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                }
            }


            if (hasError == false) {
                var elements = document.querySelectorAll('#txtPrtNbrTypCd,#txtPrdTyp,#txtEqptCls,#lctnPosIndDropdown,#txtAccntCd,#statusDropdown,#ftrTypDropdown,#txtApcl,#lbrIdDropdown');

                if (workingMtl.mtl.MtlCtgry.value() === '3' && workingMtl.mtl.FtrTyp.value() === '') {
                    $("#interstitial").hide();
                    return app.showMessage('Please select a Feature Type.');
                }

            $("#interstitial").css("height", "100%");
            $("#interstitial").show();

            for (var i = elements.length; i--;) {
                elements[i].addEventListener('invalid', function () {
                    this.scrollIntoView(false);
                });
            }
            this.setWorkingMtlNumberValue('txtHght', workingMtl.mtl.Hght);
            this.setWorkingMtlNumberValue('txtWdth', workingMtl.mtl.Wdth);
            this.setWorkingMtlNumberValue('txtDpth', workingMtl.mtl.Dpth);
            this.setWorkingMtlNumberValue('txtConvRt1', workingMtl.mtl.ConvRt1);
            this.setWorkingMtlNumberValue('txtConvRt2', workingMtl.mtl.ConvRt2);

                if (workingMtl.mtl.HasRvsns.bool()) {
                    //if (workingMtl.mtl.MtlCtgry.value() !== workingMtl.selectedMaterialCategory() || workingMtl.mtl.FtrTyp.value() !== workingMtl.selectedFeatureType())
                    if (selfmi.initialFeatureType() !== workingMtl.selectedFeatureType()) {
                        if (workingMtl.mtl.PrtNo.value() !== workingMtl.mtl.RtPrtNbr.value()) {
                            workingMtl.mtl.RtPrtNbr.value(workingMtl.mtl.PrtNo.value());
                        }

                        selfMtlEdit.verifyRootPartNumber = true;
                    } else {
                        selfMtlEdit.verifyRootPartNumber = false;
                    }
                }

            workingMtl.mtl.Stts.value(workingMtl.selectedStatus());
            workingMtl.mtl.MtlCtgry.value(workingMtl.selectedMaterialCategory());
            workingMtl.mtl.FtrTyp.value(workingMtl.selectedFeatureType());
            workingMtl.mtl.LctnPosInd.value(workingMtl.selectedLctnPosInd());
            workingMtl.mtl.LbrId.value(workingMtl.selectedLbrId());
            workingMtl.mtl.SetLgthUom.value(workingMtl.selectedSetLgthUom());
            workingMtl.mtl.Apcl.value(workingMtl.selectedApcl());
            workingMtl.mtl.CblTypId.value(workingMtl.selectedCableType());
            workingMtl.mtl.PlgInRlTyp.value(workingMtl.selectedPlugInType());

                if (selfMtlEdit.verifyRootPartNumber) {
                    this.checkRootPartNumber(workingMtl);
                } else {
                    this.saveMaterial(workingMtl);
                }
            }
        };

        MaterialItemEdit.prototype.GetNumberContainedIn = function (id) {
            var count = 0;
            var url = 'api/highlevelpart/getnumbercontained/' + id;
            $.ajax({
                type: "POST",
                url: url,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: saveGetNumberContainedInSuccess,
                error: saveGetNumberContainedInError,
                async: false
            });
            function saveGetNumberContainedInSuccess(response) {
                count = response;
            }
            function saveGetNumberContainedInError() {
            }
            return count;
            }

        MaterialItemEdit.prototype.saveMaterial = function (workingMtl) {
            var js = mapping.toJS(workingMtl);
            var specCmdPrompt = selfmi.specCmdPromptNavigate();
            var HLPNBtnBoolSave = selfmi.enableHLPNBtnBoolSave();

            js.mtl.cuid = selfMtlEdit.usr.cuid;

            $.ajax({
                type: "POST",
                url: 'api/material/update',
                data: JSON.stringify(js.mtl),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: updateSuccessful,
                error: updateError,
                context: this.workingMaterial
            });

            function updateSuccessful(data, status) {
                if (data.length != 5) {
                    $("#interstitial").hide();

                    var results;

                    try {
                        results = JSON.parse(data);
                    } catch (error) {

                    }

                    if (results && results.msg === 'Root part number already exists.') {
                        this().handleMaterialEditorException(results);
                    }
                    else if (data && data.indexOf('does not currently exist in the CDMMS manufacturer table') > 0) {
                        return app.showMessage(data);
                    }
                    else {
                        return app.showMessage('Unable to save changes to the database due to an internal error. If problem persists please contact your system administrator.');
                    }
                }
                else {
                    var id = this().mtl.id.value();
                    var getUrl = 'api/material/' + id + '/';
                    var workToDoId = data[2];
                    var specWorkToDoId = data[3];
                    var specType = data[4];
                    //var results = JSON.parse(data);

                    $.ajax({
                        type: "GET",
                        url: getUrl,
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: getSuccessful,
                        error: getError,
                        context: this
                    });

                    //if (results.Id > 0) {
                    if (workToDoId !== '0') {
                        var helper = new reference();

                        helper.getSendToNdsStatus(id, workToDoId);
                    }

                    if (specWorkToDoId !== '0') {
                        var specHelper = new reference();
                        var specId = 0;

                        if (this().mtl.SpecRvsnId && this().mtl.SpecRvsnId.value() > 0) {
                            specId = this().mtl.SpecRvsnId.value();
                        } else if (this().mtl.SpecId && this().mtl.SpecId.value() > 0) {
                            specId = this().mtl.SpecId.value();
                        }

                        specHelper.getSpecificationSendToNdsStatus(specId, specWorkToDoId, specType);;
                    }

                    function getSuccessful(response, status) {
                        var activeMtl;

                        if (response === "{}") {
                            activeMtl = new mi('{"id": {"value": -1}}');
                        }
                        else {
                            activeMtl = new mi(response);
                        }

                        this(activeMtl);
                        if (specCmdPrompt) {
                            selfmi.navigateToSpecificationOnSuccess();
                        }
                        if (HLPNBtnBoolSave) {
                            selfmi.enableHLPNBtnBoolSave(false);
                            $("#highLevelPartsModal").show();
                            $("#interstitial").hide();
                            app.showMessage('Material category updated to HLPN');
                        } else {
                            selfMtlEdit.materialSelected(id, selfMtlEdit);
                            $("#interstitial").hide();
                            return app.showMessage('Changes successfully saved to the database.');
                        }
                    }
                }
                function getError() {
                    $("#interstitial").hide();
                }
            }

            function updateError(err) {
                $("#interstitial").hide();

                console.log('UpdateError: ' + err.responseJSON.ExceptionType);

                if (err.responseJSON.ExceptionType === 'CenturyLink.Network.Engineering.Material.Editor.Utility.MaterialEditorException') {
                    //this().exception(false);
                    this().handleMaterialEditorException(err);
                } else {
                    console.log('UpdateError Message: ' + err.responseJSON.ExceptionMessage);

                    if (err && err.responseJSON && err.responseJSON.ExceptionMessage && err.responseJSON.ExceptionMessage.indexOf('does not currently exist in the CDMMS manufacturer table') > 0) {
                        app.showMessage(err.responseJSON.ExceptionMessage);
                    } else {
                        return app.showMessage('Unable to save changes to the database due to an internal error. If problem persists please contact your system administrator.');
                    }
                }
            }
        };
        
        MaterialItemEdit.prototype.sendToNDS = function () {
            var id = this.sapMaterial().mtl.id.value();
            var workToDoId = 0;
            var getUrl = 'api/material/sendToNDS/' + id + '/';
            $.ajax({
                type: "GET",
                url: getUrl,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: updateNDSSuccessful,
                error: updateNDSError,
                context: this
            });
            function updateNDSSuccessful(response, status) {
                workToDoId = response;
                if (workToDoId !== '0') {
                    var helper = new reference();
                    helper.getSendToNdsStatus(id, workToDoId);
                    app.showMessage('Update sent to NDS is successful.')
                }
                else {
                    app.showMessage('Update sent to NDS not successful.');
                }
            }
            function updateNDSError(response, status) {
                app.showMessage('Update sent to NDS not successful.');
            }
        }

        MaterialItemEdit.prototype.setWorkingMtlNumberValue = function (inputName, workingMtlField) {
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

        MaterialItemEdit.prototype.scrollToViewMaterialheader = function () {
            setTimeout(function () {
                document.getElementById('idMaterialheader').scrollIntoView();
                $("#interstitial").hide();
            }, 1000);
        };
       
        MaterialItemEdit.prototype.showModalHLPNMulti = function () {
            $(document).ready(function () {
                // $(".openMultiHlpn").click(function (event) {

                if (selfMtlEdit.showModalHLPNMultiBool()) {
                   
                $(document).on("click", ".openMultiHlpn", function (event) {
                    event.preventDefault();
                    event.stopPropagation();
                    $("#interstitial").show();
                    var idClicked = $(this).closest('tr').attr('id');
                    var currentEvent = $(this);
                    var count = 0;
                    for (var i = 0; i < idClicked.length; i++) {
                        if (idClicked[i] === '_') {
                            count++;
                        }
                    }
                    var styleField = count % 5;
                    var paddingLeft;
                    if (count >= 0) {
                        paddingLeft = 10 * (count + 1);
                    }
                    var eventCdmmsId = idClicked.substring(idClicked.lastIndexOf("_") + 1);

                    http.get('api/highlevelpart/getCIThlp/' + eventCdmmsId).then(function (response) {
                        var results = JSON.parse(response);
                        console.log(results.length);
                        for (var i = 0; i < results.length; i++) {
                            var iconValid = '';
                            if (results[i].has_level.bool) {
                                iconValid = ' <span class="openMultiHlpn glyphicon glyphicon-plus-sign" style="font-size:18px;color:green;cursor: pointer;padding-left:' + paddingLeft + 'px"></span>';
                            } else {
                                iconValid = ''
                            }
                            var relTo = '';
                            if (results[i].parent_part.value == undefined) {
                                relTo = '';
                            } else {
                                relTo = results[i].parent_part.value;
                            }
                            var modalTxt = '<tr id="' + idClicked + '_' + results[i].cin_cdmms_Id.value + '">  ' +

     '                            <td style="text-align: left !important;padding:2px 8px 2px 8px;">' + iconValid + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" data-toggle="tooltip" data-placement="right" title="Level ' + (count + 2) + '"class="contdPart_' + styleField + '">' + results[i].cin_cdmms_Id.value + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + results[i].material_code.value + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + results[i].clmc.value + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + results[i].part_number.value + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + results[i].material_category.value + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + results[i].feature_type.value + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + results[i].mtrlUOM.value + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + relTo + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + results[i].placement_front_rear.value + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + results[i].is_revision.value + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + results[i].ycoord.value + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + results[i].xcoord.value + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + results[i].quantity.value + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + results[i].spare_quantity.value + '</td>  ' +
     '                            <td style="padding:2px 8px 2px 8px" class="contdPart_' + styleField + '">' + results[i].total_quantity.value + '</td>   ' +
     '                            <td style="padding:2px 8px 2px 8px"></td>  ' +
     '                        </tr>  ';
                            currentEvent.closest('tr').after(modalTxt);
                        }
                        currentEvent.removeClass("openMultiHlpn");
                        currentEvent.addClass("closeMultiHlpn");
                        currentEvent.removeClass("glyphicon-plus-sign");
                        currentEvent.addClass("glyphicon-minus-sign");
                        $("#interstitial").hide();
                    },
                     function (error) {
                         $("#interstitial").hide();
                         return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                     });
                });

                $(document).on("click", ".closeMultiHlpn", function (event) {
                            event.preventDefault();
                            event.stopPropagation();
                            $("#interstitial").show();
                            $("[id^=" + $(this).closest('tr').attr('id') + "_" + "]").remove();
                            $(this).addClass("glyphicon-plus-sign");
                            $(this).removeClass("glyphicon-minus-sign");
                            $(this).addClass("openMultiHlpn");
                            $(this).removeClass("closeMultiHlpn");
                            $("#interstitial").hide();
                        });
                selfMtlEdit.showModalHLPNMultiBool(false);
                   
                }
            });
        };

        MaterialItemEdit.prototype.NumDecimal = function (mp, event) {
            if (event.keyCode === 13) {
                console.log("Enter");
                selfMtlEdit.searchForMaterial(mp);
            }

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
        
        return MaterialItemEdit;
    });