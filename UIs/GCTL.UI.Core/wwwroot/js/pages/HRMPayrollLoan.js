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
            LoanIdAutoGenerate:"#LoanIdAutoGenerate",
        }, options);
        var filterUrl = settings.baseUrl + "/GetFilterData";
        var employeeByIdUrl = settings.baseUrl + "/GetEmpById";
        var paymentTypeUrl = settings.baseUrl + "/getLoanType";
        var paymentModeUrl = settings.baseUrl + "/GetPaymentMode";
        var payHeadDeductionUrl = settings.baseUrl + "/getPayHeadDeduction";
        var createLoanId = settings.baseUrl + "/CreateLoanId";
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
                settings.DeductionHead
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
                    console.log(res);
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

        createLoanIdAutoGenareted = function() {
            $.ajax({
                url: createLoanId,
                type: "GET",
                success: function (res) {
                    console.log(res.loanId);
                    $(settings.LoanIdAutoGenerate).val(res.loanId);
                }, error: function (e) {
                    console.log(e);
                }
            });
        };



        var loanDatePicker = flatpickr("#loanDate", {
            dateFormat: "d/m/Y",
            defaultDate: new Date(),
            onChange: function (selectedDates, dateStr, instance) {
                console.log("Loan Date:", dateStr); // formatted string
            }
        });

        var endDatePicker = flatpickr("#endDate", {
            dateFormat: "d/m/Y",
            defaultDate: new Date(),
            onChange: function (selectedDates, dateStr, instance) {
                console.log("End Date:", dateStr);
            }
        });

        var instStartDatePicker = flatpickr("#instStartDate", {
            dateFormat: "d/m/Y",
            defaultDate: new Date(),
            onChange: function (selectedDates, dateStr, instance) {
                console.log("Installment Start Date:", dateStr);
            }
        });
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

       

        var init = function () {
            loadFilterData();
            initializeMultiselects();
            paymentType();
            paymentMode();
            payHeadDeduction();
            createLoanIdAutoGenareted();

            loanDatePicker;
            instStartDatePicker;
            endDatePicker;
        };
        init();
    };
})(jQuery);