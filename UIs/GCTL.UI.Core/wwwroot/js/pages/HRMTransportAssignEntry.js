(function ($) {
    $.HRMTransportAssignEntryJs = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            TransportTypeId: "#TransportTypeId",
            TransportNoId: "#TransportNoId",
            UserSelectEmpId: "#UserSelectEmpId",
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
            DriverSelectEmpId: "#DriverSelectEmpId",
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

        $('.searchable-select').select2({
            placeholder: 'Select an option',
            allowClear: false,
            width: '100%'
        });
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
            console.log("Selected Employee ID:", selectedValue);
            $.ajax({
                url: LoadEmpDetailsUrl,
                type: "POST",
                contentType:'application/json',
                data: JSON.stringify(selectedValue ),
                success: function (res) {
                    console.log("Employee Details:", res);
                   
                        $(commonName.DEmpName).text(res.data?.empName);
                        $(commonName.DEmpDepartment).text(res.data?.department);
                        $(commonName.DEmpDesignation).text(res.data?.designation);
                        $(commonName.DEmpPhone).text(res.data?.phone);
                   
                   
                }, error: function (e) {
                    console.log(e.message);
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

        resetFrom = function () {
            $(commonName.AutoId).val(0);
            $(commonName.TransportAssignEntryId).val(''); 
            $(commonName.DriverSelectEmpId).val(null).trigger('change'); 
            $(commonName.TransportNoId).val('');
            $(commonName.TransportTypeId).val('');
            $(commonName.EffectiveDate).val(''); 
            $(commonName.Active).prop('checked', false);
            $(commonName.TransportUser).val('');
            $(commonName.CreateDate).text('');
            $(commonName.UpdateDate).text('');

            if (typeof effectiveDatePicker !== 'undefined') {
                effectiveDatePicker.clear();
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
                TransportUser: $(commonName.TransportUser).val(),
            };
            return fromData;
        }
        //exists 
        $([commonName.DriverSelectEmpId, commonName.UserSelectEmpId, commonName.TransportNoId].join(',')).on('change', function () {
            $(commonName.VehicleTypeSaveBtn).prop('disabled', false);
        });


        
        //create and edit
        // Save Button Click
        $(document).on('click', commonName.VehicleTypeSaveBtn, function () {
            var fromData = getFromData();
            console.log(fromData);
            if (fromData.EmployeeID == null || fromData.EmployeeID.trim() === '') {               
                $(commonName.VehicleTypeSaveBtn).prop('disabled', true);
                $(commonName.DriverSelectEmpId).select2('open');
                return;
            }
            if (fromData.TransportNoId == null || fromData.TransportNoId.trim() === '') {               
                $(commonName.VehicleTypeSaveBtn).prop('disabled', true);
                $(commonName.TransportNoId).select2('open');
                return;
            }
            if (fromData.TransportUser == null || fromData.TransportUser.trim() === '') {               
                $(commonName.VehicleTypeSaveBtn).prop('disabled', true);
                $(commonName.UserSelectEmpId).select2('open');
                return;
            }
            if (!fromData.EffectiveDate || fromData.EffectiveDate.trim() === '' || !isValidDate(fromData.EffectiveDate)) {
                $(commonName.VehicleTypeSaveBtn).prop('disabled', true);
                effectiveDatePicker.open();
                return;
            }
        

            console.log(fromData);

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
                    console.log(json);
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

            console.log(id);
            $.ajax({
                url: `${PopulatedDataForUpdateUrl}?id=${id}`,
                type: "GET",
                success: function (res) {
                    console.log(res);
                    selectedIds = [];
                    selectedIds.push(res.result.autoId + '');
                    $(commonName.AutoId).val(res.result.autoId);
                    $(commonName.DriverSelectEmpId).val(res.result.employeeID).trigger('change');
                    $(commonName.TransportTypeId).val(res.result.TransportTypeId);
                    $(commonName.TransportAssignEntryId).val(res.result.TransportAssignEntryId);
                    $(commonName.CreateDate).text(res.result.showCreateDate);
                    $(commonName.UpdateDate).text(res.result.showModifyDate);
                },
                error: function (e) {
                }, complete: function () {
                }
            });
        });

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
            console.log(selectedIds);
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
            table;
            console.log("SalesDefVehicleTypeJs module loaded successfully.");
        };
        init();

    };
})(jQuery);
