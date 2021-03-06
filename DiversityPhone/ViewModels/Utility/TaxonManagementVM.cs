﻿namespace DiversityPhone.ViewModels {
    using DiversityPhone.Interface;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;

    public partial class TaxonManagementVM : PageVMBase {
        public enum Pivot {
            Local,
            Personal,
            Public
        }

        readonly IConnectivityService Connectivity;
        readonly ITaxonService Taxa;
        readonly IDiversityServiceClient Service;
        readonly INotificationService Notification;


        #region Properties

        private Pivot _CurrentPivot = Pivot.Local;
        public Pivot CurrentPivot {
            get {
                return _CurrentPivot;
            }
            set {
                this.RaiseAndSetIfChanged(x => x.CurrentPivot, ref _CurrentPivot, value);
            }
        }


        public bool IsOnlineAvailable { get { return _IsOnlineAvailable.Value; } }
        private ObservableAsPropertyHelper<bool> _IsOnlineAvailable;


        public ReactiveCollection<TaxonListVM> LocalLists { get; private set; }

        public ReactiveCollection<TaxonListVM> PersonalLists { get; private set; }

        public ReactiveCollection<TaxonListVM> PublicLists { get; private set; }

        public ReactiveCommand<TaxonListVM> Select { get; private set; }
        public ReactiveCommand<TaxonListVM> Download { get; private set; }
        public ReactiveCommand<TaxonListVM> Delete { get; private set; }
        public ReactiveCommand<TaxonListVM> Refresh { get; private set; }
        public IReactiveCommand DownloadAll { get; private set; }

        #endregion

        public TaxonManagementVM(
            IConnectivityService Connectivity,
            ITaxonService Taxa,
            IDiversityServiceClient Service,
            INotificationService Notification
            ) {
            this.Connectivity = Connectivity;
            this.Service = Service;
            this.Taxa = Taxa;
            this.Notification = Notification;

            _IsOnlineAvailable = this.ObservableToProperty(Connectivity.WifiAvailable(), x => x.IsOnlineAvailable);

            var localLists =
            this.FirstActivation()
                .SelectMany(_ =>
                    Taxa.getTaxonSelections()
                    .ToObservable(ThreadPoolScheduler.Instance)
                    .Select(list => new TaxonListVM(list)))
                    .Publish();
            LocalLists =
                localLists
                .ObserveOnDispatcher()
                .CreateCollection();



            var onlineLists =
            localLists
                .IgnoreElements() //only download lists once the local ones are loaded
                .Concat(Observable.Return(null as TaxonListVM))
                .CombineLatest(this.OnActivation(), (_, _2) => _2)
                .CheckConnectivity(Connectivity, Notification)
                .SelectMany(_ => {
                    return Service.GetTaxonLists()
                        .DisplayProgress(Notification, DiversityResources.TaxonManagement_State_DownloadingLists)
                        .TakeUntil(this.OnDeactivation());
                })
                .ObserveOnDispatcher()
                .SelectMany(lists =>
                    lists.Where(list => !LocalLists.Any(loc => loc.Model == list)) // Filter lists already present locally
                        .Select(list => new TaxonListVM(list))
                    )
                .Publish();

            PersonalLists =
                onlineLists.Where(vm => !vm.Model.IsPublicList)
                .CreateCollection();

            PublicLists =
                onlineLists.Where(vm => vm.Model.IsPublicList)
                .CreateCollection();

            onlineLists.Connect();
            localLists.Connect();

            Select = new ReactiveCommand<TaxonListVM>(vm => !vm.IsSelected && !vm.IsDownloading);
            Select.Subscribe(taxonlist => {
                foreach (var list in LocalLists) {
                    if (list.Model.TaxonomicGroup == taxonlist.Model.TaxonomicGroup)
                        list.Model.IsSelected = false;
                }

                Taxa.selectTaxonList(taxonlist.Model);
            });

            Download = new ReactiveCommand<TaxonListVM>(vm => !vm.IsDownloading);
            Download
                .CheckConnectivity(Connectivity, Notification)
                .Subscribe(taxonlist => {
                    if (Taxa.getTaxonTableFreeCount() > 0) {
                        CurrentPivot = Pivot.Local;
                        taxonlist.IsDownloading = true;

                        makeListLocal(taxonlist);

                        DownloadTaxonList(taxonlist)
                            .DisplayProgress(Notification, DiversityResources.TaxonManagement_State_DownloadingList)
                            .ObserveOnDispatcher()
                            .ShowServiceErrorNotifications(Notification)
                            .Subscribe(_ => {
                            	//Download Succeeded
                                taxonlist.IsDownloading = false;

                                if (Select.CanExecute(taxonlist))
                                    Select.Execute(taxonlist);
                            },
                                _ => //Download Failed
                                {
                                    taxonlist.IsDownloading = false;
                                    removeLocalList(taxonlist);
                                },
                                () => 
                                {

                                });
                    }
                });

            Delete = new ReactiveCommand<TaxonListVM>(vm => !vm.IsDownloading);
            Delete
                .Subscribe(taxonlist => {
                    removeLocalList(taxonlist);
                });

            Refresh = new ReactiveCommand<TaxonListVM>(vm => !vm.IsDownloading);
            Refresh
                .Subscribe(taxonlist => {
                    if (Delete.CanExecute(taxonlist)) //Deletes synchronously
                        Delete.Execute(taxonlist);

                    if (Download.CanExecute(taxonlist))
                        Download.Execute(taxonlist);
                });

            //Download all only on Personal pivot
            var canDownloadAll =
                this.WhenAny(x => x.CurrentPivot, x => x.GetValue())
                .Select(p => p == Pivot.Personal)
                .CombineLatest(Connectivity.WifiAvailable(), (p, wi) => p && wi);

            DownloadAll = new ReactiveCommand(canDownloadAll, initialCondition: false);
            DownloadAll
                .SelectMany(_ => PersonalLists.ToArray())
                .Where(vm => Download.CanExecute(vm))
                .Subscribe(Download.Execute);
        }

        private IObservable<TaxonListVM> DownloadTaxonList(TaxonListVM vm) {
            Taxa.addTaxonList(vm.Model);
            return
            Service.DownloadTaxonListChunked(vm.Model)
            .Do(chunk => Taxa.addTaxonNames(chunk, vm.Model), (Exception ex) => Taxa.deleteTaxonListIfExists(vm.Model))
            .IgnoreElements()
            .Select(_ => vm)
            .Concat(Observable.Return(vm));
        }

        private void makeListLocal(TaxonListVM list) {
            LocalLists.Add(list);
            if (list.Model.IsPublicList)
                PublicLists.Remove(list);
            else
                PersonalLists.Remove(list);
        }
        private void removeLocalList(TaxonListVM list) {
            Taxa.deleteTaxonListIfExists(list.Model);
            LocalLists.Remove(list);
            list.Model.IsSelected = false;
            if (list.Model.IsPublicList)
                PublicLists.Add(list);
            else
                PersonalLists.Add(list);
        }

    }
}
