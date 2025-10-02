using System.Text.RegularExpressions;

namespace Mechanics.Domain.Base.Validation;

public static partial class RegexUtils
{
    [GeneratedRegex(@"^\d{11}$", RegexOptions.Compiled)]
    public static partial Regex Cpf();

    [GeneratedRegex(@"^[A-Z\d]{12}\d\d$", RegexOptions.Compiled)]
    public static partial Regex Cnpj();
}
