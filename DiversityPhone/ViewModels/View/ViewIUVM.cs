﻿using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels {
    public class ViewIUVM : ViewPageVMBase<IdentificationUnit> {
        private ReactiveAsyncCommand getAnalyses = new ReactiveAsyncCommand();

        public enum Pivots {
            Subunits,
            Descriptions,
            Multimedia
        }

        private readonly IVocabularyService Vocabulary;
        private readonly IFieldDataService Storage;

        #region Commands
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand Maps { get; private set; }
        public ReactiveCommand Back { get; private set; }

        public ReactiveCommand<IElementVM<IdentificationUnit>> EditCurrent { get; private set; }
        public ReactiveCommand<IElementVM<IdentificationUnit>> SelectUnit { get; private set; }
        public ReactiveCommand<IElementVM<IdentificationUnitAnalysis>> EditAnalysis { get; private set; }
        #endregion

        #region Properties

        Stack<IElementVM<IdentificationUnit>> unitBackStack = new Stack<IElementVM<IdentificationUnit>>();

        private Pivots _SelectedPivot;
        public Pivots SelectedPivot {
            get {
                return _SelectedPivot;
            }
            set {
                this.RaiseAndSetIfChanged(x => x.SelectedPivot, ref _SelectedPivot, value);
            }
        }

        private SerialDisposable _SubunitListener = new SerialDisposable();
        private ObservableAsPropertyHelper<ReactiveCollection<IdentificationUnitVM>> _Subunits;
        public ReactiveCollection<IdentificationUnitVM> Subunits { get { return _Subunits.Value; } }

        public ReactiveCollection<IdentificationUnitAnalysisVM> Analyses { get; private set; }

        public ElementMultimediaVM MultimediaList { get; private set; }

        #endregion

        public ViewIUVM(
            IVocabularyService Vocabulary,
            IFieldDataService Storage,
            ElementMultimediaVM MultimediaList,
            [Dispatcher] IScheduler Dispatcher
            ) {
            this.Vocabulary = Vocabulary;
            this.Storage = Storage;

            EditCurrent = new ReactiveCommand<IElementVM<IdentificationUnit>>();
            EditCurrent
                .ToMessage(Messenger, MessageContracts.EDIT);

            SelectUnit = new ReactiveCommand<IElementVM<IdentificationUnit>>();
            SelectUnit
                .Do(vm => unitBackStack.Push(Current))
                .ToMessage(Messenger, MessageContracts.VIEW);

            EditAnalysis = new ReactiveCommand<IElementVM<IdentificationUnitAnalysis>>();
            EditAnalysis
                .ToMessage(Messenger, MessageContracts.EDIT);


            _Subunits = this.ObservableToProperty(
                CurrentObservable
                .Select(vm => (vm as IdentificationUnitVM).SubUnits)
                .Do(units => _SubunitListener.Disposable = units.ListenToChanges<IdentificationUnit, IdentificationUnitVM>(iu => iu.RelatedUnitID == Current.Model.UnitID)),
                x => x.Subunits);

            //Multimedia
            this.MultimediaList = MultimediaList;
            CurrentModelObservable
                .Select(m => m as IMultimediaOwner)
                .Subscribe(MultimediaList);

            Analyses = getAnalyses.RegisterAsyncFunction(iu => Storage.getIUANForIU(iu as IdentificationUnit).Select(iuan => new IdentificationUnitAnalysisVM(iuan, Vocabulary)))
                .SelectMany(vms => vms)
                .CreateCollection();

            Analyses.ListenToChanges<IdentificationUnitAnalysis, IdentificationUnitAnalysisVM>(iuan => iuan.UnitID == Current.Model.UnitID);

            CurrentModelObservable
                .Do(_ => SelectedPivot = Pivots.Subunits)
                .Do(_ => Analyses.Clear())
                .Subscribe(getAnalyses.Execute);

            var has_analyses_observable =
            CurrentModelObservable
                .Select(current =>
                    Observable.Return(Enumerable.Empty<Analysis>().ToList() as IList<Analysis>) // first clear last analyses
                    .Concat(
                        // Then Load possible Analyses in the background
                    Observable.Start(() => Vocabulary.getPossibleAnalyses(current.TaxonomicGroup) as IList<Analysis>, ThreadPoolScheduler.Instance)
                    ))
                .Switch()
                .Select(list => {
                    var hasAnalyses = list.Any();
                    Messenger.SendMessage<IList<Analysis>>(list); //Broadcast Analyses to editVM
                    return hasAnalyses;
                })
                .ObserveOn(Dispatcher);
            var can_add_observable = this.ObservableForProperty(x => x.SelectedPivot).Value().Select(p => p != Pivots.Descriptions)
                .BooleanOr(has_analyses_observable);

            Add = new ReactiveCommand(can_add_observable);
            Add.Where(_ => SelectedPivot == Pivots.Subunits)
                .Select(_ => new IdentificationUnitVM(new IdentificationUnit() { RelatedUnitID = Current.Model.UnitID, SpecimenID = Current.Model.SpecimenID }) as IElementVM<IdentificationUnit>)
                .ToMessage(Messenger, MessageContracts.EDIT);
            Add.Where(_ => SelectedPivot == Pivots.Multimedia)
                .Subscribe(MultimediaList.AddMultimedia.Execute);
            Add.Where(_ => SelectedPivot == Pivots.Descriptions)
                .Select(_ => new IdentificationUnitAnalysisVM(new IdentificationUnitAnalysis() { UnitID = Current.Model.UnitID }, Vocabulary) as IElementVM<IdentificationUnitAnalysis>)
                .ToMessage(Messenger, MessageContracts.EDIT);

            Maps = new ReactiveCommand();
            Maps
                .Select(_ => Current.Model as ILocalizable)
                .ToMessage(Messenger, MessageContracts.VIEW);


            Back = new ReactiveCommand();
            Back
                .Subscribe(_ => goBack());

        }

        private void goBack() {
            if (unitBackStack.Any()) {
                SelectedPivot = Pivots.Subunits;
                Messenger.SendMessage(unitBackStack.Pop(), MessageContracts.VIEW);
            }
            else
                Messenger.SendMessage(Page.Previous);
        }
    }
}
