let employeeDataTable = null;
let filterChangeBound = false;
let originalEmployeeId = null;
let selectedBenefitIds = new Set();
let selectedEmployeeIds = new Set();
let isEditMode = false;

$(document).ready(function () {
    setupLoadingOverlay();
    initializeMultiselects();
    initializeEmployeeGrid();
    loadAllFilterEmp();
    loadBenefitTypeDD();
    loadOtherBenefitData();
    initializeEventHandlers();
    setupEnterKeyNavigation();
    loadMonthsDD();
});
$(window).on('load', function () {
    $('#employee-filter-grid-body')[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
});
function setupLoadingOverlay() {
    console.log("Loading");
    if ($("#loadingOverlay").length === 0) {
        $("body").append(`
            <div id="loadingOverlay" style="
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
                      
                    </div>
                    
                </div>
            </div>
        `);
    }
}
function showLoading() {
    $('body').css('overflow', 'hidden');
    $("#loadingOverlay").fadeIn(200);
}

function hideLoading() {
    $('body').css('overflow', '');
    $("#loadingOverlay").fadeOut(200);
}
function setupEnterKeyNavigation() {
    const $form = $('#other-benefit-form');
    if (!$form.length) return;

    $form.on('keydown', 'input, select, textarea, button, [tabindex]:not([tabindex="-1"])', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();

            const $focusable = $form
                .find('input:not([disabled]), select:not([disabled]), textarea:not([disabled]), button, [href], [tabindex]:not([tabindex="-1"])')
                .filter(':visible');

            const index = $focusable.index(this);
            if (index > -1) {
                const $next = $focusable.eq(index + 1).length ? $focusable.eq(index + 1) : $focusable.eq(0);
                $next.focus();
            }
        }
    });
}
function initializeEventHandlers() {
    $("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect, #employeeStatus, #activityStatusSelect")
        .on("change", function () {
            loadAllFilterEmp();
        });

    $('#BenefitTypeId').focus();

    $("#js-other-benefit-dec-clear").on('click', function () {
        clearForm();
    });

    $(".js-other-benefit-dec-save").on('click', handleFormSubmission);
    $(".js-other-benefit-dec-delete-confirm").on('click', handleBulkDelete);
    $("#excelUploadForm").submit(excelUpload);

    $(document).on("click", ".other-benefit-id-link", function () {
        const id = $(this).data("id");
        if (!id) return;

        populateForm(id);
    });

    $('#other-benefit-grid').DataTable().columns.adjust().draw();

    $("#other-benefit-check-all").on('change', function () {
        const isChecked = $(this).is(':checked');
        $('#other-benefit-grid-body input[type="checkbox"]').prop('checked', isChecked);
        updateSelectedOtherBenefitIds();
    });

    $("#other-benefit-check-all").on('change', function () {
        const isChecked = $(this).is(':checked');
        $('#other-benefit-grid-body input[type="checkbox"]').prop('checked', isChecked);

        if (isChecked) {
            $('#other-benefit-grid-body input[type="checkbox"]').each(function () {
                selectedBenefitIds.add($(this).data('id'));
            });
        } else {
            $('#other-benefit-grid-body input[type="checkbox"]').each(function () {
                selectedBenefitIds.delete($(this).data('id'));
            });
        }
    });

    $(document).on('change', '#other-benefit-grid-body input[type="checkbox"]', function () {
        const id = $(this).data('id');

        if ($(this).is(':checked')) {
            selectedBenefitIds.add(id);
        } else {
            selectedBenefitIds.delete(id);
        }

        const total = $('#other-benefit-grid-body input[type="checkbox"]').length;
        const checked = $('#other-benefit-grid-body input[type="checkbox"]:checked').length;
        $("#other-benefit-check-all").prop('checked', total > 0 && total === checked);
    });
    $('#employee-filter-grid').on('keydown', 'input[type="checkbox"]', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#BenefitTypeId').focus();
        }
    });

    $("#employee-check-all").on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#BenefitTypeId').focus();
        }
    });


    if (!$('#SalaryYear').val()) {
        const currentYear = new Date().getFullYear();
        $('#SalaryYear').val(currentYear);
    }

    $(document).on("click", "#openBenefitTypeModal", function () {
        loadBenefitTypeContent();
        $("#benefitTypeModal").modal("show");
    });

    $("#employee-check-all").on('change', function () {
        const isChecked = $(this).is(':checked');
        const checkboxes = $('#employee-filter-grid-body input[type="checkbox"]').not(':disabled');

        checkboxes.prop('checked', isChecked);
        updateSelectedEmployeeIds();
    });

    $('#employee-filter-grid-body').on('change', 'input[type="checkbox"]', function () {
        updateSelectedEmployeeIds();
        updateSelectAllCheckboxState();
    });
}

