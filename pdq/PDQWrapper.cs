using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations;


namespace netPDQContainer
{
    public class PDQWrapper
    {
        private string _pdqExecutable;

        public PDQWrapper(string pdqExecutable)
        {
            if (!File.Exists(pdqExecutable))
            {
                throw new FileNotFoundException(pdqExecutable + " not found");
            }

            try
            {
                new FileIOPermission(FileIOPermissionAccess.Read, pdqExecutable).Demand();
            }
            catch (SecurityException s)
            {
                Console.WriteLine(s.Message);
            }

            _pdqExecutable = pdqExecutable;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// TODO: Integrate support for image resizing prior to passing to executable
        public PDQHashCalculation GetHash(string path)
        {
            var errorData = "";
            using (var p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = _pdqExecutable;
                p.StartInfo.Arguments = path;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                p.Start();
                
                errorData = p.StandardError.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(errorData))
                {
                    throw new Exception(errorData);
                }
                var returned = p.StandardOutput.ReadToEnd().Split(',');
                return new PDQHashCalculation(){Hash = returned[0], Quality = Convert.ToInt32(returned[1])};
            }
            
        }
}
}