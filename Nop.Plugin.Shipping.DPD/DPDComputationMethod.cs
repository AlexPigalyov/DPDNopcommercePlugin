using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Plugin.Shipping.DPD.Domain;
using Nop.Plugin.Shipping.DPD.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;

namespace Nop.Plugin.Shipping.DPD
{
    /// <summary>
    /// Represents DPD computation method
    /// </summary>
    public class DPDComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly DPDService _DPDService;

        #endregion

        #region Ctor

        public DPDComputationMethod(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            DPDService DPDService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _DPDService = DPDService;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            if (!getShippingOptionRequest.Items?.Any() ?? true)
                return new GetShippingOptionResponse { Errors = new[] { "No shipment items" } };

            if (getShippingOptionRequest.ShippingAddress?.CountryId == null)
                return new GetShippingOptionResponse { Errors = new[] { "Shipping address is not set" } };

            return _DPDService.GetRates(getShippingOptionRequest);
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/DPDShipping/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {

            


            /*
            //settings
            _settingService.SaveSetting(new DPDSettings
            {
                UseSandbox = true,
                CustomerClassification = CustomerClassification.StandardListRates,
                PickupType = PickupType.OneTimePickup,
                PackagingType = PackagingType.ExpressBox,
                PackingPackageVolume = 5184,
                PackingType = PackingType.PackByDimensions,
                PassDimensions = true,
                WeightType = "LBS",
                DimensionsType = "IN"
            });*/

            //all locales
            _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
            {
                ["Enums.Nop.Plugin.Shipping.DPD.PickupTimePeriodType.NineAMToSixPM"] = "9-18",
                ["Enums.Nop.Plugin.Shipping.DPD.PickupTimePeriodType.NineAMToOnePM"] = "9-13",
                ["Enums.Nop.Plugin.Shipping.DPD.PickupTimePeriodType.OnePMToSixPM"] = "13-18",

                ["Plugins.Shipping.DPD.Fields.UseSandbox"] = "Use sandbox",

                ["Plugins.Shipping.DPD.Fields.ClientNumber"] = "Client Number",
                ["Plugins.Shipping.DPD.Fields.ClientKey"] = "Client Key",
                ["Enums.Nop.Plugin.Shipping.DPD.PaymentType.OUP"] = "Payment at the recipient",
                ["Enums.Nop.Plugin.Shipping.DPD.PaymentType.OUO"] = "Payment at the sender",

                ["Enums.Nop.Plugin.Shipping.DPD.ServiceVariantType.DD"] = "DD (Door-to-door delivery)",
                ["Enums.Nop.Plugin.Shipping.DPD.ServiceVariantType.DT"] = "DT (Door-to-terminal delivery)",
                ["Enums.Nop.Plugin.Shipping.DPD.ServiceVariantType.TD"] = "TD (Terminal-to-door delivery)",
                ["Enums.Nop.Plugin.Shipping.DPD.ServiceVariantType.TT"] = "TT (Terminal-to-terminal delivery)",

                ["Plugins.Shipping.DPD.Fields.UseSandbox"] = "Use Sandbox",
                ["Plugins.Shipping.DPD.Fields.UseSandbox.Hint"] = "Use the test version",

                ["Plugins.Shipping.DPD.Fields.PickupTimePeriodType"] = "Time period of receipt of goods",

                ["Plugins.Shipping.DPD.Fields.ServiceCodeType"] = "DPD payment type",

                ["Plugins.Shipping.DPD.Fields.AvailableServiceCodeType"] = "DPD service code",

                ["Plugins.Shipping.DPD.Fields.AvailableServiceVariantType"] = "Delivery variant",

                ["Plugins.Shipping.DPD.Fields.CargoRegistered"] = "Cargo registered",
                ["Plugins.Shipping.DPD.Fields.CargoRegistered.Hint"] =
                    "Enclosure included into the list of goods subject to extra safety measures reducing risk of its loss or damage during transportation."
            });


            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<DPDSettings>();

            //locales
            _localizationService.DeletePluginLocaleResources("Enums.Nop.Plugin.Shipping.DPD");
            _localizationService.DeletePluginLocaleResources("Plugins.Shipping.DPD");

            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType => ShippingRateComputationMethodType.Realtime;

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker => null;

        #endregion
    }
}