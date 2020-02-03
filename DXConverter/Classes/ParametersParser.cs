using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXConverter {
    public class ParametersParser {
        public ParametersParser(string[] args) {
            var cnt = args.Count();
            if(cnt >= 3 && cnt <= 5) {
                ProjectPath = args[0];
                Version = args[1];
                IsWaitForExit = bool.Parse(args[2]);
                if(cnt == 4) {
                    bool tmpBool = false;
                    var isBool = bool.TryParse(args[3], out tmpBool);
                    if(isBool) {
                        IsLocalCacheUsed = tmpBool;
                    } else {
                        InstalledVersionPath = args[3];
                    }
                }
                if(cnt == 5) {
                    InstalledVersionPath = args[3];
                    IsLocalCacheUsed = bool.Parse(args[4]);
                }
                IsArgumentsCorrect = true;
            }
        }
        public string ProjectPath { get; set; }
        public string Version { get; set; }
        public bool IsWaitForExit { get; set; }
        public string InstalledVersionPath { get; set; }
        public bool IsLocalCacheUsed { get; set; }

        public bool IsArgumentsCorrect { get; set; }
    }
}
