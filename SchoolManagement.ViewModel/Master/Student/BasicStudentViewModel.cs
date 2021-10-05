using SchoolManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.ViewModel.Master.Student
{
    public class BasicStudentViewModel
    {
        public int Id { get; set; }
        public int AdmissionNo { get; set; }
        public string EmegencyContactNo { get; set; }
        public Gender Gender { get; set; }
        public string GenderName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsActive { get; set; }


        public string FullName { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
        public int Classes { get; set; }
        public string ClassName { get; set; }
        public int AcademicYear { get; set; }
        public int AcademicLevel { get; set; }
    }
}
