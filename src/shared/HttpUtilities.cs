using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;

namespace SimpleMDB;

public class HttpUtils
{
    public static void AddOptions(Hashtable options, string key, string value)
    {
        var prop = (NameValueCollection?)options[key] ?? new NameValueCollection();
        options[key] = prop;
        prop[key] = value;
    }

    public static async Task AddOptions(Hashtable options, string name, NameValueCollection entries)
    {
        var prop = (NameValueCollection?)options[name] ?? new NameValueCollection();
        options[name] = prop;
        
        foreach (string key in entries.AllKeys)
        {
            prop[key] = entries[key];
        }
        
        await Task.CompletedTask;
    }

    public static async Task SendResponse(HttpListenerResponse res, Hashtable options, int statuscode, string body)
    {
        byte[] content = Encoding.UTF8.GetBytes(body);

        res.StatusCode = statuscode;
        res.ContentEncoding = Encoding.UTF8;
        res.ContentType = "text/html";
        res.ContentLength64 = content.LongLength;
        await res.OutputStream.WriteAsync(content, 0, content.Length);
        res.Close();
    }

    public static async Task Redirect(HttpListenerRequest req, HttpListenerResponse res, Hashtable options, string location)
    {
        var redirectProps = (NameValueCollection?)options["redirect"] ?? new NameValueCollection();
        var query = new List<string>();
        var append = location.Contains('?') ? '&' : '?';

        foreach (var key in redirectProps.AllKeys)
        {
            if (key != null)
            {
                query.Add($"{HttpUtility.UrlEncode(key)}={HttpUtility.UrlEncode(redirectProps[key])}");
            }
        }

        res.Redirect(location + append + string.Join('&', query));
        res.Close();

        await Task.CompletedTask;
    }

    public static async Task ReadRequestFormData(HttpListenerRequest req, Hashtable options)
    {
        string type = req.ContentType ?? "";

        if (type.StartsWith("application/x-www-form-urlencoded"))
        {
            using var sr = new StreamReader(req.InputStream, Encoding.UTF8);
            string body = await sr.ReadToEndAsync();
            var formData = HttpUtility.ParseQueryString(body);

            options["req.form"] = formData;
        }
    }

    public static readonly NameValueCollection SUPPORTED_IANA_MIME_TYPES = new()
    {
        {".css", "text/css"},
        {".js", "text/javascript"}
    };

    public static async Task ServeStaticFile(HttpListenerRequest req, HttpListenerResponse res, Hashtable options)
    {
        string fileName = req.Url.AbsolutePath ?? "";
        string filepath = Path.Combine(Environment.CurrentDirectory, "static", fileName.Trim('/', '\\'));
        string fullPath = Path.GetFullPath(filepath);

        if (File.Exists(fullPath))
        {
            string ext = Path.GetExtension(fullPath);
            string type = SUPPORTED_IANA_MIME_TYPES[ext] ?? "application/octet-stream";
            using var fs = File.OpenRead(fullPath);

            res.StatusCode = (int)HttpStatusCode.OK;
            res.ContentType = type;
            res.ContentLength64 = fs.Length;

            await fs.CopyToAsync(res.OutputStream);
            res.Close();
        }
        
        await Task.CompletedTask;
    }
}