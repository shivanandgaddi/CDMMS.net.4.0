define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', 'bootstrapJS', 'plugins/router', '../Utility/user'],
    function (composition, app, ko, $, http, activator, mapping, system, bootstrapJS, router, user) {
        var nodeSpecification = function (data) {
            selfnode = this;
            specChangeArray = [];
            var results = JSON.parse(data);

            if (results.Node != null && results.Node.list.length > 0) {
                results.Node.list[0].Dpth.value = Number(results.Node.list[0].Dpth.value).toFixed(3);
                results.Node.list[0].Hght.value = Number(results.Node.list[0].Hght.value).toFixed(3);
                results.Node.list[0].Wdth.value = Number(results.Node.list[0].Wdth.value).toFixed(3);
                // Ensure there is a leading zero when a fraction like .75 comes across.  should display as 0.75
                if (results.Node.list[0].Wdth.value < 1 && results.Node.list[0].Wdth.value > 0 && results.Node.list[0].Wdth.value.substring(0, 1) == '.') {
                    results.Node.list[0].Wdth.value = '0' + results.Node.list[0].Wdth.value;
                }
                if (results.Node.list[0].Hght.value < 1 && results.Node.list[0].Hght.value > 0 && results.Node.list[0].Hght.value.substring(0, 1) == '.') {
                    results.Node.list[0].Hght.value = '0' + results.Node.list[0].Hght.value;
                }
                if (results.Node.list[0].Dpth.value < 1 && results.Node.list[0].Dpth.value > 0 && results.Node.list[0].Dpth.value.substring(0, 1) == '.') {
                    results.Node.list[0].Dpth.value = '0' + results.Node.list[0].Dpth.value;
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
            selfnode.duplicateSelectedNodeSpecDltd = ko.observable();
            selfnode.duplicateSelectedNodeSpecDltd = results.Dltd.bool;
            selfnode.selectedNodeSpec = ko.observable();
            selfnode.selectedNodeSpec(results);
            selfnode.enableAssociate = ko.observable(false);
            selfnode.completedNotSelected = ko.observable(true);
            selfnode.genricblock = ko.observable(false);
            selfnode.enableModalSave = ko.observable(false);
            selfnode.enableName = ko.observable(true);
            selfnode.associatemtlblck = ko.observableArray();
            selfnode.searchtblnode = ko.observableArray();
            selfnode.enableBackButton = ko.observable(false);
            selfnode.specName = ko.observable('');
            selfnode.nongenericBlockview = ko.observable();
            selfnode.structuretype = ko.observableArray([{ value: '', text: '' }, { value: 'S', text: 'Simple' }, { value: 'C', text: 'Complex' }]);

            selfnode.duplicateSelectedNodeSpecDpth = ko.observable();
            selfnode.duplicateSelectedNodeSpecHght = ko.observable();
            selfnode.duplicateSelectedNodeSpecWdth = ko.observable();
            selfnode.duplicateSelectedNodeSpecStrghtThru = ko.observable();
            selfnode.duplicateSelectedNodeSpecMidPln = ko.observable();
            selfnode.duplicateSelectedNodeSpecWllMnt = ko.observable();
            selfnode.duplicateSelectedNodeSpecName = ko.observable();
            selfnode.duplicateSelectedNodeSpecModel = ko.observable();


            if (results.Node != null && results.Node.list.length > 0) {
                selfnode.duplicateSelectedNodeSpecDpth(results.Node.list[0].Dpth.value);
                selfnode.duplicateSelectedNodeSpecHght(results.Node.list[0].Hght.value);
                selfnode.duplicateSelectedNodeSpecWdth(results.Node.list[0].Wdth.value);
            }
            else {
                selfnode.duplicateSelectedNodeSpecDpth(results.Dpth.value);
                selfnode.duplicateSelectedNodeSpecHght(results.Hght.value);
                selfnode.duplicateSelectedNodeSpecWdth(results.Wdth.value);
            }

            if (results.StrghtThru != null) {
                selfnode.duplicateSelectedNodeSpecStrghtThru(results.StrghtThru.value);
            }
            if (results.MidPln != null) {
                selfnode.duplicateSelectedNodeSpecMidPln(results.MidPln.value);
            }
            if (results.WllMnt != null) {
                selfnode.duplicateSelectedNodeSpecWllMnt(results.WllMnt.value);
            }

            if (selfnode.selectedNodeSpec().Cmplt.bool == true && selfnode.selectedNodeSpec().Prpgtd.enable == true) {
                selfnode.completedNotSelected(false);
            }

            if (selfspec.backToMtlItmId > 0) {
                selfnode.enableBackButton(true);
            }

            if (selfnode.selectedNodeSpec().Gnrc.bool == true) {
                selfnode.genricblock(true);
                selfnode.enableName(true);
                selfnode.nongenericBlockview(false);
            }
            else {
                selfnode.genricblock(false);
                selfnode.enableAssociate(true);
                selfnode.nongenericBlockview(true);
            }

            if (selfnode.selectedNodeSpec().Nm.value != "") {
                if (selfnode.selectedNodeSpec().Gnrc.bool == true) {
                    selfnode.specName(selfnode.selectedNodeSpec().Nm.value);
                } else {
                    selfnode.specName(selfnode.selectedNodeSpec().RvsnNm.value);
                }
            }

            if (results.Nm != null) {
                selfnode.duplicateSelectedNodeSpecName.value = selfnode.specName();
                selfnode.duplicateSelectedNodeSpecModel.value = results.Nm.value;
            }

            if (selfnode.selectedNodeSpec().Node !== undefined) {
                if (selfnode.selectedNodeSpec().Node.list[0].ElcCurrDrnNrmUom.value == "" && document.getElementById('txtNrmlDrn')) {
                    document.getElementById('txtNrmlDrn').value = "";
                }

                if (selfnode.selectedNodeSpec().Node.list[0].ElcCurrDrnNrm.value == "" && document.getElementById('idNrmlDrnUom')) {
                    document.getElementById('idNrmlDrnUom').value = "";
                }

                if (selfnode.selectedNodeSpec().Node.list[0].ElcCurrDrnMxUom.value == "" && document.getElementById('txtPkDrn')) {
                    document.getElementById('txtPkDrn').value = "";
                }

                if (selfnode.selectedNodeSpec().Node.list[0].ElcCurrDrnMx.value == "" && document.getElementById('idPkDrnUom')) {
                    document.getElementById('idPkDrnUom').value = "";
                }

                if (selfnode.selectedNodeSpec().Node.list[0].HtDssptnUom.value == "" && document.getElementById('txtPwr')) {
                    document.getElementById('txtPwr').value = "";
                }

                if (selfnode.selectedNodeSpec().Node.list[0].HtDssptn.value == "" && document.getElementById('idPwrUom')) {
                    document.getElementById('idPwrUom').value = "";
                }

                if (selfnode.selectedNodeSpec().Node.list[0].HtGntnUom.value == "" && document.getElementById('txtHtGntn')) {
                    document.getElementById('txtHtGntn').value = "";
                }

                if (selfnode.selectedNodeSpec().Node.list[0].HtGntn.value == "" && document.getElementById('idHtGntnUom')) {
                    document.getElementById('idHtGntnUom').value = "";
                }

                if (selfnode.selectedNodeSpec().Node.list[0].Wght.value == "" && document.getElementById('idWtUom')) {
                    document.getElementById('idWtUom').value = "";
                }

                if (selfnode.selectedNodeSpec().Node.list[0].WghtUom.value == "" && document.getElementById('txtWt')) {
                    document.getElementById('txtWt').value = "";
                }
                setTimeout(function () {
                    document.getElementById('txtDpth').required = true;
                    document.getElementById('txtHght').required = true;
                    document.getElementById('txtWdth').required = true;
                }, 5000);

            }

            $(document).on('change', '[type=checkbox]', function () {
                if (this.name == "gnrcnodechk") {
                    if (this.checked == true) {
                        selfnode.genricblock(true);
                        document.getElementById('rcrdonlynode').disabled = true;
                        selfnode.enableAssociate(false);
                        selfnode.enableName(true);
                    }
                    else {
                        selfnode.genricblock(false);
                        document.getElementById('rcrdonlynode').disabled = false;
                        selfnode.enableAssociate(true);
                        selfnode.enableName(true);
                    }
                }
            });

            $(document).on('change', '[type=checkbox]', function () {
                if (this.name == "cmpIndnmnode") {
                    if (this.checked == false) {
                        document.getElementById('proptIndnode').checked = false;
                    }
                }
            });

            $(document).on('change', '[type=checkbox]', function () {
                if (this.name == "rcrdonlynode") {
                    if (this.checked == true) {
                        document.getElementById('gnrcnodechk').disabled = true;
                    }
                    else {
                        document.getElementById('gnrcnodechk').disabled = false;
                    }
                }
            });

            if (selfspec.selectRadioSpec() == 'newSpec') {
                selfnode.selectedNodeSpec().Wdth.value = '';
                selfnode.selectedNodeSpec().Dpth.value = '';
                selfnode.selectedNodeSpec().Hght.value = '';
                selfnode.selectedNodeSpec().StrghtThru.value = "Y";
            }
            setTimeout(function () {
                if (document.getElementById('idndimuom') !== null) {
                    if (document.getElementById('idndimuom').value == '') {
                        document.getElementById('idndimuom').value = '22';
                    }
                }
                if (document.getElementById('idNodeDimUom') !== null) {
                    if (document.getElementById('idNodeDimUom').value == '') {
                        document.getElementById('idNodeDimUom').value = '22';
                    }
                }
            }, 5000);
        };

        nodeSpecification.prototype.onchangeCompleted = function () {
            if ($("#completedChkBox").is(':checked')) {
                selfnode.completedNotSelected(false);
            } else {
                selfnode.completedNotSelected(true);
                selfnode.selectedNodeSpec().Prpgtd.bool = false;
            }
        };
        nodeSpecification.prototype.navigateToMaterial = function () {
            if (document.getElementById('rcrdonlynode').checked == true) {
                var url = '#/roNew/' + selfspec.backToMtlItmId;
                router.navigate(url, false);
            }
            else {
                var url = '#/mtlInv/' + selfspec.backToMtlItmId;
                router.navigate(url, false);
            }


        };
        nodeSpecification.prototype.NumDecimal = function (mp, event) {
            //self.saveValidation(true);
            var charCode = event.keyCode;
            var name = event.currentTarget.id;
            var value = $('#' + name).val();
            var reqCharAfterdot = 3;

            if (name == "txtHght" || name == "txtWdth" || name == "txtDpth" || name == "idndptno" || name == "idnwdth" || name == "idnhetNo") {
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

        nodeSpecification.prototype.onchangenodegeneric = function () {
            if ($("#gnrcnodechk").is(':checked')) {
                selfnode.genricblock(true);
                selfnode.nongenericBlockview(false);
                selfnode.enableName(true);
                selfnode.selectedNodeSpec().RO.enable = false;
                document.getElementById("idndptno").required = true;
                document.getElementById("idnwdth").required = true;
                document.getElementById("idnhetNo").required = true;
                document.getElementById("idndimuom").required = true;
                document.getElementById("modelTxt").required = false;
            } else {
                selfnode.genricblock(false);
                selfnode.nongenericBlockview(true);
                selfnode.enableName(true);
                selfnode.selectedNodeSpec().RO.enable = true;
                document.getElementById("idndptno").required = false;
                document.getElementById("idnwdth").required = false;
                document.getElementById("idnhetNo").required = false;
                document.getElementById("idndimuom").required = false;
                document.getElementById("modelTxt").required = true;
            }
        };

        nodeSpecification.prototype.SaveNode = function () {
            var chkgenrc = false;
            specChangeArray = [];

            if ($("#gnrcnodechk").is(':checked')) {
                chkgenrc = true;
            }

            if ((chkgenrc) || (!chkgenrc && selfnode.selectedNodeSpec().Node !== undefined)) {
                $("#interstitial").show();
                selfnode.updateChbkCheck();

                if (selfnode.selectedNodeSpec().Gnrc.bool == true)
                    selfnode.selectedNodeSpec().Nm.value = selfnode.specName();
                else
                    selfnode.selectedNodeSpec().RvsnNm.value = selfnode.specName();

                var txtCondt = '';
                setTimeout(function () {
                    if (document.getElementById('idNodeDimUom') !== null) {
                        if (document.getElementById('idNodeDimUom').value == '') {
                            document.getElementById('idNodeDimUom').value = '22';
                        }
                    }
                }, 5000);
                if (selfspec.selectRadioSpec() == 'existSpec') {
                    if (selfnode.selectedNodeSpec().Gnrc.bool == false) {
                        if (selfnode.selectedNodeSpec().Node != null && selfnode.selectedNodeSpec().Node.list.length > 0) {
                            if (Number(selfnode.selectedNodeSpec().Node.list[0].Dpth.value).toFixed(3) != Number(selfnode.duplicateSelectedNodeSpecDpth()).toFixed(3)) {
                                txtCondt += "Depth changed to <b>" + Number(selfnode.selectedNodeSpec().Node.list[0].Dpth.value).toFixed(3) + '</b> from ' + Number(selfnode.duplicateSelectedNodeSpecDpth()).toFixed(3) + "<br/>";

                                var saveJSON = {
                                    'tablename': 'rme_node_mtrl', 'columnname': 'dpth_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfnode.selectedNodeSpec().Node.list[0].MtrlId.value, 'auditprnttblpkcolnm': '',
                                    'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfnode.duplicateSelectedNodeSpecDpth()).toFixed(3), 'newcolval': Number(selfnode.selectedNodeSpec().Node.list[0].Dpth.value).toFixed(3), 'cuid': user.cuid,
                                    'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Depth from ' + Number(selfnode.duplicateSelectedNodeSpecDpth()).toFixed(3) + ' to ' + Number(selfnode.selectedNodeSpec().Node.list[0].Dpth.value).toFixed(3) + ' on ',
                                    'materialitemid': selfnode.selectedNodeSpec().Node.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }
                            if (Number(selfnode.selectedNodeSpec().Node.list[0].Hght.value).toFixed(3) != Number(selfnode.duplicateSelectedNodeSpecHght()).toFixed(3)) {
                                txtCondt += "Height changed to <b>" + Number(selfnode.selectedNodeSpec().Node.list[0].Hght.value).toFixed(3) + '</b> from ' + Number(selfnode.duplicateSelectedNodeSpecHght()).toFixed(3) + "<br/>";

                                var saveJSON = {
                                    'tablename': 'rme_node_mtrl', 'columnname': 'hgt_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfnode.selectedNodeSpec().Node.list[0].MtrlId.value, 'auditprnttblpkcolnm': '',
                                    'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfnode.duplicateSelectedNodeSpecHght()).toFixed(3), 'newcolval': Number(selfnode.selectedNodeSpec().Node.list[0].Hght.value).toFixed(3), 'cuid': user.cuid,
                                    'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Height from ' + Number(selfnode.duplicateSelectedNodeSpecHght()).toFixed(3) + ' to ' + Number(selfnode.selectedNodeSpec().Node.list[0].Hght.value).toFixed(3) + ' on ',
                                    'materialitemid': selfnode.selectedNodeSpec().Node.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }
                            if (Number(selfnode.selectedNodeSpec().Node.list[0].Wdth.value).toFixed(3) != Number(selfnode.duplicateSelectedNodeSpecWdth()).toFixed(3)) {
                                txtCondt += "Width changed to <b>" + Number(selfnode.selectedNodeSpec().Node.list[0].Wdth.value).toFixed(3) + '</b> from ' + Number(selfnode.duplicateSelectedNodeSpecWdth()).toFixed(3) + "<br/>";

                                var saveJSON = {
                                    'tablename': 'rme_node_mtrl', 'columnname': 'wdth_no', 'audittblpkcolnm': 'mtrl_id', 'audittblpkcolval': selfnode.selectedNodeSpec().Node.list[0].MtrlId.value, 'auditprnttblpkcolnm': '',
                                    'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfnode.duplicateSelectedNodeSpecWdth()).toFixed(3), 'newcolval': Number(selfnode.selectedNodeSpec().Node.list[0].Wdth.value).toFixed(3), 'cuid': user.cuid,
                                    'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Width from ' + Number(selfnode.duplicateSelectedNodeSpecWdth()).toFixed(3) + ' to ' + Number(selfnode.selectedNodeSpec().Node.list[0].Wdth.value).toFixed(3) + ' on ',
                                    'materialitemid': selfnode.selectedNodeSpec().Node.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }

                            if (selfnode.duplicateSelectedNodeSpecStrghtThru() != selfnode.selectedNodeSpec().StrghtThru.value) {
                                txtCondt += "Straight Through changed to <b>" + selfnode.selectedNodeSpec().StrghtThru.value + '</b> from ' + selfnode.duplicateSelectedNodeSpecStrghtThru() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'node_specn', 'columnname': 'strght_thru_ind', 'audittblpkcolnm': 'node_specn_id', 'audittblpkcolval': selfnode.selectedNodeSpec().Node.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfnode.duplicateSelectedNodeSpecStrghtThru(),
                                    'newcolval': selfnode.selectedNodeSpec().StrghtThru.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Straight Thru from ' + selfnode.duplicateSelectedNodeSpecStrghtThru() + ' to ' + selfnode.selectedNodeSpec().StrghtThru.value + ' on ', 'materialitemid': selfnode.selectedNodeSpec().Node.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }
                            if (selfnode.duplicateSelectedNodeSpecMidPln() != selfnode.selectedNodeSpec().MidPln.value) {
                                txtCondt += "Mid plane changed to <b>" + selfnode.selectedNodeSpec().MidPln.value + '</b> from ' + selfnode.duplicateSelectedNodeSpecMidPln() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'node_specn', 'columnname': 'mid_pln_ind', 'audittblpkcolnm': 'node_specn_id', 'audittblpkcolval': selfnode.selectedNodeSpec().Node.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfnode.duplicateSelectedNodeSpecMidPln(),
                                    'newcolval': selfnode.selectedNodeSpec().MidPln.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Mid Plane from ' + selfnode.duplicateSelectedNodeSpecMidPln() + ' to ' + selfnode.selectedNodeSpec().MidPln.value + ' on ', 'materialitemid': selfnode.selectedNodeSpec().Node.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }
                            if (selfnode.duplicateSelectedNodeSpecWllMnt() != selfnode.selectedNodeSpec().WllMnt.value) {
                                txtCondt += "Wall Mount changed to <b>" + selfnode.selectedNodeSpec().WllMnt.value + '</b> from ' + selfnode.duplicateSelectedNodeSpecWllMnt() + "<br/>";

                                var saveJSON = {
                                    'tablename': 'node_specn', 'columnname': 'wll_mnt_allow_ind', 'audittblpkcolnm': 'node_specn_id', 'audittblpkcolval': selfnode.selectedNodeSpec().Node.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                    'oldcolval': selfnode.duplicateSelectedNodeSpecWllMnt(),
                                    'newcolval': selfnode.selectedNodeSpec().WllMnt.value,
                                    'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Wall Mount from ' + selfnode.duplicateSelectedNodeSpecWllMnt() + ' to ' + selfnode.selectedNodeSpec().WllMnt.value + ' on ', 'materialitemid': selfnode.selectedNodeSpec().Node.list[0].id.value
                                };
                                specChangeArray.push(saveJSON);
                            }
                        }
                    }
                    else if (selfnode.selectedNodeSpec() != null) {
                        if (selfnode.specName() != selfnode.duplicateSelectedNodeSpecName.value) {
                            txtCondt += "Name changed to <b>" + selfnode.specName() + '</b> from ' + selfnode.duplicateSelectedNodeSpecName.value + "<br/>";

                            var saveJSON = {
                                'tablename': 'node_specn', 'columnname': 'node_specn_nm', 'audittblpkcolnm': 'node_specn_id', 'audittblpkcolval': selfnode.selectedNodeSpec().Node.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                'oldcolval': selfnode.duplicateSelectedNodeSpecName.value,
                                'newcolval': selfnode.specName(),
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Spec Name from ' + selfnode.duplicateSelectedNodeSpecName.value + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (Number(selfnode.selectedNodeSpec().Dpth.value).toFixed(0) != Number(selfnode.duplicateSelectedNodeSpecDpth()).toFixed(0)) {
                            txtCondt += "Depth changed to <b>" + Number(selfnode.selectedNodeSpec().Dpth.value).toFixed(3) + '</b> from ' + Number(selfnode.duplicateSelectedNodeSpecDpth()).toFixed(3) + "<br/>";

                            var saveJSON = {
                                'tablename': 'node_specn_gnrc', 'columnname': 'dpth_no', 'audittblpkcolnm': 'node_specn_id', 'audittblpkcolval': selfnode.selectedNodeSpec().id.value, 'auditprnttblpkcolnm': '',
                                'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfnode.duplicateSelectedNodeSpecDpth()).toFixed(3), 'newcolval': Number(selfnode.selectedNodeSpec().Dpth.value).toFixed(3), 'cuid': user.cuid,
                                'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Depth from ' + Number(selfnode.duplicateSelectedNodeSpecDpth()).toFixed(3) + ' to ' + Number(selfnode.selectedNodeSpec().Dpth.value).toFixed(3) + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (Number(selfnode.selectedNodeSpec().Hght.value).toFixed(0) != Number(selfnode.duplicateSelectedNodeSpecHght()).toFixed(0)) {
                            txtCondt += "Height changed to <b>" + Number(selfnode.selectedNodeSpec().Hght.value).toFixed(3) + '</b> from ' + Number(selfnode.duplicateSelectedNodeSpecHght()).toFixed(3) + "<br/>";

                            var saveJSON = {
                                'tablename': 'node_specn_gnrc', 'columnname': 'hgt_no', 'audittblpkcolnm': 'node_specn_id', 'audittblpkcolval': selfnode.selectedNodeSpec().id.value, 'auditprnttblpkcolnm': '',
                                'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfnode.duplicateSelectedNodeSpecHght()).toFixed(3), 'newcolval': Number(selfnode.selectedNodeSpec().Hght.value).toFixed(3), 'cuid': user.cuid,
                                'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Height from ' + Number(selfnode.duplicateSelectedNodeSpecHght()).toFixed(3) + ' to ' + Number(selfnode.selectedNodeSpec().Hght.value).toFixed(3) + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (Number(selfnode.selectedNodeSpec().Wdth.value).toFixed(0) != Number(selfnode.duplicateSelectedNodeSpecWdth()).toFixed(0)) {
                            txtCondt += "Width changed to <b>" + Number(selfnode.selectedNodeSpec().Wdth.value).toFixed(3) + '</b> from ' + Number(selfnode.duplicateSelectedNodeSpecWdth()).toFixed(3) + "<br/>";

                            var saveJSON = {
                                'tablename': 'node_specn_gnrc', 'columnname': 'wdth_no', 'audittblpkcolnm': 'node_specn_id', 'audittblpkcolval': selfnode.selectedNodeSpec().id.value, 'auditprnttblpkcolnm': '',
                                'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': Number(selfnode.duplicateSelectedNodeSpecWdth()).toFixed(3), 'newcolval': Number(selfnode.selectedNodeSpec().Wdth.value).toFixed(3), 'cuid': user.cuid,
                                'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Width from ' + Number(selfnode.duplicateSelectedNodeSpecWdth()).toFixed(3) + ' to ' + Number(selfnode.selectedNodeSpec().Wdth.value).toFixed(3) + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }

                        if (selfnode.duplicateSelectedNodeSpecStrghtThru() != selfnode.selectedNodeSpec().StrghtThru.value) {
                            txtCondt += "Straight Through changed to <b>" + selfnode.selectedNodeSpec().StrghtThru.value + '</b> from ' + selfnode.duplicateSelectedNodeSpecStrghtThru() + "<br/>";

                            var saveJSON = {
                                'tablename': 'node_specn', 'columnname': 'strght_thru_ind', 'audittblpkcolnm': 'node_specn_id', 'audittblpkcolval': selfnode.selectedNodeSpec().Node.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                'oldcolval': selfnode.duplicateSelectedNodeSpecStrghtThru(),
                                'newcolval': selfnode.selectedNodeSpec().StrghtThru.value,
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Straight Thru from ' + selfnode.duplicateSelectedNodeSpecStrghtThru() + ' to ' + selfnode.selectedNodeSpec().StrghtThru.value + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (selfnode.duplicateSelectedNodeSpecMidPln() != selfnode.selectedNodeSpec().MidPln.value) {
                            txtCondt += "Mid plane changed to <b>" + selfnode.selectedNodeSpec().MidPln.value + '</b> from ' + selfnode.duplicateSelectedNodeSpecMidPln() + "<br/>";

                            var saveJSON = {
                                'tablename': 'node_specn', 'columnname': 'mid_pln_ind', 'audittblpkcolnm': 'node_specn_id', 'audittblpkcolval': selfnode.selectedNodeSpec().Node.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                'oldcolval': selfnode.duplicateSelectedNodeSpecMidPln(),
                                'newcolval': selfnode.selectedNodeSpec().MidPln.value,
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Mid Plane from ' + selfnode.duplicateSelectedNodeSpecMidPln() + ' to ' + selfnode.selectedNodeSpec().MidPln.value + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (selfnode.duplicateSelectedNodeSpecWllMnt() != selfnode.selectedNodeSpec().WllMnt.value) {
                            txtCondt += "Wall Mount changed to <b>" + selfnode.selectedNodeSpec().WllMnt.value + '</b> from ' + selfnode.duplicateSelectedNodeSpecWllMnt() + "<br/>";

                            var saveJSON = {
                                'tablename': 'node_specn', 'columnname': 'wll_mnt_allow_ind', 'audittblpkcolnm': 'node_specn_id', 'audittblpkcolval': selfnode.selectedNodeSpec().Node.list[0].SpecId.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                'oldcolval': selfnode.duplicateSelectedNodeSpecWllMnt(),
                                'newcolval': selfnode.selectedNodeSpec().WllMnt.value,
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Wall Mount from ' + selfnode.duplicateSelectedNodeSpecWllMnt() + ' to ' + selfnode.selectedNodeSpec().WllMnt.value + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                        if (selfnode.selectedNodeSpec().Dltd.bool != selfnode.duplicateSelectedNodeSpecDltd) {
                            txtCondt += "Unusable changed to <b>" + selfnode.selectedNodeSpec().Dltd.bool + '</b> from ' + selfnode.duplicateSelectedNodeSpecDltd + "<br/>";

                            var saveJSON = {
                                'tablename': 'node_specn_gnrc', 'columnname': 'del_ind', 'audittblpkcolnm': 'node_specn_id', 'audittblpkcolval': selfnode.selectedNodeSpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                                'oldcolval': selfnode.duplicateSelectedNodeSpecDltd,
                                'newcolval': selfnode.selectedNodeSpec().Dltd.bool,
                                'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + selfnode.specName() + ' Unusable from ' + selfnode.duplicateSelectedNodeSpecDltd + ' to ' + selfNode.selectedNodeSpec().Dltd.bool + ' on ',
                                'materialitemid': '0'
                            };
                            specChangeArray.push(saveJSON);
                        }
                    }
                }

                var textlength = txtCondt.length;
                if (selfnode.selectedNodeSpec().Node) {
                    var configNameList = selfnode.getConfigNames(selfnode.selectedNodeSpec().Node.list[0].id.value);
                    if (configNameList.length > 0) {
                        txtCondt += "<br/><br/>The following Common Configs would be affected by a change to this spec:<br/><br/>";
                        txtCondt += '<table style=\"border-spacing:5px;\"><tr><th><b>CCID</b></th><th><b>Name</b></th><th><b>Template Name</b></th></tr>';
                        for (var i = 0; i < configNameList.length; i++) {
                            var fields = configNameList[i].split('^');
                            txtCondt += '<tr><td>' + fields[0] + '</td><td style=\"white-space:nowrap\">' + fields[1] + '</td><td style=\"white-space:nowrap\">' + fields[2] + '</td></tr>';
                            //txtCondt += configNameList[i] + "<br/>";
                        }
                        txtCondt += '</table>';;
                    }
                }

                $("#interstitial").hide();
                if (txtCondt.length > 0 && textlength > 0) {
                    app.showMessage(txtCondt, 'Update Confirmation for Node', ['Ok', 'Cancel']).then(function (dialogResult) {
                        if (dialogResult == 'Ok') {
                            selfnode.saveNodeSPec();
                        }
                    });
                } else {
                    selfnode.saveNodeSPec();
                }
            } else {
                $("#interstitial").hide();
                return app.showMessage('Please associate a material item to the specification before saving.', 'Specification');
            }


        };

        nodeSpecification.prototype.saveNodeSPec = function () {
            var saveJSON = mapping.toJS(selfnode.selectedNodeSpec());
            $("#interstitial").hide();
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
                    selfnode.saveAuditChanges();
                    var backMtrlId = selfspec.backToMtlItmId;
                    if (selfspec.selectRadioSpec() == 'newSpec') {
                        selfspec.selectRadioSpec('existSpec');
                        $("#idProductID").val(specResponseOnSuccess.Id);
                        $('#idspectype').val("NODE");
                        selfspec.Searchspec();
                        selfspec.specificationSelected(specResponseOnSuccess.Id, 'NODE', selfspec, backMtrlId);
                        $("#interstitial").hide();
                        if (backMtrlId > 0) {
                            selfspec.backToMtlItmId = backMtrlId;
                            selfnode.enableBackButton(true);
                        }
                        return app.showMessage('Successfully updated specification of type NODE<br><b> having Id ' + specResponseOnSuccess.Id + '</b>', 'Specification');
                    } else {
                        selfspec.selectRadioSpec('existSpec');
                        $("#idProductID").val(specResponseOnSuccess.Id);
                        $('#idspectype').val("NODE");
                        selfspec.updateOnSuccess();
                        $("#interstitial").hide();
                        if (backMtrlId > 0) {
                            selfspec.backToMtlItmId = backMtrlId;
                            selfnode.enableBackButton(true);
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
            function updateError() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }

        };

        nodeSpecification.prototype.saveAuditChanges = function () {
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

        nodeSpecification.prototype.getConfigNames = function (materialItemID) {
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

        nodeSpecification.prototype.updateChbkCheck = function () {

            if ($("#midplnnod").is(':checked'))
                selfnode.selectedNodeSpec().MidPln.value = "Y";
            else
                selfnode.selectedNodeSpec().MidPln.value = "N";

            if ($("#strgtInd").is(':checked'))
                selfnode.selectedNodeSpec().StrghtThru.value = "Y";
            else
                selfnode.selectedNodeSpec().StrghtThru.value = "N";

            if ($("#wllmtInd").is(':checked'))
                selfnode.selectedNodeSpec().WllMnt.value = "Y";
            else
                selfnode.selectedNodeSpec().WllMnt.value = "N";

            if ($("#hasprtnd").is(':checked'))
                selfnode.selectedNodeSpec().Prts.value = "Y";
            else
                selfnode.selectedNodeSpec().Prts.value = "N";

            if ($("#hasshel").is(':checked'))
                selfnode.selectedNodeSpec().Shlvs.value = "Y";
            else
                selfnode.selectedNodeSpec().Shlvs.value = "N";

            //if ($("#escrdred").is(':checked'))
            //    selfnode.selectedNodeSpec().EsPlsCrdReqr.value = "Y";
            //else
            //    selfnode.selectedNodeSpec().EsPlsCrdReqr.value = "N";

            if ($("#ennicpbl").is(':checked'))
                selfnode.selectedNodeSpec().EnniCpbl.value = "Y";
            else
                selfnode.selectedNodeSpec().EnniCpbl.value = "N";

            if ($("#prfmnccpbl").is(':checked'))
                selfnode.selectedNodeSpec().PerfMonitrgCpbl.value = "Y";
            else
                selfnode.selectedNodeSpec().PerfMonitrgCpbl.value = "N";

            if ($("#mutlplxcpb").is(':checked'))
                selfnode.selectedNodeSpec().MuxCpbl.value = "Y";
            else
                selfnode.selectedNodeSpec().MuxCpbl.value = "N";

            if ($("#nodefrmt").is(':checked'))
                selfnode.selectedNodeSpec().NdeFrmtNcludInd.value = "Y";
            else
                selfnode.selectedNodeSpec().NdeFrmtNcludInd.value = "N";

            if ($("#qoscpbl").is(':checked'))
                selfnode.selectedNodeSpec().QoSCpbl.value = "Y";
            else
                selfnode.selectedNodeSpec().QoSCpbl.value = "N";

            if ($("#nvsrallow").is(':checked'))
                selfnode.selectedNodeSpec().NwSrvcAllw.value = "Y";
            else
                selfnode.selectedNodeSpec().NwSrvcAllw.value = "N";


        };
        nodeSpecification.prototype.associatepartNode = function (model, event) {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfnode.searchtblnode(false);
            selfnode.associatemtlblck(false);
            document.getElementById('idcdmmsnode').value = "";
            document.getElementById('materialcodenode').value = "";
            document.getElementById('partnumbernode').value = "";
            document.getElementById('clmcnode').value = "";
            document.getElementById('catlogdsptnode').value = "";

            var modal = document.getElementById('nodeModalpopup');
            var btn = document.getElementById("idAsscociatenode");
            var span = document.getElementsByClassName("close")[0];
            var rvsid = selfnode.selectedNodeSpec().RvsnId.value;
            var ro = document.getElementById('rcrdonlynode').checked ? "Y" : "N";
            var srcd = "NODE";
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
                context: selfnode,
                async: false
            });

            function errorFunc() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }

            function successSearchAssociateddisp(data, status) {
                if (data === 'no_results') {
                    $("#interstitial").hide();
                    //$(".NoRecordrp").show();
                    //setTimeout(function () { $(".NoRecordrp").hide() }, 5000);
                } else {

                    var results = JSON.parse(data);
                    selfnode.associatemtlblck(results);
                    $("#interstitial").hide();
                }

            }
            // When the user clicks the button, open the modal 
            btn.onclick = function () {
                modal.style.display = "block";
            }

            $("#interstitial").hide();
            $("#nodeModalpopup").show();

            // When the user clicks on <span> (x), close the modal
            span.onclick = function () {
                modal.style.display = "none";
            }
        };

        nodeSpecification.prototype.searchmtlnode = function () {
            $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            selfnode.searchtblnode(false);

            var mtlid = $("#idcdmmsnode").val();
            var mtlcode = $("#materialcodenode").val();
            var partnumb = $("#partnumbernode").val();
            var clmc = $("#clmcnode").val();
            var caldsp = $("#catlogdsptnode").val();
            var src = "NODE";
            if (mtlid.length > 0 || mtlcode.length > 0 || partnumb.length > 0 || clmc.length > 0 || caldsp.length > 0) {
                var searchJSON = {
                    material_item_id: mtlid, product_id: mtlcode, mfg_part_no: partnumb, mfg_id: clmc, mat_desc: caldsp, source: src
                };
                $.ajax({
                    type: "GET",
                    url: 'api/specn/searchmtl',
                    data: searchJSON,
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: successSearchAssociated,
                    error: errorFunc,
                    context: selfnode,
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
                        selfnode.searchtblnode(results);

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
                $('input.checknodepopsearch').on('change', function () {
                    $('input.checknodepopsearch').not(this).prop('checked', false);

                    $('input.checknodepopsearch').each(function () {
                        if ($(this).prop('checked') == true) {
                            selfnode.enableModalSave(true);

                            return false;
                        }

                        selfnode.enableModalSave(false);
                    });

                    $('input.checkasstspopsearch').each(function () {
                        $(this).prop('checked', false);
                    });
                });
            });

            $(document).ready(function () {
                $('input.checkasstspopsearch').on('change', function () {
                    $('input.checknodepopsearch').each(function () {
                        $(this).prop('checked', false);
                    });

                    $('input.checkasstspopsearch').each(function () {
                        if ($(this).prop('checked') == true) {
                            selfnode.enableModalSave(false);

                            return false;
                        }

                        selfnode.enableModalSave(false);
                    });
                });
            });


        };
        nodeSpecification.prototype.SaveassociateNode = function () {
            var chkflag = false;
            var checkBoxes = $("#searchresultnode .checknodepopsearch");
            var ids = $("#searchresultnode .idsnode");
            var specid = selfnode.selectedNodeSpec().RvsnId.value;
            var src = "NODE";
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
                    context: selfnode,
                    async: false
                });
            }
            //var checkBoxesassp = $("#associatedmtl .checkasstspopsearch");
            //var mtlcodedis = $("#idchknodedis").val();
            //var src1 = "NODE";
            //var saveJSONdis = {
            //    material_rev_id: mtlcodedis, source: src1
            //};
            //var chkflagdis = false;
            //for (var i = 0; i < checkBoxesassp.length; i++) {
            //    if (checkBoxesassp[i].checked == false) {
            //        chkflagdis = true;
            //    }              
            //}            
            //if (chkflagdis === true)
            //{
            //   $.ajax({
            //       type: "GET",
            //       url: 'api/specn/disassociatepart',
            //       data: saveJSONdis,
            //       contentType: "application/json; charset=utf-8",
            //       dataType: "json",
            //       success: successSearchAssociated,
            //       error: errorFunc,
            //       context: selfnode,
            //       async: false
            //   });

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
                    var nodelist = JSON.parse(data);
                    var node = { "list": [nodelist] };
                    var name = node.list[0].Mfg.value + '-' + node.list[0].PrtNo.value;

                    selfnode.selectedNodeSpec().Node = node;
                    selfnode.selectedNodeSpec().RvsnNm.value = name;
                    selfnode.selectedNodeSpec().Nm.value = name;

                    if (node != null && node.list.length > 0) {
                        selfnode.duplicateSelectedNodeSpecDpth(node.list[0].Dpth.value);
                        selfnode.duplicateSelectedNodeSpecHght(node.list[0].Hght.value);
                        selfnode.duplicateSelectedNodeSpecWdth(node.list[0].Wdth.value);
                    }
                    selfnode.selectedNodeSpec(selfnode.selectedNodeSpec());

                    $("#nodeModalpopup").hide();
                    //return app.showMessage('Success!');
                    $("#interstitial").hide();
                }
            }

        };
        nodeSpecification.prototype.CancelassociateNode = function (model, event) {
            selfnode.searchtblnode(false);
            selfnode.associatemtlblck(false);
            document.getElementById('idcdmmsnode').value = "";
            document.getElementById('materialcodenode').value = "";
            document.getElementById('partnumbernode').value = "";
            document.getElementById('clmcnode').value = "";
            document.getElementById('catlogdsptnode').value = "";
            $("#nodeModalpopup").hide();

        };

        nodeSpecification.prototype.exportNodeSpecReport = function () {
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
            var nodeSpecHeaders = ["Generic", "Record Only", "Completed", "Propagated", "Unusable in NDS", "Specification ID", "Name", "Description"
                           , "Multiplexing Capable", "Performance Monitoring Capable", "ENNI Capable", "Label Position", "Label Position Name", "Structure Type"
                           , "Format Code", "Node Type", "Use Type", "Format Value Qualifier", "Software Version" 
                           , "Depth", "Height", "Width", "Dimensions Unit of Measurement"
                           , "Has Shelves", "Has Ports", "Mid Plane", "Straight Through", "Wall Mount", "QoS Capable", "Include Format Code", "New Service Allowed"

            ];							// This array holds the HEADERS text


            for (var i = 0; i < nodeSpecHeaders.length; i++) {											//  Loop all the haders
                //alert(JSON.parse(selfspec.reptDtls[i]).key);
                excel.set(1, i, 0, nodeSpecHeaders[i], formatHeader);									//  Set CELL with header text, using header format
                excel.set(1, i, undefined, "auto");													    //  Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
            }
            //Writing data into sheet 2 for selected specification
            var i = 0;

            var Gnrc = typeof (selfnode.selectedNodeSpec().Gnrc) === "undefined" ? "" : selfnode.selectedNodeSpec().Gnrc.bool;
            var RO = typeof (selfnode.selectedNodeSpec().RO) === "undefined" ? "" : selfnode.selectedNodeSpec().RO.bool;
            var Cmplt = typeof (selfnode.selectedNodeSpec().Cmplt) === "undefined" ? "" : selfnode.selectedNodeSpec().Cmplt.bool;
            var Prpgtd = typeof (selfnode.selectedNodeSpec().Prpgtd) === "undefined" ? "" : selfnode.selectedNodeSpec().Prpgtd.bool;
            var Dltd = typeof (selfnode.selectedNodeSpec().Dltd) === "undefined" ? "" : selfnode.selectedNodeSpec().Dltd.bool;

            excel.set(1, 0, i + 1, Gnrc);                                                                   // Generic
            excel.set(1, 1, i + 1, RO);                                                                     // Record Only
            excel.set(1, 2, i + 1, Cmplt);                                                                  // Completed
            excel.set(1, 3, i + 1, Prpgtd);                                                                 // Propagated
            excel.set(1, 4, i + 1, Dltd);                                                                   // Unusable in NDS
            excel.set(1, 5, i + 1, selfnode.selectedNodeSpec().id.value);                                   // Specification ID
            excel.set(1, 6, i + 1, selfnode.specName());                                                    // Name
            excel.set(1, 7, i + 1, selfnode.selectedNodeSpec().Desc.value);                                 // Description

            
            excel.set(1, 8, i + 1, selfnode.selectedNodeSpec().MuxCpbl.value);                              // Multiplexing Capable
            excel.set(1, 9, i + 1, selfnode.selectedNodeSpec().PerfMonitrgCpbl.value);                      // Performance Monitoring Capable
            excel.set(1, 10, i + 1, selfnode.selectedNodeSpec().EnniCpbl.value);                            // ENNI Capable

            var indInt = selfnode.selectedNodeSpec().LblPosId.options.map(function (img) { return img.value; }).indexOf(selfnode.selectedNodeSpec().LblPosId.value);
            excel.set(1, 11, i + 1, selfnode.selectedNodeSpec().LblPosId.options[indInt].text);             // Label Position

            excel.set(1, 12, i + 1, selfnode.selectedNodeSpec().LblNm.value);                               // Label Position Name

            var indInt = selfnode.structuretype().map(function (img) { return img.value; }).indexOf(selfnode.selectedNodeSpec().StructType.value);
            excel.set(1, 13, i + 1, selfnode.structuretype()[indInt].text);                                 // Structure Type

            excel.set(1, 14, i + 1, selfnode.selectedNodeSpec().NdeFrmtCd.value);                           // Format Code

            var indInt = selfnode.selectedNodeSpec().NodeTypId.options.map(function (img) { return img.value; }).indexOf(selfnode.selectedNodeSpec().NodeTypId.value);
            excel.set(1, 15, i + 1, selfnode.selectedNodeSpec().NodeTypId.options[indInt].text);            // Node Type             
            excel.set(1, 16, i + 1, selfnode.selectedNodeSpec().NodeUseTypId.value);                        // Use Type

            var indInt = selfnode.selectedNodeSpec().NdeFrmtValQlfrId.options.map(function (img) { return img.value; }).indexOf(selfnode.selectedNodeSpec().NdeFrmtValQlfrId.value);
            excel.set(1, 17, i + 1, selfnode.selectedNodeSpec().NdeFrmtValQlfrId.options[indInt].text);     // Format Value Qualifier
            excel.set(1, 18, i + 1, selfnode.selectedNodeSpec().SwVrsn.value);                              // Software Version
            excel.set(1, 19, i + 1, selfnode.selectedNodeSpec().Dpth.value);                                // Depth
            excel.set(1, 20, i + 1, selfnode.selectedNodeSpec().Hght.value);                                // Height
            excel.set(1, 21, i + 1, selfnode.selectedNodeSpec().Wdth.value);                                // Width

            var indInt = selfnode.selectedNodeSpec().DimUom.options.map(function (img) { return img.value; }).indexOf(selfnode.selectedNodeSpec().DimUom.value);
            excel.set(1, 22, i + 1, selfnode.selectedNodeSpec().DimUom.options[indInt].text);               // Unit of Measurement
                  

            excel.set(1, 23, i + 1, selfnode.selectedNodeSpec().Shlvs.bool);                                // Has Shelves
            excel.set(1, 24, i + 1, selfnode.selectedNodeSpec().Prts.bool);                                 // Has Ports
            excel.set(1, 25, i + 1, selfnode.selectedNodeSpec().MidPln.value);                              // Mid Plane
            excel.set(1, 26, i + 1, selfnode.selectedNodeSpec().StrghtThru.value);                          // Straight Through
            excel.set(1, 27, i + 1, selfnode.selectedNodeSpec().WllMnt.value);                              // Wall Mount   
            excel.set(1, 28, i + 1, selfnode.selectedNodeSpec().QoSCpbl.value);                             // QoS Capable   
            excel.set(1, 29, i + 1, selfnode.selectedNodeSpec().NdeFrmtNcludInd.value);                     // Include Format Code   
            excel.set(1, 30, i + 1, selfnode.selectedNodeSpec().NwSrvcAllw.value);                          // New Service Allowed 

            excel.generate("Report_Node_Specification.xlsx");

            //End of writing node specification data into sheet 2

        };

        return nodeSpecification;
    });