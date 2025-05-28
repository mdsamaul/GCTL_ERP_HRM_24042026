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
                    console.log(getById,id);
                    $(settings.formSelector).html($(result).find(settings.formSelector).html());

                    //TimePicker();
                    GetTime(result); //get time
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

            //load pdf           

            $(document).on('click', "#downloadShiftPdf", function (e) {
                e.preventDefault();
                downloadShiftPdf();
            });

            function LoadFullShiftTable(callback) {
                $.ajax({
                    url: '/HRMATDShifts/GetAllShifts',
                    type: "GET",
                    success: function (res) {
                        if (res.success && res.data.length) {
                            callback(res.data);
                        } else {
                            alert("No shift data found.");
                        }
                    },
                    error: function (e) {
                        console.error("Error fetching shift data", e);
                    }
                });
            }

            function downloadShiftPdf() {
                LoadFullShiftTable(function (data) {
                    const { jsPDF } = window.jspdf;
                    const doc = new jsPDF({ orientation: 'landscape' });
                    const pageWidth = doc.internal.pageSize.getWidth();
                    const pageHeight = doc.internal.pageSize.getHeight();

                    // Common header/footer hook
                    const drawHeaderFooter = function (dataTable) {
                        // HEADER
                        doc.setFontSize(16);
                        doc.text("Data Path", pageWidth / 2, 10, { align: 'center' });
                        doc.setFontSize(9);
                        doc.text("HRM Shift Setup Report", pageWidth / 2, 15, { align: 'center' });

                        // Horizontal line under header
                        const lineLength = pageWidth / 8.7;
                        const startX = (pageWidth - lineLength) / 2 +.1;
                        const endX = startX + lineLength;
                        doc.setDrawColor(0);
                        doc.setLineWidth(0.3);
                        doc.line(startX, 16, endX, 16);

                        // FOOTER
                        const now = new Date();
                        const day = String(now.getDate()).padStart(2, '0');
                        const month = String(now.getMonth() + 1).padStart(2, '0');
                        const year = now.getFullYear();
                        const hours = String(now.getHours() % 12 || 12).padStart(2, '0');
                        const minutes = String(now.getMinutes()).padStart(2, '0');
                        const seconds = String(now.getSeconds()).padStart(2, '0');
                        const ampm = now.getHours() < 12 ? 'AM' : 'PM';
                        const dateTimeText = `Print Datetime: ${day}/${month}/${year} ${hours}:${minutes}:${seconds} ${ampm}`;

                        doc.setFontSize(8);
                        // left aligned date/time
                        doc.text(dateTimeText, dataTable.settings.margin.left, pageHeight - 10);
                        // right aligned page count
                        const pageInfo = doc.internal.getCurrentPageInfo();
                        const pageStr = `Page ${pageInfo.pageNumber} of ${doc.internal.getNumberOfPages()}`;
                        doc.text(pageStr, pageWidth - dataTable.settings.margin.right, pageHeight - 10, { align: 'right' });
                    };

                    // Prepare table
                    const headers = [[
                        "SN", "Shift Name", "Description", "In Time", "Out Time", "Late Time",
                        "Lunch Out Time", "Lunch In Time", "Lunch Break(Hr)", "Absent Time", "W.E.F", "Shift Type", "Remarks"
                    ]];
                    let count = 1;
                    const body = data.map(shift => ([
                        count++,
                        shift.shiftName || '',
                        shift.description || '',
                        formatTime(shift.shiftStartTime),
                        formatTime(shift.shiftEndTime),
                        formatTime(shift.lateTime),
                        formatTime(shift.lunchOutTime),
                        formatTime(shift.lunchInTime),
                        shift.lunchBreakHour,
                        formatTime(shift.absentTime),
                        formatDate(shift.wef),
                        shift.shiftTypeId || '',
                        shift.remarks || ''
                    ]));

                    // Generate PDF with autoTable and hooks
                    doc.autoTable({
                        head: headers,
                        body: body,
                        startY: 22,
                        theme: 'grid',
                        styles: {
                            fontSize: 8,
                            cellPadding: 2,
                            halign: 'center',
                            valign: 'middle',
                            lineColor: [210, 210, 210],
                            textColor: [0, 0, 0],
                            lineWidth: 0.1,
                            fillColor: [255, 255, 255]
                        },
                        headStyles: {
                            fillColor: [255, 255, 255],
                            textColor: [0, 0, 0],
                            fontStyle: 'bold',
                            halign: 'center',
                            fontSize: 7
                        },
                        columnStyles: {
                            1: { cellWidth: 35, halign: 'left' },
                            2: { cellWidth: 30, halign: 'left' }
                        },
                        didDrawPage: function (dataTable) {
                            drawHeaderFooter(dataTable);
                        }
                    });

                    doc.save("HRM_Shift_Report.pdf");
                });
            }

            function formatTime(datetimeStr) {
                if (!datetimeStr) return '';
                const d = new Date(datetimeStr);
                const hh = String(d.getHours() % 12 || 12).padStart(2, '0');
                const mm = String(d.getMinutes()).padStart(2, '0');
                const ss = String(d.getSeconds()).padStart(2, '0');
                const ampm = d.getHours() < 12 ? 'AM' : 'PM';
                return `${hh}:${mm}:${ss} ${ampm}`;
            }

            function formatDate(datetimeStr) {
                if (!datetimeStr) return '';
                const d = new Date(datetimeStr);
                const dd = String(d.getDate()).padStart(2, '0');
                const mm = String(d.getMonth() + 1).padStart(2, '0');
                const yyyy = d.getFullYear();
                return `${dd}/${mm}/${yyyy}`;
            }


            //download excel

            $(document).on('click', "#downloadShiftExcel", function (e) {
                e.preventDefault();
                downloadShiftExcel();
            });

            function downloadShiftExcel() {
                LoadFullShiftTable(function (data) {
                    if (!data || !data.length) {
                        alert("No data to export.");
                        return;
                    }

                    $.ajax({
                        url: '/HRMATDShifts/DownloadShiftExcel',
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify(data),
                        xhrFields: {
                            responseType: 'blob' // Important for file download
                        },
                        success: function (blob, status, xhr) {
                            
                            const fileName =  'HRM_Shift_Report.xlsx';

                            const link = document.createElement('a');
                            const url = window.URL.createObjectURL(blob);
                            link.href = url;
                            link.download = fileName;
                            document.body.appendChild(link);
                            link.click();
                            document.body.removeChild(link);
                            window.URL.revokeObjectURL(url);
                        },
                        error: function () {
                            alert("Failed to download Excel.");
                        }
                    });
                });
            }





        });

     

        // Initialization function
        function initialize()
        {         
            dataTable();
            GetTime(); //get time           
            WEFDatePicker();
            GenerateNextCode();
            loadTableData();
            ResetForm();
            select2DD();
            TimePicker();

            //initializeTimePicker();
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

        //$(document).ready(function () {
        //    $('#hRMATDShifts-grid').DataTable();
        //});
        $(document).ready(function () {
            if (!$.fn.dataTable.isDataTable('#hRMATDShifts-grid')) {
                $('#hRMATDShifts-grid').DataTable({
                    paging: true,
                    ordering: true,
                    info: true,
                    searching: true,
                    responsive: true,
                    pageLength: 5,
                    lengthMenu: [5, 10, 25, 50, 100],
                    scrollY: 'auto',
                    scrollCollapse: true
                });
            }
        });


        // Data table initialization function
        //function dataTable()
        //{


        //    $(settings.gridSelector).DataTable({
        //        destroy: true
        //    });
        //    $(settings.gridSelector).DataTable({
        //        "paging": true,
        //        "ordering": true,
        //        "info": true,
        //        "searching": true,
        //            responsive: true,
        //            pageLength: 5,
        //            destroy: true,
        //            lengthMenu: [5,10, 25, 50, 100],
        //        });


        //}
        $(document).ready(function () {
            if (!$.fn.dataTable.isDataTable('#hRMATDShifts-grid')) {
                $('#hRMATDShifts-grid').DataTable({
                    paging: true,
                    ordering: true,
                    info: true,
                    searching: true,
                    responsive: true,
                    pageLength: 5,
                    lengthMenu: [5, 10, 25, 50, 100]
                });
            }
        });

        function dataTable() {
            // Initialize DataTable only once
            var table = $(settings.gridSelector);
            if ($.fn.DataTable.isDataTable(table)) {
                table.DataTable().clear().destroy(); // Destroy existing instance before re-initializing
            }

            // Now initialize the DataTable with the necessary options
            table.DataTable({
                paging: true,
                ordering: true,
                info: true,
                searching: true,
                responsive: true,
                pageLength: 5,
                lengthMenu: [5, 10, 25, 50, 100]
            });
        }


        //get time value

        function GetTime(result) {
            var startTime = $(result).find("#inDateTimeInput").val();
            var endTime = $(result).find("#outDateTimeInput").val();
            var lateTime = $(result).find("#lateDateTimeInput").val();
            var absentTime = $(result).find("#AbsentDateTimeInput").val();
            var wefDate = $(result).find("#WefDate").val();
            var lunchIn = $(result).find("#LunchInDateTime").val();
            var lunchOut = $(result).find("#LunchOutDateTime").val();
            var lunchBreak = $(result).find("#LunchBreakTimeHour").val();

            initializeTimePicker("#inTimeInput", "#inDateTimeInput", startTime);
            initializeTimePicker("#outTimeInput", "#outDateTimeInput", endTime);
            initializeTimePicker("#lateTimeInput", "#lateDateTimeInput", lateTime);
            initializeTimePicker("#AbsentTimeInput", "#AbsentDateTimeInput", absentTime);
            initializeTimePicker("#LunchInTime", "#LunchInDateTime", lunchIn);
            initializeTimePicker("#LunchOutTime", "#LunchOutDateTime", lunchOut);
            //initializeTimePicker("#LunchBreakHour", "#LunchBreakTimeHour", lunchBreak);
            $("#WefDate").val(wefDate);
        }



        //// WEF Date Picker
        //function WEFDatePicker()
        //{
        //    $("#WefDate").datepicker({
        //        dateFormat: 'dd/mm/yy',
        //        changeMonth: true,
        //        changeYear: true,
        //        yearRange: "-100:+10",
        //        regional: 'en-GB', // Add localization if needed
        //        beforeShow: function (input, inst) {
        //            $(this).toggleClass("placeholder-shown", !this.value);
        //        },
        //        onClose: function (dateText, inst) {
        //            $(this).toggleClass("placeholder-shown", !this.value);
        //        },
        //        onSelect: function (dateText) {
        //            var formattedDate = $.datepicker.formatDate('dd/mm/yy', $(this).datepicker('getDate'));
        //            //var formattedDate = $.datepicker.formatDate('mm/dd/yy', $(this).datepicker('getDate'));
        //            $('#WefHidden').val(formattedDate);
        //            $("#WefDate").valid();
        //        }
        //    });

        //    var initialDate = $("#WefDate").val();
        //    if (initialDate) {
        //        var formattedInitialDate = $.datepicker.formatDate('dd/mm/yy', $("#WefDate").datepicker('getDate'));//var formattedInitialDate = $.datepicker.formatDate('mm/dd/yy', $("#WefDate").datepicker('getDate'));
        //        $('#WefHidden').val(formattedInitialDate);
        //    }

        //    $(".datepicker").trigger("input");
        //}

        function WEFDatePicker() {
            $("#WefDate").datepicker({
                dateFormat: 'mm/dd/yy', // ইউজার এই ফরম্যাটে ইনপুট দিবে (default display format)
                changeMonth: true,
                changeYear: true,
                yearRange: "-100:+10",
                //regional: 'en-GB',
                onSelect: function () {
                    var selectedDate = $(this).datepicker('getDate');
                    console.log(selectedDate);
                    if (selectedDate) {
                        var formatted = $.datepicker.formatDate('mm/dd/yy', selectedDate);
                        $('#WefHidden').val(formatted); // একই ফরম্যাটে hidden এ সেট
                    }
                    $("#WefDate").valid();
                }
            });

            var initialDate = $("#WefDate").val();
            if (initialDate) {
                var parsedDate = $("#WefDate").datepicker('getDate');
                if (parsedDate) {
                    var formattedInit = $.datepicker.formatDate('mm/dd/yy', parsedDate);
                    $('#WefHidden').val(formattedInit);
                }
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

            if (!$('#Id').val()) {
                var currentTime = moment().format('hh:mm:ss A');
                $('.TimePicker').each(function () {
                    $(this).val(currentTime);
                });
            }
            $(settings.formSelector)[0].reset();
            $('#Id').val('');
            $('#AutoId').val('');
            $('#ShiftCode').val(''); 
            $('#ShiftTypeId').val(' '); 
            $('#ShiftName').val('').trigger('focus'); 
            $('#ShiftTypeIdDD').val(null).trigger('change');
            $('#Description').val('');

            //var currentTime = moment().format('hh:mm:ss A');
            //$('.TimePicker').each(function ()
            //{
            //    $(this).val(currentTime);
            //});
          
            $('#WefDate').val(''); 
            $('#Remarks').val('');
            $('#LdateModifyHide').hide();
            $(settings.saveSelector).html('<i class="fas fa-save"></i> Save');
            $('.text-danger').text('');
            $(settings.formSelector).attr('action', testSave);
            GenerateNextCode();


            initializeTimePicker("#inTimeInput", "#inDateTimeInput");
            initializeTimePicker("#outTimeInput", "#outDateTimeInput");
            initializeTimePicker("#lateTimeInput", "#lateDateTimeInput");
            initializeTimePicker("#AbsentTimeInput", "#AbsentDateTimeInput");
            initializeTimePicker("#LunchInTime", "#LunchInDateTime");
            initializeTimePicker("#LunchOutTime", "#LunchOutDateTime");
            //initializeTimePicker("#LunchBreakHour", "#LunchBreakTimeHour");

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
            var shiftName = $("#ShiftTypeIdDD").val();
            if (!shiftName) {
                toastr.warning('Select Shift');
                $('#ShiftTypeIdDD').select2('open');
                return false;
            }
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
                },

            });
        }

        
        $(document).ready(function () {
            initializeTimePicker("#inTimeInput", "#inDateTimeInput");
            initializeTimePicker("#outTimeInput", "#outDateTimeInput");
            initializeTimePicker("#lateTimeInput", "#lateDateTimeInput");
            initializeTimePicker("#AbsentTimeInput", "#AbsentDateTimeInput");
            initializeTimePicker("#AbsentTimeInput", "#AbsentDateTimeInput");
            initializeTimePicker("#LunchInTime", "#LunchInDateTime");
            initializeTimePicker("#LunchOutTime", "#LunchOutDateTime");
            //initializeTimePicker("#LunchBreakHour", "#LunchBreakTimeHour");

        })       

        //function initializeTimePicker(timeInputId, dateTimeInputId) {
        //    flatpickr(timeInputId, {
        //        enableTime: true,
        //        noCalendar: true,
        //        enableSeconds: true,
        //        time_24hr: false,
        //        dateFormat: "h:i:s K",
        //        inline: true,

        //        defaultDate: new Date(),

        //        hourIncrement: 1,
        //        minuteIncrement: 1,
        //        secondIncrement: 1,

        //        onChange: function (selectedDates, dateStr, instance) {
        //            const today = new Date();
        //            const year = today.getFullYear();
        //            const month = String(today.getMonth() + 1).padStart(2, '0');
        //            const day = String(today.getDate()).padStart(2, '0');
        //            const fullDate = `${year}-${month}-${day}`;
        //            const dateTime = `${fullDate} ${dateStr}`;
        //            $(dateTimeInputId).val(dateTime);
        //        },

        //        onReady: function (selectedDates, dateStr, instance) {
        //            const today = new Date();
        //            const year = today.getFullYear();
        //            const month = String(today.getMonth() + 1).padStart(2, '0');
        //            const day = String(today.getDate()).padStart(2, '0');
        //            const fullDate = `${year}-${month}-${day}`;

        //            const currentTime = instance.input.value;
        //            const dateTime = `${fullDate} ${currentTime}`;
        //            $(dateTimeInputId).val(dateTime);
        //        }
        //    });
        //}

        function initializeTimePicker(timeInputId, dateTimeInputId, defaultValue = null) {
            flatpickr(timeInputId, {
                enableTime: true,
                noCalendar: true,
                enableSeconds: true,
                time_24hr: false,
                dateFormat: "h:i:s K",
                inline: true,
                defaultDate: defaultValue || new Date(),
                hourIncrement: 1,     
                minuteIncrement: 1,   
                secondIncrement: 1, 
                onChange: function (selectedDates, dateStr, instance) {
                    const today = new Date();
                    const fullDate = today.toISOString().slice(0, 10);
                    const dateTime = `${fullDate} ${dateStr}`;
                    $(dateTimeInputId).val(dateTime);
                },

                onReady: function (selectedDates, dateStr, instance) {
                    const today = new Date();
                    const fullDate = today.toISOString().slice(0, 10);
                    const currentTime = instance.input.value;
                    const dateTime = `${fullDate} ${currentTime}`;
                    $(dateTimeInputId).val(dateTime);
                }
            });
        }




    }
  
  
}(jQuery));

