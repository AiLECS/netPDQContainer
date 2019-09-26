using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace netPDQContainer
{
    /// <summary>
    /// Wrapper class for PDQ hash provider.
    /// </summary>
    public class PDQWrapper
    {
        private readonly string _pdqExecutable;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pdqExecutable">Path (absolute or relative) of PDQ hash executable.</param>
        /// <exception cref="FileNotFoundException">PDQ hash executable not found.</exception>
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
        /// Calculate PDQhash for given file path.
        /// </summary>
        /// <param name="path">Absolute/relative path of file for hashing</param>
        /// <returns><see cref="PDQHashCalculation"/> for provided image file</returns>
        /// <exception cref="Exception">PDQ hash attempt failed. Returns stderr output as part of exception thrown.</exception>
        /// TODO: Integrate support for image resizing prior to passing to executable
        public PDQHashCalculation GetHash(string path)
        {
            using (var p = new Process())
            {
                var errorData = "";
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