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
    loadMethodDD();
   // initializeFlatpickr();
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

//function initializeFlatpickr() {
//    if (flatpickrFromInstance) {
//        flatpickrFromInstance.destroy();
//        flatpickrFromInstance = null;
//    }

//    if (flatpickrToInstance) {
//        flatpickrToInstance.destroy();
//        flatpickrToInstance = null;
//    }

//    $("#joinDateFromSelect").val('');
//    $("#joinDateToSelect").val('');

//    flatpickrFromInstance = flatpickr("#joinDateFromSelect", {
//        mode: "single",
//        dateFormat: "d-m-Y",
//        allowInput: false,
//        clickOpens: true,
//        onChange: function (dates, dateStr) {
//            console.log({ fromDate: dateStr });
//        }
//    });

//    flatpickrToInstance = flatpickr("#joinDateToSelect", {
//        mode: "single",
//        dateFormat: "d-m-Y",
//        allowInput: false,
//        clickOpens: true,
//        onChange: function (dates, dateStr) {
//            console.log({ toDate: dateStr });
//        }
//    });

//    $("#joinDateFromSelect").on('keydown keypress keyup', function (e) {
//        if (e.keyCode === 9 || e.keyCode === 27 ||
//            (e.keyCode >= 35 && e.keyCode <= 39)) {
//            return true;
//        }
//        e.preventDefault();
//        return false;
//    });

//    $("#joinDateToSelect").on('keydown keypress keyup', function (e) {
//        if (e.keyCode === 9 || e.keyCode === 27 ||
//            (e.keyCode >= 35 && e.keyCode <= 39)) {
//            return true;
//        }
//        e.preventDefault();
//        return false;
//    });
//}


