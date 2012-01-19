﻿using System;
using ReactiveUI;
using System.Reactive.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using DiversityPhone.Messages;
using DiversityPhone.Services;
using ReactiveUI.Xaml;
using System.Reactive.Subjects;
using System.Linq;

namespace DiversityPhone.ViewModels
{
    public class ViewIUVM : ElementPageViewModel<IdentificationUnit>
    {
        public enum Pivots
        {
            Subunits,
            Analyses,
            Descriptions,
            Multimedia
        }     

        #region Commands
        public ReactiveCommand Add { get; private set; }
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

        private ObservableAsPropertyHelper<IList<IdentificationUnitVM>> _Subunits;
        public IList<IdentificationUnitVM> Subunits { get { return _Subunits.Value; } }
        
        #endregion

        

        public ViewIUVM()
        {            
            _Subunits = ValidModel
                .Select(iu => getSubUnits(iu))
                .ToProperty(this, vm => vm.Subunits);

            Add = new ReactiveCommand();
            var addMessageSource = 
                Add
                .Select(_ =>
                    {
                        switch(SelectedPivot)
                        {
                            case Pivots.Analyses:
                                return Page.EditIUAN;
                            case Pivots.Multimedia:
                                return Page.EditMMO;
                            case Pivots.Descriptions:
                                //TODO Description Page
                            case Pivots.Subunits:
                                return Page.EditIU;
                            default:
                                return Page.EditIU;
                        }
                    })
                .Select(p => new NavigationMessage(p,null, ReferrerType.IdentificationUnit, Current.Model.UnitID.ToString()));
            Messenger.RegisterMessageSource(addMessageSource);         
        }

        protected override IdentificationUnit ModelFromState(PageState s)
        {
            if (s.Context != null)
            {
                int id;
                if (int.TryParse(s.Context, out id))
                {
                    return Storage.getIdentificationUnitByID(id);
                }               
            }
            else if (s.Referrer != null)
            {
                int parentID;
                if (int.TryParse(s.Referrer, out parentID))
                {
                    if (s.ReferrerType == ReferrerType.IdentificationUnit)
                    {
                        var parent = Storage.getIdentificationUnitByID(parentID);
                        if (parent != null)
                            return new IdentificationUnit()
                            {
                                RelatedUnitID = parentID,
                                SpecimenID = parent.SpecimenID,
                            };
                    }
                    else if (s.ReferrerType == ReferrerType.Specimen)
                        return new IdentificationUnit()
                        {
                            SpecimenID = parentID
                        };
                }
            }                
            return null;
        } 
      
        private IList<IdentificationUnitVM> getSubUnits(IdentificationUnit iu)
        {
            return IdentificationUnitVM.getTwoLevelVMFromModelList(Storage.getSubUnits(iu),
                iu2 => Storage.getSubUnits(iu2),
                Messenger);                
        }

        protected override ElementVMBase<IdentificationUnit> ViewModelFromModel(IdentificationUnit model)
        {
            return new IdentificationUnitVM(Messenger, model, Page.EditIU);
        }
    }
}
