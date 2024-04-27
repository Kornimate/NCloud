using Microsoft.AspNetCore.Mvc;
using NCloud.ViewModels;

namespace NCloud.ViewComponents
{
    public class CloudNotification : ViewComponent
    {
        public CloudNotification() { }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return await Task.FromResult(View("Notification"));
        }
    }
}
