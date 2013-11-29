using DiversityPhone.Storage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;

namespace DiversityPhone.UnitTest
{
    [Table]
    internal class TestEntity : IEntity<TestEntity>
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

        public System.Linq.Expressions.Expression<System.Func<TestEntity, bool>> IDEqualsExpression()
        {
            return x => x.ID == ID;
        }

        public TestEntity()
        {
            // ATTENTION: Default can't be stored in DB
            Timestamp = DateTime.Now;
        }
    }

    internal class FieldDataContext : DataContext
    {
        public FieldDataContext()
            : base("isostore:/test.sdf")
        {

        }
#pragma warning disable 0649
        public Table<TestEntity> Entities;
#pragma warning restore 0649
    }

    [TestClass]
    public class RepositoryTest
    {
        private FieldDataRepository Target;

        public RepositoryTest()
        {
            Target = new FieldDataRepository(() => new FieldDataContext());
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

            Target.Delete(ent);

            var res = Target.Get<TestEntity>(x => x.ID == ent.ID);

            Assert.IsTrue(res.Count() == 0);
        }

        [TestMethod]
        public void UpdateUpdates()
        {
            var ent = new TestEntity() { Altitude = 1.0, Name = "Update", Search = 100, Timestamp = DateTime.Now };
            var exp = new TestEntity() { Search = 1000 };

            Target.Add(ent);

            exp.ID = ent.ID;

            Target.Update(exp);

            var inStore = Target.Single<TestEntity>(exp.IDEqualsExpression());

            Assert.AreEqual(exp.Search, inStore.Search);
        }
    }
}
