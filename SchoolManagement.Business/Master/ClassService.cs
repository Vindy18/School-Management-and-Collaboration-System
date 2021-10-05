using Microsoft.Extensions.Configuration;
using SchoolManagement.Business.Interfaces.MasterData;
using SchoolManagement.Data.Data;
using SchoolManagement.Master.Data.Data;
using SchoolManagement.Model;
using SchoolManagement.Util;
using SchoolManagement.ViewModel.Master;
using SchoolManagement.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolManagement.Util.Constants.ServiceClassConstants;
using SchoolManagement.ViewModel;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;

namespace SchoolManagement.Business.Master
{
  public class ClassService : IClassService
  {
    private readonly SchoolManagementContext schoolDb;
    private readonly IConfiguration config;
    private readonly ICurrentUserService currentUserService;

    public ClassService(SchoolManagementContext schoolDb, IConfiguration config, ICurrentUserService currentUserService)
    {
      this.schoolDb = schoolDb;
      this.config = config;
      this.currentUserService = currentUserService;
    }

    public async Task<ResponseViewModel> DeleteClass(int academicYearId, int academicLevelId, int classNameId, string username)
    {
      var response = new ResponseViewModel();

      var currentUser = currentUserService.GetUserByUsername(username);

      var classObj = schoolDb.Classes.FirstOrDefault(x => x.ClassNameId == classNameId && x.AcademicLevelId == academicLevelId && x.AcademicYearId == academicYearId);

      if (classObj.StudentClasses.Count() == 0)
      {
        schoolDb.ClassSubjectTeachers.ToList().ForEach(x =>
        {
          schoolDb.ClassSubjectTeachers.Remove(x);
        });

        schoolDb.Classes.Remove(classObj);
      }
      else
      {
        classObj.IsActive = false;
        classObj.UpdatedById = currentUser.Id;
        classObj.UpdatedOn = DateTime.UtcNow;

        schoolDb.Classes.Update(classObj);
      }

      await schoolDb.SaveChangesAsync();

      response.IsSuccess = true;
      response.Message = "Class has been deleted successfully.";

      return response;
    }

    public ClassViewModel GetClassDetails(int academicYearId, int academicLevelId, int classNameId)
    {
      var classObj = schoolDb.Classes.FirstOrDefault(x => x.AcademicYearId == academicYearId && x.AcademicLevelId == academicLevelId && x.ClassNameId == classNameId);

      var classTeacher = classObj.ClassTeachers.FirstOrDefault(c => c.IsPrimary == true);

      var vm = new ClassViewModel()
      {
        AcademicLevelId = classObj.AcademicLevelId,
        AcademicYearId = classObj.AcademicYearId,
        ClassCategoryId = classObj.ClassCategory,
        ClassNameId = classObj.ClassNameId,
        ClassTeacherId = classTeacher!=null? classTeacher.TeacherId:0,
        LanguageStreamId = classObj.LanguageStream,
        Name = classObj.Name
      };

      var academicLevelSubjects = schoolDb.SubjectAcademicLevels.Where(x => x.AcademicLevelId == classObj.AcademicLevelId)
        .OrderBy(s => s.Subject.Name).ToList();

      foreach (var item in academicLevelSubjects)
      {
        var allSubjectTeachers = item.Subject.SubjectTeachers
          .Where(x => x.AcademicYearId == academicYearId && x.IsActive == true)
          .Select(t => new DropDownViewModel() { Id = t.Id, Name = t.Teacher.FullName }).ToList();

        var subjectTeacher = classObj.ClassSubjectTeachers.FirstOrDefault(x => x.SubjectId == item.SubjectId);

        var classSubjectTeacherVm = new ClassSubjectTeacherViewModel()
        {
          Id = subjectTeacher != null ? subjectTeacher.Id : 0,
          AcademicLevelId = academicLevelId,
          AcademicYearId = academicYearId,
          AllSubjectTeachers = allSubjectTeachers,
          SubjectId = item.SubjectId,
          SubjectName = item.Subject.Name,
          SubjectTeacherId = subjectTeacher != null ? subjectTeacher.SubjectTeacherId : 0
        };

        vm.ClassSubjectTeachers.Add(classSubjectTeacherVm);
      }

      return vm;
    }

