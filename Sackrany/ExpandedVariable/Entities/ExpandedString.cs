using System.Text;

using Sackrany.ExpandedVariable.Abstracts;

namespace Sackrany.ExpandedVariable.Entities
{
    public class ExpandedString : BaseSimpleExpandedVariable<string>
    {
        public ExpandedString(string variable) : base(variable) { }
        private protected override string CalculateValue()
        {
            StringBuilder sb = new StringBuilder(Variable);
            for (var i = 0; i < BaseAdditional.Count; i++)
            {
                var add = BaseAdditional[i];
                sb.Append(add.Invoke());
            }

            return sb.ToString();
        }
        
        public static implicit operator ExpandedString(string value)
        {
            return new ExpandedString(value);
        }
    }
}