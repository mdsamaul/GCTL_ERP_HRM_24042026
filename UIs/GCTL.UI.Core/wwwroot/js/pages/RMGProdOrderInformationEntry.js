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

        $(document).ready(function () {
            // Initial Flatpickr setup
            flatpickr(".FlatDatePicker", {
                dateFormat: "m/d/Y",
                altInput: true,
                altFormat: "d/m/Y",
                allowInput: true,
                defaultDate: "today"
            });

            // When Delivery Date changes
            $(document).on('change', "#OrderDetailsDto_DeliveryDate", function () {
                // Get selected date
                var deliveryDate = $(this).val();

                if (deliveryDate) {
                    // Convert to Date object
                    var dateObj = new Date(deliveryDate);

                    // Subtract 7 days
                    dateObj.setDate(dateObj.getDate() - 7);

                    // Format as mm/dd/yyyy
                    var month = ("0" + (dateObj.getMonth() + 1)).slice(-2);
                    var day = ("0" + dateObj.getDate()).slice(-2);
                    var year = dateObj.getFullYear();

                    var formattedDate = `${month}/${day}/${year}`;

                    // Set X-Factory Date with Flatpickr
                    flatpickr("#OrderDetailsDto_XFactoryDate", {
                        dateFormat: "m/d/Y",
                        altInput: true,
                        altFormat: "d/m/Y",
                        allowInput: true,
                        defaultDate: formattedDate
                    });
                }
            });
        });


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
                    $("#OrderDto_POStatusId").val("");
                } else if ($('#poWise').is(':checked')) {
                    $('.styleWiseRow').hide();
                    $('.masterPoWise').show();
                    $("#OrderDto_StyleId").val('').multiselect('rebuild');
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
                filterPlaceholder: 'Search ...',
                buttonWidth: '100%',
                maxHeight: 250,
                numberDisplayed: 2,
                nonSelectedText: 'Select option',
                nSelectedText: 'selected',
                allSelectedText: 'All selected',
                buttonClass: 'btn btn-sm form-select grid-input'
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

            const target = document.getElementById('merchandiserContactPerson');
            if (!target) return;

            // Remove multiple immediately
            $(target).removeAttr('multiple');

            // Watch for any DOM changes
            const observer = new MutationObserver(() => {
                if ($(target).attr('multiple')) {
                    $(target).removeAttr('multiple');
                }
            });

            observer.observe(target, { attributes: true, attributeFilter: ['multiple'] });
        });

        var isOrderInfo = true;
        var isDetails = false;
        var isColorAndBreakup = false;

        $(document).ready(function () {
            $('#nav-tab button[data-bs-toggle="tab"]').on('show.bs.tab', function (e) {
                var activeTabName = $(e.target).text().trim();
                console.log("Active tab:", activeTabName);

                if (activeTabName === 'Details') {
                    isDetails = true;
                    isColorAndBreakup = false;
                    isOrderInfo = false;
                    $("#OrderInformationText").empty().text("Details Entry");
                }
                else if (activeTabName === 'Color And Breakup') {
                    isDetails = false;
                    isColorAndBreakup = true;
                    isOrderInfo = false;
                    $("#OrderInformationText").empty().text("Color & Breakup Entry");
                }
                else if (activeTabName === 'Order Info') {
                    isDetails = false;
                    isColorAndBreakup = false;
                    isOrderInfo = true;
                    $("#OrderInformationText").empty().text("Order Information Entry");
                }
            });
        });


        //function merchandiserContactPersonList() {
        //    $.ajax({
        //        url: '/RMGProdOrderInformationEntry/GetmerchandiserContactPersonList',
        //        type: "GET",
        //        success: function (res) {
        //            if (res.isSuccess) {
        //                console.log("✅ Employee List:", res.data);
        //                // চাইলে টেবিলেও দেখাতে পারো
                       
        //            } else {
        //                console.error("❌ Error:", res.message);
        //            }
        //        },
        //        error: function (xhr, status, error) {
        //            console.error("❌ AJAX Error:", error);
        //        }
        //    });
        //}


     

        let selectedIds = [];

        $(document).ready(function () {

            let employees = [];
            $.ajax({
                url: '/RMGProdOrderInformationEntry/GetmerchandiserContactPersonList',
                type: "GET",
                success: function (res) {
                    if (res.isSuccess) {
                        console.log("✅ Employee List:", res.data);
                        // চাইলে টেবিলেও দেখাতে পারো
                        employees = res.data;
                    } else {
                        console.error("❌ Error:", res.message);
                    }
                },
                error: function (xhr, status, error) {
                    console.error("❌ AJAX Error:", error);
                }
            });
            let tableInitialized = false;
            let isOpen = false;

            //const employees = [
            //    { id: "1", name: "Alex Johnson", designation: "Manager", phone: "01711111111", email: "alex@mail.com" },
            //    { id: "2", name: "Emily Davis", designation: "Officer", phone: "01722222222", email: "emily@mail.com" },
            //    { id: "3", name: "Chris Martin", designation: "Executive", phone: "01733333333", email: "chris@mail.com" },
            //    { id: "4", name: "Lisa Anderson", designation: "Coordinator", phone: "01744444444", email: "lisa@mail.com" },
            //    { id: "5", name: "Alex3 Johnson", designation: "Manager", phone: "01711111111", email: "alex@mail.com" },
            //    { id: "6", name: "Emily3 Davis", designation: "Officer", phone: "01722222222", email: "emily@mail.com" },
            //    { id: "7", name: "Chris3 Martin", designation: "Executive", phone: "01733333333", email: "chris@mail.com" },
            //    { id: "8", name: "Lisa3 Anderson", designation: "Coordinator", phone: "01744444444", email: "lisa@mail.com" },
            //    { id: "9", name: "Alex2 Johnson", designation: "Manager", phone: "01711111111", email: "alex@mail.com" },
            //    { id: "10", name: "Emily2 Davis", designation: "Officer", phone: "01722222222", email: "emily@mail.com" },
            //    { id: "11", name: "Chris2 Martin", designation: "Executive", phone: "01733333333", email: "chris@mail.com" },
            //    { id: "12", name: "Lisa2 Anderson", designation: "Coordinator", phone: "01744444444", email: "lisa@mail.com" }
            //];

            // ✅ Initialize DataTable
            function initDataTable() {
                if ($.fn.DataTable.isDataTable('#employeeTable')) {
                    $('#employeeTable').DataTable().destroy();
                }

                $('#employeeTable').DataTable({
                    data: employees,
                    columns: [
                        {
                            data: "employeeId",
                            render: function (data) {
                                const checked = selectedIds.includes(String(data)) ? 'checked' : '';
                                return `<input type="checkbox" class="row-check" value="${data}" ${checked}>`;
                            },
                            orderable: false,
                            width: "5%"
                        },
                        { data: "fullName" },
                        { data: "designationName" },
                        { data: "mobileNo" },
                        { data: "email" }
                    ],
                    pageLength: 5,
                    lengthMenu: [[5, 10, 15, -1], [5, 10, 15, "All"]],
                    drawCallback: function () {
                        // re-check selected rows after pagination
                        $('.row-check').each(function () {
                            const val = String($(this).val());
                            if (selectedIds.includes(val)) {
                                $(this).prop('checked', true);
                            }
                        });
                    }
                });
            }

            // ✅ Toggle Table visibility
            $(document).on('click', '#merchandiserContactPerson', function (e) {
                e.stopPropagation();
                if (isOpen) {
                    $('#employeeContainer').slideUp(300);
                    isOpen = false;
                } else {
                    $('#employeeContainer').slideDown(300);
                    if (!tableInitialized) {
                        initDataTable();
                        tableInitialized = true;
                    }
                    isOpen = true;
                }
            });

            // ✅ Click outside to close
            $(document).on('click', function (e) {
                if (isOpen && !$(e.target).closest('#employeeContainer, #merchandiserContactPerson').length) {
                    $('#employeeContainer').slideUp(300);
                    isOpen = false;
                }
            });

            // ✅ Prevent closing when clicking inside the table
            $('#employeeContainer').on('click', function (e) {
                e.stopPropagation();
            });

            // ✅ Handle individual checkbox changes
            $(document).on('change', '.row-check', function () {
                const empId = String($(this).val()); // always store as string
                if ($(this).is(':checked')) {
                    if (!selectedIds.includes(empId)) selectedIds.push(empId);
                } else {
                    selectedIds = selectedIds.filter(id => id !== empId);
                }

                updateSelectedDisplay();
                console.log("Selected IDs:", selectedIds);
            });

            // ✅ Handle Select All checkbox
            $(document).on('change', '#selectAll', function () {
                const isChecked = $(this).is(':checked');
                $('.row-check').prop('checked', isChecked);
                selectedIds = isChecked ? employees.map(emp => String(emp.id)) : [];
                updateSelectedDisplay();
                console.log("Selected IDs:", selectedIds);
            });

            // ✅ Update dropdown display text
            function updateSelectedDisplay() {
                const selectedNames = employees
                    .filter(emp => selectedIds.includes(String(emp.id)))
                    .map(emp => emp.name);

                let displayText = "Select merchandisers";
                if (selectedNames.length === 1) {
                    displayText = selectedNames[0];
                } else if (selectedNames.length > 1) {
                    displayText = `${selectedNames.length} selected`;
                }

                const select = $('#merchandiserContactPerson');
                if (select.find('option[data-placeholder]').length === 0) {
                    select.prepend(`<option data-placeholder value="">${displayText}</option>`);
                } else {
                    select.find('option[data-placeholder]').text(displayText);
                }

                select.val(""); // keep placeholder selected
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
            GridOrderInfo();
            autoEntryId();
            IntegraJOBNoAuto();
            populateMerchandiser([]);
            // Helper: null-safe setter for clearing
            const clearVal = (selector) => $(selector).val('').trigger('change');

            // 🧩 Basic Fields
            $('#OrderDto_TC').val(0);
            //clearVal('#OrderDto_OrderId');
            clearVal('#OrderDto_Date');
            clearVal('#OrderDto_BuyerOrderNo');
            clearVal('#OrderDto_BuyerOrderDate');
            clearVal('#OrderDto_MasterPurchaseOrder');
            clearVal('#OrderDto_MpoDate');
            clearVal('#OrderDto_SeasonYear');
            clearVal('#OrderDto_TotalOrderQuantity');
            clearVal('#OrderDto_TotalPrice');
            clearVal('#OrderDto_PaymentTerm');
            clearVal('#OrderDto_BuDesignation1');
            clearVal('#OrderDto_Buphone');
            clearVal('#OrderDto_BuEmail');
            clearVal('#OrderDto_MerContatPerson');
            clearVal('#OrderDto_MerDesignation1');
            clearVal('#OrderDto_Merphone');
            clearVal('#OrderDto_MerEmail');
            clearVal('#OrderDto_BuyerDeclaration');
            clearVal('#OrderDto_InspectionInfo');
            clearVal('#OrderDto_Remarks');
            //clearVal('#OrderDto_IntegraJOBNo');
            clearVal('#OrderDto_OrderDate');
            clearVal('#OrderDto_BuyerSwiftCode');
            clearVal('#OrderDto_CompanySwiftCode');
            clearVal('#OrderDto_FOBAmount');
            clearVal('#CompanyOwnBankAddress');
            clearVal('#buyerBranchAddress');

            // 🟣 Multiselects
            const multiSelectors = [
                '#OrderDto_BuyerId',
                '#OrderDto_SeasonId',
                '#OrderDto_CurrencyId_FOB',
                '#OrderDto_SupplierId',
                '#OrderDto_UnitTypID',
                '#OrderDto_CurrencyId',
                '#OrderDto_BuyerBankId',
                '#OrderDto_CompanyOwnBankId',
                '#OrderDto_POStatusId',
                '#OrderDto_StyleId',
                '#OrderDto_BuyerBrand',
                '#OrderDto_BuyerBranchId',
                '#OrderDto_CompanyOwnBranchId',
                '#OrderDto_BuContatPerson'
            ];

            multiSelectors.forEach(sel => $(sel).val([]).multiselect('rebuild'));

            // ✅ Clear merchandiser DataTable selection
            selectedIds = [];
            if ($.fn.DataTable.isDataTable('#employeeTable')) {
                $('#employeeTable').DataTable().rows().every(function () {
                    $(this.node()).find('.row-check').prop('checked', false);
                });
            }
            //updateSelectedDisplay(); // Reset dropdown display

            // 🔹 Clear StylePOWise radios
            $('#styleWise').prop('checked', true);
            $('#poWise').prop('checked', false);
            $('.styleWiseRow,').show();
            $('.masterPoWise').hide();

            console.log("🧹 Order Info Cleared Successfully");


        }

        function autoEntryId() {
            $.ajax({
                url: '/RMGProdOrderInformationEntry/EntryAutoId',
                type: "GET",
                success: function (res) {
                    $("#OrderDto_OrderId").val(res);
                }
            });
        }

        function IntegraJOBNoAuto() {
            $.ajax({
                url: '/RMGProdOrderInformationEntry/IntegraJOBNoAuto',
                type: "GET",
                success: function (res) {
                    $("#OrderDto_IntegraJOBNo").val(res);
                }
            });
        }

        //change buyer 
        $(document).on('change', "#OrderDto_BuyerId", function () {
            var buyerId = $(this).val();
            console.log(buyerId);
            $.ajax({
                url: '/RMGProdOrderInformationEntry/BuyerBrand',
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(buyerId),
                success: function (res) {
                    console.log(res);
                    var $branchSelect = $("#OrderDto_BuyerBrand");
                    $branchSelect.empty();
                    if (res && res.length > 0) {
                        $branchSelect.append('<option value="" disabled selected hidden>Select Brand</option>');
                        res.forEach(item => {
                            $branchSelect.append(`<option value="${item.id}">${item.name}</option>`);
                        });
                    } else {
                        $branchSelect.append('<option value="" disabled selected hidden>No Brand Found</option>');
                    }
                    $branchSelect.multiselect('rebuild');
                }
            });
        })

        //change buyer bank
        $(document).on('change', "#OrderDto_BuyerBankId", function () {
            var buyerBankId = $(this).val();
            console.log(buyerBankId);
            $.ajax({
                url: '/RMGProdOrderInformationEntry/BuyerBankBranch',
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(buyerBankId),
                success: function (res) {
                    console.log(res);
                    var $branchSelect = $("#OrderDto_BuyerBranchId");
                    $branchSelect.empty();
                    $("#buyerBranchAddress").val('');
                    $("#OrderDto_BuyerSwiftCode").val('');
                    if (res && res.length > 0) {
                        $branchSelect.append('<option value="" disabled selected hidden>Select Branch</option>');
                        res.forEach(item => {
                            $branchSelect.append(`<option value="${item.id}">${item.name}</option>`);
                        });
                    } else {
                        $branchSelect.append('<option value="" disabled selected hidden>No Bank Branch Found</option>');
                    }
                    $branchSelect.multiselect('rebuild');
                }
            });
        })

        //change buyer bank
        $(document).on('change', "#OrderDto_BuyerBranchId", function () {
            var buyerBankBranchId = $(this).val();
            console.log(buyerBankBranchId);
            $.ajax({
                url: '/RMGProdOrderInformationEntry/BuyerBankBranchAddressSwiftCode',
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(buyerBankBranchId),
                success: function (res) {
                    console.log(res);
                    if (res != null) {
                        if (res.swiftCode != null) {
                            $("#OrderDto_BuyerSwiftCode").empty().val(res.swiftCode);
                        }
                        if (res.address != null)
                            $("#buyerBranchAddress").empty().val(res.address);
                    }
                }
            });
        })

        //company
        //change buyer bank
        $(document).on('change', "#OrderDto_CompanyOwnBankId", function () {
            var buyerBankId = $(this).val();
            console.log(buyerBankId);
            $.ajax({
                url: '/RMGProdOrderInformationEntry/BuyerBankBranch',
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(buyerBankId),
                success: function (res) {
                    console.log(res);
                    var $branchSelect = $("#OrderDto_CompanyOwnBranchId");
                    $branchSelect.empty();
                    $("#CompanyOwnBankAddress").val("");
                    $("#OrderDto_CompanySwiftCode").val("");
                    if (res && res.length > 0) {

                        $branchSelect.append('<option value="" disabled selected hidden>Select Branch</option>');
                        res.forEach(item => {
                            $branchSelect.append(`<option value="${item.id}">${item.name}</option>`);
                        });
                    } else {
                        $branchSelect.append('<option value="" disabled selected hidden>No Bank Branch Found</option>');
                    }
                    $branchSelect.multiselect('rebuild');
                }
            });
        })
        //change buyer bank
        $(document).on('change', "#OrderDto_CompanyOwnBranchId", function () {
            var buyerBankBranchId = $(this).val();
            console.log(buyerBankBranchId);
            $.ajax({
                url: '/RMGProdOrderInformationEntry/BuyerBankBranchAddressSwiftCode',
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(buyerBankBranchId),
                success: function (res) {
                    console.log(res);
                    if (res != null) {
                        if (res.swiftCode != null) {
                            $("#OrderDto_CompanySwiftCode").empty().val(res.swiftCode);
                        }
                        if (res.address != null)
                            $("#CompanyOwnBankAddress").empty().val(res.address);
                    }
                }
            });
        })


        function getOrderInfoData() {
            // Helper: Parse decimal or return null
            const parseDecimal = (val) => {
                if (!val || val === '') return null;
                const parsed = parseFloat(val);
                return isNaN(parsed) ? null : parsed;
            };

            // Helper: Parse date or return null
            const parseDate = (val) => {
                if (!val || val === '') return null;
                return val; // Send as string, C# will parse
            };

            // Helper: Get today's date in ISO format
            const getToday = () => new Date().toISOString();
            const stylePOOption = $('input[name="option"]:checked').attr('id') === 'styleWise'
                ? 'Style Wise'
                : 'P.O Wise';

            const orderInfo = {
                TC: parseDecimal($('#OrderDto_TC').val()),
                OrderId: $('#OrderDto_OrderId').val() || null,
                Date: parseDate($('#OrderDto_Date').val()) || getToday(),
                BuyerId: $('#OrderDto_BuyerId').val() || null,
                BuyerOrderNo: $('#OrderDto_BuyerOrderNo').val() || null,
                BuyerOrderDate: parseDate($('#OrderDto_BuyerOrderDate').val()) || getToday(),
                MasterPurchaseOrder: $('#OrderDto_MasterPurchaseOrder').val() || null,
                MPO_Date: parseDate($('#OrderDto_MpoDate').val()) || getToday(),
                SeasonId: $('#OrderDto_SeasonId').val() || null,
                SeasonYear: $('#OrderDto_SeasonYear').val() || null,
                SupplierId: $('#OrderDto_SupplierId').val() || null,
                TotalOrderQuantity: parseDecimal($('#OrderDto_TotalOrderQuantity').val()),
                UnitTypID: $('#OrderDto_UnitTypID').val() || null,
                TotalPrice: parseDecimal($('#OrderDto_TotalPrice').val()),
                CurrencyId: $('#OrderDto_CurrencyId').val() || "",
                PaymentTerm: $('#OrderDto_PaymentTerm').val() || null,
                BuyerBankId: $('#OrderDto_BuyerBankId').val() || null,
                BuyerBranchId: $('#OrderDto_BuyerBranchId').val() || null,
                CompanyOwnBankId: $('#OrderDto_CompanyOwnBankId').val() || null,
                CompanyOwnBranchId: $('#OrderDto_CompanyOwnBranchId').val() || null,
                BuContatPerson: $('#OrderDto_BuContatPerson').val() || [],
                BuDesignation1: $('#OrderDto_BuDesignation1').val() || null,
                Buphone: $('#OrderDto_Buphone').val() || null,
                BuEmail: $('#OrderDto_BuEmail').val() || null,
                MerContatPerson: $('#OrderDto_MerContatPerson').val() || null,
                MerDesignation1: $('#OrderDto_MerDesignation1').val() || null,
                Merphone: $('#OrderDto_Merphone').val() || null,
                MerEmail: $('#OrderDto_MerEmail').val() || null,
                BuyerDeclaration: $('#OrderDto_BuyerDeclaration').val() || null,
                InspectionInfo: $('#OrderDto_InspectionInfo').val() || null,
                Remarks: $('#OrderDto_Remarks').val() || null,
                IntegraJOBNo: $('#OrderDto_IntegraJOBNo').val() || null,
                POStatusId: $('#OrderDto_POStatusId').val() || null,
                BuyerBrand: $('#OrderDto_BuyerBrand').val() || null,
                StyleId: $('#OrderDto_StyleId').val() || null,
                OrderDate: parseDate($('#OrderDto_OrderDate').val()) || getToday(),
                BuyerSwiftCode: $('#OrderDto_BuyerSwiftCode').val() || null,
                CompanySwiftCode: $('#OrderDto_CompanySwiftCode').val() || null,
                MerchandiserContactId: (selectedIds || []).map(String),
                StylePOWise: $('#OrderDto_StylePOWise').val() || null,
                FOBAmount: parseDecimal($('#OrderDto_FOBAmount').val()),
                CurrencyId_FOB: $('#OrderDto_CurrencyId_FOB').val() || null,
                StylePOWise: stylePOOption
            };

            console.log("RMGProdOrder Data:", orderInfo);
            return orderInfo;
        }

        $(document).on('input', "#OrderDto_TotalOrderQuantity", function () {
            $("#OrderDto_TotalOrderQuantity").removeClass('border border-danger');
            $(".js-order-info-save").prop('disabled', false);
        })


        //order info grid


        //function GridOrderInfo () {
        //    // Destroy if already initialized
        //    if ($.fn.DataTable.isDataTable('#orderInfoGrid')) {
        //        $('#orderInfoGrid').DataTable().destroy();
        //    }

        //    // Initialize DataTable
        //    var table = $('#orderInfoGrid').DataTable({
        //        processing: true,
        //        serverSide: true,
        //        ajax: {
        //            url: '/RMGProdOrderInformationEntry/GetOrderList',
        //            type: 'POST',
        //            dataSrc: function (json) {
        //                console.log("✅ Full DataTables Response:", json);
        //                return json.data;
        //            }
        //        },
        //        columns: [
        //            {
        //                data: null,
        //                render: function (data, type, row) {
        //                    return `<input type="checkbox" class="order-select" data-id="${row.tc}" />`;
        //                },
        //                orderable: false,
        //                searchable: false
        //            },
        //            {
        //                data: "orderId",
        //                render: function (data, type, row) {
        //                    console.log(row, data);
        //                    return `<a href="#" class="order-link" data-row='${JSON.stringify(row)}'>${data}</a>`;
        //                }
        //            },
        //            { data: "buyerId" },
        //            { data: "buyerBrand" },
        //            { data: "integraJOBNo" },
        //            { data: "styleId" },
        //            { data: "masterPurchaseOrder" },
        //            { data: "seasonId" },
        //            { data: "seasonYear" },
        //            { data: "totalOrderQuantity" },
        //            { data: "fobAmount" }
        //        ],
        //        columnDefs: [
        //            { width: "50px", targets: 0 },
        //            { className: "text-center align-middle", targets: "_all" }
        //        ],
        //    });

        // =====================
        // 🔹 Global Selected IDs
        // =====================
        let selectedOrderIds = [];

        // =====================
        // 🔹 Initialize / Reload DataTable
        // =====================
        function GridOrderInfo(integraJobNo = null) {
            if ($.fn.DataTable.isDataTable('#orderInfoGrid')) {
                $('#orderInfoGrid').DataTable().destroy();
            }

            $('#orderInfoGrid').DataTable({
                processing: true,
                serverSide: true,
                ajax: {
                    url: '/RMGProdOrderInformationEntry/GetOrderList',
                    type: 'POST',
                    data: function (d) {
                        d.integraJobNo = integraJobNo; // send filter to server
                    },
                    dataSrc: function (json) {
                        console.log("✅ Filtered Order Details:", json);
                        return json.data;
                    }
                },
                columns: [
                    {
                        data: null,
                        render: function (data, type, row) {
                            return `<input type="checkbox" class="order-select" data-id="${row.tc}" />`;
                        },
                        orderable: false,
                        searchable: false
                    },
                    {
                        data: "orderId",
                        render: function (data, type, row) {
                            const safeRow = $('<div>').text(JSON.stringify(row)).html();
                            return `<a href="#" class="order-link" data-row='${safeRow}'>${data}</a>`;
                        }
                    },
                    { data: "buyerId" },
                    { data: "buyerBrand" },
                    { data: "integraJOBNo" },
                    { data: "styleId" },
                    { data: "masterPurchaseOrder" },
                    { data: "seasonId" },
                    { data: "seasonYear" },
                    { data: "totalOrderQuantity" },
                    { data: "fobAmount" }
                ],
                columnDefs: [
                    { width: "50px", targets: 0 },
                    { className: "text-center align-middle", targets: "_all" }
                ]
            });
        }

        // =====================
        // 🔹 Detail Link Click
        // =====================
        $('#orderInfoGrid').on('click', '.order-link', function (e) {
            e.preventDefault();

            const rawData = $(this).attr('data-row');
            try {
                const rowData = JSON.parse(rawData);
                const jobNo = rowData.integraJOBNo;

                console.log("✅ Clicked JobNo:", jobNo);
                console.log("✅ Row Data:", rowData);

                populateOrderInfoData(rowData);
                GridOrderInfo(jobNo);

            } catch (err) {
                console.error("❌ JSON parse error:", err, rawData);
            }
        });
        //todo qty
      
        $(document).on('input', "#OrderDetailsDto_OrderQuantity", function () {
            const id = $("#OrderDetailsDto_IntegraJobNO").val();
            const qty = parseFloat($("#OrderDetailsDto_OrderQuantity").val()) || 0;
            const prevQty = parseFloat($("#OrderDetailsDto_PreviousQuantity").val()) || 0; 
            const unitPrice = parseFloat($("#OrderDetailsDto_UnitPrice").val()) || 0;

            if (!id) {
                console.warn("⚠️ Integra Job ID missing!");
                return;
            }

            $.ajax({
                url: '/RMGProdOrderInformationEntry/TotalQtyByJobId',
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(id),
                success: function (res) {
                    console.log("✅ Server Response:", res);

                    const totalUsed = parseFloat(res.totalQtyList) || 0;
                    const totalLimit = parseFloat(res.totalQty) || 0;

                    // ✅ Update mode এ হলে previousQty consider করো
                    const adjustedUsed = totalUsed - prevQty; // আগের qty বাদ দিলাম
                    let totalqtyOK = true;

                    if (qty + adjustedUsed > totalLimit) {
                        showToast('warning', "Total Quantity exceeded the allowed limit!");
                        totalqtyOK = false;
                    }

                    if (totalqtyOK) {
                        $('#OrderDetailsDto_OrderQuantity').removeClass('border border-danger');
                        $(".js-order-info-save").prop('disabled', false);
                        const totalAmount = qty * unitPrice;
                        $("#OrderDetailsDto_TotalAmount")
                            .val(totalAmount.toFixed(0))
                            .prop('disabled', true);
                    } else {
                        $("#OrderDetailsDto_TotalAmount")
                            .val(0)
                            .prop('disabled', true);
                        $('#OrderDetailsDto_OrderQuantity').addClass('border border-danger');
                        $(".js-order-info-save").prop('disabled', true);
                        $(".js-order-info-save").addClass('border-0');
                    }
                },
                error: function (xhr, status, err) {
                    console.error("❌ AJAX Error:", err);
                }
            });
        });


        $(document).on('input', "#OrderDetailsDto_UnitPrice", () => {
            var Qty = $("#OrderDetailsDto_OrderQuantity").val();
            var up = $("#OrderDetailsDto_UnitPrice").val();          
            if (Qty && up) {
                var ta = Qty * up;               
                $("#OrderDetailsDto_TotalAmount").empty().val(ta).prop('disabled', true);
            } else {
                $("#OrderDetailsDto_TotalAmount").val(0).prop('disabled', true);
            }
        })

        // =====================
        // 🔹 Row Checkbox Change
        // =====================
        $('#orderInfoGrid').on('change', '.order-select', function () {
            const id = $(this).data('id');
            console.log(id);
            if ($(this).is(':checked')) {
                if (!selectedOrderIds.includes(id)) selectedOrderIds.push(id);
            } else {
                selectedOrderIds = selectedOrderIds.filter(x => x !== id);
            }
            console.log("🟢 Selected Order IDs:", selectedOrderIds);
        });

        // =====================
        // 🔹 Select All Checkbox
        // =====================
        $('#orderInfo-check-all').on('change', function () {
            const isChecked = $(this).is(':checked');
            $('.order-select').prop('checked', isChecked).trigger('change');
        });

        // =====================
        // 🔹 Delete Selected Orders
        // =====================
        function orderInfoDelete() {

            console.log(selectedOrderIds);

            if (selectedOrderIds.length === 0) {
               
                return;
            }

            $.ajax({
                url: '/RMGProdOrderInformationEntry/DeleteOrderInfo',
                type: 'POST',
                data: JSON.stringify(selectedOrderIds),
                contentType: 'application/json',
                success: function (res) {
                    showToast(`${res.isSuccess ? "success" : "error"}`, res.message);
                    if (res.isSuccess) {
                        GridOrderInfo();
                        clearOrderInfoForm();
                        selectedOrderIds = [];
                    }
                },
                error: function (err) {
                    console.error("❌ Delete failed:", err);
                }
            });
        }

        // =====================
        // 🔹 Confirm Delete Button
        // =====================
        $(document).on('click', '#js-order-info-delete-confirm', function () {
            if (confirm("Do you want to delete selected orders?")) {
                orderInfoDelete();
            }
        });

        // =====================
        // 🔹 On Page Load
        // =====================
        //$(document).ready(function () {
        //    GridOrderInfo();
        //});



        function populateOrderInfoData(data) {
            // Helper: null-safe setter
            const setVal = (selector, value) => $(selector).val(value ?? '').trigger('change');

            // 🧩 Basic Fields
            setVal('#OrderDto_TC', data.tc);
            setVal('#OrderDto_OrderId', data.orderId);
            setVal('#OrderDto_Date', data.date);
            setVal('#OrderDto_BuyerOrderNo', data.buyerOrderNo);
            setVal('#OrderDto_BuyerOrderDate', data.buyerOrderDate);
            setVal('#OrderDto_MasterPurchaseOrder', data.masterPurchaseOrder);
            setVal('#OrderDto_MpoDate', data.mpO_Date);
            setVal('#OrderDto_SeasonYear', data.seasonYear);
            setVal('#OrderDto_TotalOrderQuantity', data.totalOrderQuantity);
            setVal('#OrderDto_TotalPrice', data.totalPrice);
            setVal('#OrderDto_PaymentTerm', data.paymentTerm);
            setVal('#OrderDto_BuDesignation1', data.buDesignation1);
            setVal('#OrderDto_Buphone', data.buphone);
            setVal('#OrderDto_BuEmail', data.buEmail);
            setVal('#OrderDto_MerContatPerson', data.merContatPerson);
            setVal('#OrderDto_MerDesignation1', data.merDesignation1);
            setVal('#OrderDto_Merphone', data.merphone);
            setVal('#OrderDto_MerEmail', data.merEmail);
            setVal('#OrderDto_BuyerDeclaration', data.buyerDeclaration);
            setVal('#OrderDto_InspectionInfo', data.inspectionInfo);
            setVal('#OrderDto_Remarks', data.remarks);
            setVal('#OrderDto_IntegraJOBNo', data.integraJOBNo);
            setVal('#OrderDetailsDto_IntegraJobNO', data.integraJOBNo).multiselect('rebuild');

            $("#OrderDetailsDto_IntegraJobNO").multiselect('disable');


            setVal('#OrderDto_OrderDate', data.orderDate);
            setVal('#OrderDto_BuyerSwiftCode', data.buyerSwiftCode);
            setVal('#OrderDto_CompanySwiftCode', data.companySwiftCode);
            setVal('#OrderDto_FOBAmount', data.fobAmount);
            setVal('#OrderDetailsDto_UnitPrice', data.fobAmount).prop('disabled', true);

            // 🟣 Multiselects
            setVal('#OrderDto_BuyerId', data.buyerId).multiselect('rebuild');
            setVal('#OrderDto_SeasonId', data.seasonId).multiselect('rebuild');
            setVal('#OrderDto_CurrencyId_FOB', data.currencyId_FOB).multiselect('rebuild');
            setVal('#OrderDto_SupplierId', data.supplierId).multiselect('rebuild');
            setVal('#OrderDto_UnitTypID', data.unitTypID).multiselect('rebuild');
            setVal('#OrderDetailsDto_POUnitTypID', data.unitTypID).multiselect('rebuild').multiselect('disable');
            setVal('#OrderDto_CurrencyId', data.currencyId).multiselect('rebuild');
            setVal('#OrderDetailsDto_CurrencyId', data.currencyId_FOB).multiselect('rebuild').multiselect('disable');
            setVal('#totalAmountCurrency', data.currencyId_FOB).multiselect('rebuild').multiselect('disable');
            setVal('#OrderDto_BuyerBankId', data.buyerBankId).multiselect('rebuild');
            setVal('#OrderDto_CompanyOwnBankId', data.companyOwnBankId).multiselect('rebuild');
            setVal('#OrderDto_POStatusId', data.poStatusId).multiselect('rebuild');
            setVal('#OrderDto_StyleId', data.styleId).multiselect('rebuild');
            setVal('#OrderDetailsDto_Style', data.styleId).multiselect('rebuild').multiselect('disable');
            $('.showCreateDateOrderInfo').empty().text(data.showCreateDate);
            $('.showModifyDateOrderInfo').empty().text(data.showModifyDate);

            setTimeout(function () {
                setVal('#OrderDto_BuyerBrand', data.buyerBrand).multiselect('rebuild');
                setVal('#OrderDto_BuyerBranchId', data.buyerBranchId).multiselect('rebuild');
                setVal('#OrderDto_CompanyOwnBranchId', data.companyOwnBranchId).multiselect('rebuild');
            }, 500);

            // Multi-select Array Fields
            if (Array.isArray(data.buContatPerson)) {
                $('#OrderDto_BuContatPerson').val(data.buContatPerson).multiselect('rebuild');
            } else {
                $('#OrderDto_BuContatPerson').val([]).multiselect('rebuild');
            }

            if (Array.isArray(data.merchandiserContactId)) {
                //$('#OrderDto_MerchandiserContactId').val(data.merchandiserContactId).multiselect('rebuild');
                populateMerchandiser(data.merchandiserContactId);
            } else {
                populateMerchandiser([]);
            }

            // 🔹 StylePOWise Radio toggle
            if (data.stylePOWise === "Style Wise") {
                $('#styleWise').prop('checked', true);
                $('.styleWiseRow').show();
                $('.masterPoWise').hide();
            } else if (data.stylePOWise === "P.O Wise") {
                $('#poWise').prop('checked', true);
                $('.styleWiseRow').hide();
                $('.masterPoWise').show();
            }

            console.log("✅ Order Info Populated Successfully:", data);
        }

        $(document).on('change', '#OrderDetailsDto_CurrencyId', function () {
            var id = $(this).val();
            console.log(id);
            $("#totalAmountCurrency").val(id).multiselect('rebuild').multiselect('disable');
        })
        $(document).on('change', '#totalAmountCurrency', function () {
            var id = $(this).val();
            console.log(id);
            $("#OrderDetailsDto_CurrencyId").val(id).multiselect('rebuild').multiselect('disable');
        })

        function populateMerchandiser(data) {
            if (Array.isArray(data) && data.length > 0) {
                selectedIds = data.map(String); // ensure string type
            } else {
                selectedIds = [];
            }

            // ✅ Rebuild DataTable checkboxes
            if ($.fn.DataTable.isDataTable('#employeeTable')) {
                $('#employeeTable').DataTable().rows().every(function () {
                    const rowId = String(this.data().id);
                    $(this.node()).find('.row-check').prop('checked', selectedIds.includes(rowId));
                });
            }

            // ✅ Update dropdown placeholder text
            employees = employees || [];
            if (!Array.isArray(employees)) {
                employees = Object.values(employees);
            }
            const select = $('#merchandiserContactPerson');
            const displayText = selectedIds.length === 0
                ? "Select merchandisers"
                : selectedIds.length === 1
                    ? employees.find(emp => emp.id === selectedIds[0])?.name
                    : `${selectedIds.length} selected`;

            // Update placeholder option without removing original options
            if (select.find('option[data-placeholder]').length === 0) {
                select.prepend(`<option data-placeholder value="">${displayText}</option>`);
            } else {
                select.find('option[data-placeholder]').text(displayText);
            }

            select.val(""); // keep placeholder selected
            //select.multiselect('rebuild');

            // ✅ Optional: console log
            console.log("Selected merchandisers:", selectedIds);
        }

        // ✅ Save button click

        $(document).on('click', '.js-order-info-save', function () {

            if (isOrderInfo) {

                const fromData = getOrderInfoData();
                console.log("Sending to server:", fromData);

                if (!fromData.BuyerId) {
                    // Just button e click trigger koro
                    $("#OrderDto_BuyerId").next('.btn-group').find('.multiselect').trigger('click');
                    return;
                }

                if (!fromData.TotalOrderQuantity) {
                    $("#OrderDto_TotalOrderQuantity").focus().addClass('border border-danger');
                    $(".js-order-info-save").addClass('boder-0').prop('disabled', true);
                    return;
                }


                $.ajax({
                    url: '/RMGProdOrderInformationEntry/OrderSaveEdit',
                    type: "POST",
                    contentType: 'application/json',
                    data: JSON.stringify(fromData),
                    success: function (res) {
                        console.log("Server Response:", res);
                        showToast(`${res.isSuccess ? "success" : "error"}`, res.message);
                        if (res.isSuccess) {
                            //
                            ReloadIndex();
                            GridOrderInfo();
                            clearOrderInfoForm();
                        }

                    },
                    error: function (xhr, status, error) {
                        console.error("Error:", error);
                    }
                });
            }
            else if (isDetails) {
                orderDetailsFun();
            } 
        });


        function ReloadIndex() {
            $.ajax({
                url: '/RMGProdOrderInformationEntry/ReloadViewData',
                type: "GET",
                success: function (res) {
                    console.log("Reload Data:", res);

                    const $select = $("#OrderDetailsDto_IntegraJobNO");

                    $select.empty().append('<option value="" disabled hidden>Select Intregra Job No</option>');

                    $.each(res.integraJobNoList, function (i, item) {
                        $select.append(`<option value="${item.id}">${item.name}</option>`);
                    });

                    $select.multiselect('rebuild');
                },
                error: function (xhr, status, error) {
                    console.error("Reload Error:", error);
                }
            });
        }



        $(document).on('click', '#js-order-info-clear', function () {

            if (isOrderInfo) {
                clearOrderInfoForm();
                
            }
        })



        $(document).ready(function () {
            autoEntryId();
            IntegraJOBNoAuto();
            GridOrderInfo();
        })





        //details

        $(document).ready(function () {
            function validatePercentages() {
                // Get all three percentage values
                let p1 = parseFloat($("#OrderDetailsDto_Percentage1").val()) || 0;
                let p2 = parseFloat($("#OrderDetailsDto_Percentage2").val()) || 0;
                let p3 = parseFloat($("#OrderDetailsDto_Percentage3").val()) || 0;

                // Total
                let total = p1 + p2 + p3;

                // Remove previous error styles
                $("#OrderDetailsDto_Percentage1, #OrderDetailsDto_Percentage2, #OrderDetailsDto_Percentage3")
                    .removeClass("is-invalid border-danger");

                // ✅ Individual range validation
                if (p1 < 0 || p1 > 100) $("#OrderDetailsDto_Percentage1").addClass("is-invalid");
                if (p2 < 0 || p2 > 100) $("#OrderDetailsDto_Percentage2").addClass("is-invalid");
                if (p3 < 0 || p3 > 100) $("#OrderDetailsDto_Percentage3").addClass("is-invalid");

                // ✅ Total validation
                if (total > 100 || total < 0) {
                    showToast('warning', "Total percentage cannot exceed 100%.");
                    // Highlight all inputs
                    $("#OrderDetailsDto_Percentage1, #OrderDetailsDto_Percentage2, #OrderDetailsDto_Percentage3")
                        .addClass("is-invalid border-danger");
                }
            }

            // Validate on input change
            $(document).on("input", "#OrderDetailsDto_Percentage1, #OrderDetailsDto_Percentage2, #OrderDetailsDto_Percentage3", function () {
                validatePercentages();
            });
        });



        function getOrderDetailsData() {
            const parseDecimal = val => val ? parseFloat(val) : null;
            const parseIntOrNull = val => val ? parseInt(val) : null;
            const parseDate = val => val ? new Date(val).toISOString() : null;

            const data = {
                TC: parseDecimal($("#OrderDetailsDto_TC").val()),
                DetailOrderId: $("#OrderDetailsDto_DetailOrderId").val(),
                OrderId: $("#OrderDetailsDto_OrderId").val(),
                Date: parseDate($("#OrderDetailsDto_Date").val()),
                ProductId: $("#OrderDetailsDto_ProductId").val(),
                Description: $("#OrderDetailsDto_Description").val(),
                BrandId: $("#OrderDetailsDto_BrandId").val(),
                Style: $("#OrderDetailsDto_Style").val(),
                RefNo: $("#OrderDetailsDto_RefNo").val(),
                HSCode: $("#OrderDetailsDto_HSCode").val(),
                PurchaseOrder: $("#OrderDetailsDto_PurchaseOrder").val(),
                PODate: parseDate($("#OrderDetailsDto_PODate").val()),
                OrderQuantity: parseIntOrNull($("#OrderDetailsDto_OrderQuantity").val()),
                POUnitTypID: $("#OrderDetailsDto_POUnitTypID").val(),
                UnitPrice: parseDecimal($("#OrderDetailsDto_UnitPrice").val()),
                CurrencyId: $("#OrderDetailsDto_CurrencyId").val(),
                TotalAmount: parseDecimal($("#OrderDetailsDto_TotalAmount").val()),
                MaterialInfo: $("#OrderDetailsDto_MaterialInfo").val(),
                PrintingInstruction: $("#OrderDetailsDto_PrintingInstruction").val(),
                WashingInstruction: $("#OrderDetailsDto_WashingInstruction").val(),
                LabelInstruction: $("#OrderDetailsDto_LabelInstruction").val(),
                PackagingInstruction: $("#OrderDetailsDto_PackagingInstruction").val(),
                OtherInstruction: $("#OrderDetailsDto_OtherInstruction").val(),
                DeliveryDate: parseDate($("#OrderDetailsDto_DeliveryDate").val()),
                DeliveryAddress: $("#OrderDetailsDto_DeliveryAddress").val(),
                DeliveryTerm: $("#OrderDetailsDto_DeliveryTerm").val(),
                DeliveryMethod: $("#OrderDetailsDto_DeliveryMethod").val(),
                PortOfLoading: $("#OrderDetailsDto_PortOfLoading").val(),
                PortOfDischarge: $("#OrderDetailsDto_PortOfDischarge").val(),
                SupplierId: $("#OrderDetailsDto_SupplierId").val(),
                PaymentTermsId: $("#OrderDetailsDto_PaymentTermsId").val(),
                GarmentsTesting: $("#OrderDetailsDto_GarmentsTesting").val(),
                GarmentsInstruction: $("#OrderDetailsDto_GarmentsInstruction").val(),
                GarmentReminderDay: $("#OrderDetailsDto_GarmentReminderDay").val(),
                GarmentReminderType: $("#OrderDetailsDto_GarmentReminderType").val(),
                GarmnetRemainderMail: $("#OrderDetailsDto_GarmnetRemainderMail").val(),
                IsGarmentTestRecieved: $("#OrderDetailsDto_IsGarmentTestRecieved").val(),
                GarmentTestAttachment: $("#OrderDetailsDto_GarmentTestAttachment").val(),
                FebricTesting: $("#OrderDetailsDto_FebricTesting").val(),
                FebricInstruction: $("#OrderDetailsDto_FebricInstruction").val(),
                FebricReminderDay: $("#OrderDetailsDto_FebricReminderDay").val(),
                FebricReminderType: $("#OrderDetailsDto_FebricReminderType").val(),
                FebricRemainderMail: $("#OrderDetailsDto_FebricRemainderMail").val(),
                IsFebricTestRecieved: $("#OrderDetailsDto_IsFebricTestRecieved").val(),
                FebricTestAttachment: $("#OrderDetailsDto_FebricTestAttachment").val(),
                TransportNo: $("#OrderDetailsDto_TransportNo").val(),
                IntegraJobNO: $("#OrderDetailsDto_IntegraJobNO").val(),
                MasterPurchaseOrder: $("#OrderDetailsDto_MasterPurchaseOrder").val(),
                Percentage1: parseDecimal($("#OrderDetailsDto_Percentage1").val()),
                DeliveryMethod2: $("#OrderDetailsDto_DeliveryMethod2").val(),
                Percentage2: parseDecimal($("#OrderDetailsDto_Percentage2").val()),
                DeliveryMethod3: $("#OrderDetailsDto_DeliveryMethod3").val(),
                Percentage3: parseDecimal($("#OrderDetailsDto_Percentage3").val()),
                XFactoryDate: parseDate($("#OrderDetailsDto_XFactoryDate").val())
            };

            return data;
        }


        //$(document).ready(function () {
        //    if ($.fn.DataTable.isDataTable('#orderDetailsGrid')) {
        //        $('#orderDetailsGrid').DataTable().destroy();
        //    }
        //    $('#orderDetailsGrid').DataTable({
        //        processing: true,
        //        serverSide: true,
        //        ajax: {
        //            url: '/RMGProdOrderInformationEntry/GetOrderDetailsList',
        //            type: 'POST',
        //            dataSrc: function (json) {
        //                console.log("✅ Order Details Data:", json);
        //                return json.data;
        //            }
        //        },
        //        columns: [
        //            {
        //                data: null,
        //                render: function (data, type, row) {
        //                    return `<input type="checkbox" class="details-select" data-id="${row.tc}" />`;
        //                },
        //                orderable: false,
        //                searchable: false
        //            },
        //            { data: "detailOrderId" },
        //            { data: "purchaseOrder" },
        //            { data: "productId" },
        //            { data: "description" },
        //            { data: "supplierId" },
        //            { data: "orderQuantity" },
        //            { data: "poUnitTypID" },
        //            { data: "integraJobNO" }
        //        ],
        //        columnDefs: [
        //            { width: "50px", targets: 0 },
        //            { className: "text-center align-middle", targets: "_all" }
        //        ]
        //    });
        //});

        let selectedDetailsIds = [];
        //function GridOrderDetails () {

        //    // ✅ Selected IDs রাখার জন্য


        //    // ✅ DataTable Destroy if exists
        //    if ($.fn.DataTable.isDataTable('#orderDetailsGrid')) {
        //        $('#orderDetailsGrid').DataTable().destroy();
        //    }

        //    // ✅ DataTable Initialization
        //    const table = $('#orderDetailsGrid').DataTable({
        //        processing: true,
        //        serverSide: true,
        //        ajax: {
        //            url: '/RMGProdOrderInformationEntry/GetOrderDetailsList',
        //            type: 'POST',
        //            dataSrc: function (json) {
        //                console.log("✅ Order Details Data:", json);
        //                return json.data;
        //            }
        //        },
        //        columns: [
        //            {
        //                data: null,
        //                render: function (data, type, row) {
        //                    return `<input type="checkbox" class="details-select" data-id="${row.tc}" />`;
        //                },
        //                orderable: false,
        //                searchable: false
        //            },
        //            {
        //                data: "detailOrderId",
        //                render: function (data, type, row) {
        //                    return `<a href="#" class="detail-link" data-row='${JSON.stringify(row)}'>${data}</a>`;
        //                }
        //            },
        //            { data: "purchaseOrder" },
        //            { data: "productId" },
        //            { data: "description" },
        //            { data: "supplierId" },
        //            { data: "orderQuantity" },
        //            { data: "poUnitTypID" },
        //            { data: "integraJobNO" }
        //        ],
        //        columnDefs: [
        //            { width: "50px", targets: 0 },
        //            { className: "text-center align-middle", targets: "_all" }
        //        ]
        //    });
        //};
        function GridOrderDetails(integraJobNo = null) {
            if ($.fn.DataTable.isDataTable('#orderDetailsGrid')) {
                $('#orderDetailsGrid').DataTable().destroy();
            }

            $('#orderDetailsGrid').DataTable({
                processing: true,
                serverSide: true,
                ajax: {
                    url: '/RMGProdOrderInformationEntry/GetOrderDetailsList',
                    type: 'POST',
                    data: function (d) {
                        d.integraJobNo = integraJobNo; // server-এ পাঠানো হচ্ছে
                    },
                    dataSrc: function (json) {
                        console.log("✅ Filtered Order Details:", json);
                        return json.data;
                    }
                },
                columns: [
                    {
                        data: null,
                        render: function (data, type, row) {
                            return `<input type="checkbox" class="details-select" data-id="${row.tc}" />`;
                        },
                        orderable: false,
                        searchable: false
                    },
                    {
                        data: "detailOrderId",
                        render: function (data, type, row) {
                            return `<a href="#" class="detail-link" data-row="${JSON.stringify(row).replace(/"/g, '&quot;')}">${data}</a>`;
                        }
                    },
                    { data: "purchaseOrder" },
                    { data: "productId" },
                    { data: "description" },
                    { data: "supplierId" },
                    { data: "orderQuantity" },
                    { data: "poUnitTypID" },
                    { data: "integraJobNO" }
                ],
                columnDefs: [
                    { width: "50px", targets: 0 },
                    { className: "text-center align-middle", targets: "_all" }
                ]
            });
        }

        //// 🔥 detail-link click handle
        //$('#orderDetailsGrid').on('click', '.detail-link', function (e) {
        //    e.preventDefault();

        //    // clicked element থেকে safe JSON string নিয়ে parse করা
        //    const rawData = $(this).attr('data-row');
        //    try {
        //        const rowData = JSON.parse(rawData);
        //        const jobNo = rowData.integraJobNO;

        //        console.log("✅ JobNo:", jobNo);
        //        console.log("✅ Populated Order Details:", rowData);
        //        console.log(rowData.purchaseOrder);
        //        $("#TempColorSizeBreakupDtoPONo").multiselect('rebuild');

        //        setTimeout(() => {
        //            $("#TempColorSizeBreakupDtoPONo")
        //                .val(rowData.purchaseOrder)
        //                .multiselect('refresh')
        //                .multiselect('disable');
        //        }, 300);


        //        $("#TempColorSizeBreakupDtoStyle").val(rowData.style).multiselect('rebuild').multiselect('disable');
        //        $("#TempColorSizeBreakupDto_IntegraJOBNo").val(rowData.integraJobNO).multiselect('rebuild').multiselect('disable');
        //        // table filter করতে পারো বা populate function কল করতে পারো
        //        populateOrderDetailsData(rowData);

        //        // চাইলে আবার filter করে table reload করতে
        //        // GridOrderDetails(jobNo);

        //        getTempcolorBreakUp(rowData.purchaseOrder, rowData.integraJobNO);

        //    } catch (err) {
        //        console.error("❌ JSON parse error:", err, rawData);
        //    }
        //});

        // 🔥 detail-link click handle
        $('#orderDetailsGrid').on('click', '.detail-link', function (e) {
            e.preventDefault();

            const rawData = $(this).attr('data-row');
            try {
                const rowData = JSON.parse(rawData);
                const jobNo = rowData.integraJobNO;

                console.log("✅ JobNo:", jobNo);
                console.log("✅ Populated Order Details:", rowData);
                console.log(rowData.purchaseOrder);

                // ✅ প্রথমে data populate করুন (এটা dropdown options load করবে)
                populateOrderDetailsData(rowData);

                // ✅ তারপর একটু delay দিয়ে multiselect set করুন
                setTimeout(() => {
                  
                    // PO Number
                    if ($("#TempColorSizeBreakupDtoPONo").length) {
                        $("#TempColorSizeBreakupDtoPONo")
                            .multiselect('destroy')  // পুরনো instance destroy
                            .val(rowData.purchaseOrder)
                            .multiselect('rebuild')
                            .multiselect('refresh')
                            .multiselect('disable');
                    }

                    // Style
                    if ($("#TempColorSizeBreakupDtoStyle").length) {
                        $("#TempColorSizeBreakupDtoStyle")
                            .multiselect('destroy')
                            .val(rowData.style)
                            .multiselect('rebuild')
                            .multiselect('refresh')
                            .multiselect('disable');
                    }

                    // Integra Job No
                    if ($("#TempColorSizeBreakupDto_IntegraJOBNo").length) {
                        $("#TempColorSizeBreakupDto_IntegraJOBNo")
                            .multiselect('destroy')
                            .val(rowData.integraJobNO)
                            .multiselect('rebuild')
                            .multiselect('refresh')
                            .multiselect('disable');
                    }

                    console.log("✅ Multiselect values set করা হয়েছে");
                }, 500); // 300ms থেকে 500ms বাড়ালাম

                // Color breakup load
                getTempcolorBreakUp(rowData.purchaseOrder, rowData.integraJobNO);

            } catch (err) {
                console.error("❌ JSON parse error:", err, rawData);
            }
        });


        //function getTempcolorBreakUp(poId, ijobno) {
        //    const tempData = {
        //        poId: poId,
        //        ijobno: ijobno
        //    };
        //    $.ajax({
        //        url: '/RMGProdOrderInformationEntry/PoIjobNoGetTemp',
        //        type: "POST",
        //        contentType: 'application/json',
        //        data: JSON.stringify(tempData),
        //        success: function (res) {
        //            console.log(res);
        //            //getTempcolorBreakUp();
        //            gridColorSizeBreakup();
        //            loadColorSizeTable();
        //        }
        //    });
        //}


        function getTempcolorBreakUp(poId, ijobno) {
            const tempData = {
                poId: poId,
                ijobno: ijobno
            };

            $.ajax({
                url: '/RMGProdOrderInformationEntry/PoIjobNoGetTemp',
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(tempData),
                success: function (res) {
                    console.log("✅ Response:", res);

                    if (res.isSuccess) {
                        // ✅ প্রথমে dropdown ready কিনা নিশ্চিত হও
                        setTimeout(() => {
                            setMultiselectValues(
                                '#TempColorSizeBreakupDto_ColorId',
                                res.colorIds,
                                'Colors'
                            );
                            setMultiselectValues(
                                '#TempColorSizeBreakupDto_SizeId',
                                res.sizeIds,
                                'Sizes'
                            );
                        }, 300);

                        // Refresh grids
                        gridColorSizeBreakup();
                        loadColorSizeTable();
                    } else {
                        console.error("❌ Error:", res.message);
                        showToast("error", res.message);
                    }
                },
                error: function (xhr, status, error) {
                    console.error("❌ AJAX Error:", error);
                    showToast("error", "Failed to load color/size data");
                }
            });
        }



        // ✅ Helper function to set multiselect values safely
        function setMultiselectValues(selector, values, label) {
            const $element = $(selector);

            if (!$element.length) {
                console.warn(`⚠️ Element not found: ${selector}`);
                return;
            }

            if (!values || values.length === 0) {
                console.warn(`⚠️ No ${label} to select`);
                return;
            }

            try {
                // ✅ Values সেট করো
                $element.val(values);

                // ✅ Refresh করে UI তে checkbox select দেখাও
                $element.multiselect('refresh');

                console.log(`✅ Selected ${label}:`, values);
            } catch (error) {
                console.error(`❌ Error setting ${label}:`, error);
            }
        }



        //function GridOrderDetails() {
        //    if ($.fn.DataTable.isDataTable('#orderDetailsGrid')) {
        //        $('#orderDetailsGrid').DataTable().destroy();
        //    }

        //    const selectedJobNo = $("#OrderDetailsDto_IntegraJobNO").val();

        //    $('#orderDetailsGrid').DataTable({
        //        processing: true,
        //        serverSide: true,
        //        ajax: {
        //            url: '/RMGProdOrderInformationEntry/GetOrderDetailsList',
        //            type: 'POST',
        //            data: function (d) {
        //                d.integraJobNo = selectedJobNo; 
        //            },
        //            dataSrc: function (json) {
        //                console.log("✅ Filtered Order Details:", json);
        //                return json.data;
        //            }
        //        },
        //        columns: [
        //            {
        //                data: null,
        //                render: function (data, type, row) {
        //                    return `<input type="checkbox" class="details-select" data-id="${row.tc}" />`;
        //                },
        //                orderable: false,
        //                searchable: false
        //            },
        //            {
        //                data: "detailOrderId",
        //                render: function (data, type, row) {
        //                    return `<a href="#" class="detail-link" data-row='${JSON.stringify(row)}'>${data}</a>`;
        //                }
        //            },
        //            { data: "purchaseOrder" },
        //            { data: "productId" },
        //            { data: "description" },
        //            { data: "supplierId" },
        //            { data: "orderQuantity" },
        //            { data: "poUnitTypID" },
        //            { data: "integraJobNO" }
        //        ],
        //        columnDefs: [
        //            { width: "50px", targets: 0 },
        //            { className: "text-center align-middle", targets: "_all" }
        //        ]
        //    });
        //}

        // 🔁 Dropdown change হলে table reload
        $(document).on('change', '#OrderDetailsDto_IntegraJobNO', function () {
            var id = $(this).val();//toto style

            $.ajax({
                url: '/RMGProdOrderInformationEntry/IntJobNoByStyle',
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(id),
                success: function (res) {
                    console.log(res);//totojob
                    if (res) {
                        $("#OrderDetailsDto_Style").val(res.styleId).multiselect('rebuild').multiselect('disable');
                        $("#OrderDetailsDto_POUnitTypID").val(res.unitTypId).multiselect('rebuild').multiselect('disable');
                        $("#OrderDetailsDto_CurrencyId").val(res.currencyIdFob).multiselect('rebuild').multiselect('disable');
                        $("#totalAmountCurrency").val(res.currencyIdFob).multiselect('rebuild').multiselect('disable');
                        $("#OrderDetailsDto_UnitPrice").empty().val(res.fobamount).prop('disabled', true);       
                    }
                               
                }
            });

            GridOrderDetails(id);
           
        });


        // ✅ Individual Checkbox Selection
        $(document).on('change', '.details-select', function () {
            const id = $(this).data('id');
            if ($(this).is(':checked')) {
                if (!selectedDetailsIds.includes(id)) selectedDetailsIds.push(id);
            } else {
                selectedDetailsIds = selectedDetailsIds.filter(x => x !== id);
            }
            console.log("Selected IDs:", selectedDetailsIds);
        });

        // ✅ Select All Checkbox
        $(document).on('change', '#orderDetails-check-all', function () {
            const isChecked = $(this).is(':checked');
            $('.details-select').prop('checked', isChecked).trigger('change');
        });

        // ✅ Link Click — পুরো Row Data Console এ
        //$(document).on('click', '.detail-link', function (e) {
        //    e.preventDefault();
        //    const rowData = $(this).data('row');
        //    console.log("🔹 Full Row Data:", rowData);

        //    // চাইলে এখানে populate function call করতে পারো 👇
        //    populateOrderDetailsData(rowData);
        //});

        $(document).on("click", "#js-order-info-delete-confirm", function () {
            //debugger;
            if (isDetails) {
                $.ajax({
                    url: '/RMGProdOrderInformationEntry/DeleteOrderDetails',
                    type: 'POST',
                    data: JSON.stringify(selectedDetailsIds),
                    contentType: 'application/json',
                    success: function (res) {
                        showToast(`${res.isSuccess ? "success" : "error"}`, res.message);
                        if (res.isSuccess) {
                            debugger;
                            clearOrderDetailsData();
                            GridOrderDetails(null);
                        }
                    }
                });
            }
        })

        function LoadPoStyleJob() {
          
                $.ajax({
                    url: '/RMGProdOrderInformationEntry/LoadPoStyleJobLoad',
                    type: 'GET',
                    success: function (res) {
                        console.log("✅ Loaded Data:", res);

                        if (res.isSuccess) {
                            // PO Number dropdown populate
                            populateMultiselect('#TempColorSizeBreakupDtoPONo', res.poList);

                            // Style dropdown populate
                            populateMultiselect('#TempColorSizeBreakupDtoStyle', res.styleList);

                            // Integra Job No dropdown populate
                            populateMultiselect('#TempColorSizeBreakupDto_IntegraJOBNo', res.integraJobNoList);

                        } 
                    },
                    error: function (error) {
                        console.error("❌ Load Error:", error);
                        reject(error);
                    }
                });
           
        }

        // ✅ Multiselect populate করার helper function
        function populateMultiselect(selector, dataList) {
            const $element = $(selector);

            if (!$element.length) {
                console.warn(`⚠️ Element not found: ${selector}`);
                return;
            }

            // Multiselect destroy করুন (যদি আগে থেকে থাকে)
            try {
                $element.multiselect('destroy');
            } catch (e) {
                // ignore if not initialized
            }

            // Options clear করুন
            $element.empty();

            // নতুন options add করুন
            if (dataList && dataList.length > 0) {
                // Default option (optional)
                $element.append('<option value="">-- Select --</option>');

                dataList.forEach(item => {
                    $element.append(`<option value="${item.id}">${item.name}</option>`);
                });
            }

            // Multiselect initialize করুন
            $element.multiselect({
                includeSelectAllOption: false,
                enableFiltering: true,
                enableCaseInsensitiveFiltering: true,
                maxHeight: 300,
                buttonWidth: '100%'
            });

            console.log(`✅ Populated ${selector} with ${dataList?.length || 0} items`);
        }

        // ✅ Populate Function (Row থেকে Form Fill করা)
        function populateOrderDetailsData(data) {
            const setVal = (selector, value) => $(selector).val(value ?? '').trigger('change');
            const setDate = (selector, value) => {
                if (value) {
                    const date = new Date(value);
                    const formatted = `${(date.getMonth() + 1)
                        .toString()
                        .padStart(2, '0')}/${date.getDate()
                            .toString()
                            .padStart(2, '0')}/${date.getFullYear()}`;
                    $(selector).val(formatted).trigger('change');
                } else {
                    $(selector).val('').trigger('change');
                }
            };

            // 🔹 Basic Info
            setVal('#OrderDetailsDto_TC', data.tc);
            setVal('#OrderDetailsDto_DetailOrderId', data.detailOrderId).multiselect('rebuild');
            setVal('#OrderDetailsDto_OrderId', data.orderId).multiselect('rebuild');;;
            setDate('#OrderDetailsDto_Date', data.date);
            setVal('#OrderDetailsDto_ProductId', data.productId).multiselect('rebuild');
            setVal('#OrderDetailsDto_Description', data.description);
            setVal('#OrderDetailsDto_BrandId', data.brandId).multiselect('rebuild');
            setVal('#OrderDetailsDto_Style', data.style).multiselect('rebuild');
            setVal('#OrderDetailsDto_RefNo', data.refNo);
            setVal('#OrderDetailsDto_HSCode', data.hsCode);
            setVal('#OrderDetailsDto_PurchaseOrder', data.purchaseOrder).prop('disabled', true);
            setDate('#OrderDetailsDto_PODate', data.poDate);

            // 🔹 Quantity & Pricing
            setVal('#OrderDetailsDto_OrderQuantity', data.orderQuantity);
            setVal('#OrderDetailsDto_PreviousQuantity', data.orderQuantity);
            setVal('#OrderDetailsDto_POUnitTypID', data.poUnitTypID).multiselect('rebuild');
            setVal('#OrderDetailsDto_UnitPrice', data.unitPrice);
            setVal('#OrderDetailsDto_CurrencyId', data.currencyId).multiselect('rebuild');
            setVal('#totalAmountCurrency', data.currencyId).multiselect('rebuild');
            setVal('#OrderDetailsDto_TotalAmount', data.totalAmount);

            // 🔹 Instructions
            setVal('#OrderDetailsDto_MaterialInfo', data.materialInfo);
            setVal('#OrderDetailsDto_PrintingInstruction', data.printingInstruction);
            setVal('#OrderDetailsDto_WashingInstruction', data.washingInstruction);
            setVal('#OrderDetailsDto_LabelInstruction', data.labelInstruction);
            setVal('#OrderDetailsDto_PackagingInstruction', data.packagingInstruction);
            setVal('#OrderDetailsDto_OtherInstruction', data.otherInstruction);

            // 🔹 Delivery
            setDate('#OrderDetailsDto_DeliveryDate', data.deliveryDate);
            setVal('#OrderDetailsDto_DeliveryAddress', data.deliveryAddress);
            setVal('#OrderDetailsDto_DeliveryTerm', data.deliveryTerm);
            setVal('#OrderDetailsDto_DeliveryMethod', data.deliveryMethod).multiselect('rebuild');
            setVal('#OrderDetailsDto_PortOfLoading', data.portOfLoading).multiselect('rebuild');
            setVal('#OrderDetailsDto_PortOfDischarge', data.portOfDischarge).multiselect('rebuild');
            setVal('#TempColorSizeBreakupDto_UnitTypeId', data.poUnitTypID).multiselect('rebuild').multiselect('disable');

            // 🔹 Supplier & Payment
            setVal('#OrderDetailsDto_SupplierId', data.supplierId).multiselect('rebuild');
            setVal('#OrderDetailsDto_PaymentTermsId', data.paymentTermsId).multiselect('rebuild');



            // 🔹 Garments Test Info
            setVal('#OrderDetailsDto_GarmentsTesting', data.garmentsTesting).multiselect('rebuild');
            setVal('#OrderDetailsDto_GarmentsInstruction', data.garmentsInstruction);
            setVal('#OrderDetailsDto_GarmentReminderDay', data.garmentReminderDay);
            setVal('#OrderDetailsDto_GarmentReminderType', data.garmentReminderType).multiselect('rebuild');
            setVal('#OrderDetailsDto_GarmnetRemainderMail', data.garmnetRemainderMail);
            $('#OrderDetailsDto_IsGarmentTestRecieved').prop('checked', data.isGarmentTestRecieved);
            setVal('#OrderDetailsDto_GarmentTestAttachment', data.garmentTestAttachment);

            // 🔹 Fabric Test Info
            setVal('#OrderDetailsDto_FebricTesting', data.febricTesting).multiselect('rebuild');
            setVal('#OrderDetailsDto_FebricInstruction', data.febricInstruction);
            setVal('#OrderDetailsDto_FebricReminderDay', data.febricReminderDay);
            setVal('#OrderDetailsDto_FebricReminderType', data.febricReminderType).multiselect('rebuild');
            setVal('#OrderDetailsDto_FebricRemainderMail', data.febricRemainderMail);
            $('#OrderDetailsDto_IsFebricTestRecieved').prop('checked', data.isFebricTestRecieved);
            setVal('#OrderDetailsDto_FebricTestAttachment', data.febricTestAttachment);

            // 🔹 Others
            setVal('#OrderDetailsDto_TransportNo', data.transportNo);
            //setVal('#OrderDetailsDto_IntegraJobNO', data.integraJobNO).multiselect('rebuild');
            setVal('#OrderDetailsDto_MasterPurchaseOrder', data.masterPurchaseOrder);

            // 🔹 Percentages and Delivery Methods
            setVal('#OrderDetailsDto_Percentage1', data.percentage1);
            setVal('#OrderDetailsDto_DeliveryMethod2', data.deliveryMethod2);
            setVal('#OrderDetailsDto_Percentage2', data.percentage2);
            setVal('#OrderDetailsDto_DeliveryMethod3', data.deliveryMethod3);
            setVal('#OrderDetailsDto_Percentage3', data.percentage3);
            $('.showCreateDateOrderDetails').empty().text(data.showCreateDate);
            $('.showModifyDateOrderDetails').empty().text(data.showModifyDate);
            // 🔹 X-Factory Date
            setDate('#OrderDetailsDto_XFactoryDate', data.xFactoryDate);

            console.log("✅ Populated Order Details:", data);
            $('#Color-Size-Breckup-total').empty().val(data.orderQuantity).prop('disabled', true);
        }

        $(document).on('click', '#js-order-info-clear', function () {

            if (isDetails) {
                clearOrderDetailsData();
            }
        })


        //$(document).on('change', "#OrderDetailsDto_ProductId", function () {
        //    var productId = $(this).val();
        //    console.log(buyerId);
        //    $.ajax({
        //        url: '/RMGProdOrderInformationEntry/itemAddress',
        //        type: "POST",
        //        contentType: 'application/json',
        //        data: JSON.stringify(productId),
        //        success: function (res) {
        //            console.log(res);
        //            var $productDescription = $("#OrderDetailsDto_Description");
        //            $branchSelect.empty();
        //            $b
        //        }
        //    });
        //})

        // Clear all form fields
        function clearOrderDetailsData() {
            const clearDate = (selector) => {
                const flatpickrInstance = $(selector)[0]?._flatpickr;
                if (flatpickrInstance) {
                    flatpickrInstance.clear();         // clear the existing date
                    flatpickrInstance.setDate("today"); // reset to default date (today)
                } else {
                    $(selector).val('').trigger('change'); // fallback
                }
            };
            const clearVal = (selector) => $(selector).val('').trigger('change');
            //const clearDate = (selector) => $(selector).val('').trigger('change');
            const clearCheck = (selector) => $(selector).prop('checked', false).trigger('change');
            $("#OrderDetailsDto_IntegraJobNO").multiselect('enable');
            // 🔹 Basic Info
            $('#OrderDetailsDto_TC').val(0);
            $('#OrderDetailsDto_PreviousQuantity').val(0);
            clearVal('#OrderDetailsDto_DetailOrderId').multiselect('rebuild');
            clearVal('#OrderDetailsDto_OrderId').multiselect('rebuild');
            clearDate('#OrderDetailsDto_Date');
            clearVal('#OrderDetailsDto_ProductId').multiselect('rebuild');
            clearVal('#OrderDetailsDto_Description');
            clearVal('#OrderDetailsDto_BrandId').multiselect('rebuild');
            clearVal('#OrderDetailsDto_Style').multiselect('rebuild');
            clearVal('#OrderDetailsDto_RefNo');
            clearVal('#OrderDetailsDto_HSCode');
            clearVal('#OrderDetailsDto_PurchaseOrder').prop('disabled', false);
            //clearDate('#OrderDetailsDto_PODate');

            // 🔹 Quantity & Pricing
            clearVal('#OrderDetailsDto_OrderQuantity');
            clearVal('#OrderDetailsDto_POUnitTypID').multiselect('rebuild');
            clearVal('#OrderDetailsDto_UnitPrice');
            clearVal('#OrderDetailsDto_CurrencyId').multiselect('rebuild');
            clearVal('#totalAmountCurrency').multiselect('rebuild');
            clearVal('#OrderDetailsDto_TotalAmount');

            // 🔹 Instructions
            clearVal('#OrderDetailsDto_MaterialInfo');
            clearVal('#OrderDetailsDto_PrintingInstruction');
            clearVal('#OrderDetailsDto_WashingInstruction');
            clearVal('#OrderDetailsDto_LabelInstruction');
            clearVal('#OrderDetailsDto_PackagingInstruction');
            clearVal('#OrderDetailsDto_OtherInstruction');

            // 🔹 Delivery
            //clearDate('#OrderDetailsDto_DeliveryDate');

            clearDate('#OrderDetailsDto_DeliveryDate');
            clearDate('#OrderDetailsDto_XFactoryDate');
            clearDate('#OrderDetailsDto_PODate');

            clearVal('#OrderDetailsDto_DeliveryAddress');
            clearVal('#OrderDetailsDto_DeliveryTerm');
            clearVal('#OrderDetailsDto_DeliveryMethod').multiselect('rebuild');
            clearVal('#OrderDetailsDto_PortOfLoading').multiselect('rebuild');
            clearVal('#OrderDetailsDto_PortOfDischarge').multiselect('rebuild');

            // 🔹 Supplier & Payment
            clearVal('#OrderDetailsDto_SupplierId').multiselect('rebuild');
            clearVal('#OrderDetailsDto_PaymentTermsId').multiselect('rebuild');

            // 🔹 Garments Test Info
            clearVal('#OrderDetailsDto_GarmentsTesting').multiselect('rebuild');
            clearVal('#OrderDetailsDto_GarmentsInstruction');
            clearVal('#OrderDetailsDto_GarmentReminderDay');
            clearVal('#OrderDetailsDto_GarmentReminderType').multiselect('rebuild');
            clearVal('#OrderDetailsDto_GarmnetRemainderMail');
            clearCheck('#OrderDetailsDto_IsGarmentTestRecieved');
            clearVal('#OrderDetailsDto_GarmentTestAttachment');

            // 🔹 Fabric Test Info
            clearVal('#OrderDetailsDto_FebricTesting').multiselect('rebuild');
            clearVal('#OrderDetailsDto_FebricInstruction');
            clearVal('#OrderDetailsDto_FebricReminderDay');
            clearVal('#OrderDetailsDto_FebricReminderType').multiselect('rebuild');
            clearVal('#OrderDetailsDto_FebricRemainderMail');
            clearCheck('#OrderDetailsDto_IsFebricTestRecieved');
            clearVal('#OrderDetailsDto_FebricTestAttachment');

            // 🔹 Others
            clearVal('#OrderDetailsDto_TransportNo');
            clearVal('#OrderDetailsDto_IntegraJobNO').multiselect('rebuild');
            clearVal('#OrderDetailsDto_MasterPurchaseOrder');

            // 🔹 Percentages and Delivery Methods
            clearVal('#OrderDetailsDto_Percentage1');
            clearVal('#OrderDetailsDto_DeliveryMethod2');
            clearVal('#OrderDetailsDto_Percentage2');
            clearVal('#OrderDetailsDto_DeliveryMethod3');
            clearVal('#OrderDetailsDto_Percentage3');

            // 🔹 X-Factory Date
            //clearDate('#OrderDetailsDto_XFactoryDate');

            console.log("🧹 Cleared all Order Details fields.");
        }

        $(document).on('change', "#OrderDetailsDto_ProductId", () => {
            $(".js-order-info-save").prop('disabled', false);
        })
        $(document).on('input', "#OrderDetailsDto_OrderQuantity", () => {
            $(".js-order-info-save").prop('disabled', false);
            $("#OrderDetailsDto_OrderQuantity").removeClass('border border-danger');
        })
        $(document).on('input', "#OrderDetailsDto_PurchaseOrder", () => {
            $(".js-order-info-save").prop('disabled', false);
            $("#OrderDetailsDto_PurchaseOrder").removeClass('border border-danger');
        })
        function orderDetailsFun() {
            const fromData = getOrderDetailsData();
            console.log("Sending to server:", fromData);

            if (!fromData.IntegraJobNO) {
                // Just button e click trigger koro
                $("#OrderDetailsDto_IntegraJobNO").next('.btn-group').find('.multiselect').trigger('click');
                return;
            }
       

            if (!fromData.ProductId) {
                $("#OrderDetailsDto_ProductId").next('.btn-group').find('.multiselect').trigger('click');
                $(".js-order-info-save").addClass('boder-0').prop('disabled', true);
                return;
            }
            if (!fromData.PurchaseOrder) {
                // Just button e click trigger koro
                $(".js-order-info-save").prop('disabled', true);
                $("#OrderDetailsDto_PurchaseOrder").focus().addClass('border border-danger');
                return;
            }
            if (!fromData.OrderQuantity) {
                $("#OrderDetailsDto_OrderQuantity").focus().addClass('border border-danger');
                $(".js-order-info-save").addClass('boder-0').prop('disabled', true);
                return;
            }


            $.ajax({
                url: '/RMGProdOrderInformationEntry/DetailsSaveEdit',
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(fromData),
                success: function (res) {
                    console.log("Server Response:", res);
                    showToast(`${res.isSuccess ? "success" : "error"}`, res.message);
                    if (res.isSuccess) {
                        clearOrderDetailsData();
                        GridOrderDetails(null);
                        LoadPoStyleJob();
                    }
                },
                error: function (xhr, status, error) {
                    console.error("Error:", error);
                }
            });
        }



        //color and breakup
        //TempColorSizeBreakupDtoPONo
        function colorAndBreakupFun() {
            const dto = {
                TC: $("#TempColorSizeBreakupDto_TC").val(),
                DetailOrderId: $("#TempColorSizeBreakupDtoPONo").val(),
                PONo: $("#TempColorSizeBreakupDtoPONo").val(),
                ColorIds: $("#TempColorSizeBreakupDto_ColorId").val() || [],
                SizeIds: $("#TempColorSizeBreakupDto_SizeId").val() || [],
                UnitTypeId: $("#TempColorSizeBreakupDto_UnitTypeId").val(),
                Remarks: $("#TempColorSizeBreakupDto_Remarks").val(),
                IntegraJOBNo: $("#TempColorSizeBreakupDto_IntegraJOBNo").val()
            };
            console.log("Sending to server:", dto);
            return dto;
        }
        //$(document).on('click', "#colorAndBreakupSaveBtn", function () {
        //    SaveEditColorAndBreakup();
        //})
        //function SaveEditColorAndBreakup() {

        //    const fromData = colorAndBreakupFun();
        //    console.log("Sending to server:", fromData);

        //    if (fromData.ColorIds.length === 0 || fromData.SizeIds.length === 0) {
        //        alert("Please select at least one Color and one Size.");
        //        return;
        //    }

        //    $.ajax({
        //        url: '/RMGProdOrderInformationEntry/SaveEditColorSizeBreakup',
        //        type: "POST",
        //        contentType: 'application/json',
        //        data: JSON.stringify(fromData),
        //        success: function (res) {
        //            console.log("Server Response:", res);
        //            if (res.isSuccess) {
        //                loadColorSizeTable();
        //            }
        //            showToast(`${res.isSuccess ? "success" : "error"}`, res.message);
        //        },
        //        error: function (xhr, status, error) {
        //            console.error("Error:", error);
        //        }
        //    });
        //};

        $(document).on('click', "#colorAndBreakupSaveBtn", function () {
            if (isColorAndBreakup) {
            SaveEditColorAndBreakupList();
            }
        });
        function SaveEditColorAndBreakupList() {

            const fromData = colorAndBreakupFun();
            console.log("Sending to server:", fromData);

            if (fromData.ColorIds.length === 0 || fromData.SizeIds.length === 0) {
                alert("Please select at least one Color and one Size.");
                return;
            }

            $.ajax({
                url: '/RMGProdOrderInformationEntry/SaveEditColorSizeBreakupList',
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(fromData),
                success: function (res) {
                    console.log("Server Response:", res);
                    if (res.isSuccess) {
                        loadColorSizeTable();
                    }
                    showToast(`${res.isSuccess ? "success" : "error"}`, res.message);
                },
                error: function (xhr, status, error) {
                    console.error("Error:", error);
                }
            });
        };

        $(document).ready(function () {
            loadColorSizeTable();
            GridOrderDetails(null);
            gridColorSizeBreakup();
        })



        function loadColorSizeTable() {
            $.ajax({
                url: '/RMGProdOrderInformationEntry/GetColorSizeBreakups',
                type: 'GET',
                success: function (response) {
                    if (!response.isSuccess) {
                        alert("❌ Failed to load data");
                        return;
                    }

                    let tbody = $("#mainGroupGrid tbody");
                    tbody.empty();

                    const colors = response.dropdowns.colors;
                    const sizes = response.dropdowns.sizes;
                    const units = response.dropdowns.units;

                    response.data.forEach(item => {
                        let colorOptions = "";
                        colors.forEach(c => {
                            const selected = c.id === item.colorId ? "selected" : "";
                            colorOptions += `<option value="${c.id}" ${selected}>${c.name}</option>`;
                        });

                        let sizeOptions = "";
                        sizes.forEach(s => {
                            const selected = s.id === item.sizeId ? "selected" : "";
                            sizeOptions += `<option value="${s.id}" ${selected}>${s.name}</option>`;
                        });

                        let unitOptions = "";
                        units.forEach(u => {
                            const selected = u.id === item.unitTypeId ? "selected" : "";
                            unitOptions += `<option value="${u.id}" ${selected}>${u.name}</option>`;
                        });

                        let row = `
                <tr data-tc="${item.tc}">
                    <td class="text-center align-middle">${item.breakNo}</td>
                    <td class="text-center align-middle">
                        <select class="form-select form-select-sm color-select searchAbleSelectMultiInTable">
                            ${colorOptions}
                        </select>
                    </td>
                    <td class="text-center align-middle">
                        <select class="form-select form-select-sm size-select searchAbleSelectMultiInTable">
                            ${sizeOptions}
                        </select>
                    </td>
                    <td class="text-center align-middle">
                        <input type="number" class="form-control form-control-sm quantity-input" value="${item.quantity || ''}" />
                    </td>
                    <td class="text-center align-middle">
                        <select class="form-select form-select-sm unit-select searchAbleSelectMultiInTable">
                            ${unitOptions}
                        </select>
                    </td>
                    <td class="text-center align-middle">
                        <textarea rows="1" class="form-control form-control-sm remarks-input">${item.remarks || ''}</textarea>
                    </td>
                    <td class="d-flex text-center justify-content-center align-middle">                   
                        <button type="button" class="btn btn-sm btn-default danger-btn delete-btn"><i class="fa fa-trash"></i></button>
                    </td>
                </tr>`;
                        tbody.append(row);
                    });

                    // 🔹 Reinitialize Bootstrap Multiselect
                    $('.searchAbleSelectMultiInTable').multiselect('destroy').multiselect({
                        appendTo: 'body',
                        includeSelectAllOption: true,
                        enableFiltering: true,
                        enableCaseInsensitiveFiltering: true,
                        buttonWidth: '100%'
                    });

                    // 🔹 Recalculate total initially after load
                    //calculateTotalQuantity();
                },
                error: function () {
                    alert("❌ Error while fetching breakup data.");
                }
            });
        }

        // 🧮 Calculate total quantity and validate
        function calculateTotalQuantity() {
            let total = 0;

            $(".quantity-input").each(function () {
                total += parseFloat($(this).val()) || 0;
            });

            console.log("🔹 Total Quantity:", total);

            // Total quantity field update
            $("#Color-Size-Breckup-input")
                .val(total)
                .prop('disabled', true);

            // Limit quantity from hidden or input field
            const totalQty = parseFloat($("#Color-Size-Breckup-total").val()) || 0;

            // Reset previous warning state
            $(".quantity-input").removeClass('border border-danger');
            $('.js-order-info-save').prop('disabled', false);
            // Validate total
            if (total > totalQty) {
                showToast("warning", "❌ Quantity limit exceeded!");
                $(".quantity-input").addClass('border border-danger');
                $('.js-order-info-save').prop('disabled', true);
            }
        }

        // 🎯 Trigger calculation on quantity input change
        $(document).on('input', '.quantity-input', function () {
            calculateTotalQuantity();
        });


        // 🗑️ Delete row and update total
        //$(document).on('click', '.delete-btn', function () {
        //    if (confirm("Are you sure you want to delete this row?")) {
        //        $(this).closest('tr').remove();
        //        calculateTotalQuantity();
        //    }
        //});



        $(document).on('click', '.color-breakup-temp-btn', function () {
            let allData = [];

            $("#mainGroupGrid tbody tr").each(function () {
                let row = $(this);
                console.log(row);
                let dto = {
                    TC: row.data('tc'),
                    ColorId: row.find('.color-select').val(),
                    SizeId: row.find('.size-select').val(),
                    Quantity: parseFloat(row.find('.quantity-input').val()) || 0,
                    UnitTypeId: row.find('.unit-select').val(),
                    Remarks: row.find('.remarks-input').val()
                };
                allData.push(dto);
            });

            console.log(allData);
            if (allData.length === 0) {
                alert("No data found to update!");
                return;
            }

            $.ajax({
                url: '/RMGProdOrderInformationEntry/UpdateColorSizeBreakups',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(allData),
                success: function (res) {
                    if (res.isSuccess) {
                        showToast('success', res.message);
                    } else {
                        showToast('danger', res.message);
                    }
                },
                error: function () {
                    showToast('danger', "❌ Update failed!");
                }
            });
        });


        $(document).on('click', '.delete-btn', function () {
            //if (!confirm("Are you sure you want to delete this row?")) return;

            let row = $(this).closest('tr');
            let tc = row.data('tc');

            $.ajax({
                url: '/RMGProdOrderInformationEntry/DeleteColorSizeBreakup',
                type: 'POST',
                data: { tc },
                success: function (res) {
                    if (res.isSuccess) {
                        row.remove();
                        showToast("success", "🗑️ Row deleted successfully!");
                        calculateTotalQuantity();
                    } else {
                        alert(res.message);
                    }
                },
                error: function () {
                    alert("❌ Delete failed.");
                }
            });
        });


        function gridColorSizeBreakup () {
            if ($.fn.DataTable.isDataTable('#colorAndSizeBrekupGrid')) {
                $('#colorAndSizeBrekupGrid').DataTable().destroy();
            }
            $('#colorAndSizeBrekupGrid').DataTable({
                processing: true,
                serverSide: true,
                ajax: {
                    url: '/RMGProdOrderInformationEntry/GetColorSizeBreakupList',
                    type: 'POST',
                    dataSrc: function (json) {
                        console.log("✅ Color/Size Breakup Data:", json);
                        return json.data;
                    }
                },
                columns: [
                    {
                        data: null,
                        render: function (data, type, row) {
                            return `<input type="checkbox" class="colorSize-select" data-id="${row.tc}" />`;
                        },
                        orderable: false,
                        searchable: false
                    },
                    { data: "breakNo" },
                    { data: "colorId" },
                    { data: "sizeId" },
                    { data: "quantity" },
                    { data: "unitTypeId" },
                    { data: "remarks" }
                ],
                columnDefs: [
                    { width: "10px", targets: 0 },
                    { className: "text-center align-middle", targets: "_all" }
                ]

            });
        };

        //save color and breakup list

        $(document).on('change', "#TempColorSizeBreakupDto_IntegraJOBNo", function () {
            var IJNo = $(this).val();
            $.ajax({
                url: '/RMGProdOrderInformationEntry/GetColorSizeBreakupIntegraJobNo',
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(IJNo),
                success: function (res) {
                    console.log(res);
                }, error: function (e) {
                    console.log(e);
                }
            });
        })




        // ✅ SAVE BUTTON
        $(".js-order-info-save").on("click", function () {
            if (isColorAndBreakup) {

           
            //const integraJobNo = $("#OrderDetailsDto_IntegraJobNO").val();
            //if (!integraJobNo) {
            //    alert("⚠️ Please select an Integra Job No first!");
            //    return;
            //}

            //$.ajax({
            //    url: '/RMGProdOrderInformationEntry/SaveFromTempToMain',
            //    type: 'POST',
            //    data: { integraJobNo: integraJobNo },
            //    success: function (response) {
            //        if (response.isSuccess) {
            //            alert(response.message);
            //            loadColorSizeTable(); 
            //            gridColorSizeBreakup();
            //        } else {
            //            alert(response.message);
            //        }
            //    },
            //    error: function () {
            //        alert("❌ Error while saving data!");
            //    }
            //});


                const fromData = colorAndBreakupFun();
                console.log("Sending to server:", fromData);

                if (fromData.ColorIds.length === 0 || fromData.SizeIds.length === 0) {
                    alert("Please select at least one Color and one Size.");
                    return;
                }

                $.ajax({
                    url: '/RMGProdOrderInformationEntry/SaveFromTempToMain',
                    type: "POST",
                    contentType: 'application/json',
                    data: JSON.stringify(fromData),
                    success: function (res) {
                        console.log("Server Response:", res);
                                if (res.isSuccess) {                                   
                                    loadColorSizeTable();
                                    gridColorSizeBreakup();
                                    showToast('success',res.message);                              
                                } else {
                                    showToast('error',response.message);
                                }
                    },
                    error: function (xhr, status, error) {
                        console.error("Error:", error);
                    }
                });



            }
        });

        // ✅ CLEAR TEMP DATA
        $("#js-order-info-clear").on("click", function () {
            if (isColorAndBreakup) {
            const integraJobNo = $("#OrderDetailsDto_IntegraJobNO").val();
            if (!integraJobNo) {
                alert("⚠️ Please select an Integra Job No first!");
                return;
            }

            if (!confirm("Are you sure to clear all temp data?")) return;

            $.ajax({
                url: '/RMGProdOrderInformationEntry/ClearTempData',
                type: 'POST',
                data: { integraJobNo: integraJobNo },
                success: function (response) {
                    if (response.isSuccess) {
                        alert(response.message);
                        loadColorSizeTable();
                    } else {
                        alert(response.message);
                    }
                },
                error: function () {
                    alert("❌ Error while clearing temp data!");
                }
            });
            }
        });


    }





}(jQuery));