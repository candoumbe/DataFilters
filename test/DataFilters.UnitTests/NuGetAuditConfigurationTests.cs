using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AwesomeAssertions;
using Xunit;
using Xunit.Categories;

namespace DataFilters.UnitTests;

[UnitTest]
public class NuGetAuditConfigurationTests
{
    [Fact]
    public void DirectoryPackagesProps_should_enable_NuGetAudit()
    {
        // Arrange
        string filePath = Path.Combine(GetRepositoryRootPath(), "Directory.Packages.props");

        // Act
        string? value = ReadPropertyValue(filePath, "NuGetAudit");

        // Assert
        value.Should()
            .Be("true");
    }

    [Fact]
    public void DirectoryPackagesProps_should_set_NuGetAuditMode_to_all()
    {
        // Arrange
        string filePath = Path.Combine(GetRepositoryRootPath(), "Directory.Packages.props");

        // Act
        string? value = ReadPropertyValue(filePath, "NuGetAuditMode");

        // Assert
        value.Should()
            .Be("all");
    }

    [Fact]
    public void DirectoryPackagesProps_should_set_NuGetAuditLevel_to_high()
    {
        // Arrange
        string filePath = Path.Combine(GetRepositoryRootPath(), "Directory.Packages.props");

        // Act
        string? value = ReadPropertyValue(filePath, "NuGetAuditLevel");

        // Assert
        value.Should()
            .Be("high");
    }

    [Fact]
    public void CoreProps_should_configure_CI_WarningsAsErrors_for_NU1900_to_NU1904()
    {
        // Arrange
        string filePath = Path.Combine(GetRepositoryRootPath(), "core.props");
        XDocument document = XDocument.Load(filePath);

        // Act
        XElement? ciPropertyGroup = document.Root?
            .Elements("PropertyGroup")
            .FirstOrDefault(group => string.Equals((string?)group.Attribute("Condition"), "'$(ContinuousIntegrationBuild)' == 'true'", StringComparison.Ordinal));

        string warningsAsErrors = ciPropertyGroup?
            .Element("WarningsAsErrors")?
            .Value ?? string.Empty;

        List<string> expectedCodes =
        [
            "NU1900",
            "NU1901",
            "NU1902",
            "NU1903",
            "NU1904",
        ];

        // Assert
        ciPropertyGroup.Should()
            .NotBeNull();
        foreach (string expectedCode in expectedCodes)
        {
            warningsAsErrors.Should()
                .Contain(expectedCode);
        }
    }

    private static string? ReadPropertyValue(string filePath, string propertyName)
    {
        XDocument document = XDocument.Load(filePath);
        string? result = document.Root?
            .Elements("PropertyGroup")
            .Elements(propertyName)
            .Select(element => element.Value)
            .FirstOrDefault();

        return result;
    }

    private static string GetRepositoryRootPath()
    {
        DirectoryInfo? current = new DirectoryInfo(AppContext.BaseDirectory);
        string result = string.Empty;

        while (current is not null && string.IsNullOrWhiteSpace(result))
        {
            string solutionPath = Path.Combine(current.FullName, "DataFilters.sln");
            if (File.Exists(solutionPath))
            {
                result = current.FullName;
            }

            current = current.Parent;
        }

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException("Unable to locate repository root from AppContext.BaseDirectory.");
        }

        return result;
    }
}
