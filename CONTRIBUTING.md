# Contributing to LanWatcher.NET

First off, thank you for considering contributing to LanWatcher.NET! It's people like you that make this tool better for everyone.

## Code of Conduct

This project and everyone participating in it is governed by our commitment to providing a welcoming and inclusive environment. Please be respectful and constructive in all interactions.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues to avoid duplicates. When you create a bug report, include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Describe the behavior you observed and what you expected**
- **Include your .NET version and operating system**
- **Include any error messages or logs**

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide a detailed description of the proposed feature**
- **Explain why this enhancement would be useful**
- **Include mockups or examples if applicable**

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Follow the coding style** used throughout the project
3. **Add tests** if you're adding new functionality
4. **Ensure the build passes** by running `dotnet build`
5. **Update documentation** if needed
6. **Write a clear commit message** describing your changes

## Development Setup

1. Clone your fork:
```bash
git clone https://github.com/yourusername/LanWatcher.NET.git
cd LanWatcher.NET
```

2. Create a branch:
```bash
git checkout -b feature/your-feature-name
```

3. Make your changes and test:
```bash
dotnet build
dotnet run
```

4. Commit and push:
```bash
git add .
git commit -m "Add your feature description"
git push origin feature/your-feature-name
```

5. Open a Pull Request on GitHub

## Coding Guidelines

- Use C# 12 features where appropriate
- Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and small
- Write async code where I/O operations are involved

## Feature Ideas

Looking for something to work on? Here are some ideas:

- [ ] Add MAC address resolution using ARP
- [ ] Export scan results to JSON/CSV
- [ ] Add scheduled/automated scans
- [ ] Implement device history tracking
- [ ] Add custom port range scanning
- [ ] Create a CLI version
- [ ] Add network speed testing
- [ ] Implement Wake-on-LAN functionality

## Questions?

Feel free to open an issue with your question or reach out to the maintainers.

Thank you for contributing! 🎉
