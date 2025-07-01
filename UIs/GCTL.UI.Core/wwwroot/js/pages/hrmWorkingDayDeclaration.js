let employeeDataTable = null;
let filterChangeBound = false;
let originalEmployeeId = null;
let selectedWorkingDayIds = new Set();
let selectedEmployeeIds = new Set();
let isEditMode = false;
var selectedDates = [];
let flatpickrInstance = null; 

$(document).ready(function () {
    setupLoadingOverlay();
    initializeMultiselects();
    initializeEmployeeGrid();
    loadAllFilterEmp();
    loadWorkingDayData();
    initializeEventHandlers();
    setupEnterKeyNavigation();

    initializeFlatpickr(isEditMode);
});

function initializeFlatpickr(isEditMode) {

    if (flatpickrInstance) {
        flatpickrInstance.destroy();
        flatpickrInstance = null;
    }

    $("#WorkingDayDate").val('');
    selectedDates = [];

    flatpickrInstance = flatpickr("#WorkingDayDate", {
        mode: isEditMode ? "single" : "multiple",
        dateFormat: "d-m-Y",
        allowInput: false,
        clickOpens: true,
        onChange: function (dates, dateStr) {
            if (isEditMode) {
                selectedDates = dateStr ? [dateStr] : [];
            } else {
                selectedDates = dateStr ? dateStr.split(",") : [];
            }

            updateWorkingDayInput(); // Fixed function name
            console.log({ selectedDates, isEditMode });
        }
    });

    $("#WorkingDayDate").on('keydown keypress keyup', function (e) {
        if (e.keyCode === 9 || e.keyCode === 27 ||
            (e.keyCode >= 35 && e.keyCode <= 39)) {
            return true;
        }
        e.preventDefault();
        return false;
    });
}

function updateWorkingDayInput() {
    const input = $("#WorkingDayDate");

    if (selectedDates.length > 0) {
        let tooltipHTML = '';
        let shortDisplayText = '';
        const datesPerLine = 3;

        for (let i = 0; i < selectedDates.length; i += datesPerLine) {
            const lineGroup = selectedDates.slice(i, i + datesPerLine);
            tooltipHTML += '<div>' + lineGroup.join(', ') + '</div>';
        }

        if (selectedDates.length <= 3) {
            shortDisplayText = selectedDates.join(', ');
        } else {
            shortDisplayText = selectedDates.slice(0, 2).join(', ') + ` ... (+${selectedDates.length - 2} more)`;
        }

        input.val(shortDisplayText);

        if (typeof $.fn.tooltip === 'function' && input.data('bs.tooltip')) {
            input.tooltip('dispose');
        }
        $('.custom-tooltip').remove();
        input.off('.customTooltip');

        if (typeof $.fn.tooltip === 'function') {
            input.tooltip({
                title: tooltipHTML,
                placement: 'top',
                trigger: 'hover focus',
                html: true,
                template: '<div class="tooltip working-day-tooltip" role="tooltip"><div class="arrow"></div><div class="tooltip-inner"></div></div>',
                delay: { show: 300, hide: 100 }
            });
        } else {

        }
    } else {
        input.val('');

        if (typeof $.fn.tooltip === 'function' && input.data('bs.tooltip')) {
            input.tooltip('dispose');
        }

        $('.custom-tooltip').remove();
        input.off('.customTooltip');
        input.removeAttr('title');
    }
}

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
    const $form = $('#workingday-declaration-form');
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

    $("#js-workingday-declaration-dec-clear").on('click', function () {
        clearForm();
    });

    $(".js-workingday-declaration-dec-save").on('click', handleFormSubmission);
    $(".js-workingday-declaration-dec-delete-confirm").on('click', handleBulkDelete);

    $(document).on("click", ".workingday-declaration-id-link", function () {
        const id = $(this).data("id");
        if (!id) return;

        populateForm(id);
    });

    $('#workingday-declaration-grid').DataTable().columns.adjust().draw();

    $("#workingday-declaration-check-all").on('change', function () {
        const isChecked = $(this).is(':checked');
        $('#workingday-declaration-grid-body input[type="checkbox"]').prop('checked', isChecked);
        updateSelectedWorkingDayIds();
    });

    $("#workingday-declaration-check-all").on('change', function () {
        const isChecked = $(this).is(':checked');
        $('#workingday-declaration-grid-body input[type="checkbox"]').prop('checked', isChecked);

        if (isChecked) {
            $('#workingday-declaration-grid-body input[type="checkbox"]').each(function () {
                selectedWorkingDayIds.add($(this).data('id'));
            });
        } else {
            $('#workingday-declaration-grid-body input[type="checkbox"]').each(function () {
                selectedWorkingDayIds.delete($(this).data('id'));
            });
        }
    });

    $(document).on('change', '#workingday-declaration-grid-body input[type="checkbox"]', function () {
        const id = $(this).data('id');

        if ($(this).is(':checked')) {
            selectedWorkingDayIds.add(id); 
        } else {
            selectedWorkingDayIds.delete(id);
        }

        const total = $('#workingday-declaration-grid-body input[type="checkbox"]').length;
        const checked = $('#workingday-declaration-grid-body input[type="checkbox"]:checked').length;
        $("#workingday-declaration-check-all").prop('checked', total > 0 && total === checked);
    });

    $('#employee-filter-grid').on('keydown', 'input[type="checkbox"]', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#WorkingDayDate').focus();
        }
    });

    $("#employee-check-all").on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#WorkingDayDate').focus();
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

