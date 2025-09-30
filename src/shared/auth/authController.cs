using System.Collections;
using System.Net;
using System.Text;

namespace SimpleMDB;

public class Authcontroller
{
    public Authcontroller()
    {

    }

    public async Task LandingPageGet(HttpListenerRequest req, HttpListenerResponse res, Hashtable options)
    {
        string html = "Hello!";
        byte[] content = Encoding.UTF8.GetBytes(html);

        res.StatusCode = (int)HttpStatusCode.OK;
        res.ContentEncoding = Encoding.UTF8;
        res.ContentType = "text/plain";
        res.ContentLength64 = content.LongLength;
        await res.OutputStream.WriteAsync(content, 0, content.Length);
        res.Close();
    }

    internal class LandingPageGet
    {
    }
}