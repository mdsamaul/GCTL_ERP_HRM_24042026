let edit = false;
let originalEmployeeId = null;
let selectedHolidayIds = new Set();


$(document).ready(function () {
    setupLoadingOverlay();
    populateHolidayTypeDropdown();
    loadEmployeeHolidayData();
    initializeEventHandlers();
});

function setupLoadingOverlay() {
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
}

function showLoading() {
    $("#customLoadingOverlay").css("display", "flex");
}

function hideLoading() {
    $("#customLoadingOverlay").hide();
}


function initializeEventHandlers() {
    $("#holidayDate").on('change', function () {
        formData.selectedDate = $(this).val();
    });

    $("#inPlaceDate").on('change', function () {
        formData.inPlaceOfDate = $(this).val();
    });

    $("#submitButton").on('click', handleFormSubmission);

    $('#excelFileInput').on('change', function () {
        if (this.files && this.files.length > 0) {
            $("#choosefileText").text(this.files[0].name);
        } else {
            $("#choosefileText").text('No file chosen');
        }
    });

    $("#excelUploadForm").submit(excelUpload);

    $("#js-emp-holiday-dec-clear").on('click', function () {
        clearForm();
    });

    $("#js-emp-holiday-dec-delete-confirm").on('click', handleBulkDelete);

    $(document).on("click", ".holiday-id-link", function () {
        originalEmployeeId
        const ehdid = $(this).data("id");
        if (!ehdid) return;

        $.ajax({
            url: `/HRMEmployeeHolidayDeclaration/Details/${ehdid}`,
            method: 'GET',
            success: populateHolidayDeclarationForm,
            error: () => toastr.error("Failed to load Employee Holiday Declaration.")
        });
    });

    $('#hdtcode').focus();

    $('#employee-holiday-grid-show').DataTable().columns.adjust().draw();

    const $checkAll = $("#employee-holiday-check-all");
    const $checkboxes = $('#employee-holiday-grid-body-show input[type="checkbox"]');
    const $gridBody = $('#employee-holiday-grid-body-show');
    const $table = $('#employee-holiday-grid-show').DataTable();

    $("#employee-holiday-check-all").on('change', function () {
        const isChecked = $(this).is(':checked');
        $('#employee-holiday-grid-body-show input[type="checkbox"]').prop('checked', isChecked);
        updateSelectedHolidayIds()
    });

    $('#employee-holiday-grid-body-show').on('change', 'input[type="checkbox"]', function () {
        const total = $('#employee-holiday-grid-body-show').find('input[type="checkbox"]').length;
        const checked = $('#employee-holiday-grid-body-show').find('input[type="checkbox"]:checked').length;

        $checkAll.prop('checked', total === checked);
        updateSelectedHolidayIds()
    });

    $table.on('draw', function () {
        const checkboxes = $gridBody.find('input[type="checkbox"]');

        checkboxes.each(function () {
            const id = $(this).data('id');
            $(this).prop('checked', selectedHolidayIds.has(id));
        });

        const total = checkboxes.length;
        const checked = checkboxes.filter(':checked').length;
        $checkAll.prop('checked', total > 0 && total === checked);
    });
}


function updateSelectedHolidayIds() {
    const currentPageCheckboxes = $('#employee-holiday-grid-body-show input[type="checkbox"]');
    currentPageCheckboxes.each(function () {
        const id = $(this).data('id');

        if ($(this).is(':checked')) {
            selectedHolidayIds.add(id);
        } else {
            selectedHolidayIds.delete(id);
        }
    });
}
function enforceEditedEmployeeSelection() {
    if (!edit) return;

    const originalEmpIdStr = String(originalEmployeeId);

    console.log("Original Employee ID:", originalEmpIdStr);

    const table = $('#employee-filter-grid').DataTable();

    table.rows().every(function () {
        const row = this.node();
        const empId = $(row).find('td:nth-child(2)').text().trim();

        $(row).find('td:first-child input[type="checkbox"]')
            .prop('checked', false)
            .prop('disabled', true);

        if (empId === originalEmpIdStr) {
            console.log("Matched Employee ID:", empId);
            $(row).find('td:first-child input[type="checkbox"]')
                .prop('checked', true);
        }
    });

    // Also disable the "check all" box
    $('#employee-check-all').prop('disabled', true);
}
const formData = {
    selectedDate: null,
    inPlaceOfDate: null,
};

