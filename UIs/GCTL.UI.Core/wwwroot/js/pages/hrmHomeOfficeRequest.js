let filterChangeBound = false;
let selectedEmpId = null;
let selectedHOId = new Set();
let isEditMode = false;
let flatpickrInstance = {};
let fromPickr, toPickr, requestDatePickr;

$(document).ready(function () {
    setupLoadingOverlay();
    loadHOD();
    initializeEventHandlers();
    loadAllFilterEmp();
    loadTableData();
    setupEnterKeyNavigation();
    initializeSelect();
    initializeFlatpickr();
});

function initializeSelect() {
    $('#employeeSelect').select2({
        width: '100%',
        placeholder: '--Select--',
        allowClear: true,
        //containerCssClass: 'form-select',
    });

    $('#hrEmpId').select2({
        width: '100%',
        placeholder: '--Select--',
        allowClear: true,
        //dropdownCssClass: 'form-select',
        //containerCssClass: 'form-select'
    });


    //$('#companySelect').select2({
    //    width: '100%',
    //    allowClear: false,
    //    //dropdownCssClass: 'form-select',
    //});
}

function initializeFlatpickr() {
    if (typeof flatpickr === 'undefined')
        return;

    fromPickr = flatpickr("#dateFrom", {
        dateFormat: "Y-m-d",
        altInput: true,
        altFormat: "d/m/Y",
        allowInput: false,
        onchange: function (selectedDates) {
            if (selectedDates.length.length) {
                toPickr.set('minDate', selectedDates[0]);
            }
        }
    });

    toPickr = flatpickr("#dateTo", {
        dateFormat: "Y-m-d",
        altInput: true,
        altFormat: "d/m/Y",
        allowInput: false,
        onchange: function (selectedDates) {
            if (selectedDates.length.length) {
                fromPickr.set('maxDate', selectedDates[0]);
            }
        }
    });

    //$("#requestDate").val(new Date().toLocaleDateString("en-GB"));
    requestDatePickr = flatpickr("#requestDate", {
        dateFormat: "Y-m-d",
        altInput: true,
        altFormat: "d/m/Y",
        allowInput: false,
        clickOpens: false,
        defaultDate: new Date()
    });
}

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
    const $form = $('#homeOffRequest-form');
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

    $("#employeeSelect").on('change', function () {
        const selectedEmpId = $("#employeeSelect").val();
        //console.log(selectedEmpId);
        loadEmpInfo(selectedEmpId); 
    });

    $(".js-homeOffRequest-info-dec-clear").on('click', function () {
        clearForm();
    });

    $(".js-homeOffRequest-dec-save").on('click', handleFormSubmission);
    $(".js-homeOffRequest-dec-delete-confirm").on('click', handleBulkDelete);
    $(".js-homeOffRequest-dec-clear").on('click', function () {
        clearForm();
    });


    $(document).on("click", ".homeOffRequest-id-link", function () {
        const id = $(this).data("id");
        if (!id) return;

        populateForm(id);
    });

    $('#homeOffRequest-grid').DataTable().columns.adjust().draw();

    $('#homeOffRequest-check-all').on('change', function () {
        const isChecked = $(this).is(':checked');
        $('#homeOffRequest-grid-body input[type="checkbox"]').prop('checked', isChecked);

        updateSelectedHorIds();
    });
}
function populateForm(id) {
    showLoading();
    $.ajax({
        url: `/HrmHomeOfficeRequest/GetById/${id}`,
        type: "GET",
        success: function (res) {
            hideLoading();
            if (!res || !res.tc) {
                showNotification("Data not found", "error");
                return;
            }
            try {
                const data = res;
                console.log(data);

                const $employeeSelect = $('#employeeSelect');
                const $hrEmpId = $('#hrEmpId');

                if ($employeeSelect.find(`option[value="${data.employeeId}"]`).length === 0) {
                    $employeeSelect.append(`<option value="${data.employeeId}" selected>${data.name}</option>`);
                }

                if ($hrEmpId.find(`option[value="${data.hremployeeId}"]`).length === 0) {
                    $hrEmpId.append(`<option value="${data.hremployeeId}" selected>${data.hremployeeName}</option>`);
                }

                $employeeSelect.prop('disabled', true);
                isEditMode = true;

                $('#employeeSelect').val(data.employeeId).trigger('change');
                $('#Tc').val(data.tc);
                $('#HodId').val(data.horid);
                $('#hrEmpId').val(data.hremployeeId).trigger('change'); 
                //const formatDate = dateStr => dateStr ? dateStr.split("T")[0] : "";
                if (requestDatePickr && data.requestDate) {
                    requestDatePickr.setDate(data.requestDate);
                }
                if (fromPickr && data.startDate) {
                    fromPickr.setDate(data.startDate);
                }
                if (toPickr && data.endDate) {
                    toPickr.setDate(data.endDate);
                }
                $("#reason").val(data.reason);

                // If there are other fields to populate, add them here
            } catch (e) {
                console.error("Error populating form:", e);
                showNotification("Error loading record details", "error");
            }
        }, error: function (xhr, status, error) {
            hideLoading();
            console.error("Error fetching record:", error);
            showNotification("Failed to load record details", "error")
        }
    })
}

