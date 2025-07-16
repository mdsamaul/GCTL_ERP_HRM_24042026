(function ($) {
    $.employeeServiceConfirmation = function (options) {
        const settings = $.extend({
            baseUrl: "/",
            companySelect: "#companySelect",
            branchSelect: "#branchSelect",
            departmentSelect: "#departmentSelect",
            designationSelect: "#designationSelect",
            empTypeSelect: "#empTypeSelect",
            empIdSelect: "#empIdSelect",
            probationEndDays: "#proPeriodSelect",
            confirmSelect: "#confirmSelect",
            employeeGrid: "#employee-filter-grid",
            employeeGridBody: "#employee-filter-grid-body",
            employeeCheckAll: "#employee-check-all",
            customSearch: "#custom-search",
            saveButton: ".js-serviceConfirm-dec-save",
            clearButton: ".js-serviceConfirm-dec-clear",
            load: () => console.log("Loading employee service confirmation...")
        }, options);

        const $elements = {
            companySelect: $(settings.companySelect),
            branchSelect: $(settings.branchSelect),
            departmentSelect: $(settings.departmentSelect),
            designationSelect: $(settings.designationSelect),
            empTypeSelect: $(settings.empTypeSelect),
            empIdSelect: $(settings.empIdSelect),
            probationEndDays: $(settings.probationEndDays),
            confirmSelect: $(settings.confirmSelect),
            employeeGrid: $(settings.employeeGrid),
            employeeGridBody: $(settings.employeeGridBody),
            employeeCheckAll: $(settings.employeeCheckAll),
            customSearch: $(settings.customSearch),
            saveButton: $(settings.saveButton),
            clearButton: $(settings.clearButton),
            loadingOverlay: null
        };

        const URLS = {
            filter: `${settings.baseUrl}/getFilterEmp`,
            save: `${settings.baseUrl}/Save`
        };

        const FILTER_SELECTORS = [
            $elements.companySelect,
            $elements.branchSelect,
            $elements.departmentSelect,
            $elements.designationSelect,
            $elements.empTypeSelect,
            $elements.probationEndDays
        ];

        const MULTISELECT_CONFIG = [
            [$elements.companySelect, 'Company'],
            [$elements.branchSelect, 'Branch'],
            [$elements.departmentSelect, 'Department'],
            [$elements.designationSelect, 'Designation'],
            [$elements.empTypeSelect, 'Emp. Type']
        ];

        const CASCADING_CLEAR_MAP = {
            branchSelect: [$elements.departmentSelect, $elements.designationSelect, $elements.empTypeSelect],
            departmentSelect: [$elements.designationSelect, $elements.empTypeSelect],
            designationSelect: [$elements.empTypeSelect]
        };

        let empDataTable = null;
        let selectedEmpIds = new Set();
        let flatpickrInstance = null;
        let currentAbortController = null;
        let debounceTimer = null;
        let isInitialLoad = true;

        const toArray = val => val ? [].concat(val) : [];

        const showLoading = () => {
            $('body').css('overflow', 'hidden');
            $elements.loadingOverlay.css("display", "flex");
        };

        const hideLoading = () => {
            $('body').css('overflow', '');
            $elements.loadingOverlay.hide();
        };

        const showNotification = (message, type) => {
            const title = type === 'success' ? 'Success' : type === 'error' ? 'Error' : 'Warning';
            typeof toastr !== 'undefined' ? toastr[type](message, title) : alert(message);
        };

        const debounce = (func, delay) => {
            return function (...args) {
                clearTimeout(debounceTimer);
                debounceTimer = setTimeout(() => func.apply(this, args), delay);
            };
        };

        const fetchWithErrorHandling = async (url, options = {}) => {
            if (currentAbortController) {
                currentAbortController.abort();
            }

            currentAbortController = new AbortController();

            try {
                const response = await fetch(url, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        ...options.headers
                    },
                    signal: currentAbortController.signal,
                    ...options
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                return await response.json();
            } catch (error) {
                if (error.name === 'AbortError') {
                    console.log('Request aborted');
                    return null;
                }
                console.error(`API request failed for ${url}:`, error);
                throw error;
            }
        };

        const apiPost = (url, data) => fetchWithErrorHandling(url, {
            method: 'POST',
            body: JSON.stringify(data)
        });

        const setupLoadingOverlay = () => {
            if ($("#employeeServiceLoadingOverlay").length === 0) {
                $("body").append(`
                    <div id="employeeServiceLoadingOverlay" style="display:none;position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.5);z-index:9999;justify-content:center;align-items:center;">
                        <div style="background:white;padding:20px;border-radius:5px;box-shadow:0 0 10px rgba(0,0,0,0.3);text-align:center;">
                            <div class="spinner-border text-primary"></div>
                        </div>
                    </div>
                `);
            }
            $elements.loadingOverlay = $("#employeeServiceLoadingOverlay");
        };

        const initializeFlatpickr = () => {
            if (typeof flatpickr === 'undefined') return;

            flatpickrInstance?.destroy();
            $elements.confirmSelect.val('');

            flatpickrInstance = flatpickr($elements.confirmSelect[0], {
                mode: "single",
                dateFormat: "d-m-Y",
                allowInput: false,
                clickOpens: true,
                onChange: (dates, dateStr) => {
                    if (dates.length) {
                        $elements.confirmSelect.val(dateStr);
                    }
                }
            });

            $elements.confirmSelect.on('keydown keypress keyup', e => {
                if (e.keyCode === 9 || e.keyCode === 27 || (e.keyCode >= 35 && e.keyCode <= 39)) {
                    return true;
                }
                e.preventDefault();
                return false;
            });
        };

        const initializeMultiselects = () => {
            const multiselectOptions = {
                enableFiltering: true,
                includeSelectAllOption: true,
                selectAllText: 'Select All',
                nSelectedText: 'Selected',
                allSelectedText: 'All Selected',
                filterPlaceholder: 'Search',
                buttonWidth: '100%',
                maxHeight: 350,
                filterBehavior: 'text',
                enableCaseInsensitiveFiltering: true,
                numberDisplayed: 1
            };

            MULTISELECT_CONFIG.forEach(([element, placeholder]) => {
                element.multiselect({
                    ...multiselectOptions,
                    nonSelectedText: `Select ${placeholder}`,
                    buttonText: (options, select) => {
                        if (options.length === 0) return `Select ${placeholder}`;
                        if (options.length > 0 && element.attr('id') !== 'companySelect') {
                            return options.length + ' Selected';
                        }
                        return $(options[0]).text();
                    }
                });
            });
        };

        const getFilterValue = () => ({
            CompanyCodes: toArray($elements.companySelect.val()),
            BranchCodes: toArray($elements.branchSelect.val()),
            DepartmentCodes: toArray($elements.departmentSelect.val()),
            DesignationCodes: toArray($elements.designationSelect.val()),
            EmployeeTypes: toArray($elements.empTypeSelect.val()),
            EmployeeIDs: toArray($elements.empIdSelect.val()),
            ProbationEndDays : $elements.probationEndDays.val()
        });

        const populateSelect = (element, dataList) => {
            const existingValues = new Set();
            element.find('option').each(function () {
                existingValues.add(this.value);
            });

            const fragment = document.createDocumentFragment();
            dataList.forEach(item => {
                if (item.code && item.name && !existingValues.has(item.code)) {
                    const option = document.createElement('option');
                    option.value = item.code;
                    option.textContent = item.name;
                    fragment.appendChild(option);
                }
            });

            if (fragment.hasChildNodes()) {
                element[0].appendChild(fragment);
                element.multiselect('rebuild');
            }
        };

        const clearDependentDropdowns = elements => {
            elements.forEach(element => {
                element.empty().multiselect('rebuild');
            });
        };

        const setupCascadingClearEvents = () => {
            Object.entries(CASCADING_CLEAR_MAP).forEach(([key, children]) => {
                const element = $elements[key.replace('Select', 'Select')];
                element.off("change.clearDropdowns").on("change.clearDropdowns", () => {
                    clearDependentDropdowns(children);
                });
            });
        };

        const loadTableData = (data) => {
            if ($.fn.DataTable.isDataTable(settings.employeeGrid) && empDataTable !== null) {
                empDataTable.destroy();
                empDataTable = null;
            }

            const tableRows = data.map(item => `
                <tr>
                    <td class="text-center p-0" width="1%">
                        <input type="checkbox" class="empSelect"/>
                    </td>
                    <td class="text-center p-1">${item.employeeId}</td>
                    <td class="p-1">${item.employeeName}</td>
                    <td class="text-center text-nowrap p-1">${item.designationName}</td>
                    <td class="text-center text-nowrap p-1">${item.departmentName}</td>
                    <td class="text-center text-nowrap p-1">${item.grossSalary}</td>
                    <td class="text-center text-nowrap p-1">${item.joiningDate}</td>
                    <td class="text-center text-nowrap p-1">${item.probationPeriod}</td>
                    <td class="text-center text-nowrap p-1">${item.probationPeriodEndOn}</td>
                    <td class="text-center text-nowrap p-1">${item.extenPeriod}</td>
                    <td class="text-center text-nowrap p-1">${item.extenPeriodEndOn}</td>
                    <td class="text-center text-nowrap p-1">${item.serviceLength}</td>
                    <td class="text-center p-1">
                        <input type="text" class="form-control form-control-sm mx-auto refLetter" 
                               value="${item.refLetterNo || ''}" />
                    </td>
                </tr>
            `).join('');

            $elements.employeeGridBody.html(tableRows);
            initializeDataTable();
        };

        const initializeDataTable = () => {
            empDataTable = $elements.employeeGrid.DataTable({
                paging: true,
                pageLength: 10,
                lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
                lengthChange: true,
                info: true,
                autoWidth: false,
                responsive: true,
                scrollX: true,
                columnDefs: [
                    { targets: [0, 12], orderable: false, className: 'no-sort' }
                ],
                initComplete() {
                    hideLoading();
                    $elements.customSearch.on('keyup', function () {
                        empDataTable.search(this.value).draw();
                    });
                    $('.dataTables_filter input').css({
                        'width': '250px',
                        'padding': '6px 12px',
                        'border': '1px solid #ddd',
                        'border-radius': '4px',
                    });
                    $elements.employeeGrid.DataTable().columns.adjust().draw();
                },
                drawCallback() {
                    $elements.employeeGridBody.find('input[type="checkbox"]').each(function () {
                        const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();
                        $(this).prop('checked', selectedEmpIds.has(employeeId));
                    });
                    updateSelectAllCheckboxState();
                }
            });
        };

        const updateSelectedEmployeeIds = () => {
            $elements.employeeGridBody.find('input[type="checkbox"]').each(function () {
                const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();
                $(this).is(':checked') ? selectedEmpIds.add(employeeId) : selectedEmpIds.delete(employeeId);
            });
        };

        const updateSelectAllCheckboxState = () => {
            const checkboxes = $elements.employeeGridBody.find('input[type="checkbox"]').not(':disabled');
            const total = checkboxes.length;
            const checked = checkboxes.filter(':checked').length;
            $elements.employeeCheckAll.prop('checked', total > 0 && total === checked);
        };

        const loadAllFilterEmp = async () => {
            showLoading();
            try {
                const result = await apiPost(URLS.filter, getFilterValue());
                console.log(getFilterValue());
                if (!result) return;
                console.log(result);
                const { lookupData, employees } = result;

                loadTableData(employees);

                const lookupMapping = [
                    [lookupData.companies, $elements.companySelect],
                    [lookupData.branches, $elements.branchSelect],
                    [lookupData.departments, $elements.departmentSelect],
                    [lookupData.designations, $elements.designationSelect],
                    [lookupData.employeeType, $elements.empTypeSelect]
                ];

                lookupMapping.forEach(([dataList, element]) => {
                    if (dataList?.length) {
                        populateSelect(element, dataList);
                    }
                });

                setupCascadingClearEvents();
                bindFilterChangeEvents();

            } catch (error) {
                console.error("Error loading filter employees:", error);
                showNotification("Failed to load employee data. Please try again.", "error");
            } finally {
                hideLoading();
            }
        };

        const bindFilterChangeEvents = () => {
            FILTER_SELECTORS.forEach(element => {
                if (element == $elements.probationEndDays) return;
                element.off("change.loadFilter").on("change.loadFilter", loadAllFilterEmp);
            }); 

            $elements.probationEndDays.off("input.loadFilter").on("input.loadFilter",
                debounce(loadAllFilterEmp, 500)
            );
        };

        const validateForm = () => {
            const selectedCheckboxes = $elements.employeeGridBody.find('input[type="checkbox"]:checked');
            if (selectedCheckboxes.length === 0) {
                showNotification("Please select at least one employee.", "error");
                return false;
            }

            if (!$elements.confirmSelect.val().trim()) {
                showNotification("Please select a confirmation date.", "error");
                return false;
            }
            return true;
        };

        const handleFormSubmission = async () => {
            if (!validateForm()) return;

            showLoading();
            try {
                const selectedEmployees = Array.from($elements.employeeGridBody.find('input[type="checkbox"]:checked'))
                    .map(checkbox => {
                        const row = $(checkbox).closest('tr');
                        return {
                            EmployeeId: row.find('td:nth-child(2)').text().trim(),
                            RefLetterNo: row.find('.refLetter').val().trim(),
                        };
                    });

                const dataToSend = {
                    confirmInfo: selectedEmployees,
                    ConfirmeDate: $elements.confirmSelect.val().trim(),
                };

                const response = await apiPost(URLS.save, dataToSend);

                if (response?.success) {
                    showNotification("Employee confirmed successfully!", "success");
                    selectedEmpIds.clear();
                    $elements.employeeCheckAll.prop('checked', false);
                    await loadAllFilterEmp();
                } else {
                    showNotification("Failed to confirm employee.", "error");
                }
            } catch (error) {
                console.error("Error in form submission:", error);
                showNotification("An error occurred while saving. Please try again.", "error");
            } finally {
                hideLoading();
            }
        };

        const clearForm = () => {
            $elements.branchSelect.add($elements.empTypeSelect).val(null).trigger('change');
            $elements.confirmSelect.val(null);
            selectedEmpIds.clear();
        };

        const bindUIEvents = () => {
            $elements.employeeCheckAll.on('change', function () {
                const isChecked = $(this).is(':checked');
                $elements.employeeGridBody.find('input[type="checkbox"]').not(':disabled')
                    .prop('checked', isChecked);
                updateSelectedEmployeeIds();
            });

            $elements.employeeGridBody.on('change', 'input[type="checkbox"]', () => {
                updateSelectedEmployeeIds();
                updateSelectAllCheckboxState();
            });

            $elements.saveButton.on('click', handleFormSubmission);
            $elements.clearButton.on('click', clearForm);
        };

        const scrollToGrid = () => {
            $elements.employeeGridBody[0]?.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        };

        const init = () => {
            settings.load();
            setupLoadingOverlay();
            initializeMultiselects();
            initializeFlatpickr();
            bindUIEvents();
            loadAllFilterEmp();
            if ($.fn.DataTable.isDataTable($elements.employeeGrid)) {
                $elements.employeeGrid.DataTable().destroy();
                $('#employee-filter-grid').DataTable().columns.adjust().draw();
            }

           //empDataTable.columns.adjust();
            if (empDataTable != null) {
                //empDataTable.columns.adjust().responsive.recalc();
            }
            //settings.empDataTable.DataTable().draw();
            $(window).on('load', scrollToGrid);
        };

        init();

        return {
            destroy: () => {
                if (currentAbortController) {
                    currentAbortController.abort();
                }

                FILTER_SELECTORS.forEach(element => {
                    element.off('.loadFilter');
                });

                Object.entries(CASCADING_CLEAR_MAP).forEach(([key]) => {
                    $elements[key.replace('Select', 'Select')].off('.clearDropdowns');
                });

                flatpickrInstance?.destroy();
                empDataTable?.destroy();

                if (debounceTimer) {
                    clearTimeout(debounceTimer);
                }

                $(window).off('load', scrollToGrid);
            }
        };
    };
})(jQuery);













