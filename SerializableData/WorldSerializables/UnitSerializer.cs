using System.Collections.Generic;

using Sackrany.SerializableData.Components;
using Sackrany.Unit;
using Sackrany.Unit.Abstracts;
using Sackrany.Unit.ModuleSystem.Main;

using UnityEngine;

namespace Sackrany.SerializableData.WorldSerializables
{
    [RequireComponent(typeof(UnitBase))]
    public class UnitSerializer : SerializableBehaviour
    {
        public bool SerializePosition = true;
        public bool SerializeRotation = true;
        public bool SerializeScale;
        
        private UnitBase unitBase;
        private protected override void OnRegister()
        {
            unitBase = GetComponent<UnitBase>();
            
            unitBase.Command(SerializeUnit);
        }

        private void SerializeUnit(UnitBase u)
        {      
            RegisterSerializable(
                "unit::modules",
                () =>
                {
                    Dictionary<string, object[]> data = new ();
                    foreach (var controller in unitBase.GetControllers())
                        foreach (var module in controller.GetModules())
                            if (module is ISerializableModule serializableModule)
                                data.Add(module.GetType().Name, serializableModule.Serialize());
                    return data;
                },
                (data) =>
                {
                    Dictionary<string, ISerializableModule> ser = new ();
                    foreach (var controller in unitBase.GetControllers())
                        foreach (var module in controller.GetModules())
                            if (module is ISerializableModule serializableModule)
                                ser.Add(module.GetType().Name, serializableModule);

                    foreach (var d in data)
                        if (ser.TryGetValue(d.Key, out ISerializableModule serializableModule))
                            serializableModule.Deserialize(d.Value);
                });
            
            if (SerializePosition)
                RegisterSerializable("unit::position", () => transform.position, (p) => transform.position = p + Vector3.up * 0.01f);
            if (SerializeRotation)
                RegisterSerializable("unit::rotation", () => transform.rotation, (p) => transform.rotation = p);
            if (SerializeScale)
                RegisterSerializable("unit::scale", () => transform.localScale, (p) => transform.localScale = p);
        }
    }
}