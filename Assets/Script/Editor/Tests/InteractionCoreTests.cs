using System.Collections.Generic;
using CoreSystem.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace CoreSystem.Tests
{
    public class InteractionCoreTests
    {
        #region [Variables/Fields]
        private sealed class ListProvider : IInteractionProvider
        {
            public readonly List<InteractionOption> Options = new List<InteractionOption>();
            public int Calls;

            public void Collect(InteractionContext context, List<InteractionOption> options)
            {
                Calls++;
                options.AddRange(Options);
            }
        }

        private sealed class ThrowingProvider : IInteractionProvider
        {
            public void Collect(InteractionContext context, List<InteractionOption> options)
            {
                options.Add(Option("broken"));
                throw new System.InvalidOperationException("provider failure");
            }
        }
        #endregion

        #region [Unity Methods]
        [SetUp]
        public void SetUp()
        {
            InteractionRegistry.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            InteractionRegistry.Clear();
        }
        #endregion

        #region [Public Methods]
        [Test]
        public void Register_IgnoresDuplicatesAndNull()
        {
            var provider = new ListProvider();
            InteractionRegistry.Register(provider);
            InteractionRegistry.Register(provider);
            InteractionRegistry.Register(null);

            Assert.AreEqual(1, InteractionRegistry.Count);

            InteractionRegistry.Unregister(provider);
            Assert.AreEqual(0, InteractionRegistry.Count);
        }

        [Test]
        public void Collect_SortsByPriority_KeepsOrderForTies()
        {
            var provider = new ListProvider();
            provider.Options.Add(Option("low", -10));
            provider.Options.Add(Option("a"));
            provider.Options.Add(Option("high", 50));
            provider.Options.Add(Option("b"));
            InteractionRegistry.Register(provider);

            var ids = Ids(InteractionRegistry.Collect(default));

            CollectionAssert.AreEqual(new[] { "high", "a", "b", "low" }, ids);
        }

        [Test]
        public void Collect_FirstProviderWinsDuplicateIds()
        {
            var first = new ListProvider();
            first.Options.Add(new InteractionOption("talk", "First", _ => { }));
            var second = new ListProvider();
            second.Options.Add(new InteractionOption("talk", "Second", _ => { }));
            InteractionRegistry.Register(first);
            InteractionRegistry.Register(second);

            var options = InteractionRegistry.Collect(default);

            Assert.AreEqual(1, options.Count);
            Assert.AreEqual("First", options[0].Label);
        }

        [Test]
        public void Collect_SkipsInvalidOptions()
        {
            var provider = new ListProvider();
            provider.Options.Add(null);
            provider.Options.Add(new InteractionOption("noop", "No Action", null));
            provider.Options.Add(new InteractionOption("", "No Id", _ => { }));
            provider.Options.Add(Option("ok"));
            InteractionRegistry.Register(provider);

            CollectionAssert.AreEqual(new[] { "ok" }, Ids(InteractionRegistry.Collect(default)));
        }

        [Test]
        public void Collect_ProviderExceptionDoesNotBreakOthers()
        {
            var good = new ListProvider();
            good.Options.Add(Option("ok"));
            InteractionRegistry.Register(new ThrowingProvider());
            InteractionRegistry.Register(good);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("provider failure"));
            var ids = Ids(InteractionRegistry.Collect(default));

            CollectionAssert.AreEqual(new[] { "ok" }, ids);
        }

        [Test]
        public void Option_NegativeRangeClampsToAnyRange_EmptyLabelUsesId()
        {
            var option = new InteractionOption("trade", null, _ => { }, -3f);

            Assert.AreEqual(InteractionOption.AnyRange, option.Range);
            Assert.IsFalse(option.RequiresRange);
            Assert.AreEqual("trade", option.Label);
        }

        [Test]
        public void IsInRange_AnyRangeAlwaysTrue_EdgeIsInside()
        {
            Assert.IsTrue(InteractionRequest.IsInRange(Vector2.zero, new Vector2(999f, 0f), InteractionOption.AnyRange));
            Assert.IsTrue(InteractionRequest.IsInRange(Vector2.zero, new Vector2(3f, 4f), 5f));
            Assert.IsFalse(InteractionRequest.IsInRange(Vector2.zero, new Vector2(3f, 4.01f), 5f));
        }

        [Test]
        public void Step_ApproachesThenExecutes()
        {
            var request = new InteractionRequest(default, Option("talk", 0, 1.5f));

            Assert.AreEqual(InteractionStep.Approach, request.Step(Vector2.zero, new Vector2(5f, 0f), true, true, 0.1f));
            Assert.AreEqual(InteractionStep.Execute, request.Step(new Vector2(4f, 0f), new Vector2(5f, 0f), true, true, 0.1f));
        }

        [Test]
        public void Step_CancelsOnInvalidTarget()
        {
            var request = new InteractionRequest(default, Option("talk", 0, 1.5f));

            Assert.AreEqual(InteractionStep.Cancel, request.Step(Vector2.zero, Vector2.zero, false, true, 0.1f));
        }

        [Test]
        public void Step_CancelsWhenControlLostOutOfRange_ButExecutesInRange()
        {
            var request = new InteractionRequest(default, Option("talk", 0, 1.5f));

            Assert.AreEqual(InteractionStep.Execute, request.Step(Vector2.zero, Vector2.one, true, false, 0.1f));
            Assert.AreEqual(InteractionStep.Cancel, request.Step(Vector2.zero, new Vector2(5f, 0f), true, false, 0.1f));
        }

        [Test]
        public void Step_TimesOut()
        {
            var request = new InteractionRequest(default, Option("talk", 0, 1f), 0.5f);

            Assert.AreEqual(InteractionStep.Approach, request.Step(Vector2.zero, new Vector2(5f, 0f), true, true, 0.4f));
            Assert.AreEqual(InteractionStep.Cancel, request.Step(Vector2.zero, new Vector2(5f, 0f), true, true, 0.2f));
        }
        #endregion

        #region [Private Methods]
        private static InteractionOption Option(string id, int priority = 0, float range = 0f)
        {
            return new InteractionOption(id, id, _ => { }, range, priority);
        }

        private static List<string> Ids(List<InteractionOption> options)
        {
            var ids = new List<string>();
            for (int i = 0; i < options.Count; i++) ids.Add(options[i].Id);
            return ids;
        }
        #endregion
    }
}
