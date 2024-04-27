using Microsoft.AspNetCore.Mvc;
using NCloud.ViewModels;

namespace NCloud.ViewComponents
{
    public class SharingDetailsTable : ViewComponent
    {
        public SharingDetailsTable() { }

        public async Task<IViewComponentResult> InvokeAsync(SharingDetailsViewModel vm)
        {
            return await Task.FromResult(View("Table",vm));
        }
    }
}
