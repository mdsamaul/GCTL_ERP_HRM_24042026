(function ($) {
    $.RMGBookingOrderEntryBukl = function (options) {
        var settings = $.extend({ baseUrl: '', load: null }, options);
        var table = null;
        var currentBookingType = '';
        window.selectedIds = window.selectedIds || new Set();
        window.selectedBookingOrderId = window.selectedBookingOrderId || new Set();
        var purchaseTable;

        function init() {
            injectTooltipStyles();
            purchaseOrderLoad();
            RMG_BookingOrderAutoId();
            bindEvents();
            setDate();
            BookingOrderGrid();
            if (typeof settings.load === 'function') settings.load();
        }

        function RMG_BookingOrderAutoId() {
            $.ajax({
                url: settings.baseUrl + "/BookingOrderAutoId",
                type: "GET",
                success: function (res) {
                    console.log("Generated Auto ID:", res);
                    $("#BookingOrderEntryBuklSetup_BookinOrderNo").val(res.data);
                },
                error: function (e) {
                    console.error("Error fetching Auto ID:", e);
                    toastr.error("Failed to generate Booking Order Auto ID");
                }
            });
        }

        function setDate() {
            flatpickr(".flatDatePicker", {
                dateFormat: "d/m/Y",
                altInput: true,
                altFormat: "d/m/Y",
                allowInput: true,
                defaultDate: "today",
            });
        }

        //$(document).on('change', "#BookingOrderEntryBuklSetup_SupplierId", function () {
        //    var id = $(this).val();
        //    console.log("Selected SupplierId:", id);
        //    $("#SelectedSupplierHidden").val(id);

        //    $.ajax({
        //        url: settings.baseUrl + "/GetSupplierDetails",
        //        type: "GET",
        //        data: { supplierId: id },
        //        success: function (res) {
        //            console.log("Supplier details:", res);
        //            $("#SupplierAddress").val(res.address);
        //            $("#SupplierCountry").val(res.countryId).select2('change');

        //        },
        //        error: function (e) {
        //            console.error("Error loading supplier details:", e);
        //        }
        //    });
        //});


        $(document).on('change', "#BookingOrderEntryBuklSetup_SupplierId", function () {
            var id = $(this).val();

            // 🔴 SupplierId validation
            if (!id) {
                console.warn("SupplierId not selected");

                $("#SelectedSupplierHidden").val("");
                $("#SupplierAddress").val("");

                if ($("#SupplierCountry").data('select2')) {
                    $("#SupplierCountry")
                        .val(null)
                        .prop("disabled", false)
                        .trigger('change');
                }

                return; // stop execution
            }

            console.log("Selected SupplierId:", id);
            $("#SelectedSupplierHidden").val(id);

            $.ajax({
                url: settings.baseUrl + "/GetSupplierDetails",
                type: "GET",
                data: { supplierId: id },
                success: function (res) {

                    if (!res || res.success !== true) {
                        console.warn("Invalid response:", res);
                        return;
                    }

                    console.log("Supplier details:", res);

                    // Address safe set
                    $("#SupplierAddress").val(res.address ?? "");                    
                    // CountryId validation
                    
                        // 🔴 Checks if Select2 is initialized
                        if ($("#SupplierCountry").data('select2')) {
                            $("#SupplierCountry")
                                // 1. Sets the value
                                .val(res.countryId)
                                // 2. Disables the element
                                .prop("disabled", true)
                                // 3. Triggers change for Select2 to update its display
                                .trigger('change');
                        } else {
                            // fallback (non-select2)
                            $("#SupplierCountry")
                                .val(res.countryId)
                                .prop("disabled", true);
                        }
                    
                },
                error: function (e) {
                    console.error("Error loading supplier details:", e);
                }
            });
        });


        function bindEvents() {
            $('#BookingOrderEntryBuklSetup_BookingType').on('change', handleBookingTypeChange);
            $(document).on('click', '.btn-delete-row', deleteRow);
        }

        function destroyTable() {
            if (table) {
                disposeAllTooltips();
                table.destroy();
                table = null;
            }
            $('#bookingTable').empty();
        }

        function applySelect2() {
            $('#bookingTable select').each(function () {
                if (!$(this).hasClass("select2-hidden-accessible")) {
                    $(this).select2({
                        width: '100%',
                        dropdownAutoWidth: true,
                        theme: "bootstrap-5",
                        placeholder: "Select",
                        allowClear: true
                    });
                }
            });
        }

        $('.select2').each(function () {
            if (!$(this).hasClass("select2-hidden-accessible")) {
                $(this).select2({
                    width: '100%',
                    dropdownAutoWidth: true,
                    theme: "bootstrap-5",
                    placeholder: "Select",
                    allowClear: true
                });
            }
        });
       
        function purchaseOrderLoad() {
            if ($.fn.DataTable.isDataTable('#purchaseOrderTable')) {
                purchaseTable.clear().destroy();
            }
            var purchaseTable = $('#purchaseOrderTable').DataTable({
                "processing": true,
                "serverSide": true,
                "ajax": {
                    "url": settings.baseUrl + "/GetPurchaseOrders",
                    "type": "POST",
                    dataSrc: function (data) {
                        console.log("Server Response:", data);
                        return data.data;
                    },
                },
                "pageLength": 3,
                "lengthMenu": [[3, 5, 10, -1], [3, 5, 10, "All"]],
                "scrollY": "221px",
                "scrollCollapse": true,
                "paging": true,
                "columns": [
                    {
                        "data": "costingId",
                        render: function (data) {
                            let checked = selectedIds.has(data) ? "checked" : "";
                            return `<input type="checkbox" class="row-check" data-id="${data}" ${checked} />`;
                        }
                    },
                    { "data": "poNo" },
                    { "data": "orderQty" },
                    { "data": "styleName" },
                    { "data": "masterPo" },
                    { "data": "funJobNo" },
                    { "data": "buyerName" }
                ]
            });


            



            //$('#purchaseOrderTable tbody').on('click', '.row-check', function () {
            //    debugger
            //    let costingIds = getSelectedCostingIds(); // returns array

            //    if (costingIds.length === 0) {
            //        toastr.warning("Please select at least one row");
            //        return;
            //    }

            //    console.log("Sending Costing IDs:", costingIds);

            //    $.ajax({
            //        url: settings.baseUrl + '/GetItemTypes',
            //        type: 'POST',
            //        contentType: 'application/json',
            //        data: JSON.stringify(costingIds),
            //        success: function (data) {
            //            console.log("Item Types:", data);

            //            var select = $('#BookingOrderEntryBuklSetup_BookingType');
            //            select.empty();
            //            select.append('<option value="" disabled selected>--Select Booking Type--</option>');

            //            $.each(data, function (i, item) {
            //                select.append(
            //                    `<option value="${item.bookingItemTypeID}">
            //            ${item.bookingItemType}
            //         </option>`
            //                );
            //            });
            //        },
            //        error: function () {
            //            toastr.error("Failed to load booking item types");
            //        }
            //    });
            //});



        }


        $(document).on("change", "#purchaseOrderTable .row-check", function () {
            let id = $(this).attr("data-id");

            if (!id) return;

            if (this.checked) {
                selectedIds.add(id);
            } else {
                selectedIds.delete(id);
                $("#selectAll").prop("checked", false);
            }

            console.log("Selected IDs:", [...selectedIds]);

            // 🔥 এখন এখানেই AJAX কল করো
            let costingIds = getSelectedCostingIds();

            if (costingIds.length === 0) return;

            $.ajax({
                url: settings.baseUrl + '/GetItemTypes',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(costingIds),
                success: function (data) {
                    console.log("Item Types:", data);

                    let select = $('#BookingOrderEntryBuklSetup_BookingType');
                    select.empty();
                    select.append('<option value="" disabled selected>--Select Booking Type--</option>');

                    $.each(data, function (i, item) {
                        select.append(
                            `<option value="${item.bookingItemTypeID}">
                        ${item.bookingItemType}
                     </option>`
                        );
                    });
                },
                error: function () {
                    toastr.error("Failed to load booking item types");
                }
            });
        });

        //function buildHeaders(type) {
        //    let html = '<tr>';
        //    html += '<th class="border-end" style="min-width:70px">PO No.</th>';
        //    html += '<th class="border-end" style="min-width:90px">Item</th>';
        //    html += '<th class="border-end" style="min-width:100px">Description</th>';
        //    html += '<th class="border-end" style="min-width:80px">Color</th>';

        //    if (type === '04') {
        //        html += '<th class="border-end " style="min-width:80px">Size</th>';
        //        html += '<th class="border-end " style="min-width:80px">Length</th>';
        //        html += '<th class="border-end " style="min-width:80px">Unit</th>';
        //        html += '<th class="border-end " style="min-width:80px">Width</th>';
        //        html += '<th class="border-end " style="min-width:80px">Unit</th>';
        //        html += '<th class="border-end " style="min-width:80px">Height</th>';
        //        html += '<th class="border-end " style="min-width:80px">Unit</th>';
        //    } else if (type === '07') {
        //        html += `
        //        <th class="border-end " style="min-width:90px">
        //            <div class="d-flex justify-content-between align-items-center">
        //                <i class="fa-solid fa-plus text-dark" style="cursor:pointer;"></i>
        //                <span>Thread Count</span>
        //            </div>
        //        </th>
        //        `;
        //    } else if (type === '03') {
        //        html += '<th class="border-end " style="min-width:80px">Length</th>';
        //        html += '<th class="border-end " style="min-width:80px">Unit</th>';
        //        html += '<th class="border-end " style="min-width:80px">Width</th>';
        //        html += '<th class="border-end " style="min-width:80px">Unit</th>';
        //        html += '<th class="border-end " style="min-width:80px">Flap</th>';
        //        html += '<th class="border-end " style="min-width:80px">Unit</th>';
        //        html += '<th class="border-end " style="min-width:80px">Guest</th>';
        //        html += '<th class="border-end " style="min-width:80px">Unit</th>';
        //    }

        //    html += '<th class="border-end text-center" style="min-width:50px">Gar. Qty</th>';
        //    html += '<th class="border-end text-center" style="min-width:50px">Unit</th>';
        //    html += '<th class="border-end text-center" style="min-width:50px">Cons/mtr</th>';
        //    html += '<th class="border-end text-center" style="min-width:60px">Unit</th>';
        //    html += '<th class="border-end text-center" style="min-width:80px">Total Qty</th>';
        //    html += '<th class="border-end text-center" style="min-width:60px">Unit</th>';
        //    html += '<th class="border-end text-center" style="min-width:50px">Order Qty</th>';
        //    html += '<th class="border-end text-center" style="min-width:60px">Unit</th>';
        //    html += '<th class="border-end text-center" style="min-width:40px">Per (%)</th>';
        //    html += '<th class="border-end text-center" style="min-width:60px">Unit Price</th>';
        //    html += '<th class="border-end text-center" style="min-width:70px">Total Price</th>';
        //    html += '<th class="border-end text-center" style="min-width:50px">Curr.</th>';
        //    html += '<th class="border-end text-center" style="min-width:80px">Remarks</th>';
        //    html += '<th class="text-center" style="min-width:40px">Action</th>';
        //    html += '</tr>';
        //    return html;
        //}

        //function buildRow(item, type, dd) {
        //    //debugger
        //    console.log(item)
        //    let row = '<tr>';
        //    row += `<td class="border-end"><input type="text" class="form-control form-control-sm" value="${type || ''}"></td>`;
        //    row += `<td class="border-end"><input type="text" class="form-control form-control-sm" value="${item.integraJobNO || ''}"></td>`;
        //    row += `<td class="border-end"><input type="text" class="form-control form-control-sm" value="${item.poNo || ''}"></td>`;
        //    row += `<td class="border-end"><select class="form-select form-select-sm"><option value="">Select</option>${dd.items.map(i => `<option value="${i.id}" ${i.id === item.itemID ? 'selected' : ''}>${i.name}</option>`).join('')}</select></td>`;

        //    const description = (item.description || '').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
        //    row += `
        //    <td class="border-end">
        //       <input type="text"
        //           class="form-control form-control-sm description-input"
        //           data-bs-toggle="tooltip"
        //           data-bs-placement="top"
        //           data-bs-custom-class="custom-tooltip"
        //           data-bs-html="true"
        //           title="${description}"
        //           value="${description.replace(/&quot;/g, '"').replace(/&#39;/g, "'")}">
        //    </td>`;

        //    row += `<td class="border-end"><select class="form-select form-select-sm"><option value="">Select</option>${dd.colors.map(c => `<option value="${c.id}" ${c.id === item.colorID ? 'selected' : ''}>${c.name}</option>`).join('')}</select></td>`;

        //    if (type === '04') {
        //        row += `<td class="border-end "><select class="form-select form-select-sm"><option>Select</option>${dd.sizes.map(s => `<option value="${s.id}" ${s.id === item.sizeID ? 'selected' : ''}>${s.name}</option>`).join('')}</select></td>`;
        //        row += `<td class="border-end "><input type="number" class="form-control form-control-sm" value="${item.cartonLength || ''}"></td>`;
        //        row += `<td class="border-end "><select class="form-select form-select-sm">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.leangthUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
        //        row += `<td class="border-end "><input type="number" class="form-control form-control-sm" value="${item.cartonWidth || ''}"></td>`;
        //        row += `<td class="border-end "><select class="form-select form-select-sm">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.widthUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
        //        row += `<td class="border-end "><input type="number" class="form-control form-control-sm" value="${item.catonHeight || ''}"></td>`;
        //        row += `<td class="border-end "><select class="form-select form-select-sm">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.heightUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
        //    } else if (type === '07') {
        //        row += `<td class="border-end "><select class="form-select form-select-sm"><option value="">Select</option>${dd.threadCounts.map(t => `<option value="${t.id}" ${t.id === item.threadCountID ? 'selected' : ''}>${t.name}</option>`).join('')}</select></td>`;
        //    } else if (type === '03') {
        //        row += `<td class="border-end "><input type="number" class="form-control form-control-sm" value="${item.length || ''}"></td>`;
        //        row += `<td class="border-end "><select class="form-select form-select-sm">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.lengthUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
        //        row += `<td class="border-end "><input type="number" class="form-control form-control-sm" value="${item.width || ''}"></td>`;
        //        row += `<td class="border-end "><select class="form-select form-select-sm">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.widthUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
        //        row += `<td class="border-end "><input type="number" class="form-control form-control-sm" value="${item.flap || ''}"></td>`;
        //        row += `<td class="border-end "><select class="form-select form-select-sm">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.flapUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
        //        row += `<td class="border-end "><input type="number" class="form-control form-control-sm" value="${item.guest || ''}"></td>`;
        //        row += `<td class="border-end "><select class="form-select form-select-sm">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.guestUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
        //    }

        //    row += `<td class="border-end"><input type="number" class="form-control form-control-sm" value="${item.garmentQty || ''}"></td>`;
        //    row += `<td class="border-end"><select class="form-select form-select-sm">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.garmentQtyUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
        //    row += `<td class="border-end"><input type="number" class="form-control form-control-sm" value="${item.consumption || ''}"></td>`;
        //    row += `<td class="border-end"><select class="form-select form-select-sm">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.consumptionUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
        //    row += `<td class="border-end"><input type="number" class="form-control form-control-sm" value="${item.totalQty || ''}"></td>`;
        //    row += `<td class="border-end"><select class="form-select form-select-sm">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.totalQtyUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
        //    row += `<td class="border-end"><input type="number" class="form-control form-control-sm" value="${item.orderQty || ''}"></td>`;
        //    row += `<td class="border-end"><select class="form-select form-select-sm">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.orderQtyUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
        //    row += `<td class="border-end"><input type="number" class="form-control form-control-sm" value="${item.percentage || 0}"></td>`;
        //    row += `<td class="border-end"><input type="number" step="0.01" class="form-control form-control-sm" value="${item.unitPrice || ''}"></td>`;
        //    row += `<td class="border-end"><input type="number" step="0.01" class="form-control form-control-sm" value="${item.totalPrice || ''}"></td>`;
        //    row += `<td class="border-end"><select class="form-select form-select-sm">${dd.currencies.map(c => `<option value="${c.id}" ${c.id === item.currencyID ? 'selected' : ''}>${c.name}</option>`).join('')}</select></td>`;
        //    row += `<td class="border-end"><input type="text" class="form-control form-control-sm" value="${item.remarks || ''}"></td>`;
        //    row += `<td class="text-center"><button class="btn btn-sm btn-danger btn-delete-row"><i class="fas fa-trash"></i></button></td>`;
        //    row += '</tr>';
        //    return row;
        //}


        // ====================================================================
        // A. HEADER BUILDER FUNCTION
        // (Hidden headers: Type, Id, Integra Job No. are correctly included)
        // ====================================================================
        function buildHeaders(type) {
  

            let html = '<tr>';
            // Hidden headers (will be hidden via CSS/Class: header-hidden)
            html += '<th class="border-end header-hidden" style="min-width:70px">Type</th>';
            html += '<th class="border-end header-hidden" style="min-width:70px">Id</th>';
            html += '<th class="border-end header-hidden" style="min-width:70px">Integra Job No.</th>';

            // Original Visible Headers
            html += '<th class="border-end" style="min-width:70px">PO No.</th>';
            html += '<th class="border-end" style="min-width:90px">Item</th>';
            html += '<th class="border-end" style="min-width:100px">Description</th>';
            html += '<th class="border-end" style="min-width:80px">Color</th>';

            if (type === '04') {
                html += '<th class="border-end " style="min-width:80px">Size</th>';
                html += '<th class="border-end " style="min-width:80px">Length</th>';
                html += '<th class="border-end " style="min-width:80px">Unit</th>';
                html += '<th class="border-end " style="min-width:80px">Width</th>';
                html += '<th class="border-end " style="min-width:80px">Unit</th>';
                html += '<th class="border-end " style="min-width:80px">Height</th>';
                html += '<th class="border-end " style="min-width:80px">Unit</th>';
            } else if (type === '07') {
                html += `
        <th class="border-end " style="min-width:90px">
            <div class="d-flex justify-content-between align-items-center">
                <i class="fa-solid fa-plus text-dark" style="cursor:pointer;"></i>
                <span>Thread Count</span>
            </div>
        </th>
        `;
            } else if (type === '03') {
                html += '<th class="border-end " style="min-width:80px">Length</th>';
                html += '<th class="border-end " style="min-width:80px">Unit</th>';
                html += '<th class="border-end " style="min-width:80px">Width</th>';
                html += '<th class="border-end " style="min-width:80px">Unit</th>';
                html += '<th class="border-end " style="min-width:80px">Flap</th>';
                html += '<th class="border-end " style="min-width:80px">Unit</th>';
                html += '<th class="border-end " style="min-width:80px">Guest</th>';
                html += '<th class="border-end " style="min-width:80px">Unit</th>';
            }

            html += '<th class="border-end text-center" style="min-width:50px">Gar. Qty</th>';
            html += '<th class="border-end text-center" style="min-width:50px">Unit</th>';
            html += '<th class="border-end text-center" style="min-width:50px">Cons/mtr</th>';
            html += '<th class="border-end text-center" style="min-width:60px">Unit</th>';
            html += '<th class="border-end text-center" style="min-width:80px">Total Qty</th>';
            html += '<th class="border-end text-center" style="min-width:60px">Unit</th>';
            html += '<th class="border-end text-center" style="min-width:50px">Order Qty</th>';
            html += '<th class="border-end text-center" style="min-width:60px">Unit</th>';
            html += '<th class="border-end text-center" style="min-width:40px">Per (%)</th>';
            html += '<th class="border-end text-center" style="min-width:60px">Unit Price</th>';
            html += '<th class="border-end text-center" style="min-width:70px">Total Price</th>';
            html += '<th class="border-end text-center" style="min-width:50px">Curr.</th>';
            html += '<th class="border-end text-center" style="min-width:80px">Remarks</th>';
            html += '<th class="text-center" style="min-width:40px">Action</th>';
            html += '</tr>';
            return html;
        }

        // ====================================================================
        // B. ROW BUILDER FUNCTION
        // (Uses data-field="Id" and row-hidden class correctly)
        // ====================================================================
        function buildRow(item, type, dd) {
            //debugger
            console.log(item)
            let row = '<tr>';

            // 1. Booking Type (Hidden)
            row += `<td class="border-end row-hidden"><input type="hidden" class="form-control form-control-sm" data-field="BookingType" value="${type || ''}"></td>`;

            // 2. ID (Hidden, data-field="Id" for DTO mapping)
            row += `<td class="border-end row-hidden"><input type="hidden" class="form-control form-control-sm" data-field="Id" value="${item.id || ''}"></td>`;

            // 3. Integra Job No. (Hidden)
            row += `<td class="border-end row-hidden"><input type="hidden" class="form-control form-control-sm" data-field="IntegraJobNo" value="${item.integraJobNo || ''}"></td>`;

            // 4. PO No. (Visible)
            row += `<td class="border-end"><input type="text" class="form-control form-control-sm" data-field="PoNo" value="${item.poNo || ''}"></td>`;

            // 5. Item ID (Dropdown)
            row += `<td class="border-end"><select class="form-select form-select-sm" data-field="ItemId"><option value="">Select</option>${dd.items.map(i => `<option value="${i.id}" ${i.id === item.itemID ? 'selected' : ''}>${i.name}</option>`).join('')}</select></td>`;

            // 6. Description
            const description = (item.description || '').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
            row += `
<td class="border-end">
    <input type="text"
        class="form-control form-control-sm description-input"
        data-field="Description"
        data-bs-toggle="tooltip"
        data-bs-placement="top"
        data-bs-custom-class="custom-tooltip"
        data-bs-html="true"
        title="${description}"
        value="${description.replace(/&quot;/g, '"').replace(/&#39;/g, "'")}">
</td>`;

            // 7. Color
            row += `<td class="border-end"><select class="form-select form-select-sm" data-field="ColorId"><option value="">Select</option>${dd.colors.map(c => `<option value="${c.id}" ${c.id === item.colorID ? 'selected' : ''}>${c.name}</option>`).join('')}</select></td>`;

            // --- Type Specific Fields ---
            if (type === '04') {
                row += `<td class="border-end "><select class="form-select form-select-sm" data-field="SizeId"><option>Select</option>${dd.sizes.map(s => `<option value="${s.id}" ${s.id === item.sizeID ? 'selected' : ''}>${s.name}</option>`).join('')}</select></td>`;
                row += `<td class="border-end "><input type="number" class="form-control form-control-sm" data-field="CartonLength" value="${item.cartonLength || ''}"></td>`;
                row += `<td class="border-end "><select class="form-select form-select-sm" data-field="LeangthUnitID">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.leangthUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
                row += `<td class="border-end "><input type="number" class="form-control form-control-sm" data-field="CartonWidth" value="${item.cartonWidth || ''}"></td>`;
                row += `<td class="border-end "><select class="form-select form-select-sm" data-field="WidthUnitID">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.widthUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
                row += `<td class="border-end "><input type="number" class="form-control form-control-sm" data-field="CatonHeight" value="${item.catonHeight || ''}"></td>`;
                row += `<td class="border-end "><select class="form-select form-select-sm" data-field="HeightUnitID">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.heightUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
            } else if (type === '07') {
                row += `<td class="border-end "><select class="form-select form-select-sm" data-field="ThreadCountID"><option value="">Select</option>${dd.threadCounts.map(t => `<option value="${t.id}" ${t.id === item.threadCountID ? 'selected' : ''}>${t.name}</option>`).join('')}</select></td>`;
            } else if (type === '03') {
                row += `<td class="border-end "><input type="number" class="form-control form-control-sm" data-field="Length" value="${item.length || ''}"></td>`;
                row += `<td class="border-end "><select class="form-select form-select-sm" data-field="LengthUnitID">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.lengthUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
                row += `<td class="border-end "><input type="number" class="form-control form-control-sm" data-field="Width" value="${item.width || ''}"></td>`;
                row += `<td class="border-end "><select class="form-select form-select-sm" data-field="WidthUnitID">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.widthUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
                row += `<td class="border-end "><input type="number" class="form-control form-control-sm" data-field="Flap" value="${item.flap || ''}"></td>`;
                row += `<td class="border-end "><select class="form-select form-select-sm" data-field="FlapUnitID">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.flapUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
                row += `<td class="border-end "><input type="number" class="form-control form-control-sm" data-field="Guest" value="${item.guest || ''}"></td>`;
                row += `<td class="border-end "><select class="form-select form-select-sm" data-field="GuestUnitID">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.guestUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
            }

            // --- Common Fields ---
            row += `<td class="border-end"><input type="number" class="form-control form-control-sm" data-field="GarmentQty" value="${item.garmentQty || ''}"></td>`;
            row += `<td class="border-end"><select class="form-select form-select-sm" data-field="GarmentQtyUnitID">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.garmentQtyUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
            row += `<td class="border-end"><input type="number" class="form-control form-control-sm" data-field="Consumption" value="${item.consumption || ''}"></td>`;
            row += `<td class="border-end"><select class="form-select form-select-sm" data-field="ConsumptionUnitID">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.consumptionUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
            row += `<td class="border-end"><input type="number" class="form-control form-control-sm" data-field="TotalQty" value="${item.totalQty || ''}"></td>`;
            row += `<td class="border-end"><select class="form-select form-select-sm" data-field="TotalQtyUnitID">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.totalQtyUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
            row += `<td class="border-end"><input type="number" class="form-control form-control-sm" data-field="OrderQty" value="${item.orderQty || ''}"></td>`;
            row += `<td class="border-end"><select class="form-select form-select-sm" data-field="OrderQtyUnitID">${dd.units.map(u => `<option value="${u.id}" ${u.id === item.orderQtyUnitID ? 'selected' : ''}>${u.name}</option>`).join('')}</select></td>`;
            row += `<td class="border-end"><input type="number" class="form-control form-control-sm" data-field="Percentage" value="${item.percentage || 0}"></td>`;
            row += `<td class="border-end"><input type="number" step="0.01" class="form-control form-control-sm" data-field="UnitPrice" value="${item.unitPrice || ''}"></td>`;
            row += `<td class="border-end"><input type="number" step="0.01" class="form-control form-control-sm" data-field="TotalPrice" value="${item.totalPrice || ''}"></td>`;
            row += `<td class="border-end"><select class="form-select form-select-sm" data-field="CurrencyID">${dd.currencies.map(c => `<option value="${c.id}" ${c.id === item.currencyID ? 'selected' : ''}>${c.name}</option>`).join('')}</select></td>`;
            row += `<td class="border-end"><input type="text" class="form-control form-control-sm" data-field="Remarks" value="${item.remarks || ''}"></td>`;
            row += `<td class="text-center"><button class="btn btn-sm btn-danger btn-delete-row"><i class="fas fa-trash"></i></button></td>`;
            row += '</tr>';
            return row;
        }


        // ====================================================================
        // C. DATA COLLECTION LOGIC
        // (Ensures Id is collected and converted to integer)
        // ====================================================================
        function collectRowData($row) {
            const data = {};

            // Collect all fields with data-field attribute
            $row.find('[data-field]').each(function () {
                const fieldName = $(this).attr('data-field');
                let value = $(this).val();

                // Convert common numbers back to number type for DTO
                if (['Id', 'GarmentQty', 'Consumption', 'TotalQty', 'OrderQty', 'Percentage', 'UnitPrice', 'TotalPrice', 'CartonLength', 'CartonWidth', 'CatonHeight', 'Length', 'Width', 'Flap', 'Guest'].includes(fieldName)) {

                    // Special handling for ID (must be integer 0 or greater)
                    if (fieldName === 'Id') {
                        value = parseInt(value) || 0;
                    } else {
                        // Otherwise, convert to float/decimal
                        value = parseFloat(value) || null;
                    }
                }

                data[fieldName] = value;
            });

            // Check if ID is successfully collected
            if (!data.Id || data.Id === 0) {
                console.error("Error: Row ID is missing or zero. Cannot update.");
                return null;
            }

            return data;
        }

        // ====================================================================
        // D. AJAX UPDATE AND EVENT HANDLER
        // ====================================================================
        function sendUpdateToServer(data) {
            // Check if settings.baseUrl is defined and accessible
            const url = (typeof settings !== 'undefined' && settings.baseUrl) ? settings.baseUrl + '/UpdateBookingItem' : '/RMGBookingOrderEntryBukl/UpdateBookingItem';

            $.ajax({
                url: url,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        console.log("Success:", response.message);
                        // Optionally show a toast/success message here
                    } else {
                        console.error("Server Error:", response.message);
                        // Optionally show an error alert here
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.error("Failed to update booking item. AJAX Status:", textStatus, "Error:", errorThrown);
                    // Show generic error message
                }
            });
        }

        // Event handler for all input/select changes in the table
        $(document).on('change', '#bookingTable input, #bookingTable select', function () {
            if ($(this).attr('data-field')) {
                const $row = $(this).closest('tr');
                const updateData = collectRowData($row);

                if (updateData) {
                    console.log("Sending Data:", updateData);
                    sendUpdateToServer(updateData);
                }
            }
        });


        function handleBookingTypeChange() {
            var type = $(this).val();
            if (!type || type === '--Select Booking Type--') return;

            let costingIds = getSelectedCostingIds() || [];
            let bookingOrderIds = getselectedBookingOrderId();

            if (bookingOrderIds.length == 0) {
                if (costingIds.length === 0) {
                    toastr.warning("Please select at least one booking order");
                    return;
                }
            }

            if (costingIds.length === 0) {
                costingIds.push("edit");
            }

            console.log("Sending Costing IDs:", costingIds);

            var dto = {
                BookingType: type,
                CostingId: costingIds
            };





            showLoader();

            $.ajax({
                url: settings.baseUrl + "/LoadBookingTable",
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(dto),
                success: function (response) {
                    console.log(response);
                    hideLoader();
                    if (!response.success) {
                        toastr.error(response.message || 'Failed');
                        return;
                    }

                    destroyTable();

                    let tableHtml = '<thead class="table-light sticky-top">' + buildHeaders(type) + '</thead>';
                    tableHtml += '<tbody>';
                    $.each(response.data || [], function (i, item) {
                        tableHtml += buildRow(item, type, response.dropdownData);
                    });
                    tableHtml += '</tbody>';

                    $('#bookingTable').html(tableHtml);
                    if ($.fn.DataTable.isDataTable('#bookingTable')) {
                        $('#bookingTable').DataTable().destroy();
                    }
                    table = $('#bookingTable').DataTable({
                        scrollX: true,
                        scrollY: '500px',
                        scrollCollapse: true,
                        paging: false,
                        info: false,
                        ordering: false,
                        autoWidth: false,
                        searching: false,
                        destroy: true,
                        dom: 'ft',
                        initComplete: function () {
                            $('.dataTables_scrollHead').css({
                                'position': 'sticky',
                                'top': '0',
                                'z-index': '10',
                                'background': 'white'
                            });
                            applySelect2();
                            initializeTooltips();
                        }
                    });

                    setTimeout(() => table.columns.adjust().draw(false), 150);
                },
                error: function () {
                    hideLoader();
                    toastr.error('Server error');
                }
            });
        }

        function deleteRow() {
            if (table) {
                table.row($(this).parents('tr')).remove().draw(false);

                setTimeout(() => {
                    table.columns.adjust();
                    applySelect2();
                    initializeTooltips();
                }, 100);
            }
        }

        function injectTooltipStyles() {
            if (!document.getElementById('custom-tooltip-styles')) {
                const style = document.createElement('style');
                style.id = 'custom-tooltip-styles';
                style.innerHTML = `
                    .custom-tooltip {
                        background-color: #333 !important;
                        color: #fff !important;
                        font-size: 14px !important;
                        padding: 8px 12px !important;
                        border-radius: 8px !important;
                        box-shadow: 0px 4px 15px rgba(0,0,0,0.3) !important;
                        max-width: 300px !important;
                        word-wrap: break-word !important;
                    }
                    .custom-tooltip .tooltip-arrow::before {
                        border-top-color: #333 !important;
                    }
                `;
                document.head.appendChild(style);
            }
        }

        function disposeAllTooltips() {
            try {
                $('#bookingTable [data-bs-toggle="tooltip"]').each(function () {
                    const tooltipInstance = bootstrap.Tooltip.getInstance(this);
                    if (tooltipInstance) {
                        tooltipInstance.dispose();
                    }
                });
            } catch (e) {
                console.warn('Tooltip disposal warning:', e);
            }
        }

        function initializeTooltips() {
            disposeAllTooltips();

            setTimeout(() => {
                try {
                    $('#bookingTable [data-bs-toggle="tooltip"]').each(function () {
                        new bootstrap.Tooltip(this, {
                            trigger: 'hover focus',
                            boundary: 'window',
                            html: true
                        });
                    });
                } catch (e) {
                    console.warn('Tooltip initialization warning:', e);
                }
            }, 250);
        }

        function showLoader() { $('.loading-overlay').addClass('active'); }
        function hideLoader() { $('.loading-overlay').removeClass('active'); }

        init();

        function getIsoDate(dateStr) {
            if (!dateStr) return null;
            const parts = dateStr.split("/");
            if (parts.length !== 3) return null;
            const day = parts[0];
            const month = parts[1];
            const year = parts[2];
            return new Date(`${year}-${month}-${day}T00:00:00`).toISOString();
        }


        function getBookingDataAndSend() {
            return {
                Tc: parseFloat($("#BookingOrderEntryBuklSetup_Tc").val()) || 0,
                BookinOrderNo: $("#BookingOrderEntryBuklSetup_BookinOrderNo").val(),
                BookinDate: getIsoDate($("#BookingOrderEntryBuklSetup_BookinDate").val()),
                PurchasedOfficer: $("#BookingOrderEntryBuklSetup_PurchasedOfficer").val(),
                Remarks: $("#BookingOrderEntryBuklSetup_Remarks").val(),
                DeliveryDate: getIsoDate($("#BookingOrderEntryBuklSetup_DeliveryDate").val()),
                DeliveryAddress: $("#BookingOrderEntryBuklSetup_DeliveryAddress").val(),
                DeliveryMethod: $("#BookingOrderEntryBuklSetup_DeliveryMethod").val(),
                PaymentTerms: $("#BookingOrderEntryBuklSetup_PaymentTerms").val(),
                TermsCondition: $("#BookingOrderEntryBuklSetup_TermsCondition").val(),
                BookingType: $("#BookingOrderEntryBuklSetup_BookingType").val(),
                Pino: $("#BookingOrderEntryBuklSetup_Pino").val(),
                Pidate: getIsoDate($("#BookingOrderEntryBuklSetup_Pidate").val()),
                Pivalue: parseFloat($("#BookingOrderEntryBuklSetup_Pivalue").val()) || null,
                PicurrencyId: $("#BookingOrderEntryBuklSetup_PicurrencyId").val(),
                SupplierId: $("#BookingOrderEntryBuklSetup_SupplierId").val(),

                SelectedCostingIds: Array.from(selectedIds)
            };
        }
        $(document).on('click', '.js-booking-order-info-save', function () {            
            if (selectedIds.size === 0) {
                toastr.warning("Please select at least one purchase order");
                return;
            }

            var dto = getBookingDataAndSend();
            console.log("Final DTO:", dto);

            $.ajax({
                url: settings.baseUrl + '/SaveBooking',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(dto),
                success: function (response) {
                    toastr.success(response.message || "Saved successfully");

                    selectedIds.clear();
                    bookingOrderIds.clear();
                    if (purchaseTable) {
                        purchaseTable.ajax.reload(null, false);
                    }

                    BookingOrderGrid();
                    RMG_BookingOrderAutoId();
                },
                error: function (xhr) {
                    console.error(xhr.responseText);
                    toastr.error("Failed to save booking");
                }
            });
        });


        function resetBookingForm() {
            // Reset plain text/number/date inputs
            $("#BookingOrderEntryBuklSetup_Tc").val("");
            $("#BookingOrderEntryBuklSetup_BookinOrderNo").val("");
            $("#BookingOrderEntryBuklSetup_BookinDate").val("");
            $("#BookingOrderEntryBuklSetup_PurchasedOfficer").val("");
            $("#BookingOrderEntryBuklSetup_Remarks").val("");
            $("#BookingOrderEntryBuklSetup_DeliveryDate").val("");
            $("#BookingOrderEntryBuklSetup_DeliveryAddress").val("");
            $("#BookingOrderEntryBuklSetup_DeliveryMethod").val("");
            $("#BookingOrderEntryBuklSetup_PaymentTerms").val("");
            $("#BookingOrderEntryBuklSetup_TermsCondition").val("");
            $("#BookingOrderEntryBuklSetup_BookingType").val("");
            $("#BookingOrderEntryBuklSetup_Pino").val("");
            $("#BookingOrderEntryBuklSetup_Pidate").val("");
            $("#BookingOrderEntryBuklSetup_Pivalue").val("");
            $("#BookingOrderEntryBuklSetup_PicurrencyId").val("");
            $("#BookingOrderEntryBuklSetup_SupplierId").val("");

            // Reset Select2 dropdowns properly
            $(".select2").each(function () {
                $(this).val(null).trigger("change.select2");
            });

            // Clear selected IDs
            window.selectedIds.clear();
            window.selectedBookingOrderId.clear();

            // Reset DataTable silently if present
            if ($.fn.DataTable && $('#BookingOrderGrid').length) {
                $('#BookingOrderGrid').DataTable().clear().draw();
            }

            // Reset date pickers if you have a helper
            setDate();
        }
        $(document).on('click', '.js-costing-info-clear', function () {
            debugger;
            resetBookingForm(); 
        })
     
        $(document).on('click', '#js-costing-info-delete-confirm', function () {
            let bookingOrderIds = getselectedBookingOrderId();

            if (bookingOrderIds.length === 0) {
                toastr.warning("Please select at least one booking order to delete.");
                return;
            }

            // প্রথমে confirm alert দেখানো হবে
            if (confirm("Are you sure you want to delete the selected booking orders?")) {
                console.log("Delete confirmed for IDs:", bookingOrderIds);

                $.ajax({
                    url: settings.baseUrl + "/DeleteBookingOrder",
                    type: "POST",
                    contentType: "application/json",
                    data: JSON.stringify(bookingOrderIds),
                    success: function (res) {
                        console.log(res);

                        if (res.success) {
                            // Success toaster
                            toastr.success(res.message || "Booking orders deleted successfully!");

                            // Grid refresh
                            BookingOrderGrid();
                            window.selectedIds.clear();
                            window.selectedBookingOrderId.clear();
                        } else {
                            toastr.error(res.message || "Failed to delete booking orders.");
                        }
                    },
                    error: function (err) {
                        console.log(err);
                        toastr.error("An error occurred while deleting booking orders.");
                    }
                });
            } else {
                toastr.info("Delete action cancelled.");
            }
        });


        function BookingOrderGrid() {
            if ($.fn.DataTable.isDataTable('#bookingOrderGridTable')) {
                $('#bookingOrderGridTable').DataTable().destroy();
            }
            var bookingGridTable = $("#bookingOrderGridTable").DataTable({
                processing: true,
                serverSide: true,
                filter: true,
                orderMulti: false,
                ajax: {
                    url: settings.baseUrl + "/GetBookingList",
                    type: "POST",
                    dataSrc: function (data) {
                        console.log(data);
                        return data.data;
                    }
                },
                columns: [
                    {
                        data: "tc",
                        render: function (data) {
                            let checked = selectedBookingOrderId.has(data) ? "checked" : "";
                            return `<input type="checkbox" class="row-check-master-check" data-id="${data}" ${checked} />`;
                        }
                    },
                    {
                        data: "bookingOrderNo",
                        render: function (data) {
                            return `<button type="button" class="btn btn-link row-check-master" data-id="${data}">${data}</button>`;
                        }
                    },
                    { data: "bookingDate" },
                    { data: "bookingType" },
                    { data: "supplierId" },
                    { data: "styleId" },
                    { data: "poNo" },
                    { data: "integraJobNo" },
                    {
                        data: "pifilePath",
                        orderable: false,
                        render: function (file) {
                            return file ? `<a href="${file}" target="_blank" class="text-success">View</a>` : "";
                        }
                    }
                ],
                drawCallback: function () {
                    let total = $(".row-check-master-check").length;
                    let checked = $(".row-check-master-check:checked").length;
                    $("#selectMasterAll").prop("checked", total > 0 && total === checked);
                }
            });

            

            $("#bookingOrderGridTable").on("click", ".row-check-master", function () {
                let id = $(this).data("id");
                let rowData = bookingGridTable.row($(this).closest("tr")).data();

                selectedBookingOrderId.add(id);

                if (rowData == undefined) {
                    return;
                }
                populateBookingForm(rowData);

                $.ajax({
                    url: settings.baseUrl + "/GetBookingItemTypes",
                    type: "POST",
                    data: { id },
                    success: function (resp) {
                        console.log("Filtered Booking Item Types:", resp);

                        if (resp.success && resp.data) {
                            // Build headers based on first item
                            if (resp.data.length > 0) {
                                buildHeaders(resp.data[0]);
                            }

                            // Destroy old table if exists
                            if ($.fn.DataTable.isDataTable('#bookingTable')) {
                                $('#bookingTable').DataTable().clear().destroy();
                            }

                            // Build table HTML
                            let tableHtml = '<thead class="table-light sticky-top">' + buildHeaders(rowData.bookingType) + '</thead>';
                            tableHtml += '<tbody>';
                            $.each(resp.data || [], function (i, item) {
                                tableHtml += buildRow(item, rowData.bookingType, resp.dropdownData);
                            });
                            tableHtml += '</tbody>';

                            $('#bookingTable').html(tableHtml);

                            // Re-initialize DataTable
                            let table = $('#bookingTable').DataTable({
                                scrollX: true,
                                scrollY: '500px',
                                scrollCollapse: true,
                                paging: false,
                                info: false,
                                ordering: false,
                                autoWidth: false,
                                searching: false,
                                dom: 'ft',
                                initComplete: function () {
                                    $('.dataTables_scrollHead').css({
                                        'position': 'sticky',
                                        'top': '0',
                                        'z-index': '10',
                                        'background': 'white'
                                    });
                                    applySelect2();
                                    initializeTooltips();
                                }
                            });

                            setTimeout(() => table.columns.adjust().draw(false), 150);
                        } else {
                            alert(resp.message);
                        }
                    },
                    error: function (xhr) {
                        console.error("Error fetching booking item types", xhr);
                    }
                });

                console.log("Selected IDs:", [...selectedBookingOrderId]);
            });


        }

        // click handler for the bookingOrderNo button
        $(document).on("change", ".row-check-master-check", function () {
            let id = $(this).data("id");
            if (this.checked) {
                selectedBookingOrderId.add(id);
            } else {
                selectedBookingOrderId.delete(id);
            }
            console.log("Selected IDs:", [...selectedBookingOrderId]);
        });

        $(document).on("change", "#selectMasterAll", function () {
            let checked = this.checked;

            $(".row-check-master-check").each(function () {
                let id = $(this).data("id");
                $(this).prop("checked", checked);

                if (checked) {
                    selectedBookingOrderId.add(id);
                } else {
                    selectedBookingOrderId.delete(id);
                }
            });

            console.log("Selected IDs:", [...selectedBookingOrderId]);
        });

        function getselectedBookingOrderId() {
            if (!window.selectedBookingOrderId) {
                console.warn("selectedIds not initialized");
                return [];
            }
            window.selectedIds.clear();
            
            let arr = Array.from(window.selectedBookingOrderId);

            console.log("Final Costing ID Array:", arr);

            return arr;
        }


        function getSelectedCostingIds() {
            if (!window.selectedIds) {
                console.warn("selectedIds not initialized");
                return [];
            }

            window.selectedBookingOrderId.clear();
            let arr = Array.from(window.selectedIds);

            console.log("Final Costing ID Array:", arr);

            return arr;
        }
        


       
        function populateBookingForm(data) {
            $("#BookingOrderEntryBuklSetup_Tc").val(data.tc);
            $("#BookingOrderEntryBuklSetup_BookinOrderNo").val(data.bookingOrderNo);
            $("#BookingOrderEntryBuklSetup_BookinDate").val(data.bookingDate);
            $("#MasterPurchaseOrder").val(data.masterPurchaseOrder);
            $("#PoNo").val(data.poNo);
            $("#IntegraJobNo").val(data.integraJobNo);
            $("#BookingOrderEntryBuklSetup_PurchasedOfficer").val(data.purchasedOfficer).trigger("change");
            $("#BookingOrderEntryBuklSetup_Remarks").val(data.remarks);
            $("#BookingOrderEntryBuklSetup_DeliveryDate").val(data.deliveryDate);
            $("#BookingOrderEntryBuklSetup_DeliveryAddress").val(data.deliveryAddress);
            $("#BookingOrderEntryBuklSetup_DeliveryMethod").val(data.deliveryMethod).trigger("change");
            $("#BookingOrderEntryBuklSetup_PaymentTerms").val(data.paymentTerms).trigger("change");
            $("#BookingOrderEntryBuklSetup_TermsCondition").val(data.termsCondition).trigger("change");
            $("#BookingOrderEntryBuklSetup_BookingType").val(data.bookingType).trigger("change"); // Select2 safe
            $("#BookingEntryType").val(data.bookingEntryType);
            $("#WarehouseId").val(data.warehouseId);
            $("#BookingOrderEntryBuklSetup_Pino").val(data.pino);
            $("#BookingOrderEntryBuklSetup_Pidate").val(data.pidate);
            $("#BookingOrderEntryBuklSetup_Pivalue").val(data.pivalue);
            $("#BookingOrderEntryBuklSetup_PicurrencyId").val(data.picurrencyId).trigger("change");
            $("#Mrbpid").val(data.mrbpid);
            $("#EnterFromPageName").val(data.enterFromPageName);
            $("#PifilePath").val(data.pifilePath);

            // Select2 fields
            $("#BuyerId").val(data.buyerId).trigger("change");
            $("#StyleId").val(data.styleId).trigger("change");
            $("#SupplierId").val(data.supplierId).trigger("change");
        }


        return {
            getTableData: function () {
                var data = [];
                $('#bookingTable tbody tr').each(function () {
                    var row = {};
                    $(this).find('input, select').each(function (i) {
                        row['field_' + i] = $(this).val();
                    });
                    data.push(row);
                });
                return data;
            },
            getCurrentBookingType: function () { return currentBookingType; },
            refreshTooltips: function () { initializeTooltips(); }
        };






    };
})(jQuery);