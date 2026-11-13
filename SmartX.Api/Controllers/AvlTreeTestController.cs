
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.DataStructures;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvlTreeTestController : ControllerBase
    {
        [HttpGet]
        public IActionResult RunTests()
        {
            var results = new List<object>();

            // Test all four AVL rotation cases.
            var rotationCases = new Dictionary<string, int[]>
            {
                { "Left-Left", new[] { 30, 20, 10 } },
                { "Right-Right", new[] { 10, 20, 30 } },
                { "Left-Right", new[] { 30, 10, 20 } },
                { "Right-Left", new[] { 10, 30, 20 } }
            };

            foreach (var testCase in rotationCases)
            {
                var tree = new AvlTree<int>();

                foreach (var minute in testCase.Value)
                {
                    tree.Insert(
                        new DateTime(2026, 11, 12, 12, 0, 0)
                            .AddMinutes(minute),
                        minute);
                }

                var ordered = tree.GetAllInOrder();

                bool passed =
                    ordered.SequenceEqual(new[] { 10, 20, 30 }) &&
                    tree.Count == 3 &&
                    tree.Height == 2;

                results.Add(new
                {
                    Test = testCase.Key + " Rotation",
                    Passed = passed,
                    TreeHeight = tree.Height,
                    OrderedValues = ordered
                });
            }

            // Test duplicate timestamps.
            var duplicateTree = new AvlTree<string>();

            var timestamp =
                new DateTime(2026, 11, 12, 14, 0, 0);

            duplicateTree.Insert(timestamp, "Reading A");
            duplicateTree.Insert(timestamp, "Reading B");

            var duplicateResults =
                duplicateTree.SearchRange(timestamp, timestamp);

            results.Add(new
            {
                Test = "Duplicate Timestamps",
                Passed =
                    duplicateTree.Count == 2 &&
                    duplicateTree.Height == 1 &&
                    duplicateResults.SequenceEqual(
                        new[] { "Reading A", "Reading B" }),
                Count = duplicateTree.Count,
                Values = duplicateResults
            });

            // Test historical range searching.
            var historyTree = new AvlTree<int>();

            var baseTime =
                new DateTime(2026, 11, 12, 10, 0, 0);

            for (int i = 0; i < 10; i++)
            {
                historyTree.Insert(
                    baseTime.AddMinutes(i * 10),
                    i);
            }

            var rangeResults = historyTree.SearchRange(
                baseTime.AddMinutes(20),
                baseTime.AddMinutes(50));

            results.Add(new
            {
                Test = "Historical Range Search",
                Passed = rangeResults.SequenceEqual(
                    new[] { 2, 3, 4, 5 }),
                Expected = new[] { 2, 3, 4, 5 },
                Actual = rangeResults
            });

            // Test invalid date ranges.
            bool invalidRangeRejected = false;

            try
            {
                historyTree.SearchRange(
                    baseTime.AddHours(2),
                    baseTime);
            }
            catch (ArgumentException)
            {
                invalidRangeRejected = true;
            }

            results.Add(new
            {
                Test = "Invalid Date Range",
                Passed = invalidRangeRejected
            });

            // Test empty tree.
            var emptyTree = new AvlTree<int>();

            results.Add(new
            {
                Test = "Empty Tree",
                Passed =
                    emptyTree.Count == 0 &&
                    emptyTree.Height == 0 &&
                    emptyTree.GetAllInOrder().Count == 0
            });

            return Ok(new
            {
                Algorithm = "AVL Tree",
                TotalTests = results.Count,
                Results = results
            });
        }
    }
}
