using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace SchedulePlannerWPF {
   public class ProductOrder {
        #region Fields

        public DateTime DateEnd;
        public DateTime DateStart;
        public Rectangle DrawnRect;
        public TimeSpan Duration;

        //public List<ProductOrder> Owner;
        //public int Container;
        public string Equipamento;
        public int Index;
        public bool IsSelected = false;
        public int State;
        public string Text;

        #endregion Fields

        #region Constructors

        public ProductOrder(string iText, int iState, DateTime iDateStart, TimeSpan iDuration, int iIndex, string iEquipamento) {
            IsSelected = false;
            //Owner = iOwner;
            Text = iText;
            State = iState;
            DateStart = iDateStart;
            Duration = iDuration;
            DateEnd = iDateStart.Add(iDuration);
            Index = iIndex;
            //Container = iContainer;
            Equipamento = iEquipamento;
        }

        public ProductOrder(string iText, int iState, DateTime iDateStart, DateTime iDateEnd, int iIndex, string iEquipamento) {
            IsSelected = false;
            //Owner=iOwner;
            Text = iText;
            State = iState;
            DateStart = iDateStart;
            DateEnd = iDateEnd;
            if (iDateEnd < DateStart) {
                DateStart = iDateEnd; DateEnd = iDateStart;
            }
            Duration = DateEnd.Subtract(DateStart);
            Index = iIndex;
            //Container=iContainer;
            Equipamento = iEquipamento;
        }

        internal string GetState() {
            if (State == 1) return "Planeado";
            if (State == 2) return "Bloqueado";
            if (State == 3) return "Paragem";
            return "Controlado";
        }

        #endregion Constructors
    }
}
