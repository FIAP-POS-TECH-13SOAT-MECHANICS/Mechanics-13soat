using Mechanics.Domain.Base;

namespace Mechanics.Domain.Customers;

public class PersonalDocument(DocumentType type, string number) : INormalizable
{
    public DocumentType Type { get; } = type;
    public string Number { get; private set; } = number;

    public static implicit operator string(PersonalDocument document) => document.Number;

    public void Normalize()
    {
        Number = Number.Replace(".", "").Replace("-", "").Replace("/", "").ToUpper().Trim();
    }

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

    public override int GetHashCode() => Number.GetHashCode();
}

public enum DocumentType
{
    Cpf,
    Cnpj,
}
