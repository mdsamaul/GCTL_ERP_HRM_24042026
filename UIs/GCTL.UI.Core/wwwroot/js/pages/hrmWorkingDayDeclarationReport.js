(function ($) {
    $.workingDayDeclarationReport = function (options) {
        const settings = $.extend({
            baseUrl: "/",
            companyIds: "#companySelect",
            branchIds: "#branchSelect",
            departmentIds: "#departmentSelect",
            designationIds: "#designationSelect",
            employeeIds: "#employeeSelect",
            typeIds: "#workingDateSelect",
            monthIds: "#monthSelect",
            salaryYear: "#yearSelect",
            previewContainer: "#workingDayReport-container .card-body",
            load: () => console.log("Loading...")
        }, options);

        const urls = {
            filter: `${settings.baseUrl}/getAllFilterEmp`,
            export: `${settings.baseUrl}/ExportReport`,
            preview: `${settings.baseUrl}/PreviewReport`
        };

        let isInitialLoad = true;
        let filterChangeBound = false;

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

        const initializeMultiselects = () => {
            const selectors = [
                [settings.companyIds, 'Company(s)'],
                [settings.branchIds, 'Branch(es)'],
                [settings.departmentIds, 'Department(s)'],
                [settings.designationIds, 'Designation(s)'],
                [settings.employeeIds, 'Employee(s)'],
                [settings.typeIds, 'a Date'],
                [settings.monthIds, 'Month(s)']
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
                    maxHeight: 350,
                    enableClickableOptGroups: true,
                    numberDisplayed: 1,
                    enableCaseInsensitiveFiltering: true
                });
            });
        };

        const setCurrentMonthAndYear = () => {
            const now = new Date();
            $(settings.salaryYear).val(now.getFullYear().toString());
            window.currentMonthToSelect = (now.getMonth() + 1).toString();
        };

        const getFilterValue = () => ({
            CompanyCodes: toArray($(settings.companyIds).val()),
            BranchCodes: toArray($(settings.branchIds).val()),
            DepartmentCodes: toArray($(settings.departmentIds).val()),
            DesignationCodes: toArray($(settings.designationIds).val()),
            EmployeeIDs: toArray($(settings.employeeIds).val()),
            MonthIDs: toArray($(settings.monthIds).val()),
            WorkingDates: toArray($(settings.typeIds).val()),
            SalaryYear: $(settings.salaryYear).val() || null
        });

        const populateSelect = (selector, dataList) => {
            const $select = $(selector);

            if (selector === settings.typeIds && $select.find('option[value=""]').length === 0) {
                $select.append(`<option value="">Select a Date</option>`);
            }

            dataList.forEach(item => {
                if (item.code && item.name && $select.find(`option[value="${item.code}"]`).length === 0) {
                    $select.append(`<option value="${item.code}">${item.name}</option>`);
                }
            });
            $select.multiselect('rebuild');
        };

        const clearDependentDropdowns = selectors => {
            selectors.forEach(selector => {
                $(selector).empty().multiselect('rebuild');
            });
        };

        const setupCascadingClearEvents = () => {
            const clearMap = {
                [settings.companyIds]: [settings.branchIds, settings.departmentIds, settings.designationIds, settings.employeeIds],
                [settings.branchIds]: [settings.departmentIds, settings.designationIds, settings.employeeIds],
                [settings.departmentIds]: [settings.designationIds, settings.employeeIds],
                [settings.designationIds]: [settings.employeeIds],
                [settings.monthIds]: [settings.typeIds, settings.departmentIds, settings.designationIds, settings.employeeIds],
                [settings.typeIds]: [settings.departmentIds, settings.designationIds, settings.employeeIds],
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
                    settings.companyIds,
                    settings.branchIds,
                    settings.departmentIds,
                    settings.designationIds,
                    settings.employeeIds,
                    settings.monthIds,
                    settings.typeIds
                ].join(',');

                $(filterSelectors).on("change.loadFilter", function () {
                    loadFilterEmp();
                });

                $(settings.salaryYear).on('input.loadFilter', function () {
                    clearTimeout(window.yearInputTimeout);
                    window.yearInputTimeout = setTimeout(() => {
                        clearDependentDropdowns([
                            settings.departmentIds,
                            settings.designationIds,
                            settings.employeeIds,
                            settings.typeIds,
                        ]);
                        loadFilterEmp();
                    }, 500);
                });

                filterChangeBound = true;
            }
        };

        const autoSelectCurrentMonth = () => {
            if (window.currentMonthToSelect && isInitialLoad) {
                const monthSelect = $(settings.monthIds);
                if (monthSelect.find(`option[value="${window.currentMonthToSelect}"]`).length) {
                    monthSelect.val(window.currentMonthToSelect).multiselect('refresh');
                    window.currentMonthToSelect = null;
                    setTimeout(() => {
                        loadFilterEmp();
                    }, 100);
                }
            }
        };

        const loadFilterEmp = () => {
            $.ajax({
                url: urls.filter,
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
                    if (data.companies?.length) 
                        populateSelect(settings.companyIds, data.companies);
                    
                    if (data.branches?.length) 
                        populateSelect(settings.branchIds, data.branches);
                    
                    if (data.departments?.length) 
                        populateSelect(settings.departmentIds, data.departments);
                    
                    if (data.designations?.length) 
                        populateSelect(settings.designationIds, data.designations);
                    
                    if (data.employees?.length) 
                        populateSelect(settings.employeeIds, data.employees);
                    
                    if (data.workingDayDates?.length) 
                        populateSelect(settings.typeIds, data.workingDayDates);
                    
                    if (data.months?.length) {
                        populateSelect(settings.monthIds, data.months);
                        autoSelectCurrentMonth();
                    }

                    setupCascadingClearEvents();
                    bindFilterChangeEvents();
                },
                error: () => {
                    showToast('error', 'Failed to load data');
                }
            });
        };

        const showPreview = () => {
            const filterData = getFilterValue();
            const hasFilters = Object.values(filterData).some(value =>
                (Array.isArray(value) && value.length > 0) ||
                (typeof value === 'string' && value.trim() !== '')
            );

            if (!hasFilters) {
                showToast('warning', 'No working day is found to generate report');
                return;
            }

            $("#workingDayReport-container").show();
            // $("#previewLoadingIndicator").show();
            showLoading();
            $("#previewIframe").hide();

            $.ajax({
                url: urls.preview,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ FilterData: filterData, ExportFormat: 'pdf' }),
                xhrFields: { responseType: 'blob' },
                success: data => {
                    //$("#previewLoadingIndicator").hide();
                    hideLoading();
                    if (window.currentPdfUrl) {
                        window.URL.revokeObjectURL(window.currentPdfUrl);
                    }

                    const blob = new Blob([data], { type: 'application/pdf' });
                    window.currentPdfUrl = window.URL.createObjectURL(blob);
                    $("#previewIframe").attr('src', window.currentPdfUrl).show();
                },
                error: xhr => {
                    //$("#previewLoadingIndicator").hide();
                    hideLoading();
                    $("#workingDayReport-container").hide();
                    let errorMessage = 'Preview failed. Please try again.'; 

                    switch (xhr.status) {
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

        const hidePreview = () => {
            $("#workingDayReport-container").hide();
            $("#previewIframe").attr('src', '');
            if (window.currentPdfUrl) {
                window.URL.revokeObjectURL(window.currentPdfUrl);
                window.currentPdfUrl = null;
            }
        };

        const exportReport = format => {
            showLoading();

            $.ajax({
                url: urls.export,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ FilterData: getFilterValue(), ExportFormat: format }),
                xhrFields: { responseType: 'blob' },
                success: (data, status, xhr) => {
                    hideLoading();

                    const contentType = xhr.getResponseHeader('Content-Type');
                    const disposition  = xhr.getResponseHeader('Content-Disposition');

                    let filename;
                    if (disposition && disposition.indexOf("filename=") != -1) {
                        const match = disposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                        if (match && match[1]) {
                            filename=match[1].replace(/['"]/g, '');
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
                    let errorMessage = 'Export failed. Please try again.'; // Default error message

                    switch (e.status) {
                        case 400: // Bad Request
                            errorMessage = 'Invalid request. Please check your inputs.';
                            break;
                        case 404: // Not Found
                            errorMessage = 'No data found for the selected criteria.';
                            break;
                        default: // Internal Server Error
                            errorMessage = 'An unexpected error occurred. Please try again later.';
                            break;
                    }
                    showToast('error', errorMessage);
                }
            });
        };

        const clearAllFilters = () => {
            const filterSelectors = [
                settings.companyIds,
                settings.branchIds,
                settings.departmentIds,
                settings.designationIds,
                settings.employeeIds,
                settings.typeIds,
                settings.monthIds
            ];

            filterSelectors.forEach(selector => {
                $(selector).multiselect('deselectAll', false);
                $(selector).multiselect('updateButtonText');
            });

            const currentYear = new Date().getFullYear();
            $(settings.salaryYear).val(currentYear.toString());

            hidePreview();

            isInitialLoad = true;

            loadFilterEmp();

        };

        const bindUIEvents = () => {
            $("#previewBtn").on("click", () =>
                $("#workingDayReport-container").is(':visible') ? hidePreview() : showPreview()
            );
            $("#downloadBtn").on("click", () => exportReport($("#exportFormatSelect").val()));
            $("#closePreviewBtn").on("click", hidePreview);
            $("#refreshPreviewBtn").on("click", showPreview);
        };

        const init = () => {
            settings.load();
            setCurrentMonthAndYear();
            initializeMultiselects();
            setupLoadingOverlay();
            bindUIEvents();
            loadFilterEmp();
        };

        init();
    };
})(jQuery);