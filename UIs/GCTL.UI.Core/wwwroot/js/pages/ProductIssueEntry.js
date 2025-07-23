
(function ($) {
    $.ProductIssueEntryJs = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            CompanyMultiSelectInput: "#",
            ShortName: "#ShortName",
            supplierName: "#supplierName",
            PurchaseIssueNo: "#purchaseIssueNo",
            StationaryDepartment: "#stationaryDepartment",
            AutoId: "#Setup_TC",
            InvoiceNo: "#stationeryInvoiceNo",
            InvoiceDate: "#stationeryInvoiceDate",
            InvoiceValue: "#stationeryInvoiceValue",
            InvoiceChallanNo: "#stationeryInvoiceChallanNo",
            InvoiceChallanDate: "#stationeryInvoiceChallanDate",
            InvoicePurchaseBy: "#stationeryInvoicePurchaseBy",
            StationeryRemarks: "#stationeryRemarks",
            RowCheckbox: ".row-checkbox",
            SelectedAll: "#selectAll",
            EditBtn: ".stationary-btn-edit",
            PrintStationerySaveBtn: ".js-Printing-Stationery-Purchase-Entry-save",
            DeleteBtn: "#js-Printing-Stationery-Purchase-delete-confirm",
            UpdateDate: ".updateDate",
            CreateDate: ".createDate",
            ClearBrn: "#js-Printing-Stationery-Purchase-clear",

            ProductModalBtn: "#productModalBtn",
            ProductPartialContainer: "#productPartialContainer",
            ProductBrandModalBtn: "#productBrandModalBtn",
            ProductBrandContainer: "#productBrandContainer",

            ProductModelBtn: "#productModelBtn",
            ProductModelContainer: "#productModelContainer",

            ProductSizeModalBtn: "#productSizeModalBtn",
            SizeModelContainer: "#sizeModelContainer",
            ProductUnitModalBtn: "#productUnitBtn",
            ProductUnitModelContainer: "#productUnitModelContainer",
            AddmoreDetailsBtn: "#addmoreDetailsBtn",

            SupplierModalBtn: "#supplierModalBtn",
            SupplierContainer: "#supplierContainer",
            SupplierListBtn: ".supplierListBtn",
            SalesSuppAddress: "#salesSuppAddress",
            ProductSelectId: ".productSelectId",
            ProductDescription: ".productDescription",
            BrandIdFromDropdown: ".brandIdFromDropdown",
            ModelPopulateFromBrandId: ".modelPopulateFromBrandId",
            UnitPriceOfProduct: ".unitPriceOfProduct",
            QtyOfProduct: ".qtyOfProduct",
            TotalPriceOfProductMulQty: ".totalPriceOfProductMulQty",
            UnitOfProduct: ".unitOfProduct",
            TotalPriceOfProductAddProductPrice: "#totalPriceOfProductAddProductPrice",
            DetailsClear: ".delete-clear-row-btn",
            StationarySupplierModalClose: "#stationarySupplierModalClose",

            ProductItemCloseBtn: "#productItemCloseBtn",
            CloseProductBrandModel: ".closeProductBrandModel",
        }, options);
        var filterUrl = commonName.baseUrl + "/GetFilterData";
        var loadTempProductIssueTableDataUrl = commonName.baseUrl + "/LoadData";
        var AutoPrintingStationeryPurchaseIdUrl = commonName.baseUrl + "/AutoPrintingStationeryPurchaseId";
        var CreateUpdateUrl = commonName.baseUrl + "/CreateUpdate";
        var PopulatedDataForUpdateUrl = commonName.baseUrl + "/PopulatedDataForUpdate";
        var deleteUrl = commonName.baseUrl + "/deletePrintingStationeryPurchase";
        var partialProductUrl = "/ItemMasterInformation/index?isPartial=true";
        var partialBrandUrl = "/Brand/Index?isPartial=true";
        var productModelUrl = "/ItemModel/Index?isPartial=true";
        var productUnitModalUrl = "/RMG_Prod_Def_UnitType/Index?isPartial=true";
        var productSizeModalUrl = "/HRM_Size/Index?isPartial=true";
        var SupplierModalUrl = "/SalesSupplier/Index?isPartial=true";
        var supplierDetailsUrl = commonName.baseUrl + "/supplierIdDetails";
        var productSelectIdDetailsUrl = commonName.baseUrl + "/productSelectIdDetails";
        var brandIdDetailsonModelUrl = commonName.baseUrl + "/brandIdDetailsonModel";
        var addMoreLoadProductUrl = commonName.baseUrl + "/addMoreLoadProduct";
        var SupplierCloseUrl = commonName.baseUrl + "/SupplierCloseList";
        //var productItemCloseUrl = commonName.baseUrl + "/productItemClose";
        //var CloseProductBrandListUrl = commonName.baseUrl + "/BrandListClose";
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


        function datePiker(selector, inputDate = null) {
            const parsedDate = inputDate ? new Date(inputDate) : new Date();

            flatpickr(selector, {
                dateFormat: "Y-m-d",
                altInput: true,
                altFormat: "d/m/Y",
                defaultDate: parsedDate,
                allowInput: true
            });
        }


        //$('.searchable-select').select2({
        //    allowClear: false,
        //    width: '100%'
        //});


        $('.searchable-select').select2({
                width: '100%',
                language: {
                    noResults: function () {

                    }
                },
                escapeMarkup: function (markup) {
                    return markup;
                }
            });
      


        // Time picker
        const timePicker = flatpickr("#purchaseIssueInlineTimePicker", {
            enableTime: true,
            noCalendar: true,
            inline: true,
            defaultDate: new Date(),
            dateFormat: "h:i:S K",
            time_24hr: false,
            enableSeconds: true,
            minuteIncrement: 1,
            secondIncrement: 1,
            onChange: function (selectedDates, dateStr) {
                document.getElementById("purchaseIssueDatePicker").value = dateStr;
            }
        });

        //load modal unit
        $(commonName.ProductUnitModalBtn).on('click', function () {
            $.ajax({
                url: productUnitModalUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.ProductUnitModelContainer).html(res);
                    if (typeof $.RmgProdDefUnitType == 'function') {
                        var options = {
                            baseUrl: '/RMG_Prod_Def_UnitType',
                            isPartial: true
                        }
                        $.RmgProdDefUnitType(options)
                    }
                },
                error: function (e) {
                }
            });
        })
       
        AutoPrintingStationeryPurchaseId = function () {
            $.ajax({
                url: AutoPrintingStationeryPurchaseIdUrl,
                type: "GET",
                success: function (res) {
                    console.log(res);
                    $(commonName.PurchaseIssueNo).val(res.data);
                },
                error: function (e) {
                    showToast('error', 'Error fetching Auto ID');
                }
            });
        }

        function loadCategoryData() {
            tableTempContainer.ajax.reload(null, false);
        }
        var tableTempContainer = $('#tempProductIssueTable').DataTable({
            "autoWidth": true,
            "ajax": {
                "url": loadTempProductIssueTableDataUrl,
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
                    "data": "tc",
                    "render": function (data) {
                        return `<input type="checkbox" class="row-checkbox" value=${data} />`;
                    },
                    "orderable": false
                },
                {
                    "data": "purchaseReceiveNo",
                    "render": function (data) {
                        return `<button class="btn btn-sm btn-link stationary-btn-edit" data-id=${data}>${data}</button>`;
                    }
                },
                {
                    "data": "showReceiveDate",
                },
                { "data": "departmentName" },
                { "data": "supplierName" },
                { "data": "invoiceNo" },
                {
                    "data": "totalAmount",
                    "render": function (data, type, row) {
                        return data != null ? data : row.invoiceValue || 0;
                    }
                },
                { "data": "employeeID_ReceiveBy" },
                { "data": "companyCode" }
            ],
            "columnDefs": [
                {
                    "targets": 2,
                    "width": "auto"
                }
            ],
            "paging": false,
            "pagingType": "full_numbers",
            "searching": true,
            "ordering": true,
            "responsive": true,
            "autoWidth": true,
            "info": false, 
            "language": {
                "search": "Search....",
                "lengthMenu": "Show _MENU_ entries per page",
                "zeroRecords": "No data found",
                              
            }
        });

        function loadCategoryData() {
            tableContainer.ajax.reload(null, false);
        }
        var tableContainer = $('#productIssueTable').DataTable({
            "autoWidth": true,
            "ajax": {
                "url": loadTempProductIssueTableDataUrl,
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
                    "data": "tc",
                    "render": function (data) {
                        return `<input type="checkbox" class="row-checkbox" value=${data} />`;
                    },
                    "orderable": false
                },
                {
                    "data": "purchaseReceiveNo",
                    "render": function (data) {
                        return `<button class="btn btn-sm btn-link stationary-btn-edit" data-id=${data}>${data}</button>`;
                    }
                },
                {
                    "data": "showReceiveDate",
                },
                { "data": "departmentName" },
                { "data": "supplierName" },
                { "data": "invoiceNo" },
                {
                    "data": "totalAmount",
                    "render": function (data, type, row) {
                        return data != null ? data : row.invoiceValue || 0;
                    }
                },
                { "data": "employeeID_ReceiveBy" },
                { "data": "companyCode" }
            ],
            "columnDefs": [
                {
                    "targets": 2,
                    "width": "auto"
                }
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
        window.categoryModuleLoaded = true;
        var init = function () {
            stHeader();
            datePiker(".datePicker");
            timePicker;
            AutoPrintingStationeryPurchaseId();
            tableContainer;
            tableTempContainer;
        };
        init();
    };
})(jQuery);
