using NRules;
using NRules.Fluent;

namespace TUM.CMS.VPL.Scripting.NRules
{
    public class NRulesCompiler
    {
        private RuleCompiler Compiler { get; set; }

        public void Compile()
        {
            //Load rules
            var repository = new RuleRepository();

            // Here is for the scriptedAssembly ... 
            // repository.Load(x => x.From(typeof(PreferredCustomerDiscountRule).Assembly));

            // Compile rules
            var factory = repository.Compile();

            //Create a working session
            var session = factory.CreateSession();

            //Load domain model
            // var customer = new Customer("John Doe") { IsPreferred = true };
            // var order1 = new Order(123456, customer, 2, 25.0);
            // var order2 = new Order(123457, customer, 1, 100.0);

            //Insert facts into rules engine's memory
            // session.Insert(customer);
            // session.Insert(order1);
            // session.Insert(order2);

            //Start match/resolve/act cycle
            session.Fire();
        }
    }
}