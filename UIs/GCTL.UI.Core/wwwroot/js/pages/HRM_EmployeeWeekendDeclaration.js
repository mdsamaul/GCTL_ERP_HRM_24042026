﻿$(document).ready(function () {
    setupLoadingOverlay();
    loadCompany();
    initializeMultiselects();
    initializeWeekendGrid();
    loadEmployeeWeekendCallAjax();
});

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

let weekendDataTable = null;

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



function initializeMultiselects() {
    $('#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect').multiselect({
        enableFiltering: true,
        includeSelectAllOption: true,
        selectAllText: 'Select All',
        nonSelectedText: 'Select Items',
        nSelectedText: 'Selected',
        allSelectedText: 'All Selected',
        filterPlaceholder: 'Search........!',
        buttonWidth: '100%',
        maxHeight: 350,
        enableClickableOptGroups: true,
        dropUp: false,
        numberDisplayed: 1,
        enableCaseInsensitiveFiltering: true
        
    });
}

function loadCompany() {
    showLoading();
    $.ajax({
        url: "/EmployeeWeekendDeclaration/GetAllCompany",
        type: "GET",
        success: function (e) {
            var optCompany = $("#companySelect");
            optCompany.empty();
            //optCompany.append("<option value=''>Select Company</option>");

            $.each(e.data, function (index, company) {
                optCompany.append(`<option value=${company.companyCode}>${company.companyName}</option>`);
            });

            if (e.data.length > 0) {
                var defaultCompanyCode = e.data[0].companyCode;
                optCompany.val(defaultCompanyCode);
            }

            optCompany.multiselect('rebuild');

            if (e.data.length > 0) {
                var defaultCompanyCode = e.data[0].companyCode;
                optCompany.multiselect('select', defaultCompanyCode);
            }

            var optStatus = $("#activityStatusSelect");
            optStatus.multiselect({
                buttonWidth: '185px'
            });
            if (optStatus.find("option").length > 0) {
                var defaultActiveVal = optStatus.find("option").filter(function () {
                    return $(this).text().toLowerCase() === "active";
                }).val();

                if (defaultActiveVal) {
                    optStatus.multiselect('select', defaultActiveVal);
                }
            }

            loadAllFilterEmp();
        },
        error: function (xhr, status, error) {
            console.error("Error loading companies:", error);
            hideLoading();
        }
    });
}

$("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect, #employeeStatus, #activityStatusSelect")
    .on("change", function () {
        loadAllFilterEmp();
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

function toArray(value) {
    if (!value) return [];
    if (Array.isArray(value)) return value;
    return [value]; 
}

function loadAllFilterEmp() {
    showLoading();
    var filterData = getAllFilterVal();

    $.ajax({
        url: `/EmployeeWeekendDeclaration/getFilterEmp`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(filterData),
        success: function (res) {
            $("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect, #employeeStatus, #activityStatusSelect")
                .off("change");

            loadTableData(res);
            const data = res.data;

           
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
                    loadAllFilterEmp();
                });

        },
        complete: function () {
            hideLoading();
        },
        error: function (xhr, status, error) {
            console.error("Error loading filtered employees:", error);
            hideLoading();
        }
    });
}

function loadTableData(res) {
    var tableDataItem = res.data.employees;

    if (weekendDataTable !== null) {
        weekendDataTable.destroy();
    }

    var tableBody = $("#employee-weekend-grid-body");
    tableBody.empty();

    $.each(tableDataItem, function (index, employee) {
        var row = $('<tr>');
        row.append('<td class="text-center" style="width:60px !important;"><input type="checkbox" /></td >');
        row.append('<td class="text-center">' + employee.code + '</td>');
        row.append('<td class="text-center">' + employee.name + '</td>');
        row.append('<td class="text-center">' + employee.designationName + '</td>');
        row.append('<td class="text-center">' + employee.departmentName + '</td>');
        row.append('<td class="text-center">' + employee.branchName + '</td>');
        row.append('<td class="text-center">' + employee.companyName + '</td>');
        row.append('<td class="text-center">' + employee.empTypeName + '</td>');
        row.append('<td class="text-center">' + employee.joiningDate + '</td>');
        row.append('<td class="text-center">' + employee.employeeStatus + '</td>');

        tableBody.append(row);
    });

  
    initializeDataTable(); 
}

