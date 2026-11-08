using CoreSystem.Skills;

namespace CoreSystem.Entity.UniquePassives
{
    public interface ISkillCastRepeater : ICastRepeater
    {
        bool CanRepeat(SkillInstance instance);
    }
}
