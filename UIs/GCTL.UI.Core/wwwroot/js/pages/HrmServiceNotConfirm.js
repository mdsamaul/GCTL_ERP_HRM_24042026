let filterChangeBound = false;
let selectedEmpId = null;
let selectedSNCId = new Set();
let isEditMode = false;

let empObj = [];

$(document).ready(function () {
    setupLoadingOverlay();
    initializeEventHandlers();
    loadAllFilterEmp();
    loadTableData();
    loadSNCId();
    setupEnterKeyNavigation();
})

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
    const $form = $('#serviceNotConfirm-form');
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


    $(".companySelect").on("change", function () {
        loadAllFilterEmp();
    });

    $(".employeeSelect").on('change', function () {
        const selectedEmpId = $("#employeeSelect").val();
        const empData = empObj.filter(e => e.employeeId === selectedEmpId);
        console.log(empObj);
        loadEmpInfo(empData[0]); 
    });


    $(".js-serviceNotConfirm-info-dec-clear").on('click', function () {
        clearForm();
    });

    $(".js-serviceNotConfirm-dec-save").on('click', handleFormSubmission);
    $(".js-serviceNotConfirm-dec-delete-confirm").on('click', handleBulkDelete);
    $(".js-serviceNotConfirm-dec-clear").on('click', function () {
        clearForm();
        loadSNCId();
    });


    $(document).on("click", ".serviceNotConfirm-id-link", function () {
        const id = $(this).data("id");
        if (!id) return;

        populateForm(id);
    });

    $('#serviceNotConfirm-grid').DataTable().columns.adjust().draw();

    $('#serviceNotConfirm-check-all').on('change', function () {
        const isChecked = $(this).is(':checked');
        $('#serviceNotConfirm-grid-body input[type="checkbox"]').prop('checked', isChecked);

        updateSelectedSNCIds();
    });
}

$(document).on('change', '#serviceNotConfirm-grid-body input[type="checkbox"]', function () {
    const id = $(this).data('id');

    if ($(this).is(':checked')) {
        selectedSNCId.add(id);
    } else {
        selectedSNCId.delete(id);
    }

    const total = $('#serviceNotConfirm-grid-body input[type="checkbox"]').length;
    const checked = $('#serviceNotConfirm-grid-body input[type="checkbox"]:checked').length;
    $("#serviceNotConfirm-check-all").prop('checked', total > 0 && total === checked);
});

function updateSelectedSNCIds() {
    const currentPageCheckboxes = $('#serviceNotConfirm-grid-body input[type="checkbox"]');

    currentPageCheckboxes.each(function () {

        const id = $(this).data('id');

        if ($(this).is(':checked')) {
            selectedSNCId.add(id);
        } else {
            selectedSNCId.delete(id);
        }
    });
}

function getAllFilterVal() {
    const filterData = {
        CompanyCodes: $("#companySelect").val(),
        EmployeeID: $("#employeeSelect").val()
    }
    return filterData;
}

function loadAllFilterEmp() {
   // showLoading();
    const filterData = getAllFilterVal();

    $.ajax({
        url: `/HrmServiceNotConfirmationEntry/getFilterEmp`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(filterData),
        success: function (res) {
            empObj = res.employees;
            console.log(empObj);
            const data = res.lookupData;

            if (data.companies?.length) {
                populateSelect("#companySelect", data.companies)
            }

            if (data.employees?.length) {
                populateSelect("#employeeSelect", data.employees);
            }

            setupClearOnChangeEvents();

            bindFilterChangeOnce();
        },
        complete: function () {
           // hideLoading();
        },
        error: function (xhr, status, error) {
            console.error("Error loading filtered employees:", error);
           // hideLoading();
        }
    });
}
function loadEmpInfo(res) {
    if (res == null) { return; }

    $(".EmployeeName").text(res.name);
    $(".EmployeeDepartment").text(res.departmentName);
    $(".EmployeeDesignation").text(res.designationName);
    $(".EmployeeGrossSalary").text(res.grossSalary);
    $(".EmployeeJoinDate").text(res.joiningDate);
    $(".EmployeeProbationPeriod").text(res.probationPeriod);
    $(".EmployeeEndOn").text(res.endOn);
    $(".ServiceLength").text(res.serviceLength);
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
        "#companySelect": ["#employeeSelect"]
    };

    Object.entries(clearMap).forEach(([parent, children]) => {
        $(parent).off("change.clearDropdowns").on("change.clearDropdowns", function () {
            children.forEach(child => $(child).empty().multiselect('rebuild'));
        });
    });
}
function bindFilterChangeOnce() {
    if (!filterChangeBound) {
        $("#companySelect, #employeeSelect")
            .on("change.loadFilter", function () {
                loadAllFilterEmp();
            });
        filterChangeBound = true;
    }
}

