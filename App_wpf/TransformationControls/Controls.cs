﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace App_wpf.TransformationControls
{
    public class TransformationControls
    {
        public UIElement Controls { get; protected set; }
        public CheckBox Backward { get; protected set; }
        public Button CloseBtn { get; protected set; }
    }
}
