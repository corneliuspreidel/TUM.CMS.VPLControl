namespace TUM.CMS.VplControl.Rules.Data
{
    public class Rule
    {
        public string MemberName
        {
            get;
            set;
        }

        public string Operator
        {
            get;
            set;
        }

        public object TargetValue
        {
            get;
            set;
        }

        public Rule(string MemberName, string Operator, object TargetValue)
        {
            this.MemberName = MemberName;
            this.Operator = Operator;
            this.TargetValue = TargetValue;
        }
    }
}

