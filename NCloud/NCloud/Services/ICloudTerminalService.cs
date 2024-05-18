using Castle.Core;
using NCloud.Models;
using NCloud.Users;

namespace NCloud.Services
{
    /// <summary>
    /// Interface to handle cloud terminal requests
    /// </summary>
    public interface ICloudTerminalService
    {
        /// <summary>
        /// Method to execute command line written action
        /// </summary>
        /// <param name="commandWord">The command word used in command</param>
        /// <param name="parameters">The parameters used in commands</param>
        /// <param name="pathData">The current state from session</param>
        /// <param name="user">The current logged in user</param>
        /// <returns>A tuple representing output actions</returns>
        Task<(bool, string, object?, bool)> Execute(string commandWord, List<string> parameters, CloudPathData pathData, CloudUser user);

        /// <summary>
        /// Getter method to get all server side commands list of objects
        /// </summary>
        /// <returns>List of strings of command names</returns>
        List<string> GetServerSideCommands();

        /// <summary>
        /// Getter method to get all client side commands list of strings
        /// </summary>
        /// <returns>List of strings of command names</returns>
        List<string> GetClientSideCommands();

        /// <summary>
        /// Getter method to get all client side commands list of objects
        /// </summary>
        /// <returns>List of object representing commands and their properties</returns>
        List<ClientSideCommandContainer> GetClientSideCommandsObjectList();

        /// <summary>
        /// Getter method to get every command regardless of server and client side execution
        /// </summary>
        /// <returns>List of strings of command names</returns>
        List<string> GetCommands();

        /// <summary>
        /// Method to get details of url to be generated for specified client side command
        /// </summary>
        /// <param name="command">Command word</param>
        /// <param name="parameters">Command parameters</param>
        /// <param name="pathData">Curren state in session</param>
        /// <returns>UrlGenerationResult with url data for url generation</returns>
        Task<UrlGenerationResult> GetClientSideCommandUrlDetails(string command, List<string> parameters, CloudPathData pathData);
    }
}
