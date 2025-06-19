(function ($) {
    $.otherBenefitsReport = function (options) {
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
            previewContainer: "#otherBenefitReport-container .card-body",
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
                [settings.typeIds, 'Benefit Type(s)'],
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
            BenefitTypeIDs: toArray($(settings.typeIds).val()),
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
                [settings.monthIds]: [ settings.departmentIds, settings.designationIds, settings.employeeIds],
                [settings.typeIds]: [ settings.departmentIds, settings.designationIds, settings.employeeIds],
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
                            //settings.companyIds,
                            //settings.branchIds,
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
                    if (data.benefitTypes?.length) {
                        populateSelect(settings.typeIds, data.benefitTypes);
                    }
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
                showToast('warning', 'No benefit is found to generate report');
                return;
            }

            $("#otherBenefitReport-container").show();
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
                    $("#otherBenefitReport-container").hide();
                    showToast('error', 'Preview failed. Please try again.');
                }
            });
        };

        const hidePreview = () => {
            $("#otherBenefitReport-container").hide();
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
                    const filename = `OtherBenefitsReport_${timestamp}.${extensions[format] || extensions.default}`;

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
                $("#otherBenefitReport-container").is(':visible') ? hidePreview() : showPreview()
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
//    $.otherBenefitsReport = function (options) {
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
//            previewContainer: "#otherBenefitReport-container .card-body",
//            load: function () {
//                console.log("Loading...");
//            }
//        }, options);

//        var filterUrl = settings.baseUrl + "/getAllFilterEmp";
//        var exportUrl = settings.baseUrl + "/ExportReport";
//        var previewUrl = settings.baseUrl + "/PreviewReport";
//        var isInitialLoad = true;

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
//                            </div>
//                            <p style="margin-top: 10px; margin-bottom: 0;">Loading data...</p>
//                        </div>
//                    </div>
//                `);
//            }
//        };

//        var setupPreviewContainer = function () {
//            var container = $(settings.previewContainer);
//            if (container.length === 0) {
//                console.error("Preview container not found:", settings.previewContainer);
//                return;
//            }

//            $("#otherBenefitReport-container").hide();

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

//        var setCurrentMonthAndYear = function () {
//            var currentDate = new Date();
//            var currentMonth = currentDate.getMonth() + 1; 
//            var currentYear = currentDate.getFullYear();

//            $(settings.salaryYear).val(currentYear.toString());

//            window.currentMonthToSelect = currentMonth.toString();
//        };

//        var initializeMultiselects = function () {
//            var configs = [
//                { selector: settings.companyIds, nonSelectedText: 'Select Company(s)' },
//                { selector: settings.branchIds, nonSelectedText: 'Select Branch(es)' },
//                { selector: settings.departmentIds, nonSelectedText: 'Select Department(s)' },
//                { selector: settings.designationIds, nonSelectedText: 'Select Designation(s)' },
//                { selector: settings.employeeIds, nonSelectedText: 'Select Employee(s)' },
//                { selector: settings.typeIds, nonSelectedText: 'Select Benefit Type(s)' },
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

//        var toArray = function (value) {
//            if (!value) return [];
//            if (Array.isArray(value)) return value;
//            return [value];
//        };

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
//                var monthSelect = $(settings.monthIds);

//                if (monthSelect.find(`option[value="${window.currentMonthToSelect}"]`).length > 0) {
//                    monthSelect.val(window.currentMonthToSelect);
//                    monthSelect.multiselect('refresh');

//                    window.currentMonthToSelect = null;

//                    console.log('Auto-selected current month:', window.currentMonthToSelect || 'Current month');

//                    setTimeout(function () {
//                        loadFilterEmp();
//                    }, 100);
//                }
//            }
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

//                    if (isInitialLoad) {
//                        clearAndRebuildDropdown(settings.monthIds, data.months);

//                        setTimeout(function () {
//                            autoSelectCurrentMonth();
//                            isInitialLoad = false; 
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

//        var togglePreview = function () {
//            if ($("#salaryDeductionReport-container").is(':visible')) {
//                hidePreview();
//            } else {
//                showPreview();
//            }
//        };

//        var showPreview = function () {
//            var filterData = getFilterValue();

//            var hasFilters = Object.values(filterData).some(value =>
//                (Array.isArray(value) && value.length > 0) ||
//                (typeof value === 'string' && value.trinm() !== '')
//            );

//            if (!hasFilters) {
//                showToast('warning', 'No deduction is found to generate report');
//                return;
//            }

//            var previewRequest = {
//                FilterData: filterData,
//                ExportFormat: 'pdf'
//            };

//            $("#otherBenefitReport-container").show();
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

//                    var blob = new Blob([data], { type: 'application/pdf' });
//                    var pdfUrl = window.URL.createObjectURL(blob);

//                    $("#previewIframe").attr('src', pdfUrl).show();

//                    if (window.currentPdfUrl) {
//                        window.URL.revokeObjectURL(window.currentPdfUrl);
//                    }
//                    window.currentPdfUrl = pdfUrl;
//                },

//                error: function (xhr, status, error) {
//                    $("#previewLoadingIndicator").hide();
//                    console.error('Preview error:', error);

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

//                    $("#otherBenefitReport-container").hide();
//                }
//            });
//        };

//        var hidePreview = function () {
//            $("#otherBenefitReport-container").hide();
//            $("#previewIframe").attr('src', '');

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

//                    var timestamp = new Date().toISOString().slice(0, 19).replace(/[-T:]/g, '');
//                    var extension = format === 'pdf' ? 'pdf' : format === 'excel' ? 'xlsx' : 'docx';
//                    var filename = `OtherBenefitsReport_${timestamp}.${extension}`;

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

//        $("#previewBtn").on("click", function () {
//            togglePreview();
//        });

//        $("#downloadBtn").on("click", function () {
//            var selectedFormat = $("#exportFormatSelect").val();
//            exportReport(selectedFormat);
//        });

//        function bindFilterChangeEvent() {
//            $(document).off('change.salaryReport');

//            $(document).on('change.salaryReport', settings.companyIds, function () {
//                clearDependentDropdowns([settings.branchIds, settings.departmentIds, settings.designationIds, settings.employeeIds]);
//                loadFilterEmp();
//            });

//            $(document).on('change.salaryReport', settings.branchIds, function () {
//                clearDependentDropdowns([settings.departmentIds, settings.designationIds, settings.employeeIds]);
//                loadFilterEmp();
//            });

//            $(document).on('change.salaryReport', settings.departmentIds, function () {
//                clearDependentDropdowns([settings.designationIds, settings.employeeIds]);
//                loadFilterEmp();
//            });

//            $(document).on('change.salaryReport', settings.designationIds, function () {
//                clearDependentDropdowns([settings.employeeIds]);
//                loadFilterEmp();
//            });

//            $(document).on('change.salaryReport', settings.monthIds + ',' + settings.typeIds, function () {
//                clearDependentDropdowns([settings.employeeIds]);
//                loadFilterEmp();
//            });

//            $(document).on('change.salaryReport', settings.employeeIds, function () {
//                loadFilterEmp();
//            });

//            $(document).off('input.salaryReport', settings.salaryYear);
//            $(document).on('input.salaryReport', settings.salaryYear, function () {
//                clearTimeout(window.yearInputTimeout);
//                window.yearInputTimeout = setTimeout(function () {
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

//        var init = function () {
//            settings.load();
//            setCurrentMonthAndYear();
//            initializeMultiselects();
//            setupLoadingOverlay();
//            setupPreviewContainer();
//            bindFilterChangeEvent();
//            loadFilterEmp();
//        };

//        init();
//    };
//})(jQuery);