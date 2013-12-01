using DiversityPhone.Interface;
using DiversityPhone.Storage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;

namespace DiversityPhone.UnitTest
{
    [Table]
    internal class TestEntity : IWriteableEntity
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int? ID { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public int Search { get; set; }

        [Column]
        public DateTime Timestamp { get; set; }

        [Column]
        public double Altitude { get; set; }

        public int? EntityID
        {
            get { return ID; }
            set { ID = value; }
        }


        public TestEntity()
        {
            // ATTENTION: Default can't be stored in DB
            Timestamp = DateTime.Now;
        }
    }

    [Table]
    internal class TestChild : IWriteableEntity
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int? ID { get; set; }

        [Column]
        public int? OwnerID { get; set; }

        [Column]
        public int? RelID { get; set; }

        public int? EntityID
        {
            get { return ID; }
            set { ID = value; }
        }
    }
    internal class TestDataContext : DataContext
    {
        public TestDataContext()
            : base("isostore:/test.sdf")
        {

        }
#pragma warning disable 0649
        public Table<TestEntity> Entities;
        public Table<TestChild> Children;
#pragma warning restore 0649
    }

    internal class TestRepository : Repository
    {
        public TestRepository()
            : base(() => new TestDataContext(), new TestDeletePolicy())
        {

        }
    }

    internal class TestDeletePolicy : IDeletePolicy
    {
        public void Enforce<T>(IDeleteOperation operation, T deletee) where T : class, IReadOnlyEntity
        {
            if (typeof(T) == typeof(TestEntity))
            {
                var ent = deletee as TestEntity;
                operation.Delete<TestChild>(x => x.OwnerID == ent.ID);
            }
            else if (typeof(T) == typeof(TestEntity))
            {
                var ch = deletee as TestChild;
                operation.Delete<TestChild>(x => x.RelID == ch.ID);
            }
        }
    }

    [TestClass]
    public class RepositoryTest
    {
        private Repository Target;

        public RepositoryTest()
        {
            Target = new TestRepository();
            Target.ClearDatabase();
        }

        [TestMethod]
        public void InsertSetsID()
        {
            var ent = new TestEntity() { ID = null, Name = "Test" };

            Target.Add(ent);

            Assert.AreNotEqual(null, ent.ID);
        }

        [TestMethod]
        public void GetGetsAll()
        {
            var entities = new[] {
                new TestEntity() { Search = 1000 },
                new TestEntity() { Search = 1000 },
                new TestEntity() { Search = 1000 }
            };

            foreach (var ent in entities)
            {
                Target.Add(ent);
            }
            var exp = entities.Select(e => e.ID).ToList();
            var res = Target.Get<TestEntity>(e => e.Search == 1000).Select(x => x.ID).ToList();

            CollectionAssert.AreEquivalent(exp, res);
        }


        [TestMethod]
        public void DeleteDeletes()
        {
            var ent = new TestEntity();

            Target.Add(ent);

            Target.Delete<TestEntity>(x => x.ID == ent.ID);

            var res = Target.Get<TestEntity>(x => x.ID == ent.ID);

            Assert.IsTrue(res.Count() == 0);
        }

        [TestMethod]
        public void UpdateUpdates()
        {
            var exp = new TestEntity() { Altitude = 1.0, Name = "Update", Search = 100, Timestamp = DateTime.Now };

            Target.Add(exp);

            Target.Update(exp, e => e.Search = 1000);

            var inStore = Target.Single<TestEntity>(x => x.ID == exp.ID);

            Assert.AreEqual(exp.Search, inStore.Search);
        }

        [TestMethod]
        public void DeletePolicyWorks()
        {
            var entities = new[] {
                new TestEntity() { Search = 1000 },
                new TestEntity() { Search = 1000 },
                new TestEntity() { Search = 1000 }
            };

            foreach (var ent in entities)
            {
                Target.Add(ent);
            }

            var children = new[] {
                new TestChild() { OwnerID = entities[0].ID },
                new TestChild() { OwnerID = entities[0].ID },
                new TestChild() { OwnerID = entities[1].ID }
            };

            foreach (var ent in children)
            {
                Target.Add(ent);
            }


            Target.Delete<TestEntity>(x => x.ID == entities[0].ID);

            var childrenLeft = Target.GetAll<TestChild>();

            Assert.IsFalse(childrenLeft.Any(x => x.OwnerID == entities[0].ID));
            Assert.IsTrue(childrenLeft.Any(x => x.OwnerID == entities[1].ID));
        }
    }
}
