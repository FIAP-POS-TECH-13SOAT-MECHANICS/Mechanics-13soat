using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Validation;

namespace Mechanics.Domain.Customers;

public class PersonalDocument(DocumentType type, string number) : INormalizable, IValidatable
{
    public DocumentType Type { get; } = type;
    public string Number { get; private set; } = number;

    public static implicit operator string(PersonalDocument document) => document.Number;

    public bool IsNormalized() =>
        Type == DocumentType.Cpf && RegexUtils.Cpf().IsMatch(Number) ||
        Type == DocumentType.Cnpj && RegexUtils.Cnpj().IsMatch(Number);

    public void Normalize()
    {
        Number = Type == DocumentType.Cpf
            ? new string(Number.Where(char.IsDigit).ToArray())
            : new string(Number.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
    }

    public void Validate(ValidationBuilder builder)
    {
        builder.AddConditionalValidation(Type == DocumentType.Cpf, conditionalBuilder =>
            conditionalBuilder.AddValidation(ValidateCpf(), nameof(Number), "Invalid CPF."));
        builder.AddConditionalValidation(Type == DocumentType.Cnpj, conditionalBuilder =>
            conditionalBuilder.AddValidation(ValidateCnpj(), nameof(Number), "Invalid CNPJ."));
    }

    public override string ToString() => Number;

    public override bool Equals(object? obj)
    {
        if (obj is not PersonalDocument document)
            return false;

        return document.Number == Number;
    }

    public override int GetHashCode() => Number.GetHashCode();

    private bool ValidateCpf()
    {
        if (!IsNormalized() || Number.Distinct().Count() == 1)
            return false;

        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += (Number[i] - '0') * (10 - i);

        var remainder = sum % 11;
        var firstDigit = remainder < 2 ? 0 : 11 - remainder;

        if (firstDigit != Number[9] - '0')
            return false;

        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += (Number[i] - '0') * (11 - i);

        remainder = sum % 11;
        var secondDigit = remainder < 2 ? 0 : 11 - remainder;

        return secondDigit == Number[10] - '0';
    }

    private bool ValidateCnpj()
    {
        if (!IsNormalized())
            return false;

        const int baseValue = 48;
        int[] weights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var sumDv1 = 0;
        var sumDv2 = 0;

        for (var i = 0; i < 12; i++)
        {
            var asciiDigit = Number[i] - baseValue;
            sumDv1 += asciiDigit * weights[i + 1];
            sumDv2 += asciiDigit * weights[i];
        }

        var dv1 = sumDv1 % 11 < 2 ? 0 : 11 - (sumDv1 % 11);

        if (dv1 != Number[12] - '0')
            return false;

        sumDv2 += dv1 * weights[12];
        var dv2 = sumDv2 % 11 < 2 ? 0 : 11 - (sumDv2 % 11);

        return dv2 == Number[13] - '0';
    }
}

public enum DocumentType
{
    Cpf,
    Cnpj,
}
