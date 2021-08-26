﻿using Microsoft.Extensions.Configuration;
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

namespace SchoolManagement.Business.Master
{
    public class ClassService: IClassService
    {
        private readonly MasterDbContext masterDb;
        private readonly SchoolManagementContext schoolDb;
        private readonly IConfiguration config;
        private readonly ICurrentUserService currentUserService;

        public ClassService(MasterDbContext masterDb, SchoolManagementContext schoolDb, IConfiguration config, ICurrentUserService currentUserService)
        {
            this.masterDb = masterDb;
            this.schoolDb = schoolDb;
            this.config = config;
            this.currentUserService = currentUserService;
        }

        public List<ClassViewModel> GetClasses()
        {
            var response = new List<ClassViewModel>();

            var query = schoolDb.Classes.Where(predicate: u => u.ClassNameId != null);

            var ClassList = query.ToList();

            foreach (var item in ClassList)
            {
                var vm = new ClassViewModel
                {
                    ClassNameId = item.ClassNameId,
                    ClassClassName = item.ClassName.Name,
                    AcademicLevelId = item.AcademicLevelId,
                    AcademicLevelName = item.AcademicLevel.Name,
                    AcademicYearId = item.AcademicYearId,
                    Name = item.Name,
                    ClassCategory = item.ClassCategory,
                    ClassCategoryName = item.ClassCategory.ToString(),
                    LanguageStream = item.LanguageStream,
                    LanguageStreamName = item.LanguageStream.ToString(),
                    CreatedOn = item.CreatedOn,
                    CreatedById = item.CreatedById,
                    UpdatedOn = item.UpdatedOn,
                    UpdatedById = item.UpdatedById,
                };

                response.Add(vm);
            }

            return response;
        }

        public async Task <ResponseViewModel> SavaClass(ClassViewModel vm, string userName)
        {
            var response = new ResponseViewModel();

            try
            {
                var currentuser = schoolDb.Users.FirstOrDefault(x => x.Username.ToUpper() == userName.ToUpper());

                var classes = schoolDb.Classes.FirstOrDefault(x => x.ClassNameId == vm.ClassNameId);

                if (classes == null)
                {
                    classes = new Class()
                    {
                        ClassNameId = vm.ClassNameId,
                        AcademicLevelId = vm.AcademicLevelId,
                        AcademicYearId = vm.AcademicYearId,
                        Name = vm.Name,
                        ClassCategory = vm.ClassCategory,
                        LanguageStream = vm.LanguageStream,
                        CreatedOn = DateTime.UtcNow,
                        CreatedById = vm.CreatedById,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedById = vm.UpdatedById
                    };

                    schoolDb.Classes.Add(classes);

                    response.IsSuccess = true;
                    response.Message = "Class is Added Successfull.";
                }
                else
                {
                    classes.Name = vm.Name;
                    classes.ClassCategory = vm.ClassCategory;
                    classes.LanguageStream = vm.LanguageStream;
                    classes.UpdatedOn = DateTime.UtcNow;
                   // classes.UpdatedById = vm.UpdatedById;

                    schoolDb.Classes.Update(classes);
                }

                await schoolDb.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.ToString();
            }

            return response;
        }

        public async Task<ResponseViewModel> DeleteClass(int id)
        {
            var response = new ResponseViewModel();

            try
            {
                var classes = schoolDb.Classes.FirstOrDefault(x => x.ClassNameId == id);

                schoolDb.Classes.Update(classes);
                await schoolDb.SaveChangesAsync();

                response.IsSuccess = true;
                response.Message = "Class Deleted Successfull.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.ToString();
            }

            return response;
        }
    }
}
