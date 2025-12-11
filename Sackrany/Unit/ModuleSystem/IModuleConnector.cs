using System;

using Sackrany.Unit.ModuleSystem.Main;

namespace Sackrany.Unit.ModuleSystem
{
    public interface IModuleConnector<TModule>
        where TModule : ModuleBase, new()
    {
        public bool Add<T>(T template) where T : ITemplate<TModule>;
        public bool Add<T>(T[] templates) where T : ITemplate<TModule>;

        public bool Remove<T>() where T : TModule;
        public bool Remove(params Type[] types);
        
        public bool Has<T>(T value) where T : TModule;
        public bool Has<T>() where T : TModule;
        public TModule Get<T>() where T : TModule;
        public TModule Get<K>(K template) where K : ITemplate<TModule>;
        public bool TryGet<T>(out T value) where T : TModule;

        public bool HasOther<T>(T value) where T : class;
        public bool HasOther<T>() where T : class;
        public T GetOther<T>() where T : class;
        public bool GetOther<T>(out T value) where T : class;
    }
}