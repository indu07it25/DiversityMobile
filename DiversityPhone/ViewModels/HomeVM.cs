﻿using System;
using System.Net;
using ReactiveUI;
using ReactiveUI.Xaml;
using DiversityPhone.Services;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using DiversityPhone.Service;
using System.Collections.Generic;
using DiversityService.Model;
using System.Collections.ObjectModel;


namespace DiversityPhone.ViewModels
{
    public class HomeVM : ReactiveObject
    {
        

        public ReactiveCommand Edit { get; private set; }
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Download { get; private set; }
        public ReactiveCommand Upload { get; private set; }

        public ReactiveCommand GetVocabulary { get; private set; }

        private IMessageBus _messenger;
        private IOfflineStorage _storage;

        private IDiversityService _repository;

        public HomeVM(IMessageBus messenger, IOfflineStorage storage, IDiversityService repo)
        {
            _messenger = messenger;
            _storage = storage;
            _repository = repo;


            (Edit = new ReactiveCommand())
                .Subscribe(_ => _messenger.SendMessage<Page>(Page.ListEventSeries));

            (Settings = new ReactiveCommand())
                .Subscribe(_ => _messenger.SendMessage<Page>(Page.Settings));

            (Download = new ReactiveCommand())
                .Subscribe(_ => _messenger.SendMessage<Page>(Page.ListEventSeries));

            (Upload = new ReactiveCommand())
                .Subscribe(_ => _messenger.SendMessage<Page>(Page.Upload));

            (GetVocabulary = new ReactiveCommand())
                .Subscribe(_ => getVoc());
        }

        private void getVoc()
        {
            var vocFunc = Observable.FromAsyncPattern<IList<Term>>(_repository.BeginGetStandardVocabulary, _repository.EndGetStandardVocabulary);

            vocFunc.Invoke().Subscribe(voc => _storage.addTerms(voc));
        }
    }
}
