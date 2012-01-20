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
using DiversityPhone.Model;

namespace DiversityPhone.View
{
    public partial class EditIU : PhoneApplicationPage
    {
        private EditIUVM VM { get { return DataContext as EditIUVM; } }

        private EditPageAppBarUpdater<IdentificationUnit> _appb;

        public EditIU()
        {
            InitializeComponent();

            _appb = new EditPageAppBarUpdater<IdentificationUnit>(this.ApplicationBar, VM);
        }        
    }
}