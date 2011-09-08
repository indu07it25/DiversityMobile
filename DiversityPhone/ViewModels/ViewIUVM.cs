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
using System.Reactive.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using DiversityPhone.Services;

namespace DiversityPhone.ViewModels
{

    public class ViewIUVM : ReactiveObject
    {
        IMessageBus _messenger;
        IOfflineStorage _storage;
        IList<IDisposable> _subscriptions;


        public IdentificationUnitVM Current { get { return _Current.Value; } }
        private ObservableAsPropertyHelper<IdentificationUnitVM> _Current;
        


        private IdentificationUnit Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<IdentificationUnit> _Model;
        



        public ViewIUVM(IMessageBus messenger,IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            _Model = _messenger.Listen<IdentificationUnit>(MessageContracts.SELECT)
                .ToProperty(this, x => x.Model);

            _Current = _Model.Select(m => new IdentificationUnitVM(
                _messenger,
                m,
                IdentificationUnitVM.getSingleLevelVMFromModelList(_storage.getSubUnits(m),_messenger)
                )).ToProperty(this, x => x.Current);
                    

            _subscriptions = new List<IDisposable>()
            {
                
            };
        }

        private void selectIU(IdentificationUnit iu)
        {

        }
    }
        
}
