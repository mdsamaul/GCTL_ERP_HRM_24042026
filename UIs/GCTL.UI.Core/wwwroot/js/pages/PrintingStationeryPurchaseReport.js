(function ($) {
    $.PrintingStationeryPurchaseReportJs = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            ShortName: "#ShortName",
            CatagoryName: "#CatagoryName",
            CatagoryID: "#CatagoryID",
            AutoId: "#AutoId",
            RowCheckbox: ".row-checkbox",
            SelectedAll: "#selectAll",
            EditBrn: ".btn-edit",
            CatagorySaveBtn: ".js-inv-catagory-save",
            DeleteBtn: "#js-inv-catagory-delete-confirm",
            UpdateDate: ".updateDate",
            CreateDate: ".createDate",
            ClearBrn: "#js-catagory-clear",
        }, options);

        var loadCategoryDataUrl = commonName.baseUrl + "/LoadData";
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
        



        $('.select2').select2({
            theme: 'bootstrap-5',
            placeholder: function () {
                return $(this).find('option:first').text();
            },
            allowClear: true,
            width: '10vw'  // এখানে width সেট করলাম
        });


        // Initialize Flatpickr
        $('.date-picker').flatpickr({
            dateFormat: "Y-m-d",
            allowInput: true,
            clickOpens: true
        });

        // Initialize DataTable
        var dataTable = $('#dataTable').DataTable({
            responsive: true,
            pageLength: 10,
            lengthMenu: [[5, 10, 25, 50, 100], [5, 10, 25, 50, 100]],
            language: {
                search: "Search Records:",
                lengthMenu: "Show _MENU_ records per page",
                info: "Showing _START_ to _END_ of _TOTAL_ records",
                paginate: {
                    first: "First",
                    last: "Last",
                    next: "Next",
                    previous: "Previous"
                }
            },
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rtip',
            columnDefs: [
                { responsivePriority: 1, targets: 0 }, // ID
                { responsivePriority: 2, targets: 1 }, // Date
                { responsivePriority: 3, targets: -1 }, // Action
                { responsivePriority: 4, targets: -2 }, // Status
                { className: "text-center", targets: [0, 6, 7, 8, 10, 11] },
                { className: "text-end", targets: [7, 8] }
            ],
            order: [[1, 'desc']] // Sort by date descending
        });

        // Filter table functionality
        $('#productTable').DataTable({
            paging: false,
            searching: false,
            ordering: false,
            info: false,
            responsive: true,
            columnDefs: [
                { className: "text-center", targets: "_all" }
            ]
        });

        // Submit button functionality
        $('#submitBtn').click(function () {
            var filters = {
                category: $('#categorySelect').val(),
                product: $('#productSelect').val(),
                brand: $('#brandSelect').val(),
                model: $('#modelSelect').val(),
                dateFrom: $('#dateFrom').val(),
                dateTo: $('#dateTo').val(),
                previewFormat: $('#previewFormatSelect').val(),
                exportFormat: $('#exportFormatSelect').val()
            };

            // Show loading
            $('#loadingSpinner').show();

            // Simulate API call
            setTimeout(function () {
                $('#loadingSpinner').hide();

                // Filter DataTable based on selections
                var searchValue = '';
                if (filters.category) searchValue += filters.category + ' ';
                if (filters.product) searchValue += filters.product + ' ';
                if (filters.brand) searchValue += filters.brand + ' ';

                dataTable.search(searchValue.trim()).draw();

                // Show success message
                showNotification('Report generated successfully!', 'success');
            }, 1500);
        });

        // Reset button functionality
        $('#resetBtn').click(function () {
            // Reset all form fields
            $('.select2').val(null).trigger('change');
            $('.date-picker').val('');
            $('#previewFormatSelect, #exportFormatSelect').val('');

            // Clear DataTable search
            dataTable.search('').draw();

            showNotification('Filters reset successfully!', 'info');
        });

        // Notification function
        function showNotification(message, type) {
            var alertClass = 'alert-' + (type === 'success' ? 'success' : type === 'error' ? 'danger' : 'info');
            var notification = $('<div class="alert ' + alertClass + ' alert-dismissible fade show position-fixed" style="top: 20px; right: 20px; z-index: 9999;">' +
                '<strong>' + message + '</strong>' +
                '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
                '</div>');

            $('body').append(notification);

            setTimeout(function () {
                notification.alert('close');
            }, 3000);
        }

        // Responsive adjustments
        $(window).resize(function () {
            dataTable.columns.adjust().draw();
        });
























        resetFrom = function () {
            $(commonName.AutoId).val(0);
            $(commonName.CatagoryName).val('');
            $(commonName.ShortName).val('');
            autoCatagoryId();
        }
        $(commonName.ClearBrn).on('click', function () {
            resetFrom();
        })
        // get data from input
        getFromData = function () {
            var fromData = {
                AutoId: $(commonName.AutoId).val(),
                CatagoryID: $(commonName.CatagoryID).val(),
                CatagoryName: $(commonName.CatagoryName).val(),
                ShortName: $(commonName.ShortName).val(),
            };
            return fromData;
        }
        
        // Initialize all functions
        var init = function () {
            stHeader();          
            table;
        };
        init();

    };
})(jQuery);
