using Mechanics.Domain.Auth;

namespace Mechanics.Infra.Data.Seeds;

public static class RoleSeeds
{
    public static IEnumerable<Role> GetSeeds() =>
    [
        new() { Id = new Guid("2afde195-550b-498e-a63d-7a6d556b25ba"), Name = "ADMINISTRATOR" },
        new() { Id = new Guid("a1097867-aa3e-416c-8685-190516b62a12"), Name = "ATTENDANT" },
        new() { Id = new Guid("f6027484-89a4-49f6-a9cb-4d1733c2bab7"), Name = "MECHANIC" },
    ];
}
