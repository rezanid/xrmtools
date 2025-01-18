# What is Xrm Tools
Xrm Tools is a Visual Studio extension that aims to make Power Platform development feel native within Visual Studio. You will get Intellisense support for your Plugins, generate boiler plate code, typed entities and more right from within Visual Studio. You have full control over code generation using [Scriban](https://github.com/scriban/scriban) templates.. Let me show you!

>🎺 NOTE!
>
>Xrm Tools is not yet published to Visual Studio Gallery, but it will be very soon. Stay tuned!

# How to install
You will soon be able to get it from Visual Studio's native Extensions gallery. Once installed, just create a new solution and a library project under it or just open an existing solution that you already have. Don't worry It will not change your project structure. You are the one behind the driving wheel at all times.

# Connecting to a development environment
First you need to connect to an environment so that XrmTools can see where you are developing so that it can help you along the way. You can add as many environments as you want and you can use your own account to connect to all your environments or you can safely use application accounts if you would prefer that. Let's connect to an environment.
1. Open the "Tool" menu and select "Options"
2. All the way to the bottom you will find "Xrm Tools".
3. Click on the three dots in front of "Environments".
   ![image](https://github.com/user-attachments/assets/6df080b6-832c-4845-a927-24a9be1ea538)

5. Here you can add a new Power Platform environment to Visual Studio. You can give your environment a name and a connection string. The connection string is very similar to the Xrm Tooling of Microsoft, only simpler and with extra security features built-in. For example you can use the following to use your current account to connect to an environment.
   ```shell
   Url=<environment URL>;Integrated Security=True;TenantId=<your tenant id>
   ```
   Your connection string can be even as simple as just the URL of your environment.
   ```shell
   <environment URL>
   ```
   Xrm Tools will try to find your tenant ID by making a request and will try to use your currently authenticated user that is running Visual Studio if you have SSO with your tenant. Otherwise it will display a popup and asks your to login.
   You can read more about connection strings [here](https://github.com/rezanid/xrmtools/wiki/Providing-Connection-Strings).
7. Set "Current Environment" to the environment that you just created.
8. Click "Ok" button to save the settings.

# Making a new Power Platform Plugin.
1. Right-click on the project that will contain the plugin and select "Manage NuGet Packages..."
2. Install the latest "XrmTools.Meta" package.
3. Right-click on the project or anywhere under it and select Add > New > Class (or simply <kbd>Shift</kbd> + <kbd>Alt</kbd> + <kbd>C</kbd>).
4. Make your class `partial` and add `Plugin` attribute to your class. You can name the class anything your like. This attribute makes XrmTools aware that this is a plugin.
   ```csharp
   [Plugin]
   public partial class MyPlugin
   {
   }
   ```
5. Now we will add the `[Step]` attribute and that is where the magic starts happening! Try to add a Step attribute similar to the one below.
   ```csharp
   [Plugin]
   [Step("account", "Create", "accountnumber,accountratingcode,accountcategorycode", Stages.PostOperation, ExecutionMode.Asynchronous)]
   public partial class MyPlugin
   {
   }
   ```
   Noticed how the Intellisense helps you pick the right entity or when you set the message, only the messages that apply to `account` entity are displayed?
   
   ![image](https://github.com/user-attachments/assets/cef86da3-6f8f-4936-b516-ca6935b2ad6a)

7. Now, let's add an [Image] attribute.
   ```csharp
   [Plugin("MyPlugin")]
   [Step("account", "Update", "accountnumber,accountratingcode,accountcategorycode", Stages.PostOperation, ExecutionMode.Asynchronous)]
   [Image(ImageTypes.PreImage, "Target")]
   public partial class MyPlugin
   {
   }
   ```

# Adding code generation to a plugin
After adding attributes to a plugin class, Xrm Tools knows more about your intentions and can help you even more. For example it can generate the typical code you would write in your plugins or generate typed entities that will be useful for your plugin. To enable code generation for your plugin do the following.
1. Make sure the name of your plugin file has the word "Plugin" in it. The casing doesn't matter.
2. Make sure that your plugin class is a `partial` class. Just like the one we created in [Making a new Power Platform Plugin](#making-a-new-Power-Platform-Plugin).
3. Right-click on your plugin class in the Solution Explorer and select "Set as plugin definition".

# Learn more
To learn more about Xrm Tools extension for Visual Studio check the [wiki](https://github.com/rezanid/xrmtools/wiki)
Now, every time you save your plugin file, another file with be generated that contains all the code you need. You can fully customize the generated code in future if you want. Let's explore what's been generated and how we can use it.
