define(['durandal/composition', 'durandal/app', 'knockout', 'jquery', 'plugins/http',
    'durandal/activator', 'knockout.mapping', 'durandal/system', 'jquerydatatable', 'jqueryui',
    'datablescelledit', '../Utility/referenceDataHelper', 'jszip', 'fileSaver', 'myexcel'],
    function (composition, app, ko, $, http, activator, mapping, system, jquerydatatable,
        jqueryui, datablescelledit, reference, jszip,fileSaver, myexcel) {
        var generateReports = function () {
            selfrep = this;
            selfrep.usr = require('Utility/user');
            selfrep.savedReports = ko.observableArray();
            selfrep.selectedSavedReport = ko.observable('');
            selfrep.assemblyLaborLt = ko.observable('');
            selfrep.selectedAssemblyLaborLt = ko.observable('');
            selfrep.selectedAssemblyLaborText = ko.observable('');
            selfrep.assemblyReportList = ko.observableArray();
            selfrep.assemblyReportListMaterial = ko.observableArray();
            selfrep.showReportTables = ko.observable(false);
            http.get('api/Reports/ReportsList').then(function (response) {
                if ('no_results' === response) {
                    console.log("Calculation list not found");
                } else {
                    var results = JSON.parse(response);
                    selfrep.savedReports(results);
                    $(document).ready(function () {
                        $('select option').filter(function () {
                         return !this.value && $.trim(this.value).length == 0 && $.trim(this.text).length == 0;
                     }).remove();
                    });
                }
            },
        function (error) {
            console.log("Error trying to retrieve calculation list.");
        });
            selfrep.getLaborIds();
        };
          
        generateReports.prototype.getLaborIds = function () {
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
                    selfrep.assemblyLaborLt(results);
                    $(document).ready(function () {
                        $('select option').filter(function () {
                            return !this.value && $.trim(this.value).length == 0 && $.trim(this.text).length == 0;
                        }).remove();
                    });
                    $("#interstitial").hide();
                }
            },
         function (error) {
             $("#interstitial").hide();
             return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
         });
        };

        generateReports.prototype.getlaborTitleDescription = function (model, event) {
            $("#interstitial").show();
            $("#interstitial").css("height", "100%");
            if ($.fn.DataTable.isDataTable('#generateReportsMaterial')) {
                $('#generateReportsMaterial').DataTable().clear();
                $('#generateReportsMaterial').DataTable().destroy();
            }
            selfrep.selectedAssemblyLaborLt(event.target.value);
           
            if (selfrep.selectedAssemblyLaborLt() != '') {
                http.get('api/Reports/LbrIdReport/' + selfrep.selectedAssemblyLaborLt()).then(function (response) {
                    console.log(response);
                    if ('no_results' === response) {
                        $("#interstitial").hide();
                        selfrep.assemblyReportList([]);
                        selfrep.assemblyReportListMaterial([]);
                        selfrep.showReportTables(false);
                        app.showMessage("Labor Id list not found").then(function () {
                            return;
                        });
                    } else {
                        var results = JSON.parse(response);
                        selfrep.assemblyReportList(results.LbrIdAUList);
                        selfrep.assemblyReportListMaterial(results.LbrIdMtrlList);
                        selfrep.showReportTables(true);
                        setTimeout(function () {
                            $("#generateReportsMaterial").DataTable();
                            $("#interstitial").hide();
                        }, 2000);
                        var exportassemblyReportList = selfrep.assemblyReportList();
                        var exportMaterialReportList = selfrep.assemblyReportListMaterial();
                        var selectedAssemblyLabor = selfrep.selectedAssemblyLaborLt();
                        $(document).ready(function () {
                            function randomDate(start, end) {
                                var d = new Date(start.getTime() + Math.random() * (end.getTime() - start.getTime()));
                                return d;
                            }

                            $('#buttonclick').on('click', function () {

                                var excel = $JExcel.new("Calibri light 10 #333333");			// Default font

                                excel.set({ sheet: 0, value: "Assembly Units" });
                                excel.addSheet("Material");
                                excel.set({ sheet: 1, value: "Material" });                                

                                var headers = ["Assembly Unit Name", "Calculation", "Unit of Measurement", "Retired", "Muliplier Number", "Alternate", " Alternate to AU"];							// This array holds the HEADERS text
                                var headersMaterial = ["CDMMS ID", "Material Code", "Part Number", "Clmc", "Catalog Description"];
                                var formatHeader = excel.addStyle({ 															// Format for headers
                                    border: "none,none,none,thin #333333", 													// 		Border for header
                                    font: "Calibri 12 #1b834c B"
                                }); 												

                                for (var i = 0; i < headers.length; i++) {																// Loop all the haders
                                    excel.set(0, i, 0, headers[i], formatHeader);													// Set CELL with header text, using header format
                                    excel.set(0, i, undefined, "auto");															// Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
                                }
                                for (var i = 0; i < headersMaterial.length; i++) {																// Loop all the haders
                                    excel.set(1, i, 0, headersMaterial[i], formatHeader);													// Set CELL with header text, using header format
                                    excel.set(1, i, undefined, "auto");															// Set COLUMN width to auto (according to the standard this is only valid for numeric columns)
                                }
                              
                                for (var i = 0; i < exportassemblyReportList.length; i++) {																			// we will fill the 50 rows
                                    excel.set(0, 0, i + 1, exportassemblyReportList[i].aunm.value);	
                                    excel.set(0, 1, i + 1, exportassemblyReportList[i].calcnm.value);														// Store the random date as STRING
                                    excel.set(0, 2, i + 1, exportassemblyReportList[i].uomnm.value);												// Store the previous random date as a NUMERIC (there is also $JExcel.toExcelUTCTime)
                                    excel.set(0, 3, i + 1, exportassemblyReportList[i].retind.value);										// Store the previous random date as a NUMERIC,  display using dateStyle format
                                    excel.set(0, 4, i + 1, exportassemblyReportList[i].mtplrno.value);
                                    excel.set(0, 5, i + 1, exportassemblyReportList[i].alternative.value);												// Store the previous random date as a NUMERIC (there is also $JExcel.toExcelUTCTime)
                                    excel.set(0, 6, i + 1, exportassemblyReportList[i].alttoau.value);
                                }
                                for (var i = 0; i < exportMaterialReportList.length; i++) {																			// we will fill the 50 rows
                                    excel.set(1, 0, i + 1, exportMaterialReportList[i].materialitemid.value);
                                    excel.set(1, 1, i + 1, exportMaterialReportList[i].productid.value);														// Store the random date as STRING
                                    excel.set(1, 2, i + 1, exportMaterialReportList[i].mfgpartno.value);												// Store the previous random date as a NUMERIC (there is also $JExcel.toExcelUTCTime)
                                    excel.set(1, 3, i + 1, exportMaterialReportList[i].mfgid.value);										// Store the previous random date as a NUMERIC,  display using dateStyle format
                                    excel.set(1, 4, i + 1, exportMaterialReportList[i].ctlgdsc.value);
                                }
                               
                                excel.generate("Report_LaborId_" +selectedAssemblyLabor + ".xlsx");

                            });
                        });
                    }
                },
             function (error) {
                 $("#interstitial").hide();
                 return app.showMessage('Unable to process your request due to an internal error. If problem persists please contact your system administrator.');
             });
            }
            else {
                $("#interstitial").hide();               
                    selfrep.assemblyReportList([]);
                    selfrep.assemblyReportListMaterial([]);                
                selfrep.showReportTables(false);
               
            }
        };

        generateReports.prototype.eventforSavedReports = function (model, event) {
            var laborIdValue = event.target.value;
            if (laborIdValue == "1064") {
                selfrep.selectedSavedReport('LABOR ID');
            } else {
                selfrep.selectedSavedReport('');
            }
        };
        
        return generateReports;
    })