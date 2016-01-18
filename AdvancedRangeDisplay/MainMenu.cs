﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace AdvancedRangeDisplay {
    internal static class MainMenu {
        public static Dictionary<Hero, Menu> RangesMenu = new Dictionary<Hero, Menu>();

        private static Menu HeroesMenu;
        public static Menu Menu { get; private set; }
        public static Menu TowersMenu { get; private set; }

        public static void Init() {
            if (Menu != null) {
                Menu.RemoveFromMainMenu();
                RangesMenu.Clear();
            }
            InitMenu();
        }

        public static void AddSpells(Hero hero) {
            var heroName = hero.Name;

            var heroMenu = new Menu(string.Empty, heroName, false, hero.Name);

            RangesMenu.Add(hero, heroMenu);

            foreach (var spell in hero.Spellbook.Spells.Where(x => x.ClassID != ClassID.CDOTA_Ability_AttributeBonus)) {
                var spellName = spell.Name;
                var key = heroName + spellName;

                var spellMenu = new Menu(string.Empty, key, false, spellName);

                spellMenu.AddItem(new MenuItem(key + "enabled", "Enabled")).SetValue(false)
                    .ValueChanged += (sender, arg) => { Drawings.DrawRange(hero, spellName, arg.GetNewValue<bool>()); };

                spellMenu.AddItem(new MenuItem(key + "bonus", "Bonus range").SetValue(new Slider(0, -300, 300)))
                    .ValueChanged +=
                    (sender, arg) => { Drawings.ChangeRange(key, hero, arg.GetNewValue<Slider>().Value); };

                spellMenu.AddItem(new MenuItem(key + "red", "Red").SetValue(new Slider(255, 0, 255)))
                    .SetFontStyle(fontColor: Color.IndianRed)
                    .ValueChanged +=
                    (sender, arg) => { Drawings.ChangeColor(key, arg.GetNewValue<Slider>().Value, Color.Red); };

                spellMenu.AddItem(new MenuItem(key + "green", "Green").SetValue(new Slider(0, 0, 255)))
                    .SetFontStyle(fontColor: Color.LightGreen)
                    .ValueChanged +=
                    (sender, arg) => { Drawings.ChangeColor(key, arg.GetNewValue<Slider>().Value, Color.Green); };

                spellMenu.AddItem(new MenuItem(key + "blue", "Blue").SetValue(new Slider(0, 0, 255)))
                    .SetFontStyle(fontColor: Color.LightBlue)
                    .ValueChanged +=
                    (sender, arg) => { Drawings.ChangeColor(key, arg.GetNewValue<Slider>().Value, Color.Blue); };

                heroMenu.AddSubMenu(spellMenu);

                Main.AbilitiesDictionary.Add(key, Math.Max(spell.Level, 1));
            }

            HeroesMenu.AddSubMenu(heroMenu);
        }

        public static void AddItem(Hero hero, Item item) {
            Menu rangeMenu;
            RangesMenu.TryGetValue(hero, out rangeMenu);

            if (rangeMenu == null)
                return;

            var castRange = item.GetCastRange();
            if (castRange >= 5000) castRange = item.GetRadius();

            var itemName = item.Name.GetDefaultName();
            var key = hero.Name + itemName;

            if (castRange > 0 && castRange <= 5000) {
                var addItem = new Menu(string.Empty, key, false, itemName);

                addItem.AddItem(new MenuItem(key + "enabled", "Enabled"))
                    .SetValue(false)
                    .ValueChanged += (sender, arg) => { Drawings.DrawRange(hero, itemName, arg.GetNewValue<bool>()); };

                addItem.AddItem(
                    new MenuItem(key + "bonus", "Bonus range").SetValue(new Slider(0, -300, 300)))
                    .ValueChanged +=
                    (sender, arg) => { Drawings.ChangeRange(key, hero, arg.GetNewValue<Slider>().Value); };

                addItem.AddItem(new MenuItem(key + "red", "Red").SetValue(new Slider(255, 0, 255)))
                    .SetFontStyle(fontColor: Color.IndianRed)
                    .ValueChanged +=
                    (sender, arg) => { Drawings.ChangeColor(key, arg.GetNewValue<Slider>().Value, Color.Red); };

                addItem.AddItem(new MenuItem(key + "green", "Green").SetValue(new Slider(0, 0, 255)))
                    .SetFontStyle(fontColor: Color.LightGreen)
                    .ValueChanged +=
                    (sender, arg) => { Drawings.ChangeColor(key, arg.GetNewValue<Slider>().Value, Color.Green); };

                addItem.AddItem(new MenuItem(key + "blue", "Blue").SetValue(new Slider(0, 0, 255)))
                    .SetFontStyle(fontColor: Color.LightBlue)
                    .ValueChanged +=
                    (sender, arg) => { Drawings.ChangeColor(key, arg.GetNewValue<Slider>().Value, Color.Blue); };

                rangeMenu.AddSubMenu(addItem);

                Main.ItemsDictionary.Add(key, item.Level);
            }

            Main.ItemsSet.Add(key);
        }

        public static void AddCustomItem(Hero hero, string texture, float range, string tooltip = null) {
            Menu rangeMenu;
            RangesMenu.TryGetValue(hero, out rangeMenu);

            if (rangeMenu == null)
                return;

            var key = hero.Name + texture;

            var addItem = new Menu(string.Empty, key, false, texture);

            addItem.AddItem(new MenuItem(key + "enabled", "Enabled"))
                .SetValue(false)
                .SetTooltip(tooltip)
                .ValueChanged +=
                (sender, arg) => { Drawings.DrawRange(hero, texture, arg.GetNewValue<bool>(), customRange: true); };

            addItem.AddItem(
                new MenuItem(key + "bonus", "Bonus range").SetValue(new Slider(0, -300, 300)))
                .ValueChanged +=
                (sender, arg) => { Drawings.ChangeRange(key, hero, arg.GetNewValue<Slider>().Value, true); };

            addItem.AddItem(new MenuItem(key + "red", "Red").SetValue(new Slider(255, 0, 255)))
                .SetFontStyle(fontColor: Color.IndianRed)
                .ValueChanged +=
                (sender, arg) => { Drawings.ChangeColor(key, arg.GetNewValue<Slider>().Value, Color.Red); };

            addItem.AddItem(new MenuItem(key + "green", "Green").SetValue(new Slider(0, 0, 255)))
                .SetFontStyle(fontColor: Color.LightGreen)
                .ValueChanged +=
                (sender, arg) => { Drawings.ChangeColor(key, arg.GetNewValue<Slider>().Value, Color.Green); };

            addItem.AddItem(new MenuItem(key + "blue", "Blue").SetValue(new Slider(0, 0, 255)))
                .SetFontStyle(fontColor: Color.LightBlue)
                .ValueChanged +=
                (sender, arg) => { Drawings.ChangeColor(key, arg.GetNewValue<Slider>().Value, Color.Blue); };

            rangeMenu.AddSubMenu(addItem);

            Main.CustomRangesDictionary.Add(key, range);
        }

        private static void InitMenu() {
            Menu = new Menu("Advanced Ranges", "advancedRanges", true);
            HeroesMenu = new Menu("Heroes", "rangeHeroes");
            TowersMenu = new Menu("Towers", "rangeTowers");

            TowersMenu.AddItem(new MenuItem("towersNightRange", "Night change"))
                .SetValue(true)
                .SetTooltip("Change vision range at night");

            Menu.AddSubMenu(HeroesMenu);

            var allyTowers = new Menu("Ally", "ally");
            var enemyTowers = new Menu("Enemy", "enemy");

            var team = Main.Hero.Team;
            var eteam = Main.Hero.GetEnemyTeam();

            for (var i = 1; i <= 4; i++) {
                var index = i - 1;
                var tower = i;

                var allyT = new Menu("T" + i, "allyT" + i);
                var enemyT = new Menu("T" + i, "enemyT" + i);

                allyT.AddItem(new MenuItem("allyTowersT" + i, "Enabled")).SetValue(true).ValueChanged +=
                    (sender, arg) => {
                        Drawings.DrawTowerRange(team, arg.GetNewValue<bool>(), Drawings.TowerLocations[index],
                            "T" + tower);
                    };

                allyT.AddItem(new MenuItem("allyTowersRT" + i, "Red").SetValue(new Slider(0, 0, 255)))
                    .SetFontStyle(fontColor: Color.IndianRed)
                    .ValueChanged +=
                    (sender, arg) => {
                        Drawings.ChangeColor(team, arg.GetNewValue<Slider>().Value, Color.Red,
                            Drawings.TowerLocations[index], "T" + tower);
                    };

                allyT.AddItem(new MenuItem("allyTowersGT" + i, "Green").SetValue(new Slider(255, 0, 255)))
                    .SetFontStyle(fontColor: Color.LightGreen)
                    .ValueChanged +=
                    (sender, arg) => {
                        Drawings.ChangeColor(team, arg.GetNewValue<Slider>().Value, Color.Green,
                            Drawings.TowerLocations[index], "T" + tower);
                    };

                allyT.AddItem(new MenuItem("allyTowersBT" + i, "Blue").SetValue(new Slider(0, 0, 255)))
                    .SetFontStyle(fontColor: Color.LightBlue)
                    .ValueChanged +=
                    (sender, arg) => {
                        Drawings.ChangeColor(team, arg.GetNewValue<Slider>().Value, Color.Blue,
                            Drawings.TowerLocations[index], "T" + tower);
                    };

                enemyT.AddItem(new MenuItem("enemyTowersT" + i, "Enabled")).SetValue(true).ValueChanged +=
                    (sender, arg) => {
                        Drawings.DrawTowerRange(eteam, arg.GetNewValue<bool>(), Drawings.TowerLocations[index],
                            "T" + tower);
                    };

                enemyT.AddItem(new MenuItem("enemyTowersRT" + i, "Red").SetValue(new Slider(255, 0, 255)))
                    .SetFontStyle(fontColor: Color.IndianRed)
                    .ValueChanged +=
                    (sender, arg) => {
                        Drawings.ChangeColor(eteam, arg.GetNewValue<Slider>().Value, Color.Red,
                            Drawings.TowerLocations[index], "T" + tower);
                    };

                enemyT.AddItem(new MenuItem("enemyTowersGT" + i, "Green").SetValue(new Slider(0, 0, 255)))
                    .SetFontStyle(fontColor: Color.LightGreen)
                    .ValueChanged +=
                    (sender, arg) => {
                        Drawings.ChangeColor(eteam, arg.GetNewValue<Slider>().Value, Color.Green,
                            Drawings.TowerLocations[index], "T" + tower);
                    };

                enemyT.AddItem(new MenuItem("enemyTowersBT" + i, "Blue").SetValue(new Slider(0, 0, 255)))
                    .SetFontStyle(fontColor: Color.LightBlue)
                    .ValueChanged +=
                    (sender, arg) => {
                        Drawings.ChangeColor(eteam, arg.GetNewValue<Slider>().Value, Color.Blue,
                            Drawings.TowerLocations[index], "T" + tower);
                    };

                allyTowers.AddSubMenu(allyT);
                enemyTowers.AddSubMenu(enemyT);
            }

            TowersMenu.AddSubMenu(allyTowers);
            TowersMenu.AddSubMenu(enemyTowers);

            Menu.AddSubMenu(TowersMenu);
            Menu.AddToMainMenu();
        }
    }
}