using System;
using Xunit;

namespace DentsuAssignmentApp.Test.Helpers;

public static class CustomAssertions
{
    public static void DecimalEqual(decimal expected, decimal actual, decimal tolerance)
    {
        Assert.True(Math.Abs(expected - actual) <= tolerance,
            $"The value {actual} was not within the tolerance {tolerance} of {expected}.");
    }
}