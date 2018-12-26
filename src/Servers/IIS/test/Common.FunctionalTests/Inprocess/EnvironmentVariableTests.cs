// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IIS.FunctionalTests.Utilities;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.AspNetCore.Testing.xunit;
using Xunit;

namespace Microsoft.AspNetCore.Server.IISIntegration.FunctionalTests
{
    [Collection(PublishedSitesCollection.Name)]

    public class EnvironmentVariableTests: IISFunctionalTestBase
    {
        private readonly PublishedSitesFixture _fixture;

        public EnvironmentVariableTests(PublishedSitesFixture fixture)
        {
            _fixture = fixture;
        }

        [ConditionalTheory]
        [InlineData(HostingModel.InProcess)]
        [InlineData(HostingModel.OutOfProcess)]
        public async Task GetLongEnvironmentVariable(HostingModel hostingModel)
        {
            var expectedValue = "AReallyLongValueThatIsGreaterThan300CharactersToForceResizeInNative" +
                                "AReallyLongValueThatIsGreaterThan300CharactersToForceResizeInNative" +
                                "AReallyLongValueThatIsGreaterThan300CharactersToForceResizeInNative" +
                                "AReallyLongValueThatIsGreaterThan300CharactersToForceResizeInNative" +
                                "AReallyLongValueThatIsGreaterThan300CharactersToForceResizeInNative" +
                                "AReallyLongValueThatIsGreaterThan300CharactersToForceResizeInNative";


            var deploymentParameters = _fixture.GetBaseDeploymentParameters(hostingModel, publish: true);
            deploymentParameters.WebConfigBasedEnvironmentVariables["ASPNETCORE_INPROCESS_TESTING_LONG_VALUE"] = expectedValue;

            Assert.Equal(
                expectedValue,
                await GetStringAsync(deploymentParameters, "/GetEnvironmentVariable?name=ASPNETCORE_INPROCESS_TESTING_LONG_VALUE"));
        }

        [ConditionalTheory]
        [InlineData(HostingModel.InProcess)]
        [InlineData(HostingModel.OutOfProcess)]
        public async Task AuthHeaderEnvironmentVariableRemoved(HostingModel hostingModel)
        {
            var deploymentParameters = _fixture.GetBaseDeploymentParameters(hostingModel, publish: true);
            deploymentParameters.WebConfigBasedEnvironmentVariables["ASPNETCORE_IIS_HTTPAUTH"] = "shouldberemoved";

            Assert.DoesNotContain("shouldberemoved", await GetStringAsync(deploymentParameters,"/GetEnvironmentVariable?name=ASPNETCORE_IIS_HTTPAUTH"));
        }

        [ConditionalTheory]
        [RequiresNewHandler]
        [InlineData(HostingModel.InProcess)]
        [InlineData(HostingModel.OutOfProcess)]
        [RequiresIIS(IISCapability.PoolEnvironmentVariables)]
        public async Task WebConfigOverridesGlobalEnvironmentVariables(HostingModel hostingModel)
        {
            var deploymentParameters = _fixture.GetBaseDeploymentParameters(hostingModel, publish: true);
            deploymentParameters.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
            deploymentParameters.WebConfigBasedEnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Production";
            Assert.Equal("Production", await GetStringAsync(deploymentParameters, "/GetEnvironmentVariable?name=ASPNETCORE_ENVIRONMENT"));
        }

        [ConditionalTheory]
        [InlineData(HostingModel.InProcess)]
        [InlineData(HostingModel.OutOfProcess)]
        [RequiresIIS(IISCapability.PoolEnvironmentVariables)]
        public async Task WebConfigAppendsHostingStartup(HostingModel hostingModel)
        {
            var deploymentParameters = _fixture.GetBaseDeploymentParameters(hostingModel, publish: true);
            deploymentParameters.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = "Asm1";
            deploymentParameters.WebConfigBasedEnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = "Asm2";
            if (hostingModel == HostingModel.InProcess)
            {
                Assert.Equal("Asm1;Asm2", await GetStringAsync(deploymentParameters, "/GetEnvironmentVariable?name=ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"));
            }
            else
            {
                Assert.Equal("Asm1;Asm2;Microsoft.AspNetCore.Server.IISIntegration", await GetStringAsync(deploymentParameters, "/GetEnvironmentVariable?name=ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"));
            }
        }
    }
}