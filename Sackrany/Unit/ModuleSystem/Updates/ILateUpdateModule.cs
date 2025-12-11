namespace Sackrany.Unit.ModuleSystem.Updates
{
    public interface ILateUpdateModule
    {
        public void OnLateUpdate(float deltaTime);
    }
}