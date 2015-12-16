using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using TUM.CMS.VplControl.Core;

namespace TUM.CMS.VplControl.Rules.Nodes
{
    public class RuleNode : Node
    {
        private List<object> inputList { get; set; }
        private string propertyString { get; set; }
        private Expression operatorExpression { get; set; }
        private object targetValue { get; set; }

        public RuleNode(Core.VplControl hostCanvas) : base(hostCanvas)
        {
            // InputPorts
            AddInputPortToNode("InputElements", typeof (List<object>));
            AddInputPortToNode("propertyString", typeof(string));
            AddInputPortToNode("operatorExpression", typeof (Expression));
            AddInputPortToNode("targetValue", typeof(object));

            // OutputPorts
            AddOutputPortToNode("Result", typeof (object));
        }

        public override void Calculate()
        {
            // First of all -> get data
            //  if (InputPorts[0].Data == null || InputPorts[1].Data == null || InputPorts[2].Data == null || InputPorts[3].Data == null)
            //      return;

            // Get all the information
            try
            {
                inputList = InputPorts[0].Data as List<object>;
                propertyString = InputPorts[1].Data as string;
                operatorExpression = InputPorts[2].Data as Expression;
                targetValue = InputPorts[3].Data;
            }
            catch (Exception)
            {
                var test = true;
            }

            object res = null;

            // check the property of the object
            try
            {
                if (propertyString != null)
                    if (inputList != null)
                        if (inputList[0].GetType().GetProperty(propertyString) != null)
                        {
                            res = inputList[0].GetType().GetProperty(propertyString).GetValue(inputList[0], null);
                        }
            }
            catch (Exception)
            {
                throw;
            }
            
            if (res == null)
                return;

            // Check datatype of input and output
            if (res.GetType() != targetValue.GetType())
                return;

            // Create a lambda expression.
            if (operatorExpression != null)
            {
                var le = Expression.Lambda<Func<object>>(operatorExpression);

                // Compile the lambda expression.
                var compiledExpression = le.Compile();

                // Execute the lambda expression.
                var expressionResult = compiledExpression();
            }

            // Create the LINQ Expression
            var result = from e in inputList
                        where e.GetType().GetProperty(propertyString).GetValue(e, null) == targetValue
                        select e;

            OutputPorts[0].Data = result;
        }

        public override Node Clone()
        {
            return new RuleNode(HostCanvas)
            {
                Top = Top,
                Left = Left
            };
        }

    }
}