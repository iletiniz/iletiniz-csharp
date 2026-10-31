using System.Text.Json.Serialization;
using Iletiniz.Models.Internal;

namespace Iletiniz.Models;

public sealed record PartnerPricing(
    [property: JsonPropertyName("discount_pct")] double? DiscountPct,
    [property: JsonPropertyName("package")] string? Package
);

/// <summary><c>GET /v1/partner</c> yanıtı.</summary>
public sealed record PartnerProfile(
    [property: JsonPropertyName("workspace_id")] long WorkspaceId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("pricing")] PartnerPricing Pricing,
    [property: JsonPropertyName("credit_balance")] long CreditBalance,
    [property: JsonPropertyName("child_count")] int ChildCount
);

[JsonConverter(typeof(SnakeCaseEnumConverter<ChildAccountStatus>))]
public enum ChildAccountStatus
{
    Active,
    Suspended,
}

/// <summary>Partner alt hesabı.</summary>
public sealed record ChildAccount(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("status")] ChildAccountStatus Status,
    [property: JsonPropertyName("credit_balance")] long CreditBalance,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("suspended_at")] string? SuspendedAt
);

public sealed class CreateChildAccountParams
{
    /// <summary>2-100 karakter.</summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>Kendi sistemindeki müşteri kimliği (harf, rakam, _ ve -; en fazla 32).</summary>
    [JsonPropertyName("externalRef")]
    public string? ExternalRef { get; init; }
}

public sealed class ListChildAccountsParams
{
    public ChildAccountStatus? Status { get; init; }

    public int? Limit { get; init; }

    public int? Offset { get; init; }
}

public sealed class CreateChildApiKeyParams
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Varsayılan: <c>messages:send</c>, <c>messages:read</c>. İzin verilenler: messages:send, messages:read,
    /// contacts:read, contacts:write, groups:read, groups:write, connections:read, account:read.
    /// </summary>
    [JsonPropertyName("scopes")]
    public IReadOnlyList<string>? Scopes { get; init; }

    /// <summary><c>true</c> → <c>iltz_test_</c> (sandbox) anahtarı.</summary>
    [JsonPropertyName("test")]
    public bool? Test { get; init; }

    [JsonPropertyName("allowedIps")]
    public IReadOnlyList<string>? AllowedIps { get; init; }
}

/// <summary>Alt hesap API anahtarı. <c>Key</c> yalnız bu yanıtta döner.</summary>
public sealed record ChildApiKey(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("workspace_id")] long WorkspaceId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("key_prefix")] string KeyPrefix,
    [property: JsonPropertyName("scopes")] IReadOnlyList<string> Scopes,
    [property: JsonPropertyName("sandbox")] bool Sandbox
);

[JsonConverter(typeof(SnakeCaseEnumConverter<ChildApiKeyStatus>))]
public enum ChildApiKeyStatus
{
    Active,
    Revoked,
}

/// <summary>
/// Alt hesabın bir API anahtarı (liste/iptal yanıtı). Düz metin anahtar veya hash asla dönmez.
/// <c>KeyPrefix</c> ilk 12 karakterdir (örn. <c>iltz_live_Ab</c>); <c>Source</c>: <c>partner</c> | <c>dashboard</c>.
/// </summary>
public sealed record ChildApiKeyInfo(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("key_prefix")] string KeyPrefix,
    [property: JsonPropertyName("scopes")] IReadOnlyList<string>? Scopes,
    [property: JsonPropertyName("sandbox")] bool Sandbox,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("status")] ChildApiKeyStatus Status,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("last_used_at")] string? LastUsedAt,
    [property: JsonPropertyName("revoked_at")] string? RevokedAt
);

/// <summary><c>GET /v1/partner/accounts/{id}/api-keys</c> yanıtı.</summary>
public sealed record ChildApiKeyList(
    [property: JsonPropertyName("data")] IReadOnlyList<ChildApiKeyInfo> Data
);

public sealed record QuotaTransferResponse(
    [property: JsonPropertyName("child_workspace_id")] long ChildWorkspaceId,
    [property: JsonPropertyName("amount")] long Amount,
    [property: JsonPropertyName("partner_balance")] long PartnerBalance,
    [property: JsonPropertyName("child_balance")] long ChildBalance
);

/// <summary>Tek kullanımlık WhatsApp bağlantı linki (24 saat geçerli).</summary>
public sealed record WhatsappSignupLink(
    [property: JsonPropertyName("child_workspace_id")] long ChildWorkspaceId,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("expires_at")] string ExpiresAt
);
