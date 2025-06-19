function showToast(iconType, message) {
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        showClass: {
            popup: 'swal2-show swal2-fade-in'
        },
        hideClass: {
            popup: 'swal2-hide swal2-fade-out'
        }
    });

    Toast.fire({
        icon: iconType,
        title: message
    });
}
$(document).ready(function () {
    setupLoadingOverlay();
    initializeMultiselects();
    loadFilterEmp();
    GetRosterScheduleApproveGride();
});

//date picker 
$(document).ready(function () {
    flatpickr('.flatpickr', {
        dateFormat: "Y-m-d",  
        altInput: true,      
        altFormat: "d/m/Y",  
        allowInput: true,
        onReady: function (selectedDates, dateStr, instance) {
            instance.input.placeholder = "dd/mm/yyyy";
        }
    });
});



//scroll top toolbars 
$(document).ready(() => {
    const header = document.getElementById("stickyHeader");

    window.addEventListener("scroll", function () {
        if (header) {
            if (window.scrollY > 50) {
                header.classList.add("sticky-scrolled");
            } else {
                header.classList.remove("sticky-scrolled");
            }
        }
    });
});

let RosterApprovalDataTable = null;

function setupLoadingOverlay() {
    if ($("#customLoadingOverlay").length === 0) {
        $("body").append(`
            <div id="customLoadingOverlay" style="
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
                    <div class="spinner-border text-primary" role="status">
                        <span class="sr-only">Loading...</span>
                    </div>
                    <p style="margin-top: 10px; margin-bottom: 0;">Loading data...</p>
                </div>
            </div>
        `);
    }
}

function showLoading() {
    $("#customLoadingOverlay").css("display", "flex");
}

function hideLoading() {
    $("#customLoadingOverlay").hide();
}
function initializeMultiselects() {
    $('#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect').multiselect({
        enableFiltering: true,
        includeSelectAllOption: true,
        selectAllText: 'Select All',
        nonSelectedText: 'Select Items',
        nSelectedText: 'Selected',
        allSelectedText: 'All Selected',
        filterPlaceholder: 'Search.......',
        buttonWidth: '100%',
        maxHeight: 350,
        enableClickableOptGroups: true,
        dropUp: false,
        numberDisplayed: 1,
        enableCaseInsensitiveFiltering: true
    });
}

function getAllFilterVal() {
    const fromDateVal = $("#FromDateFilter").val();
    const toDateVal = $("#ToDateFilter").val();

    const filterData = {
        CompanyCodes: toArray($("#companySelect").val()),
        BranchCodes: toArray($("#branchSelect").val()),
        DivisionCodes: toArray($("#divisionSelect").val()),
        DepartmentCodes: toArray($("#departmentSelect").val()),
        DesignationCodes: toArray($("#designationSelect").val()),
        EmployeeIDs: toArray($("#employeeSelect").val()),
        EmployeeStatuses: toArray($("#activityStatusSelect").val()),
        FromDate: fromDateVal ? new Date(fromDateVal).toISOString() : null,
        ToDate: toDateVal ? new Date(toDateVal).toISOString() : null
    };
    return filterData;
}

function toArray(value) {
    if (!value) return [];
    if (Array.isArray(value)) return value;
    return [value];
}

