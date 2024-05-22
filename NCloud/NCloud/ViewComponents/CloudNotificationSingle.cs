using Microsoft.AspNetCore.Mvc;

namespace NCloud.ViewComponents
{
    /// <summary>
    /// Class to create reuseable UI item for single notification
    /// </summary>
    public class CloudNotificationSingle : ViewComponent
    {
        public CloudNotificationSingle() { }

        /// <summary>
        /// Method to return view of component
        /// </summary>
        /// <returns>View of the component</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return await Task.FromResult(View("NotificationSingle"));
        }
    }
}
