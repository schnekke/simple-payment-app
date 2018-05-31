using System;

namespace SimplePaymentApp.Models
{
    public class TokenModel
    {
        public string Currency3D => "EUR";
        public string Mode => "CONNECTOR_TEST";
        public string PFunction => "paymilljstests";
        public string Account { get; set; }
        public string Holder { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Verification { get; set; }
        public double Amount3D { get; set; }
    }
}