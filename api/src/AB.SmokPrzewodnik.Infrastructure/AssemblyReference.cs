using System.Reflection;

namespace AB.SmokPrzewodnik.Infrastructure;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