    public PaginatedItemsViewModel<BasicClassViewModel> GetClassList(string searchText, int currentPage, int pageSize, int academicYearId, int academicLevelId)
    {
      int totalRecordCount = 0;
      double totalPages = 0;
      int totalPageCount = 0;
      var vms = new List<BasicClassViewModel>();

      var classes = schoolDb.Classes.OrderBy(cl => cl.Name);

      if (!string.IsNullOrEmpty(searchText))
      {
        classes = classes.Where(x => x.Name.Contains(searchText)).OrderBy(cl => cl.Name);
      }

      if (academicYearId > 0)
      {
        classes = classes.Where(x => x.AcademicYearId == academicYearId).OrderBy(cl => cl.Name);
      }

      if (academicLevelId > 0)
      {
        classes = classes.Where(x => x.AcademicLevelId == academicLevelId).OrderBy(cl => cl.Name);
      }

      totalRecordCount = classes.Count();
      totalPages = (double)totalRecordCount / pageSize;
      totalPageCount = (int)Math.Ceiling(totalPages);

      var classList = classes.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

      classList.ForEach(cl =>
      {
        var classTeacher = cl.ClassTeachers.FirstOrDefault(x => x.IsPrimary == true);

        var vm = new BasicClassViewModel()
        {
          AcademicLevelId = cl.AcademicLevelId,
          AcademicYearId = cl.AcademicYearId,
          ClassNameId= cl.ClassNameId,
          ClassTeacherName = classTeacher != null ? classTeacher.Teacher.FullName : string.Empty,
          Name = cl.Name,
          TotalStudentCount = cl.StudentClasses.Count()
        };

        vms.Add(vm);
      });

      var container = new PaginatedItemsViewModel<BasicClassViewModel>(currentPage, pageSize, totalPageCount, totalRecordCount, vms);

      return container;
    }

    public ClassMasterDataViewModel GetClassMasterData()
    {
      var response = new ClassMasterDataViewModel();

      response.CurrentAcademicYear = schoolDb.AcademicYears.FirstOrDefault(x => x.IsActive).Id;
      response.AcademicYears = schoolDb.AcademicYears.OrderBy(x => x.Id).Select(a => new DropDownViewModel() { Id = a.Id, Name = a.Id.ToString() }).ToList();
      response.ClassNames = schoolDb.ClassNames.OrderBy(cl=>cl.Name).Select(a => new DropDownViewModel() { Id = a.Id, Name =a.Name }).ToList();
      response.AcademicLevels = schoolDb.AcademicLevels.OrderBy(x => x.Name).Select(a => new DropDownViewModel() { Id = a.Id, Name = a.Name }).ToList();
      response.AllTeachers = schoolDb.Roles.Where(x => x.Id == 6).SelectMany(s => s.UserRoles).Select(s => new DropDownViewModel() { Id = s.UserId, Name = s.User.FullName }).ToList();

      foreach (ClassCategory category in (ClassCategory[])Enum.GetValues(typeof(ClassCategory)))
      {
        response.ClassCategories.Add(new DropDownViewModel() { Id = (int)category, Name = EnumHelper.GetEnumDescription(category) });
      }

      foreach (LanguageStream stream in (LanguageStream[])Enum.GetValues(typeof(LanguageStream)))
      {
        response.LanguageStreams.Add(new DropDownViewModel() { Id = (int)stream, Name = EnumHelper.GetEnumDescription(stream) });
      }

      return response;
    }

    public List<ClassSubjectTeacherViewModel> GetClassSubjectsForSelectedAcademiclevel(int academicYearId, int academicLevelId)
    {
      var response = new List<ClassSubjectTeacherViewModel>();

      var academicLevelSubjects = schoolDb.SubjectAcademicLevels.Where(x => x.AcademicLevelId == academicLevelId).OrderBy(s => s.Subject.Name).ToList();

      foreach (var item in academicLevelSubjects)
      {
        var allSubjectTeachers = item.Subject.SubjectTeachers
          .Where(x => x.AcademicYearId == academicYearId && x.IsActive == true)
          .Select(t => new DropDownViewModel() { Id = t.Id, Name = t.Teacher.FullName }).ToList();

        var vm = new ClassSubjectTeacherViewModel()
        {
          AcademicLevelId = academicLevelId,
          AcademicYearId = academicYearId,
          AllSubjectTeachers = allSubjectTeachers,
          SubjectId = item.SubjectId,
          SubjectName = item.Subject.Name
        };

        response.Add(vm);
      }

      return response;
    }

