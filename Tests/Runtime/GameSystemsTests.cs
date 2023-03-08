using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Wokarol.GameSystemsLocator.Core;

namespace Wokarol.GameSystemsLocator.Tests
{
    public class GameSystemsTests : GameSystemsTestsBase
    {
        [Test]
        public void Cleared_ContainsNoBindings()
        {
            var systems = locator.Systems;

            Assert.That(systems, Is.Empty);
        }

        [Test]
        public void Cleared_WhenTryingToLocate_Throws()
        {
            TestDelegate action = () => locator.Get<Foo>();

            Assert.That(action, 
                Throws.Exception.TypeOf<InvalidOperationException>()
                .And.Message.Matches("not.*initialized"));
        }

        [Test]
        public void Initialized_WithRoot_BoundSystem_ReturnsNull()
        {
            var systemsObject = new GameObject("Systems");

            locator.Initialize(s =>
            {
                s.Add<Foo>();
            }, systemsObject);
            var foundFoo = locator.Get<Foo>();

            Assert.That(foundFoo, Is.Null);
        }

        [Test]
        public void Initialized_WithRoot_BoundSystem_WithTry_ReturnsFalse()
        {
            var systemsObject = new GameObject("Systems");

            locator.Initialize(s =>
            {
                s.Add<Foo>();
            }, systemsObject);
            if (locator.TryGet(out Foo foundFoo))
            {
                Assert.Fail("Try Get returned true when the system should not be found");
            }
            else
            {
                Assert.That(foundFoo, Is.Null);
            }
        }

        [Test]
        public void Initialized_WithRoot_BoundSystem_WithTry_NotGeneric_ReturnsFalse()
        {
            var systemsObject = new GameObject("Systems");

            locator.Initialize(s =>
            {
                s.Add<Foo>();
            }, systemsObject);
            if (locator.TryGet(typeof(Foo), out var foundFoo))
            {
                Assert.Fail("Try Get returned true when the system should not be found");
            }
            else
            {
                Assert.That(foundFoo, Is.Null);
            }
        }

        [Test]
        public void Initialized_WithRoot_LocatesBoundSingleton()
        {
            var systemsObject = new GameObject("Systems");
            var foo = AddTestSystem<Foo>(systemsObject);

            locator.Initialize(s =>
            {
                s.Add<Foo>();
            }, systemsObject);
            var foundFoo = locator.Get<Foo>();

            Assert.That(foundFoo, Is.EqualTo(foo));
        }

        [Test]
        public void Initialized_WithRootFactoryMethod_LocatesBoundSingleton()
        {
            Foo foo = null;

            locator.Initialize(
            s =>
            {
                s.Add<Foo>();
            }, 
            b =>
            {
                var systemsObject = new GameObject("Systems");
                foo = AddTestSystem<Foo>(systemsObject);
                return systemsObject;
            });

            var foundFoo = locator.Get<Foo>();

            Assert.That(foundFoo, Is.EqualTo(foo));
        }

        [Test]
        public void Initialized_WithRoot_LocatesBoundSingleton_WithTry()
        {
            var systemsObject = new GameObject("Systems");
            var foo = AddTestSystem<Foo>(systemsObject);

            locator.Initialize(s =>
            {
                s.Add<Foo>();
            }, systemsObject);

            if (locator.TryGet(out Foo foundFoo))
            {
                Assert.That(foundFoo, Is.EqualTo(foo));
            }
            else
            {
                Assert.Fail("Try Get returned false when the system should be found");
            }
        }

        [Test]
        public void Initialized_WithRoot_LocatesBoundSingleton_NonGeneric()
        {
            var systemsObject = new GameObject("Systems");
            var foo = AddTestSystem<Foo>(systemsObject);

            locator.Initialize(s =>
            {
                s.Add<Foo>();
            }, systemsObject);
            var foundFoo = locator.Get(typeof(Foo));

            Assert.That(foundFoo, Is.EqualTo(foo));
        }

        [Test]
        public void Initialized_WithRoot_LocatesBoundSingleton_AsInterface()
        {
            var systemsObject = new GameObject("Systems");
            var bar = AddTestSystem<Bar>(systemsObject);

            locator.Initialize(s =>
            {
                s.Add<IBax>();
            }, systemsObject);
            var foundBax = locator.Get<IBax>();

            Assert.That(foundBax, Is.EqualTo(bar));
        }