function refreshBenefitTypeDropdown(selectedId) {
    $.ajax({
        url: '/HrmPayOthersAdjustment/getBenefitTypeDD',
        type: 'GET',
        success: function (data) {
            var dropdown = $("#BenefitTypeId");
            dropdown.empty();
            dropdown.append('<option value="">--Select Benefit Type--</option>');

            $.each(data, function (index, item) {
                dropdown.append($('<option></option>')
                    .attr('value', item.value || item.benefitTypeId)
                    .text(item.text || item.benefitType));
            });
            if (selectedId) {
                dropdown.val(selectedId);
                dropdown.trigger('change');
            }
        },
        error: function (error) {
            console.error("Error refreshing benefit type dropdown:", error);
        }
    });
}
function loadBenefitTypeContent() {
    $("#benefitTypeContent").html('<div class="text-center"><i class="fa fa-spinner fa-spin fa-3x"></i></div>');

    $.ajax({
        url: '/HrmPayDefBenefitType',
        data: { child: true },
        type: 'GET',
        success: function (response) {
            $("#benefitTypeContent").html(response);

            if (typeof $.benefitTypes === 'function') {
                var options = {
                    baseUrl: '/HrmPayDefBenefitType',
                    isModal: true,
                    onSaved: function (benefitTypeId) {
                        if (benefitTypeId) {
                            refreshBenefitTypeDropdown(benefitTypeId);
                        } else {
                            loadBenefitTypeDD();
                        }
                        $("#benefitTypeModal").modal("hide");
                    }
                };
                $.benefitTypes(options);
            }
        },
        error: function (error) {
            $("#benefitTypeContent").html('<div class="alert alert-danger">Error loading content. Please try again.</div>');
            console.error("Error loading benefit type content:", error);
        }
    });
}
function updateSelectedOtherBenefitIds() {
    const currentPageCheckboxes = $('#other-benefit-grid-body input[type="checkbox"]');

    currentPageCheckboxes.each(function () {

        const id = $(this).data('id');

        if ($(this).is(':checked')) {
            selectedBenefitIds.add(id);
        } else {
            selectedBenefitIds.delete(id);
        }
    });
}

function updateSelectedEmployeeIds() {
    const currentPageCheckboxes = $('#employee-filter-grid-body input[type="checkbox"]');
    currentPageCheckboxes.each(function () {
        const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();

        if ($(this).is(':checked')) {
            selectedEmployeeIds.add(employeeId);
        } else {
            selectedEmployeeIds.delete(employeeId);
        }
    });
}

function getAllFilterVal() {
    const filterData = {
        CompanyCodes: toArray($("#companySelect").val()),
        BranchCodes: toArray($("#branchSelect").val()),
        DivisionCodes: toArray($("#divisionSelect").val()),
        DepartmentCodes: toArray($("#departmentSelect").val()),
        DesignationCodes: toArray($("#designationSelect").val()),
        EmployeeIDs: toArray($("#employeeSelect").val()),
        EmployeeStatuses: toArray($("#activityStatusSelect").val()),
    };
    return filterData;
}

function toArray(value) {
    if (!value) return [];
    if (Array.isArray(value)) return value;
    return [value];
}

