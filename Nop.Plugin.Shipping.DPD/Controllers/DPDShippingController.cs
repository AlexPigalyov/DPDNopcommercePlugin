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

            model.Languages = _languageService.GetAllLanguages().ToList();

            if (_dpdSettings.ServiceVariantType == null)
            {
                var serviceVariantTypes = new List<string>
                {
                    _dpdService.GetUpsCode(ServiceVariantType.TT)
                };

                _dpdSettings.ServiceVariantType = string.Join(':', serviceVariantTypes.Select(service => $"[{service}]"));
            }

            if (_dpdSettings.ServiceCodeType == null)
            {
                var serviceCodeTypes = new List<string>
                {
                    _dpdService.GetUpsCode(ServiceCodeType.DPDOnlineExpress),
                    _dpdService.GetUpsCode(ServiceCodeType.DPDClassic),
                    _dpdService.GetUpsCode(ServiceCodeType.DPDClassicInternational)
                };

                _dpdSettings.ServiceCodeType = string.Join(':', serviceCodeTypes.Select(service => $"[{service}]"));
            }


            //prepare offered delivery services
            var servicesVariantCodes = _dpdSettings.ServiceVariantType.Split(':', StringSplitOptions.RemoveEmptyEntries)
                .Select(idValue => idValue.Trim('[', ']')).ToList();
            var servicesCodeCodes = _dpdSettings.ServiceCodeType.Split(':', StringSplitOptions.RemoveEmptyEntries)
                .Select(idValue => idValue.Trim('[', ']')).ToList();

            //prepare available options
            model.AvailablePaymentTypes = PaymentType.OUP.ToSelectList(false)
                .Select(item => new SelectListItem(item.Text.Replace(" ", ""), item.Text.Replace(" ", ""))).ToList();
            model.AvailablePickupTimePeriodTypes = PickupTimePeriodType.NineAMToSixPM.ToSelectList(false)
                .Select(item => new SelectListItem(_localizationService.GetResource($"Enums.Nop.Plugin.Shipping.DPD.PickupTimePeriodType.{item.Text.Replace(" ", "")}"), item.Value)).ToList();
            model.AvailableServiceVariantTypes = ServiceVariantType.TT.ToSelectList(false).Select(item =>
            {
                var serviceCode = _dpdService.GetUpsCode((ServiceVariantType)int.Parse(item.Value));
                return new SelectListItem($"{item.Text?.TrimStart('_').Replace(" ", "")}", serviceCode,
                    servicesVariantCodes.Contains(serviceCode));
            }).ToList();

            model.ServiceCodes = new List<ServiceCode>();

            model.AvailableServiceCodeTypes = ServiceCodeType.DPDOnlineExpress.ToSelectList(false).Select(item =>
            {
                var serviceCode = _dpdService.GetUpsCode((ServiceCodeType)int.Parse(item.Value));

                model.ServiceCodes.Add(new ServiceCode()
                {
                    Code = serviceCode
                });

                return new SelectListItem($"{item.Text?.TrimStart('_').Replace(" ", "")}", serviceCode,
                    servicesCodeCodes.Contains(serviceCode));
            }).ToList();

            return View("~/Plugins/Shipping.DPD/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public IActionResult Configure(DPDShippingModel model)
        {
            

            //whether user has the authority to manage configuration
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _dpdSettings.ClientNumber = model.ClientNumber;
            _dpdSettings.ClientKey = model.ClientKey;
            _dpdSettings.UseSandbox = model.UseSandbox;
            _dpdSettings.PaymentType = (PaymentType)model.PaymentType;
            _dpdSettings.PickupTimePeriodType = (PickupTimePeriodType)model.PickupTimePeriodType;
            _dpdSettings.CargoRegistered = model.CargoRegistered;

            //use default services if no one is selected 
            if (!model.ServiceCodeTypes.Any())
            {
                model.ServiceCodeTypes = new List<string>
                {
                    _dpdService.GetUpsCode(ServiceCodeType.DPDOnlineExpress),
                    _dpdService.GetUpsCode(ServiceCodeType.DPDClassic),
                    _dpdService.GetUpsCode(ServiceCodeType.DPDClassicInternational)
                };
            }
            _dpdSettings.ServiceCodeType = string.Join(':', model.ServiceCodeTypes.Select(service => $"[{service}]"));
           
            if (!model.ServiceVariantTypes.Any())
            {
                model.ServiceVariantTypes = new List<string>
                {
                    _dpdService.GetUpsCode(ServiceVariantType.TT)
                };
            }
            _dpdSettings.ServiceVariantType = string.Join(':', model.ServiceVariantTypes.Select(service => $"[{service}]"));

            _settingService.SaveSetting(_dpdSettings);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
    }
}