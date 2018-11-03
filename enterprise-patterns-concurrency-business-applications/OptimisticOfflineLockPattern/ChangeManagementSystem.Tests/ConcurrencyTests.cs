using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ChangeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ChangeManagementSystem.Tests
{
    [TestClass]
    public class ConcurrencyTests
    {
        [TestMethod]
        public void TestConcurrency()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDataContext>();

            AppDataContext context = new AppDataContext();

        }
    }
}
