﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerApps.TestEngine.Config;
using Microsoft.PowerApps.TestEngine.Reporting;
using Microsoft.PowerApps.TestEngine.System;
using Moq;
using Xunit;

namespace Microsoft.PowerApps.TestEngine.Tests
{
    public class TestEngineTests
    {
        private Mock<ITestState> MockState;
        private Mock<ITestReporter> MockTestReporter;
        private Mock<IFileSystem> MockFileSystem;
        private Mock<ISingleTestRunner> MockSingleTestRunner;
        private IServiceProvider ServiceProvider;
        private Mock<ILogger> MockLogger;

        public TestEngineTests()
        {
            MockState = new Mock<ITestState>(MockBehavior.Strict);
            MockTestReporter = new Mock<ITestReporter>(MockBehavior.Strict);
            MockFileSystem = new Mock<IFileSystem>(MockBehavior.Strict);
            MockSingleTestRunner = new Mock<ISingleTestRunner>(MockBehavior.Strict);
            MockLogger = new Mock<ILogger>(MockBehavior.Loose);
            ServiceProvider = new ServiceCollection()
                            .AddSingleton(MockSingleTestRunner.Object)
                            .BuildServiceProvider();
        }

        [Fact]
        public async Task TestEngineWithDefaultParamsTest()
        {
            var testSettings = new TestSettings()
            {
                WorkerCount = 2,
                BrowserConfigurations = new List<BrowserConfiguration>()
                {
                    new BrowserConfiguration()
                    {
                        Browser = "Chromium"
                    }
                }
            };
            var testSuiteDefinition = new TestSuiteDefinition()
            {
                TestSuiteName = "Test1",
                TestSuiteDescription = "First test",
                AppLogicalName = "logicalAppName1",
                Persona = "User1",
                TestCases = new List<TestCase>()
                {
                    new TestCase
                    {
                        TestCaseName = "Test Case Name",
                        TestCaseDescription = "Test Case Description",
                        TestSteps = "Assert(1 + 1 = 2, \"1 + 1 should be 2 \")"
                    }
                }
            };
            var testConfigFile = "C:\\testPlan.fx.yaml";
            var environmentId = "defaultEnviroment";
            var tenantId = "tenantId";
            var testRunId = Guid.NewGuid().ToString();
            var expectedOutputDirectory = "TestOutput";
            var testRunDirectory = Path.Combine(expectedOutputDirectory, testRunId.Substring(0, 6));
            var expectedCloud = "Prod";

            var expectedTestReportPath = "C:\\test.trx";

            SetupMocks(expectedOutputDirectory, testSettings, testSuiteDefinition, testRunId, expectedTestReportPath);

            var testEngine = new TestEngine(MockState.Object, ServiceProvider, MockTestReporter.Object, MockFileSystem.Object);
            var testReportPath = await testEngine.RunTestAsync(testConfigFile, environmentId, tenantId);

            Assert.Equal(expectedTestReportPath, testReportPath);

            Verify(testConfigFile, environmentId, tenantId, expectedCloud, expectedOutputDirectory, testRunId, testRunDirectory, testSuiteDefinition, testSettings);
        }

        [Fact]
        public async Task RunWorkerCountWithDefaultParamsTest()
        {
            var testSettings = new TestSettings()
            {
                WorkerCount = 2,
                BrowserConfigurations = new List<BrowserConfiguration>()
                {
                    new BrowserConfiguration()
                    {
                        Browser = "Chromium"
                    }
                }
            };
            var testSuiteDefinition = new TestSuiteDefinition()
            {
                TestSuiteName = "Test1",
                TestSuiteDescription = "First test",
                AppLogicalName = "logicalAppName1",
                Persona = "User1",
                TestCases = new List<TestCase>()
                {
                    new TestCase
                    {
                        TestCaseName = "Test Case Name",
                        TestCaseDescription = "Test Case Description",
                        TestSteps = "Assert(1 + 1 = 2, \"1 + 1 should be 2 \")"
                    }
                }
            };

            var testRunId = Guid.NewGuid().ToString();
            var expectedOutputDirectory = "TestOutput";
            var testRunDirectory = Path.Combine(expectedOutputDirectory, testRunId.Substring(0, 6));

            var expectedTestReportPath = "C:\\test.trx";

            SetupMocks(expectedOutputDirectory, testSettings, testSuiteDefinition, testRunId, expectedTestReportPath);

            var testEngine = new TestEngine(MockState.Object, ServiceProvider, MockTestReporter.Object, MockFileSystem.Object);
            await testEngine.RunTestByWorkerCountAsync(testRunId, testRunDirectory);

            foreach (var browserConfig in testSettings.BrowserConfigurations)
            {
                MockSingleTestRunner.Verify(x => x.RunTestAsync(testRunId, testRunDirectory, testSuiteDefinition, browserConfig), Times.Once());
            }
        }