//let empDataTable = null;
//let selectedEmpIds = new Set();
//let flatpickrInstance = null;

//$(document).ready(() => {
//    setupLoadingOverlay();
//    initializeMultiselects();
//    initializeEmpGrid();
//    loadAllFilterEmp();
//    initializeEventHandlers();
//    initializeFlatpickr();
//});

//$(window).on('load', () => {
//    $('#employee-filter-grid-body')[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
//});

//// Loading overlay functions
//function setupLoadingOverlay() {
//    if ($("#loadingOverlay").length === 0) {
//        $("body").append(`
//            <div id="loadingOverlay" style="display:none;position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.5);z-index:9999;justify-content:center;align-items:center;">
//                <div style="background:white;padding:20px;border-radius:5px;box-shadow:0 0 10px rgba(0,0,0,0.3);text-align:center;">
//                    <div class="spinner-border text-primary"></div>
//                </div>
//            </div>
//        `);
//    }
//}

//const showLoading = () => {
//    $('body').css('overflow', 'hidden');
//    $("#loadingOverlay").fadeIn(200);
//};

//const hideLoading = () => {
//    $('body').css('overflow', '');
//    $("#loadingOverlay").fadeOut(200);
//};

//// Flatpickr initialization
//function initializeFlatpickr() {
//    flatpickrInstance?.destroy();
//    $("#confirmSelect").val('');

