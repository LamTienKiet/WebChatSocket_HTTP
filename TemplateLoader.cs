using System;
using System.Collections.Generic;
using System.IO;

namespace WebSocketTest
{
    public static class TemplateLoader
    {
        public static string Load(string name)
        {
            string baseDir = AppContext.BaseDirectory;

            string path = Path.Combine(baseDir, "Views", name);

            Console.WriteLine("📂 Load template: " + path);

            if (!File.Exists(path))
            {
                throw new Exception("❌ Template not found: " + path);
            }

            return File.ReadAllText(path);
        }

        public static string Render(string template, Dictionary<string, string> data)
        {
            foreach (var kv in data)
            {
                template = template.Replace($"{{{{{kv.Key}}}}}", kv.Value ?? "");
            }
            return template;
        }
    }
}