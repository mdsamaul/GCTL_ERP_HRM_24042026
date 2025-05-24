(function ($) {
    $.patientTypes = function (options) {
        var settings = $.extend({
            baseUrl: "/",
            companyIds: "#companySelect",
            branchIds: "#branchSelect",
            departmentIds: "#departmentSelect",
            designationIds: "#designationSelect",
            employeeIds: "#employeeSelect",
            FromDate: "#FromDateSelect",
            FlatPicker: ".flatDate",
            ToDate: "#ToDateSelect",
            load: function () {
                console.log("Loading...");
            }
        }, options);

        var filterUrl = settings.baseUrl + "/getAllFilterEmp";

        var setupLoadingOverlay = function () {
            if ($("#customLoadingOverlay").length === 0) {
                $("body").append(`
                    <div id="customLoadingOverlay" style="
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
                                <span class="sr-only">Loading...</span>
                            </div>
                            <p style="margin-top: 10px; margin-bottom: 0;">Loading data...</p>
                        </div>
                    </div>
                `);
            }
        };

        function showLoading() {
            $("#customLoadingOverlay").css("display", "flex");
        }

        function hideLoading() {
            $("#customLoadingOverlay").hide();
        }

        var initializeMultiselects = function () {
            // সঠিকভাবে সিলেক্টরগুলো একত্রে যুক্ত করা হলো
            var selectors = [
                settings.companyIds,
                settings.branchIds,
                settings.departmentIds,
                settings.designationIds,
                settings.employeeIds
            ].join(", ");

            $(selectors).multiselect({
                enableFiltering: true,
                includeSelectAllOption: true,
                selectAllText: 'Select All',
                nonSelectedText: 'Select Items',
                nSelectedText: 'Selected',
                allSelectedText: 'All Selected',
                filterPlaceholder: 'Search.......',
                buttonWidth: '100%',
                maxHeight: 350,
                enableClickableOptGroups: true,
                dropUp: false,
                numberDisplayed: 1,
                enableCaseInsensitiveFiltering: true
            });
        };

        var GetFlatDate = function () {
            flatpickr($(settings.FlatPicker), {
                dateFormat: "Y-m-d",
                altInput: true,
                altFormat: "d/m/Y",
                allowInput: true,
                onReady: function (selectedDates, dateStr, instance) {
                    instance.input.placeholder = "dd/mm/yyyy";
                }
            });
        };

        // Filter Value Getter
        var getFilterValue = function () {
            const fromDateVal = $(settings.FromDate).val();
            const toDateVal = $(settings.ToDate).val();
            var filterData = {
                CompanyCode: toArray($(settings.companyIds).val()),
                BranchCodes: toArray($(settings.branchIds).val()),
                DepartmentCodes: toArray($(settings.departmentIds).val()),
                DesignationCodes: toArray($(settings.designationIds).val()),
                EmployeeIDs: toArray($(settings.employeeIds).val()),
                FromDate: fromDateVal ? new Date(fromDateVal).toISOString().split('T')[0] : null,
                ToDate: toDateVal ? new Date(toDateVal).toISOString().split('T')[0] : null
            };
            return filterData;
        };

        // Array Helper
        var toArray = function (value) {
            if (!value) return [];
            if (Array.isArray(value)) return value;
            return [value];
        };

        var loadFilterEmp = function () {
            showLoading();
            var filterData = getFilterValue();
            $.ajax({
                url: filterUrl,
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(filterData),
                success: function (res) {
                    hideLoading();
                    if (!res.isSuccess) {                    
                        showToast('error', res.message);                       
                        return;
                    }
                    $(settings.companyIds, settings, settings.branchIds, settings.departmentIds, settings.designationIds, settings.employeeIds, settings.FromDate, settings.ToDate).off('change');
                    //loadTableData(res);
                    const data = res.data;
                    //console.log(res);
                    console.log(data.companies.some(x => x.name != null ));
                    if (data.companies && data.companies.length > 0 && data.companies.some(x => x.code != null && x.name != null)) {
                        var Companys = data.companies;
                        console.log(Companys);
                        var optCompany = $(settings.companyIds);
                        $.each(Companys, function (index, company) {
                            console.log(company);
                            if (company.code != null && company.name != null && optCompany.find(`option[value="${company.code}"]`).length === 0) {
                                optCompany.append(`<option value="${company}">${company.name}</option>`)
                            }
                        });
                        optCompany.multiselect('rebuild');
                    }

                },
                error: function (e) {
                    console.log(e);
                    hideLoading();
                }
            });
        };

        // Button Click Event Binding
        $("#checkCompanyBtn").on("click", function () {
            console.log("Company Value:", getFilterValue());
        });

        // Optional: Load Callback
        var init = function () {
            GetFlatDate();
            settings.load(); // Call any custom load logic
            initializeMultiselects();
            setupLoadingOverlay();
            loadFilterEmp();
            console.log(filterUrl);
        };

        init();
    };
})(jQuery);
