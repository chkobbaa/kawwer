using System.Reflection;

namespace Kawwer.Tests.Architecture;

/// <summary>
/// Enforces the Clean Architecture dependency rules from docs/Architecture.md by inspecting the
/// compiled assembly references of each layer.
/// </summary>
public sealed class DependencyTests
{
    private static readonly Assembly Domain = typeof(Kawwer.Domain.Entities.Match).Assembly;
    private static readonly Assembly Application = typeof(Kawwer.Application.DependencyInjection).Assembly;
    private static readonly Assembly Infrastructure = typeof(Kawwer.Infrastructure.DependencyInjection).Assembly;

    private static IEnumerable<string> ReferencedNames(Assembly assembly)
        => assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty);

    [Fact]
    public void Domain_DoesNotReference_OtherLayers()
    {
        var refs = ReferencedNames(Domain).ToList();
        Assert.DoesNotContain("Kawwer.Application", refs);
        Assert.DoesNotContain("Kawwer.Infrastructure", refs);
        Assert.DoesNotContain("Kawwer.Api", refs);
        Assert.DoesNotContain("Kawwer.Contracts", refs);
    }

    [Fact]
    public void Application_DoesNotReference_InfrastructureOrApi()
    {
        var refs = ReferencedNames(Application).ToList();
        Assert.DoesNotContain("Kawwer.Infrastructure", refs);
        Assert.DoesNotContain("Kawwer.Api", refs);
    }

    [Fact]
    public void Infrastructure_DoesNotReference_Api()
    {
        var refs = ReferencedNames(Infrastructure).ToList();
        Assert.DoesNotContain("Kawwer.Api", refs);
    }

    [Fact]
    public void Domain_DoesNotReference_EntityFrameworkOrAspNet()
    {
        var refs = ReferencedNames(Domain).ToList();
        Assert.DoesNotContain(refs, name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        Assert.DoesNotContain(refs, name => name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
    }
}
