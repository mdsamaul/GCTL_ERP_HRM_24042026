(function ($) {
    $.deductionTypes = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: "/",
            formSelector: "#deduction-type-form",
            formContainer: ".js-deduction-type-form-container",
            gridSelector: "#deduction-type-grid",
            gridContainer: ".js-deduction-type-grid-container",
            editSelector: ".js-deduction-type-edit",
            saveSelector: ".js-deduction-type-save",
            selectAllSelector: "#deduction-type-check-all",
            deleteSelector: ".js-deduction-type-delete-confirm",
            deleteModal: "#deduction-type-delete-modal",
            finalDeleteSelector: ".js-deduction-type-delete",
            clearSelector: ".js-deduction-type-clear",
            topSelector: ".js-go",
            decimalSelector: ".js-deduction-type-decimalplaces",
            deductionTye: "#DeductionType",
            //shortNameSelector: "#ShortName", // Added shortNameSelector
            maxDecimalPlace: 5,
            showNagativeFormat: false,
            availabilitySelector: ".js-deduction-type-check-availability",
            haseFile: false,
            quickAddSelector: ".js-quick-add",
            quickAddModal: "#quickAddModal",
            lastCodeSelector: "#lastCode",
            onSaved: function (deductionTypeId) {

            },
            load: function () {

            }
        }, options);


        var gridUrl = settings.baseUrl + "/grid";
        var saveUrl = settings.baseUrl + "/setup";
        var deleteUrl = settings.baseUrl + "/Delete";
        var selectedItems = [];
        $(() => {
            //settings.load(settings.baseUrl, settings.gridSelector);
            loadDeductionTypes(settings.baseUrl, settings.gridSelector);
            initialize();
            //if (settings.isModal && settings.quickAddModal) {
            //    trapFocusInModal(settings.quickAddModal);
            //}
            $("body").on("click", `${settings.editSelector},${settings.clearSelector}`, function (e) {
                e.stopPropagation();
                e.preventDefault();
                e.stopImmediatePropagation();

                let url = saveUrl + "/" + $(this).data("id") ?? "";
                loadForm(url);

                // Setting id on the delete selector for delete
                var id = $(this).data('id');
                $(settings.deleteSelector).data('id', id);

                $("html, body").animate({ scrollTop: 0 }, 500);
            });

            // Save
            $("body").off("click", settings.saveSelector).on("click", settings.saveSelector, function () {
                // Check if short name is empty
                //var shortName = $(settings.shortNameSelector).val();
                //if (!shortName || shortName.trim() === "") {
                //    toastr.error("Sort name is required", "Validation Error");
                //    $(settings.shortNameSelector).focus();
                //    return false;
                //}

                var $valid = $(settings.formSelector).valid();
                if (!$valid) {
                    return false;
                }

                var data;
                if (settings.haseFile)
                    data = new FormData($(settings.formSelector)[0]);
                else
                    data = $(settings.formSelector).serialize();

                var url = $(settings.formSelector).attr("action");

                var options = {
                    url: url,
                    method: "POST",
                    data: data,
                    success: function (response) {
                        //ActiveDeductionType.storeCurrentValues();
                        //console.log(response);
                        loadForm(saveUrl)
                            .then((data) => {
                                loadDeductionTypes(settings.baseUrl, settings.gridSelector);
                                $(settings.lastCodeSelector).val(response.lastCode);
                            })
                            .catch((error) => {
                                console.log(error)
                            })

                        toastr.success(response.success, 'Success');

                        if (settings.isModal && typeof settings.onSaved === 'function') {
                            //debugger;
                            settings.onSaved(response.lastCode);
                            // Clear after use
                            //ActiveDeductionType.clear();
                        }
                    },
                    error: function (response) {
                        toastr.error(response.message, 'Error');
                        console.log(response);
                    }
                }
                if (settings.haseFile) {
                    options.processData = false;
                    options.contentType = false;
                }
                $.ajax(options);
            });

            $("body").on("click", settings.selectAllSelector, function () {
                $(".checkBox").prop('checked',
                    $(this).prop('checked'));
            });


            $("body").on("click", settings.deleteSelector, function (e) {
                e.preventDefault();
                if ($(this).data('id')) {
                    selectedItems.push($(this).data('id'));
                } else {
                    $('input:checkbox.checkBox').each(function () {
                        if ($(this).prop('checked')) {
                            if (!selectedItems.includes($(this).val())) {
                                selectedItems.push($(this).val());
                            }
                        }
                    });
                }

                if (selectedItems.length > 0) {
                    $(settings.deleteModal).modal("show");
                } else {
                    toastr.info("Please select at least one item.", 'Warning');
                }
            });


            $("body").on('show.bs.modal', settings.deleteModal, function (event) {
                //event.preventDefault();
                // Get button that triggered the modal
                var source = $(event.relatedTarget);
                var id = source.data("id");

                // Extract value from data-* attributes
                var title = source.data("title");
                title = "Are you sure want to delete these items?";
                var modal = $(this);
                $(modal).find('.title').html(title);

                $("body").on("click", settings.finalDeleteSelector, function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    e.stopImmediatePropagation();


                    // Delete
                    $.ajax({
                        url: deleteUrl + "/" + selectedItems.join(","),
                        method: "POST",
                        success: function (response) {
                            console.log(response);
                            $(modal).modal("hide");
                            if (response.success) {
                                toastr.success(response.message, 'Success');
                                selectedItems = [];
                                /*settings.load(settings.baseUrl);*/
                                loadDeductionTypes(settings.baseUrl, settings.gridSelector);
                                loadForm(saveUrl);

                                //if (settings.isModal && typeof settings.onSaved === 'function') {
                                //    settings.onSaved(null);
                                //}
                            }
                            else {
                                toastr.error(response.message, 'Error');
                                console.log(response);
                            }
                        }
                    });
                });

            }).on('hide.bs.modal', function () {
                $("body").off("click", settings.finalDeleteSelector);
            });

            $("body").on("click", settings.topSelector, function (e) {
                e.preventDefault();
                $("html, body").animate({ scrollTop: 0 }, 500);
            });


            $("body").on("keyup", settings.decimalSelector, function () {
                var self = $(this);
                showDecimalPlaces(self.val(), self.parent().find(".input-group-text"));
            });

            // Add blur event handler for short name field
            //$("body").on("blur", settings.shortNameSelector, function () {
            //    var self = $(this);
            //    if (!self.val() || self.val().trim() === "") {
            //        toastr.warning("Short name is required", "Validation");
            //    }
            //});

            $("body").on("keyup", settings.availabilitySelector, function () {
                var self = $(this);
                let code = $(".js-deduction-type-code").val();
                let name = self.val();

                // check
                $.ajax({
                    url: settings.baseUrl + "/CheckAvailability",
                    method: "POST",
                    data: { code: code, name: name },
                    success: function (response) {
                        console.log(response);
                        if (response.isSuccess) {
                            toastr.error(response.message);
                        }
                    }
                });
            });
        });

        function loadDeductionTypes(baseUrl, gridSelector) {
            //debugger;
            var dataTable = $(gridSelector).DataTable({
                ajax: {
                    url: baseUrl + "/grid",
                    type: "GET",
                    datatype: "json"
                },

                columnDefs: [
                    { targets: [0], orderable: false },
                ],
                columns: [
                    {
                        "data": "deductionTypeId", "className": "text-center", width: "2%", "render": function (data, type, row) {
                            return ` <input type="checkbox" class="checkBox" value="${row.tc}" />`;
                        }
                    },
                    {
                        "data": "deductionTypeId", width: "7%", "render": function (data, type, row) {
                            return `<a class='btn js-deduction-type-edit' data-id='${row.tc}'><i>${data}</i></a>`;
                        }
                    },
                    { "data": "deductionType", "autowidth": true, "className": "text-center" },
                    { "data": "shortName", "autowidth": true, "className": "text-center" },
                ],
                lengthChange: true,
                pageLength: 10,
                lengthMenu: [
                    [10, 25, 50, -1],
                    [10, 25, 50, 'All'],
                ],
                order: [[1, "Desc"]],
                destroy: true
            });

            $(gridSelector).on('xhr.dt', function (e, settings, json, xhr) {
                console.log("AJAX response:", json);
            });
        }

        function loadForm(url) {
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: url,
                    type: 'GET',
                    success: function (data) {
                        $(settings.formContainer).empty();
                        $(settings.formContainer).html(data);
                        $.validator.unobtrusive.parse($(settings.formSelector));

                        // Add client-side validation for short name
                        $(settings.shortNameSelector).attr("required", "required");

                        initialize();
                        resolve(data)
                    },
                    error: function (error) {
                        reject(error)
                    },
                })
            })
        }
        $(settings.formSelector).on('keydown', 'input, select, textarea, button, [tabindex]:not([tabindex="-1"])', function (e) {
            if (e.key === 'Tab') {
                const isShift = e.shiftKey;
                const $form = $(settings.formSelector);
                const $focusable = $form
                    .find('[tabindex]:not([disabled]):not([tabindex="-1"])')
                    .filter(':visible')
                    .sort(function (a, b) {
                        return parseInt($(a).attr('tabindex')) - parseInt($(b).attr('tabindex'));
                    });

                const index = $focusable.index(this);
                if (index > -1) {
                    e.preventDefault();
                    let nextIndex;

                    if (isShift) {
                        nextIndex = (index - 1 + $focusable.length) % $focusable.length;
                    } else {
                        nextIndex = (index + 1) % $focusable.length;
                    }

                    $focusable.eq(nextIndex).focus();
                }
            }
        });

        function initialize() {
            $(settings.formSelector + ' .selectpicker').select2({
                language: {
                    noResults: function () {
                        //return 'Not found <a class="add_new_item" href="javascript:void(0)">Add New</a>';
                    }
                },
                escapeMarkup: function (markup) {
                    return markup;
                }
            });
            // debugger;
            setTimeout(function () {
                //$('#DeductionType').focus();
                $(settings.formSelector).find('#DeductionType').focus();
            }, 500);
            // Focus first field

            // Unified Enter key navigation
            $(settings.formSelector).on('keydown', 'input, select, textarea, button, [tabindex]:not([tabindex="-1"])', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();

                    const $form = $(settings.formSelector);
                    const $focusable = $form
                        .find('input:not([disabled]), select:not([disabled]), textarea:not([disabled]), button, [href], [tabindex]:not([tabindex="-1"])')
                        .filter(':visible');

                    const index = $focusable.index(this);
                    if (index > -1) {
                        const $next = $focusable.eq(index + 1).length ? $focusable.eq(index + 1) : $focusable.eq(0);
                        $next.focus();
                    }
                }
            });
        }
    }
}(jQuery));