//    flatpickrInstance = flatpickr("#confirmSelect", {
//        mode: "single",
//        dateFormat: "d-m-Y",
//        allowInput: false,
//        clickOpens: true,
//        onChange: (dates, dateStr) => dates.length && $("#confirmSelect").val(dateStr)
//    });

//    $("#confirmSelect").on('keydown keypress keyup', e => {
//        if (e.keyCode === 9 || e.keyCode === 27 || (e.keyCode >= 35 && e.keyCode <= 39)) return true;
//        e.preventDefault();
//        return false;
//    });
//}

//// Event handlers
//function initializeEventHandlers() {
//    $("#companySelect, #branchSelect, #departmentSelect, #designationSelect, #empTypeSelect")
//        .on("change", loadAllFilterEmp);

//    $(".js-empSalary-info-dec-clear").on('click', clearForm);

//    $("#employee-check-all").on('change', function () {
//        const isChecked = $(this).is(':checked');
//        $('#employee-filter-grid-body input[type="checkbox"]').not(':disabled').prop('checked', isChecked);
//        updateSelectedEmployeeIds();
//    });

//    $('#employee-filter-grid-body').on('change', 'input[type="checkbox"]', () => {
//        updateSelectedEmployeeIds();
//        updateSelectAllCheckboxState();
//    });

