using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Shipping.DPD.Domain;
using Nop.Plugin.Shipping.DPD.Models;
using Nop.Plugin.Shipping.DPD.Services;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.DPD.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class DPDShippingController : BasePluginController
    {
        private readonly DPDService _dpdService;
        private readonly DPDSettings _dpdSettings;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        public DPDShippingController(
            IPermissionService permissionService,
            DPDSettings dpdSettings,
            DPDService dpdService,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            _permissionService = permissionService;
            _dpdSettings = dpdSettings;
            _dpdService = dpdService;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _languageService = languageService;
        }

        public IActionResult Configure()
        {
            //whether user has the authority to manage configuration
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            //prepare common model
            var model = new DPDShippingModel
            {
                ClientNumber = _dpdSettings.ClientNumber,
                ClientKey = _dpdSettings.ClientKey,
                CargoRegistered = _dpdSettings.CargoRegistered,
                UseSandbox = _dpdSettings.UseSandbox
            };

            if (_dpdSettings.ServiceVariantType == null)
            {
                var serviceVariantTypes = new List<string>
                {
                    _dpdService.GetUpsCode(ServiceVariantType.TT)
                };

                _dpdSettings.ServiceVariantType = string.Join(':', serviceVariantTypes.Select(service => $"[{service}]"));
            }

            if (_dpdSettings.ServiceVariantType == null)
            {
                var serviceVariantTypes = new List<string>
                {
                    _dpdService.GetUpsCode(ServiceVariantType.TT)
                };

                _dpdSettings.ServiceVariantType = string.Join(':', serviceVariantTypes.Select(service => $"[{service}]"));
            }

            //prepare offered delivery services
            var servicesVariantCodes = _dpdSettings.ServiceVariantType.Split(':', StringSplitOptions.RemoveEmptyEntries)
                .Select(idValue => idValue.Trim('[', ']')).ToList();

            //prepare available options
            model.AvailablePaymentTypes = PaymentType.OUP.ToSelectList(false)
                .Select(item => new SelectListItem(item.Text.Replace(" ", "").Trim(), item.Text.Replace(" ", ""))).ToList();
            model.AvailablePickupTimePeriodTypes = PickupTimePeriodType.NineAMToSixPM.ToSelectList(false)
                .Select(item => new SelectListItem(_localizationService.GetResource($"Enums.Nop.Plugin.Shipping.DPD.PickupTimePeriodType.{item.Text.Replace(" ", "")}"), item.Value)).ToList();
            model.AvailableServiceVariantTypes = ServiceVariantType.TT.ToSelectList(false).Select(item =>
            {
                var serviceCode = _dpdService.GetUpsCode((ServiceVariantType)int.Parse(item.Value));
                return new SelectListItem($"{item.Text?.TrimStart('_').Replace(" ", "")}", serviceCode,
                    servicesVariantCodes.Contains(serviceCode));
            }).ToList();

            model.ServiceCodes = new List<string>();
            model.ServiceCodesTD = new List<string>();
            model.ServiceCodesTT = new List<string>();

            model.ServiceCodesTD = _dpdSettings.ServiceCodes?.Select(x => x.IsTDActive ? x.Code : null).ToList();
            model.ServiceCodesTT = _dpdSettings.ServiceCodes?.Select(x => x.IsTTActive ? x.Code : null).ToList();

            model.AvailableServiceCodeTypes = ServiceCodeType.DPDOnlineExpress.ToSelectList(false).Select(item =>
            {
                var serviceCode = _dpdService.GetUpsCode((ServiceCodeType)int.Parse(item.Value));

                return new SelectListItem(
                    $"{item.Text?.TrimStart('_').Replace(" ", "")}",
                    serviceCode,
                    _dpdSettings.ServiceCodes == null ? false : _dpdSettings.ServiceCodes.Any(x => x.Code == serviceCode));
            }).ToList();

            return View("~/Plugins/Shipping.DPD/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(DPDShippingModel model)
        {
            //whether user has the authority to manage configuration
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            //save settings
            _dpdSettings.ClientNumber = model.ClientNumber;
            _dpdSettings.ClientKey = model.ClientKey;
            _dpdSettings.UseSandbox = model.UseSandbox;
            _dpdSettings.PaymentType = (PaymentType)model.PaymentType;
            _dpdSettings.PickupTimePeriodType = (PickupTimePeriodType)model.PickupTimePeriodType;
            _dpdSettings.CargoRegistered = model.CargoRegistered;

            _dpdSettings.ServiceCodes = new List<ServiceCode>();

            for(int i = 0; i < model.ServiceCodes.Count; i++)
            {
                _dpdSettings.ServiceCodes.Add(new ServiceCode()
                {
                    Code = model?.ServiceCodes[i],
                    IsTDActive = model.ServiceCodesTD != null && model.ServiceCodesTD.Count >= i ? model.ServiceCodesTD[i] != null : false,
                    IsTTActive = model.ServiceCodesTT != null && model.ServiceCodesTT.Count >= i ? model.ServiceCodesTT[i] != null : false
                });
            }

            _dpdSettings.ServiceVariantType = string.Join(':', model.ServiceVariantTypes?.Select(service => $"[{service}]") ?? Array.Empty<string>());

            _settingService.SaveSetting(_dpdSettings);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
    }
}