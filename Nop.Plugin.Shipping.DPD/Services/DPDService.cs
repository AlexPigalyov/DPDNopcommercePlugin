﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Calculator;
using Geography;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Plugin.Shipping.DPD.Domain;
using Nop.Plugin.Shipping.DPD.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Shipping;

namespace Nop.Plugin.Shipping.DPD.Services
{
    public class DPDService
    {
        private readonly DPDSettings _dpdSettings;
        private readonly HttpClient _httpClient;
        private readonly Calculator.DPDCalculatorClient _dpdCalculator;
        private readonly Geography.DPDGeography2Client _dpdGeography;
        private readonly Order.DPDOrderClient _dpdOrderClient;
        private readonly SandboxOrder.DPDOrderClient _dpdSandboxOrderClient;
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICategoryService _catergoryService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        private readonly IShippingService _shippingService;
        public DPDService(
            ILocalizationService localizationService,
            IOrderService orderService,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IAddressService addressService,
            IProductService productService,
            ICategoryService categoryService,
            DPDSettings dpdSettings,
            IShippingService shippingService)
        {
            _dpdCalculator = new DPDCalculatorClient();
            _dpdGeography = new DPDGeography2Client();
            _dpdOrderClient = new Order.DPDOrderClient();
            _dpdSandboxOrderClient = new SandboxOrder.DPDOrderClient();
            _httpClient = new HttpClient();
            _localizationService = localizationService;
            _dpdSettings = dpdSettings;
            _storeContext = storeContext;
            _shoppingCartService = shoppingCartService;
            _addressService = addressService;
            _orderService = orderService;
            _shippingService = shippingService;
            _workContext = workContext;
            _productService = productService;
            _catergoryService = categoryService;
        }

        /// <summary>
        ///     Gets an attribute value on an enum field value
        /// </summary>
        /// <typeparam name="TAttribute">Type of the attribute</typeparam>
        /// <param name="enumValue">Enum value</param>
        /// <returns>The attribute value</returns>
        private TAttribute GetAttributeValue<TAttribute>(Enum enumValue) where TAttribute : Attribute
        {

            var enumType = enumValue.GetType();
            var enumValueInfo = enumType.GetMember(enumValue.ToString()).FirstOrDefault();
            var attribute = enumValueInfo?.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault();
            return attribute as TAttribute;
        }

        public static string ToUpper(string str, int chars)
        {
            return str.Substring(0, chars).ToUpper() + (str.Length > 1 ? str.Substring(chars) : "");
        }

        /// <summary>
        ///     Get UPS code of enum value
        /// </summary>
        /// <param name="enumValue">Enum value</param>
        /// <returns>UPS code</returns>
        public string GetUpsCode(Enum enumValue)
        {
            return GetAttributeValue<DPDCodeAttribute>(enumValue)?.Code;
        }
        public object CreateNewAddress(DPDShippingModel newAddressModel)
        {
            try
            {
                var response = _dpdOrderClient.createAddressAsync(new Order.dpdClientAddress()
                {
                    auth = new Order.auth()
                    {
                        clientKey = _dpdSettings.ClientKey,
                        clientNumber = _dpdSettings.ClientNumber
                    },
                    clientAddress = new Order.clientAddress()
                    {
                        city = newAddressModel.City,
                        contactEmail = newAddressModel?.ContactEmail,
                        code = newAddressModel.Code,
                        contactFio = newAddressModel.ContactFullName,
                        contactPhone = newAddressModel.ContactPhone,
                        countryName = newAddressModel.CountryName,
                        flat = newAddressModel?.Apartament,
                        house = newAddressModel?.House,
                        street = newAddressModel.Street,
                        houseKorpus = newAddressModel?.HouseCorps,
                        instructions = newAddressModel?.Instructions,
                        name = newAddressModel.Name,
                        index = newAddressModel?.Index,
                        needPass = Convert.ToBoolean(newAddressModel?.NeedPass),
                        needPassSpecified = newAddressModel?.NeedPass != null,
                        office = newAddressModel?.Office,
                        region = newAddressModel?.Region,
                        vlad = newAddressModel?.OwnerShip,
                    }
                }).Result;

                return response;
            } 
            catch
            {
                throw new ArgumentException("Create new address expetion");
            }
        }
        public object CreateShippingRequest(int orderId, PickupPointAddress dpdPickupPointAddress)
        {
            var orderAuth = PrepareOrderAuth();
            var orderHeader = PrepareOrderHeader();
            var orderSettings = PrepareOrderSettings(orderId, dpdPickupPointAddress);

