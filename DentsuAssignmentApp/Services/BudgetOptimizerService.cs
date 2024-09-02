namespace DentsuAssignmentApp.Services 
{
    public class BudgetOptimizerService 
    {
        public event Action? OnSolve;
        public OptimizerParams Params { get; private set; }
        
        private float _result;
        public float Result { 
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

        public float BudgetEquation(
            float otherAdSpend, float thirdPartyAdSpend, float x)
        {
            float totalAdSpend = otherAdSpend + x;
            float agencyFees = Params.AgencyFeePercentage * totalAdSpend;
            // Huiun: include current ad if it also uses the third party tool
            float thirdPartyFees = Params.IsWithThirdParty 
                ? Params.ThirdPartyFeePercentage * (thirdPartyAdSpend + x) 
                : Params.ThirdPartyFeePercentage * thirdPartyAdSpend;
            float result = totalAdSpend + agencyFees + thirdPartyFees + Params.FixedCostsAgencyHours;
            return result - Params.TotalBudget;
        }

        // Huiun: I use Newton-Raphson method to find the optimized budget
        // Reference: https://www.reddit.com/r/excel/comments/81coc9/how_to_solve_for_a_goal_seek_without_using_goal/
        public async Task SolveAsync(float initialGuess, float tolerance, int maxIterations)
        {
            await Task.Run(() =>
            {
                float otherAdSpend = 0;
                float thirdPartyAdSpend = 0;
                foreach(var budget in Params.OtherAdBudgets)
                {
                    otherAdSpend += budget.Value;
                    if(budget.IsWithThirdParty) thirdPartyAdSpend += budget.Value;
                }

                float x = initialGuess;
                float iter = 0;
                
                while(
                    Math.Abs(BudgetEquation(otherAdSpend, thirdPartyAdSpend, x)) > tolerance
                    && iter < maxIterations)
                {
                    float derivative = (
                        BudgetEquation(otherAdSpend, thirdPartyAdSpend, x + tolerance) - BudgetEquation(otherAdSpend, thirdPartyAdSpend, x)) 
                        / tolerance;
                    x = x - BudgetEquation(otherAdSpend, thirdPartyAdSpend, x) / derivative;
                    
                    iter++;
                }

                Result = x;
            });
        }

        public float GetCalculatedTotalBudget(float x)
        {
            float otherAdSpend = 0;
            float thirdPartyAdSpend = 0;
            foreach(var budget in Params.OtherAdBudgets)
            {
                otherAdSpend += budget.Value;
                if(budget.IsWithThirdParty) thirdPartyAdSpend += budget.Value;
            }

            float totalAdSpend = otherAdSpend + x;
            float agencyFees = Params.AgencyFeePercentage * totalAdSpend;
            // Huiun: include current ad if it also uses the third party tool
            float thirdPartyFees = Params.IsWithThirdParty 
                ? Params.ThirdPartyFeePercentage * (thirdPartyAdSpend + x) 
                : Params.ThirdPartyFeePercentage * thirdPartyAdSpend;
            return totalAdSpend + agencyFees + thirdPartyFees + Params.FixedCostsAgencyHours;
        }

        public float TotalAdSpend 
        {
            get 
            {
                float spend = 0;
                foreach(var budget in Params.OtherAdBudgets)
                {
                    spend += budget.Value;
                }

                spend += Result;

                return spend;
            }
        }
        public float ThirdPartyAdSpend
        {
            get
            {
                float spend = 0;
                foreach(var budget in Params.OtherAdBudgets)
                {
                    if(budget.IsWithThirdParty) spend += budget.Value;
                }

                if(Params.IsWithThirdParty) spend += Result;

                return spend;
            }
        }
        public float AgencyFees { get { return Params.AgencyFeePercentage * TotalAdSpend; } }
        public float ThirdPartyFees { get { return Params.ThirdPartyFeePercentage * ThirdPartyAdSpend; } }
        public float CalculatedTotalSpend { get { return TotalAdSpend + AgencyFees + ThirdPartyFees + Params.FixedCostsAgencyHours; } }
        
        private void NotifiyOnSolve() => OnSolve?.Invoke();
        public struct OptimizerParams
        {
            public float TotalBudget;
            public float AgencyFeePercentage;
            public float ThirdPartyFeePercentage;
            public float FixedCostsAgencyHours;   
            public List<AdBudget> OtherAdBudgets;
            public bool IsWithThirdParty; // Is current ad uses thrid party tool
        }
    }

    public class AdBudget 
    {
        public float Value { get; set; }
        public bool IsWithThirdParty { get; set; }
        public AdBudget(float value, bool isWithThirdParty)
        {
            Value = value;
            IsWithThirdParty = isWithThirdParty;
        }
        // maybe name
    }
}