using NCloud.ConstantData;
using NCloud.Models;

namespace NCloud.Services
{
    /// <summary>
    /// Class to create special redirection string for data passing
    /// </summary>
    public static class RedirectionManager
    {
        /// <summary>
        /// Static method to parse special redirection string to controller and action
        /// </summary>
        /// <param name="redirectData">Special redirection string created by this class</param>
        /// <returns>Controller and action wrapped in RedirectionManagerResult class</returns>
        public static RedirectManagerResult CreateRedirectionAction(string redirectData)
        {
            try
            {
                string[]? redirectControllerAndAction = redirectData.Split(Constants.ControllerDataSeparator, StringSplitOptions.RemoveEmptyEntries);

                return new RedirectManagerResult(redirectControllerAndAction[0], redirectControllerAndAction[1]);
            }
            catch (Exception)
            {
                return null!;
            }
        }

        /// <summary>
        /// Static method to create special redirection string
        /// </summary>
        /// <param name="controller">Name of controller for redirection</param>
        /// <param name="action">Name of action for redirection</param>
        /// <returns>Special redirection string</returns>
        public static string CreateRedirectionString(string controller, string action)
        {
            try
            {
                return String.Join(Constants.ControllerDataSeparator, controller, action);
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
    }
}
