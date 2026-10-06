using System.Collections.Generic;

namespace Sinapse.BoletoAPI
{
    public class BoletoResponse
    {
        private IList<Boleto2Net.Boleto> _boletos;
        
        public virtual IList<Boleto2Net.Boleto> boletos
        {
            get
            {
                if (_boletos == null)
                    _boletos = new List<Boleto2Net.Boleto>();
                return _boletos;
            }
            set
            {
                _boletos = value;
            }
        }
        
        public Boleto2Net.Boleto boleto { get; set; }
        public byte[] pdf { get; set; }
        public byte[] rem { get; set; }
        public string html { get; set; }
        public string linhaDigitavel { get; set; }
    }
}
