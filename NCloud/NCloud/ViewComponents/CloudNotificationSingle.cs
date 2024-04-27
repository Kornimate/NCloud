using Microsoft.AspNetCore.Mvc;
using NCloud.ViewModels;

namespace NCloud.ViewComponents
{
    public class CloudNotificationSingle : ViewComponent
    {
        public CloudNotificationSingle() { }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return await Task.FromResult(View("NotificationSingle"));
        }
    }
}
