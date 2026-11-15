using System;
using System.Collections.Generic;
using PlayerDirectory;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WeaponsDirectory
{
    /// <summary>
    /// The weapons the player owns this run. Created by Player in Awake and ticked from Player.Update.
    /// </summary>
    public class WeaponInventory
    {
        private readonly Player _player;
        private readonly List<WeaponRuntime> _weapons = new();
        private Transform _root;

        public IReadOnlyList<WeaponRuntime> Weapons => _weapons;
        public int MaxSlots => _player.PlayerBaseStats.MaxWeapons;
        public bool HasFreeSlot => _weapons.Count < MaxSlots;

        /// <summary>Raised when a weapon is added or levelled up (for HUD / achievements).</summary>
        public event Action<WeaponRuntime> OnWeaponChanged;

        public WeaponInventory(Player player)
        {
            _player = player;
            WeaponVisuals.Configure(player.GetComponent<SpriteRenderer>());
        }

        public WeaponRuntime Get(WeaponDefinition definition)
        {
            for (int i = 0; i < _weapons.Count; i++)
            {
                if (_weapons[i].Definition == definition) return _weapons[i];
            }

            return null;
        }

        /// <returns>0 if the weapon isn't owned.</returns>
        public int GetLevel(WeaponDefinition definition) => Get(definition)?.Level ?? 0;

        public bool CanUpgrade(WeaponDefinition definition)
        {
            WeaponRuntime owned = Get(definition);
            return owned?.CanLevelUp ?? HasFreeSlot;
        }

        /// <summary>Adds the weapon at level 1, or levels it up if already owned.</summary>
        public void AddOrUpgrade(WeaponDefinition definition)
        {
            if (!definition) return;

            WeaponRuntime weapon = Get(definition);
            if (weapon != null)
            {
                weapon.LevelUp();
            }
            else
            {
                if (!HasFreeSlot) return;

                if (!_root) _root = new GameObject("PlayerWeapons").transform;
                Transform weaponRoot = new GameObject(definition.DisplayName).transform;
                weaponRoot.SetParent(_root, false);

                weapon = definition.CreateRuntime(_player, weaponRoot);
                _weapons.Add(weapon);
            }

            OnWeaponChanged?.Invoke(weapon);
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f) return;

            for (int i = 0; i < _weapons.Count; i++)
            {
                _weapons[i].Tick(deltaTime);
            }
        }

        public void Dispose()
        {
            foreach (WeaponRuntime weapon in _weapons)
            {
                weapon.Dispose();
            }

            _weapons.Clear();
            if (_root) Object.Destroy(_root.gameObject);
        }
    }
}
