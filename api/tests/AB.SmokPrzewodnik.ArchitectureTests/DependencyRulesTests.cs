using System.Reflection;
using Xunit;
using ApplicationAssembly = AB.SmokPrzewodnik.Application.AssemblyReference;
using DomainAssembly = AB.SmokPrzewodnik.Domain.AssemblyReference;
using InfrastructureAssembly = AB.SmokPrzewodnik.Infrastructure.AssemblyReference;

namespace AB.SmokPrzewodnik.ArchitectureTests;

public sealed class DependencyRulesTests
{
    private const string Api = "AB.SmokPrzewodnik.Api";
    private const string Application = "AB.SmokPrzewodnik.Application";
    private const string Domain = "AB.SmokPrzewodnik.Domain";
    private const string Infrastructure = "AB.SmokPrzewodnik.Infrastructure";

    [Fact]
    public void Domain_DoesNotReferenceOuterProjects()
    {
        var references = GetProjectReferences(DomainAssembly.Assembly);

        Assert.DoesNotContain(Application, references);
        Assert.DoesNotContain(Infrastructure, references);
        Assert.DoesNotContain(Api, references);
    }

    [Fact]
    public void Application_DoesNotReferenceInfrastructureOrApi()
    {
        var references = GetProjectReferences(ApplicationAssembly.Assembly);

        Assert.DoesNotContain(Infrastructure, references);
        Assert.DoesNotContain(Api, references);
    }

    [Fact]
    public void Infrastructure_DoesNotReferenceApi()
    {
        var references = GetProjectReferences(InfrastructureAssembly.Assembly);

        Assert.DoesNotContain(Api, references);
    }

    [Fact]
    public void Api_ReferencesApplicationAndInfrastructure()
    {
        var references = GetProjectReferences(typeof(Program).Assembly);

        Assert.Contains(Application, references);
        Assert.Contains(Infrastructure, references);
        Assert.DoesNotContain(Domain, references);
    }

    private static HashSet<string> GetProjectReferences(Assembly assembly)
    {
        return assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null && name.StartsWith("AB.SmokPrzewodnik.", StringComparison.Ordinal))
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);
    }
}
