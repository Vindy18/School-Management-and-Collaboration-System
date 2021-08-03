﻿using Microsoft.Extensions.Configuration;
using SchoolManagement.Business.Interfaces.MasterData;
using SchoolManagement.Data.Data;
using SchoolManagement.Master.Data.Data;
using SchoolManagement.Model;
using SchoolManagement.ViewModel.Common;
using SchoolManagement.ViewModel.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Business.Master
{
    public class SubjectService : ISubjectService
    {
        private readonly MasterDbContext masterDb;
        private readonly SchoolManagementContext schoolDb;
        private readonly IConfiguration config;
        private readonly ICurrentUserService currentUserService;

        public SubjectService(MasterDbContext masterDb, SchoolManagementContext schoolDb, IConfiguration config, ICurrentUserService currentUserService)//ctor and press double tab
        {
            this.masterDb = masterDb;
            this.schoolDb = schoolDb;
            this.config = config;
            this.currentUserService = currentUserService;

        }

        public List<SubjectViewModel> GetAllSubjects()
        {
            var response = new List<SubjectViewModel>();

            var query = schoolDb.Subjects.Where(u => u.IsActive == true);

            var SubjectList = query.ToList();

            foreach (var subject in SubjectList)
            {
                var vm = new SubjectViewModel
                {
                    Id = subject.Id,
                    Name = subject.Name,
                    SubjectCode = subject.SubjectCode,
                    SubjectCategory = subject.SubjectCategory,
                    IsParentBasketSubject = subject.IsParentBasketSubject,
                    IsBuscketSubject = subject.IsBuscketSubject,
                    ParentBasketSubjectId = subject.ParentBasketSubjectId,
                    SubjectStreamId = subject.SubjectStreamId,
                    IsActive = subject.IsActive,
                };

                response.Add(vm);

            }

            return response;

        }

        public async Task<ResponseViewModel> SaveSubject(SubjectViewModel vm, string userName)
        {
            var response = new ResponseViewModel();
            try
            {
                var loggedInUser = currentUserService.GetUserByUsername(userName);

                var subject = schoolDb.Subjects.FirstOrDefault(x => x.Id == vm.Id);

                if(subject == null)
                {
                    subject = new Subject()
                    {
                        Id = vm.Id,
                        Name = vm.Name,
                        SubjectCode = vm.SubjectCode,
                        SubjectCategory = vm.SubjectCategory,
                        IsParentBasketSubject = vm.IsParentBasketSubject,
                        IsBuscketSubject = vm.IsBuscketSubject,
                        ParentBasketSubjectId =vm.ParentBasketSubjectId,
                        SubjectStreamId = vm.SubjectStreamId,
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        CreatedById = loggedInUser.Id,
                        UpdatedOn = DateTime.UtcNow,
                        UpdatedById = loggedInUser.Id,
                     };
                    
                    schoolDb.Subjects.Add(subject);

                    response.IsSuccess = true;
                    response.Message = "Subject Add Successfull.";
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Subject Add Unsuccessfull.";
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
    }
}