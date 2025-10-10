using Mechanics.Domain.Base.Validation;
using System.Text.RegularExpressions;

namespace Mechanics.Application.Utils;

public static class StringUtils
{
    public static string RemoveDiacritics(this string text) =>
        Regex.Replace(text.Normalize(), RegexUtils.NonUnicodeChar().ToString(), string.Empty);
}