function loadFilterEmp() {
    showLoading();
    var filterData = getAllFilterVal();
    $.ajax({
        url: `/RosterScheduleApproval/GetRosterScheduleFilter`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(filterData),
        success: function (res) {
           
            if (!res.isSuccess) {
                showToast('error', res.message);
                return;
            }
          
            $("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect, #employeeStatus, #activityStatusSelect, #ToDateFilter, #FromDateFilter")
                .off("change");
            loadTableData(res);
           
            const data = res.data;

            if (data.companies && data.companies.length > 0 && data.companies.some(x => x.code != null && x.name != null)) {
                var Companys = data.companies;

                var optCompany = $("#companySelect");
                $.each(Companys, function (index, company) {
                    if (company.code != null && company.name != null && optCompany.find(`option[value="${company.code}"]`).length === 0) {
                        optCompany.append(`<option value="${company.code}">${company.name}</option>`);
                    }
                });
                optCompany.multiselect('rebuild');
            }
            if (data.branches && data.branches.length > 0 && data.branches.some(b => b.code != null && b.name != null)) {
                var dataBranch = data.branches;
                var optBranch = $("#branchSelect");
                $.each(dataBranch, function (index, item) {
                    if (item.code != null && item.name != null && optBranch.find(`option[value="${item.code}"]`).length === 0) {
                        optBranch.append(`<option value="${item.code}">${item.name}</option>`);
                    }
                });
                optBranch.multiselect('rebuild');
            }
            if (data.departments && data.departments.length > 0 && data.departments.some(b => b.code != null && b.name != null)) {
                var dataDepartments = data.departments;
                var optDepartments = $("#departmentSelect");

                $("#branchSelect").change(function () {
                    optDepartments.empty();
                })
                $("#divisionSelect").change(function () {
                    optDepartments.empty();
                })
                $.each(dataDepartments, function (index, item) {
                    if (item.code != null && item.name != null && optDepartments.find(`option[value="${item.code}"]`).length === 0) {
                        optDepartments.append(`<option value="${item.code}">${item.name}</option>`);
                    }
                });

                optDepartments.multiselect('rebuild');
            }


            //// Division
            //if (data.divisions && data.divisions.length > 0 && data.divisions.some(b => b.code != null && b.name != null)) {
            //    var dataDivisions = data.divisions;
            //    var optDivisions = $("#divisionSelect");

            //    $("#branchSelect").change(function () {
            //        optDivisions.empty();
            //    })
            //    $.each(dataDivisions, function (index, item) {
            //        if (item.code != null && item.name != null && optDivisions.find(`option[value="${item.code}"]`).length === 0) {
            //            optDivisions.append(`<option value="${item.code}">${item.name}</option>`);
            //        }
            //    });
            //    optDivisions.multiselect('rebuild');
            //}




            // Designation
            if (data.designations && data.designations.length > 0 && data.designations.some(b => b.code != null && b.name != null)) {
                var dataDesignations = data.designations;
                var optDesignations = $("#designationSelect");
                $("#branchSelect").change(function () {
                    optDesignations.empty();
                })
                $("#divisionSelect").change(function () {
                    optDesignations.empty();
                })
                $("#departmentSelect").change(function () {
                    optDesignations.empty();
                })
                $.each(dataDesignations, function (index, item) {
                    if (item.code != null && item.name != null && optDesignations.find(`option[value="${item.code}"]`).length === 0) {
                        optDesignations.append(`<option value="${item.code}">${item.name}</option>`);
                    }
                });
                optDesignations.multiselect('rebuild');
            }

            if (data.employees && data.employees.length > 0 && data.employees.some(b => b.code != null && b.name != null)) {
                var dataEmployees = data.employees;
                var optEmployees = $("#employeeSelect");
                $("#branchSelect").change(function () {
                    optEmployees.empty();
                })
                $("#divisionSelect").change(function () {
                    optEmployees.empty();
                })
                $("#departmentSelect").change(function () {
                    optEmployees.empty();
                })
                $("#designationSelect").change(function () {
                    optEmployees.empty();
                })
                $.each(dataEmployees, function (index, item) {
                    if (item.code != null && item.name != null && optEmployees.find(`option[value="${item.code}"]`).length === 0) {
                        optEmployees.append(`<option value="${item.code}">${item.name}<b> ( ${item.code} )</b></option>`);
                    }
                });
                optEmployees.multiselect('rebuild');
            }

            $("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect, #employeeStatus, #activityStatusSelect , #ToDateFilter, #FromDateFilter")
                .on("change", function () {
                    loadFilterEmp();
                });
        },
        complete: function () {
            hideLoading();
        },
        error: function (error) {
            showToast("error", error.message);
            hideLoading();
        }
    })
}

