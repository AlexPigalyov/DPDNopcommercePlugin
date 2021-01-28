using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core;

namespace Nop.Plugin.Shipping.DPD.Domain
{
    public class PickupPointAddress : BaseEntity
    {
        public int UserId { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string CountryName { get; set; }
        public string CustomerFullName { get; set; }
        public string TerminalCode { get; set; }
    }
}
