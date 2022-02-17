using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
    internal static class DiagnosticHelper
    {
        internal static void FlushToDisk(string jsonContent)
        {
            Task.Run(async () =>
            {
                try
                {
                    var tempPath = System.IO.Path.GetTempPath();
                    var logPath = System.IO.Path.Combine(tempPath, "ariane-diagnostics");
                    if (!System.IO.Directory.Exists(logPath))
                    {
                        System.IO.Directory.CreateDirectory(logPath);
                    }
                    var logFileName = System.IO.Path.Combine(logPath, $"{Guid.NewGuid()}.json");
                    await System.IO.File.WriteAllTextAsync(logFileName, jsonContent);
                }
                catch
                {
                }
            });
        }
    }
}