//    $(".js-serviceConfirm-dec-save").on('click', handleFormSubmission);
//}

//function updateSelectedEmployeeIds() {
//    $('#employee-filter-grid-body input[type="checkbox"]').each(function () {
//        const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();
//        $(this).is(':checked') ? selectedEmpIds.add(employeeId) : selectedEmpIds.delete(employeeId);
//    });
//}

//const toArray = value => !value ? [] : Array.isArray(value) ? value : [value];

//function clearForm() {
//    $("#branchSelect, #empTypeSelect").val(null).trigger('change');
//    selectedEmpIds.clear();
//}

//const getAllFilterVal = () => ({
//    CompanyCodes: toArray($("#companySelect").val()),
//    BranchCodes: toArray($("#branchSelect").val()),
//    DepartmentCodes: toArray($("#departmentSelect").val()),
//    DesignationCodes: toArray($("#designationSelect").val()),
//    EmployeeTypes: toArray($("#empTypeSelect").val()),
//    EmployeeIDs: toArray($("#empIdSelect").val()),
//    ProbationEndDays: $("#probationEndDays").val(),
//});

//// API utilities
//async function apiRequest(url, options = {}) {
//    try {
//        const response = await fetch(url, {
//            method: 'GET',
//            headers: { 'Content-Type': 'application/json', ...options.headers },
//            ...options
//        });
//        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
//        return await response.json();
//    } catch (error) {
//        console.error(`API request failed for ${url}:`, error);
//        throw error;
//    }
//}

