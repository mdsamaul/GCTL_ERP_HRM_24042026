(function ($) {
    $.workingDayDeclaration = function (options) {
        const settings = $.extend({
            baseUrl: "/HrmServiceNotConfirmationEntry",
            companySelect: "#companySelect",
            employeeSelect: "#employeeSelect",

            empNameLabel: ".EmployeeName",
            empDepartmentLabel: ".EmployeeDepartment",
            empDesignationLabel: ".EmployeeDesignation",
            empGrossSalaryLabel: ".EmployeeGrossSalary",
            empJoinDateLabel: ".EmployeeJoinDate",
            empProbationPeriodLabel: ".EmployeeProbationPeriod",
            empEndOnLabel: ".EmployeeEndOn",
            empServiceLengthLabel: ".ServiceLength",

            tcInput: "#Tc",
            remarksInputInput: "#remarks",
            effectiveDateInput: "#effectiveDate",
            duePaymentDateInput: "#duePaymentDate",
            refLetterNoInput: "#refLetterNo",
            refLetterDateInput: "#refLetterDate",
            
            serviceNotConfirmGrid: "#serviceNotConfirm-grid",
            serviceNotConfirmGridBody: "#serviceNotConfirm-grid-body",

            formContainer: "#serviceNotConfirm-form",

            saveButton: ".js-serviceNotConfirm-dec-save",
            deleteButton: ".js-serviceNotConfirm-dec-delete-confirm",
            clearButton: ".js-serviceNotConfirm-dec-clear",

            checkAll: "#serviceNotConfirm-check-all",
            load: () => console.log("Loading Service not confirmation...")
        }, options);//

        const urls = {
            getFilterEmp: `${settings.baseUrl}/getFilterEmp`,
            getEmpData: `${settings.baseUrl}/GetEmpData`,
            newId:`${settings.baseUrl}/GenerateNewId`,
            save: `${settings.baseUrl}/SaveEntry`,
            getById: `${settings.baseUrl}/GetById`,
            bulkDelete: `${settings.baseUrl}/BulkDelete`,
            getPaginatedData: `${settings.baseUrl}/GetPaginatedEntries`
        };//

        // Plugin state
        let selectedEmpId = null;
        let selectedSNCId = new Set();
        let isEditMode = false;

        // Utility functions
        //const toArray = val => val ? (Array.isArray(val) ? val : [val]) : [];

        const showLoading = () => {
            $('body').css('overflow', 'hidden');
            $("#loadingOverlay").fadeIn(200);
        };//

        const hideLoading = () => {
            $('body').css('overflow', '');
            $("#loadingOverlay").fadeOut(200);
        };//

        const showNotification = (message, type) => {
            if (typeof toastr !== 'undefined') {
                toastr[type](message, type === 'success' ? 'Success' : type === 'error' ? 'Error' : 'Warning');
            } else {
                alert(message);
            }
        };//

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
        };//


        const setupEnterKeyNavigation = () => {
            if (!settings.formContainer.length) return;

            settings.formContainer.on('keydown', 'input, select, textarea, button, [tabindex]:not([tabindex="-1"])', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();

                    const $focusable = settings.formContainer
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
        }//


        const getAllFilterVal = () => ({
            CompanyCodes: toArray($(settings.companySelect).val()),
            BranchCodes: toArray($(settings.branchSelect).val()),
            DivisionCodes: toArray($(settings.divisionSelect).val()),
            DepartmentCodes: toArray($(settings.departmentSelect).val()),
            DesignationCodes: toArray($(settings.designationSelect).val()),
            EmployeeIDs: toArray($(settings.employeeSelect).val()),
            EmployeeStatuses: toArray($(settings.activityStatusSelect).val())
        });

        const populateSelect = (selectId, dataList) => {
            const $select = $(selectId);
            dataList.forEach(item => {
                if (item.code && item.name && $select.find(`option[value="${item.code}"]`).length === 0) {
                    $select.append(`<option value="${item.code}">${item.name}</option>`);
                }
            });
            $select.multiselect('rebuild');
        };

        const setupCascadingClearEvents = () => {
            const clearMap = {
                [settings.branchSelect]: [settings.divisionSelect, settings.departmentSelect, settings.designationSelect, settings.employeeSelect],
                [settings.divisionSelect]: [settings.departmentSelect, settings.designationSelect, settings.employeeSelect],
                [settings.departmentSelect]: [settings.designationSelect, settings.employeeSelect],
                [settings.designationSelect]: [settings.employeeSelect]
            };

            Object.entries(clearMap).forEach(([parent, children]) => {
                $(parent).off("change.clearDropdowns").on("change.clearDropdowns", function () {
                    children.forEach(child => $(child).empty().multiselect('rebuild'));
                });
            });
        };

        const bindFilterChangeEvents = () => {
            if (!filterChangeBound) {
                const filterSelectors = [
                    settings.companySelect,
                    settings.branchSelect,
                    settings.divisionSelect,
                    settings.departmentSelect,
                    settings.designationSelect,
                    settings.employeeSelect,
                    settings.employeeStatusSelect,
                    settings.activityStatusSelect
                ].join(',');

                $(filterSelectors).on("change.loadFilter", function () {
                    loadAllFilterEmp();
                });
                filterChangeBound = true;
            }
        };

        const loadAllFilterEmp = () => {
            showLoading();
            const filterData = getAllFilterVal();

            $.ajax({
                url: urls.getFilterEmp,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(filterData),
                success: function (res) {
                    const data = res.lookupData;
                    console.log(res);
                    loadTableData(res);

                    if (data.companies?.length) {
                        populateSelect(settings.companySelect, data.companies);
                    }
                    if (data.branches?.length) {
                        populateSelect(settings.branchSelect, data.branches);
                    }
                    if (data.divisions?.length) {
                        populateSelect(settings.divisionSelect, data.divisions);
                    }
                    if (data.departments?.length) {
                        populateSelect(settings.departmentSelect, data.departments);
                    }
                    if (data.designations?.length) {
                        populateSelect(settings.designationSelect, data.designations);
                    }
                    if (data.employees?.length) {
                        populateSelect(settings.employeeSelect, data.employees);
                    }

                    setupCascadingClearEvents();
                    bindFilterChangeEvents();
                },
                complete: hideLoading,
                error: function (xhr, status, error) {
                    console.error("Error loading filtered employees:", error);
                    hideLoading();
                }
            });
        };

        const loadTableData = (res) => {
            const tableDataItem = res.employees;

            if ($.fn.DataTable.isDataTable(settings.employeeGrid) && employeeDataTable !== null) {
                employeeDataTable.destroy();
                employeeDataTable = null;
            }

            const tableBody = $(settings.employeeGridBody);
            tableBody.empty();

            $.each(tableDataItem, function (index, employee) {
                const row = $('<tr>');
                const isOriginalEmployee = isEditMode && String(employee.employeeId) === String(originalEmployeeId);
                const checkboxDisabled = isEditMode ? 'disabled' : '';
                const checkboxChecked = isOriginalEmployee ? 'checked' : '';

                row.append(`<td class="text-center"><input type="checkbox" class="empSelect" ${checkboxDisabled} ${checkboxChecked} /></td>`);
                row.append('<td class="text-center">' + employee.employeeId + '</td>');
                row.append('<td class="text-center">' + employee.employeeName + '</td>');
                row.append('<td class="text-center">' + employee.designationName + '</td>');
                row.append('<td class="text-center">' + employee.departmentName + '</td>');
                row.append('<td class="text-center">' + employee.branchName + '</td>');
                row.append('<td class="text-center">' + employee.employeeTypeName + '</td>');
                row.append('<td class="text-center">' + employee.joiningDate + '</td>');
                row.append('<td class="text-center">' + employee.employeeStatus + '</td>');

                tableBody.append(row);
            });

            initializeDataTable();

            if (isEditMode) {
                $(settings.employeeCheckAll).prop('disabled', true);
            }
        };

        const initializeDataTable = () => {
            employeeDataTable = $(settings.employeeGrid).DataTable({
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
                    $(settings.customSearch).on('keyup', function () {
                        employeeDataTable.search(this.value).draw();
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
                        $(this).prop('checked', selectedEmployeeIds.has(employeeId));
                    });

                    updateSelectAllCheckboxState();
                }
            });
        };

        const displayWorkingDateTable = () => {
            if ($.fn.DataTable.isDataTable(settings.workingDayGrid)) {
                $(settings.workingDayGrid).DataTable().clear().destroy();
            }

            const tableBody = $(settings.workingDayGridBody);
            tableBody.empty();

            $(settings.workingDayGrid).DataTable({
                processing: true,
                serverSide: true,
                ajax: {
                    url: urls.getPaginatedData,
                    type: 'POST',
                    data: function (d) { }
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
                            return `<a href="${settings.formContainer}" class="workingday-declaration-id-link" data-id="${row.tc}">${data}</a>`;
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
                    emptyTable: "No data available"
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
                    $(`${settings.workingDayGridBody} input[type="checkbox"]`).each(function () {
                        const id = $(this).data('id');
                        $(this).prop('checked', selectedWorkingDayIds.has(id));
                    });

                    const total = $(`${settings.workingDayGridBody} input[type="checkbox"]`).length;
                    const checked = $(`${settings.workingDayGridBody} input[type="checkbox"]:checked`).length;
                    $(settings.workingDayCheckAll).prop('checked', total > 0 && total === checked);
                }
            });
        };

        const updateSelectedEmployeeIds = () => {
            const currentPageCheckboxes = $(`${settings.employeeGridBody} input[type="checkbox"]`);
            currentPageCheckboxes.each(function () {
                const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();

                if ($(this).is(':checked')) {
                    selectedEmployeeIds.add(employeeId);
                } else {
                    selectedEmployeeIds.delete(employeeId);
                }
            });
        };

        const updateSelectAllCheckboxState = () => {
            const total = $(`${settings.employeeGridBody} input[type="checkbox"]`).not(':disabled').length;
            const checked = $(`${settings.employeeGridBody} input[type="checkbox"]`).not(':disabled').filter(':checked').length;

            $(settings.employeeCheckAll).prop('checked', total > 0 && total === checked);
        };

        const updateSelectedWorkingDayIds = () => {
            const currentPageCheckboxes = $(`${settings.workingDayGridBody} input[type="checkbox"]`);

            currentPageCheckboxes.each(function () {
                const id = $(this).data('id');

                if ($(this).is(':checked')) {
                    selectedWorkingDayIds.add(id);
                } else {
                    selectedWorkingDayIds.delete(id);
                }
            });
        };

        const validateForm = () => {
            if (selectedEmployeeIds.size === 0) {
                showNotification("Please select at least one employee", "info");
                $(settings.employeeGridBody).focus();
                return false;
            }

            if (selectedDates.length === 0) {
                showNotification("Please select a working date", "info");
                $(settings.workingDayDateInput).focus();
                return false;
            }

            return true;
        };

        const handleFormSubmission = () => {
            if (!validateForm()) {
                return;
            }

            const EmployeeIds = Array.from(selectedEmployeeIds);

            const dataToSend = {
                Tc: $(settings.tcInput).val() || 0,
                WorkingDayCode: '0',
                Remarks: $(settings.remarksInput).val(),
                WorkingDayDates: selectedDates,
                CompanyCode: $(settings.companySelect).val(),
                EmployeeIds: EmployeeIds
            };

            showLoading();
            console.log(JSON.stringify(dataToSend));

            $.ajax({
                url: urls.save,
                type: "POST",
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
                    $(settings.formContainer)[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
                    $(settings.workingDayDateInput).focus();
                },
                error: function (xhr, status, error) {
                    showNotification(`Error saving data`, "error");
                    console.log(`Error saving data `, error);
                },
                complete: hideLoading
            });
        };

        const clearForm = () => {
            isEditMode = false;
            $(settings.tcInput).val('');
            $(settings.workingDayDateInput).val('');
            $(settings.remarksInput).val('');
            $(settings.branchSelect).val(null).trigger('change');
            $(settings.activityStatusSelect).val(null).trigger('change');
            selectedWorkingDayIds.clear();
            selectedEmployeeIds.clear();
            originalEmployeeId = null;
            $(`${settings.employeeGridBody} input[type="checkbox"]`).prop('checked', false).prop('disabled', false);
            $(settings.employeeCheckAll).prop('checked', false).prop('disabled', false);

            initializeFlatpickr(isEditMode);
        };

        const populateForm = (id) => {
            $.ajax({
                url: `${urls.getById}/${id}`,
                type: "GET",
                success: function (response) {
                    //console.log(response);
                    if (!response || !response.data) {
                        showNotification("Invalid data format received", "error");
                        return;
                    }

                    try {
                        const data = response.data;
                        clearForm();

                        isEditMode = true;

                        if (settings.employeeSelect.find(`option[value="${data.code}".]`).length === 0) {
                            settings.employeeSelect.append(`<option value="${data.code}" selected>${data.name}</option>`);
                        }
                        settings.employeeSelect.prop('disabled', true);
                        settings.employeeSelect.val(data.employeeId).trigger('change');
                        settings.tcInput.val(data.tc);
                        settings.snc

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
        };
        const handleBulkDelete = () => {
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
                url: urls.bulkDelete,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ Tcs: selectedIds }),
                success: function (response) {
                    selectedWorkingDayIds.clear();
                    showNotification(response.message || "Successfully deleted items", "success");
                    loadWorkingDayData();
                    clearForm();
                },
                error: function (xhr, status, error) {
                    console.error("Error details:", xhr.responseText);
                    showNotification("Error deleting working day records", "error");
                },
                complete: hideLoading
            });
        };

        const loadWorkingDayData = () => {
            showLoading();
            displayWorkingDateTable();
            hideLoading();
        };

        const setupEnterKeyNavigation = () => {
            const $form = $(settings.formContainer);
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
        };

        const bindUIEvents = () => {
            const filterSelectors = [
                settings.companySelect,
                settings.branchSelect,
                settings.divisionSelect,
                settings.departmentSelect,
                settings.designationSelect,
                settings.employeeSelect,
                settings.employeeStatusSelect,
                settings.activityStatusSelect
            ].join(',');

            $(filterSelectors).on("change", function () {
                loadAllFilterEmp();
            });

            $(settings.clearButton).on('click', clearForm);
            $(settings.saveButton).on('click', handleFormSubmission);
            $(settings.deleteButton).on('click', handleBulkDelete);

            $(document).on("click", ".workingday-declaration-id-link", function () {
                const id = $(this).data("id");
                if (!id) return;
                populateForm(id);
            });

            $(settings.workingDayCheckAll).on('change', function () {
                const isChecked = $(this).is(':checked');
                $(`${settings.workingDayGridBody} input[type="checkbox"]`).prop('checked', isChecked);

                if (isChecked) {
                    $(`${settings.workingDayGridBody} input[type="checkbox"]`).each(function () {
                        selectedWorkingDayIds.add($(this).data('id'));
                    });
                } else {
                    $(`${settings.workingDayGridBody} input[type="checkbox"]`).each(function () {
                        selectedWorkingDayIds.delete($(this).data('id'));
                    });
                }
            });

            $(document).on('change', `${settings.workingDayGridBody} input[type="checkbox"]`, function () {
                const id = $(this).data('id');

                if ($(this).is(':checked')) {
                    selectedWorkingDayIds.add(id);
                } else {
                    selectedWorkingDayIds.delete(id);
                }

                const total = $(`${settings.workingDayGridBody} input[type="checkbox"]`).length;
                const checked = $(`${settings.workingDayGridBody} input[type="checkbox"]:checked`).length;
                $(settings.workingDayCheckAll).prop('checked', total > 0 && total === checked);
            });

            $(settings.employeeCheckAll).on('change', function () {
                const isChecked = $(this).is(':checked');
                const checkboxes = $(`${settings.employeeGridBody} input[type="checkbox"]`).not(':disabled');

                checkboxes.prop('checked', isChecked);
                updateSelectedEmployeeIds();
            });

            $(document).on('change', `${settings.employeeGridBody} input[type="checkbox"]`, function () {
                updateSelectedEmployeeIds();
                updateSelectAllCheckboxState();
            });

            $(settings.customSearch).on('keyup', function () {
                if (employeeDataTable) {
                    employeeDataTable.search(this.value).draw();
                }
            });

            setupEnterKeyNavigation();
        };

        const initializePlugin = () => {
            setupLoadingOverlay();
            initializeMultiselects();
            initializeFlatpickr(false);
            bindUIEvents();
            loadAllFilterEmp();
            loadWorkingDayData();

            if (typeof settings.load === 'function') {
                settings.load();
            }
        };

        const publicMethods = {
            init: initializePlugin,
            clearForm: clearForm,
            loadData: loadAllFilterEmp,
            loadWorkingDayData: loadWorkingDayData,
            destroy: () => {
                $(filterSelectors).off('.loadFilter');
                $(settings.clearButton).off('click');
                $(settings.saveButton).off('click');
                $(settings.deleteButton).off('click');
                $(settings.workingDayCheckAll).off('change');
                $(settings.employeeCheckAll).off('change');
                $(settings.customSearch).off('keyup');
                $(document).off('click', '.workingday-declaration-id-link');
                $(document).off('change', `${settings.workingDayGridBody} input[type="checkbox"]`);
                $(document).off('change', `${settings.employeeGridBody} input[type="checkbox"]`);

                if (employeeDataTable) {
                    employeeDataTable.destroy();
                    employeeDataTable = null;
                }

                if ($.fn.DataTable.isDataTable(settings.workingDayGrid)) {
                    $(settings.workingDayGrid).DataTable().destroy();
                }

                if (flatpickrInstance) {
                    flatpickrInstance.destroy();
                    flatpickrInstance = null;
                }

                selectedWorkingDayIds.clear();
                selectedEmployeeIds.clear();
                isEditMode = false;
                originalEmployeeId = null;
                selectedDates = [];
            },
            getSelectedEmployees: () => Array.from(selectedEmployeeIds),
            getSelectedWorkingDays: () => Array.from(selectedWorkingDayIds),
            setEditMode: (mode) => {
                isEditMode = mode;
                initializeFlatpickr(isEditMode);
            }
        };

        initializePlugin();

        return publicMethods;
    };

    $.fn.workingDayDeclaration = function (options) {
        return this.each(function () {
            if (!$.data(this, 'workingDayDeclaration')) {
                $.data(this, 'workingDayDeclaration', $.workingDayDeclaration(options));
            }
        });
    };

})(jQuery);