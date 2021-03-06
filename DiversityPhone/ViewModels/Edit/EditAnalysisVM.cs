﻿using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;



namespace DiversityPhone.ViewModels {
    public class EditAnalysisVM : EditPageVMBase<IdentificationUnitAnalysis> {
        readonly IVocabularyService Vocabulary;
        readonly IFieldDataService Storage;

        private ObservableAsPropertyHelper<IElementVM<IdentificationUnit>> _Parent;
        public IElementVM<IdentificationUnit> Parent { get { return _Parent.Value; } }

        private readonly Analysis NoAnalysis = new Analysis() { DisplayText = DiversityResources.Analysis_NoAnalysis };
        public ListSelectionHelper<Analysis> Analyses { get; private set; }


        private readonly AnalysisResult NoResult = new AnalysisResult() { DisplayText = DiversityResources.Analysis_Result_NoResult };
        private ListSelectionHelper<AnalysisResult> _Results;
        public ListSelectionHelper<AnalysisResult> Results { get { return _Results; } }

        private ObservableAsPropertyHelper<bool> _IsCustomResult;
        public bool IsCustomResult { get { return _IsCustomResult.Value; } }

        private string _CustomResult;
        public string CustomResult {
            get {
                return _CustomResult;
            }
            set {
                this.RaiseAndSetIfChanged(x => x.CustomResult, ref _CustomResult, value);
            }
        }

        public DateTime _AnalysisDate;
        public DateTime AnalysisDate {
            get { return _AnalysisDate; }
            set {
                this.RaiseAndSetIfChanged(x => x.AnalysisDate, value);
            }
        }

        ReactiveAsyncCommand getPossibleResults = new ReactiveAsyncCommand();


        public EditAnalysisVM(
            IFieldDataService Storage,
            IVocabularyService Vocabulary,
            [Dispatcher] IScheduler Dispatcher
            ) {
            Contract.Requires(Storage != null);
            Contract.Requires(Vocabulary != null);
            this.Storage = Storage;
            this.Vocabulary = Vocabulary;


            _Parent = this.ObservableToProperty(
                Messenger.Listen<IElementVM<IdentificationUnit>>(MessageContracts.VIEW),
                vm => vm.Parent);


            _Results = new ListSelectionHelper<AnalysisResult>(Dispatcher);
            Analyses = new ListSelectionHelper<Analysis>(Dispatcher);
            Messenger.Listen<IList<Analysis>>()
                .Subscribe(Analyses.ItemsObserver);

            Analyses.ItemsObservable.Where(items => items != null)
                .CombineLatest(ModelByVisitObservable, (analyses, iuan) =>
                            analyses
                            .Where(an => an.AnalysisID == iuan.AnalysisID)
                            .FirstOrDefault())
                .Where(x => x != null)
                .Subscribe(x => Analyses.SelectedItem = x);

            Analyses.SelectedItemObservable
                .Where(an => an != null)
                .SelectMany(selectedAN => {
                    if (selectedAN != NoAnalysis)
                        return Observable.Start(() => Vocabulary.getPossibleAnalysisResults(selectedAN.AnalysisID) as IList<AnalysisResult>, ThreadPoolScheduler.Instance);
                    else
                        return Observable.Return(Enumerable.Empty<AnalysisResult>().ToList() as IList<AnalysisResult>);
                })
                .Do(list => list.Insert(0, NoResult))
                .ObserveOn(Dispatcher)
                .Subscribe(Results.ItemsObserver);


            Results.ItemsObservable
                .Where(results => results != null)
                .CombineLatest(ModelByVisitObservable, (results, iuan) =>
                    results
                    .Where(res => res.Result == iuan.AnalysisResult)
                    .FirstOrDefault())
                .Where(x => x != null)
                .Subscribe(x => Results.SelectedItem = x);



            _IsCustomResult = this.ObservableToProperty(
                Results.ItemsObservable
                .Where(res => res != null)
                .Select(results => !results.Any(res => res != NoResult))
                //Don't allow Custom Results until we checked the DB
                .Merge(Analyses.SelectedItemObservable.Select(_ => false)),
                vm => vm.IsCustomResult);

            ModelByVisitObservable
                .Select(iuan => iuan.AnalysisResult)
                .Merge(
                    this.WhenAny(x => x.IsCustomResult, x => x.Value)
                    .Where(custom => !custom)
                    .Select(_ => string.Empty)
                )
                .Subscribe(x => CustomResult = x);

            Messenger.RegisterMessageSource(
              Save
              .Select(_ => Analyses.SelectedItem),
              MessageContracts.USE);

            CanSave().StartWith(false).Subscribe(CanSaveSubject);
        }

        protected IObservable<bool> CanSave() {
            var vocabularyResultValid = Results.SelectedItemObservable
                .Select(result => result != NoResult);

            var customResultValid = this.WhenAny(x => x.CustomResult, x => x.Value)
                .Select(change => !string.IsNullOrWhiteSpace(change));

            var resultValid = this.WhenAny(x => x.IsCustomResult, x => x.Value)
                .SelectMany(isCustomResult => (isCustomResult) ? customResultValid : vocabularyResultValid);

            return resultValid;
        }

        protected override void UpdateModel() {
            Current.Model.AnalysisID = Analyses.SelectedItem.AnalysisID;
            Current.Model.AnalysisResult = (IsCustomResult) ? CustomResult : Results.SelectedItem.Result;
        }
    }
}
