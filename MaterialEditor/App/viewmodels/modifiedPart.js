define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', 'jquerydatatable', 'jqueryui', 'datablescelledit', '../Utility/referenceDataHelper'],
    function (composition, app, ko, $, http, activator, mapping, system, jquerydatatable, jqueryui, datablescelledit, reference) {
        var ModifiedPart = function () {
            self = this;
            self.usr = require('Utility/user');
            self.selectedElement = ko.observable(null);
            self.recordtype = ko.observable("BOTH");
            self.FeatureTypeDD = ko.observable('');
            self.MatCat = ko.observable('');
            self.StatusDD = ko.observable('');
            self.LaborIdDD = ko.observable('');
            self.SetLengthUomDD = ko.observable('');
            self.LctnPosIndDD = ko.observable('');
            self.DefaultPositionSchemeDD = ko.observableArray(['BOTTOM', 'FLOOR', 'TOP']);
            self.PartNumberTypeCodeDD = ko.observableArray(['ATOMIC', 'CONFIGURATION', 'MANUFACTURERS ASSEMBLY', 'PLUG-IN PACKAGE']);
            self.ProdTypeDD = ko.observableArray(['CSYS', 'LOOP', 'MISC', 'POWR', 'RADO', 'SWTC', 'TRAN']);
            self.GaugeUnitDD = ko.observableArray(['NOT APPLICABLE', 'CIRCULAR MILL', 'GAUGE'])
            self.searchBy = ko.observable("MtlCd");
            self.associatetbl = ko.observableArray();
            self.searchtbl = ko.observableArray();
            self.selectedIDs = ko.observableArray();
            self.searchTableRow = ko.observable("");
            self.associatedTableRow = ko.observable("");
            self.selectedMaterialCategory = ko.observable('');
            self.selectedFeatureType = ko.observable('');
            self.selectedCableType = ko.observable('');
            //self.selectIDs = function (item) {
            //    if (self.selectedIDs.indexOf(item.Attributes.material_item_id.value) === -1)
            //        self.selectedIDs.push(item.Attributes.material_item_id.value);
            //    else
            //        self.selectedIDs.remove(item.Attributes.material_item_id.value);
            //    return true;
            //};
            var url = 'api/modifiedpart/dropdown';
            http.get(url + '/FtrTyp').then(function (response) {
                console.log('response = ' + response);
                var results = JSON.parse(response);
                self.FeatureTypeDD(results);
            });
            http.get(url + '/MtlCtgry').then(function (response) {
                console.log('response = ' + response);
                var results = JSON.parse(response);
                self.MatCat(results);
            });
            http.get(url + '/Stts').then(function (response) {
                console.log('response = ' + response);
                var results = JSON.parse(response);
                self.StatusDD(results);
            });

            http.get(url + '/LbrId').then(function (response) {
                console.log('response = ' + response);
                var results = JSON.parse(response);
                self.LaborIdDD(results);
            });

            http.get(url + '/SetLgthUom').then(function (response) {
                console.log('response = ' + response);
                var results = JSON.parse(response);
                self.SetLengthUomDD(results);
            });

            http.get(url + '/LctnPosInd').then(function (response) {
                console.log('response = ' + response);
                var results = JSON.parse(response);
                self.LctnPosIndDD(results);
            });
        };
        function errorFunc(data, status) {
            alert('error');
            $("#interstitial").hide();
        }
        ModifiedPart.prototype.checkEnterVCmodifiedPart = function (root, event) {
            if (event.keyCode === 13) {
                console.log("Enter");
                self.searchmtl(root);
            }
            else { return true; }
        };
        ModifiedPart.prototype.associatecablepopup = function (model, event) {
            var mtlID = event.selectedElement().MaterialID;
            if (mtlID === undefined) {
                mtlID = 0;
            }
            else {
                console.log("hi");
            }
            var clmcpop = document.getElementById("idManufacturer").value;
            $('#clcmcpopup').text(clmcpop);
            var descpop = document.getElementById("idCatalogDescription").value;
            $('#descpopup').text(descpop);
            var partnopop = document.getElementById("idPartNumber").value;
            $('#partnumberpopup').text(partnopop);
            var modal = document.getElementById('myModal');
            var btn = document.getElementById("btnassociate");
            var span = document.getElementsByClassName("close")[0];
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            $.ajax({
                type: "GET",
                url: 'api/variablecable/' + mtlID,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successAssociate,
                error: errorFunc,
                context: self,
                async: false
            });
            function successAssociate(data, status) {
                if (data === '{}') {
                    self.associatetbl(false);
                }
                else {
                    var results = JSON.parse(data);
                    self.associatetbl(results);
                }
                // When the user clicks the button, open the modal 
                btn.onclick = function () {
                    modal.style.display = "block";
                }
                $("#interstitial").hide();
                $("#myModal").show();
                // When the user clicks on <span> (x), close the modal
                span.onclick = function () {
                    self.Reset();
                    modal.style.display = "none";
                }
                // When the user clicks anywhere outside of the modal, close it
                window.onclick = function (event) {
                    if (event.target == modal) {
                        self.Reset();
                        modal.style.display = "none";
                    }
                }
                $(document).ready(function () {
                    $(".chkbxsearch").click(function () {
                        selectedBox = this.id;
                        $(".chkbxsearch").each(function () {
                            if (this.id == selectedBox) {
                                this.checked = true;
                            }
                            else {
                                this.checked = false;
                            };
                        });
                    });
                });
            }
        };
        ModifiedPart.prototype.Reset = function () {
            self.associatetbl(false);
            self.searchtbl(false);
            document.getElementById("idcdmmsidmodal").value = "";
            document.getElementById("materialcodemodal").value = "";
            document.getElementById("partnumbermodal").value = "";
            document.getElementById("clmcmodal").value = "";
            document.getElementById("catlogdsptn").value = "";
        };
        ModifiedPart.prototype.searchmtl = function () {
            self.searchtbl(false);
            var mtlid = document.getElementById("idcdmmsidmodal").value;
            var mtlcode = $("#materialcodemodal").val();
            var partnumb = $("#partnumbermodal").val();
            var clmc = $("#clmcmodal").val();
            var caldsp = $("#catlogdsptn").val();
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            var searchJSON = {
                source: 'ro', material_item_id: mtlid, product_id: mtlcode, mfg_part_no: partnumb, mfg_id: clmc, mat_desc: caldsp
            };
            $.ajax({
                type: "GET",
                url: 'api/variablecable/search',
                data: searchJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successSearch,
                error: errorFunc,
                context: self,
                async: false
            });
            function successSearch(data, status) {
                if (data === 'no_results') {
                    $("#interstitial").hide();
                    $(".NoRecord").show();
                    setTimeout(function () { $(".NoRecord").hide() }, 5000);
                } else {
                    var results = JSON.parse(data);
                    self.searchtbl(results);
                    $("#interstitial").hide();
                    $("#searchresults .chkbxsearchAll").change(function () {
                        $("#searchresults .chkbxsearch").prop('checked', $(this).prop("checked"));

                    });
                }
            }
        };
        ModifiedPart.prototype.saveAssociateCable = function (event) {
            var Selectedrow = ko.observableArray();
            $("#searchresults .chkbxsearch").each(function () {
                if (this.checked == true) {
                    Selectedrow.push(this.value);
                }
            });
            $("#associatedcables .chktblasc").each(function () {
                if (this.checked == true) {
                    Selectedrow.push(this.value);
                }
            });
            var saveop = Selectedrow();
            var mtlID = event.selectedElement().MaterialID;
            var url = 'api/variablecable/update/ro/' + mtlID;
            http.post(url, saveop).then(function (response) {
                console.log(response);
                if (response === 'Success') {
                    app.showMessage('Saved successfully');
                    self.Reset();
                    $("#myModal").hide();
                }
                else {
                    $(".errorAssociateCableModal").show();
                    setTimeout(function () { $(".errorAssociateCableModal").hide() }, 5000);
                }
            },
        function (error) {
            $("#interstitial").hide();
            return app.showMessage('error');
        });
        };
        ModifiedPart.prototype.cancelmodal = function () {
            self.Reset();
            $("#myModal").hide();
        };
        ModifiedPart.prototype.NumDecimal = function (mp, event) {
            var charCode = event.keyCode;
            var name = event.currentTarget.id;
            var value = $('#' + name).val();
            var reqCharAfterdot = 2;
            if (name == "idHeight" || name == "idWidth" || name == "idDepth" || name == "idtxtIntrlHght" || name == "idtxtIntrlWdth" || name == "idtxtIntrlDpth")
            { reqCharAfterdot = 3; }
            if (charCode > 31 && (charCode < 48 || charCode > 57) && !(charCode == 46 || charCode == 8))
            { return false; }
            else {
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
        ModifiedPart.prototype.ShowAssociatedSetLengthCable = function (model, event) {
            var modal = document.getElementById('myModal');
            var btn = document.getElementById("idAssociatedSetLengthCable");
            var span = document.getElementsByClassName("close")[0];
            // When the user clicks the button, open the modal 
            btn.onclick = function () {
                modal.style.display = "block";
            }
            $("#myModal").show();
            // When the user clicks on <span> (x), close the modal
            span.onclick = function () {
                modal.style.display = "none";
            }
            // When the user clicks anywhere outside of the modal, close it
            window.onclick = function (event) {
                if (event.target == modal) {
                    modal.style.display = "none";
                }
            }
        };
        ModifiedPart.prototype.changeMtlCtgry = function (model, event) {
            self.checkMtlCtgry(event.target.value);
            self.selectedMaterialCategory(event.target.value);
            self.filterLaborIds(self.selectedMaterialCategory(), self.selectedFeatureType(), self.selectedCableType());
        };
        ModifiedPart.prototype.checkMtlCtgry = function (event) {
            document.getElementById('btnassociate').disabled = true;
            if (event == "3") {
                document.getElementById('idFeatureType').disabled = false;
                document.getElementById("idSpecificationName").disabled = false;
                document.getElementById("idSpecificationName").required = true;
                //document.getElementById('idFeatureType').value = self.selectedElement().FeatureType;
                //self.checkFtrTyp(self.selectedElement().FeatureType);
            }
            else {
                document.getElementById("idSpecificationName").required = false;
                document.getElementById("idSpecificationName").disabled = true;
                document.getElementById("idSpecificationName").value = '';
                self.selectedElement().SpecificationName = '';
                document.getElementById('idFeatureType').disabled = true;
                document.getElementById('idFeatureType').value = '';
                self.checkFtrTyp('');
            }
            
        };
        ModifiedPart.prototype.changeFtrTyp = function (model, event) {
            self.checkFtrTyp(event.target.value);
            self.selectedFeatureType(event.target.value);
            self.filterLaborIds(self.selectedMaterialCategory(), self.selectedFeatureType(), self.selectedCableType());
        };
        ModifiedPart.prototype.checkFtrTyp = function (event) {
            if (event == "11") {
                if (document.getElementById("idMaterialID").value != 0) {
                    document.getElementById('btnassociate').disabled = false;
                    document.getElementById('associateCblBtnMsg').style.display = "none";
                }
                else { document.getElementById('associateCblBtnMsg').style.display = ""; }
                document.getElementById('idSetLength').disabled = true;
                self.selectedElement().SetLength = '-1';
                document.getElementById('idSetLength').value = -1;
                document.getElementById('idSetLengthUomDD').disabled = true;
                document.getElementById('idSetLengthUomDD').value = 8;
                self.selectedElement().SetLengthUOM = '8';
            }
            else if (event == "9") {
                document.getElementById('idSetLength').value = self.selectedElement().SetLength;
                document.getElementById('idSetLengthUomDD').value = self.selectedElement().SetLengthUOM;
                document.getElementById('idSetLength').disabled = false;
                document.getElementById('idSetLengthUomDD').disabled = false;
                document.getElementById('btnassociate').disabled = true;
            }
            else if (event == "10") {
                document.getElementById('idSetLength').disabled = true;
                document.getElementById('idSetLengthUomDD').disabled = true;
                document.getElementById('idSetLength').value = 0;
                self.selectedElement().SetLength = '0';
                document.getElementById('idSetLengthUomDD').value = 8;
                self.selectedElement().SetLengthUOM = '8';
                document.getElementById('btnassociate').disabled = true;
            }
            else {
                document.getElementById('btnassociate').disabled = true;
                document.getElementById('idSetLength').disabled = true;
                document.getElementById('idSetLengthUomDD').disabled = true;
                document.getElementById('idSetLength').value = '';
                self.selectedElement().SetLength = '';
                document.getElementById('idSetLengthUomDD').value = '';
                self.selectedElement().SetLengthUOM = '';
                document.getElementById('associateCblBtnMsg').style.display = "none";
            }
           
        };
        ModifiedPart.prototype.checkLocatable = function (model, event) {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            var self = this;
            var helper = new reference();
            var selectedValue = event.target.value;
            var results = helper.getFeatureTypeLocatableType(selectedValue).then(function (results) {
                if (results === null || results === '')
                { }
                else if (results === 'Locatable') {
                    document.getElementById("idSpecificationName").disabled = false;
                    document.getElementById("idSpecificationName").required = true;
                }
                else if (results === 'Non Locatable') {
                    document.getElementById("idSpecificationName").required = false;
                    document.getElementById("idSpecificationName").disabled = true;
                    document.getElementById("idSpecificationName").value = '';
                    self.selectedElement().SpecificationName = '';
                }
                $("#interstitial").hide();
            });
        };
        ModifiedPart.prototype.ReqSetLengthUOM = function (x) {
            if (x.selectedElement().SetLength != '') {
                document.getElementById("idSetLengthUomDD").required = true;
            }
            else {
                document.getElementById("idSetLengthUomDD").required = false;
                document.getElementById("idSetLengthUomDD").value = '';
                x.selectedElement().SetLengthUOM = '';
            }
        };
        ModifiedPart.prototype.getfirstword = function (x) {
            var helper = require('Utility/helper');
            var eqpCls = helper.deriveEquipmentClass(document.getElementById("idCatalogDescription").value, document.getElementById("idEquipmentClass").value)
            document.getElementById("idEquipmentClass").value = eqpCls;
            x.selectedElement().EquipmentClass = eqpCls;
        };
        ModifiedPart.prototype.ADD = function (rootcontext) {
            $("#idsearchBar").hide();
            rootcontext.selectedElement({
                HazardousMaterialIndicator: "FALSE", AccountCode: "--7C", LastUpdatedUserID: this.usr.cuid, APCL: "RO"
            });
            $("#idSave").show();
            $("#addOption").show();
            $("#idUpdate").hide();
        };
        ModifiedPart.prototype.SEARCH = function (rootcontext) {
            $("#idsearchBar").show();
            $("#addOption").hide();
        };
        ModifiedPart.prototype.Save = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            var saveJSON = this.selectedElement();
            var urlSU;
            console.log(saveJSON);
            if ($("#idUpdate").is(":visible")) {
                urlSU = 'api/modifiedpart/save/update';
            }
            else {
                urlSU = 'api/modifiedpart/save';
            }
            $.ajax({
                type: "GET",
                url: urlSU,
                data: saveJSON,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: successFunc,
                error: errorFunc,
                context: self
            });
            function successFunc(data, status) {
                $("#interstitial").hide();
                if (data[0] = "success") {
                    app.showMessage('Record only parts are updated successfully. CDMMSID: ' + data[1], 'Saved');
                    self.SEARCH(this);
                    self.partSelected(data[1], this);
                    if (data.length === 3) {
                        var helper = new reference();
                        helper.getSendToNdsStatus(data[1], data[2]);
                    }
                }
                else if (data[1] == "constraint") {
                    app.showMessage('Entered Material Code is already available in the database.', 'Failed');
                }
                else {
                    app.showMessage(data[1], 'Failed');
                }
            }
            function errorFunc() {
                $("#interstitial").hide();
                app.showMessage('Please check inputs again.', 'Failed');
            }
        };
        ModifiedPart.prototype.searchForPart = function (searchValue, bindingObject) {
            console.log('ModifiedPart.searchForPart ' + searchValue);
            $("#addOption").hide();
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            var url = 'api/modifiedpart/search/' + self.recordtype();
            return http.get(url, {
                'val': searchValue, 'searchBy': self.searchBy()
            })
        };
        ModifiedPart.prototype.partSelected = function (selectedId, part) {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            console.log("selectedId = " + selectedId);
            var url = 'api/modifiedpart/searchAll/' + selectedId;
            http.get(url).then(function (response) {
                console.log('response = ' + response);
                var results = JSON.parse(response);
                part.selectedElement(results);                
                setTimeout(function () {
                    self.checkFtrTyp(self.selectedElement().FeatureType);
                    self.checkMtlCtgry(self.selectedElement().MaterialCategory);
                    self.partSelectedOnload();
                }, 1000);
                $("#addOption").show();
                $("#idSave").hide();
                $("#idUpdate").show();
                $("#interstitial").hide();
            });
        };
        ModifiedPart.prototype.partSelectedOnload = function () {
            var partValid = self.selectedElement().PartNumber;
            var partManValid = self.selectedElement().Manufacturer;

            if (partValid.substring(0, 3) == "USW" && (partValid.indexOf("XXX") != -1)) {
                self.selectedElement().MaterialCategory = '3';
                self.selectedElement().FeatureType = '11';
                self.selectedElement().SetLength = '-1';
                self.selectedElement().SetLengthUOM = '5';
                document.getElementById("idMaterialCategory").disabled = false;
                document.getElementById("idFeatureType").disabled = false;
                document.getElementById("idSetLength").disabled = false;
                document.getElementById("idSetLengthUomDD").disabled = false;
                document.getElementById("idSpecificationName").disabled = false;
                document.getElementById("idSpecificationName").required = true;
                document.getElementById("idSpecificationName").value = self.selectedElement().SpecificationName;

            }
            if (partManValid == "OFSB" && (partValid.indexOf("XXX") != -1)) {
                self.selectedElement().MaterialCategory = '3';
                self.selectedElement().FeatureType = '11';
                self.selectedElement().SetLength = '-1';
                self.selectedElement().SetLengthUOM = '8';
                document.getElementById("idMaterialCategory").disabled = false;
                document.getElementById("idFeatureType").disabled = false;
                document.getElementById("idSetLength").disabled = false;
                document.getElementById("idSetLengthUomDD").disabled = false;
                document.getElementById("idSpecificationName").disabled = false;
                document.getElementById("idSpecificationName").required = true;
                document.getElementById("idSpecificationName").value = self.selectedElement().SpecificationName;
            }
            
            this.selectedElement(self.selectedElement());

            document.getElementById('btnassociate').disabled = false;
        };
        ModifiedPart.prototype.filterLaborIds = function (pMtrlCatId, pFeatTypId, pCablTypId) {
            $("#interstitial").show();
            var searchJSON = {
                pMtrlCatId: pMtrlCatId, pFeatTypId: pFeatTypId, pCablTypId: pCablTypId
            };
            http.get('api/assemblyunit/filterLaborIds', searchJSON).then(function (response) {
                console.log(response);
                if ('no_results' === response) {
                    $("#interstitial").hide();
                    app.showMessage("Labor Id list not found").then(function () {
                        return;
                    });
                } else {
                    var results = JSON.parse(response);
                    self.LaborIdDD(results);
                    $("#interstitial").hide();
                }
            },
        function (error) {
            $("#interstitial").hide();
            return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
        });
        };
      /*  ModifiedPart.prototype.changeCableType = function (model, event) {
            self.selectedCableType(event.target.value);           
        };*/
        return ModifiedPart;
    })