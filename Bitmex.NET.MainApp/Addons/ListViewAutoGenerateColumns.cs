using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;
using System.Collections;
using System.Windows.Data;

namespace Bitmex.NET.MainApp
{
    public class DynamicBindingListView
    {

        public static bool GetGenerateColumnsGridView(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (bool)element.GetValue(GenerateColumnsGridViewProperty);
        }

        public static void SetGenerateColumnsGridView(DependencyObject element, bool value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(GenerateColumnsGridViewProperty, value);
        }


        public static readonly DependencyProperty GenerateColumnsGridViewProperty = DependencyProperty.RegisterAttached("GenerateColumnsGridView", typeof(bool?), typeof(DynamicBindingListView), new FrameworkPropertyMetadata(null, thePropChanged));


        public static string GetDateFormatString(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (string)element.GetValue(DateFormatStringProperty);
        }

        public static void SetDateFormatString(DependencyObject element, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(DateFormatStringProperty, value);
        }


        public static readonly DependencyProperty DateFormatStringProperty = DependencyProperty.RegisterAttached("DateFormatString", typeof(string), typeof(DynamicBindingListView), new FrameworkPropertyMetadata(null));
        public static void thePropChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ListView lv = (ListView)obj;
            DependencyPropertyDescriptor descriptor = DependencyPropertyDescriptor.FromProperty(ListView.ItemsSourceProperty, typeof(ListView));
            descriptor.AddValueChanged(lv, new EventHandler(ItemsSourceChanged));
        }

        private static void ItemsSourceChanged(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;
            GridView gv = (GridView)lv.View;
            if (gv.Columns != null && gv.Columns.Count == 0)
            {
                IEnumerable its = lv.ItemsSource;
                IEnumerator itsEnumerator = its.GetEnumerator();
                bool hasItems = itsEnumerator.MoveNext();
                if (hasItems)
                {
                    SetUpTheColumns(lv, itsEnumerator.Current);
                }
            }
        }

        private static void SetUpTheColumns(ListView theListView, object firstObject)
        {
            PropertyInfo[] theClassProperties = firstObject.GetType().GetProperties();
            GridView gv = (GridView)theListView.View;
            gv.Columns.Clear();
            foreach (PropertyInfo pi in theClassProperties)
            {
                string columnName = pi.Name;
                GridViewColumn grv = new GridViewColumn { Header = columnName };

                if (object.ReferenceEquals(pi.PropertyType, typeof(DateTime)))
                {
                    Binding bnd = new Binding(columnName);
                    string formatString = (string)theListView.GetValue(DateFormatStringProperty);
                    if (formatString != string.Empty)
                    {
                        bnd.StringFormat = formatString;
                    }
                    BindingOperations.SetBinding(grv, TextBlock.TextProperty, bnd);
                    grv.DisplayMemberBinding = bnd;
                }
                else
                {
                    Binding bnd = new Binding(columnName);
                    BindingOperations.SetBinding(grv, TextBlock.TextProperty, bnd);
                    grv.DisplayMemberBinding = bnd;
                }
                gv.Columns.Add(grv);
            }
        }

    }
}