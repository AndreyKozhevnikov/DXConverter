using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DXConverter {
    public interface IWorkWithFile {
        List<string> GetRegistryVersions(string path);
        string AssemblyLoadFileFullName(string path);
    }
    public class CustomWorkWithFile : IWorkWithFile {
        public List<string> GetRegistryVersions(string path) {
            var regKey = Registry.LocalMachine.OpenSubKey(path);
            var lst = regKey.GetSubKeyNames();
            List<string> resList = new List<string>();
            foreach(string st in lst) {
                RegistryKey dxVersionKey = regKey.OpenSubKey(st);
                string projectUpgradeToolPath = dxVersionKey.GetValue("RootDirectory") as string;
                resList.Add(projectUpgradeToolPath);
            }
            return resList;
        }
        public string AssemblyLoadFileFullName(string path) {
            try {
                var assembly = Assembly.LoadFile(path);
                return assembly.FullName;
            }
            catch {
                return null;
            }
        }
    }
}
