using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;

namespace UnityEditor.Build.Pipeline.Tests
{
    public class ArchiveAndCompressTests : ArchiveAndCompressTestFixture
    {
        [Test]
        public void WhenAssetInBundleHasDependencies_DependenciesAreInDetails()
        {
            ArchiveAndCompressBundles.TaskInput input = GetDefaultInput();
            AddSimpleBundle(input, "mybundle", "internalName");
            AddSimpleBundle(input, "mybundle2", "internalName2");
            AddSimpleBundle(input, "mybundle3", "internalName3");

            input.AssetToFilesDependencies.Add(new GUID(), new List<string>() { "internalName", "internalName2" });
            input.AssetToFilesDependencies.Add(GUID.Generate(), new List<string>() { "internalName", "internalName3" });

            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output);

            Assert.AreEqual(2, output.BundleDetails["mybundle"].Dependencies.Length);
            Assert.AreEqual("mybundle2", output.BundleDetails["mybundle"].Dependencies[0]);
            Assert.AreEqual("mybundle3", output.BundleDetails["mybundle"].Dependencies[1]);
        }

        [Test]
        public void WhenBundleDoesNotHaveDependencies_DependenciesAreNotInDetails()
        {
            ArchiveAndCompressBundles.TaskInput input = GetDefaultInput();
            AddSimpleBundle(input, "mybundle", "internalName");
            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output);
            Assert.AreEqual(0, output.BundleDetails["mybundle"].Dependencies.Length);
        }

        public class RebuildTestContext
        {
            internal ArchiveAndCompressBundles.TaskInput input;
            internal ArchiveAndCompressTests _this;
        };

        public static IEnumerable RebuildTestCases
        {
            get
            {
                yield return new TestCaseData(false, new Action<RebuildTestContext>((ctx) => {})).SetName("NoChanges");
                yield return new TestCaseData(true, new Action<RebuildTestContext>((ctx) =>
                {
                    ctx.input.InternalFilenameToWriteMetaData["internalName"] = new SerializedFileMetaData() { ContentHash = new Hash128(0, 1), RawFileHash = new Hash128(1, 2) };
                })).SetName("SourceFileHashChanges");
                yield return new TestCaseData(true, new Action<RebuildTestContext>((ctx) =>
                {
                    ctx.input.GetCompressionForIdentifier = (x) => UnityEngine.BuildCompression.Uncompressed;
                })).SetName("CompressionChanges");
#if UNITY_2019_3_OR_NEWER
                yield return new TestCaseData(true, new Action<RebuildTestContext>((ctx) =>
                {
                    ctx._this.AddRawFileThatTargetsBundle(ctx.input, "mybundle", "rawInternalName");
                })).SetName("AddAdditionalFile");
#endif
            }
        }

        [Test, TestCaseSource(typeof(ArchiveAndCompressTests), "RebuildTestCases")]
        public void WhenInputsChanges_AndRebuilt_CachedDataIsUsedAsExpected(bool shouldRebuild, Action<RebuildTestContext> postFirstBuildAction)
        {
            BuildCache.PurgeCache(false);
            using (BuildCache cache = new BuildCache())
            {
                RebuildTestContext ctx = new RebuildTestContext();
                ctx.input = GetDefaultInput();
                ctx._this = this;
                ctx.input.BuildCache = cache;

                AddSimpleBundle(ctx.input, "mybundle", "internalName");

                ArchiveAndCompressBundles.Run(ctx.input, out ArchiveAndCompressBundles.TaskOutput output);
                cache.SyncPendingSaves();
                Assert.AreEqual(0, ctx.input.OutCachedBundles.Count);

                postFirstBuildAction(ctx);

                ctx.input.OutCachedBundles.Clear();
                ArchiveAndCompressBundles.Run(ctx.input, out ArchiveAndCompressBundles.TaskOutput outputReRun);

                if (shouldRebuild)
                    Assert.AreEqual(0, ctx.input.OutCachedBundles.Count);
                else
                    Assert.AreEqual(1, ctx.input.OutCachedBundles.Count);
            }
        }

        [Test]
        public void WhenArchiveIsAlreadyBuilt_CachedVersionIsUsed()
        {
            string bundleOutDir1 = Path.Combine(m_TestTempDir, "bundleoutdir1");
            string bundleOutDir2 = Path.Combine(m_TestTempDir, "bundleoutdir2");
            Directory.CreateDirectory(bundleOutDir1);
            Directory.CreateDirectory(bundleOutDir2);
            ArchiveAndCompressBundles.TaskInput input = GetDefaultInput();
            BuildCache cache = new BuildCache();
            input.BuildCache = cache;
            AddSimpleBundle(input, "mybundle", "internalName");
            input.GetOutputFilePathForIdentifier = (x) => Path.Combine(bundleOutDir1, x);
            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output);
            Assert.AreEqual(0, input.OutCachedBundles.Count);
            cache.SyncPendingSaves();

            input.GetOutputFilePathForIdentifier = (x) => Path.Combine(bundleOutDir2, x);
            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output2);
            Assert.AreEqual(1, input.OutCachedBundles.Count);
            Assert.AreEqual("mybundle", input.OutCachedBundles[0]);
            AssertDirectoriesEqual(bundleOutDir1, bundleOutDir2, 1);
        }

        [Test]
        public void WhenArchiveIsCached_AndRebuildingArchive_HashIsAssignedToOutput()
        {
            string bundleName = "mybundle";
            ArchiveAndCompressBundles.TaskInput input = GetDefaultInput();
            BuildCache cache = new BuildCache();
            input.BuildCache = cache;
            AddSimpleBundle(input, bundleName, "internalName");
            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output);
            var hash = output.BundleDetails[bundleName].Hash;
            Assert.AreNotEqual(Hash128.Parse(""), output.BundleDetails[bundleName].Hash);
            cache.SyncPendingSaves();

            //Now our bundle is cached so we'll run again and make sure the hashes are non-zero and equal
            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output2);
            Assert.AreEqual(hash, output2.BundleDetails[bundleName].Hash);
        }

        public class ContentHashTestContext
        {
            internal ArchiveAndCompressBundles.TaskInput input;
            internal GUID assetGUID;
            internal ArchiveAndCompressTests _this;
        };

        public static IEnumerable ContentHashTestCases
        {
            get
            {
                yield return new TestCaseData(true, new Action<ContentHashTestContext>((ctx) =>
                {
                    ctx.input.AssetToFilesDependencies[ctx.assetGUID] = new List<string>() { "internalName", "internalName3" };
                })).SetName("DependencyChanges");
                yield return new TestCaseData(true, new Action<ContentHashTestContext>((ctx) =>
                {
                    ctx.input.InternalFilenameToWriteMetaData["internalName"].ContentHash = new Hash128(128, 128);
                })).SetName("ContentHashChanges");
                yield return new TestCaseData(false, new Action<ContentHashTestContext>((ctx) =>
                {
                    ctx.input.InternalFilenameToWriteMetaData["internalName"].RawFileHash = new Hash128(128, 128);
                })).SetName("RawHashChanges");
            }
        }

        [Test, TestCaseSource(typeof(ArchiveAndCompressTests), "ContentHashTestCases")]
        public void WhenInputsChange_BundleOutputHashIsAffectedAsExpected(bool hashShouldChange, Action<ContentHashTestContext> postFirstBuildAction)
        {
            ContentHashTestContext ctx = new ContentHashTestContext();
            ctx.input = GetDefaultInput();
            WriteResult result = AddSimpleBundle(ctx.input, "mybundle", "internalName");
            WriteResult result2 = AddSimpleBundle(ctx.input, "mybundle2", "internalName2");
            WriteResult result3 = AddSimpleBundle(ctx.input, "mybundle3", "internalName3");
            ctx.assetGUID = GUID.Generate();
            ctx.input.AssetToFilesDependencies.Add(ctx.assetGUID, new List<string>() { "internalName", "internalName2" });

            ArchiveAndCompressBundles.Run(ctx.input, out ArchiveAndCompressBundles.TaskOutput output);

            postFirstBuildAction(ctx);

            ArchiveAndCompressBundles.Run(ctx.input, out ArchiveAndCompressBundles.TaskOutput output2);

            Hash128 prevHash = output.BundleDetails["mybundle"].Hash;
            Hash128 newHash = output2.BundleDetails["mybundle"].Hash;
            if (hashShouldChange)
                Assert.AreNotEqual(prevHash, newHash);
            else
                Assert.AreEqual(prevHash, newHash);
        }

