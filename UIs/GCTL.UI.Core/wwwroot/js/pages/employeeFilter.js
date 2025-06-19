//let employeeDataTable = null;

//$(document).ready(function () {
//    setupLoadingOverlay();
//    //loadCompany();
//    initializeMultiselects();
//    initializeWeekendGrid();
//    loadAllFilterEmp();
//    $("#employee-check-all").on('change', function () {
//        var isChecked = $(this).is(':checked');
//        $('#employee-filter-grid-body input[type="checkbox"]').prop('checked', isChecked);
//    });
//});


////=================

////============================================
//function setupLoadingOverlay() {
//    if ($("#customLoadingOverlay").length === 0) {
//        $("body").append(`
//            <div id="customLoadingOverlay" style="
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
//                        <span class="sr-only">Loading...</span>
//                    </div>
//                    <p style="margin-top: 10px; margin-bottom: 0;">Loading data...</p>
//                </div>
//            </div>
//        `);
//    }
//}

//function showLoading() {
//    $("#customLoadingOverlay").css("display", "flex");
//}

//function hideLoading() {
//    $("#customLoadingOverlay").hide();
//}


//$("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect, #employeeStatus, #activityStatusSelect")
//    .on("change", function () {
//        loadAllFilterEmp();
//    });

//function getAllFilterVal() {
//    const filterData = {
//        CompanyCodes: toArray($("#companySelect").val()),
//        BranchCodes: toArray($("#branchSelect").val()),
//        DivisionCodes: toArray($("#divisionSelect").val()),
//        DepartmentCodes: toArray($("#departmentSelect").val()),
//        DesignationCodes: toArray($("#designationSelect").val()),
//        EmployeeIDs: toArray($("#employeeSelect").val()),
//        EmployeeStatuses: toArray($("#activityStatusSelect").val()),
//    };
//    return filterData;
//}

//function toArray(value) {
//    if (!value) return [];
//    if (Array.isArray(value)) return value;
//    return [value]; 
//}

//function loadAllFilterEmp() {
//    showLoading();
//    const filterData = getAllFilterVal();
//    filterData.CompanyCodes = ["001"];
//    $.ajax({
//        url: `/EmployeeFilter/getFilterEmp`,
//        type: "POST",
//        contentType: "application/json",
//        data: JSON.stringify(filterData),

//        success: function (res) {
//            const data = res.lookupData;

//            loadTableData(res);
//            if (data.companies?.length) {
//                populateSelect("#companySelect", data.companies)
//            }
//            if (data.branches?.length) {
//                populateSelect("#branchSelect", data.branches);
//            }
//            if (data.divisions?.length) {
//                populateSelect("#divisionSelect", data.divisions);
//            }
//            if (data.departments?.length) {
//                populateSelect("#departmentSelect", data.departments);
//            }
//            if (data.designations?.length) {
//                populateSelect("#designationSelect", data.designations);
//            }
//            if (data.employees?.length) {
//                populateSelect("#employeeSelect", data.employees);
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
//        "#activityStatusSelect": ["#divisionSelect", "#departmentSelect", "#designationSelect", "#employeeSelect"],
//        "#branchSelect": ["#divisionSelect", "#departmentSelect", "#designationSelect", "#employeeSelect"],
//        "#divisionSelect": ["#departmentSelect", "#designationSelect", "#employeeSelect"],
//        "#departmentSelect": ["#designationSelect", "#employeeSelect"],
//        "#designationSelect": ["#employeeSelect"]
//    };

//    Object.entries(clearMap).forEach(([parent, children]) => {
//        $(parent).off("change.clearDropdowns").on("change.clearDropdowns", function () {
//            children.forEach(child => $(child).empty().multiselect('rebuild'));
//        });
//    });
//}

//let filterChangeBound = false;

//function bindFilterChangeOnce() {
//    if (!filterChangeBound) {
//        $("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect, #employeeStatus, #activityStatusSelect")
//            .on("change.loadFilter", function () {
//                loadAllFilterEmp();
//            });
//        filterChangeBound = true;
//    }
//}


//function loadTableData(res) {
//    console.log(res);
//    var tableDataItem = res.employees;

//    if ($.fn.DataTable.isDataTable('#employee-filter-grid') && employeeDataTable !== null) {
//        employeeDataTable.destroy();
//        employeeDataTable = null;
//    }

//    var tableBody = $("#employee-filter-grid-body");
//    tableBody.empty();

//    $.each(tableDataItem, function (index, employee) {
//        var row = $('<tr>');

//        row.append('<td class="text-center"><input type="checkbox" /></td>');
//        row.append('<td class="text-center">' + employee.employeeId + '</td>');
//        row.append('<td class="text-center">' + employee.employeeName + '</td>');
//        row.append('<td class="text-center">' + employee.designationName + '</td>');
//        row.append('<td class="text-center">' + employee.departmentName + '</td>');
//        row.append('<td class="text-center">' + employee.branchName + '</td>');
//        row.append('<td class="text-center">' + employee.companyName + '</td>');
//        row.append('<td class="text-center">' + employee.employeeTypeName + '</td>');
//        row.append('<td class="text-center">' + employee.joiningDate + '</td>');
//        row.append('<td class="text-center">' + employee.employeeStatus + '</td>');

//        tableBody.append(row);
//    });

//    initializeDataTable();
//}

//function initializeDataTable() {
//    employeeDataTable = $("#employee-filter-grid").DataTable({
//        paging: true,
//        pageLength: 10,
//        lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
//        lengthChange: true,
//        scrollY: "400px",
//        info: true,
//        autoWidth: false,
//        responsive: true,
//        fixedHeader: false, 
//        scrollX: true,
//        columnDefs: [
//            {
//                targets: 0, // first column
//                orderable: false,
//                className: 'no-sort' // optional, to help target with CSS
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
           
//            $('#employee-filter-grid_wrapper .dataTables_filter').hide();
//        },
//    });
//}

//function initializeWeekendGrid() {
//    showLoading();
//    //setTimeout(function () {
//    //    if (employeeDataTable !== null) {
//    //        employeeDataTable.destroy();
//    //    }
//        initializeDataTable();
//    //}, 500);
//}

//function initializeMultiselects() {
//    const nonSelectedTextMap = {
//        companySelect: 'Select Company',
//        branchSelect: 'Select Branch',
//        divisionSelect: 'Select Division',
//        departmentSelect: 'Select Department',
//        designationSelect: 'Select Designation',
//        employeeSelect: 'Select Employee',
//        activityStatusSelect: 'Select Status'
//    };

//    Object.keys(nonSelectedTextMap).forEach(function (id) {
//        const selector = $('#' + id);
//        selector.closest('div').css('margin', '1rem');
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
//            },
//            onFiltering: function (event) {
//                event.query = event.query.toLowerCase();
//            }
//        });
//    });
//}



























