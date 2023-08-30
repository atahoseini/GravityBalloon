

using WpfApp1;

namespace WpfAppTest
{
    public class UnitTest1
    {
        [Theory]
        [InlineData(0, 0, 10, 20, 20, 10, true)]   // Overlapping circles
        [InlineData(0, 0, 10, 40, 40, 10, false)]  // Non-overlapping circles
        [InlineData(0, 0, 10, 25, 0, 10, false)]   // Circles centered on x-axis, not overlapping
        [InlineData(0, 0, 10, 0, 25, 10, false)]   // Circles centered on y-axis, not overlapping
        public void CheckOverlap_ShouldReturnExpectedResult(
                    double x1, double y1, double size1,
                    double x2, double y2, double size2,
                    bool expectedResult)
        {
            // Arrange
            Item item1 = new Item { X = x1, Y = y1, Size = size1 };
            Item item2 = new Item { X = x2, Y = y2, Size = size2 };

            // Act
            bool actualResult = CheckOverlap(item1, item2);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        private bool CheckOverlap(Item item1, Item item2)
        {
            // Implement your CheckOverlap logic here based on the original code
            double distance = Math.Sqrt(Math.Pow(item1.X - item2.X, 2) + Math.Pow(item1.Y - item2.Y, 2));
            double minDistance = (item1.Size + item2.Size) / 2;
            return distance < minDistance;
        }
    }
}