//const apiPost = (url, data) => apiRequest(url, { method: 'POST', body: JSON.stringify(data) });

//// Load and populate data
//async function loadAllFilterEmp() {
//    showLoading();
//    try {
//        const { lookupData, employees } = await apiPost('/HrmServiceBulkConfirmationEntry/getFilterEmp', getAllFilterVal());

//        loadTableData(employees);

//        const lookupSelectors = {
//            companies: "#companySelect",
//            branches: "#branchSelect",
//            departments: "#departmentSelect",
//            designations: "#designationSelect",
//            employeeType: "#empTypeSelect"
//        };

//        Object.entries(lookupSelectors).forEach(([key, selector]) => {
//            lookupData[key]?.length && populateSelect(selector, lookupData[key]);
//        });

//        setupClearOnChangeEvents();
//    } catch (error) {
//        console.error("Error loading filter employees:", error);
//        showNotification("Failed to load employee data. Please try again.", "error");
//    } finally {
//        hideLoading();
//    }
//}

//function populateSelect(selector, items) {
//    const $select = $(selector);
//    const existingOptions = new Set($select.find('option').map((_, el) => el.value).get());

//    items.forEach(item => {
//        if (item.code && item.name && !existingOptions.has(item.code)) {
//            $select.append(`<option value="${item.code}">${item.name}</option>`);
//        }
//    });
//    $select.multiselect('rebuild');
//}

//function setupClearOnChangeEvents() {
//    const clearMap = {
//        "#branchSelect": ["#divisionSelect", "#departmentSelect", "#designationSelect", "#empTypeSelect"],
//        "#divisionSelect": ["#departmentSelect", "#designationSelect", "#employeeSelect", "#empTypeSelect"],
//        "#departmentSelect": ["#designationSelect", "#employeeSelect", "#empTypeSelect"],
//        "#designationSelect": ["#employeeSelect", "#empTypeSelect"]
//    };

//    Object.entries(clearMap).forEach(([parent, children]) => {
//        $(parent).off("change.clearDropdowns").on("change.clearDropdowns", () => {
//            children.forEach(child => $(child).empty().multiselect('rebuild'));
//        });
//    });
//}

//// Table functions
//function loadTableData(data) {
//    if ($.fn.DataTable.isDataTable('#employee-filter-grid') && empDataTable !== null) {
//        empDataTable.destroy();
//        empDataTable = null;
//    }

//    const tableBody = $('#employee-filter-grid-body');
//    tableBody.html(data.map(item => `
//        <tr>
//            <td class="text-center p-0" width="1%"><input type="checkbox" class="empSelect"/></td>
//            <td class="text-center p-1">${item.employeeId}</td>
//            <td class="p-1">${item.employeeName}</td>
//            <td class="text-center text-nowrap p-1">${item.designationName}</td>
//            <td class="text-center text-nowrap p-1">${item.departmentName}</td>
//            <td class="text-center text-nowrap p-1">${item.grossSalary}</td>
//            <td class="text-center text-nowrap p-1">${item.joiningDate}</td>
//            <td class="text-center text-nowrap p-1">${item.probationPeriod}</td>
//            <td class="text-center text-nowrap p-1">${item.probationPeriodEndOn}</td>
//            <td class="text-center text-nowrap p-1">${item.extenPeriod}</td>
//            <td class="text-center text-nowrap p-1">${item.extenPeriodEndOn}</td>
//            <td class="text-center text-nowrap p-1">${item.serviceLength}</td>
//            <td class="text-center p-1">
//                <input type="text" class="form-control form-control-sm mx-auto refLetter" value="${item.refLetterNo || ''}" />
//            </td>
//        </tr>
//    `).join(''));

//    intializeDataTable();
//}

//function intializeDataTable() {
//    empDataTable = $('#employee-filter-grid').DataTable({
//        paging: true,
//        pageLength: 10,
//        lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
//        lengthChange: true,
//        info: true,
//        autoWidth: false,
//        responsive: true,
//        scrollX: true,
//        columnDefs: [{ targets: [0, 12], orderable: false, className: 'no-sort' }],
//        initComplete() {
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
//        },
//        drawCallback() {
//            $('#employee-filter-grid-body input[type="checkbox"]').each(function () {
//                const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();
//                $(this).prop('checked', selectedEmpIds.has(employeeId));
//            });
//            updateSelectAllCheckboxState();
//        }
//    });
//}

//function updateSelectAllCheckboxState() {
//    const checkboxes = $('#employee-filter-grid-body input[type="checkbox"]').not(':disabled');
//    const total = checkboxes.length;
//    const checked = checkboxes.filter(':checked').length;
//    $("#employee-check-all").prop('checked', total > 0 && total === checked);
//}

