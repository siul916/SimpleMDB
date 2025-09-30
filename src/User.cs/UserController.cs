using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace SimpleMDB;

public class UserController
{
    private IUserService userService;

    public UserController(IUserService userService)
    {
        this.userService = userService;
    }


        //GET /users?page=1&size=5
    public async Task ViewAllGet(HttpListenerRequest request, HttpListenerResponse response, Hashtable options)
    {
    string message = request.QueryString["message"] ?? "";
        int page = int.TryParse(request.QueryString["page"], out int p) ? p : 1;
        int size = int.TryParse(request.QueryString["size"], out int s) ? s : 5;

    Result<PagedResult<User>> result = await userService.ReadAll(page, size);

        if (result.IsValid)
        {
            PagedResult<User> pagedResult = result.Value!;
            List<User> users = pagedResult.Values;
            int userCount = pagedResult.TotalCount;

            string html = UserHtmlTemplates.ViewAllUsersGet(users, size, userCount, page);

            string content = HtmlTemplates.Base("SimpleMDB", " Users View All Page", html, message);
            await HttpUtils.Respond(request, response, options, (int)HttpStatusCode.OK, content);
        }
        else
        {
            // On error, return the error message
            var errorHtml = $"<p>Error: {result.Error?.Message}</p>";
            await HttpUtils.Respond(request, response, options, (int)HttpStatusCode.InternalServerError, errorHtml);
        }

    }

    // GET/users/add
    public async Task AddGet(HttpListenerRequest request, HttpListenerResponse response, Hashtable options)
    {
        string username = request.QueryString["username"] ?? "";
        string selectedRole = request.QueryString["role"] ?? "";
        string message = request.QueryString["message"] ?? "";
       
    string html = UserHtmlTemplates.AddUserGet(username, selectedRole);
        string content = HtmlTemplates.Base("SimpleMDB", " Users Add Page", html, message);
       
        await HttpUtils.Respond(request, response, options, (int)HttpStatusCode.OK, content);
    }

    //POST /users/add
    public async Task AddPost(HttpListenerRequest request, HttpListenerResponse response, Hashtable options)
    {
        var formData = options["req.form"] as Dictionary<string, string> ?? new Dictionary<string, string>();

        formData.TryGetValue("username", out var username);
        formData.TryGetValue("password", out var password);
        formData.TryGetValue("role", out var role);

        username ??= string.Empty;
        password ??= string.Empty;
        role ??= string.Empty;
        
        User newUser = new User(0, username, password, "", role);

        Result<User> result = await userService.Create(newUser);

        if (result.IsValid)
        {
            // Set a top-level message used by Redirect
            options["message"] = "User added successfully.";

            // On success, redirect to /users
           await HttpUtils.Redirect(request, response, options, "/users");
        }
        else
        {
            // Preserve message and form data so the add form can show them
            var redirect = new System.Collections.Specialized.NameValueCollection();
            redirect["message"] = result.Error!.Message;
            if (!string.IsNullOrEmpty(username)) redirect["username"] = username;
            if (!string.IsNullOrEmpty(role)) redirect["role"] = role;

            options["redirect"] = redirect;

            // Redirect back to the add user form with error message and preserved fields
            await HttpUtils.Redirect(request, response, options, "/users/add");
        }
        
    }

    // GET /users/view?uid=1
    public async Task ViewUserGet(HttpListenerRequest request, HttpListenerResponse response, Hashtable options)
    {
        string message = request.QueryString["message"] ?? "";
        int uid = int.TryParse(request.QueryString["uid"], out int u) ? u : 1;

        Result<User> result = await userService.Read(uid);

        if (result.IsValid)
        {
            User user = result.Value!;

            string html = UserHtmlTemplates.ViewUserGet(user);

            string content = HtmlTemplates.Base("SimpleMDB", " Users View Page", html, message);
            await HttpUtils.Respond(request, response, options, (int)HttpStatusCode.OK, content);
        }
        else
        {
            var errorHtml = $"<p>Error: {result.Error?.Message}</p>";
            await HttpUtils.Respond(request, response, options, (int)HttpStatusCode.NotFound, errorHtml);
        }
    }

    // GET /users/edit?uid=1

    public async Task EditGet(HttpListenerRequest request, HttpListenerResponse response, Hashtable options)
    {
    string message = request.QueryString["message"] ?? "";

        int uid = int.TryParse(request.QueryString["uid"], out int u) ? u : 1;

        Result<User> result = await userService.Read(uid);

        if (result.IsValid)
        {
            User user = result.Value!;
        

    string html = UserHtmlTemplates.EditUserGet(uid, user);

        string content = HtmlTemplates.Base("SimpleMDB", " Users Edit Page", html, message);
        await HttpUtils.Respond(request, response, options, (int)HttpStatusCode.OK, content);
        }
    
    }

    //POST /users/edit?uid=1
    public async Task EditPost(HttpListenerRequest request, HttpListenerResponse response, Hashtable options)
    {
         int uid = int.TryParse(request.QueryString["uid"], out int u) ? u : 1;

    var formData = options["req.form"] as Dictionary<string, string> ?? new Dictionary<string, string>();

    formData.TryGetValue("username", out var username);
    formData.TryGetValue("password", out var password);
    formData.TryGetValue("role", out var role);

        username ??= string.Empty;
        password ??= string.Empty;
        role ??= string.Empty;

        User newUser = new User(0, username, password, "", role);

        Result<User> result = await userService.Update(uid, newUser);

        if (result.IsValid)
        {
            options["message"] = "User edited successfully.";

            // On success, redirect to /users
           await HttpUtils.Redirect(request, response, options, "/users");
        }
        else
        {
            // Preserve message and form data so the edit form can show them
            var redirect = new System.Collections.Specialized.NameValueCollection();
            redirect["message"] = result.Error!.Message;
            // include uid so the edit page knows which user to show
            redirect["uid"] = uid.ToString();
            if (!string.IsNullOrEmpty(username)) redirect["username"] = username;
            if (!string.IsNullOrEmpty(role)) redirect["role"] = role;

            options["redirect"] = redirect;

            // Redirect back to the edit user form with error message and preserved fields
            await HttpUtils.Redirect(request, response, options, "/users/edit");
        }
        
    }

    // GET /users/remove?uid=1

    public async Task RemovePost(HttpListenerRequest request, HttpListenerResponse response, Hashtable options)
    {
        int uid = int.TryParse(request.QueryString["uid"], out int u) ? u : 1;

        Result<User> result = await userService.Delete(uid);

        if (result.IsValid)
        {
            options["message"] = "User removed successfully.";

            // On success, redirect to /users
           await HttpUtils.Redirect(request, response, options, "/users");
        }
        else
        {
            options["message"] = result.Error!.Message;

            // Redirect back to the add user form with error message
               await HttpUtils.Redirect(request, response, options, "/users");
        }
    }
}
