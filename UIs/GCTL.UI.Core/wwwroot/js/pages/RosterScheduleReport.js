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
                //console.log("Loading...");
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
                nonSelectedText: '--select items--',
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
                    //console.log(res.data);
                    $(settings.companyIds, settings, settings.branchIds, settings.departmentIds, settings.designationIds, settings.employeeIds, settings.FromDate, settings.ToDate).off('change');
                    //loadTableData(res);
                    const data = res.data;
                    //initPdfPreview();
                    if (data.companies && data.companies.length > 0 && data.companies.some(x => x.code != null && x.name != null)) {
                        var Companys = data.companies;
                        ////console.log(Companys);
                        var optCompany = $(settings.companyIds);
                        $.each(Companys, function (index, company) {
                            ////console.log(company);
                            if (company.code != null && company.name != null && company.name != '' && optCompany.find(`option[value="${company.code}"]`).length === 0) {
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
                        var optDepartments = $(settings.departmentIds);
                            [settings.branchIds, settings.companyIds].forEach(function (selector) {
                                $(selector).change(function () {
                                    optDepartments.empty();
                                });
                            });
                        $.each(departments, function (index, department) {
                            if (department.code != null && department.name != null && optDepartments.find(`option[value="${department.code}"]`).length === 0) {
                                optDepartments.append(`<option value="${department.code}">${department.name}</option>`)
                            }
                        });
                        optDepartments.multiselect('rebuild');
                    }                   
                    if (data.designations && data.designations.length > 0 && data.designations.some(x => x.code != null && x.name != null)) {
                        var designations = data.designations;
                        var optDesignations = $(settings.designationIds);
                        $(settings.branchIds).change(function () {
                            optDesignations.empty();
                        });

                        $(settings.companyIds).change(function () {
                            optDesignations.empty();
                        });

                        $(settings.departmentIds).change(function () {
                            optDesignations.empty();
                        });

                        $.each(designations, function (index, designation) {
                            if (designation.code != null && designation.name != null && optDesignations.find(`option[value="${designation.code}"]`).length === 0) {
                                optDesignations.append(`<option value="${designation.code}">${designation.name}</option>`)
                            }
                        });
                        optDesignations.multiselect('rebuild');
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
                    $(`${settings.companyIds}, ${settings.branchIds}, ${settings.departmentIds}, ${settings.designationIds}, ${settings.employeeIds}, ${settings.FromDate}, ${settings.ToDate}`).on('change', function () {
                        //console.log("Filter changed");
                        loadFilterEmp();
                    });
                },
                complete: function () {
                    hideLoading();
                },
                error: function (e) {
                    //console.log(e);
                    hideLoading();
                }
            });
        };


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
                        if (callback && typeof callback === 'function') {
                            callback(res.data.employees); 
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


        var PdfDownload = function () {
                LoadFullRosterTable(function (employees) {
                    const { jsPDF } = window.jspdf;
                    const doc = new jsPDF({
                        orientation: 'landscape',
                        unit: 'mm',
                        format: 'a4'
                    });

                    if (!employees || employees.length === 0) {
                        alert("No data found.");
                        return;
                    }

                    const pageWidth = doc.internal.pageSize.getWidth();
                    const companyName = employees[0].companyName || "Metro Tech Ltd.";
                    const currentDate = new Date().toLocaleDateString();

                    function addHeader() {
                        doc.setFontSize(18);
                        doc.setFont("times", "bold");
                        doc.text(companyName, pageWidth / 2, 10, { align: 'center' });

                        doc.setFontSize(12);
                        doc.setFont("times", "normal");
                        doc.text("Roster Schedule Report", pageWidth / 2, 16, { align: 'center' });

                        const lineLength = pageWidth / 5;
                        const startX = (pageWidth - lineLength) / 2;
                        const endX = startX + lineLength;

                        doc.setDrawColor(0);
                        doc.setLineWidth(0.5);
                        doc.line(startX, 18, endX, 18);

                    }
                    addHeader();

                    let startY = 25;

                    let departmentGroups = {};
                    employees.forEach(function (emp) {
                        let dept = emp.departmentName || "Unknown";
                        if (!departmentGroups[dept]) {
                            departmentGroups[dept] = [];
                        }
                        departmentGroups[dept].push(emp);
                    });

                    for (const dept in departmentGroups) {
                        if (startY !== 25) startY += 10;

                        doc.setFontSize(14);
                        doc.setFont("times", "bold");
                        doc.text("Department: " + dept, 10, startY);
                        startY += 6;

                        let tempTable = $('<table>');
                        tempTable.append($('#RosterScheduleReport-grid thead').clone());
                        let tbody = $('<tbody>');

                        departmentGroups[dept].forEach(function (emp, index) {
                            let row = $('<tr>');
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
                            tbody.append(row);
                        });

                        tempTable.append(tbody);

                        doc.autoTable({
                            html: tempTable[0],
                            startY: startY,
                            theme: 'grid',
                            styles: {
                                fontSize: 8,
                                cellPadding: 2,
                                lineColor: [200, 200, 200],
                                lineWidth: 0.1,
                                textColor: [0, 0, 0],
                            },
                            headStyles: {
                                fillColor: [230, 230, 230],
                                textColor: [50, 50, 50],
                                fontStyle: 'bold',
                                lineColor: [180, 180, 180],
                                lineWidth: 0.1,
                                halign: 'center',
                                valign: 'middle'
                            },
                            margin: { top: 25, left: 10, right: 10 },
                            columnStyles: {
                                1: { minCellWidth: 22, maxCellWidth: 35, overflow: 'linebreak' },
                                2: { minCellWidth: 28, maxCellWidth: 55, overflow: 'linebreak' },
                                3: { minCellWidth: 28, maxCellWidth: 40, overflow: 'linebreak' },
                                4: { minCellWidth: 20, maxCellWidth: 35, overflow: 'linebreak' },
                                7: { minCellWidth: 55, maxCellWidth: 70, overflow: 'linebreak' },
                                8: { minCellWidth: 30, maxCellWidth: 50, overflow: 'linebreak' },
                                0: { halign: 'center', valign: 'middle' },
                                5: { halign: 'center', valign: 'middle' },
                                6: { halign: 'center', valign: 'middle' },
                                9: { halign: 'center', valign: 'middle' },
                                10: { halign: 'center', valign: 'middle' },
                                11: { halign: 'center', valign: 'middle' },
                            },
                            pageBreak: 'auto',

                            didDrawPage: function (data) {
                                const pageSize = doc.internal.pageSize;
                                const pageHeight = pageSize.height ? pageSize.height : pageSize.getHeight();
                                const pageWidth = pageSize.width ? pageSize.width : pageSize.getWidth();

                                let pageNumber = doc.internal.getCurrentPageInfo().pageNumber;
                                let totalPages = '{total_pages_count_string}';

                                let leftText = 'Page ' + pageNumber + ' of ' + totalPages;
                                let userActionText = 'Created by: ' + employees[0].luser + ' - ' + currentDate;

                                doc.setFontSize(9);
                                doc.setTextColor(100);

                                doc.text(leftText, 10, pageHeight - 10);
                                doc.text(userActionText, pageWidth - 10, pageHeight - 10, { align: 'right' });
                            }
                        });

                        startY = doc.lastAutoTable.finalY;
                    }

                    if (typeof doc.putTotalPages === 'function') {
                        doc.putTotalPages('{total_pages_count_string}');
                    }
                doc.save('Roster_Report_By_Department.pdf');
                });

        };


        $('#btnPreviewPdf').on('click', function () {
            PdfPreview();
        });

        var PdfPreview = function () {
            LoadFullRosterTable(function (employees) {
                const { jsPDF } = window.jspdf;
                const doc = new jsPDF({
                    orientation: 'landscape',
                    unit: 'mm',
                    format: 'a4'
                });
                //const doc = new jsPDF({
                //    orientation: 'landscape',
                //    unit: 'pt', 
                //    format: [842, 595]
                //});

                if (!employees || employees.length === 0) {
                    alert("No data found.");
                    return;
                }

                const pageWidth = doc.internal.pageSize.getWidth();
                const companyName = employees[0].companyName || "Metro Tech Ltd.";
                const currentDate = new Date().toLocaleDateString();

                function addHeader() {
                    doc.setFontSize(18);
                    doc.setFont("times", "bold");
                    doc.text(companyName, pageWidth / 2, 10, { align: 'center' });

                    doc.setFontSize(12);
                    doc.setFont("times", "normal");
                    doc.text("Roster Schedule Report", pageWidth / 2, 16, { align: 'center' });

                    const lineLength = pageWidth / 5; 
                    const startX = (pageWidth - lineLength) / 2;  
                    const endX = startX + lineLength;  

                    doc.setDrawColor(0);
                    doc.setLineWidth(0.5);
                    doc.line(startX, 18, endX, 18);

                }
                addHeader();

                let startY = 25;

                let departmentGroups = {};
                employees.forEach(function (emp) {
                    let dept = emp.departmentName || "Unknown";
                    if (!departmentGroups[dept]) {
                        departmentGroups[dept] = [];
                    }
                    departmentGroups[dept].push(emp);
                });

                for (const dept in departmentGroups) {
                    if (startY !== 25) startY += 10;

                    doc.setFontSize(14);
                    doc.setFont("times", "bold");
                    doc.text("Department: " + dept, 10, startY);
                    startY += 6;

                    let tempTable = $('<table>');
                    tempTable.append($('#RosterScheduleReport-grid thead').clone());
                    let tbody = $('<tbody>');

                    departmentGroups[dept].forEach(function (emp, index) {
                        let row = $('<tr>');
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
                        tbody.append(row);
                    });

                    tempTable.append(tbody);

                    doc.autoTable({
                        html: tempTable[0],
                        startY: startY,
                        theme: 'grid',
                        styles: {
                            fontSize: 8,
                            cellPadding: 2,
                            lineColor: [200, 200, 200],
                            lineWidth: 0.1,
                            textColor: [0, 0, 0],
                        },
                        headStyles: {
                            fillColor: [230, 230, 230],
                            textColor: [50, 50, 50],
                            fontStyle: 'bold',
                            lineColor: [180, 180, 180],
                            lineWidth: 0.1,
                            halign: 'center',
                            valign: 'middle'
                        },
                        margin: { top: 25, left: 10, right: 10 },
                        columnStyles: {
                            1: { minCellWidth: 22, maxCellWidth: 35, overflow: 'linebreak' },
                            2: { minCellWidth: 28, maxCellWidth: 55, overflow: 'linebreak' },
                            3: { minCellWidth: 28, maxCellWidth: 40, overflow: 'linebreak' },
                            4: { minCellWidth: 20, maxCellWidth: 35, overflow: 'linebreak' },
                            7: { minCellWidth: 55, maxCellWidth: 70, overflow: 'linebreak' },
                            8: { minCellWidth: 30, maxCellWidth: 50, overflow: 'linebreak' },
                            0: { halign: 'center', valign: 'middle' },
                            5: { halign: 'center', valign: 'middle' },
                            6: { halign: 'center', valign: 'middle' },
                            9: { halign: 'center', valign: 'middle' },
                            10: { halign: 'center', valign: 'middle' },
                            11: { halign: 'center', valign: 'middle' },
                        },
                        pageBreak: 'auto',

                        didDrawPage: function (data) {
                            const pageSize = doc.internal.pageSize;
                            const pageHeight = pageSize.height ? pageSize.height : pageSize.getHeight();
                            const pageWidth = pageSize.width ? pageSize.width : pageSize.getWidth();

                            let pageNumber = doc.internal.getCurrentPageInfo().pageNumber;
                            let totalPages = '{total_pages_count_string}';

                            let leftText = 'Page ' + pageNumber + ' of ' + totalPages;
                            let userActionText = 'Created by: ' + employees[0].luser + ' - ' + currentDate;

                            doc.setFontSize(9);
                            doc.setTextColor(100);

                            doc.text(leftText, 10, pageHeight - 10);
                            doc.text(userActionText, pageWidth - 10, pageHeight - 10, { align: 'right' });
                        }
                    });

                    startY = doc.lastAutoTable.finalY;
                }

                if (typeof doc.putTotalPages === 'function') {
                    doc.putTotalPages('{total_pages_count_string}');
                }

                const blob = doc.output('blob');
                const url = URL.createObjectURL(blob);

                $('#pdf-preview-container').html(`<iframe src="${url}" width="100%" height="600px" style="border:1px solid #ccc;"></iframe>`);
                $('#pdf-preview-container').show();
            });
        };



        var downloadTableAsWord = function () {
            LoadFullRosterTable(function (employees) {
                if (!employees || employees.length === 0) {
                    alert("No data found!");
                    return;
                }

                // Group by department
                let departmentGroups = {};
                employees.forEach(function (emp) {
                    let dept = emp.departmentName || "Unknown";
                    if (!departmentGroups[dept]) {
                        departmentGroups[dept] = [];
                    }
                    departmentGroups[dept].push(emp);
                });

                var companyName = employees[0].companyName || "";
                var reportTitle = "Employee Roster Report";
                var currentDate = new Date().toLocaleDateString();
                var userName = employees[0].luser || ""; 

                var header = "<!DOCTYPE html>" +
                    "<html xmlns:v='urn:schemas-microsoft-com:vml' " +
                    "xmlns:o='urn:schemas-microsoft-com:office:office' " +
                    "xmlns:w='urn:schemas-microsoft-com:office:word' " +
                    "xmlns:m='http://schemas.microsoft.com/office/2004/12/omml' " +
                    "xmlns='http://www.w3.org/TR/REC-html40'>" +
                    "<head>" +
                    "<meta charset='utf-8'>" +
                    "<title>" + reportTitle + "</title>" +
                    "<!--[if gte mso 9]>" +
                    "<xml><w:WordDocument><w:View>Print</w:View><w:Zoom>90</w:Zoom></w:WordDocument></xml>" +
                    "<![endif]-->" +
                    "<style>" +
                    "@page Section1 { size: 842pt 595pt; mso-page-orientation: landscape; margin: 0.5in; } " +
                    "div.Section1 { page: Section1; } " +
                    "body { font-family: 'Times New Roman', serif; margin: 0; padding: 0; } " +
                    ".header { text-align: center; margin-top: 10px; font-size: 20px; font-weight: bold; } " +
                    ".sub-header { text-align: center; font-size: 14px; margin-top: 5px; } " +
                    "h2 { font-size: 16px; font-weight: bold; margin: 20px 0 10px; color: #333; border-bottom: 2px solid #ccc; padding-bottom: 5px; } " +
                    "table { border-collapse: collapse; width: 100%; margin: 0; } " +
                    "table, th, td { border: 1px solid black; padding: 0; margin: 0; font-size: 10px; vertical-align: top; line-height: 1; } " +
                    "th { background-color: #f0f0f0; font-weight: bold; text-align: center; } " +
                    "tr { height: auto; } " +
                    "td { height: auto; } " +
                    "table td:nth-child(9), table th:nth-child(9) { " +
                    "    width: 100px; " +
                    "    max-width: 100px; " +
                    "    word-wrap: break-word; " +
                    "    font-weight: bold; " +
                    "    font-size: 11px; " +
                    "} " +
                    ".footer { margin-top: 20px; font-size: 12px; } " +
                    ".page-info { text-align: left; display: inline-block; width: 50%; } " +
                    ".date-user { text-align: right; display: inline-block; width: 49%; } " +
                    "</style>" +
                    "</head>" +
                    "<body><div class='Section1'>" +
                    "<div class='header'>" + companyName + "</div>" +
                    "<div class='sub-header'>" + reportTitle + "</div>";

                // Table header
                var originalTable = document.getElementById("RosterScheduleReport-grid");
                var headerRow = '';
                if (originalTable) {
                    var headers = originalTable.querySelectorAll('thead th');
                    headerRow = '<tr>';
                    headers.forEach(function (header, index) {
                        if (index === 8) { 
                            headerRow += '<th style="width: 100px;  font-weight: bold; padding: 0; margin: 0;">' + (header.innerText || header.textContent || '') + '</th>';
                        } else {
                            headerRow += '<th style="padding: 0; margin: 0;">' + (header.innerText || header.textContent || '') + '</th>';
                        }
                    });
                    headerRow += '</tr>';
                } else {
                    headerRow = '<tr><th style="padding: 0; margin: 0;">SN</th><th style="padding: 0; margin: 0;">Code</th><th style="padding: 0; margin: 0;">Name</th><th style="padding: 0; margin: 0;">Designation</th><th style="padding: 0; margin: 0;">Branch</th><th style="padding: 0; margin: 0;">Date</th><th style="padding: 0; margin: 0;">Day</th><th style="padding: 0; margin: 0;">Shift</th><th style="width: 100px; font-weight: bold; padding: 0; margin: 0;">Remark</th><th style="padding: 0; margin: 0;">Approval Status</th><th style="padding: 0; margin: 0;">Approved By</th><th style="padding: 0; margin: 0;">Approval DateTime</th></tr>';
                }

                var content = '';
                for (const dept in departmentGroups) {
                    content += '<h2>Department: ' + dept + '</h2>';
                    content += '<table>' + headerRow;

                    departmentGroups[dept].forEach(function (emp, index) {
                        content += '<tr>';
                        content += '<td style="text-align: center; padding: 0; margin: 0;">' + (index + 1) + '</td>';
                        content += '<td style="padding: 0; margin: 0; text-align: center;">' + (emp.code || '') + '</td>';
                        content += '<td style="padding: 0 0 0 5px; margin: 0;">' + (emp.name || '') + '</td>';
                        content += '<td style="padding: 0 0 0 5px; margin: 0;">' + (emp.designationName || '') + '</td>';
                        content += '<td style="padding: 0; margin: 0; text-align: center;">' + (emp.branchName || '') + '</td>';
                        content += '<td style="text-align: center; padding: 0; margin: 0;">' + (emp.showDate || '') + '</td>';
                        content += '<td style="text-align: center; padding: 0; margin: 0;">' + (emp.dayName || '') + '</td>';
                        content += '<td style="padding: 0 0 0 5px; margin: 0;">' + (emp.shiftName || '') + '</td>';
                        content += '<td style="width: 100px; max-width: 100px; word-wrap: break-word; padding: 0 0 0 5px; margin: 0;">' + (emp.remark || '') + '</td>';
                        content += '<td style="text-align: center; padding: 0; margin: 0;">' + (emp.approvalStatus || '') + '</td>';
                        content += '<td style="text-align: center; padding: 0; margin: 0;">' + (emp.approvedBy || '') + '</td>';
                        content += '<td style="text-align: center; padding: 0; margin: 0;">' + (emp.showApprovalDatetime || '') + '</td>';
                        content += '</tr>';
                    });

                    content += '</table>';
                }

                var footer = "<div class='footer'>" +
                    "<div class='page-info'>Page: 1 of 1</div>" +
                    "<div class='date-user'>Generated on: " + currentDate + " | By: " + userName + "</div>" +
                    "</div>" +
                    "</div></body></html>";

                var sourceHTML = header + content + footer;
                var source = 'data:application/vnd.ms-word;charset=utf-8,' + encodeURIComponent(sourceHTML);
                var fileDownload = document.createElement("a");
                document.body.appendChild(fileDownload);
                fileDownload.href = source;
                fileDownload.download = 'EmployeeRosterReport_ByDepartment.doc';
                fileDownload.click();
                document.body.removeChild(fileDownload);
            });
        };



        var downloadTableAsExcel = function () {
            LoadFullRosterTable(function (employees) {
                if (!employees || employees.length === 0) {
                    alert("No data found!");
                    return;
                }

                try {
                    if (typeof XLSX === 'undefined') {
                        alert("Excel library not loaded. Please refresh the page and try again.");
                        return;
                    }

                    var formatDateForExcel = function (dateString) {
                        if (!dateString || dateString.trim() === '') {
                            return dateString;
                        }
                        try {
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
                                var date = new Date(dateString);
                                if (!isNaN(date.getTime())) {
                                    var day = date.getDate().toString().padStart(2, '0');
                                    var month = (date.getMonth() + 1).toString().padStart(2, '0');
                                    var year = date.getFullYear();
                                    return day + '/' + month + '/' + year;
                                }
                            }
                            return dateString;
                        } catch (error) {
                            //console.log('Date parsing error for:', dateString);
                            return dateString;
                        }
                    };

                    var departmentGroups = {};
                    employees.forEach(function (emp) {
                        var dept = emp.departmentName || "Unknown";
                        if (!departmentGroups[dept]) {
                            departmentGroups[dept] = [];
                        }
                        departmentGroups[dept].push(emp);
                    });

                    var wb = XLSX.utils.book_new();
                    var headers = ['SN', 'Code', 'Name', 'Designation', 'Branch', 'Date', 'Day', 'Shift', 'Remark', 'Approval Status', 'Approved By', 'Approval DateTime'];
                    var allData = [];

                    var companyName = employees[0].companyName || "";
                    var companyRow = [];
                    companyRow[0] = companyName;
                    allData.push(companyRow);

                    var reportTitleRow = [];
                    reportTitleRow[0] = "Roster Schedule Report";
                    allData.push(reportTitleRow);

                    allData.push([]);

                    

                    for (var dept in departmentGroups) {
                        if (departmentGroups.hasOwnProperty(dept)) {
                          
                            var deptHeaderRow = [];
                            deptHeaderRow[0] = 'Department: ' + dept;
                            allData.push(deptHeaderRow);

                            allData.push(headers);

                            for (var empIndex = 0; empIndex < departmentGroups[dept].length; empIndex++) {
                                var emp = departmentGroups[dept][empIndex];
                                var rowData = [
                                    empIndex + 1,
                                    emp.code || '',
                                    emp.name || '',
                                    emp.designationName || '',
                                    emp.branchName || '',
                                    formatDateForExcel(emp.showDate || ''),
                                    emp.dayName || '',
                                    emp.shiftName || '',
                                    emp.remark || '',
                                    emp.approvalStatus || '',
                                    emp.approvedBy || '',
                                    emp.showApprovalDatetime || ''
                                ];
                                allData.push(rowData);
                            }

                            allData.push([]);
                        }
                    }

                    var ws = XLSX.utils.aoa_to_sheet(allData);
                    ws['!merges'] = ws['!merges'] || [];
                    ws['!merges'].push({ s: { r: 0, c: 0 }, e: { r: 0, c: 4 } }); 
                    ws['!merges'].push({ s: { r: 1, c: 0 }, e: { r: 1, c: 4 } });

                    try {
                        var companyCellRef = XLSX.utils.encode_cell({ r: 0, c: 0 });
                        if (ws[companyCellRef]) {
                            ws[companyCellRef].s = {
                                font: { bold: true, sz: 16 },
                                alignment: { horizontal: 'center' }
                            };
                        }

                        var reportTitleCellRef = XLSX.utils.encode_cell({ r: 1, c: 0 });
                        if (ws[reportTitleCellRef]) {
                            ws[reportTitleCellRef].s = {
                                font: { bold: true, sz: 12 },
                                alignment: { horizontal: 'center' }
                            };
                        }

                        for (var rowIndex = 0; rowIndex < allData.length; rowIndex++) {
                            if (allData[rowIndex][0] && allData[rowIndex][0].toString().startsWith("Department:")) {
                                ws['!merges'].push({ s: { r: rowIndex, c: 0 }, e: { r: rowIndex, c: 11 } });
                                var cellRef = XLSX.utils.encode_cell({ r: rowIndex, c: 0 });
                                if (ws[cellRef]) {
                                    ws[cellRef].s = {
                                        font: { bold: true },
                                        alignment: { horizontal: 'center' }
                                    };
                                }
                            }
                        }
                    } catch (styleError) {
                        //console.log("Styling error (non-critical):", styleError);
                    }

                    for (var rowIndex = 0; rowIndex < allData.length; rowIndex++) {
                        if (allData[rowIndex][0] && allData[rowIndex][0].toString().startsWith("Department:")) {
                            ws['!merges'].push({ s: { r: rowIndex, c: 0 }, e: { r: rowIndex, c: 10 } });
                        }
                    }

                    var cols = [];
                    var maxCols = 0;
                    for (var i = 0; i < allData.length; i++) {
                        if (allData[i].length > maxCols) {
                            maxCols = allData[i].length;
                        }
                    }

                    for (var col = 0; col < maxCols; col++) {
                        var maxWidth = 0;
                        for (var row = 0; row < allData.length; row++) {
                            if (allData[row] && allData[row][col]) {
                                var cellText = allData[row][col].toString();
                                var cellWidth = cellText.length;
                                if (cellWidth > maxWidth) {
                                    maxWidth = cellWidth;
                                }
                            }
                        }
                        maxWidth = col === 0 ? 5 : Math.max(8, Math.min(50, maxWidth + 2));
                        cols.push({ wch: maxWidth });
                    }
                    ws['!cols'] = cols;

                    try {
                        for (var rowIndex = 0; rowIndex < allData.length; rowIndex++) {
                            if (allData[rowIndex][0] && allData[rowIndex][0].toString().startsWith("Department:")) {
                                var cellRef = XLSX.utils.encode_cell({ r: rowIndex, c: 0 });
                                if (ws[cellRef]) {
                                    ws[cellRef].s = {
                                        font: { bold: true },
                                        alignment: { horizontal: 'center' }
                                    };
                                }
                            }
                            if (rowIndex === 0) {
                                var cellRef = XLSX.utils.encode_cell({ r: rowIndex, c: 0 });
                                if (ws[cellRef]) {
                                    ws[cellRef].s = {
                                        font: { bold: true, sz: 14 },
                                        alignment: { horizontal: 'center' }
                                    };
                                }
                            }
                        }
                    } catch (styleError) {
                        //console.log("Styling error (non-critical):", styleError);
                    }

                    XLSX.utils.book_append_sheet(wb, ws, "Roster by Department");
                    XLSX.writeFile(wb, "EmployeeRosterReport_ByDepartment.xlsx");

                } catch (error) {
                    console.error("Excel export error:", error);
                    alert("Excel export failed: " + error.message + ". Please try again.");
                }
            });
        }

        var init = function () {
            GetFlatDate();
            settings.load(); 
            initializeMultiselects();
            setupLoadingOverlay();
            loadFilterEmp();    
        };
        init();
    };
})(jQuery);
