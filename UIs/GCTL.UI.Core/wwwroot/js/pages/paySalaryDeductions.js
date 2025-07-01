//paySalaryDeduction.js
let employeeDataTable = null;
let filterChangeBound = false;
let originalEmployeeId = null;
let selectedDeductionIds = new Set();
let selectedEmployeeIds = new Set();
let isEditMode = false;

$(document).ready(function () {
    setupLoadingOverlay();
    loadMonthsDD();
    initializeMultiselects();
    initializeEmployeeGrid();
    loadAllFilterEmp();
    loadDeductionTypeDD();
    loadSalaryDeductionId();
    loadSalaryDeductionData();
    initializeEventHandlers();
    setupEnterKeyNavigation();
});

$(window).on('load', function () {
    $('#employee-filter-grid-body')[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
});

    function setupLoadingOverlay() {
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
    const $form = $('#salary-deduction-form');
    if (!$form.length) return;

    $form.on('keydown', 'input, select, textarea, button, [tabindex]:not([tabindex="-1"])', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();

            const $focusable = $form
                .find('input:not([disabled]), select:not([disabled]), textarea:not([disabled]), button, [href], [tabindex]:not([tabindex="-1"])')
                .filter(':visible');

            const index = $focusable.index(this);
            if (index > -1) {
                const $next = $focusable.eq(index + 1).length ?
                    $focusable.eq(index + 1) : $focusable.eq(0);
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

    $('input[name="durationType"]').change(function () {
        toggleFields();
    });
    $('#byMonth').prop('checked', true);
    
    toggleFields();
    $('#SalaryMonth').focus();

    $("#js-salary-deduction-dec-clear").on('click', function () {
        clearForm();
        loadSalaryDeductionId();
    });

    $(".js-salary-deduction-dec-save").on('click', handleFormSubmission);
    $(".js-salary-deduction-dec-delete-confirm").on('click', handleBulkDelete);
    $("#excelUploadForm").submit(excelUpload);

    $(document).on("click", ".salary-deduction-id-link", function () {
        const id = $(this).data("id");
        if (!id) return;

        populateForm(id);
    });

    $('#salary-deduction-grid').DataTable().columns.adjust().draw();

    $("#salary-deduction-check-all").on('change', function () {
        const isChecked = $(this).is(':checked');
        $('#salary-deduction-grid-body input[type="checkbox"]').prop('checked', isChecked);
        updateSelectedSalaryDeductionIds();
    });

    $("#salary-deduction-check-all").on('change', function () {
        const isChecked = $(this).is(':checked');
        $('#salary-deduction-grid-body input[type="checkbox"]').prop('checked', isChecked);

        if (isChecked) {
            $('#salary-deduction-grid-body input[type="checkbox"]').each(function () {
                selectedDeductionIds.add($(this).data('id'));
            });
        } else {
            $('#salary-deduction-grid-body input[type="checkbox"]').each(function () {
                selectedDeductionIds.delete($(this).data('id'));
            });
        }
    });

    $(document).on('change', '#salary-deduction-grid-body input[type="checkbox"]', function () {
        const id = $(this).data('id');

        if ($(this).is(':checked')) {
            selectedDeductionIds.add(id);
        } else {
            selectedDeductionIds.delete(id);
        }

        const total = $('#salary-deduction-grid-body input[type="checkbox"]').length;
        const checked = $('#salary-deduction-grid-body input[type="checkbox"]:checked').length;
        $("#salary-deduction-check-all").prop('checked', total > 0 && total === checked);
    });

    let today = new Date().toISOString().split('T')[0]; 

    $("#dateFrom, #dateTo").val(today); 
    //debugger;
    //loadMonthsDD();
    //if (!$('#SalaryMonth').val()) {
    //    //loadMonthsDD();
    //    const currentMonth = new Date().getMonth() + 1;
    //    $('#SalaryMonth').val(currentMonth).trigger('change');
    //}



    $('#employee-filter-grid').on('keydown', 'input[type="checkbox"]', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            // Focus on the appropriate field based on duration type
            if ($('#byMonth').is(':checked')) {
                $('#SalaryMonth').focus();
            } else if ($('#byDate').is(':checked')) {
                $('#dateFrom').focus();
            }
        }
    });

    $("#employee-check-all").on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            // Focus on the appropriate field based on duration type
            if ($('#byMonth').is(':checked')) {
                $('#SalaryMonth').focus();
            } else if ($('#byDate').is(':checked')) {
                $('#dateFrom').focus();
            }
        }
    });


    if (!$('#SalaryYear').val()) {
        const currentYear = new Date().getFullYear();
        $('#SalaryYear').val(currentYear);
    }

    $(document).on("click", "#openDeductionTypeModal", function () {
        console.log("Hello from outer");
        loadDeductionTypeContent();
        $("#deductionTypeModal").modal("show");
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


function refreshDeductionTypeDropdown(selectedId) {
    $.ajax({
        url: '/HrmPaySalaryDeduction/getDeductionTypeDD',
        type: 'GET',
        success: function (data) {
            console.log("refereshing with selected ID: ",selectedId);
            var dropdown = $("#DeductionTypeId");
            dropdown.empty();
            dropdown.append('<option value="">--Select Deduction Type--</option>');

            $.each(data, function (index, item) {
                dropdown.append($('<option></option>')
                    .attr('value', item.value || item.deductionTypeId)
                    .text(item.text || item.deductionType));
            });
            if (selectedId) {
                dropdown.val(selectedId);
                dropdown.trigger('change');
            }
        },
        error: function (error) {
            console.error("Error refreshing deduction type dropdown:", error);
        }
    });
}
function loadDeductionTypeContent() {
    $("#deductionTypeContent").html('<div class="text-center"><i class="fa fa-spinner fa-spin fa-3x"></i></div>');

    $.ajax({
        url: '/HrmPayDefDeductionType',
        data: { child: true },
        type: 'GET',
        success: function (response) {
            $("#deductionTypeContent").html(response);

            if (typeof $.deductionTypes === 'function') {
                var options = {
                    baseUrl: '/HrmPayDefDeductionType',
                    isModal: true,
                    onSaved: function (deductionTypeId) {
                        if (deductionTypeId) {
                            refreshDeductionTypeDropdown(deductionTypeId);
                        } else {
                            loadDeductionTypeDD();
                        }
                        $("#deductionTypeModal").modal("hide");
                    }
                };
                $.deductionTypes(options);
            }
        },
        error: function (error) {
            $("#deductionTypeContent").html('<div class="alert alert-danger">Error loading content. Please try again.</div>');
            console.error("Error loading deduction type content:", error);
        }
    });
}
function updateSelectedSalaryDeductionIds() {
    const currentPageCheckboxes = $('#salary-deduction-grid-body input[type="checkbox"]');

    currentPageCheckboxes.each(function () {

        const id = $(this).data('id');

        if ($(this).is(':checked')) {
            selectedDeductionIds.add(id);
        } else {
            selectedDeductionIds.delete(id);
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
function toggleFields() {
    if ($('#byDate').is(':checked')) {

        $('#dateFrom, #dateTo').prop('disabled', false).attr('tabindex', function (index) {
            return index + 1; 
        });


        $('#SalaryMonth, #SalaryYear').prop('disabled', true).attr('tabindex', -1);


        $('#dateFromLabel').html('Date From:<span style="color:red;">*</span>');
        $('#dateToLabel').html('Date To:<span style="color:red;">*</span>');
        $('#SalaryMonthLabel').html('Salary Month:');
        $('#SalaryYearLabel').html('Salary Year:');

    } else if ($('#byMonth').is(':checked')) {

        $('#dateFrom, #dateTo').prop('disabled', true).attr('tabindex', -1);


        $('#SalaryMonth').prop('disabled', false).attr('tabindex', 1);
        $('#SalaryYear').prop('disabled', false).attr('tabindex', 2);


        $('#dateFromLabel').html('Date From:');
        $('#dateToLabel').html('Date To:');
        $('#SalaryMonthLabel').html('Salary Month:<span style="color:red;">*</span>');
        $('#SalaryYearLabel').html('Salary Year:<span style="color:red;">*</span>');


        if (!$('#SalaryMonth').val()) {
            const currentMonth = new Date().getMonth() + 1;
            $('#SalaryMonth').val(currentMonth).trigger('change');
        }

        if (!$('#SalaryYear').val()) {
            const currentYear = new Date().getFullYear();
            $('#SalaryYear').val(currentYear);
        }
    }
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
        url: `/HrmPaySalaryDeduction/getFilterEmp`,
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
        "#activityStatusSelect": ["#divisionSelect", "#departmentSelect", "#designationSelect", "#employeeSelect"],
        "#branchSelect": ["#divisionSelect", "#departmentSelect", "#designationSelect", "#employeeSelect"],
        "#divisionSelect": ["#departmentSelect", "#designationSelect", "#employeeSelect"],
        "#departmentSelect": ["#designationSelect", "#employeeSelect"],
        "#designationSelect": ["#employeeSelect"]
    };

    Object.entries(clearMap).forEach(([parent, children]) => {
        $(parent).off("change.clearDropdowns").on("change.clearDropdowns", function () {
            children.forEach(child => $(child).empty().multiselect('rebuild'));
        });
    });
}

function bindFilterChangeOnce() {
    if (!filterChangeBound) {
        $("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect, #employeeStatus, #activityStatusSelect")
            .on("change.loadFilter", function () {
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

        row.append(`<td class="p-2 text-center"><input type="checkbox" class="empSelect" ${checkboxDisabled} ${checkboxChecked} /></td>`);
        row.append('<td class="p-2 text-center">' + employee.employeeId + '</td>');
        row.append('<td class="p-2 text-start">' + employee.employeeName + '</td>');
        row.append('<td class="p-2 text-start">' + employee.designationName + '</td>');
        row.append('<td class="p-2 text-center">' + employee.departmentName + '</td>');
        row.append('<td class="p-2 text-center">' + employee.branchName + '</td>');
        row.append('<td class="p-2 text-center">' + employee.employeeTypeName + '</td>');
        row.append('<td class="p-2 text-center">' + employee.employmentNature + '</td>');
        row.append('<td class="p-2 text-center">' + employee.joiningDate + '</td>');
        row.append('<td class="p-2 text-center">' + employee.employeeStatus + '</td>');

        tableBody.append(row);
    });

    initializeDataTable();

    if (isEditMode) {
        $('#employee-check-all').prop('disabled',true)
    }
}

function initializeDataTable() {
    employeeDataTable = $("#employee-filter-grid").DataTable({
        paging: true,
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
        lengthChange: true,
        scrollY: "400px",
        info: true,
        autoWidth: false,
        responsive: true,
        fixedHeader: false,
        scrollX: true,
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
                'padding': '6px 12px',
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
        companySelect: 'Select Company',
        branchSelect: 'Select Branch',
        divisionSelect: 'Select Division',
        departmentSelect: 'Select Department',
        designationSelect: 'Select Designation',
        employeeSelect: 'Select Employee',
        activityStatusSelect: 'Select Status'
    };

    Object.keys(nonSelectedTextMap).forEach(function (id) {
        const selector = $('#' + id);
       // selector.closest('div').css('margin', '1rem');
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
            filterBehavior: 'text', 
            enableCaseInsensitiveFiltering: true,
            buttonText: function (options, select) {
                if (options.length === 0) {
                    return nonSelectedTextMap[id];
                }
                else if (options.length > 1) {
                    return options.length + ' Selected';
                }
                else {
                    return $(options[0]).text();
                }
            }
        });
    });
}

function setCurrentYear() {
    const currentYear = new Date().getFullYear();
    $("#SalaryYear").val(currentYear);
}

function loadMonthsDD() {
    //debugger;
    const currentMonth = new Date().getMonth() + 1;

    $.ajax({
        url: "/HrmPaySalaryDeduction/getMonthDD",
        type: "GET",
        dataType: "json",
        success: function (data) {

            console.log(data);
            var dropdown = $("#SalaryMonth");

            dropdown.empty();
            dropdown.append('<option value="">--Select Month--</option>');

            $.each(data, function (index, month) {
                //const isSelected = month.monthId == currentMonth ? 'selected' : '';
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

function loadDeductionTypeDD() {
    $.ajax({
        url: "/HrmPaySalaryDeduction/getDeductionTypeDD",
        type: "GET",
        dataType: "json",
        success: function (data) {
            var dropdown = $("#DeductionTypeId");

            dropdown.empty();
            dropdown.append('<option value="">--Select Deduction Type--</option>');

            $.each(data, function (index, deductionType) {
                dropdown.append(
                    '<option value="' + deductionType.deductionTypeId + '">' + deductionType.deductionType + '</option>'
                );
            });

        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("Error loading customers:", textStatus, errorThrown);
        }
    });
}

function loadSalaryDeductionId() {
    $.ajax({
        url: "/HrmPaySalaryDeduction/GenerateNewId",
        type: "GET",
        dataType: "json",
        success: function (data) {
            if (data) {
                $('#Id').val(data);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error fetching Salary deduction ID:", error);
        }
    })
}

function loadSalaryDeductionData() {
    showLoading();
    displaysalaryDeductionTable();
    hideLoading();
}

function displaysalaryDeductionTable() {
    if ($.fn.DataTable.isDataTable("#salary-deduction-grid")) {
        $("#salary-deduction-grid").DataTable().clear().destroy();
    }

    const tableBody = $("#salary-deduction-grid-body");
    tableBody.empty();

    $('#salary-deduction-grid').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/HrmPaySalaryDeduction/GetPaginatedSalaryDeduction',
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
                    return `<input class="empHolidaySelect" type="checkbox" width="1%" style="padding: 0;" data-id="${row.autoId}" />`;
                }
            },
            {
                data: 'id',
                className: 'text-center',
                render: function (data, type, row) {
                    return `<a href="#salary-deduction-form" class="salary-deduction-id-link" data-id="${row.autoId}">${data}</a>`;
                }
            },
            { data: 'employeeId', className: 'text-center' },
            { data: 'employeeName', className: 'text-left' },
            { data: 'designation', className: 'text-left' },
            { data: 'deductionType', className: 'text-center' },
            { data: 'deductionAmount', className: 'text-center' },
            { data: 'salaryMonth', className: 'text-center' },
            { data: 'salaryYear', className: 'text-center' },
            { data: 'remarks', className: 'text-center' },
            { data: 'luser', className: 'text-center' }
        ],
        autoWidth: false,
        fixedHeader: false,
        info: true,
        lengthChange: true,
        lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
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
            $('#salary-deduction-grid-body input[type="checkbox"]').each(function () {
                const id = $(this).data('id');
                $(this).prop('checked', selectedDeductionIds.has(id));
            });

            const total = $('#salary-deduction-grid-body input[type="checkbox"]').length;
            const checked = $('#salary-deduction-grid-body input[type="checkbox"]:checked').length;
            $("#salary-deduction-check-all").prop('checked', total > 0 && total === checked);
        }
    });
}

function handleFormSubmission() {
    if (!validateForm()) {
        return;
    }

    let dataToSend;
    const id = $('#Id').val().trim();
    const url = `/HrmPaySalaryDeduction/SaveSalaryDeduction`;
    const type =  "POST";
    const EmployeeIds = Array.from(selectedEmployeeIds);

    dataToSend = {
        AutoId: $('#AutoId').val() || 0,
        Id: $('#Id').val(),
        DeductionType: $(`#DeductionTypeId`).val(),
        DeductionAmount: $(`#DeductionAmount`).val(),
        Remarks: $(`#remarks`).val(),
        CompanyCode: $("#companySelect").val(),
        EmployeeIds: EmployeeIds
    }

    if ($('#byDate').is(':checked')) {
        dataToSend.DateForm = $('#dateFrom').val();
        dataToSend.DateTo = $('#dateTo').val() || null;
    } else if ($('#byMonth').is(':checked')) {
        dataToSend.SalaryMonth = $('#SalaryMonth').val();
        dataToSend.SalaryYear = $('#SalaryYear').val();
    }

    showLoading();

    $.ajax({
        url: url,
        type: type,
        data: JSON.stringify(dataToSend),
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            showNotification("Data Saved Successfully.", "success");
            loadSalaryDeductionData();
            clearForm();
            loadSalaryDeductionId();
            $('#salaryDeductionForm')[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
            $('#SalaryMonth').focus();
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
        showNotification("Please select at least one employee", "warning");
        $('#employee-filter-grid-body').focus();
        return false;
    }

    if (!$("#DeductionTypeId").val()) {
        showNotification("Please select a Salary deduction type", "warning");
        $('#DeductionTypeId').focus(); 
        return false;
    }
    
    if (!$("#DeductionAmount").val()) {
        showNotification("Please enter a Salary deduction amount", "warning");
        $('#DeductionAmount').focus(); 
        return false;
    }
    
    if ($('#byDate').is(':checked')) {
        const dateFrom = $('#dateFrom').val();
        const dateTo = $('#dateTo').val();

        if (!dateFrom) {
            showNotification("Please enter date from", "warning");
            $('#dateFrom').focus();
            return false;
        }

        if (!dateTo) {
            showNotification("Please enter date To", "warning");
            $('#dateTo').focus();
            return false;
        }

        if (dateFrom && dateTo && new Date(dateFrom) > new Date(dateTo)) {
            showNotification("'Date From' cannot be later than 'Date To'");
            return false;
        }
    } else if ($('#byMonth').is(':checked')) {
        const salaryMonth = $('#SalaryMonth').val();
        const salaryYear = $('#SalaryYear').val();
        let currentYear = new Date().getFullYear();
        let yearRegex = /^\d{4}$/;

        if (!salaryMonth) {
            showNotification("Please select Salary Month", "warning");
            $('#SalaryMonth').focus();
            return false;
        }

        if (!salaryYear) {
            showNotification("Please enter Salary Year", "warning");
            $('#SalaryYear').focus();
            return false;
        }

        if (!yearRegex.test(salaryYear) || salaryYear < 2000) {
            showNotification("Please enter a valid Salary Year (2000 or later)", "warning");
            $('#SalaryYear').focus();
            return false;
        }
    } else {
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
        url: `/HrmPaySalaryDeduction/UploadExcelAsync`,
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        success: function (res) {
            if (res.success) {
                $("#choosefileText").text("Choose File");
                showNotification(res.message, "success");
                if (typeof loadSalaryDeductionData === 'function') {
                    loadSalaryDeductionData();
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
    $('#AutoId').val('');
    $('#Id').val('');
    let today = new Date().toISOString().split('T')[0];
    $("#dateFrom, #dateTo").val(today);
    const currentMonth = new Date().getMonth() + 1;
    const currentYear = new Date().getFullYear();
    $("#SalaryMonth").val(currentMonth);
    $("#SalaryYear").val(currentYear);
    $("#DeductionTypeId").val('');
    $("#DeductionAmount").val('');
    $("#remarks").val('');
    $("#branchSelect").val(null).trigger('change');
    $("#activityStatusSelect").val(null).trigger('change');
    selectedDeductionIds.clear();
    selectedEmployeeIds.clear();
    originalEmployeeId = null;
    $('#employee-filter-grid-body input[type="checkbox"]').prop('checked', false).prop('disabled', false);
    $('#employee-check-all').prop('checked', false).prop('disabled', false);
    $('#byDate').prop('checked', false).prop('disabled', false);
    $('#byMonth').prop('checked', true)
    toggleFields();
}

// Updated populateForm function to properly filter employee grid in edit mode
function populateForm(id) {
    $.ajax({
        url: `/HrmPaySalaryDeduction/GetById/${id}`,
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

                $('#byMonth').prop('checked', true);
                $('#byDate').prop('checked', false).prop('disabled', true); 

                toggleFields();

                $('#AutoId').val(data.autoId);
                $('#Id').val(data.id);
                $("#SalaryMonth").val(data.salaryMonth);
                $("#SalaryYear").val(data.salaryYear);
                $("#DeductionTypeId").val(data.deductionTypeId);
                $("#DeductionAmount").val(data.deductionAmount);
                $("#remarks").val(data.remarks);

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

                    // Disable the "select all" checkbox
                    $('#employee-check-all').prop('disabled', true);
                }, 500);
                // Filter employee grid to show only this employee
                

                //originalEmployeeId = String(data.employeeId);

                //selectedEmployeeIds.clear();
                //selectedEmployeeIds.add(originalEmployeeId);

                //$('#employee-filter-grid-body input[type="checkbox"]').each(function () {
                //    const empId = $(this).closest('tr').find('td:nth-child(2)').text().trim();
                //    if (empId === originalEmployeeId) {
                //        $(this).prop('checked', true).prop('disabled', true);
                //    } else {
                //        $(this).prop('checked', false).prop('disabled', true);
                //    }
                //});

                //$('#employee-check-all').prop('disabled', true);
            } catch (e) {
                console.error("Error populating form:", e);
                showNotification("Error loading record details", "error");
            }
        },
        error: function(xhr, status, error) {
            console.error("Error fetching record:", error);
            showNotification("Failed to load record details", "error");
        }
    });
}

// Add this new function to filter the employee grid
function filterEmployeeGridByEmployeeId(empId) {
    showLoading();
    
    // Clear existing filter values
    $("#branchSelect, #divisionSelect, #departmentSelect, #designationSelect").val(null).multiselect('refresh');
    
    // Set employee filter to just this employee
    $("#employeeSelect").val(empId).multiselect('refresh');
    
    // Execute filter with only this employee selected
    const filterData = {
        CompanyCodes: ['001'],
        BranchCodes: [],
        DivisionCodes: [],
        DepartmentCodes: [],
        DesignationCodes: [],
        EmployeeIDs: [empId],
        EmployeeStatuses: toArray($("#activityStatusSelect").val()),
    };

    $.ajax({
        url: `/HrmPaySalaryDeduction/getFilterEmp`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(filterData),
        success: function (res) {
            loadTableData(res);
            
            // Force the checkbox to be checked and disabled for this employee
            setTimeout(function() {
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
    const selectedIds = Array.from(selectedDeductionIds);

    if (selectedIds.length === 0) {
        showNotification("Please select record to delete", "warning");
        return;
    }

    if (!confirm(`Are you sure you want to delete ${selectedIds.length} selected salary deduction(s)?`)) {
        return;
    }

    showLoading();

    $.ajax({
        url: `/HrmPaySalaryDeduction/BulkDelete`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ AutoIds: selectedIds }),
        success: function (response) {
            selectedDeductionIds.clear();
            showNotification(response.message || "Successfully deleted items", "success");
            loadSalaryDeductionData();
            loadSalaryDeductionId();
            clearForm();
        },
        error: function (xhr, status, error) {
            console.error("Error details:", xhr.responseText);
            showNotification("Error deleting salary deduction records", "error");
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