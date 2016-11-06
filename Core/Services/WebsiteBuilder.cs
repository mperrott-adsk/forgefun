using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ForgeFun.Core.Services
{
    public class WebsiteBuilder
    {
        public void Build(string base64Urn, string viewerAuthToken, string outputHtmlPath)
        {
            var templateContents = File.ReadAllText(Path.Combine(AssemblyDirectory, "Templates", "template.html"));
            var websiteContent = templateContents.Replace("{urn}", base64Urn).Replace("{accessToken}", viewerAuthToken);
            File.WriteAllText(outputHtmlPath,websiteContent);
        }

        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
