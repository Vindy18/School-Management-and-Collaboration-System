﻿using SchoolManagement.ViewModel.Common;
using SchoolManagement.ViewModel.Lesson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Business.Interfaces.LessonData
{
    public interface ILessonAssignmentSubmissionService
    {
        Task<ResponseViewModel> SaveLessonAssignmentSubmission(LessonAssignmentSubmissionViewModel vm, string userName);

        List<LessonAssignmentSubmissionViewModel> GetAllLessonAssignmentSubmissions();
    }
}