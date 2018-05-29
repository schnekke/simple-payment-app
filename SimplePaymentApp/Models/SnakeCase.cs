using System;

namespace SimplePaymentApp.Models
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SnakeCase : Attribute
    {
        public String Value { get; set; }
        public Boolean Order { get; set; }
    }
}
