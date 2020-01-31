using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXConverter {
   public class ParametersParser {
        public ParametersParser(string[] args) {

        }
        public string ProjectPath{ get; set; }
        public string Version{ get; set; }
        public bool WaitForExit{ get; set; }
        public string InstalledVersionPath{ get; set; }
        public string IsLocalCacheUsed{ get; set; }
    }
}
