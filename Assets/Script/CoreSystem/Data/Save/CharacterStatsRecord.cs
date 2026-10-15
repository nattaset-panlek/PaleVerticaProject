using System;
using System.Collections.Generic;
using UnityEngine;
using CoreSystem.Entity;

namespace CoreSystem.Data.Save
{
    [Serializable]
    public class CharacterStatsRecord
    {
        public int BaseLevel = 1;
        public int JobLevel = 1;
        public long CurrentBaseExp = 0;
        public long CurrentJobExp = 0;
        public float BaseSTR = 1;
        public float BaseAGI = 1;
        public float BaseVIT = 1;
        public float BaseINT = 1;
        public float BaseDEX = 1;
        public float BaseLUK = 1;
        public int StatPoints = 0;
        public int SkillPoints = 0;
        public int PassivePoints = 0;
    }
}
