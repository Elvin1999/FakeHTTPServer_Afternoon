using FakeHTTPServer.DataAccess;
using FakeHTTPServer.Entities;
using FakeHTTPServer.Repository;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;



var listener = new TcpListener(IPAddress.Parse("10.1.10.1"), 27001);
listener.Start();

Console.WriteLine("Fake TCP Resr API Server started");
Console.WriteLine("10.1.10.1:27001/products");
Console.WriteLine("10.1.10.1:27001/products/1");

while (true)
{
    var client = await listener.AcceptTcpClientAsync();

    _ = Task.Run(async () =>
    {
        await HandleClientAsync(client);
    });
}

static async Task HandleClientAsync(TcpClient client)
{
    using (client)
    {
        await using (var stream = client.GetStream())
        {
            var buffer = new byte[8192];
            var count = await stream.ReadAsync(buffer);
            var rawRequest = Encoding.UTF8.GetString(buffer, 0, count);//GET /products
            Console.WriteLine("------------REQUEST------------");
            Console.WriteLine(rawRequest);

            var request = HttpRequest.Parse(rawRequest);
            var response = await Router.HandleAsync(request);

            var bytes = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(bytes);
        }
    }
}

public static class Router
{
    public static async Task<string> HandleAsync(HttpRequest request)
    {
        StepDBContext context = new StepDBContext();
        ProductRepository repo = new ProductRepository(context);

        if(request.Method=="GET" && request.Path == "/products")
        {
            var products = await repo.GetAllAsync();
            return HttpResponse.Parse(products);
        }

        return HttpResponse.Parse(null);
    }
}

public class HttpRequest
{
    public string Method { get; set; }
    public string Path { get; set; }
    public string Body { get; set; }

    public static HttpRequest Parse(string raw)//GET /products
    {
        var lines = raw.Split("\n", StringSplitOptions.RemoveEmptyEntries);

        var firstLine = lines[0].Trim();
        var parts = firstLine.Split(' ');

        var body = lines.Length > 1 ?
            string.Join("\n", lines.Skip(1))
            : "";

        return new HttpRequest
        {
            Method = parts[0].Trim().ToUpper(),
            Path = parts[1].Trim(),
            Body=body.Trim()
        };
        // POST
        // /products
        // {"name":"acer","price":2000}
    }
}

public static class HttpResponse
{
    public static string Parse(List<Product> products)
    {
        return JsonSerializer.Serialize(products, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}