function loadAllFilterEmp() {
    showLoading();
    const filterData = getAllFilterVal();

    $.ajax({
        url: `/HrmPayOthersAdjustment/getFilterEmp`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(filterData),
        success: function (res) {
            const data = res.lookupData;

            loadTableData(res);
            if (data.companies?.length) {
                populateSelect("#companySelect", data.companies)
            }
            if (data.branches?.length) {
                populateSelect("#branchSelect", data.branches);
            }
            if (data.divisions?.length) {
                populateSelect("#divisionSelect", data.divisions);
            }
            if (data.departments?.length) {
                populateSelect("#departmentSelect", data.departments);
            }
            if (data.designations?.length) {
                populateSelect("#designationSelect", data.designations);
            }
            if (data.employees?.length) {
                populateSelect("#employeeSelect", data.employees);
            }

            setupClearOnChangeEvents();

            bindFilterChangeOnce();
        },
        complete: function () {
            hideLoading();
        },
        error: function (xhr, status, error) {
            console.error("Error loading filtered employees:", error);
            hideLoading();
        }
    });
}

function populateSelect(selectId, dataList) {
    const $select = $(selectId);
    dataList.forEach(item => {
        if (item.code && item.name && $select.find(`option[value="${item.code}"]`).length === 0) {
            $select.append(`<option value="${item.code}">${item.name}</option>`);
        }
    });
    $select.multiselect('rebuild');
}

function setupClearOnChangeEvents() {
    const clearMap = {
        "#activityStatusSelect": ["#branchSelect", "#divisionSelect", "#departmentSelect", "#designationSelect", "#employeeSelect"],
        "#branchSelect": ["#divisionSelect", "#departmentSelect", "#designationSelect", "#employeeSelect"],
        "#divisionSelect": ["#departmentSelect", "#designationSelect", "#employeeSelect"],
        "#departmentSelect": ["#designationSelect", "#employeeSelect"],
        "#designationSelect": ["#employeeSelect"]
    };

    Object.entries(clearMap).forEach(([parent, children]) => {
        $(parent).off("change.clearDropdowns").on("change.clearDropdowns", function () {
            if (parent !== "#activityStatusSelect") {
                children.forEach(child => $(child).empty().multiselect('rebuild'));
            }
        });
    });
}
function bindFilterChangeOnce() {
    if (!filterChangeBound) {
        $("#activityStatusSelect").on("change.loadFilter", function () {
            ["#branchSelect", "#divisionSelect", "#departmentSelect", "#designationSelect", "#employeeSelect"].forEach(selector => {
                $(selector).val(null).multiselect('refresh');
            });
            loadAllFilterEmp();
        });

        $("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect").on("change.loadFilter", function () {
            loadAllFilterEmp();
        });

        filterChangeBound = true;
    }
}

function loadTableData(res) {
    var tableDataItem = res.employees;

    if ($.fn.DataTable.isDataTable('#employee-filter-grid') && employeeDataTable !== null) {
        employeeDataTable.destroy();
        employeeDataTable = null;
    }

    var tableBody = $("#employee-filter-grid-body");
    tableBody.empty();

    $.each(tableDataItem, function (index, employee) {
        var row = $('<tr>');

        const isOriginalEmployee = isEditMode && String(employee.employeeId) === String(originalEmployeeId);
        const checkboxDisabled = isEditMode ? 'disabled' : '';
        const checkboxChecked = isOriginalEmployee ? 'checked' : '';

        row.append(`<td width="1%" class="text-center"><input type="checkbox" class="empSelect" ${checkboxDisabled} ${checkboxChecked}"/></td>`);
        row.append('<td class="text-center" style="padding: 0;">' + employee.employeeId + '</td>');
        row.append('<td class="text-center" style="padding: 0;">' + employee.employeeName + '</td>');
        row.append('<td class="text-center" style="padding: 0;">' + employee.designationName + '</td>');
        row.append('<td class="text-center" style="padding: 0;">' + employee.departmentName + '</td>');
        row.append('<td class="text-center" style="padding: 0;">' + employee.branchName + '</td>');
        row.append('<td class="text-center" style="padding: 0;">' + employee.employeeTypeName + '</td>');
        row.append('<td class="text-center" style="padding: 0;">' + employee.employmentNature + '</td>');
        row.append('<td class="text-center" style="padding: 0;">' + employee.joiningDate + '</td>');
        row.append('<td class="text-center" >' + employee.employeeStatus + '</td>');

        tableBody.append(row);
    });

    initializeDataTable();

    if (isEditMode) {
        $('#employee-check-all').prop('disabled', true)
    }
}

