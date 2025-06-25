let employeeDataTable = null;
let filterChangeBound = false;
let selectedEmployeeIds = new Set();
let isEditMode = false;
var selectedFromDates;
var selectedToDates;
let flatpickrFromInstance = null;
let flatpickrToInstance = null;

$(document).ready(function () {
    setupLoadingOverlay();
    initializeMultiselects();
    initializeEmpGrid();
    loadAllFilterEmp();
    initializeEventHandlers();

    initializeFlatpickr();
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
function initializeFlatpickr() {
    if (flatpickrFromInstance) {
        flatpickrFromInstance.destroy();
        flatpickrFromInstance = null;
    }

    if (flatpickrToInstance) {
        flatpickrToInstance.destroy();
        flatpickrToInstance = null;
    }

    $("#joinDateFromSelect").val('');
    $("#joinDateToSelect").val('');

    flatpickrFromInstance = flatpickr("#joinDateFromSelect", {
        mode: "single",
        dateFormat: "d-m-Y",
        allowInput: false,
        clickOpens: true,
        onChange: function (dates, dateStr) {
            console.log({ fromDate: dateStr });
        }
    });

    flatpickrToInstance = flatpickr("#joinDateToSelect", {
        mode: "single",
        dateFormat: "d-m-Y",
        allowInput: false,
        clickOpens: true,
        onChange: function (dates, dateStr) {
            console.log({ toDate: dateStr });
        }
    });

    $("#joinDateFromSelect").on('keydown keypress keyup', function (e) {
        if (e.keyCode === 9 || e.keyCode === 27 ||
            (e.keyCode >= 35 && e.keyCode <= 39)) {
            return true;
        }
        e.preventDefault();
        return false;
    });

    $("#joinDateToSelect").on('keydown keypress keyup', function (e) {
        if (e.keyCode === 9 || e.keyCode === 27 ||
            (e.keyCode >= 35 && e.keyCode <= 39)) {
            return true;
        }
        e.preventDefault();
        return false;
    });
}


function initializeEventHandlers() {
    $("#companySelect, #branchSelect, #departmentSelect, #designationSelect, #empTypeSelect, #empNatureSelect, JoinDateFromSelect, #JoinDateToSelect, #activityStatusSelect")
        .on("change", function () {
            loadAllFilterEmp();
        });

    $("#js-workingday-declaration-dec-clear").on('click', function () {
        clearForm();
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
function toArray(value) {
    if (!value) return [];
    if (Array.isArray(value)) return value;
    return [value];
}

function getAllFilterVal() {
    const filterData = {
        CompanyCodes: toArray($("#companySelect").val()),
        BranchCodes: toArray($("#branchSelect").val()),
        DepartmentCodes: toArray($("#departmentSelect").val()),
        DesignationCodes: toArray($("#designationSelect").val()),
        EmployeeTypes: toArray($("#empTypeSelect").val()),
        EmployeeNatureCodes: toArray($("#empNatureSelect").val()),
        JoiningDateFrom: $("#JoinDateFromSelect").val(),
        JoiningDateTO: $("#JoinDateToSelect").val(),
        EmployeeStatuses: toArray($("#activityStatusSelect").val()),
    };
    return filterData;
}


function loadAllFilterEmp() {
    showLoading();
    const filterData = getAllFilterVal();

    const requestData = {
        draw: 1,
        start: 0,
        length: 1000,
        searchValue: "",
        sortDirection: "asc",
        model:filterData
    }

    $.ajax({
        url: `/HrmEmployeeSalaryInfoEntry/getFilterEmp`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestData),
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
            if (data.departments?.length) {
                populateSelect("#departmentSelect", data.departments);
            }
            if (data.designations?.length) {
                populateSelect("#designationSelect", data.designations);
            }
            if (data.employeeType?.length) {
                populateSelect("#empTypeSelect", data.employeeType);
            }
            if (data.employmentNature?.length) {
                populateSelect("#empNatureSelect", data.employmentNature);
            }
            //if (data.employeeStatuses?.length) {
            //    populateSelect("#activityStatusSelect", data.employeeStatuses);
            //}

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
        $("#companySelect, #branchSelect, #departmentSelect, #designationSelect, #empTypeSelect, #empNatureSelect, JoinDateFromSelect, #JoinDateToSelect, #activityStatusSelect")
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
        row.append('<td class="text-center">' + employee.employeeId + '</td>');
        row.append('<td class="text-center">' + employee.payId + '</td>');
        row.append('<td class="text-center">' + employee.employeeName + '</td>');
        row.append('<td class="text-center">' + employee.designationName + '</td>');
        row.append('<td class="text-center">' + employee.departmentName + '</td>');
        row.append('<td class="text-center">' + employee.employeeTypeName + '</td>');
        row.append('<td class="text-center">' + employee.employmentNature + '</td>');
        row.append('<td class="text-center">' + employee.joiningDate + '</td>');
        row.append('<td class="text-center">' + employee.separationDate + '</td>');
        row.append('<td class="text-center">' + employee.employeeStatus + '</td>');
        row.append('<td class="text-center">' + employee.lastIncDate + '</td>');
        row.append('<td class="text-center">' + employee.grossSalary + '</td>');
        row.append('<td class="text-center">' + employee.disbursementMethodId + '</td>');


        tableBody.append(row);
    });

    initializeDataTable();
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
function initializeEmpGrid() {
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
        empTypeSelect: 'Select Emp',
        departmentSelect: 'Select Department',
        designationSelect: 'Select Designation',
        empNatureSelect: 'Select Employee',
        activityStatusSelect: 'Select Status',
        JoinDateFromSelect: 'Select Status',
        JoinDateToSelect: 'Select Status'
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