$(document).on('change', '#homeOffRequest-grid-body input[type="checkbox"]', function () {
    const id = $(this).data('id');

    if ($(this).is(':checked')) {
        selectedHOId.add(id);
    } else {
        selectedHOId.delete(id);
    }

    const total = $('#homeOffRequest-grid-body input[type="checkbox"]').length;
    const checked = $('#homeOffRequest-grid-body input[type="checkbox"]:checked').length;
    $("#homeOffRequest-check-all").prop('checked', total > 0 && total === checked);
});

function updateSelectedHorIds() {
    const currentPageCheckboxes = $('#homeOffRequest-grid-body input[type="checkbox"]');

    currentPageCheckboxes.each(function () {
        const id = $(this).data('id');
        if ($(this).is(':checked')) {
            selectedHOId.add(id);
        } else {
            selectedHOId.delete(id);
        }
    });
}

function getAllFilterVal() {
    return {
        CompanyCodes: $("#companySelect").val(),
        EmployeeID: $("#employeeSelect").val()
    };
}

function loadAllFilterEmp() {
    const filterData = getAllFilterVal();

    $.ajax({
        url: `/HrmHomeOfficeRequest/getFilterEmp`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(filterData),
        success: function (res) {
            const data = res.lookupData;
            console.log(data);

            if (data.companies?.length) {
                populateSelect("#companySelect", data.companies);
            }

            if (data.employees?.length) {
                populateSelect("#employeeSelect", data.employees);
            }

            if (typeof setupClearOnChangeEvents === 'function') {
                setupClearOnChangeEvents();
            }
        },
        error: function (xhr, status, error) {
            console.error("Error loading filtered employees:", error);
        }
    });
}

function loadHOD() {
    $.ajax({
        url: `/HrmHomeOfficeRequest/GetHOD`,
        type: "GET",
        contentType: "application/json",
        success: function (res) {
            console.log(res);
            if (res == null)
                return;
            populateSelect("#hrEmpId", res);
        },
        error: function (xhr, status, error) {
            console.error("Error loading HOD:", error);
        }
    });
}

function loadEmpInfo(id) {
    if (!id || id.trim() === '' || id == null) {
        clearEmployeeInfo();
        return;
    }
    console.log(JSON.stringify({ selectedEmpId: id }));
    $.ajax({
        url: `/HrmHomeOfficeRequest/GetEmpData`,
        type: "GET",
        contentType: "application/json",
        data: { selectedEmpId: id },
        success: function (res) {
            console.log(res);
            if (res == null)
                return;
            $(".EmployeeName").text(res.name);
            $(".EmployeeDepartment").text(res.departmentName);
            $(".EmployeeDesignation").text(res.designationName);
            $(".EmployeeJoinDate").text(res.joiningDate);
            selectedEmpId = res.employeeId;
        },
        error: function (xhr, status, error) {
            console.error("Error loading employee info:", error);
        }
    });
}

function populateSelect(selectId, dataList) {
    const $select = $(selectId);
    $select.empty();
    if (selectId === "#employeeSelect" || selectId === "#hrEmpId") {
        $select.append('<option value="">--Select--</option>');
    }
    dataList.forEach(item => {
        if (item.code && item.name) {
            $select.append(`<option value="${item.code}">${item.name}</option>`);
        }
    });
}

