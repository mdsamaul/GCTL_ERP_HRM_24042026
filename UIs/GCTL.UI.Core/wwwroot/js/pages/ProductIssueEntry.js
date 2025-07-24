
(function ($) {
    $.ProductIssueEntryJs = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            PurchaseIssueNo: "#purchaseIssueNo",           
            AutoId: "#Setup_TC",
           
            RowCheckbox: ".row-checkbox",
            SelectedAll: "#selectAll",
            EditBtn: ".stationary-btn-edit",
            ProductIssueSaveEditBtn: ".js-product-issue-Entry-save",
            DeleteBtn: "#js-product-issue-delete-confirm",
            UpdateDate: ".updateDate",
            CreateDate: ".createDate",
            ClearBrn: "#js-product-issue-clear",
            //master
            PurchaseIssueDepartment:"#purchaseIssueDepartment",
            PurchaseIssueEmployeeBtn:".purchaseIssueEmployeeBtn",
            ProductIssueBy:"#productIssueBy",
            Remarks:"#Remarks",


            //details
            ProductSelectId:".productSelectId",
            BrandIdFromDropdown: ".brandIdFromDropdown ",
            ModelPopulateFromBrandId: ".modelPopulateFromBrandId ",
            SizeSelect: ".sizeSelect",
            PurchaseIssueSelectUnit:".purchaseIssueSelectUnit",
            PurchaseIssueStockQty: ".purchaseIssueStockQty",
            PurchaseIssueQtyOfIssue: ".purchaseIssueQtyOfIssue",
            PurchaseIssueFloor: ".purchaseIssueFloor",
            DetailsDeleteBtn:".delete-issue-details-row-btn",
            DetailsEdit:".details-btn-edit",
            DetailsTcId:"#detailsTcId",
            //create detais 
            PurchaseIssueAddmoreDetailsBtn:"#purchaseIssueAddmoreDetailsBtn",



            ProductItemCloseBtn: "#productItemCloseBtn",
            CloseProductBrandModel: ".closeProductBrandModel",
        }, options);
     
        var loadProductIssueTableDataUrl = commonName.baseUrl + "/LoadData";
        var loadTempProductIssueTableDataUrl = commonName.baseUrl + "/LoadTempData";
        var AutoProdutIssueIdUrl = commonName.baseUrl + "/AutoProdutIssueId";
        var SelectBrandByProductIdUrl = commonName.baseUrl + "/SelectBrandByProductId";
        var SelectModalByBrandIdUrl = commonName.baseUrl + "/SelectModalByBrandId";

        var PurchaseIssueAddmoreDetailsCreateEditUrl = commonName.baseUrl + "/PurchaseIssueAddmoreCreateEditDetails";

        var productUnitModalUrl = "/RMG_Prod_Def_UnitType/Index?isPartial=true";
        var detailsDeleteByIdUrl = commonName.baseUrl + "/detailsDeleteById";
        var detailsEditByIdUrl = commonName.baseUrl + "/detailsEditById";

        var CreateEditProductIssueUrl = commonName.baseUrl + "/CreateEditProductIssue";
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


        function datePiker(selector, inputDate = null, hiddenSelector = null) {
            const parsedDate = inputDate ? new Date(inputDate) : new Date();

            flatpickr(selector, {
                dateFormat: "Y-m-d",
                altInput: true,
                altFormat: "d/m/Y",
                defaultDate: parsedDate,
                allowInput: true,
                onChange: function (selectedDates, dateStr, instance) {
                    if (hiddenSelector) {
                        document.querySelector(hiddenSelector).value = dateStr;
                    }
                }
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
        timePicker = function () {
            flatpickr("#purchaseIssueInlineTimePicker", {
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
        }
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
       
        AutoProdutIssueId = function () {
            $.ajax({
                url: AutoProdutIssueIdUrl,
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



        $(document).on('change', commonName.ProductSelectId, function () {
            let productId = $(this).val();

            $.ajax({
                url: SelectBrandByProductIdUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(productId), // ← Send raw string
                success: function (res) {
                    const $brandSelect = $(commonName.BrandIdFromDropdown);
                    $brandSelect.empty().append('<option value="">Select Brand</option>');
                    res.brandList.forEach(b => {
                        $brandSelect.append(`<option value="${b.brandId}">${b.brandName}</option>`);
                    });
                    $(commonName.ModelPopulateFromBrandId).empty().append(`<option value="">Select Model</option>`);
                },
                error: function (e) {
                    console.log(e);
                }
            });
        });

        $(document).on('change', commonName.BrandIdFromDropdown, function () {
            let brandId = $(this).val();

            $.ajax({
                url: SelectModalByBrandIdUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(brandId),
                success: function (res) {
                    console.log(res);
                    const $modelSelect = $(commonName.ModelPopulateFromBrandId);
                    $modelSelect.empty().append('<option value="">Select Model</option>');
                    res.modelList.forEach(b => {
                        $modelSelect.append(`<option value="${b.modelId}">${b.modelName}</option>`);
                    });
                },
                error: function (e) {
                    console.log(e);
                }
            });
        });

        detailtIssueData = function () {
            fromData = {
                TC: $(commonName.DetailsTcId).val(),
                IssueNo: $(commonName.PurchaseIssueNo).val(),
                ProductCode: $(commonName.ProductSelectId).val(),
                BrandID: $(commonName.BrandIdFromDropdown).val(),
                ModelID: $(commonName.ModelPopulateFromBrandId).val(),
                SizeID: $(commonName.SizeSelect).val(),
                UnitTypID: $(commonName.PurchaseIssueSelectUnit).val(),
                StockQty: $(commonName.PurchaseIssueStockQty).val()||0,
                IssueQty: $(commonName.PurchaseIssueQtyOfIssue).val()||0,
                FloorCode: $(commonName.PurchaseIssueFloor).val()
            };
            return fromData;
        }
        function resetDetailIssueForm() {
            $(commonName.DetailsTcId).val(0);
            $(commonName.ProductSelectId).val('').trigger('change');
            $(commonName.BrandIdFromDropdown).val('').trigger('change');
            $(commonName.ModelPopulateFromBrandId).val('').trigger('change');
            $(commonName.SizeSelect).val('').trigger('change');
            $(commonName.PurchaseIssueSelectUnit).val('').trigger('change');
            $(commonName.PurchaseIssueStockQty).val('');
            $(commonName.PurchaseIssueQtyOfIssue).val('');
            $(commonName.PurchaseIssueFloor).val('').trigger('change');
            $(commonName.PurchaseIssueAddmoreDetailsBtn)
                .removeClass('btn-outline-warning') 
                .addClass('btn-outline-dark')
                .html('<i class="fas fa-plus"></i> <span class="ps-2">Add New</span>');
        }

        //details brn
        $(document).on('click', commonName.PurchaseIssueAddmoreDetailsBtn, function () {
            var fromData = detailtIssueData();
            console.log(fromData);
            $.ajax({
                url: PurchaseIssueAddmoreDetailsCreateEditUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(fromData),
                success: function (res) {
                    if (res.isSuccess) {
                        resetDetailIssueForm();
                        loadTempIssueData();
                    }
                    showToast('success', res.message);
                }, error: function (e) {
                    console.log(e)
                }
            });

        })

        function loadTempIssueData() {
            tableTempContainer.ajax.reload(null, false);
        }
        var tableTempContainer = $('#tempProductIssueTable').DataTable({
            "autoWidth": true,
            "ajax": {
                "url": loadTempProductIssueTableDataUrl,
                "type": "GET",
                "datatype": "json",
                "dataSrc": function (json) {
                    console.log(json);
                    return json.data || [];
                },
                "error": function (xhr, error, thrown) {
                    showToast("error", "Data loading failed: " + xhr.statusText);
                }
            },
            "columns": [
                {
                    "data": "tc",
                    "render": function (data, type, row, meta) {
                        const serial = (meta.row + 1).toString().padStart(3, '0');
                        return `<button class="btn btn-sm btn-link details-btn-edit" data-id=${data}>${serial}</button>`;
                    },
                    "orderable": false
                },
                {"data": "productName"},
                {"data": "description"},
                { "data": "brandName" },
                { "data": "modelName" },
                { "data": "sizeName" },               
                { "data": "unitTypName" },
                { "data": "stockQty" },
                { "data": "issueQty" },
                { "data": "floorName" },
                {
                    "data": "tc",
                    "render": function (data) {
                        return `<button class="btn btn-outline-danger rounded-md border-0 delete-issue-details-row-btn h-auto" data-id=${data}>
                <i class="fas fa-trash-alt"></i>
            </button>`
                    }
                }
            ],
            "columnDefs": [
                {
                    "targets": 2,
                    "width": "auto"
                },
                {
                    "targets": -1,
                    "className":"d-flex justify-content-center align-items-center"
                }
            ], "createdRow": function (row, data, dataIndex) {
                $(row).find('td, th').css({
                    "padding": "0",
                    "margin": "0"
                });
            },
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
       

        $(document).on('click', commonName.DetailsEdit, function () {
            $(commonName.PurchaseIssueAddmoreDetailsBtn).html('<i class="fas fa-sync-alt"></i> <spam class="ps-2">Update</span>');

            const id = $(this).data('id');
            console.log("Clicked Row ID:", id);
            $.ajax({
                url: detailsEditByIdUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(id),
                success: function (res) {
                    if (res.isSuccess) {
                        // Set Product ID only
                        $(commonName.ProductSelectId).val(res.data.productCode).trigger('change');
                        $(commonName.DetailsTcId).val(res.data.tc);
                        // Manually load Brand based on ProductId without triggering 'change'
                        $.ajax({
                            url: SelectBrandByProductIdUrl,
                            type: "POST",
                            contentType: 'application/json',
                            data: JSON.stringify(res.data.productCode),
                            success: function (brandRes) {
                                const $brandSelect = $(commonName.BrandIdFromDropdown);
                                $brandSelect.empty().append('<option value="">Select Brand</option>');
                                brandRes.brandList.forEach(b => {
                                    $brandSelect.append(`<option value="${b.brandId}">${b.brandName}</option>`);
                                });
                                $brandSelect.val(res.data.brandId);

                                // Load model based on selected brandId
                                $.ajax({
                                    url: SelectModalByBrandIdUrl,
                                    type: "POST",
                                    contentType: 'application/json',
                                    data: JSON.stringify(res.data.brandId),
                                    success: function (modelRes) {
                                        const $modelSelect = $(commonName.ModelPopulateFromBrandId);
                                        $modelSelect.empty().append('<option value="">Select Model</option>');
                                        modelRes.modelList.forEach(m => {
                                            $modelSelect.append(`<option value="${m.modelId}">${m.modelName}</option>`);
                                        });
                                        $modelSelect.val(res.data.modelId);
                                    }
                                });
                            }
                        });

                        $(commonName.SizeSelect).val(res.data.sizeId).trigger('change');
                        $(commonName.PurchaseIssueSelectUnit).val(res.data.unitTypId).trigger('change');
                        $(commonName.PurchaseIssueStockQty).val(res.data.stockQty);
                        $(commonName.PurchaseIssueQtyOfIssue).val(res.data.issueQty);
                        $(commonName.PurchaseIssueFloor).val(res.data.floorCode).trigger('change');
                    }
                }
                , error: function (e) {
                    console.log(e);
                }
            });
        });



        $(document).on('click', commonName.DetailsDeleteBtn, function () {
            const id = $(this).data('id');
            console.log("Clicked Row ID:", id);
            $.ajax({
                url: detailsDeleteByIdUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(id),
                success: function (res) {
                    loadTempIssueData();
                }, error: function (e) {
                    console.log(e);
                }
            });
        })
        



        MasterIssueData = function () {
            const dateHiddenInput = document.getElementById("purchaseIssueDatePicker");
            let dateInput = document.getElementById("purchaseIssueDateInput");
            if (!dateInput) {
                dateInput = document.getElementById("purchaseIssueDatePicker"); 
            }
            if (!dateInput) {
                dateInput = document.querySelector('input[type="text"][data-input]'); 
            }
            if (!dateInput) {
                dateInput = document.querySelector('.flatpickr-input');
            }

            const timePickerElem = document.getElementById("purchaseIssueInlineTimePicker");


            if (!timePickerElem || !timePickerElem._flatpickr) {
                return;
            }

            const timeInput = timePickerElem._flatpickr.input;
            const issueDate = dateHiddenInput ? dateHiddenInput.value : "";
            const issueTime = timeInput ? timeInput.value : "";


            if (!issueDate) {

                if (dateInput) {
                    if (dateInput._flatpickr) {
                        dateInput._flatpickr.open();
                    } else {
                        dateInput.click();
                        dateInput.focus();
                    }
                } else if (dateHiddenInput && dateHiddenInput._flatpickr) {
                  
                    dateHiddenInput._flatpickr.open();
                } else {
                    
                    const allFlatpickrInputs = document.querySelectorAll('.flatpickr-input');

                    if (allFlatpickrInputs.length > 0) {
                        const firstDatePicker = allFlatpickrInputs[0];
                        if (firstDatePicker._flatpickr) {
                            firstDatePicker._flatpickr.open();
                        }
                    }
                }
                return;
            }

            if (!issueTime) {              
                if (timeInput && timeInput._flatpickr) {
                    timeInput._flatpickr.open();
                } 
                return;
            }

            const dateTimeString = `${issueDate} ${issueTime}`;
            const parsedDate = new Date(dateTimeString);
            let isoDateTime = "";

            if (!isNaN(parsedDate.getTime())) {
                parsedDate.setHours(parsedDate.getHours() + 6); 
                isoDateTime = parsedDate.toISOString().slice(0, 19);
            } else {
                return;
            }

            const fromData = {
                TC: parseInt($(commonName.AutoId).val()) || 0,
                IssueNo: $(commonName.PurchaseIssueNo).val() || "",
                IssueDate: isoDateTime,
                DepartmentCode: $(commonName.PurchaseIssueDepartment).val() || "",
                EmployeeID: $(commonName.PurchaseIssueEmployeeBtn).val() || "",
                IssuedBy: $(commonName.ProductIssueBy).val() || "",
                Remarks: $(commonName.Remarks).val() || ""
            };

            return fromData;
        };
        function ResetMasterIssueForm() {
            datePiker(".datePicker");
            timePicker();
            $(commonName.AutoId).val(0);
            $(commonName.PurchaseIssueNo).val('');
            $(commonName.PurchaseIssueDepartment).val('').trigger('change');
            $(commonName.PurchaseIssueEmployeeBtn).val('').trigger('change');
            $(commonName.ProductIssueBy).val('').trigger('change');
            $(commonName.Remarks).val('');
        }

        $(document).on('input', commonName.PurchaseIssueNo, function () {
            var issueNo = $(this).val();
            if (!issueNo) {  
                $(commonName.ProductIssueSaveEditBtn).prop('disabled', true);
                return;
            } else {
                $(commonName.ProductIssueSaveEditBtn).prop('disabled', false);
            }
        })
        $(document).on('input', commonName.ProductIssueBy, function () {
            var issueBy = $(this).val();
            if (!issueBy) {  
                $(commonName.ProductIssueSaveEditBtn).prop('disabled', true);
                $(commonName.PurchaseIssueNo).addClass("product-issue-input");
                return;
            } else {
                $(commonName.PurchaseIssueNo).removeClass("product-issue-input");
                $(commonName.ProductIssueSaveEditBtn).prop('disabled', false);
            }
        })

        $(document).on('click', commonName.ProductIssueSaveEditBtn, function () {
            var fromData = MasterIssueData();
            console.log(fromData);
            if (!fromData) return; 

            if (fromData.IssueNo == "" || fromData.IssueNo == 0 || fromData.IssueNo == null) {
                $(commonName.PurchaseIssueNo).addClass("product-issue-input");
                $(commonName.ProductIssueSaveEditBtn).prop('disabled', true);
                return;
            }

            if (fromData.IssuedBy == "" || fromData.IssuedBy == null) {
                $(commonName.ProductIssueBy).select2('open');
                $(commonName.PurchaseIssueNo).addClass("product-issue-input");
                $(commonName.ProductIssueSaveEditBtn).prop('disabled', true);
                return;
            }
            $.ajax({
                url: CreateEditProductIssueUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(fromData),
                success: function (res) {
                    if (res.isSuccess) {
                        showToast('success', res.message);
                        loadMasterIssueData();                       
                        AutoProdutIssueId();
                        resetDetailIssueForm();
                        loadTempIssueData();
                        ResetMasterIssueForm();
                    } else {
                        showToast('error', res.message);
                    }
                },
            })
        })



        function loadMasterIssueData() {
            tableContainer.ajax.reload(null, false);
        }
        var tableContainer = $('#productIssueTable').DataTable({
            "autoWidth": true,
            "ajax": {
                "url": loadProductIssueTableDataUrl,
                "type": "GET",
                "datatype": "json",
                "dataSrc": function (json) {
                    console.log(json);
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
            timePicker();
            AutoProdutIssueId();
            tableContainer;
            tableTempContainer;
        };
        init();
    };
})(jQuery);