function initializeDataTable() {
    employeeDataTable = $("#employee-filter-grid").DataTable({
        paging: true,
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
        lengthChange: true,
        info: true,
        autoWidth: false,
        responsive: true,
        fixedHeader: false,
        //scrollX: true,
        columnDefs: [
            {
                targets: 0,
                orderable: false,
                className: 'no-sort'
            }
        ],
        initComplete: function () {
            hideLoading();
            $('#custom-search').on('keyup', function () {
                employeeDataTable.search(this.value).draw();
            });

            $('.dataTables_filter input').css({
                'width': '250px',
                'padding': '6px',
                'border': '1px solid #ddd',
                'border-radius': '4px',
            });

            $('#employee-filter-grid_wrapper .dataTables_filter').hide();
        },
        drawCallback: function () {
            $('#employee-filter-grid-body input[type="checkbox"]').each(function () {
                const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();
                $(this).prop('checked', selectedEmployeeIds.has(employeeId));
            });

            updateSelectAllCheckboxState();
        }
    });
}
function updateSelectAllCheckboxState() {
    const total = $('#employee-filter-grid-body input[type="checkbox"]').not(':disabled').length;
    const checked = $('#employee-filter-grid-body input[type="checkbox"]').not(':disabled').filter(':checked').length;

    $("#employee-check-all").prop('checked', total > 0 && total === checked);
}

function initializeEmployeeGrid() {
    showLoading();
    setTimeout(function () {
        if (employeeDataTable !== null) {
            employeeDataTable.destroy();
        }
        initializeDataTable();
    }, 100);
}

function initializeMultiselects() {
    const nonSelectedTextMap = {
        companySelect: 'Company',
        branchSelect: 'Branch',
        divisionSelect: 'Division',
        departmentSelect: 'Department',
        designationSelect: 'Designation',
        employeeSelect: 'Employee',
        activityStatusSelect: 'Status'
    };

    Object.keys(nonSelectedTextMap).forEach(function (id) {
        const selector = $('#' + id);
        selector.closest('div');
        selector.multiselect({
            enableFiltering: true,
            includeSelectAllOption: true,
            selectAllText: 'Select All',
            nonSelectedText: nonSelectedTextMap[id],
            nSelectedText: 'Selected',
            allSelectedText: 'All Selected',
            filterPlaceholder: 'Search',
            buttonWidth: '100%',
            maxHeight: 350,
            maxWidth: 150,
            filterBehavior: 'text',
            enableCaseInsensitiveFiltering: true,
            buttonText: function (options, select) {
                if (options.length === 0) {
                    return nonSelectedTextMap[id];
                }
                else if (options.length > 0 && (id != 'companySelect') && id !='activityStatusSelect') {
                    return options.length + ' Selected';
                }
                else {
                    return $(options[0]).text();
                }
            }
        });
    });
}

