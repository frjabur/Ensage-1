﻿namespace ItemManager.Core.Modules.DefensiveAbilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Abilities.Interfaces;

    using Attributes;

    using Ensage;
    using Ensage.Common.Objects.UtilityObjects;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Handlers;
    using Ensage.SDK.Helpers;

    using EventArgs;

    using Menus;
    using Menus.Modules.DefensiveAbilities;

    [Module]
    internal class DefensiveAbilities : IDisposable
    {
        private readonly List<IDefensiveAbility> defensiveAbilities = new List<IDefensiveAbility>();

        private readonly Dictionary<AbilityId, string> defensiveAbilityNames = new Dictionary<AbilityId, string>
        {
            { AbilityId.item_blade_mail, "Blade mail" },
            { AbilityId.item_lotus_orb, "Lotus orb" },
            { AbilityId.item_black_king_bar, "Black king bar" },
            { AbilityId.item_shivas_guard, "Shivas guard" },
            { AbilityId.item_mjollnir, "Mjollnir" },
            { AbilityId.item_hurricane_pike, "Hurricane pike" },
        };

        private readonly Manager manager;

        private readonly DefensiveAbilitiesMenu menu;

        private readonly Sleeper sleeper = new Sleeper();

        private readonly IUpdateHandler updateHandler;

        public DefensiveAbilities(Manager manager, MenuManager menu)
        {
            this.manager = manager;
            this.menu = menu.DefensiveAbilitiesMenu;

            updateHandler = UpdateManager.Subscribe(OnUpdate, 100, false);
            manager.OnAbilityAdd += OnAbilityAdd;
            manager.OnAbilityRemove += OnAbilityRemove;
        }

        public void Dispose()
        {
            UpdateManager.Unsubscribe(OnUpdate);
            manager.OnAbilityAdd -= OnAbilityAdd;
            manager.OnAbilityRemove -= OnAbilityRemove;

            defensiveAbilities.Clear();
        }

        private void OnAbilityAdd(object sender, AbilityEventArgs abilityEventArgs)
        {
            if (!abilityEventArgs.IsMine)
            {
                return;
            }

            var ability = abilityEventArgs.Ability;

            var usableAbility = manager.MyHero.UsableAbilities.OfType<IDefensiveAbility>()
                .FirstOrDefault(x => x.Handle == ability.Handle);

            if (usableAbility == null)
            {
                return;
            }

            var name = defensiveAbilityNames.FirstOrDefault(x => x.Key == ability.Id).Value;

            menu.CreateMenu(usableAbility, name);
            defensiveAbilities.Add(usableAbility);
            updateHandler.IsEnabled = true;
        }

        private void OnAbilityRemove(object sender, AbilityEventArgs abilityEventArgs)
        {
            if (!abilityEventArgs.IsMine)
            {
                return;
            }

            defensiveAbilities.RemoveAll(x => x.Handle == abilityEventArgs.Ability.Handle);

            if (!defensiveAbilities.Any())
            {
                updateHandler.IsEnabled = false;
            }
        }

        private void OnUpdate()
        {
            if (sleeper.Sleeping || Game.IsPaused || !manager.MyHero.IsAlive)
            {
                return;
            }

            var enemies = EntityManager<Hero>.Entities
                .Where(x => x.IsValid && x.IsAlive && x.IsVisible && x.Team != manager.MyHero.Team && !x.IsIllusion)
                .ToList();

            if (!enemies.Any())
            {
                return;
            }

            var canUseItems = manager.MyHero.CanUseItems();
            var canUseAbilities = manager.MyHero.CanUseAbilities();

            foreach (var defensiveAbility in defensiveAbilities
                .Where(
                    x => menu.IsAbilityEnabled(x.Name) && x.CanBeCasted() && (x.IsItem ? canUseItems : canUseAbilities))
                .OrderByDescending(x => menu.GetPriority(x.Name)))
            {
                if (enemies.Count(x => x.Distance2D(manager.MyHero.Position) <= defensiveAbility.Menu.Range)
                    >= defensiveAbility.Menu.EnemyCount)
                {
                    UpdateManager.BeginInvoke(
                        () =>
                            {
                                if (!defensiveAbility.CanBeCasted())
                                {
                                    return;
                                }

                                defensiveAbility.Use();
                            },
                        defensiveAbility.Menu.Delay);

                    sleeper.Sleep(defensiveAbility.Menu.Delay + 100);
                    return;
                }
            }
        }
    }
}