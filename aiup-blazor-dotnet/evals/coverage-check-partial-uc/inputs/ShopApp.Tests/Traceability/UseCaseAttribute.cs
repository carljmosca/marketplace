namespace ShopApp.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class UseCaseAttribute(string id) : Attribute
{
    public string Id { get; } = id;
    public string Scenario { get; set; } = "Main Success Scenario";
    public string[] BusinessRules { get; set; } = [];
}
