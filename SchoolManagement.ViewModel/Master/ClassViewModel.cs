﻿using SchoolManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.ViewModel.Master
{
	public class ClassViewModel
	{
		public int ClassNameId { get; set; }
        public string ClassClassName { get; set; }
        public int AcademicLevelId { get; set; }
        public string AcademicLevelName { get; set; }
        public int AcademicYearId { get; set; }
		public string Name { get; set; }
		public ClassCategory ClassCategory { get; set; }
        public string ClassCategoryName { get; set; }
        public LanguageStream LanguageStream { get; set; }
        public string LanguageStreamName { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedById { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int UpdatedById { get; set; }
    }
}