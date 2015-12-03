using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Parent.oRefresh();
        }
        public void Remove(ProductOrder item) {
            item.IsSelected = false;
            Items.Remove(item);
            if (Items.Count == 0) HasSelection = false;
            else HasSelection = true;
            Parent.oRefresh();
        }
        public void RemoveAt(int index) {
            Items[index].IsSelected = false;
            Items.RemoveAt(index);
            if (Items.Count == 0) HasSelection = false;
            else HasSelection = true;
            Parent.oRefresh();
        }
        public void Clear() {
            foreach (ProductOrder item in Items) {
                item.IsSelected = false;
            }
            Items.Clear();
            HasSelection = false;
            IsMoving = false;
            Parent.oRefresh();
        }
    }
}
