using AutoMapper;
using GCTL.Core.ViewModels.HrmPayDefDeductionTypes;
using GCTL.Data.Models;

namespace GCTL.UI.Core.Helpers.Mappers.HrmDefDeductionType
{
    public class DeductionTypeProfile:Profile
    {
        public DeductionTypeProfile()
        {
            CreateMap<HrmPayDefDeductionType, DeductionTypeSetupViewModel>().ReverseMap();
            CreateMap<DeductionTypeSetupViewModel, HrmPayDefDeductionType>()
                .ForMember(dest => dest.Tc, opt => opt.Ignore());
        }
    }
}
