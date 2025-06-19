
(function ($) {
    $.benefitTypes = function (options) {
        var settings = $.extend({
            baseUrl: "/",
            formSelector: "#Benefit-type-form",
            formContainer: ".js-Benefit-type-form-container",
            gridSelector: "#Benefit-type-grid",
            gridContainer: ".js-Benefit-type-grid-container",
            editSelector: ".js-Benefit-type-edit",
            saveSelector: ".js-Benefit-type-save",
            selectAllSelector: "#Benefit-type-check-all",
            deleteSelector: ".js-Benefit-type-delete-confirm",
            deleteModal: "#Benefit-type-delete-modal",
            finalDeleteSelector: ".js-Benefit-type-delete",
            clearSelector: ".js-Benefit-type-clear",
            topSelector: ".js-go",
            decimalSelector: ".js-Benefit-type-decimalplaces",
            maxDecimalPlace: 5,
            showNagativeFormat: false,
            availabilitySelector: ".js-Benefit-type-check-availability",
            haseFile: false,
            quickAddSelector: ".js-quick-add",
            quickAddModal: "#quickAddModal",
            lastCodeSelector: '#lastCode',
            onSaved: function (benefitTypeId) {

            },
            load: function () {

            }
        }, options);

        var gridUrl = settings.baseUrl + "/grid";
        var saveUrl = settings.baseUrl + "/setup";
        var deleteUrl = settings.baseUrl + "/Delete";
        var selectedItems = [];
        $(() => {

            loadTable();
            initialize();
            $("body").on("click", `${settings.editSelector},${settings.clearSelector}`, function (e) {
                e.stopPropagation();
                e.preventDefault();
                e.stopImmediatePropagation();
                let url = saveUrl + ($(this).data("id") ? "/" + $(this).data("id") : "");
                loadForm(url);
                //loadForm(url).then((data) => {
                //    console.info("Form Loaded Successfully", data);
                //}).catch((error) => {
                //    console.error("Failed to load form", error);
                //});

                // Setting id on the delete selector for delete
                var id = $(this).data('id');
                //debugger;
                
                if ($(this).is(settings.clearSelector)) {
                    $(settings.deleteSelector).removeData('id');
                } else {
                    var id = $(this).data('id');
                    $(settings.deleteSelector).data('id', id);
                }

                $("html, body").animate({ scrollTop: 0 }, 500);
            });

            $("body").off("click", settings.saveSelector).on("click", settings.saveSelector, function () {

                validation();

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
                        if (response.isSuccess) {
                            loadForm(saveUrl)
                                .then((data) => {

                                    loadTable();
                                    $(settings.lastCodeSelector).val(response.lastCode);
                                    // alert("The last code has been updated to: " + response.lastCode);
                                })
                                .catch((error) => {
                                    console.log(error)
                                })

                            toastr.success(response.message);

                            if (settings.isModal && typeof settings.onSaved === 'function') {
                                settings.onSaved(response.lastCode);
                            }
                        }
                        else {
                            toastr.error(response.message);
                            console.log(response);
                        }
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
                //if ($(this).data('tc')) {
                //    selectedItems.push($(this).data('tc'));

                //} else {
                    $('input:checkbox.checkBox').each(function () {
                        if ($(this).prop('checked')) {
                            if (!selectedItems.includes($(this).val())) {
                                selectedItems.push($(this).val());
                            }
                        }
                    });
                //}

                if (selectedItems.length > 0) {
                    $(settings.deleteModal).modal("show");
                } else {
                    toastr.info("Please select at least one item.");
                }
            });


            $("body").on('show.bs.modal', settings.deleteModal, function (event) {

                var source = $(event.relatedTarget);
                var id = source.data("ids");

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
                        url: deleteUrl,
                        method: "POST",
                        contentType: "application/json",
                        data: JSON.stringify(selectedItems),
                        success: function (response) {
                            console.log(response);
                            $(modal).modal("hide");
                            if (response.success) {
                                selectedItems = [];
                                console.log(selectedItems);
                                toastr.success(response.message);
                                
                                loadTable();
                                loadForm(saveUrl);
                            }
                            else {
                                toastr.error(response.message);
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

            $("body").on("keyup", settings.availabilitySelector, function () {
                var self = $(this);
                let code = $(".js-Benefit-Type-code").val();
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

        function loadTable() {
            $.get(settings.baseUrl + "/GetTableData")
                .done(html => {
                    $(settings.gridContainer).html(html);

                    if ($.fn.DataTable.isDataTable(settings.gridSelector)) {
                        $(settings.gridSelector).DataTable().destroy();
                    }
                    //$(settings.formContainer).find('#BenefitType').trigger('focus');
                    $(settings.gridSelector).DataTable({
                        lengthChange: true,
                        pageLength: 10,
                        lengthMenu: [
                            [10, 25, 50, -1],
                            [10, 25, 50, 'All'],
                        ],
                        order: [[1, "desc"]],
                        destroy: true, // Allow reinitialization
                        paging: true,
                        searching: true,
                        responsive: true,
                    });

                })
                .fail(() => toastr.error("Failed to load table data."));
        }

        function loadForm(url) {
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: url,
                    type: 'GET',
                    cache: false,
                    success: function (data) {
                        $(settings.formContainer).empty();
                        $(settings.formContainer).html(data);
                        //$(settings.formContainer).find('#BenefitType').trigger('focus');
                        $.validator.unobtrusive.parse($(settings.formSelector));


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
                $(settings.formSelector).find('#BenefitType').focus();
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
        function validation() {

            var degName = $('#BenefitType').val();
            if (!degName) {
                toastr.info('Enter Benefit types');
                $('#BenefitType').trigger('focus');
                return false;
            }

            return true;
        }
    }
}(jQuery));