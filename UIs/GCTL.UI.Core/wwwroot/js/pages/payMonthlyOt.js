let empDataTable = null;
let filterChangeBound = false;
let originalEmpId = null;
let selectedOtIds = new Set();
let selectedEmpIds = new Set();
let isEditMode = false;

$(document).ready(function () {
    setupLoadingOverlay();
    loadMonthsDD();
    initializeMultiselects();
    initializeEmployeeGrid();
    loadAllFilterEmp();
    loadMonthlyOtId();
    loadMonthlyOtData();
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
    const $form = $('#monthlyOt-form');
    if (!$form.length) return;

    $form.on('keydown', 'input, select, textarea, button, [tabindex]:not([tabindex="-1"])', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();

            const $focusable = $form
                .find('input:not([disabled]), select:not([disabled]), textarea:not([disabled]), button, [herf], [tabindex]:not([tabindex="-1"])')
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
    $('#Month').focus();

    $(".js-monthlyOt-dec-save").on('click', handleFormSubmission);
    $(".js-monthlyOt-dec-delete-confirm").on('click', handleBulkDelete);
    $(".js-monthlyOt-dec-clear").on('click', function () {
        clearForm();
        loadMonthlyOtId();
    });
    $("#excelUploadForm").submit(excelUpload);

    $(document).on("click", ".monthlyOt-id-link", function () {
        const id = $(this).data("id");
        if (!id) return;

        populateForm(id);
    });

    $('#monthlyOt-grid').DataTable().columns.adjust().draw();

    $('#monthlyOt-check-all').on('change', function () {
        const isChecked = $(this).is(':checked');
        $('#monthlyOt-grid-body input[type="checkbox"]').prop('checked', isChecked);

        updateSelectedMonthlyOtIds();
    });

    let today = new Date().toISOString().split('T')[0];

    $("#dateFrom, #dateTo").val(today);

    if (!$('#Year').val()) {
        const currentYear = new Date().getFullYear();
        $('#Year').val(currentYear);
    }

    $('#employee-filter-grid').on('keydown', 'input[type="checkbox"]', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            if ($('#byMonth').is(':checked')) {
                $('#Month').focus();
            } else if ($('#byDate').is(':checked')) {
                $('#dateFrom').focus();
            }
        }
    });

    $("#employee-check-all").on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            if ($('#byMonth').is(':checked')) {
                $('#Month').focus();
            } else if ($('#byDate').is(':checked')) {
                $('#dateFrom').focus();
            }
        }
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
$(document).on('change', '#monthlyOt-grid-body input[type="checkbox"]', function () {
    const id = $(this).data('id');

    if ($(this).is(':checked')) {
        selectedOtIds.add(id);
    } else {
        selectedOtIds.delete(id);
    }

    const total = $('#monthlyOt-grid-body input[type="checkbox"]').length;
    const checked = $('#monthlyOt-grid-body input[type="checkbox"]:checked').length;
    $("#monthlyOt-check-all").prop('checked', total > 0 && total === checked);
});

function updateSelectedMonthlyOtIds() {
    const currentPageCheckboxes = $('#monthlyOt-grid-body input[type="checkbox"]');

    currentPageCheckboxes.each(function () {

        const id = $(this).data('id');

        if ($(this).is(':checked')) {
            selectedOtIds.add(id);
        } else {
            selectedOtIds.delete(id);
        }
    });
}

function updateSelectedEmployeeIds() {
    const currentPageCheckboxes = $('#employee-filter-grid-body input[type="checkbox"]');
    currentPageCheckboxes.each(function () {
        const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();

        if ($(this).is(':checked')) {
            selectedEmpIds.add(employeeId);
        } else {
            selectedEmpIds.delete(employeeId);
        }
    });
}

