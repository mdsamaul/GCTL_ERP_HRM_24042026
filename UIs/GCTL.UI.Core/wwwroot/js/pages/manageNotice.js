//let empDataTable = null;
//let filterChangeBound = false;
//let originalEmpId = null;
//let selectedEmpIds = new Set();
//let selectedNoticeIds = new Set();
//let isEditMode = false;

//$(document).ready(function () {
//    setupLoadingOverlay();
//    initializeMultiselects();
//    initializeEventHandlers();
//    initializeEmployeeGrid();
//    loadAllFilterEmp();
//    loadNoticeId();
//    loadNoticeData();
//    setupEnterKeyNavigation();
//});

//$(window).on('load', function () {
//    $('#employee-filter-grid-body')[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
//});

//function setupLoadingOverlay() {
//    if ($("#loadingOverlay").length === 0) {
//        $("body").append(`
//            <div id="loadingOverlay" style="
//                display: none;
//                position: fixed;
//                top: 0;
//                left: 0;
//                width: 100%;
//                height: 100%;
//                background-color: rgba(0, 0, 0, 0.5);
//                z-index: 9999;
//                justify-content: center;
//                align-items: center;">
//                <div style="
//                    background-color: white;
//                    padding: 20px;
//                    border-radius: 5px;
//                    box-shadow: 0 0 10px rgba(0,0,0,0.3);
//                    text-align: center;">
//                    <div class="spinner-border text-primary" role="status">

//                    </div>
//                </div>
//            </div>
//        `);
//    }
//}
//function showLoading() {
//    $('body').css('overflow', 'hidden');
//    $("#loadingOverlay").fadeIn(200);
//}

//function hideLoading() {
//    $('body').css('overflow', '');
//    $("#loadingOverlay").fadeOut(200);
//}

//function setupEnterKeyNavigation() {
//    const $form = $('#notice-form');
//    if (!$form.length) return;

//    $form.on('keydown', 'input, select, textarea, button, [tabindex]:not([tabindex="-1"])', function (e) {
//        if (e.key === 'Enter') {
//            e.preventDefault();

//            const $focusable = $form
//                .find('input:not([disabled]), select:not([disabled]), textarea:not([disabled]), button, [herf], [tabindex]:not([tabindex="-1"])')
//                .filter(':visible');

//            const index = $focusable.index(this);
//            if (index > -1) {
//                const $next = $focusable.eq(index + 1).length ?
//                    $focusable.eq(index + 1) : $focusable.eq(0);
//                $next.focus();
//            }
//        }
//    });
//}

//function initializeEventHandlers() {
//    $("#companySelect, #departmentSelect, #designationSelect").on("change", function () {
//        loadAllFilterEmp();
//    });
//    $(".js-notice-dec-save").on('click', handleFormSubmission);
//    $(".js-notice-dec-delete-confirm").on('click', handleBulkDelete);
//    $("#sendMail").on('click', handleEmailSentBtn);
//    $(".js-notice-dec-clear").on('click', function () {
//        clearForm();
//        loadNoticeId();
//    });
//    $(document).on("click", ".notice-id-link", function () {
//        const id = $(this).data("id");
//        if (!id) return; populateForm
//        populateForm(id);
//    });
//    $('#notice-grid').DataTable().columns.adjust().draw();
//    $('#notice-check-all').on('change', function () {
//        const isChecked = $(this).is(':checked');
//        $('#notice-grid-body input[type="checkbox"]').prop('checked', isChecked);
//        updateSelectedNoticeIds();
//    });

//    $('#notice-grid-body').on('change', 'input[type="checkbox"]', function () {
//        const total = $('#notice-grid-body').find('input[type="checkbox"]').length;
//        const checked = $('#notice-grid-body').find('input[type="checkbox"]:checked').length;

//        $('#notice-check-all').prop('checked', total === checked);
//        updateSelectedNoticeIds()
//    });

//    $("#employee-check-all").on('change', function () {
//        const isChecked = $(this).is(':checked');
//        const checkboxes = $('#employee-filter-grid-body input[type="checkbox"]').not(':disabled');

//        checkboxes.prop('checked', isChecked);
//        updateSelectedEmployeeIds();
//    });
//    $('#employee-filter-grid-body').on('change', 'input[type="checkbox"]', function () {
//        updateSelectedEmployeeIds();
//        updateSelectAllCheckboxState();
//    });
//}

//function updateSelectedNoticeIds() {
//    const currentPageCheckboxes = $('#notice-grid-body input[type="checkbox"]');

//    currentPageCheckboxes.each(function () {

//        const id = $(this).data('id');

//        if ($(this).is(':checked')) {
//            selectedNoticeIds.add(id);
//        } else {
//            selectedNoticeIds.delete(id);
//        }
//    });
//}
//function updateSelectedEmployeeIds() {
//    const currentPageCheckboxes = $('#employee-filter-grid-body input[type="checkbox"]');
//    currentPageCheckboxes.each(function () {
//        const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();

//        if ($(this).is(':checked')) {
//            selectedEmpIds.add(employeeId);
//        } else {
//            selectedEmpIds.delete(employeeId);
//        }
//    });
//}

//function toArray(value) {
//    if (!value) return [];
//    if (Array.isArray(value)) return value;
//    return [value];
//}

//function getAllFilterVal() {
//    const filterData = {
//        CompanyCodes: toArray($("#companySelect").val()),
//        BranchCodes: toArray($("#branchSelect").val()),
//        DepartmentCodes: toArray($("#departmentSelect").val()),
//        DesignationCodes: toArray($("#designationSelect").val()),
//        EmployeeCodes: toArray($("#employeeSelect").val())
//    }
//    return filterData;
//}

//function loadAllFilterEmp() {
//    showLoading();
//    const filterData = getAllFilterVal();

