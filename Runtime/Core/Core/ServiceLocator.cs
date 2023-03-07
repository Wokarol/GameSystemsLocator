using System;

namespace Wokarol.GameSystemsLocator.Core
{
    // Note: This class now takes over the logic of GameSystems to move it away from static
    public class ServiceLocator
    {
        // Note: This method now takes the responsibility of ConfigurationBuilder.Add
        internal void Add(Type type, object nullObject, bool required)
        {
            throw new NotImplementedException();
        }
    }
}