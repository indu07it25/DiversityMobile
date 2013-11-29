using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
namespace DiversityPhone.Storage
{
    public interface IEntity<T>
    {
        Expression<Func<T, bool>> IDEqualsExpression();
    }

    public interface IRepository
    {
        void Add<T>(T entity) where T : class, IEntity<T>;
        IEnumerable<T> GetAll<T>() where T : class, IEntity<T>;
        IEnumerable<T> Get<T>(Expression<Func<T, bool>> filter) where T : class, IEntity<T>;
        T Single<T>(Expression<Func<T, bool>> filter) where T : class, IEntity<T>;
        void Update<T>(T entity) where T : class, IEntity<T>;
        void Delete<T>(T entity) where T : class, IEntity<T>;
    }

    public class FieldDataRepository : IRepository
    {
        private readonly Func<DataContext> GetContext;

        public FieldDataRepository(Func<DataContext> GetContext)
        {
            this.GetContext = GetContext;

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

        public void Add<T>(T entity) where T : class, IEntity<T>
        {
            using (var ctx = GetContext())
            {
                var table = ctx.GetTable<T>();

                table.InsertOnSubmit(entity);

                ctx.SubmitChanges();
            }
        }

        public IEnumerable<T> GetAll<T>() where T : class, IEntity<T>
        {
            return QueryImpl<T>(x => true);
        }

        public IEnumerable<T> Get<T>(Expression<Func<T, bool>> filter) where T : class, IEntity<T>
        {
            return QueryImpl(filter);
        }

        public T Single<T>(Expression<Func<T, bool>> filter) where T : class, IEntity<T>
        {
            return QueryImpl(filter).FirstOrDefault();
        }

        private IEnumerable<T> QueryImpl<T>(Expression<Func<T, bool>> filter) where T : class, IEntity<T>
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

        public void Update<T>(T entity) where T : class, IEntity<T>
        {
            var unmodifiedEntity = Single(entity.IDEqualsExpression());

            if (unmodifiedEntity != null)
            {
                using (var ctx = GetContext())
                {
                    var table = ctx.GetTable<T>();

                    table.Attach(entity, unmodifiedEntity);

                    ctx.SubmitChanges();
                }
            }
        }

        public void Delete<T>(T entity) where T : class, IEntity<T>
        {
            using (var ctx = GetContext())
            {
                var table = ctx.GetTable<T>();
                var attachedEntity = table.Where(entity.IDEqualsExpression()).SingleOrDefault();
                if (attachedEntity != null)
                {
                    table.DeleteOnSubmit(attachedEntity);
                }

                ctx.SubmitChanges();
            }
        }



    }
}