function loadMonthsDD() {
    const currentMonth = new Date().getMonth() + 1;

    $.ajax({
        url: "/HrmPayOthersAdjustment/getMonthDD",
        type: "GET",
        dataType: "json",
        success: function (data) {

            console.log(data);
            var dropdown = $("#SalaryMonth");

            dropdown.empty();
            dropdown.append('<option value="">--Select Month--</option>');

            $.each(data, function (index, month) {
                dropdown.append(
                    '<option value="' + month.monthId + '">' + month.monthName + '</option>'
                );
            });

            const currentMonth = new Date().getMonth() + 1;

            if (!dropdown.val()) {
                dropdown.val(currentMonth).trigger('change');
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("Error loading customers:", textStatus, errorThrown);
        }
    });
}

function loadBenefitTypeDD() {
    $.ajax({
        url: "/HrmPayOthersAdjustment/getBenefitTypeDD",
        type: "GET",
        dataType: "json",
        success: function (data) {
            console.log(data);
            var dropdown = $("#BenefitTypeId");

            dropdown.empty();
            dropdown.append('<option value="">--Select Benefit Type--</option>');

            $.each(data, function (index, benefitType) {
                dropdown.append(
                    '<option value="' + benefitType.benefitTypeId + '">' + benefitType.benefitType + '</option>'
                );
            });

        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("Error loading customers:", textStatus, errorThrown);
        }
    });
}

function loadOtherBenefitData() {
    showLoading();
    displayOtherBenefitTable();
    hideLoading();
}

function displayOtherBenefitTable() {
    if ($.fn.DataTable.isDataTable("#other-benefit-grid")) {
        $("#other-benefit-grid").DataTable().clear().destroy();
    }

    const tableBody = $("#other-benefit-grid-body");
    tableBody.empty();

    $('#other-benefit-grid').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/HrmPayOthersAdjustment/GetPaginatedOtherBenefit',
            type: 'POST',
            data: function (d) {

            }
        },
        columns: [
            {
                data: null,
                orderable: false,
                className: 'text-center',
                render: function (data, type, row) {
                    return `<input class="empBenefitSelect" type="checkbox" width="1%" style="padding-left: 0; padding-right: 0;" data-id="${row.tc}" />`;
                }
            },
            {
                data: 'otherBenefitId',
                className: 'text-center',
                render: function (data, type, row) {
                    return `<a href="#other-benefit-form" class="other-benefit-id-link" style="padding-left: 0; padding-right: 0;" data-id="${row.tc}">${data}</a>`;
                }
            },
            { data: 'employeeId', className: 'text-center', style: { paddingLeft: '0', paddingRight: '0' } },
            { data: 'employeeName', className: 'text-left', style: { paddingLeft: '0', paddingRight: '0' } },
            { data: 'designation', className: 'text-left', style: { paddingLeft: '0', paddingRight: '0' } },
            { data: 'department', className: 'text-left', style: { paddingLeft: '0', paddingRight: '0' } },
            { data: 'benefitType', className: 'text-center', style: { paddingLeft: '0', paddingRight: '0' } },
            { data: 'benefitAmount', className: 'text-center', style: { paddingLeft: '0', paddingRight: '0' } },
            { data: 'salaryMonth', className: 'text-center', style: { paddingLeft: '0', paddingRight: '0' } },
            { data: 'salaryYear', className: 'text-center', style: { paddingLeft: '0', paddingRight: '0' } },
            { data: 'remarks', className: 'text-center', style: { paddingLeft: '0', paddingRight: '0' } },
            {
                data: 'ldate',
                className: 'text-center',
                style: { paddingLeft: '0', paddingRight: '0' },
                render: function (data, type, row) {
                    if (!data) return '';
                    const date = new Date(data);

                    const pad = (n) => n < 10 ? '0' + n : n;

                    const day = pad(date.getDate());
                    const month = pad(date.getMonth() + 1); // Months are 0-indexed
                    const year = date.getFullYear();

                    let hours = date.getHours();
                    const minutes = pad(date.getMinutes());
                    const ampm = hours >= 12 ? 'PM' : 'AM';
                    hours = hours % 12;
                    hours = hours ? hours : 12; // Convert 0 to 12
                    const formattedTime = `${pad(hours)}:${minutes} ${ampm}`;

                    return `${day}/${month}/${year}<br>${formattedTime}`;
                }
            },
            { data: 'luser', className: 'text-center', style: { paddingLeft: '0', paddingRight: '0' } }
        ],
        autoWidth: false,
        fixedHeader: false,
        info: true,
        lengthChange: true,
        lengthMenu: [[10, 25, 50, 100, 1000], [10, 25, 50, 100, 1000]],
        ordering: true,
        pageLength: 10,
        paging: true,
        responsive: true,
        scrollCollapse: true,
        scrollX: true,
        scrollY: "460px",
        searching: true,
        columnDefs: [
            {
                targets: 0,
                orderable: false,
                className: 'no-sort'
            }
        ],
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
            emptyTable: "No data available",
            processing: "Loading data..."
        },
        initComplete: function () {
            const api = this.api();
            let debounceTimeout;

            $('.dataTables_filter input')
                .off()
                .on('input', function () {
                    clearTimeout(debounceTimeout);
                    const searchTerm = this.value;

                    debounceTimeout = setTimeout(function () {
                        api.search(searchTerm).page('first').draw('page');
                    }, 500);
                });
        },
        drawCallback: function () {
            $('#other-benefit--grid-body input[type="checkbox"]').each(function () {
                const id = $(this).data('id');
                $(this).prop('checked', selectedBenefitIds.has(id));
            });

            const total = $('#other-benefit--grid-body input[type="checkbox"]').length;
            const checked = $('#other-benefit--grid-body input[type="checkbox"]:checked').length;
            $("#other-benefit-check-all").prop('checked', total > 0 && total === checked);
        }
    });
}

