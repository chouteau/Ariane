using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane5.AzureTopicClientWriters
{
    public class ValidationPaymentRequest
    {
        public ValidationPaymentRequest()
        {
            BankResponseParameters = new List<ValidationPaymentKeyValueParameter>();
        }
        public string PaymentModeCode { get; set; }
        public string BankIp { get; set; }
        public DateTime TransactionDate { get; set; }
        public List<ValidationPaymentKeyValueParameter> BankResponseParameters { get; set; }
    }
}
