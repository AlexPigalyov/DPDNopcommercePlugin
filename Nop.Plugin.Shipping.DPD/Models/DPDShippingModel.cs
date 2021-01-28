using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
            AvailableServiceCodeTypes = new List<SelectListItem>();
            AvailableServiceVariantTypes = new List<SelectListItem>();
        }
        [Required]
        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.ClientNumber")]
        public long ClientNumber { get; set; }
        [Required]
        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.ClientKey")]
        public string ClientKey { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        [Required]
        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.AddressCode")]
        public string AddressCode { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.CargoRegistered")]
        public bool CargoRegistered { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.ServiceCodeTypes")]
        public IList<SelectListItem> AvailableServiceCodeTypes { get; set; } 
        public IList<string> ServiceCodeTypes { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.DPD.Fields.AvailableServiceVariantTypes")]
        public IList<SelectListItem> AvailableServiceVariantTypes { get; set; }
        public IList<string> ServiceVariantTypes { get; set; }

        
    }
}