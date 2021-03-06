﻿using System;
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
using System.Windows.Threading;

namespace SchedulePlannerWPF {
    /// <summary>
    /// Interaction logic for Calendar.xaml
    /// </summary>
    public partial class Calendar : UserControl {
        public Calendar() {
            InitializeComponent();

            ItemContextMenu = new ContextMenu();
            MenuItem Block = new MenuItem();
            Block.Header = "Block";
            Block.Click += new RoutedEventHandler(MenuBlock_Click);
            ItemContextMenu = new ContextMenu();
            MenuItem UnBlock = new MenuItem();
            UnBlock.Header = "Unblock";
            UnBlock.Click += new RoutedEventHandler(MenuUnBlock_Click);
            MenuItem Delete = new MenuItem();
            Delete.Header = "Delete";
            Delete.Click += new RoutedEventHandler(MenuDelete_Click);
            ItemContextMenu.Items.Add(Block);
            ItemContextMenu.Items.Add(UnBlock);
            ItemContextMenu.Items.Add(Delete);

            oSel = new Selection(this);
            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            oRefresh();
        }
        public ContextMenu ItemContextMenu;
        public delegate void ChangedEventHandler(object sender, DateTime DateStart, DateTime DateEnd);
        public event ChangedEventHandler RangeChanged;
        protected virtual void OnRangeChanged(DateTime DateStart, DateTime DateEnd) {
            if (RangeChanged != null)
                RangeChanged(this, DateStart, DateEnd);
        }
        private long LowlDiv = new TimeSpan(0, 20, 0).Ticks;
        private long MedlDiv = new TimeSpan(1, 0, 0).Ticks;
        private long HighDiv = new TimeSpan(24, 0, 0).Ticks;
        public Random random = new Random();
        public Line TimeLine1;
        public Line TimeLine2;
        private string MedFormat = "HH";
        private Point eGetPositionPanelCalendar = new Point(0, 0);
        private int DayHeaderSize = 30;
        private int HourHeaderSize = 0;
        private int MachineHeaderHeight = 60;
        public int ItemSlack = 3;
        private double viewPortX = 0;
        private double viewPortWith {
            get {
                if (double.IsNaN(PanelCalendar.ActualWidth) || PanelCalendar.ActualWidth == 0) return 418;
                else return PanelCalendar.ActualWidth;
            }
        }

        private DateTime StartDate = DateTime.Today;
        private DateTime EndDate = DateTime.Today;

