using Microsoft.AspNetCore.Mvc;
using NCloud.ViewModels;

namespace NCloud.ViewComponents
{
    public class SharedDetailsTable : ViewComponent
    {
        public SharedDetailsTable() { }

        public async Task<IViewComponentResult> InvokeAsync(DriveDetailsViewModel vm)
        {
            return await Task.FromResult(View("Table",vm));
        }
    }
}