function populateHolidayTypeDropdown() {
    showLoading();
    $.ajax({
        url: `/HRMEmployeeHolidayDeclaration/GetHolidayDeclarationType`,
        method: 'GET',
        success: function (response) {
            if (response.data && response.data.length > 0) {
                const $select = $('#hdtcode');
                $select.empty();
                $select.append('<option value="">Please select</option>');

                response.data.forEach(function (item) {
                    $select.append(`<option value="${item.hdtcode}">${item.name}</option>`);
                });
            }
        },
        error: function (xhr, status, error) {
            showNotification("Error fetching holiday types", "error");
            console.error("Error fetching holiday types:", error);
        },
        complete: hideLoading
    });
}

function handleFormSubmission() {
    if (!validateForm()) {
        return;
    }

    let dateValue = formData.selectedDate ? formData.selectedDate + "T00:00:00" : null;
    let inPlaceOfDateValue = formData.inPlaceOfDate ? formData.inPlaceOfDate + "T00:00:00" : null;

    let dataToSend;
    let url, type;

    if (edit) {
        url = `/HRMEmployeeHolidayDeclaration/EditHoliday/${$('#ehdid').val().trim()}`;
        type = "PUT";
        dataToSend = {
            Ehdid: $('#ehdid').val(),
            Date: dateValue,
            IsDayoffDuty: $("#isDayoffDuty").is(':checked') ? "true" : "false",
            HolidayDecType: $("#hdtcode").val(),
            InPlaceofDate: inPlaceOfDateValue,
            Remark: $("#remarks").val(),
            CompanyCode: $("#companySelect").val(),
            EmployeeIds: null
        };
    } else {
        const selectedEmployeeIds = [];
        $('#employee-filter-grid-body input[type="checkbox"]:checked').each(function () {
            const row = $(this).closest('tr');
            const empId = row.find('td:nth-child(2)').text().trim();
            selectedEmployeeIds.push(empId);
        });

        url = `/HRMEmployeeHolidayDeclaration/SaveHoliday`;
        type = "POST";
        dataToSend = {
            Date: dateValue,
            IsDayoffDuty: $("#isDayoffDuty").is(':checked') ? "true" : "false",
            HolidayDecType: $("#hdtcode").val(),
            InPlaceofDate: inPlaceOfDateValue,
            EmployeeIds: selectedEmployeeIds,
            Remark: $("#remarks").val(),
            CompanyCode: $("#companySelect").val()
        };
    }

    showLoading();
    $.ajax({
        url: url,
        type: type,
        data: JSON.stringify(dataToSend),
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            //console.log(response);
            showNotification(edit ? "Record updated successfully." : "Data saved successfully.", "success");
            loadEmployeeHolidayData();
            clearForm();
        },
        error: function (xhr, status, error) {
            showNotification(`Error ${edit ? 'updating' : 'saving'} data`, "error");
            console.error(`Error ${edit ? 'updating' : 'saving'} holiday declaration:`, error);
        },
        complete: hideLoading
    });
}

function validateForm() {
    if (!formData.selectedDate) {
        showNotification("Please select a holiday date", "warning");
        $('#holidayDate').focus();
        return false;
    }

    if (!$("#hdtcode").val()) {
        showNotification("Please select a holiday type", "warning");
        $('#hdtcode').focus();
        return false;
    }

    if (!edit) {
        const selectedEmployeeCount = $('#employee-filter-grid-body input[type="checkbox"]:checked').length;
        if (selectedEmployeeCount === 0) {
            showNotification("Please select at least one employee", "warning");
            $('#employee-filter-grid-body').focus();
            return false;
        }
    }

    return true;
}
function clearForm() {
    $("#holidayDate").val('');
    $("#ehdid").val('');
    $("#inPlaceDate").val('');
    $("#isDayoffDuty").prop('checked', false);
    $("#hdtcode").val('');
    $("#remarks").val('');

    $('#employee-filter-grid-body input[type="checkbox"]').prop('checked', false).prop('disabled', false);
    $('#employee-check-all').prop('checked', false).prop('disabled', false);

    formData.selectedDate = null;
    formData.inPlaceOfDate = null;

    $("#submitButton").html('<i class="fa fa-save">&nbsp;</i> Save');

    $("#branchSelect").val(null).trigger('change');
    $("#divisionSelect").val(null).trigger('change');
    $("#departmentSelect").val(null).trigger('change');
    $("#designationSelect").val(null).trigger('change');
    $("#employeeSelect").val(null).trigger('change');

    edit = false;
    originalEmployeeId = null;
    selectedHolidayIds.clear();
    $("#employee-filter-check-all").prop('checked', false);
}

