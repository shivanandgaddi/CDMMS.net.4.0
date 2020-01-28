define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system', '../Utility/user', '../Utility/referenceDataHelper'],
    function (composition, app, ko, $, http, activator, mapping, system, user, reference) {
        var portSpecification = function (data) {
            portSpec = this;
            specChangeArray = [];
            var dataResult = data.resp;
            var results = JSON.parse(dataResult);
            portSpec.specification = data.specification;

            portSpec.selectedPortSpec = ko.observable();

            portSpec.selectedPortSpec(results);
            portSpec.portUseTypeList = ko.observableArray();
            portSpec.completedNotSelected = ko.observable(true);
            portSpec.duplicateSelectedPortSpecDltd = ko.observable();
            portSpec.duplicateSelectedPortSpecDltd = results.Dltd.bool;
            portSpec.duplicateSelectedPortSpecName = ko.observable();
            portSpec.duplicateSelectedPortSpecName.value = results.RvsnNm.value;

            portSpec.portType = ko.observableArray([{ value: '', text: '' }, { value: 'SIGNAL', text: 'Signal' }, { value: 'AC', text: 'AC' }, { value: 'DC', text: 'DC' }, { value: 'UNKNOWN', text: 'Unknown' }]);
            portSpec.portServiceLevel = ko.observableArray([{ value: '', text: '' }, { value: 'COAX', text: 'Coax' }, { value: 'COPPER', text: 'Copper' }, { value: 'FIBER', text: 'Fiber' }, { value: 'OTHER', text: 'Other' }]);
            portSpec.portPhysicalStts = ko.observableArray([{ value: '', text: '' }, { value: 'SPARE', text: 'Spare' }, { value: 'FAULTY', text: 'Faulty' }, { value: 'IN_SERVICE', text: 'In Service/Reserved' }]);
            //Local (default), Century Link LLC, CLEC, Join, Lightcore, QVN, Qwest National, SAVVIS, or UNKNOWN
            portSpec.portDepartment = ko.observableArray([{ value: '', text: '' }, { value: 'LOCAL', text: 'Local' },
                                            { value: 'CENTURY LINK LLC', text: 'Century Link LLC' }, { value: 'CLEC', text: 'CLEC' }, { value: 'JOIN', text: 'Join' },
                                            { value: 'LIGHTCORE', text: 'Lightcore' }, { value: 'QVN', text: 'QVN' }, { value: 'QWEST NATIONAL', text: 'Qwest National' },
                                            { value: 'SAVVIS', text: 'SAVVIS' }, { value: 'UNKNOWN', text: 'Unknown' }
                                         ]);


            $(document).on('change', '[type=checkbox]', function () {
                if (this.name == "cmplIndnmport") {
                    if (this.checked == false) {
                        document.getElementById('prpgIndport').checked = false;
                    }
                }
            });
            if (portSpec.selectedPortSpec().Cmplt.bool == true && portSpec.selectedPortSpec().Prpgtd.enable == true) {
                portSpec.completedNotSelected(false);
            }

            if (selfspec.selectRadioSpec() == 'newSpec') {
                //portSpec.selectedPortSpec().Wdth.value = '';
                //portSpec.selectedPortSpec().Dpth.value = '';
                //portSpec.selectedPortSpec().Hght.value = '';
            }
            portSpec.GetPortusetypes();
        };

        portSpecification.prototype.onchangeCompleted = function () {
            if ($("#completedChkBox").is(':checked')) {
                portSpec.completedNotSelected(false);
            } else {
                portSpec.completedNotSelected(true);
                portSpec.selectedPortSpec().Prpgtd.bool = false;
            }
        };

        portSpecification.prototype.updatePortSpec = function () {
            // check for duplicate spec name
            var specname = document.getElementById("nameport").value;
            var specid = document.getElementById("specidport").value;
            if (selfspec.selectRadioSpec() == 'existSpec') {
                var specnameduplicate = portSpec.GetSpecNameDuplicate(specname, specid);
            }
            else {
                var specnameduplicate = portSpec.GetSpecNameDuplicate(specname, 0);
            }
            if (specnameduplicate != 'NODUP') {
                var fields = specnameduplicate.split('~');
                app.showMessage('Spec Name ' + fields[1] + ' already exists, ' + fields[2] + ' ID ' + fields[0]);
                return;
            }

            $("#interstitial").show();

            var txtCondt = '';
            if (selfspec.selectRadioSpec() == 'existSpec') {
                if (portSpec.selectedPortSpec().RvsnNm.value != portSpec.duplicateSelectedPortSpecName.value) {
                    txtCondt += "Name changed to <b>" + portSpec.selectedPortSpec().RvsnNm.value + '</b> from ' + portSpec.duplicateSelectedPortSpecName.value + "<br/>";

                    var saveJSON = {
                        'tablename': 'port_specn', 'columnname': 'port_specn_revsn_nm', 'audittblpkcolnm': 'port_specn_id', 'audittblpkcolval': portSpec.selectedPortSpec().id.value, 'auditprnttblpkcolnm': '', 'auditprnttblpkcolval': '', 'actncd': 'C',
                        'oldcolval': portSpec.duplicateSelectedPortSpecName.value,
                        'newcolval': portSpec.selectedPortSpec().RvsnNm.value,
                        'cuid': user.cuid, 'cmnttxt': user.cuid + ' changed ' + portSpec.selectedPortSpec().RvsnNm.value + ' Spec Name from ' + portSpec.duplicateSelectedPortSpecName.value + ' on ', 'materialitemid': 0
                    };
                    specChangeArray.push(saveJSON);
                }

                if (portSpec.selectedPortSpec().Dltd.bool != portSpec.duplicateSelectedPortSpecDltd) {
                    txtCondt += "Unusable changed to <b>" + portSpec.selectedPortSpec().Dltd.bool + '</b> from ' + portSpec.duplicateSelectedPortSpecDltd + "<br/>";

                    var saveJSON = {
                        'tablename': 'port_specn', 'columnname': 'del_ind', 'audittblpkcolnm': 'port_specn_id', 'audittblpkcolval': portSpec.selectedPortSpec().id.value, 'auditprnttblpkcolnm': '',
                        'auditprnttblpkcolval': '', 'actncd': 'C', 'oldcolval': portSpec.duplicateSelectedPortSpecDltd, 'newcolval': portSpec.selectedPortSpec().Dltd.bool, 'cuid': user.cuid,
                        'cmnttxt': user.cuid + ' changed ' + portSpec.selectedPortSpec().Dltd.bool + ' Unusable from ' + portSpec.duplicateSelectedPortSpecDltd + ' to ' + portSpec.selectedPortSpec().Dltd.bool + ' on ',
                        'materialitemid': 0
                    };
                    specChangeArray.push(saveJSON);
                }
            }
            $("#interstitial").hide();
            if (txtCondt.length > 0) {
                app.showMessage(txtCondt, 'Update Confirmation for Port', ['Yes', 'No']).then(function (dialogResult) {
                    if (dialogResult == 'Yes') {
                        portSpec.savePortSpec();
                    }
                });
            } else {
                portSpec.savePortSpec();
            }
        };

        portSpecification.prototype.savePortSpec = function () {
            $("#interstitial").show();
            var saveJSON = mapping.toJS(portSpec.selectedPortSpec());
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
                    //portSpec.saveAuditChanges();
                    //******** Send to NDS *************//
                    //var specWorkToDoId = specResponseOnSuccess.SpecWTD;
                    //var mtlWorkToDoId = specResponseOnSuccess.MtlWTD;

                    //if (specWorkToDoId !== 0) {
                    //    var specHelper = new reference();

                    //    specHelper.getSpecificationSendToNdsStatus(specResponseOnSuccess.Id, specWorkToDoId, 'PORT');
                    //}

                    //if (mtlWorkToDoId !== 0) {
                    //    var mtlHelper = new reference();

                    //    mtlHelper.getSendToNdsStatus(specResponseOnSuccess.MtlItmId, mtlWorkToDoId);
                    //}

                    //**********************************//

                    if (selfspec.selectRadioSpec() == 'newSpec') {
                        selfspec.selectRadioSpec('existSpec');
                        $("#idProductID").val(specResponseOnSuccess.Id);
                        $('#idspectype').val("PORT");
                        selfspec.Searchspec();
                        $("#interstitial").hide();
                        return app.showMessage('Successfully created specification <br> of type PORT having <b>Id ' + specResponseOnSuccess.Id + '</b>', 'Specification');
                    } else {
                        if (portSpec.specification == true) {
                            selfspec.updateOnSuccess();
                        }
                        $("#interstitial").hide();
                        return app.showMessage('Successfully Updated specification details<br> of type PORT having <b>Id ' + specResponseOnSuccess.Id + '</b>', 'Specification');
                    }                   

                } else {
                    $("#interstitial").hide();
                    app.showMessage("failure").then(function () {
                        return;
                    });
                }
            }
            function updateError() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }
        };

        portSpecification.prototype.saveAuditChanges = function () {
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
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.', 'Specification');
            }
        };

        portSpecification.prototype.GetSpecNameDuplicate = function (modelname, id) {
            var spec = { 'specname': modelname, 'id': id }
            var specnameduplicate = '';
            $.ajax({
                type: "GET",
                url: 'api/specn/specnameduplicate',
                data: spec,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: saveSpecNameDuplicateSuccess,
                error: saveSpecNameDuplicatetError,
                async: false
            });
            function saveSpecNameDuplicateSuccess(response) {
                specnameduplicate = response;
            }
            function saveSpecNameDuplicatetError() {
            }
            return specnameduplicate;
        }

        //portSpecification.prototype.slotupdateChbkCheck = function () {

        //    if ($("#SlotStrghtThruChbk").is(':checked'))
        //        portSpec.selectedPortSpec().StrghtThru.value = "Y";
        //    else
        //        portSpec.selectedPortSpec().StrghtThru.value = "N";

        //    if ($("#SbSltChbk").is(':checked'))
        //        portSpec.selectedPortSpec().SbSlt.value = "Y";
        //    else
        //        portSpec.selectedPortSpec().SbSlt.value = "N";


        //};

        portSpecification.prototype.GetPortusetypes = function () {
            var id = 8;
            $.ajax({
                type: "GET",
                url: 'api/specn/SpecUseTypes/' + id,                
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: PortUseTypeSuccess,
                error: PortUseTypeError,
                async: false
            });
            function PortUseTypeSuccess(response) {
                var res = JSON.parse(response);
                portSpec.portUseTypeList(res);
            }
            function PortUseTypeError() {
            }
        }

        return portSpecification;
    });