function loadSNCId() {
    $.ajax({
        url: "/HrmServiceNotConfirmationEntry/GenerateNewId",
        type: "GET",
        dataType: "json",
        success: function (data) {
            console.log(data);
            if (data) {
                $('#sncId').val(data);
            }
        },
        error: function (xhr, status, error) {
            console.error("Error fetching Salary deduction ID:", error);
        }
    })
}

function loadTableData() {
    //showLoading();
    displayGridData();
    //hideLoading();
}

function displayGridData() {
    if ($.fn.DataTable.isDataTable("#serviceNotConfirm-grid")) {
        $("#serviceNotConfirm-grid").DataTable().clear().destroy();
    }

    const tableBody = $("#serviceNotConfirm-grid-body");
    tableBody.empty();
    $('#serviceNotConfirm-grid').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/HrmServiceNotConfirmationEntry/GetPaginatedEntries',
            type: 'POST',
            success: function (data) { console.log(data) }
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
                data: 'sncId',
                className: 'text-center',
                render: function (data, type, row) {
                    return `<a href="#serviceNotConfirm-form" class="serviceNotConfirm-id-link" data-id="${row.tc}">${data}</a>`;
                }
            },
            { data: 'employeeId', className: 'text-center' },
            { data: 'employeeName', className: 'text-left' },
            { data: 'effectiveDate', className: 'text-center' },
            { data: 'duePaymentDate', className: 'text-center' },
            { data: 'refLetterNo', className: 'text-center' },
            { data: 'refLetterDate', className: 'text-center' },
            { data: 'remarks', className: 'text-center' }
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
            $('#serviceNotConfirm-grid-body input[type="checkbox"]').each(function () {
                const id = $(this).data('id');
                $(this).prop('checked', selectedOtIds.has(id));
            });

            const total = $('#serviceNotConfirm-grid-body input[type="checkbox"]').length;
            const checked = $('#serviceNotConfirm-grid-body input[type="checkbox"]:checked').length;
            $("#serviceNotConfirm-check-all").prop('checked', total > 0 && total === checked);
        }
    });
}

function validateForm() {
    //const selectedCheck = $('#employee-filter-grid-body input[type="checkbox"]:checked');
    //if (selectedCheck.length === 0) {
    //    showNotification("Please select at least one employee.", "error");
    //    return false;
    //}
    return true;
}

function handleFormSubmission() {
    if (!validateForm())
        return;

    showLoading();

    const selectedEmp = $("#employeeSelect").val();


    const dataToSend = {
        EmployeeId: selectedEmp,
        Sncid: $("#sncId").val(),
        EffectiveDate: $("#effectiveDate").val(),
        DuePaymentDate: $("#duePaymentDate").val(),
        RefLetterNo: $("#refLetterNo").val(),
        RefLetterDate: $("#refLetterDate").val(),
        Remarks: $("remarks").val()
    };

    console.log('Data To Send : ', dataToSend);

    $.ajax({
        url: '/HrmServiceNotConfirmationEntry/SaveEntry',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(dataToSend),
        success: function (response) {

            if (response.success) {
                showNotification("Employee salary information updated successfully!", "success");
                empObj = [];
                selectedEmpId=null;
                
                loadAllFilterEmp();
            } else {
                showNotification("Failed to update employee salary information.", "error");
            }

        },
        complete: hideLoading
    })
}

function clearForm() {
    isEditMode = false;
    $("#employeeSelect").val('');
    $("#effectiveDate").val('');
    $("#effectiveDate").val('');
    $("#duePaymentDate").val('');
    $("#refLetterNo").val('');
    $("#refLetterDate").val('');
    $("remarks").val('');
    loadSNCId();
    loadTableData();
    employees = [];
};


function handleBulkDelete() {
    const selectedIds = Array.from(selectedSNCId);

    if (selectedIds.length === 0) {
        showNotification("Please select record to delete", "warning");
        return;
    }

    if (!confirm(`Are you sure you want to delete ${selectedIds.length} selected monthly OT(s)?`)) {
        return;
    }

    showLoading();

    $.ajax({
        url: `/HrmServiceNotConfirmationEntry/BulkDelete`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Tcs: selectedSNCId }),
        success: function (response) {
            selectedSNCId.clear();
            showNotification(response.message || "Successfully deleted", "success");
            loadTableData();
            loadSNCId();
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