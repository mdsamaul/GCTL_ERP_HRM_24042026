//using AutoMapper;
//using GCTL.Core.ViewModels.EmployeeFilter;
//using GCTL.Service.Common;
//using GCTL.Service.EmployeeFilterService;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.ViewEngines;

//namespace GCTL.UI.Core.Controllers
//{
//    //public class EmployeeFilterController : Controller
//    //{
//    //    private readonly IEmployeeFilterService filterService;
//    //    private readonly ICommonService commonService;
//    //    private readonly ICompositeViewEngine viewEngine;
//    //    private readonly IMapper mapper;

//    //    public EmployeeFilterController(IEmployeeFilterService filterService,
//    //        ICommonService commonService, ICompositeViewEngine viewEngine, IMapper mapper)
//    //    {
//    //        this.filterService = filterService;
//    //        //this.commonService = commonService;
//    //        //this.viewEngine = viewEngine;
//    //        this.mapper = mapper;
//    //    }


//    //    public IActionResult GetAllCompany()
//    //    {
//    //        var result = filterService.GetAllCompany();
//    //        return Json(new { data = result });
//    //    }

//    //    public IActionResult Setup()
//    //    {
//    //        return View();
//    //    }

//    //    public async Task<IActionResult> getFilterEmp([FromBody] EmployeeFilterViewModel model)
//    //    {
//    //        var data = await filterService.GetFilterDataAsync(model);
//    //        return Json(data);
//    //    }

//    //}
//}
