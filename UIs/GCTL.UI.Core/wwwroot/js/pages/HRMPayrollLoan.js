(function ($) {
    $.patientTypes = function (options) {
        var settings = $.extend({
            baseUrl: "/",
            companyIds: "#companySelect",
            EmployeeIds: "#employeeSelect",
            LoanName:"#LoanName",
            LoanDesignation:"#LoanDesignation",
            LoanDepartment:"#LoanDepartment",
            LoanJoindate: "#LoanJoinDate",
            PaymentType:"#paymentLoanSelect",
            LoanDate:"#loanDate",
            EndDate:"#endDate",
            InstStartDate:"#instStartDate",
            LoanAmount: "#loanAmount",
            PaymentMode: "#paymentMode",
            AmountWorningMessage: "#amountWorningMessage",
            DeductionHead: "#DeductionHead",
            LoanIdAutoGenerate: "#LoanIdAutoGenerate",
            LoanEntryBtn: ".js-loan-entry-report-save",
            NoOfInstallment: "#noOfInstallment",
            MonthlyDeduction: "#monthlyDeduction",
            Remarks: "#remarks",
            ChequeDate: "#chequeDate",
            BankAccount: "#bankAccount",
            Bank: "#Bank",
            ChequeNo: "#chequeNo",
            IfCheque: "#if_cheque",
            IfWairTransferThenHide: "#if_wair-transfer-then-hide",
            ClearAll: "#js-loan-entry-report-clear",
        }, options);
        var filterUrl = settings.baseUrl + "/GetFilterData";
        var employeeByIdUrl = settings.baseUrl + "/GetEmpById";
        var paymentTypeUrl = settings.baseUrl + "/getLoanType";
        var paymentModeUrl = settings.baseUrl + "/GetPaymentMode";
        var payHeadDeductionUrl = settings.baseUrl + "/getPayHeadDeduction";
        var createLoanId = settings.baseUrl + "/CreateLoanId";
        var bankUrl = settings.baseUrl + "/GetBank";
        var createLoanUrl = settings.baseUrl + "/CreateEditLoan";
        var getLoanDataUrl = settings.baseUrl + "/GetLoanData";
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

        var initializeMultiselects = function () {
            var selectors = [
                settings.companyIds,
                settings.EmployeeIds,
                settings.PaymentType,
                settings.PaymentMode,
                settings.DeductionHead,
                settings.Bank
            ].join(", ");
            $(selectors).multiselect({
                enableFiltering: true,
                includeSelectAllOption: true,
                selectAllText: 'Select All',
                nonSelectedText: '--select items--',
                nSelectedText: 'Selected',
                allSelectedText: 'All Selected',
                filterPlaceholder: 'Search.........',
                buttonWidth: '100%',
                maxHeight: 350,
                enableClickableOptGroups: true,
                dropUp: false,
                numberDisplayed: 1,
                enableCaseInsensitiveFiltering: true,               
            });
        };
        var toArray = function (value) {
            if (!value) return [];
            if (Array.isArray(value)) return value;
            return [value];
        };
        var getFilterValue = function () {
            var filterData = {
                CompanyCodes: toArray($(settings.companyIds).val()),
                EmployeeIds: toArray($(settings.EmployeeIds).val())
            };
            return filterData;
        }
        function formatDate(inputDate) {
            if (!inputDate) return null;
            const parts = inputDate.split('/');
            return `${parts[2]}-${parts[1]}-${parts[0]}`; // yyyy-MM-dd
        }
        function submitLoanData() {
            var data = {
                LoanId: $(settings.LoanIdAutoGenerate).val() || '',
                EmployeeId: $(settings.EmployeeIds).val() || '',                
                LoanTypeId: $(settings.PaymentType).val() || '', 
                LoanAmount: $(settings.LoanAmount).val() || 0,
                NoOfInstallment: $(settings.NoOfInstallment).val() || '',
                MonthlyDeduction: $(settings.MonthlyDeduction).val() || 0,
                PayHeadNameId: $(settings.DeductionHead).val() || '',
                PaymentModeId: $(settings.PaymentMode).val() || '',
                ChequeNo: $(settings.ChequeNo).val() || '',              
                BankId: $(settings.Bank).val() || '',
                BankAccount: $(settings.BankAccount).val() || '',
                Remarks: $(settings.Remarks).val() || '',
                CompanyCode: $(settings.companyIds).val() || '',
                LoanDate: formatDate($(settings.LoanDate).val())||null,
                StartDate: formatDate($(settings.InstStartDate).val()) || null,
                EndDate: formatDate($(settings.EndDate).val()) || null,
                ChequeDate: formatDate($(settings.ChequeDate).val()) || null,
            };
            return data;
        }

        var clearLoanForm=function () {
            $(settings.EmployeeIds).val('0');
            $(settings.LoanAmount).val('');
            $(settings.NoOfInstallment).val('');
            $(settings.MonthlyDeduction).val('');
            $(settings.DeductionHead).val('0');
            $(settings.PaymentMode).val('0');
            $(settings.ChequeNo).val('');
            $(settings.Bank).val('0');
            $(settings.BankAccount).val('');
            $(settings.Remarks).val('');
            $(settings.companyIds).val('0');
            $(settings.LoanDate).val(getTodayDate());
            $(settings.InstStartDate).val(getTodayDate());
            $(settings.EndDate).val(getTodayDate());
            $(settings.ChequeDate).val(getTodayDate());
            $(settings.PaymentType).val('');
            //loanDatePicker;
            //instStartDatePicker;
            //endDatePicker;
            //chequeDatePicker;
        }
        function getTodayDate() {
            var d = new Date();
            var day = ('0' + d.getDate()).slice(-2);
            var month = ('0' + (d.getMonth() + 1)).slice(-2);
            var year = d.getFullYear();
            return day + '/' + month + '/' + year;
        }
        $(document).on('click', settings.ClearAll, function () {
            clearLoanForm();
        })


        var loadFilterData = function () {
            var filterData = getFilterValue();
            $.ajax({
                url: filterUrl,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(filterData),
                success: function (res) {
                    $(settings.companyIds, settings.EmployeeIds).off('change');
                    const data = res.data;
                    //console.log(data);
                    if (data.company && data.company.length > 0 && data.company.some(x => x.code != null && x.name != null)) {
                        var companies = data.company;
                        var optCompany = $(settings.companyIds);
                        $.each(companies, function (index, company) {
                            if (company.code != null && company.name != null && company.name != "" && optCompany.find(`option[value="${company.code}"]`).length === 0) {
                                optCompany.append(`<option value="${company.code}">${company.name}</option>`)
                            }
                        });
                        optCompany.multiselect('rebuild');
                    }
                    if (data.employees && data.employees.length > 0 && data.employees.some(x => x.code != null && x.name != null)) {
                        var Employees = data.employees;
                        var optEmployee = $(settings.EmployeeIds);
                        $.each(Employees, function (index, employee) {
                            //console.log(employee);
                            if (employee.code != null && employee.name != null && employee.name != "" && optEmployee.find(`option[value="${employee.code}"]`).length === 0) {
                                optEmployee.append(`<option value="${employee.code}">${employee.name}</option>`)
                            }
                        });
                        optEmployee.multiselect('rebuild');
                    }
                },
                error: function (e) {
                    console.log(e);
                }
            });
        }
        $(document).on('change', settings.EmployeeIds, function () {
            var empId = $(this).val();
            $.ajax({
                //url: employeeByIdUrl,
                url: employeeByIdUrl + "?empId=" + encodeURIComponent(empId),
                type: "GET",              
                success: function (res) {                    
                    $(settings.LoanName).text(res.data.name);
                    $(settings.LoanDesignation).text(res.data.designationName);
                    $(settings.LoanDepartment).text(res.data.departmentName);
                    $(settings.LoanJoindate).text(res.data.joinDate);
                },
                error: function (e) {
                    console.log(e);
                }
            });
        });
        paymentType = function () {
            $.ajax({
                url: paymentTypeUrl,
                type: "GET",
                success: function (res) {
                    var paymentTypes = res.data;
                    //console.log(paymentTypes);                  
                    var paymentTypeOpt = $(settings.PaymentType);
                    if (paymentTypes && paymentTypes.length > 0 && paymentTypes.some(x => x.loanType != null && x.loanTypeId != null)) {
                        
                        $.each(paymentTypes, function (index, paymentType) {
                            //console.log(paymentType);
                            if (paymentType.loanTypeId != null && paymentType.loanType != null && paymentTypeOpt.find(`option[value="${paymentType.loanTypeId}"]`).length === 0) {
                                paymentTypeOpt.append(`<option value="${paymentType.loanTypeId}">${paymentType.loanType}</option>`)
                            }
                        });
                        paymentTypeOpt.multiselect('rebuild');
                    }
                }, error: function (e) {
                    //console.log(e);
                }
            });
        }
        paymentMode = function () {
            $.ajax({
                url: paymentModeUrl,
                type: "GET",
                success: function (res) {
                    var optPaymentMode = $(settings.PaymentMode);
                    var PaymentModeData = res.data;
                    
                    if (PaymentModeData && PaymentModeData.length > 0 && PaymentModeData.some(x => x.paymentModeId != null && x.paymentModeName != null)) {
                        $.each(PaymentModeData, function (index, paymentMode) {                        
                            if (paymentMode.paymentModeId != null && paymentMode.paymentModeName != null && optPaymentMode.find(`option[value="${paymentMode.paymentModeId}"]`).length === 0) {
                                optPaymentMode.append(`<option value="${paymentMode.paymentModeId}">${paymentMode.paymentModeName}</option>`)
                            }
                        });
                        optPaymentMode.multiselect('rebuild')
                    }
                }, error: function (e) {
                    console.log(e);
                }
            });
        }
        payHeadDeduction = function () {
            $.ajax({
                url: payHeadDeductionUrl,
                type: "GET",
                success: function (res) {
                    //console.log(res);
                    var deductions = res.data;
                    var optDeductionHead = $(settings.DeductionHead);
                    if (deductions && deductions.length > 0 && deductions.some(x => x.payHeadNameId != null && x.name != null)) {
                        $.each(deductions, function (index, deduction) {
                            if (deduction.payHeadNameId != null & deduction.name != null && optDeductionHead.find(`option[value="${deduction.payHeadNameId}"]`).length === 0) {
                                optDeductionHead.append(`<option value="${deduction.payHeadNameId}">${deduction.name}</option>`);
                            }
                        });
                        optDeductionHead.multiselect('rebuild');
                    }
                }, error: function (e) {
                    console.log(e);
                }
            });
        }
        bankSection = function () {
            $.ajax({
                url: bankUrl,
                type: "GET",
                success: function (res) {
                    //console.log(res);
                    var banks = res.data;
                    var optBank = $(settings.Bank);
                    //console.log(optBank);
                    if (banks && banks.length > 0 && banks.some(x => x.bankId != null && x.bankName != null)) {
                        $.each(banks, function (index, bank) {
                            if (bank.bankId != null && bank.bankName != null && optBank.find(`option[value="${bank.bankId}"]`).length === 0) {
                                optBank.append(`<option value="${bank.bankId}">${bank.bankName}</option>`);
                            }
                        });
                        optBank.multiselect('rebuild');
                    }
                }, error(e) {
                    console.log(e);
                }
            });
        }

        createLoanIdAutoGenareted = function() {
            $.ajax({
                url: createLoanId,
                type: "GET",
                success: function (res) {
                    //console.log(res.loanId);
                    $(settings.LoanIdAutoGenerate).val(res.loanId);
                }, error: function (e) {
                    console.log(e);
                }
            });
        };
        
        $(document).on('change', settings.PaymentMode, function () {
            var selectedPaymentType = $(this).val();
            if (selectedPaymentType === '002') {
                //console.log("Payment Type Selected (via settings 002):", selectedPaymentType);
                $(settings.IfCheque).fadeIn(500);
                $(settings.IfWairTransferThenHide).fadeIn(500);
            } else if (selectedPaymentType === '003') {
                $(settings.IfWairTransferThenHide).fadeOut(500);
                $(settings.IfCheque).fadeIn(500);
            } else {
                $(settings.IfCheque).fadeOut(500);
            }
        });
        $(document).on('change', settings.EndDate, function () {
            var startDateStr = $(settings.InstStartDate).val(); // dd/mm/yyyy
            var endDateStr = $(settings.EndDate).val();         // dd/mm/yyyy

            if (!startDateStr || !endDateStr) {
                console.log("Start date and End date input both");
                return;
            }

            var startParts = startDateStr.split('/');
            var endParts = endDateStr.split('/');

            var startYear = parseInt(startParts[2], 10);
            var startMonth = parseInt(startParts[1], 10);
            var startDate = new Date(startYear, startMonth - 1, 1); 

            var endYear = parseInt(endParts[2], 10);
            var endMonth = parseInt(endParts[1], 10);
            var endDay = parseInt(endParts[0], 10);
            var endDate = new Date(endYear, endMonth - 1, endDay);

            var yearDiff = endDate.getFullYear() - startDate.getFullYear();
            var monthDiff = endDate.getMonth() - startDate.getMonth();

            var totalMonths = yearDiff * 12 + monthDiff;   
            if (totalMonths < 0) {
                //console.log("End date cannot be before the start date.");
                return;
            }

            //console.log("Total Month:", totalMonths);
            if (totalMonths === 0) {
                $(settings.MonthlyDeduction).val($(settings.LoanAmount).val());
                $(settings.NoOfInstallment).val(totalMonths);
            } else {
                $(settings.NoOfInstallment).val(totalMonths);
                var LoanAmount = $(settings.LoanAmount).val();
                var MonthlyDeductionAmount = LoanAmount / totalMonths;
                $(settings.MonthlyDeduction).val(Math.ceil(MonthlyDeductionAmount));
            }
           
        });

        $(document).on('input', settings.InstStartDate, function () {
            var startDateStr = $(settings.InstStartDate).val(); // dd/mm/yyyy
            var endDateStr = $(settings.EndDate).val();         // dd/mm/yyyy

            if (!startDateStr || !endDateStr) {
                console.log("Start date and End date input both");
                return;
            }

            var startParts = startDateStr.split('/');
            var endParts = endDateStr.split('/');

            var startYear = parseInt(startParts[2], 10);
            var startMonth = parseInt(startParts[1], 10);
            var startDate = new Date(startYear, startMonth - 1, 1);

            var endYear = parseInt(endParts[2], 10);
            var endMonth = parseInt(endParts[1], 10);
            var endDay = parseInt(endParts[0], 10);
            var endDate = new Date(endYear, endMonth - 1, endDay);

            var yearDiff = endDate.getFullYear() - startDate.getFullYear();
            var monthDiff = endDate.getMonth() - startDate.getMonth();

            var totalMonths = yearDiff * 12 + monthDiff;



            if (totalMonths < 0) {
                //console.log("End date cannot be before the start date.");
                return;
            }

            //console.log("Total Month:", totalMonths);
            if (totalMonths === 0) {
                $(settings.MonthlyDeduction).val($(settings.LoanAmount).val());
                $(settings.NoOfInstallment).val(totalMonths);
            } else {
                $(settings.NoOfInstallment).val(totalMonths);
                var LoanAmount = $(settings.LoanAmount).val();
                var MonthlyDeductionAmount = LoanAmount / totalMonths;
                $(settings.MonthlyDeduction).val(Math.ceil(MonthlyDeductionAmount));
            }   
        })


        $(document).on('input', settings.LoanAmount, function () {
            var totalMonth = $(settings.NoOfInstallment).val();
            if (totalMonth == 0) {
                $(settings.MonthlyDeduction).val($(settings.LoanAmount).val());
                $(settings.NoOfInstallment).val(totalMonth);
            } else {
                var LoanAmount = $(settings.LoanAmount).val();
                var MonthlyDeductionAmount = LoanAmount / totalMonth;
                $(settings.MonthlyDeduction).val(Math.ceil(MonthlyDeductionAmount));
            }
        })

        var today = new Date();
        var loanDatePicker = flatpickr("#loanDate", {
            dateFormat: "d/m/Y",
            defaultDate: today,
            minDate: today,
            allowInput: true,
            onChange: function (selectedDates, dateStr, instance) {
                console.log("Loan Date:", dateStr);
            },
            onClose: function (selectedDates, dateStr, instance) {
                if (!dateStr) {
                    instance.setDate(today);
                }
            }
        });

        

        var endDatePicker = flatpickr("#endDate", {
            dateFormat: "d/m/Y",
            defaultDate: today,    
            minDate: today,    
            allowInput: true,
            onClose: function (selectedDates, dateStr, instance) {
                if (!dateStr) {
                    instance.setDate(today);
                }
            }
        });

        // Start Date Picker
        var instStartDatePicker = flatpickr("#instStartDate", {
            dateFormat: "d/m/Y",
            defaultDate: today,
            minDate: today,
            allowInput:true,
            onChange: function (selectedDates, dateStr, instance) {
                console.log("Start Date:", dateStr);

                if (selectedDates.length > 0) {                  
                    endDatePicker.set('minDate', selectedDates[0]);
                    endDatePicker.setDate(selectedDates[0]); 
                }
            },
            onClose: function (selectedDates, dateStr, instance) {              
                if (!dateStr) {
                    instance.setDate(today);
                }
            }
        });
        var chequeDatePicker = flatpickr($(settings.ChequeDate), {
            dateFormat: "d/m/Y",
            defaultDate: new Date()
        })
        $(document).on('input', settings.LoanAmount, function () {           
            var val = $(this).val().trim();
            var amount = parseFloat(val);

            if (val === "") {
                $(settings.LoanAmount).css('border', '');
                $(settings.AmountWorningMessage).stop(true, true).fadeOut(500);
            } else if (isNaN(amount) || amount < 0) {
                $(settings.LoanAmount).css('border', '1px solid red');
                $(settings.AmountWorningMessage).stop(true, true).fadeIn(500);
            } else {
                $(settings.LoanAmount).css('border', '');
                $(settings.AmountWorningMessage).stop(true, true).fadeOut(500);
            }
        });


        //create 
        $(document).on('click', settings.LoanEntryBtn, function () {
            var filterData = submitLoanData()
            $.ajax({
                url: createLoanUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(filterData),
                success: function (res) {
                    console.log(res);
                    createLoanIdAutoGenareted();
                    displayLoanDataTable();
                },
                error: function (e) {
                    console.log(e);
                }
            });
        })

        //get loan data
        //var loadLoanData = function () {
        //    $.ajax({
        //        url: getLoanDataUrl,
        //        type: "GET",
        //        success: function (res) {
        //            console.log(res);
        //        }, error: function (e) {
        //            console.log(e);
        //        }
        //    });
        //}


        function displayLoanDataTable() {
            if ($.fn.DataTable.isDataTable("#loan-data-grid")) {
                $("#loan-data-grid").DataTable().clear().destroy();
            }

            $('#loan-data-grid').DataTable({
                processing: true,
                serverSide: false,
                ajax: {
                    url: getLoanDataUrl, 
                    type: 'GET',
                    dataSrc: 'data'
                },
                columns: [
                    {
                        data: null,
                        orderable: false,
                        className: 'text-center',
                        render: function (data, type, row) {
                            return `<input class="payrollLoan" type="checkbox" data-id="${row.loanId}" />`;
                        }
                    },
                    { data: 'loanId', className: 'text-center' },
                    { data: 'showLoanDate', className: 'text-center' },
                    { data: 'loanTypeName', className: 'text-left' , width:'100px'},
                    { data: 'employeeId', className: 'text-center' },
                    { data: 'empName', className: 'text-left', width: '150px' },
                    { data: 'designationName', className: 'text-left', width:'140px' },
                    { data: 'loanAmount', className: 'text-center' },
                    { data: 'startShowDate', className: 'text-center' },
                    { data: 'endShowDate', className: 'text-center' },
                    { data: 'noOfInstallment', className: 'text-center', width:'100px' },
                    { data: 'monthlyDeduction', className: 'text-center' },
                    { data: 'paymentModeName', className: 'text-left' }
                ],
                responsive: true,
                paging: true,
                scrollX: true,
                searching: true,
                language: {
                    search: "🔍 Search:",
                    lengthMenu: "Show _MENU_ entries",
                    info: "Showing _START_ to _END_ of _TOTAL_ entries",
                    paginate: {
                        first: "First",
                        previous: "Prev",
                        next: "Next",
                        last: "Last"
                    },
                    emptyTable: "No data available"
                }
            });
        }



        var init = function () {
            loadFilterData();
            initializeMultiselects();
            paymentType();
            paymentMode();
            payHeadDeduction();
            createLoanIdAutoGenareted();
            bankSection();
            displayLoanDataTable();
            //loadLoanData();
            loanDatePicker;
            instStartDatePicker;
            endDatePicker;
            chequeDatePicker;
        };
        init();
    };
})(jQuery);