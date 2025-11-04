(function ($) {
    $.MonthWiseOrderBookingReportJs = function (options) {
        var settings = $.extend({
            baseUrl: "/",
            companyIds: "#companySelect",
            branchIds: "#branchSelect",
            departmentIds: "#departmentSelect",
            buyerIdSelect: "#buyerIdSelect",
            styleIds: "#styleSelect",
        }, options);

        // -------------------- Loading Overlay --------------------
        var setupLoadingOverlay = function () {
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
        };

        function showLoading() {
            $("#customLoadingOverlay").css("display", "flex");
        }

        function hideLoading() {
            $("#customLoadingOverlay").hide();
        }

        // -------------------- Multi-select Initialization --------------------
        var initializeMultiselects = function () {
            var selectors = [
                settings.companyIds,
                settings.branchIds,
                settings.departmentIds,
                settings.buyerIdSelect,
                settings.styleIds
            ].join(", ");

            $(selectors).multiselect({
                enableFiltering: true,
                includeSelectAllOption: true,
                selectAllText: 'Select All',
                nonSelectedText: '--Select Buyer--',
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
        };

        // =======================
        // Initialize Flatpickr for date inputs
        // =======================
        var initializeFlatDates = function () {
            flatpickr($('.flatDate'), {
                dateFormat: "Y-m-d",      // Backend format yyyy-MM-dd
                altInput: true,           // Show user-friendly format
                altFormat: "d/m/Y",       // Display format for users
                allowInput: true,
                defaultDate: "today",
                onReady: function (selectedDates, dateStr, instance) {
                    instance.input.placeholder = "dd/mm/yyyy";
                }
            });
        };

        // =======================
        // Document Ready
        // =======================
        $(document).ready(function () {
            initializeFlatDates();

            // Toggle Date/Year inputs based on radio selection
            function toggleInputs() {
                if ($('#Date').is(':checked')) {
                    $('#FromDateSelect, #ToDateSelect')
                        .prop('disabled', false)
                        .closest('.col-12').show();

                    $('#YearFrom, #YearTo')
                        .prop('disabled', true)
                        .closest('.col-12').hide();
                } else if ($('#Year').is(':checked')) {
                    $('#FromDateSelect, #ToDateSelect')
                        .prop('disabled', true)
                        .closest('.col-12').hide();

                    $('#YearFrom, #YearTo')
                        .prop('disabled', false)
                        .closest('.col-12').show();
                }
            }

            toggleInputs();
            $('input[name="durationType"]').change(toggleInputs);

            // Initialize multi-selects (Bootstrap Multiselect example)
            //$('.multiSelect').multiselect({
            //    enableFiltering: true,
            //    includeSelectAllOption: true,
            //    selectAllText: 'Select All',
            //    nonSelectedText: '--select items--',
            //    nSelectedText: 'Selected',
            //    allSelectedText: 'All Selected',
            //    filterPlaceholder: 'Search...',
            //    buttonWidth: '100%',
            //    maxHeight: 350,
            //    enableClickableOptGroups: true,
            //    dropUp: false,
            //    numberDisplayed: 1,
            //    enableCaseInsensitiveFiltering: true
            //});
        });

        // =======================
        // Get selected filters
        // =======================
        function getFilteredValues() {
            function getMultiSelectValues(selector) {
                let val = $(selector).val();
                return val && val.length > 0 ? val : [];
            }

            return {
                FromDate: null,
                ToDate: null,
                FromYear: null,
                ToYear: null,
                BuyerIds: getMultiSelectValues("#buyerIdSelect"),
                StyleIds: getMultiSelectValues("#styleSelect")
            };
        }

        // =======================
        // Format date to yyyy-MM-dd
        // =======================
        function formatDateForBackend(dateStr) {
            if (!dateStr) return null;
            let date = new Date(dateStr);
            let month = (date.getMonth() + 1).toString().padStart(2, '0');
            let day = date.getDate().toString().padStart(2, '0');
            return `${date.getFullYear()}-${month}-${day}`;
        }

        // =======================
        // Download Month Wise Order All Style Excel
        // =======================
        function GetOrderReportAllStyleDownloadExcelReport() {
            var request = getFilteredValues();

            // Date or Year filter
            if ($('#Date').is(':checked')) {
                request.FromDate = formatDateForBackend($('#FromDateSelect').val());
                request.ToDate = formatDateForBackend($('#ToDateSelect').val());
            } else if ($('#Year').is(':checked')) {
                request.FromYear = parseInt($('#YearFrom').val()) || null;
                request.ToYear = parseInt($('#YearTo').val()) || null;
            }

            console.log(request)
            $.ajax({
                url: '/MonthWiseOrderBookingReport/DownloadOrderAllStyleReport',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(request),
                xhrFields: { responseType: 'blob' },
                beforeSend: showLoading,
                success: function (blob) {
                    var link = document.createElement('a');
                    link.href = window.URL.createObjectURL(blob);
                    link.download = 'MonthWiseOrderReport_AllStyle_' + new Date().getTime() + '.xlsx';
                    link.click();
                    window.URL.revokeObjectURL(link.href);
                },
                error: function (xhr, status, error) {
                    console.error("Error downloading report:", error);
                    alert("Failed to download report");
                },
                complete: hideLoading
            });
        }

        // =======================
        // Download Month Wise Order Style Excel (Optional separate report)
        // =======================
        function GetOrderReportStyleDownloadExcelReport() {
            var request = getFilteredValues();

            if ($('#Date').is(':checked')) {
                request.FromDate = formatDateForBackend($('#FromDateSelect').val());
                request.ToDate = formatDateForBackend($('#ToDateSelect').val());
            } else if ($('#Year').is(':checked')) {
                request.FromYear = parseInt($('#YearFrom').val()) || null;
                request.ToYear = parseInt($('#YearTo').val()) || null;
            }
            console.log(request)
            debugger
            $.ajax({
                url: '/MonthWiseOrderBookingReport/DownloadOrderStyleReport',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(request),
                xhrFields: { responseType: 'blob' },
                beforeSend: showLoading,
                success: function (blob) {
                    var link = document.createElement('a');
                    link.href = window.URL.createObjectURL(blob);
                    link.download = 'MonthWiseOrderReport_Style_' + new Date().getTime() + '.xlsx';
                    link.click();
                    window.URL.revokeObjectURL(link.href);
                },
                error: function (xhr, status, error) {
                    console.error("Error downloading report:", error);
                    alert("Failed to download report");
                },
                complete: hideLoading
            });
        }

        



        // =======================
        // Download Month Wise Order Style Excel
        // =======================
        function GetOrderReportStyleDownloadExcelReport() {
            var request = getFilteredValues();

            if ($('#Date').is(':checked')) {
                request.FromDate = formatDateForBackend($('#FromDateSelect').val());
                request.ToDate = formatDateForBackend($('#ToDateSelect').val());
            } else if ($('#Year').is(':checked')) {
                request.FromYear = parseInt($('#YearFrom').val()) || null;
                request.ToYear = parseInt($('#YearTo').val()) || null;
            }
            console.log(request);
            debugger
            $.ajax({
                url: '/MonthWiseOrderBookingReport/DownloadOrderStyleReport',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(request),
                xhrFields: { responseType: 'blob' },
                beforeSend: showLoading,
                success: function (blob) {
                    var link = document.createElement('a');
                    link.href = window.URL.createObjectURL(blob);
                    link.download = 'MonthWiseOrderReport_Style_' + new Date().getTime() + '.xlsx';
                    link.click();
                    window.URL.revokeObjectURL(link.href);
                },
                error: function (xhr, status, error) {
                    console.error("Error downloading report:", error);
                    alert("Failed to download report");
                },
                complete: hideLoading
            });
        }
        // =======================
        // Download Month Wise Order Style Excel
        // =======================
        function GetOrderReportStylePoDownloadExcelReport() {
            var request = getFilteredValues();

            if ($('#Date').is(':checked')) {
                request.FromDate = formatDateForBackend($('#FromDateSelect').val());
                request.ToDate = formatDateForBackend($('#ToDateSelect').val());
            } else if ($('#Year').is(':checked')) {
                request.FromYear = parseInt($('#YearFrom').val()) || null;
                request.ToYear = parseInt($('#YearTo').val()) || null;
            }
            console.log(request);
            debugger
            $.ajax({
                url: '/MonthWiseOrderBookingReport/DownloadOrderStylePoReport',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(request),
                xhrFields: { responseType: 'blob' },
                beforeSend: showLoading,
                success: function (blob) {
                    var link = document.createElement('a');
                    link.href = window.URL.createObjectURL(blob);
                    link.download = 'MonthWiseOrderReport_Style_' + new Date().getTime() + '.xlsx';
                    link.click();
                    window.URL.revokeObjectURL(link.href);
                },
                error: function (xhr, status, error) {
                    console.error("Error downloading report:", error);
                    alert("Failed to download report");
                },
                complete: hideLoading
            });
        }


        // =======================
        // Download Month Wise Order Style Excel
        // =======================
        function GetOrderReportStylePoCSDownloadExcelReport() {
            var request = getFilteredValues();

            if ($('#Date').is(':checked')) {
                request.FromDate = formatDateForBackend($('#FromDateSelect').val());
                request.ToDate = formatDateForBackend($('#ToDateSelect').val());
            } else if ($('#Year').is(':checked')) {
                request.FromYear = parseInt($('#YearFrom').val()) || null;
                request.ToYear = parseInt($('#YearTo').val()) || null;
            }
            console.log(request);
            debugger
            $.ajax({
                url: '/MonthWiseOrderBookingReport/DownloadOrderStylePoCSReport',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(request),
                xhrFields: { responseType: 'blob' },
                beforeSend: showLoading,
                success: function (blob) {
                    var link = document.createElement('a');
                    link.href = window.URL.createObjectURL(blob);
                    link.download = 'MonthWiseOrderReport_Style_' + new Date().getTime() + '.xlsx';
                    link.click();
                    window.URL.revokeObjectURL(link.href);
                },
                error: function (xhr, status, error) {
                    console.error("Error downloading report:", error);
                    alert("Failed to download report");
                },
                complete: hideLoading
            });
        }





        //function GetOrderReportFun() {
        //    // Helper to safely get multiselect values
        //    function getMultiSelectValues(selector) {
        //        let val = $(selector).val();
        //        return val && val.length > 0 ? val : []; // always return array
        //    }

        //    // Build the request object
        //    let request = {
        //        FromDate: null,
        //        ToDate: null,
        //        FromYear: null,
        //        ToYear: null,
        //        BuyerIds: getMultiSelectValues("#buyerIdSelect"),       // adjust IDs as needed
        //        StyleIds: getMultiSelectValues("#styleSelect"),
        //        PurchaseOrders: getMultiSelectValues("#purchaseOrderSelect"),
        //        ColorIds: getMultiSelectValues("#colorSelect"),
        //        SizeIds: getMultiSelectValues("#sizeSelect"),
        //        PageNumber: 1,
        //        PageSize: 10
        //    };

        //    // Check if date or year mode is selected
        //    if ($('#Date').is(':checked')) {
        //        request.FromDate = $('#FromDateSelect').val() || null;
        //        request.ToDate = $('#ToDateSelect').val() || null;
        //    } else if ($('#Year').is(':checked')) {
        //        request.FromYear = parseInt($('#YearFrom').val()) || null;
        //        request.ToYear = parseInt($('#YearTo').val()) || null;
        //    }

        //    console.log("Final Request Object:", request);

        //    // Send to controller
        //    $.ajax({
        //        url: '/MonthWiseOrderBookingReport/GetOrderAllStyleReport',
        //        type: 'POST',
        //        contentType: 'application/json',
        //        data: JSON.stringify(request),
        //        beforeSend: function () {
        //            showLoading(); 
        //        },
        //        success: function (res) {
        //            console.log('Report result:', res);
        //        },
        //        error: function (xhr, status, error) {
        //            console.error("Error fetching report:", error);
        //        },
        //        complete: function () {
        //            hideLoading(); // hide overlay
        //        }
        //    });
        //}


        // -------------------- Download Event Handler --------------------
        $(document).on('click', '#downloadReport', function () {
            console.log("click");
            var reportValue = $("#reportText").val();
            debugger

            if (reportValue === "downloadPdf") {
                PdfDownload();
            } else if (reportValue === "downloadWord") {
                downloadTableAsWord();
            } else if (reportValue === "downloadExcel") {
                var styleId = $(settings.styleIds).val()
                if (styleId == '001') {
                    GetOrderReportAllStyleDownloadExcelReport();
                } else if (styleId != null && styleId == '002') {
                    GetOrderReportStyleDownloadExcelReport();
                } else if (styleId != null && styleId == '003') {
                    GetOrderReportStylePoDownloadExcelReport();
                }else if (styleId != null && styleId == '004') {
                    GetOrderReportStylePoCSDownloadExcelReport();
                } else {
                    showToast("warning", "Please Select Style for Excel Report");
                }
            } else {
                showToast("warning", "Please Select Report Option");
            }
        });

        // -------------------- Initialization --------------------
        var init = function () {
            showLoading();
            initializeMultiselects();
            setupLoadingOverlay();
            //GetFlatDate();
            console.log("MonthWiseOrderBookingReport page is running");
        };

        init();
    };
})(jQuery);
