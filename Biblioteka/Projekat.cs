using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteka
{

    [Serializable]
    public enum StanjeProjekta { naCekanju, uIzradi, zavrseno }
    public class Projekat
    {
        public string NazivProjekta { get; set; }
        public StanjeProjekta stanje { get; set; }
    }
}
