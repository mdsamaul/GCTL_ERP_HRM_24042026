using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.ProductIssueEntry
{
    public class ProductIssueInformationDetailViewModel
    {
        public decimal TC { get; set; }
        public string PIDID { get; set; } = string.Empty;
        public string IssueNo { get; set; } = string.Empty;
        public string? ProductCode { get; set; }
        public string? BrandID { get; set; }
        public string? ModelID { get; set; }
        public string? SizeID { get; set; }
        public string? UnitTypID { get; set; }
        public decimal? StockQty { get; set; }
        public decimal? IssueQty { get; set; }
        public string? LUser { get; set; }
        public string? FloorCode { get; set; }
    }
}
