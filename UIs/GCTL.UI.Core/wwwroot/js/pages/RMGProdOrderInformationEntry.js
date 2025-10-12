(function ($) {
    $.RMGProdOrderInformationEntry = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: "/",
            MainSetup_MainItemID: "#MainSetup_MainItemID",
           

            ItemSaveBtn: ".js-MainI-tem-Group-save",
            ItemDeleteBtn: "#js-MainI-tem-Group-delete-confirm",
            ClearBtn: "#js-MainI-tem-Group-clear",

        }, options);


        var gridUrl = settings.baseUrl + "/grid";
        var saveEditUrl = settings.baseUrl + "/AdddEditSetup";
       

        var GetAutoAllIdUrl = settings.baseUrl + "/GetAutoAllId";
        var DeleteItemUrl = settings.baseUrl + "/DeleteItemUrl";

        var ChangeAbleDropdownUrl = settings.baseUrl + "/ChangeAbleDropdown";

        //$('.searchable-select').select2({
        //    placeholder: 'Select an option',
        //    allowClear: true,
        //    width: '100%',
        //    language: { noResults: () => 'No results found' },
        //    escapeMarkup: markup => markup
        //});
        // Sticky header on scroll
        function stHeader() {
            window.addEventListener('scroll', function () {
                const header = document.getElementById('stickyHeader');

                if (window.scrollY > 750) {
                    header.classList.add('hide-header');
                    header.classList.remove('sticky-top');
                }
                else if (window.scrollY > 10) {
                    header.classList.add('sticky-top');
                    header.classList.add('sticky-scrolled');
                    header.classList.remove('hide-header');
                }
                else {
                    header.classList.add('sticky-top');
                    header.classList.remove('sticky-scrolled');
                    header.classList.remove('hide-header');
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
        $(document).ready(function () {
            // Initial state: Show password row, hide samaul row
            $('.styleWiseRow').show();
            $('.masterPoWise').hide();

            // Toggle on radio change
            $('input[name="option"]').on('change', function () {
                if ($('#styleWise').is(':checked')) {
                    $('.styleWiseRow').show();
                    $('.masterPoWise').hide();
                } else if ($('#poWise').is(':checked')) {
                    $('.styleWiseRow').hide();
                    $('.masterPoWise').show();
                }
            });
        });


        $(document).ready(function () {
            // Initialize all multiselect dropdowns
            $('.searchAbleSelectMulti').multiselect({
                includeSelectAllOption: true,
                selectAllText: 'Select All',
                enableFiltering: true,
                enableCaseInsensitiveFiltering: true,
                filterPlaceholder: 'Search contacts...',
                buttonWidth: '100%',
                maxHeight: 250,
                numberDisplayed: 2,
                nonSelectedText: 'Select contacts',
                nSelectedText: 'selected',
                allSelectedText: 'All selected',
                buttonClass: 'btn btn-sm form-select'
            });
          
           


            //// Initialize Merchandiser Contact Person Multiselect
            //$('#merchandiserContactPerson').multiselect({
            //    includeSelectAllOption: true,
            //    selectAllText: 'Select All',
            //    enableFiltering: true,
            //    enableCaseInsensitiveFiltering: true,
            //    filterPlaceholder: 'Search merchandisers...',
            //    buttonWidth: '100%',
            //    maxHeight: 250,
            //    numberDisplayed: 2,
            //    nonSelectedText: 'Select merchandisers',
            //    nSelectedText: 'selected',
            //    allSelectedText: 'All selected',
            //    buttonClass: 'btn btn-sm form-select'
            //});           
        });


        $(document).ready(function () {
            stHeader()
            console.log("page is running");
        })










        $(document).ready(function () {
            let tableInitialized = false;
            let isOpen = false;
            let selectedIds = [];

            const employees = [
                { id: 1, name: "Alex Johnson", designation: "Manager", phone: "01711111111", email: "alex@mail.com" },
                { id: 2, name: "Emily Davis", designation: "Officer", phone: "01722222222", email: "emily@mail.com" },
                { id: 3, name: "Chris Martin", designation: "Executive", phone: "01733333333", email: "chris@mail.com" },
                { id: 4, name: "Lisa Anderson", designation: "Coordinator", phone: "01744444444", email: "lisa@mail.com" },
                { id: 1, name: "Alex Johnson", designation: "Manager", phone: "01711111111", email: "alex@mail.com" },
                { id: 2, name: "Emily Davis", designation: "Officer", phone: "01722222222", email: "emily@mail.com" },
                { id: 3, name: "Chris Martin", designation: "Executive", phone: "01733333333", email: "chris@mail.com" },
                { id: 4, name: "Lisa Anderson", designation: "Coordinator", phone: "01744444444", email: "lisa@mail.com" },
                { id: 1, name: "Alex Johnson", designation: "Manager", phone: "01711111111", email: "alex@mail.com" },
                { id: 2, name: "Emily Davis", designation: "Officer", phone: "01722222222", email: "emily@mail.com" },
                { id: 3, name: "Chris Martin", designation: "Executive", phone: "01733333333", email: "chris@mail.com" },
                { id: 4, name: "Lisa Anderson", designation: "Coordinator", phone: "01744444444", email: "lisa@mail.com" }
            ];

            // ✅ DataTable initialize
            function initDataTable() {
                if ($.fn.DataTable.isDataTable('#employeeTable')) {
                    $('#employeeTable').DataTable().destroy();
                }

                $('#employeeTable').DataTable({
                    data: employees,
                    columns: [
                        {
                            data: "id",
                            render: function (data) {
                                const checked = selectedIds.includes(data) ? 'checked' : '';
                                return `<input type="checkbox" class="row-check" value="${data}" ${checked}>`;
                            },
                            orderable: false
                        },
                        { data: "name" },
                        { data: "designation" },
                        { data: "phone" },
                        { data: "email" }
                    ],
                    pageLength: 5,
                    lengthMenu: [[5, 10, 15, -1], [5, 10, 15, "All"]],
                });
            }

            // ✅ Table show/hide toggle
            $(document).on('click', '#merchandiserContactPerson', function (e) {
                e.stopPropagation();
                if (isOpen) {
                    $('#employeeContainer').slideUp(300);
                    isOpen = false;
                    console.log("off");
                } else {
                    $('#employeeContainer').slideDown(300);
                    if (!tableInitialized) {
                        initDataTable();
                        tableInitialized = true;
                    }
                    isOpen = true;
                    console.log("on");
                }
            });

            // ✅ বাইরে ক্লিক করলে বন্ধ হবে
            $(document).on('click', function (e) {
                if (isOpen && !$(e.target).closest('#employeeContainer, #merchandiserContactPerson').length) {
                    $('#employeeContainer').slideUp(300);
                    isOpen = false;
                    console.log("off");
                }
            });

            // ✅ Table এর ভিতরে ক্লিক করলে বন্ধ হবে না
            $('#employeeContainer').on('click', function (e) {
                e.stopPropagation();
            });

            // ✅ Checkbox change event
            $(document).on('change', '.row-check', function () {
                const empId = parseInt($(this).val());
                if ($(this).is(':checked')) {
                    if (!selectedIds.includes(empId)) selectedIds.push(empId);
                } else {
                    selectedIds = selectedIds.filter(id => id !== empId);
                }

                updateSelectedDisplay();
                console.log("Selected IDs:", selectedIds);
            });

            // ✅ Select All checkbox
            $(document).on('change', '#selectAll', function () {
                const isChecked = $(this).is(':checked');
                $('.row-check').prop('checked', isChecked);
                selectedIds = isChecked ? employees.map(emp => emp.id) : [];
                updateSelectedDisplay();
                console.log("Selected IDs:", selectedIds);
            });

           
            function updateSelectedDisplay() {
                const selectedNames = employees
                    .filter(emp => selectedIds.includes(emp.id))
                    .map(emp => emp.name);

                let displayText = "Select merchandisers";
                if (selectedNames.length === 1) {
                    displayText = selectedNames[0];
                } else if (selectedNames.length > 1) {
                    displayText = `${selectedNames.length} selected`;
                }

                // Update the first placeholder option instead of replacing the HTML
                const select = $('#merchandiserContactPerson');
                if (select.find('option[data-placeholder]').length === 0) {
                    select.prepend(`<option data-placeholder value="">${displayText}</option>`);
                } else {
                    select.find('option[data-placeholder]').text(displayText);
                }

                // Keep it selected
                select.val("");
            }

        });


        //$(document).ready(function () {

        //    // ✅ Dropdown open হলে
        //    $(document).on('focus', '#merchandiserContactPerson', function () {
        //        console.log("on");
        //    });

        //    // ✅ Dropdown বন্ধ হলে
        //    $(document).on('blur', '#merchandiserContactPerson', function () {
        //        console.log("off");
        //    });

        //});




        //order

        function clearOrderInfoForm() {
            // সব text, number, date, textarea clear করা
            $('#OrderInfoSection').find('input[type="text"], input[type="number"], input[type="date"], textarea').val('');

            // সব normal select clear
            $('#OrderInfoSection').find('select').each(function () {
                const $select = $(this);
                $select.val('');

                // যদি Bootstrap Multiselect হয়
                if ($select.hasClass('multiselect') || $select.data('multiselect')) {
                    $select.multiselect('deselectAll', false);
                    $select.multiselect('refresh');
                } else {
                    $select.trigger('change');
                }
            });

            // প্রথম input এ focus দিতে চাইলে
            $('#OrderInfoSection').find('input:first').focus();

            console.log('Order Info form cleared!');
        }

        function getOrderInfoData() {
            const orderInfo = {
                TC: $('#OrderDto_TC').val(),
                BookinOrderNO: $('#OrderDto_BookinOrderNO').val(),
                BuyerID: $('#OrderDto_BuyerID').val(),
                StyleID: $('#OrderDto_StyleID').val(),
                PoNo: $('#OrderDto_PoNo').val(),
                MasterPurchaseOrder: $('#OrderDto_MasterPurchaseOrder').val(),
                IntegraJobNO: $('#OrderDto_IntegraJobNO').val(),
                PurchasedOfficer: $('#OrderDto_PurchasedOfficer').val() || [], // multiselect
                Remarks: $('#OrderDto_Remarks').val(),
                BookingType: $('#OrderDto_BookingType').val(),
                PIValue: $('#OrderDto_PIValue').val(),
                PICurrencyId: $('#OrderDto_PICurrencyId').val(),
                PaymentTerms: $('#OrderDto_PaymentTerms').val(),
                WarehouseID: $('#OrderDto_WarehouseID').val(),
                DeliveryAddress: $('#OrderDto_DeliveryAddress').val(),
                TermsCondition: $('#OrderDto_TermsCondition').val(),
                DeliveryDate: $('#OrderDto_DeliveryDate').val(),
            };

            console.log('Order Info:', orderInfo);
            return orderInfo;
        }

        $(document).on('click', '.js-order-info-save', function () {
            getOrderInfoData()
        })

        $(document).on('click', '#js-order-info-clear', function () {
            clearOrderInfoForm()
        })

    }





}(jQuery));