function updateSelectedWorkingDayIds() {
    const currentPageCheckboxes = $('#workingday-declaration-grid-body input[type="checkbox"]');

    currentPageCheckboxes.each(function () {

        const id = $(this).data('id');

        if ($(this).is(':checked')) {
            selectedWorkingDayIds.add(id);
        } else {
            selectedWorkingDayIds.delete(id);
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
        url: `/HrmWorkingDayDeclaration/getFilterEmp`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(filterData),
        success: function (res) {
            const data = res.lookupData;
            console.log(res);
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
        //"#activityStatusSelect": ["#divisionSelect", "#departmentSelect", "#designationSelect", "#employeeSelect"],
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

        row.append(`<td class="text-center p0" width="1%"><input type="checkbox" width="1%" class="empSelect" ${checkboxDisabled} ${checkboxChecked}/></td>`);
        row.append('<td class="text-center p-2">' + employee.employeeId + '</td>');
        row.append('<td class="p-2">' + employee.employeeName + '</td>');
        row.append('<td class="p-2">' + employee.designationName + '</td>');
        row.append('<td class="text-center p-2">' + employee.departmentName + '</td>');
        row.append('<td class="text-center p-2">' + employee.branchName + '</td>');
        row.append('<td class="text-center p-2">' + employee.employeeTypeName + '</td>');
        row.append('<td class="text-center p-2">' + employee.joiningDate + '</td>');
        row.append('<td class="text-center p-2">' + employee.employeeStatus + '</td>');

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
        //selector.closest('div').css('margin', '1rem');
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

function loadWorkingDayData() {
    showLoading();
    displayWorkingDateTable();
    //debugger;
    hideLoading();
}

function displayWorkingDateTable() {
    if ($.fn.DataTable.isDataTable("#workingday-declaration-grid")) {
        $("#workingday-declaration-grid").DataTable().clear().destroy();
    }

    const tableBody = $("#workingday-declaration-grid-body");
    tableBody.empty();

    $('#workingday-declaration-grid').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/HrmWorkingDayDeclaration/GetPaginatedData',
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
                    return `<input class="workingDaySelect" type="checkbox" width="1%" style="padding: 0;" data-id="${row.tc}" />`;
                }
            },
            {
                data: 'workingDayCode',
                className: 'text-center',
                render: function (data, type, row) {
                    return `<a href="#workingday-declaration-form" class="workingday-declaration-id-link" data-id="${row.tc}">${data}</a>`;
                }
            },
            { data: 'employeeId', className: 'text-center' },
            { data: 'employeeName', className: 'text-left' },
            { data: 'designation', className: 'text-left' },
            { data: 'department', className: 'text-left' },
            {
                data: 'workingDayDate',
                className: 'text-center',
                render: function (data, type, row) {
                    if (!data) return '';
                    const date = new Date(data);
                    return date.toLocaleDateString("en-GB");
                }
            },
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
            //processing: "Loading data..."
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
            $('#workingday-declaration-grid-body input[type="checkbox"]').each(function () {
                const id = $(this).data('id');
                $(this).prop('checked', selectedWorkingDayIds.has(id));
            });

            const total = $('#workingday-declaration-grid-body input[type="checkbox"]').length;
            const checked = $('#workingday-declaration-grid-body input[type="checkbox"]:checked').length;
            $("#workingday-declaration-check-all").prop('checked', total > 0 && total === checked);
        }
    });
}

