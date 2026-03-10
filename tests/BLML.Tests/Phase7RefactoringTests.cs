using BLML.Phase7Optimization.Refactoring;
using Xunit;

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
}
