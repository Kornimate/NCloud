using Microsoft.AspNetCore.Mvc;
using NCloud.ViewModels;

namespace NCloud.ViewComponents
{
    /// <summary>
    /// Class to create reuseable UI item for notifications
    /// </summary>
    public class CloudNotification : ViewComponent
    {
        public CloudNotification() { }

        /// <summary>
        /// Method to return view of component
        /// </summary>
        /// <returns>View of the component</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return await Task.FromResult(View("Notification"));
        }
    }
}
