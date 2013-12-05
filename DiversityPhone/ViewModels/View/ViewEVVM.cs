﻿namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    public class ViewEVVM : ViewPageVMBase<Event>
    {
        public enum Pivots
        {
            Specimen,
            Descriptions,
            Multimedia
        }

        #region Commands
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand Maps { get; private set; }

        public ReactiveCommand<IElementVM<Event>> EditEvent { get; private set; }
        public ReactiveCommand<IElementVM<EventProperty>> SelectProperty { get; private set; }
        public ReactiveCommand<IElementVM<Specimen>> SelectSpecimen { get; private set; }
        #endregion

        #region Properties
        private Pivots _SelectedPivot = Pivots.Specimen;
        public Pivots SelectedPivot
        {
            get
            {
                return _SelectedPivot;
            }
            set
            {
                this.RaiseAndSetIfChanged(vm => vm.SelectedPivot, ref _SelectedPivot, value);
            }
        }

        public ReactiveCollection<IElementVM<Specimen>> SpecList { get; private set; }

        public ReactiveCollection<IElementVM<EventProperty>> PropertyList { get; private set; }

        public ElementMultimediaVM MultimediaList { get; private set; }

        #endregion

        private ReactiveAsyncCommand getSpecimen = new ReactiveAsyncCommand();
        private ReactiveAsyncCommand getProperties = new ReactiveAsyncCommand();

        public ViewEVVM(
            DataVMServices Services,
            ElementMultimediaVM MultimediaList
            )
            : base(Services)
        {

            //Current
            EditEvent = new ReactiveCommand<IElementVM<Event>>();
            EditEvent
                .ToMessage(Services.Messenger, MessageContracts.EDIT);

            //Specimen
            SpecList = getSpecimen.RegisterAsyncFunction(ev => Services.Storage.Get((ev as Event).Specimen()).Select(Services.VMFactory.CreateVM))
                .SelectMany(specs => specs)
                .CreateCollection();
            SpecList.ListenToChanges<Specimen>(spec => spec.EventID == Current.Model.EventID);

            CurrentModelObservable
                .Do(_ => SpecList.Clear())
                .Subscribe(getSpecimen.Execute);

            SelectSpecimen = new ReactiveCommand<IElementVM<Specimen>>();
            SelectSpecimen
                .ToMessage(Services.Messenger, MessageContracts.VIEW);

            //Properties
            PropertyList = getProperties.RegisterAsyncFunction(ev => Services.Storage.Get((ev as Event).Properties()).Select(Services.VMFactory.CreateVM))
                .SelectMany(props => props)
                .CreateCollection();
            PropertyList.ListenToChanges<EventProperty>(p => p.EventID == Current.Model.EventID);

            CurrentModelObservable
                .Do(_ => PropertyList.Clear())
                .Subscribe(getProperties.Execute);

            SelectProperty = new ReactiveCommand<IElementVM<EventProperty>>();
            SelectProperty
                .ToMessage(Services.Messenger, MessageContracts.EDIT);

            //Multimedia
            this.MultimediaList = MultimediaList;

            CurrentModelObservable
                .Select(m => m as IMultimediaOwner)
                .Subscribe(MultimediaList);

            // Receive Latest Property List broadcast by EditPropertyVM
            var properties = Services.Messenger.Listen<IList<Property>>();
            var allPropertiesSet = properties.Select(list => list.Count)
                .CombineLatest(PropertyList.CollectionCountChanged, (available, set) => available <= set);

            var canAdd =
                this.WhenAny(x => x.SelectedPivot, x => x.GetValue())
                    .CombineLatest(allPropertiesSet, (piv, allSet) => piv != Pivots.Descriptions || !allSet);

            //Add New
            Add = new ReactiveCommand(canAdd);
            Add.Where(_ => SelectedPivot == Pivots.Specimen)
                .Select(_ => new SpecimenVM(new Specimen() { EventID = Current.Model.EventID.Value }) as IElementVM<Specimen>)
                .ToMessage(Services.Messenger, MessageContracts.EDIT);
            Add.Where(_ => SelectedPivot == Pivots.Descriptions)
                .Select(_ => new PropertyVM(new EventProperty() { EventID = Current.Model.EventID }) as IElementVM<EventProperty>)
                .ToMessage(Services.Messenger, MessageContracts.EDIT);
            Add.Where(_ => SelectedPivot == Pivots.Multimedia)
                .Subscribe(MultimediaList.AddMultimedia.Execute);

            //Maps
            Maps = new ReactiveCommand();
            Maps
                .Select(_ => Current.Model as ILocalizable)
                .ToMessage(Services.Messenger, MessageContracts.VIEW);
        }
    }
}
