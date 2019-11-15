using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace DeltaCompressionDotNet.Tests
{
    public class CompressionTests<TDeltaCompression>
        where TDeltaCompression : IDeltaCompression, new()
    {
        private const string CalcFileName = "Calc.exe";
        private const string NotepadFileName = "Notepad.exe";
        private const string MediaFolderName = "Media";
        private const string Alarm01FileName = "Alarm01.wav";
        private const string Alarm02FileName = "Alarm02.wav";

        private readonly List<CompressionTest> _compressionTests = new List<CompressionTest>();

        [SetUp]
        public void TestInitialize()
        {
            Assert.True(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), "Test not being run on Windows, this is a windows only api");
            
            _compressionTests.Clear();

            var systemFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.System);

            _compressionTests.Add(new CompressionTest(systemFolderPath, CalcFileName, NotepadFileName));

            var windowsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            var mediaFolderPath = Path.Combine(windowsFolderPath, MediaFolderName);

            _compressionTests.Add(new CompressionTest(mediaFolderPath, Alarm01FileName, Alarm02FileName));
        }

        [TearDown]
        public void TestCleanup()
        {
            foreach (var compressionTest in _compressionTests)
                compressionTest.TestCleanup();
        }

        [Test]
        public void ApplyDelta_Calls_GetLastError()
        {
            var deltaCompression = new TDeltaCompression();

            try
            {
                deltaCompression.ApplyDelta(null, null, null);
            }
            catch (Win32Exception exception)
            {
                Assert.AreNotEqual(0, exception.NativeErrorCode);
            }
        }

        [Test]
        public void CreateDelta_Calls_GetLastError()
        {
            var deltaCompression = new TDeltaCompression();

            try
            {
                deltaCompression.CreateDelta(null, null, null);
            }
            catch (Win32Exception exception)
            {
                Assert.AreNotEqual(0, exception.NativeErrorCode);
            }
        }

        [Test]
        public void CreateDelta_And_ApplyDelta_Creates_And_Applies_Delta()
        {
            var deltaCompression = new TDeltaCompression();

            foreach (var compressionTest in _compressionTests)
                compressionTest.CreateAndApplyDelta(deltaCompression);
        }

        [Test]
        public void CreateDelta_And_ApplyDelta_Handle_Big_Files()
        {
            var baseFolderPath = Path.GetTempPath();

            using (var tempFile1 = new RandomFile(baseFolderPath))
            using (var tempFile2 = new RandomFile(baseFolderPath))
            {
                var compressionTest = new CompressionTest(baseFolderPath, tempFile1.FileName, tempFile2.FileName);

                try
                {
                    var deltaCompression = new TDeltaCompression();

                    compressionTest.CreateAndApplyDelta(deltaCompression);
                }
                finally
                {
                    compressionTest.TestCleanup();
                }
            }
        }
    }
}