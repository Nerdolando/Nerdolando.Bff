# Contributing to Nerdolando.Bff

First of all: **thank you** for considering contributing to Nerdolando.Bff!  
This is a security-focused Backend-for-Frontend (BFF) library for .NET, and good contributions (code, docs, issues, ideas) are very welcome.

This document explains how to:
- report bugs and request features,
- work with the codebase locally,
- submit pull requests (PRs) in a way that is easy to review and merge.

> If anything here is unclear, feel free to open a GitHub Issue or Discussion and ask.

---

## Code of Conduct

By participating in this project, you are expected to uphold a friendly and respectful environment.

Please:
- be kind and constructive,
- focus on technical issues, not people,
- assume good intent in others.

A formal `CODE_OF_CONDUCT.md` may be added later; until then, the above rules apply.

---

## Ways you can contribute

You don’t have to write code to help:

- 🐞 **Report bugs** – unexpected behavior, runtime errors, misconfigurations.
- 💡 **Suggest features** – new options for BFF configuration, token storage, front-end integration helpers, etc.
- 📖 **Improve documentation** – README improvements, clearer examples, comments in code, samples.
- ✅ **Add tests** – unit tests for edge cases, regression tests for previously fixed bugs.
- 🧪 **Improve samples** – new scenarios under `samples/` (e.g. different IdPs, frameworks, multi-front setups).

---

## Before you start

### Project layout (high-level)

Some key paths in this repository:

- `src/` – main library projects, e.g.:
  - `Nerdolando.Bff.AspNetCore` – core BFF library,
  - `Nerdolando.Bff.Storage.Sqlite` – Sqlite token storage.
- `tests/UnitTests/` – unit tests for the libraries, e.g.:
  - `Nerdolando.Bff.AspNetCore.Tests`
- `samples/` – example applications using Nerdolando.Bff (BFF host + front-end/API).

There are also solution files:
- `Nerdolando.Bff.sln` – main solution for the library,
- `Nerdolando.Bff-Testing.sln` – solution focused on tests.

### Prerequisites

To build and test the project you will need:

- **.NET SDK** – use the version(s) specified by the target frameworks in the `.csproj` files (e.g. .NET 8 / .NET 9 / .NET 10).
- A recent version of **git**.
- An IDE or editor that supports C# (Visual Studio, Rider, VS Code with C# extension, etc.).

---

## Reporting bugs

Before opening a new bug report:

1. **Search Issues** – check if the problem was already reported.
1. If you find an existing issue that matches your problem, you can add a 👍 reaction or a constructive comment.

When you create a new bug report:

1. Go to **Issues → New Issue** and choose “Bug report” (if templates exist) or open a standard issue.
1. Include as much detail as possible:

   - What you did (short scenario).
   - What you expected to happen.
   - What actually happened (error messages, stack traces, logs).
   - Versions:
     - Nerdolando.Bff packages (e.g. `Nerdolando.Bff.AspNetCore` version).
     - .NET SDK / runtime version.
     - Hosting type (Kestrel, IIS, container, etc.) if relevant.
   - Minimal code/configuration snippet that reproduces the problem, if possible.

1. If the issue is security-sensitive (e.g. potential token leakage, auth bypass), **do not** put details in a public issue.  
   Instead, create a very general issue (“Potential security problem”) and wait for instructions, or use the security contact (once `SECURITY.md` is added).

Clear, detailed bug reports make it much easier to fix issues quickly.

---

## Suggesting new features or changes

Feature requests and design discussions are very welcome, especially if they relate to:

- new storage providers,
- better extensibility / hooks (e.g. events, custom handlers),
- new sample scenarios (different front-ends or IdPs),
- ergonomics of the public API.

Before opening a feature request:

1. **Check existing issues** labeled `enhancement`, `feature`, etc.
1. If nothing matches, create a new issue with:

   - Problem statement: what use case you want to support.
   - Why existing options are not enough.
   - A rough idea of the API/configuration you imagine (optional but helpful).
   - Whether it is a **breaking change** or not (if you’re proposing a change to existing APIs).

Often it’s best to discuss the idea in an issue first, before investing time into a PR.

---

## Working with the code

### Fork and clone the repository

