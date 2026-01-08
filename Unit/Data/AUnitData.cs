using Sackrany.Unit.Abstracts;
using Sackrany.Unit.Base;

namespace Sackrany.Unit.Data
{
    public abstract class AUnitData
    {
        private protected UnitBase _unit;
        public void Initialize(UnitBase unit)
        {
            _unit = unit;
            OnInitialize();
        }
        private protected virtual void OnInitialize() { }
        
        public abstract void Reset();
    }
}