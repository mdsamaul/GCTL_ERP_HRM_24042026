﻿(function ($) {
    $.officialInfo = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: "/",
            formSelector: "#officialInfo-form",
            gridSelector: "#officialInfo-grid",
            gridContainer: ".js-officialInfo-grid-container",
            editSelector: ".js-officialInfo-edit",
            saveSelector: ".js-officialInfo-save",
            selectAllSelector: "#officialInfo-check-all",
            deleteSelector: ".js-officialInfo-delete-confirm",
            deleteModal: "#officialInfo-delete-modal",
            finalDeleteSelector: ".js-officialInfo-delete",
            clearSelector: ".js-officialInfo-clear",
            topSelector: ".js-go",
            showNagativeFormat: false,
            haseFile: false,
            quickAddSelector: ".js-quick-add",
            quickAddModal: "#quickAddModal",
            load: function () {

            }
        }, options);


        var gridUrl = settings.baseUrl + "/GetAll";
        var saveUrl = settings.baseUrl + "/setup";
        var deleteUrl = settings.baseUrl + "/Delete";
        var selectedItems = [];
        $(() => {
            initialize();

            $('#EmployeeId').on('change', function () {
                var selectedEmployee = $(this).val();

                $.ajax({
                    url: '/OfficialInfo/GetEmployeeDetailsByCode',
                    type: 'GET',
                    data: { code: selectedEmployee },
                    success: function (data) {
                        if (data) {
                            $('#DesignationName').val(data.designationName || '');
                            $('#DepartmentName').val(data.departmentName || '');
                            $('#FullName').val(data.fullName || '');
                            if (data.isSuccess) {
                                toastr.info(data.message);
                            }
                        } else {
                            $('#DesignationName').val('');
                            $('#DepartmentName').val('');
                            $('#FullName').val('');
                        }
                    },
                    error: function () {
                        console.error('Failed to fetch employee details.');
                    }
                });
            });



            

            $('body').on('click', settings.saveSelector, function () {

                validation();
                validateEmployeeDD();
                validateCompanyCodeDD();
                validateDepartmentCodeDD();
                validateDesignationCodeDD();
                isExistsByCode();
                $(settings.formSelector).submit();
            });

            $('body').on('submit', settings.formSelector, function (e) {
                e.preventDefault();
                

                var form = $(this)[0];
                var formData = new FormData(form);
                var actionUrl = $(this).attr('action');

                $.ajax({
                    type: 'POST',
                    url: actionUrl,
                    data: formData,
                    contentType: false,
                    processData: false,
                    success: function (result) {
                        if (result.noSavePermission || result.noUpdatePermission || result.isDuplicate) {
                            toastr.info(result.message);
                        }
                        else if (result.isSuccess) {
                            /*$(settings.gridContainer).html(result.html);*/
                            /*initialize();*/
                            toastr.success(result.message, 'Success');
                            setTimeout(function () {
                                window.location.href = result.redirectUrl;
                            }, 1000);

                        } else {
                            $(settings.formSelector).html(result);
                            /*initialize();*/
                        }
                    },
                    error: function () {
                        toastr.error('Failed Insert.');
                    }
                });
            });

            function validation() {
                var employee = $('#EmployeeId').val();
                var company = $('#CompanyCode').val();
                var department = $('#DepartmentCode').val();
                var designation = $('#DesignationCode').val();

                if (!employee) {
                    toastr.info('Select Employee');
                    $('#EmployeeId').select2('open');
                    return false;
                }

                if (!company) {
                    toastr.info('Select Company');
                    $('#CompanyCode').select2('open');
                    return false;
                }

                if (!department) {
                    toastr.info('Select Department');
                    $('#DepartmentCode').select2('open');
                    return false;
                }

                if (!designation) {
                    toastr.info('Select Designation');
                    $('#DesignationCode').select2('open');
                    return false;
                }
            }

            $('#EmployeeId').on('change blur', function () {
                validateEmployeeDD();
            });

            $('#CompanyCode').on('change blur', function () {
                validateCompanyCodeDD();
            });

            $('#DepartmentCode').on('change blur', function () {
                validateDepartmentCodeDD();
            });

            $('#DesignationCode').on('change blur', function () {
                validateDesignationCodeDD();
            });

            function validateEmployeeDD() {
                if ($('#EmployeeId').val().trim() == '') {
                    $('#EmployeeId').next('.select2-container').css('border', '1px solid red');
                } else {
                    $('#EmployeeId').next('.select2-container').css('border', '');
                }
            }

            function validateCompanyCodeDD() {
                if ($('#CompanyCode').val().trim() == '') {
                    $('#CompanyCode').next('.select2-container').css('border', '1px solid red');
                } else {
                    $('#CompanyCode').next('.select2-container').css('border', '');
                }
            }

            function validateDepartmentCodeDD() {
                if ($('#DepartmentCode').val().trim() == '') {
                    $('#DepartmentCode').next('.select2-container').css('border', '1px solid red');
                } else {
                    $('#DepartmentCode').next('.select2-container').css('border', '');
                }
            }

            function validateDesignationCodeDD() {
                if ($('#DesignationCode').val().trim() == '') {
                    $('#DesignationCode').next('.select2-container').css('border', '1px solid red');
                } else {
                    $('#DesignationCode').next('.select2-container').css('border', '');
                }
            }






            $('body').on('click', settings.editSelector, function () {
                var id = $(this).data('id');

                $.get('/OfficialInfo/Setup', { id: id }, function (result) {
                    console.log(result);
                    $(settings.formSelector).html($(result).find(settings.formSelector).html());
                    $(settings.formSelector).attr('action', 'OfficialInfo/Setup');
                    $('.officialInfoDatepicker').datepicker();
                }).fail(function () {
                    toastr.error('Failed Update!');
                });
            });






            $("body").on('click', settings.selectAllSelector, function () {
                $('.checkBox').prop('checked', $(this).prop('checked'));
            });

            // Delete confirmation
            $("body").on('click', settings.deleteSelector, function (e) {
                e.preventDefault();
                var selectedIds = [];

                $('.checkBox:checked').each(function () {
                    selectedIds.push($(this).val());
                });

                if (selectedIds.length === 0) {
                    toastr.error('Please select employee to delete.');
                    return;
                }

                $(settings.deleteModal + ' ' + settings.finalDeleteSelector).data('ids', selectedIds);
                $(settings.deleteModal).modal('show');
            });

            // Final delete action
            $("body").on('click', settings.finalDeleteSelector, function (e) {
                e.preventDefault();
                var selectedIds = $(this).data('ids');

                $.ajax({
                    type: 'POST',
                    url: '/OfficialInfo/Delete',
                    contentType: 'application/json',
                    data: JSON.stringify(selectedIds),
                    success: function (result) {
                        if (result.isSuccess) {

                            loadTableData();
                            toastr.success(result.message);
                            $('.checkBox').prop('checked', false);

                        } else {

                            toastr.error(result.message, 'Error');
                        }
                        $(settings.deleteModal).modal('hide');
                    },
                    error: function (xhr) {
                        toastr.error('An error.');
                        $(settings.deleteModal).modal('hide');
                    }
                });
            });






            $('.js-officialInfo-clear').on('click', function () {
                clear();
            });

            function clear() {
                $('#officialInfo-form')[0].reset();
                $('#AutoId').val('0');
                $(".selectpickerOfficialInfo").val(null).trigger("change");
                $('.officialInfoDatepicker').val('');
                $('#LdateModifyDate').hide();
                $(settings.saveSelector).html('<i class="fas fa-save"></i> Save');
                $('.text-danger').text('');
                $('.selectpickerOfficialInfo').next('.select2-container').css('border', '');
                // Update the URL
                history.replaceState(null, '', '/OfficialInfo/Setup');
                // Handle Company Dropdown
                var companyOptions = $('#CompanyCode option');
                if (companyOptions.length === 2) { // Includes default "---- Select Company ----"
                    $('#CompanyCode').val(companyOptions.eq(1).val()).trigger("change"); // Select the only available company
                } else {
                    $('#CompanyCode').val('').trigger("change"); // Clear selection
                }
            }






            let loadUrl,
                target,
                reloadUrl,
                title,
                lastCode;
            // Quick add
            $("body").on("click", settings.quickAddSelector, function (e) {
                e.stopPropagation();
                e.preventDefault();
                e.stopImmediatePropagation();

                loadUrl = $(this).data("url");
                target = $(this).data("target");
                reloadUrl = $(this).data("reload-url");
                title = $(this).data("title");

                $(settings.quickAddModal + " .modal-title").html(title);
                $(settings.quickAddModal + " .modal-body").empty();

                $(settings.quickAddModal + " .modal-body").load(loadUrl, function () {
                    $(settings.quickAddModal).modal("show");
                    $("#header").hide();
                    $(settings.quickAddModal + " .modal-body #header").hide()

                    $("#left_menu").hide();
                    $(settings.quickAddModal + " .modal-body #left_menu").hide()

                    $("#main-content").toggleClass("collapse-main");
                    $(settings.quickAddModal + " .modal-body #main-content").toggleClass("collapse-main")

                    $("body").removeClass("sidebar-mini");
                })
            });

            $("body").on("click", ".js-modal-dismiss", function () {
                $("body").removeClass("sidebar-mini").addClass("sidebar-mini");

                $("#header").show();
                $(settings.quickAddModal + " .modal-body #header").show()

                $("#left_menu").show();

                $(settings.quickAddModal + " .modal-body #left_menu").show()

                $("#main-content").toggleClass("collapse-main");
                $(settings.quickAddModal + " .modal-body #main-content").toggleClass("collapse-main")


                lastCode = $(settings.quickAddModal + " #lastCode").val();

                $(settings.quickAddModal + " .modal-body").empty();
                $(settings.quickAddModal).modal("hide");


                $(target).empty("");
                $(target).append($('<option>', {
                    value: '',
                    text: `--Select ${title}--`
                }));
                $.ajax({
                    url: reloadUrl,
                    method: "GET",
                    success: function (response) {
                        console.log(response);
                        $.each(response, function (i, item) {
                            $(target).append($('<option>', {
                                value: item.code,
                                text: item.name
                            }));
                        });
                        console.log("Test", lastCode);
                        $(target).val(lastCode);
                    }
                });
            });
        });





        function initialize() {

            loadTableData();

            $('.selectpickerOfficialInfo').select2({
                language: {
                    noResults: function () {

                    }
                },
                escapeMarkup: function (markup) {
                    return markup;
                }
            });

            $('.officialInfoDatepicker').datepicker({
                dateFormat: 'dd/mm/yy',
                changeMonth: true,
                changeYear: true,
                yearRange: '-100:+10',
                //maxDate: '100Y', 
                //showAnim: 'fadeIn',
                //showButtonPanel: true, 
                //defaultDate: '+1w', 
                //currentText: 'Today', 
                //firstDay: 1, 
                //weekHeader: 'Wk', 
            });

            $('.timepicker').datetimepicker({
                format: 'hh:mm A',
                /*showTodayButton: true,*/
                // Your Icons
                // as Bootstrap 4 is not using Glyphicons anymore
                icons: {
                    time: 'fas fa-clock',
                    date: 'fas fa-calendar',
                    up: 'fas fa-chevron-up',
                    down: 'fas fa-chevron-down',
                    previous: 'fas fa-chevron-left',
                    next: 'fas fa-chevron-right',
                    today: 'fas fa-check',
                    clear: 'fas fa-trash',
                    close: 'fas fa-times'
                }
            });

            $('.datetimepicker').datetimepicker({
                format: 'DD/MM/YYYY hh:mm A',
                /*showTodayButton: true,*/
                // Your Icons
                // as Bootstrap 4 is not using Glyphicons anymore
                icons: {
                    time: 'fas fa-clock',
                    date: 'fas fa-calendar',
                    up: 'fas fa-chevron-up',
                    down: 'fas fa-chevron-down',
                    previous: 'fas fa-chevron-left',
                    next: 'fas fa-chevron-right',
                    today: 'fas fa-check',
                    clear: 'fas fa-trash',
                    close: 'fas fa-times'
                }
            });
        }





        function isExistsByCode() {
            $('#EmployeeId').on('change', function () {
                var code = $('#EmployeeId').val();

                $.ajax({
                    method: 'GET',
                    data: { code: code },
                    url: '/OfficialInfo/IsExistsByCode',
                    success: function (response) {
                        if (response.isSuccess) {
                            toastr.info(response.message);
                        }
                    }
                });
            });
        }






        function loadTableData() {
            $.ajax({
                type: 'GET',
                url: gridUrl,
                success: function (data) {
                    $(settings.gridContainer).html(data);
                    dataTable();
                },
                error: function () {
                    toastr.error('Failed Load Data');
                }
            });
        }

        // Data table initialization function
        function dataTable() {
            $(settings.gridSelector).DataTable({
                responsive: true,
                pageLength: 15,
                destroy: true,
                lengthMenu: [15, 50, 75, 100],
                order: [[1, "DESC"]],
            });
        }
    }

}(jQuery));

