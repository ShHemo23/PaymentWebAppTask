using PaymentGateway.Api.IntegrationTests;
using Xunit;

[CollectionDefinition("IntegrationTests", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<PaymentGatewayApiFactory>
{
    // This class has no code, it's just a marker for xUnit.
    // It applies the ICollectionFixture<> interface to the "IntegrationTests"
    // collection, telling xUnit to create a single instance of PaymentGatewayApiFactory
    // and share it across all tests in this collection.
    //
    // Disabling parallelization is crucial to prevent race conditions between
    // the database seeding in the factory's InitializeAsync and the tests that
    // depend on that seed data.
} 