# eQuantic Core Data Entity Framework Library

The **eQuantic Data Core** provides a robust implementation of the **Repository Pattern** specifically for **Entity Framework Core**.

This library offers seamless integration with the following database providers:

- **SQL Server**
- **PostgreSQL**
- **MySQL**
- **MongoDB** (via EF Core provider)

## Version 4.4.0

### Key Features and Improvements (v4.4.0)

- **.NET 10 Support**: Full compatibility with .NET 10, including optimized `ExecuteUpdate` operations using the new `UpdateSettersBuilder`.
- **Improved Expression Conversion**: Enhanced reflection-based method lookup for `ExecuteUpdate` setters, ensuring robustness across different .NET frameworks and provider-specific quirks.
- **Optimized Resource Management**: Implemented internal cleanup mechanisms in `UnitOfWork` to better manage memory and database connections.
- **Enhanced Data Integrity**: Fixed shadow field inheritance issues by replacing brittle `new` keyword usage with `internal virtual` properties.
- **Strict Pagination Validation**: Added explicit validation for pagination parameters in `QueryableReadRepository`.
- **Full Multi-Provider Support**: Optimized implementations for **SqlServer**, **PostgreSql**, **MySql**, and **MongoDb**.

## Installation

To install **eQuantic.Core.Data.EntityFramework**, run the following command in the [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console):

```powershell
Install-Package eQuantic.Core.Data.EntityFramework
```

For specific providers, install the corresponding package:

- `eQuantic.Core.Data.EntityFramework.SqlServer`
- `eQuantic.Core.Data.EntityFramework.PostgreSql`
- `eQuantic.Core.Data.EntityFramework.MySql`
- `eQuantic.Core.Data.EntityFramework.MongoDb`

## Usage Examples

The following are examples of implementing the repository pattern:

- [Repository Pattern Implementation](Repository.md)
