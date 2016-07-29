using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DXConverter {
    public interface ICustomFileDirectories {
        string[] GetDirectories(string path);
        string[] GetFiles(string path, string pattern);
        bool IsFileExist(string path);
        bool IsDirectoryExist(string path);
        DirectoryInfo CreateDirectory(string path);
        void FileCopy(string source, string desctination, bool overwrite);
        void SaveXDocument(XDocument projDocument, string projectPath);
        XDocument LoadXDocument(string projectPath);
        void WriteTextInFile(string _file, string _text);
        string GetStringFromFile(string _fileName);
    }
    public class CustomFileDirectoriesClass : ICustomFileDirectories {

        public string[] GetDirectories(string path) {
            return Directory.GetDirectories(path);
        }
        public string[] GetFiles(string path, string pattern) {
            if (Directory.Exists(path))
            return Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
            return new string[0];
        }

        public bool IsFileExist(string path) {
            return File.Exists(path);
        }


        public bool IsDirectoryExist(string path) {
            return Directory.Exists(path);
        }


        public DirectoryInfo CreateDirectory(string path) {
            return Directory.CreateDirectory(path);
        }


        public void FileCopy(string source, string desctination, bool overwrite) {
            File.Copy(source, desctination, overwrite);
        }


        public void SaveXDocument(XDocument projDocument, string projectPath) {
            projDocument.Save(projectPath);
        }


        public XDocument LoadXDocument(string projectPath) {
            return XDocument.Load(projectPath);
        }

        public void WriteTextInFile(string _file, string _text) {
            StreamWriter sw = new StreamWriter(_file, false);
            sw.Write(_text);
            sw.Close();
        }

        public string GetStringFromFile(string _fileName) {
            if (File.Exists(_fileName)) {
                var sr = new StreamReader(_fileName);
                var st = sr.ReadToEnd();
                sr.Close();
                return st;
            }
            return null;

        }
    }

}
