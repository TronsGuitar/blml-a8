using BLML.Phase7Optimization.Refactoring;

namespace BLML.Tests;

public class Phase7RefactoringTests
{
    [Fact]
    public void LinqOptimizer_ShouldSuggestCountAndSumReplacements()
    {
        var optimizer = new LinqOptimizer();
        var suggestions = optimizer.SuggestOptimizations("""
            using System.Collections.Generic;

            public class Sample
            {
                public void Run(IEnumerable<int> numbers)
                {
                    var count = 0;
                    foreach (var number in numbers)
                    {
                        count++;
                    }

                    var total = 0;
                    foreach (var number in numbers)
                    {
                        total += number;
                    }
                }
            }
            """);

        Assert.Contains(suggestions, suggestion => suggestion.Category == "Count" && suggestion.SuggestedReplacement.Contains("numbers.Count()"));
        Assert.Contains(suggestions, suggestion => suggestion.Category == "Sum" && suggestion.SuggestedReplacement.Contains("numbers.Sum(number => number)"));
    }

    [Fact]
    public void LinqOptimizer_ShouldSuggestFilteredProjectionReplacement()
    {
        var optimizer = new LinqOptimizer();
        var suggestions = optimizer.SuggestOptimizations("""
            using System.Collections.Generic;

            public class Customer
            {
                public bool IsActive { get; set; }
                public string Name { get; set; } = string.Empty;
            }

            public class Sample
            {
                public void Run(IEnumerable<Customer> customers)
                {
                    var names = new List<string>();
                    foreach (var customer in customers)
                    {
                        if (customer.IsActive)
                        {
                            names.Add(customer.Name);
                        }
                    }
                }
            }
            """);

        Assert.Contains(suggestions, suggestion =>
            suggestion.Category == "Projection" &&
            suggestion.SuggestedReplacement.Contains("customers.Where(customer => customer.IsActive).Select(customer => customer.Name).ToList()"));
    }

    [Fact]
    public void LinqOptimizer_ShouldSuggestMinMaxReplacements()
    {
        var optimizer = new LinqOptimizer();
        var suggestions = optimizer.SuggestOptimizations("""
            using System.Collections.Generic;

            public class Sample
            {
                public void Run(IEnumerable<int> numbers)
                {
                    var maxVal = int.MinValue;
                    foreach (var number in numbers)
                    {
                        if (number > maxVal) maxVal = number;
                    }

                    var minVal = int.MaxValue;
                    foreach (var number in numbers)
                    {
                        if (number < minVal) minVal = number;
                    }
                }
            }
            """);

        Assert.Contains(suggestions, s => s.Category == "Max" && s.SuggestedReplacement.Contains("numbers.Max()"));
        Assert.Contains(suggestions, s => s.Category == "Min" && s.SuggestedReplacement.Contains("numbers.Min()"));
    }

    [Fact]
    public void LinqOptimizer_ShouldSuggestMaxWithSelectorForMemberAccess()
    {
        var optimizer = new LinqOptimizer();
        var suggestions = optimizer.SuggestOptimizations("""
            using System.Collections.Generic;

            public class Order
            {
                public decimal Amount { get; set; }
            }

            public class Sample
            {
                public void Run(IEnumerable<Order> orders)
                {
                    var highest = 0m;
                    foreach (var order in orders)
                    {
                        if (order.Amount > highest) highest = order.Amount;
                    }
                }
            }
            """);

        Assert.Contains(suggestions, s =>
            s.Category == "Max" &&
            s.SuggestedReplacement.Contains("orders.Max(order => order.Amount)"));
    }
}
