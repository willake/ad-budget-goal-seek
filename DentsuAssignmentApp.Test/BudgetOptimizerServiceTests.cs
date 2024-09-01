using System.Collections.Generic;
using DentsuAssignmentApp.Services;

public class BudgetOptimizerServiceTests
{
    private readonly BudgetOptimizerService _service;

    public BudgetOptimizerServiceTests()
    {
        _service = new BudgetOptimizerService();
    }

    // Huiun: set budgets as static field because InlineData does not allow custom class array
    public static List<List<AdBudget>> TestBudgets = new() {
        new List<AdBudget>() {},
        new List<AdBudget>() { new AdBudget(5000f, true), new AdBudget(3000f, true) },
        new List<AdBudget>() { new AdBudget(500f, true), new AdBudget(300f, true), new AdBudget(200f, false) },
    };

    // testcases were precalculated on google spreadsheet with Goal Seek Addon
    [Theory]
    [InlineData(0f, 0f, 0f, 0, 0f, 0f, false, 0f)]
    [InlineData(0.1f, 0.05f, 2000f, 1, 15000f, 4000f, true, 3304.35f)]
    [InlineData(0.1f, 0.05f, 2000f, 1, 15000f, 1000f, true, 3304.35f)]
    [InlineData(0.1f, 0.05f, 100f, 2, 2000f, 200f, false, 690.9f)]
    public void BudgetOptimizer_Solve_ShouldReturnCorrectResult(
        float agencyFeePercentage, float thirdPartyFeePercentage, float fixedCostsAgencyHours, 
        int budgetIndex, float totalBudgets, float initialGuess, bool isWithThirdParty, float expected)
    {
        _service.SetParameters(new BudgetOptimizerService.OptimizerParams{
            AgencyFeePercentage = agencyFeePercentage,
            ThirdPartyFeePercentage = thirdPartyFeePercentage,
            FixedCostsAgencyHours = fixedCostsAgencyHours,
            OtherAdBudgets = TestBudgets[budgetIndex],
            TotalBudget = totalBudgets,
            IsWithThirdParty = isWithThirdParty
        });

        _service.Solve(initialGuess, 0.01f, 100);

        float actual = _service.Result;

        // Assert
        Assert.Equal(expected, actual, 0.01f);
    }
}