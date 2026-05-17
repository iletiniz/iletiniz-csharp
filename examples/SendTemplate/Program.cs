using Iletiniz;
using Iletiniz.Models;

using var client = new IletinizClient(new IletinizClientOptions
{
    ApiKey = Environment.GetEnvironmentVariable("ILETINIZ_API_KEY"),
});

var result = await client.Messages.SendAsync(new SendMessageParams
{
    To = "+905551234567",
    Template = "order_shipped",
    Variables = new Dictionary<string, object>
    {
        ["name"] = "Ayşe",
        ["tracking_no"] = "TR123456789",
    },
});

Console.WriteLine($"Sent via template: {result.TemplateKey} -> {result.Status}");
