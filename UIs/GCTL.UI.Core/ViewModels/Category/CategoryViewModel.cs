using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Category;

namespace GCTL.UI.Core.ViewModels.Category
{
    public class CategoryViewModel: BaseViewModel
    {
        public CategorySetupViewModel Setup { get; set; } = new CategorySetupViewModel();
    }
}
