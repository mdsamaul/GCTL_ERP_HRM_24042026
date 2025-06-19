using AutoMapper;
using GCTL.Data.Models;
using GCTL.Core.ViewModels.HrmPayDefBenefitTypes;

namespace GCTL.UI.Core.Helpers.Mappers.HrmPayDefBenefitTypes
{
    public class BenefitTypeProfile:Profile
    {
        public BenefitTypeProfile()
        {
            CreateMap<HrmPayDefBenefitType, HrmPayDefBenefitTypeViewModel>().ReverseMap();
            CreateMap<HrmPayDefBenefitTypeViewModel, HrmPayDefBenefitType>()
                .ForMember(dest => dest.Tc, opt => opt.Ignore());
        }

    }
}
