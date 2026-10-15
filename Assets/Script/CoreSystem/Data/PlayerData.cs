using CoreSystem.Data.Items;
using CoreSystem.Data.Items.Model;
using CoreSystem.Skills;
using System;
using System.Collections.Generic;



namespace CoreSystem.Data
{
    public class PlayerData
    {
        public Entity.PlayerCharacter PlayerCharacterModel { get; set; }
        public Entity.PlayerCharacter PlayerCharacter => PlayerCharacterModel;
        public Save.PlayerDataModel Data { get; set; } = new Save.PlayerDataModel();

        public PlayerInventory Inventory { get; } = new PlayerInventory();
        public CharacterManager CharacterManager { get; } = new CharacterManager();
        public EquipmentManager EquipmentManager { get; } = new EquipmentManager();
        public CostumeManager CostumeManager { get; } = new CostumeManager();
        public SkillManager SkillManager { get; } = new SkillManager();
    }
}
