﻿
(function ($) {
    $.ItemModel = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            CompanyMultiSelectInput: "#",
            ShortName: "#ShortName",
            CatagoryName: "#CatagoryName",
            PurchaseOrderNo: "#purchaseOrderNo",
            AutoId: "#AutoId",
            RowCheckbox: ".row-checkbox",
            SelectedAll: "#selectAll",
            EditBrn: ".btn-edit",
            CatagorySaveBtn: ".js-inv-catagory-save",
            DeleteBtn: "#js-inv-catagory-delete-confirm",
            UpdateDate: ".updateDate",
            CreateDate: ".createDate",
            ClearBrn: "#js-catagory-clear",

            ProductModalBtn: "#productModalBtn",
            ProductPartialContainer: "#productPartialContainer",
            ProductBrandModalBtn: "#productBrandModalBtn",
            ProductBrandContainer: "#productBrandContainer",

            ProductModelBtn: "#productModelBtn",
            ProductModelContainer: "#productModelContainer",

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
        var productModelUrl = "/ItemModel/Index?isPartial=true"
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
        $(commonName.ProductModalBtn).on('click', function () {
            $.ajax({
                url: productModelUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.ProductModelContainer).html(res);
                    if (typeof )
                }, error: function (e) {
                    console.log(e)
                }
            })
        })



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
            $(commonName.CatagoryName).val('');
            $(commonName.ShortName).val('');
        }
        $(commonName.ClearBrn).on('click', function () {
            resetFrom();
        })
        // get data from input
        getFromData = function () {
            var fromData = {
                AutoId: $(commonName.AutoId).val(),
                PurchaseOrderNo: $(commonName.PurchaseOrderNo).val(),
                CatagoryName: $(commonName.CatagoryName).val(),
                ShortName: $(commonName.ShortName).val(),
            };
            return fromData;
        }
        //exists 
        $(commonName.CatagoryName).on('input', function () {

            let CatagoryValue = $(this).val();

            $.ajax({
                url: alreadyExistUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(CatagoryValue),
                success: function (res) {
                    if (res.isSuccess) {
                        showToast('warning', res.message);
                        $(commonName.CatagoryName).addClass('catagory-input');
                        $(commonName.CatagorySaveBtn).prop('disabled', true);
                    } else {
                        $(commonName.CatagoryName).removeClass('catagory-input');
                        $(commonName.CatagorySaveBtn).prop('disabled', false);
                        $(commonName.CatagorySaveBtn).css('border', 'none');

                    }
                }, error: function (e) {
                }
            });
        })
        //create and edit
        // Save Button Click
        $(document).on('click', commonName.CatagorySaveBtn, function () {
            var fromData = getFromData();
            if (fromData.CatagoryName == null || fromData.CatagoryName.trim() === '') {
                $(commonName.CatagoryName).addClass('catagory-input');
                $(commonName.CatagorySaveBtn).prop('disabled', true);
                $(commonName.CatagoryName).focus();
                return;
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
                    resetFrom();
                    AutoPrintingStationeryPurchaseId();
                    loadCategoryData();
                }
            });
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
                { "data": "catagoryName" },
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
                    $(commonName.CatagoryName).val(res.result.catagoryName);
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
