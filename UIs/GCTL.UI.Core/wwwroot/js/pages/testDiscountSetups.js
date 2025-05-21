﻿(function ($) {
    $.testDiscountSetups = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: "/",
            formSelector: "#discountSetup-form",
            formContainer: ".js-discountSetup-form-container",
            gridSelector: "#discounts-grid",
            gridContainer: ".js-discountSetup-grid-container",
            editSelector: ".js-discountSetup-edit",
            saveSelector: ".js-discountSetup-save",
            selectAllSelector: "#discountSetup-check-all",
            deleteSelector: ".js-discountSetup-delete-confirm",
            deleteModal: "#discountSetup-delete-modal",
            finalDeleteSelector: ".js-discountSetup-delete",
            clearSelector: ".js-discountSetup-clear",
            topSelector: ".js-go",
            decimalSelector: ".js-discountSetup-decimalplaces",
            maxDecimalPlace: 5,
            showNagativeFormat: false,
            availabilitySelector: ".js-discountSetup-check-availability",
            haseFile: false,
            quickAddSelector: ".js-quick-add",
            quickAddModal: "#quickAddModal",
            load: function () {

            }
        }, options);


        var gridUrl = settings.baseUrl + "/grid";
        var saveUrl = settings.baseUrl + "/setup";
        var deleteUrl = settings.baseUrl + "/Delete";
        var selectedItems = [];
        $(() => {
            //  $(settings.saveSelector).attr("disabled", "disabled");
            //settings.load(settings.baseUrl, settings.gridSelector);
            loadDiscountSetups(settings.baseUrl, settings.gridSelector);
            initialize();
            setTimeout(function () {
                $(".js-error").fadeOut();
            }, 5000);
            // $(".DiscountReceiver").trigger("change");
            $("body").on("click", `${settings.editSelector}`, function (e) {
                e.stopPropagation();
                e.preventDefault();
                e.stopImmediatePropagation();

                let url = settings.baseUrl + $(this).data("url");
                // let url = saveUrl + "/" + $(this).data("id") ?? "";

                loadForm(url)
                    .then((data) => {
                        /* $(".DiscountReceiver").trigger("change");*/
                    })
                    .catch((error) => {
                        console.log(error)
                    })
                //  $(settings.saveSelector).removeAttr("disabled");
                $("html, body").animate({ scrollTop: 500 }, 500);
            });

            $("body").on("click", `${settings.clearSelector}`, function (e) {
                e.stopPropagation();
                e.preventDefault();
                e.stopImmediatePropagation();
                //  $(settings.saveSelector).attr("disabled", "disabled");
                loadDiscountSetups(settings.baseUrl, settings.gridSelector);
                loadForm(saveUrl);
                initialize();
            });

            // Save
            $("body").on("click", settings.saveSelector, function () {
                if ($("#discount-details-grid tbody tr").length == 0) {
                    toastr.info("Nothing to save", 'Info');
                    return false;
                }

                var $valid = $(settings.formSelector).valid();
                if (!$valid) {
                    return false;
                }

                $.LoadingOverlay("show");
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
                        $.LoadingOverlay("hide");
                        if (response.isSuccess) {
                            loadForm(saveUrl)
                                .then((data) => {
                                    if (response.secondaryMessage != undefined) {
                                        let warningTemplate = `<div class="alertx alert-warningx js-error"><p class="text-danger m-0"><strong><em>${response.secondaryMessage}</em></strong></p></div>`;
                                        $(".js-message").html(warningTemplate);

                                        setTimeout(function () {
                                            $(".js-error").fadeOut();
                                        }, 5000);
                                    }
                                    loadDiscountSetups(settings.baseUrl, settings.gridSelector);
                                })
                                .catch((error) => {
                                    console.log(error)
                                })

                            toastr.success(response.message, 'Success');
                        }
                        else {
                            if (response.secondaryMessage != undefined) {
                                let warningTemplate = `<div class="alertx alert-warningx js-error"><p class="text-danger m-0"><strong><em>${response.secondaryMessage}</em></strong></p></div>`;
                                $(".js-message").html(warningTemplate);

                                setTimeout(function () {
                                    $(".js-error").fadeOut();
                                }, 5000);
                            }
                            toastr.error(response.message, 'Error');
                            console.log(response);
                        }
                    }
                }
                if (settings.haseFile) {
                    options.processData = false;
                    options.contentType = false;
                }

                var preventForward = false;
                // Check existing entry
                $.ajax({
                    url: settings.baseUrl + "/CheckAvailability",
                    method: "POST",
                    data: { id: 0, name: "" },
                    success: function (response) {
                        if (response.isSuccess) {
                            let warningTemplate = `<div class="alert alert-warning js-error"><p class="m-0"><strong><em>${response.message}</em></strong></p></div>`;
                            $(".js-message").html(warningTemplate);
                            preventForward = response.isSuccess;
                            alert(preventForward);
                            return false;
                        } else {

                            if (preventForward)
                                return false;
                            else
                                $.ajax(options);
                        }
                    }
                });


                //debugger;
                //if (preventForward)
                //    return false;
                //else
                //    $.ajax(options);
            });

            $("body").on("click", settings.selectAllSelector, function () {
                $(".checkBox").prop('checked',
                    $(this).prop('checked'));
            });


            $("body").on("click", settings.deleteSelector, function (e) {
                e.preventDefault();
                $('input:checkbox.checkBox').each(function () {
                    if ($(this).prop('checked')) {
                        if (!selectedItems.includes($(this).val())) {
                            selectedItems.push($(this).val());
                        }
                    }
                });

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
                        url: deleteUrl + "/" + selectedItems,
                        method: "POST",
                        success: function (response) {
                            console.log(response);
                            $(modal).modal("hide");
                            if (response.success) {
                                toastr.success(response.message, 'Success');
                                selectedItems = [];
                                /*settings.load(settings.baseUrl);*/
                                loadDiscountSetups(settings.baseUrl, settings.gridSelector);
                                loadForm(saveUrl);
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
                $("html, body").animate({ scrollTop: 500 }, 500);
            });

            $("body").on('change', '.DiscountReceiver', function () {
                $(".receiveroptions").removeClass("d-none")
                    .addClass("d-none");

                var target = $(this).data("target");
                $(target).addClass("d-none")
                    .removeClass("d-none");
            });

            $("body").on('click', '.select-item', function () {
                $('input[class="select-item"]').not(this).prop('checked', false);
                if ($(this).is(":checked")) {
                    console.log($(this).val());

                    let url = settings.baseUrl + $(this).data("url");
                    loadForm(url);

                    $(settings.saveSelector).removeAttr("disabled");
                    $("html, body").animate({ scrollTop: 500 }, 500);
                }
            });

            $("body").on("click", "input:checkbox.checkBox", function (e) {
                if ($(this).prop('checked')) {
                    if (!selectedItems.includes($(this).val())) {
                        selectedItems.push($(this).val());
                    }
                } else {
                    selectedItems.splice($.inArray($(this).val(), selectedItems), 1);;
                }
            });

            $("body").on("click", "#discount-details-grid .Continuing", function () {
                if ($(this).is(":checked")) {
                    $(this).closest("tr").find(".EffectiveDateTo").val("");
                    $("#Discount_EffectiveDateToError").removeClass("d-none")
                        .addClass("d-none");
                }
            });

            $("body").on("click", "#discount-details-grid .js-add", function () {
                $(".js-message").html("");
                let discountSerial = $("#Discount_SerialNo").val(),
                    discountSetupDetailsId = $("#Discount_DiscountSetupDetailsId").val(),
                    discountSetupId = $("#Discount_DiscountSetupId").val(),
                    testCategoryId = $("#Discount_TestCategoryId").val(),
                    testId = $("#Discount_TestChargeId").val(),
                    discount = $("#Discount_DiscountPercent").val(),
                    amount = $("#Discount_DiscountAmount").val(),
                    effectiveDateFrom = $("#Discount_EffectiveDateFrom").val(),
                    effectiveDateTo = $("#Discount_EffectiveDateTo").val(),
                    continuing = $("#Discount_Continuing").is(":checked"),
                    counter = $("#discount-details-grid tbody tr").length;


                if ($("#Discount_TestCategoryId").val() == '') {
                    $("#Discount_TestCategoryIdError").addClass("d-none")
                        .removeClass("d-none");
                    $("#Discount_TestCategoryId").focus();
                    return;
                }
                else {
                    $("#Discount_TestCategoryIdError").removeClass("d-none")
                        .addClass("d-none");
                }
                if ($("#Discount_TestChargeId").val() == '') {
                    $("#Discount_TestChargeIdError").addClass("d-none")
                        .removeClass("d-none");
                    $("#Discount_TestChargeId").focus();
                    return;
                }
                else {
                    $("#Discount_TestChargeIdError").removeClass("d-none")
                        .addClass("d-none");
                }

                if (discount == 0 && amount == 0) {
                    $("#Discount_DiscountPercent").focus();
                    return;
                }

                if ($("#Discount_EffectiveDateFrom").val() == '') {
                    $("#Discount_EffectiveDateFromError").addClass("d-none")
                        .removeClass("d-none");
                    $("#Discount_EffectiveDateFrom").focus();
                    return;
                }
                else {
                    $("#Discount_EffectiveDateFromError").removeClass("d-none")
                        .addClass("d-none");
                }

                if (!continuing) {
                    if ($("#Discount_EffectiveDateTo").val() == '') {
                        $("#Discount_EffectiveDateToError").addClass("d-none")
                            .removeClass("d-none");
                        $("#Discount_EffectiveDateTo").focus();
                        return;
                    }
                    else {
                        $("#Discount_EffectiveDateToError").removeClass("d-none")
                            .addClass("d-none");
                    }
                } else {
                    $("#Discount_EffectiveDateToError").removeClass("d-none")
                        .addClass("d-none");
                }

                var categories = "";
                $("#Discount_TestCategoryId > option").each(function () {
                    var item = $(this).html();
                    var value = $(this).val();

                    if (value == testCategoryId) {
                        categories += `<option value='${value}' selected='selected'>${item}</option>`;
                    } else {
                        categories += `<option value='${value}'>${item}</option>`;
                    }
                })

                var tests = "";
                $("#Discount_TestChargeId > option").each(function () {
                    var item = $(this).html();
                    var value = $(this).val();

                    if (testId.includes(value)) {
                        tests += `<option value='${value}' selected='selected'>${item}</option>`;
                    } else {
                        tests += `<option value='${value}'>${item}</option>`;
                    }
                })


                $.ajax({
                    url: settings.baseUrl + "/IsDiscountSetupExist",
                    method: "POST",
                    data: {
                        discountSetupDetailsId: discountSetupDetailsId,
                        categoryId: testCategoryId,
                        testChargeId: testId,
                        effectiveDateFrom: effectiveDateFrom,
                        effectiveDateTo: effectiveDateTo,
                        continuing: continuing
                    },
                    success: function (response) {
                        if (response.isSuccess) {
                            let warningTemplate = `<div class="alert alert-danger js-error"><p class="m-0"><strong><em>${response.message}</em></strong></p></div>`;
                            $(".js-message").html(warningTemplate);
                            return false;
                        } else {
                            let item =
                                `<tr id="discounts-${counter}">
                                <td>
                                    <select id="Discounts_${counter}__TestCategoryId" name="Discounts[${counter}].TestCategoryId" class="form-control form-control-sm selectpicker js-test-category TestCategoryId" data-target="#Discounts_${counter}__TestChargeId">
                                        ${categories}
                                    </select>
                                    <input type="hidden" id="Discounts_${counter}__DiscountSetupId" name="Discounts[${counter}].DiscountSetupId" value="${discountSetupId}" class="DiscountSetupId" />
                                    <input type="hidden" id="Discounts_${counter}__SerialNo" name="Discounts[${counter}].SerialNo" value="${counter}" class="SerialNo" />
                                </td>
                                <td>
                                    <select  id="Discounts_${counter}__TestChargeId" name="Discounts[${counter}].TestChargeId" class="form-control form-control-sm multiselect TestChargeId" multiple="multiple">
                                        ${tests}
                                    </select>                          
                                </td>
                                <td>
                                    <input id="Discounts_${counter}__DiscountPercent" name="Discounts[${counter}].DiscountPercent" value="${discount}" class="form-control form-control-sm text-right DiscountPercent" />
                                </td>
                                <td>
                                    <input id="Discounts_${counter}__DiscountAmount" name="Discounts[${counter}].DiscountAmount" value="${amount}" class="form-control form-control-sm text-right DiscountAmount" />
                                </td>
                                <td class="relative">
                                    <input id="Discounts_${counter}__EffectiveDateFrom" name="Discounts[${counter}].EffectiveDateFrom" value="${effectiveDateFrom}" class="form-control form-control-sm datepicker  EffectiveDateFrom" />
                                </td>
                                <td class="relative">
                                    <input id="Discounts_${counter}__EffectiveDateTo" name="Discounts[${counter}].EffectiveDateTo" value="${effectiveDateTo}" class="form-control form-control-sm datepicker  EffectiveDateTo" />
                                </td>
                                <td class="text-center text-middle">
                                    <input type="checkbox" id="Discounts_${counter}__Continuing" name="Discounts[${counter}].Continuing" value="true" class="Continuing" >
                                </td>
                               <td>
                                    <button type='button' class='btn btn-sm btn-danger js-remove' title="Remove diagnosis"><i class='fa fa-trash'></i></button>
                                </td>
                            </tr>`;

                            $("#discount-details-grid tbody").append(item);
                            if (continuing)
                                $(`#Discounts_${counter}__Continuing`).prop("checked", true);

                            counter++;

                            $("#Discount_TestCategoryId").val("");
                            $("#Discount_TestChargeId").val("");
                            refreshControl();
                            $("#Discount_DiscountPercent").val("");
                            $("#Discount_DiscountAmount").val("");
                            $("#Discount_EffectiveDateFrom").val("");
                            $("#Discount_EffectiveDateTo").val("");
                            $("#Discount_Continuing").prop("checked", false);
                            $("#Discount_SerialNo").val("");
                            $("#discount-details-grid .js-add").html('<i class="fas fa-plus"></i>');
                        }
                    }
                });
            });


            $("body").on("click", "#discount-details-grid .js-edit", function (e) {
                e.preventDefault();
                $("#Discount_DiscountSetupId").val($(this).closest("tr").find(".DiscountSetupId").val());
                $("#Discount_SerialNo").val($(this).closest("tr").find(".SerialNo").val());
                $("#Discount_TestCategoryId").val($(this).closest("tr").find(".TestCategoryId").val());
                $("#Discount_TestChargeId").val($(this).closest("tr").find(".TestChargeid").val());
                $("#Discount_DiscountPercent").val($(this).closest("tr").find(".DiscountPercent").val());
                $("#Discount_DiscountAmount").val($(this).closest("tr").find(".DiscountAmount").val());
                $("#Discount_EffectiveDateFrom").val($(this).closest("tr").find(".EffectiveDateFrom").val());
                $("#Discount_EffectiveDateTo").val($(this).closest("tr").find(".EffectiveDateTo").val());
                $("#Discount_Continuing").val($(this).closest("tr").find(".Continuing").val());

                $("#discount-details-grid .js-add").html('<i class="fas fa-pencil-alt"></i>');
                refreshControl();
            })


            $("body").on("click", "#discount-details-grid .js-remove", function (e) {
                e.preventDefault();
                remove($(this));

                fixIndexing();
            })


            function fixIndexing() {
                $("#discount-details-grid tbody tr").each(function (index) {
                    var html = $(this).html();
                    html = html.replace(/\_(.*?)__/g, '_' + index + '__');
                    html = html.replace(/\[(.*?)\]/g, '[' + index + ']');
                    $(this).html(html);
                })
            }

            let loadUrl,
                target,
                reloadUrl,
                title,
                lastCode;
            // Quick add
            $("body").on("click", settings.quickAddSelector, function (e) {
                e.stopPropagation();
                e.preventDefault();
                e.stopImmediatePropagation();

                loadUrl = $(this).data("url");
                target = $(this).data("target");
                reloadUrl = $(this).data("reload-url");
                title = $(this).data("title");

                $(settings.quickAddModal + " .modal-title").html(title);
                $(settings.quickAddModal + " .modal-body").empty();


                $(settings.quickAddModal + " .modal-body").load(loadUrl, function (response) {
                    $(settings.quickAddModal).modal("show");

                    // $.sidebarMenu($('.sidebar-menu'));

                    $("#header").hide();
                    $(settings.quickAddModal + " .modal-body #header").hide();

                    $("#left_menu").hide();
                    $(settings.quickAddModal + " .modal-body #left_menu").hide();

                    $("#main-content").toggleClass("collapse-main");
                    $(settings.quickAddModal + " .modal-body #main-content").toggleClass("collapse-main");

                    /*  $("body").removeClass("sidebar-mini");*/
                })
            });

            $("body").on("click", ".js-modal-dismiss", function () {
                /*       $("body").removeClass("sidebar-mini").addClass("sidebar-mini");*/

                $("#header").show();
                $(settings.quickAddModal + " .modal-body #header").show();

                $("#left_menu").show();
                $(settings.quickAddModal + " .modal-body #left_menu").show();

                $("#main-content").toggleClass("collapse-main");
                $(settings.quickAddModal + " .modal-body #main-content").toggleClass("collapse-main");

                // $.sidebarMenu($('.sidebar-menu'));

                lastCode = $(settings.quickAddModal + " #lastCode").val();


                $(settings.quickAddModal + " .modal-body").empty();
                $(settings.quickAddModal).modal("hide");


                $(target).empty("");
                $(target).append($('<option>', {
                    value: '',
                    text: `---Select ${title}---`
                }));
                $.ajax({
                    url: reloadUrl,
                    method: "GET",
                    success: function (response) {
                        $.each(response, function (i, item) {
                            $(target).append($('<option>', {
                                value: item.code,
                                text: item.name
                            }));
                        });
                        $(target).val(lastCode);
                    }
                });
            });

            $("body").on("change", "#DepartmentId", function () {
                var self = $(this);
                if (self.val().length > 0) {
                    $.ajax({
                        url: normalizeUrl(getBaseUrl()) + "/Cascading/GetDoctorsByDepartment",
                        method: "POST",
                        data: { departmentCode: self.val() },
                        success: function (response) {
                            $("#DoctorId").empty();
                            //$("#DoctorId").append($('<option>', {
                            //    value: '',
                            //    text: `---Doctor---`
                            //}));
                            $.each(response, function (i, item) {
                                $("#DoctorId").append($('<option>', {
                                    value: item.code,
                                    text: item.name
                                }));
                            });

                            refreshControl();
                        }
                    });
                }
            });


            $("body").on("change", ".js-test-category", function () {
                var self = $(this);

                if (self.val().length > 0) {
                    getTests(self.val(), self.data("target"));
                }
            });

            $("body").on("keyup", "#VisitingFeeDiscount", function () {
                calculateDiscountAmount("Visiting");
            });

            $("body").on("keyup", "#ReportShowingFeeDiscount", function () {
                calculateDiscountAmount("ReportShowing");
            });

            $("body").on("keyup", "#VisitingFeeDiscountAmount", function () {
                $("#VisitingFeeDiscount").val(0);
                calculateDiscount("Visiting");
            });

            $("body").on("keyup", "#ReportShowingFeeDiscountAmount", function () {
                $("#ReportShowingFeeDiscount").val(0);
                calculateDiscount("ReportShowing");
            });

            $("body").on("keyup", settings.availabilitySelector, function () {
                var self = $(this);
                let code = $(".js-code").val();
                let id = $("#PatientId").val();
                let name = self.val();

                // check
                $.ajax({
                    url: settings.baseUrl + "/CheckAvailability",
                    method: "POST",
                    data: { id: id, name: name },
                    success: function (response) {
                        console.log(response);
                        if (response.isSuccess) {
                            toastr.error(response.message);
                        }
                    }
                });
            });

            $("body").on("click", ".js-file-chooser", function (e) {
                e.preventDefault();
                var target = $(this).data("target");
                $(target).trigger("click");
            })

            $("body").on("change", ".js-file", function (e) {
                e.preventDefault();
                var target = $(this).data("target");
                showImagePreview($(this), target);
            })

            $("body").on("click", ".js-clear-file", function (e) {
                e.preventDefault();
                var file = $(this).data("file");
                var tag = $(this).data("tag");
                clearImage(file, tag);
            })

            $("body").on("click", ".js-commisssionSetup-export", function () {
                if (selectedItems.length > 0) {
                    var self = $(this);
                    let reportRenderType = self.data("rendertype");

                    $(selectedItems).each(function (index, item) {
                        window.open(
                            settings.baseUrl + `/Export?commisssionSetupCode=${item}&reportType=Prescription&reportRenderType=${reportRenderType}`,
                            "_blank"
                        )
                    })
                } else {
                    toastr.info("Please select at least one item.", 'Warning');
                }
            });
        });

        function remove(selector) {
            $(selector).closest('tr').remove();
        }

        function showImagePreview(input, target) {
            //var target = $(input).data("target");
            if (input[0].files && input[0].files[0]) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    $(target).prop('src', e.target.result);
                };
                reader.readAsDataURL(input[0].files[0]);
            }
        }

        function clearImage(file, tag) {
            console.log(file);
            console.log(tag);
            $(file).removeAttr("src");
            $(tag).val(true);
        }

        function getTests(testCategoryCode, target) {
            $.ajax({
                url: normalizeUrl(getBaseUrl()) + "/Cascading/GetTests",
                method: "POST",
                data: { testCategoryCode: testCategoryCode },
                success: function (response) {
                    $(target).empty();
                    //$("target).append($('<option>', {
                    //    value: '',
                    //    text: `Select Test`
                    //}));
                    $.each(response, function (i, item) {
                        $(target).append($('<option>', {
                            value: item.code,
                            text: item.name
                        }));
                    });

                    refreshControl();
                }
            });
        }

        function calculateDiscountAmount(selectorPrefix) {
            let feeSelector = $("#" + selectorPrefix + "Fee"),
                discountSelector = $("#" + selectorPrefix + "FeeDiscount"),
                discountAmountSelector = $("#" + selectorPrefix + "FeeDiscountAmount"),
                fee = parseFloat(feeSelector.val() || 0),
                discount = parseFloat(discountSelector.val() || 0),
                discountAmount = parseFloat(discountAmountSelector.val() || 0);

            if (fee > 0) {
                discountAmount = (fee * discount) / 100;
            }

            discountAmountSelector.val(discountAmount);
            calculateDiscount(selectorPrefix);
        }

        function calculateDiscount(selectorPrefix) {
            let feeSelector = $("#" + selectorPrefix + "Fee"),
                discountAmountSelector = $("#" + selectorPrefix + "FeeDiscountAmount"),
                amountSelector = $("#" + selectorPrefix + "Amount"),
                fee = parseFloat(feeSelector.val() || 0),
                discountAmount = parseFloat(discountAmountSelector.val() || 0),
                amount = parseFloat(amountSelector.val() || 0);

            if (fee > 0) {
                amount = fee - discountAmount;
            }

            amountSelector.val(amount);
        }

        function loadDiscountSetups(baseUrl, gridSelector) {
            var data = {
                fromDate: $(".datefrom").val(),
                toDate: $(".dateto").val(),
                doctorCode: $(".doctor").val(),
                patientCategoryCode: $(".patientcategory").val()
            };

            var dataTable = $(gridSelector).DataTable({
                ajax: {
                    url: baseUrl + "/Grid",
                    type: "GET",
                    datatype: "json",
                    // data: data
                },

                columnDefs: [
                    { targets: [0], orderable: false }
                ],
                columns: [
                    {
                        "data": "discountSetupId", "className": "text-center", width: "30px",
                        render: function (data) {
                            return `<input type="checkbox" class="checkBox" value='${data}' />`;
                        }
                    },
                    {
                        "data": "discountSetupId", "className": "text-center", width: "40px"
                    },
                    { "data": "discountSetupDateTime", "className": "text-center", width: "120px" },
                    { "data": "testCategory", "className": "text-left", width: "80px" },
                    { "data": "testName", "className": "text-left", width: "250px" },
                    { "data": "entryUser", "className": "text-center", width: "50px" },
                    {
                        "data": "discountSetupId", "render": function (data, type, row) {
                            return `<div class='p-1 action-buttons'>
                                        <a class='btn btn-warning btn-circle btn-sm js-discountSetup-edit' data-url="/setup/${data}" title="Edit ${row.discountSetupId}" data-id='${data}'><i class='fas fa-pencil-alt'></i></a> 
                                        <button type="button" class="btn btn-danger btn-circle btn-sm js-discountSetup-delete-confirm"
                                                data-target="#deleteModalx"
                                                data-id="${data}"
                                                title="Delete ${row.discountSetupId}"
                                                data-title="Are you sure want to delete ${row.discountSetupId}?">
                                                    <i class="fas fa-trash fa-sm"></i>
                                        </button>`;
                        },
                        "orderable": false,
                        "searchable": false,
                        width: "30px"
                    }
                ],
                lengthChange: true,
                pageLength: 10,
                order: [],
                sScrollY: "100%",
                scrollX: true,
                sScrollX: "100%",
                bDestroy: true
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

                        initialize();
                        resolve(data)
                    },
                    error: function (error) {
                        reject(error)
                    },
                })
            })
        }

        function execute(url, data = {}, type = "POST") {
            return new Promise((resolve, reject) => {
                $.ajax({
                    url: url,
                    type: type,
                    data: data,
                    success: function (data) {
                        resolve(data)
                    },
                    error: function (error) {
                        reject(error)
                    },
                })
            })
        }

        function initialize(selectedText = '', selectedValue = '') {
            $('.selectpicker').select2({
                language: {
                    noResults: function () {
                        //return 'Not found <a class="add_new_item" href="javascript:void(0)">Add New</a>';
                    }
                },
                escapeMarkup: function (markup) {
                    return markup;
                }
            });

            $('.multiselect').multiselect({
                includeSelectAllOption: true,
                enableCaseInsensitiveFiltering: true,
                buttonContainer: '<div class="btn-group w-100" />',
                onSelectAll: function (options) {
                    // alert('onSelectAll triggered, ' + options.length + ' options selected!');
                }
            });

            $('.datepicker').datetimepicker({
                format: 'DD/MM/YYYY',
                /*showTodayButton: true,*/
                // Your Icons
                // as Bootstrap 4 is not using Glyphicons anymore
                icons: {
                    time: 'fas fa-clock',
                    date: 'fas fa-calendar',
                    up: 'fas fa-chevron-up',
                    down: 'fas fa-chevron-down',
                    previous: 'fas fa-chevron-left',
                    next: 'fas fa-chevron-right',
                    today: 'fas fa-check',
                    clear: 'fas fa-trash',
                    close: 'fas fa-times'
                }
            });

            $('.timepicker').datetimepicker({
                format: 'hh:mm A',
                /*showTodayButton: true,*/
                // Your Icons
                // as Bootstrap 4 is not using Glyphicons anymore
                icons: {
                    time: 'fas fa-clock',
                    date: 'fas fa-calendar',
                    up: 'fas fa-chevron-up',
                    down: 'fas fa-chevron-down',
                    previous: 'fas fa-chevron-left',
                    next: 'fas fa-chevron-right',
                    today: 'fas fa-check',
                    clear: 'fas fa-trash',
                    close: 'fas fa-times'
                }
            });

            $('.datetimepicker').datetimepicker({
                format: 'DD/MM/YYYY hh:mm A',
                /*showTodayButton: true,*/
                // Your Icons
                // as Bootstrap 4 is not using Glyphicons anymore
                icons: {
                    time: 'fas fa-clock',
                    date: 'fas fa-calendar',
                    up: 'fas fa-chevron-up',
                    down: 'fas fa-chevron-down',
                    previous: 'fas fa-chevron-left',
                    next: 'fas fa-chevron-right',
                    today: 'fas fa-check',
                    clear: 'fas fa-trash',
                    close: 'fas fa-times'
                }
            });
        }
        function refreshControl() {
            $('.multiselect').multiselect('destroy');
            initialize();
        }
    }
}(jQuery));

