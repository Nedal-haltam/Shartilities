//namespace Shartilities
//{
//}
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

public static class Shartilities
{
    public enum LogType
    {
        INFO, WARNING, ERROR
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
    public static void Logln(LogType type, string msg, int? ExitCode)
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
