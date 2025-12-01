# What is Xrm Tools
Xrm Tools is a Visual Studio extension that aims to make Power Platform development feel native within Visual Studio. You will get Intellisense support for your Plugins, generate boiler plate code, typed entities and more right from within Visual Studio. You also have full control over the code generation using [Scriban](https://github.com/scriban/scriban) templates.

## Main Features

* Define plugins registration entirely in code using attributes.
* Intellisense for entities, attributes and more based on connected environment.
* Supports traditional and packaged plugins (dependenct assemblies).
* [Solution aware plugins](https://github.com/rezanid/xrmtools/wiki/Solution%E2%80%90Aware-Plugins)
* [Environment management at User, Solution or Project level](https://github.com/rezanid/xrmtools/wiki/Environment-Management)
* [Secure secret managment using Windows Credential Management or environment variables](https://github.com/rezanid/xrmtools/wiki/Connection-String-Secrets)
* Design-time code generation for plugins, custom APIs, entities and Fetch XML.
* One-click plugin / assembly registration.
* [Fully customizable code generation templates](https://github.com/rezanid/xrmtools/wiki/Customizing-Code-Generation-Templates).
* Supports CSharp v12.0 via PolySharp.
* FetchXML editor and automatic code generation at save.
* [Retro-fit to your existing plugins using smart code-fixers (aka bulb actions)](https://github.com/rezanid/xrmtools/wiki/Enabling-Xrm-Tools-for-Old-plugins)
* [Easily call Custom APIs and Actions from your plugin](https://github.com/rezanid/xrmtools/wiki/Calling-Custom-APIs-Action-and-Other-Messages)

## Xrm Tools is a good citizen in Visual Studio
* XrmTools does not need admin privileges and will not apply any system-wide change in your machine nor the configuration of your Visual Studio or projects. 
* Xrm Tools does not send any telemetry information from your machine.
* Xrm Tools does not do any code generation at build time. In other words everything is done in development time and you will see the changes transparently and can benefit from Git for diffing and reviewing the code.
* Xrm Tools does not force you to change your coding style. In other words, you don't have to inherit from a specific class or apply a special interface or things like that.
* All you need to apply are a number of attributes to define your intent so the extension knows where and how to help.
* Should you decide to uninstall Xrm Tools, whatever you have developed with it will continue to work because it is standard C# code and there is no unsupported trickery going on behind the scenes.

## Plugin Registration As Code

XRM Tools enables you to define and implement Power Platform plugins and custom APIs entirely as code. The attributes in your source code are the [single source of truth](https://en.wikipedia.org/wiki/Single_source_of_truth).
By brining plugin registrations to the code you will benefit from Git and the entire echosystem that you have around the code. You are able to review the code and trace the history of every change and the reason behind them. Your team mates are able to just clone the code and know where to test (if you manage the environment at solution or project file level).

# Where to start
The best way to start using Xrm Tools is:
* [Getting Started](https://github.com/rezanid/xrmtools/wiki/Getting-started) guide in the [Wiki](https://github.com/rezanid/xrmtools/wiki)
* YouTube Videos:
  * [Short - Why Xrm Tools](https://www.youtube.com/shorts/nnoKjmEwEVY)
  * [Writing a Hello World Plugin](https://www.youtube.com/watch?v=6E2AE8vrEbI)
  * [Enabling Code Generation and Dependency Injection](https://www.youtube.com/watch?v=C6XR08AckP0)

# Learn more
To learn more about Xrm Tools extension for Visual Studio check the [wiki](https://github.com/rezanid/xrmtools/wiki)
