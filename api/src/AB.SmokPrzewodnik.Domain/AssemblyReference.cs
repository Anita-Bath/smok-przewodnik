using System.Reflection;

namespace AB.SmokPrzewodnik.Domain;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
