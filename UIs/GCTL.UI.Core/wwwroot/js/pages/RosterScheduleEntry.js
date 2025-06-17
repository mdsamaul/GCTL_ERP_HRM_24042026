﻿function showToast(iconType, message) {
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 5000,
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
var rosterId=null;
$(document).on('input blur', "#YearRosterFrom", function () {
    validateYearInput();
})

$(document).ready(function () {

    showLoading();

    AllMonth();  //get all month
    initializeDataTable();
    FadeInOutBydateOrMont(); //by date or month fadein fadeout
    setupLoadingOverlay();
    initializeMultiselects();
    loadFilterEmp();
    getCurrentYear();
    getShift(); //get all shift
    getRosterScheduleGrid();
    getDateFromFlatPicker("#FromDatePicker");
    getDateFromFlatPicker("#toDateRosterFrom");
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

let tableData;

//function initializeDataTable() {
//    try {
//        if ($.fn.DataTable.isDataTable('#RosterScheduleEntry-grid')) {
//            $('#RosterScheduleEntry-grid').DataTable().destroy();
//        }

//        RosterDataTable = $("#RosterScheduleEntry-grid").DataTable({
//            // Configuration options...
//        });

//        return RosterDataTable;
//    } catch (error) {
//        //console.error("DataTable initialization error:", error);
//        hideLoading();
//    }
//}




let RosterDataTable = null;


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



$(document).on('mousedown', '.dropdown-wrapper .multiselect', function (e) {
    e.preventDefault();
    e.stopPropagation();

    var $dropdown = $(this).next('.multiselect-container');
    var isVisible = $dropdown.is(':visible');

    $('.multiselect-container').hide();

    if (!isVisible) {
        $dropdown.show();
        $(this).addClass('active');
    }

    return false;
});

// Close dropdown when clicking outside
$(document).on('click', function (e) {
    if (!$(e.target).closest('.dropdown-wrapper').length) {
        $('.multiselect-container').hide();
        $('.multiselect').removeClass('active');
    }
});





//$(document).on('click', '.dropdown-wrapper .multiselect', function (e) {
//    e.preventDefault();
//    e.stopPropagation();

//    var $dropdown = $(this).next('.multiselect-container');
//    var isVisible = $dropdown.is(':visible');

//    // Hide all dropdowns and remove active classes
//    $('.multiselect-container').hide();
//    $('.multiselect').removeClass('active');

//    // If not visible before, then show current
//    if (!isVisible) {
//        $dropdown.show();
//        $(this).addClass('active');
//    }
//});




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



$("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect, #employeeStatus, #activityStatusSelect")
    .on("change", function () {
        loadFilterEmp();
    });
function getAllFilterVal() {
    const filterData = {
        CompanyCodes: toArray($("#companySelect").val()),
        BranchCodes: toArray($("#branchSelect").val()),
        DivisionCodes: toArray($("#divisionSelect").val()),
        DepartmentCodes: toArray($("#departmentSelect").val()),
        DesignationCodes: toArray($("#designationSelect").val()),
        EmployeeIDs: toArray($("#employeeSelect").val()),
        EmployeeStatuses: toArray($("#activityStatusSelect").val()),
    };
    return filterData;
}
//function toArray(value) {
//    if (!value) return [];
//    if (Array.isArray(value)) return value;
//    return [value];
//}

function toArray(value) {
    if (!value) return [];
    if (Array.isArray(value)) return value;
    if (typeof value === "string" && value.includes(',')) {
        return value.split(',').map(v => v.trim());
    }
    return [value];
}

function loadFilterEmp() {
  
    showLoading();
    var filterData = getAllFilterVal();
    ClearFrom();
    if (filterData.BranchCodes.length != 0) {
        //console.log(filterData);
        $(".editHideRosterSchedule").show();
        $(".editHide").show();
        $(".editDate").fadeIn(1000).html(`
    <label for="FromDatePicker" class="col-md-12 mb-0 text-right" style="padding-right: 0px;">
        Date From
        <sup class="text-danger">
            <span style="font-size: 20px;">*</span>
        </sup>
    </label>`);
        var isByDateCheck = $("#byDate").is(':checked');
        var isByMonthCheck = $("#byMonth").is(':checked');

        if (isByMonthCheck) {
            $("#monthFields").show();
            $("#dateFields").hide();
        }
        if (isByDateCheck) {
            $("#monthFields").hide();
            $("#dateFields").show();
        }
    }

    //console.log(filterData);
    $.ajax({
        url: `/RosterScheduleEntry/getAllFilterEmp`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(filterData),
        success: function (res) {
            //console.log(res);
            if (!res.isSuccess) {
                showToast('error', res.message);
                return;
            }
            //ClearFrom();
            //showToast('success', res.message);
            //console.log(res.data);
            $("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect, #employeeStatus, #activityStatusSelect")
                .off("change");
            loadTableData(res);
            //console.log(res);
            const data = res.data;
            //console.log(data);
            if (data.companies && data.companies.length > 0 && data.companies.some(x => x.code != null && x.name != null)) {
                var Companys = data.companies;
                //console.log(dataCompany);
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


            // Division
            if (data.divisions && data.divisions.length > 0 && data.divisions.some(b => b.code != null && b.name != null)) {
                var dataDivisions = data.divisions;
                var optDivisions = $("#divisionSelect");

                $("#branchSelect").change(function () {
                    optDivisions.empty();
                })
                $.each(dataDivisions, function (index, item) {
                    if (item.code != null && item.name != null && optDivisions.find(`option[value="${item.code}"]`).length === 0) {
                        optDivisions.append(`<option value="${item.code}">${item.name}</option>`);
                    }
                });
                optDivisions.multiselect('rebuild');
            }




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

            $("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect, #employeeStatus, #activityStatusSelect")
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
    //console.log(res);
    var tableData = res.data.employees;
    if (RosterDataTable !== null) {
        RosterDataTable.destroy();
    }
    var tableBody = $("#RosterScheduleEntry-grid-body");
    tableBody.empty();
    $.each(tableData, function (index, employee) {
        //console.log(employee);
        var row = $('<tr>');
        row.append('<td class="text-center" style="width:60px !important;"><input type="checkbox" /></td >');
        row.append('<td class="text-center" style="width:120px !important;">' + employee.code + '</td>');
        row.append('<td class="text-center">' + employee.name + '</td>');
        row.append('<td class="text-center">' + employee.designation + '</td>');
        row.append('<td class="text-center">' + employee.department + '</td>');
        row.append('<td class="text-center">' + employee.branch + '</td>');
        row.append('<td class="text-center">' + employee.employeeType + '</td>');
        row.append('<td class="text-center">' + employee.joiningDate + '</td>');
        row.append('<td class="text-center">' + employee.employeeStatus + '</td>');
        

        tableBody.append(row);
    });


    initializeDataTable();
}

function initializeDataTable() {
    RosterDataTable = $("#RosterScheduleEntry-grid").DataTable({
        paging: true,
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
        lengthChange: true,
        searching: true,
        ordering: true,
        info: true,
        autoWidth: false,
        responsive: true,
        fixedHeader: false,
        scrollX: true,
        scrollCollapse: true,
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
            { targets: 'no-sort', orderable: false }
        ],
        initComplete: function () {
            hideLoading();
            $('.dataTables_filter input').css({
                'width': '250px',
                'padding': '6px 12px',
                'border': '1px solid #ddd',
                'border-radius': '4px'
            });
        }
    });
}
function getDateFromFlatPicker(id) {
    //console.log(id);
    flatpickr(id, {
        dateFormat: "Y-m-d", 
        defaultDate: new Date(),
        minDate: "today",
        disableMobile: true
    });
}


//yyyy - MM - dd
$(document).ready(function () {
    $("#rosterSchedule-check-all").on('change', function () {
        var isChecked = $(this).is(':checked');
        $('#RosterScheduleEntry-grid-body input[type="checkbox"]').prop('checked', isChecked);
    });

    $(document).on('change', '#RosterScheduleEntry-grid-body input[type="checkbox"]', function () {
        var totalCheck = $('#RosterScheduleEntry-grid-body input[type="checkbox"]').length;
        var singleCheck = $('#RosterScheduleEntry-grid-body input[type="checkbox"]:checked').length;
        if (totalCheck === singleCheck) {
            $("#rosterSchedule-check-all").prop('checked', true);
        } else {
            $("#rosterSchedule-check-all").prop('checked', false);
        }
    });
   
  
   
  
    ////todo
    //$("#FromDatePicker, #toDateRosterFrom").on('change', function () {
    //    var dateFromVal = $("#FromDatePicker").val();
    //    var dateToVal = $("#toDateRosterFrom").val();

    //    if (dateFromVal && dateToVal) {
    //        var dateFrom = new Date(dateFromVal);
    //        var dateTo = new Date(dateToVal);

    //        if (dateFrom > dateTo) {
    //            showToast("warning", "From Date cannot be greater than To Date.");
    //            $(".js-roster-schedule-entry-save").prop("disabled", true);
    //        } else {
    //            $(".js-roster-schedule-entry-save").prop("disabled", false);
    //        }
    //    }
    //});

    $(".js-roster-schedule-entry-save").click(function () {
        var rosterId = $("#hiddenRosterId").val();
        //console.log({rosterId});
        var formData = getValueRosterSchedule();

        if (rosterId == '') {

            var dateFromVal = $("#FromDatePicker").val();
            var dateToVal = $("#toDateRosterFrom").val();

            if (dateFromVal && dateToVal) {
                var dateFrom = new Date(dateFromVal);
                var dateTo = new Date(dateToVal);

                if (dateFrom > dateTo) {
                    showToast("warning", "From Date cannot be greater than To Date.");
                    return;
                }
            }

            var isByDateCheck = $("#byDate").is(':checked');
            var isByMonthCheck = $("#byMonth").is(':checked');

            if (isByDateCheck) {
                if (!formData.FromDate) {
                    $("#FromDatePicker").focus().css('border', '1px solid red');
                    return;
                }
                if (!formData.ToDate) {
                    $("#toDateRosterFrom").focus().css('border', '1px solid red');
                    return;
                }
                if (!formData.ShiftCode) {
                    $("#shiftSelect").focus().css('border', '1px solid red');
                    if ($('#shiftSelect').hasClass("select2-hidden-accessible")) {
                        $('#shiftSelect').select2('open');
                    } else {
                        $('#shiftSelect').select2().select2('open');
                    }

                    return;
                }
            }


            if (isByMonthCheck) {
                if (!formData.Month) {
                    $("#monthRosterFrom").focus().css('border', '1px solid red');
                    if ($('#monthRosterFrom').hasClass("select2-hidden-accessible")) {
                        $('#monthRosterFrom').select2('open');
                    } else {
                        $('#monthRosterFrom').select2().select2('open');
                    }
                    return;
                }
                if (!formData.year) {
                    $("#YearRosterFrom").focus().css('border', '1px solid red');
                    return;
                }
                if (!formData.ShiftCode) {
                    $("#shiftSelect").focus().css('border', '1px solid red');
                    if ($('#shiftSelect').hasClass("select2-hidden-accessible")) {
                        $('#shiftSelect').select2('open');
                    } else {
                        $('#shiftSelect').select2().select2('open');
                    }

                    return;
                }

            }
           

            $.ajax({
                url: '/RosterScheduleEntry/CreateAndUpdateRosterSchedule',
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(formData),
                beforeSend: function () {
                    showLoading();
                },
                success: function (res) {
                    //console.log("Success response:", res);
                    if (res.isSuccess) {
                        $("#rosterSchedule-check-all").prop('checked', false);
                        $('#RosterScheduleEntry-grid-body input[type="checkbox"]').prop('checked', false);
                        ClearFrom();
                        getRosterScheduleGrid();
                        showToast("success", res.isMessage);
                    }

                },
                error: function (xhr, status, error) {
                    //console.log("Error status:", status);
                },
                complete: function () {
                    hideLoading();
                }
            });
        } else {
            //console.log("click edit");
            //formData = getValueRosterSchedule();
            formData.RosterScheduleId = rosterId;
            formData.TC = $("#TcIdRosterDelete").val();
            formData.isUpdate = true;
            $.ajax({
                url: '/RosterScheduleEntry/CreateAndUpdateRosterSchedule',
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(formData),
                beforeSend: function () {
                    showLoading();
                },
                success: function (res) {
                    //console.log("Success response:", res);
                    if (res.isSuccess) {
                        $("#rosterSchedule-check-all").prop('checked', false);
                        $('#RosterScheduleEntry-grid-body input[type="checkbox"]').prop('checked', false);
                        ClearFrom();
                        getRosterScheduleGrid();
                        loadFilterEmp();
                        showToast("success", res.isMessage);
                    } else {
                        showToast("warning", res.isMessage);                       
                    }

                },
                error: function (xhr, status, error) {
                    //console.log("Error status:", status);
                },
                complete: function () {
                    hideLoading();
                }
            });
        }
        
    });

})
$(document).on('input change', '#FromDatePicker, #toDateRosterFrom, #shiftSelect #shiftSelect, #monthRosterFrom' , function () {
    $("#FromDatePicker, #toDateRosterFrom, #shiftSelect, #shiftSelect, #monthRosterFrom").css('border', '');
})
function getValueRosterSchedule() {
    var selectEmployeeIds = [];

    $('#RosterScheduleEntry-grid-body input[type="checkbox"]:checked').each(function () {
        var row = $(this).closest('tr');
        var empId = row.find('td:nth-child(2)').text().trim();
        selectEmployeeIds.push(empId);
    });
    var comId = $("#companySelect").val();
    let FromDate = "";
    let ToDate = "";
    const selectMonth = $("#monthRosterFrom").val();
    const selectYear = $("#YearRosterFrom").val();
    if (selectMonth) {
        const Month = parseInt(selectMonth);
        const Year = parseInt(selectYear);

        const firstDate = new Date(Year, Month - 1, 1);
        firstDate.setHours(12, 0, 0, 0); 

        const lastDate = new Date(Year, Month, 0);
        lastDate.setHours(12, 0, 0, 0);

        FromDate = `${firstDate.getFullYear()}-${String(firstDate.getMonth() + 1).padStart(2, '0')}-${String(firstDate.getDate()).padStart(2, '0')}`;
        ToDate = `${lastDate.getFullYear()}-${String(lastDate.getMonth() + 1).padStart(2, '0')}-${String(lastDate.getDate()).padStart(2, '0')}`;
      
    } else {
        FromDate = $("#FromDatePicker").val();
        ToDate = $("#toDateRosterFrom").val();
    }


    var formData = {
        EmployeeListID: selectEmployeeIds,
        FromDate: FromDate.toString(),
        ToDate: ToDate.toString(),
        ShiftCode: $("#shiftSelect").val(),
        Remark: $("#remarks").val(),
        Month: selectMonth,
        year: selectYear,
        CompanyCode:comId
    };

    return formData;
}


//function validation() {
//   var FromDate = $("#FromDatePicker").val();
//   var ToDate = $("#toDateRosterFrom").val();
//    var ShiftCode= $("#shiftSelect").val();
//    if (!FromDate) {
//        toastr.warning('select From Date');
//        return;
//    }
//}

function FadeInOutBydateOrMont() {
    $('#byDate').on('change', function () {
        if ($(this).is(':checked')) {
            //console.log("date");
           $("#monthRosterFrom").val('');
            $('#monthFields').fadeOut(100, function () {
                $('#dateFields').fadeIn(100).css('display', 'flex');
            });
        }
    });

    $('#byMonth').on('change', function () {
        if ($(this).is(':checked')) {
            //console.log("month");

            $('#dateFields').fadeOut(100, function () {
                $('#monthFields').fadeIn(100).css('display', 'flex');
            });
        }
    });
}


function getCurrentYear() {
    var currentYear = new Date().getFullYear();
    $("#YearRosterFrom").val(currentYear);

    //console.log(currentYear);
}

function validateYearInput() {
    var year = $("#YearRosterFrom").val();
    if (!year || isNaN(year) || year < 1900 || year > 2100) {
        //console.log("valid Year");
        $(".validYearMessage").fadeIn(500).text("Please Enter Valid Year");
        $("#YearRosterFrom").css('border', '1px solid red');
        return;
    }
    $(".validYearMessage").fadeOut(1000);
    $("#YearRosterFrom").css('border', '');
}


function AllMonth() {
    //console.log("check all month");
    $.ajax({
        url: '/RosterScheduleEntry/GetPayMonth',
        type: 'GET',
        success: function (res) {
            //console.log("Response:", res);
            if (!res || !res.isSuccess || !Array.isArray(res.data)) {
                //showToast(''); //todo
                //console.warn("Unexpected Res", res);
                return;
            }

            const $monthSelect = $('#monthRosterFrom');
            $monthSelect.empty().append('<option value="">--- Select Month ---</option>');
            res.data.forEach(x => {
                const optionHtml = `
                <option value="${x.monthId}">
                ${x.monthName}</option>
                `
                $monthSelect.append(optionHtml);
            })
        },
        error: function (xhr, status, error) {
            //console.error("Error:", status, error);
        }
    });
}

function getShift() {
    $.ajax({
        url: '/RosterScheduleEntry/getAlllShift',
        type: "GET",
        success: function (res) {
            //console.log({ res });
            if (!res || !res.isSuccess || !Array.isArray(res.data)) {
                //console.log("shift not found");
                return;
            }
            const $shiftSelect = $("#shiftSelect");
            $shiftSelect.empty().append('<option value="">--- Select Shift ---</option>');
            res.data.forEach(shift => {
                //console.log({ shift });
                const $shiftStartTime = new Date(shift.shiftStartTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: true });
                //console.log($shiftStartTime);
                const $shiftEndTime = new Date(shift.shiftEndTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: true });
                
                const optionShiftHtml = `
                 <option value="${shift.shiftCode}">
                ${shift.shiftName} (${$shiftStartTime} - ${$shiftEndTime})</option>
                `
                $shiftSelect.append(optionShiftHtml);
             
            })
        },
        error: function (e) {
            //console.log(e.message);
        }
    });
}


function getRosterScheduleGrid() {
    if ($.fn.DataTable.isDataTable('#rosterScheduleEntry-grid-show')) {
        $('#rosterScheduleEntry-grid-show').DataTable().clear().destroy();
    }

    var tableData = $('#rosterScheduleEntry-grid-show').DataTable({
        "processing": true,
        "serverSide": true,
        scrollY: "350px",
        scrollCollapse: true,
        scrollX: true,
        "ajax": {
            "url": "/RosterScheduleEntry/GetRosterScheduleGrid",
            "type": "POST",
            "dataSrc": function (json) {
                return json.data || json;
            }
        },
        "columns": [
            // First column - Roster ID with clickable link
            {
                "data": "rosterScheduleId",
                "render": function (data, type, row) {
                    return `<a data-id='${data}'>${data}</a>`;
                },
                "width": "60px"
            },
            { "data": "employeeID", "width": "100px" },
            { "data": "name", "width": "150px" },
            { "data": "designationName", "width": "120px" },
            { "data": "dateShow", "width": "80px" },
            { "data": "dayName", "width": "80px" },
            { "data": "shiftName", "width": "100px" },
            { "data": "remark", "width": "120px" },
            { "data": "luser", "width": "100px" },
        ],
        "order": [[0, 'asc']], // Changed from [[1, 'asc']] to [[0, 'asc']] since we removed column 0
        "paging": true,
        "searching": true,
        paging: true,
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
        lengthChange: true,
        ordering: true,
        info: true,
        autoWidth: false,
        responsive: false, // Changed to false for better column width control
        fixedHeader: true,
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
            { targets: 'no-sort', orderable: false },
            // Updated target indices since we removed the first column
            { "width": "60px", "targets": 0 },
            { "width": "100px", "targets": 1 },
            { "width": "150px", "targets": 2 },
            { "width": "120px", "targets": 3 },
            { "width": "80px", "targets": 4 },
            { "width": "80px", "targets": 5 },
            { "width": "100px", "targets": 6 },
            { "width": "120px", "targets": 7 },
            { "width": "100px", "targets": 8 }
        ],
        initComplete: function () {
            if (typeof hideLoading === 'function') {
                hideLoading();
            }

            $('.dataTables_filter input').css({
                'width': '250px',
                'padding': '6px 12px',
                'border': '1px solid #ddd',
                'border-radius': '4px'
            });

            // Add this to fix header/body column alignment
            $(window).resize(function () {
                $('#rosterScheduleEntry-grid-show').DataTable().columns.adjust();
            });

            // Ensure columns are properly adjusted
            setTimeout(function () {
                $('#rosterScheduleEntry-grid-show').DataTable().columns.adjust();
            }, 100);
        },
        drawCallback: function () {
            $('#rosterScheduleEntry-grid-show').DataTable().columns.adjust();
        }
    });
        
}


$(window).resize(function () {
    if ($.fn.DataTable.isDataTable('#rosterScheduleEntry-grid-show')) {
        $('#rosterScheduleEntry-grid-show').DataTable().columns.adjust();
    }
});

//$(document).ready(function () {
   

//    $("#rosterSchedule-grid-check-all").on('change', function () {
//        var isChecked = $(this).is(':checked');
//        $('#rosterScheduleEntry-grid-body-show input[type="checkbox"]').prop('checked', isChecked);
//    })

//    $(document).on('change', '#rosterScheduleEntry-grid-body-show input[type="checkbox"]', function () {
//        var total = $('#rosterScheduleEntry-grid-body-show input[type="checkbox"]').length;
//        var checked = $('#rosterScheduleEntry-grid-body-show input[type="checkbox"]:checked').length;

//        if (total === checked) {
//            $("#rosterSchedule-grid-check-all").prop('checked', true);
//        } else {
//            $("#rosterSchedule-grid-check-all").prop('checked', false);
//        }
//    });


//    $("#js-roster-schedule-entry-delete-confirm").click(function () {
//        var listRosterIds = [];
//        var employeeId = $("#TcIdRosterDelete").val();
//        $('#rosterScheduleEntry-grid-body-show input[type="checkbox"]:checked').each(
//            function () {
//                listRosterIds.push($(this).data('id'));
//            });
//        if (employeeId != '' && listRosterIds.length == 0) {
//            listRosterIds.push(parseFloat(employeeId));
//        }
//        //console.log(listRosterIds);
//        if (listRosterIds.length == 0) {
//            showToast("info", "Select at least one employee");
//            return;
//        }
//        $.ajax({
//            url: "/RosterScheduleEntry/BulkDeleteRosterEmp",
//            type: "POST",
//            data: JSON.stringify(listRosterIds),
//            traditional: true,
//            contentType: "application/json",  
//            beforeSend: function () {
//                showLoading();
//            },
//            success: function (res) {
//                //console.log(res);
//                if (res.isSuccess) {
//                    ClearFrom();
//                    showToast("success", res.message);
//                    $("#submitButton").html(`
//                    	<i class="fa fa-save">&nbsp;</i>Save
//                    `);
//                    //$("#submitButton").text("Save");
//                    $(".editDate").fadeIn(1000).text("From Date");
//                    $(".editHide").fadeIn(500);
//                    $(".editHideRosterSchedule").fadeIn(500);
//                    $("#monthFields").fadeIn(500);
//                    getRosterScheduleGrid();
//                    $("#rosterSchedule-grid-check-all").prop('checked', false);
//                    $('#rosterScheduleEntry-grid-body-show input[type="checkbox"]').prop('checked', false);
//                    var isByDateCheck = $("#byDate").is(':checked');
//                    var isByMonthCheck = $("#byMonth").is(':checked');

//                    if (isByMonthCheck) {
//                        $("#monthFields").show();
//                        $("#dateFields").hide();
//                    }
//                    if (isByDateCheck) {
//                        $("#monthFields").hide();
//                        $("#dateFields").show();
//                    }

//                }
//            },
//            erros: function (e) {
//                //console.log(e);
//                showToast("error", e.message);
//            },
//            complete: function () {
//                hideLoading();
//            }
//        })
//    })
//})
$("#js-roster-schedule-entry-clear").click(function () {
    ClearFrom();
    loadFilterEmp();
})

function ClearFrom() {
  
    $('#RosterScheduleEntry-grid-body input[type="checkbox"]').prop('disabled', false);
    $('#rosterSchedule-check-all').prop('disabled', false);
    //$("#submitButton").text("Save");
    $("#submitButton").html(`
                    	<i class="fa fa-save">&nbsp;</i>Save
                    `);
    $("#TcIdRosterDelete").val('');
    $("#hiddenRosterId").val('');
    $("#FromDatePicker").val(new Date().toISOString().split('T')[0]);
    $("#shiftSelect").val('');
    $("#remarks").val('');
    $("#toDateRosterFrom").val(new Date().toISOString().split('T')[0]);
    $("#monthRosterFrom").val('');
    $("#monthRosterFrom").val('');
    $(".editDate").fadeIn(1000).html(`
    <label for="FromDatePicker" class="col-md-12 mb-0 text-right" style="padding-right: 0px;">
        Date From
        <sup class="text-danger">
            <span style="font-size: 20px;">*</span>
        </sup>
    </label>
`);


    $(".editHide").fadeIn(500);

    $(".editHideRosterSchedule").fadeIn(500);


    var isByDateCheck = $("#byDate").is(':checked');
    var isByMonthCheck = $("#byMonth").is(':checked');

    if (isByMonthCheck) {
        $("#monthFields").show();
        $("#dateFields").hide();
    }
    if (isByDateCheck) {
        $("#monthFields").hide();
        $("#dateFields").show();
    }

    $("#rosterSchedule-check-all").prop("checked", false);
    $('#RosterScheduleEntry-grid-body input[type="checkbox"]').prop("checked", false);
    $("#rosterSchedule-grid-check-all").prop("checked", false);
    $('#rosterScheduleEntry-grid-body-show input[type="checkbox"]').prop("checked", false);
    //loadFilterEmp();
}

$(document).ready(function () {


    

    
    $(document).on('click', 'a[data-id]', function (e) {
        e.preventDefault();

       
        var isByDateCheck = $("#byDate").is(':checked');
        var isByMonthCheck = $("#byMonth").is(':checked');
        if (isByMonthCheck) {
            $(".editHide").hide();
            //console.log("click month");
            $('#dateFields').fadeIn(100).css('display', 'flex');
            //$('#editHide ').hide();
        }
        if (isByDateCheck) {
            $(".editHide").hide();
        }
        $('#RosterScheduleEntry-grid-body input[type="checkbox"]').prop('disabled', true);
        $('#rosterSchedule-check-all').prop('disabled', true);
        var id = $(this).data('id');
        $("#hiddenRosterId").val(id);
        //$("#submitButton").text("Update");
        $("#submitButton").html(`
                    	 <i class="fas fa-edit px-1"></i>Update
                    `);
        //$(".editDate").fadeIn(1000).text("Date");
        $(".editDate").fadeIn(1000).html(`
         <label for="FromDatePicker" class="col-md-12 mb-0 text-right" style="padding-right: 0px;">
        Date
        <sup class="text-danger">
            <span style="font-size: 20px;">*</span>
        </sup>
    </label>
        `);

        $(".editHideRosterSchedule").hide();
        $("#monthFields").hide();  
       
       

        //$(".empRostSelect")
        //console.log({id});
        $.ajax({
            url: "/RosterScheduleEntry/EditGetRoster",
            type: "POST",
            data: { id: id },
            traditional: true,
            success: function (res) {
                //console.log(res);
                if (res.isSuccess) {
                    $("#TcIdRosterDelete").val(res.data.tc);
                    $("#hiddenRosterId").val(res.data.rosterScheduleId);
                    $("#FromDatePicker").val(res.data.date.split('T')[0]);
                    $("#shiftSelect").val(res.data.shiftCode);
                    $("#remarks").val(res.data.remark);
                    var filterData = getAllFilterVal();
                    filterData.EmployeeIDs.push(res.data.employeeID);
                    //console.log(filterData);

                    

                   

                    $.ajax({
                        url: `/RosterScheduleEntry/getAllFilterEmp`,
                        type: "POST",
                        contentType: "application/json",
                        data: JSON.stringify(filterData),
                        beforeSend: function () {
                            showLoading();
                        },
                        success: function (res) {
                            if (!res.isSuccess) {
                                showToast('error', res.message);
                                return;
                            }
                            //console.log(res);
                            loadTableData(res);
                            $('#RosterScheduleEntry-grid-body input[type="checkbox"]').prop('checked', true);
                            //showToast('success', res.message);
                        },
                        error: function (err) {
                            //console.log(err);
                            showToast('error', 'Something went wrong');
                        },
                        complete: function () {
                            hideLoading();
                        }
                    });

                        
                

                }
                           
            },
            error: function (e) {
                //console.log(e);
            }
        })
    })
})

//upload excel file 
$(document).on('change', "#excelFileInput", function () {
    if (this.files && this.files.length > 0) {
        //console.log("click");
        $("#choosefileText").text(this.files[0].name);
    } else {
        $("#choosefileText").text('No file chosen');
    }
})

$("#excelUploadForm").submit(function (e) {
    e.preventDefault();
    var formData = new FormData(this);
    var comId = $("#companySelect").val();
    if (comId) {
        formData.append('CompanyCode', comId);
    }

    for (var pair of formData.entries()) {
        //console.log(pair[0] + ': ' + pair[1]);
    }

    $.ajax({
        url: `/RosterScheduleEntry/UploadExcel`,
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        success: function (res) {
            if (res.isSuccss) {
                $("#excelFileInput").val("");
                $("#choosefileText").text("Choose File");
                showToast('success', res.message);
                getRosterScheduleGrid();
            }
        },
        error: function (xhr, status, error) {
            $("#checkExcelFileFormate").text("Only .xlsx or .xls files are allowed").fadeIn().delay(500).fadeOut(5000);
        }
    });

});

