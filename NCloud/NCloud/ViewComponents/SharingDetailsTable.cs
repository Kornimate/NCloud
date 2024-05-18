using Microsoft.AspNetCore.Mvc;
using NCloud.ViewModels;

namespace NCloud.ViewComponents
{
    /// <summary>
    /// Class to create reuseable UI item for list of file system item in current sharing state
    /// </summary>
    public class SharingDetailsTable : ViewComponent
    {
        public SharingDetailsTable() { }

        /// <summary>
        /// Method to return view of component
        /// </summary>
        /// <returns>View of the component</returns>
        public async Task<IViewComponentResult> InvokeAsync(SharingDetailsViewModel vm)
        {
            return await Task.FromResult(View("Table",vm));
        }
    }
}