            if (_dpdSettings.UseSandbox)
            {
                var orderResponse = _dpdSandboxOrderClient.createOrderAsync(new SandboxOrder.dpdOrdersData()
                {
                    auth = new SandboxOrder.auth()
                    {
                        clientKey = orderAuth.clientKey,
                        clientNumber = orderAuth.clientNumber
                    },
                    header = new SandboxOrder.header()
                    {
                        datePickup = orderHeader.datePickup,
                        senderAddress = new SandboxOrder.address()
                        {
                            code = orderHeader.senderAddress.code
                        }
                    },
                    order = new SandboxOrder.order[]
                    {
                        new SandboxOrder.order()
                        {
                            cargoCategory = orderSettings.cargoCategory,
                            cargoRegistered = orderSettings.cargoRegistered,
                            cargoValue = orderSettings.cargoValue,
                            cargoValueSpecified = true,
                            cargoWeight = orderSettings.cargoWeight,
                            orderNumberInternal = orderSettings.orderNumberInternal,
                            serviceCode = orderSettings.serviceCode,
                            serviceVariant = orderSettings.serviceVariant,
                            receiverAddress = new SandboxOrder.address()
                            {
                                name = orderSettings.receiverAddress.name,
                                countryName = orderSettings.receiverAddress.countryName,
                                city = orderSettings.receiverAddress.city,
                                house = orderSettings.receiverAddress.house,
                                street = orderSettings.receiverAddress.street,
                                contactFio = orderSettings.receiverAddress.contactFio,
                                terminalCode = orderSettings.receiverAddress.terminalCode,
                                contactPhone = orderSettings.receiverAddress.contactPhone,
                                contactEmail = orderSettings.receiverAddress.contactEmail
                            }
                        }
                    }
                }).Result;

                return new Order.createOrderResponse()
                {
                    @return = orderResponse.@return.ToList().Select(x => new Order.dpdOrderStatus()
                    {
                        errorMessage = x.errorMessage,
                        orderNum = x.orderNum,
                        orderNumberInternal = x.orderNumberInternal,
                        status = x.status
                    }).ToArray()
                };
            }
            else
            {
                var orderResponse = _dpdOrderClient.createOrderAsync(new Order.dpdOrdersData()
                {
                    auth = orderAuth,
                    header = orderHeader,
                    order = new Order.order[]
                    {
                        orderSettings
                    }
                }).Result;

                return orderResponse;
            }
        }

        private Order.auth PrepareOrderAuth()
        {
            return new Order.auth()
            {
                clientKey = _dpdSettings.ClientKey,
                clientNumber = _dpdSettings.ClientNumber
            };
        }
        private Order.header PrepareOrderHeader()
        {
            return new Order.header()
            {
                datePickup = DateTime.Now,
                senderAddress = new Order.address()
                {
                    code = _dpdSettings.AddressCode,
                }
            };
        }
        private Order.address PrepareReceiverAdress(Core.Domain.Orders.Order currentOrder, bool isDDServiceVariantType, PickupPointAddress dpdPickupPointAddress)
        {
            var receiverAddress = new Order.address();

            
            var shippingAddress = _addressService.GetAddressById(currentOrder.ShippingAddressId.GetValueOrDefault());
            
            string receiverFullName = shippingAddress.FirstName + " " + shippingAddress.LastName;
            if (isDDServiceVariantType)
            {
                receiverAddress = new Order.address()
                {
                    name = receiverFullName,
                    countryName = shippingAddress.County,
                    city = dpdPickupPointAddress.City,
                    street = dpdPickupPointAddress.Street ?? throw new ArgumentNullException("Shipping street can be null"),
                    house = dpdPickupPointAddress.House,
                    contactFio = receiverFullName,
                    contactPhone = shippingAddress.PhoneNumber,
                    contactEmail = shippingAddress.Email
                };
            }
            else
            {
                receiverAddress = new Order.address()
                {
                    contactFio = receiverFullName,
                    contactPhone = shippingAddress.PhoneNumber,
                    contactEmail = shippingAddress.Email,
                    terminalCode = dpdPickupPointAddress.TerminalCode,
                    countryName = dpdPickupPointAddress.CountryName,
                    city = dpdPickupPointAddress.City,
                    street = dpdPickupPointAddress.Street,
                    house = dpdPickupPointAddress.House,
                    name = receiverFullName
                };
            }

            return receiverAddress;
        }
        private Order.order PrepareOrderSettings(int orderId, PickupPointAddress dpdPickupPointAddress)
        {
            var currentOrder = _orderService.GetOrderById(orderId);

            bool isDDServiceVariantType = new string(currentOrder.ShippingRateComputationMethodSystemName.Reverse().Take(2).Reverse().ToArray()) == "DD" ? true : false;

            var receiverAddress = PrepareReceiverAdress(currentOrder, isDDServiceVariantType, dpdPickupPointAddress);

            return new Order.order()
            {
                orderNumberInternal = currentOrder.Id.ToString(),
                serviceCode = GetUpsCode((ServiceCodeType)Enum.Parse(typeof(ServiceCodeType), ToUpper(currentOrder.ShippingMethod.Split('(', ')')[1].Trim().ToLower().Replace(" ", ""), 4))),
                serviceVariant = isDDServiceVariantType ? "ДД" : "ДТ",
                cargoNumPack = 1,
                cargoWeight = 0.050,
                cargoRegistered = _dpdSettings.CargoRegistered,
                cargoValue = 50000,
                cargoValueSpecified = true,
                cargoCategory = dpdPickupPointAddress.Category,
                receiverAddress = receiverAddress
            };
        }

