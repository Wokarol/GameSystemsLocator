
# Game Systems Locator

Singletons... **but better**

## About
This package provides a way to bootstrap and manage global game systems like saving or music.  
It allows for nice and centralized place to define and get systems as opposed to normal messy singleton spaghetti.

## Installation
To install the package, open Unity Package Manager

![](readme-src/upm.png)

Then select "Add package from git URL..."  
Then type ` https://github.com/Wokarol/GameSystemsLocator.git ` as the URL  
> *If you want to target a specific version, suffix the URL with ` #version `, for example ` https://github.com/Wokarol/GameSystemsLocator.git#v0.2.0 `*

## Usage
### Configuration
To start using the package you first have to **create a config class**.  
This class has to implement ` ISystemConfiguration ` interface and cannot be static.

This interface defines a single method, ` void Configure(GameSystems.ConfigurationBuilder builder) ` which should contain all the configuration and setup.

Example of such class might look like so:
```cs
public class GameConfig : ISystemConfiguration
{
    public void Configure(GameSystems.ConfigurationBuilder builder)
    {
        builder.PrefabPath = "Systems";

        builder.Add<IMusicSource>(nullObject: new NullMusicSource());
        builder.Add<SaveSystem>(required: true);
        builder.Add<PlayerHandle>();
    }
}
```

For configuration you have to define the ` PrefabPath ` which points to prefab in resources that will be loaded on game start.

` .Add<T>(...) ` is used to add a game system container to the locator. Optionally it can define if the system is ` required ` or if it has a ` nullObject ` instance.

> For more information, refer to in-code documentation

### Overrides
Game Systems can be overwritten, for that attach ` SystemOverrider ` component to the game object with game systems that should be used as children. Like shown on the example:

![](readme-src/overrider.png)

No additional configuration is needed

> You can also pass the systems in a list directly, this will call ` GetComponent ` on them to retrieve a system.

> Currently enabling an overrider loops over all systems calling ` GetComponentInChildren ` which might cause a performance hit but I did not yet test it well enough to confirm that or deny

### Locating
Locator configured like so can then be used to obtain references to game system using ` GameSystem.Get<IMusicSource>().Play() ` at any point in code

This method will get the current system even including the overrides, if no instance is present, it will attempt to return the null object.

> Currently, getting the system in ` Enable() ` at the beginning of the application lifetime (when entering game mode for example) will almost always lead to an exception. Considering using ` Start() ` or calling the method right before the system is needed

## Changelog
### v0.5.0
- **Add:** Systems list in System Overrider
- **Fix:** Having System Overrider enabled while the playmode is entered does not register systems properly

### v0.4.0
- **Add:** Readme content
- **Add:** Code documentation

### v0.3.0
- **Add:** System Overrider

### v0.2.1
- **Fix:** Created system has "(Clone)" suffix
- **Fix:** Failure to instantiate a prefab leaves empty object in the hierarchy

### v0.2.0
- **Rename:** ` AddSingleton ` to ` Add `
