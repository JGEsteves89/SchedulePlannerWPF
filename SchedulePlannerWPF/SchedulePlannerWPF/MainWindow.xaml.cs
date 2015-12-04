using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SchedulePlannerWPF {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            oCal.RangeChanged += new Calendar.ChangedEventHandler(RangeChanged);
            oCal.SetDateRange(DateTime.Today, DateTime.Today);
        }

        private void RangeChanged(object sender, DateTime DateStart, DateTime DateEnd) {
            DataTable Controlos = SGCEBL.PGGET.GetCONTROLOSinputCODCOPERACAOandDATAINIandDATAFIM("01", DateStart, DateEnd);
            List<ProductOrder> Cntrls = new List<ProductOrder>();
            foreach (Machine item in oCal.Machines) {
                item.Ordens.Clear();
            }
            foreach (DataRow iRow in Controlos.Rows) {
                DateTime Ini = iRow.Field<DateTime>("Data Início");
                DateTime Fim = iRow.Field<DateTime>("Data Fim");
                String Maq = iRow.Field<string>("Equipamento");
                String txt = iRow.Field<object>("OPP").ToString() + ":" + iRow.Field<string>("CODARTIGO");
                ProductOrder po = Cntrls.Find(x => x.Equipamento == Maq && x.DateStart == Ini);
                if (po == null)Cntrls.Add(new ProductOrder(Maq + "_" + Cntrls.Count, txt, 0, Ini, Fim, 0, Maq));
                else po.Text += "+" + txt;
            }
            foreach (ProductOrder po in Cntrls) {
                Machine CurMac = oCal.Machines.Find(x => x.Name == po.Equipamento);
                if (CurMac == null) {
                    CurMac = new Machine(po.Equipamento);
                    oCal.Machines.Add(CurMac);
                }
                CurMac.Ordens.Add(po);
            }
            oCal.oRefresh();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            
        }

    }

}
