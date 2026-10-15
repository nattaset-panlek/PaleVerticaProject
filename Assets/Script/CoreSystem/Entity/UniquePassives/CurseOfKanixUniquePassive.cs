namespace CoreSystem.Entity.UniquePassives
{
    [UniquePassiveId("CurseOfKanix")]
    public class CurseOfKanixUniquePassive : UniquePassive
    {
        #region [Public Methods]
        public override void ModifyCoreAttributes(ref float str, ref float agi, ref float vit, ref float @int, ref float dex, ref float luk)
        {
            float tempInt = @int;
            @int = str;
            str = tempInt;

            float tempDex = dex;
            dex = vit;
            vit = tempDex;

            float tempLuk = luk;
            luk = agi;
            agi = tempLuk;
        }
        #endregion
    }
}