function loadTableData(res) {
    
    var tableData = res.data.employees;

    if ($.fn.DataTable.isDataTable("#RosterScheduleApprove-grid")) {
        RosterApprovalDataTable.destroy();
    }

    var tableBody = $("#RosterScheduleApprove-grid-body");
    tableBody.empty();

    // Row build
    $.each(tableData, function (index, employee) {
        var row = $('<tr>');
        row.append('<td class="text-center"><input type="checkbox" /></td>');
        row.append('<td class="text-center">' + employee.rosterScheduleId + '</td>');
        row.append('<td class="text-center">' + employee.code + '</td>');
        row.append('<td class="text-left">' + employee.name + '</td>');
        row.append('<td class="text-left">' + employee.designationName + '</td>');
        row.append('<td class="text-center">' + employee.departmentName + '</td>');
        row.append('<td class="text-center">' + employee.showDate + '</td>');
        row.append('<td class="text-center">' + employee.dayName + '</td>');
        row.append('<td class="text-center">' + employee.shiftName + '</td>');
        row.append('<td class="text-center">' + employee.remark + '</td>');
        tableBody.append(row);
    });

    initializeDataTable();
}


function initializeDataTable() {
    RosterApprovalDataTable = $("#RosterScheduleApprove-grid").DataTable({
        destroy: true,
        paging: true,
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
        lengthChange: true,
        searching: true,
        info: true,
        autoWidth: false,
        scrollX: true,
        ordering: true, 
        responsive: false,
        language: {
            search: "🔍 Search:",
            lengthMenu: "Show _MENU_ entries",
            searchPlaceholder: "Search here.......",
            info: "Showing _START_ to _END_ of _TOTAL_ entries",
            paginate: {
                first: "First",
                previous: "Prev",
                next: "Next",
                last: "Last"
            },
            emptyTable: "No data available"
        },
        columnDefs: [
            { orderable: false, targets: 0 },
            { className: "text-center", targets: "_all" },
            { targets: 0, width: "80px" }
        ],
        initComplete: function () {
            $('.dataTables_filter input').css({
                width: '250px',
                padding: '6px 12px',
                border: '1px solid #ddd',
                borderRadius: '4px'
            });
        }
    });

    setTimeout(function () {
        RosterApprovalDataTable.columns.adjust().draw(false);
    }, 300);
}



$(document).ready(function () {
    $("#rosterScheduleApprove-check-all").on('change', function () {
        var isChecked = $(this).is(":checked");
        $('#RosterScheduleApprove-grid-body input[type="checkbox"]').prop('checked', isChecked);
    });

    $(document).on('change', '#RosterScheduleApprove-grid-body input[type="checkbox"]', function () {
        var totalCheck = $('#RosterScheduleApprove-grid-body input[type="checkbox"]').length;
        var singleCheck = $('#RosterScheduleApprove-grid-body input[type="checkbox"]:checked').length;
        if (totalCheck === singleCheck) {
            $("#rosterScheduleApprove-check-all").prop("checked", true);
        } else {
            $("#rosterScheduleApprove-check-all").prop("checked", false);
        }
    });

    $(".js-roster-approval-save").click(function () {

        var checkedApprovalList = [];
        $('#RosterScheduleApprove-grid-body input[type="checkbox"]:checked').each(function () {
            var row = $(this).closest('tr');
            var rosterId = row.find('td:nth-child(2)').text().trim();
            checkedApprovalList.push(rosterId);
        });
        if (checkedApprovalList.length === 0) {
            showToast("warning", "Please Select at Lest one roster Item.");
            return;
        }
        var remark = $("#remarks").val();
        var FromData = {
            checkedApprovalList: checkedApprovalList,
            remark: remark
        }
        $.ajax({
            url: "/RosterScheduleApproval/ApprovalSetUp",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(FromData),
            success: function (res) {
                loadFilterEmp();
                GetRosterScheduleApproveGride();
                
                $("#remarks").val("");
                $("#rosterScheduleApprove-check-all").prop('checked', false);
            },
            error: function (e) {
                console.log(e.message);
            }
        });
    })

})

