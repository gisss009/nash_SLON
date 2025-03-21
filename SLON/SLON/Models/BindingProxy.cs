using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace SLON.Models
{
    public class BindingProxy : BindableObject
    {
        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly BindableProperty DataProperty =
            BindableProperty.Create(nameof(Data), typeof(object), typeof(BindingProxy), null);
    }
}

