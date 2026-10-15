using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using System;
using System.Collections.Generic;



namespace CoreSystem.Data
{
    public class PlayerData
    {
        public Save.PlayerDataModel Data { get; set; } = new Save.PlayerDataModel();

        public PlayerInventory Inventory { get; } = new PlayerInventory();
        public PlayerStats Stats { get; } = new PlayerStats();
        public PlayerEquipment Equipment { get; } = new PlayerEquipment();
        public PlayerCostumes Costumes { get; } = new PlayerCostumes();
        public PlayerSkills Skills { get; } = new PlayerSkills();
    }
}
