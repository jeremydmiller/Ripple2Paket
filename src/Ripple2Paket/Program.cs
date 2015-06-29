using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Xml;
using FubuCore;

namespace Ripple2Paket
{
    internal class Program
    {
        private static bool success;

        public static int Main(string[] args)
        {
            var directory = args.FirstOrDefault();
            if (directory.IsEmpty()) throw new Exception("You must specify the codebase directory");

            if (!Directory.Exists(directory))
            {
                throw new Exception("The specified codebase directory {0} could not be found".ToFormat(directory));
            }
            
            copyPaketExe(directory);
            return 0;
            var ripple = new XmlDocument();
            ripple.Load(directory.AppendPath("ripple.config"));

            var feeds = findFeeds(ripple);
            var nugets = findNugets(ripple);


            

            var dependencies = readRippleDependencies(directory);

            
            
            
            removeRippleFiles(directory);

            writePaketDependencies(directory, feeds, nugets);
            var cmd = writeInstallationCmd(directory, dependencies);

            executeInstallation(cmd);

            return 0;
        }

        private static void executeInstallation(string cmd)
        {
            throw new NotImplementedException();
        }

        private static string writeInstallationCmd(string directory, IEnumerable<ProjectDependency> dependencies)
        {
            throw new NotImplementedException();
        }

        private static void copyPaketExe(string directory)
        {
            var paketPath = AppDomain.CurrentDomain.BaseDirectory.AppendPath("paket.exe");
            Console.WriteLine("Copying paket.exe to " + directory);
            new FileSystem().CopyToDirectory(paketPath, directory);
        }

        private static IEnumerable<ProjectDependency> readRippleDependencies(string directory)
        {
            throw new NotImplementedException();
        }

        private static void removeRippleFiles(string directory)
        {
            throw new NotImplementedException();
        }

        private static void writePaketDependencies(string directory, IEnumerable<string> feeds, IEnumerable<Nuget> nugets)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<Nuget> findNugets(XmlDocument ripple)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<string> findFeeds(XmlDocument ripple)
        {
            throw new NotImplementedException();
        }
    }

    public class Nuget
    {
        public string Name;
        public string Version;
        public bool Floated;
    }

    public class ProjectDependency
    {
        public string Project;
        public string Nuget;
    }
}