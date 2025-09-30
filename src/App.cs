using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMDB;

public class App
{
    private HttpListener server;
    private HttpRouter router;
    private AuthController authController;

    public App()
    {
        string host = "http://127.0.0.1:8080/";
        server = new HttpListener();
        server.Prefixes.Add(host);

        authController = new AuthController();
        router = new HttpRouter();

        // Register route handler (ensure LandingPageGet exists in AuthController)
        router.AddGet("/", authController.LandingPageGet);

        Console.WriteLine("Server listening on... " + host);
    }

    public async Task Start()
    {
        server.Start();

        while (server.IsListening)
        {
            var ctx = await server.GetContextAsync();
            await HandleContextAsync(ctx);
        }
    }

    public void Stop()
    {
        server.Stop();
        server.Close();
    }

    private async Task HandleContextAsync(HttpListenerContext ctx)
    {
        var req = ctx.Request;
        var res = ctx.Response;

        // Handle the request through the router
        await router.Handle(req, res);
    }
}