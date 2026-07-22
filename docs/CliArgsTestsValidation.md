# CliArgsTestsValidation

Utility class providing validation and inspection for command-line arguments used in test scenarios. It offers methods to check argument validity, collect validation messages, and enforce validation rules with exceptions.

## API

### `IReadOnlyList<string> Validate(string[] args)`

Validates the provided command-line arguments and returns a list of validation messages. Each message describes a specific validation failure. If no arguments are invalid, the returned list is empty.

- **Parameters**
  - `args`: The command-line arguments to validate.
- **Return value**
  - A read-only list of strings describing validation failures. Never `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `args` is `null`.

### `bool IsValid(string[] args)`

Determines whether the provided command-line arguments are valid without collecting detailed messages.

- **Parameters**
  - `args`: The command-line arguments to inspect.
- **Return value**
  - `true` if all arguments are valid; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `args` is `null`.

### `void EnsureValid(string[] args)`

Validates the provided command-line arguments and throws an exception if any are invalid. This method does not return a list of messages; instead, it fails fast on the first validation error.

- **Parameters**
  - `args`: The command-line arguments to validate.
- **Exceptions**
  - Throws `ArgumentNullException` if `args` is `null`.
  - Throws `InvalidOperationException` with a descriptive message if any argument fails validation.

## Usage