//    $.ajax({
//        url: `/ManageNoticeEntry/getFilterEmp`,
//        type: "POST",
//        contentType: "application/json",
//        data: JSON.stringify(filterData),
//        success: function (res) {
//            const data = res.lookupData;

//            loadTableData(res.employees);
//            if (data.companies?.length) {
//                populateSelect("#companySelect", data.companies)
//            }
//            if (data.departments?.length) {
//                populateSelect("#departmentSelect", data.departments);
//            }
//            if (data.designations?.length) {
//                populateSelect("#designationSelect", data.designations);
//            }
//            if (data.employees?.length) {
//                populateSelect("#employeeSelect", data.employees)
//            }
//            if (data.branches?.length) {
//                populateSelect("#branchSelect", data.branches)
//            }
//            setupClearOnChangeEvents();

//            bindFilterChangeOnce();
//        },
//        complete: function () {
//            hideLoading();
//        },
//        error: function (xhr, status, error) {
//            console.error("Error loading filtered employees:", error);
//            hideLoading();
//        }
//    });
//}

//function populateSelect(selectId, dataList) {
//    const $select = $(selectId);
//    dataList.forEach(item => {
//        if (item.code && item.name && $select.find(`option[value="${item.code}"]`).length === 0) {
//            $select.append(`<option value="${item.code}">${item.name}</option>`);
//        }
//    });
//    $select.multiselect('rebuild');
//}


//function setupClearOnChangeEvents() {
//    const clearMap = {
//        "#companySelect": ["#branchSelect", "#departmentSelect", "#designationSelect", "#employeeSelect"],
//        "#branchSelect": ["#departmentSelect", "#designationSelect","#employeeSelect"],
//        "#departmentSelect": ["#designationSelect", "#employeeSelect"],
//    };

//    Object.entries(clearMap).forEach(([parent, children]) => {
//        $(parent).off("change.clearDropdowns").on("change.clearDropdowns", function () {
//            children.forEach(child => $(child).empty().multiselect('rebuild'));
//        });
//    });
//}

//function bindFilterChangeOnce() {
//    if (!filterChangeBound) {
//        $("#companySelect, #branchSelect, #departmentSelect, #designationSelect, #employeeSelect")
//            .on("change.loadFilter", function () {
//                loadAllFilterEmp();
//            });
//        filterChangeBound = true;
//    }
//}

//function loadTableData(res) {
//    var tableDataItem = res;
//    console.log(tableDataItem);
//    if ($.fn.DataTable.isDataTable('#employee-filter-grid') && empDataTable !== null) {
//        empDataTable.destroy();
//        empDataTable = null;
//    }

//    var tableBody = $("#employee-filter-grid-body");
//    tableBody.empty();

//    $.each(tableDataItem, function (index, employee) {
//        var row = $('<tr>');

//        const isOriginalEmployee = isEditMode && String(employee.employeeId) === String(originalEmpId);
//        const checkboxDisabled = isEditMode ? 'disabled' : '';
//        const checkboxChecked = isOriginalEmployee ? 'checked' : '';

//        row.append(`<td class="text-center"><input type="checkbox" style="padding-left:0; padding-right:0" class="empSelect" ${checkboxDisabled} ${checkboxChecked} /></td>`);
//        row.append('<td class="p-1 text-center">' + employee.employeeId + '</td>');
//        row.append('<td class="p-1 text-start">' + employee.employeeName + '</td>');
//        row.append('<td class="p-1 text-start">' + employee.designationName + '</td>');
//        row.append('<td class="p-1 text-center">' + employee.departmentName + '</td>');
//        row.append('<td class="p-1 text-center">' + employee.branchName + '</td>');
//        row.append('<td class="p-1 text-center">' + employee.employeeTypeName + '</td>');
//        row.append('<td class="p-1 text-center">' + employee.employmentNature + '</td>');
//        row.append('<td class="p-1 text-center">' + employee.joiningDate + '</td>');
//        row.append('<td class="p-1 text-center">' + employee.employeeStatus + '</td>');

//        tableBody.append(row);
//    });

//    initializeDataTable();

//    if (isEditMode) {
//        $('#employee-check-all').prop('disabled', true)
//    }
//}

//function initializeDataTable() {
//    empDataTable = $("#employee-filter-grid").DataTable({
//        paging: true,
//        pageLength: 10,
//        lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
//        lengthChange: true,
//        info: true,
//        autoWidth: false,
//        responsive: true,
//        fixedHeader: false,
//        columnDefs: [
//            {
//                targets: 0,
//                orderable: false,
//                className: 'no-sort'
//            }
//        ],
//        initComplete: function () {
//            hideLoading();
//            $('#custom-search').on('keyup', function () {
//                empDataTable.search(this.value).draw();
//            });

//            $('.dataTables_filter input').css({
//                'width': '250px',
//                'padding': '6px 12px',
//                'border': '1px solid #ddd',
//                'border-radius': '4px',
//            });

//            $('#employee-filter-grid_wrapper .dataTables_filter').hide();
//        },
//        drawCallback: function () {
//            $('#employee-filter-grid-body input[type="checkbox"]').each(function () {
//                const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();
//                $(this).prop('checked', selectedEmpIds.has(employeeId));
//            });

//            updateSelectAllCheckboxState();
//        }
//    });
//}

//function updateSelectAllCheckboxState() {
//    const total = $('#employee-filter-grid-body input[type="checkbox"]').not(':disabled').length;
//    const checked = $('#employee-filter-grid-body input[type="checkbox"]').not(':disabled').filter(':checked').length;

//    $("#employee-check-all").prop('checked', total > 0 && total === checked);
//}

