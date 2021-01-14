using System.Collections.Generic;
using iTextSharp.text;
using Nop.Core.Configuration;
using Nop.Plugin.Shipping.DPD.Domain;

namespace Nop.Plugin.Shipping.DPD
{
    /// <summary>
    /// Represents settings of the DPD shipping plugin
    /// </summary>
    public class DPDSettings : ISettings
    {
        public long ClientNumber { get; set; }
        public string ClientKey { get; set; }
        public bool UseSandbox { get; set; }
        public bool CargoRegistered { get; set; }
        public PickupTimePeriodType PickupTimePeriodType { get; set; }
        public string ServiceCodeType { get; set; }
        public string ServiceVariantType { get; set; }
        public PaymentType PaymentType { get; set; }
        public List<ServiceCode> ServiceCodes { get; set; }
    }
}