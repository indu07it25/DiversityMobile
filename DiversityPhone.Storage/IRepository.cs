using DiversityPhone.Interface;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
namespace DiversityPhone.Storage
{
    public class Repository : IRepository
    {
        protected readonly Func<DataContext> GetContext;
        protected readonly IDeletePolicy DeletePolicy;

        public Repository(Func<DataContext> GetContext, IDeletePolicy DeletePolicy)
        {
            this.GetContext = GetContext;
            this.DeletePolicy = DeletePolicy;

            CreateDBIfNecessary();
        }

        private void CreateDBIfNecessary()
        {
            using (var ctx = GetContext())
            {
                if (!ctx.DatabaseExists())
                {
                    ctx.CreateDatabase();
                }
            }
        }

        public void ClearDatabase()
        {
            using (var ctx = GetContext())
            {
                if (ctx.DatabaseExists())
                {
                    ctx.DeleteDatabase();
                }
                ctx.CreateDatabase();
            }
        }

        public virtual void Add<T>(T entity) where T : class, IReadOnlyEntity
        {
            using (var ctx = GetContext())
            {
                var table = ctx.GetTable<T>();

                table.InsertOnSubmit(entity);

                ctx.SubmitChanges();
            }
        }

        public virtual IEnumerable<T> GetAll<T>() where T : class, IReadOnlyEntity
        {
            return QueryImpl<T>(x => true);
        }

        public virtual IEnumerable<T> Get<T>(Expression<Func<T, bool>> filter) where T : class, IReadOnlyEntity
        {
            return QueryImpl(filter);
        }

        public virtual T Single<T>(Expression<Func<T, bool>> filter) where T : class, IReadOnlyEntity
        {
            return QueryImpl(filter).FirstOrDefault();
        }

        private IEnumerable<T> QueryImpl<T>(Expression<Func<T, bool>> filter) where T : class, IReadOnlyEntity
        {
            using (var ctx = GetContext())
            {
                var q = ctx.GetTable<T>()
                           .Where(filter);

                foreach (var x in q)
                {
                    yield return x;
                }
            }
        }

        public virtual void Update<T>(T unmodifiedEntity, Action<T> updateValues) where T : class, IWriteableEntity
        {
            using (var ctx = GetContext())
            {
                var table = ctx.GetTable<T>();

                table.Attach(unmodifiedEntity, asModified: false);

                updateValues(unmodifiedEntity);

                ctx.SubmitChanges();
            }
        }

        public virtual void Delete<T>(Expression<Func<T, bool>> filter) where T : class, IReadOnlyEntity
        {
            using (var ctx = GetContext())
            {
                var op = new DeleteOperation(this, ctx);

                DeleteImpl(op, filter);

                ctx.SubmitChanges();
            }
        }

        private void DeleteImpl<T>(DeleteOperation op, Expression<Func<T, bool>> filter) where T : class, IReadOnlyEntity
        {
            var table = op.Context.GetTable<T>();
            var tobeDeleted = table.Where(filter);
            table.DeleteAllOnSubmit(tobeDeleted);

            foreach (var item in tobeDeleted)
            {
                DeletePolicy.Enforce(op, item);
            }
        }

        private class DeleteOperation : IDeleteOperation
        {
            public readonly DataContext Context;
            private readonly Repository Owner;

            public DeleteOperation(Repository Owner, DataContext Context)
            {
                this.Owner = Owner;
                this.Context = Context;
            }

            public void Delete<T>(Expression<Func<T, bool>> filter) where T : class, IReadOnlyEntity
            {
                Owner.DeleteImpl<T>(this, filter);
            }
        }
    }
}
