using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DiversityPhone.Interface
{
    public interface IDeleteOperation
    {
        void Delete<T>(Expression<Func<T, bool>> filter) where T : class, IReadOnlyEntity;
    }

    public interface IDeletePolicy
    {
        void Enforce<T>(IDeleteOperation operation, T deletee) where T : class, IReadOnlyEntity;
    }

    public interface IRepository
    {
        void Add<T>(T entity) where T : class, IReadOnlyEntity;
        IEnumerable<T> GetAll<T>() where T : class, IReadOnlyEntity;
        IEnumerable<T> Get<T>(Expression<Func<T, bool>> filter) where T : class, IReadOnlyEntity;
        T Single<T>(Expression<Func<T, bool>> filter) where T : class, IReadOnlyEntity;
        void Update<T>(T unmodifiedEntity, Action<T> updateValues) where T : class, IWriteableEntity;
        void Delete<T>(Expression<Func<T, bool>> filter) where T : class, IReadOnlyEntity;
    }
}