function setupClearOnChangeEvents() {
    const clearMap = {
        "#companySelect": ["#employeeSelect"]
    };
    Object.entries(clearMap).forEach(([parent, children]) => {
        $(parent).off("change.clearDropdowns").on("change.clearDropdowns", function () {
            children.forEach(child => $(child).empty().multiselect && $(child).multiselect('rebuild'));
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

function loadTableData() {
    if (typeof displayGridData === 'function') {
        displayGridData();
    } else {
        console.warn('displayGridData function is not defined.');
    }
}
function formatDateToDDMMYYYY(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return '';
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
}
function displayGridData() {
    if ($.fn.DataTable.isDataTable("#homeOffRequest-grid")) {
        $("#homeOffRequest-grid").DataTable().clear().destroy();
    }

    const tableBody = $("#homeOffRequest-grid-body");
    tableBody.empty();
    $('#homeOffRequest-grid').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/HrmHomeOfficeRequest/GetPaginatedEntires',
            type: 'POST',
            //success: function (data) {
            //    console.log(data);
            //}
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
                data: 'horid',
                className: 'text-center',
                render: function (data, type, row) {
                    return `<a href="#homeOffRequest-form" class="homeOffRequest-id-link" data-id="${row.tc}">${data}</a>`;
                }
            },
            {
                data: 'requestDate',
                className: 'text-center',
                render: function (data, type, row) {
                    return formatDateToDDMMYYYY(data);
                }
            },
            { data: 'approvalStatus', className: 'text-center' },
            { data: 'employeeId', className: 'text-center' },
            {
                data: 'startDate',
                className: 'text-center',
                render: function (data, type, row) {
                    return formatDateToDDMMYYYY(data);
                }
            },
            {
                data: 'endDate',
                className: 'text-center',
                render: function (data, type, row) {
                    return formatDateToDDMMYYYY(data);
                }
            },
            { data: 'reason', className: 'text-center' },
            { data: 'entryBy', className: 'text-center' },
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
            $('#homeOffRequest-grid-body input[type="checkbox"]').each(function () {
                const id = $(this).data('id');
                $(this).prop('checked', selectedHOId.has(id));
            });

            const total = $('#homeOffRequest-grid-body input[type="checkbox"]').length;
            const checked = $('#homeOffRequest-grid-body input[type="checkbox"]:checked').length;
            $("#homeOffRequest-check-all").prop('checked', total > 0 && total === checked);
        }
    });
}

function validateForm() {
    if (!$('#employeeSelect').val()) {
        showNotification('Please select an employee', 'warning');
        $('#employeeSelect').focus();
        return false;
    }
    if (!$('#hrEmpId').val()) {
        showNotification('Please select a Head of Department', 'warning');
        $('#hrEmpId').focus();
        return false;
    }
    if (!$('#requestDate').val()) {
        showNotification('Please select a request date', 'warning');
        $('#requestDate').focus();
        return false;
    }
    if (!$('#dateFrom').val()) {
        showNotification('Please select a start date', 'warning');
        $('#dateFrom').focus();
        return false;
    }
    if (!$('#dateTo').val()) {
        showNotification('Please select an end date', 'warning');
        $('#dateTo').focus();
        return false;
    }
    return true;
}

function handleFormSubmission() {
    if (!validateForm())
        return;

    showLoading();
    console.log(selectedEmpId);

    const dataToSend = {
        Tc: parseInt($('#Tc').val()) || 0,
        Horid: $('#HodId').val(),
        CompanyCode: $('#companySelect').val(),
        EmployeeId: selectedEmpId,
        HremployeeId: $('#hrEmpId').val(),
        RequestDate: new Date($('#requestDate').val()),
        StartDate: new Date($('#dateFrom').val()),
        EndDate: new Date($('#dateTo').val()),
        Reason: $('#reason').val() || ''
    };

    console.log(JSON.stringify(dataToSend));

    $.ajax({
        url: '/HrmHomeOfficeRequest/SaveEntry',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(dataToSend),
        success: function (response) {
            if (response.success) {
                clearForm();
                showNotification(response.message, 'success');
                selectedEmpId = null;
                loadAllFilterEmp();
                loadTableData();
            } else {
                showNotification(response.message, 'error');
            }
        },
        complete: hideLoading
    });
}

function clearEmployeeInfo() {
    const employeeFields = [
        '.EmployeeName',
        '.EmployeeDepartment',
        '.EmployeeDesignation',
        '.EmployeeJoinDate'
    ];
    employeeFields.forEach(field => {
        $(field).text('');
    });
    selectedEmpId = null;
}
function clearForm() {
    if ($('#employeeSelect').is(':disabled')) {
        $('#employeeSelect').prop('disabled', false);
    }
    isEditMode = false;
    $('#employeeSelect').val('').trigger('change');
    $('#hrEmpId').val('').trigger('change');
    $('#companySelect').val('').trigger('change');
    $('#Tc').val(0);
    $('#HodId').val('');
    $('#requestDate').val('');
    $('#reason').val(''); 
    if (requestDatePickr) {
        requestDatePickr.clear();
        requestDatePickr.setDate(new Date());
    }
    if (fromPickr) {
        fromPickr.clear();
    }
    if (toPickr) {
        toPickr.clear();
    }
    clearEmployeeInfo();
    selectedHOId.clear();
    $('#homeOffRequest-check-all').prop('checked', false);
    loadAllFilterEmp();
    
    loadTableData();
}
function handleBulkDelete() {
    const selectedIds = Array.from(selectedHOId);

    if (selectedIds.length === 0) {
        showNotification('Please select record(s) to delete', 'warning');
        return;
    }

    if (!confirm(`Are you sure you want to delete ${selectedIds.length} selected request(s)?`)) {
        return;
    }

    showLoading();

    $.ajax({
        url: '/HrmHomeOfficeRequest/BulkDelete',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Tcs: selectedIds }),
        success: function (response) {
            selectedHOId.clear();
            showNotification(response.message || 'Successfully deleted', 'success');
            loadTableData();
            clearForm();
        },
        error: function (xhr, status, error) {
            console.error('Error details:', xhr.responseText);
            showNotification('Error deleting records', 'error');
        },
        complete: hideLoading
    });
}

function showNotification(message, type) {
    if (typeof toastr !== 'undefined' && toastr[type]) {
        toastr[type](message, type === 'success' ? 'Success' : type === 'error' ? 'Error' : 'Warning');
    } else {
        alert(message);
    }
}