function loadEmployeeHolidayData() {
    const tableId = "#employee-holiday-grid-show";

    if ($.fn.DataTable.isDataTable(tableId)) {
        $(tableId).DataTable().clear().destroy();
    }

    const tableBody = $("#employee-holiday-grid-body-show");
    tableBody.empty();

    $(tableId).DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/HRMEmployeeHolidayDeclaration/GetPaginatedHolidayDeclaration',
            type: 'POST',
            data: function (d) {
                // Add any custom parameters if needed
                return d;
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
                data: 'ehdid',
                className: 'text-center',
                render: function (data, type, row) {
                    return `<a href="#holidayForm" class="holiday-id-link" data-id="${row.autoId}">${row.ehdid}</a>`;
                }
            },
            { data: 'employeeId', className: 'text-center' },
            { data: 'employeeName', className: 'text-left' },
            { data: 'designation', className: 'text-left' },
            {
                data: 'date',
                className: 'text-left',
                render: function (data) {
                    return data ? new Date(data).toLocaleDateString('en-GB') : '';
                }
            },
            {
                data: 'inPlaceofDate',
                className: 'text-left',
                render: function (data) {
                    return data ? new Date(data).toLocaleDateString('en-GB') : '';
                }
            },
            { data: 'isDayoffDuty', className: 'text-center' },
            { data: 'holidayDecType', className: 'text-center' },
            { data: 'remark', className: 'text-center' },
            { data: 'entryUser', className: 'text-center' },

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

                    debounceTimeout = setTimeout(() => {
                        api.search(searchTerm).page('first').draw('page');
                    }, 500);
                });
        },
        drawCallback: function () {
            $('#employee-holiday-grid-body-show input[type="checkbox"]').each(function () {
                const id = $(this).data('id');
                $(this).prop('checked', selectedHolidayIds.has(id));
            });

            const total = $('#employee-holiday-grid-body-show input[type="checkbox"]').length;
            const checked = $('#employee-holiday-grid-body-show input[type="checkbox"]:checked').length;

            // Corrected selector
            $("#employee-holiday-check-all").prop('checked', total > 0 && total === checked);
        }
    });
}

