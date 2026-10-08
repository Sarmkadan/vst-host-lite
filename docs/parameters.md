# Parameter Management in VstHostLite

## Overview

The `ParameterManager` class provides a centralized system for managing audio parameters with normalization, automation support, and preset functionality. All parameters are internally normalized to the range [0, 1] for consistent handling across different parameter types and ranges.

## Parameter Normalization

Parameters in VstHostLite are always stored and manipulated internally as normalized floating-point values in the range [0.0, 1.0]. This normalization allows:

- Consistent automation curves regardless of parameter range
- Uniform handling of different parameter types (frequency, gain, time, etc.)
- Easy interpolation between values
- Standardized preset storage and recall

### Conversion Formulas

To convert from a parameter's actual range to normalized form:
```
normalized = (actualValue - min) / (max - min)
```

To convert from normalized form to actual range:
```
actualValue = min + (normalized * (max - min))
```

Where:
- `min` = minimum value of the parameter's natural range
- `max` = maximum value of the parameter's natural range
- `normalized` = value in [0, 1] range
- `actualValue` = value in the parameter's natural range

## Automation Support

The ParameterManager supports automation through the normalized value interface. Automation systems can:

1. Read current parameter values via `GetNormalizedValue()`
2. Set parameter values via `SetNormalizedValue()` 
3. Work with any parameter range using `GetValue(min, max)` and `SetValue(value, min, max)`

Automation hosts should work exclusively with normalized values (0-1) for consistency, converting to/from actual ranges only when displaying values to users or when required by specific plugin formats.

## Preset Format

Presets in VstHostLite store parameter values in their normalized form. The `ParameterPreset` class contains:

- A name identifier for the preset
- A dictionary mapping parameter names to their normalized values (0-1 range)

### Preset Creation

To create a preset:
```csharp
var preset = new ParameterPreset("My Preset");
preset.AddParameterValue("CutoffFrequency", 0.75f); // 75% of range
preset.AddParameterValue("Resonance", 0.3f);       // 30% of range
```

### Preset Application

To apply a preset:
```csharp
parameterManager.ApplyPreset(preset);
```

This sets each parameter in the manager to its stored normalized value, automatically converting to the appropriate actual range for each parameter.

## Thread Safety

All public methods of `ParameterManager` are thread-safe and can be called concurrently from multiple threads. Internal synchronization is handled via locking mechanisms to prevent race conditions when multiple threads access or modify parameters simultaneously.

## Usage Examples

### Adding Parameters
```csharp
var paramManager = new ParameterManager();
paramManager.AddParameter("CutoffFrequency", 0.5f); // Default to middle of range
paramManager.AddParameter("Resonance", 0.2f);       // Default to 20%
```

### Getting and Setting Values
```csharp
// Get normalized value (0-1)
float normalized = paramManager.GetNormalizedValue("CutoffFrequency");

// Get actual value in Hz (assuming 20Hz-20kHz range)
float cutoffHz = paramManager.GetValue("CutoffFrequency", 20f, 20000f);

// Set value using actual range
paramManager.SetValue("CutoffFrequency", 1000f, 20f, 20000f);

// Set normalized value directly
paramManager.SetNormalizedValue("CutoffFrequency", 0.75f);
```

### Working with Presets
```csharp
// Create preset from current state
paramManager.CreatePreset("Default Settings");

// Later, apply the preset
var defaultPreset = new ParameterPreset("Default Settings");
// ... populate preset with values ...
paramManager.ApplyPreset(defaultPreset);
```

## Best Practices

1. Always use normalized values (0-1) for automation and storage
2. Specify appropriate min/max ranges when getting/setting actual values
3. Validate parameter ranges when creating new parameters
4. Use descriptive parameter names that indicate their function
5. Consider the perceptual characteristics of parameters when choosing ranges (e.g., logarithmic vs linear)
6. Presets should be created with meaningful names that describe their intended use