function initializeDataTable() {
    setTimeout(function () {
        weekendDataTable = $("#employee-weekend-grid").DataTable({
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
    }, 50);
   
}

function initializeWeekendGrid() {
    showLoading();
    setTimeout(function () {
        if (weekendDataTable !== null) {
            weekendDataTable.destroy();
        }
        initializeDataTable();
    }, 500);
}


$(document).ready(function () {

    let selectedDates = [];
    let fp = null;

    function initFlatpickr(preselectedDates = []) {
        const datepickerInput = document.getElementById("datepicker");
        console.log(datepickerInput);

        if (!datepickerInput) {
            console.warn("datepicker element not found");
            return;
        }

        // Destroy previous flatpickr instance if exists
        if (datepickerInput._flatpickr) {
            try {
                datepickerInput._flatpickr.destroy();
            } catch (e) {
                console.log("Error destroying _flatpickr instance:", e);
            }
        }

        // Create new flatpickr instance
        fp = flatpickr(datepickerInput, {
            mode: "multiple",
            dateFormat: "Y-m-d",
            defaultDate: preselectedDates,
            onChange: function (selectedDatesObj) {
                selectedDates = selectedDatesObj.map(d => d.toISOString().split('T')[0]);
            }
        });
    }

    // Clear form safely
    function clearFormSafely() {
        try {
            selectedDates = [];
            $("#remark").val("");
            $('#employee-weekend-grid-body input[type="checkbox"]').prop('checked', false);
            $("#employee-check-all").prop('checked', false);

            const datepickerInput = document.getElementById("datepicker");
            if (datepickerInput && datepickerInput._flatpickr) {
                try {
                    datepickerInput._flatpickr.destroy();
                } catch (e) {
                    console.log("Error clearing _flatpickr:", e);
                }
            }
            $("#datepicker").val(""); 
            initFlatpickr([]);
        } catch (e) {
            console.error("Error in clearFormSafely:", e);
        }
    }

    // Initialize flatpickr initially
    initFlatpickr();

    // Select/Deselect all employees
    $("#employee-check-all").on('change', function () {
        var isChecked = $(this).is(':checked');
        $('#employee-weekend-grid-body input[type="checkbox"]').prop('checked', isChecked);
    });

    // Save or Update
    $("#submitButton").on('click', function () {
        var hId = $("#hiddenEWDId").val();

        if (hId === '') {
            // New Entry
            var selectedEmployeeIds = [];
            $('#employee-weekend-grid-body input[type="checkbox"]:checked').each(function () {
                var row = $(this).closest('tr');
                var empId = row.find('td:nth-child(2)').text().trim();
                selectedEmployeeIds.push(empId);
            });
            if (selectedDates.length === 0) {
                $("#massageError").html(`<i class="fa fa-times-circle text-danger  px-2"></i>` + "Please select at least one date.").fadeIn().delay(1000).fadeOut(10000);
                return;
            }

            if (selectedEmployeeIds.length === 0) {
                $("#massageError").html(`<i class="fa fa-times-circle text-danger  px-2"></i>` + "Please select at least one employee.").fadeIn().delay(1000).fadeOut(10000);
                return;
            }

            var remark = $("#remark").val();
            var comId = $("#companySelect").val();

            var dataToSend = {
                WeekendDates: selectedDates,
                WeekendEmployeeIds: selectedEmployeeIds,
                Remark: remark,
                CompanyCode: comId
            };

            $.ajax({
                url: `/EmployeeWeekendDeclaration/SaveSelectedDatesAndEmployees`,
                type: "POST",
                data: JSON.stringify(dataToSend),
                contentType: "application/json; charset=utf-8",
                success: function (response) {                
                    $("#massageSeccess").html(`<i class="fa fa-check-circle text-success mx-3"></i>`+response.message).fadeIn().delay(500).fadeOut(5000);
                    loadEmployeeWeekendCallAjax();
                    clearFormSafely();
                },
                error: function (xhr, status, error) {
                    $("#massageError").html(`<i class="fa fa-times-circle text-danger  px-2"></i>`+error.message).fadeIn().delay(500).fadeOut(5000);
                }
            });

        } else {
            // Update
            var weekendDate = $("#datepicker").val();
            var remark = $("#remark").val();

            $.ajax({
                url: `/EmployeeWeekendDeclaration/UpdateEmpWeekDec`,
                type: 'POST',
                data: {
                    Id: hId,
                    WeekendDate: weekendDate,
                    Remarks: remark
                },
                success: function (res) {                  
                    $("#massageUpdate").html(`<i class="fa fa-sync-alt text-primary px-2 "></i>` + res.message).fadeIn().delay(500).fadeOut(5000);
                    loadEmployeeWeekendCallAjax();
                    $("#submitButton").text("Save");
                    $("#hiddenEWDId").val("");
                    clearFormSafely();
                },
                error: function (error) {
                    $("#massageError").html(`<i class="fa fa-times-circle text-danger  px-2"></i>` + error.message).fadeIn().delay(500).fadeOut(5000);
                }
            });
        }
    });

    // Function to edit an existing entry
    window.editEmployeeWeekendDeclaration = function (id, weekendDate, remarks) {
        try {
            $("#hiddenEWDId").val(id);
            $("#submitButton").text("Update");
            $("#remark").val(remarks);

            let dates = [];
            if (typeof weekendDate === 'string') {
                dates = [weekendDate];
            } else if (Array.isArray(weekendDate)) {
                dates = weekendDate;
            }

            selectedDates = dates;
            initFlatpickr(dates);

            const $datepicker = $("#datepicker");
            if ($datepicker.length) {
                $('html, body').animate({
                    scrollTop: $datepicker.offset().top - 100
                }, 500);
            } else {
                $("#massageError").html(`<i class="fa fa-times-circle text-danger  px-2"></i>` + "#datepicker element not found, cannot scroll").fadeIn().delay(500).fadeOut(5000);
            }

        } catch (e) {
            $("#massageError").html(`<i class="fa fa-times-circle text-danger  px-2"></i>` + e).fadeIn().delay(500).fadeOut(5000);
        }
    };

    var listWekEmp = [];
    $(document).on('click', '.empRow', function () {
        var tcId = $(this).data('id');
        listWekEmp.push(tcId);
    });

    $("#employee-weekend-check-all").on('change', function () {
        var isChecked = $(this).is(':checked');
        $('#employee-weekend-grid-body-show input[type="checkbox"]').prop('checked', isChecked);
    })
    $("#js-emp-weekend-dec-delete-confirm").click(function () {
       

        $('#employee-weekend-grid-body-show input[type="checkbox"]:checked').each(function () {
            listWekEmp.push($(this).data('id'));
        });
        if (listWekEmp.length == 0) {
            alert("select at last one weekend employee");
            return;
        }
        console.log(listWekEmp);
        $.ajax({
            url: `/EmployeeWeekendDeclaration/BulkDeleteEmpWeelend`,
            type: "POST",
            data: { ids: listWekEmp },
            traditional: true,
            success: function (res) {
                $("#massageSeccess").html(`<i class="fa fa-trash text-danger  px-2"></i>` + res.message).fadeIn().delay(500).fadeOut(5000);
                loadEmployeeWeekendCallAjax();
                $("#submitButton").text("Save");
                $("#hiddenEWDId").val("");
                clearFormSafely();
            },
            error: function (error) {
                $("#massageError").html(`<i class="fa fa-times-circle text-danger  px-2"></i>` + error.message).fadeIn().delay(500).fadeOut(5000);
              
            }
        })
    })
});

function loadEmployeeWeekendCallAjax() {

    $.ajax({

        url: `/EmployeeWeekendDeclaration/GetWeekendEmployeeDeclaration`,

        type: "GET",

        contentType: "application/json",

        success: function (res) {
            loadEmployeeWeekend(res);
        },

        error: function (error) {
            //$("#massageError").html(`<i class="fa fa-times-circle text-danger  px-2"></i>` + error.message).fadeIn().delay(500).fadeOut(5000);        

        }

    });

}

function loadEmployeeWeekend(res) {

    var tableDataItem = res.data;

    if ($.fn.DataTable.isDataTable('#employee-weekend-grid-show')) {
        $('#employee-weekend-grid-show').DataTable().clear().destroy();
    }

    var tableBody = $("#employee-weekend-grid-body-show");
    tableBody.empty();
    $.each(tableDataItem.result, function (index, employee) {
        var row = $(`<tr class="empRow" data-id="${employee.tc}">`);
        row.append(`<td class="text-center" width="50"><input class="empWekSelect" type="checkbox" data-id="${employee.tc}" /></td>`);
        row.append(`<td class="text-center"><a data-id='${employee.id}'>` + employee.id + '</a></td>');
        row.append('<td class="text-center">' + employee.empID + '</td>');
        row.append('<td class="text-start">' + employee.name + '</td>');
        row.append('<td class="text-start">' + employee.designation + '</td>');
        row.append('<td class="text-center">' + employee.weekendDate + '</td>');
        row.append('<td class="text-center">' + employee.day + '</td>');
        row.append('<td class="text-center">' + employee.remarks + '</td>');
        row.append('<td class="text-center">' + employee.entryUser + '</td>');
        tableBody.append(row);

    });

    setTimeout(function () {
        $('#employee-weekend-grid-show').DataTable({
            responsive: true,
            paging: true,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100, 1000, -1], [10, 25, 50, 100, 1000, "All"]],
            lengthChange: true,
            searching: true,
            ordering: true,
            info: true,
            autoWidth: false,
            fixedHeader: true, 
            scrollY: '350px',  
            scrollCollapse: true,
            language: {
                search: "🔍 Search:",
                searchPlaceholder: "Search here...",
                lengthMenu: "Show _MENU_ entries",
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
                { width: "50px", targets: 0 } 
            ]
        });
    }, 50);

    

}

