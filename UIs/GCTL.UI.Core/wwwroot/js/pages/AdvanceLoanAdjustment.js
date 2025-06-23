
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
            Remarks: ".custom-remarks-textarea",
            AdjustmentMultiselectInput: "#multiselectInput",
            ClearBtn:"#js-Advance-loan-adjustment-clear",
        }, options);
        var filterUrl = commonName.baseUrl + "/GetFilterData";
        var getAllAndFilterCompanyUrl = commonName.baseUrl + "/GetAllAndFilterCompany";
        var GetEmployeesByFilterUrl = commonName.baseUrl + "/GetEmployeesByFilter";
        var GetLoadEmployeeByIdUrl = commonName.baseUrl + "/GetLoadEmployeeById";
        var GetLoanByEmployeeIdUrl = commonName.baseUrl + "/GetLoanByEmployeeId";
        var GetLoanByIdUrl = commonName.baseUrl + "/GetLoanById";
        var SaveUpdateLoanAdjustmentUrl = commonName.baseUrl + "/SaveUpdateLoanAdjustment";
        var AdjustmentAutoGanarateIdUrl = commonName.baseUrl + "/AdjustmentAutoGanarateId";
        var GetMonthUrl = commonName.baseUrl + "/GetMonth";
        var GetHeadDeductionUrl = commonName.baseUrl + "/GetHeadDeduction";
        var GetAdvancePayDataUrl = commonName.baseUrl +"/GetAdvancePayData"
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
        $(document).on('input', commonName.LoanAmount, function () {
            $(commonName.MonthDeduction).val($(this).val());
        })
        function logAdjustmentSelectedValues() {
            var selectedValue = $input.attr('data-selected-value');

            if (!selectedValue) {
                adjustmentGenerateOptions(adjustmentOption);
                return;
            }

            console.log("Selected:", selectedValue);

            // Clear employee input
            $employeeInput.val('');
            $employeeInput.attr("data-selected-value", '');
            $employeeDropdown.empty();

            // Set loan adjustment flag
            loanAdjustment = selectedValue === "Loan";

            //console.log(loanAdjustment);
            if (!loanAdjustment) {
                ClearInputData();
                $(commonName.LoanSelectInput)
                    .removeClass('enable')
                    .addClass('disable');

                $('input[name="filterType"][value="By Month"]').prop('checked', true);
                $(commonName.DateFrom).removeClass('enable').addClass('disable');
                $(commonName.ToDate).removeClass('enable').addClass('disable');
                $(commonName.Month).removeClass('disable').addClass("enable");
                $(commonName.Year).removeClass('disable').addClass("enable");
                $(commonName.LoanAmount).removeClass('disable').addClass("enable");
                $(commonName.LoanInstallment).removeClass('disable').addClass("enable").val("1");
                $(commonName.MonthDeduction).removeClass('enable').addClass("disable");
                $(commonName.DeductionHead).removeClass('disable').addClass("enable");
                SelectDropdownMonth();
                SelectDropdownHeadDeduction();
                $(commonName.Year).val(new Date().getFullYear());
            }

            if (loanAdjustment) {
                ClearInputData();
                $('input[name="filterType"][value="By Date"]').prop('checked', true);
                $(commonName.LoanSelectInput).removeClass('disable').addClass('enable');
                $(commonName.DateFrom).removeClass('disable').addClass('enable');
                $(commonName.ToDate).removeClass('disable').addClass('enable');
                $(commonName.Month).removeClass('enable').addClass('disable');
                $(commonName.Year).removeClass('enable').addClass('disable');
                $(commonName.MonthDeduction).removeClass('enable').addClass('disable');
                $(commonName.LoanAmount).removeClass('enable').addClass("disable");
                $(commonName.LoanInstallment).removeClass('enable').addClass("disable");
                $(commonName.MonthDeduction).removeClass('enable').addClass("disable");
                $(commonName.DeductionHead).removeClass('enable').addClass("disable");
            }



            // Reload employee filter and re-bind employee dropdown
            setTimeout(function () {
                getEmployeeByFilter('');
                initEmployeeMultiselect(); // only binds click once
                $(commonName.EmployeeArrow).addClass('rotated');
            }, 100);
        }
        //month
        SelectDropdownMonth = function () {
            $.ajax({
                url: GetMonthUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.Month).empty();

                    $(commonName.Month).append(`<option value="">--Select Month--</option>`);
                    if (res.length > 0) {
                        $.each(res, function (index, month) {
                            $(commonName.Month).append(`<option value="${month.monthId}">${month.monthName}</option>  `);
                        })
                    }
                },
                error: function (e) {
                    console.log(e);
                }
            });
        }
        //head deduction
        SelectDropdownHeadDeduction = function () {
            $.ajax({
                url: GetHeadDeductionUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.DeductionHead).empty();

                    $(commonName.DeductionHead).append(`<option value="">--Select Deduction Head--</option>`);

                    console.log(res);
                    if (res.length > 0) {
                        $.each(res, function (index, dHead) {
                            $(commonName.DeductionHead).append(`<option value="${dHead.payHeadNameId}">${dHead.name}</option>  `);
                        })
                    }
                },
                error: function (e) {
                    console.log(e);
                }
            });
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
            console.log(loanAdjustment);
            $.ajax({
                url: GetEmployeesByFilterUrl,
                type: 'GET',
                data: {
                    employeeStatusId: '01',
                    companyCode: '001',
                    employeeName: SearchEmployeeText,
                    loanAdjustment: loanAdjustment
                },
                success: function (data) {
                    renderEmployeeOptions(data);   
                    //$employeeDropdown.addClass('show');
                    //$(commonName.EmployeeArrow).addClass('rotated');
                }, error: function (e) {
                    console.log(e);
                }
            });
        }

        function renderEmployeeOptions(employees) {
            $employeeDropdown.empty();
            console.log(employees);
            if (!employees || employees.length === 0) {
                $employeeDropdown.append(`<div class="EmployeeMultiselect-option">Employee Not Found</div>`);
                return;
            }

            $.each(employees, function (index, employee) {
                var $loanId = "";
                if (employee.loanId != null) {
                    $loanId = `(${employee.loanId})`
                }
                var $optionEmpDiv = $('<div>', {
                    class: 'employee-list-item',
                    html: `
                <div style="padding: 6px 10px; cursor: pointer; border-bottom: 1px solid #eee;" 
                     data-value="${employee.employeeId}">
                    <strong>${employee.fullName}</strong>
                    <small>( ${employee.employeeId} )</small>
                    <span>${$loanId}</span>
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

        //function initEmployeeMultiselect() {
        //    $employeeInput.on('click', function () {
        //        $employeeDropdown.toggleClass('show');
        //        $companyDropdown.removeClass('show');
        //        $dropdown.removeClass('show');
        //        $loanDropdown.removeClass('show');
        //        $(commonName.EmployeeArrow).toggleClass('rotated');
        //    });

        //    $employeeInput.on('input', function () {
        //        var searchText = $(this).val();
        //        getEmployeeByFilter(searchText);
        //    });

        //    $(document).on('click', function (e) {
        //        if (!$(e.target).closest('.custom-multiselect').length) {
        //            $employeeDropdown.removeClass('show');
        //            $(commonName.EmployeeArrow).removeClass('rotated');
        //        }
        //    });

        //    $employeeDropdown.on('click', function (e) {
        //        e.stopPropagation();
        //    });

        //    getEmployeeByFilter('');
        //}

        function initEmployeeMultiselect() {
            $employeeInput.off('click').on('click', function () {
                if (!$employeeDropdown.hasClass('show')) {
                    $employeeDropdown.addClass('show');
                    $(commonName.EmployeeArrow).addClass('rotated');
                }

                // Hide others
                $companyDropdown.removeClass('show');
                $dropdown.removeClass('show');
                $loanDropdown.removeClass('show');
            });

            $employeeInput.off('input').on('input', function () {
                var searchText = $(this).val();
                getEmployeeByFilter(searchText);
            });

            $(document).off('click.employeeDropdown').on('click.employeeDropdown', function (e) {
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
            $.ajax({
                url:GetLoadEmployeeByIdUrl,
                type: 'POST',
                data: { employeeId },
                success: function (res) {
                    if (res != null) {
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
            $.ajax({
                url: GetLoanByEmployeeIdUrl,
                type: 'GET',
                data: { employeeId },
                success: function (res) {
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
            $loanDropdown.empty();
            $loanInput.val("--Select Loan--");
            $loanDropdown.removeClass('show');
            if (!loans || loans.length === 0) {
                $loanDropdown.append(`<div class="loanMultiselectOption">Loan Id Not Found</div>`);                
                return;
            }

            $.each(loans, function (index, loan) {
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
            $.ajax({
                url: GetLoanByIdUrl,
                type: "GET",
                data: { loanId },
                success: function (res) {
                    if (res) {
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
            flatpickr(id, {
                dateFormat: "d/m/Y",
                defaultDate: date ?? "today",
                allowInput: false
            });
        }

        AdjustmentAutoGanarateId = function () {
            $.ajax({
                url: AdjustmentAutoGanarateIdUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.AdjustmentAutoId).val(res);
                }, error: function (e) {
                    console.log(e);
                }
            });
        }
        //create
      

        CustomFlatDate = function (dateStr) {
            if (!dateStr) return null;
            var parts = dateStr.split('/');
            if (parts.length !== 3) return null;
            return `${parts[2]}-${parts[1].padStart(2, '0')}-${parts[0].padStart(2, '0')}`;
        }

        //$(document).ready(function () {
        //    console.log("Initial loan amount:", $("input.loanAmount.enable").val());  // প্রাথমিক মান (ফাঁকা হবে)

        //    $(document).on('input', 'input.loanAmount.enable', function () {
        //        console.log("User typed loan amount:", $(this).val());
        //    });
        //});



        //CreateInputData = function () {
        //    var FormValue = {
        //        AdjustmentType: $input.attr('data-selected-value'),
        //        AdvancePayId: $(commonName.AdjustmentAutoId).val(),
        //        EmployeeID: $(commonName.EmployeeMultiselectInput).attr("data-selected-value"),
        //        AdvanceAdjustStatus: $('input[name="filterType"]:checked').val(),
        //        AdvanceAmount: $(commonName.LoanAmount).val(),
        //        MonthlyDeduction: $(commonName.MonthDeduction).val(),
        //        SalaryMonth: $(commonName.Month).val(),
        //        SalaryYear: $(commonName.Year).val(),
        //        NoOfPaymentInstallment: $(commonName.LoanInstallment).val(),
        //        PayHeadNameId: $(commonName.DeductionHead).val(),
        //        Remarks: $(commonName.Remarks).val(),
        //        LoanID: $(commonName.LoanSelectInput).attr('data-selected-value') ,
        //        FromDate: CustomFlatDate($(commonName.DateFrom).val()),
        //       ToDate: CustomFlatDate($(commonName.ToDate).val())
        //    }
        //    return FormValue;
        //}

        // ১. একবার শুধু event handler বসান (ডকুমেন্ট রেডি বা স্ক্রিপ্ট লোডে)
        $(document).on('input', 'input.loanAmount.enable', function () {
            console.log("User typed loan amount:", $(this).val());
        });

        // ২. CreateInputData ফাংশন — শুধুমাত্র ডেটা রিটার্ন করবে
        CreateInputData = function () {
            var FormValue = {
                AdjustmentType: $input.attr('data-selected-value'),
                AdvancePayId: $(commonName.AdjustmentAutoId).val(),
                EmployeeID: $(commonName.EmployeeMultiselectInput).attr("data-selected-value"),
                AdvanceAdjustStatus: $('input[name="filterType"]:checked').val(),
                AdvanceAmount: $(commonName.LoanAmount).val(),
                MonthlyDeduction: $(commonName.MonthDeduction).val(),
                SalaryMonth: $(commonName.Month).val(),
                SalaryYear: $(commonName.Year).val(),
                NoOfPaymentInstallment: $(commonName.LoanInstallment).val(),
                PayHeadNameId: $(commonName.DeductionHead).val(),
                Remarks: $(commonName.Remarks).val(),
                LoanID: $(commonName.LoanSelectInput).attr('data-selected-value'),
                FromDate: CustomFlatDate($(commonName.DateFrom).val()),
                ToDate: CustomFlatDate($(commonName.ToDate).val())
            }
            return FormValue;
        }

        // ৩. যখন data তৈরি করবেন, তখন console.log করুন
        console.log("Form data:", CreateInputData());

      

        $(document).on('click', commonName.ClearBtn, function () {
            console.log("click");
            ClearInputData();
            $(commonName.AdjustmentMultiselectInput).attr('data-selected-value', '').val(''); // EmployeeID
            $(commonName.EmployeeMultiselectInput).attr('data-selected-value', '').val(''); 
        })


        ClearInputData = function () {
            AdjustmentAutoGanarateId();
            $(commonName.AdjustmentAutoId).val(''); // AdvancePayId
            //$(commonName.AdjustmentMultiselectInput).attr('data-selected-value', '').val(''); // EmployeeID
            //$(commonName.EmployeeMultiselectInput).attr('data-selected-value', '').val(''); 
            $('input[name="filterType"][value="By Date"]').prop('checked', true);


            $(commonName.LoanAmount).val(''); // AdvanceAmount
            $(commonName.MonthDeduction).val(''); // MonthlyDeduction
            $(commonName.Month).val(''); // SalaryMonth
            $(commonName.Year).val(''); // SalaryYear
            $(commonName.LoanInstallment).val(''); // NoOfPaymentInstallment
            $(commonName.DeductionHead).val(''); // PayHeadNameId
            $(commonName.Remarks).val(''); // Remarks
            $(commonName.LoanSelectInput).attr('data-selected-value', '').val(''); // LoanID


            customDateFlatpicker(commonName.DateFrom,null);
            customDateFlatpicker(commonName.ToDate, null);

            $(commonName.EmployeeShowName).text('');
            $(commonName.EmployeeShowDesignation).text('');
            $(commonName.EmployeeShowDepartment).text('');
            $(commonName.EmployeeShowJoiningDate).text('');

            $(commonName.LoanId).text('');
            $(commonName.LoanDate).text('');
            $(commonName.LoanType).text('');
            $(commonName.LoanAmount).text('');
            $(commonName.LoanStartEndDate).text('');
            $(commonName.LoanInstallment).text('');
           
        }


        //$(document).on('input', 'input.loanAmount.enable', function () {
        //    console.log("User typed loan amount:", $(this).val());
        //});
        $(document).on('click', commonName.SaveBtn, function () {
            var formData = CreateInputData();
            if (!loanAdjustment) {
                formData.AdvanceAmount = $("input.loanAmount.enable").val();
            }

            $.ajax({
                type: "POST",
                url: SaveUpdateLoanAdjustmentUrl,
                data: JSON.stringify(formData),
                contentType: "application/json; charset=utf-8",
                //dataType: "json",
                success: function (response) {
                    if (response.success) {
                        alert(response.message);
                       
                    } else {
                        alert("Error: " + response.message);
                    }
                }
                ,
                error: function (xhr, status, error) {
                    alert("Ajax Error: " + error);
                },complete: function () {
                    // Always call auto ID generate after request completes
                    ClearInputData();
                    $(commonName.AdjustmentMultiselectInput).attr('data-selected-value', '').val(''); // EmployeeID
                    $(commonName.EmployeeMultiselectInput).attr('data-selected-value', '').val(''); 
                    AdjustmentAutoGanarateId();
                }
            });
        });

        //GridAdvanceLoan = function () {
        //    $('#advancePayTable').DataTable({
        //        "processing": true,
        //        "serverSide": true,
        //        "ajax": {
        //            "url": GetAdvancePayDataUrl,
        //            "type": "POST",
        //            "data": function (d) {
        //                d.page = (d.start / d.length) + 1;
        //                d.pageSize = d.length;
        //            },
        //            "dataSrc": "data"
        //        },
        //        "columns": [
        //            { "data": "AdvancePayId" },
        //            { "data": "FullName" },
        //            { "data": "DepartmentName" },
        //            { "data": "DesignationName" },
        //            { "data": "AdvanceAmount" },
        //            { "data": "SalaryMonth" },
        //            { "data": "SalaryYear" }
        //        ]
        //    });
        //}

        //GridAdvanceLoan = function () {
        //    var table = $('#advancePayTable').DataTable({
        //        "processing": true,
        //        "serverSide": true,
        //        "ajax": {
        //            "url": GetAdvancePayDataUrl,  // আপনার API URL দিন
        //            "type": "POST",
        //            "data": function (d) {
        //                // DataTables থেকে page number এবং page size বের করা
        //                d.page = (d.start / d.length) + 1;
        //                d.pageSize = d.length;
        //            },
        //            "dataSrc": function (json) {
        //                console.log("Received Data:", json.data); // কনসোলে ডেটা দেখুন
        //                return json.data; // ডেটাকে DataTable এ পাঠান
        //            }
        //        },
        //        "columns": [
        //            { "data": "advancePayId" },
        //            { "data": "fullName" },
        //            { "data": "departmentName" },
        //            { "data": "designationName" },
        //            { "data": "advanceAmount" },
        //            { "data": "salaryMonth" },
        //            { "data": "salaryYear" }
        //        ]
        //    });

        //    // Optional: প্রতিবার AJAX call হবার পরে ডেটা দেখতে এই ইভেন্ট ইউজ করতে পারেন
        //    table.on('xhr', function (e, settings, json) {
        //        console.log("XHR Data:", json);
        //    });
        //};










        // Updated GridAdvanceLoan function with proper error handling
        GridAdvanceLoan = function () {
            var table = $('#advancePayTable').DataTable({
                "processing": true,
                "serverSide": true,
                "responsive": true,
                "pageLength": 10,
                "lengthMenu": [[5, 10, 25, 50, 100], [5, 10, 25, 50, 100]],
                "language": {
                    "processing": "Processing...",
                    "lengthMenu": "Show _MENU_ entries",
                    "zeroRecords": "No matching records found",
                    "info": "Showing _START_ to _END_ of _TOTAL_ entries",
                    "infoEmpty": "Showing 0 to 0 of 0 entries",
                    "infoFiltered": "(filtered from _MAX_ total entries)",
                    "search": "Search:",
                    "paginate": {
                        "first": "First",
                        "last": "Last",
                        "next": "Next",
                        "previous": "Previous"
                    },
                    "emptyTable": "No data available in table",
                    "loadingRecords": "Loading..."
                },
                "ajax": {
                    "url": GetAdvancePayDataUrl,
                    "type": "POST",
                    "data": function (d) {
                        // DataTables pagination parameters
                        d.page = Math.floor(d.start / d.length) + 1;
                        d.pageSize = d.length;
                        d.searchValue = d.search.value || "";

                        // Custom filter parameters
                        d.department = $('#departmentFilter').val() || "";
                        d.month = $('#monthFilter').val() || "";
                        d.year = $('#yearFilter').val() || "";

                        console.log('Request Data:', d);
                        return d;
                    },
                    "dataSrc": function (json) {
                        console.log("Response Data:", json);

                        // Check for errors
                        if (json.error) {
                            console.error('Server Error:', json.error);
                            alert('Error: ' + json.error);
                            return [];
                        }

                        // Validate response structure
                        if (!json.data) {
                            console.error('Invalid response: missing data property');
                            return [];
                        }

                        // Ensure data is an array
                        if (!Array.isArray(json.data)) {
                            console.error('Invalid response: data is not an array');
                            return [];
                        }

                        // Set total records for pagination
                        json.recordsTotal = json.recordsTotal || 0;
                        json.recordsFiltered = json.recordsFiltered || json.recordsTotal || 0;

                        return json.data;
                    },
                    "error": function (xhr, error, thrown) {
                        console.error('Ajax Error:', {
                            status: xhr.status,
                            statusText: xhr.statusText,
                            responseText: xhr.responseText,
                            error: error,
                            thrown: thrown
                        });

                        let errorMessage = 'Unknown error occurred';
                        try {
                            const response = JSON.parse(xhr.responseText);
                            errorMessage = response.error || response.message || errorMessage;
                        } catch (e) {
                            errorMessage = xhr.statusText || errorMessage;
                        }

                        alert('Error loading data: ' + errorMessage);
                    }
                },
                "columns": [
                    {
                        "data": null,
                        "title": "Select",
                        "render": function (data, type, row, meta) {
                            return `<input type="checkbox" class="row-checkbox" data-id="${row.advancePayId}">`;
                        },
                        "orderable": false,
                        "width": "5%"
                    },
                    {
                        "data": "advancePayId",
                        "title": "ID",
                        "width": "5%",
                        "render": function (data) {
                            return data || 'N/A';
                        }
                    },
                    {
                        "data": "loanID",
                        "title": "Loan ID",
                    },
                    {
                        "data": "employeeID",
                        "title": "Employee ID",                        
                    },
                    {
                        "data": "fullName",
                        "title": "Name",
                        "width": "15%",
                        "render": function (data) {
                            return data || 'N/A';
                        }
                    },
                    {
                        "data": "designationName",
                        "title": "Designation",
                        "width": "12%",
                        "render": function (data) {
                            return data || 'N/A';
                        }
                    },
                    {
                        "data": "advanceAmount",
                        "title": "Loan Amount",
                        "width": "10%",
                        "render": function (data) {
                            const amount = parseFloat(data) || 0;
                            return  amount.toLocaleString('en-US', {
                                minimumFractionDigits: 2,
                                maximumFractionDigits: 2
                            });
                        }
                    },
                    {
                        "data": "noOfPaymentInstallment",
                        "title": "No. Of Inst(s)",
                        "width": "8%",
                        "render": function (data) {
                            return data || 'N/A';
                        }
                    },
                    {
                        "data": "monthlyDeduction",
                        "title": "Monthly Deduction",
                        "width": "10%",
                        "render": function (data) {
                            if (data == null || data === '') return 'N/A';
                            const amount = parseFloat(data) || 0;
                            return  amount.toLocaleString('en-US', {
                                minimumFractionDigits: 2,
                                maximumFractionDigits: 2
                            });
                        }
                    },
                    {
                        "data": "salaryMonth",
                        "title": "Salary Month",
                        "width": "8%",
                        "render": function (data) {
                            return data || 'N/A';
                        }
                    },
                    {
                        "data": "salaryYear",
                        "title": "Salary Year",
                        "width": "8%",
                        "render": function (data) {
                            return data || 'N/A';
                        }
                    }
                ],
                "order": [[1, "desc"]], // Order by ID column
                "drawCallback": function (settings) {
                    $('[title]').tooltip(); // Tooltip reinit if needed
                },
                "initComplete": function () {
                    console.log("Advance Pay Table initialized.");
                }

            });

            // Event handlers for action buttons
            $('#advancePayTable').on('click', '.view-btn', function () {
                const id = $(this).data('id');
                if (id && id > 0) {
                    viewAdvancePay(id);
                } else {
                    alert('Invalid ID for view operation');
                }
            });

            $('#advancePayTable').on('click', '.edit-btn', function () {
                const id = $(this).data('id');
                if (id && id > 0) {
                    editAdvancePay(id);
                } else {
                    alert('Invalid ID for edit operation');
                }
            });

            $('#advancePayTable').on('click', '.delete-btn', function () {
                const id = $(this).data('id');
                if (id && id > 0) {
                    deleteAdvancePay(id);
                } else {
                    alert('Invalid ID for delete operation');
                }
            });

            return table;
        };

        // Action functions with validation
        function viewAdvancePay(id) {
            if (!id || id <= 0) {
                alert('Invalid ID');
                return;
            }
            console.log('View Advance Pay ID: ' + id);
            // Implement your view logic here
        }

        function editAdvancePay(id) {
            if (!id || id <= 0) {
                alert('Invalid ID');
                return;
            }
            console.log('Edit Advance Pay ID: ' + id);
            // Implement your edit logic here
        }

        function deleteAdvancePay(id) {
            if (!id || id <= 0) {
                alert('Invalid ID');
                return;
            }

            if (confirm('Are you sure you want to delete this advance pay record?')) {
                console.log('Delete Advance Pay ID: ' + id);
                // Implement your delete logic here

                // Example AJAX call for delete:
                /*
                $.ajax({
                    url: '/AdvanceLoanAdjustment/Delete',
                    type: 'POST',
                    data: { id: id },
                    success: function(response) {
                        if (response.success) {
                            alert('Record deleted successfully');
                            advancePayTable.ajax.reload();
                        } else {
                            alert('Error: ' + response.message);
                        }
                    },
                    error: function(xhr, status, error) {
                        alert('Error deleting record: ' + error);
                    }
                });
                */
            }
        }
























        var init = function () {
            stHeader();
            customDateFlatpicker($(commonName.DateFrom)[0], null);
            customDateFlatpicker($(commonName.ToDate)[0], null);
            initMultiselect();
            initCompanyMultiselect();
            initLoan();
            AdjustmentAutoGanarateId();
            GridAdvanceLoan();
            console.log("test", filterUrl);
        };
        init();
    };
})(jQuery);
