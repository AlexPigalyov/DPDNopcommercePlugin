using System;
using System.Collections.Generic;
using System.Linq;
using Calculator;
using Geography;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Plugin.Shipping.DPD.Domain;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using auth = Order.auth;

namespace Nop.Plugin.Shipping.DPD.Services
{
    public class DPDService
    {
        private readonly IRepository<Address> _addressRepository;
        private readonly DPDSettings _dpdSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
        private readonly IWorkContext _workContext;
        
        private readonly IShippingService _shippingService;

        public DPDService(
            ILocalizationService localizationService,
            IRepository<ShoppingCartItem> shoppingCartItemRepository,
            IRepository<Product> productRepository,
            IWorkContext workContext,
            DPDSettings dpdSettings, 
            IRepository<Address> addressRepository,
            IShippingService shippingService)
        {
            _localizationService = localizationService;
            _dpdSettings = dpdSettings;
            _addressRepository = addressRepository;
            _shippingService = shippingService;
            _workContext = workContext;
            _productRepository = productRepository;
            _shoppingCartItemRepository = shoppingCartItemRepository;
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
        /*
        private List<ServiceCode> GetServicesCodes()
        {
            
            //List<string> serviceCodesSTR = new List<string>();
            /*
            if (_dpdSettings.ServiceCodesOffered != null)
            {
                if (_dpdSettings.ServiceCodesOffered.Split(':', StringSplitOptions.RemoveEmptyEntries).ToList().Count > 0)
                {
                    serviceCodesSTR = _dpdSettings.ServiceCodesOffered.Split(':', StringSplitOptions.RemoveEmptyEntries)
                        .Select(idValue => idValue.Trim('[', ']')).ToList();
                }
                else
                {
                    serviceCodesSTR.Add(_dpdSettings.ServiceCodesOffered.Trim('[', ']'));
                }
            }
            */
            //List<ServiceCode> serviceCodes = new List<ServiceCode>();                            ;
            /*
            for (int i = 0; i < serviceCodesSTR.Count; i++)
            {
                serviceCodes.Add(new ServiceCode()
                {
                    Code = serviceCodesSTR[i].Split('-')[0],
                    IsTDActive = bool.Parse(serviceCodesSTR[i].Split('-')[1]),
                    IsTTActive = bool.Parse(serviceCodesSTR[i].Split('-')[2])
                });
            }
            
            serviceCodes.Add(new ServiceCode()
            {
                Code = "PickUp Delivery (TT)",
                IsTDActive = true, 
                IsTTActive = true;
            })
            return serviceCodes;
        }
        
        private List<Product> GetCustomerShoppingCartItems()
        {
            var shoppingCartItems = new List<Product>();

            foreach (var shoppingCartItem in _shoppingCartItemRepository.Table)
            {
                if (shoppingCartItem.CustomerId == _workContext.CurrentCustomer.Id)
                {
                    foreach (var product in _productRepository.Table)
                    {
                        if (product.Id == shoppingCartItem.ProductId)
                        {
                            shoppingCartItems.Add(product);
                        }
                    }
                }
            }

            return shoppingCartItems;
        }
    
        private (double, double, double) GetProductsPriceVolumeWeight(List<Product> products)
        {
            double priceOfShoppingCart = 0;
            double volumeOfShoppingCart = 0;
            double weightOfShoppingCart = 0;

            foreach (var shoppingCartItem in products)
            {
                priceOfShoppingCart += (double)shoppingCartItem.Price;
                volumeOfShoppingCart += (double)shoppingCartItem.Length * (double)shoppingCartItem.Width * (double)shoppingCartItem.Height;
                weightOfShoppingCart += (double)shoppingCartItem.Weight;
            }

            return (priceOfShoppingCart, volumeOfShoppingCart, weightOfShoppingCart);
        }
    */
        public virtual GetShippingOptionResponse GetRates(GetShippingOptionRequest shippingOptionRequest)
        {
            
            var response = new GetShippingOptionResponse();
            /*
            //get services variants and codes
            List<ServiceCode> servicesCode = GetServicesCodes();

            List<Product> shoppingCartItems = GetCustomerShoppingCartItems();

            double priceOfShoppingCart = 0;
            double volumeOfShoppingCart = 0;
            double weightOfShoppingCart = 0;

            //get price, volume and weight of all items in shopping cart 
            (priceOfShoppingCart, volumeOfShoppingCart, weightOfShoppingCart) =
                GetProductsPriceVolumeWeight(shoppingCartItems);

            priceOfShoppingCart = Math.Round(priceOfShoppingCart, 2);

            var calculator = new DPDCalculatorClient();

            var geographyClient = new DPDGeography2Client();

            var customerAddress =
                _addressRepository.Table.FirstOrDefault(x => x.Id == _workContext.CurrentCustomer.ShippingAddressId);
            var geographyResponse = geographyClient.getCitiesCashPayAsync(new dpdCitiesCashPayRequest
            {
                auth = new Geography.auth
                {
                    clientKey = _dpdSettings.ClientKey,
                    clientNumber = _dpdSettings.ClientNumber
                },
                countryCode = "RU"
            }).Result;
            var deliveryCity = geographyResponse.@return.FirstOrDefault(x =>
                x.cityName.Trim().ToLower() == "москва");
            var pickupCity = geographyResponse.@return.FirstOrDefault(x =>
                x.cityName == "Санкт-Петербург"); 

            
            foreach (var serviceCode in servicesCode)
            {
                int countRequests = 1;
                bool[] serviceVariantsDelivery = new bool[2];
                serviceVariantsDelivery[0] = !serviceCode.IsTTActive;
                if (serviceCode.IsTDActive && serviceCode.IsTTActive)
                {
                    countRequests = 2;
                    serviceVariantsDelivery[0] = false;
                    serviceVariantsDelivery[1] = true;
                }
                
                for (int i = 0; i < countRequests; i++)
                {
                    
                    var serviceCost = calculator.getServiceCost2Async(new serviceCostRequest
                    {
                        auth = new Calculator.auth
                        {
                            clientKey = _dpdSettings.ClientKey,
                            clientNumber = _dpdSettings.ClientNumber
                        },
                        declaredValue = priceOfShoppingCart,
                        delivery = new cityRequest
                        {
                            cityId = deliveryCity.cityId,
                            cityIdSpecified = deliveryCity.cityIdSpecified,
                            cityName = deliveryCity.cityName,
                            countryCode = deliveryCity.countryCode,
                            regionCode = deliveryCity.regionCode,
                            regionCodeSpecified = deliveryCity.regionCodeSpecified
                        },
                        pickup = new cityRequest()
                        {
                            cityId = pickupCity.cityId,
                            cityIdSpecified = pickupCity.cityIdSpecified,
                            cityName = pickupCity.cityName,
                            countryCode = pickupCity.countryCode,
                            regionCode = pickupCity.regionCode,
                            regionCodeSpecified = pickupCity.regionCodeSpecified
                        },
                        serviceCode = serviceCode.Code,
                        selfDelivery = serviceVariantsDelivery[i],
                        weight = 1
                    }).Result;

                    response.ShippingOptions.Add(new ShippingOption
                    {
                        Name = serviceCode.Code,
                        Description = serviceVariantsDelivery[i] ? "<button>12312312</button>" : "Самовывоз",
                        Rate = Math.Ceiling((decimal)serviceCost.@return[0].cost)
                    });
                }
                
            }
            */
            response.ShippingOptions.Add(new ShippingOption
            {
                Name = "Pick-up in shop (TD)",
                Description = "Delivery to the some Shop",
                Rate = 555
            });
            response.ShippingOptions.Add(new ShippingOption
            {
                Name = "Courier delivery (TT)",
                Description = "Delivery from door to door",
                Rate = 200
            });
            return response;
        }
    }
}