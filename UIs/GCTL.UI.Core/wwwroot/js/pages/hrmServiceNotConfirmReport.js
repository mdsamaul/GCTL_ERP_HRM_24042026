(function ($) {
    $.notConfirmReport = function (options) {
        const settings = $.extend({
            baseUrl: "/",
            companyIds: "#companySelect",
            branchIds: "#branchSelect",
            departmentIds: "#departmentSelect",
            designationIds: "#designationSelect",
            employeeIds: "#employeeSelect",
            joiningDateFrom: "#joiningDateFromSelect",
            joiningDateTo: "#joiningDateToSelect",
            effectiveDateFrom: "#effectiveDateFromSelect",
            effectiveDateTo: "#effectiveDateToSelect",
            duePaymentDateFrom: "#duePaymentDateFromSelect",
            duePaymentDateTo: "#duePaymentDateToSelect",
            previewContainer: "#notConfirmationReport-container .card-body",
            previewContainerBody: "#notConfirmationReport-container",
            iFrame: "#previewIframe",
            load: () => console.log("Loading...")
        }, options);

        const $elements = {
            companyIds: $(settings.companyIds),
            branchIds: $(settings.branchIds),
            departmentIds: $(settings.departmentIds),
            designationIds: $(settings.designationIds),
            employeeIds: $(settings.employeeIds),
            joiningDateFrom: $(settings.joiningDateFrom),
            joiningDateTo: $(settings.joiningDateTo),
            effectiveDateFrom: $(settings.effectiveDateFrom),
            effectiveDateTo: $(settings.effectiveDateTo),
            duePaymentDateFrom: $(settings.duePaymentDateFrom),
            duePaymentDateTo: $(settings.duePaymentDateTo),
            previewContainer: $(settings.previewContainer),
            previewContainerBody: $(settings.previewContainerBody),
            iFrame: $(settings.iFrame),
            loadingOverlay: null,
            previewBtn: null,
            downloadBtn: null,
            closePreviewBtn: null,
            refreshPreviewBtn: null,
            exportFormatSelect: null
        };

        const URLS = {
            filter: `${settings.baseUrl}/getAllFilterEmp`,
            export: `${settings.baseUrl}/ExportReport`,
            preview: `${settings.baseUrl}/PreviewReport`
        };

        const DATE_SELECTORS = [
            { from: $elements.joiningDateFrom, to: $elements.joiningDateTo },
            { from: $elements.effectiveDateFrom, to: $elements.effectiveDateTo },
            { from: $elements.duePaymentDateFrom, to: $elements.duePaymentDateTo }
        ];

        const FILTER_SELECTORS = [
            $elements.companyIds,
            $elements.branchIds,
            $elements.departmentIds,
            $elements.designationIds,
            $elements.employeeIds
        ];

        const MULTISELECT_CONFIG = [
            [$elements.companyIds, 'Company(s)'],
            [$elements.branchIds, 'Branch(es)'],
            [$elements.departmentIds, 'Department(s)'],
            [$elements.designationIds, 'Designation(s)'],
            [$elements.employeeIds, 'Employee(s)']
        ];

        const CASCADING_CLEAR_MAP = {
            companyIds: [$elements.branchIds, $elements.departmentIds, $elements.designationIds, $elements.employeeIds],
            branchIds: [$elements.departmentIds, $elements.designationIds, $elements.employeeIds],
            departmentIds: [$elements.designationIds, $elements.employeeIds],
            designationIds: [$elements.employeeIds]
        };

        let isInitialLoad = true;
        let filterChangeBound = false;
        let flatpickrInstances = {};
        let currentPdfUrl = null;
        let debounceTimer = null;
        let currentAbortController = null;

        const toArray = val => val ? [].concat(val) : [];

        const showLoading = () => $elements.loadingOverlay.css("display", "flex");
        const hideLoading = () => $elements.loadingOverlay.hide();

        const showToast = (type, message) => {
            typeof toastr !== 'undefined' ? toastr[type](message) : alert(message);
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

                return response;
            } catch (error) {
                if (error.name === 'AbortError') {
                    console.log('Request aborted');
                    return null;
                }
                console.error('Fetch error:', error);
                throw error;
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
            $elements.loadingOverlay = $("#customLoadingOverlay");
        };

        const initializeFlatpickr = () => {
            if (typeof flatpickr === 'undefined') return;

            const flatpickrOptions = {
                dateFormat: "d-m-Y",
                allowInput: true
            };

            DATE_SELECTORS.forEach(({ from, to }) => {
                const fromPicker = flatpickr(from[0], {
                    ...flatpickrOptions,
                    onChange: function (selectedDates) {
                        if (selectedDates.length) {
                            flatpickrInstances[to[0].id].set('minDate', selectedDates[0]);
                            if (!isInitialLoad) loadFilterEmp();
                        }
                    }
                });

                const toPicker = flatpickr(to[0], {
                    ...flatpickrOptions,
                    onChange: function (selectedDates) {
                        if (selectedDates.length) {
                            flatpickrInstances[from[0].id].set('maxDate', selectedDates[0]);
                            if (!isInitialLoad) loadFilterEmp();
                        }
                    }
                });

                flatpickrInstances[from[0].id] = fromPicker;
                flatpickrInstances[to[0].id] = toPicker;
            });
        };

        const initializeMultiselects = () => {
            const multiselectOptions = {
                enableFiltering: true,
                includeSelectAllOption: true,
                selectAllText: 'Select All',
                nSelectedText: 'Selected',
                allSelectedText: 'All Selected',
                filterPlaceholder: 'Search.......',
                buttonWidth: '100%',
                maxHeight: 350,
                enableClickableOptGroups: true,
                numberDisplayed: 1,
                enableCaseInsensitiveFiltering: true
            };

            MULTISELECT_CONFIG.forEach(([element, text]) => {
                element.multiselect({
                    ...multiselectOptions,
                    nonSelectedText: `Select ${text}`
                });
            });
        };

        const getFilterValue = () => ({
            CompanyCodes: toArray($elements.companyIds.val()),
            BranchCodes: toArray($elements.branchIds.val()),
            DepartmentCodes: toArray($elements.departmentIds.val()),
            DesignationCodes: toArray($elements.designationIds.val()),
            EmployeeIDs: toArray($elements.employeeIds.val()),
            JoiningDateFrom: $elements.joiningDateFrom.val() || null,
            JoiningDateTo: $elements.joiningDateTo.val() || null,
            EffectiveDateFrom: $elements.effectiveDateFrom.val() || null,
            EffectiveDateTo: $elements.effectiveDateTo.val() || null,
            DuePaymentDateFrom: $elements.duePaymentDateFrom.val() || null,
            DuePaymentDateTo: $elements.duePaymentDateTo.val() || null
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
                const element = $elements[key];
                element.off("change.clearDropdowns").on("change.clearDropdowns", () => {
                    clearDependentDropdowns(children);
                });
            });
        };

        const bindFilterChangeEvents = () => {
            if (filterChangeBound) return;

            FILTER_SELECTORS.forEach(element => {
                element.on("change.loadFilter", loadFilterEmp);
            });

            const debouncedLoadFilter = debounce(() => {
                clearDependentDropdowns([
                    $elements.departmentIds,
                    $elements.designationIds,
                    $elements.employeeIds
                ]);
                loadFilterEmp();
            }, 300);

            DATE_SELECTORS.forEach(({ from, to }) => {
                from.add(to).on('input.loadFilter', debouncedLoadFilter);
            });

            filterChangeBound = true;
        };

        const loadFilterEmp = async () => {
            try {
                const response = await fetchWithErrorHandling(URLS.filter, {
                    method: 'POST',
                    body: JSON.stringify(getFilterValue())
                });

                if (!response) return;

                const result = await response.json();

                if (!result.isSuccess) {
                    showToast('error', result.message);
                    return;
                }

                const { data } = result;
                console.log(data);

                const populateMapping = [
                    [data.companies, $elements.companyIds],
                    [data.branches, $elements.branchIds],
                    [data.departments, $elements.departmentIds],
                    [data.designations, $elements.designationIds],
                    [data.employeeIds, $elements.employeeIds]
                ];

                populateMapping.forEach(([dataList, element]) => {
                    if (dataList?.length) {
                        populateSelect(element, dataList);
                    }
                });

                setupCascadingClearEvents();
                bindFilterChangeEvents();

            } catch (error) {
                showToast('error', 'Failed to load data');
                console.error('Load filter error:', error);
            }
        };

        const hasFilterData = (filterData) => {
            return Object.values(filterData).some(value =>
                (Array.isArray(value) && value.length > 0) ||
                (typeof value === 'string' && value.trim() !== '')
            );
        };

        const showPreview = async () => {
            const filterData = getFilterValue();

            if (!hasFilterData(filterData)) {
                showToast('warning', 'No working day is found to generate report');
                return;
            }

            $elements.previewContainer.show();
            showLoading();
            $elements.iFrame.attr('src', '');

            try {
                const response = await fetchWithErrorHandling(URLS.preview, {
                    method: 'POST',
                    body: JSON.stringify({ FilterData: filterData, ExportFormat: 'pdf' })
                });

                if (!response) return;

                const blob = await response.blob();
                hideLoading();

                if (currentPdfUrl) {
                    URL.revokeObjectURL(currentPdfUrl);
                }

                currentPdfUrl = URL.createObjectURL(blob);
                $elements.iFrame.attr('src', currentPdfUrl).show();

            } catch (error) {
                hideLoading();
                $elements.previewContainer.hide();
                handleApiError(error);
            }
        };

        const hidePreview = () => {
            $elements.previewContainer.hide();
            $elements.iFrame.attr('src', '');

            if (currentPdfUrl) {
                URL.revokeObjectURL(currentPdfUrl);
                currentPdfUrl = null;
            }
        };

        const handleApiError = (error) => {
            const errorMessages = {
                400: 'Invalid request. Please check your inputs.',
                404: 'No data found for the selected criteria.',
                default: 'An unexpected error occurred. Please try again later.'
            };

            const errorMessage = errorMessages[error.status] || errorMessages.default;
            showToast('error', errorMessage);
        };

        const createDownloadLink = (blob, filename) => {
            const link = document.createElement("a");
            link.href = URL.createObjectURL(blob);
            link.download = filename;
            link.style.display = 'none';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(link.href);
        };

        const handlePrintDownload = (blob) => {
            const iframe = document.createElement('iframe');
            iframe.style.display = 'none';
            iframe.src = URL.createObjectURL(blob);
            iframe.onload = function () {
                this.contentWindow.focus();
                this.contentWindow.print();

                setTimeout(() => {
                    URL.revokeObjectURL(iframe.src);
                    document.body.removeChild(iframe);
                }, 1000);
            };
            document.body.appendChild(iframe);
        };

        const exportReport = async (format) => {
            showLoading();

            try {
                const response = await fetchWithErrorHandling(URLS.export, {
                    method: 'POST',
                    body: JSON.stringify({ FilterData: getFilterValue(), ExportFormat: format })
                });

                if (!response) return;

                const blob = await response.blob();
                hideLoading();

                let filename = 'report.pdf';
                const disposition = response.headers.get('Content-Disposition');
                if (disposition?.includes('filename=')) {
                    const match = disposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                    if (match?.[1]) {
                        filename = match[1].replace(/['"]/g, '');
                    }
                }

                if (format === "download") {
                    handlePrintDownload(blob);
                } else {
                    createDownloadLink(blob, filename);
                }

                clearAllFilters();
                showToast('success', 'Report exported successfully');

            } catch (error) {
                hideLoading();
                handleApiError(error);
            }
        };

        const clearAllFilters = () => {
            FILTER_SELECTORS.forEach(element => {
                element.multiselect('deselectAll', false);
                element.multiselect('updateButtonText');
            });

            Object.values(flatpickrInstances).forEach(instance => {
                instance?.clear?.();
            });

            hidePreview();
            isInitialLoad = true;
            loadFilterEmp();
        };

        const cacheUIElements = () => {
            $elements.previewBtn = $("#previewBtn");
            $elements.downloadBtn = $("#downloadBtn");
            $elements.closePreviewBtn = $("#closePreviewBtn");
            $elements.refreshPreviewBtn = $("#refreshPreviewBtn");
            $elements.exportFormatSelect = $("#exportFormatSelect");
        };

        const bindUIEvents = () => {
            cacheUIElements();

            $elements.previewBtn.on("click", () => {
                $elements.previewContainer.is(':visible') ? hidePreview() : showPreview();
            });

            $elements.downloadBtn.on("click", () => {
                exportReport($elements.exportFormatSelect.val());
            });

            $elements.closePreviewBtn.on("click", hidePreview);
            $elements.refreshPreviewBtn.on("click", showPreview);
        };

        const init = () => {
            settings.load();
            setupLoadingOverlay();
            initializeMultiselects();
            initializeFlatpickr();
            bindUIEvents();
            loadFilterEmp();
        };

        $elements.previewContainer.hide();
        init();

        return {
            destroy: () => {
                if (currentAbortController) {
                    currentAbortController.abort();
                }

                FILTER_SELECTORS.forEach(element => {
                    element.off('.loadFilter');
                });

                DATE_SELECTORS.forEach(({ from, to }) => {
                    from.add(to).off('.loadFilter');
                });

                Object.entries(CASCADING_CLEAR_MAP).forEach(([key]) => {
                    $elements[key].off('.clearDropdowns');
                });

                Object.values(flatpickrInstances).forEach(instance => {
                    instance?.destroy?.();
                });

                if (currentPdfUrl) {
                    URL.revokeObjectURL(currentPdfUrl);
                }

                if (debounceTimer) {
                    clearTimeout(debounceTimer);
                }
            }
        };
    };
})(jQuery);