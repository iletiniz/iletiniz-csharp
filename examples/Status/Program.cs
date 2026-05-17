using Iletiniz;
using Iletiniz.Exceptions;

if (args.Length < 1)
{
    Console.Error.WriteLine("Kullanım: Status <job_id>");
    Environment.Exit(2);
}

using var client = new IletinizClient(new IletinizClientOptions
{
    ApiKey = Environment.GetEnvironmentVariable("ILETINIZ_API_KEY"),
});

try
{
    var info = await client.Messages.RetrieveAsync(args[0]);
    Console.WriteLine($"Status: {info.Status}, Provider: {info.Provider}, Created: {info.CreatedAt}");
}
catch (IletinizNotFoundException)
{
    Console.Error.WriteLine("Mesaj bulunamadı.");
    Environment.Exit(1);
}
