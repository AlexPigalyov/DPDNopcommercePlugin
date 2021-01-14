using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Shipping.DPD.Domain;
using Nop.Services.Localization;
using Nop.Services.Shipping;

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

        public DPDService(
            ILocalizationService localizationService,
            IRepository<ShoppingCartItem> shoppingCartItemRepository,
            IRepository<Product> productRepository,
            IWorkContext workContext,
            DPDSettings dpdSettings, IRepository<Address> addressRepository)
        {
            _localizationService = localizationService;
            _dpdSettings = dpdSettings;
            _addressRepository = addressRepository;
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

        private (List<string>, List<string>) GetServicesVariantAndCodeCodes()
        {
            var servicesVariantCodes = _dpdSettings.ServiceVariantType.Split(':', StringSplitOptions.RemoveEmptyEntries)
                .Select(idValue => idValue.Trim('[', ']')).ToList();
            var servicesCodeCodes = _dpdSettings.ServiceCodeType.Split(':', StringSplitOptions.RemoveEmptyEntries)
                .Select(idValue => idValue.Trim('[', ']')).ToList();

            return (servicesVariantCodes, servicesCodeCodes);
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

        private (decimal, decimal, decimal) GetProductsPriceVolumeWeight(List<Product> products)
        {
            decimal priceOfShoppingCart = 0;
            decimal volumeOfShoppingCart = 0; 
            decimal weightOfShoppingCart = 0;

            foreach (var shoppingCartItem in products)
            {
                priceOfShoppingCart += shoppingCartItem.Price;
                volumeOfShoppingCart += shoppingCartItem.Length * shoppingCartItem.Width * shoppingCartItem.Height;
                weightOfShoppingCart += shoppingCartItem.Weight;
            }

            return (priceOfShoppingCart, volumeOfShoppingCart, weightOfShoppingCart);
        }

        public virtual GetShippingOptionResponse GetRates(GetShippingOptionRequest shippingOptionRequest)
        {
            var response = new GetShippingOptionResponse();

            
            List<string> servicesVariantCodes, servicesCodeCodes = new List<string>();

            //get services variants and codes
            (servicesVariantCodes, servicesCodeCodes) = GetServicesVariantAndCodeCodes();

            List<Product> shoppingCartItems = GetCustomerShoppingCartItems();

            decimal priceOfShoppingCart = 0;
            decimal volumeOfShoppingCart = 0;
            decimal weightOfShoppingCart = 0;

            //get price, volume and weight of all items in shopping cart 
            (priceOfShoppingCart, volumeOfShoppingCart, weightOfShoppingCart) =
                GetProductsPriceVolumeWeight(shoppingCartItems);

            priceOfShoppingCart = Math.Round(priceOfShoppingCart, 2);

            //var calculator = new DPDCalculatorClient();

            //var geographyClient = new DPDGeography2Client();

            var customerAddress =
                _addressRepository.Table.FirstOrDefault(x => x.Id == _workContext.CurrentCustomer.ShippingAddressId);

            var venderAddress =
                _addressRepository.Table.FirstOrDefault(x => x.Id == _workContext.CurrentVendor.AddressId);

            /*var geographyResponse = geographyClient.getCitiesCashPayAsync(new dpdCitiesCashPayRequest
            {
               auth = new auth
                {
                    clientKey = _dpdSettings.ClientKey,
                    clientNumber = _dpdSettings.ClientNumber
                },
                countryCode = "+" + customerAddress.CountryId
            }).Result;

            var customerCity = geographyResponse.@return.FirstOrDefault(x =>
                customerAddress.City.Trim().ToLower() == x.cityName.Trim().ToLower());

            var venderCity = geographyResponse.@return.FirstOrDefault(x =>
                venderAddress.City.Trim().ToLower() == x.cityName.Trim().ToLower());


            foreach (var serviceVariantCode in servicesCodeCodes)
                foreach (var servicesCodeCode in servicesCodeCodes)
                {
                    var serviceCost = calculator.getServiceCost2Async(new serviceCostRequest
                    {
                        auth = new Calculator.auth
                        {
                            clientKey = _dpdSettings.ClientKey,
                            clientNumber = _dpdSettings.ClientNumber
                        },
                        declaredValue = priceAllProductsInShoppingCart,
                        delivery = new cityRequest
                        {
                            cityId = customerCity.cityId,
                            cityIdSpecified = customerCity.cityIdSpecified,
                            cityName = customerCity.cityName,
                            countryCode = customerCity.countryCode,
                            index = customerCity.indexMax,
                            regionCode = customerCity.regionCode,
                            regionCodeSpecified = customerCity.regionCodeSpecified
                        },
                        pickup = new cityRequest
                        {
                            cityId = venderCity.cityId,
                            cityIdSpecified = venderCity.cityIdSpecified,
                            cityName = venderCity.cityName,
                            countryCode = venderCity.countryCode,
                            index = venderCity.indexMax,
                            regionCode = venderCity.regionCode,
                            regionCodeSpecified = venderCity.regionCodeSpecified
                        },
                        serviceCode = servicesCodeCode,
                        selfDelivery = serviceVariantCode == "ToTerminal",
                        volume = volumeOfShoppingCart,
                        weight = weightOfShoppingCart
                    }).Result;

                    response.ShippingOptions.Add(new ShippingOption
                    {
                        Name = servicesCodeCode,
                        Description = serviceVariantCode,
                        Rate = Math.Ceiling((decimal)serviceCost.@return[0].cost)
                    });
                }
            */
            return response;
        }
    }
}