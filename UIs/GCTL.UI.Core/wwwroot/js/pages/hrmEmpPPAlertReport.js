(function ($) {
    $.empPPAlertReport = function (options) {
        const settings = $.extend({
            baseUrl: "/",
            companySelect: "#companySelect",
            branchSelect: "#branchSelect",
            departmentSelect: "#departmentSelect",
            designationSelect: "#designationSelect",
            pEndDaysSelect: "#ppEndDaysSelect",
            joiningDateFrom: "#joinDateFromSelect",
            joiningDateTo: "#joinDateToSelect",
            exportFormatSelect: "exportFormatSelect",
            downloadBtn: "#downloadBtn",
            employeeGrid: "#employee-filter-grid",
            employeeGridBody: "#employee-filter-grid-body",
            customSearch: "#custom-search",
            clearButton: ".js-serviceConfirm-dec-clear",
            load: () => console.log("Loading employee probational period alert report..."), 
        }, options);

        const URLS = {
            filter: `${settings.baseUrl}/getFilterEmp`,
            export: `${settings.baseUrl}/ExportReport`,
        };

        let isInitialLoad = true;
        let filterChangeBound = false;
        let flatpickrInstance = {};
        let empDataTable = null;

        const toArray = val => val ? (Array.isArray(val) ? val : [val]) : [];
        const showLoading = () => $("#customLoadingOverlay").css("display", "flex");
        const hideLoading = () => $("#customLoadingOverlay").hide();

        const showToast = (type, message) => {
            if (typeof toastr !== 'undefined') {
                toastr[type](message);
            } else {
                alert(message);
            }
        };

        const setupLoadingOverlay = () => {
            if ($("#customLoadingOverlay").length === 0) {
                $("body").append(`
                    <div id="customLoadingOverlay" style="display:none;position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.5);z-index:9999;justify-content:center;align-items:center;">
                        <div style="background:white;padding:20px;border-radius:5px;box-shadow:0 0 10px rgba(0,0,0,0.3);text-align:center;">
                            <div class="spinner-border text-primary"></div>
                            
                        </div>
                    </div>
                `);
            }
        };

        const initializeFlatpickr = () => {
            if (typeof flatpickr === 'undefined')
                return;

            const fromPicker = flatpickr(settings.joiningDateFrom, {
                dateFormat: "d-m-Y",
                allowInput: true,
                onChange: function (selectedDates) {
                    if (selectedDates.length) {
                        toPicker.set('minDate', selectedDates[0]);
                        if (!isInitialLoad) loadFilterEmp();
                    }
                }
            });

            const toPicker = flatpickr(settings.joiningDateTo, {
                dateFormat: "d-m-Y",
                allowInput: true,
                onChange: function (selectedDates) {
                    if (selectedDates.length) {
                        fromPicker.set('maxDate', selectedDates[0]);
                        if (!isInitialLoad) loadFilterEmp();
                    }
                }
            });
        }


        const initializeMultiselects = () => {
            const selectors = [
                [settings.companySelect, 'Company(s)'],
                [settings.departmentSelect, 'Department(s)'],
                [settings.designationSelect, 'Designation(s)'],
            ];

            selectors.forEach(([selector, text]) => {
                $(selector).multiselect({
                    enableFiltering: true,
                    includeSelectAllOption: true,
                    selectAllText: 'Select All',
                    nonSelectedText: `Select ${text}`,
                    nSelectedText: 'Selected',
                    allSelectedText: 'All Selected',
                    filterPlaceholder: 'Search.......',
                    buttonWidth: '100%',
                    enableClickableOptGroups: true,
                    numberDisplayed: 1,
                    enableCaseInsensitiveFiltering: true
                });
            });
        };

        const getFilterValue = () => ({
            CompanyCodes: toArray($(settings.companySelect).val()),
            DepartmentCodes: toArray($(settings.departmentSelect).val()),
            DesignationCodes: toArray($(settings.designationSelect).val()),

            DateFrom: $(settings.joiningDateFrom).val() || null,
            DateTo: $(settings.joiningDateTo).val() || null,

            ProbationEndDays: $(settings.pEndDaysSelect).val() || null
        });

        const populateSelect = (selector, dataList) => {
            const $select = $(selector);

            dataList.forEach(item => {
                if (item.code && item.name && $select.find(`option[value="${item.code}"]`).length === 0) {
                    $select.append(`<option value="${item.code}">${item.name}</option>`);
                }
            });
            $select.multiselect('rebuild');

            if (selector === settings.empStatus)
                selectActiveStatus(dataList);
        }

        const clearDependentDropdowns = selectors => {
            selectors.forEach(selector => {
                $(selector).empty().multiselect('rebuild');
            });
        };

        const setupCascadingClearEvents = () => {
            const clearMap = {
                [settings.companySelect]: [ settings.departmentSelect, settings.designationSelect],
                [settings.departmentSelect]: [settings.designationSelect],
                
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
                    settings.departmentSelect,
                    settings.designationSelect
                ].join(',');

                $(filterSelectors).on("change.loadFilter", function () {
                    loadFilterEmp();
                })


                $(settings.joiningDateFrom).on('input.loadFilter', function () {
                    clearTimeout(window.yearInputTimeout);
                    window.yearInputTimeout = setTimeout(() => {
                        clearDependentDropdowns([
                            settings.departmentIds,
                            settings.designationIds,
                        ]);
                        loadFilterEmp();
                    }, 300);
                });

                $(settings.joiningDateTo).on('input.loadFilter', function () {
                    clearTimeout(window.yearInputTimeout);
                    window.yearInputTimeout = setTimeout(() => {
                        clearDependentDropdowns([
                            settings.departmentIds,
                            settings.designationIds
                        ]);
                        loadFilterEmp();
                    }, 300);
                });

                $(settings.pEndDaysSelect).on('input.loadFilter', function () {
                    clearTimeout(window.yearInputTimeout);
                    window.yearInputTimeout = setTimeout(() => {
                        clearDependentDropdowns([
                            settings.departmentIds,
                            settings.designationIds
                        ]);
                        loadFilterEmp();
                    }, 300);
                });

                filterChangeBound = true;
            };
        };

        const loadFilterEmp = () => {
            $.ajax({
                url: URLS.filter,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(getFilterValue()),
                success: res => {
                    if (!res.isSuccess) {
                        showToast('error', res.message);
                        return;
                    }

                    const { data } = res;

                    console.log(data);

                    if (data.lookupData.companies?.length)
                        populateSelect(settings.companySelect, data.lookupData.companies);

                    if (data.lookupData.departments?.length)
                        populateSelect(settings.departmentSelect, data.lookupData.departments);

                    if (data.lookupData.designations?.length)
                        populateSelect(settings.designationSelect, data.lookupData.designations);

                    if (data.employees !== null)
                        loadTableData(data);

                    setupCascadingClearEvents();
                    bindFilterChangeEvents();
                },
                error: () => {
                    showToast('error', 'Failed to load data');
                }
            });
        };

        const exportReport = format => {
            showLoading();

            $.ajax({
                url: URLS.export,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ FilterData: getFilterValue(), ExportFormat: format }),
                xhrFields: { responseType: 'blob' },
                success: (data, status, xhr) => {
                    hideLoading();

                    const contentType = xhr.getResponseHeader('Content-Type');
                    const disposition = xhr.getResponseHeader('Content-Disposition');

                    let filename;
                    if (disposition && disposition.indexOf("filename=") != -1) {
                        const match = disposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                        if (match && match[1]) {
                            filename = match[1].replace(/['"]/g, '');
                        }
                    }

                    if (format === "download") {
                        const blob = new Blob([data], { type: 'application/pdf' });
                        const iframe = Object.assign(document.createElement('iframe'), {
                            style: 'display:none',
                            src: window.URL.createObjectURL(blob),
                            onload: function () {
                                this.contentWindow.focus();
                                this.contentWindow.print();
                            }
                        });
                        document.body.appendChild(iframe);
                        return;
                    }
                    const blob = new Blob([data], { type: contentType });
                    const link = Object.assign(document.createElement("a"), {
                        href: window.URL.createObjectURL(blob),
                        download: filename
                    });

                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                    window.URL.revokeObjectURL(link.href);
                    clearAllFilters();
                    showToast('success', 'Report exported successfully');
                },
                error: (e) => {
                    hideLoading();
                    let errorMessage = 'Export failed. Please try again.';

                    switch (e.status) {
                        case 400:
                            errorMessage = 'Invalid request. Please check your inputs.';
                            break;
                        case 404:
                            errorMessage = 'No data found for the selected criteria.';
                            break;
                        default:
                            errorMessage = 'An unexpected error occurred. Please try again later.';
                            break;
                    }
                    showToast('error', errorMessage);
                }
            });
        };

        const initializeEmpGrid = () => {
            //showLoading();
            setTimeout(function () {
                if (empDataTable !== null) {
                    empDataTable.destroy();
                }
                    
                initializeDataTable();
            }, 200);
        }

        const initializeDataTable = () => {
            //try {
                empDataTable = $(settings.employeeGrid).DataTable({
                    paging: true,
                    pageLength: 10,
                    lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
                    lengthChange: true,
                    info: true,
                    autoWidth: false,
                    responsive: true,
                    fixedHeader: false,
                    scrollX: true,
                    //columnDefs: [
                    //    {
                    //        targets: [0, 12, 13],
                    //        orderable: false,
                    //        className: 'no-sort'
                    //    }
                    //],
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
                    }
                });
            //} catch (error) {
            //    console.error('DataTable initialization error:', error);
            //    hideLoading();
            //}
        }

        const loadTableData = (res) => {
            var tableItem = res.employees;

            if($.fn.dataTable.isDataTable(settings.employeeGrid) && empDataTable !== null){
                $(settings.employeeGrid).DataTable().destroy();
                empDataTable = null;
            }

            var tableBody = $(settings.employeeGridBody);
            tableBody.empty();

            $.each(tableItem, function (index, employee) {
                var row = $('<tr>');

                row.append('<td class="text-center p-1">' + employee.code + '</td>');
                row.append('<td class="p-1">' + employee.name + '</td>');
                row.append('<td class="p-1">' + employee.departmentName + '</td>');
                row.append('<td class="text-center p-1">' + employee.desingationName + '</td>');
                row.append('<td class="text-center p-1">' + employee.grossSalary + '</td>');
                row.append('<td class="text-center p-1">' + employee.joiningDate + '</td>');
                row.append('<td class="text-center p-1">' + employee.probationPeriod + '</td>');
                row.append('<td class="text-center p-1">' + employee.probationPeriodEndOn + '</td>');
                row.append('<td class="text-center p-1">' + employee.serviceLength + '</td>');

                tableBody.append(row);
            });

            initializeDataTable();
        }

        const clearAllFilters = () => {
            //const filterSelectors = [
            //    settings.companySelect,
            //    settings.departmentSelect,
            //    settings.designationSelect
            //];
            //
            //filterSelectors.forEach(selector => {
            //    $(selector).multiselect('deselectAll', false);
            //    $(selector).multiselect('updateButtonText');
            //});
            //
            //if (flatpickrInstances.joiningDateFrom) {
            //    flatpickrInstances.joiningDateFrom.clear();
            //}
            //if (flatpickrInstances.joiningDateTo) {
            //    flatpickrInstances.joiningDateTo.clear();
            //}

            isInitialLoad = true;
            loadFilterEmp();
        }

        const bindUIEvents = () => {
            //$("#previewBtn").on("click", () =>
            //    $(settings.previewContainer).is(':visible') ? hidePreview() : showPreview()
            //);
            $("#downloadBtn").on("click", () => exportReport($("#exportFormatSelect").val()));
            //$("#closePreviewBtn").on("click", hidePreview);
            //$("#refreshPreviewBtn").on("click", showPreview);
        };

        const init = () => {
            settings.load();

            initializeMultiselects();
            initializeFlatpickr();
            setupLoadingOverlay();
            bindUIEvents();
            loadFilterEmp();
            initializeEmpGrid();
        }

        init();
        //const $elements = {
        //    companySelect: $(settings.companySelect),
        //    //branchSelect: $(settings.branchSelect),
        //    departmentSelect: $(settings.departmentSelect),
        //    designationSelect: $(settings.designationSelect),
        //    pEndDaysSelect: $(settings.pEndDaysSelect),
        //    joiningDateFrom: $(settings.joiningDateFrom),
        //    joiningDateTo: $(settings.joiningDateTo),
        //    exportFormatSelect: $(settings.exportFormatSelect),
        //    downloadBtn: $(settings.downloadBtn),
        //    employeeGrid: $(settings.employeeGrid),
        //    employeeGridBody: $(settings.employeeGridBody),
        //    customSearch: $(settings.customSearch),
        //    clearButton: $(settings.clearButton),
        //    loadingOverlay: null
        //};

        //const URLS = {
        //    filter: `${settings.baseUrl}/getFilterEmp`,
        //    export: `${settings.baseUrl}/ExportReport`,
        //};


        //const FILTER_SELECTORS = [
        //    $elements.companySelect,
        //    //settings.branchSelect,
        //    $elements.departmentSelect,
        //    $elements.designationSelect,
        //    $elements.pEndDaysSelect,
        //    $elements.joiningDateFrom,
        //    $elements.joiningDateTo
        //];

        //const MULTISELECT_CONFIG = [
        //    [$elements.companySelect, "Company"],
            
        //    [$elements.departmentSelect, "Department"],
        //    [$elements.designationSelect, "Designation"]
        //];

        //const CASCADING_CLEAR_MAP = {
        //    departmentSelect: [$elements.designationSelect]
        //};

        //let empDataTable = null;
        //let flatpickrInstances = {};
        //let currentAbortController = null;
        //let filterChangeBound = false;
        //let debounceTimer = null;
        //let isInitialLoad = true;

        //const toArray = val => val ? [].concat(val) : [];

        //const showLoading = () => $elements.loadingOverlay.css("display", "flex");
        
        //const hideLoading = () => $elements.loadingOverlay.hide();
        
        //const showNotification = (message, type) => {
        //    if (typeof message !== 'string' || !message.trim()) {
        //        console.warn("Empty or invalid message for notification:", message);
        //        return;
        //    }

        //    const validTypes = ['success', 'error', 'warning'];
        //    const title = validTypes.includes(type) ?
        //        (type === 'success' ? 'Success' : type === 'error' ? 'Error' : 'Warning') : 'Notification';

        //    typeof toastr !== 'undefined' ? toastr[type]?.(message, title) : alert(`${title}: ${message}`);
        //};


        //const debounce = (func, delay) => {
        //    return function (...args) {
        //        clearTimeout(debounceTimer);
        //        debounceTimer = setTimeout(() => func.apply(this, args), delay);
        //    };
        //};

        //const fetchWithErrorHandling = async (url, options = {}) => {
        //    if (currentAbortController) {
        //        currentAbortController.abort();
        //    }

        //    currentAbortController = new AbortController();

        //    try {
        //        const response = await fetch(url, {
        //            method: 'GET',
        //            headers: {
        //                'Content-Type': 'application/json',
        //                ...options.headers
        //            },
        //            signal: currentAbortController.signal,
        //            ...options
        //        });

        //        if (!response.ok) {
        //            throw new Error(`HTTP error! status: ${response.status}`);
        //        }

        //        return await response.json();
        //    } catch (error) {
        //        if (error.name === 'AbortError') {
        //            console.log('Request aborted');
        //            return null;
        //        }
        //        console.error(`API request failed for ${url}:`, error);
        //        throw error;
        //    }
        //};

        //const apiPost = (url, data) => fetchWithErrorHandling(url, {
        //    method: 'POST',
        //    body: JSON.stringify(data)
        //});


        //const setupLoadingOverlay = () => {
        //    if ($("#employeeServiceLoadingOverlay").length === 0) {
        //        $("body").append(`
        //            <div id="employeeServiceLoadingOverlay" style="display:none;position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.5);z-index:9999;justify-content:center;align-items:center;">
        //                <div style="background:white;padding:20px;border-radius:5px;box-shadow:0 0 10px rgba(0,0,0,0.3);text-align:center;">
        //                    <div class="spinner-border text-primary"></div>
        //                </div>
        //            </div>
        //        `);
        //    }
        //    $elements.loadingOverlay = $("#employeeServiceLoadingOverlay");
        //};

        //const DATE_SELECTORS = [
        //    { from: $elements.joiningDateFrom, to: $elements.joiningDateTo }
        //];
        //const initializeFlatpickr = () => {
        //    if (typeof flatpickr === 'undefined') return;

        //    const flatpickrOptions = {
        //        dateFormat: "d-m-Y",
        //        allowInput: true
        //    };

        //    DATE_SELECTORS.forEach(({ from, to }) => {
                
        //        const fromPicker = flatpickr(from[0], {
        //            ...flatpickrOptions,
        //            onChange: function (selectedDates) {
        //                if (selectedDates.length) {
        //                    flatpickrInstances[to[0].id].set('minDate', selectedDates[0]);
        //                    if (!isInitialLoad) loadFilterEmp();
        //                }
        //            }
        //        });

        //        const toPicker = flatpickr(to[0], {
        //            ...flatpickrOptions,
        //            onChange: function (selectedDates) {
        //                if (selectedDates.length) {
        //                    flatpickrInstances[from[0].id].set('maxDate', selectedDates[0]);
        //                    if (!isInitialLoad) loadFilterEmp();
        //                }
        //            }
        //        });

        //        flatpickrInstances[from[0].id] = fromPicker;
        //        flatpickrInstances[to[0].id] = toPicker;
        //    });
        //};

        //const initializeMultiselects = () => {
        //    const multiselectOptions = {
        //        enableFiltering: true,
        //        includeSelectAllOption: true,
        //        selectAllText: 'Select All',
        //        nSelectedText: 'Selected',
        //        allSelectedText: 'All Selected',
        //        filterPlaceholder: 'Search.......',
        //        buttonWidth: '100%',
        //        maxHeight: 350,
        //        enableClickableOptGroups: true,
        //        numberDisplayed: 1,
        //        enableCaseInsensitiveFiltering: true
        //    };

        //    MULTISELECT_CONFIG.forEach(([element, text]) => {
        //        element.multiselect({
        //            ...multiselectOptions,
        //            nonSelectedText: `Select ${text}`
        //        });
        //    });
        //};
        //const styleNonMultiselectElements = () => {
        //    const elementsToStyle = [
        //        $elements.joiningDateFrom,
        //        $elements.joiningDateTo,
        //        $elements.pEndDaysSelect,
        //        $elements.exportFormatSelect
        //    ];

        //    elementsToStyle.forEach(element => {
        //        element.addClass('form-control');
        //        // Add any additional CSS classes that match your multiselect styling
        //        // For example, if your multiselects have specific bootstrap classes:
        //        element.css({
        //            'width': '100%',
        //            'height': '38px', // Adjust to match multiselect height
        //            'border': '1px solid #ccc',
        //            'border-radius': '4px',
        //            'padding': '6px 12px',
        //            'font-size': '14px',
        //            'line-height': '1.42857143'
        //        });
        //    });
        //};
        //const getFilterValue = () => ({
        //    CompanyCodes: toArray($elements.companySelect.val()),

        //    DepartmentCodes: toArray($elements.departmentSelect.val()),
        //    DesignationCodes: toArray($elements.designationSelect.val()),

        //    JoiningDateFrom: $elements.joiningDateFrom.val() || null,
        //    JoiningDateTo: $elements.joiningDateTo.val() || null,
        //});

        //const populateSelect = (element, dataList) => {
        //    const existingValues = new Set();
        //    element.find('option').each(function () {
        //        existingValues.add(this.value);
        //    });

        //    const fragment = document.createDocumentFragment();
        //    dataList.forEach(item => {
        //        if (item.code && item.name && !existingValues.has(item.code)) {
        //            const option = document.createElement('option');
        //            option.value = item.code;
        //            option.textContent = item.name;
        //            fragment.appendChild(option);
        //        }
        //    });

        //    if (fragment.hasChildNodes()) {
        //        element[0].appendChild(fragment);
        //        element.multiselect('rebuild');
        //    }
        //};

        //const clearDependentDropdowns = elements => {
        //    elements.forEach(element => {
        //        element.empty().multiselect('rebuild');
        //    });
        //};

        //const setupCascadingClearEvents = () => {
        //    Object.entries(CASCADING_CLEAR_MAP).forEach(([key, children]) => {
        //        const element = $elements[key];
        //        element.off("change.clearDropdowns").on("change.clearDropdowns", () => {
        //            clearDependentDropdowns(children);
        //        });
        //    });
        //};

        //const bindFilterChangeEvents = () => {
        //    if (filterChangeBound) return;

        //    FILTER_SELECTORS.forEach(element => {
        //        element.on("change.loadFilter", loadFilterEmp);
        //    });

        //    const debouncedLoadFilter = debounce(() => {
        //        clearDependentDropdowns([
        //            $elements.departmentIds,
        //            $elements.designationIds,
        //            $elements.employeeIds
        //        ]);
        //        loadFilterEmp();
        //    }, 300);

        //    DATE_SELECTORS.forEach(({ from, to }) => {
        //        from.add(to).on('input.loadFilter', debouncedLoadFilter);
        //    });

        //    filterChangeBound = true;
        //};

        //const loadFilterEmp = async () => {
        //    try {
        //        const response = await fetchWithErrorHandling(URLS.filter, {
        //            method: 'POST',
        //            body: JSON.stringify(getFilterValue())
        //        });

        //        if (!response) return;
        //        //debugger;
        //        const result = await response;

        //        if (!result.isSuccess) {
        //            showNotification(result.message, 'error');
        //            return;
        //        }

        //        const { data } = result;
        //        console.log(data);

        //        const populateMapping = [
        //            [data.companies, $elements.companyIds],
        //            [data.branches, $elements.branchIds],
        //            [data.departments, $elements.departmentIds],
        //            [data.designations, $elements.designationIds],
        //            [data.employeeIds, $elements.employeeIds]
        //        ];

        //        populateMapping.forEach(([dataList, element]) => {
        //            if (dataList?.length) {
        //                populateSelect(element, dataList);
        //            }
        //        });

        //        setupCascadingClearEvents();
        //        bindFilterChangeEvents();

        //    } catch (error) {
        //        showNotification('Failed to load data', 'error');
        //        console.error('Load filter error:', error);
        //    }
        //};

        //const hasFilterData = (filterData) => {
        //    return Object.values(filterData).some(value =>
        //        (Array.isArray(value) && value.length > 0) ||
        //        (typeof value === 'string' && value.trim() !== '')
        //    );
        //};

        //const handleApiError = (error) => {
        //    const errorMessages = {
        //        400: 'Invalid request. Please check your inputs.',
        //        404: 'No data found for the selected criteria.',
        //        default: 'An unexpected error occurred. Please try again later.'
        //    };

        //    const errorMessage = errorMessages[error.status] || errorMessages.default;
        //    showNotification(errorMessage, 'error');
        //};

        //const createDownloadLink = (blob, filename) => {
        //    const link = document.createElement("a");
        //    link.href = URL.createObjectURL(blob);
        //    link.download = filename;
        //    link.style.display = 'none';
        //    document.body.appendChild(link);
        //    link.click();
        //    document.body.removeChild(link);
        //    URL.revokeObjectURL(link.href);
        //};

        //const handlePrintDownload = (blob) => {
        //    const iframe = document.createElement('iframe');
        //    iframe.style.display = 'none';
        //    iframe.src = URL.createObjectURL(blob);
        //    iframe.onload = function () {
        //        this.contentWindow.focus();
        //        this.contentWindow.print();

        //        setTimeout(() => {
        //            URL.revokeObjectURL(iframe.src);
        //            document.body.removeChild(iframe);
        //        }, 1000);
        //    };
        //    document.body.appendChild(iframe);
        //};

        //const exportReport = async (format) => {
        //    showLoading();

        //    try {
        //        const response = await fetchWithErrorHandling(URLS.export, {
        //            method: 'POST',
        //            body: JSON.stringify({ FilterData: getFilterValue(), ExportFormat: format })
        //        });

        //        if (!response) return;

        //        const blob = await response.blob();
        //        hideLoading();

        //        let filename = 'report.pdf';
        //        const disposition = response.headers.get('Content-Disposition');
        //        if (disposition?.includes('filename=')) {
        //            const match = disposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
        //            if (match?.[1]) {
        //                filename = match[1].replace(/['"]/g, '');
        //            }
        //        }

        //        if (format === "download") {
        //            handlePrintDownload(blob);
        //        } else {
        //            createDownloadLink(blob, filename);
        //        }

        //        clearAllFilters();
        //        showNotification('Report exported successfully', 'success');

        //    } catch (error) {
        //        hideLoading();
        //        handleApiError(error);
        //    }
        //};

        //const clearAllFilters = () => {
        //    FILTER_SELECTORS.forEach(element => {
        //        element.multiselect('deselectAll', false);
        //        element.multiselect('updateButtonText');
        //    });

        //    Object.values(flatpickrInstances).forEach(instance => {
        //        instance?.clear?.();
        //    });

        //    hidePreview();
        //    isInitialLoad = true;
        //    loadFilterEmp();
        //};

        //const cacheUIElements = () => {
        //    $elements.downloadBtn = $("#downloadBtn");
        //    $elements.exportFormatSelect = $("#exportFormatSelect");
        //};

        //const bindUIEvents = () => {
        //    cacheUIElements();

        //    $elements.downloadBtn.on("click", () => {
        //        exportReport($elements.exportFormatSelect.val());
        //    });

        //};

        //const init = () => {
        //    settings.load();
        //    setupLoadingOverlay();
        //    initializeMultiselects();
        //    styleNonMultiselectElements();
        //    bindUIEvents();
        //    loadFilterEmp();
        //    initializeFlatpickr();
        //};

        ////$elements.previewContainer.hide();
        //init();

        //return {
        //    destroy: () => {
        //        if (currentAbortController) {
        //            currentAbortController.abort();
        //        }

        //        FILTER_SELECTORS.forEach(element => {
        //            element.off('.loadFilter');
        //        });

        //        DATE_SELECTORS.forEach(({ from, to }) => {
        //            from.add(to).off('.loadFilter');
        //        });

        //        Object.entries(CASCADING_CLEAR_MAP).forEach(([key]) => {
        //            $elements[key].off('.clearDropdowns');
        //        });

        //        Object.values(flatpickrInstances).forEach(instance => {
        //            instance?.destroy?.();
        //        });

        //        if (currentPdfUrl) {
        //            URL.revokeObjectURL(currentPdfUrl);
        //        }

        //        if (debounceTimer) {
        //            clearTimeout(debounceTimer);
        //        }
        //    }
        //};
    }
})(jQuery);