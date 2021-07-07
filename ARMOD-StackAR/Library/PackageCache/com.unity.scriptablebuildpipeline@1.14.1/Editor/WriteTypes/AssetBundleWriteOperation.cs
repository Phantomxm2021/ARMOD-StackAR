using System;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;

namespace UnityEditor.Build.Pipeline.WriteTypes
{
    /// <summary>
    /// Explicit implementation for writing a serialized file that can be used with the Asset Bundle systems.
    /// </summary>
    [Serializable]
    public class AssetBundleWriteOperation : IWriteOperation
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
        /// Information needed for generating the Asset Bundle object to be included in the serialized file.
        /// <see cref="AssetBundleInfo"/>
        /// </summary>
        public AssetBundleInfo Info { get; set; }

        /// <inheritdoc />
        public WriteResult Write(string outputFolder, BuildSettings settings, BuildUsageTagGlobal globalUsage)
        {
#if UNITY_2019_3_OR_NEWER
            return ContentBuildInterface.WriteSerializedFile(outputFolder, new WriteParameters
            {
                writeCommand = Command,
                settings = settings,
                globalUsage = globalUsage,
                usageSet = UsageSet,
                referenceMap = ReferenceMap,
                bundleInfo = Info
            });
#else
            return ContentBuildInterface.WriteSerializedFile(outputFolder, Command, settings, globalUsage, UsageSet, ReferenceMap, Info);
#endif
        }

        /// <inheritdoc />
        public Hash128 GetHash128(IBuildLogger log)
        {
            HashSet<CacheEntry> hashObjects = new HashSet<CacheEntry>();
            using (log.ScopedStep(LogLevel.Verbose, $"Gather Objects {GetType().Name}", Command.fileName))
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
            using (log.ScopedStep(LogLevel.Verbose, $"Hashing Info", Command.fileName))
                hashes.Add(Info.GetHash128());
            using (log.ScopedStep(LogLevel.Verbose, $"Hashing Objects", Command.fileName))
                hashes.Add(HashingMethods.Calculate(hashObjects).ToHash128());
            hashes.Add(DependencyHash);

            return HashingMethods.Calculate(hashes).ToHash128();
        }

        /// <inheritdoc />
        public Hash128 GetHash128()
        {
            return GetHash128(null);
        }
    }
}