function handleFormSubmission() {
    if (!validateForm()) {
        return;
    }

    const url = `/HrmPayOthersAdjustment/SaveOthersBenefit`;
    const type = "POST";
    const EmployeeIds = Array.from(selectedEmployeeIds);

    const dataToSend = {
        Tc: $('#Tc').val() || 0,
        OtherBenefitId: '0',
        //Id: $('#Id').val(),
        BenefitTypeId: $(`#BenefitTypeId`).val(),
        BenefitAmount: $(`#BenefitAmount`).val() || null,
        Remarks: $(`#Remarks`).val(),
        SalaryMonth : $(`#SalaryMonth`).val(),
        SalaryYear : $('#SalaryYear').val(),
        PaidDays: $('#PaidDays').val() || 0,
        CompanyCode: $("#companySelect").val(),
        EmployeeIds: EmployeeIds
    }

    showLoading();
    $.ajax({
        url: url,
        type: type,
        data: JSON.stringify(dataToSend),
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            showNotification("Data Saved Successfully.", "success");
            loadOtherBenefitData();
            clearForm();
            //loadOtherBenefitId();
            $('#other-benefit-form')[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
            $('#BenefitTypeId').focus();
        },
        error: function (xhr, status, error) {
            showNotification(`Error saving data`, "error");
            console.log(`Error saving data `, error);
        },
        complete: hideLoading
    })
}

function validateForm() {

    if (selectedEmployeeIds.size === 0) {
        showNotification("Please select at least one employee", "info");
        $('#employee-filter-grid-body').focus();
        return false;
    }

    if (!$("#BenefitTypeId").val()) {
        showNotification("Please select a Salary benefit type", "info");
        $('#BenefitTypeId').focus();
        return false;
    }

    const salaryMonth = $('#SalaryMonth').val();
    const salaryYear = $('#SalaryYear').val();
    let yearRegex = /^\d{4}$/;

    if (!salaryMonth) {
        showNotification("Please select Salary Month","warning");
        $('#SalaryMonth').focus();
        return false;
    }

    if (!salaryYear) {
        showNotification("Please enter Salary Year","warning");
        $('#SalaryYear').focus();
        return false;
    }

    if (!yearRegex.test(salaryYear) || salaryYear <2000) {
        showNotification("Please enter a valid SalaryYear (2000 or later)", "warning");
        $('#SalaryYear').focus();
        return false;
    }

    return true;
}

function excelUpload(e) {
    e.preventDefault();

    var formData = new FormData(this);
    var fileInput = $('#excelFileInput')[0].files[0];

    if (fileInput) {
        formData.append('file', fileInput);
    }

    var comId = $("#companySelect").val();
    if (comId) {
        formData.append('CompanyCode', comId);
    }

    $.ajax({
        url: `/HrmPayOthersAdjustment/UploadExcelAsync`,
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        success: function (res) {
            if (res.success) {
                $("#choosefileText").text("Choose File");
                showNotification(res.message, "success");
                if (typeof loadOtherBenefitData === 'function') {
                    loadOtherBenefitData();
                }
            } else {
                showNotification(res.message, "error");
            }
        },
        error: function (xhr, status, error) {
            console.log(xhr.responseText);
            let errorMessage = "An error occurred";

            try {
                const response = JSON.parse(xhr.responseText);
                if (response) {
                    errorMessage = response.message;
                }
            } catch (e) {
                errorMessage = xhr.statusText || error;
            }

            showNotification(errorMessage, "error");
        }
    });
}

function clearForm() {
    isEditMode = false;
    $('#Tc').val('');
    const currentMonth = new Date().getMonth() + 1;
    const currentYear = new Date().getFullYear();
    $("#SalaryMonth").val(currentMonth);
    $("#SalaryYear").val(currentYear);
    $("#BenefitTypeId").val('');
    $("#BenefitAmount").val('');
    $("#Remarks").val('');
    $("#PaidDays").val('');
    $("#branchSelect").val(null).trigger('change');
    $("#activityStatusSelect").val(null).trigger('change');
    selectedBenefitIds.clear();
    selectedEmployeeIds.clear();
    originalEmployeeId = null;
    $('#employee-filter-grid-body input[type="checkbox"]').prop('checked', false).prop('disabled', false);
    $('#employee-check-all').prop('checked', false).prop('disabled', false);
}

