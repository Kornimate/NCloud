# NCloud
This is a project to create a One-Drive like application, with authentication, file upload possibilities and file sharing service.

## Start Up
To start the project you need a file named **appSettings.json** in the same folder as the **.csproj** file is. \
The structure of the file is the following:
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AssetRoute": "/",
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "SQLServerConnection": "<your-connection-string>",
    "SQLiteConnection": "Data Source=database.db"

  },
  "EncryptionKey": "<encryption-key>",
  "AdminPassword": "<admin-password>",
  "Branding": {
    "AppName": "<your-app-name>",
    "LogoPath": "<path/to/your/logo>",
    "LogoNoTextPath": "<path/to/logo-with-text>"
  },
  "EmailCredentials": {
    "Email": "<email-address>",
    "Password": "<email-address-application-password>"
  }
}
```
The Email Password is not the original password to your Gmail account. It is a password created by google for applications to have access to your email sending facilities.