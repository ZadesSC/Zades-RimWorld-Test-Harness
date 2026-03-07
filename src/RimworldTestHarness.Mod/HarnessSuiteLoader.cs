using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RimworldTestHarness.Mod;

internal static class HarnessSuiteLoader
{
    public static List<IHarnessTestCase> Create(string suiteName)
    {
        foreach (var providerType in EnumerateProviderTypes())
        {
            if (providerType.IsAbstract || providerType.IsInterface)
            {
                continue;
            }

            if (Activator.CreateInstance(providerType) is not IHarnessSuiteProvider provider)
            {
                continue;
            }

            if (!string.Equals(provider.SuiteName, suiteName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return provider.Create().ToList();
        }

        throw new InvalidOperationException("Unknown suite: " + suiteName);
    }

    private static IEnumerable<Type> EnumerateProviderTypes()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(static type => type != null).ToArray();
            }

            foreach (var type in types)
            {
                if (type != null && typeof(IHarnessSuiteProvider).IsAssignableFrom(type))
                {
                    yield return type;
                }
            }
        }
    }
}