//const initializeEmpGrid = () => {
//    showLoading();
//    setTimeout(() => {
//        empDataTable?.destroy();
//        intializeDataTable();
//    }, 100);
//};

//// Multiselect initialization
//function initializeMultiselects() {
//    const multiselectConfig = {
//        companySelect: 'Select Company',
//        branchSelect: 'Branch',
//        empTypeSelect: 'Emp. Type',
//        departmentSelect: 'Department',
//        designationSelect: 'Designation',
//    };

//    const baseOptions = {
//        enableFiltering: true,
//        includeSelectAllOption: true,
//        selectAllText: 'Select All',
//        nSelectedText: 'Selected',
//        allSelectedText: 'All Selected',
//        filterPlaceholder: 'Search',
//        buttonWidth: '100%',
//        maxHeight: 350,
//        filterBehavior: 'text',
//        enableCaseInsensitiveFiltering: true,
//    };

//    Object.entries(multiselectConfig).forEach(([id, placeholder]) => {
//        $('#' + id).multiselect({
//            ...baseOptions,
//            nonSelectedText: placeholder,
//            buttonText: (options, select) => {
//                if (options.length === 0) return placeholder;
//                if (options.length > 0 && !['companySelect', 'activityStatusSelect'].includes(id)) {
//                    return options.length + ' Selected';
//                }
//                return $(options[0]).text();
//            }
//        });
//    });
//}

//const validateForm = () => {
//    const selectedCheck = $('#employee-filter-grid-body input[type="checkbox"]:checked');
//    if (selectedCheck.length === 0) {
//        showNotification("Please select at least one employee.", "error");
//        return false;
//    }

//    if (!$("#confirmSelect").val().trim()) {
//        showNotification("Please select a confirmation date.", "error");
//        return false;
//    }
//    return true;
//};

//// Form submission
//async function handleFormSubmission() {
//    if (!validateForm()) return;

//    try {
//        const selectedEmp = Array.from($('#employee-filter-grid-body input[type="checkbox"]:checked')).map(checkbox => {
//            const row = $(checkbox).closest('tr');
//            return {
//                EmployeeId: row.find('td:nth-child(2)').text().trim(),
//                RefLetterNo: row.find('.refLetter').val().trim(),
//            };
//        });

//        const dataToSend = {
//            confirmInfo: selectedEmp,
//            ConfirmeDate: $("#confirmSelect").val().trim(),
//        };

//        const response = await apiPost('/HrmServiceBulkConfirmationEntry/Save', dataToSend);

//        if (response.success) {
//            showNotification("Employee salary information updated successfully!", "success");
//            selectedEmpIds.clear();
//            $("#employee-check-all").prop('checked', false);
//            await loadAllFilterEmp();
//        } else {
//            showNotification("Failed to update employee salary information.", "error");
//        }
//    } catch (error) {
//        console.error("Error in form submission:", error);
//        showNotification("An error occurred while saving. Please try again.", "error");
//    }
//}

//const showNotification = (message, type) => {
//    if (typeof toastr !== 'undefined') {
//        const title = type === 'success' ? 'Success' : type === 'error' ? 'Error' : 'Warning';
//        toastr[type](message, title);
//    } else {
//        alert(message);
//    }
//};









//let empDataTable = null;
//let filterChangeBound = false;
//let selectedEmpIds = new Set();
////let isEditMode = false;
//let flatpickrInstance = null;

//$(document).ready(function () {
//    setupLoadingOverlay();
//    initializeMultiselects();
//    initializeEmpGrid();
//    loadAllFilterEmp();
//    initializeEventHandlers();
//    initializeFlatpickr();
//});

//$(window).on('load', function () {
//    $('#employee-filter-grid-body')[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
//});

//function setupLoadingOverlay() {
//    console.log("Loading");
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

//function initializeFlatpickr() {
//    if (flatpickrInstance) {
//        flatpickrInstance.destroy();
//        flatpickrInstance = null;
//    }

//    $("#confirmSelect").val('');

//    flatpickrInstance = flatpickr("#confirmSelect", {
//        mode: "single",
//        dateFormat: "d-m-Y",
//        allowInput: false,
//        clickOpens: true,
//        //defaultDate: new Date(),
//        onChange: function (dates, dateStr) {
//            if (dates.length > 0) {
//                $("#confirmSelect").val(dateStr);
//            }
//        }
//    });

//    $("#confirmSelect").on('keydown keypress keyup', function (e) {
//        if (e.keyCode === 9 || e.keyCode === 27 ||
//            (e.keyCode >= 35 && e.keyCode <= 39)) {
//            return true;
//        }
//        e.preventDefault();
//        return false;
//    });
//}

//function initializeEventHandlers() {
//    $("#companySelect, #branchSelect, #departmentSelect, #designationSelect, #empTypeSelect")
//        .on("change", function () {
//            loadAllFilterEmp();
//        });

