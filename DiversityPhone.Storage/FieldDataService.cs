using DiversityPhone.Interface;
using DiversityPhone.Model;
using DiversityPhone.Storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;

namespace DiversityPhone.Services
{


    public partial class FieldDataService : Repository, IFieldDataService, IKeyMappingService
    {
        readonly IMessageBus Messenger;
        readonly INotificationService Notifications;
        readonly ICurrentProfile Profile;
        readonly ICreateViewModels VMFactory;


        public FieldDataService(
            IMessageBus Messenger,
            INotificationService Notifications,
            ICurrentProfile Profile,
            ICreateViewModels VMFactory,
            IDeletePolicy CascadingDelete
            )
            : base(() => new DiversityDataContext(Profile), CascadingDelete)
        {
            this.Messenger = Messenger;
            this.Notifications = Notifications;
            this.Profile = Profile;
            this.VMFactory = VMFactory;

            Messenger.SendMessage<EventSeries>(null, MessageContracts.START);
        }

        public IEnumerable<IElementVM> CollectModifications()
        {
            using (var ctx = new DiversityDataContext(Profile))
            {
                // Finished Series
                foreach (var i in (from es in ctx.EventSeries
                                   where es.CollectionSeriesID == null && es.SeriesEnd != null
                                   select VMFactory.CreateVM(es) as IElementVM))
                    yield return i;
                // Events in Uploaded Series
                foreach (var i in (from es in ctx.EventSeries
                                   where es.CollectionSeriesID != null
                                   join ev in ctx.Events on es.SeriesID equals ev.SeriesID
                                   where ev.CollectionEventID == null
                                   select VMFactory.CreateVM(ev) as IElementVM))
                    yield return i;
                // Events in NoEventSeries
                foreach (var i in (from ev in ctx.Events
                                   where ev.SeriesID == null && ev.CollectionEventID == null
                                   select VMFactory.CreateVM(ev) as IElementVM))
                    yield return i;
                // Specimen in Uploaded Series
                foreach (var i in (from ev in ctx.Events
                                   where ev.CollectionEventID != null
                                   join s in ctx.Specimen on ev.EventID equals s.EventID
                                   where s.CollectionSpecimenID == null
                                   select VMFactory.CreateVM(s) as IElementVM))
                    yield return i;

                foreach (var i in (from iu in
                                       //New IU with parent Spec Uploaded
                                       (from s in ctx.Specimen
                                        where s.CollectionSpecimenID != null
                                        join iu in ctx.IdentificationUnits on s.SpecimenID equals iu.SpecimenID
                                        where iu.CollectionUnitID == null
                                        && iu.RelatedUnitID == null
                                        select iu)
                                           //New IU with parent Unit uploaded
                                   .Union(from u in ctx.IdentificationUnits
                                          where u.CollectionUnitID != null
                                          join sub in ctx.IdentificationUnits on u.UnitID equals sub.RelatedUnitID
                                          where sub.CollectionUnitID == null
                                          select sub)
                                   select VMFactory.CreateVM(iu) as IElementVM))
                    yield return i;
            }

        }

        public override void Delete<T>(Expression<Func<T, bool>> filter)
        {
            Notifications.showProgress(
                Observable.Start(() => base.Delete(filter))
                .StartWith(Unit.Default)
                .Select(_ => DiversityResources.Info_DeletingObjects)
                );
        }


        public int? ResolveToServerKey(DBObjectType ownerType, int localID)
        {
            IWriteableEntity owner = null;
            switch (ownerType)
            {
                case DBObjectType.EventSeries:
                    owner = Single<EventSeries>(x => x.SeriesID == localID);
                    break;
                case DBObjectType.Event:
                    owner = Single<Event>(x => x.EventID == localID);
                    break;
                case DBObjectType.Specimen:
                    owner = Single<Specimen>(x => x.SpecimenID == localID);
                    break;
                case DBObjectType.IdentificationUnit:
                    owner = Single<IdentificationUnit>(x => x.UnitID == localID);
                    break;
                default:
                    throw new ArgumentException("ownerType");
            }

            return (owner == null) ? null : owner.EntityID;
        }

        public int? ResolveToLocalKey(DBObjectType ownerType, int localID)
        {
            using (var ctx = new DiversityDataContext(Profile))
            {
                IEnumerable<int?> key;
                switch (ownerType)
                {
                    case DBObjectType.EventSeries:
                        key = from es in ctx.EventSeries
                              where es.CollectionSeriesID == localID
                              select es.SeriesID as int?;
                        break;
                    case DBObjectType.Event:
                        key = from ev in ctx.Events
                              where ev.CollectionEventID == localID
                              select ev.EventID as int?;
                        break;
                    case DBObjectType.Specimen:
                        key = from s in ctx.Specimen
                              where s.CollectionSpecimenID == localID
                              select s.SpecimenID as int?;
                        break;
                    case DBObjectType.IdentificationUnit:
                        key = from iu in ctx.IdentificationUnits
                              where iu.CollectionUnitID == localID
                              select iu.UnitID as int?;
                        break;
                    default:
                        throw new ArgumentException("ownerType");
                }

                key = key ?? Enumerable.Empty<int?>();

                return key.FirstOrDefault();
            }
        }

        public void AddMapping(DBObjectType ownerType, int ownerID, int serverID)
        {
            using (var ctx = new DiversityDataContext(Profile))
            {
                switch (ownerType)
                {
                    case DBObjectType.EventSeries:
                        var a =
                        (from es in ctx.EventSeries
                         where es.SeriesID == ownerID
                         select es).Single();
                        a.CollectionSeriesID = serverID;
                        a.ModificationState = ModificationState.Unmodified;
                        break;
                    case DBObjectType.Event:
                        var b =
                        (from ev in ctx.Events
                         where ev.EventID == ownerID
                         select ev).Single();
                        b.CollectionEventID = serverID;
                        b.ModificationState = ModificationState.Unmodified;
                        break;
                    case DBObjectType.Specimen:
                        var c =
                        (from s in ctx.Specimen
                         where s.SpecimenID == ownerID
                         select s).Single();
                        c.CollectionSpecimenID = serverID;
                        c.ModificationState = ModificationState.Unmodified;
                        break;
                    case DBObjectType.IdentificationUnit:
                        var d =
                        (from iu in ctx.IdentificationUnits
                         where iu.UnitID == ownerID
                         select iu).Single();
                        d.CollectionUnitID = serverID;
                        d.ModificationState = ModificationState.Unmodified;
                        break;
                    default:
                        throw new ArgumentException("ownerType");
                }

                ctx.SubmitChanges();
            }
        }
    }
}
