(function ($) {
    $.patientTypes = function (options) {
        var settings = $.extend({
            baseUrl: "/",
            companyIds: "#companySelect",
            branchIds: "#branchSelect",
            departmentIds: "#departmentSelect",
            designationIds: "#designationSelect",
            employeeIds: "#employeeSelect",
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

        //var GetFlatDate = function () {
        //    flatpickr($(settings.FlatPicker), {
        //        dateFormat: "Y-m-d",
        //        altInput: true,
        //        altFormat: "d/m/Y",
        //        allowInput: true,
        //        onReady: function (selectedDates, dateStr, instance) {
        //            instance.input.placeholder = "dd/mm/yyyy";
        //        }
        //    });
        //};

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
        var toArray = function (value) {
            if (!value) return [];
            if (Array.isArray(value)) return value;
            return [value];
        };

        var loadFilterEmp = function () {
            //showLoading();
            var filterData = getFilterValue();
            //console.log(filterData);
            //console.log("samaul");
            $.ajax({
                url: filterUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(filterData),
                success: function (res) {
                    if (!res.isSuccess) {
                        alert(res.message);
                        return;
                    }

                    $(settings.companyIds, settings, settings.branchIds, settings.departmentIds, settings.designationIds, settings.employeeIds, settings.FromDate, settings.ToDate).off('change');
                    //loadTableData(res);
                    const data = res.data;
                    //console.log(res);
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
                }, complete: function () {
                    hideLoading();
                },
                error: function (e) {
                    //hideLoading();
                    //console.log(e);
                }
            });
        }


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
            //console.log(filterData);
            $.ajax({
                url: DownloadUrl,
                type: 'POST',
                data: JSON.stringify(filterData),
                contentType: 'application/json',
                success: function (res) {
                    var tableBody = $('#employee-pfAssign-grid-report tbody');
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
                if (!employees || employees.length === 0) {
                    alert("No data found.");
                    return;
                }
                //console.log(employees);
                getImageBase64FromUrl('/images/DP_logo.png', function (base64Logo) {
                    const { jsPDF } = window.jspdf;
                    const doc = new jsPDF({
                        orientation: 'portrait',
                        unit: 'pt',
                        format: [842, 595]
                    });                   
                        const pageWidth = doc.internal.pageSize.getWidth();
                        const companyName = employees[0].company || "";
                        const fromDate = employees[0].fromDate || "";
                        const toDate = employees[0].toDate || "";
                        const userName = employees[0].luser || "";
                        const now = new Date();

                        const day = String(now.getDate()).padStart(2, '0');
                        const month = String(now.getMonth() + 1).padStart(2, '0');
                        const year = now.getFullYear();

                        let hours = now.getHours();
                        const minutes = String(now.getMinutes()).padStart(2, '0');
                        const seconds = String(now.getSeconds()).padStart(2, '0');
                        const ampm = hours >= 12 ? 'PM' : 'AM';
                        hours = hours % 12 || 12;
                        hours = String(hours).padStart(2, '0');
                        const currentDate = `${day}/${month}/${year} ${hours}:${minutes}:${seconds} ${ampm}`;
                        const Address = 'Corner Glaze, Block#L, Road#8, Plot#2490/B, Bashundhara Residential Area, Dhaka-1229';

                        let TotalEmpCount = 0;

                        function drawHeader(doc) {
                            // Logo (if available)
                            if (base64Logo) {
                                doc.addImage(base64Logo, 'PNG', 15, 10, 80, 50);
                            }

                            // Company Name
                            doc.setFontSize(18);
                            doc.setFont("times", "bold");
                            doc.text(companyName, pageWidth / 2, 45, { align: 'center' });

                            doc.setFontSize(13);
                            doc.setFont("times", "normal");
                            doc.text("PF Assign Report", pageWidth / 2, 75, { align: 'center' });

                            const lineLength = pageWidth / 6.5;
                            const startX = (pageWidth - lineLength) / 2;
                            const endX = startX + lineLength;
                            doc.setDrawColor(0);
                            doc.setLineWidth(0.5);
                            doc.line(startX, 80, endX, 80);

                            doc.setFontSize(8);
                            doc.setFont("times", "normal");
                            const fromToText = Address;
                            doc.text(fromToText, pageWidth / 2, 58, { align: 'center' });
                        }

                        drawHeader(doc);

                        let startY = 95;

                        let departmentGroups = {};
                        employees.forEach(function (emp) {
                            let dept = emp.department || "Unknown";
                            if (!departmentGroups[dept]) {
                                departmentGroups[dept] = [];
                            }
                            departmentGroups[dept].push(emp);
                        });

                        for (const dept in departmentGroups) {
                            if (startY !== 95) startY += 20;

                            doc.setFontSize(12);
                            doc.setFont("times", "bold");
                            doc.text("Department: " + dept, 20, startY);
                            startY += 6;

                            let tempTable = $('<table>');
                            tempTable.append($('#employee-pfAssign-grid-report thead').clone());
                            let tbody = $('<tbody>');

                            departmentGroups[dept].forEach(function (emp, index) {
                                TotalEmpCount++;
                                let row = $('<tr>');
                                row.append('<td>' + (index + 1) + '</td>');
                                row.append('<td>' + emp.code + '</td>');
                                row.append('<td>' + emp.name + '</td>');
                                row.append('<td>' + emp.designation + '</td>');
                                row.append('<td>' + emp.branch + '</td>');
                                row.append('<td>' + emp.pfApprove + '</td>');
                                row.append('<td>' + emp.showDate + '</td>');
                                row.append('<td>' + emp.dayName + '</td>');
                                row.append('<td>' + emp.remarks + '</td>');
                                tbody.append(row);
                            });

                            tempTable.append(tbody);
                            doc.autoTable({
                                head: [['SN', 'Employee ID', 'Name', 'Designation', 'Branch', 'PF Approval', 'Effective Date', 'Day', 'Remarks']],
                                body: departmentGroups[dept].map((emp, index) => [
                                    index + 1,
                                    emp.code,
                                    emp.name,
                                    emp.designation,
                                    emp.branch,
                                    emp.pfApprove,
                                    emp.showDate,
                                    emp.dayName,
                                    emp.remarks
                                ]),
                                startY: startY,
                                theme: 'grid',
                                tableWidth: 'auto', // or 'wrap' depending on fit
                                margin: { top: 100, left: 15, right: 15 },
                                styles: {
                                    fontSize: 8,
                                    cellPadding: 2,
                                    cellWidth: 'wrap',
                                    lineColor: [0, 0, 0],
                                    lineWidth: 0.1,
                                    textColor: [0, 0, 0],
                                },
                                headStyles: {
                                    fillColor: [255, 255, 255],
                                    textColor: [0, 0, 0],
                                    fontStyle: 'bold',
                                    halign: 'center',
                                    valign: 'middle',
                                    lineWidth: 0.1,
                                    lineColor: [0, 0, 0],
                                },
                                columnStyles: {
                                    0: { cellWidth: 20, halign: 'center' },
                                    1: { cellWidth: 60, halign: 'center' },
                                    2: { cellWidth: 80 },
                                    3: { cellWidth: 70 },
                                    4: { cellWidth: 70, halign: 'center' },
                                    5: { cellWidth: 70, halign: 'center' },
                                    6: { cellWidth: 60, halign: 'center' },
                                    7: { cellWidth: 60, halign: 'center' },
                                    8: { cellWidth: 70 }
                                },
                                pageBreak: 'auto',
                                didDrawPage: function (data) {
                                    drawHeader(doc);
                                    const pageNumber = doc.internal.getCurrentPageInfo().pageNumber;
                                    const totalPages = '{total_pages_count_string}';
                                    const pageHeight = doc.internal.pageSize.getHeight();

                                    let leftText = 'Print Datetime: ' + currentDate + " | " + userName;
                                    let rightText = 'Page ' + pageNumber + ' of ' + totalPages;

                                    doc.setFontSize(10);
                                    doc.setTextColor(50, 50, 50);
                                    doc.setFont("times", "normal");

                                    doc.text(leftText, 15, pageHeight - 10);
                                    doc.text(rightText, pageWidth + 85, pageHeight - 10, { align: 'right' });
                                }
                            });

                            startY = doc.lastAutoTable.finalY;
                        }

                        if (typeof doc.putTotalPages === 'function') {
                            doc.putTotalPages('{total_pages_count_string}');
                        }

                        let finalY = doc.lastAutoTable.finalY || 270;
                        let pageHeight = doc.internal.pageSize.getHeight();

                        if (finalY + 20 > pageHeight - 20) {
                            doc.addPage();
                            drawHeader(doc);
                            finalY = 100;
                        }

                        doc.setFontSize(10);
                        doc.setTextColor(0, 0, 0);
                        doc.text('Total Employee : ' + TotalEmpCount, 15, finalY + 20);
                   
                    doc.save('Employee pfAssign Report.pdf');
                });
            });
        };

        $('#btnPreviewPdf').on('click', function () {
            PdfPreview();
        });



        function getImageBase64FromUrl(url, callback) {
            var img = new Image();
            img.crossOrigin = 'Anonymous';
            img.onload = function () {
                var canvas = document.createElement('canvas');
                canvas.width = img.width;
                canvas.height = img.height;
                var ctx = canvas.getContext('2d');
                ctx.drawImage(img, 0, 0);
                var dataURL = canvas.toDataURL('image/png');
                callback(dataURL);
            };
            img.onerror = function () {
                //console.error("Image not found or CORS issue.");
                callback(null);
            };
            img.src = url;
        }

        var PdfPreview = function () {
            LoadFullRosterTable(function (employees) {
                if (!employees || employees.length === 0) {
                    alert("No data found.");
                    return;
                }
                console.log(employees);
                getImageBase64FromUrl('/images/DP_logo.png', function (base64Logo) {
                    const { jsPDF } = window.jspdf;
                    const doc = new jsPDF({
                        orientation: 'portrait',
                        unit: 'pt',
                        format: 'a4'
                    });


                    const pageWidth = doc.internal.pageSize.getWidth();
                    const companyName = employees[0].company || "";
                    const fromDate = employees[0].fromDate || "";
                    const toDate = employees[0].toDate || "";
                    const userName = employees[0].luser || "";
                    const now = new Date();

                    const day = String(now.getDate()).padStart(2, '0');
                    const month = String(now.getMonth() + 1).padStart(2, '0');
                    const year = now.getFullYear();

                    let hours = now.getHours();
                    const minutes = String(now.getMinutes()).padStart(2, '0');
                    const seconds = String(now.getSeconds()).padStart(2, '0');
                    const ampm = hours >= 12 ? 'PM' : 'AM';
                    hours = hours % 12 || 12;
                    hours = String(hours).padStart(2, '0');
                    const currentDate = `${day}/${month}/${year} ${hours}:${minutes}:${seconds} ${ampm}`;
                    const Address = 'Corner Glaze, Block#L, Road#8, Plot#2490/B, Bashundhara Residential Area, Dhaka-1229';

                    let TotalEmpCount = 0;

                    function drawHeader(doc) {
                        // Logo (if available)
                        if (base64Logo) {
                            doc.addImage(base64Logo, 'PNG', 15, 10, 80, 50);
                        }

                        // Company Name
                        doc.setFontSize(18);
                        doc.setFont("times", "bold");
                        doc.text(companyName, pageWidth / 2, 45, { align: 'center' });

                        doc.setFontSize(13);
                        doc.setFont("times", "normal");
                        doc.text("PF Assign Report", pageWidth / 2, 75, { align: 'center' });

                        const lineLength = pageWidth / 6.5;
                        const startX = (pageWidth - lineLength) / 2;
                        const endX = startX + lineLength;
                        doc.setDrawColor(0);
                        doc.setLineWidth(0.5);
                        doc.line(startX, 80, endX, 80);

                        doc.setFontSize(8);
                        doc.setFont("times", "normal");
                        const fromToText = Address;
                        doc.text(fromToText, pageWidth / 2, 58, { align: 'center' });
                    }

                    drawHeader(doc);

                    let startY = 95;

                    let departmentGroups = {};
                    employees.forEach(function (emp) {
                        let dept = emp.department || "Unknown";
                        if (!departmentGroups[dept]) {
                            departmentGroups[dept] = [];
                        }
                        departmentGroups[dept].push(emp);
                    });

                    for (const dept in departmentGroups) {
                        if (startY !== 95) startY += 20;

                        doc.setFontSize(12);
                        doc.setFont("times", "bold");
                        doc.text("Department: " + dept, 20, startY);
                        startY += 6;

                        let tempTable = $('<table>');
                        tempTable.append($('#employee-pfAssign-grid-report thead').clone());
                        let tbody = $('<tbody>');

                        departmentGroups[dept].forEach(function (emp, index) {
                            TotalEmpCount++;
                            let row = $('<tr>');
                            row.append('<td>' + (index + 1) + '</td>');
                            row.append('<td>' + emp.code + '</td>');
                            row.append('<td>' + emp.name + '</td>');
                            row.append('<td>' + emp.designation + '</td>');
                            row.append('<td>' + emp.branch+ '</td>');
                            row.append('<td>' + emp.pfApprove + '</td>');
                            row.append('<td>' + emp.showDate + '</td>');
                            row.append('<td>' + emp.dayName + '</td>');
                            row.append('<td>' + emp.remarks + '</td>');
                            tbody.append(row);
                        });

                        tempTable.append(tbody);
                        doc.autoTable({
                            head: [['SN', 'Employee ID', 'Name', 'Designation', 'Branch', 'PF Approval', 'Effective Date', 'Day', 'Remarks']],
                            body: departmentGroups[dept].map((emp, index) => [
                                index + 1,
                                emp.code,
                                emp.name,
                                emp.designation,
                                emp.branch,
                                emp.pfApprove,
                                emp.showDate,
                                emp.dayName,
                                emp.remarks
                            ]),
                            startY: startY,
                            theme: 'grid',
                            tableWidth: 'auto', // or 'wrap' depending on fit
                            margin: { top: 100, left: 15, right: 15 },
                            styles: {
                                fontSize: 8,
                                cellPadding: 2,
                                cellWidth: 'wrap',
                                lineColor: [0, 0, 0],
                                lineWidth: 0.1,
                                textColor: [0, 0, 0],
                            },
                            headStyles: {
                                fillColor: [255, 255, 255],
                                textColor: [0, 0, 0],
                                fontStyle: 'bold',
                                halign: 'center',
                                valign: 'middle',
                                lineWidth: 0.1,
                                lineColor: [0, 0, 0],
                            },
                            columnStyles: {
                                0: { cellWidth: 20, halign: 'center' },    
                                1: { cellWidth: 60, halign: 'center' },   
                                2: { cellWidth: 80 },                      
                                3: { cellWidth: 70 },                      
                                4: { cellWidth: 70, halign: 'center' },   
                                5: { cellWidth: 70, halign: 'center' },  
                                6: { cellWidth: 60, halign: 'center' },   
                                7: { cellWidth: 60, halign: 'center' },    
                                8: { cellWidth: 70 }                      
                            },
                            pageBreak: 'auto',
                            didDrawPage: function (data) {
                                drawHeader(doc);
                                const pageNumber = doc.internal.getCurrentPageInfo().pageNumber;
                                const totalPages = '{total_pages_count_string}';
                                const pageHeight = doc.internal.pageSize.getHeight();

                                let leftText = 'Print Datetime: ' + currentDate + " | " + userName;
                                let rightText = 'Page ' + pageNumber + ' of ' + totalPages;

                                doc.setFontSize(10);
                                doc.setTextColor(50, 50, 50);
                                doc.setFont("times", "normal");

                                doc.text(leftText, 15, pageHeight - 10);
                                doc.text(rightText, pageWidth + 85, pageHeight - 10, { align: 'right' });
                            }
                        });

                        startY = doc.lastAutoTable.finalY;
                    }

                    if (typeof doc.putTotalPages === 'function') {
                        doc.putTotalPages('{total_pages_count_string}');
                    }

                    let finalY = doc.lastAutoTable.finalY || 270;
                    let pageHeight = doc.internal.pageSize.getHeight();

                    if (finalY + 20 > pageHeight - 20) {
                        doc.addPage();
                        drawHeader(doc);
                        finalY = 100;
                    }

                    doc.setFontSize(10);
                    doc.setTextColor(0, 0, 0);
                    doc.text('Total Employee : ' + TotalEmpCount, 15, finalY + 20);

                    const blob = doc.output('blob');
                    const url = URL.createObjectURL(blob);

                    $('#pdf-preview-container').html(`<iframe src="${url}" width="100%" height="600px" style="border:1px solid #ccc;"></iframe>`);
                    $('#pdf-preview-container').show();
                });
            });
        };

        var downloadTableAsWord = function () {
            LoadFullRosterTable(function (employees) {
                if (!employees || employees.length === 0) {
                    alert("No data found!");
                    return;
                }

                let departmentGroups = {};
                employees.forEach(function (emp) {
                    let dept = emp.department || "Unknown";
                    if (!departmentGroups[dept]) {
                        departmentGroups[dept] = [];
                    }
                    departmentGroups[dept].push(emp);
                });

                let companyName = employees[0].company || "";
                let reportTitle = "PF Assign Report";
               
                let userName = employees[0].luser || "";
                let currentDate = new Date();
                let formattedDate = currentDate.toLocaleDateString() + " " + currentDate.toLocaleTimeString();

                let htmlContent = `
        <html xmlns:o='urn:schemas-microsoft-com:office:office' 
              xmlns:w='urn:schemas-microsoft-com:office:word' 
              xmlns='http://www.w3.org/TR/REC-html40'>
        <head><meta charset='utf-8'>
            <title>${reportTitle}</title>
            <style>
                @page {
                    size: A4 portrait;
                    margin: 0.5cm; 
                }
                body {
                    font-family: 'Times New Roman', serif;
                    font-size: 12pt;
                    margin: 0; 
                    padding: 0; 
                }
                h4, h5, h6 {
                    text-align: center;
                    margin: 2px 0; 
                }
                .dept-title {
                    margin-top: 10px;
                    font-size: 10pt;
                    font-weight: 400;
                }
                table {
                    width: 100%;
                    border-collapse: collapse;
                    margin-top: 5px;
                }
                th, td {
                    border: 1px solid #000;
                    text-align: center;
                    font-size: 10pt;
                    padding-left: 2px;
                    padding-right: 2px;
                    padding-bottom: 2px;
                    padding-top: 2px;
                    line-height: 1.2;
                }
                th {
                    background-color: #ffffff;
                    font-weight: bold;
                }
                .footer {
                    margin-top: 10px;
                    font-size: 10pt;
                }
            </style>
        </head>
        <body>
            <h4>${companyName}</h4>
            <h5>${reportTitle}</h5>
            <h6>Address: Corner Glaze, Block#L, Road#8, Plot#2490/B, Bashundhara Residential Area, Dhaka-1229</h6>
        `;

                let totalEmp = 0;

                for (const dept in departmentGroups) {
                    htmlContent += `<div class='dept-title'>Department: ${dept}</div>`;
                    htmlContent += `
                <table>
                    <thead>
                        <tr>
                            <th>SN</th>
                            <th>Employee ID</th>
                            <th>Name</th>
                            <th>Designation</th>
                            <th>Branch</th>
                            <th>PF Approval</th>
                            <th>Effective Date</th>
                            <th>Day</th>
                            <th>Remarks</th>
                        </tr>
                    </thead>
                    <tbody>
            `;

                    departmentGroups[dept].forEach(function (emp, index) {
                        totalEmp++;
                        htmlContent += `
                        <tr>
                            <td>${index + 1}</td>
                            <td>${emp.code}</td>
                            <td>${emp.name}</td>
                            <td>${emp.designation}</td>
                            <td>${emp.branchName}</td>
                            <td>${emp.pfApprove}</td>
                            <td>${emp.showDate}</td>
                            <td>${emp.dayName}</td>
                            <td style="text-align:left;">${emp.remarks}</td>
                        </tr>
                `;
                    });

                    htmlContent += `
                    </tbody>
                </table>
            `;
                }

                htmlContent += `
            <div class="footer">
                <p>Total Employees: ${totalEmp}</p>
                <p>Printed At: ${formattedDate} | ${userName}</p>
            </div>
        </body></html>
        `;

                let blob = new Blob(['\ufeff', htmlContent], {
                    type: 'application/msword'
                });

                let link = document.createElement('a');
                link.href = URL.createObjectURL(blob);
                link.download = `${reportTitle}.doc`;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            });
        };



        var downloadTableAsExcel = function () {
            LoadFullRosterTable(function (employees) {
                if (!employees || employees.length === 0) {
                    alert("No data found!");
                    return;
                }
                console.log(employees);
                $.ajax({
                    url: "/PFAssignEntryReport/DownloadExcel",
                    type: "POST",
                    contentType: "application/json",
                    data: JSON.stringify(employees),
                    xhrFields: { responseType: "blob" },
                    success: function (res) {
                        var link = document.createElement("a");
                        link.href = URL.createObjectURL(res);
                        link.download = "Employee pfAssign Report.xlsx";
                        document.body.appendChild(link);
                        link.click();
                        document.body.removeChild(link);
                    },
                    error: function (e) {
                        //console.log(e.message);
                        //showToast("error", "Excel download failed!");
                    }
                });
            });
        }


        var init = function () {
            showLoading();
            //GetFlatDate();
            settings.load();
            initializeMultiselects();
            setupLoadingOverlay();
            loadFilterEmp();
        };
        init();
    };
})(jQuery);
