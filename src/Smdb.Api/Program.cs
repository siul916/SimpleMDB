namespace Smdb.Api;
using System.IO;
using System.Runtime.Loader;
using System.Threading.Tasks;

public class Program
{
	public static async Task Main()
	{
		// Copy runtime dependencies to a temporary folder and load from there.
		// This allows the running process to use copies of the DLLs so the original
		// files in bin/Debug are not locked and can be overwritten by subsequent builds.
		var baseDir = AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
		var tempDir = Path.Combine(Path.GetTempPath(), "Smdb.Api.Deps");

		try
		{
			if (Directory.Exists(tempDir))
			{
				Directory.Delete(tempDir, recursive: true);
			}
		}
		catch
		{
			// ignore deletion errors - we'll try to create anyway
		}

		Directory.CreateDirectory(tempDir);

		foreach (var file in Directory.GetFiles(baseDir, "*.dll"))
		{
			var dest = Path.Combine(tempDir, Path.GetFileName(file));
			try
			{
				File.Copy(file, dest, overwrite: true);
			}
			catch
			{
				// ignore copy errors - best-effort
			}
		}

		AssemblyLoadContext.Default.Resolving += (context, assemblyName) =>
		{
			var candidate = Path.Combine(tempDir, assemblyName.Name + ".dll");
			if (File.Exists(candidate))
			{
				try
				{
					return context.LoadFromAssemblyPath(candidate);
				}
				catch
				{
					return null;
				}
			}
			return null;
		};

		App app = new App();
		await app.Start();
	}
}