# Copilot Instructions

## General Guidelines
- Prefer resolving analyzer-related attribute symbols via fully-qualified metadata name (e.g., "XrmTools.Meta.Attributes.DependencyAttribute") rather than unqualified names.

## Commit Messages
- Use Conventional Commitsformat for commit messages.
- Syntax: `<type>(<optional scope>): <description>`
- Valid types include: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `perf`, `ci`.
- Use scope to indicate the area of the codebase affected (e.g., `analyzers`, `code-fixes`, `refactorings`).
- Write subject line in imperative mood and lowercase and under 72 characters and do not end with a period.
- Write a concise description of the change in the commit message body.
- Reference any relevant issues or pull requests in the body of the commit message using `#issue-number` or `#pull-request-number` (e.g. `Closes #100`).
- If a commit is a breaking change, include `BREAKING CHANGE:` in the footer of the commit message followed by a description of the breaking change and its impact.