        [Test]
        public void Initialized_WithRoot_ReturnsNullObject_WhenDefined()
        {
            var systemsObject = new GameObject("Systems");

            var nullBax = new NullBax();

            locator.Initialize(s =>
            {
                s.Add<IBax>(nullObject: nullBax);
            }, systemsObject);
            var foundBax = locator.Get<IBax>();

            Assert.That(foundBax, Is.EqualTo(nullBax));
        }

        [Test]
        public void Initialized_WithRoot_ReturnsInstance_EvenIf_NullObjectWhenDefined()
        {
            var systemsObject = new GameObject("Systems");
            var bar = AddTestSystem<Bar>(systemsObject);

            var nullBax = new NullBax();

            locator.Initialize(s =>
            {
                s.Add<IBax>(nullObject: nullBax);
            }, systemsObject);
            var foundBax = locator.Get<IBax>();

            Assert.That(foundBax, Is.EqualTo(bar));
        }

        [Test]
        public void Initialized_WithRoot_LocatesOnlyCreatedSingletons()
        {
            var systemsObject = new GameObject("Systems");
            var foo = AddTestSystem<Foo>(systemsObject);

            locator.Initialize(s =>
            {
                s.Add<Foo>();
                s.Add<Bar>();
            }, systemsObject);
            var foundFoo = locator.Get<Bar>();

            Assert.That(foundFoo, Is.Null);
        }

