using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Wokarol.GameSystemsLocator.Tests
{
    public class GameSystemsTests : GameSystemsTestsBase
    {
        [UnityTest]
        public IEnumerator Cleared_ContainsNoBindings()
        {
            var systems = GameSystems.Systems;

            Assert.That(systems, Is.Empty);
            yield break;
        }

        [UnityTest]
        public IEnumerator Cleared_WhenTryingToLocate_Throws()
        {
            TestDelegate action = () => GameSystems.Get<Foo>();

            Assert.That(action, Throws.Exception.TypeOf<InvalidOperationException>());
            yield break;
        }

        [UnityTest]
        public IEnumerator Initialized_BoundSystem_ReturnsNull()
        {
            var systemsObject = new GameObject("Systems");

            GameSystems.Initialize(systemsObject, s =>
            {
                s.Add<Foo>();
            });
            var foundFoo = GameSystems.Get<Foo>();

            Assert.That(foundFoo, Is.Null);
            yield break;
        }

        [UnityTest]
        public IEnumerator Initialized_LocatesBoundSingleton()
        {
            var systemsObject = new GameObject("Systems");
            var foo = AddTestSystem<Foo>(systemsObject);

            GameSystems.Initialize(systemsObject, s =>
            {
                s.Add<Foo>();
            });
            var foundFoo = GameSystems.Get<Foo>();

            Assert.That(foundFoo, Is.EqualTo(foo));
            yield break;
        }

        [UnityTest]
        public IEnumerator Initialized_LocatesBoundSingleton_AsInterface()
        {
            var systemsObject = new GameObject("Systems");
            var bar = AddTestSystem<Bar>(systemsObject);

            GameSystems.Initialize(systemsObject, s =>
            {
                s.Add<IBax>();
            });
            var foundBax = GameSystems.Get<IBax>();

            Assert.That(foundBax, Is.EqualTo(bar));
            yield break;
        }

        [UnityTest]
        public IEnumerator Initialized_ReturnsNullObject_WhenDefined()
        {
            var systemsObject = new GameObject("Systems");

            var nullBax = new NullBax();

            GameSystems.Initialize(systemsObject, s =>
            {
                s.Add<IBax>(nullObject: nullBax);
            });
            var foundBax = GameSystems.Get<IBax>();

            Assert.That(foundBax, Is.EqualTo(nullBax));
            yield break;
        }

        [UnityTest]
        public IEnumerator Initialized_ReturnsInstance_EvenIf_NullObjectWhenDefined()
        {
            var systemsObject = new GameObject("Systems");
            var bar = AddTestSystem<Bar>(systemsObject);

            var nullBax = new NullBax();

            GameSystems.Initialize(systemsObject, s =>
            {
                s.Add<IBax>(nullObject: nullBax);
            });
            var foundBax = GameSystems.Get<IBax>();

            Assert.That(foundBax, Is.EqualTo(bar));
            yield break;
        }

        [UnityTest]
        public IEnumerator Initialized_LocatesOnlyCreatedSingletons()
        {
            var systemsObject = new GameObject("Systems");
            var foo = AddTestSystem<Foo>(systemsObject);

            GameSystems.Initialize(systemsObject, s =>
            {
                s.Add<Foo>();
                s.Add<Bar>();
            });
            var foundFoo = GameSystems.Get<Bar>();

            Assert.That(foundFoo, Is.Null);
            yield break;
        }

        [UnityTest]
        public IEnumerator Invalid_CannotAddSingletonTwice()
        {
            var systemsObject = new GameObject("Systems");

            TestDelegate action = () => GameSystems.Initialize(systemsObject, s =>
            {
                s.Add<Foo>();
                s.Add<Foo>();
            });

            Assert.That(action, Throws.Exception.TypeOf<InvalidOperationException>());
            yield break;
        }
    }

    public class GameSystemsTestLogging : GameSystemsTestsBase
    {
        readonly TestLogHandler logger = new();
        private ILogHandler previousLogHandler;

        [SetUp]
        public void SetupLogger()
        {
            logger.Clear();
        }

        [OneTimeSetUp]
        public void OneTimeSetupLogger()
        {
            var unityLogger = Debug.unityLogger;
            previousLogHandler = unityLogger.logHandler;
            unityLogger.logHandler = logger;
        }

        [OneTimeTearDown]
        public void OneTimeTeardownLogger()
        {
            Debug.unityLogger.logHandler = previousLogHandler;
        }


        [UnityTest]
        public IEnumerator Invalid_RequiredSingletons_HaveToBePresent()
        {
            var systemsObject = new GameObject("Systems");

            GameSystems.Initialize(systemsObject, s =>
            {
                s.Add<Foo>(required: true);
            });

            Assert.That(logger.Errors, Has.Count.EqualTo(1));
            Assert.That(logger.Errors[0], Does.Match(".* binding.* required.*"));
            yield break;
        }
    }

    public class GameSystemsTestOverrides : GameSystemsTestsBase
    {
        private Bar baseBar;

        [SetUp]
        public void SetupBaseSystems()
        {
            var systemsObject = new GameObject("Systems");
            baseBar = AddTestSystem<Bar>(systemsObject);

            GameSystems.Initialize(systemsObject, s =>
            {
                s.Add<IBax>();
            });
        }

        [Test]
        public void NotOverriten_ReturnsBase()
        {
            var bax = GameSystems.Get<IBax>();

            Assert.That(bax, Is.EqualTo(baseBar));
        }


        [Test]
        public void Overriten_ReturnsOverride()
        {
            var holder = new GameObject("Better Systems");
            var betterBar = AddTestSystem<BetterBar>(holder);

            GameSystems.ApplyOverride(holder);
            var bax = GameSystems.Get<IBax>();

            Assert.That(bax, Is.EqualTo(betterBar));
        }

        [Test]
        public void OverritenTwice_ReturnsLastOverride()
        {
            var holder1 = new GameObject("Better Systems 1");
            var holder2 = new GameObject("Better Systems 2");
            var betterBar1 = AddTestSystem<BetterBar>(holder1);
            var betterBar2 = AddTestSystem<BetterBar>(holder2);

            GameSystems.ApplyOverride(holder1);
            GameSystems.ApplyOverride(holder2);
            var bax = GameSystems.Get<IBax>();

            Assert.That(bax, Is.EqualTo(betterBar2));
        }

        [Test]
        public void Overriten_AndRemoved_ReturnsBase()
        {
            var holder = new GameObject("Better Systems");
            var betterBar = AddTestSystem<BetterBar>(holder);

            GameSystems.ApplyOverride(holder);
            GameSystems.RemoveOverride(holder);

            var bax = GameSystems.Get<IBax>();

            Assert.That(bax, Is.EqualTo(baseBar));
        }

        [Test]
        public void Overriten_AndRemoved_Repeated_ReturnsOverride()
        {
            var holder = new GameObject("Better Systems");
            var betterBar = AddTestSystem<BetterBar>(holder);

            GameSystems.ApplyOverride(holder);
            GameSystems.RemoveOverride(holder);
            GameSystems.ApplyOverride(holder);

            var bax = GameSystems.Get<IBax>();

            Assert.That(bax, Is.EqualTo(betterBar));
        }

        [Test]
        public void OverritenTwice_AndRemoved_ReturnsBase()
        {
            var holder1 = new GameObject("Better Systems 1");
            var holder2 = new GameObject("Better Systems 2");
            var betterBar1 = AddTestSystem<BetterBar>(holder1);
            var betterBar2 = AddTestSystem<BetterBar>(holder2);

            GameSystems.ApplyOverride(holder1);
            GameSystems.ApplyOverride(holder2);

            GameSystems.RemoveOverride(holder2);
            GameSystems.RemoveOverride(holder1);

            var bax = GameSystems.Get<IBax>();

            Assert.That(bax, Is.EqualTo(baseBar));
        }

        [Test]
        public void OverritenTwice_AndRemoved_OutOfOrder_ReturnsBase()
        {
            var holder1 = new GameObject("Better Systems 1");
            var holder2 = new GameObject("Better Systems 2");
            var betterBar1 = AddTestSystem<BetterBar>(holder1);
            var betterBar2 = AddTestSystem<BetterBar>(holder2);

            GameSystems.ApplyOverride(holder1);
            GameSystems.ApplyOverride(holder2);

            GameSystems.RemoveOverride(holder1);
            GameSystems.RemoveOverride(holder2);

            var bax = GameSystems.Get<IBax>();

            Assert.That(bax, Is.EqualTo(baseBar));
        }

        [Test]
        public void OverritenTwice_AndRemoved_Middle_ReturnsLates()
        {
            var holder1 = new GameObject("Better Systems 1");
            var holder2 = new GameObject("Better Systems 2");
            var betterBar1 = AddTestSystem<BetterBar>(holder1);
            var betterBar2 = AddTestSystem<BetterBar>(holder2);

            GameSystems.ApplyOverride(holder1);
            GameSystems.ApplyOverride(holder2);

            GameSystems.RemoveOverride(holder1);

            var bax = GameSystems.Get<IBax>();

            Assert.That(bax, Is.EqualTo(betterBar2));
        }

        [Test]
        public void OverritenTwice_AndRemoved_Second_ReturnsFirst()
        {
            var holder1 = new GameObject("Better Systems 1");
            var holder2 = new GameObject("Better Systems 2");
            var betterBar1 = AddTestSystem<BetterBar>(holder1);
            var betterBar2 = AddTestSystem<BetterBar>(holder2);

            GameSystems.ApplyOverride(holder1);
            GameSystems.ApplyOverride(holder2);

            GameSystems.RemoveOverride(holder2);

            var bax = GameSystems.Get<IBax>();

            Assert.That(bax, Is.EqualTo(betterBar1));
        }
    }


    internal interface IBax { }
    internal class Bar : MonoBehaviour, IBax { }
    internal class BetterBar : MonoBehaviour, IBax { }
    internal class NullBax : IBax { }
    internal class Foo : MonoBehaviour { }

    public class GameSystemsTestsBase
    {
        [SetUp]
        public void Setup()
        {
            GameSystems.Clear();
        }

        protected static T AddTestSystem<T>(GameObject systemsObject) where T : Component
        {
            var foo = new GameObject("Foo").AddComponent<T>();
            foo.transform.SetParent(systemsObject.transform);
            return foo;
        }
    }
}