using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Sackrany.CameraEntity
{
    public partial struct EntitiesCameraSystem : ISystem
    {
        private bool _isFound;
        private Entity _cameraEntity;
        [BurstDiscard]
        public void OnUpdate(ref SystemState state)
        {
            if (!_isFound)
            {
                if (UnityEngine.Camera.main == null) return;
                _cameraEntity = state.EntityManager.CreateEntity(
                    typeof(CameraRef),
                    typeof(CameraPosition),
                    typeof(CameraForward),
                    typeof(CameraRight),
                    typeof(CameraUp),
                    typeof(CameraRotation)
                );
                state.EntityManager.SetComponentData(_cameraEntity, new CameraRef()
                {
                    Value = UnityEngine.Camera.main
                });
                _isFound = true;
                return;
            }

            foreach (var (refer, pos, forward, right, up, rot) in SystemAPI.Query<
                         RefRO<CameraRef>,
                         RefRW<CameraPosition>,
                         RefRW<CameraForward>,
                         RefRW<CameraRight>,
                         RefRW<CameraUp>,
                         RefRW<CameraRotation>
                     >())
            {
                if (!refer.ValueRO.Value.IsValid())
                {
                    _isFound = false;
                    return;
                }
                
                pos.ValueRW.Value = refer.ValueRO.Value.Value.transform.position;
                forward.ValueRW.Value = refer.ValueRO.Value.Value.transform.forward;
                right.ValueRW.Value = refer.ValueRO.Value.Value.transform.right;
                up.ValueRW.Value = refer.ValueRO.Value.Value.transform.up;
                rot.ValueRW.Value = refer.ValueRO.Value.Value.transform.rotation;
            }
        }
    }

    public struct CameraRef : IComponentData
    {
        public UnityObjectRef<UnityEngine.Camera> Value;
    }
    public struct CameraPosition : IComponentData
    {
        public float3 Value;
    }
    public struct CameraForward : IComponentData
    {
        public float3 Value;
    }
    public struct CameraRight : IComponentData
    {
        public float3 Value;
    }
    public struct CameraUp : IComponentData
    {
        public float3 Value;
    }
    public struct CameraRotation : IComponentData
    {
        public quaternion Value;
    }
}