        [Test]
        public void Invalid_CannotAddSingletonTwice()
        {
            var systemsObject = new GameObject("Systems");

            TestDelegate action = () => locator.Initialize(s =>
            {
                s.Add<Foo>();
                s.Add<Foo>();
            }, systemsObject);

            Assert.That(action, Throws.Exception.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Initialized_NoRoot_DoesNotThrow()
        {
            TestDelegate action = () => locator.Initialize(s =>
            {
                s.Add<Foo>();
            });

            Assert.That(action, Throws.Nothing);
        }

        [Test]
        public void Initialized_NoRoot_BoundsSystems()
        {
            locator.Initialize(s =>
            {
                s.Add<Foo>();
            });

            var boundSystems = locator.Systems.Select(vp => vp.Key);

            Assert.That(boundSystems, Contains.Item(typeof(Foo)));
        }

        [Test]
        public void Initialized_NoRoot_GetReturnNulls()
        {
            locator.Initialize(s =>
            {
                s.Add<Foo>();
            });

            TestDelegate action = () => locator.Get<Foo>();

            Assert.That(action, Throws.Nothing);
        }

        [Test]
        public void Initialized_NoRoot_ViaFactoryMethod_GetReturnNulls()
        {
            locator.Initialize(s =>
            {
                s.Add<Foo>();
            }, b => null);

            TestDelegate action = () => locator.Get<Foo>();

            Assert.That(action, Throws.Nothing);
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


        [Test]
        public void Invalid_RequiredSingletons_HaveToBePresent()
        {
            var systemsObject = new GameObject("Systems");

            locator.Initialize(s =>
            {
                s.Add<Foo>(required: true);
            }, systemsObject);

            Assert.That(logger.Errors, Has.Count.EqualTo(1));
            Assert.That(logger.Errors[0], Does.Match(".* binding.* required.*"));
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

            locator.Initialize(s =>
            {
                s.Add<IBax>();
            }, systemsObject);
        }

        [Test]
        public void NotOverriten_ReturnsBase()
        {
            var bax = locator.Get<IBax>();

            Assert.That(bax, Is.EqualTo(baseBar));
        }

        [Test]
        public void Overriten_ReturnsOverride()
        {
            var holder = new GameObject("Better Systems");
            var betterBar = AddTestSystem<BetterBar>(holder);

            locator.ApplyOverride(holder);
            var bax = locator.Get<IBax>();

            Assert.That(bax, Is.EqualTo(betterBar));
        }

        [Test]
        public void Overriten_WithList_ReturnsOverride()
        {
            var holder = new GameObject("Better Systems");
            var betterBar = AddTestSystem<BetterBar>(holder);

            List<GameObject> overrides = new List<GameObject>()
            {
                betterBar.gameObject,
            };

            locator.ApplyOverride(null, overrides);
            var bax = locator.Get<IBax>();

            Assert.That(bax, Is.EqualTo(betterBar));
        }

        [Test]
        public void OverritenTwice_ReturnsLastOverride()
        {
            var holder1 = new GameObject("Better Systems 1");
            var holder2 = new GameObject("Better Systems 2");
            var betterBar1 = AddTestSystem<BetterBar>(holder1);
            var betterBar2 = AddTestSystem<BetterBar>(holder2);

            locator.ApplyOverride(holder1);
            locator.ApplyOverride(holder2);
            var bax = locator.Get<IBax>();

            Assert.That(bax, Is.EqualTo(betterBar2));
        }

        [Test]
        public void Overriten_AndRemoved_ReturnsBase()
        {
            var holder = new GameObject("Better Systems");
            var betterBar = AddTestSystem<BetterBar>(holder);

            locator.ApplyOverride(holder);
            locator.RemoveOverride(holder);

            var bax = locator.Get<IBax>();

            Assert.That(bax, Is.EqualTo(baseBar));
        }

        [Test]
        public void Overriten_WithList_AndRemoved_ReturnsBase()
        {
            var holder = new GameObject("Better Systems");
            var betterBar = AddTestSystem<BetterBar>(holder);

            List<GameObject> overrides = new List<GameObject>()
            {
                betterBar.gameObject,
            };

            locator.ApplyOverride(null, overrides);
            locator.RemoveOverride(null, overrides);
            var bax = locator.Get<IBax>();

            Assert.That(bax, Is.EqualTo(baseBar));
        }

        [Test]
        public void Overriten_AndRemoved_Repeated_ReturnsOverride()
        {
            var holder = new GameObject("Better Systems");
            var betterBar = AddTestSystem<BetterBar>(holder);

            locator.ApplyOverride(holder);
            locator.RemoveOverride(holder);
            locator.ApplyOverride(holder);

            var bax = locator.Get<IBax>();

            Assert.That(bax, Is.EqualTo(betterBar));
        }

        [Test]
        public void OverritenTwice_AndRemoved_ReturnsBase()
        {
            var holder1 = new GameObject("Better Systems 1");
            var holder2 = new GameObject("Better Systems 2");
            var betterBar1 = AddTestSystem<BetterBar>(holder1);
            var betterBar2 = AddTestSystem<BetterBar>(holder2);

            locator.ApplyOverride(holder1);
            locator.ApplyOverride(holder2);

            locator.RemoveOverride(holder2);
            locator.RemoveOverride(holder1);

            var bax = locator.Get<IBax>();

            Assert.That(bax, Is.EqualTo(baseBar));
        }

        [Test]
        public void OverritenTwice_AndRemoved_OutOfOrder_ReturnsBase()
        {
            var holder1 = new GameObject("Better Systems 1");
            var holder2 = new GameObject("Better Systems 2");
            var betterBar1 = AddTestSystem<BetterBar>(holder1);
            var betterBar2 = AddTestSystem<BetterBar>(holder2);

            locator.ApplyOverride(holder1);
            locator.ApplyOverride(holder2);

            locator.RemoveOverride(holder1);
            locator.RemoveOverride(holder2);

            var bax = locator.Get<IBax>();

            Assert.That(bax, Is.EqualTo(baseBar));
        }

        [Test]
        public void OverritenTwice_AndRemoved_Middle_ReturnsLates()
        {
            var holder1 = new GameObject("Better Systems 1");
            var holder2 = new GameObject("Better Systems 2");
            var betterBar1 = AddTestSystem<BetterBar>(holder1);
            var betterBar2 = AddTestSystem<BetterBar>(holder2);

            locator.ApplyOverride(holder1);
            locator.ApplyOverride(holder2);

            locator.RemoveOverride(holder1);

            var bax = locator.Get<IBax>();

            Assert.That(bax, Is.EqualTo(betterBar2));
        }

        [Test]
        public void OverritenTwice_AndRemoved_Second_ReturnsFirst()
        {
            var holder1 = new GameObject("Better Systems 1");
            var holder2 = new GameObject("Better Systems 2");
            var betterBar1 = AddTestSystem<BetterBar>(holder1);
            var betterBar2 = AddTestSystem<BetterBar>(holder2);

            locator.ApplyOverride(holder1);
            locator.ApplyOverride(holder2);

            locator.RemoveOverride(holder2);

            var bax = locator.Get<IBax>();

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
        protected ServiceLocator locator;

        [SetUp]
        public void Setup()
        {
            locator = new ServiceLocator();
        }

        protected static T AddTestSystem<T>(GameObject systemsObject) where T : Component
        {
            var foo = new GameObject("Foo").AddComponent<T>();
            foo.transform.SetParent(systemsObject.transform);
            return foo;
        }
    }
}