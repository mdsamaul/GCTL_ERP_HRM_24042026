(function ($) {
    $.RMGProdOrderInformationEntry = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: "/",
            MainSetup_MainItemID: "#MainSetup_MainItemID",
           

            ItemSaveBtn: ".js-MainI-tem-Group-save",
            ItemDeleteBtn: "#js-MainI-tem-Group-delete-confirm",
            ClearBtn: "#js-MainI-tem-Group-clear",

        }, options);


        var gridUrl = settings.baseUrl + "/grid";
        var saveEditUrl = settings.baseUrl + "/AdddEditSetup";
       

        var GetAutoAllIdUrl = settings.baseUrl + "/GetAutoAllId";
        var DeleteItemUrl = settings.baseUrl + "/DeleteItemUrl";

        var ChangeAbleDropdownUrl = settings.baseUrl + "/ChangeAbleDropdown";

        //$('.searchable-select').select2({
        //    placeholder: 'Select an option',
        //    allowClear: true,
        //    width: '100%',
        //    language: { noResults: () => 'No results found' },
        //    escapeMarkup: markup => markup
        //});
        // Sticky header on scroll
        function stHeader() {
            window.addEventListener('scroll', function () {
                const header = document.getElementById('stickyHeader');

                if (window.scrollY > 750) {
                    header.classList.add('hide-header');
                    header.classList.remove('sticky-top');
                }
                else if (window.scrollY > 10) {
                    header.classList.add('sticky-top');
                    header.classList.add('sticky-scrolled');
                    header.classList.remove('hide-header');
                }
                else {
                    header.classList.add('sticky-top');
                    header.classList.remove('sticky-scrolled');
                    header.classList.remove('hide-header');
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
        $(document).ready(function () {
            // Initial state: Show password row, hide samaul row
            $('.styleWiseRow').show();
            $('.masterPoWise').hide();

            // Toggle on radio change
            $('input[name="option"]').on('change', function () {
                if ($('#styleWise').is(':checked')) {
                    $('.styleWiseRow').show();
                    $('.masterPoWise').hide();
                } else if ($('#poWise').is(':checked')) {
                    $('.styleWiseRow').hide();
                    $('.masterPoWise').show();
                }
            });
        });


        $(document).ready(function () {
            // Initialize Buyer Contact Person Multiselect
            $('#buyerContactPerson').multiselect({
                includeSelectAllOption: true,
                selectAllText: 'Select All',
                enableFiltering: true,
                enableCaseInsensitiveFiltering: true,
                filterPlaceholder: 'Search contacts...',
                buttonWidth: '100%',
                maxHeight: 250,
                numberDisplayed: 2,
                nonSelectedText: 'Select contacts',
                nSelectedText: 'selected',
                allSelectedText: 'All selected',
                buttonClass: 'btn btn-sm form-select'
            });

            // Initialize Merchandiser Contact Person Multiselect
            $('#merchandiserContactPerson').multiselect({
                includeSelectAllOption: true,
                selectAllText: 'Select All',
                enableFiltering: true,
                enableCaseInsensitiveFiltering: true,
                filterPlaceholder: 'Search merchandisers...',
                buttonWidth: '100%',
                maxHeight: 250,
                numberDisplayed: 2,
                nonSelectedText: 'Select merchandisers',
                nSelectedText: 'selected',
                allSelectedText: 'All selected',
                buttonClass: 'btn btn-sm form-select'
            });           
        });


        $(document).ready(function () {
            stHeader()
            console.log("page is running");
        })


        $(document).ready(function () {
            // ✅ Dropdown open হলে table show হবে
            $('#merchandiserContactPerson').on('mousedown', function (e) {
                // prevent default dropdown close
                e.stopPropagation();

                // show the table
                $('#employeeContainer').slideToggle(200);

                // initialize datatable only once
                if (!tableInitialized) {
                    initDataTable();
                    tableInitialized = true;
                }
            });

            // ✅ Body তে click করলে table hide হবে
            $(document).on('click', function (e) {
                if (!$(e.target).closest('#employeeContainer, #merchandiserContactPerson').length) {
                    $('#employeeContainer').slideUp(200);
                }
            });

            // ✅ DataTable initialize function
            function initDataTable() {
                $('#employeeTable').DataTable({
                    data: employees,
                    columns: [
                        {
                            data: "id",
                            render: function (data) {
                                return `<input type="checkbox" class="row-check" value="${data}">`;
                            },
                            orderable: false
                        },
                        { data: "name" },
                        { data: "designation" },
                        { data: "phone" },
                        { data: "email" }
                    ]
                });
            }

            // ✅ Checkbox select event
            $('#employeeTable').on('change', '.row-check', function () {
                const empId = parseInt($(this).val());
                if ($(this).is(':checked')) {
                    selectedIds.push(empId);
                } else {
                    selectedIds = selectedIds.filter(id => id !== empId);
                }
                console.log("✅ Selected Employee IDs:", selectedIds);
            });

            // ✅ Select all checkbox
            $('#selectAll').on('change', function () {
                const isChecked = $(this).is(':checked');
                $('.row-check').prop('checked', isChecked).trigger('change');
            });
        });


    }





}(jQuery));