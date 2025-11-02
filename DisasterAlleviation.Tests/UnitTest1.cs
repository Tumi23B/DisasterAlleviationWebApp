using Xunit;

namespace DisasterAlleviation.Tests
{
    public class BasicTests
    {
        [Fact]
        public void Check_If_Test_Works()
        {
            int a = 2;
            int b = 3;
            int sum = a + b;

            Assert.Equal(5, sum);
        }
    }
}
