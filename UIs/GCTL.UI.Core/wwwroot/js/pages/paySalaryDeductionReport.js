(function ($) {
    $.salaryDeductionReport = function (options) {
        const settings = $.extend({
            baseUrl: "/",
            companyIds: "#companySelect",
            branchIds: "#branchSelect",
            departmentIds: "#departmentSelect",
            designationIds: "#designationSelect",
            employeeIds: "#employeeSelect",
            typeIds: "#typeSelect",
            monthIds: "#monthSelect",
            salaryYear: "#yearSelect",
            previewContainer: "#salaryDeductionReport-container .card-body",
            load: () => console.log("Loading...")
        }, options);

        const urls = {
            filter: `${settings.baseUrl}/getAllFilterEmp`,
            export: `${settings.baseUrl}/ExportReport`,
            preview: `${settings.baseUrl}/PreviewReport`
        };

        let isInitialLoad = true;
        let filterChangeBound = false;

        // Utility functions
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
                [settings.typeIds, 'Deduction Type(s)'],
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
            // Store current month for later selection
            window.currentMonthToSelect = (now.getMonth() + 1).toString();
        };

        const getFilterValue = () => ({
            CompanyCodes: toArray($(settings.companyIds).val()),
            BranchCodes: toArray($(settings.branchIds).val()),
            DepartmentCodes: toArray($(settings.departmentIds).val()),
            DesignationCodes: toArray($(settings.designationIds).val()),
            EmployeeIDs: toArray($(settings.employeeIds).val()),
            MonthIDs: toArray($(settings.monthIds).val()),
            DeductionTypeIDs: toArray($(settings.typeIds).val()),
            SalaryYear: $(settings.salaryYear).val() || null
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
                $(selector).empty().multiselect('rebuild');
            });
        };

        const setupCascadingClearEvents = () => {
            const clearMap = {
                [settings.companyIds]: [settings.branchIds, settings.departmentIds, settings.designationIds, settings.employeeIds],
                [settings.branchIds]: [settings.departmentIds, settings.designationIds, settings.employeeIds],
                [settings.departmentIds]: [settings.designationIds, settings.employeeIds],
                [settings.designationIds]: [settings.employeeIds],
                [settings.monthIds]: [settings.departmentIds, settings.designationIds, settings.employeeIds],
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

                // Handle year input with debounce
                $(settings.salaryYear).on('input.loadFilter', function () {
                    clearTimeout(window.yearInputTimeout);
                    window.yearInputTimeout = setTimeout(() => {
                        clearDependentDropdowns([
                            settings.companyIds,
                            settings.branchIds,
                            settings.departmentIds,
                            settings.designationIds,
                            settings.employeeIds
                            //settings.typeIds,
                            //settings.monthIds
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
                    // Trigger cascade after month selection
                    setTimeout(() => {
                        loadFilterEmp();
                    }, 1000);
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
                    console.log(res.data);
                    // Populate dropdowns with available data
                    if (data.companies?.length) {
                        populateSelect(settings.companyIds, data.companies);
                    }
                    if (data.branches?.length) {
                        populateSelect(settings.branchIds, data.branches);
                    }
                    if (data.departments?.length) {
                        populateSelect(settings.departmentIds, data.departments);
                    }
                    if (data.designations?.length) {
                        populateSelect(settings.designationIds, data.designations);
                    }
                    if (data.employees?.length) {
                        populateSelect(settings.employeeIds, data.employees);
                    }
                    if (data.deductionTypes?.length) {
                        populateSelect(settings.typeIds, data.deductionTypes);
                    }

                    // Setup cascading events
                    setupCascadingClearEvents();
                    bindFilterChangeEvents();

                    // Handle initial load - populate months and auto-select current month

                    //setTimeout(() => {
                    if (data.months?.length) {
                        populateSelect(settings.monthIds, data.months);
                        autoSelectCurrentMonth();
                    }
                    //isInitialLoad = false;
                    //}, 100);
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
                showToast('warning', 'No salary Deduction is found to generate report');
                return;
            }

            $("#salaryDeductionReport-container").show();
            $("#previewLoadingIndicator").show();
            $("#previewIframe").hide();

            $.ajax({
                url: urls.preview,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ FilterData: filterData, ExportFormat: 'pdf' }),
                xhrFields: { responseType: 'blob' },
                success: data => {
                    $("#previewLoadingIndicator").hide();

                    if (window.currentPdfUrl) {
                        window.URL.revokeObjectURL(window.currentPdfUrl);
                    }

                    const blob = new Blob([data], { type: 'application/pdf' });
                    window.currentPdfUrl = window.URL.createObjectURL(blob);
                    $("#previewIframe").attr('src', window.currentPdfUrl).show();
                },
                error: xhr => {
                    $("#previewLoadingIndicator").hide();
                    $("#salaryDeductionReport-container").hide();
                    showToast('error', 'Preview failed. Please try again.');
                }
            });
        };

        const hidePreview = () => {
            $("#salaryDeductionReport-container").hide();
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

                    const timestamp = new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '');
                    const extensions = { pdf: 'pdf', excel: 'xlsx', default: 'docx' };
                    const filename = `SalaryDeductionReport_${timestamp}.${extensions[format] || extensions.default}`;

                    const blob = new Blob([data], { type: xhr.getResponseHeader('Content-Type') });
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
                error: () => {
                    hideLoading();
                    showToast('error', 'Export failed. Please try again.');
                }
            });
        };

        const clearAllFilters = () => {
            // Clear all multiselect dropdowns
            console.log('clear');
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
                $("#salaryDeductionReport-container").is(':visible') ? hidePreview() : showPreview()
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








//(function ($) {
//    $.salaryDeductionReport = function (options) {
//        var settings = $.extend({
//            baseUrl: "/",
//            companyIds: "#companySelect",
//            branchIds: "#branchSelect",
//            departmentIds: "#departmentSelect",
//            designationIds: "#designationSelect",
//            employeeIds: "#employeeSelect",
//            typeIds: "#typeSelect",
//            monthIds: "#monthSelect",
//            salaryYear: "#yearSelect",
//            previewContainer: "#salaryDeductionReport-container .card-body", // Container for iframe
//            load: function () {
//                console.log("Loading...");
//            }
//        }, options);

//        var filterUrl = settings.baseUrl + "/getAllFilterEmp";
//        var exportUrl = settings.baseUrl + "/ExportReport";
//        var previewUrl = settings.baseUrl + "/PreviewReport";
//        var reportDataTable = null;
//        var isInitialLoad = true;

//        // Setup loading overlay
//        var setupLoadingOverlay = function () {
//            if ($("#customLoadingOverlay").length === 0) {
//                $("body").append(`
//                    <div id="customLoadingOverlay" style="
//                        display: none;
//                        position: fixed;
//                        top: 0;
//                        left: 0;
//                        width: 100%;
//                        height: 100%;
//                        background-color: rgba(0, 0, 0, 0.5);
//                        z-index: 9999;
//                        justify-content: center;
//                        align-items: center;">
//                        <div style="
//                            background-color: white;
//                            padding: 20px;
//                            border-radius: 5px;
//                            box-shadow: 0 0 10px rgba(0,0,0,0.3);
//                            text-align: center;">
//                            <div class="spinner-border text-primary" role="status">
//                                <span class="sr-only">Loading...</span>
//                            </div>
//                            <p style="margin-top: 10px; margin-bottom: 0;">Loading data...</p>
//                        </div>
//                    </div>
//                `);
//            }
//        };

//        // Setup preview iframe in container
//        var setupPreviewContainer = function () {
//            var container = $(settings.previewContainer);
//            if (container.length === 0) {
//                console.error("Preview container not found:", settings.previewContainer);
//                return;
//            }

//            // Ensure the main container is initially hidden
//            $("#salaryDeductionReport-container").hide();

//            // Clear existing content and add iframe structure
//            container.html(`
//                <div id="previewToolbar" style="
//                    padding: 10px 15px;
//                    background-color: #f8f9fa;
//                    border-bottom: 1px solid #dee2e6;
//                    display: flex;
//                    justify-content: space-between;
//                    align-items: center;
//                    border-radius: 0.375rem 0.375rem 0 0;">
//                    <h6 style="margin: 0; color: #495057;">Report Preview</h6>
//                    <div>
//                        <button id="refreshPreviewBtn" class="btn btn-sm btn-outline-primary me-2" title="Refresh Preview">
//                            <i class="fas fa-sync-alt"></i> Refresh
//                        </button>
//                        <button id="closePreviewBtn" class="btn btn-sm btn-outline-danger" title="Close Preview">
//                            <i class="fas fa-times"></i> Close
//                        </button>
//                    </div>
//                </div>
//                <div id="previewContent" style="position: relative;">
//                    <iframe id="previewIframe" 
//                        style="width: 100%; height: 600px; border: none; border-radius: 0 0 0.375rem 0.375rem; display: none;"
//                        src="">
//                    </iframe>
//                    <div id="previewLoadingIndicator" style="
//                        position: absolute;
//                        top: 50%;
//                        left: 50%;
//                        transform: translate(-50%, -50%);
//                        text-align: center;
//                        background: rgba(255,255,255,0.9);
//                        padding: 20px;
//                        border-radius: 8px;
//                        box-shadow: 0 2px 10px rgba(0,0,0,0.1);
//                        display: none;">
//                        <div class="spinner-border text-primary" role="status">
//                            <span class="sr-only">Loading...</span>
//                        </div>
//                        <p style="margin-top: 10px; margin-bottom: 0;">Loading preview...</p>
//                    </div>
//                </div>
//            `);

//            // Bind toolbar events
//            $("#closePreviewBtn").on("click", function () {
//                hidePreview();
//            });

//            $("#refreshPreviewBtn").on("click", function () {
//                showPreview();
//            });
//        };

//        function showLoading() {
//            $("#customLoadingOverlay").css("display", "flex");
//        }

//        function hideLoading() {
//            $("#customLoadingOverlay").hide();
//        }

//        //var reportDataTable = null;

//        // Set current month and year
//        var setCurrentMonthAndYear = function () {
//            var currentDate = new Date();
//            var currentMonth = currentDate.getMonth() + 1; // JavaScript months are 0-indexed
//            var currentYear = currentDate.getFullYear();

//            // Set current year in the year textbox
//            $(settings.salaryYear).val(currentYear.toString());

//            // Store current month to select it later when options are loaded
//            window.currentMonthToSelect = currentMonth.toString();
//        };

//        var initializeMultiselects = function () {
//            var configs = [
//                { selector: settings.companyIds, nonSelectedText: 'Select Company(s)' },
//                { selector: settings.branchIds, nonSelectedText: 'Select Branch(es)' },
//                { selector: settings.departmentIds, nonSelectedText: 'Select Department(s)' },
//                { selector: settings.designationIds, nonSelectedText: 'Select Designation(s)' },
//                { selector: settings.employeeIds, nonSelectedText: 'Select Employee(s)' },
//                { selector: settings.typeIds, nonSelectedText: 'Select Deduction Type(s)' },
//                { selector: settings.monthIds, nonSelectedText: 'Select Month(s)' }
//            ];

//            configs.forEach(function (config) {
//                $(config.selector).multiselect({
//                    enableFiltering: true,
//                    includeSelectAllOption: true,
//                    selectAllText: 'Select All',
//                    nonSelectedText: config.nonSelectedText,
//                    nSelectedText: 'Selected',
//                    allSelectedText: 'All Selected',
//                    filterPlaceholder: 'Search.......',
//                    buttonWidth: '100%',
//                    maxHeight: 350,
//                    enableClickableOptGroups: true,
//                    dropUp: false,
//                    numberDisplayed: 1,
//                    enableCaseInsensitiveFiltering: true
//                });
//            });
//        };

//        // Get filter values
//        var getFilterValue = function () {
//            const yearVal = $(settings.salaryYear).val();
//            var filterData = {
//                CompanyCodes: toArray($(settings.companyIds).val()),
//                BranchCodes: toArray($(settings.branchIds).val()),
//                DepartmentCodes: toArray($(settings.departmentIds).val()),
//                DesignationCodes: toArray($(settings.designationIds).val()),
//                EmployeeIDs: toArray($(settings.employeeIds).val()),
//                MonthIDs: toArray($(settings.monthIds).val()),
//                DeductionTypeIDs: toArray($(settings.typeIds).val()),
//                SalaryYear: yearVal || null
//            };
//            return filterData;
//        };

//        // Array helper
//        var toArray = function (value) {
//            if (!value) return [];
//            if (Array.isArray(value)) return value;
//            return [value];
//        };

//        // Auto-select current month after months are loaded
//        //var autoSelectCurrentMonth = function () {
//        //    if (window.currentMonthToSelect) {
//        //        var monthSelect = $(settings.monthIds);

//        //        // Check if the current month option exists
//        //        if (monthSelect.find(`option[value="${window.currentMonthToSelect}"]`).length > 0) {
//        //            // Select the current month
//        //            monthSelect.val(window.currentMonthToSelect);
//        //            monthSelect.multiselect('refresh');

//        //            // Clear the stored value to prevent repeated selections
//        //            window.currentMonthToSelect = null;

//        //            console.log('Auto-selected current month:', window.currentMonthToSelect || 'Current month');
//        //        }
//        //    }
//        //};

//        var clearAndRebuildDropdown = function (selector, items, includeCode = false) {
//            var dropdown = $(selector);
//            //dropdown.empty();

//            var selectedValues = dropdown.val();
//            var selectedArray = [];

//            if (selectedValues) {
//                if (Array.isArray(selectedValues)) {
//                    selectedArray = selectedValues;
//                } else {
//                    selectedArray = [selectedValues];
//                }
//            }

//            dropdown.empty();

//            if (items && items.length > 0) {
//                $.each(items, function (index, item) {
//                    if (item.code != null && item.name != null) {
//                        var text = includeCode ? `${item.name} (${item.code})` : item.name;
//                        dropdown.append(`<option value="${item.code}">${text}</option>`);
//                    }
//                });
//            }

//            dropdown.multiselect('rebuild');

//            var validSelectedValues = selectedArray.filter(function (value) {
//                return dropdown.find(`option[value="${value}"]`).length > 0;
//            })

//            if (validSelectedValues.length > 0) {
//                dropdown.val(validSelectedValues);
//            }

//            dropdown.multiselect('rebuild');
//        };
         
//        var autoSelectCurrentMonth = function () {
//            if (window.currentMonthToSelect && isInitialLoad) {
//                //debugger;
//                var monthSelect = $(settings.monthIds);

//                // Check if the current month option exists
//                if (monthSelect.find(`option[value="${window.currentMonthToSelect}"]`).length > 0) {
//                    // Select the current month
//                    monthSelect.val(window.currentMonthToSelect);
//                    monthSelect.multiselect('refresh');

//                    // Clear the stored value to prevent repeated selections
//                    window.currentMonthToSelect = null;

//                    console.log('Auto-selected current month:', window.currentMonthToSelect || 'Current month');

//                    setTimeout(function () {
//                        loadFilterEmp();
//                    }, 100);
//                }
//            }
//        };

//        // Load filter data
//        var loadFilterEmp = function () {
//            showLoading();
//            var filterData = getFilterValue();

//            $.ajax({
//                url: filterUrl,
//                type: "POST",
//                contentType: "application/json",
//                data: JSON.stringify(filterData),
//                success: function (res) {
//                    console.log(res);
//                    hideLoading();
//                    if (!res.isSuccess) {
//                        showToast('error', res.message);
//                        return;
//                    }

//                    // Clear previous event handlers
//                    $(settings.companyIds, settings.branchIds, settings.departmentIds,
//                        settings.designationIds, settings.employeeIds, settings.monthIds,
//                        settings.typeIds).off('change');

//                    const data = res.data;

//                    clearAndRebuildDropdown(settings.companyIds, data.companies);
//                    clearAndRebuildDropdown(settings.branchIds, data.branches);
//                    clearAndRebuildDropdown(settings.departmentIds, data.departments);
//                    clearAndRebuildDropdown(settings.designationIds, data.designations);
//                    clearAndRebuildDropdown(settings.employeeIds, data.employees, true);
//                    clearAndRebuildDropdown(settings.typeIds, data.deductionTypes);

//                    // Only rebuild months on initial load
//                    if (isInitialLoad) {
//                        clearAndRebuildDropdown(settings.monthIds, data.months);

//                        // Auto-select current month after months are populated
//                        setTimeout(function () {
//                            autoSelectCurrentMonth();
//                            isInitialLoad = false; // Mark initial load as complete
//                        }, 100);
//                    }
//                },
//                error: function (e) {
//                    console.log(e);
//                    hideLoading();
//                    showToast('error', 'Failed to load data');
//                }
//            });
//        };

//        //// Helper function to populate dropdowns
//        //var populateDropdown = function (items, selector, includeCode = false) {
//        //    if (items && items.length > 0 && items.some(x => x.code != null && x.name != null)) {
//        //        var dropdown = $(selector);
//        //        $.each(items, function (index, item) {
//        //            if (item.code != null && item.name != null &&
//        //                dropdown.find(`option[value="${item.code}"]`).length === 0) {
//        //                var text = includeCode ? `${item.name} (${item.code})` : item.name;
//        //                dropdown.append(`<option value="${item.code}">${text}</option>`);
//        //            }
//        //        });
//        //        dropdown.multiselect('rebuild');
//        //    }
//        //};

//        // Toggle preview - open if closed, close if open
//        var togglePreview = function () {
//            // Check if preview container is currently visible
//            if ($("#salaryDeductionReport-container").is(':visible')) {
//                // If visible, close the preview
//                hidePreview();
//            } else {
//                // If hidden, open the preview
//                showPreview();
//            }
//        };

//        // Show PDF preview in iframe
//        var showPreview = function () {
//            var filterData = getFilterValue();

//            var hasFilters = Object.values(filterData).some(value =>
//                (Array.isArray(value) && value.length > 0) ||
//                (typeof value === 'string' && value.trim() !== '')
//            );

//            if (!hasFilters) {
//                showToast('warning', 'No deduction is found to generate report');
//                return;
//            }

//            var previewRequest = {
//                FilterData: filterData,
//                ExportFormat: 'pdf'
//            };

//            // Show the main container and preview elements
//            $("#salaryDeductionReport-container").show();
//            $("#previewLoadingIndicator").show();
//            $("#previewIframe").hide();

//            $.ajax({
//                url: previewUrl,
//                type: "POST",
//                contentType: "application/json",
//                data: JSON.stringify(previewRequest),
//                xhrFields: {
//                    responseType: 'blob'
//                },
//                success: function (data) {
//                    $("#previewLoadingIndicator").hide();
                    
//                    // Create blob URL for PDF
//                    var blob = new Blob([data], { type: 'application/pdf' });
//                    var pdfUrl = window.URL.createObjectURL(blob);

//                    // Set iframe source to PDF and show it
//                    $("#previewIframe").attr('src', pdfUrl).show();

//                    // Clean up previous blob URL if exists
//                    if (window.currentPdfUrl) {
//                        window.URL.revokeObjectURL(window.currentPdfUrl);
//                    }
//                    window.currentPdfUrl = pdfUrl;
//                },
//                error: function (xhr, status, error) {
//                    $("#previewLoadingIndicator").hide();
//                    console.error('Preview error:', error);

//                    // Try to parse error message from response
//                    try {
//                        var reader = new FileReader();
//                        reader.onload = function () {
//                            try {
//                                var errorResponse = JSON.parse(reader.result);
//                                showToast('error', errorResponse.message || 'Preview failed');
//                            } catch (e) {
//                                showToast('error', 'Preview failed. Please try again.');
//                            }
//                        };
//                        reader.readAsText(xhr.responseText);
//                    } catch (e) {
//                        showToast('error', 'Preview failed. Please try again.');
//                    }

//                    // Hide container on error
//                    $("#salaryDeductionReport-container").hide();
//                }
//            });
//        };

//        // Hide preview
//        var hidePreview = function () {
//            // Hide the main container
//            $("#salaryDeductionReport-container").hide();
//            $("#previewIframe").attr('src', '');

//            // Clean up blob URL
//            if (window.currentPdfUrl) {
//                window.URL.revokeObjectURL(window.currentPdfUrl);
//                window.currentPdfUrl = null;
//            }
//        };

//        var exportReport = function (format) {
//            showLoading();
//            var filterData = getFilterValue();

//            var exportRequest = {
//                FilterData: filterData,
//                ExportFormat: format
//            };

//            $.ajax({
//                url: exportUrl,
//                type: "POST",
//                contentType: "application/json",
//                data: JSON.stringify(exportRequest),
//                xhrFields: {
//                    responseType: 'blob'
//                },
//                success: function (data, status, xhr) {
//                    hideLoading();
                    
//                    if (format === "download") {
//                        var blob = new Blob([data], { type: 'application/pdf' }); // Force correct MIME type
//                        var blobUrl = window.URL.createObjectURL(blob);

//                        var iframe = document.createElement('iframe');
//                        iframe.style.display = 'none';
//                        iframe.src = blobUrl;
//                        document.body.appendChild(iframe);

//                        iframe.onload = function () {
//                            iframe.contentWindow.focus();
//                            iframe.contentWindow.print();
//                        };
//                        return;
//                    }
//                    // Generate default filename if extraction failed
//                    var timestamp = new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '');
//                    var extension = format === 'pdf' ? 'pdf' : format === 'excel' ? 'xlsx' : 'docx';
//                    var filename = `SalaryDeductionReport_${timestamp}.${extension}`;

//                    // Create download link
//                    var blob = new Blob([data], { type: xhr.getResponseHeader('Content-Type') });
//                    var downloadUrl = window.URL.createObjectURL(blob);
//                    var downloadLink = document.createElement("a");
//                    downloadLink.href = downloadUrl;
//                    downloadLink.download = filename;
//                    document.body.appendChild(downloadLink);
//                    downloadLink.click();
//                    document.body.removeChild(downloadLink);
//                    window.URL.revokeObjectURL(downloadUrl);

//                    showToast('success', 'Report exported successfully');
//                },
//                error: function (xhr, status, error) {
//                    hideLoading();
//                    console.error('Export error:', error);

//                    // Try to parse error message from response
//                    try {
//                        var reader = new FileReader();
//                        reader.onload = function () {
//                            try {
//                                var errorResponse = JSON.parse(reader.result);
//                                showToast('error', errorResponse.message || 'Export failed');
//                            } catch (e) {
//                                showToast('error', 'Export failed. Please try again.');
//                            }
//                        };
//                        reader.readAsText(xhr.responseText);
//                    } catch (e) {
//                        showToast('error', 'Export failed. Please try again.');
//                    }
//                }
//            });
//        };

//        // Button Click Event Binding
//        $("#checkCompanyBtn").on("click", function () {
//            console.log("Company Value:", getFilterValue());
//        });

//        // Preview button functionality - toggle open/close
//        $("#previewBtn").on("click", function () {
//            togglePreview();
//        });

//        $("#downloadBtn").on("click", function () {
//            var selectedFormat = $("#exportFormatSelect").val();
//            exportReport(selectedFormat);
//        });

//        // Filter change event binding
//        function bindFilterChangeEvent() {
//            // Remove previous event handlers to prevent multiple bindings
//            $(document).off('change.salaryReport');

//            // Company change - clear all dependent dropdowns
//            $(document).on('change.salaryReport', settings.companyIds, function () {
//                clearDependentDropdowns([settings.branchIds, settings.departmentIds, settings.designationIds, settings.employeeIds]);
//                loadFilterEmp();
//            });

//            // Branch change - clear department, designation, employee
//            $(document).on('change.salaryReport', settings.branchIds, function () {
//                clearDependentDropdowns([settings.departmentIds, settings.designationIds, settings.employeeIds]);
//                loadFilterEmp();
//            });

//            // Department change - clear designation, employee
//            $(document).on('change.salaryReport', settings.departmentIds, function () {
//                clearDependentDropdowns([settings.designationIds, settings.employeeIds]);
//                loadFilterEmp();
//            });

//            // Designation change - clear employee
//            $(document).on('change.salaryReport', settings.designationIds, function () {
//                clearDependentDropdowns([settings.employeeIds]);
//                loadFilterEmp();
//            });

//            // Month or Type change - reload to filter employees
//            $(document).on('change.salaryReport', settings.monthIds + ',' + settings.typeIds, function () {
//                clearDependentDropdowns([settings.employeeIds]);
//                loadFilterEmp();
//            });

//            // Employee change - just reload (no cascading needed)
//            $(document).on('change.salaryReport', settings.employeeIds, function () {
//                loadFilterEmp();
//            });

//            // Year change with debounce
//            $(document).off('input.salaryReport', settings.salaryYear);
//            $(document).on('input.salaryReport', settings.salaryYear, function () {
//                clearTimeout(window.yearInputTimeout);
//                window.yearInputTimeout = setTimeout(function () {
//                    // Clear all dropdowns except months when year changes
//                    clearDependentDropdowns([settings.companyIds, settings.branchIds, settings.departmentIds, settings.designationIds, settings.employeeIds, settings.typeIds]);
//                    loadFilterEmp();
//                }, 500);
//            });
//        }

//        function clearDependentDropdowns(selectors) {
//            selectors.forEach(function (selector) {
//                var dropdown = $(selector);
//                dropdown.empty();
//                dropdown.multiselect('rebuild');
//            });
//        }

//        // Toast notification helper (assumes you have a toast function)
//        function showToast(type, message) {
//            if (typeof toastr !== 'undefined') {
//                toastr[type](message);
//            } else if (typeof Swal !== 'undefined') {
//                Swal.fire({
//                    icon: type === 'error' ? 'error' : type === 'warning' ? 'warning' : 'success',
//                    title: type === 'error' ? 'Error' : type === 'warning' ? 'Warning' : 'Success',
//                    text: message,
//                    timer: 3000,
//                    showConfirmButton: false
//                });
//            } else {
//                alert(message);
//            }
//        }

//        // Initialize
//        var init = function () {
//            settings.load();
//            setCurrentMonthAndYear(); // Set current month and year first
//            initializeMultiselects();
//            setupLoadingOverlay();
//            setupPreviewContainer(); // Setup preview in container
//            bindFilterChangeEvent();
//            loadFilterEmp();
//        };

//        init();
//    };
//})(jQuery);







//(function ($) {
//    $.salaryDeductionReport = function (options) {
//        var settings = $.extend({
//            baseUrl: "/",
//            companyIds: "#companySelect",
//            branchIds: "#branchSelect",
//            departmentIds: "#departmentSelect",
//            designationIds: "#designationSelect",
//            employeeIds: "#employeeSelect",
//            typeIds: "#typeSelect",
//            monthIds: "#monthSelect",
//            salaryYear: "#yearSelect",
//            load: function () {
//                console.log("Loading...");
//            }
//        }, options);

//        var filterUrl = settings.baseUrl + "/getAllFilterEmp";
//        var exportUrl = settings.baseUrl + "/ExportReport";
//        var reportDataTable = null;
//        var isPreviewVisible = false;
//        var isInitialLoad = true;
//        var isLoadingFilters = false;
//        var multiselectInitialized = new Set(); // Track initialized multiselects

//        // Setup loading overlay
//        var setupLoadingOverlay = function () {
//            if ($("#customLoadingOverlay").length === 0) {
//                $("body").append(`
//                    <div id="customLoadingOverlay" style="
//                        display: none;
//                        position: fixed;
//                        top: 0;
//                        left: 0;
//                        width: 100%;
//                        height: 100%;
//                        background-color: rgba(0, 0, 0, 0.5);
//                        z-index: 9999;
//                        justify-content: center;
//                        align-items: center;">
//                        <div style="
//                            background-color: white;
//                            padding: 20px;
//                            border-radius: 5px;
//                            box-shadow: 0 0 10px rgba(0,0,0,0.3);
//                            text-align: center;">
//                            <div class="spinner-border text-primary" role="status">
//                                <span class="sr-only">Loading...</span>
//                            </div>
//                            <p style="margin-top: 10px; margin-bottom: 0;">Loading data...</p>
//                        </div>
//                    </div>
//                `);
//            }
//        };

//        function showLoading() {
//            $("#customLoadingOverlay").css("display", "flex");
//        }

//        function hideLoading() {
//            $("#customLoadingOverlay").hide();
//        }

//        // Set current month and year
//        var setCurrentMonthAndYear = function () {
//            var currentDate = new Date();
//            var currentMonth = currentDate.getMonth() + 1;
//            var currentYear = currentDate.getFullYear();
//            $(settings.salaryYear).val(currentYear.toString());
//            window.currentMonthToSelect = currentMonth.toString();
//        };

//        // Safe multiselect operations
//        var safeMultiselectOperation = function (selector, operation, params) {
//            var $element = $(selector);
//            if (!$element.length) {
//                console.warn('Element not found:', selector);
//                return false;
//            }

//            try {
//                if (operation === 'init') {
//                    if (!multiselectInitialized.has(selector)) {
//                        $element.multiselect(params);
//                        multiselectInitialized.add(selector);
//                        return true;
//                    }
//                } else if (operation === 'refresh') {
//                    if (multiselectInitialized.has(selector)) {
//                        // Check if multiselect is still attached
//                        if ($element.data('multiselect')) {
//                            $element.multiselect('refresh');
//                            return true;
//                        } else {
//                            // Reinitialize if needed
//                            multiselectInitialized.delete(selector);
//                            return safeMultiselectOperation(selector, 'init', getMultiselectConfig());
//                        }
//                    }
//                } else if (operation === 'destroy') {
//                    if (multiselectInitialized.has(selector) && $element.data('multiselect')) {
//                        $element.multiselect('destroy');
//                        multiselectInitialized.delete(selector);
//                        return true;
//                    }
//                }
//            } catch (error) {
//                console.warn(`Multiselect ${operation} failed for ${selector}:`, error);
//                if (operation === 'refresh') {
//                    // Try to reinitialize on refresh failure
//                    multiselectInitialized.delete(selector);
//                    return safeMultiselectOperation(selector, 'init', getMultiselectConfig());
//                }
//                return false;
//            }
//            return false;
//        };

//        // Get multiselect configuration
//        var getMultiselectConfig = function () {
//            return {
//                enableFiltering: true,
//                includeSelectAllOption: true,
//                selectAllText: 'Select All',
//                nonSelectedText: 'Select Items',
//                nSelectedText: 'Selected',
//                allSelectedText: 'All Selected',
//                filterPlaceholder: 'Search.......',
//                buttonWidth: '100%',
//                maxHeight: 350,
//                enableClickableOptGroups: true,
//                dropUp: false,
//                numberDisplayed: 1,
//                enableCaseInsensitiveFiltering: true
//            };
//        };

//        // Initialize multiselects with better error handling
//        var initializeMultiselects = function () {
//            var selectors = [
//                settings.companyIds,
//                settings.branchIds,
//                settings.departmentIds,
//                settings.designationIds,
//                settings.employeeIds,
//                settings.typeIds,
//                settings.monthIds
//            ];

//            selectors.forEach(function (selector) {
//                // Wait for DOM to be ready
//                setTimeout(function () {
//                    safeMultiselectOperation(selector, 'init', getMultiselectConfig());
//                }, 50);
//            });
//        };

//        // Get filter values
//        var getFilterValue = function () {
//            const yearVal = $(settings.salaryYear).val();
//            var filterData = {
//                CompanyCodes: toArray($(settings.companyIds).val()),
//                BranchCodes: toArray($(settings.branchIds).val()),
//                DepartmentCodes: toArray($(settings.departmentIds).val()),
//                DesignationCodes: toArray($(settings.designationIds).val()),
//                EmployeeIDs: toArray($(settings.employeeIds).val()),
//                MonthIDs: toArray($(settings.monthIds).val()),
//                DeductionTypeIDs: toArray($(settings.typeIds).val()),
//                SalaryYear: yearVal || null
//            };
//            console.log("Filter Data:", filterData);
//            return filterData;
//        };

//        // Array helper
//        var toArray = function (value) {
//            if (!value) return [];
//            if (Array.isArray(value)) return value;
//            return [value];
//        };

//        // Clear dependent dropdowns with better error handling
//        var clearDependentDropdowns = function (level) {
//            var dropdownsToClear = [];

//            switch (level) {
//                case 'company':
//                    dropdownsToClear = [settings.branchIds, settings.departmentIds, settings.designationIds, settings.employeeIds];
//                    break;
//                case 'branch':
//                    dropdownsToClear = [settings.departmentIds, settings.designationIds, settings.employeeIds];
//                    break;
//                case 'department':
//                    dropdownsToClear = [settings.designationIds, settings.employeeIds];
//                    break;
//                case 'designation':
//                    dropdownsToClear = [settings.employeeIds];
//                    break;
//            }

//            dropdownsToClear.forEach(function (selector) {
//                var $element = $(selector);
//                if ($element.length) {
//                    // Clear selected values first
//                    $element.val([]);

//                    // Clear options except the first one
//                    $element.find('option:not(:first)').remove();

//                    // Refresh multiselect safely
//                    setTimeout(function () {
//                        safeMultiselectOperation(selector, 'refresh');
//                    }, 100);
//                }
//            });
//        };

//        // Setup cascading change events
//        var setupCascadingEvents = function () {
//            // Remove existing event handlers to prevent duplicates
//            $(settings.companyIds).off('change.cascade');
//            $(settings.branchIds).off('change.cascade');
//            $(settings.departmentIds).off('change.cascade');
//            $(settings.designationIds).off('change.cascade');

//            // Company change
//            $(settings.companyIds).on('change.cascade', function () {
//                if (isLoadingFilters) return;

//                console.log('Company changed');
//                clearDependentDropdowns('company');

//                clearTimeout(window.companyChangeTimeout);
//                window.companyChangeTimeout = setTimeout(function () {
//                    loadFilterDropdowns('company');
//                }, 300);
//            });

//            // Branch change
//            $(settings.branchIds).on('change.cascade', function () {
//                if (isLoadingFilters) return;

//                console.log('Branch changed');
//                clearDependentDropdowns('branch');

//                clearTimeout(window.branchChangeTimeout);
//                window.branchChangeTimeout = setTimeout(function () {
//                    loadFilterDropdowns('branch');
//                }, 300);
//            });

//            // Department change
//            $(settings.departmentIds).on('change.cascade', function () {
//                if (isLoadingFilters) return;

//                console.log('Department changed');
//                clearDependentDropdowns('department');

//                clearTimeout(window.departmentChangeTimeout);
//                window.departmentChangeTimeout = setTimeout(function () {
//                    loadFilterDropdowns('department');
//                }, 300);
//            });

//            // Designation change
//            $(settings.designationIds).on('change.cascade', function () {
//                if (isLoadingFilters) return;

//                console.log('Designation changed');
//                clearDependentDropdowns('designation');

//                clearTimeout(window.designationChangeTimeout);
//                window.designationChangeTimeout = setTimeout(function () {
//                    loadFilterDropdowns('designation');
//                }, 300);
//            });
//        };

//        // Helper function to populate dropdowns with improved error handling
//        var populateDropdown = function (items, selector, includeCode = false) {
//            if (!items || items.length === 0) return;

//            var $dropdown = $(selector);
//            if (!$dropdown.length) return;

//            var currentValues = toArray($dropdown.val());

//            // Clear existing options except the first "Select..." option
//            $dropdown.find('option:not(:first)').remove();

//            // Add new options
//            $.each(items, function (index, item) {
//                if (item.code != null && item.name != null) {
//                    var text = includeCode ? `${item.name} (${item.code})` : item.name;
//                    $dropdown.append(`<option value="${item.code}">${text}</option>`);
//                }
//            });

//            // Restore selected values if they still exist in the new options
//            if (currentValues.length > 0) {
//                var validValues = currentValues.filter(function (val) {
//                    return $dropdown.find(`option[value="${val}"]`).length > 0;
//                });
//                if (validValues.length > 0) {
//                    $dropdown.val(validValues);
//                }
//            }

//            // Refresh multiselect safely with delay
//            setTimeout(function () {
//                safeMultiselectOperation(selector, 'refresh');
//            }, 100);
//        };

//        // Auto-select current month with better error handling
//        var autoSelectCurrentMonth = function () {
//            if (window.currentMonthToSelect && isInitialLoad) {
//                setTimeout(function () {
//                    var monthSelect = $(settings.monthIds);
//                    if (monthSelect.find(`option[value="${window.currentMonthToSelect}"]`).length > 0) {
//                        monthSelect.val(window.currentMonthToSelect);
//                        safeMultiselectOperation(settings.monthIds, 'refresh');
//                        console.log('Auto-selected current month:', window.currentMonthToSelect);
//                        window.currentMonthToSelect = null;
//                        isInitialLoad = false;
//                    }
//                }, 200);
//            }
//        };

//        // Load filter data for dropdowns
//        var loadFilterDropdowns = function (triggerLevel = null) {
//            if (isLoadingFilters) {
//                console.log('Filter loading already in progress, skipping...');
//                return;
//            }

//            isLoadingFilters = true;
//            showLoading();

//            var filterData = getFilterValue();
//            console.log('Loading filters triggered by:', triggerLevel, 'with data:', filterData);

//            $.ajax({
//                url: filterUrl,
//                type: "POST",
//                contentType: "application/json",
//                data: JSON.stringify(filterData),
//                success: function (res) {
//                    console.log("Filter Response:", res);
//                    hideLoading();
//                    isLoadingFilters = false;

//                    if (!res.isSuccess) {
//                        showToast('error', res.message || 'Failed to load filter data');
//                        return;
//                    }

//                    const data = res.data;

//                    // Populate dropdowns based on trigger level and available data
//                    if (!triggerLevel || triggerLevel === 'initial') {
//                        // Initial load - populate all dropdowns
//                        if (data.companies) {
//                            console.log("Populating companies");
//                            populateDropdown(data.companies, settings.companyIds);
//                        }
//                        if (data.deductionTypes) {
//                            console.log("Populating deduction types");
//                            populateDropdown(data.deductionTypes, settings.typeIds);
//                        }
//                        if (data.months) {
//                            console.log("Populating months");
//                            populateDropdown(data.months, settings.monthIds);
//                            // Auto-select current month only on initial load
//                            autoSelectCurrentMonth();
//                        }
//                    }

//                    // Populate dependent dropdowns based on trigger level
//                    if (!triggerLevel || triggerLevel === 'initial' || triggerLevel === 'company') {
//                        if (data.branches) {
//                            console.log("Populating branches");
//                            populateDropdown(data.branches, settings.branchIds);
//                        }
//                    }

//                    if (!triggerLevel || triggerLevel === 'initial' || ['company', 'branch'].includes(triggerLevel)) {
//                        if (data.departments) {
//                            console.log("Populating departments");
//                            populateDropdown(data.departments, settings.departmentIds);
//                        }
//                    }

//                    if (!triggerLevel || triggerLevel === 'initial' || ['company', 'branch', 'department'].includes(triggerLevel)) {
//                        if (data.designations) {
//                            console.log("Populating designations");
//                            populateDropdown(data.designations, settings.designationIds);
//                        }
//                    }

//                    if (!triggerLevel || triggerLevel === 'initial' || ['company', 'branch', 'department', 'designation'].includes(triggerLevel)) {
//                        if (data.employees) {
//                            console.log("Populating employees");
//                            populateDropdown(data.employees, settings.employeeIds, true);
//                        }
//                    }

//                    // Setup cascading events after populating dropdowns (only on initial load)
//                    if (!triggerLevel || triggerLevel === 'initial') {
//                        setTimeout(function () {
//                            setupCascadingEvents();
//                        }, 300);
//                    }
//                },
//                error: function (xhr, status, error) {
//                    console.error('Filter loading error:', error);
//                    hideLoading();
//                    isLoadingFilters = false;
//                    showToast('error', 'Failed to load filter data');
//                }
//            });
//        };

//        // Get report period based on selected months and year
//        var getReportPeriod = function () {
//            var selectedMonthIds = toArray($(settings.monthIds).val());
//            var selectedYear = $(settings.salaryYear).val();

//            if (!selectedMonthIds || selectedMonthIds.length === 0) {
//                return selectedYear ? `Year ${selectedYear}` : 'All Periods';
//            }

//            var monthNames = [];
//            selectedMonthIds.forEach(function (monthId) {
//                var monthOption = $(settings.monthIds).find(`option[value="${monthId}"]`);
//                if (monthOption.length > 0) {
//                    monthNames.push(monthOption.text());
//                }
//            });

//            if (monthNames.length > 0 && selectedYear) {
//                if (monthNames.length === 1) {
//                    return `${monthNames[0]} ${selectedYear}`;
//                } else {
//                    return `${monthNames.join(", ")} ${selectedYear}`;
//                }
//            } else if (monthNames.length > 0) {
//                return monthNames.join(", ");
//            } else if (selectedYear) {
//                return `Year ${selectedYear}`;
//            }

//            return 'All Periods';
//        };

//        // Group data by department
//        var groupByDepartment = function (data) {
//            var grouped = {};
//            data.forEach(function (item) {
//                var deptKey = item.departmentName || 'Unknown Department';
//                if (!grouped[deptKey]) {
//                    grouped[deptKey] = [];
//                }
//                grouped[deptKey].push(item);
//            });
//            return grouped;
//        };

//        // Calculate department totals
//        var calculateDepartmentTotal = function (departmentData) {
//            return departmentData.reduce(function (sum, item) {
//                return sum + (parseFloat(item.deductionAmount) || 0);
//            }, 0);
//        };

//        // Load filter data with table update (for preview)
//        var loadFilterEmp = function () {
//            showLoading();
//            var filterData = getFilterValue();

//            $.ajax({
//                url: filterUrl,
//                type: "POST",
//                contentType: "application/json",
//                data: JSON.stringify(filterData),
//                success: function (res) {
//                    console.log("Preview Response:", res);
//                    hideLoading();

//                    if (!res.isSuccess) {
//                        showToast('error', res.message || 'Failed to load report data');
//                        return;
//                    }

//                    // Update table data
//                    loadTableData(res);
//                },
//                error: function (xhr, status, error) {
//                    console.error('Preview loading error:', error);
//                    hideLoading();
//                    showToast('error', 'Failed to load report data');
//                }
//            });
//        };

//        // Show/hide preview functions
//        var showPreview = function () {
//            $("#salaryDeductionReport-container, #RosterScheduleReport-container").show();
//        };

//        var hidePreview = function () {
//            $("#salaryDeductionReport-container, #RosterScheduleReport-container").hide();
//        };

//        // Load table data with department grouping
//        var loadTableData = function (res) {
//            var tableData = res.data.salaryDeduction;
//            console.log("Table Data:", tableData);

//            if (reportDataTable !== null) {
//                reportDataTable.destroy();
//                reportDataTable = null;
//            }

//            var tableContainer = $("#RosterScheduleReport-container");
//            var gridContainer = $("#salaryDeductionReport-container");

//            if (tableContainer.length === 0) {
//                console.error("Table container #RosterScheduleReport-container not found");
//                return;
//            }

//            tableContainer.empty();

//            if (!tableData || tableData.length === 0) {
//                tableContainer.append('<div class="alert alert-info">No data found for the selected criteria.</div>');
//                showPreview();
//                return;
//            }

//            var reportPeriod = getReportPeriod();

//            // Add title section
//            var titleSection = $(`
//                <div class="report-header text-center mb-4">
//                    <h4 class="mb-0" style="font-weight: bold; font-size: 20px;">Data Path</h4>
//                    <h3 class="mb-0" style="font-weight: bold; font-size: 16px;">Salary Deduction Report</h3>
//                    <h4 class="mb-4" style="font-size: 14px;">For the month of ${reportPeriod}</h4>
//                </div>
//            `);

//            tableContainer.append(titleSection);

//            var groupedData = groupByDepartment(tableData);
//            var grandTotal = 0;
//            var totalEmployees = 0;
//            var totalDeductions = 0;

//            // Process each department
//            Object.keys(groupedData).sort().forEach(function (departmentName) {
//                var departmentData = groupedData[departmentName];
//                var departmentTotal = calculateDepartmentTotal(departmentData);
//                grandTotal += departmentTotal;
//                totalDeductions += departmentData.length;

//                // Track unique employees
//                var uniqueEmployees = new Set();
//                departmentData.forEach(function (item) {
//                    if (item.code) {
//                        uniqueEmployees.add(item.code);
//                    }
//                });
//                totalEmployees += uniqueEmployees.size;

//                var departmentSerialNumber = 1;

//                var departmentHeader = $(`
//                    <div class="department-section mb-4">
//                        <h5 class="department-title mb-3" style="font-weight: bold; font-size: 12px; margin-top: 15px; margin-bottom: 10px;">
//                            Department: ${departmentName}
//                        </h5>
//                        <div class="table-responsive">
//                            <table class="table table-bordered department-table" style="width: 100%; margin-bottom: 5px;">
//                                <thead>
//                                    <tr>
//                                        <th style="width: 4%; text-align: center; border: 1px solid #000; font-weight: bold;">SL</th>
//                                        <th style="width: 7%; text-align: center; border: 1px solid #000; font-weight: bold;">Employee ID</th>
//                                        <th style="width: 12%; text-align: center; border: 1px solid #000; font-weight: bold;">Name</th>
//                                        <th style="width: 15%; text-align: center; border: 1px solid #000; font-weight: bold;">Designation</th>
//                                        <th style="width: 10%; text-align: center; border: 1px solid #000; font-weight: bold;">Branch</th>
//                                        <th style="width: 10%; text-align: center; border: 1px solid #000; font-weight: bold;">Deduction Type</th>
//                                        <th style="width: 7%; text-align: center; border: 1px solid #000; font-weight: bold;">Amount</th>
//                                        <th style="width: 7%; text-align: center; border: 1px solid #000; font-weight: bold;">Month</th>
//                                        <th style="width: 7%; text-align: center; border: 1px solid #000; font-weight: bold;">Year</th>
//                                        <th style="width: 12%; text-align: center; border: 1px solid #000; font-weight: bold;">Remarks</th>
//                                    </tr>
//                                </thead>
//                                <tbody class="department-tbody">
//                                </tbody>
//                            </table>
//                        </div>
//                        <div class="department-total text-right mb-3" style="font-weight: bold; margin-top: 5px; margin-bottom: 10px;">
//                            Total: ${departmentTotal.toFixed(2)}
//                        </div>
//                    </div>
//                `);

//                var tbody = departmentHeader.find('.department-tbody');

//                // Sort and add rows
//                var orderedItems = departmentData.sort(function (a, b) {
//                    return (a.code || '').localeCompare(b.code || '');
//                });

//                orderedItems.forEach(function (salaryDeduction) {
//                    var amount = parseFloat(salaryDeduction.deductionAmount) || 0;

//                    var row = $(`
//                        <tr>
//                            <td style="text-align: center; border: 1px solid #000;">${departmentSerialNumber}</td>
//                            <td style="text-align: center; border: 1px solid #000;">${salaryDeduction.code || ''}</td>
//                            <td style="text-align: left; border: 1px solid #000;">${salaryDeduction.name || ''}</td>
//                            <td style="text-align: left; border: 1px solid #000;">${salaryDeduction.designationName || ''}</td>
//                            <td style="text-align: center; border: 1px solid #000;">${salaryDeduction.branchName || ''}</td>
//                            <td style="text-align: center; border: 1px solid #000;">${salaryDeduction.deductionType || ''}</td>
//                            <td style="text-align: right; border: 1px solid #000;">${amount.toFixed(2)}</td>
//                            <td style="text-align: center; border: 1px solid #000;">${salaryDeduction.month || ''}</td>
//                            <td style="text-align: center; border: 1px solid #000;">${salaryDeduction.year || ''}</td>
//                            <td style="text-align: center; border: 1px solid #000;">${salaryDeduction.remarks || ''}</td>
//                        </tr>
//                    `);
//                    tbody.append(row);
//                    departmentSerialNumber++;
//                });

//                tableContainer.append(departmentHeader);
//            });

//            // Add summary section
//            var summarySection = $(`
//                <div class="summary-section mt-4">
//                    <h5 style="font-weight: bold; font-size: 12px; margin-top: 20px; margin-bottom: 10px;">Summary</h5>
//                    <div class="row" style="margin-bottom: 0;">
//                        <div class="col-md-4" style="text-align: left;">
//                            <span>Total Employee: ${totalEmployees}</span>
//                        </div>
//                        <div class="col-md-4" style="text-align: center;">
//                            <span>Total Deduction: ${totalDeductions}</span>
//                        </div>
//                        <div class="col-md-4" style="text-align: right;">
//                            <span>Grand Total: ${grandTotal.toFixed(2)}</span>
//                        </div>
//                    </div>
//                </div>
//            `);

//            tableContainer.append(summarySection);
//            showPreview();
//        };

//        // Export report function
//        var exportReport = function (format) {
//            showLoading();
//            var filterData = getFilterValue();

//            var exportRequest = {
//                FilterData: filterData,
//                ExportFormat: format
//            };

//            $.ajax({
//                url: exportUrl,
//                type: "POST",
//                contentType: "application/json",
//                data: JSON.stringify(exportRequest),
//                xhrFields: {
//                    responseType: 'blob'
//                },
//                success: function (data, status, xhr) {
//                    hideLoading();

//                    var timestamp = new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '');
//                    var extension = format === 'pdf' ? 'pdf' : format === 'excel' ? 'xlsx' : 'docx';
//                    var filename = `SalaryDeductionReport_${timestamp}.${extension}`;

//                    var blob = new Blob([data]);
//                    var downloadUrl = window.URL.createObjectURL(blob);
//                    var downloadLink = document.createElement("a");
//                    downloadLink.href = downloadUrl;
//                    downloadLink.download = filename;
//                    document.body.appendChild(downloadLink);
//                    downloadLink.click();
//                    document.body.removeChild(downloadLink);
//                    window.URL.revokeObjectURL(downloadUrl);

//                    showToast('success', 'Report exported successfully');
//                },
//                error: function (xhr, status, error) {
//                    hideLoading();
//                    console.error('Export error:', error);
//                    showToast('error', 'Export failed. Please try again.');
//                }
//            });
//        };

//        // Toast notification helper
//        function showToast(type, message) {
//            if (typeof toastr !== 'undefined') {
//                toastr[type](message);
//            } else if (typeof Swal !== 'undefined') {
//                Swal.fire({
//                    icon: type === 'error' ? 'error' : 'success',
//                    title: type === 'error' ? 'Error' : 'Success',
//                    text: message,
//                    timer: 3000,
//                    showConfirmButton: false
//                });
//            } else {
//                console.log(`${type.toUpperCase()}: ${message}`);
//                alert(message);
//            }
//        }

//        // Event handlers
//        $("#checkCompanyBtn").on("click", function () {
//            console.log("Current Filter Values:", getFilterValue());
//        });

//        $("#previewBtn").on("click", function () {
//            if (isPreviewVisible) {
//                hidePreview();
//                isPreviewVisible = false;
//                $(this).text('Show Preview');
//            } else {
//                loadFilterEmp();
//                isPreviewVisible = true;
//                $(this).text('Hide Preview');
//            }
//        });

//        $("#downloadBtn").on("click", function () {
//            var selectedFormat = $("#exportFormatSelect").val() || 'pdf';
//            exportReport(selectedFormat);
//        });

//        // Initialize
//        var init = function () {
//            settings.load();
//            setCurrentMonthAndYear();
//            setupLoadingOverlay();
//            hidePreview(); // Hide preview initially

//            // Initialize multiselects first, then load data
//            setTimeout(function () {
//                initializeMultiselects();
//                // Wait a bit more for multiselects to initialize before loading data
//                setTimeout(function () {
//                    loadFilterDropdowns('initial');
//                }, 200);
//            }, 100);
//        };

//        init();

//        // Public methods
//        return {
//            refresh: function () {
//                loadFilterDropdowns('initial');
//            },
//            getFilterValues: function () {
//                return getFilterValue();
//            },
//            showPreview: function () {
//                loadFilterEmp();
//            },
//            reinitializeMultiselects: function () {
//                multiselectInitialized.clear();
//                initializeMultiselects();
//            }
//        };

//    };
//})(jQuery);













//WOrked
//(function ($) {
//    $.salaryDeductionReport = function (options) {
//        var settings = $.extend({
//            baseUrl: "/",
//            companyIds: "#companySelect",
//            branchIds: "#branchSelect",
//            departmentIds: "#departmentSelect",
//            designationIds: "#designationSelect",
//            employeeIds: "#employeeSelect",
//            typeIds: "#typeSelect",
//            monthIds: "#monthSelect",
//            salaryYear: "#yearSelect",
//            load: function () {
//                console.log("Loading...");
//            }
//        }, options);

//        var filterUrl = settings.baseUrl + "/getAllFilterEmp";
//        var exportUrl = settings.baseUrl + "/ExportReport";

//        // Setup loading overlay
//        var setupLoadingOverlay = function () {
//            if ($("#customLoadingOverlay").length === 0) {
//                $("body").append(`
//                    <div id="customLoadingOverlay" style="
//                        display: none;
//                        position: fixed;
//                        top: 0;
//                        left: 0;
//                        width: 100%;
//                        height: 100%;
//                        background-color: rgba(0, 0, 0, 0.5);
//                        z-index: 9999;
//                        justify-content: center;
//                        align-items: center;">
//                        <div style="
//                            background-color: white;
//                            padding: 20px;
//                            border-radius: 5px;
//                            box-shadow: 0 0 10px rgba(0,0,0,0.3);
//                            text-align: center;">
//                            <div class="spinner-border text-primary" role="status">
//                                <span class="sr-only">Loading...</span>
//                            </div>
//                            <p style="margin-top: 10px; margin-bottom: 0;">Loading data...</p>
//                        </div>
//                    </div>
//                `);
//            }
//        };

//        function showLoading() {
//            $("#customLoadingOverlay").css("display", "flex");
//        }

//        function hideLoading() {
//            $("#customLoadingOverlay").hide();
//        }

//        var reportDataTable = null;

//        // Set current month and year
//        var setCurrentMonthAndYear = function () {
//            var currentDate = new Date();
//            var currentMonth = currentDate.getMonth() + 1; // JavaScript months are 0-indexed
//            var currentYear = currentDate.getFullYear();

//            // Set current year in the year textbox
//            $(settings.salaryYear).val(currentYear.toString());

//            // Store current month to select it later when options are loaded
//            window.currentMonthToSelect = currentMonth.toString();
//        };

//        // Initialize multiselects
//        var initializeMultiselects = function () {
//            var selectors = [
//                settings.companyIds,
//                settings.branchIds,
//                settings.departmentIds,
//                settings.designationIds,
//                settings.employeeIds,
//                settings.typeIds,
//                settings.monthIds
//            ].join(", ");

//            $(selectors).multiselect({
//                enableFiltering: true,
//                includeSelectAllOption: true,
//                selectAllText: 'Select All',
//                nonSelectedText: 'Select Items',
//                nSelectedText: 'Selected',
//                allSelectedText: 'All Selected',
//                filterPlaceholder: 'Search.......',
//                buttonWidth: '100%',
//                maxHeight: 350,
//                enableClickableOptGroups: true,
//                dropUp: false,
//                numberDisplayed: 1,
//                enableCaseInsensitiveFiltering: true
//            });
//        };

//        // Get filter values
//        var getFilterValue = function () {
//            const yearVal = $(settings.salaryYear).val();
//            var filterData = {
//                CompanyCodes: toArray($(settings.companyIds).val()),
//                BranchCodes: toArray($(settings.branchIds).val()),
//                DepartmentCodes: toArray($(settings.departmentIds).val()),
//                DesignationCodes: toArray($(settings.designationIds).val()),
//                EmployeeIDs: toArray($(settings.employeeIds).val()),
//                MonthIDs: toArray($(settings.monthIds).val()),
//                DeductionTypeIDs: toArray($(settings.typeIds).val()),
//                SalaryYear: yearVal || null
//            };
//            return filterData;
//        };

//        // Array helper
//        var toArray = function (value) {
//            if (!value) return [];
//            if (Array.isArray(value)) return value;
//            return [value];
//        };

//        // Get report period based on selected months and year
//        var getReportPeriod = function () {
//            var selectedMonthIds = toArray($(settings.monthIds).val());
//            var selectedYear = $(settings.salaryYear).val();

//            if (!selectedMonthIds || selectedMonthIds.length === 0) {
//                return selectedYear ? `Year ${selectedYear}` : 'All Periods';
//            }

//            // Get month names from the dropdown options
//            var monthNames = [];
//            selectedMonthIds.forEach(function (monthId) {
//                var monthOption = $(settings.monthIds).find(`option[value="${monthId}"]`);
//                if (monthOption.length > 0) {
//                    monthNames.push(monthOption.text());
//                }
//            });

//            // If we have month names and year, combine them
//            if (monthNames.length > 0 && selectedYear) {
//                if (monthNames.length === 1) {
//                    return `${monthNames[0]} ${selectedYear}`;
//                } else {
//                    return `${monthNames.join(", ")} ${selectedYear}`;
//                }
//            } else if (monthNames.length > 0) {
//                // Only months selected, no year
//                return monthNames.join(", ");
//            } else if (selectedYear) {
//                // Only year selected
//                return `Year ${selectedYear}`;
//            }

//            return 'All Periods';
//        };

//        // Group data by department
//        var groupByDepartment = function (data) {
//            var grouped = {};
//            data.forEach(function (item) {
//                var deptKey = item.departmentName || 'Unknown Department';
//                if (!grouped[deptKey]) {
//                    grouped[deptKey] = [];
//                }
//                grouped[deptKey].push(item);
//            });
//            return grouped;
//        };

//        // Calculate department totals
//        var calculateDepartmentTotal = function (departmentData) {
//            return departmentData.reduce(function (sum, item) {
//                return sum + (parseFloat(item.deductionAmount) || 0);
//            }, 0);
//        };

//        // Auto-select current month after months are loaded
//        var autoSelectCurrentMonth = function () {
//            if (window.currentMonthToSelect) {
//                var monthSelect = $(settings.monthIds);

//                // Check if the current month option exists
//                if (monthSelect.find(`option[value="${window.currentMonthToSelect}"]`).length > 0) {
//                    // Select the current month
//                    monthSelect.val(window.currentMonthToSelect);
//                    monthSelect.multiselect('refresh');

//                    // Clear the stored value to prevent repeated selections
//                    window.currentMonthToSelect = null;

//                    console.log('Auto-selected current month:', window.currentMonthToSelect || 'Current month');
//                }
//            }
//        };

//        // Load filter data
//        var loadFilterEmp = function () {
//            showLoading();
//            var filterData = getFilterValue();

//            $.ajax({
//                url: filterUrl,
//                type: "POST",
//                contentType: "application/json",
//                data: JSON.stringify(filterData),
//                success: function (res) {
//                    console.log(res);
//                    hideLoading();
//                    if (!res.isSuccess) {
//                        showToast('error', res.message);
//                        return;
//                    }

//                    // Clear previous event handlers
//                    $(settings.companyIds, settings.branchIds, settings.departmentIds,
//                        settings.designationIds, settings.employeeIds, settings.monthIds,
//                        settings.typeIds).off('change');

//                    loadTableData(res);
//                    const data = res.data;

//                    if (data.branches && data.branches.length > 0 && data.branches.some(x => x.code != null && x.name != null)) {
//                        var branches = data.branches;
//                        var optbranche = $(settings.branchIds);
//                        $.each(branches, function (index, branche) {
//                            if (branche.code != null && branche.name != null && optbranche.find(`option[value="${branche.code}"]`).length === 0) {
//                                optbranche.append(`<option value="${branche.code}">${branche.name}</option>`)
//                            }
//                        });
//                        optbranche.multiselect('rebuild');
//                    }
//                    if (data.departments && data.departments.length > 0 && data.departments.some(x => x.code != null && x.name != null)) {
//                        var departments = data.departments;
//                        var optDepartment = $(settings.departmentIds);
//                        $(settings.branchIds).change(function () {
//                            optDepartment.empty();
//                        });
//                        $.each(departments, function (index, department) {
//                            if (department.code != null && department.name != null && optDepartment.find(`option[value="${department.code}"]`).length === 0) {
//                                optDepartment.append(`<option value="${department.code}">${department.name}</option>`)
//                            }
//                        })
//                        optDepartment.multiselect('rebuild');
//                    }
//                    if (data.designations && data.designations.length > 0 && data.designations.some(x => x.code != null && x.name != null)) {
//                        var designations = data.designations;
//                        var optDesignation = $(settings.designationIds);
//                        $(settings.branchIds).change(function () {
//                            optDesignation.empty();
//                        });
//                        $(settings.departmentIds).change(function () {
//                            optDesignation.empty();
//                        });
//                        $.each(designations, function (index, designation) {
//                            if (designation.code != null && designation.name != null && optDesignation.find(`option[value="${designation.code}"]`).length === 0) {
//                                optDesignation.append(`<option value=${designation.code}>${designation.name}</option>`)
//                            }
//                        });
//                        optDesignation.multiselect('rebuild');
//                    }
//                    if (data.employees && data.employees.length > 0 && data.employees.some(x => x.code != null && x.name != null)) {
//                        var employees = data.employees;
//                        var optEmployee = $(settings.employeeIds);
//                        [settings.branchIds, settings.departmentIds, settings.designationIds, settings.monthIds, settings.typeIds].forEach(function (selector) {
//                            $(selector).change(function () {
//                                optEmployee.empty();
//                            });
//                        });

//                        $.each(employees, function (index, employee) {
//                            if (employee.code != null && employee.name != null && optEmployee.find(`option[value=${employee.code}]`).length === 0) {
//                                optEmployee.append(`<option value=${employee.code}>${employee.name}( ${employee.code}  )</option>`)
//                            }
//                        });
//                        optEmployee.multiselect('rebuild');
//                    }

//                    // Populate dropdowns
//                    populateDropdown(data.companies, settings.companyIds);
//                    populateDropdown(data.branches, settings.branchIds);
//                    populateDropdown(data.departments, settings.departmentIds);
//                    populateDropdown(data.designations, settings.designationIds);
//                    populateDropdown(data.employees, settings.employeeIds, true); // with employee code
//                    populateDropdown(data.deductionTypes, settings.typeIds);
//                    populateDropdown(data.months, settings.monthIds);

//                    // Auto-select current month after months are populated
//                    setTimeout(function () {
//                        autoSelectCurrentMonth();
//                    }, 100); // Small delay to ensure DOM is updated
//                },
//                error: function (e) {
//                    console.log(e);
//                    hideLoading();
//                    showToast('error', 'Failed to load data');
//                }
//            });
//        };

//        // Helper function to populate dropdowns
//        var populateDropdown = function (items, selector, includeCode = false) {
//            if (items && items.length > 0 && items.some(x => x.code != null && x.name != null)) {
//                var dropdown = $(selector);
//                $.each(items, function (index, item) {
//                    if (item.code != null && item.name != null &&
//                        dropdown.find(`option[value="${item.code}"]`).length === 0) {
//                        var text = includeCode ? `${item.name} (${item.code})` : item.name;
//                        dropdown.append(`<option value="${item.code}">${text}</option>`);
//                    }
//                });
//                dropdown.multiselect('rebuild');
//            }
//        };

//        // Show/hide preview functions
//        var showPreview = function () {
//            var tableContainer = $("#salaryDeductionReport-container");
//            var gridContainer = $("#RosterScheduleReport-container");
//            tableContainer.show();
//            gridContainer.show();
//        };

//        var hidePreview = function () {
//            var tableContainer = $("#salaryDeductionReport-container");
//            var gridContainer = $("#RosterScheduleReport-container");
//            tableContainer.hide();
//            gridContainer.hide();
//        };

//        // Load table data with department grouping
//        var loadTableData = function (res) {
//            console.log(res);
//            var tableData = res.data.salaryDeduction;
//            console.log(tableData);

//            if (reportDataTable !== null) {
//                reportDataTable.destroy();
//            }

//            var tableContainer = $("#RosterScheduleReport-container");
//            var gridContainer = $("#salaryDeductionReport-container"); 
//            if (tableContainer.length === 0) {
//                console.error("Table container not found");
//                return;
//            }

//            tableContainer.empty();
//            tableContainer.hide(); // Hide by default
//            gridContainer.hide();
//            if (!tableData || tableData.length === 0) {
//                tableContainer.append('<div class="alert alert-info">No data found for the selected criteria.</div>');
//                return;
//            }

//            // Get report period based on selected filters (not from data)
//            var reportPeriod = getReportPeriod();

//            // Add title section (matching PDF format)
//            var titleSection = $(`
//                <div class="report-header text-center mb-4">
//                    <h4 class="mb-0" style="font-weight: bold; font-size: 20px;">Data Path</h3>
//                    <h3 class="mb-0" style="font-weight: bold; font-size: 16px;">Salary Deduction Report</h3>
//                    <h4 class="mb-4" style="font-size: 14px;">For the month of ${reportPeriod}</h4>
//                </div>
//            `);

//            tableContainer.append(titleSection);

//            var groupedData = groupByDepartment(tableData);
//            var grandTotal = 0;
//            var totalEmployees = 0;
//            var totalDeductions = 0;

//            Object.keys(groupedData).sort().forEach(function (departmentName) {
//                var departmentData = groupedData[departmentName];
//                var departmentTotal = calculateDepartmentTotal(departmentData);
//                grandTotal += departmentTotal;
//                totalDeductions += departmentData.length;

//                // Track unique employees in this department
//                var uniqueEmployees = new Set();
//                departmentData.forEach(function (item) {
//                    if (item.code) {
//                        uniqueEmployees.add(item.code);
//                    }
//                });
//                totalEmployees += uniqueEmployees.size;

//                var departmentSerialNumber = 1;

//                var departmentHeader = $(`
//            <div class="department-section mb-4">
//                <h5 class="department-title mb-3" style="font-weight: bold; font-size: 12px; margin-top: 15px; margin-bottom: 10px;">
//                    Department: ${departmentName}
//                </h5>
//                <div class="table-responsive">
//                    <table class="table table-bordered department-table" style="width: 100%; margin-bottom: 5px;">
//                        <thead>
//                            <tr>
//                                <th style="width: 4%; text-align: center; border: 1px solid #000; font-weight: bold;">SL No.</th>
//                                <th style="width: 7%; text-align: center; border: 1px solid #000; font-weight: bold;">Employee ID</th>
//                                <th style="width: 12%; text-align: center; border: 1px solid #000; font-weight: bold;">Name</th>
//                                <th style="width: 15%; text-align: center; border: 1px solid #000; font-weight: bold;">Designation</th>
//                                <th style="width: 10%; text-align: center; border: 1px solid #000; font-weight: bold;">Branch</th>
//                                <th style="width: 10%; text-align: center; border: 1px solid #000; font-weight: bold;">Deduction Type</th>
//                                <th style="width: 7%; text-align: center; border: 1px solid #000; font-weight: bold;">Amount</th>
//                                <th style="width: 7%; text-align: center; border: 1px solid #000; font-weight: bold;">Month</th>
//                                <th style="width: 7%; text-align: center; border: 1px solid #000; font-weight: bold;">Year</th>
//                                <th style="width: 12%; text-align: center; border: 1px solid #000; font-weight: bold;">Remarks</th>
//                            </tr>
//                        </thead>
//                        <tbody class="department-tbody">
//                        </tbody>
//                    </table>
//                </div>
//                <div class="department-total text-right mb-3" style="font-weight: bold; margin-top: 5px; margin-bottom: 10px;">
//                    Total: ${departmentTotal.toFixed(2)}
//                </div>
//            </div>
//        `);

//                var tbody = departmentHeader.find('.department-tbody');

//                var orderedItems = departmentData.sort(function (a, b) {
//                    return (a.code || '').localeCompare(b.code || '');
//                });

//                orderedItems.forEach(function (salaryDeduction, index) {
//                    var amount = parseFloat(salaryDeduction.deductionAmount) || 0;

//                    var row = $(`
//                <tr>
//                    <td style="text-align: center; border: 1px solid #000;">${departmentSerialNumber}</td>
//                    <td style="text-align: center; border: 1px solid #000;">${salaryDeduction.code || ''}</td>
//                    <td style="text-align: left; border: 1px solid #000;">${salaryDeduction.name || ''}</td>
//                    <td style="text-align: left; border: 1px solid #000;">${salaryDeduction.designationName || ''}</td>
//                    <td style="text-align: center; border: 1px solid #000;">${salaryDeduction.branchName || ''}</td>
//                    <td style="text-align: center; border: 1px solid #000;">${salaryDeduction.deductionType || ''}</td>
//                    <td style="text-align: right; border: 1px solid #000;">${amount.toFixed(2)}</td>
//                    <td style="text-align: center; border: 1px solid #000;">${salaryDeduction.month || ''}</td>
//                    <td style="text-align: center; border: 1px solid #000;">${salaryDeduction.year || ''}</td>
//                    <td style="text-align: center; border: 1px solid #000;">${salaryDeduction.remarks || ''}</td>
//                </tr>
//            `);
//                    tbody.append(row);
//                    departmentSerialNumber++;
//                });

//                tableContainer.append(departmentHeader);
//            });

//            var summarySection = $(`
//        <div class="summary-section mt-4">
//            <h5 style="font-weight: bold; font-size: 12px; margin-top: 20px; margin-bottom: 10px;">Summary</h5>
//            <div class="row" style="margin-bottom: 0;">
//                <div class="col-md-4" style="text-align: left;">
//                    <span>Total Employee: ${totalEmployees}</span>
//                </div>
//                <div class="col-md-4" style="text-align: center;">
//                    <span>Total Deduction: ${totalDeductions}</span>
//                </div>
//                <div class="col-md-4" style="text-align: right;">
//                    <span>Grand Total: ${grandTotal.toFixed(2)}</span>
//                </div>
//            </div>
//        </div>
//    `);

//            tableContainer.append(summarySection);
//        };

//        var exportReport = function (format) {
//            showLoading();
//            var filterData = getFilterValue();

//            var exportRequest = {
//                FilterData: filterData,
//                ExportFormat: format
//            };

//            $.ajax({
//                url: exportUrl,
//                type: "POST",
//                contentType: "application/json",
//                data: JSON.stringify(exportRequest),
//                xhrFields: {
//                    responseType: 'blob'
//                },
//                success: function (data, status, xhr) {
//                    hideLoading();

//                    // Generate default filename if extraction failed
//                    var timestamp = new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '');
//                    var extension = format === 'pdf' ? 'pdf' : format === 'excel' ? 'xlsx' : 'docx';
//                    var filename = `SalaryDeductionReport_${timestamp}.${extension}`;

//                    // Create download link
//                    var blob = new Blob([data]);
//                    var downloadUrl = window.URL.createObjectURL(blob);
//                    var downloadLink = document.createElement("a");
//                    downloadLink.href = downloadUrl;
//                    downloadLink.download = filename;
//                    document.body.appendChild(downloadLink);
//                    downloadLink.click();
//                    document.body.removeChild(downloadLink);
//                    window.URL.revokeObjectURL(downloadUrl);

//                    showToast('success', 'Report exported successfully');
//                },
//                error: function (xhr, status, error) {
//                    hideLoading();
//                    console.error('Export error:', error);

//                    // Try to parse error message from response
//                    try {
//                        var reader = new FileReader();
//                        reader.onload = function () {
//                            try {
//                                var errorResponse = JSON.parse(reader.result);
//                                showToast('error', errorResponse.message || 'Export failed');
//                            } catch (e) {
//                                showToast('error', 'Export failed. Please try again.');
//                            }
//                        };
//                        reader.readAsText(xhr.responseText);
//                    } catch (e) {
//                        showToast('error', 'Export failed. Please try again.');
//                    }
//                }
//            });
//        };

//        // Button Click Event Binding
//        $("#checkCompanyBtn").on("click", function () {
//            console.log("Company Value:", getFilterValue());
//        });

//        // Preview toggle functionality
//        let isPreviewVisible = false;
//        $("#previewBtn").on("click", function () {
//            if (isPreviewVisible) {
//                hidePreview();
//                //$(this).text("Show Preview");
//            } else {
//                showPreview();
//                //$(this).text("Hide Preview");
//            }
//            isPreviewVisible = !isPreviewVisible;
//        });

//        $("#downloadBtn").on("click", function () {
//            var selectedFormat = $("#exportFormatSelect").val();
//            exportReport(selectedFormat);
//        });

//        // Filter change event binding
//        function bindFilterChangeEvent() {
//            var selectors = [
//                settings.companyIds,
//                settings.branchIds,
//                settings.departmentIds,
//                settings.designationIds,
//                settings.employeeIds,
//                settings.monthIds,
//                settings.typeIds,
//                settings.salaryYear
//            ].join(", ");

//            $(document).off('change', selectors);
//            $(document).on('change', selectors, function () {
//                loadFilterEmp();
//            });

//            $(document).off('input', settings.salaryYear);
//            $(document).on('input', settings.salaryYear, function () {
//                clearTimeout(window.yearInputTimeout);
//                window.yearInputTimeout = setTimeout(function () {
//                    loadFilterEmp();
//                }, 500);
//            });
//        }

//        // Toast notification helper (assumes you have a toast function)
//        function showToast(type, message) {
//            if (typeof toastr !== 'undefined') {
//                toastr[type](message);
//            } else if (typeof Swal !== 'undefined') {
//                Swal.fire({
//                    icon: type === 'error' ? 'error' : 'success',
//                    title: type === 'error' ? 'Error' : 'Success',
//                    text: message,
//                    timer: 3000,
//                    showConfirmButton: false
//                });
//            } else {
//                alert(message);
//            }
//        }

//        // Initialize
//        var init = function () {
//            settings.load();
//            setCurrentMonthAndYear(); // Set current month and year first
//            initializeMultiselects();
//            setupLoadingOverlay();
//            bindFilterChangeEvent();
//            loadFilterEmp();
//        };

//        init();
//    };
//})(jQuery);












//(function ($) {
//    $.salaryDeductionReport = function (options) {
//        var settings = $.extend({
//            baseUrl: "/",
//            companyIds: "#companySelect",
//            branchIds: "#branchSelect",
//            departmentIds: "#departmentSelect",
//            designationIds: "#designationSelect",
//            employeeIds: "#employeeSelect",
//            typeIds: "#typeSelect",
//            monthIds: "#monthSelect",
//            salaryYear: "#yearSelect",
//            load: function () {
//                console.log("Loading...");
//            }
//        }, options);

//        var filterUrl = settings.baseUrl + "/getAllFilterEmp";

//        var setupLoadingOverlay = function () {
//            if ($("#customLoadingOverlay").length === 0) {
//                $("body").append(`
//                    <div id="customLoadingOverlay" style="
//                        display: none;
//                        position: fixed;
//                        top: 0;
//                        left: 0;
//                        width: 100%;
//                        height: 100%;
//                        background-color: rgba(0, 0, 0, 0.5);
//                        z-index: 9999;
//                        justify-content: center;
//                        align-items: center;">
//                        <div style="
//                            background-color: white;
//                            padding: 20px;
//                            border-radius: 5px;
//                            box-shadow: 0 0 10px rgba(0,0,0,0.3);
//                            text-align: center;">
//                            <div class="spinner-border text-primary" role="status">
//                                <span class="sr-only">Loading...</span>
//                            </div>
//                            <p style="margin-top: 10px; margin-bottom: 0;">Loading data...</p>
//                        </div>
//                    </div>
//                `);
//            }
//        };

//        function showLoading() {
//            $("#customLoadingOverlay").css("display", "flex");
//        }

//        function hideLoading() {
//            $("#customLoadingOverlay").hide();
//        }

//        var reportDataTable = null;

//        var initializeMultiselects = function () {
//            var selectors = [
//                settings.companyIds,
//                settings.branchIds,
//                settings.departmentIds,
//                settings.designationIds,
//                settings.employeeIds,
//                settings.typeIds,
//                settings.monthIds
//            ].join(", ");

//            $(selectors).multiselect({
//                enableFiltering: true,
//                includeSelectAllOption: true,
//                selectAllText: 'Select All',
//                nonSelectedText: 'Select Items',
//                nSelectedText: 'Selected',
//                allSelectedText: 'All Selected',
//                filterPlaceholder: 'Search.......',
//                buttonWidth: '100%',
//                maxHeight: 350,
//                enableClickableOptGroups: true,
//                dropUp: false,
//                numberDisplayed: 1,
//                enableCaseInsensitiveFiltering: true
//            });
//        };

//        // Filter Value Getter
//        var getFilterValue = function () {
//            const yearVal = $(settings.salaryYear).val();
//            var filterData = {
//                CompanyCodes: toArray($(settings.companyIds).val()),
//                BranchCodes: toArray($(settings.branchIds).val()),
//                DepartmentCodes: toArray($(settings.departmentIds).val()),
//                DesignationCodes: toArray($(settings.designationIds).val()),
//                EmployeeIDs: toArray($(settings.employeeIds).val()),
//                MonthIDs: toArray($(settings.monthIds).val()),
//                DeductionTypeIDs: toArray($(settings.typeIds).val()),
//                SalaryYear: yearVal || null
//            };
//            return filterData;
//        };

//        // Array Helper
//        var toArray = function (value) {
//            if (!value) return [];
//            if (Array.isArray(value)) return value;
//            return [value];
//        };

//        // Group data by department
//        var groupByDepartment = function (data) {
//            var grouped = {};
//            data.forEach(function (item) {
//                var deptKey = item.departmentName || 'Unknown Department';
//                if (!grouped[deptKey]) {
//                    grouped[deptKey] = [];
//                }
//                grouped[deptKey].push(item);
//            });
//            return grouped;
//        };

//        // Calculate totals for a department group
//        var calculateDepartmentTotal = function (departmentData) {
//            return departmentData.reduce(function (sum, item) {
//                return sum + (parseFloat(item.deductionAmount) || 0);
//            }, 0);
//        };

//        var loadFilterEmp = function () {
//            showLoading();
//            var filterData = getFilterValue();

//            $.ajax({
//                url: filterUrl,
//                type: "POST",
//                contentType: "application/json",
//                data: JSON.stringify(filterData),
//                success: function (res) {
//                    console.log(res);
//                    hideLoading();
//                    if (!res.isSuccess) {
//                        showToast('error', res.message);
//                        return;
//                    }

//                    $(settings.companyIds, settings.branchIds, settings.departmentIds, settings.designationIds, settings.employeeIds, settings.monthIds, settings.typeIds).off('change');
//                    loadTableData(res);
//                    const data = res.data;

//                    // Populate filter dropdowns (existing code)
//                    if (data.companies && data.companies.length > 0 && data.companies.some(x => x.code != null && x.name != null)) {
//                        var Companys = data.companies;
//                        var optCompany = $(settings.companyIds);
//                        $.each(Companys, function (index, company) {
//                            if (company.code != null && company.name != null && optCompany.find(`option[value="${company.code}"]`).length === 0) {
//                                optCompany.append(`<option value="${company.code}">${company.name}</option>`)
//                            }
//                        });
//                        optCompany.multiselect('rebuild');
//                    }

//                    if (data.branches && data.branches.length > 0 && data.branches.some(x => x.code != null && x.name != null)) {
//                        var branches = data.branches;
//                        var optbranche = $(settings.branchIds);
//                        $.each(branches, function (index, branche) {
//                            if (branche.code != null && branche.name != null && optbranche.find(`option[value="${branche.code}"]`).length === 0) {
//                                optbranche.append(`<option value="${branche.code}">${branche.name}</option>`)
//                            }
//                        });
//                        optbranche.multiselect('rebuild');
//                    }

//                    if (data.departments && data.departments.length > 0 && data.departments.some(x => x.code != null && x.name != null)) {
//                        var departments = data.departments;
//                        var optDepartment = $(settings.departmentIds);
//                        $(settings.branchIds).change(function () {
//                            optDepartment.empty();
//                        });
//                        $.each(departments, function (index, department) {
//                            if (department.code != null && department.name != null && optDepartment.find(`option[value="${department.code}"]`).length === 0) {
//                                optDepartment.append(`<option value="${department.code}">${department.name}</option>`)
//                            }
//                        })
//                        optDepartment.multiselect('rebuild');
//                    }

//                    if (data.designations && data.designations.length > 0 && data.designations.some(x => x.code != null && x.name != null)) {
//                        var designations = data.designations;
//                        var optDesignation = $(settings.designationIds);
//                        $(settings.branchIds).change(function () {
//                            optDesignation.empty();
//                        });
//                        $(settings.departmentIds).change(function () {
//                            optDesignation.empty();
//                        });
//                        $.each(designations, function (index, designation) {
//                            if (designation.code != null && designation.name != null && optDesignation.find(`option[value="${designation.code}"]`).length === 0) {
//                                optDesignation.append(`<option value=${designation.code}>${designation.name}</option>`)
//                            }
//                        });
//                        optDesignation.multiselect('rebuild');
//                    }

//                    if (data.employees && data.employees.length > 0 && data.employees.some(x => x.code != null && x.name != null)) {
//                        var employees = data.employees;
//                        var optEmployee = $(settings.employeeIds);
//                        [settings.branchIds, settings.departmentIds, settings.designationIds, settings.typeIds, settings.monthIds, settings.salaryYear].forEach(function (selector) {
//                            $(selector).change(function () {
//                                optEmployee.empty();
//                            });
//                        });

//                        $.each(employees, function (index, employee) {
//                            if (employee.code != null && employee.name != null && optEmployee.find(`option[value=${employee.code}]`).length === 0) {
//                                optEmployee.append(`<option value=${employee.code}>${employee.name}( ${employee.code}  )</option>`)
//                            }
//                        });
//                        optEmployee.multiselect('rebuild');
//                    }

//                    if (data.deductionTypes && data.deductionTypes.length > 0 && data.deductionTypes.some(x => x.code != null && x.name != null)) {
//                        var deductionType = data.deductionTypes;
//                        var optDeductionType = $(settings.typeIds);
//                        $.each(deductionType, function (index, type) {
//                            if (type.code != null && type.name != null && optDeductionType.find(`option[value="${type.code}"]`).length === 0) {
//                                optDeductionType.append(`<option value=${type.code}>${type.name}</option>`)
//                            }
//                        });
//                        optDeductionType.multiselect('rebuild');
//                    }

//                    if (data.months && data.months.length > 0 && data.months.some(x => x.code != null && x.name != null)) {
//                        var month = data.months
//                        var optSalaryMonth = $(settings.monthIds);
//                        $.each(data.months, function (index, month) {
//                            if (month.code != null && month.name != null && optSalaryMonth.find(`option[value="${month.code}"]`).length === 0) {
//                                optSalaryMonth.append(`<option value="${month.code}">${month.name}</option>`);
//                            }
//                        });
//                        optSalaryMonth.multiselect('rebuild');
//                    }
//                },
//                error: function (e) {
//                    console.log(e);
//                    hideLoading();
//                }
//            });
//        };

//        // Function to show/hide preview based on button click
//        var showPreview = function () {
//            var tableContainer = $("#RosterScheduleReport-container");
//            tableContainer.show();
//        };

//        var hidePreview = function () {
//            var tableContainer = $("#RosterScheduleReport-container");
//            tableContainer.hide();
//        };

//        // Enhanced table loading with department grouping (hidden by default)
//        var loadTableData = function (res) {
//            console.log(res);
//            var tableData = res.data.salaryDeduction;
//            console.log(tableData);

//            if (reportDataTable !== null) {
//                reportDataTable.destroy();
//            }

//            var tableContainer = $("#RosterScheduleReport-container");
//            if (tableContainer.length === 0) {
//                console.error("Table container not found");
//                return;
//            }

//            tableContainer.empty();
//            tableContainer.hide(); // Hide by default until preview button is clicked

//            // Group data by department
//            var groupedData = groupByDepartment(tableData);
//            var grandTotal = 0;
//            var totalEmployees = 0;

//            // Create tables for each department
//            Object.keys(groupedData).forEach(function (departmentName) {
//                var departmentData = groupedData[departmentName];
//                var departmentTotal = calculateDepartmentTotal(departmentData);
//                grandTotal += departmentTotal;
//                totalEmployees += departmentData.length;
//                var departmentSerialNumber = 1; // Reset serial number for each department

//                // Create department header
//                var departmentHeader = $(`
//                    <div class="department-section mb-4">
//                        <h4 class="department-title mb-3">
//                            Department: ${departmentName}
//                        </h4>
//                        <div class="table-responsive">
//                            <table class="table table-striped table-bordered department-table" style="margin-bottom: 0;">
//                                <thead>
//                                    <tr>
//                                        <th>SL</th>
//                                        <th>Employee Id</th>
//                                        <th>Name</th>
//                                        <th>Designation</th>
//                                        <th>Branch</th>
//                                        <th>Deduction Type</th>
//                                        <th>Deduction Amount</th>
//                                        <th>Month</th>
//                                        <th>Year</th>
//                                        <th>Remarks</th>
//                                    </tr>
//                                </thead>
//                                <tbody class="department-tbody">
//                                </tbody>
//                                <tfoot>
//                                    <tr class="font-weight-bold">
//                                        <td colspan="6" class="text-right"><strong> Total:</strong></td>
//                                        <td><strong>${departmentTotal.toFixed(2)}</strong></td>
//                                        <td colspan="3"></td>
//                                    </tr>
//                                </tfoot>
//                            </table>
//                        </div>
//                    </div>
//                `);

//                var tbody = departmentHeader.find('.department-tbody');

//                // Add rows for each employee in this department
//                departmentData.forEach(function (salaryDeduction, index) {
//                    var row = $(`
//                        <tr>
//                            <td>${departmentSerialNumber}</td>
//                            <td>${salaryDeduction.code || ''}</td>
//                            <td>${salaryDeduction.name || ''}</td>
//                            <td>${salaryDeduction.designationName || ''}</td>
//                            <td>${salaryDeduction.branchName || ''}</td>
//                            <td>${salaryDeduction.deductionType || ''}</td>
//                            <td>${parseFloat(salaryDeduction.deductionAmount || 0).toFixed(2)}</td>
//                            <td>${salaryDeduction.month || ''}</td>
//                            <td>${salaryDeduction.year || ''}</td>
//                            <td>${salaryDeduction.remarks || ''}</td>
//                        </tr>
//                    `);
//                    tbody.append(row);
//                    departmentSerialNumber++;
//                });

//                tableContainer.append(departmentHeader);
//            });

//            // Add grand total section
//            var grandTotalSection = $(`
//                <div class="grand-total-section mt-4">
//                    <div class="card">
//                        <div class="card-header">
//                            <h5 class="mb-0">Summary</h5>
//                        </div>
//                        <div class="card-body">
//                            <div class="row">
//                                <div class="col-md-4">
//                                    <h6>Total Departments: <span class="badge badge-primary">${Object.keys(groupedData).length}</span></h6>
//                                </div>
//                                <div class="col-md-4">
//                                    <h6>Total Employees: <span class="badge badge-info">${totalEmployees}</span></h6>
//                                </div>
//                                <div class="col-md-4">
//                                    <h6>Grand Total Deduction: <span class="badge badge-success">${grandTotal.toFixed(2)}</span></h6>
//                                </div>
//                            </div>
//                        </div>
//                    </div>
//                </div>
//            `);

//            tableContainer.append(grandTotalSection);
//        };

//        // Button Click Event Binding
//        $("#checkCompanyBtn").on("click", function () {
//            console.log("Company Value:", getFilterValue());
//        });

//let isPreviewVisible = false;

//$("#previewBtn").on("click", function () {
//    if (isPreviewVisible) {
//        hidePreview();
//        $(this).text("Show Preview"); // Change button text
//    } else {
//        showPreview();
//        $(this).text("Hide Preview"); // Change button text
//    }
//    isPreviewVisible = !isPreviewVisible; // Toggle the flag
//});
//        function bindFilterChangeEvent() {
//            var selectors = [
//                settings.companyIds,
//                settings.branchIds,
//                settings.departmentIds,
//                settings.designationIds,
//                settings.employeeIds,
//                settings.monthIds,
//                settings.typeIds,
//                settings.salaryYear
//            ].join(", ");

//            $(document).off('change', selectors);
//            $(document).on('change', selectors, function () {
//                loadFilterEmp();
//            });

//            $(document).off('input', settings.salaryYear);
//            $(document).on('input', settings.salaryYear, function () {
//                clearTimeout(window.yearInputTimeout);
//                window.yearInputTimeout = setTimeout(function () {
//                    loadFilterEmp();
//                }, 500);
//            });
//        }

//        // Enhanced PDF export for grouped tables
//        $("#downloadPdfBtn").on("click", function () {
//            const { jsPDF } = window.jspdf;
//            const doc = new jsPDF('landscape');

//            doc.setFontSize(16);
//            doc.text('Data Path')
//            doc.text('Salary Deduction Report', 14, 15);

//            let yPosition = 25;

//            $('.department-section').each(function (index) {
//                const departmentTitle = $(this).find('.department-title').text();
//                const table = $(this).find('.department-table')[0];

//                // Add department title
//                doc.setFontSize(12);
//                doc.setFont(undefined, 'bold');
//                doc.text(departmentTitle, 14, yPosition);
//                yPosition += 7;

//                // Add table
//                doc.autoTable({
//                    html: table,
//                    startY: yPosition,
//                    theme: 'grid',
//                    styles: {
//                        fontSize: 7,
//                        cellPadding: 1,
//                        lineColor: [0, 0, 0],
//                        lineWidth: 0.3
//                    },
//                    headStyles: {
//                        fillColor: [240, 240, 240],
//                        textColor: [30, 30, 30],
//                        fontStyle: 'bold'
//                    },
//                    footStyles: {
//                        fillColor: [220, 255, 220],
//                        textColor: [0, 0, 0],
//                        fontStyle: 'bold'
//                    },
//                    margin: { top: 10, left: 10, right: 10 }
//                });

//                yPosition = doc.lastAutoTable.finalY + 10;

//                // Add new page if needed
//                if (yPosition > 180 && index < $('.department-section').length - 1) {
//                    doc.addPage();
//                    yPosition = 20;
//                }
//            });

//            doc.save('Salary-Deduction-Department-Wise.pdf');
//        });

//        // Enhanced Word export
//        $("#downloadWordBtn").on("click", function () {
//            const header = `
//                <html xmlns:o='urn:schemas-microsoft-com:office:office'
//                      xmlns:w='urn:schemas-microsoft-com:office:word'
//                      xmlns='http://www.w3.org/TR/REC-html40'>
//                <head>
//                    <meta charset='utf-8'>
//                    <title>Salary Deduction Report</title>
//                    <style>
//                        body { font-family: Arial, sans-serif; font-size: 12px; }
//                        .department-title { background-color: #007bff; color: white; padding: 10px; margin-top: 20px; }
//                        table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }
//                        th, td { border: 1px solid black; padding: 5px; text-align: left; }
//                        .total-row { background-color: #d1ecf1; font-weight: bold; }
//                        .grand-total { background-color: #d4edda; padding: 15px; margin-top: 20px; }
//                    </style>
//                </head>
//                <body>

//                    <h1>Data Path</h1>
//                    <h2>Salary Deduction Report</h2>
//            `;

//            let content = '';
//            $('.department-section').each(function () {
//                const departmentTitle = $(this).find('.department-title').text();
//                const tableHtml = $(this).find('.department-table')[0].outerHTML;
//                content += `<div class="department-title">${departmentTitle}</div>`;
//                content += tableHtml;
//            });

//            // Add summary
//            const summaryHtml = $('.grand-total-section')[0].outerHTML;
//            content += summaryHtml;

//            const footer = `</body></html>`;
//            const fullHtml = header + content + footer;

//            const blob = new Blob(['\ufeff', fullHtml], { type: 'application/msword' });
//            const downloadLink = document.createElement("a");
//            downloadLink.href = URL.createObjectURL(blob);
//            downloadLink.download = 'Salary-Deduction-Department-Wise.doc';

//            document.body.appendChild(downloadLink);
//            downloadLink.click();
//            document.body.removeChild(downloadLink);
//        });

//        // Enhanced Excel export
//        $("#downloadExcelBtn").on("click", function () {
//            let htmlContent = `
//                <html xmlns:o="urn:schemas-microsoft-com:office:office"
//                      xmlns:x="urn:schemas-microsoft-com:office:excel"
//                      xmlns="http://www.w3.org/TR/REC-html40">
//                <head>
//                    <meta charset="utf-8">
//                    <style>
//                        table { border-collapse: collapse; width: 100%; margin-bottom: 20px; }
//                        th, td { border: 1px solid black; padding: 5px; text-align: left; }
//                        .department-header { background-color: #4472C4; color: white; font-weight: bold; }
//                        .total-row { background-color: #D1E7DD; font-weight: bold; }
//                    </style>
//                </head>
//                <body>
//                    <h1>Data Path</h1>
//                    <h2>Salary Deduction Report</h2>
//            `;

//            $('.department-section').each(function () {
//                const departmentTitle = $(this).find('.department-title').text();
//                const tableHtml = $(this).find('.department-table')[0].outerHTML;
//                htmlContent += `<h3 class="department-header">${departmentTitle}</h3>`;
//                htmlContent += tableHtml;
//            });

//            htmlContent += $('.grand-total-section')[0].outerHTML;
//            htmlContent += '</body></html>';

//            const blob = new Blob([htmlContent], { type: 'application/vnd.ms-excel' });
//            const url = URL.createObjectURL(blob);

//            const downloadLink = document.createElement("a");
//            downloadLink.href = url;
//            downloadLink.download = "Salary-Deduction-Department-Wise.xls";
//            document.body.appendChild(downloadLink);
//            downloadLink.click();
//            document.body.removeChild(downloadLink);
//        });

//        // Initialize
//        var init = function () {
//            settings.load();
//            initializeMultiselects();
//            setupLoadingOverlay();
//            bindFilterChangeEvent();
//            loadFilterEmp();
//        };
//        init();
//    };
//})(jQuery);