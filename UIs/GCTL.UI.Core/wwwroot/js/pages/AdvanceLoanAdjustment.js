
(function ($) {
    $.patientTypes = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            CompanyMultiSelectInput: "#companyMultiselectInput",
            CompanyDropdown: "#companyMultiselectDropdown",
            EmployeeDropdown: "#employeeMultiselectDropdown",
            EmployeeMultiselectInput:"#employeeMultiselectInput",
        }, options);
        var filterUrl = commonName.baseUrl + "/GetFilterData";
        var getAllAndFilterCompanyUrl = commonName.baseUrl + "/GetAllAndFilterCompany";
        var GetEmployeesByFilterUrl = commonName.baseUrl + "/GetEmployeesByFilter";


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
        var $arrow = $('#multiselectArrow');
        var $dropdown = $('#multiselectDropdown');
        var $selectedValuesSpan = $('#selectedValues');

        var adjustmentOption = [
            { value: 'Advance', label: 'Advance' },
            { value: 'Loan', label: 'Loan' }          
        ];

        // Generate option list with checkboxes, maintaining checked states
        function adjustmentGenerateOptions(options) {
            var currentlySelected = $dropdown.find('input[type="checkbox"]:checked').map(function () {
                return $(this).val();
            }).get();

            $dropdown.empty();

            $.each(options, function (index, option) {
                var checkboxId = 'option' + (index + 1);
                var $optionDiv = $('<div>', { class: 'multiselect-option' });
                var $checkbox = $('<input>', {
                    type: 'checkbox',
                    id: checkboxId,
                    value: option.value
                });
                var $label = $('<label>', {
                    for: checkboxId,
                    text: option.label
                });

                if (currentlySelected.indexOf(option.value) !== -1) {
                    $checkbox.prop('checked', true);
                }

                $optionDiv.append($checkbox).append($label);
                $dropdown.append($optionDiv);

                $checkbox.on('change', function () {
                    updateAdjustmentInputValue();
                    updateAdjustmentSelectedValues();
                    logAdjustmentSelectedValues();
                });
            });
        }

        // Show/hide dropdown options based on input search text
        function filterOptions(searchText) {
            searchText = searchText.toLowerCase();

            $dropdown.find('.multiselect-option').each(function () {
                var label = $(this).find('label').text().toLowerCase();
                $(this).toggle(label.indexOf(searchText) !== -1);
            });
        }

        function initMultiselect() {
            adjustmentGenerateOptions(adjustmentOption);

            // Show/hide dropdown on input click
            $input.on('click', function () {
                $dropdown.toggleClass('show');
                $arrow.toggleClass('rotated');
                if ($dropdown.hasClass('show')) {
                    filterOptions($input.val());
                }
            });

            // Filter options as user types in input
            $input.on('input', function () {
                var val = $(this).val();
                filterOptions(val);
                if (!$dropdown.hasClass('show')) {
                    $dropdown.addClass('show');
                    $arrow.addClass('rotated');
                }
            });

            // Click outside closes dropdown
            $(document).on('click', function (event) {
                if ($(event.target).closest('.custom-multiselect').length === 0) {
                    $dropdown.removeClass('show');
                    $arrow.removeClass('rotated');
                }
            });

            $dropdown.on('click', function (event) {
                event.stopPropagation();
            });

            updateAdjustmentInputValue();
            updateAdjustmentSelectedValues();
            logAdjustmentSelectedValues();
        }

        // Update main input text based on selected checkboxes
        function updateAdjustmentInputValue() {
            var selectedLabels = $dropdown.find('input[type="checkbox"]:checked').map(function () {
                return $(this).next('label').text().trim();
            }).get();

            if (selectedLabels.length === 0) {
                $input.val('');
                $input.attr('placeholder', '~~Select Adjustment Type~~');
            } else if (selectedLabels.length === 1) {
                $input.val(selectedLabels[0]);
                $input.attr('placeholder', '');
            } else {
                $input.val(selectedLabels[0] + ' +' + (selectedLabels.length - 1) + ' more');
                $input.attr('placeholder', '');
            }
        }

        // Update span below with selected values (comma separated)
        function updateAdjustmentSelectedValues() {
            var selectedValues = $dropdown.find('input[type="checkbox"]:checked').map(function () {
                return $(this).val();
            }).get();

            $selectedValuesSpan.text(selectedValues.length > 0 ? selectedValues.join(', ') : 'None');
        }

        // Console log selected values array on every change
        function logAdjustmentSelectedValues() {
            var selectedValues = $dropdown.find('input[type="checkbox"]:checked').map(function () {
                return $(this).val();
            }).get();
            console.log("Selected Values:", selectedValues);
            if (selectedValues.length === 0) {
                adjustmentGenerateOptions(adjustmentOption);
            }
        }


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
                    $companyInput.attr("data-selected-value", selectedValue); // Store ID if needed

                    $companyDropdown.removeClass('show');
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
            });

            $companyInput.on('input', function () {
                var searchText = $(this).val();
                GetAllCompany(searchText);
            });

            // Hide dropdown when clicking outside
            $(document).on('click', function (e) {
                if (!$(e.target).closest('.custom-multiselect').length) {
                    $companyDropdown.removeClass('show');
                }
            });

            // Prevent closing when clicking inside the dropdown
            $companyDropdown.on('click', function (e) {
                e.stopPropagation();
            });

            // Initial load (optional)
            GetAllCompany('');
        }
               

        var statusId = "";
        var company = "";


        //get employee


        var $employeeInput = $(commonName.EmployeeMultiselectInput);
        var $employeeDropdown = $(commonName.EmployeeDropdown);
        function getEmployeeByFilter(SearchEmployeeText) {
            console.log(statusId);
            console.log(company);
            console.log(SearchEmployeeText);
            $.ajax({
                url: GetEmployeesByFilterUrl,
                type: 'GET',
                data: {
                    employeeStatusId: '',
                    companyCode: '',
                    employeeName: ''
                },
                success: function (data) {
                    console.log(data);
                    //renderEmployeeOptions(data);
                    //$('#employeeTableBody').empty();
                    //$.each(data, function (i, emp) {
                    //    $('#employeeTableBody').append(`
                    //        <tr>
                    //            <td>${emp.employeeId}</td>
                    //            <td>${emp.fullName}</td>
                    //            <td>${emp.departmentName}</td>
                    //            <td>${emp.designationName}</td>
                    //            <td>${emp.joiningDate}</td>
                    //        </tr>
                    //    `);
                    //});
                }, error: function (e) {
                    console.log(e);
                }
            });
        }

        function renderEmployeeOptions(employees) {
            $employeeDropdown.empty();

            if (!employees || employees.length === 0) {
                $employeeDropdown.append(`<div class="multiselect-option">Employee Not Found</div>`);
                return;
            }

            $.each(employees, function (index, employee) {
                var $optionDiv = $('<div>', {
                    class: 'multiselect-option',
                    text: employee.employeeName,
                    'data-value': employee.employeeId
                });

                // Click event to set value
                $optionDiv.on('click', function () {
                    var selectedName = $(this).text();
                    var selectedValue = $(this).data("value");

                    $employeeInput.val(selectedName);
                    $employeeInput.attr("data-selected-value", selectedValue); // Store ID if needed

                    $employeeDropdown.removeClass('show');
                });

                $employeeDropdown.append($optionDiv);
            });
        }

        function initEmployeeMultiselect() {
            $companyInput.on('click', function () {
                $employeeDropdown.toggleClass('show');
            });

            $employeeInput.on('input', function () {
                var searchText = $(this).val();
                getEmployeeByFilter(searchText);
            });

            // Hide dropdown when clicking outside
            $(document).on('click', function (e) {
                if (!$(e.target).closest('.custom-multiselect').length) {
                    $employeeDropdown.removeClass('show');
                }
            });

            // Prevent closing when clicking inside the dropdown
            $employeeDropdown.on('click', function (e) {
                e.stopPropagation();
            });

            // Initial load (optional)
            getEmployeeByFilter('');
        }
        var init = function () {
            stHeader();
            initMultiselect();
            initCompanyMultiselect();
            initEmployeeMultiselect();
            console.log("test", filterUrl);
        };
        init();
    };
})(jQuery);
