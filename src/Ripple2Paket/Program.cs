using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Xml;
using FubuCore;
using FubuCore.CommandLine;

namespace Ripple2Paket
{
    internal class Program
    {
        public const string RippleDependenciesConfig = "ripple.dependencies.config";
        private static bool success;

        public static int Main(string[] args)
        {
            var directory = args.FirstOrDefault();
            if (directory.IsEmpty()) throw new Exception("You must specify the codebase directory");

            if (!Directory.Exists(directory))
            {
                throw new Exception("The specified codebase directory {0} could not be found".ToFormat(directory));
            }

            directory = directory.ToFullPath();
            
            copyPaketExe(directory);
            
            var ripple = new XmlDocument();
            ripple.Load(directory.AppendPath("ripple.config"));
            
            var feeds = findFeeds(ripple).ToArray();

 
            var nugets = findNugets(ripple).ToArray();


            var dependencies = readRippleDependencies(directory);

            dependencies.Each(x => Console.WriteLine(x));

            
            var rippleFiles = findRippleFiles(directory);

            writePaketDependencies(directory, feeds, nugets);



            
            var cmd = writeInstallationCmd(directory, dependencies, rippleFiles);

            ConsoleWriter.Write(ConsoleColor.Green, "Writing installation command file " + cmd);

            

            return 0;
        }

        private static string writeInstallationCmd(string directory, IEnumerable<ProjectDependency> dependencies, IEnumerable<string> rippleFiles)
        {
            var file = directory.AppendPath("install-paket.cmd");

            new FileSystem().WriteToFlatFile(file, writer =>
            {
                rippleFiles.Each(f =>
                {
                    writer.WriteLine("del " + f);
                });

                writer.WriteLine("");

                writer.WriteLine("paket install");
                writer.WriteLine("");

                dependencies.Each(dep =>
                {
                    writer.WriteLine(dep.ToPaketAddCommand());
                });
            });

            return file;
        }


        private static IEnumerable<string> findRippleFiles(string directory)
        {
            var fileSystem = new FileSystem();
            var list = fileSystem.FindFiles(directory, FileSet.Deep("ripple.dependencies.config")).ToList();

            var rippleMain = directory.AppendPath("ripple.config");
            list.Add(rippleMain);

            return list;
        }

        private static void writePaketDependencies(string directory, IEnumerable<string> feeds,
            IEnumerable<Nuget> nugets)
        {
            var file = directory.AppendPath("paket.dependencies");
            new FileSystem().WriteToFlatFile(file, writer =>
            {
                feeds.Each(feed => writer.WriteLine("source " + feed));

                writer.WriteLine("");

                nugets.Each(nuget =>
                {
                    writer.WriteLine(nuget.PaketLine());
                });
            });
        }

        private static void copyPaketExe(string directory)
        {
            var paketPath = AppDomain.CurrentDomain.BaseDirectory.AppendPath("paket.exe");
            Console.WriteLine("Copying paket.exe to " + directory);
            new FileSystem().CopyToDirectory(paketPath, directory);
        }

        private static IEnumerable<ProjectDependency> readRippleDependencies(string directory)
        {
            var fileSystem = new FileSystem();
            var projectFiles = fileSystem.FindFiles(directory, FileSet.Deep("*.csproj"));
            return projectFiles.SelectMany(file =>
            {
                var projectName = Path.GetFileNameWithoutExtension(file);
                IList<string> names = null;

                fileSystem.AlterFlatFile(file.ParentDirectory().AppendPath(RippleDependenciesConfig), list => names = list.Where(x => x.IsNotEmpty()).ToList());

                return names.Select(x => new ProjectDependency {Nuget = x, Project = projectName});
            });
        }




        private static IEnumerable<Nuget> findNugets(XmlDocument ripple)
        {
            foreach (XmlElement element in ripple.DocumentElement.SelectNodes("//Dependency"))
            {
                yield return new Nuget
                {
                    Name = element.GetAttribute("Name"),
                    Version = element.GetAttribute("Version"),
                    Floated = element.GetAttribute("Mode") == "Float"
                };
            }
        }

        private static IEnumerable<string> findFeeds(XmlDocument ripple)
        {
            foreach (XmlElement elem in ripple.DocumentElement.SelectNodes("//Feed"))
            {
                yield return elem.GetAttribute("Url");
            }
        }
    }

    public class Nuget
    {
        public string Name;
        public string Version;
        public bool Floated;


        public override string ToString()
        {
            return string.Format("name: {0}, version: {1}, floated: {2}", Name, Version, Floated);
        }

        public string PaketLine()
        {
            return Floated 
                ? "nuget {0} ~> {1}".ToFormat(Name, Version) 
                : "nuget {0} {1}".ToFormat(Name, Version);
        }
    }

    public class ProjectDependency
    {
        public string Project;
        public string Nuget;

        public override string ToString()
        {
            return string.Format("project: {0}, nuget: {1}", Project, Nuget);
        }


        public string ToPaketAddCommand()
        {
            return "paket add nuget {0} project {1} --hard".ToFormat(Nuget, Project);
        }
    }
}