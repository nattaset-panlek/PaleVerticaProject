namespace CoreSystem.Entity
{
    public enum CharacterControllerType
    {
        None,
        Player,
        Agent
    }

    public enum ControlLane
    {
        Owner = 10,
        Scripted = 30,
        Possession = 50,
        Dev = 100
    }

    [System.Flags]
    public enum CharacterTag
    {
        None = 0,
        Villager = 1 << 0,
        Monster = 1 << 1,
        Boss = 1 << 2,
        Merchant = 1 << 3,
        TrainingDummy = 1 << 4,
        ImmuneToControl = 1 << 5
    }
}
