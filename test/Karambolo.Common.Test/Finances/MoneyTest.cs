using System.Globalization;
using Xunit;

namespace Karambolo.Common.Finances.Test
{
    public class MoneyTest
    {
        static MoneyTest()
        {
            Currency.Register("USD", "$", 2);
            Currency.Register("EUR", "€", 2);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [Fact]
        public void ParseTest()
        {
            var expected = new Money(5005.25m, "eur");

            var actual = Money.Parse(" €5,005.25 ", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

#if !NETCOREAPP1_0
            actual = Money.Parse("€ 5 005,25", CultureInfo.CreateSpecificCulture("hu-HU"));
            Assert.Equal(new Money(5005.25m, "eur"), actual);
#endif

            actual = Money.Parse("5005.25 EUR", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse("+5,005.25EUR", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse("+EUR5,005.25", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse("€ +5,005.25", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse("5,005.25+ eur", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse("5,005.25 eur+", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            expected = -expected;

            actual = Money.Parse("-5,005.25EUR", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse("-EUR5,005.25", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse("€ -5,005.25", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse("5,005.25- eur", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse("5,005.25 eur-", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            expected = new Money(-.25m, "usd");

            actual = Money.Parse("$-.25", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse("(USD.25)", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse("USD (0.25)", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse("(0.25$)", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse(" (.25) USD", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            expected = Money.ChangeCurrency(actual, Currency.None);

            actual = Money.Parse(" (.25) ", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);

            actual = Money.Parse(" (.25) ¤ ", CultureInfo.InvariantCulture);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringTest()
        {
#if !NETCOREAPP1_0
            Assert.Equal("XBT25.00", new Money(25, "xbt").ToString(CultureInfo.InvariantCulture));
#endif

            Assert.Equal("$25.0000", new Money(25, Currency.FromSymbol("$")).ToString("C4", CultureInfo.InvariantCulture));
            Assert.Equal("25.00", new Money(25, new Currency(null)).ToString("C2", CultureInfo.InvariantCulture));
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void RoundTest()
        {
            Assert.Equal(24.56m, Money.Round(new Money(24.555m, "eur")).Amount);
            Assert.Equal(24.6m, Money.Round(new Money(24.55m, Currency.FromCode("Eur")), 1).Amount);
            Assert.Equal(24m, Money.Round(new Money(24.5m, Currency.FromCode("XBT")), 0).Amount);
        }
    }
}
