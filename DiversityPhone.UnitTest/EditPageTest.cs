using DiversityPhone.Interface;
using DiversityPhone.Model;
using DiversityPhone.ViewModels;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using ReactiveUI;
using System;
using System.Reactive.Concurrency;

namespace DiversityPhone.UnitTest
{
    internal class TestEditPage : EditPageVM<TestEntity>
    {
        public TestEntity _LastUpdateView, _LastUpdateModel;

        public TestEditPage(DataVMServices Services)
            : base(Services, Model.Page.Current)
        {

        }

        protected override bool CanSave(TestEntity e)
        {
            return e.CanSave;
        }

        protected override void UpdateView(TestEntity model)
        {
            _LastUpdateView = model;
        }

        protected override void UpdateModel(TestEntity model)
        {
            _LastUpdateModel = model;
            model.Updated = true;
        }
    }

    internal class TestEntityVM : ReactiveObject, IElementVM<TestEntity>
    {
        public TestEntity Model
        {
            get;
            private set;
        }

        public string Description
        {
            get { return Model.Name; }
        }

        public Model.Icon Icon
        {
            get { throw new System.NotImplementedException(); }
        }

        object IElementVM.Model
        {
            get { throw new System.NotImplementedException(); }
        }

        public TestEntityVM(TestEntity m)
        {
            Model = m;
        }
    }

    internal class TestEditPolicy : IViewEditPolicy
    {
        public bool CanView<T>(T entity)
        {
            throw new System.NotImplementedException();
        }

        public bool CanViewDetails<T>(T entity)
        {
            throw new System.NotImplementedException();
        }

        public bool CanEdit<T>(T entity)
        {
            return (entity as TestEntity).CanEdit;
        }
    }

    internal class TestInsertService : IFieldDataService
    {
        #region stub implementations
        public void ClearDatabase()
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<IElementVM> CollectModifications()
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<T> GetAll<T>() where T : class, IReadOnlyEntity
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<T> Get<T>(System.Linq.Expressions.Expression<Func<T, bool>> filter) where T : class, IReadOnlyEntity
        {
            throw new NotImplementedException();
        }

        public T Single<T>(System.Linq.Expressions.Expression<Func<T, bool>> filter) where T : class, IReadOnlyEntity
        {
            throw new NotImplementedException();
        }

        public void Delete<T>(System.Linq.Expressions.Expression<Func<T, bool>> filter) where T : class, IReadOnlyEntity
        {
            throw new NotImplementedException();
        }
        #endregion

        public TestEntity _LastAdd;

        public Action<TestEntity> _PreUpdate, _PostUpdate;

        public void Update<T>(T unmodifiedEntity, Action<T> updateValues) where T : class, IWriteableEntity
        {
            if (typeof(T) != typeof(TestEntity))
                throw new InvalidOperationException();

            if (_PreUpdate != null) _PreUpdate(unmodifiedEntity as TestEntity);
            updateValues(unmodifiedEntity);
            if (_PostUpdate != null) _PostUpdate(unmodifiedEntity as TestEntity);
        }

        public void Add<T>(T entity) where T : class, IReadOnlyEntity
        {
            if (typeof(T) != typeof(TestEntity))
                throw new InvalidOperationException();

            _LastAdd = entity as TestEntity;
        }
    }

    [TestClass]
    public class EditPageTest
    {
        DataVMServices Services;
        TestEditPage Target;

        public EditPageTest()
        {
            Services = new DataVMServices()
            {
                ThreadPool = Scheduler.Immediate,
                Dispatcher = Scheduler.Immediate,
                EditPolicy = new TestEditPolicy(),
                Messenger = new MessageBus(),
                Activation = new PageActivator(),
                Storage = new TestInsertService(),
            };

            Target = new TestEditPage(Services);
        }

        [TestMethod, TestCategory("EditPageBase")]
        public void ShowsNewElements()
        {
            var entity = new TestEntity() { EntityID = 1 };
            var vm = new TestEntityVM(entity);

            Target._LastUpdateView = null;
            Target._LastUpdateModel = null;

            Services.Messenger.SendMessage<IElementVM<TestEntity>>(vm, MessageContracts.NEW);
            Services.Activation.Activate();

            Assert.AreSame(entity, Target._LastUpdateView);
            Assert.IsTrue(Target.IsEditable);
        }

        [TestMethod, TestCategory("EditPageBase")]
        public void ShowsPageOnElements()
        {
            var entity = new TestEntity() { EntityID = 1 };
            var vm = new TestEntityVM(entity);

            var pageRequested = false;

            Services.Messenger.Listen<Page>()
                .Subscribe(p => pageRequested = (p == Page.Current));

            Services.Messenger.SendMessage<IElementVM<TestEntity>>(vm, MessageContracts.NEW);

            Assert.IsTrue(pageRequested);

            pageRequested = false;

            Services.Messenger.SendMessage<IElementVM<TestEntity>>(vm, MessageContracts.VIEW_DETAILS);

            Assert.IsTrue(pageRequested);
        }

        [TestMethod, TestCategory("EditPageBase")]
        public void ShowsDetailsOfElements()
        {
            var entity = new TestEntity() { EntityID = 1 };
            var vm = new TestEntityVM(entity);

            Target._LastUpdateView = null;
            Target._LastUpdateModel = null;

            Services.Messenger.SendMessage<IElementVM<TestEntity>>(vm, MessageContracts.VIEW_DETAILS);
            Services.Activation.Activate();

            Assert.AreSame(entity, Target._LastUpdateView);
        }

