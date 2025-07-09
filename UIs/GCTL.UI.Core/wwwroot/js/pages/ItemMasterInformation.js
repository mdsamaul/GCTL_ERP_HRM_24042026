
(function ($) {
    $.ItemMasterInformation = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            CompanyMultiSelectInput: "#",
            CatagoryBtn: ".catagoryBtn",
            CloseCatagoryModel: ".closeCatagoryModel",
            DropdownCategory:".dropdownCategory",

        }, options);
        var filterUrl = commonName.baseUrl + "/GetFilterData";
        var categoryListUrl = commonName.baseUrl + "/categoryList";
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
        //$(document).on('click', commonName.CatagoryBtn, function () {
        //    console.log("click");
        //})
      

      
        $(commonName.CatagoryBtn).on('click', function () {
            
            const $btn = $(this);

            //$btn.prop('disabled', true).text('Loading...');

            $.ajax({
                url: '/INV_Catagory/Index?isPartial=true',
                type: 'GET',
                success: function (result) {                   
                    $('#catagoryContainer').html(result);
                    if (typeof $.INV_Catagory === 'function') {
                        var options = {
                            baseUrl: '/INV_Catagory',
                            isPartial: true,                            
                        };
                        $.INV_Catagory(options);
                    }                   
                },
                error: function () {
                    alert("Failed to load category page");
                    $btn.prop('disabled', false).text('Load Category');
                }
            });
        });


        $(commonName.CloseCatagoryModel).on('click', function () {     
            $.ajax({
                url: categoryListUrl,
                type: "GET",
                success: function (res) {
                    console.log(res);
                    $(commonName.DropdownCategory).empty();

                    res.data.forEach(function (item) {
                        $(commonName.DropdownCategory).append(
                            $('<option></option>').val(item.catagoryId).text(item.catagoryName)
                        );
                    });

                }, error: function (error) {
                    console.log(error);
                }
            });
        })

        var init = function () {
            stHeader();
        };
        init();
    };
})(jQuery);
