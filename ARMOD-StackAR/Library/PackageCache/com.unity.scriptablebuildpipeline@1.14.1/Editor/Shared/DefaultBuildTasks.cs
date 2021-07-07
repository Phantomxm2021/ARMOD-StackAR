using System;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;

namespace UnityEditor.Build.Pipeline
{
    /// <summary>
    /// Basic static class containing preset build pipeline task collections.
    /// </summary>
    public static class DefaultBuildTasks
    {
        /// <summary>
        /// Options for different preset build pipelines
        /// </summary>
        public enum Preset
        {
            /// <summary>
            /// Use to indicate that the pipeline only executes player scripts.
            /// </summary>
            PlayerScriptsOnly,
            /// <summary>
            /// Use to indicate that the pipeline should create asset bundles.
            /// </summary>
            AssetBundleCompatible,
            /// <summary>
            /// Use to indicate that the pipeline should create asset bundles and the built-in shader bundle.
            /// </summary>
            AssetBundleBuiltInShaderExtraction,
        }

        /// <summary>
        /// Constructs and returns an IList containing the build tasks in the correct order for the preset build pipeline.
        /// </summary>
        /// <param name="preset">The preset build pipeline to construct and return.</param>
        /// <returns>IList containing the build tasks in the correct order for the preset build pipeline.</returns>
        public static IList<IBuildTask> Create(Preset preset)
        {
            switch (preset)
            {
                case Preset.PlayerScriptsOnly:
                    return PlayerScriptsOnly();
                case Preset.AssetBundleCompatible:
                    return AssetBundleCompatible();
                case Preset.AssetBundleBuiltInShaderExtraction:
                    return AssetBundleBuiltInShaderExtraction();
                default:
                    throw new NotImplementedException(string.Format("Preset for '{0}' not yet implemented.", preset));
            }
        }

        static IList<IBuildTask> PlayerScriptsOnly()
        {
            var buildTasks = new List<IBuildTask>();

            // Setup
            buildTasks.Add(new SwitchToBuildPlatform());

            // Player Scripts
            buildTasks.Add(new BuildPlayerScripts());
            buildTasks.Add(new PostScriptsCallback());

            // Dependency
            // - Empty

            // Packing
            // - Empty

            // Writing
            // - Empty

            return buildTasks;
        }

        static IList<IBuildTask> AssetBundleCompatible()
        {
            var buildTasks = new List<IBuildTask>();

            // Setup
            buildTasks.Add(new SwitchToBuildPlatform());
            buildTasks.Add(new RebuildSpriteAtlasCache());

            // Player Scripts
            buildTasks.Add(new BuildPlayerScripts());
            buildTasks.Add(new PostScriptsCallback());

            // Dependency
            buildTasks.Add(new CalculateSceneDependencyData());
#if UNITY_2019_3_OR_NEWER
            buildTasks.Add(new CalculateCustomDependencyData());
#endif
            buildTasks.Add(new CalculateAssetDependencyData());
            buildTasks.Add(new StripUnusedSpriteSources());
            buildTasks.Add(new PostDependencyCallback());

            // Packing
            buildTasks.Add(new GenerateBundlePacking());
            buildTasks.Add(new GenerateBundleCommands());
            buildTasks.Add(new GenerateSubAssetPathMaps());
            buildTasks.Add(new GenerateBundleMaps());
            buildTasks.Add(new PostPackingCallback());

            // Writing
            buildTasks.Add(new WriteSerializedFiles());
            buildTasks.Add(new ArchiveAndCompressBundles());
            buildTasks.Add(new AppendBundleHash());
            buildTasks.Add(new GenerateLinkXml());
            buildTasks.Add(new PostWritingCallback());

            // Generate manifest files
            // TODO: IMPL manifest generation

            return buildTasks;
        }

        static IList<IBuildTask> AssetBundleBuiltInShaderExtraction()
        {
            var buildTasks = new List<IBuildTask>();

            // Setup
            buildTasks.Add(new SwitchToBuildPlatform());
            buildTasks.Add(new RebuildSpriteAtlasCache());

            // Player Scripts
            buildTasks.Add(new BuildPlayerScripts());
            buildTasks.Add(new PostScriptsCallback());

            // Dependency
            buildTasks.Add(new CalculateSceneDependencyData());
#if UNITY_2019_3_OR_NEWER
            buildTasks.Add(new CalculateCustomDependencyData());
#endif
            buildTasks.Add(new CalculateAssetDependencyData());
            buildTasks.Add(new StripUnusedSpriteSources());
            buildTasks.Add(new CreateBuiltInShadersBundle("UnityBuiltInShaders.bundle"));
            buildTasks.Add(new PostDependencyCallback());

            // Packing
            buildTasks.Add(new GenerateBundlePacking());
            buildTasks.Add(new UpdateBundleObjectLayout());
            buildTasks.Add(new GenerateBundleCommands());
            buildTasks.Add(new GenerateSubAssetPathMaps());
            buildTasks.Add(new GenerateBundleMaps());
            buildTasks.Add(new PostPackingCallback());

            // Writing
            buildTasks.Add(new WriteSerializedFiles());
            buildTasks.Add(new ArchiveAndCompressBundles());
            buildTasks.Add(new AppendBundleHash());
            buildTasks.Add(new GenerateLinkXml());
            buildTasks.Add(new PostWritingCallback());

            // Generate manifest files
            // TODO: IMPL manifest generation

            return buildTasks;
        }
    }
}
