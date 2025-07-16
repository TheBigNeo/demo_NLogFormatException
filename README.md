# NLog 6.0: Logging Now Throws on Invalid Format Strings

## Summary

After upgrading from **NLog 5.5** to **NLog 6.0**, we discovered a critical change in behavior:

> **NLog 6.0 throws `System.FormatException` when an invalid format string is used in log messages.**

While this is technically caused by invalid usage of format specifiers, this change introduces **severe risks** in production environments.

## Why This Is a Problem

Logging should never interfere with application logic. In NLog 5.5, incorrect format specifiers were simply ignored in log output:

```csharp
// NLog 5.5 - invalid format specifier does NOT throw
Log.Info("My ID is {id:l}", Guid.Parse("502f088f-1c1f-4f1e-8401-a2528211086a"));
// Output: My ID is {id:l}
```

In NLog 6.0, this exact line now throws:

```csharp
// NLog 6.0 - same line now throws System.FormatException
Log.Info("My ID is {id:l}", Guid.Parse("502f088f-1c1f-4f1e-8401-a2528211086a"));
```

This behavior change is dangerous because logging is often used **inside `catch` blocks** to record unhandled exceptions. A malformed log statement can now cause:

* The original exception to be **suppressed**.
* A misleading `System.FormatException` to be thrown instead.
* Loss of critical debugging information.

### Example

Here is a realistic and alarming scenario:

```csharp
try
{
    throw new Exception("Master Exception");
}
catch (Exception e)
{
    // The logging line below throws and hides the real exception
    Log.Error(e, "This exception has the ID {id:l}", Guid.NewGuid());
}
```

Instead of logging the `"Master Exception"`, the application crashes with:

```
System.FormatException: Format string can be only "D", "d", "N", "n", "P", "p", "B", "b", "X" or "x".
```

As a result, the root cause is **lost**, and we cannot help our customers.
Even though the format is invalid, this is **not critical enough to justify crashing** or hiding the real exception in a `catch` block.

## Request

We strongly suggest **restoring the forgiving behavior** or introducing a configuration option like:

```csharp
LogManager.Setup().AllowInvalidFormatSpecifiers(true);
```

This change in behavior should not silently break existing production systems or risk masking real errors.


