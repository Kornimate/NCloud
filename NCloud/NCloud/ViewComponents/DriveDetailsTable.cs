using Microsoft.AspNetCore.Mvc;
using NCloud.ViewModels;

namespace NCloud.ViewComponents
{
    public class DriveDetailsTable : ViewComponent
    {
        public DriveDetailsTable() { }

        public async Task<IViewComponentResult> InvokeAsync(DriveDetailsViewModel vm)
        {
            return await Task.FromResult(View("Table",vm));
        }
    }
}