1. Click **Fork** on GitHub to create your own copy.
1. Clone your fork:

   ```bash
   git clone https://github.com/<your-username>/Nerdolando.Bff.git
   cd Nerdolando.Bff
   git remote add upstream https://github.com/Nerdolando/Nerdolando.Bff.git
   ```
                                      
1. Create a feature branch:
	Create a new branch off master:
   ```bash
   git checkout -b feature/my-new-feature
   # or:
   git checkout -b fix/token-refresh-bug
   ```
   
   Try to keep each branch focused on a single logical change (small, reviewable PRs are much easier to merge).

1. Run tests
	From the repo root:
    ```bash
    dotnet test
    ```
	This will run the unit tests (e.g. under tests/UnitTests/Nerdolando.Bff.AspNetCore.Tests).

    Please make sure tests pass before you push your branch and open a PR.
    If you add new behavior, consider adding or updating tests to cover it.
    But please test only your code.

        
1. Run and use samples (optional but recommended)
	The `samples/` folder contains example applications showing how to use Nerdolando.Bff.
    - You can open a sample’s .csproj or solution in your IDE and run it.
    - Using a sample is a great way to manually verify your change in a realistic scenario.

---

### Coding style
The project includes shared configuration (e.g. Directory.Build.props, .editorconfig) to keep a consistent style.

General guidelines:
- Follow existing patterns and naming in the codebase.
- Prefer clarity over cleverness.
- Keep public APIs as small and intention-revealing as possible.
- Avoid breaking changes unless they are clearly justified and discussed in an issue.
- Delete unused code rather than commenting it out.
- Delete unused usings and namespaces.

If you use tools like dotnet format, that’s fine, but please avoid large formatting-only PRs.
If you fix formatting, try to limit it to the lines/files you are already editing for your change.

---

### Submitting a Pull Request (PR)
When you’re ready to contribute your change:
1. Sync with upstream (optional but recommended if some time has passed):
    ```bash
    git fetch upstream
    git checkout master
    git merge upstream/master
    git checkout feature/my-branch
    git rebase master
    ```
1. Push your branch to your fork:
    ```bash
    git push origin feature/my-branch
    ```
1. On GitHub, open a Pull Request targeting `master`.

    In the PR description, please include:
    - A short summary of the change (1–3 sentences).
    - The motivation: what problem it solves / what it improves.
    - Any breaking changes (if applicable).
    - How you tested it (commands you ran, manual scenarios).
    - Links to related issues or discussions, if any.

### PR checklist
Before asking for review, please check:

- [ ] The project builds (dotnet build).
- [ ] All tests pass (dotnet test).
- [ ] New/changed behavior is covered by tests (when reasonable).
- [ ] Public API changes are documented (e.g. README, XML comments or docs).
- [ ] The change is focused and not mixing unrelated refactors.

PRs that are small and focused, with a clear description, are much easier and faster to review.

--- 

## Documentation and samples
Improvements to docs and samples are highly appreciated:

- Fix typos or unclear wording in README.md.
- Add small, focused code snippets that illustrate specific configuration options.
- Improve existing samples in samples/ (better error handling, configuration, comments).
- Add new sample scenarios showing:
    - different front-end frameworks,
    - different Identity Providers,
    - multi-front setups.

For documentation-only PRs you still don’t have to be perfect – even small improvements make the project more approachable.

--- 

## Release & versioning
`Nerdolando.Bff` uses semantic versioning (SemVer) on NuGet packages as a guideline:
- PATCH version – bug fixes and small improvements, no breaking changes.
- MINOR version – new features, still backward compatible.
- MAJOR version – breaking changes.

As a contributor, you usually don’t need to bump versions yourself – maintainers will handle it during the release process.
If your change includes a potential breaking change, please call it out clearly in the PR description.

---

## Questions and support
If you’re unsure how to implement something or whether your idea fits the roadmap, you can:
- Open an Issue with the question or discussion label.
- Use GitHub Discussions (if enabled) to talk about ideas, design, and usage patterns.

Don’t hesitate to ask – it’s better to clarify things early than to invest a lot of time in a direction that doesn’t fit the project.


Thank you again for your interest in `Nerdolando.Bff` 💙
Every issue, comment, and PR helps make secure BFFs in .NET easier for everyone.