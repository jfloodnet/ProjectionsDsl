using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ProjectionsDsl.Tests.Asserts
{
    /// <summary>
    /// Assert (throw exceptions) 
    /// </summary>
    public static class ShouldAssertions
    {
        public static void ShouldContainOnly<T>(this IEnumerable<object> messages) where T : class
        {
            ShouldContainOnly<T>(messages, null);
        }

        public static void ShouldContainOnly<T>(this IEnumerable<object> messages, Func<T, bool> isExpected) where T : class
        {
            Assert.Equal(1, messages.Count());
            ShouldContain(messages, isExpected);
        }

        public static void ShouldContain<T>(this IEnumerable<object> messages)
            where T : class
        {
            ShouldContain<T>(messages, null);
        }

        public static void ShouldContain<T>(this IEnumerable<object> messages, Func<T, bool> isExpected) where T : class
        {
            Assert.True(messages.Any(m =>
                {
                    return m is T && (isExpected == null || isExpected(m as T));
                }));
        }

        public static void ShouldBeEmpty(this IEnumerable<object> messages)
        {
            Assert.Empty(messages);
        }
    }
}