function toggleFields() {
    if ($('#byDate').is(':checked')) {

        $('#dateFrom, #dateTo').prop('disabled', false).attr('tabindex', function (index) {
            return index + 1;
        });


        $('#Month, #Year').prop('disabled', true).attr('tabindex', -1);


        $('#dateFromLabel').html('Date From:<span style="color:red;">*</span>');
        $('#dateToLabel').html('Date To:<span style="color:red;">*</span>');
        $('#MonthLabel').html('Month:');
        $('#YearLabel').html('Year:');

    } else if ($('#byMonth').is(':checked')) {

        $('#dateFrom, #dateTo').prop('disabled', true).attr('tabindex', -1);


        $('#Month').prop('disabled', false).attr('tabindex', 1);
        $('#Year').prop('disabled', false).attr('tabindex', 2);


        $('#dateFromLabel').html('Date From:');
        $('#dateToLabel').html('Date To:');
        $('#MonthLabel').html('Month:<span style="color:red;">*</span>');
        $('#YearLabel').html('Year:<span style="color:red;">*</span>');


        if (!$('#Month').val()) {
            const currentMonth = new Date().getMonth() + 1;
            $('#Month').val(currentMonth).trigger('change');
        }

        if (!$('#Year').val()) {
            const currentYear = new Date().getFullYear();
            $('#Year').val(currentYear);
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
        url: `/HrmPayMonthlyOtEntry/getFilterEmp`,
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

    if ($.fn.DataTable.isDataTable('#employee-filter-grid') && empDataTable !== null) {
        empDataTable.destroy();
        empDataTable = null;
    }

    var tableBody = $("#employee-filter-grid-body");
    tableBody.empty();

    $.each(tableDataItem, function (index, employee) {
        var row = $('<tr>');

        const isOriginalEmployee = isEditMode && String(employee.employeeId) === String(originalEmpId);
        const checkboxDisabled = isEditMode ? 'disabled' : '';
        const checkboxChecked = isOriginalEmployee ? 'checked' : '';

        row.append(`<td class="text-center"><input type="checkbox" style="padding-left:0; padding-right:0" class="empSelect" ${checkboxDisabled} ${checkboxChecked} /></td>`);
        row.append('<td class="p-2 text-center">' + employee.employeeId + '</td>');
        row.append('<td class="p-2 text-start">' + employee.employeeName + '</td>');
        row.append('<td class="p-2 text-start">' + employee.designationName + '</td>');
        row.append('<td class="p-2 text-center">' + employee.branchName + '</td>');
        row.append('<td class="p-2 text-center">' + employee.departmentName + '</td>');
        row.append('<td class="p-2 text-center">' + employee.employeeTypeName + '</td>');
        row.append('<td class="p-2 text-center">' + employee.joiningDate + '</td>');
        row.append('<td class="p-2 text-center">' + employee.employeeStatus + '</td>');

        tableBody.append(row);
    });

    initializeDataTable();

    if (isEditMode) {
        $('#employee-check-all').prop('disabled', true)
    }
}

function initializeDataTable() {
    empDataTable = $("#employee-filter-grid").DataTable({
        paging: true,
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000,"All"]],
        lengthChange: true,
       // scrollY: "400px",
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
                empDataTable.search(this.value).draw();
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
                $(this).prop('checked', selectedEmpIds.has(employeeId));
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
        if (empDataTable !== null) {
            empDataTable.destroy();
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
    $("#Year").val(currentYear);
}

function loadMonthsDD() {
    //debugger;
    const currentMonth = new Date().getMonth() + 1;

    $.ajax({
        url: "/HrmPayMonthlyOtEntry/getMonthDD",
        type: "GET",
        dataType: "json",
        success: function (data) {

            console.log(data);
            var dropdown = $("#Month");

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

function loadMonthlyOtId() {
    $.ajax({
        url: "/HrmPayMonthlyOtEntry/GenerateNewId",
        type: "GET",
        dataType: "json",
        success: function (data) {
            if (data) {
                $('#MonthlyOtid').val(data);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error fetching Salary deduction ID:", error);
        }
    })
}


function loadMonthlyOtData() {
    showLoading();
    displayMonthlyOtTable();
    hideLoading();
}

function displayMonthlyOtTable() {
    if ($.fn.DataTable.isDataTable("#monthlyOt-grid")) {
        $("#monthlyOt-grid").DataTable().clear().destroy();
    }

    const tableBody = $("#monthlyOt-grid-body");
    tableBody.empty();

    $('#monthlyOt-grid').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/HrmPayMonthlyOtEntry/GetPaginatedMonthlyOt',
            type: 'POST',
            // success: function (data) { console.log(data) }
        },
        columns: [
            {
                data: null,
                orderable: false,
                className: 'text-center',
                render: function (data, type, row) {
                    return `<input type="checkbox" width="1%" style="padding:0;" data-id="${row.tc}"/>`;
                }
            },
            {
                data: 'monthlyOtid',
                className: 'text-center',
                render: function (data, type, row) {
                    return `<a href="#monthlyOt-form" class="monthlyOt-id-link" data-id="${row.tc}">${data}</a>`;
                }
            },
            { data: 'employeeId', className: 'text-center' },
            { data: 'employeeName', className: 'text-left' },
            { data: 'designation', className: 'text-left' },
            { data: 'ot', className: 'text-center' },
            { data: 'otamount', className: 'text-center' },
            { data: 'month', className: 'text-center' },
            { data: 'year', className: 'text-center' },
            { data: 'remarks', className: 'text-center' },
            {
                data: 'ldate',
                className: 'text-center',
                render: function (data, type, row) {
                    if (!data) return '';
                    const date = new Date(data);
                    const day = String(date.getDate()).padStart(2, '0');
                    const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are zero-based
                    const year = date.getFullYear();
                    return `${day}/${month}/${year}`;
                }
            },
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
            $('#monthlyOt-grid-body input[type="checkbox"]').each(function () {
                const id = $(this).data('id');
                $(this).prop('checked', selectedOtIds.has(id));
            });

            const total = $('#monthlyOt-grid-body input[type="checkbox"]').length;
            const checked = $('#monthlyOt-grid-body input[type="checkbox"]:checked').length;
            $("#monthlyOt-check-all").prop('checked', total > 0 && total === checked);
        }
    });
}

function handleFormSubmission() {
    if (!validateForm()) {
        return;
    }

    let dataToSend;
    const id = $('#MonthlyOtid').val().trim();
    const url = `/HrmPayMonthlyOtEntry/SaveMonthlyOt`;
    const type = "POST";
    const EmployeeIds = Array.from(selectedEmpIds);

    dataToSend = {
        Tc: $('#Tc').val() || 0,
        MonthlyOtid: $('#MonthlyOtid').val(),
        Ot: parseFloat($('#Ot').val()),
        Otamount: parseFloat($('#Amount').val())||0,
        Remarks: $(`#remarks`).val(),
        CompanyCode: $("#companySelect").val(),
        EmployeeIds: EmployeeIds
    }

    if ($('#byDate').is(':checked')) {
        dataToSend.DateForm = $('#dateFrom').val();
        dataToSend.DateTo = $('#dateTo').val() || null;
    } else if ($('#byMonth').is(':checked')) {
        dataToSend.Month = $('#Month').val();
        dataToSend.Year = $('#Year').val();
    }

    showLoading();
    console.log(JSON.stringify(dataToSend));

    $.ajax({
        url: url,
        type: type,
        data: JSON.stringify(dataToSend),
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            showNotification("Data Saved Successfully.", "success");
            loadMonthlyOtData();
            clearForm();
            loadMonthlyOtId();
            $('#monthlyOtForm')[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
            $('#Month').focus();
        },
        error: function (xhr, status, error) {
            showNotification(`Error saving data`, "error");
            console.log(`Error saving data `, error);
        },
        complete: hideLoading
    })
}

function validateForm() {
    if (selectedEmpIds.size === 0) {
        showNotification("Please select at least one employee", "warning");
        $('#employee-filter-grid-body').focus();
        return false;
    }

    if (!$("#Ot").val()) {
        showNotification("Please enter Ot", "warning");
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
        const month = $('#Month').val();
        const year = $('#Year').val();
        //let currentYear = new Date().getFullYear();
        let yearRegex = /^\d{4}$/;

        if (!month) {
            showNotification("Please enter Salary Year", "warning");
            $('#Year').focus();
            return false;
        }

        if (!yearRegex.test(year) || year < 2000) {
            showNotification("Please enter a valid Salary Year", "warning");
            $('#Year').focus();
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
        url: `/HrmPayMonthlyOtEntry/UploadExcelAsync`,
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        success: function (res) {
            if (res.success) {
                $("#choosefileText").text("Choose File");
                showNotification(res.message, "success");
                if (typeof loadSalaryDeductionData === 'function') {
                    loadMonthlyOtData();
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
    $('#MonthlyOtid').val('');
    let today = new Date().toISOString().split('T')[0];
    $("#dateFrom, #dateTo").val(today);
    const currentMonth = new Date().getMonth() + 1;
    const currentYear = new Date().getFullYear();
    $("#Month").val(currentMonth);
    $("#Year").val(currentYear);
    $("#Ot").val('');
    $("#Amount").val('');
    $("#remarks").val('');
    $("#branchSelect").val(null).trigger('change');
    $("#activityStatusSelect").val(null).trigger('change');
    selectedEmpIds.clear();
    selectedOtIds.clear();
    originalEmpId = null;
    $('#employee-filter-grid-body input[type="checkbox"]').prop('checked', false).prop('disabled', false);
    $('#employee-check-all').prop('checked', false).prop('disabled', false);
    $('#byDate').prop('checked', false).prop('disabled', false);
    $('#byMonth').prop('checked', true)
    toggleFields();
}

function populateForm(id) {
    $.ajax({
        url: `/HrmPayMonthlyOtEntry/GetById/${id}`,
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

                $('#Tc').val(data.tc);
                $('#MonthlyOtid').val(data.monthlyOtid);
                $('#Month').val(data.month);
                $('#Year').val(data.year);
                $('#Ot').val(data.ot);
                $('#Amount').val(data.otamount);
                $('#remarks').val(data.remarks);

                originalEmpId = String(data.employeeId);

                selectedEmpIds.clear();
                selectedEmpIds.add(origin);

                setTimeout(() => {
                    filterEmployeeGridByEmployeeId(originalEmpId);
                    $('#employee-filter-grid-body input[type="checkbox"]').each(function () {
                        const empId = $(this).closest('tr').find('td:nth-child(2)').text().trim();
                        if (empId === originalEmpId) {
                            $(this).prop('checked', true).prop('disabled', true);
                        } else {
                            $(this).prop('checked', true).prop('disabled', true);
                        }
                    });

                    $('employee-check-all').prop('disabled', true);
                }, 300);

            } catch (e) {
                console.error("Error populating form:", e);
                showNotification("Error loading record details", "error");
            }
        }, error: function (xhr, status, error) {
            console.error("Error fetching record:", error);
            showNotification("Failed to load record details", "error")
        }
    })
}

function filterEmployeeGridByEmployeeId(empId) {
    showLoading();

    $("#branchSelect, #divisionSelect, #departmentSelect, #designationSelect").val(null).multiselect('refresh');

    $("#employeeSelect").val(empId).multiselect('refresh');

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
        url: `/HrmPayMonthlyOtEntry/getFilterEmp`,
        type: "POST",
        contentType: "application/json",
        data:JSON.stringify(filterData),
        success: function (res) {
            loadTableData(res);
            //hideLoading();
            setTimeout(function () {
                $('#employee-filter-grid-body input[type="checkbox"]').prop('checked', true).prop('disabled', true);
            }, 100);
        },
        complete: hideLoading,
        error: function (xhr, status, error) {
            console.error("Error filtering employee:", error);
            hideLoading();
            showNotification("Error filtering employee data", "error");
        }
    });
}

function handleBulkDelete() {
    const selectedIds = Array.from(selectedOtIds);

    if (selectedIds.length === 0) {
        showNotification("Please select record to delete", "warning");
        return;
    }

    if (!confirm(`Are you sure you want to delete ${selectedIds.length} selected monthly OT(s)?`)) {
        return;
    }

    showLoading();

    $.ajax({
        url: `/HrmPayMonthlyOtEntry/BulkDelete`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Tcs: selectedIds }),
        success: function (response) {
            selectedOtIds.clear();
            showNotification(response.message || "Successfully deleted", "success");
            loadMonthlyOtData();
            loadMonthlyOtId();
            clearForm();
        },
        error: function (xhr, status, error) {
            console.error("Error details:", xhr.responseText);
            showNotification("Error deleting Monthly OT records", "error");
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