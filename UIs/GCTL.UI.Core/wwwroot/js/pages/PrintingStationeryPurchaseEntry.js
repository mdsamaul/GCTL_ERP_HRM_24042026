
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

        datePiker = flatpickr("#datePicker", {
                dateFormat: "Y-m-d",
                defaultDate: new Date(), // current date
                allowInput: true
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
            onChange: function (selectedDates, dateStr) {
                document.getElementById("timePicker").value = dateStr;
            }
        });

       
        var init = function () {
            stHeader();
            datePiker;
            timePicker;
        };
        init();
    };
})(jQuery);
