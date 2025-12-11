using Sackrany.Unit.Abstracts;
using Sackrany.Unit.Base;

namespace Sackrany.Unit.Data
{
    public abstract class AUnitData
    {
        private protected UnitBase _unit;
        public void Initialize(UnitBase unit) => _unit = unit;
        public abstract void Reset();
    }
}