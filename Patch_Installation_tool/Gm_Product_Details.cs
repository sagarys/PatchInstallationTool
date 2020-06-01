using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patch_Installation_tool
{
    class Gm_Product_Details
    {
        string prodName;
        string osType;
        string calculusName;
        string oem;

        public Gm_Product_Details(string prodName, string osType, string calculusName, string oem)
        {
            this.prodName = prodName;
            this.osType = osType;
            this.calculusName = calculusName;
            this.oem = oem;
        }

        public string CalculusName { get => calculusName; set => calculusName = value; }
        public string OsType { get => osType; set => osType = value; }
        public string Oem { get => oem; set => oem = value; }
        public string ProdName { get => prodName; set => prodName = value; }
    }
}