function handleBulkDelete() {
    const selectedIds = Array.from(selectedHolidayIds);


    if (!edit) {
        if (selectedIds.length > 0) {
            if (!confirm("Are you sure you want to delete the selected holiday declarations?")) {
                return;
            }

            console.log(selectedIds);

            showLoading();
            $.ajax({
                url: `/HRMEmployeeHolidayDeclaration/BulkDelete`,
                type: "POST",
                data: { ids: selectedIds },
                traditional: true,
                success: function (response) {
                    selectedHolidayIds.clear();
                    showNotification(response.message, "success");
                    loadEmployeeHolidayData();
                },
                error: function (xhr, status, error) {
                    showNotification("Error deleting holiday declarations", "error");
                    console.error("Error deleting holiday declarations:", error);
                },
                complete: hideLoading
            });

            return;
        }

    }

    if (!formData.selectedDate) {
        showNotification("Please select a date or check holiday declarations to delete", "warning");
        return false;
    }

    const selectedEmployeeIds = [];
    $('#employee-filter-grid-body input[type="checkbox"]:checked').each(function () {
        const row = $(this).closest('tr');
        const empId = row.find('td:nth-child(2)').text().trim();
        selectedEmployeeIds.push(empId);
    });

    if (selectedEmployeeIds.length === 0) {
        showNotification("Please select at least one employee", "warning");
        return false;
    }

    if (!confirm("Are you sure you want to delete the holiday declarations for the selected employees on the selected date?")) {
        return;
    }

    const deleteData = {
        Date: formData.selectedDate + "T00:00:00",
        EmployeeIds: selectedEmployeeIds,
        CompanyCode: $("#companySelect").val()
    };

    showLoading();
    $.ajax({
        url: "/HRMEmployeeHolidayDeclaration/DeleteHolidayByDateAndEmployees",
        type: "POST",
        data: JSON.stringify(deleteData),
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            showNotification("Holiday declarations deleted successfully!", "success");
            loadEmployeeHolidayData();
            clearForm();
        },
        error: function (xhr, status, error) {
            showNotification("Error deleting holiday declarations", "error");
            console.error("Error deleting holiday declarations:", error);
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

function populateHolidayDeclarationForm(response) {
    if (!response || !response.data) {
        console.error("Invalid response format:", response);
        showNotification("Invalid data format received", "error");
        return;
    }

    try {
        const data = response.data;

        clearForm();

        $('#ehdid').val(data.ehdid);
        $('#employee-filter-grid-body input[type="checkbox"]').prop('checked', false);
        function convertToLocalDate(utcDateString) {
            const date = new Date(utcDateString);
            return date.getFullYear() + "-" +
                String(date.getMonth() + 1).padStart(2, '0') + "-" +
                String(date.getDate()).padStart(2, '0');
        }

        const holidayDate = convertToLocalDate(data.date);
        const inPlaceDate = data.inPlaceofDate ? convertToLocalDate(data.inPlaceofDate) : null;

        $('#holidayDate').val(holidayDate);



        $('#remarks').val(data.remark || '');
        $('#hdtcode').val(data.holidayDecType);

        if (inPlaceDate) {
            $('#inPlaceDate').val(inPlaceDate);
            console.log($('#inPlaceDate').val());
        }

        $('#isDayoffDuty').prop('checked', data.isDayoffDuty === 'true' || data.isDayoffDuty === 'Yes');

        formData.selectedDate = holidayDate;
        formData.inPlaceOfDate = inPlaceDate;

        originalEmployeeId = String(data.employeeId);

        edit = true;

        $("#submitButton").html('<i class="fa fa-edit">&nbsp;</i> Update');

        setTimeout(() => {
            enforceEditedEmployeeSelection();
        }, 200);

    } catch (error) {
        console.error("Error populating form:", error);
        showNotification("Error populating form with holiday declaration details", "error");
    }
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
        url: `/HrmEmployeeHolidayDeclaration/UploadExcelAsync`,
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        success: function (res) {
            //console.log(res);
            if (res.success) {
                $("#choosefileText").text("Choose File");
                showNotification(res.message, "success");

                if (res.data && res.data.validationErrors && res.data.validationErrors.length > 0) {
                    console.warn("Some rows failed:", res.data.validationErrors);
                }

                if (typeof loadEmployeeHolidayData === 'function') {
                    loadEmployeeHolidayData();
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









let employeeDataTable = null;

$(document).ready(function () {
    setupLoadingOverlay();
    //loadCompany();
    initializeMultiselects();
    initializeWeekendGrid();
    loadAllFilterEmp();
    $("#employee-check-all").on('change', function () {
        var isChecked = $(this).is(':checked');
        $('#employee-filter-grid-body input[type="checkbox"]').prop('checked', isChecked);
    });
});

$("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect, #employeeStatus, #activityStatusSelect")
    .on("change", function () {
        loadAllFilterEmp();
    });

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
    filterData.CompanyCodes = ["001"];
    $.ajax({
        url: `/HRMEmployeeHolidayDeclaration/getFilterEmp`,
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

let filterChangeBound = false;

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
    console.log(res);
    var tableDataItem = res.employees;

    if ($.fn.DataTable.isDataTable('#employee-filter-grid') && employeeDataTable !== null) {
        employeeDataTable.destroy();
        employeeDataTable = null;
    }

    var tableBody = $("#employee-filter-grid-body");
    tableBody.empty();

    $.each(tableDataItem, function (index, employee) {
        var row = $('<tr>');

        row.append('<td class="text-center"><input type="checkbox" /></td>');
        row.append('<td class="text-center">' + employee.employeeId + '</td>');
        row.append('<td class="text-center">' + employee.employeeName + '</td>');
        row.append('<td class="text-center">' + employee.designationName + '</td>');
        row.append('<td class="text-center">' + employee.departmentName + '</td>');
        row.append('<td class="text-center">' + employee.branchName + '</td>');
        row.append('<td class="text-center">' + employee.companyName + '</td>');
        row.append('<td class="text-center">' + employee.employeeTypeName + '</td>');
        row.append('<td class="text-center">' + employee.joiningDate + '</td>');
        row.append('<td class="text-center">' + employee.employeeStatus + '</td>');

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
    });
}

function initializeWeekendGrid() {
    showLoading();
    initializeDataTable();
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
            },
            onFiltering: function (event) {
                event.query = event.query.toLowerCase();
            }
        });
    });
}
