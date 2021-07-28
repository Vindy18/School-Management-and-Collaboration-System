﻿using SchoolManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.ViewModel.Master
{
    class ClassViewModel
    {
		public string ClassNameId { get; set; }
		public string AcademicLevelId { get; set; }
		public string AcademicYearId { get; set; }
		public string Name { get; set; }
		public List<string> ClassCategory { get; set; }
		public List<string> LanguageStream { get; set; }
		public bool IsActive { get; set; }
		public string Description { get; set; }
		public DateTime CreatedOn { get; set; }
		public int? CreatedById { get; set; }
		public DateTime UpdatedOn { get; set; }
		public int? UpdatedById { get; set; }
	}
}
