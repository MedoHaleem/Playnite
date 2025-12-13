using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Threading;
using System.IO;
using Playnite.SDK;

namespace Playnite.Common
{
    public class MonitorProcess
    {
        private readonly Process process;

        public MonitorProcess(Process process)
        {
            this.process = process;
        }

        public bool IsProcessRunning()
        {
            return !process.HasExited;
        }
    }

	    public class MonitorProcessTree
	    {
	        private HashSet<int> relatedIds = new HashSet<int>();

	        public MonitorProcessTree(int originalId)
	        {
	            relatedIds.Add(originalId);
	        }

	        public bool IsProcessTreeRunning()
	        {
	            if (relatedIds.Count == 0)
	            {
	                return false;
	            }

	            var runningIds = new HashSet<int>();
	            foreach (var proc in Process.GetProcesses())
	            {
	                using (proc)
	                {
	                    try
	                    {
	                        if (proc.SessionId == 0)
	                        {
	                            continue;
	                        }
	                    }
	                    catch
	                    {
	                        continue;
	                    }

	                    if (proc.TryGetParentId(out var parent))
	                    {
	                        if (relatedIds.Contains(parent))
	                        {
	                            relatedIds.Add(proc.Id);
	                        }
	                    }

	                    if (relatedIds.Contains(proc.Id))
	                    {
	                        runningIds.Add(proc.Id);
	                    }
	                }
	            }

	            relatedIds = runningIds;
	            return relatedIds.Count > 0;
	        }
	    }

	    public class MonitorProcessNames
	    {
	        private readonly ILogger logger = LogManager.GetLogger();
	        private readonly HashSet<string> procNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	        private readonly HashSet<string> procNamesNoExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	        public MonitorProcessNames(string directory)
	        {
	            var dir = directory;
	            try
            {
                dir = Paths.GetFinalPathName(directory);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to get target path for a directory {directory}");
	            }

	            if (FileSystem.DirectoryExists(dir))
	            {
	                try
	                {
	                    foreach (var exe in new SafeFileEnumerator(dir, "*.exe", SearchOption.AllDirectories))
	                    {
	                        var fileInfo = exe as FileInfo;
	                        if (fileInfo == null)
	                        {
	                            continue;
	                        }

	                        procNames.Add(fileInfo.Name);
	                        procNamesNoExt.Add(Path.GetFileNameWithoutExtension(fileInfo.Name));
	                    }
	                }
	                catch (Exception e)
	                {
	                    logger.Error(e, $"Failed to enumerate executables in {dir}");
	                }
	            }
	        }

	        public bool IsTrackable()
	        {
	            return procNamesNoExt.Count > 0;
	        }

		        public int IsProcessRunning()
		        {
		            foreach (var process in Process.GetProcesses())
		            {
		                using (process)
		                {
		                    try
		                    {
		                        if (process.SessionId == 0)
		                        {
		                            continue;
		                        }
		                    }
		                    catch
		                    {
		                        continue;
		                    }

		                    if (procNamesNoExt.Contains(process.ProcessName))
		                    {
		                        return process.Id;
		                    }

		                    if (process.TryGetMainModuleFileName(out var procPath) &&
		                        procNames.Contains(Path.GetFileName(procPath)))
		                    {
		                        return process.Id;
		                    }
		                }
		            }

		            return 0;
	        }
	    }

    public class MonitorDirectory
    {
        private readonly ILogger logger = LogManager.GetLogger();
        private readonly string dir;

        public MonitorDirectory(string directory)
        {
            dir = directory;

            try
            {
                dir = Paths.GetFinalPathName(directory);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to get target path for a directory {directory}");
            }
        }

        public bool IsTrackable()
        {
            if (dir.IsNullOrWhiteSpace())
            {
                return false;
            }

            return FileSystem.DirectoryExists(dir);
        }

	        public int IsProcessRunning()
	        {
	            foreach (var process in Process.GetProcesses())
	            {
	                using (process)
	                {
	                    try
	                    {
	                        if (process.SessionId == 0)
	                        {
	                            continue;
	                        }
	                    }
	                    catch
	                    {
	                        continue;
	                    }

	                    if (process.TryGetMainModuleFileName(out var procPath) &&
	                        procPath.IndexOf(dir, StringComparison.OrdinalIgnoreCase) >= 0)
	                    {
	                        return process.Id;
	                    }
	                }
	            }

	            return 0;
        }
    }
}

public class MonitorProcessName
{
    public string ProcessName { get; }

    public MonitorProcessName(string processName)
    {
        if (processName.IsNullOrWhiteSpace())
        {
            throw new Exception("Non empty process name must be specified.");
        }

        ProcessName = processName;
    }

	    public int IsProcessRunning()
	    {
	        foreach (var process in Process.GetProcesses())
	        {
	            using (process)
	            {
	                try
	                {
	                    if (process.SessionId == 0)
	                    {
	                        continue;
	                    }
	                }
	                catch
	                {
	                    continue;
	                }

	                if (process.ProcessName == ProcessName)
	                {
	                    return process.Id;
	                }
	            }
        }

        return 0;
    }
}
