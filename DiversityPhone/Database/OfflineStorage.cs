﻿using System;
using System.Reactive.Linq;
using System.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using ReactiveUI;
using DiversityPhone.Messages;
using System.Data.Linq;
using System.Text;
using System.Linq.Expressions;
using Svc = DiversityPhone.DiversityService;
using System.IO.IsolatedStorage;
using DiversityPhone.ViewModels;
using Funq;
using System.Reactive;

namespace DiversityPhone.Services
{
   

    public partial class OfflineStorage : IFieldDataService, IKeyMappingService
    {
        private IList<IDisposable> _subscriptions;
        private IMessageBus _messenger;
        private INotificationService _Notifications;
     

        public OfflineStorage(Container ioc)
        {
            this._messenger = ioc.Resolve<IMessageBus>();
            _Notifications = ioc.Resolve<INotificationService>();

            

            _subscriptions = new List<IDisposable>()
            {
                _messenger.Listen<IElementVM<EventSeries>>(MessageContracts.SAVE).Model()                        
                    .Subscribe(es => addOrUpdateEventSeries(es)),
                _messenger.Listen<IElementVM<EventSeries>>(MessageContracts.DELETE).Model()  
                    .Subscribe(es => deleteEventSeries(es)),

                _messenger.Listen<IElementVM<Event>>(MessageContracts.SAVE).Model() 
                    .Subscribe(ev => addOrUpdateEvent(ev)),
                _messenger.Listen<IElementVM<Event>>(MessageContracts.DELETE).Model() 
                    .Subscribe(ev=>deleteEvent(ev)),

                _messenger.Listen<IElementVM<EventProperty>>(MessageContracts.SAVE).Model() 
                    .Subscribe(cep=>addOrUpdateCollectionEventProperty(cep)),
                _messenger.Listen<IElementVM<EventProperty>>(MessageContracts.DELETE).Model() 
                    .Subscribe(cep => deleteEventProperty(cep)),

                _messenger.Listen<IElementVM<Specimen>>(MessageContracts.SAVE).Model() 
                    .Subscribe(spec => addOrUpdateSpecimen(spec)),
                _messenger.Listen<IElementVM<Specimen>>(MessageContracts.DELETE).Model() 
                    .Subscribe(spec=>deleteSpecimen(spec)),

                _messenger.Listen<IElementVM<IdentificationUnit>>(MessageContracts.SAVE).Model() 
                    .Subscribe(iu => addOrUpdateIUnit(iu)),
                _messenger.Listen<IElementVM<IdentificationUnit>>(MessageContracts.DELETE).Model() 
                    .Subscribe(iu=>deleteIU(iu)),

                _messenger.Listen<IElementVM<IdentificationUnitAnalysis>>(MessageContracts.SAVE).Model() 
                    .Subscribe(iua=>addOrUpdateIUA(iua)),
                 _messenger.Listen<IElementVM<IdentificationUnitAnalysis>>(MessageContracts.DELETE).Model() 
                    .Subscribe(iua=>deleteIUA(iua)),

                _messenger.Listen<IElementVM<MultimediaObject>>(MessageContracts.SAVE).Model()
                    .Subscribe(mmo => addMultimediaObject(mmo)),
                _messenger.Listen<IElementVM<MultimediaObject>>(MessageContracts.DELETE).Model()
                    .Subscribe(mmo=>deleteMMO(mmo)),

                _messenger.Listen<GeoPointForSeries>(MessageContracts.SAVE)
                    .Subscribe(gp => addGeoPoint(gp)),
                _messenger.Listen<GeoPointForSeries>(MessageContracts.DELETE)
                    .Subscribe(gp => deleteGeoPoint(gp)),

                _messenger.Listen<ILocalizable>(MessageContracts.SAVE)
                    .Subscribe(loc => 
                        {
                            if (loc is Event)
                                addOrUpdateEvent(loc as Event);
                            else if (loc is IdentificationUnit)
                                addOrUpdateIUnit(loc as IdentificationUnit);
                            else if (loc is GeoPointForSeries)
                                addGeoPoint(loc as GeoPointForSeries);
                        })
                   
            };           
        }

        public void deleteAndNotifyAsync<T>(T detachedRow) where T : class
        {
            _Notifications.showProgress(
                CascadingDelete.deleteCascadingAsync(detachedRow)
                .StartWith(Unit.Default)
                .Select(_ => DiversityResources.Info_DeletingObjects)
                );
        }

       

        #region EventSeries

