﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public class EditIUVM : ReactiveObject
    {
        IMessageBus _messenger;
        IList<IDisposable> _subscriptions;
        IOfflineStorage _storage;

        #region Properties

        public ReactiveCommand Save { get; private set; }
        public ReactiveCommand Cancel { get; private set; }
        
        public IdentificationUnit Model { get { return _Model.Value; } }
        private ObservableAsPropertyHelper<IdentificationUnit> _Model;        
        

        private string _AccessionNumber;

        public string AccessionNumber
        {
            get
            {
                return _AccessionNumber;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.AccessionNumber,ref _AccessionNumber, value);
            }
        }        


        private IList<Term> _TaxonomicGroups = null;
        public IList<Term> TaxonomicGroups
        {
            get
            {
                return _TaxonomicGroups ?? (_TaxonomicGroups = _storage.getTerms(0));
            }            
        }

        
        public Term _SelectedTaxGroup = null; 

        public Term SelectedTaxGroup
        {
            get
            {
                return _SelectedTaxGroup;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SelectedTaxGroup,ref _SelectedTaxGroup, value);
            }
        }


        private string _Description;

        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.Description, ref _Description, value);
            }
        }
        

        public bool IsToplevel { get { return _IsToplevel.Value; } }
        private ObservableAsPropertyHelper<bool> _IsToplevel;     
        

        #endregion



        public EditIUVM(IMessageBus messenger, IOfflineStorage storage)
        {
            _messenger = messenger;
            _storage = storage;

            var model = _messenger.Listen<IdentificationUnit>(MessageContracts.EDIT);
            
                                                   
            _Model = model.ToProperty(this, x => x.Model);
            

            var isToplevel = model
                .Select(m => m.RelatedUnitID == null);
            _IsToplevel = isToplevel.ToProperty(this, x => x.IsToplevel);

            var canSave = this.ObservableForProperty(x => x.SelectedTaxGroup)
                                .Select(change => change.Value != null).StartWith(false);

            _subscriptions = new List<IDisposable>()
            {            
                (Cancel = new ReactiveCommand())
                    .Subscribe(_ => _messenger.SendMessage<Message>(Message.NavigateBack)),

                model.Select(m => m.TaxonomicGroup)
                    .Select(tg => TaxonomicGroups.FirstOrDefault(t => t.Code == tg) ?? ((TaxonomicGroups.Count > 0) ? TaxonomicGroups[0] : null))
                    .Subscribe(x => SelectedTaxGroup = x),

                model.Select(m => m.AccessionNumber)
                    .Subscribe(accessNo => AccessionNumber = accessNo),

                model.Select(m => m.UnitDescription)
                    .Subscribe(desc => Description = desc),

                (Save = new ReactiveCommand(canSave))
                    .Subscribe(_ =>
                        {
                            updateModel();
                            _messenger.SendMessage<IdentificationUnit>(Model, MessageContracts.SAVE);
                            _storage.addIUnit(Model);
                            _messenger.SendMessage<Message>(Message.NavigateBack);
                        }),

                (Cancel = new ReactiveCommand())
                    .Subscribe(_=>_messenger.SendMessage<Message>(Message.NavigateBack)),
            };
        }        

        private void updateModel()
        {
            Model.AccessionNumber = AccessionNumber;
            Model.UnitDescription = Description;
            Model.TaxonomicGroup = SelectedTaxGroup.Code;            
        }


    }
}