//function initializeEmployeeGrid() {
//    showLoading();
//    setTimeout(function () {
//        if (empDataTable !== null) {
//            empDataTable.destroy();
//        }
//        initializeDataTable();
//    }, 100);
//}


//function initializeMultiselects() {
//    const nonSelectedTextMap = {
//        companySelect: 'Select Company',
//        departmentSelect: 'Select Department',
//        designationSelect: 'Select Designation',
//        branchSelect: 'Select Branch',
//        employeeSelect: 'Select Employee'
//    };

//    Object.keys(nonSelectedTextMap).forEach(function (id) {
//        const selector = $('#' + id);
//        selector.multiselect({
//            enableFiltering: true,
//            includeSelectAllOption: true,
//            selectAllText: 'Select All',
//            nonSelectedText: nonSelectedTextMap[id],
//            nSelectedText: 'Selected',
//            allSelectedText: 'All Selected',
//            filterPlaceholder: 'Search',
//            buttonWidth: '100%',
//            maxHeight: 350,
//            filterBehavior: 'text',
//            enableCaseInsensitiveFiltering: true,
//            buttonText: function (options, select) {
//                if (options.length === 0) {
//                    return nonSelectedTextMap[id];
//                }
//                else if (options.length > 1) {
//                    return options.length + ' Selected';
//                }
//                else {
//                    return $(options[0]).text();
//                }
//            }
//        });
//    });
//}

//function loadNoticeId() {
//    $.ajax({
//        url: "/ManageNoticeEntry/GenerateNewId",
//        type: "GET",
//        dataType: "json",
//        success: function (data) {
//            if (data) {
//                $("#NoticeId").val(data);
//            }
//        },
//        error: function (xhr, status, error) {
//            console.error("Error fetching Notice Id: ", error);
//        }
//    });
//}

//function loadNoticeData() {
//    showLoading();
//    displayNoticeTable();
//    hideLoading();
//}

//function displayNoticeTable() {
//    if ($.fn.DataTable.isDataTable("#notice-grid")) {
//        $("#notice-grid").DataTable().clear().destroy();
//    }

//    const tableBody = $("#notice-grid-body");
//    tableBody.empty();

//    $('#notice-grid').DataTable({
//        processing: true,
//        serverSide: true,
//        ajax: {
//            url: '/ManageNoticeEntry/GetPaginatedData',
//            type: 'POST',
//            //success: function (data) { console.log(data) }
//        },
//        columns: [
//            {
//                data: null,
//                orderable: false,
//                className: 'text-center',
//                render: function (data, type, row) {
//                    return `<input type="checkbox" width="1%" style="padding:0;" data-id="${row.tc}"/>`;
//                }
//            },
//            {
//                data: 'noticeId',
//                className: 'text-center',
//                render: function (data, type, row) {
//                    return `<a href="#notice-form" class="notice-id-link" data-id="${row.tc}">${data}</a>`;
//                }
//            },
//            { data: 'noticeTitle', className: 'text-left' },
//            { data: 'noticeDesc', className: 'text-left' },
//            {
//                data: 'entryDate', className: 'text-center',
//                render: function (data, type, row) {
//                    const date = new Date(data);
//                    const day = String(date.getDate()).padStart(2, '0');
//                    const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are zero-based
//                    const year = date.getFullYear();
//                    return `${day}/${month}/${year}`;
//                }
//            },
//            {
//                data: 'priorityLevel',
//                className: 'text-center',
//                render: function (data, type, row) {
//                    if (data === 1) {
//                        return '<span style="color: #b22222; font-weight: bold;">Important</span>';
//                    } else {
//                        return 'Normal';
//                    }
//                }
//            },
//            {
//                data: 'status', className: 'text-center',
//                render: function (data, type, row) {
//                    if (data === "1") {
//                        return "Active"
//                    } else {
//                        return 'Hold'
//                    }
//                }
//            },
//            {
//                data: 'filePath',
//                className: 'text-center',
//                render: function (data, type, row) {
//                    if (data && data.trim() !== '') {
//                        const fileName = data.split('/').pop().split('\\').pop();
//                        const fileExtension = fileName.split('.').pop().toLowerCase();

//                        return `<a href="/NoticeDocument/${data}" target="_blank" class="file-link" title="Click to open ${fileName}">
//                                    ${fileName}
//                                </a>`;
//                    } else {
//                        return '<span class="text-muted">No file</span>';
//                    }
//                }
//            }
//        ],
//        autoWidth: false,
//        fixedHeader: false,
//        info: true,
//        lengthChange: true,
//        lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
//        ordering: true,
//        pageLength: 10,
//        paging: true,
//        responsive: true,
//        scrollCollapse: true,
//        scrollX: true,
//        scrollY: "460px",
//        searching: true,
//        columnDefs: [
//            {
//                targets: 0,
//                orderable: false,
//                className: 'no-sort'
//            }
//        ],
//        language: {
//            search: "🔍 Search:",
//            lengthMenu: "Show _MENU_ entries",
//            info: "Showing _START_ to _END_ of _TOTAL_ entries",
//            paginate: {
//                first: "First",
//                previous: "Prev",
//                next: "Next",
//                last: "Last"
//            },
//            emptyTable: "No data available",
//            processing: "Loading data..."
//        },
//        initComplete: function () {
//            const api = this.api();
//            let debounceTimeout;

//            $('.dataTables_filter input')
//                .off()
//                .on('input', function () {
//                    clearTimeout(debounceTimeout);
//                    const searchTerm = this.value;

//                    debounceTimeout = setTimeout(function () {
//                        api.search(searchTerm).page('first').draw('page');
//                    }, 500);
//                });
//        },
//        drawCallback: function () {
//            $('#notice-grid-body input[type="checkbox"]').each(function () {
//                const id = $(this).data('id');
//                $(this).prop('checked', selectedNoticeIds.has(id));
//            });

