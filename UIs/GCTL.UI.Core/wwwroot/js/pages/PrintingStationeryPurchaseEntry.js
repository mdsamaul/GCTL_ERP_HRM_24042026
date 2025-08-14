
(function ($) {
    $.patientTypes = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            CompanyMultiSelectInput: "#",
            ShortName: "#ShortName",
            supplierName: "#supplierName",
            PurchaseOrderNo: "#purchaseOrderNo",
            StationaryDepartment: "#stationaryDepartment",
            AutoId: "#Setup_TC",
            InvoiceNo:"#stationeryInvoiceNo",
            InvoiceDate:"#stationeryInvoiceDate",
            InvoiceValue: "#stationeryInvoiceValue",
            InvoiceChallanNo:"#stationeryInvoiceChallanNo",
            InvoiceChallanDate:"#stationeryInvoiceChallanDate",
            InvoicePurchaseBy:"#stationeryInvoicePurchaseBy",
            StationeryRemarks:"#stationeryRemarks",
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

            ProductModelBtn:"#productModelBtn",
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
            CloseProductBrandModel:".closeProductBrandModel",
        }, options);
        var filterUrl = commonName.baseUrl + "/GetFilterData";
        var loadCategoryDataUrl = commonName.baseUrl + "/LoadData";
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


        $('.searchable-select').select2({
            allowClear: false,
            width: '100%'
        });
  

        // Time picker
        const timePicker = flatpickr("#inlineTimePicker", {
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
                document.getElementById("timePicker").value = dateStr;
            }
        });

        //load partial page product
        $(commonName.ProductModalBtn).on('click', function () {
            $.ajax({
                url: partialProductUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.ProductPartialContainer).html(res);
                    if (typeof $.ItemMasterInformation == 'function') {
                        var options = {
                            baseUrl: '/ItemMasterInformation',
                            isPartial: true,
                        };
                        $.ItemMasterInformation(options);
                    }                    
                },
                error: function (e) {
                }
            });
        });
        //load brand
        $(commonName.ProductBrandModalBtn).on('click', function () {
            $.ajax({
                url: partialBrandUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.ProductBrandContainer).html(res);
                    if (typeof $.HrmBrand == 'function') {
                        var options = {
                            baseUrl: '/Brand',
                            isPartial: true
                        };
                        $.HrmBrand(options);
                    }
                },
                error: function (e) {
                }
            });
        })
        //load model
        $(commonName.ProductModelBtn).on('click', function () {
            $.ajax({
                url: productModelUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.ProductModelContainer).html(res);
                    if (typeof $.ItemModel == 'function') {
                        var options = {
                            baseUrl: '/ItemModel',
                            isPartial: true
                        };
                        $.ItemModel(options);
                    }
                }, error: function (e) {
                }
            })
        })
        //load modal size
        $(commonName.ProductSizeModalBtn).on('click', function () {
            $.ajax({
                url: productSizeModalUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.SizeModelContainer).html(res);
                    if (typeof $.HRM_SizeJs == 'function') {
                        var options = {
                            baseUrl: '/HRM_Size',
                            isPartial: true
                        }
                        $.HRM_SizeJs(options)
                    }
                },
                error: function (e) {
                }
            });
        })

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
        $(commonName.SupplierModalBtn).on('click', function () {
            $.ajax({
                url: SupplierModalUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.SupplierContainer).html(res);
                    if (typeof $.SalesSupplier == 'function') {
                        var options = {
                            baseUrl: '/SalesSupplier',
                            isPartial: true
                        }
                        $.SalesSupplier(options)
                    }
                },
                error: function (e) {
                }
            });
        })

        $(commonName.StationarySupplierModalClose).on('click', function () {
            $.ajax({
                url: SupplierCloseUrl,
                type: "GET",
                success: function (res) {
                    if (res.data && Array.isArray(res.data)) {
                        $(commonName.SupplierListBtn).empty();
                        res.data.forEach(function (supplier) {
                            $(commonName.SupplierListBtn).append(`
                        <option value="${supplier.supplierId}">${supplier.supplierName}</option>
                    `);
                        });

                        if (res.data.length > 0) {
                            const firstSupplierId = res.data[0].supplierId;
                            $(commonName.supplierListBtn).val(firstSupplierId).trigger('change');
                            $(commonName.SalesSuppAddress).val(res.data[0].supplierAddress);
                        }
                    }
                },
                error: function (e) {
                }
            });
        });

        //close 
        //$(commonName.ProductItemCloseBtn).on('click', function () {
        //    $.ajax({
        //        url: productItemCloseUrl,
        //        type: "GET",
        //        success: function (res) {
        //            $(commonName.ProductSelectId).empty();
        //            res.data.map(function (product) {
        //                $(commonName.ProductSelectId).append(`
        //            <option value="${product.productCode}">${product.productName}</option>
        //            `);
        //            })
                  

        //        }
        //    });
        //})
        //$(commonName.CloseProductBrandModel).on('click', function () {
        //    $.ajax({
        //        url: CloseProductBrandListUrl,
        //        type: "GET",
        //        success: function (res) {
        //            $(commonName.BrandIdFromDropdown).empty();
        //            res.data.result.map(function (Brand) {
        //                $(commonName.BrandIdFromDropdown).append(`
        //            <option value="${Brand.brandId}">${Brand.brandName}</option>
        //            `);
        //            })
                  

        //        }
        //    });
        //})


        $(commonName.SupplierListBtn).on('change', function () {
            var supplierId = $(this).val();
            $.ajax({
                url: supplierDetailsUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(supplierId),
                success: function (res) {
                    if (res.data != null) {
                        $(commonName.SalesSuppAddress).val(res.data.supplierAddress);
                    }
                }, error: function (e) {
                }
            })
        })
        //produt id
        $(document).on('change', '.ProductSelectId', function () {
            var productId = $(this).val();
            var $row = $(this).closest('tr'); 
            $.ajax({
                url: productSelectIdDetailsUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(productId),
                success: function (res) {
                    if (res.data != null) {
                        $row.find('.ProductDescription').val(res.data.description);
                        let $brandDropdown = $row.find('.BrandIdFromDropdown');
                        $brandDropdown.empty().append('<option value="">Brand</option>');
                        res.data.brandList.forEach(function (brand) {
                            $brandDropdown.append(`<option value="${brand.brandID}">${brand.brandName}</option>`);
                        });

                        $row.find('.UnitPriceOfProduct').val(res.data.purchaseCost);
                        $row.find('.QtyOfProduct').val(1);
                        $row.find('.TotalPriceOfProductMulQty').val(res.data.purchaseCost);
                        $row.find('.UnitOfProduct').val(res.data.unitID).trigger('change');
                        $row.find('.ModelPopulateFromBrandId').empty();
                        calculateGrandTotal(); 
                    }
                },
                error: function (e) {
                }
            });
        });

        //brand 
        $(document).on('change', '.brandIdFromDropdown ', function () {
            var brandId = $(this).val();
            var $row = $(this).closest('tr');
            $.ajax({
                url: brandIdDetailsonModelUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(brandId),
                success: function (res) {
                    if (res.data != null) {
                        let $modelDropdown = $row.find('.modelPopulateFromBrandId');
                        $modelDropdown.empty().append('<option value="">Model</option>');
                        res.data.forEach(function (model) {
                            $modelDropdown.append(`<option value="${model.modelID}">${model.modelName}</option>`);
                        });
                    }
                },
                error: function (e) {
                }
            });
        });
        



        $(document).on('input', '.qtyOfProduct', function () {
            let $row = $(this).closest('tr');
            let qtyValue = parseFloat($(this).val()) || 0;
            let unitPrice = parseFloat($row.find('.unitPriceOfProduct').val()) || 0;
            let $totalPrice = $row.find('.totalPriceOfProductMulQty');

            if (qtyValue > 0) {
                let rowTotal = qtyValue * unitPrice;

                $totalPrice.val(rowTotal.toFixed(2));

                $(this).removeClass('printingStation-input');
                $totalPrice.removeClass('printingStation-input');
                $('#printStationerySaveBtn').prop('disabled', false);

                calculateGrandTotal(); 
            } else {
                $(this).addClass('printingStation-input');
                $totalPrice.addClass('printingStation-input');
                $('#totalPriceOfProductAddProductPrice').addClass('printingStation-input');
                $('#printStationerySaveBtn').prop('disabled', true);

                $totalPrice.val(0);
                $('#totalPriceOfProductAddProductPrice').val(0);
                showToast("warning", "Quantity must be at least one or more.");
            }
        });

        function calculateGrandTotal() {
            let grandTotal = 0;
            $('.totalPriceOfProductMulQty').each(function () {
                let val = parseFloat($(this).val()) || 0;
                grandTotal += val;
            });
            $('#totalPriceOfProductAddProductPrice').val(grandTotal.toFixed(2));
        }
       
        let dataList = [];
        function listOfProdut() {
            let allRows = $('table #dinamciDataAppend tr').not('.total-row');

            dataList = []; 

            allRows.each(function () {
                let $row = $(this);

                let rowData = {
                    TC: 0,
                    PurchaseReceiveNo: "",
                    ProductCode: $row.find('.productSelectId').val(),
                    Description: $row.find('.productDescription').val(),
                    BrandID: $row.find('.brandIdFromDropdown').val(),
                    ModelID: $row.find('.modelPopulateFromBrandId').val(),
                    SizeID: $row.find('.sizeSelect').val(),
                    WarrantyPeriod: $row.find('.warrantyInput').val(),
                    WarrentyTypeID: $row.find('.periodSelect').val(),
                    ReqQty: parseFloat($row.find('.qtyOfProduct').val()) || 0,
                    UnitTypID: $row.find('.unitOfProduct').val(),
                    UnitPrice: parseFloat($row.find('.unitPriceOfProduct').val()) || 0,
                    TotalPrice: parseFloat($row.find('.totalPriceOfProductMulQty').val()) || 0,
                    SLNO: 0
                };
                dataList.push(rowData);
            });
        }

        $(document).on('click', '#addmoreDetailsBtn', function () {
            $.ajax({
                url: addMoreLoadProductUrl,
                type: "GET",
                success: function (res) {                  
                    let productOptions = `<option value="">Product</option>`;
                    res.productList.forEach(function (item) {
                        productOptions += `<option value="${item.value}">${item.text}</option>`;
                    });

                    let sizeOptions = `<option value="">Size</option>`;
                    res.sizeList.forEach(function (item) {
                        sizeOptions += `<option value="${item.value}">${item.text}</option>`;
                    });

                    let periodOptions = `<option value="">Period</option>`;
                    res.periodList.forEach(function (item) {
                        periodOptions += `<option value="${item.value}">${item.text}</option>`;
                    });

                    let unitOptions = `<option value="">Unit</option>`;
                    res.unitList.forEach(function (item) {
                        unitOptions += `<option value="${item.value}">${item.text}</option>`;
                    });

                    let newRow = `
<tr>
    <td><select class="form-control-sm form-control searchable-select productSelectId">${productOptions}</select></td>
    <td><input type="text" class="form-control-sm form-control productDescription" placeholder="Description"/></td>
    <td><select class="form-control-sm form-control searchable-select brandIdFromDropdown"><option value="">Brand</option></select></td>
    <td><select class="form-control-sm form-control searchable-select modelPopulateFromBrandId"><option value="">Model</option></select></td>
    <td><select class="form-control-sm form-control searchable-select sizeSelect">${sizeOptions}</select></td>
    <td><input type="number" class="form-control-sm form-control warrantyInput" placeholder="Warranty" /></td>
    <td><select class="form-control-sm form-control searchable-select periodSelect">${periodOptions}</select></td>
    <td><input type="number" class="form-control-sm form-control qtyOfProduct text-center" placeholder="Qty" /></td>
    <td><select class="form-control-sm form-control searchable-select unitOfProduct">${unitOptions}</select></td>
    <td><input type="number" class="form-control-sm form-control unitPriceOfProduct text-end" value="0" readonly /></td>
    <td><input type="number" class="form-control-sm form-control totalPriceOfProductMulQty text-end mb-2" value="0" readonly /></td>
    <td>
        <div class="d-flex gap-2">
         <button class="btn btn-outline-success rounded-md shadow d-flex justify-content-center align-items-center" id="addmoreDetailsBtn" style="width: 30px; height: 30px; font-size: 9px;">
                        <i class="fas fa-plus"></i>
                    </button>  
            <button class="btn btn-outline-danger rounded-md shadow d-flex justify-content-center align-items-center delete-row-btn" style="width: 30px; height: 30px; font-size: 9px;">
                <i class="fas fa-trash-alt"></i>
            </button>
        </div>
    </td>
</tr>`;

                    $('table #dinamciDataAppend tr:last').before(newRow);

                    // Reinitialize select2
                    $('.searchable-select').select2({ width: '100%' });
                    //appendProductRow(res);

                }

            });
        });

        function appendProductRow(resData) {
            let productOptions = `<option value="">Product</option>`;
            resData.productList.forEach(function (item) {
                productOptions += `<option value="${item.value}">${item.text}</option>`;
            });

            let sizeOptions = `<option value="">Size</option>`;
            resData.sizeList.forEach(function (item) {
                sizeOptions += `<option value="${item.value}">${item.text}</option>`;
            });

            let periodOptions = `<option value="">Period</option>`;
            resData.periodList.forEach(function (item) {
                periodOptions += `<option value="${item.value}">${item.text}</option>`;
            });

            let unitOptions = `<option value="">Unit</option>`;
            resData.unitList.forEach(function (item) {
                unitOptions += `<option value="${item.value}">${item.text}</option>`;
            });

            let newRow = `
<tr>
    <td><select class="form-control-sm form-control searchable-select productSelectId">${productOptions}</select></td>
    <td><input type="text" class="form-control-sm form-control productDescription" placeholder="Description"/></td>
    <td><select class="form-control-sm form-control searchable-select brandIdFromDropdown"><option value="">Brand</option></select></td>
    <td><select class="form-control-sm form-control searchable-select modelPopulateFromBrandId"><option value="">Model</option></select></td>
    <td><select class="form-control-sm form-control searchable-select sizeSelect">${sizeOptions}</select></td>
    <td><input type="number" class="form-control-sm form-control warrantyInput" placeholder="Warranty" /></td>
    <td><select class="form-control-sm form-control searchable-select periodSelect">${periodOptions}</select></td>
    <td><input type="number" class="form-control-sm form-control qtyOfProduct text-center" placeholder="Qty" /></td>
    <td><select class="form-control-sm form-control searchable-select unitOfProduct">${unitOptions}</select></td>
    <td><input type="number" class="form-control-sm form-control unitPriceOfProduct text-end" value="0" readonly /></td>
    <td><input type="number" class="form-control-sm form-control totalPriceOfProductMulQty text-end mb-2" value="0" readonly /></td>
    <td>
        <div class="d-flex gap-2">
            <button class="btn btn-outline-success rounded-md shadow d-flex justify-content-center align-items-center" id="addmoreDetailsBtn" style="width: 30px; height: 30px; font-size: 9px; line-height: 1;">
                <i class="fas fa-plus"></i>
            </button>
            <button class="btn btn-outline-danger rounded-md shadow d-flex justify-content-center align-items-center delete-row-btn" style="width: 30px; height: 30px; font-size: 9px; line-height: 1;">
                <i class="fa fa-eraser">&nbsp;</i>
            </button>
        </div>
    </td>
</tr>
`;

            // Append row BEFORE total-row if exists, otherwise just append
            let $table = $('#dinamciDataAppend');
            let $totalRow = $table.find('.total-row');
            $table.append(newRow);
            // Then append total-row
            let totalRow = `
<tr class="total-row">
    <td colspan="10"><div class="total-label">Total:</div></td>
    <td>
        <input type="number" class="form-control-sm form-control text-end" value="0" id="totalPriceOfProductAddProductPrice" readonly />
    </td>
</tr>`;
            $table.append(totalRow);
            $('.searchable-select').select2({ width: '100%' });
        }




        $(document).on('change', '.productSelectId', function () {
            let productId = $(this).val();
            let $row = $(this).closest('tr'); 

            $.ajax({
                url: productSelectIdDetailsUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(productId),
                success: function (res) {
                    if (res.data != null) {

                        // row scoped set
                        $row.find('.productDescription').val(res.data.description);

                        let brandDropdown = $row.find('.brandIdFromDropdown');
                        brandDropdown.empty().append(`<option value="">Brand</option>`);
                        res.data.brandList.forEach(function (brand) {
                            brandDropdown.append(`<option value="${brand.brandID}">${brand.brandName}</option>`);
                        });

                        $row.find('.unitPriceOfProduct').val(res.data.purchaseCost);
                        $row.find('.qtyOfProduct').val(1);
                        $row.find('.totalPriceOfProductMulQty').val(res.data.purchaseCost);
                        $row.find('.unitOfProduct').val(res.data.unitID).trigger('change');
                        $row.find('.modelPopulateFromBrandId').empty().append(`<option value="">Model</option>`);

                        // Calculate grand total (total of all rows)
                        calculateGrandTotal();
                    }
                },
                error: function (e) {
                   
                }
            });
        });

        function calculateGrandTotal() {
            let total = 0;
            $('.totalPriceOfProductMulQty').each(function () {
                let value = parseFloat($(this).val()) || 0;
                total += value;
            });
            $('#totalPriceOfProductAddProductPrice').val(total);
        }
        $(document).on('click', '.delete-row-btn', function () {
            let $targetRow = $(this).closest('tr');
            $targetRow.remove();

            calculateGrandTotal();

            const $tableBody = $('#dinamciDataAppend');
            const $remainingRows = $tableBody.find('tr.data-row'); 
            if ($remainingRows.length === 0) {
                $.ajax({
                    url: addMoreLoadProductUrl,
                    type: "GET",
                    success: function (res) {
                     $("#dinamciDataAppend tr").empty();
                     appendProductRow(res); 
                    }
                });
            } else {
                const $firstDataRow = $remainingRows.first();
                const $actionCell = $firstDataRow.find('td').last();

                $actionCell.html(`
            <div class="d-flex gap-2">
                <button class="btn btn-outline-success rounded-md shadow d-flex justify-content-center align-items-center" id="addmoreDetailsBtn" style="width: 30px; height: 30px; font-size: 9px;">
                    <i class="fas fa-plus"></i>
                </button>
                <button class="btn btn-outline-danger rounded-md shadow d-flex justify-content-center align-items-center delete-row-btn" style="width: 30px; height: 30px; font-size: 9px;">
                     <i class="fa fa-eraser">&nbsp;</i>
                </button>
            </div>
        `);
            }
        });

       

        AutoPrintingStationeryPurchaseId = function () {
            $.ajax({
                url: AutoPrintingStationeryPurchaseIdUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.PurchaseOrderNo).val(res.data);
                },
                error: function (e) {
                }
            });
        }


        resetForm = function () {
            // Reset all input fields
            $(commonName.AutoId).val(0);
            $(commonName.MainCompanyCode).val('');
            $(commonName.PurchaseOrderNo).val('');
            $(commonName.SupplierListBtn).val('').trigger('change');
            $(commonName.SalesSuppAddress).val('');
            $(commonName.StationaryDepartment).val('').trigger('change');
            $(commonName.InvoiceNo).val('');
            $(commonName.InvoiceValue).val('');
            $(commonName.InvoiceChallanNo).val('');
            $(commonName.InvoicePurchaseBy).val('');
            $(commonName.StationeryRemarks).val('');
            $(commonName.CompanyCode).val('');
            $(commonName.TotalPriceOfProductAddProductPrice).val('');
            $(commonName.CreateDate).text('');
            $(commonName.UpdateDate).text('');
            const today = new Date();
            const formattedDate = today.toISOString().split('T')[0];

            datePiker("#datePicker1", formattedDate);
            datePiker($(commonName.InvoiceDate), formattedDate);
            datePiker($(commonName.InvoiceChallanDate), formattedDate);

            // === Re-initialize time picker with current time ===
            if ($("#inlineTimePicker")[0]?._flatpickr) {
                $("#inlineTimePicker")[0]._flatpickr.setDate(today, true);
            }
            $.ajax({
                url: addMoreLoadProductUrl,
                type: "GET",
                success: function (res) {
                    $("#dinamciDataAppend tr").empty();
                    appendProductRow(res); // default row
                }
            });
        }


        $(commonName.ClearBrn).on('click', function () {
            resetForm();
            AutoPrintingStationeryPurchaseId(); 
        });


        function formatDateTimeToSql(dateStr, timeStr) {
            if (timeStr.includes('AM') || timeStr.includes('PM')) {
                timeStr = convertTo24Hour(timeStr);
            }

            const dateTimeStr = `${dateStr}T${timeStr}`;

            const dt = new Date(dateTimeStr + "Z");

            const options = {
                timeZone: 'Asia/Dhaka',
                year: 'numeric',
                month: '2-digit',
                day: '2-digit',
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit',
                hour12: false,
            };

            const formatter = new Intl.DateTimeFormat('en-GB', options);
            const parts = formatter.formatToParts(dt);

            let year, month, day, hour, minute, second;
            parts.forEach(part => {
                if (part.type === 'year') year = part.value;
                else if (part.type === 'month') month = part.value;
                else if (part.type === 'day') day = part.value;
                else if (part.type === 'hour') hour = part.value;
                else if (part.type === 'minute') minute = part.value;
                else if (part.type === 'second') second = part.value;
            });

            return `${year}-${month}-${day} ${hour}:${minute}:${second}.000`;
        }

        function convertTo24Hour(timeStr) {
            const [time, modifier] = timeStr.split(' ');
            let [hours, minutes, seconds] = time.split(':');

            if (modifier === 'PM' && hours !== '12') {
                hours = String(parseInt(hours, 10) + 12);
            }
            if (modifier === 'AM' && hours === '12') {
                hours = '00';
            }

            return `${hours.padStart(2, '0')}:${minutes}:${seconds}`;
        }
        getFromData = function () {
            listOfProdut(); 
                const date = $("#datePicker1").val();
            const time = $("#inlineTimePicker").val();
            if (!date || !time || isNaN(Date.parse(`${date} ${time}`))) {
                showToast("error", "Please select a valid Receive Date and Time.");

                const flatpickrInstance = $("#datePicker1")[0]._flatpickr;
                if (flatpickrInstance) {
                    flatpickrInstance.open();
                }

                $("#datePicker1").addClass("printingStation-input"); 
                $(commonName.PrintStationerySaveBtn).prop('disabled', true);
                return;
            }

            $("#datePicker1").removeClass("printingStation-input");

            const fromData = {
                TC: parseInt($(commonName.AutoId).val()) || 0,
                MainCompanyCode: $(commonName.MainCompanyCode).val() || null,
                PurchaseReceiveNo: $(commonName.PurchaseOrderNo).val() || null,
                SupplierID: $(commonName.SupplierListBtn).val() || null,

                ReceiveDate: formatDateTimeToSql(date, time)
                    ? new Date(formatDateTimeToSql(date, time)).toISOString()
                    : null,              

                DepartmentCode: $(commonName.StationaryDepartment).val() || null,
                InvoiceNo: $(commonName.InvoiceNo).val() || null,

                InvoiceDate: $(commonName.InvoiceDate).val()
                    ? new Date($(commonName.InvoiceDate).val()).toISOString()
                    : null,

                InvoiceValue: parseFloat($(commonName.InvoiceValue).val()) || 0,
                ChallanNo: $(commonName.InvoiceChallanNo).val() || null,

                ChallanDate: $(commonName.InvoiceChallanDate).val()
                    ? new Date($(commonName.InvoiceChallanDate).val()).toISOString()
                    : null,

                EmployeeID_ReceiveBy: $(commonName.InvoicePurchaseBy).val() || null,
                Remarks: $(commonName.StationeryRemarks).val() || null,
                TotalAmount: parseFloat($(commonName.TotalPriceOfProductAddProductPrice).val()) || 0,
                CompanyCode: $(commonName.CompanyCode).val() || null,
                ShowCreateDate: null,
                ShowModifyDate: null,
                purchaseOrderReceiveDetailsDTOs: dataList
            };

            return fromData;
        };


        //exists
        //$(commonName.supplierName).on('input', function () {

        //    let supplierValue = $(this).val();
        //    $.ajax({
        //        url: alreadyExistUrl,
        //        type: "POST",
        //        contentType: 'application/json',
        //        data: JSON.stringify(supplierValue),
        //        success: function (res) {
        //            if (res.isSuccess) {
        //                showToast('warning', res.message);
        //                $(commonName.supplierName).addClass('catagory-input');
        //                $(commonName.PrintStationerySaveBtn).prop('disabled', true);
        //            } else {
        //                $(commonName.supplierName).removeClass('catagory-input');
        //                $(commonName.PrintStationerySaveBtn).prop('disabled', false);
        //                $(commonName.PrintStationerySaveBtn).css('border', 'none');

        //            }
        //        }, error: function (e) {
        //        }
        //    });
        //})

        $(document).on('change','#datePicker1', function () {
            $(commonName.receiveDate).removeClass('printingStation-input');
            $(commonName.supplierName).removeClass('printingStation-input');
            $(commonName.PrintStationerySaveBtn).prop('disabled', false);

        })

        $(document).on('change', commonName.SupplierListBtn, function () {
            $(commonName.receiveDate).removeClass('printingStation-input');
            $(commonName.supplierName).removeClass('printingStation-input');
            $(commonName.PrintStationerySaveBtn).prop('disabled', false);

        })
        $(document).on('change', commonName.ProductSelectId, function () {
            $(commonName.receiveDate).removeClass('printingStation-input');
            $(commonName.supplierName).removeClass('printingStation-input');
            $(commonName.PrintStationerySaveBtn).prop('disabled', false);
        })
        //create and edit
        // Save Button Click
        $(document).on('click', commonName.PrintStationerySaveBtn, function () {
            var fromData = getFromData();
            console.log(fromData);
            if (!fromData.SupplierID || fromData.SupplierID.trim() === "") {
                showToast("error", "Please select a supplier.");
                $('.supplierListBtn').addClass('printingStation-input');
                $('.supplierListBtn').select2('open');
                $(commonName.PrintStationerySaveBtn).prop('disabled', true);
                return;
            }

            var dataListDetails = fromData.purchaseOrderReceiveDetailsDTOs;

            // Step: Product Validation
            if (!dataListDetails || dataListDetails.length === 0) {
                showToast("error", "Please add at least one product.");
                $(commonName.PrintStationerySaveBtn).prop('disabled', true);
                return;
            }

            $('.productSelectId').removeClass('printingStation-input');

            for (let i = 0; i < dataListDetails.length; i++) {
                const product = dataListDetails[i];

                if (!product.ProductCode || product.ProductCode.trim() === "") {
                    showToast("error", `Product selection missing in row ${i + 1}`);
                    const $productSelect = $('.productSelectId').eq(i);

                    $productSelect.addClass('printingStation-input');

                    if ($productSelect.hasClass("select2-hidden-accessible")) {
                        $productSelect.select2('open');
                    } else {
                        $productSelect.focus();
                    }

                    $(commonName.PrintStationerySaveBtn).prop('disabled', true);
                    return;
                }
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
                    resetForm();
                    AutoPrintingStationeryPurchaseId();
                    loadCategoryData();
                    dataList.length = 0; 

                    $('table #dinamciDataAppend tr').not('.total-row').remove();
                }
            });
        });

        // Reload DataTable Function
        function loadCategoryData() {
            tableContainer.ajax.reload(null, false);
        }
        var tableContainer = $('#printingStationTable').DataTable({
            "autoWidth": true,
            "ajax": {
                "url": loadCategoryDataUrl,
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

        let selectedIds = [];
        //edit
        $(document).on('click', commonName.EditBtn, function () {
            let id = $(this).data('id');
            $.ajax({
                url: `${PopulatedDataForUpdateUrl}?id=${id}`,
                type: "GET",
                success: function (res) {
                    selectedIds = [];
                    selectedIds.push(res.result.tc + '');                    
                    // Master form populate
                    $(commonName.AutoId).val(res.result.tc);
                    $(commonName.MainCompanyCode).val(res.result.mainCompanyCode);
                    $(commonName.PurchaseOrderNo).val(res.result.purchaseReceiveNo);
                    $(commonName.SupplierListBtn).val(res.result.supplierID).trigger("change");
                    $(commonName.StationaryDepartment).val(res.result.departmentCode);
                    $(commonName.InvoiceNo).val(res.result.invoiceNo);
                    $(commonName.CreateDate).text(res.result.showCreateDate);
                    $(commonName.UpdateDate).text(res.result.showModifyDate);
                    if (res.result.receiveDate) {
                        const dateTimeParts = res.result.receiveDate.split("T"); 
                        datePiker("#datePicker1", dateTimeParts[0]);
                        const [hour, minute] = dateTimeParts[1].split(":");
                        const formattedTime = `${hour}:${minute}`;
                       
                        // Set time in flatpickr (must be already initialized)
                        if ($("#inlineTimePicker")[0]._flatpickr) {
                            $("#inlineTimePicker")[0]._flatpickr.setDate(formattedTime, true);
                        }
                    }

                    if (res.result.invoiceDate) {
                        let InvoiceDate = res.result.invoiceDate.split("T")[0];
                        datePiker($(commonName.InvoiceDate), InvoiceDate);
                    }
                    $(commonName.InvoiceValue).val(res.result.invoiceValue);
                    $(commonName.InvoiceChallanNo).val(res.result.challanNo);
                    if (res.result.challanDate) {
                        let challanDate = res.result.challanDate.split("T")[0];
                        datePiker($(commonName.InvoiceChallanDate), challanDate);
                    }

                    $(commonName.InvoicePurchaseBy).val(res.result.employeeID_ReceiveBy);
                    $(commonName.StationeryRemarks).val(res.result.remarks);
                    $(commonName.CompanyCode).val(res.result.companyCode);
                    $(commonName.TotalPriceOfProductAddProductPrice).val(res.result.totalAmount);

                    // Remove all existing detail rows before populate
                    $('table #dinamciDataAppend').empty();
                    if (res.result.purchaseOrderReceiveDetailsDTOs.length === 0) {
                     $.ajax({
                            url: addMoreLoadProductUrl,
                            type: "GET",
                            success: function (res) {
                                $("#dinamciDataAppend tr").empty();
                                appendProductRow(res); // default row
                            }
                        });
                    }

                    // Populate details
                    if (res.result.purchaseOrderReceiveDetailsDTOs && res.result.purchaseOrderReceiveDetailsDTOs.length > 0) {
                        res.result.purchaseOrderReceiveDetailsDTOs.forEach(function (item, index) {
                            let isFirstRow = index === 0;
                            let newRow = `
<tr class="data-row">
    <td><select class="form-control-sm form-control searchable-select productSelectId">
        <option value="${item.productCode}" selected>${item.productName}</option>
    </select></td>
    <td><input type="text" class="form-control-sm form-control productDescription" value="${item.description || ''}" /></td>
    <td><select class="form-control-sm form-control searchable-select brandIdFromDropdown">
        <option value="${item.brandID}" selected>${item.brandName}</option>
    </select></td>
    <td><select class="form-control-sm form-control searchable-select modelPopulateFromBrandId">
        <option value="${item.modelID}" selected>${item.modelName}</option>
    </select></td>
    <td><select class="form-control-sm form-control searchable-select sizeSelect">
        <option value="${item.sizeID}" selected>${item.sizeName}</option>
    </select></td>
    <td><input type="number" class="form-control-sm form-control warrantyInput" value="${item.warrantyPeriod || ''}" /></td>
    <td><select class="form-control-sm form-control searchable-select periodSelect">
        <option value="${item.warrentyTypeID}" selected>${item.warrantyPeriodName}</option>
    </select></td>
    <td><input type="number" class="form-control-sm form-control qtyOfProduct text-center" value="${item.reqQty || 0}" /></td>
    <td><select class="form-control-sm form-control searchable-select unitOfProduct">
        <option value="${item.unitTypID}" selected>${item.unitTypName}</option>
    </select></td>
    <td><input type="number" class="form-control-sm form-control unitPriceOfProduct text-end" value="${item.unitPrice || 0}" readonly /></td>
    <td><input type="number" class="form-control-sm form-control totalPriceOfProductMulQty text-end" value="${item.totalPrice || 0}" readonly /></td>
    <td>
        ${isFirstRow
                                    ? `<div class="d-flex gap-2">
                    <button class="btn btn-outline-success rounded-md shadow d-flex justify-content-center align-items-center" id="addmoreDetailsBtn" style="width: 30px; height: 30px; font-size: 9px;">
                        <i class="fas fa-plus"></i>
                    </button>                   
                    <button class="btn btn-outline-danger rounded-md shadow d-flex justify-content-center align-items-center delete-row-btn" style="width: 30px; height: 30px; font-size: 9px;">
                         <i class="fas fa-trash-alt"></i>
                    </button>
                </div>`
                                : `<div class="d-flex gap-2">
                                     <button class="btn btn-outline-success rounded-md shadow d-flex justify-content-center align-items-center" id="addmoreDetailsBtn" style="width: 30px; height: 30px; font-size: 9px;">
                        <i class="fas fa-plus"></i>
                    </button>
                    <button class="btn btn-outline-danger rounded-md shadow d-flex justify-content-center align-items-center delete-row-btn" style="width: 30px; height: 30px; font-size: 9px;">
                        <i class="fas fa-trash-alt"></i>
                    </button>
               </div>`
                                }
    </td>
</tr>`;
                            $('table #dinamciDataAppend').append(newRow);
                        });

                        // Append total row
                        let totalRow = `
<tr class="total-row">
    <td colspan="10"><div class="total-label">Total:</div></td>
    <td>
        <input type="number" class="form-control-sm form-control text-end" value="${res.result.totalAmount}" id="totalPriceOfProductAddProductPrice" readonly />
    </td>
    <td></td>
</tr>`;
                        $('table #dinamciDataAppend').append(totalRow);
                    }

                    // Reinitialize Select2
                    $('.searchable-select').select2({ width: '100%' });
                },
                error: function (e) {
                    showToast("error", "Failed to load data");
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
                    resetForm();
                    AutoPrintingStationeryPurchaseId();
                    loadCategoryData();
                    $('#selectAll').prop('checked', false);
                    selectedIds = [];
                    dataList.length = 0; 
                    $('table #dinamciDataAppend tr').not('.total-row').remove();
                }
            })
        })

        function closeAllModals(callback) {
            $(".modal").modal('hide'); // Bootstrap modal close
            setTimeout(() => {
                if (typeof callback === 'function') {
                    callback();
                }
            }, 300); 
        }

        $(document).on('click', ".closesupplierTypeModel", function () {
            closeAllModals(() => {
                $(commonName.SupplierModalBtn).trigger('click');
            });
        });

        $(document).on('click', ".closeCountryModel", function () {
            closeAllModals(() => {
                $(commonName.SupplierModalBtn).trigger('click');
            });
        });

        $(document).on('click', ".closeBrandModel", function () {
            closeAllModals(() => {
                $(commonName.ProductModalBtn).trigger('click');
            });
        });

        $(document).on('click', ".closeCatagoryModel", function () {
            closeAllModals(() => {
                $(commonName.ProductModalBtn).trigger('click');
            });
        });

        $(document).on('click', ".itemBrandModalLabelClose", function () {
            closeAllModals(() => {
                $(commonName.ProductModelBtn).trigger('click');
            });
        });


        window.categoryModuleLoaded = true;
        var init = function () {
            stHeader();
            datePiker(".datePicker");
            timePicker;
            AutoPrintingStationeryPurchaseId();
            tableContainer;
        };
        init();
    };
})(jQuery);
