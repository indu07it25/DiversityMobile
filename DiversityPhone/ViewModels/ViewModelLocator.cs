﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Funq;

namespace DiversityPhone
{
    public class ViewModelLocator
    {
        Container _ioc = new Container();
        public ViewModelLocator()
        {
            _ioc.Register<IDiversityDatabase>(App.OfflineDB);
        }
    }
}
