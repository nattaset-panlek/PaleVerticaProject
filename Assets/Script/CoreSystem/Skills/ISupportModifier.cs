namespace CoreSystem.Skills
{
    public interface ISupportModifier
    {
        void ModifyPayload(ref SkillPayload payload);
    }
}
