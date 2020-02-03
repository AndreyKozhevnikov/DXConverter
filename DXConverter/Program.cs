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
            ParametersParser parser = new ParametersParser(args);
            if(parser.IsArgumentsCorrect) {
                ConvertProject(parser.ProjectPath, parser.Version, parser.InstalledVersionPath, parser.IsLocalCacheUsed);
                if(parser.IsWaitForExit) {
                    Console.WriteLine("Finish");
                    Console.Read();
                }
            } else {
                Console.WriteLine("Wrong arguments");
                Console.WriteLine(string.Join("\r\n", args));
                Console.Read();
            }
        }

        private static void ConvertProject(string projPath, string vers, string installedPath, bool isLocalCache) {
            AssemblyConverter a = new AssemblyConverter();
            a.CustomFileDirectoriesObject = new CustomFileDirectoriesClass();
            a.ProjectConverterProcessorObject = new ProjectConverterProcessor();
            a.MessageProcessor = new ConsoleMessageProcessor();
            a.MyWorkWithFile = new CustomWorkWithFile();
            a.ProcessProject(projPath, vers, installedPath, isLocalCache);
            Console.WriteLine("end");
        }

    }



}
