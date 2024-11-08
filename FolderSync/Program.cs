using System.Security.Cryptography;
using System.Text;
// Program which performs one-way file synchronization between two folders
// All changes in folder A should be reflected in folder B
// Does not handle symlinks

public class Program
{
    public static string ComputeHash(string fileName)
    // Compute MD5 hash of a given file
    {
        using (MD5 md5 = MD5.Create())
        {
            using (FileStream stream = File.OpenRead(fileName))
            {
                byte[] hash = md5.ComputeHash(stream);
                StringBuilder sb = new StringBuilder();
                foreach (var bajt in hash)
                {
                    sb.Append(bajt);
                }
                return sb.ToString();
            }
        }
    }
    public static void DeleteDir(string targetDir, string logFile)
    // Recursivly deletes files and directories in a given directory
    // This is a little scary if run in the wrong directory...
    {
        foreach (var file in Directory.EnumerateFiles(targetDir))
        {
            File.Delete(file);
            File.AppendAllText(logFile, DateTime.Now.ToString() + " - Removed file: " + file + "\n");
        }
        foreach (var currentDir in Directory.EnumerateDirectories(targetDir))
        {
            DeleteDir(currentDir, logFile);
            File.AppendAllText(logFile, DateTime.Now.ToString() + " - Removed directory: " + currentDir + "\n");
        }
        Directory.Delete(targetDir);
    }
    public static void SyncDir(string source, string replica, string logFile)
    // Check that all files in source exist and are up to date in replica
    {
        foreach (string file in Directory.EnumerateFiles(source))
        {
            string filename = file.Replace(source + Path.DirectorySeparatorChar, null);
            string sourceFile = Path.Combine(source, filename);
            string replicaFile = Path.Combine(replica, filename);
            if (!File.Exists(replicaFile))
            {
                try
                {
                    File.Copy(sourceFile, replicaFile);
                    File.AppendAllText(logFile, DateTime.Now.ToString() + " - Created file: " + replicaFile + "\n");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error when copying file in SyncDir! " + ex.Message);
                }
            }
            else
            {
                string sourceHash = ComputeHash(sourceFile);
                string replicaHash = ComputeHash(replicaFile);
                if (sourceHash != replicaHash)
                {
                    try
                    {
                        File.Delete(replicaFile);
                        File.Copy(sourceFile, replicaFile);
                        File.AppendAllText(logFile, DateTime.Now.ToString() + " - Updated file: " + sourceFile + " in replica folder" + "\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error when replacing file in SyncDir! " + ex.Message);
                    }
                }
            }
        }
    }
    public static void CleanDir(string source, string replica, string logFile)
    // Remove files and directories from replica that do not exist in source
    {
        foreach (var file in Directory.EnumerateFiles(replica))
        {
            string filename = file.Replace(replica + Path.DirectorySeparatorChar, null);
            string sourceFile = Path.Combine(source, filename);
            string replicaFile = Path.Combine(replica, filename);
            if (!File.Exists(sourceFile))
            {
                try
                {
                    File.Delete(replicaFile);
                    File.AppendAllText(logFile, DateTime.Now.ToString() + " - Removed file: " + replicaFile + "\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error when deleting file in CleanDir! " + ex.Message);
                }
            }
        }
        foreach (var dir in Directory.EnumerateDirectories(replica))
        {
            string dirname = dir.Replace(replica + Path.DirectorySeparatorChar, null);
            string sourceDir = Path.Combine(source, dirname);
            string replicaDir = Path.Combine(replica, dirname);
            if (!Directory.Exists(sourceDir))
            {
                DeleteDir(replicaDir, logFile);
            }
        }
    }
    public static void WalkDirectories(string source, string replica, string logFile)
    // Recursivly parse the directories and update/remove files accordingly
    {
        SyncDir(source, replica, logFile);
        CleanDir(source, replica, logFile);
        foreach (var dir in Directory.EnumerateDirectories(source))
        {
            string replicaDir = dir.Replace(source, replica);
            if (!Directory.Exists(replicaDir))
            {
                try
                {
                    Directory.CreateDirectory(replicaDir);
                    File.AppendAllText(logFile, DateTime.Now.ToString() + " - Created directory: " + replicaDir + "\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error when creating dir in DirExists " + ex.Message);
                }
            }
            WalkDirectories(dir, replicaDir, logFile);
        }
    }

    static void Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Thats not how to use this little program!");
            Console.WriteLine("Args: <source> <replica> <logfile> <sync-interval (ms)>");
            return;
        }
        string logFile = args[2];
        int interval = Convert.ToInt32(args[3]); //Milliseconds

        if (!File.Exists(logFile))
        {
            File.Create(logFile);
        }
        File.AppendAllText(logFile, DateTime.Now.ToString() + " - Starting sync" + "\n");
        Console.WriteLine("Starting sync!");
        Console.WriteLine("...");
        // Run the thing
        while (true)
        {
            WalkDirectories(args[0], args[1], logFile);
            Thread.Sleep(interval);
        }
    }
}
