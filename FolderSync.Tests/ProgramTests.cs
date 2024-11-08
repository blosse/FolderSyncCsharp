using Xunit;

namespace FolderSync.Tests
{
    public class ProgramTests : IDisposable
    {
        private string tempDirA;
        private string tempDirB;
        private string tempLogFile;

        public ProgramTests()
        {
            tempDirA = Path.Combine(Path.GetTempPath(), "FolderA");
            tempDirB = Path.Combine(Path.GetTempPath(), "FolderB");
            logFile = Path.Combine(Path.GetTempPath(), "sync.log");

            Directory.CreateDirectory(tempDirA);
            Directory.CreateDirectory(tempDirB);
        }

        [Fact]
        public void ComputeHash_ShouldComputeCorrectHash()
        {
            var testFile = Path.Combine(tempDirA, "testsson.txt");
            File.WriteAllText(testFile, "hashelly-hash-dash");

            var hash1 = Program.ComputeHash(testFile);
            var hash2 = Program.ComputeHash(testFile);

            Assert.Equal(hash1, hash2); // Make sure hashes are equal
        }

        [Fact]
        public void DeleteDir_ShouldDeleteDirectoryAndFiles()
        {
            var subDir = Path.Combine(tempDirA, "SubDir");
            Directory.CreateDirectory(subDir);
            var testFile = Path.Combine(subDir, "test.txt");
            File.WriteAllText(testFile, "Hello World");

            Program.DeleteDir(subDir, logFile);

            Assert.False(Directory.Exists(subDir));
        }

        [Fact]
        public void SyncDir_ShouldCopyFileFromSourceToReplica()
        {
            var sourceFile = Path.Combine(tempDirA, "test.txt");
            File.WriteAllText(sourceFile, "Hello World");
            var replicaFile = Path.Combine(tempDirB, "test.txt");

            Program.SyncDir(tempDirA, tempDirB, logFile);

            Assert.True(File.Exists(replicaFile));
            Assert.Equal(File.ReadAllText(sourceFile), File.ReadAllText(replicaFile));
        }

        [Fact]
        public void CleanDir_ShouldRemoveExtraFilesFromReplica()
        {
            var extraFile = Path.Combine(tempDirB, "extra.txt");
            File.WriteAllText(extraFile, "Extra file in replica");

            Program.CleanDir(tempDirA, tempDirB, logFile);

            Assert.False(File.Exists(extraFile));
        }

        [Fact]
        public void WalkDirectories_ShouldSyncAllSubdirectories()
        {
            var sourceSubDir = Path.Combine(tempDirA, "SubDir");
            Directory.CreateDirectory(sourceSubDir);
            var testFile = Path.Combine(sourceSubDir, "test.txt");
            File.WriteAllText(testFile, "Subdirectory file");

            Program.WalkDirectories(tempDirA, tempDirB, logFile);

            var replicaSubDir = Path.Combine(tempDirB, "SubDir");
            var replicaFile = Path.Combine(replicaSubDir, "test.txt");
            Assert.True(File.Exists(replicaFile));
            Assert.Equal(File.ReadAllText(testFile), File.ReadAllText(replicaFile));
        }

        // Clean up after each test
        public void Dispose()
        {
            if (Directory.Exists(tempDirA)) Directory.Delete(tempDirA, true);
            if (Directory.Exists(tempDirB)) Directory.Delete(tempDirB, true);
            if (File.Exists(logFile)) File.Delete(logFile);
        }
    }
}
