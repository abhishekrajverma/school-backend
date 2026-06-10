using System.Reflection;
using NetArchTest.Rules;

namespace EduSync.ArchitectureTests;

public sealed class LayerArchitectureTests
{
  private static readonly string ModulesNamespace = "EduSync.Modules";
  private static readonly string InfrastructureNamespace = "EduSync.Infrastructure";
  private static readonly string ApiNamespace = "EduSync.Api";

  [Fact]
  public void Modules_should_not_reference_Infrastructure()
  {
    var moduleAssemblies = Types.InAssemblies(GetModuleAssemblies())
      .That()
      .ResideInNamespace(ModulesNamespace)
      .GetTypes()
      .Select(t => t.Assembly)
      .Distinct()
      .ToArray();

    foreach (var assembly in moduleAssemblies)
    {
      var result = Types.InAssembly(assembly)
        .ShouldNot()
        .HaveDependencyOn(InfrastructureNamespace)
        .GetResult();

      Assert.True(result.IsSuccessful, $"{assembly.GetName().Name} references Infrastructure: {FormatFailingTypes(result)}");
    }
  }

  [Fact]
  public void Modules_should_not_reference_Api()
  {
    var moduleAssemblies = Types.InAssemblies(GetModuleAssemblies())
      .That()
      .ResideInNamespace(ModulesNamespace)
      .GetTypes()
      .Select(t => t.Assembly)
      .Distinct()
      .ToArray();

    foreach (var assembly in moduleAssemblies)
    {
      var result = Types.InAssembly(assembly)
        .ShouldNot()
        .HaveDependencyOn(ApiNamespace)
        .GetResult();

      Assert.True(result.IsSuccessful, $"{assembly.GetName().Name} references Api: {FormatFailingTypes(result)}");
    }
  }

  [Fact]
  public void Infrastructure_should_not_reference_Api()
  {
    var result = Types.InAssembly(typeof(EduSync.Infrastructure.DependencyInjection).Assembly)
      .ShouldNot()
      .HaveDependencyOn(ApiNamespace)
      .GetResult();

    Assert.True(result.IsSuccessful, FormatFailingTypes(result));
  }

  [Fact]
  public void Domain_entities_should_not_reference_MediatR()
  {
    foreach (var assembly in GetDomainAssemblies())
    {
      var domainNamespace = assembly.GetTypes()
        .First(t => t.Namespace?.EndsWith(".Domain", StringComparison.Ordinal) == true)
        .Namespace!;

      var result = Types.InAssembly(assembly)
        .That()
        .ResideInNamespace(domainNamespace)
        .ShouldNot()
        .HaveDependencyOn("MediatR")
        .GetResult();

      Assert.True(result.IsSuccessful, $"{assembly.GetName().Name}: {FormatFailingTypes(result)}");
    }
  }

  private static Assembly[] GetDomainAssemblies() =>
  [
    typeof(EduSync.Modules.Students.Domain.Student).Assembly,
    typeof(EduSync.Modules.Staff.Domain.Teacher).Assembly,
    typeof(EduSync.Modules.Admissions.Domain.AdmissionApplication).Assembly,
    typeof(EduSync.Modules.Tenancy.Domain.Tenant).Assembly,
    typeof(EduSync.Modules.Exams.Domain.Exam).Assembly,
    typeof(EduSync.Modules.Assignments.Domain.Assignment).Assembly,
    typeof(EduSync.Modules.Identity.Domain.User).Assembly,
    typeof(EduSync.Modules.Fees.Domain.FeeInvoice).Assembly,
  ];

  private static Assembly[] GetModuleAssemblies() =>
  [
    typeof(EduSync.Modules.Students.Domain.Student).Assembly,
    typeof(EduSync.Modules.Staff.Domain.Teacher).Assembly,
    typeof(EduSync.Modules.Admissions.Domain.AdmissionApplication).Assembly,
    typeof(EduSync.Modules.Tenancy.Domain.Tenant).Assembly,
    typeof(EduSync.Modules.Exams.Domain.Exam).Assembly,
    typeof(EduSync.Modules.Assignments.Domain.Assignment).Assembly,
    typeof(EduSync.Modules.Identity.Domain.User).Assembly,
    typeof(EduSync.Modules.Fees.Domain.FeeInvoice).Assembly,
    typeof(EduSync.Modules.Portals.Application.GetStudentPortalProfileQuery).Assembly,
  ];

  private static string FormatFailingTypes(TestResult result) =>
    result.FailingTypes is null || result.FailingTypes.Count == 0
      ? string.Empty
      : string.Join(", ", result.FailingTypes.Select(t => t.FullName));
}
