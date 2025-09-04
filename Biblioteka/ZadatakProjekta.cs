using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteka
{

    [Serializable]
    public enum StanjeProjekta { naCekanju, uIzradi, zavrseno }
    public class ZadatakProjekta
    {
        public string NazivProjekta { get; set; }
        public string zaposleni {  get; set; }
        string rok;
        int prioritet;
        public StanjeProjekta stanje { get; set; }
    }
}
