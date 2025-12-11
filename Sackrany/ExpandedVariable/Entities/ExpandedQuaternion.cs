using Sackrany.ExpandedVariable.Abstracts;

using UnityEngine;

namespace Sackrany.ExpandedVariable.Entities
{
    public class ExpandedQuaternion : BaseComplexityExpandedVariable<Quaternion>
    {
        public ExpandedQuaternion(Quaternion variable) : base(variable) { }
        private protected override Quaternion CalculateValue()
        {
            Quaternion _preadd = Quaternion.identity;
            for (var i = 0; i < BaseAdditional.Count; i++)
            {
                var a = BaseAdditional[i];
                _preadd *= a.Invoke();
            }

            Quaternion _postadd = Quaternion.identity;
            for (var i = 0; i < PostAdditional.Count; i++)
            {
                var a = PostAdditional[i];
                _postadd *= a.Invoke();
            }

            Quaternion _mult = Quaternion.identity;
            for (var i = 0; i < Multiply.Count; i++)
            {
                var a = Multiply[i];
                _mult *= a.Invoke();
            }

            return _preadd * _mult * _postadd;
        }

        public static implicit operator ExpandedQuaternion(Quaternion value)
        {
            return new ExpandedQuaternion(value);
        }
    }
}