//            const total = $('#notice-grid-body input[type="checkbox"]').length;
//            const checked = $('#notice-grid-body input[type="checkbox"]:checked').length;
//            $("#notice-check-all").prop('checked', total > 0 && total === checked);
//        }
//    });
//}

//function validateForm() {
//    if (!$("#NoticeTitle").val()) {
//        showNotification("Please enter Ot", "warning");
//        return false;
//    }

//    const fileInput = $('#fileUpload')[0];
//    if (fileInput && fileInput.files.length > 0) {
//        const file = fileInput.files[0];
//        const maxSize = 5 * 1024 * 1024;
//        const allowTypes = ['image/jpeg', 'image/png', 'image/gif', 'application/pdf'];

//        if (file.size > maxSize) {
//            showNotification("File size should not exceed 5MB", "warning");
//            return false;
//        }

//        if (!allowTypes.includes(file.type)) {
//            showNotification("Only JPG, PNG, GIF, PDF, DOC, DOCX files are allowed", "warning");
//            return false;
//        }
//    }
//    return true
//}

//function handleFormSubmission() {
//    if (!validateForm()) return;

//    const formData = new FormData();
//    //debugger;
//    formData.append('Tc', $('#Tc').val().trim() || 0);
//    formData.append('NoticeId', $('#NoticeId').val());
//    formData.append('NoticeTitle', $('#NoticeTitle').val());
//    formData.append('NoticeDesc', $('#NoticeDesc').val());
//    formData.append('Status', $('#Status').is(':checked') ? '1' : '0');
//    formData.append('EntryDate', $('#EntryDate').val());
//    formData.append('PriorityLevel', $('#PriorityLevel').val());

//    const fileInput = $('#fileUpload')[0];
//    if (fileInput && fileInput.files.length > 0) {
//        formData.append('formFile', fileInput.files[0]);
//    }

//    showLoading();
//    $.ajax({
//        url: `/ManageNoticeEntry/SaveNotice`,
//        type: "POST",
//        data: formData,
//        processData: false,
//        contentType: false,
//        success: function (response) {
//            showNotification("Data Saved Successfully.", "success");
//            loadNoticeData();
//            clearForm();
//            loadNoticeId();

//            //$('#NoticeForm')[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
//            //$('#Month').focus();
//        },
//        error: function (xhr, status, error) {
//            showNotification(`Error saving data`, "error");
//            console.log(`Error saving data `, error);
//        },
//        complete: hideLoading
//    })
//}

//function validateEmailForm() {
//    if (selectedEmpIds.size ===0) {
//        showNotification("Please select at least one employee", "warning");
//        $('#employee-filter-grid-body').focus();
//        return false;
//    }

//    if (selectedNoticeIds.size === 0) {
//        showNotification("Please select a notice to sent.", "warning");
//        $('#notice-grid').focus();
//        return false;
//    }
//    return true;
//}
//function handleEmailSentBtn() {
//    if (!validateEmailForm()) return;

//    const EmployeeIds = Array.from(selectedEmpIds);
//    const Tcs = Array.from(selectedNoticeIds);

//    dataToSend = {
//        EmployeeIds, Tcs
//    }

//    showLoading();

//    console.log(JSON.stringify(dataToSend));

//    $.ajax({
//        url: `/ManageNoticeEntry/SendNoticeToEmployee`,
//        type: "POST",
//        contentType: "application/json",
//        data: JSON.stringify(dataToSend),
//        success: function (res) {
//            showLoading("Email Sent Successfully", "success");
//            clearForm();
//            loadNoticeId();
//        },
//        error: function (xhr, status, error) {
//            showNotification(`Error saving data`, "error");
//            console.log(`Error saving data `, error);
//        },
//        complete: hideLoading
//    })
//    //const
//}

//function clearForm() {
//    isEditMode = false;
//    $('#fileUpload').val('');
//    $('#Tc').val('');
//    $('#NoticeId').val('');
//    $('#NoticeTitle').val('');
//    $('#NoticeDesc').val('');
//    $('#Status').prop('checked',false);
//    $('#EntryDate').val('');
//    $('#PriorityLevel').val('');
//    $('#employee-filter-grid-body input[type="checkbox"]').prop('checked', false).prop('disabled', false);
//    $('#employee-check-all').prop('checked', false).prop('disabled', false);
//    $('#notice-grid-body input[type="checkbox"]').prop('checked', false).prop('disabled', false);
//    $('#notice-check-all').prop('checked', false).prop('disabled', false);
//    selectedNoticeIds.clear();
//    selectedEmpIds.clear();
//}

//function populateForm(id) {
//    $.ajax({
//        url: `/ManageNoticeEntry/GetById/${id}`,
//        type: "GET",
//        success: function (res) {
//            if (!res || !res.data) {
//                showNotification("Invalid data format received", "error");
//                return;
//            }

//            const data = res.data;
//            console.log(data);
//            clearForm();

//            isEditMode = true;

//            $('#Tc').val(data.tc);
//            $('#NoticeId').val(data.noticeId);
//            $('#NoticeTitle').val(data.noticeTitle);
//            $('#NoticeDesc').val(data.noticeDesc);
//            $('#Status').prop('checked', data.status === '1');
//            $('#PriorityLevel').val(data.priorityLevel);
//        }
//    });
//}

//function handleBulkDelete() {
//    console.log(selectedNoticeIds);
//    const selectedIds = Array.from(selectedNoticeIds);

//    if (selectedNoticeIds.length === 0) {
//        showNotification("Please select record to delete", "warning");
//        return;
//    }

//    if (!confirm(`Are you sure you want to delete ${selectedNoticeIds.length} selected monthly OT(s)?`)) {
//        return;
//    }

