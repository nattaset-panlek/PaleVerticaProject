using System.Collections.Generic;
using CoreSystem.Loot;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class LootLabelLayoutTests
    {
        #region [Variables/Fields]
        private const float Gap = 2f;
        private static readonly Vector2 Size = new Vector2(100f, 20f);
        #endregion

        #region [Properties]
        #endregion

        #region [Unity Methods]
        #endregion

        #region [Public Methods]
        [Test]
        public void Resolve_KeepsLabelsThatDoNotOverlap()
        {
            var desired = new List<Vector2> { new Vector2(0f, 0f), new Vector2(300f, 0f) };
            var results = Run(desired);
            Assert.AreEqual(desired[0], results[0]);
            Assert.AreEqual(desired[1], results[1]);
        }

        [Test]
        public void Resolve_StacksIdenticalPositions()
        {
            var results = Run(new List<Vector2> { Vector2.zero, Vector2.zero, Vector2.zero });
            Assert.AreEqual(0f, results[0].y, 0.01f);
            Assert.AreEqual(22f, results[1].y, 0.01f);
            Assert.AreEqual(44f, results[2].y, 0.01f);
        }

        [Test]
        public void Resolve_NoTwoLabelsOverlapInACluster()
        {
            var desired = new List<Vector2>();
            for (int i = 0; i < 30; i++) desired.Add(new Vector2((i * 37) % 90, (i * 13) % 40));
            var results = Run(desired);

            for (int a = 0; a < results.Count; a++)
            {
                for (int b = a + 1; b < results.Count; b++)
                {
                    bool apart = Mathf.Abs(results[a].x - results[b].x) >= Size.x
                                 || Mathf.Abs(results[a].y - results[b].y) >= Size.y;
                    Assert.IsTrue(apart, $"labels {a} and {b} overlap");
                }
            }
        }

        [Test]
        public void Resolve_EmptyInputGivesEmptyResult()
        {
            Assert.AreEqual(0, Run(new List<Vector2>()).Count);
        }
        #endregion

        #region [Private Methods]
        private static List<Vector2> Run(List<Vector2> desired)
        {
            var sizes = new List<Vector2>();
            foreach (Vector2 _ in desired) sizes.Add(Size);
            var results = new List<Vector2>();
            LootLabelLayout.Resolve(desired, sizes, Gap, results);
            return results;
        }
        #endregion
    }
}
