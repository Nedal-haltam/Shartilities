//namespace Shartilities
//{
//}
using CliWrap.Buffered;
using CliWrap.EventStream;
using Microsoft.VisualBasic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;

public static class Shartilities
{
    public enum LogType
    {
        INFO, WARNING, ERROR
    }
    public struct Command
    {
        private string[] m_cmd;
        public Command()
        {
            m_cmd = [];
        }
        public Command(string[] cmd)
        {
            m_cmd = [.. cmd];
        }
        public bool IsValid => m_cmd.Length > 0;
        public void Append(string arg)
        {
            m_cmd.Append(arg);
        }
        public string Head() => m_cmd[0];
        public string Args()
        {
            StringBuilder ret = new();
            ret.AppendJoin(' ', m_cmd[1..]);
            return ret.ToString();
        }
        public string Cmd() => Head() + " " + Args();
        public void Reset() => m_cmd = [];
        public async Task<bool> RunSync()
        {
            if (!this.IsValid)
            {
                Logln(LogType.ERROR, $"no command was provided to run (empty command)");
                return false;
            }
            var Command = CliWrap.Cli
              .Wrap(this.Head())
              .WithArguments(this.Args())
              .WithValidation(CliWrap.CommandResultValidation.None);
            var CommandTask = Command.ExecuteBufferedAsync();

            await foreach (var CommandEvent in Command.ListenAsync())
            {
                switch (CommandEvent)
                {
                    case StandardOutputCommandEvent StdOut:
                        Console.WriteLine(StdOut.Text);
                        break;
                    case StandardErrorCommandEvent StdErr:
                        Console.Error.WriteLine(StdErr.Text);
                        break;
                }
            }
            var result = CommandTask.GetAwaiter().GetResult();
            return result.IsSuccess;
        }
        public void RunAsync()
        {
            if (!this.IsValid)
            {
                Logln(LogType.ERROR, $"no command was provided to run (empty command)");
            }

            var command = CliWrap.Cli
                .Wrap(this.Head())
                .WithArguments(this.Args())
                .WithValidation(CliWrap.CommandResultValidation.None);
            var commandTask = command.ExecuteBufferedAsync();

            var listeningTask = Task.Run(async () =>
            {
                await foreach (var commandEvent in command.ListenAsync())
                {
                    switch (commandEvent)
                    {
                        case StandardOutputCommandEvent stdout:
                            Console.WriteLine(stdout.Text);
                            break;
                        case StandardErrorCommandEvent stderr:
                            Console.Error.WriteLine(stderr.Text);
                            break;
                    }
                }
            });
            //var result = await commandTask;
            //await listeningTask;
            //return result.IsSuccess;
        }
    }
    public static void Log(LogType type, string msg, int? ExitCode = null)
    {
        switch (type)
        {
            case LogType.INFO:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"\x1b[32mINFO: {msg}\x1b[0m");
                break;
            case LogType.WARNING:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"\x1b[33mWARNING: {msg}\x1b[0m");
                break;
            case LogType.ERROR:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\x1b[31mERROR: {msg}\x1b[0m");
                break;
            default:
                UNREACHABLE("invalid log type provided");
                return;
        }
        if (ExitCode.HasValue)
            Environment.Exit(ExitCode.Value);
    }
    public static void Logln(LogType type, string msg, int? ExitCode = null)
    {
        Log(type, msg + '\n', ExitCode);
    }
    public static bool ShiftArgs(ref string[] args, out string arg)
    {
        if (args.Length <= 0)
        {
            arg = "";
            return false;
        }
        arg = args[0];
        args = args[1..];
        return true;
    }
    public static void UNREACHABLE(string msg)
    {
        Log(LogType.ERROR, $"UNREACHABLE: {msg}\n");
        Environment.Exit(1);
    }
    public static void TODO(string msg)
    {
        Log(LogType.ERROR, $"TODO: {msg}\n");
        Environment.Exit(1);
    }
    public static int Assert(bool Condition, string msg = "false")
    {
        if (!Condition)
        {
            Log(LogType.ERROR, $"Assert: {msg}\n");
            Environment.Exit(1);
        }
        return 0;
    }
    public static dynamic UNUSED(dynamic foo) => foo;
    public static string ReadFile(string FilePath, int? ExitCode = null)
    {
        if (!File.Exists(FilePath))
            Shartilities.Logln(Shartilities.LogType.ERROR, $"file {FilePath} doesn't exists", 1);
        string ret = "";
        try
        {
            ret = File.ReadAllText(FilePath);
        }
        catch
        {
            Shartilities.Logln(LogType.ERROR, $"could not write to file {FilePath}");
            if (ExitCode.HasValue)
                Environment.Exit(ExitCode.Value);
            return "";
        }
        Shartilities.Logln(LogType.INFO, $"file {FilePath} was read successfully");
        return ret;
    }
    public static bool WriteFile(string FilePath, string contents, int? ExitCode = null)
    {
        string? DirPath = Path.GetDirectoryName(FilePath);
        if (DirPath == null)
        {
            Shartilities.Logln(Shartilities.LogType.ERROR, $"invalid path: {FilePath}", 1);
            return false;
        }
        if (!Directory.Exists(DirPath))
            Directory.CreateDirectory(DirPath);
        try
        {
            File.WriteAllText(FilePath, contents);
        }
        catch
        {
            Shartilities.Logln(LogType.ERROR, $"could not write to file {FilePath}");
            if (ExitCode.HasValue)
                Environment.Exit(ExitCode.Value);
            return false;
        }
        Shartilities.Logln(LogType.INFO, $"file {FilePath} was written successfully");
        return true;
    }
    public static List<string> SplitAndRemoveWhite(string src)
    {
        List<string> words = [.. src.Split(' ')];
        words.RemoveAll(x => string.IsNullOrWhiteSpace(x) || string.IsNullOrEmpty(x));
        return words;
    }
    [RequiresUnreferencedCode("Calls System.Xml.Serialization.XmlSerializer.XmlSerializer(Type)")]
    public static void SaveObject<T>(string filePath, T objectToWrite, bool append = false) where T : new()
    {
        TextWriter? writer = null;
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            writer = new StreamWriter(filePath, append);
            serializer.Serialize(writer, objectToWrite);
        }
        finally
        {
            writer?.Close();
        }
    }
    [RequiresUnreferencedCode("Calls System.Xml.Serialization.XmlSerializer.XmlSerializer(Type)")]
    public static T LoadOject<T>(string filePath) where T : new()
    {
        TextReader? reader = null;
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            reader = new StreamReader(filePath);
            object? o = serializer.Deserialize(reader);
            if (o == null)
                return new();
            return (T)o;
        }
        finally
        {
            reader?.Close();
        }
    }
}
