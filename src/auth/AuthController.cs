using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.Tracing;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace SimpleMDB;

public class AuthController
{
  private IUSerService userService;
  public AuthController(IUSerService userService)
  {
    this.userService = userService;

  }

  public async Task LandingPageGet(HttpListenerRequest req, HttpListenerResponse res, Hashtable options)
  {
    string message = req.QueryString["message"] ?? "";
    string html = $@"
        <nav>
          <ul>
            <li><a href= ""/register"">Register</a></li>
            <li><a href= ""/login"">Login</a></li>
            <li><form action=""/logout"" method=""POST"">
            <input type=""submit"" value=""Logout"">
            </form>
            </li>
            <li><a href= ""/users"">Users</a></li>
            <li><a href= ""/actors"">Actors</a></li>
            <li><a href= ""/movies"">Movies</a></li>
          </ul>

        </nav>
        ";

    string content = HtmlTemplates.Base("SimpleMDB", "Landing Page", html, message);
    await HttpUtils.Respond(req, res, options, (int)HttpStatusCode.OK, content);

  }

  //GET /register
  public async Task RegisterGet(HttpListenerRequest req, HttpListenerResponse res, Hashtable options)
  {
    var returnUrl = req.QueryString["returnUrl"] ?? "";
    var rQuery = string.IsNullOrWhiteSpace(returnUrl) ? "" : $"?returnUrl={HttpUtility.UrlEncode(returnUrl)}";

    string message = req.QueryString["message"] ?? "";
    string username = req.QueryString["username"] ?? "";

    string html = $@"
    <form action=""/register{rQuery}"" method=""POST"">
      <label for=""username"">Username</label>
      <input id=""username"" name=""username"" type=""text"" placeholder=""Username"" value=""{username}"">
      <label for=""password"">Password</label>
      <input id=""password"" name=""password"" type=""password"" placeholder=""Password"">
      <label for=""cpassword"">Confirm Password</label>
      <input id=""cpassword"" name=""cpassword"" type=""password"" placeholder=""Confirm Password"">
      <input type=""submit"" value=""Register"">
      </form>
    ";
    string content = HtmlTemplates.Base("SimpleMDB", "Register Page", html, message);
    await HttpUtils.Respond(req, res, options, (int)HttpStatusCode.OK, content);

  }

  //POST /register
  public async Task RegisterPost(HttpListenerRequest req, HttpListenerResponse res, Hashtable options)
  {
    var returnUrl = req.QueryString["returnUrl"] ?? "";


    var formData = (NameValueCollection?)options["req.form"] ?? [];
    var username = formData["username"] ?? "";
    var password = formData["password"] ?? "";
    var cpassword = formData["cpassword"] ?? "";

    if (password != cpassword)
    {
      HttpUtils.AddOptions(options, "redirect", "message", "Passwords do not match.");
      HttpUtils.AddOptions(options, "redirect", "username", username);
      HttpUtils.AddOptions(options, "redirect", "returnUrl", returnUrl);
      await HttpUtils.Redirect(req, res, options, $"/register");
    }
    else
    {
      User newUser = new User(0, username, password, "", "");
      var result = await userService.Create(newUser);

      if (result.IsValid)
      {
        HttpUtils.AddOptions(options, "redirect", "message", "User registered successfully!");
        HttpUtils.AddOptions(options, "redirect", "username", username);
        HttpUtils.AddOptions(options, "redirect", "returnUrl", returnUrl);

        await HttpUtils.Redirect(req, res, options, $"/login");

      }
      else
      {
        HttpUtils.AddOptions(options, "redirect", "message", result.Error!.Message);
        HttpUtils.AddOptions(options, "redirect", "returnUrl", returnUrl);

        await HttpUtils.Redirect(req, res, options, $"/register");

      }
    }
  }

