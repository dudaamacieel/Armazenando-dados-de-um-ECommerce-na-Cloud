using System;

namespace fnPayment.Model
{
    public class PaymanetModel
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public Guid IdPaymanet { get; set; } = Guid.NewGuid();

        public string nome { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string modelo { get; set; } = string.Empty;
        public int ano { get; set; }
        public string tempoAluguel { get; set; } = string.Empty;
        public DateTime data { get; set; }
        public string status { get; set; } = string.Empty;
        public DateTime? dataAprovacao { get; set; }
    }
}