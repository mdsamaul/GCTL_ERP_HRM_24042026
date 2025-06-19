let empDataTable = null;
let filterChangeBound = false;
let originalEmpId = null;
let selectedOtIds = new Set();
let selectedEmpIds = new Set();
let isEditMode = false;

$(document).ready(function () {
    setupLoadingOverlay();
    loadMonthsDD();
    initializeMultiselects();
    initializeEmployeeGrid();
    loadAllFilterEmp();
    loadMonthlyOtId();
    loadMonthlyOtData();
    initializeEventHandlers();
    setupEnterKeyNavigation();
});

$(window).on('load', function () {
    $('#employee-filter-grid-body')[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
});

function setupLoadingOverlay() {
    if ($("#loadingOverlay").length === 0) {
        $("body").append(`
            <div id="loadingOverlay" style="
                display: none;
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background-color: rgba(0, 0, 0, 0.5);
                z-index: 9999;
                justify-content: center;
                align-items: center;">
                <div style="
                    background-color: white;
                    padding: 20px;
                    border-radius: 5px;
                    box-shadow: 0 0 10px rgba(0,0,0,0.3);
                    text-align: center;">
                    <div class="spinner-border text-primary" role="status">
                       
                    </div>
                </div>
            </div>
        `);
    }
}
function showLoading() {
    $('body').css('overflow', 'hidden');
    $("#loadingOverlay").fadeIn(200);
}

function hideLoading() {
    $('body').css('overflow', '');
    $("#loadingOverlay").fadeOut(200);
}

function setupEnterKeyNavigation() {
    const $form = $('#monthlyOt-form');
    if (!$form.length) return;

    $form.on('keydown', 'input, select, textarea, button, [tabindex]:not([tabindex="-1"])', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();

            const $focusable = $form
                .find('input:not([disabled]), select:not([disabled]), textarea:not([disabled]), button, [herf], [tabindex]:not([tabindex="-1"])')
                .filter(':visible');

            const index = $focusable.index(this);
            if (index > -1) {
                const $next = $focusable.eq(index + 1).length ?
                    $focusable.eq(index + 1) : $focusable.eq(0);
                $next.focus();
            }
        }
    });
}

function initialEventHandlers() {
    $("#companySelect, #branchSelect, #divisionSelect, #departmentSelect, #designationSelect, #employeeSelect, #employeeStatus, #activityStatusSelect")
        .on("change", function () {
            loadAllFilterEmp();
        });

    $('input[name="durationType"]').change(function () {
        toggleFields();
    });

    $('#byMonth').prop('checked', true);

    toggleFields();
    $('#SalaryMonth').focus();

    $(".js-monthlyOt-dec-save").on('click', handleFormSubmission);
    $(".js-monthlyOt-dec-delete-confirm").on('click', handleBulkDelete);
    $(".js-monthlyOt-dec-clear").on('click', function () {
        clearForm();
        loadMonthlyOtId();
    });
    $("#excelUploadForm").submit(excelUpload);

    $(document).on("click", ".monthlyOt-id-link", function () {
        const id = $(this).data("id");
        if (!id) return;

        populateForm(id);
    });

    $('#monthlyOt-grid').DataTable().columns.adjust().draw();

    $('#monthlyOt-check-all').on('change', function () {
        const isChecked = $(this).is(':checked');
        $('#monthlyOt-grid-body input[type="checkbox"]').prop('checked', isChecked);

        updateSelectedMonthlyOtIds();
    });

    let today = new Date().toISOString().split('T')[0];

    $("#dateFrom, #dateTo").val(today);

    if (!$('#SalaryYear').val()) {
        const currentYear = new Date().getFullYear();
        $('#SalaryYear').val(currentYear);
    }

    $('#employee-filter-grid').on('keydown', 'input[type="checkbox"]', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            if ($('#byMonth').is(':checked')) {
                $('#SalaryMonth').focus();
            } else if ($('#byDate').is(':checked')) {
                $('#dateFrom').focus();
            }
        }
    });

    $("#employee-check-all").on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            if ($('#byMonth').is(':checked')) {
                $('#SalaryMonth').focus();
            } else if ($('#byDate').is(':checked')) {
                $('#dateFrom').focus();
            }
        }
    });


    $("#employee-check-all").on('change', function () {
        const isChecked = $(this).is(':checked');
        const checkboxes = $('#employee-filter-grid-body input[type="checkbox"]').not(':disabled');

        checkboxes.prop('checked', isChecked);
        updateSelectedEmployeeIds();
    });

    $('#employee-filter-grid-body').on('change', 'input[type="checkbox"]', function () {
        updateSelectedEmployeeIds();
        updateSelectAllCheckboxState();
    });
}
$(document).on('change', '#monthlyOt-grid-body input[type="checkbox"]', function () {
    const id = $(this).data('id');

    if ($(this).is(':checked')) {
        selectedOtIds.add(id);
    } else {
        selectedOtIds.delete(id);
    }

    const total = $('#monthlyOt-grid-body input[type="checkbox"]').length;
    const checked = $('#smonthlyOt-grid-body input[type="checkbox"]:checked').length;
    $("#monthlyOt-check-all").prop('checked', total > 0 && total === checked);
});

function updateSelectedMonthlyOtIds() {
    const currentPageCheckboxes = ('')

}