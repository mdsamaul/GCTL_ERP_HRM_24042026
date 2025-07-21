﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.PrintingStationeryPurchaseReport;

namespace GCTL.Service.PrintingStationeryPurchaseReportService
{
    public interface IPrintingStationeryPurchaseReportService
    {
        Task<List<PrintingStationeryPurchaseReportResultDto>> GetAllPROCPrintingAndStationeryReport(PrintingStationeryPurchaseReportFilterDto model);
        Task<PrintingStationeryDropdownDto> GetFilteredDropdownsAsync(PrintingStationeryPurchaseReportFilterDto model);
    }
}
