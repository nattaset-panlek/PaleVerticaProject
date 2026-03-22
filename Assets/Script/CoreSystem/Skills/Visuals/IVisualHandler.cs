namespace CoreSystem.Skills.Visuals
{
    public interface IVisualHandler
    {
        void Execute(VisualPayload data, SkillPayload context);
    }
}
