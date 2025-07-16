
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
            EditBrn: ".btn-edit",
            PrintStationerySaveBtn: ".js-Printing-Stationery-Purchase-Entry-save",
            DeleteBtn: "#js-inv-catagory-delete-confirm",
            UpdateDate: ".updateDate",
            CreateDate: ".createDate",
            ClearBrn: "#js-catagory-clear",

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
            DetailsClear:"#delete-clear-row-btn",
        }, options);
        var filterUrl = commonName.baseUrl + "/GetFilterData";
        var loadCategoryDataUrl = commonName.baseUrl + "/LoadData";
        var AutoPrintingStationeryPurchaseIdUrl = commonName.baseUrl + "/AutoPrintingStationeryPurchaseId";
        var CreateUpdateUrl = commonName.baseUrl + "/CreateUpdate";
        var PopulatedDataForUpdateUrl = commonName.baseUrl + "/PopulatedDataForUpdate";
        var deleteUrl = commonName.baseUrl + "/deleteCatagory";
        var alreadyExistUrl = commonName.baseUrl + "/alreadyExist";
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

      
        datePiker = flatpickr(".datePicker", {
            dateFormat: "Y-m-d",       
            altInput: true,            
            altFormat: "d/m/Y",       
            defaultDate: new Date(),    
            allowInput: true           
        });

        $('.searchable-select').select2({
            placeholder: 'Select an option',
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
            console.log("click btn");
            $.ajax({
                url: partialProductUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.ProductPartialContainer).html(res);
                    console.log(res);
                    if (typeof $.ItemMasterInformation == 'function') {
                        var options = {
                            baseUrl: '/ItemMasterInformation',
                            isPartial: true,
                        };
                        $.ItemMasterInformation(options);
                    }                    
                },
                error: function (e) {
                    console.log(e);
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
                    console.log(e);
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
                    console.log(e)
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
                    console.log(e);
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
                    console.log(e);
                }
            });
        })
        $(commonName.SupplierModalBtn).on('click', function () {
            console.log("test supplier");
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
                    console.log(e);
                }
            });
        })

        $(commonName.SupplierListBtn).on('change', function () {
            console.log("asdfasdf", $(this).val());
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
                    console.log(e);
                }
            })
        })
        //produt id
        $(document).on('change', '.ProductSelectId', function () {
            var productId = $(this).val();
            var $row = $(this).closest('tr'); // শুধু এই row

            $.ajax({
                url: productSelectIdDetailsUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(productId),
                success: function (res) {
                    if (res.data != null) {
                        console.log(res);

                        // Row scoped data binding
                        $row.find('.ProductDescription').val(res.data.description);

                        let $brandDropdown = $row.find('.BrandIdFromDropdown');
                        $brandDropdown.empty().append('<option value="">Select Brand</option>');
                        res.data.brandList.forEach(function (brand) {
                            $brandDropdown.append(`<option value="${brand.brandID}">${brand.brandName}</option>`);
                        });

                        $row.find('.UnitPriceOfProduct').val(res.data.purchaseCost);
                        $row.find('.QtyOfProduct').val(1);
                        $row.find('.TotalPriceOfProductMulQty').val(res.data.purchaseCost);
                        $row.find('.UnitOfProduct').val(res.data.unitID).trigger('change');
                        $row.find('.ModelPopulateFromBrandId').empty();
                        calculateGrandTotal(); // optional: if you want to update total
                    }
                },
                error: function (e) {
                    console.log(e);
                }
            });
        });

        //brand 
        $(document).on('change', '.BrandIdFromDropdown', function () {
            var brandId = $(this).val();
            var $row = $(this).closest('tr'); // ঐ row select

            $.ajax({
                url: brandIdDetailsonModelUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(brandId),
                success: function (res) {
                    if (res.data != null) {
                        console.log(res);

                        let $modelDropdown = $row.find('.ModelPopulateFromBrandId');
                        $modelDropdown.empty().append('<option value="">Select Model</option>');

                        res.data.forEach(function (model) {
                            $modelDropdown.append(`<option value="${model.modelID}">${model.modelName}</option>`);
                        });
                    }
                },
                error: function (e) {
                    console.log(e);
                }
            });
        });
        $(document).on('click', commonName.DetailsClear, function () {
            let $targetRow = $(this).closest('tr');
            clearRowFields($targetRow);
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


        $(document).on('change','.totalPriceOfProductMulQty', function () {
            var price = $(this).val();
            console.log({price});
        })


        function initializeSelect2() {
            $('.searchable-select').select2({
                width: '100%' 
            });
        }

        $(document).on('click', '#addmoreDetailsBtn', function () {
            $.ajax({
                url: addMoreLoadProductUrl,
                type: "GET",
                success: function (res) {                  
                    let productOptions = `<option>Select Product</option>`;
                    res.productList.forEach(function (item) {
                        productOptions += `<option value="${item.value}">${item.text}</option>`;
                    });

                    let sizeOptions = `<option>Select Size</option>`;
                    res.sizeList.forEach(function (item) {
                        sizeOptions += `<option value="${item.value}">${item.text}</option>`;
                    });

                    let periodOptions = `<option>Select Period</option>`;
                    res.periodList.forEach(function (item) {
                        periodOptions += `<option value="${item.value}">${item.text}</option>`;
                    });

                    let unitOptions = `<option>Select Unit</option>`;
                    res.unitList.forEach(function (item) {
                        unitOptions += `<option value="${item.value}">${item.text}</option>`;
                    });

                    let newRow = `
<tr>
    <td><select class="form-control-sm form-control searchable-select productSelectId">${productOptions}</select></td>
    <td><input type="text" class="form-control-sm form-control productDescription" placeholder="Description"/></td>
    <td><select class="form-control-sm form-control searchable-select brandIdFromDropdown"><option>Select Brand</option></select></td>
    <td><select class="form-control-sm form-control searchable-select modelPopulateFromBrandId"><option>Select Model</option></select></td>
    <td><select class="form-control-sm form-control searchable-select sizeSelect">${sizeOptions}</select></td>
    <td><input type="number" class="form-control-sm form-control warrantyInput" placeholder="Warranty" /></td>
    <td><select class="form-control-sm form-control searchable-select periodSelect">${periodOptions}</select></td>
    <td><input type="number" class="form-control-sm form-control qtyOfProduct text-center" placeholder="Qty" /></td>
    <td><select class="form-control-sm form-control searchable-select unitOfProduct">${unitOptions}</select></td>
    <td><input type="number" class="form-control-sm form-control unitPriceOfProduct text-end" value="0" readonly /></td>
    <td><input type="number" class="form-control-sm form-control totalPriceOfProductMulQty text-end mb-2" value="0" readonly /></td>
    <td>
        <div class="d-flex justify-content-center align-items-center">
            <button class="btn btn-outline-danger rounded-md shadow d-flex justify-content-center align-items-center delete-row-btn" style="width: 30px; height: 30px; font-size: 9px;">
                <i class="fas fa-trash-alt"></i>
            </button>
        </div>
    </td>
</tr>`;

                    $('table tbody tr:last').before(newRow);

                    // Reinitialize select2
                    $('.searchable-select').select2({ width: '100%' });
                }

            });
        });
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
                        console.log(res);

                        // row scoped set
                        $row.find('.productDescription').val(res.data.description);

                        let brandDropdown = $row.find('.brandIdFromDropdown');
                        brandDropdown.empty().append(`<option>Select Brand</option>`);
                        res.data.brandList.forEach(function (brand) {
                            brandDropdown.append(`<option value="${brand.brandID}">${brand.brandName}</option>`);
                        });

                        $row.find('.unitPriceOfProduct').val(res.data.purchaseCost);
                        $row.find('.qtyOfProduct').val(1);
                        $row.find('.totalPriceOfProductMulQty').val(res.data.purchaseCost);
                        $row.find('.unitOfProduct').val(res.data.unitID).trigger('change');
                        $row.find('.modelPopulateFromBrandId').empty();

                        // Calculate grand total (total of all rows)
                        calculateGrandTotal();
                    }
                },
                error: function (e) {
                    console.log(e);
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
            $(this).closest('tr').remove();
            calculateGrandTotal();
        });





    //    $(document).on('click', '#addmoreDetailsBtn', function () {
           
    //        let newRow = `
    //    <tr>
    //        <td>
    //            <select class="form-control-sm form-control searchable-select">
    //                <option>Select Product</option>
    //                ${populateOptions(window.productList)}
    //            </select>
    //        </td>
    //        <td><input type="text" class="form-control-sm form-control" placeholder="Description" /></td>
    //        <td>
    //            <select class="form-control-sm form-control searchable-select">
    //                <option>Select Brand</option>
    //                ${populateOptions(window.brandList)}
    //            </select>
    //        </td>
    //        <td>
    //            <select class="form-control-sm form-control searchable-select">
    //                <option>Select Model</option>
    //                ${populateOptions(window.modelList)}
    //            </select>
    //        </td>
    //        <td>
    //            <select class="form-control-sm form-control searchable-select">
    //                <option>Select Size</option>
    //                ${populateOptions(window.sizeList)}
    //            </select>
    //        </td>
    //        <td><input type="number" class="form-control-sm form-control" placeholder="Warranty" /></td>
    //        <td>
    //            <select class="form-control-sm form-control searchable-select">
    //                <option>Select Period</option>
    //                ${populateOptions(window.periodList)}
    //            </select>
    //        </td>
    //        <td><input type="number" class="form-control-sm form-control text-center" placeholder="Qty" /></td>
    //        <td>
    //            <select class="form-control-sm form-control searchable-select">
    //                <option>Select Unit</option>
    //                ${populateOptions(window.unitList)}
    //            </select>
    //        </td>
    //        <td><input type="number" class="form-control-sm form-control text-end" value="0" readonly /></td>
    //        <td><input type="number" class="form-control-sm form-control text-end mb-2" value="0" readonly /></td>
    //        <td class="d-flex justify-content-center align-items-center gap-1">
    //            <div class="d-flex gap-2">                   
    //                <button class="btn btn-outline-danger rounded-md shadow d-flex justify-content-center align-items-center delete-row-btn" style="width: 30px; height: 30px; font-size: 9px;">
    //                    <i class="fas fa-trash-alt"></i>
    //                </button>
    //            </div>
    //        </td>
    //    </tr>
    //`;

    //        $('.product-table tbody tr:last').before(newRow);

    //        initializeSelect2(); // Call again after adding row
    //    });
        function populateOptions(dataList) {
            if (!dataList) return '';
            return dataList.map(item => `<option value="${item.value}">${item.text}</option>`).join('');
        }

        $(document).on('click', '.delete-row-btn', function () {
            $(this).closest('tr').remove();
        });

        AutoPrintingStationeryPurchaseId = function () {
            $.ajax({
                url: AutoPrintingStationeryPurchaseIdUrl,
                type: "GET",
                success: function (res) {
                    console.log(res);
                    $(commonName.PurchaseOrderNo).val(res.data);
                },
                error: function (e) {
                }
            });
        }

        resetFrom = function () {
            $(commonName.AutoId).val(0);
            $(commonName.supplierName).val('');
            $(commonName.ShortName).val('');
        }
        $(commonName.ClearBrn).on('click', function () {
            resetFrom();
            AutoPrintingStationeryPurchaseId();
        })
        // get data from input //todo input
        function formatDateTimeToSql(dateStr, timeStr) {
            const dateTimeStr = dateStr + " " + timeStr;
            const dt = new Date(dateTimeStr);

            const pad = n => String(n).padStart(2, '0');
            const year = dt.getFullYear();
            const month = pad(dt.getMonth() + 1);
            const day = pad(dt.getDate());

            const hours = pad(dt.getHours());
            const minutes = pad(dt.getMinutes());
            const seconds = pad(dt.getSeconds());

            return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}.000`;
        }


        getFromData = function () {
            const date = $("#datePicker1").val();
            const time = $("#inlineTimePicker").val();
            var fromData = {
                AutoId: $(commonName.AutoId).val(),
                PurchaseOrderNo: $(commonName.PurchaseOrderNo).val(),
                SupplierID: $(commonName.SupplierListBtn).val(),
                ShortName: $(commonName.ShortName).val(),
                ReceiveDate: formatDateTimeToSql(date, time), 
                DepartmentCode: $(commonName.StationaryDepartment).val(),
                InvoiceNo: $(commonName.InvoiceNo).val(),
                InvoiceDate: $(commonName.InvoiceDate).val(),
                InvoiceValue: $(commonName.InvoiceValue).val(),
                ChallanNo: $(commonName.InvoiceChallanNo).val(),
                ChallanDate: $(commonName.InvoiceChallanDate).val(),
                EmployeeID_ReceiveBy: $(commonName.InvoicePurchaseBy).val(),
                Remarks: $(commonName.StationeryRemarks).val(),
                TotalAmount: $(commonName.TotalPriceOfProductAddProductPrice).val()
            };
            return fromData;
        };



        //exists 
        //$(commonName.supplierName).on('input', function () {

        //    let supplierValue = $(this).val();
        //    console.log();
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
        //create and edit
        // Save Button Click
        $(document).on('click', commonName.PrintStationerySaveBtn, function () {
            var fromData = getFromData();
            console.log(fromData);
            if (fromData.supplierName == null || fromData.supplierName.trim() === '') {
                $(commonName.supplierName).addClass('catagory-input');
                $(commonName.PrintStationerySaveBtn).prop('disabled', true);
                $(commonName.supplierName).focus();
                return;
            }


            //$.ajax({
            //    url: CreateUpdateUrl,
            //    type: "POST",
            //    contentType: "application/json",
            //    data: JSON.stringify(fromData),
            //    success: function (res) {
            //        if (res.isSuccess) {
            //            showToast("success", res.message);
            //        } else {
            //            showToast("error", res.message);
            //        }
            //    },
            //    error: function (e) {
            //        showToast("error", res.message);
            //    },
            //    complete: function () {
            //        resetFrom();
            //        AutoPrintingStationeryPurchaseId();
            //        loadCategoryData();
            //    }
            //});
        });

        // Reload DataTable Function
        function loadCategoryData() {
            table.ajax.reload(null, false);
        }

        var table = $('#printingStationTable').DataTable({
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
                    "data": "autoId",
                    "render": function (data) {
                        return `<input type="checkbox" class="row-checkbox" value=${data} />`;
                    },
                    "orderable": false
                },
                {
                    "data": "PurchaseOrderNo",
                    "render": function (data) {
                        return `<button class="btn btn-sm btn-link btn-edit" data-id=${data}>${data}</button>`;
                    }
                },
                { "data": "supplierName" },
                { "data": "shortName" }
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
            $.ajax({
                url: `${PopulatedDataForUpdateUrl}?id=${id}`,
                type: "GET",
                success: function (res) {
                    selectedIds = [];
                    selectedIds.push(res.result.autoId + '');
                    $(commonName.AutoId).val(res.result.autoId);
                    $(commonName.supplierName).val(res.result.supplierName);
                    $(commonName.ShortName).val(res.result.shortName);
                    $(commonName.PurchaseOrderNo).val(res.result.PurchaseOrderNo);
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
                    AutoPrintingStationeryPurchaseId();
                    loadCategoryData();
                    $('#selectAll').prop('checked', false);
                    selectedIds = [];
                }
            })
        })



        window.categoryModuleLoaded = true;
        var init = function () {
            stHeader();
            datePiker;
            timePicker;
            AutoPrintingStationeryPurchaseId();
            table;
            console.log("test");
        };
        init();
    };
})(jQuery);