//    $(".js-empSalary-info-dec-clear").on('click', function () {
//        clearForm();
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

//    //$("#excelUploadForm").submit(excelUpload);

//    $(".js-serviceConfirm-dec-save").on('click', handleFormSubmission);
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
//function clearForm() {
//    $("#branchSelect").val(null).trigger('change');
//    $("#empTypeSelect").val(null).trigger('change');
//    selectedEmpIds.clear();
//}

//function getAllFilterVal() {
//    const filterData = {
//        CompanyCodes: toArray($("#companySelect").val()),
//        BranchCodes: toArray($("#branchSelect").val()),
//        DepartmentCodes: toArray($("#departmentSelect").val()),
//        DesignationCodes: toArray($("#designationSelect").val()),
//        EmployeeTypes: toArray($("#empTypeSelect").val()),
//        EmployeeIDs: toArray($("#empIdSelect").val()),
//        ProbationEndDays: $("#probationEndDays").val(),
//    }
//    return filterData;
//}

//function loadAllFilterEmp() {
//    showLoading();
//    const filterData = getAllFilterVal();

//    $.ajax({
//        url: `/HrmServiceBulkConfirmationEntry/getFilterEmp`,
//        type: 'POST',
//        contentType: 'application/json',
//        data: JSON.stringify(filterData),
//        success: function (res) {
//            const data = res.lookupData;
//            console.log(res);
//            loadTableData(res.employees);
//            if (data.companies?.length) {
//                populateSelect("#companySelect", data.companies)
//            }
//            if (data.branches?.length) {
//                populateSelect("#branchSelect", data.branches);
//            }
//            if (data.departments?.length) {
//                populateSelect("#departmentSelect", data.departments);
//            }
//            if (data.designations?.length) {
//                populateSelect("#designationSelect", data.designations);
//            }
//            if (data.employeeType?.length) {
//                populateSelect("#empTypeSelect", data.employeeType);
//            }

//            setupClearOnChangeEvents();
//            bindFilterChangeOnce();
//        },
//        complete: function () {
//            hideLoading();
//        },
//        error: function (xhr, status, error) {
//            console.error("Error loading filter employees:", error);
//        }
//    });
//}

//function populateSelect(selector, items) {
//    const $select = $(selector);
//    items.forEach(item => {
//        if (item.code && item.name && $select.find(`option[vlaue="${item.code}"]`).length === 0) {
//            $select.append(`<option value="${item.code}">${item.name}</option>`);
//        }
//    });
//    $select.multiselect('rebuild');
//}

//function setupClearOnChangeEvents() {
//    const clearMap = {
//        "#branchSelect": ["#divisionSelect", "#departmentSelect", "#designationSelect", "#empTypeSelect"],
//        "#divisionSelect": ["#departmentSelect", "#designationSelect", "#employeeSelect", "#empTypeSelect"],
//        "#departmentSelect": ["#designationSelect", "#employeeSelect", "#empTypeSelect"],
//        "#designationSelect": ["#employeeSelect", "#empTypeSelect"]
//    };

//    Object.entries(clearMap).forEach(([parent, children]) => {
//        $(parent).off("change.clearDropdowns").on("change.clearDropdowns", function () {

//            children.forEach(child => $(child).empty().multiselect('rebuild'));
//        });
//    });
//}

//function bindFilterChangeOnce() {
//    if (!filterChangeBound) {
//        $("#companySelect, #branchSelect, #departmentSelect, #designationSelect, #empTypeSelect")
//            .on("change", function () {
//                loadAllFilterEmp();
//            });
//        filterChangeBound = true;
//    }
//}

//function loadTableData(data) {
//    var tableDataItem = data;
//    console.log("Table Data Item: ", data);
//    if ($.fn.DataTable.isDataTable('#employee-filter-grid') && empDataTable !== null) {
//        empDataTable.destroy();
//        empDataTable = null;
//    }

//    var tableBody = $('#employee-filter-grid-body');
//    tableBody.empty();

//    var refLetterNo = '<td class="text-center p-1">' +
//        '<input type="text" id="refLetterNo" class="form-control form-control-sm mx-auto" />' +
//        '</td>';

//    $.each(tableDataItem, function (index, item) {
//        var row = $('<tr>');

//        row.append(`<td class="text-center p-0" width="1%"><input type="checkbox" width="1%" class="empSelect"/></td>`);
//        row.append('<td class="text-center p-1">' + item.employeeId + '</td>');
//        row.append('<td class="p-1">' + item.employeeName + '</td>');
//        row.append('<td class="text-center text-nowrap p-1">' + item.designationName + '</td>');
//        row.append('<td class="text-center text-nowrap p-1">' + item.departmentName + '</td>');
//        row.append('<td class="text-center text-nowrap p-1">' + item.grossSalary + '</td>')
//        row.append('<td class="text-center text-nowrap p-1">' + item.joiningDate + '</td>');
//        row.append('<td class="text-center text-nowrap p-1">' + item.probationPeriod + '</td>');
//        row.append('<td class="text-center text-nowrap p-1">' + item.probationPeriodEndOn + '</td>');
//        row.append('<td class="text-center text-nowrap p-1">' + item.extenPeriod + '</td>');
//        row.append('<td class="text-center text-nowrap p-1">' + item.extenPeriodEndOn + '</td>');
//        row.append('<td class="text-center text-nowrap p-1">' + item.serviceLength + '</td>');

