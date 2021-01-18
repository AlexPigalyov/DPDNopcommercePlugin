using System;
using System.Collections.Generic;
using System.Text;
using Nop.Web.Models.Checkout;

namespace Nop.Plugin.Shipping.DPD.Models
{
    public class DPDCheckoutShippingMethodModel : CheckoutShippingMethodModel
    {
        public List<string> Cities { get; set; }
        public List<string> GeoCoordinates { get; set; }
    }
}
