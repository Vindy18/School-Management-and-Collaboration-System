using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Data.Data;
using SchoolManagement.ViewModel.Master;
using SchoolManagement.ViewModel.Report;
using SchoolManagement.WebService.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolManagement.WebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassReportController : ControllerBase
    {
        private readonly SchoolManagementContext schoolDb;
        private readonly IIdentityService identityService;

        public ClassReportController(SchoolManagementContext schoolDb, IIdentityService identityService)
        {
            this.schoolDb = schoolDb;
            this.identityService = identityService;
        }

        [HttpGet]
        [Route("getTeachers")]
        public List<ClassReportViewModel> GetTeachers()
        {
            var query = schoolDb.Classes.Where(u => u.ClassNameId != null);
            var ClassList = query.ToList();

            List<ClassReportViewModel> response = new List<ClassReportViewModel>();

            foreach (var classReportViewModel in ClassList)
            {
                ClassReportViewModel vm = new ClassReportViewModel()
                {
                    AcademicYearId = classReportViewModel.AcademicYear.Id,
                    AcademicLevelId = classReportViewModel.AcademicLevel.Name,
                    ClassNameId = classReportViewModel.ClassName.Name,
                    Name = classReportViewModel.Name,
                    //ClassTeacherId = classReportViewModel.CreatedById
                };
                response.Add(vm);
            }
            return response;
        }

        [HttpPost]
        [Route("report")]
        public ActionResult Report(ClassReportViewModel classReportViewModel)
        {

            ClassReport classReport = new ClassReport();
            byte[] abytes = classReport.PrepareReport(GetTeachers());
            return File(abytes, "application/pdf");

        }
    }
}
