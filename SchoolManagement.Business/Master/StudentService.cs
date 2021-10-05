using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Configuration;
using SchoolManagement.Business.Interfaces.MasterData;
using SchoolManagement.Data.Data;
using SchoolManagement.Master.Data.Data;
using SchoolManagement.Model;
using SchoolManagement.Util;
using SchoolManagement.Util.Constants.ServiceClassConstants;
using SchoolManagement.ViewModel;
using SchoolManagement.ViewModel.Common;
using SchoolManagement.ViewModel.Master;
using SchoolManagement.ViewModel.Master.Student;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Business.Master
{
    public class StudentService : IStudentService
    {
        private readonly MasterDbContext masterDb;
        private readonly SchoolManagementContext schoolDb;
        private readonly IConfiguration config;
        private readonly ICurrentUserService currentUserService;

        public StudentService(MasterDbContext masterDb, SchoolManagementContext schoolDb, IConfiguration config, ICurrentUserService currentUserService)
        {
            this.masterDb = masterDb;
            this.schoolDb = schoolDb;
            this.config = config;
            this.currentUserService = currentUserService;
        }

        public async Task<ResponseViewModel> DeleteStudent(int id)
        {
            var response = new ResponseViewModel();

            try
            {
                var user = schoolDb.Users.FirstOrDefault(x => x.Id == id);
                var student = schoolDb.Students.FirstOrDefault(a => a.Id == id);
                var userRole = schoolDb.UserRoles.FirstOrDefault(d => d.UserId == id);
                var studentClass = schoolDb.StudentClasses.FirstOrDefault(sc => sc.StudentId == id);

                userRole.IsActive = false;
                schoolDb.UserRoles.Update(userRole);
                await schoolDb.SaveChangesAsync();

                student.IsActive = false;
                schoolDb.Students.Update(student);
                await schoolDb.SaveChangesAsync();

                studentClass.IsActive = false;
                schoolDb.StudentClasses.Update(studentClass);
                response.IsSuccess = true;
                response.Message = StudentServiceConstants.STUDENT_DISABLE_MESSAGE;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.ToString();
            }

            return response;
        }

        public List<DropDownViewModel> GetAllAcademicLevels()
        {
            return schoolDb.AcademicLevels.Where(al => al.IsActive == true)
                .Select(li => new DropDownViewModel() { Id = li.Id, Name = li.Name }).ToList();
        }

        public List<DropDownViewModel> GetAllAcademicYears()
        {
            return schoolDb.AcademicYears.Where(ay => ay.IsActive == true)
                .Select(li => new DropDownViewModel() { Id = li.Id, Name = null }).ToList();
        }

        public List<DropDownViewModel> GetAllClasses()
        {
            return schoolDb.Classes.Where(s => s.IsActive == true)
                .Select(d => new DropDownViewModel() { Id = d.ClassNameId, Name = d.Name }).ToList();
        }

        public List<DropDownViewModel> GetAllGenders()
        {
            var genderList = new List<DropDownViewModel>();

            foreach (var item in Enum.GetValues(typeof(Gender)))
            {
                var listItem = new DropDownViewModel()
                {
                    Id = (int)item,
                    Name = item.ToString()
                };
                genderList.Add(listItem);
            }

            return genderList;
        }

        public List<StudentViewModel> GetAllStudent()
        {
            var response = new List<StudentViewModel>();

            var studentQuery = schoolDb.Students.Where(u => u.IsActive == true);
            var studentList = studentQuery.ToList();

                foreach (var item in studentList)
                {
                    var user = schoolDb.Users.Find(item.Id);
                    var studentClass = schoolDb.StudentClasses.FirstOrDefault(sc => sc.StudentId == item.Id);
                    var classNameSet = schoolDb.Classes.FirstOrDefault(cn => cn.ClassNameId == studentClass.ClassNameId);

                    if (user != null)
                    {
                        var vm = new StudentViewModel
                        {
                            Id = item.Id,
                            AdmissionNo = item.AdmissionNo,
                            EmegencyContactNo = item.EmegencyContactNo2,
                            Gender = item.Gender,
                            GenderName = item.Gender.ToString(),
                            DateOfBirth = (DateTime)item.DateOfBirth,
                            IsActive = item.IsActive,
                            FullName = user.FullName,
                            Email = user.Email,
                            Password = user.Password,
                            MobileNo = user.MobileNo,
                            Username = user.Username,
                            Address = user.Address,
                            ClassName = classNameSet.Name,
                            Classes = classNameSet.ClassNameId,
                            AcademicYear = studentClass.AcademicYearId,
                            AcademicLevel = studentClass.AcademicLevelId
                        };
                        response.Add(vm);
                    }
                }

            return response;
        }


        public async Task<ResponseViewModel> SaveStudent(StudentViewModel vm, string userName)
        {
            var response = new ResponseViewModel();

            try
            {
                var loggedInUser = currentUserService.GetUserByUsername(userName);

                var student = schoolDb.Students.FirstOrDefault(a => a.Id == vm.Id);

                if (student == null)
                {
                    //Add student as a user
                    var user = new User()
                    {
                        Username = vm.Email,
                        Email = vm.Email,
                        FullName = vm.FullName,
                        MobileNo = vm.MobileNo,
                        Password = CustomPasswordHasher.GenerateHash(vm.Password),
                        IsActive = false,
                        CreatedById = loggedInUser.Id,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow,
                        Address = vm.Address,
                        LastLoginDate = DateTime.UtcNow,
                        LoginSessionId = 0
                    };
                    schoolDb.Users.Add(user);
                    await schoolDb.SaveChangesAsync();

                    //get inserted user id 
                    var insertedId = schoolDb.Users.Max(i => i.Id);

                    //Add student role to UserRoles table
                    var roleItems = schoolDb.Roles.Where(s => s.Name == "student");

                    foreach (var item in roleItems)
                    {
                        var role = new Role()
                        {
                            Id = item.Id
                        };

                        var userRole = new UserRole()
                        {
                            UserId = insertedId,
                            RoleId = role.Id,
                            IsActive = true,
                            CreatedOn = DateTime.UtcNow,
                            UpdatedOn = DateTime.UtcNow,
                            CreatedById = loggedInUser.Id,
                            UpdatedById = loggedInUser.Id
                        };
                        schoolDb.UserRoles.Add(userRole);
                        await schoolDb.SaveChangesAsync();
                    }

                    //Add student to Student table
                    student = new Student()
                    {
                        Id = insertedId,
                        AdmissionNo = vm.AdmissionNo,
                        EmegencyContactNo1 = user.MobileNo,
                        EmegencyContactNo2 = vm.EmegencyContactNo,
                        Gender = vm.Gender,
                        DateOfBirth = vm.DateOfBirth,
                        IsActive = true,
                        CreateOn = DateTime.UtcNow,
                        CreatedById = loggedInUser.Id,
                        UpdateOn = DateTime.UtcNow,
                        UpdatedById = loggedInUser.Id,
                    };

                    schoolDb.Students.Add(student);
                    await schoolDb.SaveChangesAsync();

                    //student details to StudentClass table
                    var studentClass = new StudentClass()
                    {
                        StudentId = insertedId,
                        ClassNameId = vm.Classes,
                        AcademicLevelId = vm.AcademicLevel,
                        AcademicYearId = vm.AcademicYear,
                        IsActive = true
                    };
                    schoolDb.StudentClasses.Add(studentClass);

                    response.IsSuccess = true;
                    response.Message = StudentServiceConstants.NEW_STUDENT_ADD_SUCCESS_MESSAGE;
                }  
                else
                {
                    student.AdmissionNo = vm.AdmissionNo;
                    student.EmegencyContactNo1 = vm.MobileNo;
                    student.EmegencyContactNo2 = vm.EmegencyContactNo;
                    student.Gender = vm.Gender;
                    student.IsActive = true;
                    student.UpdatedById = loggedInUser.Id;
                    student.UpdateOn = DateTime.UtcNow;
                    student.DateOfBirth = vm.DateOfBirth;

                    schoolDb.Students.Update(student);
                    await schoolDb.SaveChangesAsync();

                    var user = schoolDb.Users.Find(student.Id);
                    user.FullName = vm.FullName;
                    user.Address = vm.Address;
                    user.UpdatedById = loggedInUser.Id;
                    user.Email = vm.Email;
                    user.MobileNo = vm.MobileNo;
                    user.Username = vm.Username;
                    user.Password = CustomPasswordHasher.GenerateHash(vm.Password);
                    user.UpdatedOn = DateTime.UtcNow;

                    schoolDb.Users.Update(user);
                    await schoolDb.SaveChangesAsync();

                    var studentClass = schoolDb.StudentClasses.First(r => r.StudentId == student.Id);
                    studentClass.ClassNameId = vm.Classes;
                    studentClass.AcademicYearId = vm.AcademicYear;
                    studentClass.AcademicLevelId = vm.AcademicLevel;

                    schoolDb.StudentClasses.Update(studentClass);

                    response.IsSuccess = true;
                    response.Message = StudentServiceConstants.STUDENT_UPDATE_MESSAGE;
                }

                await schoolDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.ToString();
            }
            return response;
        }

        public DownloadFileModel DownloadStudentReport()
        {
            var studentReport = new StudentReport();

            byte[] abytes = studentReport.PrepareReport(GetAllStudent());

            var response = new DownloadFileModel();
            response.FileData = abytes;
            response.FileType = "application/pdf";
            response.FileName = "StudentReport";

            return response;
        }
    }

    public class StudentReport
    {
        #region Declaration
        int totalColumns = 4;
        Document document;
        Font fontStyle;
        PdfPTable pdfTable = new PdfPTable(4);
        PdfPCell pdfCell;
        MemoryStream memoryStream = new MemoryStream();
        List<StudentViewModel> studentsList = new List<StudentViewModel>();
        #endregion

        public byte[] PrepareReport(List<StudentViewModel> students)
        {
            studentsList = students;

            #region
            document = new Document(PageSize.A4, 0f, 0f, 0f, 0f);
            document.SetPageSize(PageSize.A4);
            document.SetMargins(20f, 20f, 20f, 20f);
            pdfTable.WidthPercentage = 100;
            pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
            fontStyle = FontFactory.GetFont("Tahoma", 8f, 1);
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();
            //admission, student name, phone number, address
            pdfTable.SetWidths(new float[] { 20f, 90f, 40f, 40f });
            #endregion

            this.ReportHeader();
            this.ReportBody(studentsList);
            pdfTable.HeaderRows = 2;
            document.Add(pdfTable);
            document.Close();
            return memoryStream.ToArray();
        }

        private void ReportHeader()
        {
            fontStyle = FontFactory.GetFont("Tahoma", 11f, 1);
            pdfCell = new PdfPCell(new Phrase("SolisTech Solutions", fontStyle));
            pdfCell.Colspan = totalColumns;
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            pdfCell.BackgroundColor = BaseColor.WHITE;
            pdfCell.ExtraParagraphSpace = 0;
            pdfTable.AddCell(pdfCell);
            pdfTable.CompleteRow();

            fontStyle = FontFactory.GetFont("Tahoma", 9f, 1);
            pdfCell = new PdfPCell(new Phrase("Students List Report", fontStyle));
            pdfCell.Colspan = totalColumns;
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            pdfCell.BackgroundColor = BaseColor.WHITE;
            pdfCell.ExtraParagraphSpace = 0;
            pdfTable.AddCell(pdfCell);
            pdfTable.CompleteRow();
        }

        private void ReportBody(List<StudentViewModel> students)
        {
            #region Table header
            fontStyle = FontFactory.GetFont("Tahoma", 8f, 1);
            pdfCell = new PdfPCell(new Phrase("Admis", fontStyle));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            pdfTable.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Phrase("Student name", fontStyle));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            pdfTable.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Phrase("Contact no:", fontStyle));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            pdfTable.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Phrase("Address", fontStyle));
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            pdfTable.AddCell(pdfCell);
            pdfTable.CompleteRow();
            #endregion

            #region Table body
            fontStyle = FontFactory.GetFont("Tahoma", 8f, 0);
            foreach (var item in students)
            {
                pdfCell = new PdfPCell(new Phrase(item.AdmissionNo.ToString(), fontStyle));
                pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                pdfCell.BackgroundColor = BaseColor.WHITE;
                pdfTable.AddCell(pdfCell);

                pdfCell = new PdfPCell(new Phrase(item.FullName, fontStyle));
                pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                pdfCell.BackgroundColor = BaseColor.WHITE;
                pdfTable.AddCell(pdfCell);

                pdfCell = new PdfPCell(new Phrase(item.EmegencyContactNo, fontStyle));
                pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                pdfCell.BackgroundColor = BaseColor.WHITE;
                pdfTable.AddCell(pdfCell);

                pdfCell = new PdfPCell(new Phrase(item.Address, fontStyle));
                pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
                pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                pdfCell.BackgroundColor = BaseColor.WHITE;
                pdfTable.AddCell(pdfCell);
                pdfTable.CompleteRow();
            }
            #endregion
        }
    }
}
