(function ($) {
    $.INV_Catagory = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            ShortName: "#ShortName",
            CatagoryName: "#CatagoryName",
            CatagoryID: "#CatagoryID",
            AutoId: "#AutoId",
            RowCheckbox: ".row-checkbox",
            SelectedAll: "#selectAll",
            EditBrn:".btn-edit",
            CatagorySaveBtn: ".js-inv-catagory-save",
            DeleteBtn:"#js-inv-catagory-delete-confirm",
        }, options);

        var loadCategoryDataUrl = commonName.baseUrl + "/LoadData";
        var autoCatagoryIdUrl = commonName.baseUrl + "/AutoCatagoryId";
        var CreateUpdateUrl = commonName.baseUrl + "/CreateUpdate";
        var PopulatedDataForUpdateUrl = commonName.baseUrl + "/PopulatedDataForUpdate";
        var deleteUrl = commonName.baseUrl + "/deleteCatagory";

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
        autoCatagoryId = function () {
            $.ajax({
                url: autoCatagoryIdUrl,
                type: "GET",
                success: function (res) {                   
                    $(commonName.CatagoryID).val(res.data);
                },
                error: function (e) {
                    console.log(e);
                }
            });
        }

        resetFrom = function () {
            $(commonName.AutoId).val(0);
            $(commonName.CatagoryName).val('');
            $(commonName.ShortName).val('');
        }
        // get data from input
        getFromData = function () {
            var fromData = {
                AutoId: $(commonName.AutoId).val(),
                CatagoryID: $(commonName.CatagoryID).val(),
                CatagoryName:$(commonName.CatagoryName).val(),
                ShortName:$(commonName.ShortName).val(),
            };
            return fromData;
        }
        //exists 
        //$(document).on()
        //create and edit
        // Save Button Click
        $(document).on('click', commonName.CatagorySaveBtn, function () {
            var fromData = getFromData();
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
                    autoCatagoryId();
                    loadCategoryData();
                }
            });
        });

        // Reload DataTable Function
        function loadCategoryData() {
            table.ajax.reload(null, false);
        }

        var table = $('#categoryTable').DataTable({
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
                    "data": "catagoryID",
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
       
        //edit
        $(document).on('click', commonName.EditBrn, function () {
            let id = $(this).data('id');
            $.ajax({
                url: `${PopulatedDataForUpdateUrl}?id=${id}`,
                type: "GET",
                success: function (res) {
                    $(commonName.AutoId).val(res.result.autoId);
                    $(commonName.CatagoryName).val(res.result.catagoryName);
                    $(commonName.ShortName).val(res.result.shortName);
                    $(commonName.CatagoryID).val(res.result.catagoryID);
                },
                error: function (e) {
                    console.log(e);
                }              
            });
        });

        //selected id

        let selectedIds = [];

        $(document).on('change', commonName.RowCheckbox, function () {
            console.log($(this).val());
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
            console.log(selectedIds);
            $.ajax({
                url: deleteUrl,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(selectedIds),
                success: function (res) {
                    console.log(res);                   
                    showToast(res.isSuccess?"success":"error", res.message)
                },
                error: function (e) {
                    console.log(e);
                }, complete: function () {                  
                        resetFrom();
                        autoCatagoryId();
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
            autoCatagoryId();
            table;           
        };
        init();

    };
})(jQuery);
