(function ($) {
    $.hrmAtdShift = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: "/",
            formSelector: "#hRMATDShifts-form",
            formContainer: ".js-hRMATDShifts-form-container",
            gridSelector: "#hRMATDShifts-grid",
            gridContainer: ".js-hRMATDShifts-grid-container",
            editSelector: ".js-hRMATDShifts-edit",
            saveSelector: ".js-hRMATDShifts-save",
            selectAllSelector: "#hRMATDShifts-check-all",
            deleteSelector: ".js-hRMATDShifts-delete-confirm",
            deleteModal: "#hRMATDShifts-delete-modal",
            finalDeleteSelector: ".js-hRMATDShifts-delete",
            clearSelector: ".js-hRMATDShifts-clear",
            topSelector: ".js-go",
            decimalSelector: ".js-hRMATDShifts-decimalplaces",
            maxDecimalPlace: 5,
            showNagativeFormat: false,
            availabilitySelector: ".js-hRMATDShifts-check-duplicateName",
            haseFile: false,
            quickAddSelector: ".js-quick-add",
            quickAddModal: "#quickAddModal",
            lastCodeSelector: '#lastCode',
            load: function () { }
        }, options);


        var baseControllerNameUrl = "/HRMATDShifts";
        var testSave = baseControllerNameUrl + "/Setup";
        var nextCodeULR = baseControllerNameUrl + "/GenerateNextCode";
        var loadTableURL = baseControllerNameUrl + "/GetTableData";
        var deleteURL = baseControllerNameUrl + "/Delete";
        var getById = baseControllerNameUrl + "/Index";
        var duplicateCheckURL = baseControllerNameUrl + "/CheckAvailability";

        var selectedItems = [];

        $(() => {
            initialize();

            //
            $("body").on('click', '#exportExcel', function () {
               
                window.location.href = '/HRMATDShifts/ExportToExcel';
            });
            //
            // Save button click event
            $("body").on('click', settings.saveSelector, function ()
            {
                validation();
                $(settings.formSelector).submit();
            });

            // Form submission event
            $("body").on('submit', settings.formSelector, function (e) {
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
                        if (result.noSavePermission || result.noUpdatePermission || result.isDuplicate)
                        {

                            toastr.error(result.message);
                        }
                        else if (result.isSuccess) {
                            $(settings.gridContainer).html(result.html);
                            initialize();
                           
                            toastr.success(result.message);
                        } else
                        {
                            $(settings.formSelector).html(result);
                            initialize();
                        }
                    },
                    error: function () {
                        toastr.error('Failed Insert.');
                    }
                });
            });

            // Availability check
            $("body").on("keyup", settings.availabilitySelector, function ()
            {
                var self = $(this);
                let code = $(".js-ShiftCode-code").val();
                let name = self.val();

                $.ajax({
                    url: duplicateCheckURL,
                    method: "POST",
                    data: { code: code, name: name },
                    success: function (response)
                    {
                        console.log(response);
                        if (response.isSuccess)
                        {
                            toastr.error(response.message);
                        }
                    }
                });
            });

            // Edit leave type
            $("body").on('click', settings.editSelector, function ()
            {
                var id = $(this).data('id');
                $.get(getById, { id: id }, function (result)
                {
                    $(settings.formSelector).html($(result).find(settings.formSelector).html());

                    TimePicker();
                  
                    WEFDatePicker();
                    select2DD();
                    $(settings.saveSelector).html('<i class="fas fa-edit"></i> Update');
                    $(settings.formSelector).attr('action', testSave);
                    $(settings.deleteSelector).data('id', id);
                }).fail(function () {
                    toastr.error('Failed Update.','Error');
                });
            });

            // Select all checkboxes
            $("body").on('click', settings.selectAllSelector, function () {
                $('.checkBox').prop('checked', $(this).prop('checked'));
            });
            $("body").on('click', settings.deleteSelector, function (e) {
                e.preventDefault();

                var selectedIds = [];

                // Check if was clicked from the edit button
                if ($(this).data('id')) {
                    // If the delete button has a 'data-id' from edit,
                    var editCode = $(this).data('id');
                    selectedIds.push(editCode); // Add the single ID to the array
                } else {
                    // Otherwise, collect all selected checkboxes' IDs for bulk deletion
                    $('.checkBox:checked').each(function () {
                        selectedIds.push($(this).val());
                    });
                }

                if (selectedIds.length === 0) {
                    toastr.error('Please select at least one leave type to delete.');
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
                    url: deleteURL,
                    contentType: 'application/json',
                    data: JSON.stringify(selectedIds),
                    success: function (result) {
                        if (result.isSuccess) {

                            initialize();
                            toastr.success(result.message);
                        } else {

                            toastr.error(result.message);
                        }
                        $(settings.deleteModal).modal('hide');
                    },
                    error: function (xhr)
                    {
                        toastr.error('Failed Delete.');
                        $(settings.deleteModal).modal('hide');
                    }
                });
            });

            //

          

        });




        // Initialization function
        function initialize()
        {
         
            WEFDatePicker();
            GenerateNextCode();
            loadTableData();
            ResetForm();
            select2DD();
            TimePicker();
     
        }
        function select2DD()
        {

            $('#ShiftTypeIdDD').select2({
                language: {
                    noResults: function ()
                    {

                    }
                },
                escapeMarkup: function (markup)
                {
                    return markup;
                }
            });

        }

        // Load leave types list
        function loadTableData()
        {
            $.ajax({
                type: 'GET',
                url: loadTableURL,
                success: function (data)
                {
                    $(settings.gridContainer).html(data);
                    dataTable();

                },
                error: function ()
                {
                    toastr.error('Failed Load Data.');
                }
            });
        }

        // Data table initialization function
        function dataTable()
        {


           
                $(settings.gridSelector).DataTable({
                    responsive: true,
                    pageLength: 5,
                    destroy: true,
                    lengthMenu: [5,10, 25, 50, 100],
                });
           

        }


        // WEF Date Picker
        function WEFDatePicker()
        {
            $("#WefDate").datepicker({
                dateFormat: 'dd/mm/yy',
                changeMonth: true,
                changeYear: true,
                yearRange: "-100:+10",
                regional: 'en-GB', // Add localization if needed
                beforeShow: function (input, inst) {
                    $(this).toggleClass("placeholder-shown", !this.value);
                },
                onClose: function (dateText, inst) {
                    $(this).toggleClass("placeholder-shown", !this.value);
                },
                onSelect: function (dateText) {
                    var formattedDate = $.datepicker.formatDate('mm/dd/yy', $(this).datepicker('getDate'));
                    $('#WefHidden').val(formattedDate);
                    $("#WefDate").valid();
                }
            });

            var initialDate = $("#WefDate").val();
            if (initialDate) {
                var formattedInitialDate = $.datepicker.formatDate('mm/dd/yy', $("#WefDate").datepicker('getDate'));
                $('#WefHidden').val(formattedInitialDate);
            }

            $(".datepicker").trigger("input");
        }
      
        function GenerateNextCode()
        {
            $("#ShiftCode").val('');
            $.ajax({
                type: 'GET',
                url: nextCodeULR,
                success: function (result)
                {
                    
                    $('#ShiftCode').val(result);
                },
                error: function ()
                {
                    toastr.error('Failed Next Code.');
                }
            });
        }





      
        function ResetForm()
        {
            $(settings.formSelector)[0].reset();
            $('#Id').val('');
            $('#AutoId').val('');
            $('#ShiftCode').val(''); 
            $('#ShiftTypeId').val(' '); 
            $('#ShiftName').val('').trigger('focus'); 
            $('#ShiftTypeIdDD').val(null).trigger('change');
            $('#Description').val('');

            var currentTime = moment().format('hh:mm:ss A');
            $('.TimePicker').each(function ()
            {
                $(this).val(currentTime);
            });
          
            $('#WefDate').val(''); 
            $('#Remarks').val('');
            $('#LdateModifyHide').hide();
            $(settings.saveSelector).html('<i class="fas fa-save"></i> Save');
            $('.text-danger').text('');
            $(settings.formSelector).attr('action', testSave);
            GenerateNextCode();
        }


        // Clear form on click
        $(document).on('click', settings.clearSelector, function ()
        {
            ResetForm();
        });

       

        // Validation function
        function validation()
        {
            var wefdate = $('#WefDate').val();
            var name = $("#ShiftName").val();
            if (!name)
            {
                toastr.warning('Enter Shift Name');
                $('#ShiftName').trigger('focus');
                return false;
            }
            if (!wefdate)
            {
                toastr.warning('Select WEF Date');
            }

        }


       

     
        function TimePicker()
        {
            $('.TimePicker').datetimepicker({
                format: 'hh:mm:ss A',
                showTodayButton: true,
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
    }
}(jQuery));