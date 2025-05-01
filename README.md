# What is Xrm Tools
Xrm Tools is a Visual Studio extension that aims to make Power Platform development feel native within Visual Studio. You will get Intellisense support for your Plugins, generate boiler plate code, typed entities and more right from within Visual Studio. You also have full control over the code generation using [Scriban](https://github.com/scriban/scriban) templates.

## Features

* Define plugins entirely in code using attributes.
* Intellisense for entities, attributes and more based on connected environment.
* Attribute colorization within strings.
* Traditional and packaged plugins.
* Solution awareness.
* Multi-environment.
* Secure secret managment using Windows Credential Management or environment variables.
* Binding environment to solution, project or global level per developer or per team.
* Customizable seamless code generation for plugins, custom APIs and entities.
* One-click plugin registration.
* One-click project registration.
* Fully customizable code generation using liquid-like templates.
* Support CSharp v12.0 via PolySharp.

## Xrm Tools is a good citizin in Visual Studio
* XrmTools does not need admin privileges and will not apply any system-wide change in your machine nor the configuration of your Visual Studio or projects. 
* You can always remove the extension safely and completely when you don't need it.
* XrmTools does not send any information from your machine unlike some other extensions.
* XrmTools does not do any code generation at build time. In other words everything is done in development time where you see the changes transparently and benefit from Git for diffing and reviewing the code.
* XrmTools does not make you change your coding style. In other words, you don't have to inherit from a specific class or apply a special interface or things like that.
* All you need to apply are a number of attributes to define your intent so the extensions knows where and how to help.
* Should you decide to uninstall Xrm Tools, whatever you have developed with it will continue to work as there is no unsupported trickery going on behind the scenes.

## Plugin Registration As Code

XRM Tools enables you to define and implement Power Platform plugins and custom APIs entirely as code. The attributes in your source code are the [single source of truth](https://en.wikipedia.org/wiki/Single_source_of_truth). This means that if you have used previously registered your plugins using any other tool, **all plugin registrations** will be replaced by those defined in your source code.

By brining plugin registrations to the code you will benefit from Git and the entire echosystem that you have around the code. You are able to review the code and trace the history of every change and the reason behind them. You team mate are able to just clone the code and know where to test (if you manage the environment at solution or project file level).

# Learn more
To learn more about Xrm Tools extension for Visual Studio check the [wiki](https://github.com/rezanid/xrmtools/wiki)