        [Theory]
        [ClassData(typeof(TestDataGenerator))]
        public async Task TestEngineTest(string outputDirectory, string cloud, TestSettings testSettings, TestSuiteDefinition testSuiteDefinition)
        {
            var testConfigFile = "C:\\testPlan.fx.yaml";
            var environmentId = "defaultEnviroment";
            var tenantId = "tenantId";
            var testRunId = Guid.NewGuid().ToString();
            var expectedOutputDirectory = outputDirectory;
            if (string.IsNullOrEmpty(expectedOutputDirectory))
            {
                expectedOutputDirectory = "TestOutput";
            }
            var testRunDirectory = Path.Combine(expectedOutputDirectory, testRunId.Substring(0, 6));
            var expectedCloud = cloud;
            if (string.IsNullOrEmpty(expectedCloud))
            {
                expectedCloud = "Prod";
            }

            var expectedTestReportPath = "C:\\test.trx";

            SetupMocks(expectedOutputDirectory, testSettings, testSuiteDefinition, testRunId, expectedTestReportPath);

            var testEngine = new TestEngine(MockState.Object, ServiceProvider, MockTestReporter.Object, MockFileSystem.Object);
            var testReportPath = await testEngine.RunTestAsync(testConfigFile, environmentId, tenantId, outputDirectory, cloud);

            Assert.Equal(expectedTestReportPath, testReportPath);

            Verify(testConfigFile, environmentId, tenantId, expectedCloud, expectedOutputDirectory, testRunId, testRunDirectory, testSuiteDefinition, testSettings);
        }

        private void SetupMocks(string outputDirectory, TestSettings testSettings, TestSuiteDefinition testSuiteDefinition, string testRunId, string testReportPath)
        {
            MockState.Setup(x => x.ParseAndSetTestState(It.IsAny<string>()));
            MockState.Setup(x => x.SetEnvironment(It.IsAny<string>()));
            MockState.Setup(x => x.SetTenant(It.IsAny<string>()));
            MockState.Setup(x => x.SetCloud(It.IsAny<string>()));
            MockState.Setup(x => x.SetOutputDirectory(It.IsAny<string>()));
            MockState.Setup(x => x.GetOutputDirectory()).Returns(outputDirectory);
            MockState.Setup(x => x.GetTestSettings()).Returns(testSettings);
            MockState.Setup(x => x.GetTestSuiteDefinition()).Returns(testSuiteDefinition);
            MockState.Setup(x => x.GetWorkerCount()).Returns(testSettings.WorkerCount);

            MockTestReporter.Setup(x => x.CreateTestRun(It.IsAny<string>(), It.IsAny<string>())).Returns(testRunId);
            MockTestReporter.Setup(x => x.StartTestRun(It.IsAny<string>()));
            MockTestReporter.Setup(x => x.EndTestRun(It.IsAny<string>()));
            MockTestReporter.Setup(x => x.GenerateTestReport(It.IsAny<string>(), It.IsAny<string>())).Returns(testReportPath);

            MockFileSystem.Setup(x => x.CreateDirectory(It.IsAny<string>()));

            MockSingleTestRunner.Setup(x => x.RunTestAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TestSuiteDefinition>(), It.IsAny<BrowserConfiguration>())).Returns(Task.CompletedTask);
        }


