using Nop.Web.Models.Checkout;

using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Shipping.DPD.Models
{
    public class DPDCheckoutShippingMethodModel : CheckoutShippingMethodModel
    {
        public OnePageCheckoutModel OnePageModel { get; set; }
    }
}
