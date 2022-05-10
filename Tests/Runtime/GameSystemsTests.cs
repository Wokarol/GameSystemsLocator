using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Wokarol.GameSystemsLocator.Tests
{
    public class GameSystemsTests
    {

        [SetUp]
        public void Setup()
        {
            GameSystems.Clear();
        }

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
                s.AddSingleton<Foo>();
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
                s.AddSingleton<Foo>();
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
                s.AddSingleton<IBax>();
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
                s.AddSingleton<IBax>(nullObject: nullBax);
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
                s.AddSingleton<IBax>(nullObject: nullBax);
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
                s.AddSingleton<Foo>();
                s.AddSingleton<Bar>();
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
                s.AddSingleton<Foo>();
                s.AddSingleton<Foo>();
            });

            Assert.That(action, Throws.Exception.TypeOf<InvalidOperationException>());
            yield break;
        }

        private static T AddTestSystem<T>(GameObject systemsObject) where T : Component
        {
            var foo = new GameObject("Foo").AddComponent<T>();
            foo.transform.SetParent(systemsObject.transform);
            return foo;
        }
    }

    internal interface IBax { }

    internal class Bar : MonoBehaviour, IBax { }
    internal class NullBax : IBax { }

    internal class Foo : MonoBehaviour { }
}
