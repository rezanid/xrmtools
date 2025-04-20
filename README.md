# What is Xrm Tools
Xrm Tools is a Visual Studio extension that aims to make Power Platform development feel native within Visual Studio. You will get Intellisense support for your Plugins, generate boiler plate code, typed entities and more right from within Visual Studio. You will also have full control over code generation using [Scriban](https://github.com/scriban/scriban) templates. Let me show you!

> [!NOTE]
>
> Xrm Tools is not yet published to Visual Studio Gallery, but it will be very soon. Stay tuned!

# How to install
You will soon be able to get it from Visual Studio's native Extensions gallery. Once installed, just create a new solution and a library project under it or just open an existing solution that you already have. Don't worry It will not change your project structure. You are the one behind the driving wheel at all times.

# Connecting to a development environment
First you need to connect to an environment so that XrmTools can connect to that environment. You can add as many environments as you want in Visual Studio and assign them to your solutions or projects. Let's add a new environment.
1. Open the "Tool" menu and select "Options"
2. All the way to the bottom you will find "Xrm Tools".
3. Click on the three dots in front of "Environments".

<img src="https://github.com/user-attachments/assets/fe042c93-b923-4522-9811-ea7032674ec0" alt="https://github.com/user-attachments/assets/fe042c93-b923-4522-9811-ea7032674ec0" width=605 />

5. Here you can add a new Power Platform environment to Visual Studio. You can give your environment a name and a connection string. The connection string is very similar to the Xrm Tooling of Microsoft, only simpler and with extra security features built-in. For example you can use the following syntax to connect to an environment using your AD account.
   ```shell
   Url=<environment URL>;Integrated Security=True;TenantId=<your tenant id>
   ```
   Since integrated security is the default, you can leave it out and then Url will be the only parameter you can just enter the Url of your environment as connection string.
   ```shell
   <environment URL>
   ```
   Xrm Tools will try to find your tenant ID by making a request and will try to use your currently authenticated user that is running Visual Studio if you have SSO with your tenant. Otherwise it will display a popup and asks for your credentials.
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

<img src="https://github.com/user-attachments/assets/3afd3687-3ca2-4c96-8e6f-51a047da7b02" alt="attribute auto-completion screenshot" width=500 />

> [!NOTE]
>
> Noticed those buttons under the auto-completion context menu? You can use them to filter table types. ***S**tandard*, ***A**ctivity*, ***V**irtual*, and ***E**lastic* tables are supported. You can also use shortcuts to apply them. <kbd>Alt</kbd> + <kbd>S</kbd>, <kbd>A</kbd>, <kbd>V</kbd>, or <kbd>E</kbd>.

7. Now, let's add an [Image] attribute.
   ```csharp
   [Plugin("MyPlugin")]
   [Step("account", "Update", "accountnumber,accountratingcode,accountcategorycode", Stages.PostOperation, ExecutionMode.Asynchronous)]
   [Image(ImageTypes.PreImage, "Target")]
   public partial class MyPlugin
   {
   }
   ```
   You will get a similar experience when writing filtering attributes. You'll notice that attribute names are colored differently so you can distinguish them easily.

<img src="https://github.com/user-attachments/assets/1f841973-9b07-4c78-b2b5-9331188b4231" alt="Screenshot of Intellisense in Visual Studio for filtering attributes" width=700 />

# Adding code generation to a plugin
After adding attributes to a plugin class, Xrm Tools knows more about your intentions and can help you even more. For example it can generate the typical code you would write in your plugins or generate typed entities that will be useful for your plugin. To enable code generation for your plugin do the following.
1. Make sure the name of your plugin file has the word "Plugin" in it. The casing doesn't matter.
2. Make sure that your plugin class is a `partial` class. Just like the one we created in [Making a new Power Platform Plugin](#making-a-new-Power-Platform-Plugin).
3. Right-click on your plugin class in the Solution Explorer and select "Set as plugin definition".
4. Now you just need to save your code. just press <kbd>Ctrl</kbd> + <kbd>S</kbd> to save the file that has your plugin class.

> [!NOTE]
>
> Every time you save the file, XRM Tools reads the attributes and the name of your class, retrieves all the necessary metadata from the environment, finds the best matching template and finally sends everything to the code generator.

# Registering Plugins
Developing plugins doesn't end with the build of course. Once the first version of your plugin is ready and it's passing all its unit tests you will need to register your plugin. This is the time that you would typically open the Terminal and type `pac tool prt` to start Microsoft's Plugin Registration tool and carefully register the plugin along with its registration steps and images, making sure you are not making any mistakes because it won't be always easy to detect. You might be thinking with all those attributes that you added to your plugin in [Making a new Power Platform Plugin](#making-a-new-power-platform-plugin) could Xrm Tools magically figure out what's needed and take care of the registration for you? That's exactly what we are about to do.

## Registering a single plugin
1. Right-click on the plugin file in the Solution Explorer.
2. From the context menu, select "Register Plugin(s)".

<img src="https://github.com/user-attachments/assets/20bd65f7-2bbf-4598-8381-c27548225a8b" alt="Screenshot of context menu of CS file in Visual Studio Solution Explorer" width=450 />

> [!NOTE]
>
> If there are more than one plugin in the ".cs" file, all of them will be registered. 

## Registering all plugins in a project
1. Right-click on the **project** that contains your plugin in the Solution Explorer.
2. From the context menu, select "Register Plugin(s)".

<img src="https://github.com/user-attachments/assets/5c67ee94-cc77-43e2-abb0-4d4c82a6eebb" alt="Screenshot of context menu of project in Visual Studio Solution Explorer" width=500 />

> [!NOTE]
>
> When running this command at project level, the output assembly of your project along with all the plugins, including their steps and images will be registered.

When running "Register plugin(s)" command either at project or file level, everything that's needed will be done automatically for you. For example:
* If you have not yet built the project or the build is not up to date with the latest changes, Xrm Tools will build your project.
* The output assembly from your project will be uploaded as part of the registration along with assembly registration, plugin registraions, steps and images.
* The entire registration will happen in one transaction meaning that if anything goes wrong you will not be left with a faulty registration and instead everything will be rolled back to previous state.

> [!NOTE]
> The attributes in your source code are the [single source of truth](https://en.wikipedia.org/wiki/Single_source_of_truth). This means that if you have used previously registered your plugins using any other tool, **all plugin registrations** will be replaced by those defined in your source code.

# Learn more
To learn more about Xrm Tools extension for Visual Studio check the [wiki](https://github.com/rezanid/xrmtools/wiki)
Now, every time you save your plugin file, another file with be generated that contains all the code you need. You can fully customize the generated code in future if you want. Let's explore what's been generated and how we can use it.
