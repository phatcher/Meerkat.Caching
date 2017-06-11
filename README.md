Meerkat Caching
===============

The [Meerkat.Caching](https://www.nuget.org/packages/Meerkat.Caching/) library is a simple cache that abstracts an interface from System.Runtime.Caching's MemoryCache

The rationale for this is so we can have a simple caching implementation for using in, say an ASP.NET MVC application, which can then be subsituted for a more advanced cache such as Redis without changing the calls.

We also address the lack of region support in MemoryCache in our wrapper by providing a strategy pattern for constructing a composite key/region key which is then passed to the underlying MemoryCache instance.

[![NuGet](https://img.shields.io/nuget/v/Meerkat.Caching.svg)](https://www.nuget.org/packages/Meerkat.Caching/)
[![Build status](https://ci.appveyor.com/api/projects/status/7ycnghu7s0umys9e/branch/master?svg=true)](https://ci.appveyor.com/project/PaulHatcher/meerkat-caching/branch/master)


Welcome to contributions from anyone.

You can see the version history [here](RELEASE_NOTES.md).

## Build the project
* Windows: Run *build.cmd*

I have my tools in C:\Tools so I use *build.cmd Default tools=C:\Tools encoding=UTF-8*

## Library License

The library is available under the [MIT License](http://en.wikipedia.org/wiki/MIT_License), for more information see the [License file][1] in the GitHub repository.

 [1]: https://github.com/phatcher/Meerkat.Caching/blob/master/License.md

## Getting Started

The library is intended to be a drop-in replacement for MemoryCache so we have a static class called MemoryObjectCache that wraps it and provides region support. We also provide a static
factory class, MemoryObjectCacheFactory to ease the change over.

So originally you might have

```
    var cache = MemoryCache.Default
```

You now have
```
    var cache = MemoryObjectCacheFactory.Default
```

Under the hood, the factory is just creating a MemoryObjectCache which wraps the MemoryCache and also a RegionKeyStrategy which modifies the keys you add/lookup to provide region support e.g.

```
    /// <summary>
    /// Get a reference to the default <see cref="MemoryObjectCache"/> instance.
    /// </summary>
    /// <returns></returns>
    public static MemoryObjectCache Default()
    {
        var strategy = new RegionKeyStrategy();
        var cache = MemoryCache.Default;

        return new MemoryObjectCache(cache, strategy);
    }
```
