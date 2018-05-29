using System;

namespace SimplePaymentApp.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Updateable : Attribute
    {
        public String Name { get; set; }
        public String OnlyProperty{ get; set; }
    }
}