        private async Task<string> GetCityByCityNameAsync(string cityName)
        {
            var requestContent = new StringContent($"text={cityName}",
                Encoding.UTF8, MimeTypes.ApplicationXWwwFormUrlencoded);
            var cityReponse = await _httpClient.PostAsync("http://rentoolo.ru/api/AllCities", requestContent);
            cityReponse.EnsureSuccessStatusCode();
            return await cityReponse.Content.ReadAsStringAsync();
        }
        public virtual GetShippingOptionResponse GetRates(GetShippingOptionRequest shippingOptionRequest)
        {
            var response = new GetShippingOptionResponse();

            /*
            var cityDeliveryJson = GetCityByCityNameAsync(shippingOptionRequest.CityFrom).Result;
            var cityDelivery = JsonConvert.DeserializeObject<Geography.city>(cityDeliveryJson);
            var cityFromJson = GetCityByCityNameAsync(shippingOptionRequest.ShippingAddress.City).Result;
            var cityFrom = JsonConvert.DeserializeObject<Geography.city>(cityFromJson);
            */

            var serviceVariantTypes = _dpdSettings
                .ServiceVariantsOffered.Split(':')
                .Select(x => x.Replace("[", "").Replace("]", ""))
                .ToList();

            var serviceCodeTypes = _dpdSettings
                .ServiceCodesOffered.Split(':')
                .Select(x => x.Replace("[", "").Replace("]", ""))
                .ToList();

            double priceOfProducts = 0;

            foreach (var product in shippingOptionRequest.Items.Select(x => x.Product))
            {
                priceOfProducts += (double)product.Price;
            }

            var cities = _dpdGeography.getCitiesCashPayAsync(new dpdCitiesCashPayRequest()
            {
                auth = new Geography.auth()
                {
                    clientKey = _dpdSettings.ClientKey,
                    clientNumber = _dpdSettings.ClientNumber
                }
            }).Result;

            var cityFrom = cities.@return.FirstOrDefault(x => x.cityName == shippingOptionRequest.CityFrom);
            var cityDelivery = cities.@return.FirstOrDefault(x => x.cityName == shippingOptionRequest.ShippingAddress.City);

            for (int i = 0; i < serviceVariantTypes.Count; i++)
            {
                var serviceCosts = _dpdCalculator.getServiceCost2Async(new serviceCostRequest()
                {
                    auth = new Calculator.auth()
                    {
                        clientKey = _dpdSettings.ClientKey,
                        clientNumber = _dpdSettings.ClientNumber
                    },

                    delivery = new cityRequest()
                    {
                        cityId = cityFrom.cityId,
                        countryCode = cityFrom.countryCode,
                        cityName = cityFrom.cityName,
                        regionCode = cityFrom.regionCode,
                        cityIdSpecified = true,
                        index = shippingOptionRequest.ShippingAddress.ZipPostalCode,
                        regionCodeSpecified = true,
                    },
                    pickup = new cityRequest()
                    {
                        cityId = cityDelivery.cityId,
                        countryCode = cityDelivery.countryCode,
                        cityName = cityDelivery.cityName,
                        cityIdSpecified = true,
                        regionCodeSpecified = true,
                        index = shippingOptionRequest.ZipPostalCodeFrom,
                        regionCode = cityDelivery.regionCode,
                    },
                    selfPickup = false,
                    selfDelivery = serviceVariantTypes[i] == "DT",
                    declaredValue = priceOfProducts,
                    weight = 0.050
                }).Result;

                foreach (var service in serviceCosts.@return.ToList())
                {
                    if (serviceCodeTypes.Any(x => x.ToLower() == service.serviceName.Replace(" ", "").ToLower()))
                    {
                        response.ShippingOptions.Add(new ShippingOption()
                        {
                            Name = (serviceVariantTypes[i] == "DT" ? "DPD Pick-up delivery" : "DPD Courier delivery") + $" ({service.serviceName})",
                            Rate = (decimal)service.cost,
                            TransitDays = service.days,
                            ShippingRateComputationMethodSystemName = "DPD-" + serviceVariantTypes[i]
                        });
                    }
                }
            }

            return response;
        }
    }
}