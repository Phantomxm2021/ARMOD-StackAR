using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Utilities;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEditor.Build.Pipeline.Utilities
{
    /// <summary>
    /// Default implementation of the Build Cache
    /// </summary>
    public class BuildCache : IBuildCache, IDisposable
    {
        const string k_CachePath = "Library/BuildCache";
        const int k_Version = 2;
        internal const long k_BytesToGigaBytes = 1073741824L;

        [NonSerialized]
        IBuildLogger m_Logger;

        [NonSerialized]
        Hash128 m_GlobalHash;

        [NonSerialized]
        CacheServerUploader m_Uploader;

        [NonSerialized]
        CacheServerDownloader m_Downloader;

        /// <summary>
        /// Creates a new build cache object.
        /// </summary>
        public BuildCache()
        {
            m_GlobalHash = CalculateGlobalArtifactVersionHash();
        }

        /// <summary>
        /// Creates a new remote build cache object.
        /// </summary>
        /// <param name="host">The server host.</param>
        /// <param name="port">The server port.</param>
        public BuildCache(string host, int port = 8126)
        {
            m_GlobalHash = CalculateGlobalArtifactVersionHash();

            if (string.IsNullOrEmpty(host))
                return;

            m_Uploader = new CacheServerUploader(host, port);
            m_Downloader = new CacheServerDownloader(this, host, port);
        }

        // internal for testing purposes only
        internal void OverrideGlobalHash(Hash128 hash)
        {
            m_GlobalHash = hash;
            if (m_Uploader != null)
                m_Uploader.SetGlobalHash(m_GlobalHash);
            if (m_Downloader != null)
                m_Downloader.SetGlobalHash(m_GlobalHash);
        }

        static Hash128 CalculateGlobalArtifactVersionHash()
        {
#if UNITY_2019_3_OR_NEWER
            return HashingMethods.Calculate(Application.unityVersion, k_Version).ToHash128();
#else
            return HashingMethods.Calculate(PlayerSettings.scriptingRuntimeVersion, Application.unityVersion, k_Version).ToHash128();
#endif
        }

        internal void ClearCacheEntryMaps()
        {
            BuildCacheUtility.ClearCacheHashes();
        }

        /// <summary>
        /// Disposes the build cache instance.
        /// </summary>
        public void Dispose()
        {
            if (m_Downloader != null)
                m_Downloader.Dispose();
            m_Uploader = null;
            m_Downloader = null;
        }

        /// <inheritdoc />
        public CacheEntry GetCacheEntry(GUID asset, int version = 1)
        {
            return BuildCacheUtility.GetCacheEntry(asset, version);
        }

        /// <inheritdoc />
        public CacheEntry GetCacheEntry(string path, int version = 1)
        {
            return BuildCacheUtility.GetCacheEntry(path, version);
        }

        /// <inheritdoc />
        public CacheEntry GetCacheEntry(ObjectIdentifier objectID, int version = 1)
        {
            if (objectID.guid.Empty())
                return GetCacheEntry(objectID.filePath, version);
            return GetCacheEntry(objectID.guid, version);
        }

        internal CacheEntry GetUpdatedCacheEntry(CacheEntry entry)
        {
            if (entry.Type == CacheEntry.EntryType.File)
                return GetCacheEntry(entry.File, entry.Version);
            if (entry.Type == CacheEntry.EntryType.Asset)
                return GetCacheEntry(entry.Guid, entry.Version);
            return entry;
        }

        internal bool LogCacheMiss(string msg)
        {
            if (!ScriptableBuildPipeline.logCacheMiss)
                return false;
            m_Logger.AddEntrySafe(LogLevel.Warning, msg);
            UnityEngine.Debug.LogWarning(msg);
            return true;
        }

        /// <inheritdoc />
        public bool HasAssetOrDependencyChanged(CachedInfo info)
        {
            if (info == null || !info.Asset.IsValid())
                return true;

            var result = false;
            var updatedEntry = GetUpdatedCacheEntry(info.Asset);
            if (info.Asset != updatedEntry)
            {
                if (!LogCacheMiss($"[Cache Miss]: Source asset changed.\nOld: {info.Asset}\nNew: {updatedEntry}"))
                    return true;
                result = true;
            }

            foreach (var dependency in info.Dependencies)
            {
                if (!dependency.IsValid())
                {
                    if (!LogCacheMiss($"[Cache Miss]: Dependency is no longer valid.\nAsset: {info.Asset}\nDependency: {dependency}"))
                        return true;
                    result = true;
                }

                updatedEntry = GetUpdatedCacheEntry(dependency);
                if (dependency != GetUpdatedCacheEntry(updatedEntry))
                {
                    if (!LogCacheMiss($"[Cache Miss]: Dependency changed.\nAsset: {info.Asset}\nOld: {dependency}\nNew: {updatedEntry}"))
                        return true;
                    result = true;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public string GetCachedInfoFile(CacheEntry entry)
        {
            var guid = entry.Guid.ToString();
            string finalHash = HashingMethods.Calculate(m_GlobalHash, entry.Hash).ToString();
            return string.Format("{0}/{1}/{2}/{3}/{2}.info", k_CachePath, guid.Substring(0, 2), guid, finalHash);
        }

        /// <inheritdoc />
        public string GetCachedArtifactsDirectory(CacheEntry entry)
        {
            var guid = entry.Guid.ToString();
            string finalHash = HashingMethods.Calculate(m_GlobalHash, entry.Hash).ToString();
            return string.Format("{0}/{1}/{2}/{3}", k_CachePath, guid.Substring(0, 2), guid, finalHash);
        }

        class FileOperations
        {
            public FileOperations(int size)
            {
                data = new FileOperation[size];
                waitLock = new Semaphore(0, size);
            }

            public FileOperation[] data;
            public Semaphore waitLock;
        }

        struct FileOperation
        {
            public string file;
            public MemoryStream bytes;
        }

        static void Read(object data)
        {
            var ops = (FileOperations)data;
            for (int index = 0; index < ops.data.Length; index++, ops.waitLock.Release())
            {
                try
                {
                    var op = ops.data[index];
                    if (File.Exists(op.file))
                    {
                        byte[] bytes = File.ReadAllBytes(op.file);
                        if (bytes.Length > 0)
                            op.bytes = new MemoryStream(bytes, false);
                    }
                    ops.data[index] = op;
                }
                catch (Exception e)
                {
                    BuildLogger.LogException(e);
                }
            }
        }

        static void Write(object data)
        {
            var ops = (FileOperations)data;
            for (int index = 0; index < ops.data.Length; index++)
            {
                // Basic spin lock
                ops.waitLock.WaitOne();
                var op = ops.data[index];
                if (op.bytes != null && op.bytes.Length > 0)
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(op.file));
                        File.WriteAllBytes(op.file, op.bytes.GetBuffer());
                    }
                    catch (Exception e)
                    {
                        BuildLogger.LogException(e);
                    }
                }
            }
            ((IDisposable)ops.waitLock).Dispose();
        }

        /// <inheritdoc />
        public void LoadCachedData(IList<CacheEntry> entries, out IList<CachedInfo> cachedInfos)
        {
            if (entries == null)
            {
                cachedInfos = null;
                return;
            }

            if (entries.Count == 0)
            {
                cachedInfos = new List<CachedInfo>();
                return;
            }

            using (m_Logger.ScopedStep(LogLevel.Info, "LoadCachedData"))
            {
                m_Logger.AddEntrySafe(LogLevel.Info, $"{entries.Count} items");
                // Setup Operations
                var ops = new FileOperations(entries.Count);
                using (m_Logger.ScopedStep(LogLevel.Info, "GetCachedInfoFile"))
                {
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var op = ops.data[i];
                        op.file = GetCachedInfoFile(entries[i]);
                        ops.data[i] = op;
                    }
                }

                int cachedCount = 0;
                using (m_Logger.ScopedStep(LogLevel.Info, "Read and deserialize cache info"))
                {
                    // Start file reading
                    Thread thread = new Thread(Read);
                    thread.Start(ops);

                    cachedInfos = new List<CachedInfo>(entries.Count);

                    // Deserialize as files finish reading
                    Stopwatch deserializeTimer = Stopwatch.StartNew();
                    var formatter = new BinaryFormatter();
                    for (int index = 0; index < entries.Count; index++)
                    {
                        // Basic wait lock
                        if (!ops.waitLock.WaitOne(0))
                        {
                            deserializeTimer.Stop();
                            ops.waitLock.WaitOne();
                            deserializeTimer.Start();
                        }

                        CachedInfo info = null;
                        try
                        {
                            var op = ops.data[index];
                            if (op.bytes != null && op.bytes.Length > 0)
                            {
                                info = formatter.Deserialize(op.bytes) as CachedInfo;
                                cachedCount++;
                            }
                            else
                                LogCacheMiss($"[Cache Miss]: Missing cache entry.\nEntry: {entries[index]}");
                        }
                        catch (Exception e)
                        {
                            BuildLogger.LogException(e);
                        }
                        cachedInfos.Add(info);
                    }
                    thread.Join();
                    ((IDisposable)ops.waitLock).Dispose();

                    deserializeTimer.Stop();
                    m_Logger.AddEntrySafe(LogLevel.Info, $"Time spent deserializing: {deserializeTimer.ElapsedMilliseconds}ms");
                    m_Logger.AddEntrySafe(LogLevel.Info, $"Local Cache hit count: {cachedCount}");
                }

                using (m_Logger.ScopedStep(LogLevel.Info, "Check for changed dependencies"))
                {
                    for (int i = 0; i < cachedInfos.Count; i++)
                    {
                        if (HasAssetOrDependencyChanged(cachedInfos[i]))
                            cachedInfos[i] = null;
                    }
                }

                // If we have a cache server connection, download & check any missing info
                int downloadedCount = 0;
                if (m_Downloader != null)
                {
                    using (m_Logger.ScopedStep(LogLevel.Info, "Download Missing Entries"))
                    {
                        m_Downloader.DownloadMissing(entries, cachedInfos);
                        downloadedCount = cachedInfos.Count(i => i != null) - cachedCount;
                    }
                }

                m_Logger.AddEntrySafe(LogLevel.Info, $"Local Cache hit count: {cachedCount}, Cache Server hit count: {downloadedCount}");

                Assert.AreEqual(entries.Count, cachedInfos.Count);
            }
        }

        /// <inheritdoc />
        public void SaveCachedData(IList<CachedInfo> infos)
        {
            if (infos == null || infos.Count == 0)
                return;

            using (m_Logger.ScopedStep(LogLevel.Info, "SaveCachedData"))
            {
                m_Logger.AddEntrySafe(LogLevel.Info, $"Saving {infos.Count} infos");
                // Setup Operations
                var ops = new FileOperations(infos.Count);
                using (m_Logger.ScopedStep(LogLevel.Info, "SetupOperations"))
                {
                    for (int i = 0; i < infos.Count; i++)
                    {
                        var op = ops.data[i];
                        op.file = GetCachedInfoFile(infos[i].Asset);
                        ops.data[i] = op;
                    }
                }

                // Start writing thread
                ThreadingManager.QueueTask(ThreadingManager.ThreadQueues.SaveQueue, Write, ops);

                using (m_Logger.ScopedStep(LogLevel.Info, "SerializingCacheInfos"))
                {
                    // Serialize data as previous data is being written out
                    var formatter = new BinaryFormatter();
                    for (int index = 0; index < infos.Count; index++, ops.waitLock.Release())
                    {
                        try
                        {
                            var op = ops.data[index];
                            var stream = new MemoryStream();
                            formatter.Serialize(stream, infos[index]);
                            if (stream.Length > 0)
                            {
                                op.bytes = stream;
                                ops.data[index] = op;

                                // If we have a cache server connection, upload the cached data
                                if (m_Uploader != null)
                                    m_Uploader.QueueUpload(infos[index].Asset, GetCachedArtifactsDirectory(infos[index].Asset), new MemoryStream(stream.GetBuffer(), false));
                            }
                        }
                        catch (Exception e)
                        {
                            BuildLogger.LogException(e);
                        }
                    }
                }
            }
        }

        internal void SyncPendingSaves()
        {
            using (m_Logger.ScopedStep(LogLevel.Info, "SyncPendingSaves"))
                ThreadingManager.WaitForOutstandingTasks();
        }

        internal struct CacheFolder
        {
            public DirectoryInfo directory;
            public long Length { get; set; }
            public void Delete() => directory.Delete(true);
            public DateTime LastAccessTimeUtc
            {
                get => directory.LastAccessTimeUtc;
                internal set => directory.LastAccessTimeUtc = value;
            }
        }

        /// <summary>
        /// Deletes the build cache directory.
        /// </summary>
        /// <param name="prompt">The message to display in the popup window.</param>
        public static void PurgeCache(bool prompt)
        {
            ThreadingManager.WaitForOutstandingTasks();
            BuildCacheUtility.ClearCacheHashes();
            if (!Directory.Exists(k_CachePath))
            {
                if (prompt)
                    UnityEngine.Debug.Log("Current build cache is empty.");
                return;
            }

            if (prompt)
            {
                if (!EditorUtility.DisplayDialog("Purge Build Cache", "Do you really want to purge your entire build cache?", "Yes", "No"))
                    return;

                EditorUtility.DisplayProgressBar(ScriptableBuildPipeline.Properties.purgeCache.text, ScriptableBuildPipeline.Properties.pleaseWait.text, 0.0F);
                Directory.Delete(k_CachePath, true);
                EditorUtility.ClearProgressBar();
            }
            else
                Directory.Delete(k_CachePath, true);
        }

        /// <summary>
        /// Prunes the build cache so that its size is within the maximum cache size.
        /// </summary>
        public static void PruneCache()
        {
            ThreadingManager.WaitForOutstandingTasks();
            int maximumSize = ScriptableBuildPipeline.maximumCacheSize;
            long maximumCacheSize = maximumSize * k_BytesToGigaBytes;

            // Get sizes based on common directory root for a guid / hash
            ComputeCacheSizeAndFolders(out long currentCacheSize, out List<CacheFolder> cacheFolders);

            if (currentCacheSize < maximumCacheSize)
            {
                UnityEngine.Debug.LogFormat("Current build cache currentCacheSize {0}, prune threshold {1} GB. No prune performed. You can change this value in the \"Edit/Preferences...\" window.", EditorUtility.FormatBytes(currentCacheSize), maximumSize);
                return;
            }

            if (!EditorUtility.DisplayDialog("Prune Build Cache", string.Format("Current build cache currentCacheSize is {0}, which is over the prune threshold of {1}. Do you want to prune your build cache now?", EditorUtility.FormatBytes(currentCacheSize), EditorUtility.FormatBytes(maximumCacheSize)), "Yes", "No"))
                return;

            EditorUtility.DisplayProgressBar(ScriptableBuildPipeline.Properties.pruneCache.text, ScriptableBuildPipeline.Properties.pleaseWait.text, 0.0F);

            PruneCacheFolders(maximumCacheSize, currentCacheSize, cacheFolders);

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Prunes the build cache without showing UI prompts.
        /// </summary>
        /// <param name="maximumCacheSize">The maximum cache size.</param>
        public static void PruneCache_Background(long maximumCacheSize)
        {
            ThreadingManager.QueueTask(ThreadingManager.ThreadQueues.PruneQueue, PruneCache_Background_Internal, maximumCacheSize);
        }

        internal static void PruneCache_Background_Internal(object maximumCacheSize)
        {
            long maxCacheSize = (long)maximumCacheSize;
            // Get sizes based on common directory root for a guid / hash
            ComputeCacheSizeAndFolders(out long currentCacheSize, out List<CacheFolder> cacheFolders);
            if (currentCacheSize < maxCacheSize)
                return;

            PruneCacheFolders(maxCacheSize, currentCacheSize, cacheFolders);
        }

        internal static void ComputeCacheSizeAndFolders(out long currentCacheSize, out List<CacheFolder> cacheFolders)
        {
            currentCacheSize = 0;
            cacheFolders = new List<CacheFolder>();

            var directory = new DirectoryInfo(k_CachePath);
            if (!directory.Exists)
                return;

            int length = directory.FullName.Count(x => x == Path.DirectorySeparatorChar) + 3;
            DirectoryInfo[] subDirectories = directory.GetDirectories("*", SearchOption.AllDirectories);
            foreach (var subDirectory in subDirectories)
            {
                if (subDirectory.FullName.Count(x => x == Path.DirectorySeparatorChar) != length)
                    continue;

                FileInfo[] files = subDirectory.GetFiles("*", SearchOption.AllDirectories);
                var cacheFolder = new CacheFolder { directory = subDirectory, Length = files.Sum(x => x.Length) };
                cacheFolders.Add(cacheFolder);

                currentCacheSize += cacheFolder.Length;
            }
        }

        internal static void PruneCacheFolders(long maximumCacheSize, long currentCacheSize, List<CacheFolder> cacheFolders)
        {
            cacheFolders.Sort((a, b) => a.LastAccessTimeUtc.CompareTo(b.LastAccessTimeUtc));
            // Need to delete sets of files as the .info might reference a specific file artifact
            foreach (var cacheFolder in cacheFolders)
            {
                currentCacheSize -= cacheFolder.Length;
                cacheFolder.Delete();
                if (currentCacheSize < maximumCacheSize)
                    break;
            }
        }

        // TODO: Add to IBuildCache interface when IBuildLogger becomes public
        internal void SetBuildLogger(IBuildLogger profiler)
        {
            m_Logger = profiler;
        }
    }
}
