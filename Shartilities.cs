//namespace Shartilities
//{
//}
public static class Shartilities
{
    public enum LogType
    {
        INFO, WARNING, ERROR, NORMAL
    }
    public static void Log(LogType type, string msg)
    {
        ConsoleColor before = Console.ForegroundColor;
        string head;
        switch (type)
        {
            case LogType.INFO:
                Console.ForegroundColor = ConsoleColor.Green;
                head = "INFO: ";
                break;
            case LogType.WARNING:
                Console.ForegroundColor = ConsoleColor.Yellow;
                head = "WARNING: ";
                break;
            case LogType.ERROR:
                Console.ForegroundColor = ConsoleColor.Red;
                head = "ERROR: ";
                break;
            case LogType.NORMAL:
                head = "";
                break;
            default:
                UNREACHABLE("Log");
                return;
        }
        Console.Write(head + msg);
        Console.ForegroundColor = before;
    }
    public static string ShifArgs(ref string[] args, string msg)
    {
        if (args.Length <= 0)
        {
            Log(LogType.ERROR, msg);
            Environment.Exit(1);
        }
        string arg = args[0];
        args = args[1..];
        return arg;
    }
    public static void UNREACHABLE(string msg)
    {
        Log(LogType.ERROR, $"UNREACHABLE: {msg}\n");
        Environment.Exit(1);
    }

    public static List<string> SplitAndRemoveWhite(string src)
    {
        List<string> words = [.. src.Split(' ')];
        words.RemoveAll(x => string.IsNullOrWhiteSpace(x) || string.IsNullOrEmpty(x));
        return words;
    }
}
