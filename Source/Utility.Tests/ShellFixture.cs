﻿using System;
using NUnit.Framework;	

using static Nake.Shell;

namespace Nake.Utility	
{
    public class ShellFixture
    {
        class UsingCmd
        {
            [Test]
            public void Runs_via_shell_interpreter()
            {
                var result = Cmd("echo 42");
                Assert.That(result.StdOut[0], Is.EqualTo("42"));
            }

            [Test]
            public void Non_zero_exit_code() => Assert.Throws<ApplicationException>(() =>
                Cmd("foo blah", quiet: true));

            [Test]
            public void Ignore_exit_code()
            {
                var (exitCode, stdOut, stdError) = Cmd("foo blah", ignoreExitCode: true, quiet: true);
                Assert.That(exitCode != 0);
                Assert.That(stdError.Count > 0);
                Assert.That(stdOut.Count == 0);
            }

            [Test]
            public void Bash_style_line_continuations()
            {
                var result = Cmd(@"dotnet \
                                    tool --help");

                Assert.That(result.ExitCode == 0);
                Assert.That(result.StdError.Count == 0);
                Assert.That(((string) result).Contains("Usage: dotnet tool"));

                result = Cmd(@"dotnet \
                                 tool \  
                                 list");

                Assert.That(result.ExitCode == 0);
                Assert.That(result.StdError.Count == 0);
                Assert.That(((string) result).Contains("---------------"));
            }
        }

        class UsingRun
        {
            [Test]
            [TestCase("d", "d")]
            [TestCase("dotnet pack", "dotnet", "pack")]
            [TestCase("dotnet   pack", "dotnet", "pack")]
            [TestCase("dotnet   pack -c  Release", "dotnet", "pack", "-c", "Release")]
            [TestCase(@"dotnet 'C:\Program Files'", "dotnet", @"C:\Program Files")]
            [TestCase(@"dotnet 'C:\Program Files'  pack", "dotnet", @"C:\Program Files", "pack")]
            [TestCase(@"dotnet  '''C:\Program Files''' pack", "dotnet", @"'C:\Program Files'", "pack")]
            [TestCase(@"dotnet   '''C:\Program'' Files'''  pack", "dotnet", @"'C:\Program' Files'", "pack")]
            public void Command_line_splitting(string command, params string[] args)
            {
                CollectionAssert.AreEqual(args, ToCommandLineArgs(command));
            }

            [Test]
            public void Unbalanced_quote()
            {
                const string command = @"dotnet '''C:\Program' Files'''  pack";
                var exception = Assert.Throws<Exception>(()=> ToCommandLineArgs(command));
                Assert.AreEqual("The command contains unbalanced quote at position 27", exception.Message);
            }
        }
    }	
}