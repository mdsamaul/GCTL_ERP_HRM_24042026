(function ($) {
    $.patientTypes = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            ShortName: "#ShortName",
            CatagoryName: "#CatagoryName",
            CatagoryID: "#CatagoryID",
            AutoId: "#AutoId",
            CatagorySaveBtn: ".js-inv-catagory-save",
        }, options);

        var loadCategoryDataUrl = commonName.baseUrl + "/LoadData";
        var autoCatagoryIdUrl = commonName.baseUrl + "/AutoCatagoryId";
        var CreateUpdateUrl = commonName.baseUrl + "/CreateUpdate";
        var PopulatedDataForUpdateUrl = commonName.baseUrl + "/PopulatedDataForUpdate";

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
                    console.log(res);
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


        // DataTable with console log of data
        var table = $('#categoryTable').DataTable({
            "ajax": {
                "url": loadCategoryDataUrl,
                "type": "GET",
                "datatype": "json",
                "dataSrc": function (json) {
                    console.log("AJAX response from server:", json); // এখানে console এ পূর্ণ JSON দেখাবে
                    return json.data || []; // fallback to empty array if data is undefined/null
                },
                "error": function (xhr, error, thrown) {
                    console.error("AJAX error occurred:", xhr.responseText);
                    alert("Data loading failed: " + xhr.statusText);
                }
            },
            "columns": [
                {
                    "data": "autoId",
                    "render": function (data) {
                        return `<input type="checkbox" class="row-checkbox" value="${data}" />`;
                    },
                    "orderable": false
                },
                {
                    "data": "catagoryID",
                    "render": function (data) {
                        return `<button class="btn btn-sm btn-link btn-edit" data-id=${data}>${data}</button>`
                    }
                },
                { "data": "catagoryName" },
                { "data": "shortName" },                
            ]
        });
    //edit
        $(document).on('click', '.btn-edit', function () {
            let id = $(this).data('id');
            console.log("Clicked Edit ID:", id);
            $.ajax({
                url: `${PopulatedDataForUpdateUrl}?id=${id}`,
                type: "GET",
                success: function (res) {
                    console.log(res);
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

        // Initialize all functions
        var init = function () {
            stHeader(); // Header scroll effect
            autoCatagoryId();
            table;
        };
        init();
    };
})(jQuery);
