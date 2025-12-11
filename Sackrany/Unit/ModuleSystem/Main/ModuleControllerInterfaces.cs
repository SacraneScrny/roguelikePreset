using System;

using Sackrany.Unit.Abstracts;
using Sackrany.Unit.Base;

namespace Sackrany.Unit.ModuleSystem.Main
{
    public interface IBaseModuleController : IDisposable
    {
        void Initialize(UnitBase unit);
        void Reset();
        bool IsModulesInitialized();
        bool IsUpdating();
    }
    
    public interface ISimpleModuleController : IBaseModuleController { }
    public interface IModuleController : ISimpleModuleController { }

    public interface IUpdatable
    {
        public void Update(float deltaTime);
        public void FixedUpdate(float deltaTime);
        public void LateUpdate(float deltaTime);
    }
    public interface IUpdatableModuleController : IModuleController, IUpdatable { }
    public interface ISimpleUpdatableModuleController : ISimpleModuleController, IUpdatable { }
}