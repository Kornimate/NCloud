using Castle.Core;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services.Exceptions;
using System.Text;
using System.Text.RegularExpressions;

namespace NCloud.Services
{
    /// <summary>
    /// Class to tokenize string from cloud terminal
    /// </summary>
    public class CloudTerminalTokenizationManager
    {
        /// <summary>
        /// Static method to tokenize a command into command word and arguments
        /// </summary>
        /// <param name="command">Command to pe tokenized</param>
        /// <param name="userId">Id of requester user</param>
        /// <returns>Pair containing the command word and arguments</returns>
        public static Pair<string, List<string>> Tokenize(string command, string userId)
        {

            string commandWord = command;
            string parametersString = String.Empty;

            int commandParameterSeparator = command.IndexOf(Constants.TerminalWhiteSpace);

            if (commandParameterSeparator != -1)
            {
                commandWord = command.Substring(0, commandParameterSeparator).Trim().TrimStart(Constants.SingleLineCommandMarker);
                parametersString = command.Substring(commandParameterSeparator + 1).Trim();
            }

            List<string> parameters = TokenizeByRules(parametersString);

            return new Pair<string, List<string>>(commandWord, parameters);
        }

        /// <summary>
        /// MEthod to tokenize the argument of command
        /// </summary>
        /// <param name="parametersString">The string of arguments</param>
        /// <returns>The list of arguments aws strings</returns>
        private static List<string> TokenizeByRules(string parametersString)
        {
            StringBuilder sb = new StringBuilder();

            List<string> parameters = new List<string>();

            char[] splitters = new char[] { Constants.TerminalStringMarker, Constants.TerminalWhiteSpace };

            char nextSplitter = Constants.TerminalWhiteSpace;

            foreach (char c in parametersString)
            {
                if (splitters.Contains(c))
                {
                    if (c == nextSplitter)
                    {
                        string param = sb.ToString().Trim().Replace(Constants.PathSeparator, Path.DirectorySeparatorChar);

                        if (param != String.Empty)
                        {
                            parameters.Add(param);
                        }

                        sb.Clear();

                        nextSplitter = Constants.TerminalWhiteSpace;
                    }
                    else if (c != Constants.TerminalStringMarker)
                    {
                        sb.Append(c);
                    }
                    else if (c == Constants.TerminalStringMarker)
                    {
                        nextSplitter = c;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            string last = sb.ToString().Trim().Replace(Constants.PathSeparator, Path.DirectorySeparatorChar);

            if (last != String.Empty)
            {
                parameters.Add(last);
            }

            return parameters;
        }

        /// <summary>
        /// Static methdo to filter relative path elements (. and .. as relative path marking strings)
        /// </summary>
        /// <param name="path">Relative path in app</param>
        /// <returns>The filtered path</returns>
        public static string NormalizeCommandPath(string path)
        {
            string newPath = new string(path);

            if (newPath.StartsWith(Path.DirectorySeparatorChar))
                newPath = newPath[1..];

            while (newPath.StartsWith($"{Constants.CommandCurrentPathMarker}{Path.DirectorySeparatorChar}"))
                newPath = newPath[2..];

            newPath = newPath.Replace($"{Path.DirectorySeparatorChar}{Constants.CommandCurrentPathMarker}{Path.DirectorySeparatorChar}", $"{Path.DirectorySeparatorChar}");

            if (newPath.EndsWith(Constants.CommandCurrentPathMarker) && !newPath.EndsWith(Constants.DirectoryBack))
                newPath = newPath[..^1];

            if (newPath.EndsWith(Path.DirectorySeparatorChar))
                newPath = newPath[..^1];

            return newPath;
        }

        /// <summary>
        /// Checks correctness of command related to specified rules
        /// </summary>
        /// <param name="command">The string of command</param>
        /// <exception cref="CloudFunctionStopException">Throws if there is problem with command</exception>
        public static void CheckCorrectnessOfCommand(string command)
        {
            if (!Regex.IsMatch(command, Constants.CommandRegex))
                throw new CloudFunctionStopException("invalid data in path");

            if (command.Count(x => x == Constants.TerminalStringMarker) % 2 != 0)
                throw new CloudFunctionStopException("string markers incorrect");

        }

        /// <summary>
        /// Checks correctness of single line command related to specified rules
        /// </summary>
        /// <param name="command">The string of command</param>
        /// <exception cref="CloudFunctionStopException">Throws if there is problem with command</exception>
        public static void CheckCorrectnessOfSingleLineCommand(string command)
        {
            if (!command.StartsWith(Constants.SingleLineCommandMarker))
            {
                throw new CloudFunctionStopException("command should start with '@'");
            }

            CheckCorrectnessOfCommand(command);
        }

        /// <summary>
        /// Method to check the correctness of client side command
        /// </summary>
        /// <param name="command">The string of command</param>
        /// <param name="paramCount">The parameters of command</param>
        /// <param name="commands">The list of client side commands</param>
        /// <exception cref="CloudFunctionStopException">Throws if there is problem with command</exception>
        public static void CheckClientSideCommandSyntax(string command, int paramCount, List<ClientSideCommandContainer> commands)
        {
            var commandItem = commands.FirstOrDefault(x => x.Command == command) ?? throw new ArgumentException($"no command with name: {command}");

            if (commandItem.Parameters != paramCount)
                throw new CloudFunctionStopException("wrong number of parameters");
        }
    }
}
