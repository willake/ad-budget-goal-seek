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

    [Fact]
    public void BudgetOptimizerService_SetParameters_ShouldSetCorrectly()
    {
        _service.SetParameters(new BudgetOptimizerService.OptimizerParams{
            AgencyFeePercentage = 0.01f,
            ThirdPartyFeePercentage = 0.02f,
            FixedCostsAgencyHours = 100f,
            OtherAdBudgets = TestBudgets[1],
            TotalBudget = 10000f,
            IsWithThirdParty = true
        });

        Assert.Equal(0.01f, _service.Params.AgencyFeePercentage, 0.01f);
        Assert.Equal(0.02f, _service.Params.ThirdPartyFeePercentage, 0.01f);
        Assert.Equal(100f, _service.Params.FixedCostsAgencyHours, 0.01f);
        for(int i = 0; i < TestBudgets[1].Count; i++)
        {
            Assert.Equal(TestBudgets[1][i].Value, _service.Params.OtherAdBudgets[i].Value, 0.01f);
            Assert.Equal(TestBudgets[1][i].IsWithThirdParty, _service.Params.OtherAdBudgets[i].IsWithThirdParty);
        }
        Assert.Equal(10000f, _service.Params.TotalBudget, 0.01f);
        Assert.True(_service.Params.IsWithThirdParty);
    }

    // testcases were precalculated on google spreadsheet with Goal Seek Addon
    [Theory]
    [InlineData(0f, 0f, 0f, 0, 0f, 0f, false, 0f)]
    [InlineData(0.1f, 0.05f, 2000f, 1, 15000f, 4000f, true, 3304.35f)]
    [InlineData(0.1f, 0.05f, 2000f, 1, 15000f, 1000f, true, 3304.35f)]
    [InlineData(0.1f, 0.05f, 100f, 2, 2000f, 200f, false, 690.9f)]
    public void BudgetOptimizerService_Solve_ShouldReturnCorrectResult(
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

    [Fact]
    public void BudgetOptimizerService_Gets_ShouldProvideCorrectValues()
    {
        _service.SetParameters(new BudgetOptimizerService.OptimizerParams{
            AgencyFeePercentage = 0.1f,
            ThirdPartyFeePercentage = 0.05f,
            FixedCostsAgencyHours = 2000f,
            OtherAdBudgets = TestBudgets[1],
            TotalBudget = 15000f,
            IsWithThirdParty = true
        });

        _service.Solve(4000, 0.01f, 100);

        float actual = _service.Result;

        // Assert
        Assert.Equal(3304.35f, actual, 0.01f);
        Assert.Equal(11304.35f, _service.TotalAdSpend, 0.01f);
        Assert.Equal(11304.35f, _service.ThirdPartyAdSpend, 0.01f);
        Assert.Equal(1130.44f, _service.AgencyFees, 0.01f);
        Assert.Equal(565.22f, _service.ThirdPartyFees, 0.01f);
        Assert.Equal(15000f, _service.CalculatedTotalSpend, 0.01f);
    }
}