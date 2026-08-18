using System.Xml.Linq;
using FluentAssertions;
using NetArchTest.Rules;
using Template.BackgroundJobs.Domain;
using Xunit;

namespace Template.BackgroundJobs.ArchitectureTests;

public sealed class ArchitectureRulesTests
{
    private static readonly System.Reflection.Assembly ServiceAssembly = typeof(TaskEntity).Assembly;

    [Fact]
    public void Domain_does_not_depend_on_outer_layers()
    {
        var result = Types.InAssembly(ServiceAssembly)
            .That().ResideInNamespace("Template.BackgroundJobs.Domain")
            .ShouldNot().HaveDependencyOnAny(
                "Template.BackgroundJobs.Endpoints",
                "Template.BackgroundJobs.Jobs",
                "Template.BackgroundJobs.Persistence",
                "Template.BackgroundJobs.Configuration")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Format(result));
    }

    [Theory]
    [InlineData("Template.BackgroundJobs.Endpoints", "Template.BackgroundJobs.Endpoints")]
    [InlineData("Template.BackgroundJobs.Jobs", "Template.BackgroundJobs.Jobs")]
    [InlineData("Template.BackgroundJobs.Persistence", "Template.BackgroundJobs.Persistence")]
    public void Layer_types_remain_in_their_namespace_family(
        string selection,
        string requiredNamespace)
    {
        var selectionPattern = "^" + System.Text.RegularExpressions.Regex.Escape(selection) + "($|\\.)";
        var requiredPattern = "^" + System.Text.RegularExpressions.Regex.Escape(requiredNamespace) + "($|\\.)";
        var result = Types.InAssembly(ServiceAssembly)
            .That().ResideInNamespaceMatching(selectionPattern)
            .Should().ResideInNamespaceMatching(requiredPattern)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(Format(result));
    }

    [Fact]
    public void Project_references_point_in_the_documented_direction()
    {
        var root = FindRepositoryRoot();
        var backgroundJobs = ProjectReferences(Path.Combine(
            root.FullName,
            "src/services/Template.BackgroundJobs/Template.BackgroundJobs.csproj"));
        var defaults = ProjectReferences(Path.Combine(
            root.FullName,
            "src/common/Template.ServiceDefaults/Template.ServiceDefaults.csproj"));
        var appHost = ProjectReferences(Path.Combine(
            root.FullName,
            "src/services/Template.AppHost/Template.AppHost.csproj"));

        backgroundJobs.Should().Contain(path => path.EndsWith("Template.ServiceDefaults.csproj"));
        backgroundJobs.Should().NotContain(path => path.EndsWith("Template.Api.csproj"));
        backgroundJobs.Should().NotContain(path => path.EndsWith("Template.AppHost.csproj"));
        defaults.Should().NotContain(path => path.EndsWith("Template.BackgroundJobs.csproj"));
        appHost.Should().Contain(path => path.EndsWith("Template.Api.csproj"));
        appHost.Should().Contain(path => path.EndsWith("Template.BackgroundJobs.csproj"));
    }

    private static string Format(NetArchTest.Rules.TestResult result) =>
        $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}";

    private static DirectoryInfo FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;
        return directory ?? throw new InvalidOperationException(
            $"Could not find repository root from {AppContext.BaseDirectory}.");
    }

    private static string[] ProjectReferences(string path) => XDocument.Load(path)
        .Descendants("ProjectReference")
        .Select(element => (string?)element.Attribute("Include"))
        .Where(value => value is not null)
        .Cast<string>()
        .ToArray();
}