        private IList<EventSeries> esQuery(Expression<Func<EventSeries, bool>> restriction = null)
        {
            return uncachedQuery(ctx =>
            {
                var q = (from es in ctx.EventSeries
                         select es);
                if (restriction == null)
                    return q;
                else
                    return q.Where(restriction);
            });
        }

        public IList<EventSeries> getAllEventSeries()
        {
            return esQuery();
        }

        public IList<EventSeries> getNewEventSeries()
        {
            return esQuery(es => es.ModificationState == ModificationState.Modified);
        }

        public EventSeries getEventSeriesByID(int? id)
        {
            if (!id.HasValue)
                return EventSeries.NoEventSeries;

            return singletonQuery(ctx => from es in ctx.EventSeries
                                         where es.SeriesID == id.Value
                                         select es);

        }

        public void addOrUpdateEventSeries(EventSeries newSeries)
        {
            if (EventSeries.isNoEventSeries(newSeries))
                return;
            addOrUpdateRow(EventSeries.Operations, ctx => ctx.EventSeries, newSeries);
            _messenger.SendMessage<EventSeries>(newSeries, MessageContracts.START);
        }

        public void deleteEventSeries(EventSeries toDeleteEs)
        {                   
            
            deleteAndNotifyAsync(toDeleteEs);
        }




        #endregion

        #region GeoPointForSeries

        public IList<GeoPointForSeries> getAllGeoPoints()
        {
            return uncachedQuery(
            ctx =>
                from gt in ctx.GeoTour
                select gt
                );
        }

        public IEnumerable<GeoPointForSeries> getGeoPointsForSeries(int SeriesID)
        {           
            using (var ctx = new DiversityDataContext())
            {
                var query = from gt in ctx.GeoTour
                            where gt.SeriesID == SeriesID
                            select gt;

                foreach (var gp in query)
                    yield return gp;
            }
        }

        public void addGeoPoint(GeoPointForSeries gp)
        {
            addOrUpdateRow(GeoPointForSeries.Operations,
                ctx => ctx.GeoTour,
                gp
            );
        }

        public void deleteGeoPoint(GeoPointForSeries toDeleteGp)
        {
            deleteAndNotifyAsync(toDeleteGp);
        }

       

        #endregion

        #region Event

        public IEnumerable<Event> getAllEvents()
        {
            return enumerateQuery(ctx => ctx.Events);
        }


        public IEnumerable<Event> getEventsForSeries(EventSeries es)
        {
            //Workaround for the fact, that ev.SeriesID == es.SeriesID doesn't work for null values
            if (EventSeries.isNoEventSeries(es)) 
                return enumerateQuery(
                    ctx => from ev in ctx.Events
                           where ev.SeriesID == null
                           select ev);

            return enumerateQuery(
                ctx => from ev in ctx.Events
                       where ev.SeriesID == es.SeriesID
                       select ev);
        }
         

        public Event getEventByID(int id)
        {
            return singletonQuery(
                ctx => from ev in ctx.Events
                       where ev.EventID == id
                       select ev);
        }
          

        public void addOrUpdateEvent(Event ev)
        {
            var wasNewEvent = ev.IsNew();

            addOrUpdateRow(Event.Operations,
                  ctx => ctx.Events,
                  ev
              );

            if (wasNewEvent)
            {
                Specimen observation = new Specimen().MakeObservation();
                observation.EventID = ev.EventID;
                addOrUpdateSpecimen(observation);

            }
        }

        public void deleteEvent(Event toDeleteEv)
        {
            deleteAndNotifyAsync(toDeleteEv);
        }

        

        #endregion

        #region CollectionEventProperties

        public IEnumerable<EventProperty> getPropertiesForEvent(int eventID)
        {
            return enumerateQuery(ctx =>
                from cep in ctx.EventProperties
                where cep.EventID == eventID 
                select cep
                );
        }

        public EventProperty getPropertyByID(int eventId, int propertyId)
        {
            return singletonQuery(ctx => from cep in ctx.EventProperties
                                         where cep.EventID == eventId &&
                                                cep.PropertyID == propertyId
                                         select cep);
        }

        public void addOrUpdateCollectionEventProperty(EventProperty cep)
        {
            addOrUpdateRow(EventProperty.Operations,
                  ctx => ctx.EventProperties,
                  cep
              );
        }    

        public void deleteEventProperty(EventProperty toDeleteCep)
        {
            deleteAndNotifyAsync(toDeleteCep);
        }

        #endregion


        #region Specimen        
        
        public IEnumerable<Specimen> getAllSpecimen()
        {   
            return enumerateQuery(ctx => ctx.Specimen);
        }