//        row.append('<td class="text-center p-1"><input type="text" class="form-control form-control-sm mx-auto refLetter" value="' + item.refLetterNo + '"/></td>');

//        tableBody.append(row);
//    });

//    intializeDataTable();
//}

//function intializeDataTable() {
//    empDataTable = $('#employee-filter-grid').DataTable({
//        paging: true,
//        pageLength: 10,
//        lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
//        lengthChange: true,
//        info: true,
//        autoWidth: false,
//        responsive: true,
//        fixedHeader: false,
//        scrollX: true,
//        columnDefs: [
//            {
//                targets: [0, 12],
//                orderable: false,
//                className: 'no-sort'
//            }
//        ],
//        initComplete: function () {
//            hideLoading();
//            $('#custom-search').on('keyup', function () {
//                employeeDataTable.search(this.value).draw();
//            });

//            $('.dataTables_filter input').css({
//                'width': '250px',
//                'padding': '6px 12px',
//                'border': '1px solid #ddd',
//                'border-radius': '4px',
//            });

//            // $('#employee-filter-grid_wrapper .dataTables_filter').hide();
//        },
//        drawCallback: function () {
//            $('#employee-filter-grid-body input[type="checkbox"]').each(function () {
//                const employeeId = $(this).closest('tr').find('td:nth-child(2)').text().trim();
//                $(this).prop('checked', selectedEmpIds.has(employeeId));
//            });

//            updateSelectAllCheckboxState();

//            //loadMethodDD();
//        }
//    });
//}

//function updateSelectAllCheckboxState() {
//    const total = $('#employee-filter-grid-body input[type="checkbox"]').not(':disabled').length;
//    const checked = $('#employee-filter-grid-body input[type="checkbox"]').not(':disabled').filter(':checked').length;

//    $("#employee-check-all").prop('checked', total > 0 && total === checked);
//}
//function initializeEmpGrid() {
//    showLoading();
//    setTimeout(function () {
//        if (empDataTable !== null) {
//            empDataTable.destroy();
//        }
//        intializeDataTable();
//    }, 100);
//}

//function initializeMultiselects() {
//    const nonSelectedTextMap = {
//        companySelect: 'Select Company',
//        branchSelect: 'Branch',
//        empTypeSelect: 'Emp. Type',
//        departmentSelect: 'Department',
//        designationSelect: 'Designation',
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
//            maxWidth: 50,
//            filterBehavior: 'text',
//            enableCaseInsensitiveFiltering: true,
//            buttonText: function (options, select) {
//                if (options.length === 0) {
//                    return nonSelectedTextMap[id];
//                }

//                else if (options.length > 0 && (id != 'companySelect') && (id != 'activityStatusSelect')) {
//                    return options.length + ' Selected';
//                }
//                else {
//                    return $(options[0]).text();
//                }
//            }
//        });
//    });
//}

//function validateForm() {
//    const selectedCheck = $('#employee-filter-grid-body input[type="checkbox"]:checked');
//    if (selectedCheck.length === 0) {
//        showNotification("Please select at least one employee.", "error");
//        return false;
//    }
//    return true;
//}

//function handleFormSubmission() {
//    if (!validateForm())
//        return;

//    //showLoading();

//    const selectedEmp = [];
//    const selectedCheckboxes = $('#employee-filter-grid-body input[type="checkbox"]:checked');

//    selectedCheckboxes.each(function () {
//        const row = $(this).closest('tr');
//        const empData = {
//            EmployeeId: row.find('td:nth-child(2)').text().trim(),
//            RefLetterNo: row.find('.refLetter').val().trim(),

//        };

//        selectedEmp.push(empData);
//    });

//    const dataToSend = {
//        confirmInfo: selectedEmp,
//        ConfirmeDate: $("#confirmSelect").val().trim(),
//    };

//    console.log('Data To Send : ', dataToSend);

//    $.ajax({
//        url: '/HrmServiceBulkConfirmationEntry/Save',
//        type: 'POST',
//        contentType: 'application/json',
//        data: JSON.stringify(dataToSend),
//        success: function (response) {

//            if (response.success) {
//                showNotification("Employee salary information updated successfully!", "success");

//                selectedEmpIds.clear();
//                $("#employee-check-all").prop('checked', false);

//                loadAllFilterEmp();

//            } else {
//                showNotification("Failed to update employee salary information.", "error");
//            }
//        }
//    })
//}

//function showNotification(message, type) {
//    if (typeof toastr !== 'undefined') {
//        toastr[type](message, type === 'success' ? 'Success' : type === 'error' ? 'Error' : 'Warning');
//    } else {
//        alert(message);
//    }
//}