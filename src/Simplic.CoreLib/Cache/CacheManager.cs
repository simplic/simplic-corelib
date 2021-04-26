using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simplic.Cache
{
    #region Delegate
    /// <summary>
    /// Delegate which will be fired, if the cached was cleared
    /// </summary>
    /// <param name="sender">Cache manager isntance</param>
    /// <param name="arg">- Event args instance -</param>
    public delegate void CacheClearedEventHandler(object sender, EventArgs arg);
    #endregion

    /// <summary>
    /// Verwaltet CacheObjects
    /// </summary>
    public class CacheManager
    {
        #region Singleton
        private static readonly CacheManager singleton = new CacheManager();

        public static CacheManager Singleton
        {
            get { return CacheManager.singleton; }
        }
        #endregion

        #region Events
        /// <summary>
        /// Cache clear event
        /// </summary>
        public event CacheClearedEventHandler CacheCleared;
        #endregion

        #region Public Static
        /// <summary>
        /// Prepare a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string PrepareKey(string key)
        {
            return key.ToLower().Trim();
        }
        #endregion

        #region Private Member
        private IDictionary<CacheKeyItem, ICacheable> cache;
        private bool enableCaching;
        private object _lockObj = new object();
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        private CacheManager()
        {
            cache = new Dictionary<CacheKeyItem, ICacheable>();
            enableCaching = true;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Find a cachable by its key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        private T Find<T>(string key, bool throwWeakRefException)
        {
            var returnValue = cache[new CacheKeyItem(typeof(T), PrepareKey(key))];

            if (returnValue == null)
            {
                return default(T);
            }
            else if (returnValue.GetType().BaseType == typeof(TypedWeakReference))
            {
                var wr = (returnValue as TypedWeakReference);
                if (!wr.IsAlive)
                {
                    // remove from cache
                    RemoveObjectNoException<T>(key);
                    if (throwWeakRefException)
                    {
                        throw new CoreException("S-0000001", "08b53e6a-ff10-4f29-b011-83c0fbe8b6f4");
                    }
                    return default(T);
                }
                else
                {
                    return (T)wr.Target;
                }
            }
            else
            {
                return (T)returnValue;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Stores the object as a weak reference in the cache
        /// </summary>
        /// <typeparam name="T">Type of object to stre</typeparam>
        /// <param name="cacheable">Object to cache</param>
        /// <param name="ignoreIfExists">If set to true, existing objects will be ignored</param>
        public void AddWeakReferenceObject<T>(T cacheable, bool ignoreIfExists = true) where T : ICacheable
        {
            var _ref = new WeakReferenceCachable<T>(cacheable);
            AddObject(_ref, ignoreIfExists);
        }

        /// <summary>
        /// Fügt ein neues Objekt in den Cache ein
        /// </summary>
        /// <param name="Cacheable">If set to true, the object will be ignored if it already exists</param>
        public void AddObject(ICacheable Cacheable)
        {
            AddObject(Cacheable, true);
        }

        /// <summary>
        /// Fügt ein neues Objekt in den Cache ein
        /// </summary>
        /// <param name="Cacheable">If set to true, the object will be ignored if it already exists</param>
        /// <param name="ignoreIfExists">If set to true, existing objects will be ignored</param>
        public void AddObject(ICacheable Cacheable, bool ignoreIfExists = true)
        {
            lock (_lockObj)
            {
                if (enableCaching)
                {
                    if (Cacheable == null)
                    {
                        throw new CoreException("S-0000002", "9c2917a6-41a7-4dc6-876c-43e6336a610f");
                    }
                    else
                    {

                        // If we have a TypedWeakReference here, we need to store it by the type of its target
                        if (Cacheable.GetType().BaseType == typeof(TypedWeakReference))
                        {
                            var wr = (Cacheable as TypedWeakReference);
                            var _key = new CacheKeyItem(wr.TargetType, PrepareKey(Cacheable.Key));
                            if (wr.IsAlive && !cache.ContainsKey(_key))
                            {
                                cache.Add(_key, Cacheable);
                            }
                            // If it not not alive, override the object
                            else if (!wr.IsAlive)
                            {
                                cache[_key] = Cacheable;
                            }
                            else
                            {
                                if (!ignoreIfExists)
                                {
                                    throw new CoreException("S-0000005", "17771eac-511e-4d83-bd95-288417867990", () => Cacheable.Key.ToString());
                                }
                            }
                        }
                        else
                        {
                            if (!cache.ContainsKey(new CacheKeyItem(Cacheable.GetType(), PrepareKey(Cacheable.Key))))
                            {
                                cache.Add(new CacheKeyItem(Cacheable.GetType(), PrepareKey(Cacheable.Key)), Cacheable);
                            }
                            else
                            {
                                if (!ignoreIfExists)
                                {
                                    throw new CoreException("S-0000005", "8ba4d284-62f7-4c53-8c95-2c9efe5ff3fd", () => Cacheable.Key.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Entfernt ein Cache-Objekt aus dem speicher
        /// </summary>
        /// <param name="Key">Eindeutiger Schlüssel, anhand dessen das Objekt identifiziert wird</param>
        public void RemoveObject<T>(string Key)
        {
            lock (_lockObj)
            {
                if (enableCaching)
                {
                    if (Key == null)
                    {
                        throw new CoreException("S-0000001", "97d1454c-1780-442a-a47c-b70e59c06024");
                    }
                    else
                    {
                        if (cache.ContainsKey(new CacheKeyItem(typeof(T), PrepareKey(Key))))
                        {
                            cache[new CacheKeyItem(typeof(T), PrepareKey(Key))].OnRemove();
                            cache.Remove(new CacheKeyItem(typeof(T), PrepareKey(Key)));
                        }
                        else
                        {
                            throw new CoreException("S-0000005", "2f0326bf-bf90-4e22-a689-2ebe41294ccf", () => Key.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Entfernt ein Cache-Objekt aus dem speicher
        /// </summary>
        /// <param name="Key">Eindeutiger Schlüssel, anhand dessen das Objekt identifiziert wird</param>
        public void RemoveObjectNoException<T>(string Key)
        {
            lock (_lockObj)
            {
                if (enableCaching)
                {
                    if (Key == null)
                    {
                        throw new CoreException("S-0000004", "cc0a2887-93a8-494c-903c-3eb0cfdb1e43");
                    }
                    else
                    {
                        var _key = new CacheKeyItem(typeof(T), PrepareKey(Key));
                        if (cache.ContainsKey(_key))
                        {
                            cache[_key].OnRemove();
                            cache.Remove(_key);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Holt ein Objekt aus dem Cache und gibt es zurück
        /// </summary>
        /// <param name="Key">Eindeutiger schlüssel, anhand dessen das Objekt identifiziert wird</param>
        /// <returns>Instanz eines Cache-Objekts</returns>
        public T GetObject<T>(string Key) where T : ICacheable
        {
            lock (_lockObj)
            {
                if (!enableCaching)
                {
                    throw new CoreException("S-0000003", "2abb15a7-5350-4c68-8f6c-c35b68027535");
                }

                if (Key == null)
                {
                    throw new CoreException("S-0000004", "f9f90e72-166e-4b48-89d6-153a8ffe3b6c");
                }
                else
                {
                    if (!enableCaching)
                    {
                        throw new CoreException("S-0000005", "7486f66d-043f-4dc5-bd3a-80f7d1cdbec0", () => Key.ToString());
                    }

                    if (cache.ContainsKey(new CacheKeyItem(typeof(T), PrepareKey(Key))))
                    {
                        return Find<T>(Key, true);
                    }
                    else
                    {
                        throw new CoreException("S-0000006", "de1bf46d-87cc-4242-89d5-e71088797581", () => Key.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Holt ein Objekt aus dem Cache und gibt es zurück und wirft keine Exception falls das Objekt nicht gefunden werden konnte
        /// </summary>
        /// <param name="Key">Eindeutiger schlüssel, anhand dessen das Objekt identifiziert wird</param>
        /// <returns>Instanz eines Cache-Objekts</returns>
        public T GetObjectNoException<T>(string Key) where T : ICacheable
        {
            lock (_lockObj)
            {
                if (!enableCaching)
                {
                    return default(T);
                }

                if (Key == null)
                {
                    return default(T);
                }
                else
                {
                    if (cache.ContainsKey(new CacheKeyItem(typeof(T), PrepareKey(Key))))
                    {
                        return Find<T>(Key, false);
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// Returns the instance of a generic type just by passing its content type. The type of the data will be used for getting the item
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns></returns>
        public GenericCacheItem<T> GenericCacheItemNoException<T>()
        {
            return GetObjectNoException<GenericCacheItem<T>>(typeof(T).ToString());
        }

        /// <summary>
        /// Leert den gesamten Cache
        /// </summary>
        public void ClearCache()
        {
            lock (_lockObj)
            {
                foreach (var cacheObject in cache)
                {
                    cacheObject.Value.OnRemove();
                }

                cache = new Dictionary<CacheKeyItem, ICacheable>();

                if (CacheCleared != null)
                {
                    CacheCleared(this, new EventArgs());
                }
            }
        }
        #endregion

        #region Public Member
        /// <summary>
        /// Caching
        /// </summary>
        public bool EnableCaching
        {
            get { return enableCaching; }
            set
            {
                lock (_lockObj)
                {
                    enableCaching = value;
                }
            }
        }
        #endregion
    }
}
