using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteka
{


    public enum StanjeProjekta { naCekanju, uIzradi, zavrseno }

    [Serializable]
    public class ZadatakProjekta
    {
        public string NazivProjekta { get; set; }
        public string Zaposleni { get; set; }
        public string RokIzrade { get; set; }
        public int prioritet { get; set; }
        public StanjeProjekta stanje { get; set; }
    }
}
