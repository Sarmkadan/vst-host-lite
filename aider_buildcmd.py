#!/usr/bin/env python3
"""
A simple build command script for the VstHostLite repository.

Running this script will:
1. Restore NuGet packages (`dotnet restore`).
2. Execute the unit test suite (`dotnet test`).

It is intended to be invoked from the repository root, but the script
automatically determines the correct working directory based on its own
location, so it works regardless of the current working directory.
"""

import subprocess
import sys
from pathlib import Path


def _run_command(command: list[str], cwd: Path) -> subprocess.CompletedProcess:
    """Run a command via subprocess, forwarding stdout/stderr."""
    return subprocess.run(
        command,
        cwd=cwd,
        stdout=sys.stdout,
        stderr=sys.stderr,
        check=False,
        text=True,
    )


def main() -> None:
    # Determine the repository root (the directory containing this script)
    repo_root = Path(__file__).resolve().parent

    # 1. Restore packages
    restore_result = _run_command(["dotnet", "restore"], cwd=repo_root)
    if restore_result.returncode != 0:
        print("dotnet restore failed.", file=sys.stderr)
        sys.exit(restore_result.returncode)

    # 2. Run tests (skip restore because we already did it)
    test_result = _run_command(
        ["dotnet", "test", "--no-restore", "--verbosity", "minimal"], cwd=repo_root
    )
    if test_result.returncode != 0:
        print("dotnet test failed.", file=sys.stderr)

    sys.exit(test_result.returncode)


if __name__ == "__main__":
    main()
