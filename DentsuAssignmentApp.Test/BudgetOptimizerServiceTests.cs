using System.Collections.Generic;
using DentsuAssignmentApp.Services;
using System.Threading.Tasks;
using DentsuAssignmentApp.Test.Helpers;

namespace DentsuAssignmentApp.Test;

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
        new List<AdBudget>() { new AdBudget(5000m, true), new AdBudget(3000m, true) },
        new List<AdBudget>() { new AdBudget(500m, true), new AdBudget(300m, true), new AdBudget(200m, false) },
    };

    [Fact]
    public void BudgetOptimizerService_SetParameters_ShouldSetCorrectly()
    {
        _service.SetParameters(new BudgetOptimizerService.OptimizerParams{
            AgencyFeePercentage = 0.01m,
            ThirdPartyFeePercentage = 0.02m,
            FixedCostsAgencyHours = 100,
            OtherAdBudgets = TestBudgets[1],
            TotalBudget = 10000,
            IsWithThirdParty = true
        });

        // Huiun: There might be a potential issue, which is using euqal on floating points 
        CustomAssertions.DecimalEqual(0.01m, _service.Params.AgencyFeePercentage, 0.01m);
        CustomAssertions.DecimalEqual(0.02m, _service.Params.ThirdPartyFeePercentage, 0.01m);
        CustomAssertions.DecimalEqual(100m, _service.Params.FixedCostsAgencyHours, 0.01m);
        for(int i = 0; i < TestBudgets[1].Count; i++)
        {
            CustomAssertions.DecimalEqual(TestBudgets[1][i].Value, _service.Params.OtherAdBudgets[i].Value, 0.01m);
            Assert.Equal(TestBudgets[1][i].IsWithThirdParty, _service.Params.OtherAdBudgets[i].IsWithThirdParty);
        }
        CustomAssertions.DecimalEqual(10000m, _service.Params.TotalBudget, 0.01m);
        Assert.True(_service.Params.IsWithThirdParty);
    }

    // testcases were precalculated on google spreadsheet with Goal Seek Addon
    // InlineData is using float instead of decimal because decimal is not allowed
    [Theory]
    [InlineData(0f, 0f, 0f, 0, 0f, 0f, false, 0f)]
    [InlineData(0.1f, 0.05f, 2000f, 1, 15000f, 4000f, true, 3304.35f)]
    [InlineData(0.1f, 0.05f, 2000f, 1, 15000f, 1000f, true, 3304.35f)]
    [InlineData(0.1f, 0.05f, 100f, 2, 2000f, 200f, false, 690.9f)]
    public async Task BudgetOptimizerService_Solve_ShouldReturnCorrectResult(
        decimal agencyFeePercentage, decimal thirdPartyFeePercentage, decimal fixedCostsAgencyHours, 
        int budgetIndex, decimal totalBudgets, decimal initialGuess, bool isWithThirdParty, decimal expected)
    {
        _service.SetParameters(new BudgetOptimizerService.OptimizerParams{
            AgencyFeePercentage = agencyFeePercentage,
            ThirdPartyFeePercentage = thirdPartyFeePercentage,
            FixedCostsAgencyHours = fixedCostsAgencyHours,
            OtherAdBudgets = TestBudgets[budgetIndex],
            TotalBudget = totalBudgets,
            IsWithThirdParty = isWithThirdParty
        });

        await _service.SolveAsync(initialGuess, 0.01m, 100);

        decimal actual = _service.Result;

        CustomAssertions.DecimalEqual(expected, actual, 0.01m);
    }

    [Fact]
    public async Task BudgetOptimizerService_Gets_ShouldProvideCorrectValues()
    {
        _service.SetParameters(new BudgetOptimizerService.OptimizerParams{
            AgencyFeePercentage = 0.1m,
            ThirdPartyFeePercentage = 0.05m,
            FixedCostsAgencyHours = 2000m,
            OtherAdBudgets = TestBudgets[1],
            TotalBudget = 15000m,
            IsWithThirdParty = true
        });

        await _service.SolveAsync(4000, 0.01m, 100);

        decimal actual = _service.Result;

        // Assert
        CustomAssertions.DecimalEqual(3304.35m, actual, 0.01m);
        CustomAssertions.DecimalEqual(11304.35m, _service.TotalAdSpend, 0.01m);
        CustomAssertions.DecimalEqual(11304.35m, _service.ThirdPartyAdSpend, 0.01m);
        CustomAssertions.DecimalEqual(1130.44m, _service.AgencyFees, 0.01m);
        CustomAssertions.DecimalEqual(565.22m, _service.ThirdPartyFees, 0.01m);
        CustomAssertions.DecimalEqual(15000m, _service.CalculatedTotalSpend, 0.01m);
    }
}