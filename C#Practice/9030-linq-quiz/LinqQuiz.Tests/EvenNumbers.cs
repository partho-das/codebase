using System;
using Xunit;
using LinqQuiz.Library;

namespace LinqQuiz.Tests
{
    public class EvenNumbers
    {
        [Fact]
        public void CurrectResult()
        {
            var result = Quiz.GetEvenNumbers(10);
            Assert.Equal(new[] { 2, 4, 6, 8 }, result);
        }

        [Fact]
        public void EmptyResult()
        {
            Assert.Empty(Quiz.GetEvenNumbers(1));
        }

        [Fact]
        public void InvalidArgument()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Quiz.GetEvenNumbers(0));
        }
    }
}