    public async Task<ResponseViewModel> SaveClassDetail(ClassViewModel vm, string userName)
    {
      var response = new ResponseViewModel();

      try
      {
        var currentUser = currentUserService.GetUserByUsername(userName);

        var classObj = schoolDb.Classes.FirstOrDefault(x => x.AcademicYearId == vm.AcademicYearId && x.AcademicLevelId == vm.AcademicLevelId && x.ClassNameId == vm.ClassNameId);

        if (classObj == null)
        {
          classObj = new Class()
          {
            AcademicLevelId = vm.AcademicLevelId,
            AcademicYearId = vm.AcademicYearId,
            ClassCategory = vm.ClassCategoryId,
            ClassNameId = vm.ClassNameId,
            CreatedById = currentUser.Id,
            CreatedOn = DateTime.UtcNow,
            IsActive = true,
            LanguageStream = vm.LanguageStreamId,
            Name = vm.Name,
            UpdatedById = currentUser.Id,
            UpdatedOn = DateTime.UtcNow,
          };

          classObj.ClassTeachers = new HashSet<ClassTeacher>();
          classObj.ClassTeachers.Add(new ClassTeacher()
          {
            TeacherId = vm.ClassTeacherId,
            IsPrimary = true,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedById = currentUser.Id,
            UpdatedOn = DateTime.UtcNow,
            UpdatedById = currentUser.Id
          });

          classObj.ClassSubjectTeachers = new HashSet<ClassSubjectTeacher>();

          foreach (var item in vm.ClassSubjectTeachers)
          {
            classObj.ClassSubjectTeachers.Add(new ClassSubjectTeacher()
            {
              CreatedById = currentUser.Id,
              SubjectId = item.SubjectId,
              SubjectTeacherId = item.SubjectTeacherId,
              StartDate = DateTime.UtcNow,
              IsActive = true,
              UpdatedById = currentUser.Id,
              UpdatedOn = DateTime.UtcNow,
              CreatedOn = DateTime.UtcNow
            });
          }

          schoolDb.Classes.Add(classObj);

          response.Message = "New class has been successfully added.";
        }
        else
        {

          classObj.LanguageStream = vm.LanguageStreamId;
          classObj.UpdatedById = currentUser.Id;
          classObj.UpdatedOn = DateTime.UtcNow;
          if(classObj.ClassTeachers.Count()>0)
          {
            var classTeacher = classObj.ClassTeachers.FirstOrDefault();
            classTeacher.TeacherId = vm.ClassTeacherId;
            classTeacher.UpdatedById = currentUser.Id;
            classTeacher.UpdatedOn = DateTime.UtcNow;

            schoolDb.ClassTeachers.Update(classTeacher);
          }
          else
          {
            classObj.ClassTeachers.Add(new ClassTeacher()
            {
              TeacherId = vm.ClassTeacherId,
              IsPrimary = true,
              IsActive = true,
              CreatedOn = DateTime.UtcNow,
              CreatedById = currentUser.Id,
              UpdatedOn = DateTime.UtcNow,
              UpdatedById = currentUser.Id
            });
          }


          var savedClassSubjectTeachers = classObj.ClassSubjectTeachers.Where(x => x.IsActive == true).ToList();

          var newlyAddedClassSubjectTeachers = (from n in vm.ClassSubjectTeachers where !savedClassSubjectTeachers.Any(x => x.Id == n.Id) select n).ToList();

          foreach (var item in newlyAddedClassSubjectTeachers)
          {
            classObj.ClassSubjectTeachers.Add(new ClassSubjectTeacher()
            {
              CreatedById = currentUser.Id,
              SubjectId = item.SubjectId,
              SubjectTeacherId = item.SubjectTeacherId,
              StartDate = DateTime.UtcNow,
              IsActive = true,
              UpdatedById = currentUser.Id,
              UpdatedOn = DateTime.UtcNow,
              CreatedOn = DateTime.UtcNow
            });
          }

          var deletedClassSubjectTeachers = (from d in savedClassSubjectTeachers where !vm.ClassSubjectTeachers.Any(x => x.Id == d.Id) select d).ToList();

          foreach (var item in deletedClassSubjectTeachers)
          {
            item.IsActive = false;
            item.UpdatedById = currentUser.Id;
            item.UpdatedOn = DateTime.UtcNow;
            item.EndDate = DateTime.UtcNow;

            schoolDb.ClassSubjectTeachers.Update(item);
          }

          response.Message = "Class detail has been successfully updated.";
        }

        await schoolDb.SaveChangesAsync();

        response.IsSuccess = true;
      }
      catch (Exception ex)
      {
        response.IsSuccess = false;
        response.Message = "Error has been occured while saving the class details.";
      }

      return response;
    }

        public List<ClassViewModel> GetAllClasses()
        {
            var response = new List<ClassViewModel>();

            var query = schoolDb.Classes.Where(u => u.ClassNameId != null);

            var ClassList = query.ToList();

            foreach (var item in ClassList)
            {
                var vm = new ClassViewModel
                {
                    AcademicYearId = item.AcademicYear.Id,
                    AcademicLevelId = item.AcademicLevelId,
                    ClassNameId = item.ClassNameId,
                    ClassTeacherId = item.CreatedById,
                    ClassCategoryId = item.ClassCategory,
                    LanguageStreamId = item.LanguageStream
                };

                response.Add(vm);
            }

            return response;
        }

        public DownloadFileModel downloadClassListReport()
        {
            var classListReport = new ClassListReport();

            byte[] abytes = classListReport.PrepareReport(GetAllClasses());

            var response = new DownloadFileModel();

            response.FileData = abytes;
            response.FileType = "application/pdf";

            return response;
        }

