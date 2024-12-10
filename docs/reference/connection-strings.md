When creating a new environment, you need to provide a connection string so that Xrm Tools can connect to the environment and enhance your development with information that it gets from that environment behind the scene. In its simplest form a connection string can be just the URL of your environment like the following:
```
https://myenvironment1.crm4.dynamics.com
```
The above connection string is the same as:
```
Url=https://myenvironment1.crm4.dynamics.com
```
Which is the same as:
```
Url=https://myenvironment1.crm4.dynamics.com;Integrated Security=True
```
When you leave out a parameter Xrm Tools tries to guess the best possible value. The authentication is always based on Azure Entra (formerly Azure Active Directory). `Integrated Security` means that we try to use your currently logged in account and if required you will see the familiar Microsoft's login page in a popup where your can securely provide your credentials.

You can easily find the URL and your tenant ID from the **Session details** link in https://make.powerapps.com.
![image](https://github.com/user-attachments/assets/132ff744-30d4-4057-87f5-035255b44941)

The following table, lists all the supported parameters you can use when building a connection string. The casing of the letters in the parameter name is not taken into account and is just for better readability.
| Parameter | Description |
|---|---|
|URL, Resource | The URL of the Power Platform environment. Please always use a development environment for your development to make sure you are developing based on the latest version of your solution in development.
| Integrated Security | When set to `True`, the currently logged in account will be used for authentication. In case required, you might see a standard Microsoft login popup where you can securely provide your credentials as configured by your administrator. Xrm Tools will never have access your credentials. Only after the authentication is successful the authentication token will be cached securely. |
| TenantID, Tenant | The tenant ID of your Azure Entra. You can find this information in your **Session details** in https://make.powerapps.com. If you leave out this parameter, Xrm Tools will do a request to the URL you have provided to find our your tenant ID. This means that you can improve your login performance a bit by providing this parameter :wink: |
| ClientID | The client ID or application ID of an application registration in Azure Entra. You can create an application registration to be used by Xrm Tools. [Quickstart: Register an application with the Microsoft identity platform](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-register-app). If you leave out this parameter the default application ID 51F81489-12EE-4A9E-AAAE-A2591F45987D will be used |
| RedirectURI | The default redirect URI is app://58145B91-0C36-4500-8554-080854F2AC97, but you can override it when creating a new application registration and provide the value in this parameter. |
| ClientSecret | When creating an application registration, you can set a client secret, then create an application user in Power Platform's Admin portal and then assign security roles to it. If you set the ClientSecret in the connection string, it will be used to authenticate to the environment. Please never put your client secret value directly in the connection string. You can instead store it in an environment variable or Windows Credential Manager and refer to it in the connection string. More about that later in this page. |
| Thumbprint | Same as the `ClientSecret` above it is used when using an application registration for authenticating to an environment, but instead of a client secret, you provide a client certificate. This is an easier and more secure way of authenticating in many companies and preferred over the client secret. This way your administrator can install the certificate in your machine and manage it for you and your just refer to it by its thumbprint. You can find this in your `certmgr.exe` or your administrator will provide it to you. |
| Device | NOT Currently supported! When set to `True`, Xrm Toolbox will use [Device Flow](https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-device-code) to authenticate. This is useful for special scenarios and does not provide the best user experience in Visual Studio. I recommend you use the `Integrated Security` instead.
