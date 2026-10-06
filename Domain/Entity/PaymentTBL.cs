using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class PaymentTBL : MainEntity
    {
        public long ID { get; set; }
        public string FriendlyID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public long amount { get; set; }
        public string subject { get; set; }
        public string source { get; set; }
        public string date { get; set; }
        public float tax { get; set; }
        public long netAmount { get; set; }
        public string documentNumber { get; set; }
        public string? bankName { get; set; }
        public string? paymentType { get; set; }
        public string to { get; set; }
    }
}
