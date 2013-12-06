
namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.Reactive.Linq;


    public class EditESVM : EditPageVM<EventSeries>
    {
        public ReactiveCommand FinishSeries { get; private set; }

        #region Properties
        private string _Description;
        public string Description
        {
            get { return _Description; }
            set { this.RaiseAndSetIfChanged(x => x.Description, ref _Description, value); }
        }

        private string _SeriesCode;
        public string SeriesCode
        {
            get { return _SeriesCode; }
            set { this.RaiseAndSetIfChanged(x => x.SeriesCode, ref _SeriesCode, value); }
        }

        private string _SeriesStart;
        public string SeriesStart
        {
            get
            {
                return _SeriesStart;
            }
            private set { this.RaiseAndSetIfChanged(x => x.SeriesStart, ref _SeriesStart, value); }
        }

        private DateTime? _SeriesEnd;
        public DateTime? SeriesEnd
        {
            get
            {
                return _SeriesEnd;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SeriesEnd, ref _SeriesEnd, value);
            }
        }
        #endregion


        public EditESVM(DataVMServices Services)
            : base(Services)
        {


            (FinishSeries = new ReactiveCommand(CurrentModelObservable.Select(es => es.SeriesEnd == null)))
                .Select(_ => DateTime.Now as DateTime?)
                .Subscribe(x => SeriesEnd = x);

            Save
                .Where(_ => _SeriesEnd != null)
                .Subscribe(_ => Services.Messenger.SendMessage<EventSeries>(null, MessageContracts.STOP));


        }

        private IObservable<bool> CanSave()
        {
            var descriptionNonEmpty =
                this.WhenAny(x => x.Description, x => x.Value)
                .Select(desc => !string.IsNullOrWhiteSpace(desc))
                .StartWith(false);

            var endsAfterItBegins =
                this.WhenAny(x => x.SeriesEnd, x => x.Value)
                .CombineLatest(CurrentModelObservable, (end, model) => new { SeriesEnd = end, Model = model })
                .Select(pair => (pair.SeriesEnd == null) ? true : pair.SeriesEnd.Value > pair.Model.SeriesStart)
                .StartWith(true);

            return descriptionNonEmpty.BooleanAnd(endsAfterItBegins);
        }

        protected override void UpdateView(EventSeries model)
        {
            Description = model.Description ?? string.Empty;
            SeriesCode = model.SeriesCode;
            SeriesEnd = model.SeriesEnd;
            var start = model.SeriesStart;
            SeriesStart = string.Format("{0} {1}", start.ToShortDateString(), start.ToShortTimeString());
        }

        protected override void UpdateModel()
        {
            Current.Model.Description = Description;
            Current.Model.SeriesCode = SeriesCode;
            Current.Model.SeriesEnd = SeriesEnd ?? Current.Model.SeriesEnd;
        }


    }
}
