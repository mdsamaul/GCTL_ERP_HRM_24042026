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
            clearButton: ".js-pPAlert-dec-clear",
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
                allowInput: false,
                onChange: function (selectedDates) {
                    if (selectedDates.length) {
                        toPicker.set('minDate', selectedDates[0]);
                        if (!isInitialLoad) loadFilterEmp();
                    }
                }
            });

            const toPicker = flatpickr(settings.joiningDateTo, {
                dateFormat: "d-m-Y",
                allowInput: false,
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
                    scrollX: true, columnDefs: [
                        {
                            targets: 5,
                            type: 'date',
                            render: function (data, type, row) {
                                if (type === 'sort' || type === 'type') {
                                    if (!data) return '';

                                    let dateParts;
                                    if (data.includes('-')) {
                                        dateParts = data.split('-');
                                    } else if (data.includes('/')) {
                                        dateParts = data.split('/');
                                    } else {
                                        return data;
                                    }

                                    if (dateParts.length === 3) {
                                        return dateParts[2] + '-' + dateParts[1].padStart(2, '0') + '-' + dateParts[0].padStart(2, '0');
                                    }
                                }
                                return data;
                            }
                        },
                        {
                            targets: 7,
                            type: 'date',
                            render: function (data, type, row) {
                                if (type === 'sort' || type === 'type') {
                                    if (!data) return '';

                                    let dateParts;
                                    if (data.includes('-')) {
                                        dateParts = data.split('-');
                                    } else if (data.includes('/')) {
                                        dateParts = data.split('/');
                                    } else {
                                        return data;
                                    }

                                    if (dateParts.length === 3) {
                                        return dateParts[2] + '-' + dateParts[1].padStart(2, '0') + '-' + dateParts[0].padStart(2, '0');
                                    }
                                }
                                return data;
                            }
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
                    }
                });
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
                row.append('<td class="text-center p-1">' + employee.branchName + '</td>');
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
            console.log("Text");
            const filterSelector = [
                settings.departmentSelect,
                settings.designationSelect
            ]

            filterSelector.forEach(sel => {
                $(sel).multiselect('deselectAll', false);
                $(sel).multiselect('updateButtonText');
            })

            $(settings.joiningDateFrom).val('').trigger('change');
            $(settings.joiningDateTo).val('').trigger('change');

            $(settings.pEndDaysSelect).val('');

            isInitialLoad = true;
            loadFilterEmp();
        }

        const bindUIEvents = () => {
            $("#downloadBtn").on("click", () => exportReport($("#exportFormatSelect").val()));
            $(settings.clearButton).on("click", () => clearAllFilters());
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
    }
})(jQuery);