﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using DiversityPhone.ViewModels;

namespace DiversityPhone
{
    public partial class EventSeriesPage : PhoneApplicationPage
    {
        private ListESVM VM { get { return this.DataContext as ListESVM; } }
        public EventSeriesPage()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, EventArgs e)
        {
            if (VM != null)
                VM.AddSeries.Execute(null);
        }
    }
}