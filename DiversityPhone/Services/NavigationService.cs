﻿using System;
using System.Net;
using Microsoft.Phone.Controls;


namespace DiversityPhone.Services
{
    public class NavigationService : INavigationService
    {             

        public NavigationService()
        {
           
        }

        public void Navigate(Page p)
        {
            Uri destination = null;
            switch (p)
            {
                case Page.Home:
                    destination = new Uri("/Home.xaml",UriKind.Relative);
                    break;
                case Page.Settings:
                    destination = new Uri("/Settings.xaml", UriKind.Relative);
                    break;
                case Page.Setup:
                    destination = new Uri("/FirstTimeSetup.xaml", UriKind.Relative);
                    break;
                case Page.Upload:
                    destination = new Uri("/Upload.xaml", UriKind.Relative);
                    break;
                case Page.ListEventSeries:
                    destination = new Uri("/ListES.xaml", UriKind.Relative);
                    break;                
                case Page.EditEventSeries:
                    destination = new Uri("/EditES.xaml", UriKind.Relative);
                    break;
                case Page.ListEvents:
                    destination = new Uri("/ListEV.xaml", UriKind.Relative);
                    break;
                case Page.EditEvent:
                    destination = new Uri("/EditEV.xaml", UriKind.Relative);
                    break;
#if DEBUG
                default:
                    throw new NotImplementedException();
#endif
            }
            if (destination != null && App.RootFrame != null)
                App.RootFrame.Navigate(destination);
        }

        public bool CanNavigateBack()
        {
            return App.RootFrame.CanGoBack;
        }

        public void NavigateBack()
        {
            App.RootFrame.GoBack();
        }


        public void ClearHistory()
        {
            while (App.RootFrame.CanGoBack)
                App.RootFrame.RemoveBackEntry();
        }
    }
}
