(function ($) {
    $.patientTypes = function (options) {
        var settings = $.extend({
            baseUrl: "/",
            companyIds: "#companySelect",
            branchIds: "#branchSelect",
            departmentIds: "#departmentSelect",
            designationIds: "#designationSelect",
            employeeIds: "#employeeSelect",
            FromDate: "#FromDateSelect",
            FlatPicker: ".flatDate",
            ToDate: "#ToDateSelect",
            load: function () {
                console.log("Loading...");
            }
        }, options);

        var filterUrl = settings.baseUrl + "/getAllFilterEmp";
        var DownloadUrl = settings.baseUrl + "/getAllPdfFilterEmp";

        var setupLoadingOverlay = function () {
            if ($("#customLoadingOverlay").length === 0) {
                $("body").append(`
                    <div id="customLoadingOverlay" style="
                        display: none;
                        position: fixed;
                        top: 0;
                        left: 0;
                        width: 100%;
                        height: 100%;
                        background-color: rgba(0, 0, 0, 0.5);
                        z-index: 9999;
                        justify-content: center;
                        align-items: center;">
                        <div style="
                            background-color: white;
                            padding: 20px;
                            border-radius: 5px;
                            box-shadow: 0 0 10px rgba(0,0,0,0.3);
                            text-align: center;">
                            <div class="spinner-border text-primary" role="status">
                                <span class="sr-only">Loading...</span>
                            </div>
                            <p style="margin-top: 10px; margin-bottom: 0;">Loading data...</p>
                        </div>
                    </div>
                `);
            }
        };

        function showLoading() {
            $("#customLoadingOverlay").css("display", "flex");
        }

        function hideLoading() {
            $("#customLoadingOverlay").hide();
        }
        var reportDataTable = null;
        var initializeMultiselects = function () {
            var selectors = [
                settings.companyIds,
                settings.branchIds,
                settings.departmentIds,
                settings.designationIds,
                settings.employeeIds
            ].join(", ");

            $(selectors).multiselect({
                enableFiltering: true,
                includeSelectAllOption: true,
                selectAllText: 'Select All',
                nonSelectedText: 'Select Items',
                nSelectedText: 'Selected',
                allSelectedText: 'All Selected',
                filterPlaceholder: 'Search.......',
                buttonWidth: '100%',
                maxHeight: 350,
                enableClickableOptGroups: true,
                dropUp: false,
                numberDisplayed: 1,
                enableCaseInsensitiveFiltering: true
            });
        };

        var GetFlatDate = function () {
            flatpickr($(settings.FlatPicker), {
                dateFormat: "Y-m-d",
                altInput: true,
                altFormat: "d/m/Y",
                allowInput: true,
                onReady: function (selectedDates, dateStr, instance) {
                    instance.input.placeholder = "dd/mm/yyyy";
                }
            });
        };

        // Filter Value Getter
        var getFilterValue = function () {
            const fromDateVal = $(settings.FromDate).val();
            const toDateVal = $(settings.ToDate).val();
            var filterData = {
                CompanyCodes: toArray($(settings.companyIds).val()),
                BranchCodes: toArray($(settings.branchIds).val()),
                DepartmentCodes: toArray($(settings.departmentIds).val()),
                DesignationCodes: toArray($(settings.designationIds).val()),
                EmployeeIDs: toArray($(settings.employeeIds).val()),
                FromDate: fromDateVal ? new Date(fromDateVal).toISOString().split('T')[0] : null,
                ToDate: toDateVal ? new Date(toDateVal).toISOString().split('T')[0] : null
            };
            return filterData;
        };

        // Array Helper
        var toArray = function (value) {
            if (!value) return [];
            if (Array.isArray(value)) return value;
            return [value];
        };

        var loadFilterEmp = function () {
            showLoading();
            var filterData = getFilterValue();
            //console.log(filterData);
            $.ajax({
                url: filterUrl,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(filterData),
                success: function (res) {
                    hideLoading();
                    if (!res.isSuccess) {                    
                        showToast('error', res.message);                       
                        return;
                    }
                    console.log(res);
                    $(settings.companyIds, settings, settings.branchIds, settings.departmentIds, settings.designationIds, settings.employeeIds, settings.FromDate, settings.ToDate).off('change');
                    //loadTableData(res);
                    const data = res.data;
                    //initPdfPreview();
                    if (data.companies && data.companies.length > 0 && data.companies.some(x => x.code != null && x.name != null)) {
                        var Companys = data.companies;
                        //console.log(Companys);
                        var optCompany = $(settings.companyIds);
                        $.each(Companys, function (index, company) {
                            //console.log(company);
                            if (company.code != null && company.name != null && optCompany.find(`option[value="${company.code}"]`).length === 0) {
                                optCompany.append(`<option value="${company.code}">${company.name}</option>`)
                            }
                        });
                        optCompany.multiselect('rebuild');
                    }
                    
                    if (data.branches && data.branches.length > 0 && data.branches.some(x => x.code != null && x.name != null)) {
                        var branches = data.branches;
                        var optbranche = $(settings.branchIds);                       
                        $.each(branches, function (index, branche) {
                            if (branche.code != null && branche.name != null && optbranche.find(`option[value="${branche.code}"]`).length === 0) {
                                optbranche.append(`<option value="${branche.code}">${branche.name}</option>`)
                            }
                        });
                        optbranche.multiselect('rebuild');
                    }
                    if (data.departments && data.departments.length > 0 && data.departments.some(x => x.code != null && x.name != null)) {
                        var departments = data.departments;
                        var optDepartment = $(settings.departmentIds);
                        $(settings.branchIds).change(function () {
                            optDepartment.empty();
                        });                       
                        $.each(departments, function (index, department) {
                            if (department.code != null && department.name != null && optDepartment.find(`option[value="${department.code}"]`).length === 0) {
                                optDepartment.append(`<option value="${department.code}">${department.name}</option>`)
                            }
                        })
                        optDepartment.multiselect('rebuild');
                    }
                    if (data.designations && data.designations.length > 0 && data.designations.some(x => x.code != null && x.name != null)) {
                        var designations = data.designations;
                        var optDesignation = $(settings.designationIds);
                        $(settings.branchIds).change(function () {
                            optDesignation.empty();
                        });
                        $(settings.departmentIds).change(function () {
                            optDesignation.empty();
                        });
                        $.each(designations, function (index, designation) {
                            if (designation.code != null && designation.name != null && optDesignation.find(`option[value="${designation.code}"]`).length === 0) {
                                optDesignation.append(`<option value=${designation.code}>${designation.name}</option>`)
                            }
                        });
                        optDesignation.multiselect('rebuild');
                    }
                    if (data.employees && data.employees.length > 0 && data.employees.some(x => x.code != null && x.name != null)) {
                        var employees = data.employees;
                        var optEmployee = $(settings.employeeIds);
                        [settings.branchIds, settings.departmentIds, settings.designationIds].forEach(function (selector) {
                            $(selector).change(function () {
                                optEmployee.empty();
                            });
                        });

                        $.each(employees, function (index, employee) {
                            if (employee.code != null && employee.name != null && optEmployee.find(`option[value=${employee.code}]`).length === 0) {
                                optEmployee.append(`<option value=${employee.code}>${employee.name}( ${employee.code}  )</option>`)
                            }
                        });
                        optEmployee.multiselect('rebuild');
                    }
                                      
                },
                error: function (e) {
                    console.log(e);
                    hideLoading();
                }
            });
        };

        // Button Click Event Binding
        $("#checkCompanyBtn").on("click", function () {
            console.log("Company Value:", getFilterValue());
        });

        function bindFilterChangeEvent() {
            var selectors = [
                settings.companyIds,
                settings.branchIds,
                settings.departmentIds,
                settings.designationIds,
                settings.employeeIds,
                settings.FromDate,
                settings.ToDate
            ].join(", ");

            $(document).off('change', selectors);
            $(document).on('change', selectors, function () {
                loadFilterEmp();
            });
        }
        var loadTableData = function (res) {
            var tableData = res.data.employees;
            console.log(tableData);
            if (reportDataTable !== null) {
                reportDataTable.destroy();
            }
            var tableBody = $("#RosterScheduleReport-grid-body");
            tableBody.empty();
            $.each(tableData, function (index, employee) {               
                var row = $('<tr>');
                row.append(`<td>${++index}</td>`);
                row.append('<td>' + employee.code + '</td >')
                row.append('<td>' + employee.name + '</td >')
                row.append('<td>' + employee.designationName + '</td >')
                //row.append('<td>' + employee.departmentName + '</td >')
                row.append('<td>' + employee.branchName + '</td >')
                row.append('<td>' + employee.showDate + '</td >')
                row.append('<td>' + employee.dayName + '</td >')
                row.append('<td>' + employee.shiftName + '</td >')
                row.append('<td>' + employee.remark + '</td >')
                row.append('<td>' + employee.approvalStatus + '</td >')
                row.append('<td>' + employee.approvedBy + '</td >')
                row.append('<td>' + employee.showApprovalDatetime + '</td >')
            tableBody.append(row);
            })           
        }

       
        //$(document).on('click', '#downloadReport', function () {
        //    LoadFullRosterTable();
        //    var reportValue = $("#reportText").val();
        //    if (reportValue === "downloadPdf") {
        //        PdfDownload();
        //    } else if (reportValue === "downloadWord") {
        //        downloadTableAsWord();
        //    } else if (reportValue === "downloadExcel") {
        //        downloadTableAsExcel();
        //    } else {
        //        showToast("warning", "Plseace Slect Report Option");
        //    }
        //})


        ////var PdfDownload = function () {
        ////    const { jsPDF } = window.jspdf;
        ////    const doc = new jsPDF('landscape');

        ////    doc.autoTable({
        ////        html: '#RosterScheduleReport-grid',
        ////        startY: 10,
        ////        theme: 'grid',
        ////        styles: {
        ////            fontSize: 8,
        ////            cellPadding: 2,
        ////            lineColor: [0, 0, 0],
        ////            lineWidth: 0.3
        ////        },
        ////        headStyles: {
        ////            fillColor: [240, 240, 240],
        ////            textColor: [30, 30, 30],
        ////            fontStyle: 'bold',
        ////            lineColor: [0, 0, 0],
        ////            lineWidth: 0.5,
        ////            halign: 'center',
        ////            valign: 'middle'
        ////        },
        ////        margin: { top: 10, left: 10, right: 10 },

        ////        columnStyles: {
        ////            2: { minCellWidth: 30, maxCellWidth: 55, overflow: 'linebreak' },
        ////            3: { minCellWidth: 30, maxCellWidth: 40, overflow: 'linebreak' },
        ////            4: { minCellWidth: 25, maxCellWidth: 40, overflow: 'linebreak' },
        ////            7: { minCellWidth: 48, maxCellWidth: 70, overflow: 'linebreak' },
        ////        },
        ////        didDrawPage: function (data) {
        ////            pageCount = doc.getNumberOfPages();
        ////            let pageSize = doc.internal.pageSize;
        ////            let pageHeight = pageSize.height ? pageSize.height : pageSize.getHeight();

        ////            doc.setFontSize(10);
        ////            doc.setTextColor(0, 0, 0);
        ////            doc.text(`Page ${pageCount}`, data.settings.margin.left, pageHeight - 10);
        ////        }
        ////    });

        ////    doc.save('Roster-Schedule.pdf');
        ////}
        //function LoadFullRosterTable(callback) {
        //    showLoading();
        //    var filterData = getFilterValue();
        //    $.ajax({
        //        url: DownloadUrl,
        //        type: 'POST',
        //        data: JSON.stringify(filterData),
        //        contentType: 'application/json',
        //        success: function (res) {
        //            var tableBody = $('#RosterScheduleReport-grid tbody');
        //            tableBody.empty();
        //            hideLoading();
        //            $.each(res.data.employees, function (index, emp) {
        //                var row = $('<tr>');
        //                row.append('<td>' + (index + 1) + '</td>');
        //                row.append('<td>' + emp.code + '</td>');
        //                row.append('<td>' + emp.name + '</td>');
        //                row.append('<td>' + emp.designationName + '</td>');
        //                row.append('<td>' + emp.branchName + '</td>');
        //                row.append('<td>' + emp.showDate + '</td>');
        //                row.append('<td>' + emp.dayName + '</td>');
        //                row.append('<td>' + emp.shiftName + '</td>');
        //                row.append('<td>' + emp.remark + '</td>');
        //                row.append('<td>' + emp.approvalStatus + '</td>');
        //                row.append('<td>' + emp.approvedBy + '</td>');
        //                row.append('<td>' + emp.showApprovalDatetime + '</td>');
        //                tableBody.append(row);
        //            });

        //            callback();  // ডেটা লোড শেষে callback চালাও
        //        },
        //        error: function () {
        //            alert('Failed to load data');
        //        }
        //    });
        //}

        //var PdfDownload = function () {
        //    LoadFullRosterTable(function () {
        //        const { jsPDF } = window.jspdf;
        //        const doc = new jsPDF('landscape');

        //        doc.autoTable({
        //            html: '#RosterScheduleReport-grid', 
        //            startY: 10,
        //            theme: 'grid',
        //            styles: {
        //                fontSize: 8,
        //                cellPadding: 2,
        //                lineColor: [0, 0, 0],
        //                lineWidth: 0.3
        //            },
        //            headStyles: {
        //                fillColor: [240, 240, 240],
        //                textColor: [30, 30, 30],
        //                fontStyle: 'bold',
        //                lineColor: [0, 0, 0],
        //                lineWidth: 0.5,
        //                halign: 'center',
        //                valign: 'middle'
        //            },
        //            margin: { top: 10, left: 10, right: 10 },
        //            columnStyles: {
        //                2: { minCellWidth: 30, maxCellWidth: 55, overflow: 'linebreak' },
        //                3: { minCellWidth: 30, maxCellWidth: 40, overflow: 'linebreak' },
        //                4: { minCellWidth: 25, maxCellWidth: 40, overflow: 'linebreak' },
        //                7: { minCellWidth: 48, maxCellWidth: 70, overflow: 'linebreak' },
        //            },
        //            didDrawPage: function (data) {
        //                const pageCount = doc.getNumberOfPages();
        //                let pageSize = doc.internal.pageSize;
        //                let pageHeight = pageSize.height ? pageSize.height : pageSize.getHeight();

        //                doc.setFontSize(10);
        //                doc.setTextColor(0, 0, 0);
        //                doc.text(`Page ${pageCount}`, data.settings.margin.left, pageHeight - 10);
        //            }
        //        });

        //        doc.save('Roster-Schedule.pdf');
        //    });
        //}

        
        //var downloadTableAsWord = function () {
        //    var header = "<!DOCTYPE html>" +
        //        "<html xmlns:v='urn:schemas-microsoft-com:vml' " +
        //        "xmlns:o='urn:schemas-microsoft-com:office:office' " +
        //        "xmlns:w='urn:schemas-microsoft-com:office:word' " +
        //        "xmlns:m='http://schemas.microsoft.com/office/2004/12/omml' " +
        //        "xmlns='http://www.w3.org/TR/REC-html40'>" +
        //        "<head>" +
        //        "<meta charset='utf-8'>" +
        //        "<title>Employee Report</title>" +
        //        "<!--[if gte mso 9]>" +
        //        "<xml>" +
        //        "<w:WordDocument>" +
        //        "<w:View>Print</w:View>" +
        //        "<w:Zoom>90</w:Zoom>" +
        //        "<w:DoNotPromptForConvert/>" +
        //        "<w:DoNotShowRevisions/>" +
        //        "<w:DoNotPrintBodyBackgroundShapes/>" +
        //        "<w:DoNotDisplayPageBoundaries/>" +
        //        "<w:DisplayHorizontalDrawingGridEvery>0</w:DisplayHorizontalDrawingGridEvery>" +
        //        "<w:DisplayVerticalDrawingGridEvery>2</w:DisplayVerticalDrawingGridEvery>" +
        //        "<w:UseMarginsForDrawingGridOrigin/>" +
        //        "<w:ValidateAgainstSchemas/>" +
        //        "<w:SaveIfXMLInvalid>false</w:SaveIfXMLInvalid>" +
        //        "<w:IgnoreMixedContent>false</w:IgnoreMixedContent>" +
        //        "<w:AlwaysShowPlaceholderText>false</w:AlwaysShowPlaceholderText>" +
        //        "</w:WordDocument>" +
        //        "</xml>" +
        //        "<![endif]-->" +
        //        "<style>" +
        //        "@page Section1 { " +
        //        "size: 842pt 595pt; " +
        //        "mso-page-orientation: landscape; " +
        //        "margin: 0.5in 0.5in 0.5in 0.5in; " +
        //        "mso-header-margin: 0in; " +
        //        "mso-footer-margin: 0in; " +
        //        "mso-paper-source: 0; " +
        //        "} " +
        //        "div.Section1 { page: Section1; } " +
        //        "body { " +
        //        "font-family: Arial, sans-serif; " +
        //        "margin: 0; " +
        //        "padding: 0; " +
        //        "} " +
        //        "table { " +
        //        "border-collapse: collapse; " +
        //        "width: 100%; " +
        //        "margin: 0; " +
        //        "padding: 0; " +
        //        "mso-table-lspace: 0pt; " +
        //        "mso-table-rspace: 0pt; " +
        //        "} " +
        //        "table, th, td { " +
        //        "border: 1px solid black; " +
        //        "padding: 4px; " +
        //        "font-size: 10px; " +
        //        "vertical-align: top; " +
        //        "mso-border-alt: solid black .5pt; " +
        //        "} " +
        //        "th { " +
        //        "background-color: #f0f0f0; " +
        //        "font-weight: bold; " +
        //        "} " +
        //        "</style>" +
        //        "</head>" +
        //        "<body>" +
        //        "<div class='Section1'>";

        //    var footer = "</div></body></html>";
        //    var content = document.getElementById("RosterScheduleReport-grid").outerHTML;
        //    var sourceHTML = header + content + footer;
        //    var source = 'data:application/vnd.ms-word;charset=utf-8,' + encodeURIComponent(sourceHTML);
        //    var fileDownload = document.createElement("a");
        //    document.body.appendChild(fileDownload);
        //    fileDownload.href = source;
        //    fileDownload.download = 'EmployeeRosterReport.doc';
        //    fileDownload.click();
        //    document.body.removeChild(fileDownload);
        //}


        ////excel

        //// Date format conversion function
        //var formatDateToDDMMYYYY = function (dateString) {
        //    if (!dateString || dateString.trim() === '') {
        //        return dateString;
        //    }

        //    try {
        //        var date;

        //        // Different date formats handle kora
        //        if (dateString.includes('/')) {
        //            // Already in DD/MM/YYYY or MM/DD/YYYY format
        //            var parts = dateString.split('/');
        //            if (parts.length === 3) {
        //                // Check if it's DD/MM/YYYY (day <= 12) or MM/DD/YYYY
        //                var first = parseInt(parts[0]);
        //                var second = parseInt(parts[1]);
        //                var year = parseInt(parts[2]);

        //                if (first > 12) {
        //                    // First part is day, so DD/MM/YYYY
        //                    return dateString;
        //                } else if (second > 12) {
        //                    // Second part is day, so MM/DD/YYYY - convert to DD/MM/YYYY
        //                    return parts[1] + '/' + parts[0] + '/' + parts[2];
        //                } else {
        //                    // Ambiguous - assume DD/MM/YYYY
        //                    return dateString;
        //                }
        //            }
        //        } else if (dateString.includes('-')) {
        //            // YYYY-MM-DD format
        //            var parts = dateString.split('-');
        //            if (parts.length === 3) {
        //                var year = parts[0];
        //                var month = parts[1];
        //                var day = parts[2];

        //                // Convert to DD/MM/YYYY
        //                return day.padStart(2, '0') + '/' + month.padStart(2, '0') + '/' + year;
        //            }
        //        } else {
        //            // Other formats - try to parse as Date
        //            date = new Date(dateString);
        //            if (!isNaN(date.getTime())) {
        //                var day = date.getDate().toString().padStart(2, '0');
        //                var month = (date.getMonth() + 1).toString().padStart(2, '0');
        //                var year = date.getFullYear();
        //                return day + '/' + month + '/' + year;
        //            }
        //        }

        //        // If nothing works, return original
        //        return dateString;

        //    } catch (error) {
        //        console.log('Date parsing error for:', dateString);
        //        return dateString;
        //    }
        //};
               

        //var downloadTableAsExcel = function () {
        //    // SheetJS CDN link add korte hobe head e:
        //    // <script src="https://cdnjs.cloudflare.com/ajax/libs/xlsx/0.18.5/xlsx.full.min.js"></script>

        //    try {
        //        // Table element get kora
        //        var table = document.getElementById("RosterScheduleReport-grid");
        //        if (!table) {
        //            alert("Table not found!");
        //            return;
        //        }

        //        // Manual data extraction with date formatting
        //        var data = [];
        //        var rows = table.getElementsByTagName('tr');

        //        for (var i = 0; i < rows.length; i++) {
        //            var row = [];
        //            var cells = rows[i].getElementsByTagName(i === 0 ? 'th' : 'td');

        //            for (var j = 0; j < cells.length; j++) {
        //                var cellText = cells[j].innerText || cells[j].textContent || '';
        //                cellText = cellText.trim().replace(/\s+/g, ' ');

        //                // Date format check kora (column 5 = Date column)
        //                if (j === 5 && i > 0 && cellText) { // Header row skip kora
        //                    cellText = formatDateToDDMMYYYY(cellText);
        //                }

        //                row.push(cellText);
        //            }

        //            if (row.length > 0) {
        //                data.push(row);
        //            }
        //        }

        //        // Workbook create kora
        //        var wb = XLSX.utils.book_new();
        //        var ws = XLSX.utils.aoa_to_sheet(data);

        //        // Auto column width calculate kora
        //        var cols = [];
        //        var maxCols = 0;

        //        // Maximum column count ber kora
        //        for (var i = 0; i < data.length; i++) {
        //            if (data[i].length > maxCols) {
        //                maxCols = data[i].length;
        //            }
        //        }

        //        // Prottek column er maximum width calculate kora
        //        for (var col = 0; col < maxCols; col++) {
        //            var maxWidth = 0;

        //            for (var row = 0; row < data.length; row++) {
        //                if (data[row][col]) {
        //                    var cellText = data[row][col].toString();
        //                    var cellWidth = cellText.length;

        //                    // Header text slightly wider rakhte
        //                    if (row === 0) {
        //                        cellWidth = cellWidth * 1.2;
        //                    }

        //                    if (cellWidth > maxWidth) {
        //                        maxWidth = cellWidth;
        //                    }
        //                }
        //            }

        //            // Minimum width 8, maximum width 50 set kora
        //            maxWidth = Math.max(8, Math.min(50, maxWidth + 2));
        //            cols.push({ wch: maxWidth });
        //        }

        //        ws['!cols'] = cols;

        //        // Header row styling
        //        if (data.length > 0) {
        //            for (var col = 0; col < data[0].length; col++) {
        //                var cellRef = XLSX.utils.encode_cell({ r: 0, c: col });
        //                if (!ws[cellRef]) continue;
        //                ws[cellRef].s = {
        //                    font: { bold: true },
        //                    fill: { fgColor: { rgb: "F0F0F0" } },
        //                    alignment: { horizontal: "center" }
        //                };
        //            }
        //        }

        //        // Worksheet add kora
        //        XLSX.utils.book_append_sheet(wb, ws, "Employee Roster");

        //        // File download kora
        //        XLSX.writeFile(wb, "EmployeeRosterReport.xlsx");

        //    } catch (error) {
        //        console.error("Excel export error:", error);
        //        alert("Excel export failed. Please try again.");
        //    }
        //};
              



        // Fixed download event handler
        $(document).on('click', '#downloadReport', function () {
            var reportValue = $("#reportText").val();
            if (reportValue === "downloadPdf") {
                PdfDownload();
            } else if (reportValue === "downloadWord") {
                downloadTableAsWord();
            } else if (reportValue === "downloadExcel") {
                downloadTableAsExcel();
            } else {
                showToast("warning", "Please Select Report Option");
            }
        });

        // Common function to load data for all download types
        function LoadFullRosterTable(callback) {
            showLoading();
            var filterData = getFilterValue();
            $.ajax({
                url: DownloadUrl,
                type: 'POST',
                data: JSON.stringify(filterData),
                contentType: 'application/json',
                success: function (res) {
                    var tableBody = $('#RosterScheduleReport-grid tbody');
                    tableBody.empty();
                    hideLoading();

                    if (res.data && res.data.employees && res.data.employees.length > 0) {
                        $.each(res.data.employees, function (index, emp) {
                            var row = $('<tr>');
                            row.append('<td>' + (index + 1) + '</td>');
                            row.append('<td>' + emp.code + '</td>');
                            row.append('<td>' + emp.name + '</td>');
                            row.append('<td>' + emp.designationName + '</td>');
                            row.append('<td>' + emp.branchName + '</td>');
                            row.append('<td>' + emp.showDate + '</td>');
                            row.append('<td>' + emp.dayName + '</td>');
                            row.append('<td>' + emp.shiftName + '</td>');
                            row.append('<td>' + emp.remark + '</td>');
                            row.append('<td>' + emp.approvalStatus + '</td>');
                            row.append('<td>' + emp.approvedBy + '</td>');
                            row.append('<td>' + emp.showApprovalDatetime + '</td>');
                            tableBody.append(row);
                        });

                        // Data load hoye gele callback call koro
                        if (callback && typeof callback === 'function') {
                            callback();
                        }
                    } else {
                        alert('No data found to download');
                        hideLoading();
                    }
                },
                error: function () {
                    hideLoading();
                    alert('Failed to load data');
                }
            });
        }

        // PDF Download - Already correct
        var PdfDownload = function () {
            LoadFullRosterTable(function () {
                const { jsPDF } = window.jspdf;
                const doc = new jsPDF('landscape');

                doc.autoTable({
                    html: '#RosterScheduleReport-grid',
                    startY: 10,
                    theme: 'grid',
                    styles: {
                        fontSize: 8,
                        cellPadding: 2,
                        lineColor: [0, 0, 0],
                        lineWidth: 0.3
                    },
                    headStyles: {
                        fillColor: [240, 240, 240],
                        textColor: [30, 30, 30],
                        fontStyle: 'bold',
                        lineColor: [0, 0, 0],
                        lineWidth: 0.5,
                        halign: 'center',
                        valign: 'middle'
                    },
                    margin: { top: 10, left: 10, right: 10 },
                    columnStyles: {
                        2: { minCellWidth: 30, maxCellWidth: 55, overflow: 'linebreak' },
                        3: { minCellWidth: 30, maxCellWidth: 40, overflow: 'linebreak' },
                        4: { minCellWidth: 25, maxCellWidth: 40, overflow: 'linebreak' },
                        7: { minCellWidth: 48, maxCellWidth: 70, overflow: 'linebreak' },
                    },
                    didDrawPage: function (data) {
                        const pageCount = doc.getNumberOfPages();
                        let pageSize = doc.internal.pageSize;
                        let pageHeight = pageSize.height ? pageSize.height : pageSize.getHeight();

                        doc.setFontSize(10);
                        doc.setTextColor(0, 0, 0);
                        doc.text(`Page ${pageCount}`, data.settings.margin.left, pageHeight - 10);
                    }
                });

                doc.save('Roster-Schedule.pdf');
            });
        };

        // FIXED Word Download - Now loads data first
        var downloadTableAsWord = function () {
            LoadFullRosterTable(function () {
                // Check if table has data
                var table = document.getElementById("RosterScheduleReport-grid");
                if (!table) {
                    alert("Table not found!");
                    return;
                }

                var rows = table.querySelectorAll('tbody tr');
                if (rows.length === 0) {
                    alert("No data found in table!");
                    return;
                }

                var header = "<!DOCTYPE html>" +
                    "<html xmlns:v='urn:schemas-microsoft-com:vml' " +
                    "xmlns:o='urn:schemas-microsoft-com:office:office' " +
                    "xmlns:w='urn:schemas-microsoft-com:office:word' " +
                    "xmlns:m='http://schemas.microsoft.com/office/2004/12/omml' " +
                    "xmlns='http://www.w3.org/TR/REC-html40'>" +
                    "<head>" +
                    "<meta charset='utf-8'>" +
                    "<title>Employee Report</title>" +
                    "<!--[if gte mso 9]>" +
                    "<xml>" +
                    "<w:WordDocument>" +
                    "<w:View>Print</w:View>" +
                    "<w:Zoom>90</w:Zoom>" +
                    "<w:DoNotPromptForConvert/>" +
                    "<w:DoNotShowRevisions/>" +
                    "<w:DoNotPrintBodyBackgroundShapes/>" +
                    "<w:DoNotDisplayPageBoundaries/>" +
                    "<w:DisplayHorizontalDrawingGridEvery>0</w:DisplayHorizontalDrawingGridEvery>" +
                    "<w:DisplayVerticalDrawingGridEvery>2</w:DisplayVerticalDrawingGridEvery>" +
                    "<w:UseMarginsForDrawingGridOrigin/>" +
                    "<w:ValidateAgainstSchemas/>" +
                    "<w:SaveIfXMLInvalid>false</w:SaveIfXMLInvalid>" +
                    "<w:IgnoreMixedContent>false</w:IgnoreMixedContent>" +
                    "<w:AlwaysShowPlaceholderText>false</w:AlwaysShowPlaceholderText>" +
                    "</w:WordDocument>" +
                    "</xml>" +
                    "<![endif]-->" +
                    "<style>" +
                    "@page Section1 { " +
                    "size: 842pt 595pt; " +
                    "mso-page-orientation: landscape; " +
                    "margin: 0.5in 0.5in 0.5in 0.5in; " +
                    "mso-header-margin: 0in; " +
                    "mso-footer-margin: 0in; " +
                    "mso-paper-source: 0; " +
                    "} " +
                    "div.Section1 { page: Section1; } " +
                    "body { " +
                    "font-family: Arial, sans-serif; " +
                    "margin: 0; " +
                    "padding: 0; " +
                    "} " +
                    "table { " +
                    "border-collapse: collapse; " +
                    "width: 100%; " +
                    "margin: 0; " +
                    "padding: 0; " +
                    "mso-table-lspace: 0pt; " +
                    "mso-table-rspace: 0pt; " +
                    "} " +
                    "table, th, td { " +
                    "border: 1px solid black; " +
                    "padding: 4px; " +
                    "font-size: 10px; " +
                    "vertical-align: top; " +
                    "mso-border-alt: solid black .5pt; " +
                    "} " +
                    "th { " +
                    "background-color: #f0f0f0; " +
                    "font-weight: bold; " +
                    "} " +
                    "</style>" +
                    "</head>" +
                    "<body>" +
                    "<div class='Section1'>";

                var footer = "</div></body></html>";
                var content = table.outerHTML;
                var sourceHTML = header + content + footer;
                var source = 'data:application/vnd.ms-word;charset=utf-8,' + encodeURIComponent(sourceHTML);
                var fileDownload = document.createElement("a");
                document.body.appendChild(fileDownload);
                fileDownload.href = source;
                fileDownload.download = 'EmployeeRosterReport.doc';
                fileDownload.click();
                document.body.removeChild(fileDownload);
            });
        };

        // Date format conversion function
        var formatDateToDDMMYYYY = function (dateString) {
            if (!dateString || dateString.trim() === '') {
                return dateString;
            }

            try {
                var date;

                if (dateString.includes('/')) {
                    var parts = dateString.split('/');
                    if (parts.length === 3) {
                        var first = parseInt(parts[0]);
                        var second = parseInt(parts[1]);
                        var year = parseInt(parts[2]);

                        if (first > 12) {
                            return dateString;
                        } else if (second > 12) {
                            return parts[1] + '/' + parts[0] + '/' + parts[2];
                        } else {
                            return dateString;
                        }
                    }
                } else if (dateString.includes('-')) {
                    var parts = dateString.split('-');
                    if (parts.length === 3) {
                        var year = parts[0];
                        var month = parts[1];
                        var day = parts[2];
                        return day.padStart(2, '0') + '/' + month.padStart(2, '0') + '/' + year;
                    }
                } else {
                    date = new Date(dateString);
                    if (!isNaN(date.getTime())) {
                        var day = date.getDate().toString().padStart(2, '0');
                        var month = (date.getMonth() + 1).toString().padStart(2, '0');
                        var year = date.getFullYear();
                        return day + '/' + month + '/' + year;
                    }
                }

                return dateString;

            } catch (error) {
                console.log('Date parsing error for:', dateString);
                return dateString;
            }
        };

        // FIXED Excel Download - Now loads data first
        var downloadTableAsExcel = function () {
            LoadFullRosterTable(function () {
                try {
                    var table = document.getElementById("RosterScheduleReport-grid");
                    if (!table) {
                        alert("Table not found!");
                        return;
                    }

                    // Check if table has data
                    var dataRows = table.querySelectorAll('tbody tr');
                    if (dataRows.length === 0) {
                        alert("No data found in table!");
                        return;
                    }

                    // Manual data extraction with date formatting
                    var data = [];
                    var rows = table.getElementsByTagName('tr');

                    for (var i = 0; i < rows.length; i++) {
                        var row = [];
                        var cells = rows[i].getElementsByTagName(i === 0 ? 'th' : 'td');

                        for (var j = 0; j < cells.length; j++) {
                            var cellText = cells[j].innerText || cells[j].textContent || '';
                            cellText = cellText.trim().replace(/\s+/g, ' ');

                            // Date format check (column 5 = Date column)
                            if (j === 5 && i > 0 && cellText) {
                                cellText = formatDateToDDMMYYYY(cellText);
                            }

                            row.push(cellText);
                        }

                        if (row.length > 0) {
                            data.push(row);
                        }
                    }

                    // Workbook create
                    var wb = XLSX.utils.book_new();
                    var ws = XLSX.utils.aoa_to_sheet(data);

                    // Auto column width calculate
                    var cols = [];
                    var maxCols = 0;

                    for (var i = 0; i < data.length; i++) {
                        if (data[i].length > maxCols) {
                            maxCols = data[i].length;
                        }
                    }

                    for (var col = 0; col < maxCols; col++) {
                        var maxWidth = 0;

                        for (var row = 0; row < data.length; row++) {
                            if (data[row][col]) {
                                var cellText = data[row][col].toString();
                                var cellWidth = cellText.length;

                                if (row === 0) {
                                    cellWidth = cellWidth * 1.2;
                                }

                                if (cellWidth > maxWidth) {
                                    maxWidth = cellWidth;
                                }
                            }
                        }

                        maxWidth = Math.max(8, Math.min(50, maxWidth + 2));
                        cols.push({ wch: maxWidth });
                    }

                    ws['!cols'] = cols;

                    // Header row styling
                    if (data.length > 0) {
                        for (var col = 0; col < data[0].length; col++) {
                            var cellRef = XLSX.utils.encode_cell({ r: 0, c: col });
                            if (!ws[cellRef]) continue;
                            ws[cellRef].s = {
                                font: { bold: true },
                                fill: { fgColor: { rgb: "F0F0F0" } },
                                alignment: { horizontal: "center" }
                            };
                        }
                    }

                    XLSX.utils.book_append_sheet(wb, ws, "Employee Roster");
                    XLSX.writeFile(wb, "EmployeeRosterReport.xlsx");

                } catch (error) {
                    console.error("Excel export error:", error);
                    alert("Excel export failed. Please try again.");
                }
            });
        };










        //function LoadFullRosterTable(callback) {
        //    $.ajax({
        //        url: '/YourController/GetFullRosterData',
        //        type: 'POST',
        //        data: JSON.stringify(yourFilterDto),
        //        contentType: 'application/json',
        //        success: function (res) {
        //            var tableBody = $('#RosterScheduleReport-Full tbody');
        //            tableBody.empty();

        //            $.each(res.data.employees, function (index, emp) {
        //                var row = $('<tr>');
        //                row.append('<td>' + (index + 1) + '</td>');
        //                row.append('<td>' + emp.code + '</td>');
        //                row.append('<td>' + emp.name + '</td>');
        //                row.append('<td>' + emp.designationName + '</td>');
        //                row.append('<td>' + emp.branchName + '</td>');
        //                row.append('<td>' + emp.showDate + '</td>');
        //                row.append('<td>' + emp.dayName + '</td>');
        //                row.append('<td>' + emp.shiftName + '</td>');
        //                row.append('<td>' + emp.remark + '</td>');
        //                row.append('<td>' + emp.approvalStatus + '</td>');
        //                row.append('<td>' + emp.approvedBy + '</td>');
        //                row.append('<td>' + emp.showApprovalDatetime + '</td>');
        //                tableBody.append(row);
        //            });

        //            callback();  // ডেটা লোড শেষে callback চালাও
        //        },
        //        error: function () {
        //            alert('Failed to load data');
        //        }
        //    });
        //}






        //$(document).click("#preview", function () {
        //    PdfPreview();
        //})
        ////preview
        //async function PdfPreview() {
        //    const { jsPDF } = window.jspdf;
        //    const doc = new jsPDF('landscape');

        //    doc.autoTable({
        //        html: '#RosterScheduleReport-grid',
        //        startY: 10,
        //        theme: 'grid',
        //        styles: { fontSize: 8, cellPadding: 2, lineColor: [0, 0, 0], lineWidth: 0.3 },
        //        headStyles: {
        //            fillColor: [240, 240, 240], textColor: [30, 30, 30], fontStyle: 'bold',
        //            lineColor: [0, 0, 0], lineWidth: 0.5, halign: 'center', valign: 'middle'
        //        },
        //        margin: { top: 10, left: 10, right: 10 },
        //        columnStyles: {
        //            2: { minCellWidth: 30, maxCellWidth: 55, overflow: 'linebreak' },
        //            3: { minCellWidth: 30, maxCellWidth: 40, overflow: 'linebreak' },
        //            4: { minCellWidth: 25, maxCellWidth: 40, overflow: 'linebreak' },
        //            7: { minCellWidth: 48, maxCellWidth: 70, overflow: 'linebreak' },
        //        },
        //        didDrawPage: function (data) {
        //            const pageCount = doc.getNumberOfPages();
        //            const pageSize = doc.internal.pageSize;
        //            const pageHeight = pageSize.height ? pageSize.height : pageSize.getHeight();
        //            doc.setFontSize(10);
        //            doc.setTextColor(0, 0, 0);
        //            doc.text(`Page ${pageCount}`, data.settings.margin.left, pageHeight - 10);
        //        }
        //    });

        //    const pdfBlob = doc.output('blob');
        //    const pdfUrl = URL.createObjectURL(pdfBlob);
        //    window.open(pdfUrl);
        //}

        // Simple PDF preview with iframe
//async function generateAndPreviewPdf() {
//    try {
//        // Check if table exists and has data
//        const table = document.getElementById('RosterScheduleReport-grid');
//        if (!table) {
//            alert('No data table found. Please generate the report first.');
//            return;
//        }

//        // Check if table has rows
//        const rows = table.querySelectorAll('tbody tr');
//        if (rows.length === 0) {
//            alert('No data found in the table. Please load data first.');
//            return;
//        }

//        // Show loading message
//        const previewContainer = document.getElementById('pdf-preview-container');
//        previewContainer.style.display = 'block';
//        previewContainer.innerHTML = '<div style="text-align: center; padding: 50px;"><i class="fas fa-spinner fa-spin"></i> Generating PDF Preview...</div>';

//        // Generate PDF
//        const { jsPDF } = window.jspdf;
//        const doc = new jsPDF('landscape');

//        // Add title
//        doc.setFontSize(16);
//        doc.text('Roster Schedule Report', 14, 15);

//        doc.autoTable({
//            html: '#RosterScheduleReport-grid',
//            startY: 25,
//            theme: 'grid',
//            styles: {
//                fontSize: 8,
//                cellPadding: 2
//            },
//            headStyles: {
//                fillColor: [41, 128, 185],
//                textColor: 255,
//                fontSize: 9,
//                fontStyle: 'bold'
//            },
//            columnStyles: {
//                0: { cellWidth: 'auto' }
//            }
//        });

//        // Create blob URL for iframe
//        const pdfBlob = doc.output('blob');
//        const pdfUrl = URL.createObjectURL(pdfBlob);

//        // Create iframe preview
//        previewContainer.innerHTML = `
//            <div style="background: #f5f5f5; padding: 10px; border-bottom: 1px solid #ddd; display: flex; justify-content: space-between; align-items: center;">
//                <h5 style="margin: 0;">PDF Preview</h5>
//                <button type="button" class="btn btn-sm btn-danger" onclick="closePdfPreview()">
//                    <i class="fas fa-times"></i> Close
//                </button>
//            </div>
//            <iframe
//                src="${pdfUrl}"
//                style="width: 100%; height: 550px; border: none;"
//                title="PDF Preview">
//            </iframe>
//        `;

//        // Scroll to preview
//        previewContainer.scrollIntoView({ behavior: 'smooth' });

//    } catch (error) {
//        console.error('Error generating PDF preview:', error);
//        alert('Error generating PDF preview: ' + error.message);

//        // Hide preview container on error
//        const previewContainer = document.getElementById('pdf-preview-container');
//        previewContainer.style.display = 'none';
//    }
//}

//// Function to close PDF preview
//function closePdfPreview() {
//    const previewContainer = document.getElementById('pdf-preview-container');
//    previewContainer.style.display = 'none';
//}

//// Export functions
//var exportToPdf = function () {
//    const { jsPDF } = window.jspdf;
//    const doc = new jsPDF('landscape');

//    // Add title
//    doc.setFontSize(16);
//    doc.text('Roster Schedule Report', 14, 15);

//    doc.autoTable({
//        html: '#RosterScheduleReport-grid',
//        startY: 25,
//        theme: 'grid',
//        styles: {
//            fontSize: 8,
//            cellPadding: 2
//        },
//        headStyles: {
//            fillColor: [41, 128, 185],
//            textColor: 255,
//            fontSize: 9,
//            fontStyle: 'bold'
//        }
//    });

//    doc.save('roster-schedule-report.pdf');
//};

//var exportToExcel = function () {
//    const table = document.getElementById('RosterScheduleReport-grid');
//    const wb = XLSX.utils.table_to_book(table, { sheet: "Roster Schedule Report" });
//    XLSX.writeFile(wb, 'roster-schedule-report.xlsx');
//};

//var exportToWord = function () {
//    const table = document.getElementById('RosterScheduleReport-grid');
//    const tableHtml = table.outerHTML;

//    // Simple HTML to Word conversion
//    const htmlContent = `
//        <html>
//            <head><title>Roster Schedule Report</title></head>
//            <body>
//                <h1>Roster Schedule Report</h1>
//                ${tableHtml}
//            </body>
//        </html>
//    `;

//    const blob = new Blob([htmlContent], { type: 'application/msword' });
//    const url = URL.createObjectURL(blob);
//    const a = document.createElement('a');
//    a.href = url;
//    a.download = 'roster-schedule-report.doc';
//    a.click();
//    URL.revokeObjectURL(url);
//};

//// Initialize PDF preview functionality
//var initPdfPreview = function () {
//    // Hide preview container initially
//    const previewContainer = document.getElementById('pdf-preview-container');
//    if (previewContainer) {
//        previewContainer.style.display = 'none';
//    }

//    // Bind preview button click
//    $('#checkCompanyBtn, #preview').off('click').on('click', function (e) {
//        e.preventDefault();
//        generateAndPreviewPdf();
//    });

//    // Bind export button click
//    $('#downloadReport').off('click').on('click', function (e) {
//        e.preventDefault();

//        const table = document.getElementById('RosterScheduleReport-grid');
//        if (!table) {
//            alert('No data table found. Please generate the report first.');
//            return;
//        }

//        const reportFormat = $('#reportText').val();

//        try {
//            switch (reportFormat) {
//                case 'downloadPdf':
//                    exportToPdf();
//                    break;
//                case 'downloadExcel':
//                    exportToExcel();
//                    break;
//                case 'downloadWord':
//                    exportToWord();
//                    break;
//                default:
//                    exportToPdf();
//                    break;
//            }
//        } catch (error) {
//            console.error('Export error:', error);
//            alert('Error exporting report: ' + error.message);
//        }
//    });
        //};


        // Enhanced PDF preview with auto-update functionality
let previewState = {
    isVisible: false,
    lastDataHash: null,
    observer: null
};

// Generate hash of table data to detect changes
function generateDataHash() {
    const table = document.getElementById('RosterScheduleReport-grid');
    if (!table) return null;
    
    const tableContent = table.innerHTML;
    return btoa(tableContent).slice(0, 20); // Simple hash
}

// Enhanced PDF generation and preview
async function generateAndPreviewPdf(autoUpdate = false) {
    try {
        // Check if table exists and has data
        const table = document.getElementById('RosterScheduleReport-grid');
        if (!table) {
            if (!autoUpdate) {
                alert('No data table found. Please generate the report first.');
            }
            return;
        }

        // Check if table has rows
        const rows = table.querySelectorAll('tbody tr');
        if (rows.length === 0) {
            if (!autoUpdate) {
                alert('No data found in the table. Please load data first.');
            }
            return;
        }

        // Check if data has changed
        const currentHash = generateDataHash();
        if (autoUpdate && previewState.lastDataHash === currentHash) {
            return; // No changes, skip update
        }
        previewState.lastDataHash = currentHash;

        // Show loading message
        const previewContainer = document.getElementById('pdf-preview-container');
        previewContainer.style.display = 'block';
        previewState.isVisible = true;
        
        if (!autoUpdate) {
            previewContainer.innerHTML = '<div style="text-align: center; padding: 50px;"><i class="fas fa-spinner fa-spin"></i> Generating PDF Preview...</div>';
        }

        // Generate PDF
        const { jsPDF } = window.jspdf;
        const doc = new jsPDF('landscape');
        
        // Add title with timestamp
        doc.setFontSize(16);
        doc.text('Roster Schedule Report', 14, 15);
        
        // Add generated time
        doc.setFontSize(8);
        const now = new Date();
        doc.text(`Generated: ${now.toLocaleString()}`, 14, 22);
        
        doc.autoTable({
            html: '#RosterScheduleReport-grid tbody',
            startY: 30,
            theme: 'grid',
            styles: {
                fontSize: 8,
                cellPadding: 2
            },
            headStyles: {
                fillColor: [41, 128, 185],
                textColor: 255,
                fontSize: 9,
                fontStyle: 'bold'
            },
            columnStyles: {
                0: { cellWidth: 'auto' }
            }
        });

        // Create blob URL for iframe
        const pdfBlob = doc.output('blob');
        const pdfUrl = URL.createObjectURL(pdfBlob);
        
        // Create iframe preview with enhanced controls
        previewContainer.innerHTML = `            
            <div style="border: 1px solid #ddd; border-top: none; background: #f8f9fa;">
                <iframe 
                    src="${pdfUrl}" 
                    style="width: 100%; height: 600px; border: none; display: block;" 
                    title="PDF Preview">
                </iframe>
            </div>
        `;
        
        // Scroll to preview only if not auto-updating
        if (!autoUpdate) {
            previewContainer.scrollIntoView({ behavior: 'smooth' });
        }
        
        // Store current PDF blob for download
        previewContainer.currentPdfBlob = pdfBlob;
        
    } catch (error) {
        console.error('Error generating PDF preview:', error);
        if (!autoUpdate) {
            alert('Error generating PDF preview: ' + error.message);
        }
        
        // Hide preview container on error
        const previewContainer = document.getElementById('pdf-preview-container');
        previewContainer.style.display = 'none';
        previewState.isVisible = false;
    }
}

// Function to refresh preview manually
function refreshPreview() {
    generateAndPreviewPdf(false);
}

// Function to download current PDF
function downloadCurrentPdf() {
    const previewContainer = document.getElementById('pdf-preview-container');
    if (previewContainer.currentPdfBlob) {
        const url = URL.createObjectURL(previewContainer.currentPdfBlob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `roster-schedule-report-${new Date().toISOString().slice(0,10)}.pdf`;
        a.click();
        URL.revokeObjectURL(url);
    }
}

// Function to close PDF preview
function closePdfPreview() {
    const previewContainer = document.getElementById('pdf-preview-container');
    previewContainer.style.display = 'none';
    previewState.isVisible = false;
    
    // Stop observing changes
    if (previewState.observer) {
        previewState.observer.disconnect();
        previewState.observer = null;
    }
}

// Setup table observer for auto-update
function setupTableObserver() {
    const table = document.getElementById('RosterScheduleReport-grid');
    if (!table) return;

    // Disconnect existing observer
    if (previewState.observer) {
        previewState.observer.disconnect();
    }

    // Create new observer
    previewState.observer = new MutationObserver(function(mutations) {
        // Check if preview is visible and data has changed
        if (previewState.isVisible) {
            // Debounce updates
            clearTimeout(previewState.updateTimeout);
            previewState.updateTimeout = setTimeout(() => {
                generateAndPreviewPdf(true);
            }, 1000); // Wait 1 second after last change
        }
    });

    // Start observing
    previewState.observer.observe(table, {
        childList: true,
        subtree: true,
        characterData: true,
        attributes: true
    });
}

// Load preview on page load if data exists
function loadPreviewOnPageLoad() {
    // Wait for page to be fully loaded
    setTimeout(() => {
        const table = document.getElementById('RosterScheduleReport-grid');
        if (table) {
            const rows = table.querySelectorAll('tbody tr');
            if (rows.length > 0) {
                // Data exists, show preview
                generateAndPreviewPdf(false);
                setupTableObserver();
            }
        }
    }, 1000);
}

// Export functions (unchanged)
var exportToPdf = function () {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF('landscape');
    
    doc.setFontSize(16);
    doc.text('Roster Schedule Report', 14, 15);
    
    doc.autoTable({
        html: '#RosterScheduleReport-grid',
        startY: 25,
        theme: 'grid',
        styles: {
            fontSize: 8,
            cellPadding: 2
        },
        headStyles: {
            fillColor: [41, 128, 185],
            textColor: 255,
            fontSize: 9,
            fontStyle: 'bold'
        }
    });

    doc.save('roster-schedule-report.pdf');
};

var exportToExcel = function () {
    const table = document.getElementById('RosterScheduleReport-grid');
    const wb = XLSX.utils.table_to_book(table, { sheet: "Roster Schedule Report" });
    XLSX.writeFile(wb, 'roster-schedule-report.xlsx');
};

var exportToWord = function () {
    const table = document.getElementById('RosterScheduleReport-grid');
    const tableHtml = table.outerHTML;
    
    const htmlContent = `
        <html>
            <head><title>Roster Schedule Report</title></head>
            <body>
                <h1>Roster Schedule Report</h1>
                ${tableHtml}
            </body>
        </html>
    `;
    
    const blob = new Blob([htmlContent], { type: 'application/msword' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'roster-schedule-report.doc';
    a.click();
    URL.revokeObjectURL(url);
};

// Enhanced initialization
var initPdfPreview = function () {
    // Hide preview container initially
    const previewContainer = document.getElementById('pdf-preview-container');
    if (previewContainer) {
        previewContainer.style.display = 'none';
    }

    // Bind preview button clicks
    $('#checkCompanyBtn, #preview').off('click').on('click', function (e) {
        e.preventDefault();
        generateAndPreviewPdf(false);
        setupTableObserver();
    });

    // Bind export button clicks
    //$('#downloadReport').off('click').on('click', function (e) {
    //    e.preventDefault();
        
    //    const table = document.getElementById('RosterScheduleReport-grid');
    //    if (!table) {
    //        alert('No data table found. Please generate the report first.');
    //        return;
    //    }

    //    const reportFormat = $('#reportText').val();
        
    //    try {
    //        switch (reportFormat) {
    //            case 'downloadPdf':
    //                exportToPdf();
    //                break;
    //            case 'downloadExcel':
    //                exportToExcel();
    //                break;
    //            case 'downloadWord':
    //                exportToWord();
    //                break;
    //            default:
    //                exportToPdf();
    //                break;
    //        }
    //    } catch (error) {
    //        console.error('Export error:', error);
    //        alert('Error exporting report: ' + error.message);
    //    }
    //});

    // Load preview on page load
    loadPreviewOnPageLoad();
};

// Auto-initialize when DOM is ready
$(document).ready(function() {
    initPdfPreview();
});




        // Optional: Load Callback
        var init = function () {
            GetFlatDate();
            settings.load(); 
            initializeMultiselects();
            setupLoadingOverlay();
            bindFilterChangeEvent();
            loadFilterEmp();
            initPdfPreview();

        };
        init();
    };
})(jQuery);
