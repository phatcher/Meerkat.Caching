### 2.3.0 (2020-09-06)
* Drop netstandard2.0 dependencies back to 2.1 so we can use with netcoreapp2.1
* Introduce netstandard2.1 target for netcoreapp3.1 projects
* Remove net462 and net471 targets as framework versions no longer suppported

### 2.2.1 (2019-04-29)
* Further helper methods for IMemoryCache/IDistributedCache.
* Add ISynchronizer.Remove for releasing semaphores

### 2.2.0 (2019-04-26)
* Add helpers for service level caching and IMemoryCache/IDistributedCache.
* Include pdbs in package

### 2.1.3 (2019-03-10)
* Introduce SourceLink

### 2.1.2 (2018-09-03)
* Tidy up package dependencies

### 2.1.1 (2018-09-02)
* Enable MemoryCache for net462, net471 and net472

### 2.1.0 (2018-08-30)
* Target net462, net471 and net472

### 2.0.0 (2018-04-03)
* Target net45 and netstandard2.0

### 1.0.7 (2017-06-11)
* Introduce CacheConfiguration helper to retrieve cache durations from AppSettings
* Add async version of AddOrGetExisting extension method

### 1.0.6 (2017-02-4)
* Replace LibLog with Meerkat.Logging

### 1.0.5 (2016-12-22)
* Replace Common.Logging with LibLog

### 1.0.4 (2016-09-20)
* Protect against nulls being stored against MemoryCache

### 1.0.3 (2016-06-06)
* Strong name assembly

### 1.0.2 (2016-06-06)
* Drop framework version to 4.5 from 4.5.2

### 1.0.1 (2016-06-05)
* Extension method for lazy AddOrGetExisting<T>

### 1.0.0 (2016-05-18)
* First release