        public IEnumerable<Specimen> getSpecimenForEvent(Event ev)
        {
            return enumerateQuery(ctx =>
                from spec in ctx.Specimen                 
                where spec.EventID == ev.EventID
                select spec
                );
        }
      

        public Specimen getSpecimenByID(int id)
        {
            return singletonQuery(
                ctx => from spec in ctx.Specimen
                       where spec.SpecimenID == id
                       select spec);
        }

        public IEnumerable<Specimen> getSpecimenWithoutEvent()
        {
          return enumerateQuery(ctx =>
                from spec in ctx.Specimen
                where spec.EventID == null 
                select spec
                );
        }

        public void addOrUpdateSpecimen(Specimen spec)
        {
            addOrUpdateRow(Specimen.Operations,
                ctx => ctx.Specimen,
                spec
            );
        }

        public void deleteSpecimen(Specimen toDeleteSpec)
        {
            deleteAndNotifyAsync(toDeleteSpec);
        }



        #endregion

        #region IdentificationUnit

        public IList<IdentificationUnit> getIUForSpecimen(int specimenID)
        {
            return uncachedQuery(
                ctx =>
                    from iu in ctx.IdentificationUnits
                    where iu.SpecimenID == specimenID
                    orderby iu.WorkingName
                    select iu
                    );
        }

        public IEnumerable<IdentificationUnit> getTopLevelIUForSpecimen(int specimenID)
        {
            return enumerateQuery(ctx =>
                from iu in ctx.IdentificationUnits
                where iu.SpecimenID == specimenID && iu.RelatedUnitID == null 
                orderby iu.WorkingName
                select iu
                );
        }


        public IEnumerable<IdentificationUnit> getSubUnits(IdentificationUnit unit)
        {
            return enumerateQuery(ctx =>
                from iu in ctx.IdentificationUnits
                where iu.RelatedUnitID == unit.UnitID
                orderby iu.WorkingName
                select iu
                );
        }

        public IdentificationUnit getIdentificationUnitByID(int id)
        {
            IdentificationUnit result = null;
            withDataContext((ctx) =>
                {
                    result = (from iu in ctx.IdentificationUnits
                              where iu.UnitID == id
                              select iu).FirstOrDefault();
                });
            return result;
        }

        public void addOrUpdateIUnit(IdentificationUnit iu)
        {   
            addOrUpdateRow(IdentificationUnit.Operations, ctx => ctx.IdentificationUnits, iu);           
        }

        public void deleteIU(IdentificationUnit toDeleteIU)
        {
            deleteAndNotifyAsync(toDeleteIU);
        }


        #endregion

        #region Analysis

        public IList<IdentificationUnitAnalysis> getIUANForIU(IdentificationUnit iu)
        {
            return uncachedQuery(ctx =>
                from iuan in ctx.IdentificationUnitAnalyses
                where iuan.UnitID == iu.UnitID
                select iuan
                );
        }

        public IdentificationUnitAnalysis getIUANByID(int iuanalysisID)
        {
            return singletonQuery(ctx => from iuan in ctx.IdentificationUnitAnalyses
                                         where iuan.IdentificationUnitAnalysisID == iuanalysisID
                                         select iuan);
        }

        public void addOrUpdateIUA(IdentificationUnitAnalysis iua)
        {            
            addOrUpdateRow(IdentificationUnitAnalysis.Operations,
                ctx => ctx.IdentificationUnitAnalyses,
                iua
            );
        }

        public void deleteIUA(IdentificationUnitAnalysis toDeleteIUA)
        {
            deleteAndNotifyAsync(toDeleteIUA);
        }

        #endregion

        #region Multimedia

        public IList<MultimediaObject> getMultimediaForObject(IMultimediaOwner owner)
        {
             IList<MultimediaObject> objects= uncachedQuery(ctx => from mm in ctx.MultimediaObjects
                                        where mm.OwnerType == owner.OwnerType
                                                && mm.RelatedId == owner.OwnerID
                                        select mm);
             return objects;

        }

        public MultimediaObject getMultimediaByID(int id)
        {
            return singletonQuery(ctx => from mm in ctx.MultimediaObjects
                                        where mm.MMOID == id
                                        select mm);
        }

        public MultimediaObject getMultimediaByURI(string uri)
        {
            IList<MultimediaObject> objects = uncachedQuery(ctx => from mm in ctx.MultimediaObjects
                                                                   where mm.Uri==uri
                                                                   select mm);
            if (objects.Count == 0)
                throw new KeyNotFoundException();
            if (objects.Count > 1)
                throw new DuplicateKeyException(objects);
            return objects[0];
        }
     

