
(function ($) {
    $.patientTypes = function (options) {
        var commonName = $.extend({
            baseUrl: "/",
            CompanyMultiSelectInput: "#",

        }, options);
        var filterUrl = commonName.baseUrl + "/GetFilterData";
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

        function test() {
            console.log("test");
            showToast("success","test");
        }

        var init = function () {
            stHeader();
            test();
        };
        init();
    };
})(jQuery);
