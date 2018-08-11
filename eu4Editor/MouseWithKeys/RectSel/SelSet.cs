using GeoPPT.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace GeoPPT.Draw
{
    public interface ISelected
    {
        bool IsSelected { get;set; }
    }

    public class SelectSet
    {
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        private HashSet<ISelected> items;

        public SelectSet()
        {
            items = new HashSet<ISelected>();
        }

        public void Add(ISelected visual)
        {
            if (null == visual)
                return;

            //没按control键
            if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                if (items.Count == 1 && items.FirstOrDefault() == visual)
                    return;
                else
                {
                    if (items.Contains(visual) && visual.IsSelected == true)
                    {
                        items.Remove(visual);
                        Clear();
                        items.Add(visual);
                    }
                    else
                    {
                        Clear();
                        items.Add(visual);
                        visual.IsSelected = true;
                        OnCollectionChanged(NotifyCollectionChangedAction.Add, visual);
                    }

                }
            }
            else
            {
                if (items.Contains(visual))
                {
                    visual.IsSelected = false;
                    items.Remove(visual);

                    OnCollectionChanged(NotifyCollectionChangedAction.Remove, visual);

                }
                else
                {
                    visual.IsSelected = true;
                    items.Add(visual);

                    OnCollectionChanged(NotifyCollectionChangedAction.Add, visual);
                }
            }
        }

        public void Clear()
        {
            foreach (ISelected item in items)
            {
                item.IsSelected = false;
                //OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, 0);
            }
            var l = items.ToList();
            items.Clear();

            OnCollectionReset(l);
        }


        //public void ClearExcept(MyDrawingVisual visual)
        //{
        //    foreach (MyDrawingVisual item in this)
        //    {
        //        if(item!=visual)
        //            item.IsSelected = false;
        //    }

        //    base.Clear();

        //    base.Add(visual);
        //}

        public void ForEach(Action<ISelected> action)
        {
            if (action != null)
            {
                foreach (ISelected item in items)
                {
                    action(item);
                }
            }
        }


        public void AddRange(IEnumerable<ISelected> query)
        {
            if (query == null)
                return;

            foreach (var item in query)
            {
                if (item != null)
                {
                    item.IsSelected = true;

                    items.Add(item);

                }
            }

            OnCollectionChanged(NotifyCollectionChangedAction.Add, query.ToList());
        }

        public IEnumerable<ISelected> Items
        {
            get
            {
                foreach (var item in items)
                {
                    yield return item;
                }
            }
        }

        public IEnumerator<ISelected> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }


        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged == null)
                return;
            //lock
            this.CollectionChanged((object)this, e);
        }
        private void OnCollectionChanged(NotifyCollectionChangedAction action, List<ISelected> items)
        {
            var e = new NotifyCollectionChangedEventArgs(action, items);
            this.OnCollectionChanged(e);
        }
        private void OnCollectionChanged(NotifyCollectionChangedAction action, ISelected item)
        {
            var e = new NotifyCollectionChangedEventArgs(action, item);
            this.OnCollectionChanged(e);
        }

        private void OnCollectionReset(List<ISelected> items)
        {
            if (items.Count != 0)
            {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items);
                this.OnCollectionChanged(e);
            }
        }



    }
}
