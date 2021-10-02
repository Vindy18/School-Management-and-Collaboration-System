using SchoolManagement.ViewModel;
using SchoolManagement.ViewModel.Common;
using SchoolManagement.ViewModel.Master;
using SchoolManagement.ViewModel.Master.ClassName;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Business.Interfaces.MasterData
{
    public interface IClassNameService
    {
        List<ClassNameViewModel> GetClassNames();
        Task<ResponseViewModel> SavaClassName(ClassNameViewModel classNameVM, string userName);
        Task<ResponseViewModel> DeleteClassName(int id);
        PaginatedItemsViewModel<BasicClassNameViewModel> GetClassNameList(string searchText, int currentPage, int pageSize);
    }
}