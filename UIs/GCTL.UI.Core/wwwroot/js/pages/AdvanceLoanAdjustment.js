
(function ($) {
    $.patientTypes = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            CompanyMultiSelectInput: "#companyMultiselectInput",
            CompanyDropdown: "#companyMultiselectDropdown",
            EmployeeDropdown: "#employeeMultiselectDropdown",
            EmployeeMultiselectInput: "#employeeMultiselectInput",
            EmployeeShowName:".EmployeeShowName",
            EmployeeShowDesignation: ".EmployeeShowDesignation",
            EmployeeShowDepartment: ".EmployeeShowDepartment",
            EmployeeShowJoiningDate: ".EmployeeShowJoiningDate",
            EmployeeArrow: '#employeeMultiselectArrow',
            CompanyArrow: '#companyMultiselectArrow',
            LoanSelectInput:"#loanSelectInput",
            LoanSelectItem: "#loanSelectItem",
            LoanArrow: "#loanArrow",
            LoanId:".loanId",
            LoanDate:".loanDate",
            LoanType:".loanType",
            LoanAmount:".loanAmount",
            LoanStartEndDate:".loanStartEndDate",
            LoanInstallment: ".loanInstallment",
            DateFrom: "#dateFrom",
            ToDate: "#toDate",
            MonthDeduction:".monthDeduction",
            DeductionHead: ".deductionHead",
            SaveBtn: ".js-Advance-loan-adjustment-save",
            AdjustmentAutoId: "#AdjustmentAutoId",
            Month: ".month",
            Year: ".year",
            Remarks:".custom-remarks-textarea",
        }, options);
        var filterUrl = commonName.baseUrl + "/GetFilterData";
        var getAllAndFilterCompanyUrl = commonName.baseUrl + "/GetAllAndFilterCompany";
        var GetEmployeesByFilterUrl = commonName.baseUrl + "/GetEmployeesByFilter";
        var GetLoadEmployeeByIdUrl = commonName.baseUrl + "/GetLoadEmployeeById";
        var GetLoanByEmployeeIdUrl = commonName.baseUrl + "/GetLoanByEmployeeId";
        var GetLoanByIdUrl = commonName.baseUrl + "/GetLoanById";
        function stHeader() {
            window.addEventListener('scroll', function () {
                const header = document.getElementById('stickyHeader');
                if (window.scrollY > 0) {
                    header.classList.add('scrolled');
                } else {
                    header.classList.remove('scrolled');
                }
            });
        }

        var $input = $('#multiselectInput');
        var $adjustmentArrow = $('#multiselectArrow');
       
        var $dropdown = $('#multiselectDropdown');
        var $selectedValuesSpan = $('#selectedValues');

        var adjustmentOption = [
            { value: 'Advance', label: 'Advance' },
            { value: 'Loan', label: 'Loan' }          
        ];
        function adjustmentGenerateOptions(options) {
            $dropdown.empty();

            $.each(options, function (index, option) {
                var $optionDiv = $('<div>', {
                    class: 'multiselect-option',
                    text: option.label,
                    'data-value': option.value
                });

                // ক্লিক করলে input-এ label বসবে, dropdown বন্ধ হবে
                $optionDiv.on('click', function () {
                    $input.val(option.label);
                    $input.attr("data-selected-value", option.value);
                    $dropdown.removeClass('show');
                    $adjustmentArrow.removeClass('rotated');

                    updateAdjustmentSelectedValues();
                    logAdjustmentSelectedValues();
                });

                $dropdown.append($optionDiv);
            });
        }

        // Selected value span update
        function updateAdjustmentSelectedValues() {
            var selectedValue = $input.attr('data-selected-value');
            $selectedValuesSpan.text(selectedValue ? selectedValue : 'None');
        }

        // Console log
        function logAdjustmentSelectedValues() {
            var selectedValue = $input.attr('data-selected-value');
            console.log('Selected Value:', selectedValue);

            if (!selectedValue) {
                adjustmentGenerateOptions(adjustmentOption);
            }
        }

        // Initialization function
        function initMultiselect() {
            adjustmentGenerateOptions(adjustmentOption);

            $input.on('click', function () {
                $dropdown.toggleClass('show');
                $companyDropdown.removeClass('show');
                $employeeDropdown.removeClass('show');
                $loanDropdown.removeClass('show');
                $adjustmentArrow.toggleClass('rotated');
            });

            $(document).on('click', function (event) {

                if ($(event.target).closest('.custom-multiselect').length === 0) {
                    $dropdown.removeClass('show');                  
                    $adjustmentArrow.removeClass('rotated');
                }
            });

            $dropdown.on('click', function (event) {
                event.stopPropagation();
            });

            updateAdjustmentSelectedValues();
            logAdjustmentSelectedValues();
        }
       
        //company

        var $companyInput = $(commonName.CompanyMultiSelectInput);
        var $companyDropdown = $(commonName.CompanyDropdown);

        function GetAllCompany(SearchCompanyText) {
            $.ajax({
                url: getAllAndFilterCompanyUrl,
                type: "GET",
                data: { companyName: SearchCompanyText },
                success: function (res) {
                    renderCompanyOptions(res);
                },
                error: function (e) {
                    console.log(e);
                }
            });
        }

        function renderCompanyOptions(companies) {
            $companyDropdown.empty();

            if (!companies || companies.length === 0) {
                $companyDropdown.append(`<div class="multiselect-option">Company Not Found</div>`);
                return;
            }

            $.each(companies, function (index, company) {
                var $optionDiv = $('<div>', {
                    class: 'multiselect-option',
                    text: company.companyName,
                    'data-value': company.companyCode
                });

                // Click event to set value
                $optionDiv.on('click', function () {
                    var selectedName = $(this).text();
                    var selectedValue = $(this).data("value");

                    $companyInput.val(selectedName);
                    $companyInput.attr("data-selected-value", selectedValue);
                    $companyDropdown.removeClass('show');
                    $(commonName.CompanyArrow).removeClass('rotated');
                });

                $companyDropdown.append($optionDiv);
            });

            if (companies.length > 0) {
                var firstCompay = companies[0];
                $companyInput.val(firstCompay.companyName);
                $companyInput.attr("data-selected-value", firstCompay.companyCode);
            }
        }

        function initCompanyMultiselect() {
            $companyInput.on('click', function () {
                $companyDropdown.toggleClass('show');
                $dropdown.removeClass('show');
                $employeeDropdown.removeClass('show');
                $loanDropdown.removeClass('show');
                $(commonName.CompanyArrow).addClass('rotated');
            });

            $companyInput.on('input', function () {
                var searchText = $(this).val();
                GetAllCompany(searchText);
            });

            // Hide dropdown when clicking outside
            $(document).on('click', function (e) {
                if (!$(e.target).closest('.custom-multiselect').length) {
                    $companyDropdown.removeClass('show');
                    $(commonName.CompanyArrow).removeClass('rotated');
                    
                }
            });

            $companyDropdown.on('click', function (e) {
                e.stopPropagation();
            });

            GetAllCompany('');
        }
               

        //get employee


        var $employeeInput = $(commonName.EmployeeMultiselectInput);
        var $employeeDropdown = $(commonName.EmployeeDropdown);
        function getEmployeeByFilter(SearchEmployeeText) {          
            $.ajax({
                url: GetEmployeesByFilterUrl,
                type: 'GET',
                data: {
                    employeeStatusId: '01',
                    companyCode: '001',
                    employeeName: SearchEmployeeText
                },
                success: function (data) {
                    renderEmployeeOptions(data);                    
                }, error: function (e) {
                    console.log(e);
                }
            });
        }

        function renderEmployeeOptions(employees) {
            $employeeDropdown.empty();

            if (!employees || employees.length === 0) {
                $employeeDropdown.append(`<div class="EmployeeMultiselect-option">Employee Not Found</div>`);
                return;
            }

            $.each(employees, function (index, employee) {
               
                var $optionEmpDiv = $('<div>', {
                    class: 'employee-list-item',
                    html: `
                <div style="padding: 6px 10px; cursor: pointer; border-bottom: 1px solid #eee;" 
                     data-value="${employee.employeeId}">
                    <strong>${employee.fullName}</strong>
                    <small>( ${employee.employeeId} )</small>
                </div>
            `
                });

                $optionEmpDiv.on('click', function () {
                    var selectedEmpName = $(this).find("strong").text().trim(); 
                    var selectedEmpId = $(this).find("small").text().replace(/[()]/g, '').trim(); 

                    var displayText = `${selectedEmpName} (${selectedEmpId})`;

                    $employeeInput.val(displayText);
                    $employeeInput.attr("data-selected-value", selectedEmpId);

                    $employeeDropdown.removeClass('show');
                    LoadEmployeeById(selectedEmpId);
                    $(commonName.EmployeeArrow).removeClass('rotated');
                });

                $employeeDropdown.append($optionEmpDiv);
            });
        }

        function initEmployeeMultiselect() {
            $employeeInput.on('click', function () {
                $employeeDropdown.toggleClass('show');
                $companyDropdown.removeClass('show');
                $dropdown.removeClass('show');
                $loanDropdown.removeClass('show');
                $(commonName.EmployeeArrow).toggleClass('rotated');
            });

            $employeeInput.on('input', function () {
                var searchText = $(this).val();
                getEmployeeByFilter(searchText);
            });

            $(document).on('click', function (e) {
                if (!$(e.target).closest('.custom-multiselect').length) {
                    $employeeDropdown.removeClass('show');
                    $(commonName.EmployeeArrow).removeClass('rotated');
                }
            });

            $employeeDropdown.on('click', function (e) {
                e.stopPropagation();
            });

            getEmployeeByFilter('');
        }

        LoadEmployeeById = function (employeeId) {
            console.log(employeeId);
            $.ajax({
                url:GetLoadEmployeeByIdUrl,
                type: 'POST',
                data: { employeeId },
                success: function (res) {
                    if (res != null) {
                        console.log(res);
                        LoadLoanByEmployeeId(res.employeeId);
                        $(commonName.EmployeeShowName).text(res.fullName);
                        $(commonName.EmployeeShowDesignation).text(res.designationName);
                        $(commonName.EmployeeShowDepartment).text(res.departmentName);
                        $(commonName.EmployeeShowJoiningDate).text(res.joiningDate);
                    }
                },
                error: function (e) {
                    console.log(e);
                }
            });
        }

        //loan id


        var $loanInput = $(commonName.LoanSelectInput);
        var $loanDropdown = $(commonName.LoanSelectItem);
        LoadLoanByEmployeeId = function (employeeId) {
            console.log(employeeId);
            console.log($loanInput);
            console.log($loanDropdown);
            $.ajax({
                url: GetLoanByEmployeeIdUrl,
                type: 'GET',
                data: { employeeId },
                success: function (res) {
                    console.log(res);
                    if (res != null) {
                        RenderLoanOption(res);
                    }
                },
                error: function (e) {
                    console.log(e);
                }
            });
        }

        RenderLoanOption = function (loans) {
            console.log(loans);

            $loanDropdown.empty();
            $loanInput.val("--Select Loan--");
            $loanDropdown.removeClass('show');
            if (!loans || loans.length === 0) {
                $loanDropdown.append(`<div class="loanMultiselectOption">Loan Id Not Found</div>`);                
                return;
            }

            $.each(loans, function (index, loan) {
                console.log(loan);
                var $optionLoanDiv = $('<div>', {
                    class: 'loan-list-item',
                    html: `
                <div style="padding:6px 10px; cursor:pointer; border-bottom:1px solid #eee;" data-value="${loan.loanId}">
                    <strong>${loan.loanType}</strong>
                    <small>(${loan.loanId})</small>
                </div>
            `
                });

                $optionLoanDiv.on('click', function () {
                    var selectedLoanName = $(this).find("strong").text().trim();
                    var selectedLoanId = $(this).find("small").text().replace(/[()]/g, '').trim();
                    var displayText = `${selectedLoanName} (${selectedLoanId})`;
                    $loanInput.val(displayText);
                    $loanInput.attr("data-selected-value", selectedLoanId);
                    $loanDropdown.removeClass('show');
                    GetLoanById(selectedLoanId);
                    $(commonName.LoanArrow).removeClass('rotated'); 
                });

                $loanDropdown.append($optionLoanDiv);
            });
        }
        function initLoan() {
            $loanInput.on('click', function () {
                $loanDropdown.toggleClass('show');
                $companyDropdown.removeClass('show');
                $dropdown.removeClass('show');
                $employeeDropdown.removeClass('show');
                $(commonName.LoanArrow).toggleClass('rotated');
            });

            $loanInput.on('input', function () {
                var searchText = $(this).val();
                LoadLoanByEmployeeId(searchText);
            });

            $(document).on('click', function (e) {
                if (!$(e.target).closest('.custom-multiselect').length) {
                    $loanDropdown.removeClass('show');
                    $(commonName.LoanArrow).removeClass('rotated');
                }
            });

            $loanDropdown.on('click', function (e) {
                e.stopPropagation();
            });

            LoadLoanByEmployeeId('');
        }
        GetLoanById = function (loanId) {
            console.log(loanId);
            $.ajax({
                url: GetLoanByIdUrl,
                type: "GET",
                data: { loanId },
                success: function (res) {
                    if (res) {
                    console.log(res);
                        $(commonName.LoanId).text(res.loanId);
                        $(commonName.LoanDate).text(res.loanDate);
                        $(commonName.LoanType).text(res.loanType);
                        $(commonName.LoanAmount).text(res.loanAmount);
                        $(commonName.LoanAmount).val(res.loanAmount);
                        $(commonName.LoanStartEndDate).text(res.loanStartEndDate);
                        $(commonName.LoanInstallment).text(res.noOfInstallment);
                        $(commonName.LoanInstallment).val(res.noOfInstallment);
                        customDateFlatpicker($(commonName.DateFrom)[0], res.starDate);
                        customDateFlatpicker($(commonName.ToDate)[0], res.endDate);
                        $(commonName.MonthDeduction).val(res.monthlyDeduction);
                        $(commonName.DeductionHead).val(res.payHeadNameId);
                    }
                }, error: function (e) {
                    console.log(e);
                }
            });
        }

        customDateFlatpicker = function (id, date) {
            console.log(id, date);
            flatpickr(id, {
                dateFormat: "d/m/Y",
                defaultDate: date ?? "today",
                allowInput: false
            });
        }

        $(commonName.AdjustmentAutoId).val("00000011");
        //create 
        CreateInputData = function () {
            var FormValue = {
                AdjuctmentType: $input.attr('data-selected-value'),
                AdjustmentAutoId: $(commonName.AdjustmentAutoId).val(),
                EmployeeId: $(commonName.EmployeeMultiselectInput).attr("data-selected-value"),
                AdvanceAdjustmentStatus: $('input[name="filterType"]:checked').val(),
                AdvanceAmount: $(commonName.LoanAmount).val(),
                MonthlyDeduction: $(commonName.MonthDeduction).val(),
                SalaryMonth: $(commonName.Month).val(),
                SalaryYear: $(commonName.Year).val(),
                NoOfPaymentInstallment: $(commonName.LoanInstallment).val(),
                PayHeadNameId: $(commonName.DeductionHead).val(),
                Remarks: $(commonName.Remarks).val(),
                LoanId: $(commonName.LoanSelectInput).attr('data-selected-value'),
            }
            return FormValue;
        }

        $(document).on('click', commonName.SaveBtn, function () {
            console.log(CreateInputData());
        })
        var init = function () {
            stHeader();
            customDateFlatpicker($(commonName.DateFrom)[0], null);
            customDateFlatpicker($(commonName.ToDate)[0], null);
            initMultiselect();
            initCompanyMultiselect();
            initEmployeeMultiselect();
            initLoan();
            console.log("test", filterUrl);
        };
        init();
    };
})(jQuery);
