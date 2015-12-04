using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SchedulePlannerWPF {
    public class Selection {
        public Calendar Parent;
        public Selection(Calendar iParent) {
            Parent = iParent;
        }
        public List<ProductOrder> Items = new List<ProductOrder>();
        public bool HasSelection = false;
        public bool IsMoving = false;
        public void Add(ProductOrder item) {
            item.IsSelected = true;
            Items.Add(item);
            HasSelection = true;
        }
        public void Remove(ProductOrder item) {
            item.IsSelected = false;
            Items.Remove(item);
            if (Items.Count == 0) HasSelection = false;
            else HasSelection = true;
        }
        public void RemoveAt(int index) {
            Items[index].IsSelected = false;
            Items.RemoveAt(index);
            if (Items.Count == 0) HasSelection = false;
            else HasSelection = true;
        }
        public void Clear() {
            foreach (ProductOrder item in Items) {
                item.IsSelected = false;
            }
            Items.Clear();
            HasSelection = false;
            IsMoving = false;
        }

        internal void Move(Point point) {
            double StackPos = 0;
            foreach (ProductOrder item in Items) {
                double MacPostion = point.Y;
                foreach (Machine iMac in Parent.Machines) {
                    if (point.Y > Canvas.GetTop( iMac.DrawnRect) &&  point.Y < Canvas.GetTop(iMac.DrawnRect)+iMac.DrawnRect.Height) {
                        MacPostion = Canvas.GetTop(iMac.DrawnRect) + Parent.ItemSlack;
                        break;
                    }
                }
                Canvas.SetTop(item.DrawnRect, MacPostion);
                Canvas.SetLeft(item.DrawnRect, point.X + StackPos);
                StackPos += item.DrawnRect.Width;
            }
        }
    }
}
