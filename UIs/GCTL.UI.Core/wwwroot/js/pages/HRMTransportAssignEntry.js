(function ($) {
    $.HRMTransportAssignEntryJs = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            TransportTypeId: "#TransportTypeId",
            TransportNoId: "#TransportNoId",
            DriverSelectEmpId: "#DriverSelectEmpId",
            EffectiveDate: "#EffectiveDate",
            TransportUser: "#UserSelectEmpId",
            Active: "#Active",
            TransportAssignEntryId: "#TAID",
            AutoId: "#AutoId",
            RowCheckbox: ".row-checkbox",
            SelectedAll: "#selectAll",
            EditBrn: ".btn-transport-entry-edit",
            VehicleTypeSaveBtn: ".js-transport-entry-assign-save",
            DeleteBtn: "#js-transport-entry-assign-delete-confirm",
            UpdateDate: ".updateDate",
            CreateDate: ".createDate",
            ClearBrn: "#js-transport-entry-assign-clear",
            //DriverSelectEmpId: "#DriverSelectEmpId",
            DEmpName:"#DEmpName",
            DEmpDesignation:"#DEmpDesignation",
            DEmpDepartment:"#DEmpDepartment",
            DEmpPhone:"#DEmpPhone",
        }, options);

        var loadVehicleTypeDataUrl = commonName.baseUrl + "/LoadData";
        var autoIdUrl = commonName.baseUrl + "/AutoId";
        var CreateUpdateUrl = commonName.baseUrl + "/CreateUpdate";
        var PopulatedDataForUpdateUrl = commonName.baseUrl + "/PopulatedDataForUpdate";
        var deleteUrl = commonName.baseUrl + "/deleteTransport";
        var alreadyExistUrl = commonName.baseUrl + "/alreadyExist";
        var LoadEmpDetailsUrl = commonName.baseUrl + "/GetEmpDetailsId"; 
        var transportTypeUrl = commonName.baseUrl + "/transportTypeGetByTransportNo"; 
        // Sticky header on scroll
        function stHeader() {
            window.addEventListener('scroll', function () {
                const header = document.getElementById('stickyHeader');
                if (window.scrollY > 10) {
                    header.classList.add('scrolled');
                } else {
                    header.classList.remove('scrolled');
                }
            });
        }

        // SweetAlert toast message
        function showToast(iconType, message) {
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

        //$('.searchable-select').select2({
        //    //placeholder: 'Select an option',
        //    allowClear: false,
        //    width: '100%'
        //});

        //function UserSelectEmpMultiselect() {
        //    $('#DriverSelectEmpId').multiselect({
        //        includeSelectAllOption: true,       // Select All checkbox
        //        enableFiltering: true,              // Search box
        //        enableCaseInsensitiveFiltering: true,
        //        buttonWidth: '100%',                // Dropdown width
        //        maxHeight: 300,                     // Scroll height
        //        nonSelectedText: '--Select Employee--',
        //        allSelectedText: 'All Employees Selected',
        //        nSelectedText: 'Employees Selected'
        //    });
        //}
        $(document).ready(function () {
            // Wait a bit for multiselect to render buttons
            setTimeout(function () {
                // Clear filter button icon replace
                $('.multiselect-clear-filter i').removeClass('glyphicon glyphicon-remove-circle')
                    .addClass('fa fa-times-circle'); // Font Awesome icon
            }, 200);
        });

        //function UserSelectEmpMultiselect() {
        //    // Check if multiselect plugin is available
        //    if (typeof $.fn.multiselect === 'undefined') {
        //        console.error('Bootstrap Multiselect plugin is not loaded!');
        //        return;
        //    }
        //    try {

        //        // Initialize Transport User multiselect if element exists
        //        if ($(commonName.DriverSelectEmpId).length) {
        //            $(commonName.DriverSelectEmpId).multiselect({
        //                includeSelectAllOption: true,
        //                selectAllText: 'Select All Users',
        //                enableFiltering: true,
        //                enableCaseInsensitiveFiltering: true,
        //                filterPlaceholder: 'Search users...',
        //                buttonWidth: '100%',
        //                maxHeight: 300,
        //                nonSelectedText: '--Select Users--',
        //                allSelectedText: 'All Users Selected',
        //                nSelectedText: ' users selected',
        //                buttonClass: 'btn btn-outline-secondary'
        //            });
        //        }

        //        console.log('Multiselect initialized successfully');
        //    } catch (error) {
        //        console.error('Error initializing multiselect:', error);
        //    }
        //}
        let UserEmplist = [];
        function DriverSelectEmpId() {
            var $dropdown = $('#DriverSelectEmpId');
            if ($dropdown.length && typeof $.fn.multiselect !== 'undefined') {
                if (!$dropdown.data('multiselect')) { // যদি আগে initialized না হয়
                    $dropdown.multiselect({
                        enableFiltering: true,
                        enableCaseInsensitiveFiltering: true,
                        filterPlaceholder: 'Search for an employee...',
                        buttonWidth: '100%',
                        maxHeight: 300,
                        nonSelectedText: '--Select Employee--',
                        buttonClass: 'btn btn-outline-secondary text-start',
                        onChange: function (option, checked) {
                            let selectedValue = $dropdown.val();
                            console.log('Selected Employee ID:', selectedValue);
                        }
                    });
                    console.log('Driver multiselect initialized');
                }
            }
        }
        function TransportNoList() {
            var $dropdown = $(commonName.TransportNoId);
            if ($dropdown.length && typeof $.fn.multiselect !== 'undefined') {
                if (!$dropdown.data('multiselect')) { // যদি আগে initialized না হয়
                    $dropdown.multiselect({
                        enableFiltering: true,
                        enableCaseInsensitiveFiltering: true,
                        filterPlaceholder: 'Search for an employee...',
                        buttonWidth: '100%',
                        maxHeight: 300,
                        nonSelectedText: '--Select Employee--',
                        buttonClass: 'btn btn-outline-secondary text-start',
                        onChange: function (option, checked) {
                            let selectedValue = $dropdown.val();
                            console.log('Selected Employee ID:', selectedValue);
                        }
                    });
                    console.log('Driver multiselect initialized');
                }
            }
        }

        function UserSelectEmpMultiselect() {
            if (typeof $.fn.multiselect === 'undefined') {
                console.error('Bootstrap Multiselect plugin is not loaded!');
                return;
            }

            try {
                if ($('#UserSelectEmpId').length) {
                    $('#UserSelectEmpId').multiselect({
                        includeSelectAllOption: true,
                        selectAllText: 'Select All Users',
                        enableFiltering: true,
                        enableCaseInsensitiveFiltering: true,
                        filterPlaceholder: 'Search users...',
                        buttonWidth: '100%',
                        maxHeight: 300,
                        nonSelectedText: '--Select Users--',
                        allSelectedText: 'All Users Selected',
                        nSelectedText: ' users selected',
                        buttonClass: 'btn btn-outline-secondary',

                        // onChange event
                        onChange: function (option, checked) {
                            let value = option.val(); // single value
                            console.log('Option ID:', value, 'Checked:', checked);

                            if (checked) {
                                if (!UserEmplist.includes(value)) {
                                    UserEmplist.push(value);
                                }
                            } else {
                                UserEmplist = UserEmplist.filter(id => id !== value);
                            }

                            console.log('Updated UserEmplist:', UserEmplist);
                        }
                    });
                }

                console.log('Multiselect initialized successfully');
            } catch (error) {
                console.error('Error initializing multiselect:', error);
            }
        }



   
        //function loadEmployeeDetails(empId) {
        //    $.ajax({
        //        url: LoadEmpDetailsUrl,
        //        type: "POST",
        //        contentType: 'application/json',
        //        data: JSON.stringify(empId),
        //        success: function (res) {
        //            $(commonName.DEmpName).text(res.data?.empName);
        //            $(commonName.DEmpDepartment).text(res.data?.department);
        //            $(commonName.DEmpDesignation).text(res.data?.designation);
        //            $(commonName.DEmpPhone).text(res.data?.phone);
        //        },
        //        error: function (e) {
        //            console.error('Error loading employee details:', e);
        //        }
        //    });
        //}

        //function clearEmployeeDetails() {
        //    $(commonName.DEmpName).text("");
        //    $(commonName.DEmpDepartment).text("");
        //    $(commonName.DEmpDesignation).text("");
        //    $(commonName.DEmpPhone).text("");
        //}

        const effectiveDatePicker = flatpickr("input[name='EffectiveDate']", {
            altInput: true,
            altFormat: "d/m/Y",
            dateFormat: "Y-m-d",
            allowInput: true,
            defaultDate: "today"
        });

        function isValidDate(dateStr) {
            const date = Date.parse(dateStr);
            return !isNaN(date);
        }

        $(commonName.DriverSelectEmpId).on('change', function () {
            var selectedValue = $(this).val();
            $.ajax({
                url: LoadEmpDetailsUrl,
                type: "POST",
                contentType:'application/json',
                data: JSON.stringify(selectedValue ),
                success: function (res) {                   
                        $(commonName.DEmpName).text(res.data?.empName);
                        $(commonName.DEmpDepartment).text(res.data?.department);
                        $(commonName.DEmpDesignation).text(res.data?.designation);
                        $(commonName.DEmpPhone).text(res.data?.phone);
                   
                   
                }, error: function (e) {
                }
            })
        })
        autoTransportAssignEntryId = function () {
            $.ajax({
                url: autoIdUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.TransportAssignEntryId).val(res.data);
                },
                error: function (e) {
                }
            });
        }

        //resetFrom = function () {
        //    $(commonName.AutoId).val(0);
        //    $(commonName.TransportAssignEntryId).val('');
        //    $(commonName.Active).prop('checked', false);
        //    $(commonName.TransportUser).val('');
        //    $(commonName.DriverSelectEmpId).val('').trigger('change');
        //    $(commonName.TransportNoId).val("").trigger('change');
        //    $(commonName.TransportTypeId).val("").trigger('change');
        //    $(commonName.DriverSelectEmpId).val("").trigger('change');

        //    UserEmplist = [];
        //    if ($('#DriverSelectEmpId').length) {
        //        $('#DriverSelectEmpId').multiselect('deselectAll', false);
        //        $('#DriverSelectEmpId').multiselect('updateButtonText');
        //    }

        //    $(commonName.DEmpName).text("");
        //    $(commonName.DEmpPhone).text("");
        //    $(commonName.DEmpDesignation).text("");
        //    $(commonName.DEmpDepartment).text("");
        //    $(commonName.CreateDate).text("");
        //    $(commonName.UpdateDate).text("");


        //    if (typeof effectiveDatePicker !== 'undefined') {
        //        effectiveDatePicker.setDate("today", true);
        //    }


        //    autoTransportAssignEntryId();
        //}

        resetFrom = function () {
            $(commonName.AutoId).val(0);
            $(commonName.TransportAssignEntryId).val('');
            $(commonName.Active).prop('checked', false);
            //$(commonName.TransportUser).val('');

            // Regular select dropdowns
            $(commonName.TransportNoId).val('').trigger('change');
            $(commonName.TransportTypeId).val('').trigger('change');

            // Reset multiselects
            UserEmplist = [];

            // UserSelectEmpId reset
            if ($('#UserSelectEmpId').length && $('#UserSelectEmpId').data('multiselect')) {
                $('#UserSelectEmpId').multiselect('deselectAll', false);
                $('#UserSelectEmpId').multiselect('refresh');
            }

            // DriverSelectEmpId reset  
            $('#DriverSelectEmpId').val('');  // Empty string
            if ($('#DriverSelectEmpId').data('multiselect')) {
                $('#DriverSelectEmpId').multiselect('rebuild');
            }
            $(commonName.TransportNoId).val('');  // Empty string
            if ($(commonName.TransportNoId).data('multiselect')) {
                $(commonName.TransportNoId).multiselect('rebuild');
            }


            // Clear employee details
            $(commonName.DEmpName).text("");
            $(commonName.DEmpPhone).text("");
            $(commonName.DEmpDesignation).text("");
            $(commonName.DEmpDepartment).text("");
            $(commonName.CreateDate).text("");
            $(commonName.UpdateDate).text("");

            // Reset date picker
            if (typeof effectiveDatePicker !== 'undefined') {
                effectiveDatePicker.setDate("today", true);
            }

            autoTransportAssignEntryId();
        }
        $(commonName.ClearBrn).on('click', function () {
            resetFrom();
        })
        // get data from input
        getFromData = function () {
            var fromData = {
                AutoId: $(commonName.AutoId).val(),
                TAID: $(commonName.TransportAssignEntryId).val(),
                EmployeeID: $(commonName.DriverSelectEmpId).val(),
                TransportNoId: $(commonName.TransportNoId).val(),
                TransportTypeId: $(commonName.TransportTypeId).val(),
                EffectiveDate: $(commonName.EffectiveDate).val(),
                Active: $(commonName.Active).prop("checked") ? "true" : "false",
                TransportUser: UserEmplist,
            };
            return fromData;
        }
        //exists 
        $([commonName.DriverSelectEmpId, commonName.TransportUser, commonName.TransportNoId].join(',')).on('change', function () {
            $(commonName.VehicleTypeSaveBtn).prop('disabled', false);
        });


        
        //create and edit
        // Save Button Click
        $(document).on('click', commonName.VehicleTypeSaveBtn, function () {
            var fromData = getFromData();
            console.log(fromData);

            if (!fromData.EmployeeID || fromData.EmployeeID.trim() === '') {
                $(commonName.VehicleTypeSaveBtn).prop('disabled', true);

                var $dropdown = $('#DriverSelectEmpId');

                if ($dropdown.length && $dropdown.data('multiselect')) {
                    var $button = $dropdown.siblings('.btn-group').find('button.multiselect');

                    if ($button.length) {
                        $button.focus();
                        setTimeout(function () {
                            $button.click();
                        }, 50);
                    }
                } else {
                    console.warn('Driver multiselect not ready yet');
                }

                return;
            }



            if (fromData.TransportNoId == null || fromData.TransportNoId.trim() === '') {               
                $(commonName.VehicleTypeSaveBtn).prop('disabled', true);
                //$(commonName.TransportNoId).select2('open');

                var $dropdown = $(commonName.TransportNoId);

                if ($dropdown.length && $dropdown.data('multiselect')) {
                    var $button = $dropdown.siblings('.btn-group').find('button.multiselect');

                    if ($button.length) {
                        $button.focus();
                        setTimeout(function () {
                            $button.click();
                        }, 50);
                    }
                } else {
                    console.warn('Driver multiselect not ready yet');
                }

                return;
            }
            if (fromData.TransportUser == null || fromData.TransportUser.length === 0) {               
                $(commonName.VehicleTypeSaveBtn).prop('disabled', true);
                //$(commonName.DriverSelectEmpId).select2('open');

                var $dropdown = $("#UserSelectEmpId");

                if ($dropdown.length && $dropdown.data('multiselect')) {
                    var $button = $dropdown.siblings('.btn-group').find('button.multiselect');

                    if ($button.length) {
                        $button.focus();
                        setTimeout(function () {
                            $button.click();
                        }, 50);
                    }
                } else {
                    console.warn('Driver multiselect not ready yet');
                }

                return;
            }
            if (!fromData.EffectiveDate || fromData.EffectiveDate.trim() === '' || !isValidDate(fromData.EffectiveDate)) {
                $(commonName.VehicleTypeSaveBtn).prop('disabled', true);
                effectiveDatePicker.open();
                return;
            }
        
            $.ajax({
                url: CreateUpdateUrl,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(fromData),
                success: function (res) {
                    if (res.isSuccess) {
                        showToast("success", res.message);
                    } else {
                        showToast("error", res.message);
                    }
                },
                error: function (e) {
                    showToast("error", res.message);
                },
                complete: function () {
                    resetFrom();
                    autoTransportAssignEntryId();
                    loadCategoryData();
                }
            });
        });

        // Reload DataTable Function
        function loadCategoryData() {
            table.ajax.reload(null, false);
        }

        var table = $('#TransportAssignEntyTable').DataTable({
            destroy: true,
            "autoWidth": true,
            "ajax": {
                "url": loadVehicleTypeDataUrl,
                "type": "GET",
                "datatype": "json",
                "dataSrc": function (json) {
                    return json.data || [];
                },
                "error": function (xhr, error, thrown) {
                    showToast("error", "Data loading failed: " + xhr.statusText);
                }
            },
            "columns": [
                {
                    "data": "autoId",
                    "render": function (data) {
                        return `<input type="checkbox" class="row-checkbox" value=${data} />`;
                    },
                    "orderable": false
                },
                {
                    "data": "taid",
                    "render": function (data) {
                        return `<button class="btn btn-sm btn-link btn-transport-entry-edit" data-id=${data}>${data}</button>`;
                    }
                },
                { "data": "showTransportNoId" },
                { "data": "showTransportTypeId" },
                { "data": "showEffectiveDate" },
                { "data": "active" },
                { "data": "showEmployeeID" },
                { "data": "entryUserEmployeeID" },
            ],
            "paging": true,
            "pagingType": "full_numbers",
            "searching": true,
            "ordering": true,
            "responsive": true,
            "autoWidth": true,
            "language": {
                "search": "Search....",
                "lengthMenu": "Show _MENU_ entries per page",
                "zeroRecords": "No data found",
                "info": "Showing _START_ to _END_ of _TOTAL_ entries",
                "paginate": {
                    "first": "First",
                    "last": "Last",
                    "next": "Next",
                    "previous": "Previous"
                }
            }
        });
        let selectedIds = [];
        //edit
        $(document).on('click', commonName.EditBrn, function () {
            let id = $(this).data('id');

            $.ajax({
                url: `${PopulatedDataForUpdateUrl}?id=${id}`,
                type: "GET",
                success: function (res) {
                    console.log(res);
                    selectedIds = [];
                    selectedIds.push(res.result.autoId + '');
                    $(commonName.AutoId).val(res.result.autoId);
                    //$(commonName.DriverSelectEmpId).val(res.result.employeeID).trigger('change');
                    $(commonName.TransportAssignEntryId).val(res.result.taid);
                    //$(commonName.TransportNoId).val(res.result.transportNoId).trigger('change');
                    $(commonName.TransportTypeId).val(res.result.transportTypeId).trigger('change');

                    $(commonName.DriverSelectEmpId).val(res.result.employeeID);
                    if ($(commonName.DriverSelectEmpId).data('multiselect')) {
                        $(commonName.DriverSelectEmpId).multiselect('rebuild');
                        $(commonName.DriverSelectEmpId).change(); 
                    }
                    $(commonName.TransportNoId).val(res.result.transportNoId);
                    if ($(commonName.TransportNoId).data('multiselect')) {
                        $(commonName.TransportNoId).multiselect('rebuild');
                        $(commonName.TransportNoId).change(); 
                    }
                    $(commonName.TransportUser).val(res.result.transportUser);
                    if ($(commonName.TransportUser).data('multiselect')) {
                        $(commonName.TransportUser).multiselect('rebuild');
                        $(commonName.TransportUser).change(); 
                    }

                    if (res.result.effectiveDate) {
                        effectiveDatePicker.setDate(res.result.effectiveDate);
                    }
                    //$(commonName.Active).prop('checked',res.result.active);
                    $(commonName.Active).prop('checked', res.result.active === true || res.result.active === "true");
                    $(commonName.CreateDate).text(res.result.showCreateDate);
                    $(commonName.UpdateDate).text(res.result.showModifyDate);
                },
                error: function (e) {
                }, complete: function () {
                }
            });
        });
        $(document).on('change', commonName.TransportNoId, function () {
            console.log($(this).val());
            var transportNoId = $(this).val();
            $.ajax({
                url: transportTypeUrl,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify( transportNoId),
                success: function (res) {
                    console.log(res);
                    if (res.data.length >0) {
                        $(commonName.TransportTypeId).val(res.data[0].vehicleTypeId);
                        if ($(commonName.TransportTypeId).data('multiselect')) {
                            $(commonName.TransportTypeId).multiselect('rebuild');
                            $(commonName.TransportTypeId).change();
                        }
                    }
                   
                }, error: function (e) {
                    console.log(e);
                }
            });
        })
        //selected id        

        $(document).on('change', commonName.RowCheckbox, function () {
            const id = $(this).val();
            if ($(this).is(':checked')) {
                if (!selectedIds.includes(id)) {
                    selectedIds.push(id);
                }
            } else {
                selectedIds = selectedIds.filter(item => item != id);
            }

            let totalCheckboxes = $(commonName.RowCheckbox).length;
            let totalChecked = $(commonName.RowCheckbox + ":checked").length;

            $('#selectAll').prop('checked', totalChecked === totalCheckboxes);
        })
        //select all
        $(document).on('change', commonName.SelectedAll, function () {
            const isChecked = $(this).is(':checked');
            $(commonName.RowCheckbox).prop('checked', isChecked).trigger('change');
        })
        $(document).on('click', commonName.DeleteBtn, function () {
            $.ajax({
                url: deleteUrl,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    showToast(res.isSuccess ? "success" : "error", res.message)
                },
                error: function (e) {
                }, complete: function () {
                    resetFrom();
                    autoTransportAssignEntryId();
                    loadCategoryData();
                    $('#selectAll').prop('checked', false);
                    selectedIds = [];
                }
            })
        })


        window.VehicleTypeModuleLoaded = true;
        // Initialize all functions
        var init = function () {
            stHeader();
            autoTransportAssignEntryId();
            setTimeout(function () {
                UserSelectEmpMultiselect();
            }, 100);
            setTimeout(function () {
                DriverSelectEmpId();
            }, 100);

            setTimeout(function () {
                TransportNoList();
            }, 100);
            table;
        };
        init();

    };
})(jQuery);
