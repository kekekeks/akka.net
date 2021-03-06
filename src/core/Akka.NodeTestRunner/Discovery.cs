﻿using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Akka.NodeTestRunner
{
    [Serializable]
    public class Discovery : TestMessageVisitor<IDiscoveryCompleteMessage>
    {
        private readonly string _assemblyName;
        private readonly string _className;
        public List<ITestCase> TestCases { get; private set; }

        public Discovery(string assemblyName, string className)
        {
            _assemblyName = assemblyName;
            _className = className;
            TestCases = new List<ITestCase>();
        }

        protected override bool Visit(ITestCaseDiscoveryMessage discovery)
        {
            var name = discovery.TestAssembly.Assembly.AssemblyPath.Split('\\').Last();
            if (!name.Equals(_assemblyName, StringComparison.OrdinalIgnoreCase))
                return true;

            var testName = discovery.TestClass.Class.Name;
            if (testName.Equals(_className, StringComparison.OrdinalIgnoreCase))
            {
                TestCases.Add(discovery.TestCase);
            }
            return true;
        }
    }
}