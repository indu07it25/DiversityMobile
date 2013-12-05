namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    public class ViewCSVM : ViewPageVMBase<Specimen>, IPageServices<DataVMServices>
    {
        private ReactiveAsyncCommand getSubunits = new ReactiveAsyncCommand();

        public new DataVMServices Services { get; private set; }

        public enum Pivots
        {
            Units,
            Multimedia
        }

        #region Commands
        public ReactiveCommand Add { get; private set; }

        public ReactiveCommand<IElementVM<Specimen>> EditSpecimen { get; private set; }
        public ReactiveCommand<IElementVM<IdentificationUnit>> SelectUnit { get; private set; }

        #endregion

        #region Properties
        private Pivots _SelectedPivot;
        public Pivots SelectedPivot
        {
            get
            {
                return _SelectedPivot;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.SelectedPivot, ref _SelectedPivot, value);
            }
        }

        public ElementMultimediaVM MultimediaList { get; private set; }

        public ReactiveCollection<IElementVM<IdentificationUnit>> UnitList { get; private set; }
        #endregion

        public ViewCSVM(
            DataVMServices Services,
            ElementMultimediaVM MultimediaList
            )
            : base(Services)
        {

            EditSpecimen = new ReactiveCommand<IElementVM<Specimen>>(vm => !vm.Model.IsObservation());
            EditSpecimen
                .ToMessage(Services.Messenger, MessageContracts.EDIT);

            //SubUnits
            UnitList = getSubunits.RegisterAsyncFunction(spec => buildIUTree(spec as Specimen))
                .SelectMany(vms => vms)
                .ObserveOn(Services.Dispatcher)
                .CreateCollection();

            UnitList.ListenToChanges<IdentificationUnit>(iu => iu.RelatedUnitID == null);

            CurrentModelObservable
                .Do(_ => UnitList.Clear())
                .Subscribe(getSubunits.Execute);

            SelectUnit = new ReactiveCommand<IElementVM<IdentificationUnit>>();
            SelectUnit
                .ToMessage(Services.Messenger, MessageContracts.VIEW);

            //Multimedia
            this.MultimediaList = MultimediaList;
            CurrentModelObservable
                .Select(m => m as IMultimediaOwner)
                .Subscribe(MultimediaList);



            Add = new ReactiveCommand();
            Add.Where(_ => SelectedPivot == Pivots.Units)
                .Select(_ => new IdentificationUnitVM(new IdentificationUnit() { SpecimenID = Current.Model.SpecimenID.Value, RelatedUnitID = null }) as IElementVM<IdentificationUnit>)
                .ToMessage(Services.Messenger, MessageContracts.EDIT);
            Add.Where(_ => SelectedPivot == Pivots.Multimedia)
                .Subscribe(MultimediaList.AddMultimedia.Execute);
        }


        private IEnumerable<IElementVM<IdentificationUnit>> buildIUTree(Specimen spec)
        {
            var vmMap = new Dictionary<int, IdentificationUnitVM>();
            var toplevel = new List<IElementVM<IdentificationUnit>>();

            Queue<IdentificationUnit> work_left = new Queue<IdentificationUnit>(Services.Storage.Get<IdentificationUnit>(spec.Units()));

            while (work_left.Any())
            {
                var unit = work_left.Dequeue();
                IdentificationUnitVM vm;

                if (unit.RelatedUnitID.HasValue)
                {
                    IdentificationUnitVM parent;
                    if (vmMap.TryGetValue(unit.RelatedUnitID.Value, out parent))
                    {
                        vm = new IdentificationUnitVM(unit);
                        parent.SubUnits.Add(vm);
                    }
                    else
                    {
                        work_left.Enqueue(unit);
                        continue;
                    }
                }
                else
                {
                    vm = new IdentificationUnitVM(unit);
                    toplevel.Add(vm);
                }

                vmMap.Add(unit.UnitID.Value, vm);
            }

            return toplevel;
        }
    }
}
