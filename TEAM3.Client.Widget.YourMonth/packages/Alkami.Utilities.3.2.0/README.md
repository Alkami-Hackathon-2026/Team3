# Alkami.Utilities

General-purpose utility library providing string formatting, PII masking, credit card validation, cryptographic operations, LINQ extensions, and application configuration helpers for Alkami services.

## ⚠️ Deprecation Notice

Most APIs in this library are deprecated on `.NET 8.0+` and absent on `.NET 10.0+`. Alkami has published focused replacement packages for each feature area. **For new code, prefer the replacements listed below.**

## Framework Support

| Framework | Status | Notes |
|-----------|--------|-------|
| `.NET Framework 4.7.2` | ✅ Full | All features supported |
| `.NET 8.0` | ⚠️ Deprecated | Most types marked `[Obsolete]`; see replacements below |
| `.NET 10.0` | ⏸️ Limited | Only `StringUtilities` and `CreditCardValidator` available |

## String & PII Utilities

Safely mask and redact sensitive data (card numbers, tax IDs, phone numbers, email addresses).

**Namespace:** `Alkami.Utilities.Formatting.Utilities`

```csharp
using Alkami.Utilities.Formatting.Utilities;

// Redact card numbers, tax IDs, and private data all at once
string sanitized = StringUtilities.SanitizeData(
    "Account 123-45-6789, card 4532-xxxx-xxxx-1234, SSN 987-65-4321");
// Output: "Account [TAX_ID_REDACTED], card [CARD_REDACTED], SSN [PRIVATE_NUMBER_REDACTED]"

// Mask email addresses
string masked = StringUtilities.MaskEmailAddress("john.doe@example.com");
// Output: "jo...@example.com"

// Mask phone numbers (show only last 4 digits)
string phone = StringUtilities.MaskPhoneNumber("555-123-4567");
// Output: "***-***-4567"

// Redact connection strings
string connStr = "Server=localhost;Password=SecretPass123;User=admin";
string safe = StringUtilities.SanitizeConnStr(connStr);
// Output: "Server=localhost;Password=[REDACTED];User=admin"
```

**Deprecated on `.NET 8.0+`** — use standard .NET logging with encrypted appenders for sensitive data.

## Credit Card Validation & Redaction

Validate and redact credit card numbers using Luhn check and PAN regex matching.

**Namespace:** `Alkami.Utilities.Validation`

```csharp
using Alkami.Utilities.Validation;

// Validate a credit card number
if (CreditCardValidator.IsValidCreditCardNumber("4532-1234-5678-9010"))
{
    Console.WriteLine("Valid card");
}

// Normalize a card number (strips formatting, validates)
string normalized = CreditCardValidator.NormalizeCreditCardNumber("4532 1234 5678 9010");
// Output: "4532123456789010"

// Redact card numbers in a string (keeps first 2 and last 4 digits)
string text = "Process card 4532-1234-5678-9010 for account 12345";
string redacted = CreditCardValidator.RedactCreditCardNumbers(text);
// Output: "Process card 45****5678-9010 for account 12345"
```

**Deprecated on `.NET 8.0+`** — consider implementing in your service that needs credit card validation.

## LINQ Extensions

Convenient methods for common enumerable operations: batching, deduplication, safe single-item selection.

**Namespace:** `Alkami.Utilities.Extensions`

```csharp
using Alkami.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;

var numbers = new[] { 1, 2, 2, 3, 3, 3, 4, 5 };

// Remove duplicates by key selector
var distinct = numbers.DistinctBy(n => n % 2);

// Split into batches of 2
var batches = numbers.ChunkBy(2);
// [[1, 2], [2, 3], [3, 3], [4, 5]]

// Safely get a single item (returns null + flag if 0 or 2+ items)
var items = new[] { 42 };
var (item, found) = items.TryGetSingleOrDefault();
if (found) Console.WriteLine($"Found: {item}");

// Execute a side effect on each item
var doubled = numbers.ForEach(n => Console.WriteLine(n)).ToList();

// Check if empty
if (!numbers.IsNullOrEmpty())
{
    Console.WriteLine("Has items");
}
```

**Deprecated on `.NET 8.0+`** — most methods are now built-in to LINQ or available via LINQ-to-Objects.

## Object-Property Formatting

Template-based string formatting: replace `{PropertyName}` tokens with object property values. Supports nested properties, format specifiers, padding, and regex matching.

**Namespace:** `Alkami.Utilities.Formatting`

