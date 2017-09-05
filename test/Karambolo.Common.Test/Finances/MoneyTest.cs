using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace Karambolo.Common.Finances.Test
{


    /// <summary>
    ///This is a test class for MoneyTest and is intended
    ///to contain all MoneyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MoneyTest
    {


        private TestContext _testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get => _testContextInstance;
            set => _testContextInstance = value;
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
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseTest()
        {
            var expected = new Money(5005.25m, "eur");

            var actual = Money.Parse(" €5,005.25 ", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse("€ 5 005,25", CultureInfo.CreateSpecificCulture("hu-HU"));
            Assert.AreEqual(new Money(5005.25m, "eur"), actual);

            actual = Money.Parse("5005.25 EUR", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse("+5,005.25EUR", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse("+EUR5,005.25", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse("€ +5,005.25", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse("5,005.25+ eur", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse("5,005.25 eur+", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            expected = -expected;

            actual = Money.Parse("-5,005.25EUR", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse("-EUR5,005.25", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse("€ -5,005.25", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse("5,005.25- eur", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse("5,005.25 eur-", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            expected = new Money(-.25m, "usd");

            actual = Money.Parse("$-.25", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse("(USD.25)", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse("USD (0.25)", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse("(0.25$)", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse(" (.25) USD", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            expected = Money.ChangeCurrency(actual, Currency.None);

            actual = Money.Parse(" (.25) ", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);

            actual = Money.Parse(" (.25) ¤ ", CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest()
        {
            Assert.AreEqual("XBT25.00", new Money(25, "xbt").ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual("$25.0000", new Money(25, Currency.FromSymbol("$")).ToString("C4", CultureInfo.InvariantCulture));
            Assert.AreEqual("25.00", new Money(25, new Currency(null)).ToString("C2", CultureInfo.InvariantCulture));
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void RoundTest()
        {
            Assert.AreEqual(24.56m, Money.Round(new Money(24.555m, "eur")).Amount);
            Assert.AreEqual(24.6m, Money.Round(new Money(24.55m, Currency.FromCode("Eur")), 1).Amount);
            Assert.AreEqual(24m, Money.Round(new Money(24.5m, Currency.FromCode("XBT")), 0).Amount);
        }
    }
}
