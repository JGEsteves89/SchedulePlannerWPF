using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace WpfApplication1 {
    /// <summary>
    /// Interaction logic for Calendar.xaml
    /// </summary>
    public partial class Calendar : UserControl {
        public Calendar() {
            InitializeComponent();
            oRefresh();
        }
        private DateTime StartDate=DateTime.Today;
        private DateTime EndDate = DateTime.Today.AddHours(24);
        public void SetDateRange(DateTime iStartDate, DateTime iEndDate) {
            if (iStartDate == null && iEndDate == null) {
                StartDate = DateTime.Today;
                EndDate = StartDate.Date.AddHours(24);
            } else if (iStartDate == null) {
                StartDate = iEndDate.Date;
                EndDate = StartDate.Date.AddHours(24);
            } else if (iEndDate == null) {
                StartDate = iStartDate.Date;
                EndDate = StartDate.Date.AddHours(24);
            } else if (iEndDate.Date == iStartDate.Date) {
                StartDate = iStartDate.Date;
                EndDate = StartDate.Date.AddHours(24);
            } else if (iEndDate.Date < iStartDate.Date) {
                StartDate = iEndDate.Date;
                EndDate = iStartDate.Date.AddHours(24);
            } else {
                StartDate = iStartDate.Date;
                EndDate = iEndDate.Date.AddHours(24);
            }
            oRefresh();
        }

        private long LowlDiv = new TimeSpan(0, 20, 0).Ticks;
        private long MedlDiv = new TimeSpan(1, 0, 0).Ticks;
        private long HighDiv = new TimeSpan(24, 0, 0).Ticks;
        private string MedFormat = "HH";
        public enum TimeDivisons {
            Minute,
            FiveMinute,
            QuarterHour,
            HalfHour,
            Hour,
            TwoHours,
            FourHours,
            Shift,
            Day
        }

        private TimeDivisons timeDivision=TimeDivisons.Hour;

        public TimeDivisons TimeDivison {
            get { return timeDivision; }
            set { timeDivision = value;
                switch (value) {
                    case TimeDivisons.Minute:
                        LowlDiv = new TimeSpan(0, 0, 20).Ticks;
                        MedlDiv = new TimeSpan(0, 1, 0).Ticks;
                        HighDiv = new TimeSpan(24, 0, 0).Ticks;
                        MedFormat = "HH:mm";
                        break;
                    case TimeDivisons.FiveMinute:
                        LowlDiv = new TimeSpan(0, 1 ,40).Ticks;
                        MedlDiv = new TimeSpan(0, 5, 0).Ticks;
                        HighDiv = new TimeSpan(24, 0, 0).Ticks;
                        MedFormat = "HH:mm";
                        break;
                    case TimeDivisons.QuarterHour:
                        LowlDiv = new TimeSpan(0, 5, 0).Ticks;
                        MedlDiv = new TimeSpan(0, 15, 0).Ticks;
                        HighDiv = new TimeSpan(24, 0, 0).Ticks;
                        MedFormat = "HH:mm";
                        break;
                    case TimeDivisons.HalfHour:
                        LowlDiv = new TimeSpan(0, 10, 0).Ticks;
                        MedlDiv = new TimeSpan(0, 30, 0).Ticks;
                        HighDiv = new TimeSpan(24, 0, 0).Ticks;
                        MedFormat = "HH:mm";
                        break;
                    case TimeDivisons.Hour:
                        LowlDiv = new TimeSpan(0, 20, 0).Ticks;
                        MedlDiv = new TimeSpan(1, 0, 0).Ticks;
                        HighDiv = new TimeSpan(24, 0, 0).Ticks;
                        MedFormat = "HH";
                        break;
                    case TimeDivisons.TwoHours:
                        LowlDiv = new TimeSpan(0, 40, 0).Ticks;
                        MedlDiv = new TimeSpan(2, 0, 0).Ticks;
                        HighDiv = new TimeSpan(24, 0, 0).Ticks;
                        MedFormat = "HH";
                        break;
                    case TimeDivisons.FourHours:
                        LowlDiv = new TimeSpan(1, 20, 0).Ticks;
                        MedlDiv = new TimeSpan(4, 0, 0).Ticks;
                        HighDiv = new TimeSpan(24, 0, 0).Ticks;
                        MedFormat = "HH";
                        break;
                    case TimeDivisons.Shift:
                        LowlDiv = new TimeSpan(2, 40, 0).Ticks;
                        MedlDiv = new TimeSpan(8, 0, 0).Ticks;
                        HighDiv = new TimeSpan(24, 0, 0).Ticks;
                        MedFormat = "HH";
                        break;
                    case TimeDivisons.Day:
                        LowlDiv = new TimeSpan(8, 0, 0).Ticks;
                        MedlDiv = new TimeSpan(24, 0, 0).Ticks;
                        HighDiv = new TimeSpan(24, 0, 0).Ticks;
                        MedFormat = "HH";
                        break;
                    default:
                        break;
                }
                oRefresh(); }
        }
        private double zoom = 10;

        public double Zoom {
            get { return zoom; }
            set { zoom = value; oRefresh(); }
        }

        private double tickPixel= 0.00000000011574074074;

        public double TickPixel {
            get { return tickPixel* zoom; }
            set { tickPixel = value; oRefresh(); }
        }

        private int DayHeaderSize = 30;
        private int HourHeaderSize = 0;
        private int viewPortX = 0;
        private double viewPortWith {
            get {
                if (double.IsNaN(PanelCalendar.ActualWidth)) return 418;
                else return PanelCalendar.ActualWidth;
            }
        }
        public void oRefresh() {
            RefreshPanelCalendar();
            RefreshPanelOverView();
        }
        public void RefreshPanelCalendar() {
            long TimeWindow = EndDate.Subtract(StartDate).Ticks;
            double TempHeight;
            if (PanelCalendar.ActualHeight != 0) TempHeight = PanelMac.ActualHeight;
            else TempHeight = 91;
            PanelCalendar.Children.Clear();
            for (int i = 0; i < TimeWindow / LowlDiv; i++) {
                int Xt = (int)((double)i * ((double)TickPixel * (double)LowlDiv))- viewPortX;
                int Xf = Xt + (int)((double)((double)TickPixel * (double)LowlDiv));
                if ((Xt >= 0 && Xt <= 0 + viewPortWith) || ((Xt < 0 && Xf >= 0))) {
                    PanelCalendar.Children.Add(CreateFrameLine(Xt, DayHeaderSize + HourHeaderSize, TempHeight, 5, Colors.White));
                    PanelCalendar.Children.Add(CreateFrameDivision(Xt, DayHeaderSize + HourHeaderSize, TickPixel * LowlDiv, TempHeight, 1, Colors.White));
                } 
            }

            for (int i = 0; i < TimeWindow / MedlDiv; i++) {
                DateTime Cur = StartDate.Add(new TimeSpan((long)((double)i * ((double)MedlDiv))));
                string text = Cur.ToString(MedFormat);
                int Xt = (int)((long)i * (TickPixel * MedlDiv))- viewPortX;
                int Xf = Xt + (int)((double)(TickPixel * MedlDiv));
                if ((Xt >= 0 && Xt <= 0 + viewPortWith) || ((Xt < 0 && Xf >= 0))) {
                    PanelCalendar.Children.Add(CreateFrameDivision(Xt, DayHeaderSize, TickPixel * MedlDiv, TempHeight, 1, Colors.LightGray));
                    PanelCalendar.Children.Add(CreateFrameLine(Xt, DayHeaderSize, TempHeight, 10, Colors.LightGray));
                    PanelCalendar.Children.Add(CrateTextBlock(Xt, DayHeaderSize - 15, text, "Arial,12"));
                } 
            }
            for (int i = 0; i < TimeWindow / HighDiv; i++) {
                DateTime Cur = StartDate.Add(new TimeSpan((long)((double)i * ((double)HighDiv))));
                string text = Cur.Date.ToString("yyyy/MM/dd");
                int Xt = (int)((double)i * ((double)TickPixel * (double)HighDiv)) - viewPortX;
                int Xf = Xt + (int)((double)(TickPixel * HighDiv));
                int Xt1 = (int)((double)Xt + ((double)Xf - (double)Xt) / (double)2);
                if ((Xt >= 0 && Xt <= 0 + viewPortWith) || ((Xt < 0 && Xf >= 0))) {
                    PanelCalendar.Children.Add(CreateFrameDivision(Xt, 0, TickPixel * HighDiv, TempHeight, 2, Colors.Gray));
                    PanelCalendar.Children.Add(CrateTextBlock(Xt1, 3, text, "Arial,14"));
                }
            }
        }
        public void RefreshPanelOverView() {
            long TimeWindow = EndDate.Subtract(StartDate).Ticks;
            PanelOverView.Children.Clear();
            double TempHeight;
            if (PanelOverView.ActualHeight != 0) TempHeight = PanelOverView.ActualHeight;
            else TempHeight = 91;
            long MyLowlDiv = new TimeSpan(1, 20, 0).Ticks;
            long MyMedlDiv = new TimeSpan(4, 0, 0).Ticks;
            long MyHighDiv = new TimeSpan(24, 0, 0).Ticks;
            double MyTickPixel = ((double)PanelOverView.ActualWidth / (double)TimeWindow);
            string MyMedFormat = "HH";

            for (int i = 0; i < TimeWindow / MyLowlDiv; i++) {
                int Xt = (int)((double)i * ((double)MyTickPixel * (double)MyLowlDiv));
                int Xf = Xt + (int)((double)((double)MyTickPixel * (double)MyLowlDiv));
                PanelOverView.Children.Add(CreateFrameLine(Xt, DayHeaderSize + HourHeaderSize, MyLowlDiv, 5, Colors.White));
                PanelOverView.Children.Add(CreateFrameDivision(Xt, DayHeaderSize + HourHeaderSize, MyTickPixel * MyLowlDiv, TempHeight, 1, Colors.White));
            }
            for (int i = 0; i < TimeWindow / MyMedlDiv; i++) {
                DateTime Cur = StartDate.Add(new TimeSpan((long)((double)i * ((double)MyMedlDiv))));
                string text = Cur.ToString(MyMedFormat);
                int Xt = (int)((long)i * (MyTickPixel * MyMedlDiv));
                int Xf = Xt + (int)((double)(MyTickPixel * MyMedlDiv));
                PanelOverView.Children.Add(CreateFrameDivision(Xt, DayHeaderSize, MyTickPixel * MyMedlDiv, TempHeight, 1, Colors.LightGray));
                PanelOverView.Children.Add(CreateFrameLine(Xt, DayHeaderSize, MyMedlDiv, 10, Colors.LightGray));
                PanelOverView.Children.Add(CrateTextBlock(Xt, DayHeaderSize - 15, text, "Arial,12"));
            }
            for (int i = 0; i < TimeWindow / MyHighDiv; i++) {
                DateTime Cur = StartDate.Add(new TimeSpan((long)((double)i * ((double)MyHighDiv))));
                string text = Cur.Date.ToString("yyyy/MM/dd");
                int Xt = (int)((double)i * ((double)MyTickPixel * (double)MyHighDiv));
                int Xf = Xt + (int)((double)(MyTickPixel * MyHighDiv));
                int Xt1 = (int)((double)Xt + ((double)Xf - (double)Xt) / (double)2);
                PanelOverView.Children.Add(CreateFrameDivision(Xt, 0, MyTickPixel * MyHighDiv, TempHeight, 2, Colors.Gray));
                PanelOverView.Children.Add(CrateTextBlock(Xt1, 3, text, "Arial,14"));
            }
            int ActuaPos = (int)((double)viewPortX * ((double)MyTickPixel / (double)TickPixel));
            int ActuaWid = (int)((double)viewPortWith * ((double)MyTickPixel / (double)TickPixel));

            Rectangle ViewPort = CreateFrameDivision(ActuaPos, 0, ActuaWid, TempHeight, 2, Colors.DarkGray);
            ViewPort.Fill = new SolidColorBrush(Colors.LightGray);
            ViewPort.Opacity = 0.5;
            PanelOverView.Children.Add(ViewPort);
        }
        private TextBlock CrateTextBlock(int x, int y,string text, string Font) {
            TextBlock txt = new TextBlock();
            txt.Text = text;
            txt.FontStyle = FontStyles.Normal;
            txt.FontFamily = new FontFamily(Font);
            var formattedText = new FormattedText(text,CultureInfo.CurrentUICulture,FlowDirection.LeftToRight,
                new Typeface(txt.FontFamily, txt.FontStyle, txt.FontWeight, txt.FontStretch),
                txt.FontSize,Brushes.Black);
            Canvas.SetTop(txt, y);
            Canvas.SetLeft(txt, x-(int)((double)formattedText.Width / (double)2));
            return txt;
        }
        private Rectangle CreateFrameDivision(int x, int y, double Width, double height,int Stroke,Color Cltr) {
            Rectangle rect = new Rectangle();
            rect.StrokeThickness = Stroke;
            rect.Stroke = new SolidColorBrush(Cltr);
            rect.Fill = null;
            rect.Width = Width;
            rect.Height = height-y;
            Canvas.SetTop(rect,y);
            Canvas.SetLeft(rect, x);
            return rect;
        }
        private Line CreateFrameLine(int x, int y, double height, int Stroke, Color Cltr) {
            Line line = new Line();
            line.StrokeThickness = 2;
            line.Stroke = new SolidColorBrush(Cltr);
            line.X1 = x;
            line.X2 = x;
            line.Y1 = y -  Stroke;
            line.Y2 = height;
            Canvas.SetTop(line,0);
            Canvas.SetLeft(line,0);
            return line;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            SetDateRange(DateTime.Today, DateTime.Today.AddDays(1));
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e) {
            double value = Zoom;
            if (e.Delta > 0) {
                value = value * 1.1;
            } else if (e.Delta < 0) {
                value = value * 0.9;
            }
            again:
            if (value < 0.7) {
                value = 0.7;
                TimeDivison = TimeDivisons.Day;
            } else if (value >= 0.7 && value < 1.4) {
                TimeDivison = TimeDivisons.Day;
            } else if (value >= 1.4 && value < 2.8) {
                TimeDivison = TimeDivisons.Shift;
            } else if (value >= 2.8 && value < 4.6) {
                TimeDivison = TimeDivisons.FourHours;
            } else if (value >= 4.6 && value < 14) {
                TimeDivison = TimeDivisons.TwoHours;
            } else if (value >= 14 && value < 30) {
                TimeDivison = TimeDivisons.Hour;
            } else if (value >= 30 && value < 80) {
                TimeDivison = TimeDivisons.HalfHour;
            } else if (value >= 80 && value < 350) {
                TimeDivison = TimeDivisons.QuarterHour;
            } else if (value >= 350 && value < 730) {
                TimeDivison = TimeDivisons.FiveMinute;
            } else if (value > 730) {
                value = 730;
                TimeDivison = TimeDivisons.Minute;
            }
            double BeforeSize = (double)EndDate.Subtract(StartDate).Ticks * (double)TickPixel;
            double AfterSize = (double)EndDate.Subtract(StartDate).Ticks * (double)tickPixel * (double)value;
            viewPortX = viewPortX + (int)(e.GetPosition(PanelCalendar).X);
            viewPortX = (int)((double)viewPortX * (AfterSize / BeforeSize));
            viewPortX = viewPortX - (int)(e.GetPosition(PanelCalendar).X);
            if (viewPortX < 0) viewPortX = 0;
            if (AfterSize < viewPortWith)viewPortX = 0;
            if (AfterSize < viewPortWith) {
                if (BeforeSize < viewPortWith) {
                    value = value * 1.1;
                    goto again;
                } else {
                    value = Zoom;
                    goto again;
                }
            } else Zoom = value;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) {
            oRefresh();
        }
    }
}
