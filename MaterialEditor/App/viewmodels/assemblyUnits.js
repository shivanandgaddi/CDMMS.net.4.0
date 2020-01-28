define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http', 'durandal/activator', 'knockout.mapping', 'durandal/system','jquerydatatable', 'jqueryui', 'datablescelledit'],
    function (composition, app, ko, $, http, activator, mapping, system, jquerydatatable, jqueryui, datablescelledit) {

        var AssemblyUnits = function () {
            selfau = this;
            selfau.usr = require('Utility/user');
            selfau.selectedAssemblyLaborLt = ko.observable('');
            selfau.selectedCalculation = ko.observable('');
            selfau.selectedUnitMeasure = ko.observable('');
            selfau.selectedSearchCalculation = ko.observable('');
            selfau.assemblyLaborLt = ko.observable('');
            selfau.laborTitle = ko.observable('');
            selfau.newLaborId = ko.observable('');
            selfau.laborDescription = ko.observable('');
            selfau.laborIdDetails = ko.observableArray();
            selfau.laborIdDetailsAssembly = ko.observableArray();
            selfau.assemblyUnitSearchResults = ko.observableArray();
            selfau.calculationValues = ko.observableArray('');
            selfau.calculationOperations = ko.observableArray();
            selfau.operators = ko.observableArray('');
            selfau.variableTermConstantTxtFirst = ko.observable('');
            selfau.variableTermConstantTxtSecond = ko.observable('');
            selfau.operationbindingTextFirst = ko.observable('');
            selfau.operationbindingTextSecond = ko.observable('');
            selfau.operationbindingTextThird = ko.observable('');
            selfau.dataTerms = ko.observableArray('');
            selfau.variableTerms = ko.observableArray('');
            selfau.searchCalculationValues = ko.observableArray('');
            selfau.UnitMeasureValues = ko.observableArray(['', 'sample-1', 'sample-2', 'sample-3', 'sample-4']);
            selfau.btnAddLabor = ko.observable(false);
            selfau.btnEditLabor = ko.observable(false);
            selfau.assemblyUnitName = ko.observable('');
            selfau.searchRetired = ko.observable(false);
            selfau.addNewCalcMode = ko.observable(false);
            selfau.searchChooseChecked = ko.observable(false);
            selfau.inputFireWorksName = ko.observable('');
            selfau.fireworkNotRetiredChkb = ko.observable(true);
            selfau.fireworkSectionChkb = ko.observable(true);
            selfau.alternativeAssemblyUnitsList = ko.observableArray();
            selfau.getFireworksCalculation= ko.observableArray();
            selfau.getFireworksMeasurement= ko.observableArray();
            selfau.fireworksSearchResults = ko.observableArray();            
            selfau.getLaborIds(false);           
            selfau.fireworksDisableValidSave = ko.observable(true);
            selfau.selectedAlternativeAUID = ko.observable('');
            selfau.EnableDeleteCalcBtn = ko.observable(false);
            selfau.VisibleDeleteCalcBtn = ko.observable(true);
           
            selfau.visibleMtrlclsfnUI = ko.observable(false);
            selfau.visibleSearchResultsUI = ko.observable(false);           
            selfau.calulationOprByFlag = ko.observable(false);

            http.get('api/assemblyunit/CalcList').then(function (response) {              
                if ('no_results' === response) {
                    console.log("Calculation list not found");
                } else {
                    var results = JSON.parse(response);
                    selfau.searchCalculationValues(results);
                }
            },
           function (error) {
               console.log("Error trying to retrieve calculation list.");
           });

            http.get('api/assemblyunit/operators').then(function (response) {             
                if ('no_results' === response) {
                    console.log("Operator list not found");
                } else {
                    var results = JSON.parse(response);
                    selfau.operators(results);
                }
            },
           function (error) {
               console.log("Error trying to retrieve operators list.");
           });

            http.get('api/assemblyunit/terms').then(function (response) {
                console.log(response);

                if ('no_results' === response) {
                    console.log("Term list not found");
                } else {
                    var results = JSON.parse(response);

                    selfau.dataTerms(results);
                }
            },
           function (error) {
               console.log("Error trying to retrieve term list.");
           });

            http.get('api/assemblyunit/terms').then(function (response) {
                console.log(response);

                if ('no_results' === response) {
                    console.log("Term list not found");
                } else {
                    var results = JSON.parse(response);

                    selfau.variableTerms(results);
                }
            },
           function (error) {
               console.log("Error trying to retrieve term list.");
           });
        };

        AssemblyUnits.prototype.getLaborIds = function (initial) {
            $("#interstitial").show();
            $("#interstitial").css("height", "100%");
            http.get('api/assemblyunit/lbrIdsList').then(function (response) {
                console.log(response);
                if ('no_results' === response) {
                    $("#interstitial").hide();
                    app.showMessage("Labor Id list not found").then(function () {
                        return;
                    });
                } else {
                    var results = JSON.parse(response);
                    selfau.assemblyLaborLt(results);
                    if (initial) {
                        selfau.btnAddLabor(false);
                        selfau.btnEditLabor(true);
                    } else {
                        selfau.btnAddLabor(true);
                        selfau.btnEditLabor(false);
                    }                   
                    $("#interstitial").hide();
                }
            },
         function (error) {
             $("#interstitial").hide();
             return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
         });
        };
        AssemblyUnits.prototype.getlaborTitleDescription = function (model, event) {
            $("#interstitial").show();
            $("#interstitial").css("height", "100%");
            if ($.fn.DataTable.isDataTable('#laborIdAttributesTbl')) {
                $('#laborIdAttributesTbl').DataTable().clear();
                $('#laborIdAttributesTbl').DataTable().destroy();
              }
            selfau.selectedAssemblyLaborLt(event.target.value);
            if (selfau.selectedAssemblyLaborLt() != '') {
                http.get('api/assemblyunit/lbrIdAttribute/' + selfau.selectedAssemblyLaborLt()).then(function (response) {
                    console.log(response);
                    if ('no_results' === response) {
                        $("#interstitial").hide();
                        selfau.btnAddLabor(true);
                        selfau.btnEditLabor(false);
                        app.showMessage("Labor Id list not found").then(function () {
                            return;
                        });
                    } else {
                        var results = JSON.parse(response);
                        console.log(results);
                        selfau.laborTitle(results.LaborTitle);
                        selfau.laborDescription(results.LaborDesc);

                        selfau.laborIdDetails(results.AttributesMtrlClsfnLst);

                        selfau.laborIdDetailsAssembly(results.AttrLbrIdAssemblyUnitLst);
                        selfau.visibleMtrlclsfnUI(true);
                        selfau.visibleSearchResultsUI(false);

                        selfau.btnAddLabor(false);
                        selfau.btnEditLabor(true);
                        var oTable;
                        $(document).ready(function () {
                            oTable = $('#laborIdAttributesTbl').DataTable({
                                paging: false
                        });
                            $('#classificationSearchBox').keyup(function() {
                                  oTable.search($(this).val()).draw();
                            })
                        });
                        $("#interstitial").hide();
                    }
                },
            function (error) {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            });
            } else {
                $("#interstitial").hide();
                selfau.btnAddLabor(true);
                selfau.btnEditLabor(false);
                selfau.laborTitle('');
                selfau.laborDescription('');
                selfau.laborIdDetails([]);
                selfau.laborIdDetailsAssembly([]);
                selfau.visibleMtrlclsfnUI(false);
                selfau.visibleSearchResultsUI(false);
            }
        };

        AssemblyUnits.prototype.saveNewLaborTitleDesc = function () {
            $("#interstitial").show();
            $("#interstitial").css("height", "100%");
            var saveJSON = {
                newLaborId: selfau.newLaborId(), title: selfau.laborTitle(), desc: selfau.laborDescription()
            };
            if (selfau.newLaborId().length > 0 && selfau.laborTitle().length > 0 && selfau.laborDescription().length > 0) {
                http.get('api/assemblyunit/SaveNewLaborId', saveJSON).then(function (response) {
                    console.log(response);
                    if ("success" == JSON.parse(response)) {
                        selfau.getLaborIds(false);
                        selfau.laborTitle('');
                        selfau.laborDescription('');
                        selfau.newLaborId('');
                        $("#interstitial").hide();
                        return app.showMessage('New labor Id has been added successfully');
                    } else if (JSON.parse(response) == "Labor Id already exists please enter an unique id") {
                        $("#interstitial").hide();
                        app.showMessage("Labor Id already exists please enter an unique id").then(function () {
                            return;
                        });
                    } else {
                        $("#interstitial").hide();
                        app.showMessage("failure").then(function () {
                            return;
                        });
                    }
                },
            function (error) {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            });
            } else {
                        $("#interstitial").hide();
                        app.showMessage("Labor Id, Title and description are mandatory").then(function () {
                            return;
                        });
        }
        };
        AssemblyUnits.prototype.editLaborTitleDesc = function () {
            $("#interstitial").show();
            $("#interstitial").css("height", "100%");
            var saveJSON = {
                lbrid: selfau.selectedAssemblyLaborLt(), title: selfau.laborTitle(), desc: selfau.laborDescription()
            };
            http.get('api/assemblyunit/EditLaborId', saveJSON).then(function (response) {
                console.log(response);

                if ("success" == JSON.parse(response)) {
                    selfau.getLaborIds(true);
                    $("#interstitial").hide();
                    return app.showMessage('Labor Id details updated successfully');
                } else {
                    $("#interstitial").hide();
                    app.showMessage("failure").then(function () {
                        return;
                    });
                }
            },
        function (error) {
            $("#interstitial").hide();
            return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
        });
        };

        AssemblyUnits.prototype.saveSearchAssembly = function () {
            $("#interstitial").show();
            var brCheckBoxesExisting = $("#assemblyUnitDt .chkbx_assemblyUnit");
            var assemblyUnitsExisting = selfau.laborIdDetailsAssembly();
            var multiplierValid = true;
            var selectedRows1 = [];
            for (var i = 0; i < brCheckBoxesExisting.length; i++) {
                if (brCheckBoxesExisting[i].checked == true) {
                    for (var j = 0; j < assemblyUnitsExisting.length; j++) {
                        if (brCheckBoxesExisting[i].value == assemblyUnitsExisting[j].aunm.value) {
                            assemblyUnitsExisting[j].isselected.value = 'Y';
                            if (assemblyUnitsExisting[j].mtplrno.value.length > 0) {
                                selectedRows1.push(assemblyUnitsExisting[j]);
                            } else {
                                multiplierValid = false;
                            }
                          
                        }
                    }
                } else {
                    for (var j = 0; j < assemblyUnitsExisting.length; j++) {
                        if (brCheckBoxesExisting[i].value == assemblyUnitsExisting[j].aunm.value) {
                            assemblyUnitsExisting[j].isselected.value = 'N';
                            selectedRows1.push(assemblyUnitsExisting[j]);
                        }
                    }
                }
            }
            var saveJSON1 = mapping.toJS(selectedRows1);

            var brCheckBoxes = $("#assemblyUnitSearchDt .chkbx_searchAssemblyUnit");
            var abc = selfau.assemblyUnitSearchResults();
            var selectedRows = [];           
            for (var i = 0; i < brCheckBoxes.length; i++) {
                if (brCheckBoxes[i].checked == true) {
                    for (var j = 0; j < abc.length; j++) {
                        if (brCheckBoxes[i].value == abc[j].auid.value) {
                            if (abc[j].mtplrno.value.length > 0) {
                                selectedRows.push(abc[j]);
                            } else {
                                multiplierValid = false;
                            }
                        }

                    }
                }
            }
            var saveJSON = mapping.toJS(selectedRows);

            var model = { saveJSONNew: saveJSON, saveJSONExisting: saveJSON1 };

            if (multiplierValid) {
                $.ajax({
                    type: "POST",
                    url: 'api/assemblyunit/UpdateLbrIdAU/' + selfau.selectedAssemblyLaborLt(),
                    data: JSON.stringify(model),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: updateSuccess,
                    error: updateError
                });
                function updateSuccess(response) {                
                    if ("" === response) {
                        $("#interstitial").hide();
                        app.showMessage("failure").then(function () {
                            return;
                        });
                    } else {
                        http.get('api/assemblyunit/lbrIdAttribute/' + selfau.selectedAssemblyLaborLt()).then(function (response) {
                            console.log(response);
                            if ('no_results' === response) {
                                $("#interstitial").hide();                            
                                app.showMessage("failure").then(function () {
                                    return;
                                });
                            } else {
                                var results = JSON.parse(response);
                                selfau.laborIdDetailsAssembly([]);
                                selfau.laborIdDetailsAssembly(results.AttrLbrIdAssemblyUnitLst);
                                selfau.assemblyUnitSearchResults([]);
                                selfau.visibleMtrlclsfnUI(true);
                                selfau.visibleSearchResultsUI(false);
                                selfau.btnAddLabor(false);
                                selfau.btnEditLabor(true);
                                $("#interstitial").hide();
                            }
                        },
           function (error) {
               $("#interstitial").hide();
               return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
           });
                        $("#interstitial").hide();
                        return app.showMessage('success');                       
                    }                       
                }
                function updateError() {
                    $("#interstitial").hide();
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                }

            } else {
                $("#interstitial").hide();
                app.showMessage("Multiplier number is mandatory").then(function () {
                    return;
                });
            }
        };
       
        AssemblyUnits.prototype.saveLbrIdMtrlclsfn = function () {
            var brCheckBoxes = $("#laborIdAttributesTbl .chkbx_assemblyUnit");
            console.log(selfau.laborIdDetails());
            for (var i = 0; i < brCheckBoxes.length; i++) {
                if (brCheckBoxes[i].checked == true) {
                    selfau.laborIdDetails()[i].selected.value = "Y";
                }
                else {
                    selfau.laborIdDetails()[i].selected.value = "N";
                }
            }

            //  var saveJSON = mapping.toJSON(selfau.laborIdDetails());
            var saveJSON = mapping.toJS(selfau.laborIdDetails());
            $.ajax({
                type: "POST",
                url: 'api/assemblyunit/SaveLbrIdMtrlclsfn/' + selfau.selectedAssemblyLaborLt(),
                data: JSON.stringify(saveJSON),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: updateSuccess,
                error: updateError
            });
            function updateSuccess(response) {
                console.log(response);
                if ("" === response) {
                    $("#interstitial").hide();
                    app.showMessage("failure").then(function () {
                        return;
                    });
                } else {
                    return app.showMessage('Classification details updated successfully');
                    $("#interstitial").hide();

                }
            }
            function updateError() {
                $("#interstitial").hide();
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            }
            //http.post('api/assemblyunit/SaveLbrIdMtrlclsfn', saveJSON).then(function (response) {
            //    console.log(response);
            //    if ("" === response) {
            //        $("#interstitial").hide();
            //        app.showMessage("failure").then(function () {
            //            return;
            //        });
            //    } else {
            //        return app.showMessage('success');
            //        $("#interstitial").hide();

            //    }
            //},

            // save JSON for saving the details


        };

        AssemblyUnits.prototype.chooseAssemblyUnit = function () {

            var checkBoxes = $("#assemblyUnitDt .chkbx_assemblyUnit");
            var ids = $("#assemblyUnitDt .aunm_assemblyUnit");
            var comments = $("#assemblyUnitDt .calcnm_assemblyUnit");
            var removedChildIds = [];
            //  $("#interstitial").css("height", "100%");
            $("#interstitial").show();
            sr.selectedrowslist = ko.observableArray();
            for (var i = 0; i < checkBoxes.length; i++) {
                if (checkBoxes[i].checked == true) {
                    alert(ids[i].innerText);
                    alert(comments[i].value);
                    // sr.selectedrowslist.push({ id: ids[i].innerText, cmnt: comments[i].value });
                }
            }
        };

        AssemblyUnits.prototype.searchAssemblyUnit = function () {
            var retSearch = 'N';
          /*  if ($.fn.DataTable.isDataTable('#assemblyUnitSearchDt')) {
                $('#assemblyUnitSearchDt').DataTable().destroy();
            }*/
            $("#interstitial").show();
            $("#interstitial").css("height", "100%");
            if (selfau.searchRetired() === true) {
                retSearch = 'Y';
            }
            var searchJSON = {
                id: selfau.selectedAssemblyLaborLt(), auNm: selfau.assemblyUnitName(), calcID: selfau.selectedSearchCalculation(), retInd: retSearch
            };
            http.get('api/assemblyunit/SearchAU', searchJSON).then(function (response) {
                console.log(response);
                if ('no_results' === response) {
                    $("#interstitial").hide();
                    app.showMessage("Labor Id list not found").then(function () {
                        return;
                    });
                } else {
                    var results = JSON.parse(response);
                    selfau.assemblyUnitSearchResults(results);
                    selfau.visibleMtrlclsfnUI(true);
                    selfau.visibleSearchResultsUI(true);
                  
                   /* $('#assemblyUnitSearchDt').DataTable({
                        "lengthMenu": [[10, 20, 30, 40, 50, -1], [10, 20, 30, 40,50, "All"]],
                        "pageLength": 10
                    });*/
                    $("#interstitial").hide();
                }
            },
        function (error) {
            $("#interstitial").hide();
            return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
        });
        };

        AssemblyUnits.prototype.selectedCalculationValues = function (model, event) {
            selfau.selectedCalculation(event.target.value);
            selfau.addNewCalcMode(false);

            if (event.target.value !== '') {
                http.get('api/assemblyunit/calcOperations/' + event.target.value).then(function (response) {

                    if ('no_results' === response) {
                        $("#interstitial").hide();

                        app.showMessage("Calculation list not found").then(function () {
                            return;
                        });
                    } else {
                        var results = JSON.parse(response);

                        selfau.calculationOperations(results);
                        
                        DisplayDeleteButton(event.target.value);
                    }
                },
               function (error) {
                   return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
               });
            }
        };

        function DisplayDeleteButton(id) {
            http.get('api/assemblyunit/calcCanBeDeleted/' + id).then(function (response) {
                if ('true' === response) {
                    selfau.EnableDeleteCalcBtn(true);
                }
                else {
                    selfau.EnableDeleteCalcBtn(false);
                }
            },
            function (error) {
                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
            });
        }

        AssemblyUnits.prototype.deleteNewCalcBtnPress = function (model, event) {
            
            // get the selection from the list
            var option = document.getElementById("modalCalcValues").options[document.getElementById("modalCalcValues").selectedIndex];
            var index = document.getElementById("modalCalcValues").selectedIndex;
            
            if (option.value !== '') {
                http.get('api/assemblyunit/calcDeleteCalc/' + option.value).then(function (response) {
                    // need to delete from the dropdown
                    if (index > -1)
                    {
                        selfau.calculationValues.splice(index, 1);
                        selfau.calculationOperations([]);
                    }
                },
               function (error) {
                   return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
               });
            }
        }

        AssemblyUnits.prototype.selectedUnitMeasureValues = function (model, event) {
            selfau.selectedUnitMeasure(event.target.value);
        };

        AssemblyUnits.prototype.calculationModalClick = function (model, event) {
            $("#interstitial").show();
            $("#calculationModal").show().draggable();
            $("#staticFromTextLbl").hide();
            http.get('api/assemblyunit/CalcList').then(function (response) {
                console.log(response);

                if ('no_results' === response) {
                    $("#interstitial").hide();
                    app.showMessage("Calculation list not found").then(function () {
                        return;
                    });
                } else {
                    var results = JSON.parse(response);
                    selfau.calculationValues(results);
                    $(document).ready(function () {
                        $('input.operatorsTblCbk').on('click', function () {
                            if ($(this).prop('checked') == true) {
                                selfau.operationbindingTextSecond('');
                                selfau.operationbindingTextThird('');
                                $("#variableConstantTxtFirstId").show();
                                $("#variableConstantTxtSecondId").show();
                                $("#variableConstantTxtFirstLabel").show();
                                $("#variableConstantTxtSecondLabel").show();
                                $("#variableConstantTxtFirstId").prop("disabled", false);
                                $("#variableConstantTxtSecondId").prop("disabled", false);
                                $('input.dataTermsTblCbk').prop('disabled', false);
                                $('input.variableTermsTblCbk').prop('disabled', false);
                                $('input.dataTermsTblCbk').prop('checked', false);
                                $('input.variableTermsTblCbk').prop('checked', false);

                                $('input.operatorsTblCbk').not(this).prop('checked', false);
                                var operatorList = selfau.operators();
                                var oprId = '';
                                var oprName = '';
                                oprId = $(this).val();
                                for (var i = 0 ; i < operatorList.length; i++) {
                                    if (oprId == operatorList[i].value) {
                                        oprName = operatorList[i].text;
                                        if (oprId == '1' || oprId == '2') {
                                            selfau.calulationOprByFlag(true);
                                            $("#variableConstantTxtFirstId").hide();
                                            $("#variableConstantTxtSecondId").show();
                                            $("#variableConstantTxtFirstLabel").hide();
                                            $("#variableConstantTxtSecondLabel").show();
                                            $("#staticByTextLbl").show();
                                            $("#staticFromTextLbl").hide();
                                        }
                                        else {
                                            $("#variableConstantTxtFirstId").show();
                                            $("#variableConstantTxtSecondId").hide();
                                            $("#variableConstantTxtFirstLabel").show();
                                            $("#variableConstantTxtSecondLabel").hide();
                                            selfau.calulationOprByFlag(false);
                                            $("#staticByTextLbl").hide();
                                            $("#staticFromTextLbl").show();
                                        }
                                           
                                    }
                                        
                                }
                                selfau.operationbindingTextFirst(oprName);
                            } else {
                                selfau.operationbindingTextFirst('');
                                selfau.operationbindingTextSecond('');
                                selfau.operationbindingTextThird('');
                                $("#variableConstantTxtFirstId").show();
                                $("#variableConstantTxtSecondId").show();
                                $("#variableConstantTxtFirstLabel").show();
                                $("#variableConstantTxtSecondLabel").show();
                                $("#variableConstantTxtFirstId").prop("disabled", false);
                                $("#variableConstantTxtSecondId").prop("disabled", false);
                                $('input.dataTermsTblCbk').prop('disabled', false);
                                $('input.variableTermsTblCbk').prop('disabled', false);
                                $('input.dataTermsTblCbk').prop('checked', false);
                                $('input.variableTermsTblCbk').prop('checked', false);
                            }
                            
                        });
                        $('input.dataTermsTblCbk').on('click', function () {
                            if($(this).prop('checked')==true){
                                $('input.dataTermsTblCbk').not(this).prop('checked', false);
                                var operatorList = selfau.dataTerms();
                                var oprId = '';
                                var oprName = '';
                                oprId = $(this).val();
                                for (var i = 0 ; i < operatorList.length; i++) {
                                    if (oprId == operatorList[i].value)
                                        oprName = operatorList[i].text;
                                }
                                $("#variableConstantTxtFirstId").val('');
                                $("#variableConstantTxtFirstId").prop("disabled", true);
                                if (selfau.calulationOprByFlag()) {
                                    selfau.operationbindingTextSecond(oprName+" BY ");
                                } else {
                                    selfau.operationbindingTextSecond(oprName + " FROM ");
                                }
                                
                            } else {
                                selfau.operationbindingTextSecond('');
                                $('input.dataTermsTblCbk').not(this).prop('checked', false);
                                $("#variableConstantTxtFirstId").prop("disabled", false);
                            }
                            
                        });
                        $('#variableConstantTxtFirstId').on('keyup', function () {
                            var textVal=$(this).val();;
                            if (textVal.length > 0 ) {
                                $('input.dataTermsTblCbk').prop('disabled', true);  
                                selfau.operationbindingTextSecond(textVal+ " FROM ");
                            } else {
                                selfau.operationbindingTextSecond('');
                                $('input.dataTermsTblCbk').prop('disabled', false);
                            }

                        });
                        $('input.variableTermsTblCbk').on('click', function () {                         
                            if ($(this).prop('checked') == true) {
                                $('input.variableTermsTblCbk').not(this).prop('checked', false);
                                var operatorList = selfau.variableTerms();
                                var oprId = '';
                                var oprName = '';
                                oprId = $(this).val();
                                for (var i = 0 ; i < operatorList.length; i++) {
                                    if (oprId == operatorList[i].value)
                                        oprName = operatorList[i].text;
                                }
                                $("#variableConstantTxtSecondId").val('');
                                $("#variableConstantTxtSecondId").prop("disabled", true);

                                selfau.operationbindingTextThird(oprName);
                            } else {
                                selfau.operationbindingTextThird('');
                                $('input.variableTermsTblCbk').not(this).prop('checked', false);
                                $("#variableConstantTxtSecondId").prop("disabled", false);
                            }
                        });
                        $('#variableConstantTxtSecondId').on('keyup', function () {
                            var textVal = $(this).val();;
                            if (textVal.length > 0) {
                                $('input.variableTermsTblCbk').prop('disabled', true);
                                selfau.operationbindingTextThird(textVal + "");
                            } else {
                                selfau.operationbindingTextThird('');
                                $('input.variableTermsTblCbk').prop('disabled', false);
                            }

                        });

                    });
                    $("#interstitial").hide();
                }
            },
           function (error) {
               $("#interstitial").hide();
               return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
           });
        };

        AssemblyUnits.prototype.calculationModalClose = function (model, event) {
            selfau.calculationOperations([]);
            selfau.selectedCalculation(0);
            selfau.operationbindingTextFirst('');
            selfau.operationbindingTextSecond('');
            selfau.operationbindingTextThird('');
            $("#variableConstantTxtFirstId").show();
            $("#variableConstantTxtSecondId").show();
            $("#variableConstantTxtFirstLabel").show();
            $("#variableConstantTxtSecondLabel").show();
            $("#variableConstantTxtFirstId").prop("disabled", false);
            $("#variableConstantTxtSecondId").prop("disabled", false);
            $('input.dataTermsTblCbk').prop('disabled', false);
            $('input.variableTermsTblCbk').prop('disabled', false);
            $('input.operatorsTblCbk').prop('disabled', false);
            $('input.dataTermsTblCbk').prop('checked', false);
            $('input.variableTermsTblCbk').prop('checked', false);
            $('input.operatorsTblCbk').prop('checked', false);
            $("#variableConstantTxtFirstId").val('');
            $("#variableConstantTxtSecondId").val('');
            $('.collapse').collapse('hide');
            $("#calculationModal").hide();
        };

        AssemblyUnits.prototype.openFireworks = function (model, event) {
            $("#fireworksModal").show();
            http.get('api/assemblyunit/CalcList').then(function (response) {
                console.log(response);
                if ('no_results' === response) {
                    $("#interstitial").hide();
                    app.showMessage("Labor Id list not found").then(function () {
                        return;
                    });
                } else {
                    var results = JSON.parse(response);
                    selfau.getFireworksCalculation(results);
                    $("#interstitial").hide();
                }
            },
        function (error) {
            $("#interstitial").hide();
            return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
        });
            http.get('api/assemblyunit/UOMList').then(function (response) {
                console.log(response);
                if ('no_results' === response) {
                    $("#interstitial").hide();
                    app.showMessage("Labor Id list not found").then(function () {
                        return;
                    });
                } else {
                    var results = JSON.parse(response);
                    selfau.getFireworksMeasurement(results);
                    $("#interstitial").hide();
                }
            },
      function (error) {
          $("#interstitial").hide();
          return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
      });
        };
      

        AssemblyUnits.prototype.closeFireworks = function (model, event) {
            $("#fireworksModal").hide();
        };

        AssemblyUnits.prototype.addNewCalcBtnPress = function () {
           // document.getElementById("newCalcNameTextBox").style.visibility = "visible";

            selfau.addNewCalcMode(true);

            http.get('api/assemblyunit/calcOperations/0').then(function (response) {
                //console.log(response);

                if ('no_results' === response) {
                    $("#interstitial").hide();

                    app.showMessage("Calculation list not found").then(function () {
                        return;
                    });
                } else {
                    var results = JSON.parse(response);

                    selfau.calculationOperations(results);
                    selfau.xx = mapping.fromJSON(response);
                    selfau.VisibleDeleteCalcBtn(false);

                }
            },
               function (error) {
                   return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
               });
        };

       

        AssemblyUnits.prototype.addNewCalcCancelBtnPress = function () {
            selfau.addNewCalcMode(false);
            selfau.calculationOperations([]);
            selfau.VisibleDeleteCalcBtn(true);
        };

        AssemblyUnits.prototype.calcOperationsClick = function (data, event) {
            console.log("Click");

            if (event.target.checked == true) {
                var checkBoxes = $("#calcOperationsTbl .chkbx_calcOperationsTbl");
                var ids = $("#calcOperationsTbl .auOpId_calcOperationsTbl");
                var orderOfOps = $("#calcOperationsTbl .slct_calcOperationsTbl");
            }           
            var brCheckBoxes = $("#calcOperationsTbl .chkbx_calcOperationsTbl");
            var calName = '';
            var selectedOperationsRows = [];
            var selectedOperationsOrder = [];
            for (var i = 0; i < brCheckBoxes.length; i++) {
                if (brCheckBoxes[i].checked == true) {
                    selectedOperationsRows.push(selfau.calculationOperations()[i]);
                    selectedOperationsOrder.push(+selfau.calculationOperations()[i].ordrOfOprtn.value);
                }
            }
            //selectedOperationsOrder = $.unique(selectedOperationsOrder);
            var uniqueArr = [];
            for (var i = selectedOperationsOrder.length; i--;) {
                var val = selectedOperationsOrder[i];
                if ($.inArray(val, uniqueArr) === -1) {
                    uniqueArr.unshift(val);
                }
            }
            selectedOperationsOrder = uniqueArr;
            if (selectedOperationsOrder.length > 0) {
                selectedOperationsOrder.sort(function (a, b) { return a - b });
            }

            for (var j = 0; j < selectedOperationsOrder.length; j++) {
                for (var k = 0 ; k < selectedOperationsRows.length; k++) {
                    if (selectedOperationsOrder[j] == selectedOperationsRows[k].ordrOfOprtn.value) {
                        calName += selectedOperationsRows[k].opNm.value + ", ";
                    }
                }
            }
            calName = calName.substring(0, calName.length - 2);
           // var txt = $("#newCalcNameTextBox").text();            
            // $("#newCalcNameTextBox").html(txt + calName);
            $("#newCalcNameTextBox").text("");  // added this so there would only be 1 div tag displaying instead of a (possibly) lengthy list
            $("#newCalcNameTextBox").append("<div class=\"alert alert-success\"><b>" + calName + "</b></div>");
            return true;
        };

        AssemblyUnits.prototype.calcOperationsDDChange = function (data, event) {
            console.log("DD: " + event.target.selectedIndex);
            var brCheckBoxes = $("#calcOperationsTbl .chkbx_calcOperationsTbl");
            var calName = '';
            var selectedOperationsRows = [];
            var selectedOperationsOrder = [];
            for (var i = 0; i < brCheckBoxes.length; i++) {
                if (brCheckBoxes[i].checked == true) {
                    selectedOperationsRows.push(selfau.calculationOperations()[i]);
                    selectedOperationsOrder.push(+selfau.calculationOperations()[i].ordrOfOprtn.value);
                }
            }
           // selectedOperationsOrder = $.unique(selectedOperationsOrder);
            var uniqueArr = [];
            for (var i = selectedOperationsOrder.length; i--;) {
                var val = selectedOperationsOrder[i];
                if ($.inArray(val, uniqueArr) === -1) {
                    uniqueArr.unshift(val);
                }
            }
            selectedOperationsOrder = uniqueArr;
            if (selectedOperationsOrder.length > 0) {
                selectedOperationsOrder.sort(function (a, b) { return a - b });
            }

            for (var j = 0; j < selectedOperationsOrder.length; j++) {
                for (var k = 0 ; k < selectedOperationsRows.length; k++) {
                    if (selectedOperationsOrder[j] == selectedOperationsRows[k].ordrOfOprtn.value) {
                        calName += selectedOperationsRows[k].opNm.value + ", ";
                    }
                }
            }
            calName = calName.substring(0, calName.length - 2);

            $("#newCalcNameTextBox").text(""); // added this so there would only be 1 div tag displaying instead of a (possibly) lengthy list
            $("#newCalcNameTextBox").append("<div class=\"alert alert-success\"><b>" + calName + "</b></div>");
            
            return true;
        };
        AssemblyUnits.prototype.alternativeCheckbox = function (data, event) {
            selfau.selectedAlternativeAUID(event.target.value);
            if (event.target.checked) {
                $("#alternativeModal").show();
                
                var url = 'api/assemblyunit/SearchAUforLbrId/' + selfau.selectedAssemblyLaborLt();
                http.get(url).then(function (response) {
                    console.log(response);
                    if ('no_results' === response) {
                        $("#interstitial").hide();
                        app.showMessage("Labor Id list not found").then(function () {
                            return;
                        });
                    } else {
                        var results = JSON.parse(response);
                        selfau.alternativeAssemblyUnitsList(results);
                        $("#interstitial").hide();                      
                    }
                },
        function (error) {
            $("#interstitial").hide();
            return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
        });
            } else {
                $("#alternativeModal").hide();
                var selAuid = selfau.selectedAlternativeAUID();
                for (var j = 0; j < selfau.assemblyUnitSearchResults().length; j++) {
                    if (selAuid == selfau.assemblyUnitSearchResults()[j].auid.value) {
                        var xyz = selfau.assemblyUnitSearchResults()[j];
                        xyz.alttoau.value = '';
                        xyz.alttoauid.value = selAuid;
                        xyz.alternative.value = 'N';
                        selfau.assemblyUnitSearchResults()[j] = xyz;
                    }
                }
                var abc = selfau.assemblyUnitSearchResults();
                selfau.assemblyUnitSearchResults([]);
                selfau.assemblyUnitSearchResults(abc);
            }           
        };
        AssemblyUnits.prototype.closeAlternativeModal = function (model, event) {
            $("#alternativeModal").hide();
        };
        AssemblyUnits.prototype.saveAlternative = function (model, event) {
           
            var brCheckBoxes1 = $("#alternativeModalTb .chkbx_alternative");
            var abc1 = selfau.alternativeAssemblyUnitsList();
            var altaunm = '';
            var altauid = '';
            var lbridauid = '';
            var selectedRows1 = [];
            for (var i = 0; i < brCheckBoxes1.length; i++) {
                if (brCheckBoxes1[i].checked == true) {
                    for (var j = 0; j < abc1.length; j++) {
                        if (brCheckBoxes1[i].value == abc1[j].auid.value) {
                            altaunm = abc1[j].aunm.value;
                            altauid = abc1[j].auid.value;
                            lbridauid = abc1[j].lbridauid.value;
                        }
                    }
                }
            }
           
            var selAuid = selfau.selectedAlternativeAUID();           
            for (var j = 0; j < selfau.assemblyUnitSearchResults().length; j++) {
                if (selAuid == selfau.assemblyUnitSearchResults()[j].auid.value) {
                    var xyz = selfau.assemblyUnitSearchResults()[j];
                    xyz.alttoau.value = altaunm;
                    xyz.alttoauid.value = altauid;
                    xyz.lbridauid.value = lbridauid;
                    xyz.alternative.value = 'Y';
                    xyz.default.value = 'N';
                    selfau.assemblyUnitSearchResults()[j] = xyz;                 
                }
            }
            var abc = selfau.assemblyUnitSearchResults();
            selfau.assemblyUnitSearchResults([]);
            selfau.assemblyUnitSearchResults(abc);
     
            $("#alternativeModal").hide();         
        };
        AssemblyUnits.prototype.alternativechooseCheckbox = function (model, event) {
            var selAuid = event.target.value;
            if (event.target.checked) {
                selfau.searchChooseChecked(true);
                for (var j = 0; j < selfau.assemblyUnitSearchResults().length; j++) {
                    if (selAuid == selfau.assemblyUnitSearchResults()[j].auid.value) {
                        var xyz = selfau.assemblyUnitSearchResults()[j];
                        xyz.alttoau.value = '';
                        xyz.alttoauid.value = '';
                        xyz.default.value = 'Y';
                        xyz.alternative.value = 'N';
                        selfau.assemblyUnitSearchResults()[j] = xyz;
                    }
                }
                var abc = selfau.assemblyUnitSearchResults();
                selfau.assemblyUnitSearchResults([]);
                selfau.assemblyUnitSearchResults(abc);
            } else {
                selfau.searchChooseChecked(false);
                for (var j = 0; j < selfau.assemblyUnitSearchResults().length; j++) {
                    if (selAuid == selfau.assemblyUnitSearchResults()[j].auid.value) {
                        var xyz = selfau.assemblyUnitSearchResults()[j];
                        xyz.alttoau.value = '';
                        xyz.alttoauid.value = '';
                        xyz.default.value = 'N';
                        xyz.alternative.value = 'N';
                        selfau.assemblyUnitSearchResults()[j] = xyz;
                    }
                }
                var abc = selfau.assemblyUnitSearchResults();
                selfau.assemblyUnitSearchResults([]);
                selfau.assemblyUnitSearchResults(abc);
            }
            return true;
        };
        AssemblyUnits.prototype.alternativeDefaultCheckbox = function (model, event) {
            var selAuid = event.target.value;
            if (event.target.checked) {            
                for (var j = 0; j < selfau.assemblyUnitSearchResults().length; j++) {
                    if (selAuid == selfau.assemblyUnitSearchResults()[j].auid.value) {
                        var xyz = selfau.assemblyUnitSearchResults()[j];
                        xyz.alttoau.value = '';
                        xyz.alttoauid.value = '';
                        xyz.default.value = 'Y';
                        xyz.alternative.value = 'N';
                        selfau.assemblyUnitSearchResults()[j] = xyz;
                    }
                }
                var abc = selfau.assemblyUnitSearchResults();
                selfau.assemblyUnitSearchResults([]);
                selfau.assemblyUnitSearchResults(abc);
            } else {
                selfau.searchChooseChecked(false);
                for (var j = 0; j < selfau.assemblyUnitSearchResults().length; j++) {
                    if (selAuid == selfau.assemblyUnitSearchResults()[j].auid.value) {
                        var xyz = selfau.assemblyUnitSearchResults()[j];
                        xyz.alttoau.value = '';
                        xyz.alttoauid.value = '';
                        xyz.default.value = 'N';
                        xyz.alternative.value = 'N';
                        selfau.assemblyUnitSearchResults()[j] = xyz;
                    }
                }
                var abc = selfau.assemblyUnitSearchResults();
                selfau.assemblyUnitSearchResults([]);
                selfau.assemblyUnitSearchResults(abc);
            }
            return true;
        };

        AssemblyUnits.prototype.searchFireworksModal = function () {
           
            selfau.fireworkNotRetiredChkb();
            selfau.fireworkSectionChkb();
            var retSearch = 'N';
            var sectionSearch = 'N';
            if ( selfau.fireworkNotRetiredChkb() === true) {
                retSearch = 'Y';
            }
            if (selfau.fireworkSectionChkb() === true) {
                retSearch = 'Y';
            }
            var searchJSON = {
                name: selfau.inputFireWorksName(), notRetired: retSearch, section: sectionSearch
            };
            http.get('api/assemblyunit/SearchFireworks', searchJSON).then(function (response) {
                console.log(response);
                if ('no_results' === response) {
                    $("#interstitial").hide();
                    app.showMessage("Labor Id list not found").then(function () {
                        return;
                    });
                } else {
                    var results = JSON.parse(response);
                    selfau.fireworksSearchResults(results);
                    selfau.fireworksDisableValidSave(true);
                    $('#fireworksSearchTb').DataTable();
                    $("#interstitial").hide();
                }
                $(document).ready(function () {
                    $('input.fireworksChkbx').on('click', function () {
                        $('input.fireworksChkbx').not(this).prop('checked', false);
                    });
                });
            },
        function (error) {
            $("#interstitial").hide();
            return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
        });
        };
        AssemblyUnits.prototype.saveFireworksModal = function () {

            var selectedFireWorks = [];
            var searchJSON;
            var brCheckBoxes = $("#fireworksSearchTb .fireworksChkbx");
            var searchFireworks = selfau.fireworksSearchResults(); 
            var calcId = $('#getFireworksCalculationId').find(":selected").val();
            var uomId = $('#getFireworksMeasurementId').find(":selected").val();
            console.log(selfau.laborIdDetails());
            for (var i = 0; i < brCheckBoxes.length; i++) {
                if (brCheckBoxes[i].checked == true) {
                    for (var j = 0; j < searchFireworks.length; j++) {
                        var searchJSON = {
                            name: searchFireworks[j].description.value, calcId: calcId, uomId: uomId, AuSK: searchFireworks[j].assemblyunitsk.value, cuid: selfau.usr.cuid
                        };
                        selectedFireWorks.push(searchJSON);
                    }
                }
            }
            http.get('api/assemblyunit/SaveFireworks', selectedFireWorks[0]).then(function (response) {
                    console.log(response);
                    if ('no_results' === response) {
                        $("#interstitial").hide();
                        app.showMessage("failure").then(function () {
                            return;
                        });
                    } else {
                        var results = JSON.parse(response); 
                        selfau.fireworksSearchResults([])
                        return app.showMessage('success');
                        $("#interstitial").hide();
                    }
                },
        function (error) {
            $("#interstitial").hide();
            return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
        });         
            };
        AssemblyUnits.prototype.fireworkNotRetiredSectionChkbValid = function () {
          
            if (selfau.fireworkNotRetiredChkb() && selfau.fireworkSectionChkb()) {
                $('input.fireworksChkbx').prop('disabled', false);
                $('#fireworksSave').prop('disabled', false);                
            } else {
                $('input.fireworksChkbx').prop('disabled', true);
                $('#fireworksSave').prop('disabled', true);
            }
            return true;

        };
       
        AssemblyUnits.prototype.saveAddOperation = function () {

            var brCheckBoxesOpr = $("#operatorsTbl .operatorsTblCbk");    
            var brCheckBoxesData = $("#dataTermsTbl .dataTermsTblCbk");
            var brCheckBoxesVar = $("#variableTermsTbl .variableTermsTblCbk");    
            var operatorList = selfau.operators();
            var oprId = '';
            var oprName = selfau.operationbindingTextFirst() + selfau.operationbindingTextSecond() + selfau.operationbindingTextThird();
            for (var i = 0; i < brCheckBoxesOpr.length; i++) {
                if (brCheckBoxesOpr[i].checked == true) {
                    oprId = brCheckBoxesOpr[i].value;
                }
            }

            var dataId = '';             
            for (var i = 0; i < brCheckBoxesData.length; i++) {
                if (brCheckBoxesData[i].checked == true) {
                    dataId = brCheckBoxesData[i].value;
                }
            }
            var varId = '';
            for (var i = 0; i < brCheckBoxesVar.length; i++) {
                if (brCheckBoxesVar[i].checked == true) {
                    varId = brCheckBoxesVar[i].value;
                }
            }
            var constantNo = '';
            if (selfau.calulationOprByFlag()) {
                constantNo = $("#variableConstantTxtSecondId").val();
            } else {
                constantNo = $("#variableConstantTxtFirstId").val();
            }
           
            var searchJSON = {
                opNm: oprName, auOpOprId: oprId, dataTermId: dataId,
                constantNo: constantNo, varTermId: varId
            };
            http.get('api/assemblyunit/SaveAddOperations', searchJSON).then(function (response) {
                console.log(response);
                if ('no_results' === response) {
                    $("#interstitial").hide();
                    app.showMessage("Labor Id list not found").then(function () {
                        return;
                    });
                } else {
                    $("#interstitial").hide();
                    selfau.operationbindingTextFirst('');
                    selfau.operationbindingTextSecond('');
                    selfau.operationbindingTextThird('');
                    $("#variableConstantTxtFirstId").show();
                    $("#variableConstantTxtSecondId").show();
                    $("#variableConstantTxtFirstLabel").show();
                    $("#variableConstantTxtSecondLabel").show();
                    $("#variableConstantTxtFirstId").prop("disabled", false);
                    $("#variableConstantTxtSecondId").prop("disabled", false);
                    $('input.dataTermsTblCbk').prop('disabled', false);
                    $('input.variableTermsTblCbk').prop('disabled', false);
                    $('input.dataTermsTblCbk').prop('checked', false);
                    $('input.variableTermsTblCbk').prop('checked', false);
                    $('input.operatorsTblCbk').prop('checked', false);
                    var results = JSON.parse(response);
                    return app.showMessage('success');
                }
            },
         function (error) {
             $("#interstitial").hide();
             return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
         });
        };
        AssemblyUnits.prototype.saveNewCalcBtnPress = function () {
            var brCheckBoxes = $("#calcOperationsTbl .chkbx_calcOperationsTbl");
            var selectedOperations = [];            
            var calName = '';
            var oderofOperationValid =true;
            for (var i = 0; i < brCheckBoxes.length; i++) {
                if (brCheckBoxes[i].checked == true) {
                    selfau.calculationOperations()[i].isSelected.value = "Y";
                    if (selfau.calculationOperations()[i].ordrOfOprtn.value == "0") {
                        oderofOperationValid = false;
                        
                    }
                    selectedOperations.push(selfau.calculationOperations()[i]);
                    //calName += selfau.calculationOperations()[i].opNm.value+",";
                }
                else {
                    selfau.calculationOperations()[i].isSelected.value = "N";
                }
            }

            // Make sure the name displayed in the dropdown is in the same order of the selection, which may not
            // be the same order of the checkboxes
            selectedOperations.sort(function (a, b) {
                return a.ordrOfOprtn.value - b.ordrOfOprtn.value;
            });
            for (var i = 0; i < selectedOperations.length; i++) {
                calName += selectedOperations[i].opNm.value + ', ';
            }
            

            if (oderofOperationValid) {
                calName = calName.substring(0, calName.length - 2);
                var saveJSON = mapping.toJS(selectedOperations);
                $.ajax({
                    type: "POST",
                    url: 'api/assemblyunit/SaveOprforCalculation/' + calName,
                    data: JSON.stringify(saveJSON),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: updateSuccess,
                    error: updateError
                });
                function updateSuccess(response) {
                    console.log(response);
                    if ('""' === response) {
                        $("#interstitial").hide();
                        app.showMessage("failure").then(function () {
                            return;
                        });
                    } else {
                        http.get('api/assemblyunit/CalcList').then(function (response) {
                            console.log(response);

                            if ('no_results' === response) {
                                $("#interstitial").hide();
                                app.showMessage("Calculation list not found").then(function () {
                                    return;
                                });
                            } else {                              
                                $("#interstitial").hide();
                                var results = JSON.parse(response);
                                selfau.calculationValues([]);
                                selfau.calculationOperations([]);
                                selfau.calculationValues(results);
                                selfau.VisibleDeleteCalcBtn(true);
                                return app.showMessage('New Calculation Added Successfully');
                            }
                        },
                        function (error) {
                             $("#interstitial").hide();
                                return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                        });
                     
                    }
                }
                function updateError() {
                    $("#interstitial").hide();
                    return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
                }
            } else {
                $("#interstitial").hide();
                return app.showMessage('Order of operation is mandatory');

            }

        };
       
            return AssemblyUnits;
        })