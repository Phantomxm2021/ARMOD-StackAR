using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;

namespace UnityEditor.Build.Pipeline.WriteTypes
{
    /// <summary>
    /// Explicit implementation for writing a scene serialized file that can be used with the Asset Bundle systems.
    /// </summary>
    [Serializable]
    public class SceneBundleWriteOperation : IWriteOperation
    {
        /// <inheritdoc />
        public WriteCommand Command { get; set; }
        /// <inheritdoc />
        public BuildUsageTagSet UsageSet { get; set; }
        /// <inheritdoc />
        public BuildReferenceMap ReferenceMap { get; set; }
        /// <inheritdoc />
        public Hash128 DependencyHash { get; set; }

        /// <summary>
        /// Source scene asset path
        /// </summary>
        public string Scene { get; set; }

        /// <summary>
        /// Processed scene path returned by the ProcessScene API.
        /// <seealso cref="ContentBuildInterface.PrepareScene"/>
        /// </summary>
#if UNITY_2019_3_OR_NEWER
        [Obsolete("ProcessedScene has been deprecated.")]
#endif
        public string ProcessedScene { get; set; }

        /// <summary>
        /// Information needed for scene preloadeding.
        /// <seealso cref="PreloadInfo"/>
        /// </summary>
        public PreloadInfo PreloadInfo { get; set; }

        /// <summary>
        /// Information needed for generating the Asset Bundle object to be included in the serialized file.
        /// <see cref="SceneBundleInfo"/>
        /// </summary>
        public SceneBundleInfo Info { get; set; }

        /// <inheritdoc />
        public WriteResult Write(string outputFolder, BuildSettings settings, BuildUsageTagGlobal globalUsage)
        {
#if UNITY_2019_3_OR_NEWER
            return ContentBuildInterface.WriteSceneSerializedFile(outputFolder, new WriteSceneParameters
            {
                scenePath = Scene,
                writeCommand = Command,
                settings = settings,
                globalUsage = globalUsage,
                usageSet = UsageSet,
                referenceMap = ReferenceMap,
                preloadInfo = PreloadInfo,
                sceneBundleInfo = Info
            });
#else
            return ContentBuildInterface.WriteSceneSerializedFile(outputFolder, Scene, ProcessedScene, Command, settings, globalUsage, UsageSet, ReferenceMap, PreloadInfo, Info);
#endif
        }

        /// <inheritdoc />
        public Hash128 GetHash128(IBuildLogger log)
        {
#if UNITY_2019_3_OR_NEWER
            CacheEntry entry = BuildCacheUtility.GetCacheEntry(Scene);
#else
            CacheEntry entry = BuildCacheUtility.GetCacheEntry(ProcessedScene);
#endif
            HashSet<CacheEntry> hashObjects = new HashSet<CacheEntry>();
            using (log.ScopedStep(LogLevel.Verbose, $"Gather Objects", Command.fileName))
            {
                if (Command.serializeObjects != null)
                    foreach (var serializeObject in Command.serializeObjects)
                        hashObjects.Add(BuildCacheUtility.GetCacheEntry(serializeObject.serializationObject));
            }

            List<Hash128> hashes = new List<Hash128>();
            using (log.ScopedStep(LogLevel.Verbose, $"Hashing Command", Command.fileName))
                hashes.Add(Command.GetHash128());
            using (log.ScopedStep(LogLevel.Verbose, $"Hashing UsageSet", Command.fileName))
                hashes.Add(UsageSet.GetHash128());
            using (log.ScopedStep(LogLevel.Verbose, $"Hashing ReferenceMap", Command.fileName))
                hashes.Add(ReferenceMap.GetHash128());
            using (log.ScopedStep(LogLevel.Verbose, $"Hashing PreloadInfo", Command.fileName))
                hashes.Add(PreloadInfo.GetHash128());
            using (log.ScopedStep(LogLevel.Verbose, $"Hashing Info", Command.fileName))
                hashes.Add(Info.GetHash128());
            using (log.ScopedStep(LogLevel.Verbose, $"Hashing Objects", Command.fileName))
                hashes.Add(HashingMethods.Calculate(hashObjects).ToHash128());
            hashes.Add(new Hash128(0, 0, 0, (uint)QualitySettingsApi.GetNumberOfLODsStripped()));
            hashes.Add(DependencyHash);

            return HashingMethods.Calculate(hashes, Scene, entry).ToHash128();
        }

        /// <inheritdoc />
        public Hash128 GetHash128()
        {
            return GetHash128(null);
        }
    }
}