        [TestMethod, TestCategory("EditPageBase")]
        public void ShowsNewElementsEditable()
        {
            var entity = new TestEntity() { EntityID = 1 };
            var vm = new TestEntityVM(entity);

            Services.Messenger.SendMessage<IElementVM<TestEntity>>(vm, MessageContracts.NEW);
            Services.Activation.Activate();

            Assert.IsTrue(Target.IsEditable);
        }

        [TestMethod, TestCategory("EditPageBase")]
        public void CanToggleElementsEditable()
        {
            var entity = new TestEntity() { CanEdit = true };
            var vm = new TestEntityVM(entity);

            Services.Messenger.SendMessage<IElementVM<TestEntity>>(vm, MessageContracts.VIEW_DETAILS);
            Services.Activation.Activate();

            Assert.IsFalse(Target.IsEditable);
            Assert.IsTrue(Target.ToggleEditable.CanExecute(null));

            Target.ToggleEditable.Execute(null);

            Assert.IsTrue(Target.IsEditable);

            // Can toggle editable multiple times 
            // only reason to disable command, is if the item can not be edited at all (then, display info banner)
            Assert.IsTrue(Target.ToggleEditable.CanExecute(null));

            Target.ToggleEditable.Execute(null);

            Assert.IsTrue(Target.IsEditable);
            Assert.IsTrue(Target.ToggleEditable.CanExecute(null));
        }

        [TestMethod, TestCategory("EditPageBase")]
        public void CannotToggleEditableIllegally()
        {
            var entity = new TestEntity() { CanEdit = false };
            var vm = new TestEntityVM(entity);

            Services.Messenger.SendMessage<IElementVM<TestEntity>>(vm, MessageContracts.VIEW_DETAILS);
            Services.Activation.Activate();

            Assert.IsFalse(Target.IsEditable);
            Assert.IsTrue(!Target.ToggleEditable.CanExecute(null));
        }

        [TestMethod, TestCategory("EditPageBase")]
        public void CannotSaveIllegaly()
        {
            var entity = new TestEntity() { CanSave = false, Updated = false };
            var vm = new TestEntityVM(entity);

            Services.Messenger.SendMessage<IElementVM<TestEntity>>(vm, MessageContracts.NEW);
            Services.Activation.Activate();

            Assert.IsFalse(Target.Save.CanExecute(null));
        }

        [TestMethod, TestCategory("EditPageBase")]
        public void SaveNewPerformsInsert()
        {
            // Setup
            var entity = new TestEntity() { CanSave = true, Updated = false };
            var vm = new TestEntityVM(entity);

            var repo = Services.Storage as TestInsertService;

            repo._LastAdd = null;
            repo._PreUpdate = x => Assert.Fail();
            repo._PostUpdate = x => Assert.Fail();

            // Execution
            Services.Messenger.SendMessage<IElementVM<TestEntity>>(vm, MessageContracts.NEW);
            Services.Activation.Activate();


            // Assertion
            Assert.IsTrue(Target.Save.CanExecute(null));
            Assert.IsNull(repo._LastAdd);
            Assert.IsFalse(entity.Updated);

            Target.Save.Execute(null);

            Assert.AreSame(entity, repo._LastAdd);
            Assert.IsTrue(entity.Updated);
        }

        [TestMethod, TestCategory("EditPageBase")]
        public void SaveOldPerformsUpdate()
        {
            // Setup
            var entity = new TestEntity() { CanSave = true, Updated = false };
            var vm = new TestEntityVM(entity);

            var repo = Services.Storage as TestInsertService;
            var canUpdate = false;

            repo._LastAdd = null;
            repo._PreUpdate = x =>
                {
                    Assert.IsTrue(canUpdate);
                    Assert.IsFalse(x.Updated);
                    Assert.AreSame(entity, x);
                };
            repo._PostUpdate = x => Assert.IsTrue(x.Updated);

            // Execution
            Services.Messenger.SendMessage<IElementVM<TestEntity>>(vm, MessageContracts.VIEW_DETAILS);
            Services.Activation.Activate();
            Target.ToggleEditable.Execute(null);


            Assert.IsTrue(Target.Save.CanExecute(null));
            Assert.IsNull(repo._LastAdd);
            Assert.IsFalse(entity.Updated);

            canUpdate = true;

            Target.Save.Execute(null);

            Assert.IsNull(repo._LastAdd);
        }

        [TestMethod, TestCategory("EditPageBase")]
        public void SaveOldDoesntNotify()
        {
            var notified = false;

            // Setup
            var entity = new TestEntity() { CanSave = true, Updated = false };
            var vm = new TestEntityVM(entity);
            var subscription = Services.Messenger.Listen<IElementVM<TestEntity>>(MessageContracts.SAVE_NEW)
                .Subscribe(x => notified = true);

            // Execution
            Services.Messenger.SendMessage<IElementVM<TestEntity>>(vm, MessageContracts.VIEW_DETAILS);
            Services.Activation.Activate();
            Target.ToggleEditable.Execute(null);
            Target.Save.Execute(null);

            Assert.IsFalse(notified);

            subscription.Dispose();

        }

        [TestMethod, TestCategory("EditPageBase")]
        public void SaveNewNotifies()
        {
            var notified = false;

            // Setup
            var entity = new TestEntity() { CanSave = true, Updated = false };
            var vm = new TestEntityVM(entity);
            var subscription = Services.Messenger.Listen<IElementVM<TestEntity>>(MessageContracts.SAVE_NEW)
                .Subscribe(x => notified = true);

            // Execution
            Services.Messenger.SendMessage<IElementVM<TestEntity>>(vm, MessageContracts.NEW);
            Services.Activation.Activate();
            Target.Save.Execute(null);

            Assert.IsTrue(notified);

            subscription.Dispose();
        }
    }
}