```csharp
using Alkami.Utilities.Formatting;

public class Payment
{
    public string AccountNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

var payment = new Payment
{
    AccountNumber = "123456789",
    Amount = 1500.50m,
    CreatedAt = DateTime.Now
};

var binder = new ObjectPropertyBinder();

// Simple replacement
string msg = binder.Bind("Account {AccountNumber} charged {Amount:C}", payment);
// Output: "Account 123456789 charged $1,500.50"

// With format specifier and padding
string msg2 = binder.Bind("Date: {CreatedAt:yyyy-MM-dd}", payment);
// Output: "Date: 2024-06-24"
```

**Deprecated on `.NET 8.0+`** — use string interpolation or standard formatting APIs.

## HMAC Hashing

Compute HMAC signatures for message authentication and integrity checking.

**Namespace:** `Alkami.Utilities.Hashing`

```csharp
using Alkami.Utilities.Hashing;

var message = "important data";
var key = "secret-key";

// Compute HMAC SHA256
string hash = HashUtility.Compute(HashMethod.HMACSHA256, message, key);

// Compute with binary key
byte[] binaryKey = System.Text.Encoding.UTF8.GetBytes(key);
string hash2 = HashUtility.Compute(HashMethod.HMACSHA256, message, binaryKey);

// Get raw bytes instead of hex string
byte[] hashBytes = HashUtility.ComputeBinary(HashMethod.HMACSHA256, message, key);
```

Supported algorithms: `HMACMD5`, `HMACSHA1`, `HMACSHA256`, `HMACSHA384`, `HMACSHA512`.

**Deprecated on `.NET 8.0+`** — use `System.Security.Cryptography.HMAC` classes directly:

```csharp
using System.Security.Cryptography;
using System.Text;

var key = Encoding.UTF8.GetBytes("secret-key");
var message = Encoding.UTF8.GetBytes("important data");
byte[] hash = HMACSHA256.HashData(key, message);
```

## Asymmetric Encryption (RSA + AES)

Hybrid encryption combining RSA for key exchange and AES for data encryption. Designed for encrypting large payloads like log entries.

**Namespace:** `Alkami.Utilities.Cryptography`

```csharp
using Alkami.Utilities.Cryptography;

var thumbprint = "a4c12a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f";
var plaintext = "sensitive log message";

// Encrypt using an X.509 certificate by thumbprint
string encrypted = AsymmetricEncryption.EncryptWithThumbprint(plaintext, thumbprint);

// Decrypt using the matching private key
string decrypted = AsymmetricEncryption.DecryptWithThumbprint(encrypted, thumbprint);
// Output: "sensitive log message"
```

**Deprecated on `.NET 8.0+`** — use `Alkami.Utilities.Encryption` or `Alkami.Extensions.Certificates` instead.

## Application Configuration

Read strongly-typed application settings from the configuration section, including deployment environment, hosting type, server role, and application type.

**Namespace:** `Alkami.Utilities.Configuration`

```csharp
using Alkami.Utilities.Configuration;

// Access application configuration from app.config / web.config
var name = ApplicationConfiguration.Name;
var env = ApplicationConfiguration.Environment;
var logging = ApplicationConfiguration.Logging;

Console.WriteLine($"App: {name} | Env: {env.EnvironmentType} | Host: {env.HostingType}");
```

Enums for structured configuration:

- **EnvironmentType**: `Development`, `TeamQA`, `QA`, `Integration`, `Staging`, `Production`, etc.
- **HostingType**: `OnPremise`, `AWS`, `Azure`, `Firehost`, etc.
- **ServerType**: `Web`, `App`, `SQL`, `Radium`, etc.
- **ApplicationType**: `WebClient`, `WebService`, `Microservice`, `IPSTS`, `RPSTS`, etc.

**Deprecated on `.NET 8.0+`** — use `Microsoft.Extensions.Configuration` (`IConfiguration`) instead.

## Migrating Away from Alkami.Utilities

| Deprecated Feature | Replacement Package | Notes |
|---|---|---|
| `CertificateUtility`, `AsymmetricEncryption` | `Alkami.Extensions.Certificates` + `Alkami.Utilities.Encryption` | Net-new packages with .NET 8+ support |
| `LogUtility` (log4net integration) | `Microsoft.Extensions.Logging` + `Alkami.Extensions.EncryptedLogging` | Standard logging + encrypted appender |
| `HashUtility` | `System.Security.Cryptography` | Use `HMACSHA256`, `HMACSHA512`, etc. directly |
| `ApplicationConfiguration` | `Microsoft.Extensions.Configuration` | Use dependency-injected `IConfiguration` |
| `IpAddressUtility` | — | Framework 4.7.2 only; use ASP.NET Core `HttpContext.Connection.RemoteIpAddress` |
| `CreditCardValidator` | PCI-DSS compliant processor APIs | Implement in your source code; store only tokenized data |
| `StringUtilities` (on net8+) | Application-specific logic | Replace masking with structured logging + encrypted appenders |

