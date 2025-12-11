using Sackrany.Unit.ModuleSystem.Main;

namespace Sackrany.Unit.Modules.ComponentsFeature
{
    public sealed class UnitComponentsController : ModuleController<UnitComponent, IUnitComponentTemplate>
    { 
        
    }
    
    public class UnitComponent : LinkedModule<UnitComponent, UnitComponentsController>
    {
        
    }
    
    public interface IUnitComponentTemplate : ITemplate<UnitComponent>
    {
        
    }
}