namespace DentsuAssignmentApp.Services 
{
    public class BudgetOptimizerService 
    {
        public event Action? OnSolve;
        public OptimizerParams Params { get; private set; }
        
        private decimal _result;
        public decimal Result { 
            get => _result; 
            private set
            {
                _result = value;
                NotifiyOnSolve();
            } 
        }

        public void SetParameters(OptimizerParams optimizerParams) 
        {
            Params = optimizerParams;
        }

        public decimal BudgetEquation(
            decimal otherAdSpend, decimal thirdPartyAdSpend, decimal x)
        {
            decimal totalAdSpend = otherAdSpend + x;
            decimal agencyFees = Params.AgencyFeePercentage * totalAdSpend;
            // Huiun: include current ad if it also uses the third party tool
            decimal thirdPartyFees = Params.IsWithThirdParty 
                ? Params.ThirdPartyFeePercentage * (thirdPartyAdSpend + x) 
                : Params.ThirdPartyFeePercentage * thirdPartyAdSpend;
            decimal result = totalAdSpend + agencyFees + thirdPartyFees + Params.FixedCostsAgencyHours;
            return result - Params.TotalBudget;
        }

        // Huiun: I use Newton-Raphson method to find the optimized budget
        // Reference: https://www.reddit.com/r/excel/comments/81coc9/how_to_solve_for_a_goal_seek_without_using_goal/
        public async Task SolveAsync(decimal initialGuess, decimal tolerance, int maxIterations)
        {
            await Task.Run(() =>
            {
                decimal otherAdSpend = 0;
                decimal thirdPartyAdSpend = 0;
                foreach(var budget in Params.OtherAdBudgets)
                {
                    otherAdSpend += budget.Value;
                    if(budget.IsWithThirdParty) thirdPartyAdSpend += budget.Value;
                }

                decimal x = initialGuess;
                decimal iter = 0;
                
                while(
                    Math.Abs(BudgetEquation(otherAdSpend, thirdPartyAdSpend, x)) > tolerance
                    && iter < maxIterations)
                {
                    decimal derivative = (
                        BudgetEquation(otherAdSpend, thirdPartyAdSpend, x + tolerance) - BudgetEquation(otherAdSpend, thirdPartyAdSpend, x)) 
                        / tolerance;
                    x = x - BudgetEquation(otherAdSpend, thirdPartyAdSpend, x) / derivative;
                    
                    iter++;
                }

                Result = x;
            });
        }

        public decimal GetCalculatedTotalBudget(decimal x)
        {
            decimal otherAdSpend = 0;
            decimal thirdPartyAdSpend = 0;
            foreach(var budget in Params.OtherAdBudgets)
            {
                otherAdSpend += budget.Value;
                if(budget.IsWithThirdParty) thirdPartyAdSpend += budget.Value;
            }

            decimal totalAdSpend = otherAdSpend + x;
            decimal agencyFees = Params.AgencyFeePercentage * totalAdSpend;
            // Huiun: include current ad if it also uses the third party tool
            decimal thirdPartyFees = Params.IsWithThirdParty 
                ? Params.ThirdPartyFeePercentage * (thirdPartyAdSpend + x) 
                : Params.ThirdPartyFeePercentage * thirdPartyAdSpend;
            return totalAdSpend + agencyFees + thirdPartyFees + Params.FixedCostsAgencyHours;
        }

        public decimal TotalAdSpend 
        {
            get 
            {
                decimal spend = 0;
                foreach(var budget in Params.OtherAdBudgets)
                {
                    spend += budget.Value;
                }

                spend += Result;

                return spend;
            }
        }
        public decimal ThirdPartyAdSpend
        {
            get
            {
                decimal spend = 0;
                foreach(var budget in Params.OtherAdBudgets)
                {
                    if(budget.IsWithThirdParty) spend += budget.Value;
                }

                if(Params.IsWithThirdParty) spend += Result;

                return spend;
            }
        }
        public decimal AgencyFees { get { return Params.AgencyFeePercentage * TotalAdSpend; } }
        public decimal ThirdPartyFees { get { return Params.ThirdPartyFeePercentage * ThirdPartyAdSpend; } }
        public decimal CalculatedTotalSpend { get { return TotalAdSpend + AgencyFees + ThirdPartyFees + Params.FixedCostsAgencyHours; } }
        
        private void NotifiyOnSolve() => OnSolve?.Invoke();
        public struct OptimizerParams
        {
            public decimal TotalBudget;
            public decimal AgencyFeePercentage;
            public decimal ThirdPartyFeePercentage;
            public decimal FixedCostsAgencyHours;   
            public List<AdBudget> OtherAdBudgets;
            public bool IsWithThirdParty; // Is current ad uses thrid party tool
        }
    }

    public class AdBudget 
    {
        public decimal Value { get; set; }
        public bool IsWithThirdParty { get; set; }
        public AdBudget(decimal value, bool isWithThirdParty)
        {
            Value = value;
            IsWithThirdParty = isWithThirdParty;
        }
        // maybe name
    }
}