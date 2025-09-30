using System.Formats.Tar;
using System.Reflection.Emit;

namespace SimpleMDB;

public class ActorMovieHtmlTemplates
{
  public static string ViewAllMoviesByActor(Actor actor, List<(ActorMovie, Movie)> amms, int totalCount, int page, int size)
  {
    int pageCount = (int)Math.Ceiling((double)totalCount / size);

    string rows = "";

    foreach (var (am, movie) in amms)
    {
      rows += @$"
                <tr>
                   <td>{movie.Id}</td>
                   <td>{movie.Title}</td>
                   <td>{movie.Year}</td>
                   <td>{movie.Description}</td>
                   <td>{movie.Rating}</td>
                   <td>{am.RoleName}</td>
                   <td>
                   <form action=""/actors/movies/remove?amid={am.Id}"" method=""POST"" onsubmit=""return confirm('Are you sure that you want to delete this actor?')"">
                     <input type=""submit"" value=""Remove"">
                   </form>
                   </td>
                </tr>
                ";
    }

    string pDisable = (page > 1).ToString().ToLower();
    string nDisable = (page < pageCount).ToString().ToLower();

    string html = $@"
            <div class=""add"">
              <a href=""/actors/movies/add?aid={actor.Id}"">Add New ActorMovie</a>
            </div>
            <table class=""viewall"">
             <thead>
               <th>Id</th>
               <th>Title</th>
               <th>Year</th>
               <th>Description</th>
               <th>Rating</th>
               <th>Role Name</th>
               <th>Remove</th>
             </thead>
             <tbody>
               {rows}
             </tbody>
            </table>
            <div class=""pagination"">
              <a href=""?aid={actor.Id}&page=1&size={size}"" onclick=""return {pDisable};"">First</a>
              <a href=""?aid={actor.Id}&page={page - 1}&size={size}"" onclick=""return {pDisable};"">Prev</a>
              <span>{page} / {pageCount}</span>
              <a href=""?aid={actor.Id}&page={page + 1}&size={size}"" onclick=""return {nDisable};"">Next</a>
              <a href=""?aid={actor.Id}&page={pageCount}&size={size}"" onclick=""return {nDisable};"">Last</a>
            </div>
            ";
    return html;
  }

  public static string ViewAllActorsByMovie(Movie movie, List<(ActorMovie, Actor)> amas, int totalCount, int page, int size)
  {
    int pageCount = (int)Math.Ceiling((double)totalCount / size);

    string rows = "";

    foreach (var (am, actor) in amas)
    {
      rows += @$"
                <tr>
                   <td>{actor.Id}</td>
                   <td>{actor.FirstName}</td>
                   <td>{actor.LastName}</td>
                   <td>{actor.Bio}</td>
                   <td>{actor.Rating}</td>
                   <td>{am.RoleName}</td>
                   <td>
                   <form action=""/movies/actors/remove?amid={am.Id}"" method=""POST"" onsubmit=""return confirm('Are you sure that you want to delete this actor?')"">
                     <input type=""submit"" value=""Remove"">
                   </form>
                   </td>
                </tr>
                ";
    }

    string pDisable = (page > 1).ToString().ToLower();
    string nDisable = (page < pageCount).ToString().ToLower();

    string html = $@"
            <div class=""add"">
              <a href=""/movies/actors/add?mid={movie.Id}"">Add New ActorMovie</a>
            </div>
            <table class=""viewall"">
             <thead>
               <th>Id</th>
               <th>First Name</th>
               <th>Last Name</th>
               <th>Bio</th>
               <th>Rating</th>
               <th>Role Name</th>
               <th>Remove</th>
             </thead>
             <tbody>
               {rows}
             </tbody>
            </table>
            <div class=""pagination"">
              <a href=""?mid={movie.Id}&page=1&size={size}"" onclick=""return {pDisable};"">First</a>
              <a href=""?mid={movie.Id}&page={page - 1}&size={size}"" onclick=""return {pDisable};"">Prev</a>
              <span>{page} / {pageCount}</span>
              <a href=""?mid={movie.Id}&page={page + 1}&size={size}"" onclick=""return {nDisable};"">Next</a>
              <a href=""?mid={movie.Id}&page={pageCount}&size={size}""  onclick=""return {nDisable};"">Last</a>
            </div>
            ";
    return html;
  }

  public static string AddMoviesByActor(Actor actor, List<Movie> movies)
  {
    string movieOptions = "";

    foreach (var movie in movies)
    {
      movieOptions += $@" <option value=""{movie.Id}"">{movie.Title} ({movie.Year})</option>";
    }

    string html = $@"
      <form action=""/actors/movies/add"" method=""POST"">
      <label for=""aid"">Actor</label>
      <select id=""aid"" name=""aid"">
        <option value=""{actor.Id}"">{actor.FirstName} {actor.LastName}</option>
      </select>
      <label for=""mid"">Movies</label>
       <select id=""mid"" name=""mid"">
       {movieOptions}
       </select>
       <label for=""rolename"">Role Name</label>
       <input id=""rolename"" name=""rolename"" type=""text"" placeholder=""Role Name"">
       <input type=""submit"" value=""Add"">
       </form>
    ";
    return html;
  }

  public static string AddActorsByMovie(Movie movie, List<Actor> actors)
  {
    string actorOptions = "";

    foreach (var actor in actors)
    {
      actorOptions += $@" <option value=""{actor.Id}"">{actor.FirstName} {actor.LastName}</option>";
    }

    string html = $@"
      <form action=""/movies/actors/add"" method=""POST"">
      <label for=""mid"">Movie</label>
      <select id=""mid"" name=""mid"">
        <option value=""{movie.Id}"">{movie.Title} ({movie.Year})</option>
      </select>
      <label for=""aid"">Actors</label>
       <select id=""aid"" name=""aid"">
       {actorOptions}
       </select>
       <label for=""rolename"">Role Name</label>
       <input id=""rolename"" name=""rolename"" type=""text"" placeholder=""Role Name"">
       <input type=""submit"" value=""Add"">
       </form>
    ";
    return html;
  }
}

