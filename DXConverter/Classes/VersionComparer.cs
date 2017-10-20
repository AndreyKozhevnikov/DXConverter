using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DXConverter {
    public class VersionComparer : IComparer<string> {
        public int Compare(string x, string y) {
            int counter = 0, res = 0;
            while(counter < 3 && res == 0) {
                int versionX = Convert.ToInt32(x.Split('.')[counter]);
                int versionY = Convert.ToInt32(y.Split('.')[counter]);
                res = Comparer.Default.Compare(versionX, versionY);
                counter++;
            }
            return -res;
        }
    }
}