        private void Verify(string testConfigFile, string environmentId, string tenantId, string cloud,
            string outputDirectory, string testRunId, string testRunDirectory, TestSuiteDefinition testSuiteDefinition, TestSettings testSettings)
        {
            MockState.Verify(x => x.ParseAndSetTestState(testConfigFile), Times.Once());
            MockState.Verify(x => x.SetEnvironment(environmentId), Times.Once());
            MockState.Verify(x => x.SetTenant(tenantId), Times.Once());
            MockState.Verify(x => x.SetCloud(cloud), Times.Once());
            MockState.Verify(x => x.SetOutputDirectory(outputDirectory), Times.Once());

            MockTestReporter.Verify(x => x.CreateTestRun("Power Fx Test Runner", "User"), Times.Once());
            MockTestReporter.Verify(x => x.StartTestRun(testRunId), Times.Once());

            MockFileSystem.Verify(x => x.CreateDirectory(testRunDirectory), Times.Once());

            foreach (var browserConfig in testSettings.BrowserConfigurations)
            {
                MockSingleTestRunner.Verify(x => x.RunTestAsync(testRunId, testRunDirectory, testSuiteDefinition, browserConfig), Times.Once());
            }

            MockTestReporter.Verify(x => x.EndTestRun(testRunId), Times.Once());
            MockTestReporter.Verify(x => x.GenerateTestReport(testRunId, testRunDirectory), Times.Once());
        }

        [Theory]
        [InlineData("", "defaultEnvironment", "tenantId")]
        [InlineData("C:\\testPlan.fx.yaml", "", "tenantId")]
        [InlineData("C:\\testPlan.fx.yaml", "defaultEnvironment", "")]
        public async Task TestEngineThrowsOnNullArguments(string testConfigFile, string environmentId, string tenantId)
        {
            var testEngine = new TestEngine(MockState.Object, ServiceProvider, MockTestReporter.Object, MockFileSystem.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await testEngine.RunTestAsync(testConfigFile, environmentId, tenantId));
        }

