using System.Globalization;
using System.Text;
using Iletiniz.Exceptions;
using Iletiniz.Models.Internal;

namespace Iletiniz.Resources;

internal static class QueryString
{
    public static string List<TStatus>(string path, TStatus? status, int? limit, int? offset)
        where TStatus : struct, Enum
    {
        var sb = new StringBuilder(path);
        var sep = '?';
        void Add(string key, string value)
        {
            sb.Append(sep).Append(key).Append('=').Append(Uri.EscapeDataString(value));
            sep = '&';
        }
        if (status is not null) Add("status", SnakeCaseEnumConverter<TStatus>.ToSnake(status.Value.ToString()));
        if (limit is not null) Add("limit", limit.Value.ToString(CultureInfo.InvariantCulture));
        if (offset is not null) Add("offset", offset.Value.ToString(CultureInfo.InvariantCulture));
        return sb.ToString();
    }

    public static void AssertId(long id, string label = "id")
    {
        if (id <= 0) throw new IletinizException($"{label} pozitif tam sayı olmalıdır.");
    }
}
