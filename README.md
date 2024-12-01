# What is Xrm Tools
Xrm Tools is a Visual Studio extension that aims to make Power Platform development feel native within Visual Studio. You will get Intellisense support for your Plugins, generate boiler plate code, typed entities and more right from within Visual Studio. If you don't agree with the opinionated code or anything like that you get to customize your craft easily and safely. Let me show you!

# How to install
You will soon be able to get it from Visual Studio's native Extensions gallery. Once installed, just create a new solution and a library project under it or just open an existing solution that you already have. Don't worry It will not change anything in your project. You are the one behind the driving wheel at all times.

# Connecting to a development environment
First you need to connect to an environment so that XrmTools can see where you are developing so that it can help you along the way. You can add as many environments as you want and you can use your own account to connect to all your environments or you can safely use application accounts if you would prefer that. Let's connect to an environment.
1. Open the "Tool" menu and select "Options"
2. All the way to the bottom you will find "Xrm Tools".
3. Click on the three dots in front of "Environments"

# Making a new Power Platform Plugin.
1. Right-click on the project that will contain the plugin and select "Manage NuGet Packages..."
2. Install the latest "XrmTools.Plugins" package.
3. Right-click on the project or anywhere under it and select Add > New > Class (or simple Shift + Alt + C).
4. Make your class `partial` and add `Plugin` attribute to your class. You can name the class anything your like. This attribute makes XrmTools aware that this is a plugin.
   ```csharp
   [Plugin]
   public partial class MyPlugin
   {
   }
   ```
5. Now we will add the `[Step]` attribute and that is where the magic starts happening! Using this attribute you will define plugin registration steps. Its required parameters are the following.
   * `string entityName` - is the logical name of the entity (table) that your plugin will act on.
   * `string message` - is the message sent to that entity (table).
   * `string filteringAttributes` - is the list of attributes (columns) of the table that your plugin will be informed of.
   * `Stages stage` - is the stage of the execution pipeline that you plugin will get called in.
   * `ExecutionMode mode` - is the execution mode of your plugin (think async or sync).
   These should look familiar if you have been registering your plugins using PRT (Plugin Registration Tool), Xrm Toolbox or some other tool. But now you can define them write in your code and you get full Intellisense support to help not make any human errors.
