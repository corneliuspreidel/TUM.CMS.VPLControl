using TUM.CMS.VPL.Scripting;

namespace TUM.CMS.VplControl.Rules.NRule
{
    public class RuleFile : ScriptFile
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RuleFile" /> class.
        /// </summary>
        public RuleFile()
        {
            // Add the Rule Engine and all the other stuff ... 
            ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            ReferencedAssemblies.Add("System.dll");
            ReferencedAssemblies.Add("System.Core.dll");
            ReferencedAssemblies.Add("System.Data.dll");
            ReferencedAssemblies.Add("System.Linq.dll");

            ReferencedAssemblies.Add("NRules.dll");

            ScriptContent =
                @"
                using NRules.Fluent.Dsl;

                public class PreferredCustomerDiscountRule : Rule
                {
                    public override void Define()
                    {
                        Customer customer = null;
                        IEnumerable<Order> orders = null;

                        When()
                            .Match<Customer>(() => customer, c => c.IsPreferred)
                            .Query(() => orders, x => x
                                .Match<Order>(
                                    o => o.Customer == customer,
                                    o => !o.IsDiscounted)
                                .Collect()
                                .Where(c => c.Any()));

                        Then()
                            .Do(ctx => orders.ToList().ForEach(o => o.ApplyDiscount(10.0)))
                            .Do(ctx => orders.ToList().ForEach(ctx.Update));
                    }
                }
                ";
        }
    }
}