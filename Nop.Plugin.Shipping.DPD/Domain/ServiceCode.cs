using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Shipping.DPD.Domain
{
    public class ServiceCode
    { 
        public bool IsTTActive { get; set; }
        public bool IsTDActive { get; set; }
        public string Code { get; set; }
    }
}
