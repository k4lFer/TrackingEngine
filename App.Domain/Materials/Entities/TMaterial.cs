using App.Domain.Materials.Events;
using App.Shared.Domain;

namespace App.Domain.Materials.Entities;

public class TMaterial : BaseDomain
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Unit { get; private set; } = "ton";
    public bool Active { get; private set; } = true;

    private TMaterial() { }

    private TMaterial(string code, string name, string unit)
    {
        Code = code;
        Name = name;
        Unit = string.IsNullOrWhiteSpace(unit) ? "ton" : unit;
        Active = true;
    }

    public static TMaterial Create(string code, string name, string unit)
    {
        var m = new TMaterial(code, name, unit);
        m.AddDomainEvent(new MaterialCreatedEvent(m.Id, m.Code, m.Name, m.Unit));
        return m;
    }

    public void Update(string name, string unit, bool active)
    {
        Name = name;
        Unit = string.IsNullOrWhiteSpace(unit) ? "ton" : unit;
        Active = active;
    }
}