function populateForm(id) {
    $.ajax({
        url: `/HrmPayOthersAdjustment/GetById/${id}`,
        type: "GET",
        success: function (response) {
            if (!response || !response.data) {
                showNotification("Invalid data format received", "error");
                return;
            }

            try {
                const data = response.data;
                clearForm();

                isEditMode = true;

                $('#Tc').val(data.autoId);
                $("#SalaryMonth").val(data.salaryMonth);
                $("#SalaryYear").val(data.salaryYear);
                $("#BenefitTypeId").val(data.benefitTypeId);
                $("#BenefitAmount").val(data.benefitAmount);
                $("#PaidDays").val(data.payDate);
                $("#Remarks").val(data.remarks);

                originalEmployeeId = String(data.employeeId);

                selectedEmployeeIds.clear();
                selectedEmployeeIds.add(originalEmployeeId);
                setTimeout(() => {
                    filterEmployeeGridByEmployeeId(originalEmployeeId);
                    $('#employee-filter-grid-body input[type="checkbox"]').each(function () {
                        const empId = $(this).closest('tr').find('td:nth-child(2)').text().trim();
                        if (empId === originalEmployeeId) {
                            $(this).prop('checked', true).prop('disabled', true);
                        } else {
                            $(this).prop('checked', false).prop('disabled', true);
                        }
                    });

                    $('#employee-check-all').prop('disabled', true);
                }, 500);
            } catch (e) {
                console.error("Error populating form:", e);
                showNotification("Error loading record details", "error");
            }
        },
        error: function (xhr, status, error) {
            console.error("Error fetching record:", error);
            showNotification("Failed to load record details", "error");
        }
    });
}

function filterEmployeeGridByEmployeeId(empId) {
    showLoading();
    $("#branchSelect, #divisionSelect, #departmentSelect, #designationSelect").val(null).multiselect('refresh');

    $("#employeeSelect").val(empId).multiselect('refresh');

    const filterData = {
        CompanyCodes: ["001"],
        BranchCodes: [],
        DivisionCodes: [],
        DepartmentCodes: [],
        DesignationCodes: [],
        EmployeeIDs: [empId],
        EmployeeStatuses: toArray($("#activityStatusSelect").val()),
    };

    $.ajax({
        url: `/HrmPayOthersAdjustment/getFilterEmp`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(filterData),
        success: function (res) {
            loadTableData(res);

            setTimeout(function () {
                $('#employee-filter-grid-body input[type="checkbox"]').prop('checked', true).prop('disabled', true);
            }, 100);
        },
        complete: function () {
            hideLoading();
        },
        error: function (xhr, status, error) {
            console.error("Error filtering employee:", error);
            hideLoading();
            showNotification("Error filtering employee data", "error");
        }
    });
}

function handleBulkDelete() {
    const selectedIds = Array.from(selectedBenefitIds);

    if (selectedIds.length === 0) {
        showNotification("Please select record to delete", "warning");
        return;
    }

    if (!confirm(`Are you sure you want to delete ${selectedIds.length} selected other Benefit(s)?`)) {
        return;
    }

    showLoading();

    $.ajax({
        url: `/HrmPayOthersAdjustment/BulkDelete`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Tcs: selectedIds }),
        success: function (response) {
            selectedBenefitIds.clear();
            showNotification(response.message || "Successfully deleted items", "success");
            loadOtherBenefitData();
            clearForm();
        },
        error: function (xhr, status, error) {
            console.error("Error details:", xhr.responseText);
            showNotification("Error deleting salary benefit records", "error");
        },
        complete: hideLoading
    });
}
function showNotification(message, type) {
    if (typeof toastr !== 'undefined') {
        toastr[type](message, type === 'success' ? 'Success' : type === 'error' ? 'Error' : 'Warning');
    } else {
        alert(message);
    }
}