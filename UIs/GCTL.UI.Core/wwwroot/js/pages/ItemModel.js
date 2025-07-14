(function ($) {
    $.ItemModel = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            ShortName: "#shortName",
            ItemModelName: "#itemModelName",
            ItemModelBrand: "#itemModelBrand",
            ItemModelID: "#itemModelId",
            AutoId: "#AutoId",
            RowCheckbox: ".row-checkbox",
            SelectedAll: "#selectAll",
            EditBrn: ".model-btn-edit",
            ItemModelSaveBtn: ".js-inv-ItemModel-save",
            DeleteBtn: "#js-inv-ItemModel-delete-confirm",
            UpdateDate: ".updateDate",
            CreateDate: ".createDate",
            ClearBrn: "#js-ItemModel-clear",
        }, options);

        var loadCategoryDataUrl = commonName.baseUrl + "/LoadData";
        var autoItemModelIdUrl = commonName.baseUrl + "/AutoItemModelId";
        var CreateUpdateUrl = commonName.baseUrl + "/CreateUpdate";
        var PopulatedDataForUpdateUrl = commonName.baseUrl + "/PopulatedDataForUpdate";
        var deleteUrl = commonName.baseUrl + "/deleteItemModel";
        var alreadyExistUrl = commonName.baseUrl + "/alreadyExist";

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
        autoItemModelId = function () {
            $.ajax({
                url: autoItemModelIdUrl,
                type: "GET",
                success: function (res) {
                    $(commonName.ItemModelID).val(res.data);
                },
                error: function (e) {
                }
            });
        }

        resetFrom = function () {
            $(commonName.AutoId).val(0);
            $(commonName.ItemModelName).val('');
            $(commonName.ShortName).val('');
            autoItemModelId();
        }
        $(commonName.ClearBrn).on('click', function () {
            resetFrom();
        })
        // get data from input
        getFromData = function () {
            var fromData = {
                AutoId: $(commonName.AutoId).val(),
                ItemModelID: $(commonName.ItemModelID).val(),
                ItemModelName: $(commonName.ItemModelName).val(),
                ShortName: $(commonName.ShortName).val(),
            };
            return fromData;
        }
        //exists 
        $(commonName.ItemModelName).on('input', function () {

            let ItemModelValue = $(this).val();

            $.ajax({
                url: alreadyExistUrl,
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(ItemModelValue),
                success: function (res) {
                    if (res.isSuccess) {
                        showToast('warning', res.message);
                        $(commonName.ItemModelName).addClass('ItemModel-input');
                        $(commonName.ItemModelSaveBtn).prop('disabled', true);
                    } else {
                        $(commonName.ItemModelName).removeClass('ItemModel-input');
                        $(commonName.ItemModelSaveBtn).prop('disabled', false);
                        $(commonName.ItemModelSaveBtn).css('border', 'none');

                    }
                }, error: function (e) {
                }
            });
        })
        //create and edit
        // Save Button Click
        $(document).on('click', commonName.ItemModelSaveBtn, function () {
            var fromData = getFromData();
            if (fromData.ItemModelName == null || fromData.ItemModelName.trim() === '') {
                $(commonName.ItemModelName).addClass('ItemModel-input');
                $(commonName.ItemModelSaveBtn).prop('disabled', true);
                $(commonName.ItemModelName).focus();
                return;
            }

            console.log(fromData);
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
                    autoItemModelId();
                    loadCategoryData();
                }
            });
        });

        // Reload DataTable Function
        function loadCategoryData() {
            table.ajax.reload(null, false);
        }

        var table = $('#itemModelTable').DataTable({
            "autoWidth": true,
            "ajax": {
                "url": loadCategoryDataUrl,
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
                    "data": "autoId",
                    "render": function (data) {
                        return `<input type="checkbox" class="row-checkbox" value=${data} />`;
                    },
                    "orderable": false
                },
                {
                    "data": "modelID",
                    "render": function (data) {
                        return `<button class="btn btn-sm btn-link model-btn-edit" data-id=${data}>${data}</button>`;
                    }
                },
                { "data": "modelName" },
                { "data": "shortName" },
                { "data": "brandName" }
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
            console.log(id);
            $.ajax({
                url: `${PopulatedDataForUpdateUrl}?id=${id}`,
                type: "GET",
                success: function (res) {
                    console.log(res);
                    selectedIds = [];
                    selectedIds.push(res.result.autoId + '');
                    $(commonName.AutoId).val(res.result.autoId);
                    $(commonName.ItemModelName).val(res.result.modelName);
                    $(commonName.ShortName).val(res.result.shortName);
                    $(commonName.ItemModelID).val(res.result.modelID); $(commonName.ItemModelBrand).val(res.result.brandID);
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
                    autoItemModelId();
                    loadCategoryData();
                    $('#selectAll').prop('checked', false);
                    selectedIds = [];
                }
            })
        })


        window.categoryModuleLoaded = true;
        // Initialize all functions
        var init = function () {
            stHeader();
            autoItemModelId();
            table;
            console.log("tepppppppppppp");
        };
        init();

    };
})(jQuery);