        public class ClassListReport
        {
            #region Declaration
            int _totalColumn = 6;
            Document _document;
            Font _fontStyle;
            iTextSharp.text.pdf.PdfPTable _pdfPTable = new PdfPTable(6);
            iTextSharp.text.pdf.PdfPCell _pdfPCell;
            MemoryStream _memoryStream = new MemoryStream();
            List<ClassViewModel> _classes = new List<ClassViewModel>();
            #endregion

            public byte[] PrepareReport(List<ClassViewModel> response)
            {
                _classes = response;

                #region
                _document = new Document(PageSize.A4, 0f, 0f, 0f, 0f);
                _document.SetPageSize(PageSize.A4);
                _document.SetMargins(20f, 20f, 20f, 20f);
                _pdfPTable.WidthPercentage = 100;
                _pdfPTable.HorizontalAlignment = Element.ALIGN_LEFT;
                _fontStyle = FontFactory.GetFont("TimesNewRoman", 8f, 1);

                iTextSharp.text.pdf.PdfWriter.GetInstance(_document, _memoryStream);
                _document.Open();
                _pdfPTable.SetWidths(new float[] { 50f, 50f, 50f, 50f, 50f, 50f });
                #endregion

                this.ReportHeader();
                this.ReportBody();
                _pdfPTable.HeaderRows = 6;
                _document.Add(_pdfPTable);
                _document.Close();
                return _memoryStream.ToArray();
            }

            private void ReportHeader()
            {
                _fontStyle = FontFactory.GetFont("TimesNewRoman", 18f, 1);
                _pdfPCell = new PdfPCell(new Phrase("Class Report", _fontStyle));
                _pdfPCell.Colspan = _totalColumn;
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.Border = 0;
                _pdfPCell.BackgroundColor = BaseColor.WHITE;
                _pdfPCell.ExtraParagraphSpace = 0;
                _pdfPTable.AddCell(_pdfPCell);
                _pdfPTable.CompleteRow();

                _fontStyle = FontFactory.GetFont("TimesNewRoman", 12f, 1);
                _pdfPCell = new PdfPCell(new Phrase("Class List", _fontStyle));
                _pdfPCell.Colspan = _totalColumn;
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.Border = 0;
                _pdfPCell.BackgroundColor = BaseColor.WHITE;
                _pdfPCell.ExtraParagraphSpace = 0;
                _pdfPTable.AddCell(_pdfPCell);
                _pdfPTable.CompleteRow();
            }

            private void ReportBody()
            {
                #region Table header
                _fontStyle = FontFactory.GetFont("TimesNewRoman", 10f, 1);
                _pdfPCell = new PdfPCell(new Phrase("Academic Year", _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);

                _pdfPCell = new PdfPCell(new Phrase("Academic Level", _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);

                _pdfPCell = new PdfPCell(new Phrase("Class Name", _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);

                _pdfPCell = new PdfPCell(new Phrase("Class Teacher", _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);

                _pdfPCell = new PdfPCell(new Phrase("Class Category", _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);

                _pdfPCell = new PdfPCell(new Phrase("Language Stream", _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);
                _pdfPTable.CompleteRow();
                #endregion

                #region Table Body
                _fontStyle = FontFactory.GetFont("TimesNewRoman", 10f, 0);
                foreach (ClassViewModel vm in _classes)
                {
                    _pdfPCell = new PdfPCell(new Phrase(vm.AcademicYearId.ToString(), _fontStyle));
                    _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //_pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    _pdfPTable.AddCell(_pdfPCell);

                    _pdfPCell = new PdfPCell(new Phrase(vm.AcademicLevelId.ToString(), _fontStyle));
                    _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //_pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    _pdfPTable.AddCell(_pdfPCell);

                    _pdfPCell = new PdfPCell(new Phrase(vm.ClassNameId.ToString(), _fontStyle));
                    _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //_pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    _pdfPTable.AddCell(_pdfPCell);

                    _pdfPCell = new PdfPCell(new Phrase(vm.ClassTeacherId.ToString(), _fontStyle));
                    _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //_pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    _pdfPTable.AddCell(_pdfPCell);

                    _pdfPCell = new PdfPCell(new Phrase(vm.ClassCategoryId.ToString(), _fontStyle));
                    _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //_pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    _pdfPTable.AddCell(_pdfPCell);

                    _pdfPCell = new PdfPCell(new Phrase(vm.LanguageStreamId.ToString(), _fontStyle));
                    _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //_pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    _pdfPTable.AddCell(_pdfPCell); ;
                    _pdfPTable.CompleteRow();
                }
                #endregion
            }
        }
    }
}
