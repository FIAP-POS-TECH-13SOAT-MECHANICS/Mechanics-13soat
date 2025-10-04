namespace Mechanics.Domain.Base;

public interface INormalizable
{
    bool IsNormalized();
    void Normalize();
}

public static class NormalizableExtensions
{
    public static bool IsTrimmedUpperCase(this string str) =>
        !str.StartsWith(' ') &&
        !str.EndsWith(' ') &&
        str.All(c => !char.IsLetter(c) || char.IsUpper(c));

    public static bool IsTrimmedLowerCase(this string str) =>
        !str.StartsWith(' ') &&
        !str.EndsWith(' ') &&
        str.All(c => !char.IsLetter(c) || char.IsLower(c));
}