//    showLoading();

//    $.ajax({
//        url: `/ManageNoticeEntry/BulkDelete`,
//        type: "POST",
//        contentType: "application/json",
//        data: JSON.stringify({ Tcs: selectedIds }),
//        success: function (response) {
//            selectedNoticeIds.clear();
//            showNotification(response.message || "Successfully deleted", "success");
//            loadNoticeData();
//            loadNoticeId();
//            clearForm();
//        },
//        error: function (xhr, status, error) {
//            console.error("Error details:", xhr.responseText);
//            showNotification("Error deleting Monthly OT records", "error");
//        },
//        complete: hideLoading
//    });
//}
//function showNotification(message, type) {
//    if (typeof toastr !== 'undefined') {
//        toastr[type](message, type === 'success' ? 'Success' : type === 'error' ? 'Error' : 'Warning');
//    } else {
//        alert(message);
//    }
//}




(function ($) {
    $.noticeManagement = function (options) {
        
        const settings = $.extend({
            baseUrl: "/",
            noticeForm: "#notice-form",
            tcField: "#Tc",
            noticeIdField: "#NoticeId",
            noticeTitleField: "#NoticeTitle",
            noticeDescField: "#NoticeDesc",
            statusField: "#Status",
            entryDateField: "#EntryDate",
            priorityLevelField: "#PriorityLevel",
            fileUploadField: "#fileUpload",

            companySelect: "#companySelect",
            branchSelect: "#branchSelect",
            departmentSelect: "#departmentSelect",
            designationSelect: "#designationSelect",
            employeeSelect: "#employeeSelect",

            employeeGrid: "#employee-filter-grid",
            employeeGridBody: "#employee-filter-grid-body",
            employeeCheckAll: "#employee-check-all",
            noticeGrid: "#notice-grid",
            noticeGridBody: "#notice-grid-body",
            noticeCheckAll: "#notice-check-all",
            customSearch: "#custom-search",

            saveBtn: ".js-notice-dec-save",
            deleteBtn: ".js-notice-dec-delete-confirm",
            clearBtn: ".js-notice-dec-clear",
            sendMailBtn: "#sendMail",

            load: () => console.log("Loading notice management..."),
            onSave: () => console.log("Notice saved successfully"),
            onDelete: () => console.log("Notice deleted successfully"),
            onEmailSent: () => console.log("Email sent successfully")
        }, options);

        const URLS = {
            filterEmp: `${settings.baseUrl}/getFilterEmp`,
            generateId: `${settings.baseUrl}/GenerateNewId`,
            saveNotice: `${settings.baseUrl}/SaveNotice`,
            getById: `${settings.baseUrl}/GetById`,
            bulkDelete: `${settings.baseUrl}/BulkDelete`,
            sendEmail: `${settings.baseUrl}/SendNoticeToEmployee`,
            paginatedData: `${settings.baseUrl}/GetPaginatedData`,
            getFile: `${settings.baseUrl}/GetNoticeFile`
        };

        let empDataTable = null;
        let filterChangeBound = false;
        let originalEmpId = null;
        let selectedEmpIds = new Set();
        let selectedNoticeIds = new Set();
        let isEditMode = false;

        const toArray = val => val ? (Array.isArray(val) ? val : [val]) : [];
        const showLoading = () => $("#loadingOverlay").fadeIn(200);
        const hideLoading = () => $("#loadingOverlay").fadeOut(200);

        const showToast = (type, message) => {
            if (typeof toastr !== 'undefined') {
                const title = type === 'success' ? 'Success' : type === 'error' ? 'Error' : 'Warning';
                toastr[type](message, title);
            } else {
                alert(message);
            }
        };

        const setupLoadingOverlay = () => {
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
                            <div class="spinner-border text-primary" role="status"></div>
                        </div>
                    </div>
                `);
            }
        };

        const initializeMultiselects = () => {
            const multiselectConfig = {
                companySelect: 'Company',
                branchSelect: 'Branch',
                departmentSelect: 'Department',
                designationSelect: 'Designation',
                employeeSelect: 'Employee'
            };

            Object.entries(multiselectConfig).forEach(([id, text]) => {
                $(`#${id}`).multiselect({
                    enableFiltering: true,
                    includeSelectAllOption: true,
                    selectAllText: 'Select All',
                    nonSelectedText: `Select ${text}`,
                    nSelectedText: 'Selected',
                    allSelectedText: 'All Selected',
                    filterPlaceholder: 'Search',
                    buttonWidth: '100%',
                    maxHeight: 350,
                    filterBehavior: 'text',
                    enableCaseInsensitiveFiltering: true,
                    buttonText: function (options, select) {
                        if (options.length === 0) {
                            return `Select ${text}`;
                        } else if (options.length > 1) {
                            return options.length + ' Selected';
                        } else {
                            return $(options[0]).text();
                        }
                    }
                });
            });
        };

        const getFilterValue = () => ({
            CompanyCodes: toArray($(settings.companySelect).val()),
            BranchCodes: toArray($(settings.branchSelect).val()),
            DepartmentCodes: toArray($(settings.departmentSelect).val()),
            DesignationCodes: toArray($(settings.designationSelect).val()),
            EmployeeCodes: toArray($(settings.employeeSelect).val())
        });

        const populateSelect = (selector, dataList) => {
            const $select = $(selector);
            dataList.forEach(item => {
                if (item.code && item.name && $select.find(`option[value="${item.code}"]`).length === 0) {
                    $select.append(`<option value="${item.code}">${item.name}</option>`);
                }
            });
            $select.multiselect('rebuild');
        };

        const clearDependentDropdowns = selectors => {
            selectors.forEach(selector => {
                $(selector).multiselect('deselectAll', false);
                $(selector).empty().multiselect('rebuild');
            });
        };

        const setupCascadingClearEvents = () => {
            const clearMap = {
                [settings.companySelect]: [settings.branchSelect, settings.departmentSelect, settings.designationSelect, settings.employeeSelect],
                [settings.branchSelect]: [settings.departmentSelect, settings.designationSelect, settings.employeeSelect],
                [settings.departmentSelect]: [settings.designationSelect, settings.employeeSelect],
                [settings.designationSelect]: [settings.employeeSelect]
            };

            Object.entries(clearMap).forEach(([parent, children]) => {
                $(parent).off("change.clearDropdowns").on("change.clearDropdowns", function () {
                    clearDependentDropdowns(children);
                });
            });
        };

        const bindFilterChangeEvents = () => {
            if (!filterChangeBound) {
                const filterSelectors = [
                    settings.companySelect,
                    settings.branchSelect,
                    settings.departmentSelect,
                    settings.designationSelect,
                    settings.employeeSelect
                ].join(',');

                $(filterSelectors).on("change.loadFilter", function () {
                    loadFilterEmp();
                });

                filterChangeBound = true;
            }
        };

        const loadFilterEmp = () => {
            showLoading();

            $.ajax({
                url: URLS.filterEmp,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(getFilterValue()),
                success: res => {
                    const data = res.lookupData;

                    loadTableData(res.employees);

                    console.log(data);

                    if (data.companies?.length) populateSelect(settings.companySelect, data.companies);
                    if (data.departments?.length) populateSelect(settings.departmentSelect, data.departments);
                    if (data.designations?.length) populateSelect(settings.designationSelect, data.designations);
                    if (data.employees?.length) populateSelect(settings.employeeSelect, data.employees);
                    if (data.branches?.length) populateSelect(settings.branchSelect, data.branches);

                    setupCascadingClearEvents();
                    bindFilterChangeEvents();
                },
                error: (xhr, status, error) => {
                    console.error("Error loading filtered employees:", error);
                    showToast('error', 'Failed to load employee data');
                },
                complete: hideLoading
            });
        };

        const loadTableData = (employees) => {
            if ($.fn.DataTable.isDataTable(settings.employeeGrid) && empDataTable !== null) {
                empDataTable.destroy();
                empDataTable = null;
            }

            const tableBody = $(settings.employeeGridBody);
            tableBody.empty();

            $.each(employees, function (index, employee) {
                const row = $('<tr>');
                const isOriginalEmployee = isEditMode && String(employee.employeeId) === String(originalEmpId);
                const checkboxDisabled = isEditMode ? 'disabled' : '';
                const checkboxChecked = isOriginalEmployee ? 'checked' : '';

                row.append(`<td class="text-center"><input type="checkbox" style="padding-left:0; padding-right:0" class="empSelect" ${checkboxDisabled} ${checkboxChecked} /></td>`);
                row.append('<td class="p-1 text-center">' + employee.employeeId + '</td>');
                row.append('<td class="p-1 text-start">' + employee.employeeName + '</td>');
                row.append('<td class="p-1 text-start">' + employee.designationName + '</td>');
                row.append('<td class="p-1 text-center">' + employee.departmentName + '</td>');
                row.append('<td class="p-1 text-center">' + employee.branchName + '</td>');
                row.append('<td class="p-1 text-center">' + employee.employeeTypeName + '</td>');
                row.append('<td class="p-1 text-center">' + employee.employmentNature + '</td>');
                row.append('<td class="p-1 text-center">' + employee.joiningDate + '</td>');
                row.append('<td class="p-1 text-center">' + employee.employeeStatus + '</td>');

                tableBody.append(row);
            });

            initializeDataTable();

            if (isEditMode) {
                $(settings.employeeCheckAll).prop('disabled', true);
            }
        };

        const initializeDataTable = () => {
            empDataTable = $(settings.employeeGrid).DataTable({
                paging: true,
                pageLength: 10,
                lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
                lengthChange: true,
                info: true,
                autoWidth: false,
                responsive: true,
                fixedHeader: false,
                columnDefs: [
                    {
                        targets: 0,
                        orderable: false,
                        className: 'no-sort'
                    }
                ],
                initComplete: function () {
                    hideLoading();
                    $(settings.customSearch).on('keyup', function () {
                        empDataTable.search(this.value).draw();
                    });

                    $('.dataTables_filter input').css({
                        'width': '250px',
                        'padding': '6px 12px',
                        'border': '1px solid #ddd',
                        'border-radius': '4px',
                    });

                    $(`${settings.employeeGrid}_wrapper .dataTables_filter`).hide();
                },
                drawCallback: function () {
                    $(`${settings.employeeGridBody} input[type="checkbox"]`).each(function () {
                        const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();
                        $(this).prop('checked', selectedEmpIds.has(employeeId));
                    });

                    updateSelectAllCheckboxState();
                }
            });
        };

        const updateSelectAllCheckboxState = () => {
            const total = $(`${settings.employeeGridBody} input[type="checkbox"]`).not(':disabled').length;
            const checked = $(`${settings.employeeGridBody} input[type="checkbox"]`).not(':disabled').filter(':checked').length;

            $(settings.employeeCheckAll).prop('checked', total > 0 && total === checked);
        };

        const updateSelectedEmployeeIds = () => {
            const currentPageCheckboxes = $(`${settings.employeeGridBody} input[type="checkbox"]`);
            currentPageCheckboxes.each(function () {
                const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();

                if ($(this).is(':checked')) {
                    selectedEmpIds.add(employeeId);
                } else {
                    selectedEmpIds.delete(employeeId);
                }
            });
        };

        const updateSelectedNoticeIds = () => {
            const currentPageCheckboxes = $(`${settings.noticeGridBody} input[type="checkbox"]`);

            currentPageCheckboxes.each(function () {
                const id = $(this).data('id');

                if ($(this).is(':checked')) {
                    selectedNoticeIds.add(id);
                } else {
                    selectedNoticeIds.delete(id);
                }
            });
        };

        const loadNoticeId = () => {
            $.ajax({
                url: URLS.generateId,
                type: "GET",
                dataType: "json",
                success: data => {
                    if (data) {
                        $(settings.noticeIdField).val(data);
                    }
                },
                error: (xhr, status, error) => {
                    console.error("Error fetching Notice Id: ", error);
                    showToast('error', 'Failed to generate notice ID');
                }
            });
        };

        const displayNoticeTable = () => {
            if ($.fn.DataTable.isDataTable(settings.noticeGrid)) {
                $(settings.noticeGrid).DataTable().clear().destroy();
            }

            const tableBody = $(settings.noticeGridBody);
            tableBody.empty();

            $(settings.noticeGrid).DataTable({
                processing: true,
                serverSide: true,
                ajax: {
                    url: URLS.paginatedData,
                    type: 'POST'
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
                        data: 'noticeId',
                        className: 'text-center',
                        render: function (data, type, row) {
                            return `<a href="#notice-form" class="notice-id-link" data-id="${row.tc}">${data}</a>`;
                        }
                    },
                    { data: 'noticeTitle', className: 'text-left' },
                    { data: 'noticeDesc', className: 'text-left' },
                    {
                        data: 'entryDate',
                        className: 'text-center',
                        render: function (data, type, row) {
                            const date = new Date(data);
                            const day = String(date.getDate()).padStart(2, '0');
                            const month = String(date.getMonth() + 1).padStart(2, '0');
                            const year = date.getFullYear();
                            return `${day}/${month}/${year}`;
                        }
                    },
                    {
                        data: 'priorityLevel',
                        className: 'text-center',
                        render: function (data, type, row) {
                            return data === 1 ?
                                '<span style="color: #b22222; font-weight: bold;">Important</span>' :
                                'Normal';
                        }
                    },
                    {
                        data: 'status',
                        className: 'text-center',
                        render: function (data, type, row) {
                            return data === "1" ? "Active" : 'Hold';
                        }
                    },
                    {
                        data: 'filePath',
                        className: 'text-center',
                        render: function (data, type, row) {
                            if (data && data.trim() !== '') {
                                const fileName = data.split('/').pop().split('\\').pop();
                                const fileExtension = fileName.split('.').pop().toLowerCase();

                                return `<a href="/NoticeDocument/${data}" target="_blank" class="file-link" title="Click to open ${fileName}">
                                            ${fileName}
                                        </a>`;
                            } else {
                                return '<span class="text-muted">No file</span>';
                            }
                        }
                    }
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
                    $(`${settings.noticeGridBody} input[type="checkbox"]`).each(function () {
                        const id = $(this).data('id');
                        $(this).prop('checked', selectedNoticeIds.has(id));
                    });

                    const total = $(`${settings.noticeGridBody} input[type="checkbox"]`).length;
                    const checked = $(`${settings.noticeGridBody} input[type="checkbox"]:checked`).length;
                    $(settings.noticeCheckAll).prop('checked', total > 0 && total === checked);
                }
            });
        };

        const validateForm = () => {
            if (!$(settings.noticeTitleField).val()) {
                showToast("warning", "Please enter Notice Title");
                return false;
            }

            const fileInput = $(settings.fileUploadField)[0];
            if (fileInput && fileInput.files.length > 0) {
                const file = fileInput.files[0];
                const maxSize = 5 * 1024 * 1024;
                const allowTypes = ['image/jpeg', 'image/png', 'image/gif', 'application/pdf'];

                if (file.size > maxSize) {
                    showToast("warning", "File size should not exceed 5MB");
                    return false;
                }

                if (!allowTypes.includes(file.type)) {
                    showToast("warning", "Only JPG, PNG, GIF, PDF files are allowed");
                    return false;
                }
            }
            return true;
        };

        const validateEmailForm = () => {
            if (selectedEmpIds.size === 0) {
                showToast("warning", "Please select at least one employee");
                $(settings.employeeGridBody).focus();
                return false;
            }

            if (selectedNoticeIds.size === 0) {
                showToast("warning", "Please select a notice to send");
                $(settings.noticeGrid).focus();
                return false;
            }
            return true;
        };

        const handleFormSubmission = () => {
            if (!validateForm()) return;

            const formData = new FormData();
            formData.append('Tc', $(settings.tcField).val().trim() || 0);
            formData.append('NoticeId', $(settings.noticeIdField).val());
            formData.append('NoticeTitle', $(settings.noticeTitleField).val());
            formData.append('NoticeDesc', $(settings.noticeDescField).val());
            formData.append('Status', $(settings.statusField).is(':checked') ? '1' : '0');
            formData.append('EntryDate', $(settings.entryDateField).val());
            formData.append('PriorityLevel', $(settings.priorityLevelField).val());

            const fileInput = $(settings.fileUploadField)[0];
            if (fileInput && fileInput.files.length > 0) {
                formData.append('formFile', fileInput.files[0]);
            }

            showLoading();
            $.ajax({
                url: URLS.saveNotice,
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: response => {
                    showToast("success", "Data Saved Successfully");
                    loadNoticeData();
                    clearForm();
                    loadNoticeId();
                    settings.onSave();
                },
                error: (xhr, status, error) => {
                    showToast("error", "Error saving data");
                    console.error("Error saving data:", error);
                },
                complete: hideLoading
            });
        };

        const handleEmailSent = () => {
            if (!validateEmailForm()) return;

            const dataToSend = {
                EmployeeIds: Array.from(selectedEmpIds),
                Tcs: Array.from(selectedNoticeIds)
            };

            showLoading();

            $.ajax({
                url: URLS.sendEmail,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(dataToSend),
                success: res => {
                    showToast("success", "Email Sent Successfully");
                    clearForm();
                    loadNoticeId();
                    settings.onEmailSent();
                },
                error: (xhr, status, error) => {
                    showToast("error", "Error sending email");
                    console.error("Error sending email:", error);
                },
                complete: hideLoading
            });
        };

        const handleBulkDelete = () => {
            const selectedIds = Array.from(selectedNoticeIds);

            if (selectedIds.length === 0) {
                showToast("warning", "Please select records to delete");
                return;
            }

            if (!confirm(`Are you sure you want to delete ${selectedIds.length} selected notice(s)?`)) {
                return;
            }

            showLoading();

            $.ajax({
                url: URLS.bulkDelete,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ Tcs: selectedIds }),
                success: response => {
                    selectedNoticeIds.clear();
                    showToast("success", response.message || "Successfully deleted");
                    loadNoticeData();
                    loadNoticeId();
                    clearForm();
                    settings.onDelete();
                },
                error: (xhr, status, error) => {
                    console.error("Error details:", xhr.responseText);
                    showToast("error", "Error deleting notice records");
                },
                complete: hideLoading
            });
        };

        const populateForm = (id) => {
            $.ajax({
                url: `${URLS.getById}/${id}`,
                type: "GET",
                success: res => {
                    if (!res || !res.data) {
                        showToast("error", "Invalid data format received");
                        return;
                    }

                    const data = res.data;
                    clearForm();

                    isEditMode = true;

                    $(settings.tcField).val(data.tc);
                    $(settings.noticeIdField).val(data.noticeId);
                    $(settings.noticeTitleField).val(data.noticeTitle);
                    $(settings.noticeDescField).val(data.noticeDesc);
                    $(settings.statusField).prop('checked', data.status === '1');
                    $(settings.priorityLevelField).val(data.priorityLevel);
                },
                error: (xhr, status, error) => {
                    showToast("error", "Error loading notice data");
                    console.error("Error loading notice:", error);
                }
            });
        };

        const clearForm = () => {
            isEditMode = false;
            $(settings.fileUploadField).val('');
            $(settings.tcField).val('');
            $(settings.noticeIdField).val('');
            $(settings.noticeTitleField).val('');
            $(settings.noticeDescField).val('');
            $(settings.statusField).prop('checked', false);
            $(settings.entryDateField).val('');
            $(settings.priorityLevelField).val('');
            $(`${settings.employeeGridBody} input[type="checkbox"]`).prop('checked', false).prop('disabled', false);
            $(settings.employeeCheckAll).prop('checked', false).prop('disabled', false);
            $(`${settings.noticeGridBody} input[type="checkbox"]`).prop('checked', false).prop('disabled', false);
            $(settings.noticeCheckAll).prop('checked', false).prop('disabled', false);
            selectedNoticeIds.clear();
            selectedEmpIds.clear();
        };

        const loadNoticeData = () => {
            showLoading();
            displayNoticeTable();
            hideLoading();
        };

        const setupEnterKeyNavigation = () => {
            const $form = $(settings.noticeForm);
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
        };

        const bindUIEvents = () => {
            // Filter change events
            $(`${settings.companySelect}, ${settings.departmentSelect}, ${settings.designationSelect}`).on("change", loadFilterEmp);

            // Form events
            $(settings.saveBtn).on('click', handleFormSubmission);
            $(settings.deleteBtn).on('click', handleBulkDelete);
            $(settings.sendMailBtn).on('click', handleEmailSent);
            $(settings.clearBtn).on('click', () => {
                clearForm();
                loadNoticeId();
            });

            // Notice grid events
            $(document).on("click", ".notice-id-link", function () {
                const id = $(this).data("id");
                if (!id) return;
                populateForm(id);
            });

            // Checkbox events
            $(settings.noticeCheckAll).on('change', function () {
                const isChecked = $(this).is(':checked');
                $(`${settings.noticeGridBody} input[type="checkbox"]`).prop('checked', isChecked);
                updateSelectedNoticeIds();
            });

            $(settings.noticeGridBody).on('change', 'input[type="checkbox"]', function () {
                const total = $(settings.noticeGridBody).find('input[type="checkbox"]').length;
                const checked = $(settings.noticeGridBody).find('input[type="checkbox"]:checked').length;

                $(settings.noticeCheckAll).prop('checked', total === checked);
                updateSelectedNoticeIds();
            });

            $(settings.employeeCheckAll).on('change', function () {
                const isChecked = $(this).is(':checked');
                const checkboxes = $(`${settings.employeeGridBody} input[type="checkbox"]`).not(':disabled');

                checkboxes.prop('checked', isChecked);
                updateSelectedEmployeeIds();
            });

            $(settings.employeeGridBody).on('change', 'input[type="checkbox"]', function () {
                updateSelectedEmployeeIds();
                updateSelectAllCheckboxState();
            });
        };

        const initializeEmployeeGrid = () => {
            showLoading();
            setTimeout(function () {
                if (empDataTable !== null) {
                    empDataTable.destroy();
                }
                initializeDataTable();
            }, 100);
        };

        const init = () => {
            settings.load();

            setupLoadingOverlay();
            initializeMultiselects();
            bindUIEvents();
            initializeEmployeeGrid();
            loadFilterEmp();
            loadNoticeId();
            loadNoticeData();
            setupEnterKeyNavigation();

            $(window).on('load', function () {
                $(settings.noticeForm)[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
            });
        };

        init();
    };
})(jQuery);