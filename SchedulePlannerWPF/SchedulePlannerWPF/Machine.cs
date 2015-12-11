using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace SchedulePlannerWPF {
    public class Machine {
        #region Fields

        public Rectangle DrawnRect;
        public string Name;
        public List<ProductOrder> Ordens;

        #endregion Fields

        #region Constructors

        public Machine(string iName) {
            Name = iName;
            Ordens = new List<ProductOrder>();
        }

        public void SortOrders() {
            ProductOrder Aux =null;
            for (int i = 0; i < Ordens.Count-1; i++) {
                for (int j = i+1; j < Ordens.Count; j++) {
                    if (Ordens[i].DateStart> Ordens[j].DateStart ) {
                        Aux = Ordens[i];
                        Ordens[i] = Ordens[j];
                        Ordens[j] = Aux;
                    }
                }
            }
            RecalcID();
       }

        internal void RecalcID() {
            for (int t = 0; t < Ordens.Count; t++) {
                Ordens[t].ItemID = Name + "_" + t;
                Ordens[t].DrawnRect.Name = Name + "_" + t;
            }
        }
        #endregion Constructors
    }
}
