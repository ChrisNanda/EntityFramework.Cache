using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppFabricCacheProvider;
using System.Threading;

namespace EFCodeFirstCache.Test
{
    [TestClass]
    public class AppFabricCacheProviderTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //

        [TestInitialize()]
        public void MyTestInitialize()
        {
            using (ProductContext context = new ProductContext())
            {
                context.Products.ToList().ForEach(one => context.Products.Remove(one));
                context.SaveChanges();
                Product p = new Product()
                {
                    IsActive = true,
                    ProductName = "Plates",
                    ProductNumber = "001"
                };
                context.Products.Add(p);

                p = new Product()
                {
                    IsActive = true,
                    ProductName = "Cups",
                    ProductNumber = "002"
                };
                context.Products.Add(p);

                p = new Product()
                {
                    IsActive = false,
                    ProductName = "Napkins",
                    ProductNumber = "003"
                };
                context.Products.Add(p);
                context.SaveChanges();
            }
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            using (ProductContext context = new ProductContext())
            {
                context.Products.ToList().ForEach(one => context.Products.Remove(one));
                context.SaveChanges();
            }
        }

        #endregion


        /// <summary>
        ///A test for GetOrCreateCache
        ///</summary>
        [TestMethod]
        public void AppFabricCacheProviderGetOrCreateCacheWithExpirationTest()
        {
            AppFabricCacheProvider.AppFabricCacheProvider target = AppFabricCacheProvider.AppFabricCacheProvider.GetInstance();
            IQueryable<Product> query = null;
            using (ProductContext context = new ProductContext())
            {
                query = context.Products.OrderBy(one => one.ProductNumber).Where(one => one.IsActive);
                target.RemoveFromCache<Product>(query);

                TimeSpan cacheDuration = TimeSpan.FromSeconds(3);

                var actual = target.GetOrCreateCache<Product>(query, cacheDuration);
                Assert.AreEqual(2, actual.Count(), "Should have 2 rows");
                SQLCommandHelper.ExecuteNonQuery("Update Products Set IsActive = 0");
                actual = target.GetOrCreateCache<Product>(query, cacheDuration);
                Assert.AreEqual(2, actual.Count(), "Should have 2 rows");
                Thread.Sleep(4000);

                actual = target.GetOrCreateCache<Product>(query, cacheDuration);
                Assert.AreEqual(0, actual.Count(), "Should have 0 rows");

                target.RemoveFromCache<Product>(query);
            }
        }

        /// <summary>
        ///A test for GetOrCreateCache
        ///</summary>
        [TestMethod]
        public void AppFabricCacheProviderGetOrCreateCacheTest()
        {
            AppFabricCacheProvider.AppFabricCacheProvider target = AppFabricCacheProvider.AppFabricCacheProvider.GetInstance();
            IQueryable<Product> query = null;
            using (ProductContext context = new ProductContext())
            {
                query = context.Products.OrderBy(one => one.ProductNumber).Where(one => one.IsActive);

                target.RemoveFromCache<Product>(query);

                var actual = target.GetOrCreateCache<Product>(query);
                Assert.AreEqual(2, actual.Count(), "Should have 2 rows");

                SQLCommandHelper.ExecuteNonQuery("Update Products Set IsActive = 0");

                actual = target.GetOrCreateCache<Product>(query);
                Assert.AreEqual(2, actual.Count(), "Should have 2 rows");

                target.RemoveFromCache<Product>(query);

                actual = target.GetOrCreateCache<Product>(query);
                Assert.AreEqual(0, actual.Count(), "Should have 0 rows");

                target.RemoveFromCache<Product>(query);
            }

        }
    }
}
