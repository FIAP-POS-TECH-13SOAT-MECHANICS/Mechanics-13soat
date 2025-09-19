namespace Mechanics.Domain.Customers;

public class PersonalDocument(DocumentType type, string number)
{
    public DocumentType Type { get; } = type;
    public string Number { get; } = Normalize(number);

    public static implicit operator string(PersonalDocument document) => document.Number;

    private static string Normalize(string number) => number.Replace(".", "").Replace("-", "").Replace("/", "").ToUpper().Trim();

    public bool IsValid()
    {
        return Type == DocumentType.Cpf ? Number.Length is 11 : Number.Length is 14;
    }

    public override string ToString() => Number;

    public override bool Equals(object? obj)
    {
        if (obj is not PersonalDocument document)
            return false;

        return document.Number == Number;
    }

    public override int GetHashCode() => HashCode.Combine(Number, (int)Type);
}

public enum DocumentType
{
    Cpf,
    Cnpj,
}
