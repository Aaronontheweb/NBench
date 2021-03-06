﻿// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Akka.Tests.Performance.Actor;
using FluentAssertions;
using NBench.Reporting;
using NBench.Sdk;
using Xunit;
using Xunit.Abstractions;

namespace NBench.Tests.End2End
{

    public class NBenchIntregrationTestWithDependenciesLoadAssembly
    {
        private readonly ITestOutputHelper _output;

        private readonly IBenchmarkOutput _benchmarkOutput;

        public NBenchIntregrationTestWithDependenciesLoadAssembly(ITestOutputHelper output)
        {
            _output = output;
            _benchmarkOutput = new XunitBenchmarkOutputHelper(output);
        }

        [Fact()]
        public void LoadAssemblyCorrect()
        {
            var package = LoadPackageWithDependencies().AddOutput(_benchmarkOutput);
            var result = TestRunner.Run(package);

            try
            {
                result.AllTestsPassed.Should().BeTrue("Expected all tests to pass, but did not.");
                result.ExecutedTestsCount.Should().NotBe(0);
                result.IgnoredTestsCount.Should().Be(0);
            }
            catch
            {
                foreach (var test in result.FullResults)
                {
                    _output.WriteLine($"DEBUG: Checking output for {test.BenchmarkName}");
                    foreach(var a in test.AssertionResults)
                        _output.WriteLine($"ASSERT: {a.MetricName} - Passed? {a.Passed}");
                }
                throw;
            }
        }

        private static TestPackage LoadPackageWithDependencies(IEnumerable<string> include = null, IEnumerable<string> exclude = null)
        {
            var package = NBenchRunner.CreateTest<ActorThroughputSpec>();

            if (include != null)
                foreach (var i in include)
                {
                    package.AddInclude(i);
                }

            if (exclude != null)
                foreach (var e in exclude)
                {
                    package.AddExclude(e);
                }

            // need to set this to true in order to resolve https://github.com/petabridge/NBench/issues/314
            package.Concurrent = true;

            return package;
        }
    }
}

