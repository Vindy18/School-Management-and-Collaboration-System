using Microsoft.Extensions.Configuration;
using SchoolManagement.Business.Interfaces.MasterData;
using SchoolManagement.Data.Data;
using SchoolManagement.Master.Data.Data;
using SchoolManagement.Model;
using SchoolManagement.Util;
using SchoolManagement.ViewModel.Master;
using SchoolManagement.ViewModel.Common;
using SchoolManagement.ViewModel.Master.ClassName;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolManagement.Util.Constants.ServiceClassConstants;
using SchoolManagement.ViewModel;

namespace SchoolManagement.Business.Master
{
    public class ClassNameService: IClassNameService
    {
        private readonly MasterDbContext masterDb;
        private readonly SchoolManagementContext schoolDb;
        private readonly IConfiguration config;
        private readonly ICurrentUserService currentUserService;

        public ClassNameService(MasterDbContext masterDb, SchoolManagementContext schoolDb, IConfiguration config, ICurrentUserService currentUserService)
        {
            this.masterDb = masterDb;
            this.schoolDb = schoolDb;
            this.config = config;
            this.currentUserService = currentUserService;
        }

        public List<ClassNameViewModel> GetClassNames()
        {
            var response = new List<ClassNameViewModel>();

            var query = schoolDb.ClassNames.Where(u => u.IsActive == true);

            var ClassNameList = query.ToList();

            foreach (var item in ClassNameList)
            {
                var vm = new ClassNameViewModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    IsActive = item.IsActive,
                    CreatedOn = item.CreatedOn,
                    CreatedById = item.CreatedById,
                    CreatedByName = item.CreatedBy.FullName,
                    UpdatedOn = item.UpdatedOn,
                    UpdatedById = item.UpdatedById,
                    UpdatedByName = item.UpdatedBy.FullName,
                };

                response.Add(vm);
            }

            return response;
        }

        public async Task <ResponseViewModel> SavaClassName(ClassNameViewModel vm, string userName)
        {
            var response = new ResponseViewModel();

            try
            {
                var currentuser = currentUserService.GetUserByUsername(userName);

                var className = schoolDb.ClassNames.FirstOrDefault(cn => cn.Id == vm.Id);

                if (className == null)
                {
                    className = new ClassName()
                    {
                        Id = vm.Id,
                        Name = vm.Name,
                        Description = vm.Description,
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        CreatedById = currentuser.Id,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedById = currentuser.Id,
                    };

                    schoolDb.ClassNames.Add(className);

                    response.IsSuccess = true;
                    response.Message = ClassNameServiceConstants.NEW_CLASS_NAME_SAVE_SUCCESS_MESSAGE;
                }
                else
                {
                    className.Name = vm.Name;
                    className.Description = vm.Description;
                    className.IsActive = true;
                    className.UpdatedOn = DateTime.UtcNow;
                    className.UpdatedById = currentuser.Id;

                    schoolDb.ClassNames.Update(className);

                    response.IsSuccess = true;
                    response.Message = ClassNameServiceConstants.EXISTING_CLASS_NAME_SAVE_SUCCESS_MESSAGE;
                }

                await schoolDb.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ClassNameServiceConstants.CLASS_NAME_SAVE_EXCEPTION_MESSAGE;
            }

            return response;
        }

        public async Task<ResponseViewModel> DeleteClassName(int id)
        {
            var response = new ResponseViewModel();

            try
            {
                var className = schoolDb.ClassNames.FirstOrDefault(cn => cn.Id == id);

                className.IsActive = false;

                schoolDb.ClassNames.Update(className);
                await schoolDb.SaveChangesAsync();

                response.IsSuccess = true;
                response.Message = ClassNameServiceConstants.CLASS_NAME_DELETE_SUCCESS_MESSAGE;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ClassNameServiceConstants.CLASS_NAME_DELETE_EXCEPTION_MESSAGE;
            }

            return response;
        }

        public PaginatedItemsViewModel<BasicClassNameViewModel> GetClassNameList(string searchText, int currentPage, int pageSize)
        {
            int totalRecordCount = 0;
            double totalPages = 0;
            int totalPageCount = 0;

            var vmu = new List<BasicClassNameViewModel>();

            var classNames = schoolDb.ClassNames.OrderBy(x => x.Id);
            //var classNames = schoolDb.ClassNames.Where(x => x.IsActive == true).OrderBy(s => s.Name);

            if (!string.IsNullOrEmpty(searchText))
            {
                classNames = classNames.Where(x => x.Name.Contains(searchText)).OrderBy(s => s.Id);
            }

            totalRecordCount = classNames.Count();
            totalPages = (double)totalRecordCount / pageSize;
            totalPageCount = (int)Math.Ceiling(totalPages);

            var classNameList = classNames.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

            var classNameAcademicLevel = new List<DropDownViewModel>();

            classNameList.ForEach(classNames =>
            {
                var vm = new BasicClassNameViewModel()
                {
                    Id = classNames.Id,
                    Name = classNames.Name,
                    Description = classNames.Description,
                    CreatedByName = classNames.CreatedBy.FullName,
                    CreatedOn = classNames.CreatedOn,
                    UpdatedByName = classNames.UpdatedBy.FullName,
                    UpdatedOn = classNames.UpdatedOn,

                };
                vmu.Add(vm);
            });

            var container = new PaginatedItemsViewModel<BasicClassNameViewModel>(currentPage, pageSize, totalPageCount, totalRecordCount, vmu);

            return container;
        }
    }
}