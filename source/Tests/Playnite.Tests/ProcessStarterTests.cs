using NUnit.Framework;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
	    public class ProcessStarterTests
	    {
	        [Test]
	        public void StartProcessWaitTest()
	        {
	            var initialCount = Process.GetProcessesByName("notepad").Count();
	            var notepad = ProcessStarter.StartProcess("notepad");
	            Assert.AreEqual(initialCount + 1, Process.GetProcessesByName("notepad").Count());

	            var ivalidRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, "/f /pid 999999", null, true);
	            Assert.AreEqual(128, ivalidRes);

            var validRes = ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, $"/f /pid {notepad.Id}", null, true);
	            Assert.AreEqual(0, validRes);
	            Thread.Sleep(200);
	            Assert.AreEqual(initialCount, Process.GetProcessesByName("notepad").Count());
	        }

	        [Test]
	        public void ShellExecuteTest()
	        {
	            var initialIds = new HashSet<int>();
	            foreach (var proc in Process.GetProcessesByName("notepad"))
	            {
	                using (proc)
	                {
	                    initialIds.Add(proc.Id);
	                }
	            }

	            var procid = ProcessStarter.ShellExecute(@"notepad");
	            Assert.AreNotEqual(0, procid);

	            Thread.Sleep(500);
	            var newIds = new List<int>();
	            foreach (var proc in Process.GetProcessesByName("notepad"))
	            {
	                using (proc)
	                {
	                    if (!initialIds.Contains(proc.Id))
	                    {
	                        newIds.Add(proc.Id);
	                    }
	                }
	            }

	            Assert.IsTrue(newIds.Count > 0);

	            foreach (var id in newIds)
	            {
	                ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, $"/f /pid {id}", null, true);
	            }

	            Thread.Sleep(500);
	            var finalIds = new HashSet<int>();
	            foreach (var proc in Process.GetProcessesByName("notepad"))
	            {
	                using (proc)
	                {
	                    finalIds.Add(proc.Id);
	                }
	            }

	            Assert.IsTrue(finalIds.SetEquals(initialIds));
	        }

        [Test]
        public void StartProcessWaitStdTest()
        {
            ProcessStarter.StartProcessWait(CmdLineTools.IPConfig, null, null, out var stdOut, out var stdErr);
            StringAssert.Contains("Windows IP Configuration", stdOut);
            Assert.IsTrue(stdErr.IsNullOrEmpty());

            ProcessStarter.StartProcessWait(CmdLineTools.TaskKill, "/pid 999999", null, out var stdOut2, out var stdErr2);
            StringAssert.Contains("ERROR: The process", stdErr2);
            Assert.IsTrue(stdOut2.IsNullOrEmpty());
        }
    }
}
