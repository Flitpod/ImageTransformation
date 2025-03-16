using Core.Detection;
using Core.Processors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Tests
{
    [TestFixture]
    public class LineIntersectionProcessorTests
    {
        public static object[] GetIntersection_ShouldPass =
        {
            new object[]
            {
                new LinePolar[]
                {
                    new LinePolar()
                    {
                        Radius = 0.948683298,
                        Theta = 1.892546881
                    },
                    new LinePolar()
                    {
                        Radius = 3.535533906,
                        Theta = Math.PI / 4
                    },
                },
                true,
                new Point(x: 3, y: 2)
            },
            new object[]
            {
                new LinePolar[]
                {
                    new LinePolar()
                    {
                        Radius = 3.535533906,
                        Theta = Math.PI / 4
                    },
                    new LinePolar()
                    {
                        Radius = 6,
                        Theta = 0
                    },
                },
                true,
                new Point(x: 6, y: -1)
            },
            new object[]
            {
                new LinePolar[]
                {
                    new LinePolar()
                    {
                        Radius = 4,
                        Theta = Math.PI / 2
                    },
                    new LinePolar()
                    {
                        Radius = 2,
                        Theta = Math.PI / 2
                    },
                },
                false,
                new Point(x: 0, y: 0)
            }
        };
        [Category("Functional")]
        [TestCaseSource(nameof(GetIntersection_ShouldPass))]
        public void TwoLineIntersectionCalculation_ShouldPass(IEnumerable<LinePolar> lines, bool existsExpected, Point expectedPoint)
        {
            // arrange 
            var linesArray = lines.ToArray();
            LinePolar line1 = linesArray[0];
            LinePolar line2 = linesArray[1];

            // act
            bool existsActual = LineIntersectionProcessor.GetIntersection(line1, line2, out Point actualPoint);

            // assert
            Assert.That(existsActual, Is.EqualTo(existsExpected));

            double tolerance = 10e-3;
            Assert.That(actualPoint.X, Is.InRange(expectedPoint.X - tolerance, expectedPoint.X + tolerance));
            Assert.That(actualPoint.Y, Is.InRange(expectedPoint.Y - tolerance, expectedPoint.Y + tolerance));
        }

        public static object[] GetIntersectionsFromPolarLines_ShouldPass =
        {
            new object[]
            {
                new LinePolar[]
                {
                    new LinePolar()
                    {
                        Radius = 0.948683298,
                        Theta = 1.892546881
                    },
                    new LinePolar()
                    {
                        Radius = 3.535533906,
                        Theta = Math.PI / 4
                    },
                    new LinePolar()
                    {
                        Radius = 6,
                        Theta = 0
                    },
                },
                new Point[]
                {
                    new Point(x: 6, y: -1),
                    new Point(x: 3, y: 2),
                    new Point(x: 6, y: 3),
                }
            }
        };
        [Category("Functional")]
        [TestCaseSource(nameof(GetIntersectionsFromPolarLines_ShouldPass))]
        public void GetIntersectionsFromPolarLinesTest_ShouldPass(IEnumerable<LinePolar> lines, IEnumerable<Point> expected)
        {
            // arrange 

            // act
            var actual = LineIntersectionProcessor.GetIntersectionsFromPolarLines(lines).OrderBy(l => l.Y);     

            // assert
            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }
}