        public void addMultimediaObject(MultimediaObject mmo)
        {
            addOrUpdateRow(MultimediaObject.Operations,
            ctx => ctx.MultimediaObjects,
            mmo
            ); 
        }

        public void deleteMMO(MultimediaObject toDeleteMMO)
        {
            deleteAndNotifyAsync(toDeleteMMO);            
        }


        #endregion                

        #region Generische Implementierungen
        private void addOrUpdateRow<T>(IQueryOperations<T> operations, Func<DiversityDataContext, Table<T>> tableProvider, T row) where T : class, IModifyable
        {
            if(row == null)
            {
                throw new ArgumentNullException ("row");
            }

            withDataContext((ctx) =>
                {
                    var table = tableProvider(ctx);
                    var allRowsQuery = table as IQueryable<T>;



                    if (row.IsNew())      //New Object
                    {
                        operations.SetFreeKeyOnItem(allRowsQuery, row);
                        row.ModificationState = ModificationState.Modified; //Mark for Upload

                        table.InsertOnSubmit(row);                        
                        try
                        {
                            ctx.SubmitChanges();                            
                        }
                        catch (Exception)
                        {
                            System.Diagnostics.Debugger.Break();
                            
                            //Object not new
                            //TODO update?
                            
                        }
                    }
                    else
                    {
                        var existingRow = operations.WhereKeyEquals(allRowsQuery, row)
                                                    .FirstOrDefault();
                        if (existingRow != default(T))
                        {
                            
                            //Second DataContext necessary 
                            //because the action of querying for an existing row prevents a new version of that row from being Attach()ed
                            withDataContext((ctx2) =>
                                {
                                    tableProvider(ctx2).Attach(row, true);
                                    try
                                    {
                                        ctx2.SubmitChanges();
                                    }
                                    catch (Exception)
                                    {
                                        System.Diagnostics.Debugger.Break();
                                    }
                                });
                        }
                    }              
                });
        }

        private T singletonQuery<T>(Func<DiversityDataContext, IQueryable<T>> queryProvider)
        {
            T result = default(T);
            withDataContext(ctx =>
                {
                    var query = queryProvider(ctx);
                    result = query
                        .FirstOrDefault();
                });
            return result;
        }


        private void withDataContext(Action<DiversityDataContext> operation)
        {
            using (var ctx = new DiversityDataContext())
                operation(ctx);
        }

        private IList<T> uncachedQuery<T>(Func<DiversityDataContext, IQueryable<T>> query)
        {
            IList<T> result = null;
            withDataContext(ctx => result = query(ctx).ToList());
            return result;
        }


        private IEnumerable<T> enumerateQuery<T>(Func<DiversityDataContext, IQueryable<T>> query)
        {
            using (var ctx = new DiversityDataContext())
            {
                var q = query(ctx);

                foreach (var res in q)
                {
                    yield return res;
                }
            }
        }       

        #endregion

        public void clearDatabase()
        {
            using (var context = new DiversityDataContext())
            {
                context.DeleteDatabase();
                context.CreateDatabase();
            }
        }




        public int? ResolveKey(DBObjectType ownerType, int ownerID)
        {
            using (var ctx = new DiversityDataContext())
            {
                IEnumerable<int?> key;
                switch (ownerType)
                {
                    case DBObjectType.EventSeries:
                        key = from es in ctx.EventSeries
                              where es.SeriesID == ownerID
                              select es.CollectionSeriesID;
                        break;
                    case DBObjectType.Event:
                        key = from ev in ctx.Events
                              where ev.EventID == ownerID
                              select ev.CollectionEventID;
                        break;
                    case DBObjectType.Specimen:
                        key = from s in ctx.Specimen
                              where s.SpecimenID == ownerID
                              select s.CollectionSpecimenID;
                        break;
                    case DBObjectType.IdentificationUnit:
                        key = from iu in ctx.IdentificationUnits
                              where iu.UnitID == ownerID
                              select iu.CollectionUnitID;
                        break;
                    default:
                        throw new ArgumentException("ownerType");                        
                }

                return key.FirstOrDefault();
            }
        }

        public void AddMapping(DBObjectType ownerType, int ownerID, int serverID)
        {
            using (var ctx = new DiversityDataContext())
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


        public IEnumerable<MultimediaObject> getModifiedMMOs()
        {
            return enumerateQuery(ctx =>  from mmo in ctx.MultimediaObjects
                                          where mmo.ModificationState == ModificationState.Modified
                                          select mmo);
        }


        public int? ResolveKey<T>(T entity)
        {
            throw new NotImplementedException();
        }
    }
}