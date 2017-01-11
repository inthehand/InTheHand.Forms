// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BindablePicker.cs" company="In The Hand Ltd">
//   Copyright (c) 2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Xamarin.Forms;
using System.Collections;
//
// <ith:BindablePicker ItemsSource="{Binding Path=myCollection}" SelectedItem="{Binding Path=myItem}" />
//
namespace InTheHand.Forms
{
    /// <summary>
    /// Adds data-binding support to Picker.
    /// </summary>
    public class BindablePicker : Picker
    {
        public BindablePicker()
        {
            this.SelectedIndexChanged += OnSelectedIndexChanged;
        }

        public static BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(BindablePicker), default(IEnumerable), BindingMode.OneWay,null, OnItemsSourceChanged);

        public static BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(BindablePicker), null, BindingMode.TwoWay, null, OnSelectedItemChanged);

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        
        private static void OnItemsSourceChanged(object bindable, object oldvalue, object newvalue)
        {
            var picker = bindable as BindablePicker;
            picker.Items.Clear();
            if(newvalue != null)
            {
                foreach(var item in (IEnumerable)newvalue)
                {
                    picker.Items.Add(item.ToString());
                }
            }
        }

        private void OnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            if(SelectedIndex < 0 || SelectedIndex > Items.Count - 1)
            {
                SelectedItem = null;
            }
            else
            {
                if (ItemsSource is IList)
                {
                    SelectedItem = ((IList)ItemsSource)[SelectedIndex];
                }
                else
                {
                    int i = 0;
                    foreach(object item in ItemsSource)
                    {
                        if(i == SelectedIndex)
                        {
                            SelectedItem = item;
                            break;
                        }

                        i++;
                    }
                }
            }
        }

        private static void OnSelectedItemChanged(object bindable, object oldvalue, object newvalue)
        {
            var picker = bindable as BindablePicker;
            if(newvalue != null)
            {
                picker.SelectedIndex = picker.Items.IndexOf(newvalue.ToString());
            }
        }
    }
}
