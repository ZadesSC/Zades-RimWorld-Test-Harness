namespace RimworldTestHarness.Core.Models;

public interface ITestCase
{
    string Name { get; }
}

public interface ITestContext
{
    HarnessManifest Manifest { get; }
}

public interface IGameAction
{
    string Description { get; }
}

public interface IGameAssertion
{
    string Description { get; }
}
