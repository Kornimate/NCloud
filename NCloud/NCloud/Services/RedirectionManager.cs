using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
using NCloud.Models;

namespace NCloud.Services
{
    public static class RedirectionManager
    {
        public static RedirectManagerResult CreateRedirectionAction(string redirectData)
        {
            string[]? redirectControllerAndAction = redirectData.Split(Constants.ControllerDataSeparator, StringSplitOptions.RemoveEmptyEntries);

            return new RedirectManagerResult(redirectControllerAndAction[0], redirectControllerAndAction[1]);
        }

        public static string CreateRedirectionString(string controller, string action)
        {
            return String.Join(Constants.ControllerDataSeparator, controller, action);
        }
    }
}
