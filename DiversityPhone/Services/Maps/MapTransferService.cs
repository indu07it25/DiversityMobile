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
using ReactiveUI;
using DiversityPhone.MapMediaService;
using System.IO.IsolatedStorage;
using System.IO;
using DiversityPhone.Model;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace DiversityPhone.Services
{
    public class MapTransferService : IMapTransferService
    {

        private PhoneMediaServiceClient _mapinfo = new PhoneMediaServiceClient();
        private HttpWebRequest _imageHttp;
        private HttpWebRequest _xmlHttp;


        public MapTransferService()            
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("Maps"))
                {
                    isoStore.CreateDirectory("Maps");
                }
            }
        }



        #region Load Available Maps

        public IObservable<IEnumerable<String>> GetAvailableMaps(String searchString)
        {
            var source = Observable.FromEvent<EventHandler<GetMapListCompletedEventArgs>, GetMapListCompletedEventArgs>((a) => (s, args) => a(args), d => _mapinfo.GetMapListCompleted += d, d => _mapinfo.GetMapListCompleted -= d)
                .Select(args => args.Result as IEnumerable<String>);
            var res = singleResultObservable(source);
            _mapinfo.GetMapListAsync(searchString);
            return res;
        }


        #endregion


        #region downLoadMap

        public IObservable<Map> downloadMap(String serverKey)
        {
            var mapUrl = Observable.FromEvent<EventHandler<GetMapUrlCompletedEventArgs>, GetMapUrlCompletedEventArgs>((a) => (s, args) => a(args), d => _mapinfo.GetMapUrlCompleted += d, d => _mapinfo.GetMapUrlCompleted -= d)
                .Select(args => args.Result as String);
            var xmlUrl = Observable.FromEvent<EventHandler<GetXmlUrlCompletedEventArgs>, GetXmlUrlCompletedEventArgs>((a) => (s, args) => a(args), d => _mapinfo.GetXmlUrlCompleted += d, d => _mapinfo.GetXmlUrlCompleted -= d)
               .Select(args => args.Result as String);
            _mapinfo.GetMapUrlAsync(serverKey);
            _mapinfo.GetXmlUrlAsync(serverKey);

           // _imageHttp = (HttpWebRequest)WebRequest.CreateHttp(mapUrl.Value);


            throw new NotImplementedException();
        }


        #endregion

        #region Helper


        private static IObservable<T> singleResultObservable<T>(IObservable<T> source)
        {
            var res = source
                .FirstAsync()
                .Replay(1);

            res.Connect();
            return res;
        }

        #endregion
    }
}