  //GET /login
  public async Task LoginGet(HttpListenerRequest req, HttpListenerResponse res, Hashtable options)
  {
    var returnUrl = req.QueryString["returnUrl"] ?? "";
    var rQuery = string.IsNullOrWhiteSpace(returnUrl) ? "" : $"?returnUrl={HttpUtility.UrlEncode(returnUrl)}";
    string message = req.QueryString["message"] ?? "";
    string username = req.QueryString["username"] ?? "";

    string html = $@"
    <form action=""/login{rQuery}"" method=""POST"">
      <label for=""username"">Username</label>
      <input id=""username"" name=""username"" type=""text"" placeholder=""Username"" value=""{username}"">
      <label for=""password"">Password</label>
      <input id=""password"" name=""password"" type=""password"" placeholder=""Password"">
      <input type=""submit"" value=""Login"">
      </form>
    ";
    string content = HtmlTemplates.Base("SimpleMDB", "Login Page", html, message);
    await HttpUtils.Respond(req, res, options, (int)HttpStatusCode.OK, content);

  }


  //POST /login
  public async Task LoginPost(HttpListenerRequest req, HttpListenerResponse res, Hashtable options)
  {
    var returnUrl = req.QueryString["returnUrl"] ?? "/";


    var formData = (NameValueCollection?)options["req.form"] ?? [];
    var username = formData["username"] ?? "";
    var password = formData["password"] ?? "";

    var result = await userService.GetToken(username, password);

    if (result.IsValid)
    {
      string token = result.Value!;
      HttpUtils.AddOptions(options, "redirect", "message", "User logged in successfully!");
      res.SetCookie(new Cookie("token", result.Value!, "/"));

      res.AddHeader("Authorization", $"Bearer {token!}");

      await HttpUtils.Redirect(req, res, options, $"{returnUrl}");

    }
    else
    {
      HttpUtils.AddOptions(options, "redirect", "message", result.Error!.Message);
      HttpUtils.AddOptions(options, "redirect", "username", username);
      HttpUtils.AddOptions(options, "redirect", "returnUrl", returnUrl);

      await HttpUtils.Redirect(req, res, options, "/login");

    }

  }

  //POST /logout
  public async Task LogoutPost(HttpListenerRequest req, HttpListenerResponse res, Hashtable options)
  {
    res.AddHeader("Set-Cookie", "token=; Max-Age=0; Path=/");
    res.AddHeader("WWW-Authenticate", @"Bearer error=""invalid_token"", error_description=""The user logged out.""");
    HttpUtils.AddOptions(options, "redirect", "message", "User logged out successfully!");
    await HttpUtils.Redirect(req, res, options, "/login");
  }


  public async Task CheckAuth(HttpListenerRequest req, HttpListenerResponse res, Hashtable options)
  {
    string token = req.Headers["Authorization"]?.Substring(7) ?? req.Cookies["token"]?.Value ?? "";
    var result = await userService.ValidateToken(token);

    if (result.IsValid)
    {

      options["claims"] = result.Value!;

    }
    else
    {
      HttpUtils.AddOptions(options, "redirect", "message", result.Error!.Message);

      await HttpUtils.Redirect(req, res, options, "/login");
    }
  }

  public async Task CheckAdmin(HttpListenerRequest req, HttpListenerResponse res, Hashtable options)
  {
    string token = req.Headers["Authorization"]?.Substring(7) ?? req.Cookies["token"]?.Value ?? "";
    var result = await userService.ValidateToken(token);


    if (result.IsValid)

    {
      if (result.Value!["role"] == Roles.ADMIN)
      {
        options["claims"] = result.Value!;
      }
      else
      {
        HttpUtils.AddOptions(options, "redirect", "message", "Autheticated but not authorized. You must be an administrator to access this resource.");

        await HttpUtils.Redirect(req, res, options, "/login");
      }

    }
    else
    {
     HttpUtils.AddOptions(options, "redirect", "message", result.Error!.Message);

     await HttpUtils.Redirect(req, res, options, "/login");
    }
  }



}
