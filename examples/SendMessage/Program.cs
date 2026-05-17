using Iletiniz;
using Iletiniz.Models;

using var client = new IletinizClient(new IletinizClientOptions
{
    ApiKey = Environment.GetEnvironmentVariable("ILETINIZ_API_KEY"),
});

var result = await client.Messages.SendAsync(new SendMessageParams
{
    To = "+905551234567",
    Body = "Merhaba! Bu Iletiniz SDK ile gönderilen test mesajıdır.",
});

Console.WriteLine($"Job: {result.JobId} Status: {result.Status}");
