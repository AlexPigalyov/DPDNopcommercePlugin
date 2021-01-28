using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Shipping.DPD.Infrastructure
{
    class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            /*
            var pattern = string.Empty;
            if (DataSettingsManager.DatabaseIsInstalled)
            {
                var localizationSettings = endpointRouteBuilder.ServiceProvider.GetRequiredService<LocalizationSettings>();
                if (localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    var langservice = endpointRouteBuilder.ServiceProvider.GetRequiredService<ILanguageService>();
                    var languages = langservice.GetAllLanguages().ToList();
                    pattern = "{language:lang=" + languages.FirstOrDefault().UniqueSeoCode + "}/";
                }
            }
            */
            //checkout pages
            endpointRouteBuilder.MapControllerRoute("areaRouteDPDCheckoutSaveBilling", $"checkout/OpcSaveBilling",
                new { controller = "DPDCheckout", action = "OpcSaveBilling" });
            endpointRouteBuilder.MapControllerRoute("areaRouteDPDSaveShippingMethod", $"checkout/opcsaveshippingmethod/",
                new { controller = "DPDCheckout", action = "OpcSaveShippingMethod" });
            endpointRouteBuilder.MapControllerRoute("areaRouteDPDSaveShipping", $"checkout/opcsaveshipping/",
                new { controller = "DPDCheckout", action = "OpcSaveShipping" });
            endpointRouteBuilder.MapControllerRoute("areaRouteCheckoutOnePage", $"onepagecheckout/",
                new { controller = "DPDCheckout", action = "OnePageCheckout" }); 
                endpointRouteBuilder.MapControllerRoute("areaRouteOpcConfirmOrder", $"checkout/opcconfirmorder",
                new { controller = "DPDCheckout", action = "OpcConfirmOrder" });
            /*
            endpointRouteBuilder.MapControllerRoute("Checkout", $"{pattern}checkout/",
                new { controller = "DPDCheckout", action = "Index" });

            

            endpointRouteBuilder.MapControllerRoute("CheckoutShippingAddress", $"{pattern}checkout/shippingaddress",
                new { controller = "DPDCheckout", action = "ShippingAddress" });

            endpointRouteBuilder.MapControllerRoute("CheckoutSelectShippingAddress", $"{pattern}checkout/selectshippingaddress",
                new { controller = "DPDCheckout", action = "SelectShippingAddress" });

            endpointRouteBuilder.MapControllerRoute("CheckoutBillingAddress", $"{pattern}checkout/billingaddress",
                new { controller = "DPDCheckout", action = "BillingAddress" });

            endpointRouteBuilder.MapControllerRoute("CheckoutSelectBillingAddress", $"{pattern}checkout/selectbillingaddress",
                new { controller = "DPDCheckout", action = "SelectBillingAddress" });

            endpointRouteBuilder.MapControllerRoute("areaRouteCheckoutShippingMethod", $"{pattern}checkout/shippingmethod",
                new { controller = "DPDCheckout", action = "ShippingMethod" });

            endpointRouteBuilder.MapControllerRoute("CheckoutPaymentMethod", $"{pattern}checkout/paymentmethod",
                new { controller = "DPDCheckout", action = "PaymentMethod" });

            endpointRouteBuilder.MapControllerRoute("CheckoutPaymentInfo", $"{pattern}checkout/paymentinfo",
                new { controller = "DPDCheckout", action = "PaymentInfo" });

            endpointRouteBuilder.MapControllerRoute("CheckoutConfirm", $"{pattern}checkout/confirm",
                new { controller = "DPDCheckout", action = "Confirm" });

            endpointRouteBuilder.MapControllerRoute("CheckoutCompleted",
                pattern + "checkout/completed/{orderId:int}",
                new { controller = "DPDCheckout", action = "Completed" });
            */
        }

        public int Priority { get; } = int.MaxValue;
    }
}
