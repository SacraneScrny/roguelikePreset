using Sackrany.Utils;

namespace Logic.Utils
{
    public class PhysicsManager : AManager<PhysicsManager>
    {
        
    }

    public static class PhysicsLayers
    {
        public static uint Mask(PhysLayer a) => 1u << (int)a;
        public static uint Mask(PhysLayer a, PhysLayer b) => (1u << (int)a) | (1u << (int)b);
        public static uint Mask(PhysLayer a, PhysLayer b, PhysLayer c) => (1u << (int)a) | (1u << (int)b) | (1u << (int)c);
        
        public static uint DefaultObstacle => Mask(PhysLayer.Default, PhysLayer.Obstacle);
        public static uint DefaultUnit => Mask(PhysLayer.Default, PhysLayer.Unit);
        
        public enum PhysLayer : uint
        {
            Default = 0,
            Unit = 1,
            Obstacle = 2,
        }
    }
}