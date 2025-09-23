using Mechanics.Tests.Integration.Helpers;

namespace Mechanics.Tests.Integration.Tests;

[TestClass]
[TestCategory("HealthChecks")]
public class HealthCheckTest(TestContext testContext)
{
    [TestMethod]
    public async Task It_ShouldReturnOk()
    {
        var factory = TestProperties.Factory;
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health", testContext.CancellationTokenSource.Token);

        response.EnsureSuccessStatusCode();
    }
}
