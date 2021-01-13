using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Shipping.DPD.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.DPD.Models
{
    public class DPDShippingModel : BaseNopModel
    {
        public DPDShippingModel()
        {
            AvailablePaymentTypes = new List<SelectListItem>();
            AvailablePickupTimePeriodTypes = new List<SelectListItem>();
            AvailableServiceCodeTypes = new List<SelectListItem>();
            AvailableServiceVariantTypes = new List<SelectListItem>();
        }
        
        
        public List<Language> Languages { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.ClientNumber")]
        public long ClientNumber { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.ClientKey")]
        public string ClientKey { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.CargoRegistered")]
        public bool CargoRegistered { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.PaymentType")]
        public int PaymentType { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.PickupTimePeriodType")]
        public int PickupTimePeriodType { get; set; }

        public IList<SelectListItem> AvailablePickupTimePeriodTypes { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.ServiceCodeType")]
        public IList<SelectListItem> AvailableServiceCodeTypes { get; set; }
        public IList<ServiceCode> ServiceCodes { get; set; }
        public IList<string> ServiceCodeTypes { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.ServiceCodeType")]
        public IList<SelectListItem> AvailableServiceVariantTypes { get; set; }
        public IList<string> ServiceVariantTypes { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.ServiceCodeType")]
        public int ServiceCodeType { get; set; }

        public IList<SelectListItem> AvailablePaymentTypes { get; set; }
    }
}