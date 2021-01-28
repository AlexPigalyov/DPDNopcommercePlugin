using System;
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
using auth = Order.auth;

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

        /// <summary>
        ///     Get UPS code of enum value
        /// </summary>
        /// <param name="enumValue">Enum value</param>
        /// <returns>UPS code</returns>
        public string GetUpsCode(Enum enumValue)
        {
            return GetAttributeValue<DPDCodeAttribute>(enumValue)?.Code;
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
                                street = orderSettings.receiverAddress.street,
                                contactFio = orderSettings.receiverAddress.contactFio,
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
            return new auth()
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
                    code = _dpdSettings.AddressCode
                }
            };
        }
        private Order.address PrepareReceiverAdress(Core.Domain.Orders.Order currentOrder, bool isTTServiceVariantType, PickupPointAddress dpdPickupPointAddress)
        {
            var receiverAddress = new Order.address();
            
            if (isTTServiceVariantType)
            {
                var shippingAddress = _addressService.GetAddressById(currentOrder.ShippingAddressId.GetValueOrDefault());

                string receiverFullName = shippingAddress.FirstName + " " + shippingAddress.LastName;

                receiverAddress = new Order.address()
                {
                    name = receiverFullName,
                    countryName = shippingAddress.County,
                    city = shippingAddress.City,
                    street = shippingAddress.Address1 ?? shippingAddress.Address2 ?? throw new ArgumentNullException("Shipping street can be null"),
                    contactFio = receiverFullName,
                    contactPhone = shippingAddress.PhoneNumber,
                    contactEmail = shippingAddress.Email
                };
            }
            else
            {
                receiverAddress = new Order.address()
                {
                    terminalCode = dpdPickupPointAddress.TerminalCode,
                    countryName = dpdPickupPointAddress.CountryName,
                    city = dpdPickupPointAddress.City,
                    street = dpdPickupPointAddress.Street,
                    name = dpdPickupPointAddress.CustomerFullName
                };
            }

            return receiverAddress;
        }
        private Order.order PrepareOrderSettings(int orderId, PickupPointAddress dpdPickupPointAddress)
        {
            double productsWeight = 0;
            double productsCost = 0;

            var currentOrder = _orderService.GetOrderById(orderId);

            bool isTTServiceVariantType = new string(currentOrder.ShippingRateComputationMethodSystemName.Reverse().Take(2).Reverse().ToArray()) == "TT" ? true : false;

            var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            List<string> productsCategories = new List<string>();

            foreach (var productId in cart.Select(x => x.ProductId))
            {
                var product = _productService.GetProductById(productId);

                productsWeight += (double)product.Weight;
                productsCost += (double)product.Price;

                int[] categoriyIds = _catergoryService
                    .GetProductCategoriesByCategoryId(product.TaxCategoryId)
                        .Select(x => x.CategoryId)
                    .ToArray();

                List<Category> categories = _catergoryService
                    .GetCategoriesByIds(categoriyIds);

                List<string> uniqueCategoryNames = categories.Select(x => x.Name).ToList();

                productsCategories.AddRange(uniqueCategoryNames);
            }

            var receiverAddress = PrepareReceiverAdress(currentOrder, isTTServiceVariantType, dpdPickupPointAddress);

            string uniqueCategoryNamesString = string.Join(", ", productsCategories.Distinct());

            return new Order.order()
            {
                orderNumberInternal = currentOrder.Id.ToString(),
                serviceCode = currentOrder.ShippingMethod.Split('(', ')')[1],
                serviceVariant = isTTServiceVariantType ? "ТТ" : "ТД",
                cargoNumPack = 1,
                cargoWeight = productsWeight,
                cargoRegistered = _dpdSettings.CargoRegistered,
                cargoValue = productsCost,
                cargoValueSpecified = true,
                cargoCategory = uniqueCategoryNamesString,
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

            foreach(var product in shippingOptionRequest.Items.Select(x => x.Product))
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
                    selfDelivery = serviceVariantTypes[i] == "TD",
                    declaredValue = priceOfProducts,
                    weight = 0.050
                }).Result;

                foreach(var service in serviceCosts.@return.ToList())
                {
                    if(serviceCodeTypes.Any(x => x.ToLower() == service.serviceName.Replace(" ", "").ToLower()))
                    {
                        response.ShippingOptions.Add(new ShippingOption()
                        {
                            Name = (serviceVariantTypes[i] == "TD" ? "DPD Pick-up delivery" : "DPD Courier delivery") + $" ({service.serviceName})",
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