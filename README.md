# CRM Backend ASP Web Core

# Introduction

This Repo is the backend web api for the [CRM frontend Vuejs App](https://github.com/MohammedAlsayed/crm-frontend).

# Getting Started
This is backend is built with dotnet 6.0.416 SDK.

# Installation

1. Run `dotnet restore` to install the dependencies
2. Run `dotnet run` to start the server
3. Run `dotnet watch run` to start the server with watch mode
4. Run `dotnet ef database update --context CrmContext` for database migration

# Running

- The server will be running on `https://localhost:7222`
- To view available APIs go to `https://localhost:7222/swagger/index.html`

# Helpful commands

- To remove migrations `dotnet ef migrations remove`
- create migrations `dotnet ef migrations add InitialCreate --context CrmContext`
- add modifications `dotnet ef migrations add ModelRevisions --context CrmContext`
- update database `dotnet ef database update --context CrmContext`