function handleFormSubmission() {
    if (!validateForm()) {
        return;
    }
      
    const url = `/HrmWorkingDayDeclaration/Save`;
    const type = "POST";
    const EmployeeIds = Array.from(selectedEmployeeIds);

    const dataToSend = {
        Tc: $('#Tc').val() || 0,
        WorkingDayCode: '0',
        Remarks: $(`#Remarks`).val(),
        WorkingDayDates: selectedDates,
        CompanyCode: $("#companySelect").val(),
        EmployeeIds: EmployeeIds
    }

    showLoading();
    console.log(JSON.stringify(dataToSend))
    $.ajax({
        url: url,
        type: type,
        data: JSON.stringify(dataToSend),
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            console.log(response);

            if (response.success) {
                showNotification(response.message, "success");
            } else {
                showNotification(response.message, "error");
            }

            loadWorkingDayData();
            clearForm();
            $('#workingday-declaration-form')[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
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

    if (selectedDates.length===0) {
        showNotification("Please select a working date", "info");
        $('#WorkingDayDate').focus();
        return false;
    }

    return true;
}

function clearForm() {
    isEditMode = false;
    $('#Tc').val('');

    const input = $("#WorkingDayDate");
    input.val('');

    if (typeof $.fn.tooltip === 'function' && input.data('bs.tooltip')) {
        input.tooltip('dispose');
    }

    $('.custom-tooltip').remove();
    input.off('.customTooltip');
    input.removeAttr('title');


    $('#WorkingDayDate').val('')
    $("#Remarks").val('');
    $("#branchSelect").val(null).trigger('change');
    $("#activityStatusSelect").val(null).trigger('change');
    selectedWorkingDayIds.clear();
    selectedEmployeeIds.clear();
    originalEmployeeId = null;
    $('#employee-filter-grid-body input[type="checkbox"]').prop('checked', false).prop('disabled', false);
    $('#employee-check-all').prop('checked', false).prop('disabled', false);
    selectedDates = [];
    initializeFlatpickr(isEditMode);
}

function populateForm(id) {
    $.ajax({
        url: `/HrmWorkingDayDeclaration/GetById/${id}`,
        type: "GET",
        success: function (response) {
            console.log(response);
            if (!response || !response.data) {
                showNotification("Invalid data format received", "error");
                return;
            }

            try {
                const data = response.data;
                clearForm();

                isEditMode = true;
                initializeFlatpickr(isEditMode);
                $('#Tc').val(data.tc);
                if (data.workingDayDate) {
                    let dateToSet;

                    if (data.workingDayDate.includes('-') && data.workingDayDate.split('-')[0].length <= 2) {
                        dateToSet = data.workingDayDate;
                    } else {
                        const date = new Date(data.workingDayDate);
                        const day = String(date.getDate()).padStart(2, '0');
                        const month = String(date.getMonth() + 1).padStart(2, '0');
                        const year = date.getFullYear();
                        dateToSet = `${day}-${month}-${year}`;
                    }

                    if (flatpickrInstance) {
                        flatpickrInstance.setDate(dateToSet, true, "d-m-Y");
                    }

                    selectedDates = [dateToSet];
                    console.log({ selectedDates });
                };
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
        CompanyCodes: [],
        BranchCodes: [],
        DivisionCodes: [],
        DepartmentCodes: [],
        DesignationCodes: [],
        EmployeeIDs: [empId],
        EmployeeStatuses: toArray($("#activityStatusSelect").val()),
    };

    $.ajax({
        url: `/HrmWorkingDayDeclaration/getFilterEmp`,
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
    const selectedIds = Array.from(selectedWorkingDayIds);

    if (selectedIds.length === 0) {
        showNotification("Please select record to delete", "warning");
        return;
    }

    if (!confirm(`Are you sure you want to delete ${selectedIds.length} selected working day(s)?`)) {
        return;
    }

    showLoading();

    $.ajax({
        url: `/HrmWorkingDayDeclaration/BulkDelete`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Tcs: selectedIds }),
        success: function (response) {
            selectedWorkingDayIds.clear();
            showNotification(response.message || "Successfully deleted items", "success");
            loadWorkingDayData();
            //loadOtherBenefitId();
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