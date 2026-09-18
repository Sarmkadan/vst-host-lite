# vst-host-lite

Minimal experimental VST3 host in C#/.NET 10: a CLI (`vsthost`) plus a native-interop library with a simple audio-graph model (generators, delay, mixer, pan, metering nodes).

## Build

```bash
dotnet build VstHostLite.slnx                       # Debug
dotnet build VstHostLite.slnx --configuration Release
dotnet run --project src/VstHostLite.Cli -- info <plugin.vst3>
```

Requires .NET SDK 10.x (solution file is the new `.slnx` format). No external NuGet deps in `src/`.

## Test

```bash
dotnet test VstHostLite.slnx                        # all test projects (xUnit 2.9)
dotnet test tests/VstHostLite.Native.Tests
dotnet test tests/VstHostLite.Cli.Tests
dotnet test --filter "FullyQualifiedName~DelayNodeTests"
```

`aider_buildcmd.py` runs `dotnet test --no-build` (build first). Coverage via `coverlet.collector`.

## Lint / Format

No `.editorconfig`, analyzers or formatter configured. `Nullable` and `ImplicitUsings` are enabled in every project; keep builds warning-free (currently 0 warnings). Use `dotnet format` only if asked.

## Layout

- `src/VstHostLite.Native/` - core library (`VstHostLite.Native` namespace). `AllowUnsafeBlocks` on.
  - `Vst3Interop.cs` - COM-style vtable walking of VST3 factories (no plugin instantiation/processing yet).
  - `IAudioNode.cs` / `AudioGraph.cs` - node contract (`Prepare`, `Process`, `Reset`) and graph container.
  - Nodes: `DelayNode`, `MixerNode`, `PanNode`, `MeteringNode`, `SineGeneratorNode`, `NoiseGeneratorNode`, `SilenceDetectorNode`, `ClipDetector`, `ParameterSmoother`, `SampleRateConverter`.
  - `AudioBuffer.cs`, `MidiEvent.cs`, `MidiEventQueue.cs`, `PluginScanCache.cs` (`.vst3.cache.json` files).
- `src/VstHostLite.Cli/` - console app, assembly name `vsthost`. Entry: `Program.cs`; subcommands in `Commands/` implement `ICliCommand.Run(string[] args)` and are registered in `Program.CreateCommands()` (`info`, `validate`, `graph`, `stats`, `scan`, `scan-one`, `play` - `play` is unfinished).
- `tests/VstHostLite.Native.Tests/`, `tests/VstHostLite.Cli.Tests/` - xUnit; `InternalsVisibleTo` granted in `AssemblyInfo.cs`.
- `docs/` - one markdown page per public type with usage examples; `README.md` mirrors them.
- Stray files at repo root (`AudioGraph.cs`, `test_validation_consistency.cs`, `src/Audio/`, `tests/VstHostLite.Tests/`, `sql-index-advisor/`) are not part of the solution and are not compiled.

## Conventions

- File-scoped namespaces, `public sealed`/`static` classes, XML doc comments on all public members.
- Per-type companion files: `Foo.cs`, `IFoo.cs`, `FooConstants.cs` (`internal static class`, defaults/limits), `FooExtensions.cs`, `FooJsonExtensions.cs` (System.Text.Json, camelCase, `ToJson`/`FromJson`), `FooValidation.cs`.
- Argument checks via `ArgumentNullException.ThrowIfNull` / `ArgumentOutOfRangeException`; validation failures throw, they do not return bools.
- Private static fields `_camelCase`; constants `PascalCase`.
- Tests: class `FooTests`, methods `Method_ExpectedBehavior_WhenCondition`, `[Fact]`/`[Theory]`, namespace `VstHostLite.Native.Tests`.
- Commit messages: conventional commits (`fix(DelayNode): ...`, `docs: ...`, `chore: ...`).
- When adding a node: implement `IAudioNode`, add the companion files above, a `docs/<Node>.md` page and a `<Node>Tests.cs`.
