### Description:
A CRUD MVC .NET Core web application for products with image upload support

### Features:
All CRUD operations

### Requirements:

 1. SQL Server 2017 or higher
 2. Visual Studio 2019
 3. Azure Storage Emulator
 4. .NET Core 2.2

### How to run:

 1. Open project in Visual Studio
 2. Create storage blob container named "products" using the Azure
    storage emulator (preferably easier to use the Azure storage
    explorer to achieve this)
 3. Delete migrations in project structure
 4. Run "update-database" in package manager console

### Nuget packages used:

 1. Microsoft.AspNetCore.App - https://www.nuget.org/packages/Microsoft.AspNetCore.App/
 2. Microsoft.NETCore.App - https://www.nuget.org/packages/Microsoft.NETCore.App/
 3. WindowsAzure.Storage - https://www.nuget.org/packages/WindowsAzure.Storage/
