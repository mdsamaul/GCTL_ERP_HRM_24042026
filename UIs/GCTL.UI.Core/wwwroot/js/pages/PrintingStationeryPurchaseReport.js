(function ($) {
    $.fn.PrintingStationeryPurchaseReportJs = function (options) {
        const settings = $.extend({
            noResultsText: 'No results found.',
            multiSelect: true, // Enable multi-select by default
            onSelect: function (values, texts) { }
        }, options);

        return this.each(function () {
            const $originalSelect = $(this);
            const selectId = $originalSelect.attr('id') || 'dropdown_' + Math.random().toString(36).substr(2, 9);
            const selectName = $originalSelect.attr('name') || '';

            // Store selected values
            let selectedValues = [];
            let selectedTexts = [];

            const dropdownHtml = `
                <div class="searchable-dropdown">
                    <div class="position-relative">
                        <input type="text"
                               class="form-control dropdown-input form-control form-control-sm"
                               id="${selectId}_input"
                               placeholder="${$originalSelect.find('option:first').text()}"
                               autocomplete="off">
                        <i class="bi bi-search search-icon"></i>
                        <i class="bi bi-chevron-down dropdown-arrow" id="${selectId}_arrow"></i>
                    </div>
                    <input type="hidden" id="${selectId}_value" name="${selectName}" value="">
                    <ul class="dropdown-menu" id="${selectId}_menu" style="display: none;">
                        ${$originalSelect.find('option:not(:first)').map(function () {
                return `<li>
                            <a class="dropdown-item d-flex align-items-center" href="#" data-value="${$(this).val()}">
                                <input type="checkbox" class="me-2" style="pointer-events: none;">
                                <span>${$(this).text()}</span>
                            </a>
                        </li>`;
            }).get().join('')}
                    </ul>
                </div>
            `;

            $originalSelect.hide().after(dropdownHtml);

            const $dropdownInput = $(`#${selectId}_input`);
            const $dropdownMenu = $(`#${selectId}_menu`);
            const $dropdownArrow = $(`#${selectId}_arrow`);
            const $hiddenInput = $(`#${selectId}_value`);
            let $dropdownItems = $dropdownMenu.find('.dropdown-item');

            let isOpen = false;
            let isSearchMode = false;

            // Update display text based on selections
            function updateDisplayText() {
                if (selectedTexts.length === 0) {
                    $dropdownInput.val('');
                    $dropdownInput.attr('placeholder', $originalSelect.find('option:first').text());
                } else if (selectedTexts.length === 1) {
                    $dropdownInput.val(selectedTexts[0]);
                } else if (selectedTexts.length === 1) {
                    $dropdownInput.val(selectedTexts.join(', '));
                } else {
                    $dropdownInput.val(selectedTexts.slice(0, 1).join(', ') + ` + ${selectedTexts.length - 1} items`);
                }
                isSearchMode = false;
            }

            // Update hidden input with selected values
            function updateHiddenInput() {
                $hiddenInput.val(selectedValues.join(','));
                settings.onSelect(selectedValues, selectedTexts);

                // Console log for debugging
                console.log(`${selectId} Selected IDs:`, selectedValues);
                console.log(`${selectId} Selected Texts:`, selectedTexts);
            }

            // Enter search mode
            function enterSearchMode() {
                if (selectedTexts.length > 0) {
                    $dropdownInput.val('');
                    $dropdownInput.attr('placeholder', 'Search...');
                    isSearchMode = true;
                }
            }

            // Updated input click/focus handler
            $dropdownInput.on('click', function (e) {
                if (!isOpen) {
                    openDropdown();
                } else if (!isSearchMode && selectedTexts.length > 0) {
                    enterSearchMode();
                }
            });

            $dropdownInput.on('focus', function (e) {
                if (!isOpen) {
                    openDropdown();
                }
            });

            $dropdownInput.on('input', function () {
                const searchTerm = $(this).val().toLowerCase();
                isSearchMode = true;
                filterOptions(searchTerm);
                if (!isOpen) {
                    openDropdown();
                }
            });

            $dropdownInput.on('blur', function () {
                // Restore display text when input loses focus
                setTimeout(() => {
                    if (!isOpen && !isSearchMode) {
                        updateDisplayText();
                    }
                }, 200);
            });

            $(document).on('click', function (e) {
                if (!$(e.target).closest('.searchable-dropdown').is($dropdownInput.parent().parent())) {
                    if (isOpen) {
                        closeDropdown();
                    }
                }
            });

            // Event delegation for dropdown items (handles dynamic content)
            $dropdownMenu.on('click', '.dropdown-item', function (e) {
                e.preventDefault();
                const value = $(this).data('value');
                const text = $(this).find('span').text();
                const checkbox = $(this).find('input[type="checkbox"]');

                if (selectedValues.includes(value)) {
                    // Remove from selection
                    selectedValues = selectedValues.filter(v => v !== value);
                    selectedTexts = selectedTexts.filter(t => t !== text);
                    checkbox.prop('checked', false);
                } else {
                    // Add to selection
                    selectedValues.push(value);
                    selectedTexts.push(text);
                    checkbox.prop('checked', true);
                }

                updateHiddenInput();

                // Keep dropdown open and maintain search functionality
                setTimeout(() => {
                    if (isSearchMode) {
                        $dropdownInput.focus();
                    } else {
                        updateDisplayText();
                    }
                }, 10);
            });

            $dropdownInput.on('keydown', function (e) {
                if (e.key === 'Escape') {
                    closeDropdown();
                    updateDisplayText();
                } else if (e.key === 'Tab') {
                    closeDropdown();
                } else if (e.key === 'Backspace' && $(this).val() === '' && selectedTexts.length > 0 && isSearchMode) {
                    // Remove last selected item when backspace is pressed on empty input
                    const removedValue = selectedValues.pop();
                    selectedTexts.pop();

                    // Update checkbox state
                    $dropdownMenu.find('.dropdown-item').each(function () {
                        if ($(this).data('value') == removedValue) {
                            $(this).find('input[type="checkbox"]').prop('checked', false);
                        }
                    });

                    updateHiddenInput();
                    if (selectedTexts.length === 0) {
                        updateDisplayText();
                    }
                }
            });

            function openDropdown() {
                $('.searchable-dropdown .dropdown-menu').hide();
                $('.searchable-dropdown .dropdown-arrow').removeClass('rotated');
                $('.searchable-dropdown').removeClass('active');

                $dropdownMenu.show();
                $dropdownArrow.addClass('rotated');
                $originalSelect.closest('.searchable-dropdown').addClass('active');
                filterOptions($dropdownInput.val().toLowerCase());
                isOpen = true;
            }

            function closeDropdown() {
                $dropdownMenu.hide();
                $dropdownArrow.removeClass('rotated');
                $originalSelect.closest('.searchable-dropdown').removeClass('active');
                updateDisplayText();
                isOpen = false;
                isSearchMode = false;
            }

            function filterOptions(searchTerm) {
                let hasResults = false;
                $dropdownMenu.find('.dropdown-item').each(function () {
                    const text = $(this).find('span').text().toLowerCase();
                    const value = $(this).data('value');

                    // Update checkbox state based on current selections
                    const checkbox = $(this).find('input[type="checkbox"]');
                    checkbox.prop('checked', selectedValues.includes(value));

                    if (text.includes(searchTerm)) {
                        $(this).parent().show();
                        hasResults = true;
                    } else {
                        $(this).parent().hide();
                    }
                });

                let $noResultsItem = $dropdownMenu.find('.no-results');
                if (!hasResults) {
                    if (!$noResultsItem.length) {
                        $dropdownMenu.append('<li class="no-results" style="padding: 0.5rem 1rem; color: #6c757d;">' + settings.noResultsText + '</li>');
                    }
                    $dropdownMenu.find('.no-results').show();
                } else {
                    if ($noResultsItem.length) {
                        $noResultsItem.remove();
                    }
                }
            }

            // Public method to get selected values
            $originalSelect[0].getSelectedValues = function () {
                return selectedValues;
            };

            // Public method to get selected texts
            $originalSelect[0].getSelectedTexts = function () {
                return selectedTexts;
            };

            // Public method to clear selections
            $originalSelect[0].clearSelections = function () {
                selectedValues = [];
                selectedTexts = [];
                $dropdownMenu.find('.dropdown-item input[type="checkbox"]').prop('checked', false);
                updateDisplayText();
                updateHiddenInput();
            };

            // Public method to refresh dropdown items (for dynamic content)
            $originalSelect[0].refreshDropdown = function () {
                $dropdownItems = $dropdownMenu.find('.dropdown-item');
                filterOptions('');
            };
        });
    };

    function getSelectedIds(selector) {
        return typeof $(selector)[0].getSelectedValues === 'function'
            ? $(selector)[0].getSelectedValues()
            : [];
    }

    const filterData = {
        //CategoryIds: getSelectedIds('#categoryDropdown'),
        CategoryIds: $('#categoryDropdown')[0].getSelectedValues ? $('#categoryDropdown')[0].getSelectedValues() : [],
        ProductIds: getSelectedIds('#productDropdown'),
        BrandIds: getSelectedIds('#brandDropdown'),
        ModelIds: getSelectedIds('#modelDropdown'),
        FromDate: $('#formDate').val() ? new Date($('#formDate').val()).toISOString() : null,
        ToDate: $('#toDate').val() ? new Date($('#toDate').val()).toISOString() : null
    };

    function CategoryLoad() {
        console.log("Loading categories...");
        return $.ajax({
            url: "/PrintingStationeryPurchaseReport/GetFilteredDropdowns",
            type: "POST",
            contentType: 'application/json',
            data: JSON.stringify(filterData),
            success: function (res) {
                console.log("Dropdown data loaded:", res);

                populateDropdown("#categoryDropdown", res.categoryIds);
                populateDropdown("#productDropdown", res.productIds);
                populateDropdown("#brandDropdown", res.brandIds);
                populateDropdown("#modelDropdown", res.modelIds);
            },
            error: function (e) {
                console.log("Error loading dropdowns:", e);
            }
        });
    }

    function populateDropdown(selector, items, defaultText = '-- Select --') {
        const $dropdown = $(selector);

        // Clear and reset original select
        $dropdown.empty().append($('<option>').val('').text(defaultText));

        // Populate with new items
        if (items && Array.isArray(items)) {
            items.forEach(item => {
                if (item && item.id !== undefined && item.name !== undefined) {
                    $dropdown.append($('<option>').val(item.id).text(item.name));
                }
            });
        }

        // Update the custom dropdown menu if it exists
        const selectId = $dropdown.attr('id');
        const $dropdownMenu = $(`#${selectId}_menu`);
        const $dropdownInput = $(`#${selectId}_input`);

        if ($dropdownMenu.length && $dropdownInput.length) {
            // Clear existing menu items
            $dropdownMenu.empty();

            // Add new items to custom dropdown
            if (items && Array.isArray(items)) {
                items.forEach(item => {
                    if (item && item.id !== undefined && item.name !== undefined) {
                        const itemHtml = `
                            <li>
                                <a class="dropdown-item d-flex align-items-center" href="#" data-value="${item.id}">
                                    <input type="checkbox" class="me-2" style="pointer-events: none;">
                                    <span>${item.name}</span>
                                </a>
                            </li>
                        `;
                        $dropdownMenu.append(itemHtml);
                    }
                });
            }

            // Clear any existing selections and refresh dropdown
            if (typeof $dropdown[0].clearSelections === 'function') {
                $dropdown[0].clearSelections();
            }

            // Refresh dropdown items
            if (typeof $dropdown[0].refreshDropdown === 'function') {
                $dropdown[0].refreshDropdown();
            }

            // Update placeholder
            $dropdownInput.attr('placeholder', defaultText);
            $dropdownInput.val('');
        }
    }

    function initializeDropdowns() {
        // Initialize multi-select dropdowns
        $('#categoryDropdown').PrintingStationeryPurchaseReportJs({
            onSelect: function (values, texts) {
                console.log("Category - Selected Values:", values);
                console.log("Category - Selected Texts:", texts);
            }
        });

        $('#productDropdown').PrintingStationeryPurchaseReportJs({
            onSelect: function (values, texts) {
                console.log("Product - Selected Values:", values);
                console.log("Product - Selected Texts:", texts);
            }
        });

        $('#brandDropdown').PrintingStationeryPurchaseReportJs({
            onSelect: function (values, texts) {
                console.log("Brand - Selected Values:", values);
                console.log("Brand - Selected Texts:", texts);
            }
        });

        $('#modelDropdown').PrintingStationeryPurchaseReportJs({
            onSelect: function (values, texts) {
                console.log("Model - Selected Values:", values);
                console.log("Model - Selected Texts:", texts);
            }
        });

        $('#formatDropdown').PrintingStationeryPurchaseReportJs({
            onSelect: function (values, texts) {
                console.log("Format - Selected Values:", values);
                console.log("Format - Selected Texts:", texts);
            }
        });
    }

    $(document).ready(function () {
        // First load data, then initialize plugins
        CategoryLoad().then(() => {
            initializeDropdowns();
        });

        // Flatpickr initialization
        flatpickr(".dateInput", {
            dateFormat: "Y-m-d",
            defaultDate: "2025-07-30",
            altInput: true,
            altFormat: "d/m/Y",
            allowInput: true,
        });

        // Example: Get all selected values programmatically
        $(document).on('click', '#btnPreviewPdf', function () {
            console.log({ filterData });
            const filterDatas = {
                //CategoryIds: getSelectedIds('#categoryDropdown'),
                CategoryIds: $('#categoryDropdown')[0].getSelectedValues ? $('#categoryDropdown')[0].getSelectedValues() : [],
                ProductIds: $('#productDropdown')[0].getSelectedValues ? $('#productDropdown')[0].getSelectedValues() : [],
                BrandIds: $('#brandDropdown')[0].getSelectedValues ? $('#brandDropdown')[0].getSelectedValues() : [],
                ModelIds: $('#modelDropdown')[0].getSelectedValues ? $('#modelDropdown')[0].getSelectedValues() : [],
                FromDate: $('#formDate').val() || null,
                ToDate: $('#toDate').val()|| null
            };
            console.log("=== All Selected Values ===");
            console.log("Category:", $('#categoryDropdown')[0].getSelectedValues ? $('#categoryDropdown')[0].getSelectedValues() : []);
            console.log("Product:", $('#productDropdown')[0].getSelectedValues ? $('#productDropdown')[0].getSelectedValues() : []);
            console.log("Brand:", $('#brandDropdown')[0].getSelectedValues ? $('#brandDropdown')[0].getSelectedValues() : []);
            console.log("Model:", $('#modelDropdown')[0].getSelectedValues ? $('#modelDropdown')[0].getSelectedValues() : []);
            console.log("Format:", $('#formatDropdown')[0].getSelectedValues ? $('#formatDropdown')[0].getSelectedValues() : []);

            console.log({ filterDatas});
            $.ajax({
                url: "/PrintingStationeryPurchaseReport/CategoryLoadList",
                type: "POST",
                contentType: 'application/json',
                data: JSON.stringify(filterDatas),
                success: function (res) {
                    console.log("Dropdown data loaded:", res);

                    //populateDropdown("#categoryDropdown", res.categoryIds);
                    //populateDropdown("#productDropdown", res.productIds);
                    //populateDropdown("#brandDropdown", res.brandIds);
                    //populateDropdown("#modelDropdown", res.modelIds);
                },
                error: function (e) {
                    console.log("Error loading dropdowns:", e);
                }
            });
        });
    });

})(jQuery);