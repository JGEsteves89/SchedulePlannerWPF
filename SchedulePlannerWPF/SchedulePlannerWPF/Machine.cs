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

        #endregion Constructors
    }
}
