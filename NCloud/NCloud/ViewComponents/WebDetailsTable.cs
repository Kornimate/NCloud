using Microsoft.AspNetCore.Mvc;
using NCloud.ViewModels;

namespace NCloud.ViewComponents
{
    public class WebDetailsTable : ViewComponent
    {
        public WebDetailsTable() { }

        public async Task<IViewComponentResult> InvokeAsync(WebDetailsViewModel vm)
        {
            return await Task.FromResult(View("Table",vm));
        }
    }
}
