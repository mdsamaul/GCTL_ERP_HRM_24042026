(function ($) {
    $.patientTypes = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
        }, options);

        var filterUrl = commonName.baseUrl + "/GetLoanDetails";
        var GetLoanEmployeesUrl = commonName.baseUrl + "/GetLoanEmployees";

        $("#btnDownloadPDF").click(function () {
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
            getLoanEmployee();
        }
        init();
    }
})(jQuery);
