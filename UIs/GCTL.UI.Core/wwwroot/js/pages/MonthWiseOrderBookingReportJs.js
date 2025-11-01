(function ($) {
    $.MonthWiseOrderBookingReportJs = function (options) {
        var settings = $.extend({
            baseUrl: "/",
            companyIds: "#companySelect",
            branchIds: "#branchSelect",
            departmentIds: "#departmentSelect",
            buyerIdSelect: "#buyerIdSelect",
            employeeIds: "#employeeSelect",
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
                settings.employeeIds
            ].join(", ");

            $(selectors).multiselect({
                enableFiltering: true,
                includeSelectAllOption: true,
                selectAllText: 'Select All',
                nonSelectedText: '--select items--',
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

        // -------------------- Flatpickr Initialization --------------------
        var GetFlatDate = function () {
            flatpickr($('.flatDate'), {
                dateFormat: "Y-m-d",
                altInput: true,
                altFormat: "d/m/Y",
                allowInput: true,
                defaultDate: "today",
                onReady: function (selectedDates, dateStr, instance) {
                    instance.input.placeholder = "dd/mm/yyyy";
                }
            });
        };

        // -------------------- Document Ready --------------------
        $(document).ready(function () {
            GetFlatDate();

            // Toggle inputs based on selection
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
        });

        // -------------------- Get Report Function --------------------
        //function GetOrderReportFun() {
        //    let request = {};

        //    if ($('#Date').is(':checked')) {
        //        request.FromDate = $('#FromDateSelect').val();
        //        request.ToDate = $('#ToDateSelect').val();
        //    } else {
        //        request.FromYear = parseInt($('#YearFrom').val());
        //        request.ToYear = parseInt($('#YearTo').val());
        //    }

        //    console.log(request);

        //    $.ajax({
        //        url: '/MonthWiseOrderBookingReport/GetOrderReport',
        //        type: 'POST',
        //        contentType: 'application/json',
        //        data: JSON.stringify(request),
        //        success: function (res) {
        //            console.log('Report result:', res);
        //        }
        //    });
        //}
        function GetOrderReportAllStyleDownloadExcelReport() {
                function getMultiSelectValues(selector) {
                    let val = $(selector).val();
                    return val && val.length > 0 ? val : []; 
                }
            let request = {
                FromDate: null,
                ToDate: null,
                FromYear: null,
                ToYear: null,
                BuyerIds: getMultiSelectValues("#buyerIdSelect"),
                StyleIds: getMultiSelectValues("#styleSelect"),
                PurchaseOrders: getMultiSelectValues("#purchaseOrderSelect"),
                ColorIds: getMultiSelectValues("#colorSelect"),
                SizeIds: getMultiSelectValues("#sizeSelect")
            };

            if ($('#Date').is(':checked')) {
                request.FromDate = $('#FromDateSelect').val() || null;
                request.ToDate = $('#ToDateSelect').val() || null;
            } else if ($('#Year').is(':checked')) {
                request.FromYear = parseInt($('#YearFrom').val()) || null;
                request.ToYear = parseInt($('#YearTo').val()) || null;
            }

            $.ajax({
                url: '/MonthWiseOrderBookingReport/DownloadOrderAllStyleReport',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(request),
                xhrFields: {
                    responseType: 'blob'
                },
                beforeSend: function () {
                    showLoading();
                },
                success: function (blob) {
                    var link = document.createElement('a');
                    var url = window.URL.createObjectURL(blob);
                    link.href = url;
                    link.download = 'MonthWiseOrderReport_' + new Date().getTime() + '.xlsx';
                    link.click();
                    window.URL.revokeObjectURL(url);
                },
                error: function (xhr, status, error) {
                    console.error("Error downloading report:", error);
                    alert("Failed to download report");
                },
                complete: function () {
                    hideLoading();
                }
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

            if (reportValue === "downloadPdf") {
                PdfDownload();
            } else if (reportValue === "downloadWord") {
                downloadTableAsWord();
            } else if (reportValue === "downloadExcel") {
                GetOrderReportAllStyleDownloadExcelReport();
            } else {
                showToast("warning", "Please Select Report Option");
            }
        });

        // -------------------- Initialization --------------------
        var init = function () {
            showLoading();
            initializeMultiselects();
            setupLoadingOverlay();
            GetFlatDate();
            console.log("MonthWiseOrderBookingReport page is running");
        };

        init();
    };
})(jQuery);
