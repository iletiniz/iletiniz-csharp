using System.Text.Json;

namespace Iletiniz.Exceptions;

internal static class ApiExceptionFactory
{
    public static IletinizApiException Build(int status, string? raw, string? requestId)
    {
        string? code = null;
        string? message = null;

        if (!string.IsNullOrEmpty(raw))
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("error", out var errEl) && errEl.ValueKind == JsonValueKind.String)
                    {
                        code = errEl.GetString();
                    }
                    if (doc.RootElement.TryGetProperty("message", out var msgEl) && msgEl.ValueKind == JsonValueKind.String)
                    {
                        message = msgEl.GetString();
                    }
                }
            }
            catch (JsonException)
            {
                // Düz metin gövde olabilir.
                if (raw.Length > 0 && raw[0] != '{' && raw[0] != '[')
                {
                    message = raw;
                }
            }
        }

        if (string.IsNullOrEmpty(message))
        {
            message = $"HTTP {status}";
        }

        return status switch
        {
            401 => new IletinizAuthenticationException(message!, status, code, raw, requestId),
            403 => new IletinizPermissionException(message!, status, code, raw, requestId),
            404 => new IletinizNotFoundException(message!, status, code, raw, requestId),
            400 or 422 => new IletinizValidationException(message!, status, code, raw, requestId),
            429 => new IletinizRateLimitException(message!, status, code, raw, requestId),
            >= 500 and <= 599 => new IletinizServerException(message!, status, code, raw, requestId),
            _ => new IletinizApiException(message!, status, code, raw, requestId),
        };
    }
}
