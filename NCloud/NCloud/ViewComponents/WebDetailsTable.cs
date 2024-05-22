using Microsoft.AspNetCore.Mvc;
using NCloud.ViewModels;

namespace NCloud.ViewComponents
{
    /// <summary>
    /// Class to create reuseable UI item for list of file system item in current web sharing state
    /// </summary>
    public class WebDetailsTable : ViewComponent
    {
        public WebDetailsTable() { }

        /// <summary>
        /// Method to return view of component
        /// </summary>
        /// <returns>View of the component</returns>
        public async Task<IViewComponentResult> InvokeAsync(WebDetailsViewModel vm)
        {
            return await Task.FromResult(View("Table", vm));
        }
    }
}
