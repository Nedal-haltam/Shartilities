# Shartilities
Simple C# utilities library that is constantly changing

## Getting Started
When you add it as a project reference in you main project. You don't have to do
```csharp
using Shartilities;
```
you can just start using it like this
```csharp
namespace ProjectName
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Shartilities.Log(Shartilities.LogType.INFO, "Hello World\n");
        }
    }
}
```
NOTE: you can see the source code of course to know how to use it