        class TestDataGenerator : TheoryData<string, string, TestSettings, TestSuiteDefinition>
        {
            public TestDataGenerator()
            {
                // Simple test
                Add("C:\\testResults",
                    "GCC",
                    new TestSettings()
                    {
                        BrowserConfigurations = new List<BrowserConfiguration>()
                        {
                            new BrowserConfiguration()
                            {
                                Browser = "Chromium"
                            }
                        }
                    },
                    new TestSuiteDefinition()
                    {
                        TestSuiteName = "Test1",
                        TestSuiteDescription = "First test",
                        AppLogicalName = "logicalAppName1",
                        Persona = "User1",
                        TestCases = new List<TestCase>()
                        {
                            new TestCase
                            {
                                TestCaseName = "Test Case Name",
                                TestCaseDescription = "Test Case Description",
                                TestSteps = "Assert(1 + 1 = 2, \"1 + 1 should be 2 \")"
                            }
                        }
                    });

                // Simple test with null params
                Add(null,
                    null,
                    new TestSettings()
                    {
                        BrowserConfigurations = new List<BrowserConfiguration>()
                        {
                            new BrowserConfiguration()
                            {
                                Browser = "Chromium"
                            }
                        }
                    },
                    new TestSuiteDefinition()
                    {
                        TestSuiteName = "Test1",
                        TestSuiteDescription = "First test",
                        AppLogicalName = "logicalAppName1",
                        Persona = "User1",
                        TestCases = new List<TestCase>()
                        {
                            new TestCase
                            {
                                TestCaseName = "Test Case Name",
                                TestCaseDescription = "Test Case Description",
                                TestSteps = "Assert(1 + 1 = 2, \"1 + 1 should be 2 \")"
                            }
                        }
                    });

                // Simple test with empty string params
                Add("",
                    "",
                    new TestSettings()
                    {
                        BrowserConfigurations = new List<BrowserConfiguration>()
                        {
                            new BrowserConfiguration()
                            {
                                Browser = "Chromium"
                            }
                        }
                    },
                    new TestSuiteDefinition()
                    {
                        TestSuiteName = "Test1",
                        TestSuiteDescription = "First test",
                        AppLogicalName = "logicalAppName1",
                        Persona = "User1",
                        TestCases = new List<TestCase>()
                        {
                            new TestCase
                            {
                                TestCaseName = "Test Case Name",
                                TestCaseDescription = "Test Case Description",
                                TestSteps = "Assert(1 + 1 = 2, \"1 + 1 should be 2 \")"
                            }
                        }
                    });

                // Multiple browsers
                Add("C:\\testResults",
                    "Prod",
                    new TestSettings()
                    {
                        BrowserConfigurations = new List<BrowserConfiguration>()
                        {
                            new BrowserConfiguration()
                            {
                                Browser = "Chromium"
                            },
                            new BrowserConfiguration()
                            {
                                Browser = "Firefox"
                            },
                            new BrowserConfiguration()
                            {
                                Browser = "Chromium",
                                Device = "Pixel 2"
                            }
                        }
                    },
                    new TestSuiteDefinition()
                    {
                        TestSuiteName = "Test1",
                        TestSuiteDescription = "First test",
                        AppLogicalName = "logicalAppName1",
                        Persona = "User1",
                        TestCases = new List<TestCase>()
                        {
                            new TestCase
                            {
                                TestCaseName = "Test Case Name",
                                TestCaseDescription = "Test Case Description",
                                TestSteps = "Assert(1 + 1 = 2, \"1 + 1 should be 2 \")"
                            }
                        }
                    });

                // Multiple tests
                Add("C:\\testResults",
                    "Prod",
                    new TestSettings()
                    {
                        BrowserConfigurations = new List<BrowserConfiguration>()
                        {
                            new BrowserConfiguration()
                            {
                                Browser = "Chromium"
                            }
                        }
                    },
                    new TestSuiteDefinition()
                    {
                        TestSuiteName = "Test1",
                        TestSuiteDescription = "First test",
                        AppLogicalName = "logicalAppName1",
                        Persona = "User1",
                        TestCases = new List<TestCase>()
                        {
                            new TestCase
                            {
                                TestCaseName = "Test1",
                                TestCaseDescription = "First test",
                                TestSteps = "Assert(1 + 1 = 2, \"1 + 1 should be 2 \")"
                            },
                            new TestCase
                            {
                                TestCaseName = "Test2",
                                TestCaseDescription = "Second test",
                                TestSteps = "Assert(2 + 1 = 3, \"2 + 1 should be 3 \")"
                            }
                        }
                    });

                // Multiple tests and browsers
                Add("C:\\testResults",
                    "Prod",
                    new TestSettings()
                    {
                        BrowserConfigurations = new List<BrowserConfiguration>()
                        {
                            new BrowserConfiguration()
                            {
                                Browser = "Chromium"
                            },
                            new BrowserConfiguration()
                            {
                                Browser = "Firefox"
                            },
                            new BrowserConfiguration()
                            {
                                Browser = "Chromium",
                                Device = "Pixel 2"
                            }
                        }
                    },
                    new TestSuiteDefinition()
                    {
                        TestSuiteName = "Test1",
                        TestSuiteDescription = "First test",
                        AppLogicalName = "logicalAppName1",
                        Persona = "User1",
                        TestCases = new List<TestCase>()
                        {
                            new TestCase
                            {
                                TestCaseName = "Test1",
                                TestCaseDescription = "First test",
                                TestSteps = "Assert(1 + 1 = 2, \"1 + 1 should be 2 \")"
                            },
                            new TestCase
                            {
                                TestCaseName = "Test2",
                                TestCaseDescription = "Second test",
                                TestSteps = "Assert(2 + 1 = 3, \"2 + 1 should be 3 \")"
                            }
                        }
                    });
            }
        }
    }
}
