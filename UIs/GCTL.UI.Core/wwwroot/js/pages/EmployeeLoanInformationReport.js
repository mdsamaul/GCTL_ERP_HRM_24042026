(function ($) {
    $.patientTypes = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
        }, options);

        var filterUrl = commonName.baseUrl + "/GetLoanDetails";
        var GetLoanEmployeesUrl = commonName.baseUrl + "/GetLoanEmployees";

        

        //loadEmployeeLoanReport = function (companyId, employeeId, loanId, dateFrom, dateTo) {
        //    //var companyId = $("#companyId").val()?.trim();
        //    //var employeeId = $("#employeeId").val()?.trim();
        //    //var loanId = $("#loanId").val()?.trim();
        //    //var dateFrom = $("#dateFrom").val();
        //    //var dateTo = $("#dateTo").val();


        //    $.ajax({
        //        url: filterUrl,
        //        type: "GET",
        //        data: {
        //            CompanyID: companyId,
        //            EmployeeID: employeeId,
        //            LoanID: loanId,
        //            DateFrom: dateFrom,
        //            DateTo: dateTo
        //        },
        //        success: function (data) {
        //            console.log(data);
        //            DropdownAppendCompany(data.companies);
        //            DropdownAppendLoanType(data.loanIDs);
        //            DropdownAppendEmployee(data.employees);
        //        },
        //        error: function () {
        //            alert("Error fetching data");
        //        }
        //    });
        //}

        loadEmployeeLoanReport = function (companyId = null, employeeId = null, loanId = null, dateFrom = null, dateTo = null) {
            $.ajax({
                url: filterUrl,
                type: "GET",
                data: {
                    CompanyID: companyId,
                    EmployeeID: employeeId,
                    LoanID: loanId,
                    DateFrom: dateFrom,
                    DateTo: dateTo
                },
                success: function (data) {
                    console.log("AJAX data:", data);
                    DropdownAppendCompany(data.companies);
                    DropdownAppendLoanType(data.loanIDs);
                    DropdownAppendEmployee(data.employees);
                },
                error: function () {
                    alert("Error fetching data");
                }
            });
        }

        DropdownAppendCompany = function (dropdownDataCompany) {
            console.log(dropdownDataCompany);
            window.dropdownDataCompany = dropdownDataCompany;

            if (!dropdownDataCompany || !Array.isArray(dropdownDataCompany)) {
                console.log("Invalid company data");
                return;
            }

            const $container = $('[data-field="companyId"]');
            const $optionContainer = $container.find(".multiselect-options");
            $optionContainer.empty();

            dropdownDataCompany.forEach((company, index) => {
                console.log(company);

                const $option = $('<div class="multiselect-option"></div>')
                    .text(company.companyName)
                    .attr("data-value", company.companyCode);

                $option.on("click", function () {
                    $container.find(".selected-items").text(company.companyName);
                    $container.find(".placeholder-text").hide();
                    $container.find(".multiselect-dropdown").hide();
                    $container.find(".multiselect-arrow").removeClass("rotate");
                    $container.find("#companyId").val(company.companyCode);

                    // Call load report on selection with company code
                    loadEmployeeLoanReport(company.companyCode, null, null, null, null);

                    console.log("Company selected:", company.companyCode);
                });

                $optionContainer.append($option);

                if (index === 0) {
                    setCompanySelection($container, company.companyName, company.companyCode);
                }
            });
        };

        function setCompanySelection($container, name, code) {
            $container.find(".selected-items")
                .text(name)
                .attr("data-id", code); 
            $container.find(".selected-items").text(name);
            $container.find(".placeholder-text").hide();
            $container.find(".multiselect-dropdown").hide();
            $container.find(".multiselect-arrow").removeClass("rotate");
            $container.find("#companyId").val(code);
            console.log("Company code selected:", code);
            setTextClear();
            const $containerLoan = $('[data-field="loanType"]'); // loanType dropdown container
            $containerLoan.find(".multiselect-options").empty();  // option গুলো মুছে ফেলো
            $containerLoan.find(".selected-items").text('');      // selected value খালি করো
            $containerLoan.find(".placeholder-text").show();      // placeholder আবার দেখাও

            // Optional: loan type এর hidden input খালি করতে চাইলে
            $containerLoan.find("#loanTypeId").val(''); // যদি থাকে hidden input
            // Call load report on initial set
            //loadEmployeeLoanReport(code, null, null, null, null);
            setTextClear();
            // employee dropdown খালি করা
            const $containerEmployee = $('[data-field="employeeId"]');
            $containerEmployee.find(".multiselect-options").empty();
            $containerEmployee.find(".selected-items").text('');
            $containerEmployee.find(".placeholder-text").show();
            $containerEmployee.find("#employeeId").val(''); // hidden input থাকলে খালি করো
        }

        // Search company
        $('[data-field="companyId"] .multiselect-search input').on("input", function () {
            const searchTerm = $(this).val().toLowerCase().trim();
            const $container = $(this).closest('[data-field="companyId"]');
            const $optionContainer = $container.find(".multiselect-options");

            const allCompanies = window.dropdownDataCompany || [];

            $optionContainer.empty();

            const filtered = allCompanies.filter(company =>
                company.companyName.toLowerCase().includes(searchTerm)
            );

            if (filtered.length === 0) {
                $optionContainer.append('<div class="multiselect-option disabled">No data available</div>');
            } else {
                filtered.forEach(company => {
                    const $option = $('<div class="multiselect-option"></div>')
                        .text(company.companyName)
                        .attr("data-value", company.companyCode);

                    $option.on("click", function () {
                        $container.find(".selected-items").text(company.companyName);
                        $container.find(".placeholder-text").hide();
                        $container.find(".multiselect-dropdown").hide();
                        $container.find(".multiselect-arrow").removeClass("rotate");
                        $container.find("#companyId").val(company.companyCode);

                        //loadEmployeeLoanReport(company.companyCode, null, null, null, null);

                        console.log("Company selected from search:", company.companyCode);
                    });

                    $optionContainer.append($option);
                });
            }
        });

        // Dropdown toggle
        $('[data-field="companyId"] .multiselect-input').on("click", function () {
            const $container = $(this).closest('[data-field="companyId"]');
            const $dropdown = $container.find(".multiselect-dropdown");
            const $arrow = $container.find(".multiselect-arrow");

            $(".multiselect-dropdown").not($dropdown).hide();
            $(".multiselect-arrow").not($arrow).removeClass("rotate");

            $dropdown.toggle();
            $arrow.toggleClass("rotate", $dropdown.is(":visible"));
        });

        // Close on outside click
        $(document).on("click", function (e) {
            if (!$(e.target).closest('[data-field="companyId"]').length) {
                $('[data-field="companyId"] .multiselect-dropdown').hide();
                $('[data-field="companyId"] .multiselect-arrow').removeClass("rotate");
            }
        });

           

        function setLoanTypeSelection($container, value) {
            $container.find(".selected-items").text(value);
            $container.find(".placeholder-text").hide();
            $container.find(".multiselect-dropdown").hide();
            $container.find(".multiselect-arrow").removeClass("rotate");

            // Hidden field থাকলে সেটাও সেট করতে পারো
            // $container.find("#loanTypeId").val(value);
            console.log("loan id ",value);
            let companyId = $('[data-field="companyId"] .selected-items').attr("data-id");
            console.log("Selected Company ID from data-id:", companyId);
            setTextClear();
            const $containerEmployee = $('[data-field="employeeId"]');
            $containerEmployee.find(".multiselect-options").empty();
            $containerEmployee.find(".selected-items").text('');
            $containerEmployee.find(".placeholder-text").show();
            $containerEmployee.find("#employeeId").val(''); // hidden input থাকলে খালি করো
          
            loadEmployeeByLoanIdAndCompanyIdReport(companyId, null, value, null, null);
         

        }

        DropdownAppendLoanType = function (loanTypesArray) {
            console.log(loanTypesArray);
            window.dropdownLoanTypes = loanTypesArray;

            const $container = $('[data-field="loanType"]');
            const $optionContainer = $container.find(".multiselect-options");
            $optionContainer.empty();

            loanTypesArray.forEach((loanId, index) => {
                const $option = $('<div class="multiselect-option"></div>')
                    .text(loanId)
                    .attr("data-value", loanId);

                $option.on("click", function () {
                    setLoanTypeSelection($container, loanId);
                });

                $optionContainer.append($option);

                // ✅ Default select first one
                //if (index === 0) {
                //    setLoanTypeSelection($container, loanId);
                //}
            });
        };

        // Dropdown toggle
        $('[data-field="loanType"] .multiselect-input').on("click", function (e) {
            const $container = $(this).closest('[data-field="loanType"]');
            const $dropdown = $container.find(".multiselect-dropdown");
            const $arrow = $container.find(".multiselect-arrow");

            $(".multiselect-dropdown").not($dropdown).hide();
            $(".multiselect-arrow").not($arrow).removeClass("rotate");

            $dropdown.toggle();
            $arrow.toggleClass("rotate", $dropdown.is(":visible"));
        });

        // Outside click
        $(document).on("click", function (e) {
            if (!$(e.target).closest('[data-field="loanType"]').length) {
                $('[data-field="loanType"] .multiselect-dropdown').hide();
                $('[data-field="loanType"] .multiselect-arrow').removeClass("rotate");
            }
        });

        // Search functionality
        $('[data-field="loanType"] .multiselect-search input').on("input", function () {
            const searchTerm = $(this).val().toLowerCase().trim();
            const $container = $(this).closest('[data-field="loanType"]');
            const $optionContainer = $container.find(".multiselect-options");

            const allLoanTypes = window.dropdownLoanTypes || [];

            $optionContainer.empty();

            const filtered = allLoanTypes.filter(id =>
                id.toLowerCase().includes(searchTerm)
            );

            if (filtered.length === 0) {
                $optionContainer.append('<div class="multiselect-option disabled">No data available</div>');
            } else {
                filtered.forEach(id => {
                    const $option = $('<div class="multiselect-option"></div>')
                        .text(id)
                        .attr("data-value", id);

                    $option.on("click", function () {
                        setLoanTypeSelection($container, id);
                    });

                    $optionContainer.append($option);
                });
            }
        });



        loadEmployeeByLoanIdAndCompanyIdReport = function (companyId = null, employeeId = null, loanId = null, dateFrom = null, dateTo = null) {
            $.ajax({
                url: filterUrl,
                type: "GET",
                data: {
                    CompanyID: companyId,
                    EmployeeID: employeeId,
                    LoanID: loanId,
                    DateFrom: dateFrom,
                    DateTo: dateTo
                },
                success: function (data) {
                    console.log("AJAX data:", data);
                    DropdownAppendEmployee(data.employees);
                },
                error: function () {
                    alert("Error fetching data");
                }
            });
        }

        function setEmployeeSelection($container, employee) {
            const empText = `${employee.fullName} (${employee.employeeID})`; // ✅ display text
            const empId = employee.employeeID; // ✅ employee ID

            $container.find(".selected-items")
                .text(empText)
                .attr("data-value", empId); // ✅ set employee ID as data-value

            $container.find(".placeholder-text").hide();
            $container.find(".multiselect-dropdown").hide();
            $container.find(".multiselect-arrow").removeClass("rotate");

            let companyId = $('[data-field="companyId"] .selected-items').attr("data-id") || '';
            let loanId = $('[data-field="loanType"] .selected-items').text() || '';

            console.log("📦 Selected Company ID:", companyId);
            console.log("💸 Selected Loan ID:", loanId);
            console.log("👨‍💼 Selected Employee:", empId);

            // Call the report loader
            loadEmployeeDetailsByEmployeeIdLoanIdAndCompanyIdReport(companyId, empId, loanId, null, null);
        }


        DropdownAppendEmployee = function (employeeList) {
            console.log(employeeList);
            window.dropdownEmployees = employeeList;

            const $container = $('[data-field="employeeId"]');
            const $optionContainer = $container.find(".multiselect-options");
            $optionContainer.empty();

            employeeList.forEach((emp, index) => {
                const displayText = `${emp.fullName} (${emp.employeeID})`;

                const $option = $('<div class="multiselect-option"></div>')
                    .text(displayText)
                    .attr("data-value", emp.employeeID);

                $option.on("click", function () {
                    setEmployeeSelection($container, emp);
                });

                $optionContainer.append($option);

                // ✅ Default select first employee
                //if (index === 0) {
                //    setEmployeeSelection($container, emp);
                //}
            });
        };


        // Dropdown toggle
        $('[data-field="employeeId"] .multiselect-input').on("click", function (e) {
            const $container = $(this).closest('[data-field="employeeId"]');
            const $dropdown = $container.find(".multiselect-dropdown");
            const $arrow = $container.find(".multiselect-arrow");

            $(".multiselect-dropdown").not($dropdown).hide();
            $(".multiselect-arrow").not($arrow).removeClass("rotate");

            $dropdown.toggle();
            $arrow.toggleClass("rotate", $dropdown.is(":visible"));
        });

        // Click outside to close
        $(document).on("click", function (e) {
            if (!$(e.target).closest('[data-field="employeeId"]').length) {
                $('[data-field="employeeId"] .multiselect-dropdown').hide();
                $('[data-field="employeeId"] .multiselect-arrow').removeClass("rotate");
            }
        });

        // Search functionality
        $('[data-field="employeeId"] .multiselect-search input').on("input", function () {
            const searchTerm = $(this).val().toLowerCase().trim();
            const $container = $(this).closest('[data-field="employeeId"]');
            const $optionContainer = $container.find(".multiselect-options");

            const allEmployees = window.dropdownEmployees || [];

            $optionContainer.empty();

            const filtered = allEmployees.filter(emp =>
                emp.fullName.toLowerCase().includes(searchTerm) ||
                emp.employeeID.toLowerCase().includes(searchTerm)
            );

            if (filtered.length === 0) {
                $optionContainer.append('<div class="multiselect-option disabled">No data available</div>');
            } else {
                filtered.forEach(emp => {
                    const displayText = `${emp.fullName} (${emp.employeeID})`;

                    const $option = $('<div class="multiselect-option"></div>')
                        .text(displayText)
                        .attr("data-value", emp.employeeID);

                    $option.on("click", function () {
                        setEmployeeSelection($container, emp);
                    });

                    $optionContainer.append($option);
                });
            }
        });


        setTextClear = function () {
            $("#showEmployeeName").text('');
            $("#showDepartmentName").text('');
            $("#showDesignationName").text('');
        }

        loadEmployeeDetailsByEmployeeIdLoanIdAndCompanyIdReport = function (companyId = null, employeeId = null, loanId = null, dateFrom = null, dateTo = null) {
            $.ajax({
                url: filterUrl,
                type: "GET",
                data: {
                    CompanyID: companyId,
                    EmployeeID: employeeId,
                    LoanID: loanId,
                    DateFrom: dateFrom,
                    DateTo: dateTo
                },
                success: function (data) {
                    console.log("AJAX data:", data);
                    //DropdownAppendEmployee(data.employees);
                    var empData = data.employees;
                    console.log(empData);
                    $("#showEmployeeName").text(empData[0].fullName);
                    $("#showDepartmentName").text(empData[0].departmentName);
                    $("#showDesignationName").text(empData[0].designationName);
                },
                error: function () {
                    alert("Error fetching data");
                }
            });
        }



        flatpickr("#dateFrom", {
            altInput: true,
            altFormat: "d/m/Y",   // ইউজার যা দেখবে
            dateFormat: "Y-m-d",  // যা যাবে controller-এ
            allowInput: true
        });

        flatpickr("#toDate", {
            altInput: true,
            altFormat: "d/m/Y",
            dateFormat: "Y-m-d",
            allowInput: true
        });


        
        $(document).on('click', "#downloadReport", function () {
            let companyId = $('[data-field="companyId"] .selected-items').attr("data-id");
            const loanId = $('[data-field="loanType"] .selected-items').text();
            const employeeId = $('[data-field="employeeId"] .selected-items').attr("data-value"); // ✅ correct way

            console.log("📦 Company ID:", companyId);
            console.log("💸 Loan ID:", loanId);
            console.log("👨‍💼 Employee ID:", employeeId);

            let dateFrom = $("#dateFrom").val();
            let toDate = $("#toDate").val();
            console.log("📅 Date From:", dateFrom);
            console.log("📅 To Date:", toDate);
        });





















        $("#btnDownloadPDF1").click(function () {
            var empId = $("#employeeIdOption").val().trim();
            console.log(empId);
            $.ajax({
                url: filterUrl,
                type: 'GET',
                data: { employeeId: empId },
                success: function (data) {
                    if (!data || data.length === 0) {
                        alert("No data found");
                        $("#loanInfo").empty();
                        $("#btnDownloadPDF").hide();
                        return;
                    }

                    window.loanData = data; 

                    $("#loanInfo").empty();

                    data.forEach(function (loan, index) {
                        let totalDeposit = 0;
                        let installmentRows = "";

                        $.each(loan.installments, function (i, inst) {
                            totalDeposit += inst.deposit;

                            installmentRows += `
                                <tr>
                                    <td>${inst.installmentNo}</td>
                                    <td>${new Date(inst.installmentDate).toLocaleDateString()}</td>
                                    <td>${inst.paymentMode || ""}</td>
                                    <td>BDT ${inst.deposit.toFixed(2)}</td>
                                    <td>BDT ${inst.outstandingBalance.toFixed(2)}</td>
                                </tr>
                            `;
                        });

                        
                        let loanHtml = `
                            <div class="loan-block" style="border:1px solid #ddd; padding:15px; margin-bottom:15px;">
                                <h3 style="text-align:center;">${loan.companyName}</h3>
                                <p><strong>Employee ID:</strong> ${loan.employeeID}</p>
                                <p><strong>Name:</strong> ${loan.fullName}</p>
                                <p><strong>Department:</strong> ${loan.departmentName}</p>
                                <p><strong>Designation:</strong> ${loan.designationName}</p>
                                <p><strong>Reason:</strong> ${loan.reason || ""}</p>
                                <p><strong>Loan ID:</strong> ${loan.totalLoans}</p>
                                <p><strong>Loan Amount:</strong> BDT ${loan.loanAmount.toFixed(2)}</p>
                                <p><strong>Start Date:</strong> ${new Date(loan.startDate).toLocaleDateString()}</p>
                                <p><strong>End Date:</strong> ${new Date(loan.endDate).toLocaleDateString()}</p>
                                <p><strong>Installment Details:</strong> ${loan.installmentDetails || ""}</p>
                                <p><strong>Remarks:</strong> ${loan.remarks || ""}</p>
                                <h6>Installments</h6>
                                <table border="1" cellpadding="5" cellspacing="0" style="width: 100%; border-collapse: collapse;">
                                    <thead>
                                        <tr>
                                            <th>Installment No</th>
                                            <th>Date</th>
                                            <th>Payment Mode</th>
                                            <th>Deposit</th>
                                            <th>Outstanding Balance</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        ${installmentRows}
                                    </tbody>
                                </table>
                                <p><strong>Total Deposit:</strong> BDT ${totalDeposit.toFixed(2)}</p>
                            </div>
                        `;

                        $("#loanInfo").append(loanHtml);
                    });

                    $("#btnDownloadPDF").show();
                },
                error: function () {
                    alert("Error retrieving loan details");
                    $("#loanInfo").empty();
                    $("#btnDownloadPDF").hide();
                }
            });
        });




        getLoanEmployee = function () {
            $.ajax({
                url: GetLoanEmployeesUrl,
                type: 'GET',
                success: function (data) {
                   
                    if (data && data.length > 0) {
                        data.forEach(function (item) {
                            console.log(item);
                            $('#employeeIdOption').append(
                                $('<option>', {
                                    value: item.employeeID,
                                    text: item.fullName + ' (' + item.employeeID + ')'
                                })
                            );
                        });
                    }
                },
                error: function () {
                    alert('Failed to load employee list.');
                }
            });
        }


        // PDF Download button click handler
        $("#btnDownloadPDF").click(function () {
            const { jsPDF } = window.jspdf;
            const doc = new jsPDF('p', 'pt', 'a4');
            const pageWidth = doc.internal.pageSize.getWidth();
            const marginLeft = 40;
            let y = 20;

            if (!window.loanData || window.loanData.length === 0) {
                alert("No data available for PDF");
                return;
            }

            window.loanData.forEach(function (loan, index) {
                if (index > 0) {
                    doc.addPage();
                    y = 20;
                }

                console.log(loan);
                doc.setFontSize(14);
                doc.setFont(undefined, 'bold'); // Optional: bold
                let companyText = `${loan.companyName}`;
                let textWidth = doc.getTextWidth(companyText);
                doc.text(companyText, (pageWidth - textWidth) / 2, y);
                y += 25;


                doc.setFontSize(11);

                // Helper function to center text
                const centerX = pageWidth / 2;
                const labelWidth = 0;   // label এর জন্য ফিক্সড জায়গা
                const valueWidth = 100;   // value এর জন্য ফিক্সড জায়গা
                const colonX = centerX;   // ‘:’ সবসময় center-এ থাকবে
                const labelX = colonX - 5 - labelWidth;  // ‘:’ এর বাঁ দিকে label
                const valueX = colonX + 5;               // ‘:’ এর ডান দিকে value
                function drawKeyValue(label, value, y) {
                    doc.text(label, labelX, y, { align: "right" });
                    doc.text(":", colonX, y, { align: "center" });
                    doc.text(value, valueX, y, { align: "left" });
                }
                drawKeyValue("Employee ID", loan.employeeID.toString(), y); y += 18;
                drawKeyValue("Name", loan.fullName, y); y += 18;
                drawKeyValue("Department", loan.departmentName, y); y += 18;
                drawKeyValue("Designation", loan.designationName, y); y += 18;
                drawKeyValue("Reason of Loan taken", loan.reason || "", y); y += 18;
                drawKeyValue("Loan ID", loan.totalLoans.toString(), y); y += 18;
                drawKeyValue("Loan Amount", `BDT ${loan.loanAmount.toFixed(2)}`, y); y += 18;
                drawKeyValue("Loan Disbursed Date", new Date(loan.startDate).toLocaleDateString(), y); y += 18;
                drawKeyValue("Loan Repayment Method", loan.loanRepaymentMethod.toString(), y); y += 18;
                drawKeyValue("Installemnt Details", loan.installmentDetails.toString(), y); y += 18;
                drawKeyValue("Loan Paidout Date", new Date(loan.endDate).toLocaleDateString(), y); y += 18;
                drawKeyValue("Remarks", loan.remarks || "", y); y += 25;


                let totalDeposit = 0;

                // Prepare installments for autotable
                let installmentRows = loan.installments.map(inst => {
                    totalDeposit += inst.deposit || 0;
                    return [
                        inst.installmentNo,
                        inst.installmentDate || "",
                        inst.paymentMode || "",
                        inst.deposit ? `BDT ${inst.deposit.toFixed(2)}` : "",
                        `BDT ${inst.outstandingBalance.toFixed(2)}`
                    ];
                });

                doc.autoTable({
                    startY: y,
                    head: [['Installment No', 'Date', 'Payment Mode', 'Deposit', 'Outstanding Balance']],
                    body: installmentRows,
                    margin: { left: marginLeft, right: marginLeft },
                    styles: {
                        fontSize: 10,
                        lineColor: [0, 0, 0],
                        lineWidth: 0.2,
                        textColor: [0, 0, 0],
                        halign: 'center',
                        valign: 'middle'
                    },
                    headStyles: {
                        fillColor: [255, 255, 255],
                        textColor: [0, 0, 0],
                        lineColor: [0, 0, 0],
                        lineWidth: 0.2,
                        halign: 'center',
                        valign: 'middle'
                    },
                    theme: 'grid',
                    columnStyles: {
                        0: { cellWidth: 80 },   // Installment No
                        1: { cellWidth: 100 },   // Date
                        2: { cellWidth: 110 },  // Payment Mode
                        3: { cellWidth: 115, halign:'right' },  // Deposit
                        4: { cellWidth: 'auto', halign: 'right' }   // Outstanding Balance
                    },
                    didDrawPage: (data) => {
                        y = data.cursor.y + 10;
                    },
                    didDrawCell: function (data) {
                        // Optional: for custom styling per cell
                    }
                });

                // Show total deposit under the 'Deposit' column
                let depositColumnIndex = 3; // 0-based index: Deposit is 4th column
                let columnStartX = doc.autoTable.previous.finalX;
                let cellWidth = (pageWidth - marginLeft * 2) / 5.24; // 5 columns
                let depositColumnX = marginLeft + depositColumnIndex * cellWidth;

                doc.setFontSize(11);
                doc.setFont(undefined, 'bold');
                doc.text(
                    `Total Deposit: BDT ${totalDeposit.toFixed(2)}`,
                    depositColumnX,
                    y + 15,
                    { align: 'center' }
                );
                y += 25;

            });

            doc.save("Loan_Report.pdf");
        });

        init = function () {
            //getLoanEmployee();
            console.log("asdfasdf");
                loadEmployeeLoanReport();          
        }
        init();
    }
})(jQuery);
