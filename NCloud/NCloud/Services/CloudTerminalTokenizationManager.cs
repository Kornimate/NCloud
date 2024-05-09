using Castle.Core;
using NCloud.ConstantData;
using NCloud.Models;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace NCloud.Services
{
    public class CloudTerminalTokenizationManager
    {
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

                    if (c == Constants.TerminalStringMarker)
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

        public static void CheckCorrectnessOfCommand(string command)
        {
            if (!Regex.IsMatch(command, Constants.CommandRegex))
                throw new InvalidDataException("invalid data in path");

            if (command.Count(x => x == Constants.TerminalStringMarker) % 2 != 0)
                throw new InvalidDataException("string markers incorrect");

        }

        public static void CheckCorrectnessOfSingleLineCommand(string command)
        {
            if (!command.StartsWith(Constants.SingleLineCommandMarker))
            {
                throw new InvalidDataException("command should start with '@'");
            }

            CheckCorrectnessOfCommand(command);
        }

        public static void CheckClientSideCommandSyntax(string command, int paramCount, List<ClientSideCommandContainer> commands)
        {
            var commandItem = commands.FirstOrDefault(x => x.Command == command) ?? throw new InvalidOperationException($"no command with name: {command}");

            if (commandItem.Parameters != paramCount)
                throw new InvalidDataException("wrong number of parameters");
        }
    }
}
