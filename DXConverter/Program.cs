using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DXConverter {
    class Program {
        static void Main(string[] args) {
            var cnt = args.Count();
            if(cnt == 3 || cnt == 4) {
                bool waitForExit = bool.Parse(args[2]);

              
                string installedPath = null;
                if(cnt == 4) {
                    installedPath = args[3];
                }
                ConvertProject(args[0], args[1],  installedPath);
                if(waitForExit) {
                    Console.WriteLine("Finish");
                    Console.Read();
                }

            } else {
                Console.WriteLine("Wrong arguments");
                Console.WriteLine(string.Join("\r\n", args));
                Console.Read();
            }
        }

        private static void ConvertProject(string projPath, string vers,  string installedPath) {
            AssemblyConverter a = new AssemblyConverter();
            a.CustomFileDirectoriesObject = new CustomFileDirectoriesClass();
            a.ProjectConverterProcessorObject = new ProjectConverterProcessor();
            a.MessageProcessor = new ConsoleMessageProcessor();
            a.MyWorkWithFile = new CustomWorkWithFile();
            a.ProcessProject(projPath, vers,  installedPath);
            Console.WriteLine("end");
        }
    }

}
