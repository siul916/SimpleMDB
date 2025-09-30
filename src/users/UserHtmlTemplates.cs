using System.Dynamic;

namespace SimpleMDB;

public class UserHtmlTemplates
{
  public static string ViewAllUsersGet(List<User> users, int userCount, int size, int page)
  {
    int pageCount = (int)Math.Ceiling((double)userCount / size);


    string rows = "";

    foreach (var user in users)
    {
      rows += @$"
                <tr>
                   <td>{user.Id}</td>
                   <td>{user.Username}</td>
                   <td>{user.Password}</td>
                   <td>{user.Salt}</td>
                   <td>{user.Role}</td>
                   <td><a href=""/users/view?uid={user.Id}"">View</a></td>
                   <td><a href=""/users/edit?uid={user.Id}"">Edit</a></td>
                   <td><form action=""/users/remove?uid={user.Id}"" method= ""POST"" onsubmit=""return confirm('Are you sure that you want to delete this user?')"">
                     <input type= ""submit"" value=""Remove"">
                   </form>
                   </td>
                </tr>

                ";

    }
      string pDisable = (page > 1).ToString().ToLower();
      string nDisable = (page < pageCount).ToString().ToLower();

    string html = $@"
            <div class=""add"">
            <a  href=""/users/add"">Add New User</a>
            </div>
            <table class""viewall"">
             <thead>
               <th>Id</th>
               <th>Username</th>
               <th>Password</th>
               <th>Salt</th>
               <th>Role</th>
               <th>View</th>
               <th>Edit</th>
               <th>Remove</th>
             </thead>
             <tbody>
               {rows}
             </tbody>
            </table>
            <div class=""pagination"">
              <a href=""?page=1&size={size}"" onclick=""return {pDisable};"">First</a>
              <a href=""?page={page - 1}&size={size}"" onclick=""return {pDisable};"">Prev</a>
              <span>{page} / {pageCount}</span>
              <a href=""?page={page + 1}&size={size}"" onclick=""return {nDisable};"">Next</a>
              <a href=""?page={pageCount}&size={size}"" onclick=""return {nDisable};"">Last</a>


            </div>


            ";
    return html;
  }

  public static string AddUserGet(string username, string role)
  {
    string roles = "";


    foreach (var r in Roles.ROLES)
    {
      string selected = r == role ? " selected" : "";
      roles += $@"<option value=""{r}""{selected}>{r}</option>";
    }
    string html = $@"
            <form class=""addform"" action=""/users/add"" method=""POST"">
              <label for=""username"">Username</label>
              <input id=""username"" name=""username"" type= ""text"" placeholder=""Username"" value=""{username}"">
              <label for=""password"">Password</label>
              <input id=""password"" name=""password"" type= ""text"" placeholder=""Password"">
              <label for=""role"">Role</label>
              <select id= ""role"" name= ""role"">
                {roles}
              </select>
              <input type=""submit"" value=""Add"">
            </form>

            ";
    return html;

  }

  public static string ViewUserGet(User user)
  {
    string html = $@"
            <table class=""view"">
             <thead>
               <th>Id</th>
               <th>Username</th>
               <th>Password</th>
               <th>Salt</th>
               <th>Role</th>
             </thead>
             <tbody>
              <tr>
                <td>{user.Id}</td>
                <td>{user.Username}</td>
                <td>{user.Password}</td>
                <td>{user.Salt}</td>
                <td>{user.Role}</td>
                </tr>
             </tbody>
            </table>


            ";
    return html;
  }

  public static string EditUserGet(int uid, User user)
  {
    string roles = "";



    foreach (var role in Roles.ROLES)
    {
      string selected = (role == user.Role) ? " selected" : "";
      roles += $@"<option value=""{role}""{selected}>{role}</option>";
    }

    string html = $@"
            <form class=""editform"" action=""/users/edit?uid={uid}"" method=""POST"">
              <label for=""username"">Username</label>
              <input id=""username"" name=""username"" type= ""text"" placeholder=""Username"" value=""{user.Username}"">
              <label for=""password"">Password</label>
              <input id=""password"" name=""password"" type= ""text"" placeholder=""Password"" value=""{user.Password}"">
              <label for=""role"">Role</label>
              <select id= ""role"" name= ""role"">
                {roles}
              </select>
              <input type=""submit"" value=""Edit"">
            </form>
            ";
    return html;
  }
}