$("#js-emp-weekend-dec-clear").click(function () {

    location.reload();
});

$(document).ready(function () {
    $(document).on('click', 'a[data-id]', function (e) {
        e.preventDefault();
        var id = $(this).data('id');        
        $("#submitButton").text("Update");
        $.ajax({
            url: `/EmployeeWeekendDeclaration/editEmpWeekDec`,
            type: "POST",
            data: { id: id },
            traditional: true,
            success: function (res) {
                //console.log(res);
                $('#datepicker').flatpickr().setDate(res.weekendDate);
                $('#remark').val(res.remarks);
                $('#hiddenEWDId').val(res.id);
                loadAllFilterEmp();
            }, error: function (error) {
                console.log(error);
            }
        })
    })
})
$(document).ready(function () {
    $('#excelFileInput').on('change', function () {
        if (this.files && this.files.length > 0) {
            $("#choosefileText").text(this.files[0].name);
        } else {
            $("#choosefileText").text('No file chosen');
        }
    });
});

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
        url: `/EmployeeWeekendDeclaration/UploadExcelAsync`,
        type: "POST",
        data: formData,
        contentType: false,
        processData: false,
        success: function (res) {
            if (res.isSuccess) {
                $("#choosefileText").text("Choose File");                
                $("#massageUpdate").html(`<i class="fa fa-sync-alt text-primary px-2"></i>` + "Excel data imported successfully").fadeIn().delay(500).fadeOut(5000);

                if (typeof loadEmployeeWeekendCallAjax === 'function') {
                    loadEmployeeWeekendCallAjax();
                }
            } else {
                $("#checkFileFormate").html(`<i class="fa fa-times-circle text-danger  px-2"></i>`+res.message).fadeIn().delay(500).fadeOut(5000);
            }
        },
        error: function (xhr, status, error) {
            $("#checkFileFormate").text("Only .xlsx or .xls files are allowed").fadeIn().delay(500).fadeOut(5000);
        }
    });

});