function GetRosterScheduleApproveGride() {
    showLoading();

    if ($.fn.DataTable.isDataTable('#RosterScheduleApproveShowData-grid')) {
        $('#RosterScheduleApproveShowData-grid').DataTable().clear().destroy();
    }

    $('#RosterScheduleApproveShowData-grid').DataTable({
        processing: true,
        serverSide: true,
        scrollY: "350px",
        scrollX: true,
        scrollCollapse: true,
        ajax: {
            url: "/RosterScheduleApproval/GetRosterScheduleApproveGrid",
            type: "POST",
            dataSrc: function (json) {
                console.log("Response data:", json);
                return json.data || json;
            },
            error: function (xhr, error, thrown) {
                console.error("DataTable error:", error, thrown);
                console.error("Server response:", xhr.responseText);
                hideLoading();
                alert("An error occurred while loading data. Please check console for details.");
            }
        },
        columns: [
            { data: "rosterScheduleId", orderable: false },
            { data: "employeeID", orderable: false },
            { data: "name", orderable: false },
            { data: "designationName", orderable: false },
            {
                data: "date",
                render: function (data) {
                    if (!data) return '';
                    let d = new Date(data);
                    return d.toLocaleDateString();
                },
                orderable: false
            },
            { data: "shiftName", orderable: false },
            { data: "remark", orderable: false },
            {
                data: "approvalStatus",
                render: function (data) {
                    let statusClass = '';
                    if (data === 'Approved') statusClass = 'text-dark font-weight-bold';
                    else if (data === 'Rejected') statusClass = 'text-danger font-weight-bold';
                    else statusClass = 'text-warning';

                    return '<span class="' + statusClass + '">' + data + '</span>';
                },
                orderable: false
            },
            { data: "approvedBy", orderable: false },
            {
                data: "approvalDatetime",
                render: function (data) {
                    if (!data) return '';
                    let d = new Date(data);
                    return d.toLocaleString('en-US', {
                        year: 'numeric',
                        month: '2-digit',
                        day: '2-digit',
                        hour: '2-digit',
                        minute: '2-digit',
                        hour12: true
                    });
                },
                orderable: false
            },
            { data: "luser", orderable: false }
        ],
        order: [],
        ordering: false,
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
        language: {
            search: "🔍 Search:",
            lengthMenu: "Show _MENU_ entries",
            searchPlaceholder: "Search here...",
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
            hideLoading();

            $('.dataTables_filter input').css({
                width: '250px',
                padding: '6px 12px',
                border: '1px solid #ddd',
                borderRadius: '4px'
            });

            setTimeout(function () {
                $('#RosterScheduleApproveShowData-grid').DataTable().columns.adjust().draw(false);
            }, 300); 


            $(window).off('resize.rosterGridResize').on('resize.rosterGridResize', function () {
                $('#RosterScheduleApproveShowData-grid').DataTable().columns.adjust();
            });
        },
        drawCallback: function () {
            setTimeout(function () {
                $('#RosterScheduleApproveShowData-grid').DataTable().columns.adjust();
            }, 300); 
        }
    });
}


$(document).ready(function () {
    $(document).on('click', "#js-roster-approval-clear", function () {
        clearRosterApproveFrom();
    });

})
function clearRosterApproveFrom() {
    $("#rosterScheduleApprove-check-all").prop('checked', false);
    $('#RosterScheduleApprove-grid input[type="checkbox"]').prop('checked', false);
    $("#remarks").val("");
}