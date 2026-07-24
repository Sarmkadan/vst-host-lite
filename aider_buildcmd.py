#!/usr/bin/env python3
"""
aider_buildcmd.py

A simple helper script used by the Aider toolchain to build and test the
VstHostLite.Native project. It runs `dotnet test` in the repository root and
exits with the same return code as the test runner.

If additional build steps are required they can be added to the `main`
function.
"""

import subprocess
import sys
from pathlib import Path


def _run_command(command: list[str], cwd: Path) -> subprocess.CompletedProcess:
    """
    Execute a command in a subprocess, forwarding stdout and stderr.

    Args:
        command: The command and its arguments to execute.
        cwd: The working directory for the command.

    Returns:
        The CompletedProcess instance containing the execution result.
    """
    try:
        result = subprocess.run(
            command,
            cwd=cwd,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
            check=False,
        )
        # Echo the output so the caller sees the test results.
        sys.stdout.write(result.stdout)
        return result
    except Exception as exc:
        sys.stderr.write(f"Error running command {' '.join(command)}: {exc}\n")
        raise


def main() -> None:
    """
    Entry point for the build command.

    It runs `dotnet test` against the solution located in the repository root.
    """
    repo_root = Path(__file__).resolve().parent
    # Adjust the path if the solution file is in a subdirectory; for this repo
    # the tests are under `tests/` and the project files are under `src/`.
    # `dotnet test` will discover the test projects automatically.
    command = ["dotnet", "test", "--no-build", "--verbosity", "minimal"]
    result = _run_command(command, cwd=repo_root)

    # Propagate the exit code from the test runner.
    sys.exit(result.returncode)


if __name__ == "__main__":
    main()
