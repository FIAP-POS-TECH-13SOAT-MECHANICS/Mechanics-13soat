namespace Mechanics.Domain.Vehicles;

public class Plate(string number)
{
    public string Number { get; } = Normalize(number);

    public static implicit operator string(Plate plate) => plate.Number;
    private static string Normalize(string number) => number.Replace("-", "").ToUpper().Trim();

    public override string ToString() => Number;

    public bool IsValid()
    {
        // TODO
        return Number.Length is 7;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Plate plate)
            return false;

        return plate.Number == Number;
    }

    public override int GetHashCode() => Number.GetHashCode();
}
