using System.Text.Json;
using System.Text.Json.Nodes;
using Iletiniz.Exceptions;
using Iletiniz.Http;
using Iletiniz.Models;

namespace Iletiniz.Resources;

/// <summary>Hatırlatma kuralları (<c>/v1/reminders</c>): tek seferlik veya tekrarlayan.</summary>
public sealed class RemindersResource
{
    private static readonly JsonSerializerOptions BodyOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly InternalHttpClient _http;

    internal RemindersResource(InternalHttpClient http)
    {
        _http = http;
    }

    public async Task<Reminder> CreateAsync(
        CreateReminderParams parameters,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (parameters is null) throw new IletinizException("CreateAsync() parametre objesi gerektirir.");
        if (parameters.Type == ReminderType.OneTime)
        {
            if (parameters.EventAt is null) throw new IletinizException("Tek seferlik kural için 'EventAt' zorunlu.");
            if (parameters.OffsetsMinutes is null || parameters.OffsetsMinutes.Count == 0)
                throw new IletinizException("Tek seferlik kural için 'OffsetsMinutes' zorunlu.");
        }
        else
        {
            if (parameters.Frequency is null || string.IsNullOrEmpty(parameters.TimeOfDay))
                throw new IletinizException("Tekrarlayan kural için 'Frequency' ve 'TimeOfDay' zorunlu.");
            if (parameters.Frequency == ReminderFrequency.Weekly && parameters.DayOfWeek is null)
                throw new IletinizException("Haftalık tekrar için 'DayOfWeek' zorunlu.");
            if (parameters.Frequency == ReminderFrequency.Monthly && parameters.DayOfMonth is null)
                throw new IletinizException("Aylık tekrar için 'DayOfMonth' zorunlu.");
        }
        var res = await _http.RequestAsync<Reminder>(
            HttpMethod.Post, "/v1/reminders", body: parameters, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    public async Task<ListResponse<Reminder>> ListAsync(
        ListRemindersParams? parameters = null,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var path = QueryString.List("/v1/reminders", parameters?.Status, parameters?.Limit, parameters?.Offset);
        var res = await _http.RequestAsync<ListResponse<Reminder>>(
            HttpMethod.Get, path, body: null, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    public async Task<Reminder> RetrieveAsync(
        long id,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QueryString.AssertId(id);
        var res = await _http.RequestAsync<Reminder>(
            HttpMethod.Get, $"/v1/reminders/{id}", body: null, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    /// <summary>Kuralı günceller; bekleyen gönderimler yeni ayarlarla yeniden planlanır.</summary>
    public async Task<Reminder> UpdateAsync(
        long id,
        UpdateReminderParams parameters,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QueryString.AssertId(id);
        if (parameters is null) throw new IletinizException("UpdateAsync() parametre objesi gerektirir.");
        var body = JsonSerializer.SerializeToNode(parameters, BodyOptions)!.AsObject();
        if (parameters.ClearEndsAt)
        {
            body["endsAt"] = null;
        }
        if (body.Count == 0)
        {
            throw new IletinizException("En az bir alan güncellenmeli.");
        }
        var res = await _http.RequestAsync<Reminder>(
            HttpMethod.Patch, $"/v1/reminders/{id}", body: body, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }

    /// <summary>Kuralı iptal eder (<c>canceled</c>) ve bekleyen gönderimlerini siler.</summary>
    public async Task<Reminder> DeleteAsync(
        long id,
        RequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        QueryString.AssertId(id);
        var res = await _http.RequestAsync<Reminder>(
            HttpMethod.Delete, $"/v1/reminders/{id}", body: null, options: options, cancellationToken: cancellationToken
        ).ConfigureAwait(false);
        return res!;
    }
}
