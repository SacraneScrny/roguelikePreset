using System;
using System.Linq;
using System.Reflection;

using Sackrany.UserInterface.Abstract;

namespace Sackrany.UserInterface.Factory
{
    public static class PresenterFactory
    {
        public static Presenter[] GetPresenters(params PresenterType[] presenterTypes) 
            => Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Presenter)))
                .Where(t => presenterTypes.Any(p => p.ToString() == t.Name))
                .Select(t => (Presenter)Activator.CreateInstance(t))
                .ToArray();
    }
}