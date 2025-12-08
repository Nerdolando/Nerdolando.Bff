# Security Policy

Thank you for helping keep Nerdolando.Bff and its users secure.

Nerdolando.Bff is a security-focused Backend-for-Frontend (BFF) library for .NET.
It deals with authentication cookies, access tokens and refresh tokens, so
**security reports are taken very seriously**.

This document explains how to report a vulnerability and what you can expect from the maintainers.

---

## Reporting a vulnerability

If you believe you have found a security vulnerability,
**please do not open a public issue or Pull Request**.

Instead, use one of these private channels:

1. **Preferred:** Use GitHub’s “Report a vulnerability” / Security Advisory feature  
   (if available on this repository).

2. Or send an email to:  
   `admin@nerdolando.pl`

When reporting, please include as much detail as you reasonably can (see next section).

---

## What to include in your report

To help reproduce and fix the issue quickly, please include:

- A short **summary** of the vulnerability.
- The **potential impact** (e.g. token theft, privilege escalation, information disclosure).
- **Exact versions**:
  - Nerdolando.Bff packages (e.g. `Nerdolando.Bff.AspNetCore` 1.0.7),
  - .NET runtime / SDK version,
  - Any relevant environment details (hosting type, reverse proxies, etc.).
- Clear **steps to reproduce**:
  - configuration snippets (sanitized – no real secrets),
  - minimal code examples if possible,
  - sequence of HTTP requests if relevant.
- Any **logs or screenshots** that help illustrate the issue
  (again: make sure they do not contain real secrets or personal data).

If you already have an idea for a fix or mitigation, you are welcome to include it,
but it is not required.

---

## Our response process

When a security report is received via a private channel, the typical process is:

1. **Acknowledgement**  
   I aim to acknowledge your report as soon as possible.

2. **Assessment & reproduction**  
   I will try to reproduce the issue and assess its impact and severity.

3. **Fix & release**  
   If the issue is confirmed, I will:
   - develop and test a fix,
   - release a patched version of the affected package(s),
   - update the documentation where needed.

4. **Disclosure**  
   After a fix is available, I may:
   - mention the vulnerability in the changelog / release notes in a way
     that does not make exploitation easier,
   - coordinate with you on public disclosure if appropriate.

During this process I may contact you for additional details or clarification.

---

## Supported versions

Security fixes are generally applied to:

- The **latest stable release** of `Nerdolando.Bff.AspNetCore` and related packages (1.x series).

Older versions may not receive patches.  
If you are running an older version, you may be asked to upgrade to a supported release.

---

## Non-security issues

If your report concerns:

- regular bugs,
- feature requests,
- performance issues,
- documentation problems,

please use **GitHub Issues** instead of the security contact.  
This keeps the private channel focused on genuine security problems.

---

## Responsible use

Please avoid:

- actively exploiting the vulnerability on real systems,
- running automated attacks against third-party deployments,
- sharing proof-of-concept exploits publicly before a fix is available.

Testing in your own controlled environment and responsible reporting is highly appreciated.

---

Thank you again for helping to improve the security of Nerdolando.Bff 🙌
Your effort directly benefits everyone using this library in production.
