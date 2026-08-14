using System.Xml.Linq;
using FluentAssertions;
using NetArchTest.Rules;
using Template.Api.Domain;
using Xunit;

namespace Template.ArchitectureTests;

public sealed class ArchitectureRulesTests
{
    private static readonly System.Reflection.Assembly ApiAssembly = typeof(TaskEntity).Assembly;

    [Fact]
    public void Domain_does_not_depend_on_outer_layers()
    {
        var result = Types.InAssembly(ApiAssembly).That().ResideInNamespace("Template.Api.Domain")
            .ShouldNot().HaveDependencyOnAny(
                "Template.Api.Endpoints", "Template.Api.Features", "Template.Api.Persistence",
                "Template.Api.Messaging", "Template.Api.Configuration").GetResult();
        result.IsSuccessful.Should().BeTrue(Format(result));
    }

    [Fact]
    public void Features_do_not_depend_on_concrete_persistence_or_publishers()
    {
        var result = Types.InAssembly(ApiAssembly).That().ResideInNamespaceMatching("^Template\\.Api\\.Features($|\\.)")
            .ShouldNot().HaveDependencyOnAny(
                "Template.Api.Persistence.ApplicationDbContext",
                "Template.ServiceDefaults.Messaging.Kafka.IMessagePublisher",
                "Template.ServiceDefaults.Messaging.Kafka.KafkaMessagePublisher").GetResult();
        result.IsSuccessful.Should().BeTrue(Format(result));
    }

    [Theory]
    [InlineData("Template.Api.Endpoints", "Template.Api.Endpoints")]
    [InlineData("Template.Api.Features", "Template.Api.Features")]
    [InlineData("Template.Api.Persistence", "Template.Api.Persistence")]
    [InlineData("Template.Api.Messaging", "Template.Api.Messaging")]
    public void Layer_types_remain_in_their_namespace_family(string selection, string requiredNamespace)
    {
        var pattern = "^" + System.Text.RegularExpressions.Regex.Escape(selection) + "($|\\.)";
        var requiredPattern = "^" + System.Text.RegularExpressions.Regex.Escape(requiredNamespace) + "($|\\.)";
        var result = Types.InAssembly(ApiAssembly).That().ResideInNamespaceMatching(pattern)
            .Should().ResideInNamespaceMatching(requiredPattern).GetResult();
        result.IsSuccessful.Should().BeTrue(Format(result));
    }

    [Fact]
    public void Project_references_point_in_the_documented_direction()
    {
        var root = FindRepositoryRoot();
        var api = ProjectReferences(Path.Combine(root.FullName, "src/services/Template.Api/Template.Api.csproj"));
        var defaults = ProjectReferences(Path.Combine(root.FullName, "src/common/Template.ServiceDefaults/Template.ServiceDefaults.csproj"));
        var appHost = ProjectReferences(Path.Combine(root.FullName, "src/services/Template.AppHost/Template.AppHost.csproj"));

        api.Should().Contain(x => x.EndsWith("Template.ServiceDefaults.csproj"));
        api.Should().NotContain(x => x.EndsWith("Template.AppHost.csproj"));
        defaults.Should().NotContain(x => x.EndsWith("Template.Api.csproj"));
        appHost.Should().Contain(x => x.EndsWith("Template.Api.csproj"));
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
        .Select(x => (string?)x.Attribute("Include"))
        .Where(x => x is not null)
        .Cast<string>()
        .ToArray();
}
