namespace Nop.Plugin.Shipping.DPD.Domain
{
    public enum ServiceVariantType
    {
        [DPDCode("ToTerminal")] TD,
        [DPDCode("ToPerson")] TT
    }
}