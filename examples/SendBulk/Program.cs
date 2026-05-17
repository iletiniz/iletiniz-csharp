using Iletiniz;
using Iletiniz.Models;

using var client = new IletinizClient(new IletinizClientOptions
{
    ApiKey = Environment.GetEnvironmentVariable("ILETINIZ_API_KEY"),
});

var result = await client.Messages.SendBulkAsync(new SendBulkParams
{
    Template = "low_stock_alert",
    Items = new[]
    {
        new BulkItemInput
        {
            To = "+905551111111",
            Variables = new Dictionary<string, object> { ["product"] = "Ürün A", ["stock"] = 3 },
        },
        new BulkItemInput
        {
            To = "+905552222222",
            Variables = new Dictionary<string, object> { ["product"] = "Ürün B", ["stock"] = 1 },
        },
    },
});

Console.WriteLine($"Toplam: {result.Total}, Gönderilen: {result.Sent}, Başarısız: {result.Failed}");
