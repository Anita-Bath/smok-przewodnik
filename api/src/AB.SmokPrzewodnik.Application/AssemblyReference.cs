using System.Reflection;

namespace AB.SmokPrzewodnik.Application;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
