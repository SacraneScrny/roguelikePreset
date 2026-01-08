using System;

using Sackrany.ExpandedVariable.Entities;

using Unity.Mathematics;

using UnityEngine;

namespace Sackrany.Unit.Features.ComponentsFeature.Modules
{
    public class UnitHealthComponent : UnitComponent
    {
        private bool _alreadyDied;
        private UnitHealth _template;

        public float Health;
        public ExpandedFloat MaxHealth;
        
        public UnitHealthComponent Construct(UnitHealth template)
        {
            _template = template;
            Health = _template.maxHealth;
            MaxHealth = _template.maxHealth;
            return this;
        }
        protected override void OnStart()
        {
            MaxHealth.OnValueChanged += OnMaxHealthChanged;
        }
        private void OnMaxHealthChanged(float value)
        {
            Health = math.clamp(Health, 0, value);
        }
        private void HealthChanged(float val)
        {
            OnHealthChanged?.Invoke(Health);
            if (_alreadyDied || val > 0) return;
            Unit.Event.Publish("OnDied");
            OnDied?.Invoke();
            _alreadyDied = true;
        }
        protected override void OnReset()
        {
            MaxHealth.Clear();
            _alreadyDied = false;
            OnDied = null;
            OnHealed = null;
            OnDamaged = null;
            OnHealthChanged = null;
        }

        public void Damage(DamageInfo info)
        {
            Health -= info.Damage;
            HealthChanged(Health);
            Health = math.clamp(Health, 0, MaxHealth);
            OnDamaged?.Invoke(info);
            Debug.Log($"{Unit.gameObject.name} damaged {info.Damage} to {Health}");
        }
        public void Heal(float heal)
        {
            Health += heal;
            HealthChanged(Health);
            Health = math.clamp(Health, 0, MaxHealth);
            OnHealed?.Invoke(heal);
            Debug.Log($"{Unit.gameObject.name} healed {heal} to {Health}");
        }

        public bool IsDead => _alreadyDied;
        public Action OnDied;
        public Action<DamageInfo> OnDamaged;
        public Action<float> OnHealed;
        public Action<float> OnHealthChanged;
    }
    
    [Serializable]
    public struct UnitHealth : IUnitComponentTemplate
    {
        public float maxHealth;
        public Type GetModuleType() => typeof(UnitHealthComponent);
        public UnitComponent GetModuleInstance() => new UnitHealthComponent().Construct(this);
    }

    public struct DamageInfo
    {
        public float Damage;
        public Vector3 Direction;
        public Vector3 Position;
    }
}