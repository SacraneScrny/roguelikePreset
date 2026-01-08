using System;
using System.Collections.Generic;

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
        
        public IEnumerable<ModuleBase> GetModules();
        public bool TryGetModule<T>(out T module) where T : ModuleBase;
        public bool TryGetModule(Type type, out ModuleBase module);
        public bool HasModule<T>() where T : ModuleBase;
    }
    
    public interface IModuleController : IBaseModuleController { }
    public interface IComplexityModuleController : IModuleController { }

    public interface IUpdatable
    {
        public void Update(float deltaTime);
        public void FixedUpdate(float deltaTime);
        public void LateUpdate(float deltaTime);
    }
    public interface IUpdatableComplexityModuleController : IComplexityModuleController, IUpdatable { }
    public interface IUpdatableModuleController : IModuleController, IUpdatable { }
}