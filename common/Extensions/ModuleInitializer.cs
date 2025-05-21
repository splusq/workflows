using System.Runtime.CompilerServices;

public class ModuleInitializer
{
    [ModuleInitializer]
    public static void Load()
    {
        foreach(var file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.env", SearchOption.TopDirectoryOnly))
        {
            foreach (var line in File.ReadAllLines(file))
            {
                var parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                    continue;

                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }
    }
}