function initializeEventHandlers() {
    $("#companySelect, #branchSelect, #departmentSelect, #designationSelect, #empTypeSelect, #empNatureSelect, #joinDateFromSelect, #joinDateToSelect, #activityStatusSelect")
        .on("change", function () {
            loadAllFilterEmp();
        });

    $(".js-empSalary-info-dec-clear").on('click', function () {
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

    $("#excelUploadForm").submit(excelUpload);

    $(".js-empSalary-info-dec-save").on('click', handleFormSubmission);
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
function clearForm() {
    $("#branchSelect").val(null).trigger('change');
    $("#activityStatusSelect").val(null).trigger('change');
    $("#joinDateFromSelect").val(null).trigger('change');
    $("#joinDateToSelect").val(null).trigger('change');
    selectedEmployeeIds.clear();
}

function getAllFilterVal() {
    const formatDate = (val) => {
        const date = val ? new Date(val) : null;
        return date ? date.toISOString().split('T')[0] : null;
    };

    const filterData = {
        CompanyCodes: toArray($("#companySelect").val()),
        BranchCodes: toArray($("#branchSelect").val()),
        DepartmentCodes: toArray($("#departmentSelect").val()),
        DesignationCodes: toArray($("#designationSelect").val()),
        EmployeeTypes: toArray($("#empTypeSelect").val()),
        EmployeeNatureCodes: toArray($("#empNatureSelect").val()),
        JoiningDateFrom: formatDate($("#joinDateFromSelect").val()),
        JoiningDateTO: formatDate($("#joinDateToSelect").val()),
        EmployeeStatuses: toArray($("#activityStatusSelect").val()),
    };
    console.log(filterData);
    return filterData;
}

function loadMethodDD() {
    $.ajax({
        url: `/HrmEmployeeSalaryInfoEntry/getMethodDD`,
        type: "GET",
        dataType: "json",
        success: function (data) {
            //console.log(data);
            $(".disbursement-dd").each(function () {
                var $select = $(this);
                var currentVal = $select.attr('value') || $select.closest("tr").data("disbursement-method-id");
                $select.empty();
                $select.append('<option value="">Select Method</option>');
                $.each(data, function (i, item) {
                    var selected = currentVal === item.disbursementMethodId ? 'selected' : '';
                    $select.append(
                        '<option value="' + item.disbursementMethodId + '" ' + selected + '>' +
                        item.disbursementMethod + '</option>'
                    );
                });

            });
        },
        error: function (xhr, status, error) {
            console.error('Error loading method dropdown:', error);
        }
    })
}

function loadAllFilterEmp() {
    showLoading();
    const filterData = getAllFilterVal();

    $.ajax({
        url: `/HrmEmployeeSalaryInfoEntry/getFilterEmp`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(filterData),
        success: function (res) {
            const data = res.lookupData;
            console.log(res);
            //debugger;
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
            //  }

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
        "#activityStatusSelect": ["#divisionSelect", "#departmentSelect", "#designationSelect", "#empTypeSelect", "#empNatureSelect"],
        "#branchSelect": ["#divisionSelect", "#departmentSelect", "#designationSelect", "#empTypeSelect","#empNatureSelect"],
        "#divisionSelect": ["#departmentSelect", "#designationSelect", "#employeeSelect", "#empTypeSelect", "#empNatureSelect"],
        "#departmentSelect": ["#designationSelect", "#employeeSelect", "#empTypeSelect", "#empNatureSelect"],
        "#designationSelect": ["#employeeSelect", "#empTypeSelect", "#empNatureSelect"]
    };

    Object.entries(clearMap).forEach(([parent, children]) => {
        $(parent).off("change.clearDropdowns").on("change.clearDropdowns", function () {

            children.forEach(child => $(child).empty().multiselect('rebuild'));
        });
    });
}

function bindFilterChangeOnce() {
    if (!filterChangeBound) {
        $("#companySelect, #branchSelect, #departmentSelect, #designationSelect, #empTypeSelect, #empNatureSelect, #joinDateFromSelect, #joinDateToSelect, #activityStatusSelect")
            .on("change", function () {
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

    var methodSelect = '<select class="form-control disbursement-dd text-center" data-id="' + /*employee.id */+ '">';
    //methodSelect += '<option disabled value="">Loading...</option></select>';

    $.each(tableDataItem, function (index, employee) {
        var row = $('<tr>');

        const isOriginalEmployee = isEditMode && String(employee.employeeId) === String(originalEmployeeId);
        const checkboxDisabled = isEditMode ? 'disabled' : '';
        const checkboxChecked = isOriginalEmployee ? 'checked' : '';

        row.append(`<td class="text-center p-0" width="1%"><input type="checkbox" width="1%" class="empSelect" ${checkboxDisabled} ${checkboxChecked}/></td>`);
        row.append('<td class="text-center p-1">' + employee.employeeId + '</td>');
        row.append('<td class="text-center p-1">' + employee.payId + '</td>');
        row.append('<td class="text-center p-1">' + employee.employeeName + '</td>');
        row.append('<td class="text-center p-1">' + employee.designationName + '</td>');
        row.append('<td class="text-center p-1">' + employee.departmentName + '</td>');
        row.append('<td class="text-center p-1">' + employee.employeeTypeName + '</td>');
        row.append('<td class="text-center p-1">' + employee.employmentNature + '</td>');
        row.append('<td class="text-center p-1">' + employee.joiningDate + '</td>');
        row.append('<td class="text-center p-0">' + employee.separationDate + '</td>');
        row.append('<td class="text-center p-1">' + employee.employeeStatus + '</td>');
        row.append('<td class="text-center p-1">' + employee.lastIncDate + '</td>');
        row.append('<td class="text-center p-1"><input type="text" value="' + employee.grossSalary + '" class="form-control text-center" /></td>');

        var selectHtml = '<td class="text-center p-1">' +
            '<select class="form-control disbursement-dd text-center" ' +
            'data-employee-id="' + employee.employeeId + '" ' +
            'value="' + (employee.disbursementMethodId || '') + '">' +
            '<option value="">Select Method</option>' +
            '</select></td>';

        row.append(selectHtml);

        tableBody.append(row);
    });

    initializeDataTable();
    loadMethodDD();
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
                targets: [0,12,13],
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

           // $('#employee-filter-grid_wrapper .dataTables_filter').hide();
        },
        drawCallback: function () {
            $('#employee-filter-grid-body input[type="checkbox"]').each(function () {
                const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();
                $(this).prop('checked', selectedEmployeeIds.has(employeeId));
            });

            updateSelectAllCheckboxState();

            loadMethodDD();
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
        branchSelect: 'Branch',
        empTypeSelect: 'Emp. Type',
        departmentSelect: 'Department',
        designationSelect: 'Designation',
        empNatureSelect: 'Emp. Nature',
        activityStatusSelect: 'Select Status'
    };

    Object.keys(nonSelectedTextMap).forEach(function (id) {
        const selector = $('#' + id);
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
            maxWidth: 50,
            filterBehavior: 'text',
            enableCaseInsensitiveFiltering: true,
            buttonText: function (options, select) {
                if (options.length === 0) {
                    return nonSelectedTextMap[id];
                }

                else if (options.length > 0 && (id != 'companySelect') && (id != 'activityStatusSelect')) {
                    return options.length + ' Selected';
                }
                else {
                    return $(options[0]).text();
                }
            }
        });
    });
}
function excelUpload(e) {
    e.preventDefault();
    //debugger;
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
        url: `/HrmEmployeeSalaryInfoEntry/UploadExcelAsync`,
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        success: function (res) {
            if (res.success) {
                $("#choosefileText").text("Choose File");
                showNotification(res.message, "success");

                if (res.data && res.data.validationErrors && res.data.validationErrors.length > 0) {
                    console.warn("Some rows failed:", res.data.validationErrors);
                }

                if (typeof loadAllFilterEmp === 'function') {
                    loadAllFilterEmp();
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

function validateForm() {
    const selectedCheck = $('#employee-filter-grid-body input[type="checkbox"]:checked');
    if (selectedCheck.length === 0) {
        showNotification("Please select at least one employee.", "error");
        return false;
    }
    return true;
}
function handleFormSubmission() {
    if (!validateForm())
        return;

    showLoading();

    const selectedEmp = [];
    const selectedCheckboxes = $('#employee-filter-grid-body input[type="checkbox"]:checked');

    selectedCheckboxes.each(function () {
        const row = $(this).closest('tr');
        const empData = {
            EmployeeId: row.find('td:nth-child(2)').text().trim(),
            PayId: row.find('td:nth-child(3)').text().trim(),
            GrossSalary: parseFloat(row.find('input[type="text"]').val().trim()),
            DisbursementMethodId: row.find('.disbursement-dd').val()
        };

        selectedEmp.push(empData);
    });

    const dataToSend = {
        SalaryInfoUpdate: selectedEmp
    };

    console.log('Data To Send : ', dataToSend);

    $.ajax({
        url: '/HrmEmployeeSalaryInfoEntry/Save',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(dataToSend),
        success: function (response) {

            if (response.success) {
                showNotification("Employee salary information updated successfully!", "success");

                selectedEmployeeIds.clear();
                $("#employee-check-all").prop('checked', false);

                loadAllFilterEmp();

            } else {
                showNotification("Failed to update employee salary information.", "error");
            }

        }
    })

}

function showNotification(message, type) {
    if (typeof toastr !== 'undefined') {
        toastr[type](message, type === 'success' ? 'Success' : type === 'error' ? 'Error' : 'Warning');
    } else {
        alert(message);
    }
}

