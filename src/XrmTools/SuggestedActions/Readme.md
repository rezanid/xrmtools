# Suggested Action Source Providers (aka Light Bulb Suggestions)

Light bulbs are icons in the Visual Studio editor that expand to display a set of actions, for example, fixes for problems identified by the built-in code analyzers or code refactoring.

In the Visual C# and Visual Basic editors, you can also use the .NET Compiler Platform ("Roslyn") to write and package your own code analyzers with actions that display light bulbs automatically. For more information, see:
[How To: Write a C# diagnostic and code fix](https://github.com/dotnet/roslyn/blob/main/docs/wiki/How-To-Write-a-C%23-Analyzer-and-Code-Fix.md)

To learn more about Light Bulb Suggestions see:
* [Walkthrough: Display light bulb suggestions](https://learn.microsoft.com/en-us/visualstudio/extensibility/walkthrough-displaying-light-bulb-suggestions?view=vs-2022)

> [! NOTE]
> I abandoned SuggestedActions in favor of CodeRefactoringProvider, mainly because we can focus on providing the refactoring logic while VS will take care of user experience. This means not only less maintenance, but also the UX will always evolve as VS evolves over time. Supporting SuggestedAction means mimicing the native UX in this case. The only viable scenario for SuggestedAction is to provide a totally different UX for reasons other than code changes like refactoring, completions or code fixing.

> [! INFO]
> For new implementation check /CodeRefactoringProviders/CustomApiRefactoringProvider.cs