        public void SetDateRange(DateTime iStartDate, DateTime iEndDate) {
            if (iStartDate == null && iEndDate == null) {
                StartDate = DateTime.Today;
                EndDate = StartDate.Date.AddHours(24);
                textinit.Text = StartDate.ToLongDateString();
                textfinit.Text = "";
            } else if (iStartDate == null) {
                StartDate = iEndDate.Date;
                EndDate = StartDate.Date.AddHours(24);
                textinit.Text = StartDate.ToLongDateString();
                textfinit.Text = "";
            } else if (iEndDate == null) {
                StartDate = iStartDate.Date;
                EndDate = StartDate.Date.AddHours(24);
                textinit.Text = StartDate.ToLongDateString();
                textfinit.Text = "";
            } else if (iEndDate.Date == iStartDate.Date) {
                StartDate = iStartDate.Date;
                EndDate = StartDate.Date.AddHours(24);
                textinit.Text = StartDate.ToLongDateString();
                textfinit.Text = "";
            } else if (iEndDate.Date < iStartDate.Date) {
                StartDate = iEndDate.Date;
                EndDate = iStartDate.Date.AddHours(24);
                textinit.Text = StartDate.ToLongDateString();
                textfinit.Text = " - " + EndDate.ToLongDateString();
            } else {
                StartDate = iStartDate.Date;
                EndDate = iEndDate.Date.AddHours(24);
                textinit.Text = StartDate.ToLongDateString();
                textfinit.Text = " - " + EndDate.ToLongDateString();
            }
            double TimeWindow = EndDate.Subtract(StartDate).Ticks;
            double SpectedPixelTick = viewPortWith / TimeWindow;
            zoom = SpectedPixelTick / tickPixel;
            Zoom = SpectedPixelTick / tickPixel;
            viewPortX = 0;
            OnRangeChanged(StartDate, EndDate);
        }

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
        private TimeDivisons timeDivision = TimeDivisons.Hour;
        public TimeDivisons TimeDivison {
            get { return timeDivision; }
            set {
                timeDivision = value;
                switch (value) {
                    case TimeDivisons.Minute:
                        LowlDiv = new TimeSpan(0, 0, 20).Ticks;
                        MedlDiv = new TimeSpan(0, 1, 0).Ticks;
                        HighDiv = new TimeSpan(24, 0, 0).Ticks;
                        MedFormat = "HH:mm";
                        break;
                    case TimeDivisons.FiveMinute:
                        LowlDiv = new TimeSpan(0, 1, 40).Ticks;
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
            }
        }

        private double zoom = 1;
        public double Zoom {
            get { return zoom; }
            set {
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
                viewPortX = viewPortX + (int)(eGetPositionPanelCalendar.X);
                viewPortX = (int)((double)viewPortX * (AfterSize / BeforeSize));
                viewPortX = viewPortX - (int)(eGetPositionPanelCalendar.X);
                if (viewPortX < 0) viewPortX = 0;
                if (Math.Round(AfterSize, 0) < Math.Round(viewPortWith, 0)) {
                    viewPortX = 0;
                    double TimeWindow = EndDate.Subtract(StartDate).Ticks;
                    double SpectedPixelTick = viewPortWith / TimeWindow;
                    value = SpectedPixelTick / tickPixel;
                    goto again;
                }
                zoom = value;
                oRefresh();
            }
        }

        private double tickPixel = 0.00000000011574074074;
        public double TickPixel {
            get { return tickPixel * zoom; }
            set { tickPixel = value; oRefresh(); }
        }

        public Selection oSel ;
        public List<Machine> Machines = new List<Machine>();

        public void oRefresh() {
            RefreshPanelCalendar();
            RefreshPanelOverView();
            RefreshPanelItems();
            RefreshTimeLine();
        }
        public void RefreshPanelCalendar() {
            long TimeWindow = EndDate.Subtract(StartDate).Ticks;
            double TempHeight = 91;
            if (PanelCalendar.ActualHeight != 0) TempHeight = PanelMac.ActualHeight;
            PanelCalendar.Children.Clear();
            for (int i = 0; i < TimeWindow / LowlDiv; i++) {
                double Xt = ((double)i * ((double)TickPixel * (double)LowlDiv))- viewPortX;
                double Xf = Xt + ((double)((double)TickPixel * (double)LowlDiv));
                if ((Xt >= 0 && Xt <= 0 + viewPortWith) || ((Xt < 0 && Xf >= 0))) {
                    PanelCalendar.Children.Add(CreateFrameLine(Xt, DayHeaderSize + HourHeaderSize, 0, TempHeight, 5, 2,Colors.White));
                    PanelCalendar.Children.Add(CreateFrameDivision(Xt, DayHeaderSize + HourHeaderSize, TickPixel * LowlDiv, TempHeight, 1, Colors.White));
                } 
            }

            for (int i = 0; i < TimeWindow / MedlDiv; i++) {
                DateTime Cur = StartDate.Add(new TimeSpan((long)((double)i * ((double)MedlDiv))));
                string text = Cur.ToString(MedFormat);
                double Xt = (double)((double)i * ((double)TickPixel * (double)MedlDiv))- viewPortX;
                double Xf = Xt + (double)((double)((double)TickPixel * (double)MedlDiv));
                if ((Xt >= 0 && Xt <= 0 + viewPortWith) || ((Xt < 0 && Xf >= 0))) {
                    PanelCalendar.Children.Add(CreateFrameDivision(Xt, DayHeaderSize, TickPixel * MedlDiv, TempHeight, 1, Colors.LightGray));
                    PanelCalendar.Children.Add(CreateFrameLine(Xt, DayHeaderSize,0, TempHeight, 10,2, Colors.LightGray));
                    PanelCalendar.Children.Add(CrateTextBlock(Xt, DayHeaderSize - 15, text, "Source Sans Pro Light,12",Colors.Black));
                } 
            }
            for (int i = 0; i < TimeWindow / HighDiv; i++) {
                DateTime Cur = StartDate.Add(new TimeSpan((long)((double)i * ((double)HighDiv))));
                string text = Cur.Date.ToString("yyyy/MM/dd");
                double Xt = (double)((double)i * ((double)TickPixel * (double)HighDiv)) - viewPortX;
                double Xf = Xt + (double)((double)(TickPixel * HighDiv));
                double Xt1 = (double)((double)Xt + ((double)Xf - (double)Xt) / (double)2);
                if ((Xt >= 0 && Xt <= 0 + viewPortWith) || ((Xt < 0 && Xf >= 0))) {
                    PanelCalendar.Children.Add(CreateFrameDivision(Xt, 0, TickPixel * HighDiv, TempHeight, 2, Colors.Gray));
                    PanelCalendar.Children.Add(CrateTextBlock(Xt1, 3, text, "Source Sans Pro Light,14", Colors.Black));
                }
            }
        }
        public void RefreshPanelOverView() {
            long TimeWindow = EndDate.Subtract(StartDate).Ticks;
            PanelOverView.Children.Clear();
            double TempHeight = 91;
            if (PanelOverView.ActualHeight != 0) TempHeight = PanelOverView.ActualHeight;
            long MyLowlDiv = new TimeSpan(1, 20, 0).Ticks;
            long MyMedlDiv = new TimeSpan(4, 0, 0).Ticks;
            long MyHighDiv = new TimeSpan(24, 0, 0).Ticks;
            double MyTickPixel = ((double)PanelOverView.ActualWidth / (double)TimeWindow);
            string MyMedFormat = "HH";

            for (int i = 0; i < TimeWindow / MyLowlDiv; i++) {
                int Xt = (int)((double)i * ((double)MyTickPixel * (double)MyLowlDiv));
                int Xf = Xt + (int)((double)((double)MyTickPixel * (double)MyLowlDiv));
                PanelOverView.Children.Add(CreateFrameLine(Xt, DayHeaderSize + HourHeaderSize,0, MyLowlDiv, 5,2, Colors.White));
                PanelOverView.Children.Add(CreateFrameDivision(Xt, DayHeaderSize + HourHeaderSize, MyTickPixel * MyLowlDiv, TempHeight, 1, Colors.White));
            }
            for (int i = 0; i < TimeWindow / MyMedlDiv; i++) {
                DateTime Cur = StartDate.Add(new TimeSpan((long)((double)i * ((double)MyMedlDiv))));
                string text = Cur.ToString(MyMedFormat);
                int Xt = (int)((long)i * (MyTickPixel * MyMedlDiv));
                int Xf = Xt + (int)((double)(MyTickPixel * MyMedlDiv));
                PanelOverView.Children.Add(CreateFrameDivision(Xt, DayHeaderSize, MyTickPixel * MyMedlDiv, TempHeight, 1, Colors.LightGray));
                PanelOverView.Children.Add(CreateFrameLine(Xt, DayHeaderSize,0, MyMedlDiv, 10,2, Colors.LightGray));
                PanelOverView.Children.Add(CrateTextBlock(Xt, DayHeaderSize - 15, text, "Source Sans Pro Light,10", Colors.Black));
            }
            for (int i = 0; i < TimeWindow / MyHighDiv; i++) {
                DateTime Cur = StartDate.Add(new TimeSpan((long)((double)i * ((double)MyHighDiv))));
                string text = Cur.Date.ToString("yyyy/MM/dd");
                int Xt = (int)((double)i * ((double)MyTickPixel * (double)MyHighDiv));
                int Xf = Xt + (int)((double)(MyTickPixel * MyHighDiv));
                int Xt1 = (int)((double)Xt + ((double)Xf - (double)Xt) / (double)2);
                PanelOverView.Children.Add(CreateFrameDivision(Xt, 0, MyTickPixel * MyHighDiv, TempHeight, 2, Colors.Gray));
                PanelOverView.Children.Add(CrateTextBlock(Xt1, 3, text, "Source Sans Pro Light,12", Colors.Black));
            }

            for (int i = 0; i < Machines.Count; i++) {
                double MyMachineHeaderHeight = (TempHeight - DayHeaderSize - HourHeaderSize) / (Machines.Count) ;
                double y0 = (i) * MyMachineHeaderHeight + DayHeaderSize + HourHeaderSize;
                double yt = (i + 1) * MyMachineHeaderHeight + DayHeaderSize + HourHeaderSize;

                Rectangle DrawnRect = CreateFrameDivision(0, yt, TimeWindow* MyTickPixel, yt + MyMachineHeaderHeight, 1, Colors.DimGray);
                Line Line1 = CreateFrameLine(0, yt, TimeWindow * MyTickPixel, 0, 0, 2, Colors.DimGray);
                PanelOverView.Children.Add(Line1);
                if (i == 0) {
                    Line Line0 = CreateFrameLine(0, y0, TimeWindow * MyTickPixel, 0, 0, 2, Colors.DimGray);
                    PanelOverView.Children.Add(Line0);
                }
                for (int po = 0; po < Machines[i].Ordens.Count; po++) {
                    double Posi = Machines[i].Ordens[po].DateStart.Subtract(StartDate).Ticks * MyTickPixel;
                    double Posf = Machines[i].Ordens[po].DateEnd.Subtract(StartDate).Ticks * MyTickPixel;
                    if ((Posi >= 0 && Posi <= 0 + TimeWindow * MyTickPixel) || ((Posi < 0 && Posf >= 0))) {
                        Rectangle DrawnRects = CreateFrameDivision(Posi, y0 + ItemSlack, Posf - Posi, yt - ItemSlack, 2, Colors.DimGray);
                        DrawnRects.Fill = new SolidColorBrush(Color.FromArgb(85, Colors.DimGray.R, Colors.DimGray.G, Colors.DimGray.B));
                        PanelOverView.Children.Add(DrawnRects);
                    }
                }
            }
            double ActuaPos = (double)((double)viewPortX * ((double)MyTickPixel / (double)TickPixel));
            double ActuaWid = (double)((double)viewPortWith * ((double)MyTickPixel / (double)TickPixel));

            Rectangle ViewPort = CreateFrameDivision(ActuaPos, 0, ActuaWid, TempHeight, 2, Colors.DarkGray);
            ViewPort.Fill = new SolidColorBrush(Colors.LightGray);
            ViewPort.Opacity = 0.5;
            PanelOverView.Children.Add(ViewPort);
        }
        public void RefreshPanelItems() {
            PanelItems.Children.Clear();
            for (int i = 0; i < Machines.Count; i++) {
                double y0 = PanelCalendar.Margin.Top + (i) * MachineHeaderHeight + DayHeaderSize + HourHeaderSize;
                double yt = PanelCalendar.Margin.Top + (i + 1) * MachineHeaderHeight + DayHeaderSize + HourHeaderSize;
                double x0 = PanelCalendar.Margin.Left;
                double xt = 0;
                if (double.IsNaN(y0)) return;

                Machines[i].DrawnRect = CreateFrameDivision(x0, y0, viewPortWith, y0 + MachineHeaderHeight, 1, Colors.DimGray);
                Line Line1 = CreateFrameLine(x0, yt, viewPortWith, 0, 0, 2, Colors.DimGray);
                PanelItems.Children.Add(Line1);
                if (i == 0) {
                    Line Line0 = CreateFrameLine(x0, y0, viewPortWith, 0, 0, 2, Colors.DimGray);
                    PanelItems.Children.Add(Line0);
                }
                for (int po = 0; po < Machines[i].Ordens.Count; po++) {
                    double Posi = Machines[i].Ordens[po].DateStart.Subtract(StartDate).Ticks * TickPixel - viewPortX;
                    double Posf = Machines[i].Ordens[po].DateEnd.Subtract(StartDate).Ticks * TickPixel - viewPortX;
                    if ((Posi >= 0 && Posi <= 0 + viewPortWith) || ((Posi < 0 && Posf >= 0))) {
                        Machines[i].Ordens[po].DrawnRect = CreateFrameDivision(x0 + Posi, y0 + ItemSlack, Posf - Posi, yt - ItemSlack, 2, Colors.DimGray);
                        if (Machines[i].Ordens[po].State == 1) {//planeado
                            Machines[i].Ordens[po].DrawnRect.Fill = new SolidColorBrush(Color.FromArgb(85, Colors.LightBlue.R, Colors.LightBlue.G, Colors.LightBlue.B));
                        } else if (Machines[i].Ordens[po].State == 2) {//bloqueado
                            Machines[i].Ordens[po].DrawnRect.Fill = new SolidColorBrush(Color.FromArgb(85, Colors.RosyBrown.R, Colors.RosyBrown.G, Colors.RosyBrown.B));
                        } else if (Machines[i].Ordens[po].State == 3) {//Paragem
                            Machines[i].Ordens[po].DrawnRect.Fill = new SolidColorBrush(Color.FromArgb(85, Colors.LightSteelBlue.R, Colors.LightSteelBlue.G, Colors.LightSteelBlue.B));
                        } else {//Paragem
                            Machines[i].Ordens[po].DrawnRect.Fill = new SolidColorBrush(Color.FromArgb(85, Colors.DimGray.R, Colors.DimGray.G, Colors.DimGray.B));
                        }

                        Machines[i].Ordens[po].DrawnRect.MouseEnter += new MouseEventHandler(Item_MouseEnter);
                        Machines[i].Ordens[po].DrawnRect.MouseLeave += new MouseEventHandler(Item_MouseLeave);
                        Machines[i].Ordens[po].DrawnRect.MouseLeftButtonDown += new MouseButtonEventHandler(Item_MouseLeftButtonDown);
                        Machines[i].Ordens[po].DrawnRect.MouseLeftButtonUp += new MouseButtonEventHandler(Item_MouseLeftButtonUp);
                        Machines[i].Ordens[po].DrawnRect.MouseRightButtonUp += new MouseButtonEventHandler(Item_MouseRightButtonUp);
                        Machines[i].Ordens[po].DrawnRect.ToolTip = string.Format("{0}\nTipo: {2}\nEquipamento: {1}", Machines[i].Ordens[po].Text, Machines[i].Ordens[po].Equipamento, Machines[i].Ordens[po].GetState());
                        Machines[i].Ordens[po].DrawnRect.Name = Machines[i].Ordens[po].ItemID;

                        if (Machines[i].Ordens[po].IsSelected)
                            Machines[i].Ordens[po].DrawnRect.StrokeThickness = 4;
                        PanelItems.Children.Add(Machines[i].Ordens[po].DrawnRect);
                        if (Machines[i].Ordens[po].State == 1) {
                            Canvas.SetZIndex(Machines[i].Ordens[po].DrawnRect, PanelItems.Children.Count - 1);
                        } else {
                            Canvas.SetZIndex(Machines[i].Ordens[po].DrawnRect, 0);
                        }
                    }
                }
                y0 = PanelCalendar.Margin.Top + (i) * MachineHeaderHeight + DayHeaderSize + HourHeaderSize;
                yt = PanelCalendar.Margin.Top + (i + 1) * MachineHeaderHeight + DayHeaderSize + HourHeaderSize;
                x0 = PanelMac.ActualWidth/2;
                xt = 0;
                PanelItems.Children.Add(CrateTextBlock(x0, y0 + (yt - y0) / 2, Machines[i].Name, "Source Sans Pro Semibold,12", Colors.White));

            }
        }
        public void RefreshTimeLine() {
            if (DateTime.Now >= StartDate || DateTime.Now <= EndDate) {
                long TimeWindow = EndDate.Subtract(StartDate).Ticks;
                long TimeNow = DateTime.Now.Subtract(StartDate).Ticks;
                double MyTickPixel = ((double)PanelOverView.ActualWidth / (double)TimeWindow);
                double TempHeightPanelCalendar = 91;
                if (PanelCalendar.ActualHeight != 0) TempHeightPanelCalendar = PanelMac.ActualHeight;
                double TempHeightPanelOverView = 91;
                if (PanelOverView.ActualHeight != 0) TempHeightPanelOverView = PanelOverView.ActualHeight;
                double PosNow = ((double)TickPixel * (double)TimeNow) - viewPortX;
                if ((PosNow >= 0 && PosNow <= 0 + viewPortWith)) {
                    TimeLine1 = CreateFrameLine(PosNow, 0, 0, TempHeightPanelCalendar, 0, 3, Colors.Red);
                    TimeLine1.Name = "TimeLine1";

                    try {
                        if (((Shape)PanelCalendar.Children[PanelCalendar.Children.Count - 1]).Name == TimeLine1.Name) {
                            PanelCalendar.Children.RemoveAt(PanelCalendar.Children.Count - 1);
                        }
                    } catch (Exception) { }

                    PanelCalendar.Children.Add(TimeLine1);
                }
                PosNow = ((double)MyTickPixel * (double)TimeNow);
                if ((PosNow >= 0 && PosNow <= 0 + TimeWindow * MyTickPixel)) {
                    TimeLine2 = CreateFrameLine(PosNow, 0, 0, TempHeightPanelOverView, 0, 3, Colors.Red);
                    TimeLine2.Name = "TimeLine2";

                    try {
                        if (((Shape)PanelOverView.Children[PanelOverView.Children.Count - 1]).Name == TimeLine2.Name) {
                            PanelOverView.Children.RemoveAt(PanelOverView.Children.Count - 1);
                        }
                    } catch (Exception) { }
                    PanelOverView.Children.Add(TimeLine2);
                }
            }
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            RefreshTimeLine();
        }
    


        private TextBlock CrateTextBlock(double x, double y,string text, string Font,Color iColor) {
            TextBlock txt = new TextBlock();
            txt.Text = text;
            txt.FontStyle = FontStyles.Normal;
            txt.FontFamily = new FontFamily(Font);
            txt.Foreground = new SolidColorBrush(iColor);
            var formattedText = new FormattedText(text,CultureInfo.CurrentUICulture,FlowDirection.LeftToRight,
                new Typeface(txt.FontFamily, txt.FontStyle, txt.FontWeight, txt.FontStretch),
                txt.FontSize,new SolidColorBrush(iColor));
            Canvas.SetTop(txt, y);
            Canvas.SetLeft(txt, x-(int)((double)formattedText.Width / (double)2));
            return txt;
        }
        private Rectangle CreateFrameDivision(double x, double y, double Width, double bottom,int Stroke,Color Cltr) {
            Rectangle rect = new Rectangle();
            rect.StrokeThickness = Stroke;
            rect.Stroke = new SolidColorBrush(Cltr);
            rect.Fill = null;
            rect.Width = Width;
            rect.Height = bottom - y;
            Canvas.SetTop(rect,y);
            Canvas.SetLeft(rect, x);
            return rect;
        }
        private Line CreateFrameLine(double x, double y, double Width, double height, int offset, int stroke, Color Cltr) {
            Line line = new Line();
            line.StrokeThickness = stroke;
            line.Stroke = new SolidColorBrush(Cltr);
            line.X1 = x;
            line.X2 = x+ Width;
            line.Y1 = y - offset;
            line.Y2 = y+ height;
            Canvas.SetTop(line,0);
            Canvas.SetLeft(line,0);
            return line;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            SetDateRange(StartDate, EndDate);
        }
        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e) {
            double value = Zoom;
            if (e.Delta > 0) {
                value = value * 1.1;
            } else if (e.Delta < 0) {
                value = value * 0.9;
            }
            eGetPositionPanelCalendar = e.GetPosition(PanelCalendar);
            Zoom = value;
        }
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (IsLoaded)
                Zoom = Zoom;
        }
        private void PanelOverView_MouseDown(object sender, MouseButtonEventArgs e) {
            GotoPositionPanelOverView(e.GetPosition(PanelOverView));
        }
        private void PanelOverView_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton== MouseButtonState.Pressed) {
                GotoPositionPanelOverView(e.GetPosition(PanelOverView));
            }
        }
        private void GotoPositionPanelOverView( Point MousePosition) {
            double viewPortXInPanel = MousePosition.X;
            double SizeInCal = (double)EndDate.Subtract(StartDate).Ticks * (double)TickPixel;
            double WidthViewPortOnPanelOverView = (double)((double)viewPortWith * (SizeInCal / PanelOverView.ActualWidth));
            double newviewPortX = (double)viewPortXInPanel * (SizeInCal / PanelOverView.ActualWidth) - viewPortWith / 2;
            if (newviewPortX < 0)newviewPortX = 0;
            if (newviewPortX+ viewPortWith > SizeInCal) newviewPortX = SizeInCal- viewPortWith;
            viewPortX = newviewPortX;
            oRefresh();
        }
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                SetDateRange(StartDate.Date.AddDays(1), EndDate.Date.AddDays(1).AddHours(-24));
            }
        }
        private void TextBlock_MouseDown_1(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                SetDateRange(StartDate.Date.AddDays(-1), EndDate.Date.AddDays(-1).AddHours(-24));
            }
        }
        private void DockPanel_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {

        }
        private void textBlock_MouseEnter(object sender, MouseEventArgs e) {

        }
        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e) {
            if (AuxCalendar.SelectedDates.Count == 0) return;
            List<DateTime> Dates = AuxCalendar.SelectedDates.ToList<DateTime>();
            Dates.Sort();
            if (Dates.Count > 7) {
                AuxCalendar.SelectedDates.Clear();
                AuxCalendar.SelectedDates.AddRange(Dates.First(), Dates[6]);
            } else {
                SetDateRange(Dates.First(), Dates.Last());
            }

        }

        private void Item_MouseEnter(object sender, MouseEventArgs e) {
            ((Rectangle)sender).StrokeThickness = 4;
        }

        private void Item_MouseLeave(object sender, MouseEventArgs e) {
            if (GetItemByID(((Rectangle)sender).Name) == null) return;
            if (!GetItemByID(((Rectangle)sender).Name).IsSelected) ((Rectangle)sender).StrokeThickness = 2;
        }
        bool StopMouseLeftButtonUp = false;
        bool MouseIsMoving = false;
        private void Item_MouseLeftButtonDown(object sender, MouseEventArgs e) {
            MouseIsMoving = false;
            ProductOrder SelectedItem = GetItemByID(((Rectangle)sender).Name);
            if (SelectedItem == null) return;
            if (!SelectedItem.IsSelected && SelectedItem.State==1) {
                oSel.Add(SelectedItem);
                StopMouseLeftButtonUp = true;
            }
        }
        private void Item_MouseLeftButtonUp(object sender, MouseEventArgs e) {
            if (StopMouseLeftButtonUp) {
                StopMouseLeftButtonUp = false;
                return;
            }
            ProductOrder SelectedItem = GetItemByID(((Rectangle)sender).Name);
            if (SelectedItem == null) return;
            if (SelectedItem.IsSelected) {
                oSel.Remove(SelectedItem);
            }
        }
        private void Item_MouseRightButtonUp(object sender, MouseEventArgs e) {
            ProductOrder SelectedItem = GetItemByID(((Rectangle)sender).Name);
            if (SelectedItem == null) return;
            if (!SelectedItem.IsSelected && (SelectedItem.State == 1|| SelectedItem.State == 2)) {
                oSel.Add(SelectedItem);
            }
            if (oSel.Items.Count == 0) return;
            ItemContextMenu.IsOpen = true;
        }
        private void button_Click(object sender, RoutedEventArgs e) {
            if (Machines.Count == 0) return;
            int randomNumber = random.Next(0, 8);
            ProductOrder NewOrder = new ProductOrder(Machines[0].Name + "_" + Machines[0].Ordens.Count, "Tonto", 1, StartDate, StartDate.AddHours(randomNumber), Machines[0].Ordens.Count, Machines[0].Name);
            Machines[0].Ordens.Add(NewOrder);
            oRefresh();
        }

        public ProductOrder GetItemByID(string itemID) {
            return (from mac in Machines
                    from ord in mac.Ordens
                    where ord.ItemID == itemID
                    select ord).FirstOrDefault();
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e) {
            if (oSel.Items.Count==0) return;
            foreach (ProductOrder item in oSel.Items) {
                if (item.DrawnRect.IsMouseOver) return;
            }
            oSel.Clear();
        }
        protected override void OnMouseMove(MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                for (int i = 0; i < oSel.Items.Count; i++) {
                    if (oSel.Items[i].State != 1) {
                        oSel.Remove(oSel.Items[i]);
                        i = i - 1;
                    }
                }
                if (oSel.Items.Count == 0) return;
                StopMouseLeftButtonUp = true;
                oSel.Move(e.GetPosition(PanelItems));
                MouseIsMoving = true;
            }
            base.OnMouseMove(e);
        }
        void RemoveOTFromMachine(string IDMac,string IDPo) {
            for (int i = 0; i < Machines.Count; i++) {
                if (IDMac == Machines[i].Name) {
                    for (int j = 0; j < Machines[i].Ordens.Count; j++) {
                        if(Machines[i].Ordens[j].ItemID== IDPo) {
                            Machines[i].Ordens.Remove(Machines[i].Ordens[j]);
                            Machines[i].RecalcID();
                           return;
                        }
                    }
                }
            }
        }
        void SortPlanned(string IDMac) {
            bool HasChanged = false;
            for (int i = 0; i < Machines.Count; i++) {
                if (IDMac == Machines[i].Name) {
                    Machines[i].SortOrders();
                    for (int j = 0; j < Machines[i].Ordens.Count-1; j++) {
                        HasChanged = false;
                        for (int t = j+1; t < Machines[i].Ordens.Count; t++) {
                            if (Machines[i].Ordens[t].State == 1 || Machines[i].Ordens[j].State == 1) {
                                if (Machines[i].Ordens[j].DateStart <= Machines[i].Ordens[t].DateStart &&
                                    Machines[i].Ordens[j].DateEnd > Machines[i].Ordens[t].DateStart) {
                                    if (Machines[i].Ordens[t].State == 1) {
                                        Machines[i].Ordens[t].DateStart = Machines[i].Ordens[j].DateEnd.AddMinutes(1);
                                        Machines[i].Ordens[t].DateEnd = Machines[i].Ordens[t].DateStart.Add(Machines[i].Ordens[t].Duration);
                                    } else {
                                        ProductOrder Aux = Machines[i].Ordens[j];
                                        Machines[i].Ordens[j] = Machines[i].Ordens[t];
                                        Machines[i].Ordens[t] = Aux;
                                        Machines[i].Ordens[t].DateStart = Machines[i].Ordens[j].DateEnd.AddMinutes(1);
                                        Machines[i].Ordens[t].DateEnd = Machines[i].Ordens[t].DateStart.Add(Machines[i].Ordens[t].Duration);
                                    }
                                    HasChanged = true;
                                    //Machines[i].SortOrders();
                                }
                            }
                        }
                        if (HasChanged) {
                            oRefresh();
                            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                        }
                    }
                    Machines[i].RecalcID();
                    return;
                }
            }
        }
        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e) {
            if (oSel.Items.Count == 0) return;
            if (MouseIsMoving) {
                Point Position = e.GetPosition(PanelCalendar);
                Point point = e.GetPosition(PanelItems);
                foreach (Machine iMac in Machines) {
                    if (point.Y >= Canvas.GetTop(iMac.DrawnRect) && point.Y < Canvas.GetTop(iMac.DrawnRect) + iMac.DrawnRect.Height) {
                        Double PointX = viewPortX + (Double)(Position.X);
                        DateTime NewStart = StartDate.Add(new TimeSpan((long)(PointX / TickPixel)));
                        //if (NewStart >= DateTime.Now) {
                        for (int i = 0; i < oSel.Items.Count; i++) {
                            RemoveOTFromMachine(oSel.Items[i].Equipamento, oSel.Items[i].ItemID);
                        }
                        for (int i = oSel.Items.Count-1; i >= 0; i--) {
                            oSel.Items[i].DateStart = NewStart;
                            oSel.Items[i].DateEnd = oSel.Items[i].DateStart.Add(oSel.Items[i].Duration);
                            oSel.Items[i].Equipamento = iMac.Name;
                            oSel.Items[i].ItemID = iMac.Name + "_" + iMac.Ordens.Count;
                            oSel.Items[i].DrawnRect.Name = iMac.Name + "_" + iMac.Ordens.Count;
                            iMac.Ordens.Add(oSel.Items[i]);
                            NewStart = NewStart.AddMilliseconds(-1);
                            SortPlanned(iMac.Name);
                        }
                        SortPlanned(iMac.Name);
                        //oSel.Clear();
                        //}
                    }
                }
                oRefresh();
            }
            StopMouseLeftButtonUp = false;
            MouseIsMoving = false;
        }

        private void MenuDelete_Click(object sender, RoutedEventArgs e) {
            if (oSel.Items.Count == 0) return;
            foreach (ProductOrder item in oSel.Items) {
                if (item.State == 1) {
                    RemoveOTFromMachine(item.Equipamento, item.ItemID);
                }
            }
            oSel.Clear();
            oRefresh();
        }

        private void MenuBlock_Click(object sender, RoutedEventArgs e) {
            if (oSel.Items.Count == 0) return;
            for (int i = 0; i < oSel.Items.Count; i++) {
                if (oSel.Items[i].State == 1) {
                    oSel.Items[i].State = 2;
                }
            }
            oSel.Clear();
            oRefresh();
        }
        private void MenuUnBlock_Click(object sender, RoutedEventArgs e) {
            if (oSel.Items.Count == 0) return;
            for (int i = 0; i < oSel.Items.Count; i++) {
                if (oSel.Items[i].State == 2) {
                    oSel.Items[i].State = 1;
                }
            }
            oSel.Clear();
            oRefresh();
        }
    }
}
