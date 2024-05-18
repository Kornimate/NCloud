using Microsoft.AspNetCore.Mvc;
using NCloud.ViewModels;

namespace NCloud.ViewComponents
{
    /// <summary>
    /// Class to create reuseable UI item for list of file system item in current state
    /// </summary>
    public class DriveDetailsTable : ViewComponent
    {
        public DriveDetailsTable() { }

        /// <summary>
        /// Method to return view of component
        /// </summary>
        /// <returns>View of the component</returns>
        public async Task<IViewComponentResult> InvokeAsync(DriveDetailsViewModel vm)
        {
            return await Task.FromResult(View("Table",vm));
        }
    }
}
