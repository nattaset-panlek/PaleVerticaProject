using CoreSystem.Player;
using CoreSystem.UI.Effects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace CoreSystem.Tests
{
    public class DeathScreenTests
    {
        #region [Public Methods]
        [Test]
        public void InkPalette_MatchesTheFigmaStops()
        {
            var ink = FogAuraPalette.Ink;
            Assert.AreEqual(Color.black, ink.Evaluate(0f));
            AssertColor(new Color32(0x8E, 0x14, 0x18, 0xFF), ink.Evaluate(0.65f));
            AssertColor(new Color32(0xFF, 0x5A, 0x4E, 0xFF), ink.Evaluate(1f));
            AssertColor(new Color32(0xFF, 0x5A, 0x4E, 0xFF), ink.Evaluate(2f));
        }

        [Test]
        public void Palette_WritesTheGradientMapToTheMaterial()
        {
            var material = new Material(Shader.Find(FogAuraView.ShaderName));
            try
            {
                FogAuraPalette.Ink.ApplyTo(material);
                AssertColor(FogAuraPalette.Ink.Colors[3], material.GetColor("_RampColor3"));
                Assert.AreEqual(new Vector4(0.15f, 0.4f, 0.65f, 0f), material.GetVector("_RampStops"));
            }
            finally
            {
                Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void InkMaterial_LoadsFromResources()
        {
            var material = FogAuraView.LoadMaterial(PlayerDeathScreen.FogMaterialPath);
            Assert.IsNotNull(material);
            Assert.AreEqual(FogAuraView.ShaderName, material.shader.name);
        }

        [Test]
        public void DeathInfo_FillsMissingValues()
        {
            var info = new PlayerDeathInfo(null, 0, "", null);
            Assert.AreEqual("Unknown", info.CharacterName);
            Assert.AreEqual("Unknown", info.Epitaph);
            Assert.AreEqual("Unknown", info.Location);
            Assert.AreEqual("Unknown", info.KillerName);
            Assert.AreEqual("Axnise, Lv. 24", PlayerDeathInfo.Sample.Epitaph);
        }

        [Test]
        public void FogShader_IsIncluded()
        {
            Assert.IsNotNull(Shader.Find(FogAuraView.ShaderName));
        }

        [Test]
        public void FogAura_StretchesBehindItsTarget()
        {
            var root = new GameObject("Root", typeof(RectTransform));
            try
            {
                var window = new GameObject("Window", typeof(RectTransform));
                window.transform.SetParent(root.transform, false);
                var source = FogAuraView.LoadMaterial(PlayerDeathScreen.FogMaterialPath);
                var fog = FogAuraView.Create((RectTransform)window.transform, source, false);

                Assert.IsNotNull(fog);
                Assert.AreEqual(FogAuraView.ObjectName, fog.name);
                Assert.AreEqual(0, fog.transform.GetSiblingIndex());
                Assert.AreEqual(1, window.transform.GetSiblingIndex());
                var rect = (RectTransform)fog.transform;
                Assert.AreEqual(Vector2.zero, rect.anchorMin);
                Assert.AreEqual(Vector2.one, rect.anchorMax);
                Assert.IsFalse(fog.GetComponent<RawImage>().raycastTarget);
                Assert.AreEqual(1f, fog.Material.GetFloat("_Fade"));
                Assert.AreNotSame(source, fog.Material);
                Assert.Greater(fog.Material.GetFloat("_StartTime"), 0f);
                AssertColor(source.GetColor("_RampColor4"), fog.Material.GetColor("_RampColor4"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
        #endregion

        #region [Private Methods]
        private static void AssertColor(Color expected, Color actual)
        {
            Assert.AreEqual(expected.r, actual.r, 0.01f);
            Assert.AreEqual(expected.g, actual.g, 0.01f);
            Assert.AreEqual(expected.b, actual.b, 0.01f);
        }
        #endregion
    }
}
