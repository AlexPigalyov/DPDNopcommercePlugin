namespace Nop.Plugin.Shipping.DPD.Domain
{
    public enum PaymentType
    {
        [DPDCode("ОУП")] OUP,
        [DPDCode("ОУО")] OUO
    }
}