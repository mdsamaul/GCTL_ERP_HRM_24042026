(function ($) {
    $.patientTypes = function (options) {
        var settings = $.extend({
            baseUrl: "/",
        }, options);
        var filterUrl = settings.baseUrl + "/GetFilterData";

        function stHeader() {
            window.addEventListener('scroll', function () {
                const header = document.getElementById('stickyHeader');
                if (window.scrollY > 0) {
                    header.classList.add('scrolled');
                } else {
                    header.classList.remove('scrolled');
                }
            });
        }

        var $input = $('#multiselectInput');
        var $arrow = $('#multiselectArrow');
        var $dropdown = $('#multiselectDropdown');
        var $selectedValuesSpan = $('#selectedValues');

        // Dynamic options array
        var dynamicOptions = [
            { value: 'personal-loan', label: 'Personal Loan' },
            { value: 'home-loan', label: 'Home Loan' },
            { value: 'car-loan', label: 'Car Loan' },
            { value: 'business-loan', label: 'Business Loan' },
            { value: 'education-loan', label: 'Education Loan' }
        ];

        async function fetchOptionsFromAPI() {
            try {
                // Demo data for example
                return [
                    { value: 'api-option-1', label: 'API Option 1' },
                    { value: 'api-option-2', label: 'API Option 2' },
                    { value: 'api-option-3', label: 'API Option 3' }
                ];
            } catch (error) {
                console.error('Error fetching options:', error);
                return [];
            }
        }

        function generateOptions(options) {
            $dropdown.empty();
            $.each(options, function (index, option) {
                var checkboxId = 'option' + (index + 1);
                var $optionDiv = $('<div>', { class: 'multiselect-option' });
                var $checkbox = $('<input>', {
                    type: 'checkbox',
                    id: checkboxId,
                    value: option.value
                });
                var $label = $('<label>', {
                    for: checkboxId,
                    text: option.label
                });
                $optionDiv.append($checkbox).append($label);
                $dropdown.append($optionDiv);

                $checkbox.on('change', function () {
                    updateInputValue();
                    updateSelectedValues();
                });
            });
        }

        // নতুন ফাংশন: initMultiselect
        function initMultiselect() {
            generateOptions(dynamicOptions);

            $input.on('click', function () {
                $dropdown.toggleClass('show');
                $arrow.toggleClass('rotated');
            });

            $(document).on('click', function (event) {
                if ($(event.target).closest('.custom-multiselect').length === 0) {
                    $dropdown.removeClass('show');
                    $arrow.removeClass('rotated');
                }
            });

            $dropdown.on('click', function (event) {
                event.stopPropagation();
            });

            updateInputValue();
            updateSelectedValues();
        }

        function updateInputValue() {
            var selectedLabels = $dropdown.find('input[type="checkbox"]:checked').map(function () {
                return $(this).next('label').text().trim();
            }).get();

            if (selectedLabels.length === 0) {
                $input.val('');
                $input.attr('placeholder', 'Select options...');
            } else if (selectedLabels.length === 1) {
                $input.val(selectedLabels[0]);
                $input.attr('placeholder', '');
            } else {
                $input.val(selectedLabels[0] + ' +' + (selectedLabels.length - 1) + ' more');
                $input.attr('placeholder', '');
            }
        }

        function updateSelectedValues() {
            var selectedValues = $dropdown.find('input[type="checkbox"]:checked').map(function () {
                return $(this).val();
            }).get();

            $selectedValuesSpan.text(selectedValues.length > 0 ? selectedValues.join(', ') : 'None');
        }

        // Global functions (window scope) for API load, add, remove, update, clear
        window.loadFromAPI = async function () {
            var apiOptions = await fetchOptionsFromAPI();
            dynamicOptions = apiOptions;
            generateOptions(dynamicOptions);
            updateInputValue();
            updateSelectedValues();
        };

        window.addOption = function (value, label) {
            dynamicOptions.push({ value: value, label: label });
            generateOptions(dynamicOptions);
            updateInputValue();
            updateSelectedValues();
        };

        window.removeOption = function (value) {
            dynamicOptions = dynamicOptions.filter(function (opt) {
                return opt.value !== value;
            });
            generateOptions(dynamicOptions);
            updateInputValue();
            updateSelectedValues();
        };

        window.updateOptions = function (newOptions) {
            dynamicOptions = newOptions;
            generateOptions(dynamicOptions);
            updateInputValue();
            updateSelectedValues();
        };

        window.clearOptions = function () {
            dynamicOptions = [];
            $dropdown.empty();
            $input.val('');
            $input.attr('placeholder', 'No options available');
            $selectedValuesSpan.text('None');
        };

        var init = function () {
            stHeader();
            initMultiselect();
            console.log("test", filterUrl);
        };
        init();
    };
})(jQuery);