#if UNITY_2019_3_OR_NEWER
        [Test]
        public void WhenBuildingManyArchives_ThreadedAndNonThreadedResultsAreIdentical()
        {
            const int kBundleCount = 100;
            ArchiveAndCompressBundles.TaskInput input = GetDefaultInput();

            for (int i = 0; i < kBundleCount; i++)
                AddSimpleBundle(input, $"mybundle{i}", $"internalName{i}");

            input.Threaded = false;
            input.GetOutputFilePathForIdentifier = (x) => Path.Combine(m_TestTempDir, "bundleoutdir_nothreading", x);
            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output1);

            input.Threaded = true;
            input.GetOutputFilePathForIdentifier = (x) => Path.Combine(m_TestTempDir, "bundleoutdir_threading", x);
            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output2);

            AssertDirectoriesEqual(Path.Combine(m_TestTempDir, "bundleoutdir_nothreading"), Path.Combine(m_TestTempDir, "bundleoutdir_threading"), kBundleCount);
        }

#endif

        // Start is called before the first frame update
        [Test]
        public void ResourceFilesAreAddedToBundles()
        {
            ArchiveAndCompressBundles.TaskInput input = GetDefaultInput();
            string bundleOutDir = Path.Combine(m_TestTempDir, "bundleoutdir");

            AddSimpleBundle(input, "mybundle", "internalName");

            string srcFile = input.InternalFilenameToWriteResults["internalName"].resourceFiles[0].fileName;

            ReturnCode code = ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output);
            Assert.AreEqual(ReturnCode.Success, code);

            string[] files = RunWebExtract(Path.Combine(bundleOutDir, "mybundle"));
            Assert.AreEqual(1, files.Length);
            FileAssert.AreEqual(files[0], srcFile);
        }

        [Test]
        public void WhenBuildingArchive_BuildLogIsPopulated()
        {
            ArchiveAndCompressBundles.TaskInput input = GetDefaultInput();
            var log = new BuildLog();
            input.Log = log;
            AddSimpleBundle(input, "mybundle", "internalName");
            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output);
            var node = log.Root.Children.Find((x) => x.Name.StartsWith("ArchiveItems"));
            Assert.IsNotNull(node);
        }

        class ScopeCapturer : IBuildLogger
        {
            public List<string> Scopes = new List<string>();
            public void AddEntry(LogLevel level, string msg)
            {
            }

            public void BeginBuildStep(LogLevel level, string stepName, bool subStepsCanBeThreaded)
            {
                lock (Scopes)
                {
                    Scopes.Add(stepName);
                }
            }

            public bool ContainsScopeWithSubstring(string subString)
            {
                return Scopes.Count((x) => x.Contains(subString)) != 0;
            }

            public void EndBuildStep()
            {
            }
        }

        private void AddSimpleBundleAndBuild(out ArchiveAndCompressBundles.TaskInput input, out string bundleBuildDir)
        {
            ScopeCapturer log1 = new ScopeCapturer();
            string bDir = Path.Combine(m_TestTempDir, "bundleoutdir1");
            Directory.CreateDirectory(bDir);
            input = GetDefaultInput();
            BuildCache cache = new BuildCache();
            input.BuildCache = cache;
            input.Log = log1;
            AddSimpleBundle(input, "mybundle", "internalName");
            input.GetOutputFilePathForIdentifier = (x) => Path.Combine(bDir, x);
            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output);
            Assert.AreEqual(0, input.OutCachedBundles.Count);
            Assert.IsTrue(log1.ContainsScopeWithSubstring("Copying From Cache"));
            cache.SyncPendingSaves();
            bundleBuildDir = bDir;
        }

        [Test]
        public void WhenArchiveIsAlreadyBuilt_AndArchiveIsInOutputDirectory_ArchiveIsNotCopied()
        {
            AddSimpleBundleAndBuild(out ArchiveAndCompressBundles.TaskInput input, out string bundleOutDir1);
            ScopeCapturer log2 = new ScopeCapturer();

            input.GetOutputFilePathForIdentifier = (x) => Path.Combine(bundleOutDir1, x);
            input.Log = log2;
            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output2);
            Assert.AreEqual(1, input.OutCachedBundles.Count);
            Assert.AreEqual("mybundle", input.OutCachedBundles[0]);
            Assert.IsFalse(log2.ContainsScopeWithSubstring("Copying From Cache"));
        }

        [Test]
        public void WhenArchiveIsAlreadyBuilt_AndArchiveIsInOutputDirectoryButTimestampMismatch_ArchiveIsCopied()
        {
            AddSimpleBundleAndBuild(out ArchiveAndCompressBundles.TaskInput input, out string bundleOutDir1);

            // Change the creation timestamp on the bundles
            string bundlePath = Path.Combine(bundleOutDir1, "mybundle");
            File.SetLastWriteTime(bundlePath, new DateTime(2019, 1, 1));

            ScopeCapturer log2 = new ScopeCapturer();

            input.GetOutputFilePathForIdentifier = (x) => Path.Combine(bundleOutDir1, x);
            input.Log = log2;
            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output2);

            Assert.AreEqual(1, input.OutCachedBundles.Count);
            Assert.AreEqual("mybundle", input.OutCachedBundles[0]);
            Assert.IsTrue(log2.ContainsScopeWithSubstring("Copying From Cache"));
        }

#if UNITY_2019_3_OR_NEWER
        [Test]
        public void CanAddRawFilesToBundles()
        {
            ArchiveAndCompressBundles.TaskInput input = GetDefaultInput();
            AddSimpleBundle(input, "mybundle", "internalName");
            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output);

            AddRawFileThatTargetsBundle(input, "mybundle", "rawName");
            ArchiveAndCompressBundles.Run(input, out ArchiveAndCompressBundles.TaskOutput output2);
            Assert.IsTrue(output.BundleDetails["mybundle"].Hash.isValid);
            Assert.IsTrue(output2.BundleDetails["mybundle"].Hash.isValid);
            Assert.AreNotEqual(output.BundleDetails["mybundle"].Hash, output2.BundleDetails["mybundle"].Hash);
        }

        [Test]
        public void SupportsMultiThreadedArchiving_WhenEditorIs20193OrLater_IsTrue()
        {
            Assert.IsTrue(ReflectionExtensions.SupportsMultiThreadedArchiving);
        }

#else
        [Test]
        public void SupportsMultiThreadedArchiving_WhenEditorIsBefore20193_IsFalse()
        {
            Assert.IsFalse(ReflectionExtensions.SupportsMultiThreadedArchiving);
        }

#endif
    }
}
