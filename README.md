# Food Orders App

A full-stack application for coordinating restaurant orders within an organization. Users can manage organizations and their members, browse restaurants and meals, prepare shared carts, and turn those carts into orders.

The project demonstrates a complete React and ASP.NET Core application with connected business workflows across its frontend, API and database layers.

## Features

- Account registration and JWT-based authentication
- Organization and user management
- Restaurant and meal management
- Shared carts with meal quantities and calculated prices
- Order creation, editing and tracking
- Multiple saved account delivery locations, default selection, and profile settings in the Flutter app
- Paginated, sortable and filterable data views
- Swagger documentation for the REST API

## Technology stack

### Frontend

- React 18
- Vite
- React Router
- Axios
- CSS

### Backend

- ASP.NET Core 8 Web API
- Entity Framework Core
- SQL Server
- JWT authentication
- AutoMapper
- Swagger / OpenAPI

## Project structure

```text
FoodOrdersApp/   React frontend
FoodOrdersApi/   ASP.NET Core REST API
```

## Local development

The project currently runs locally and does not have a hosted production deployment. The Flutter customer app is in the sibling [`FlutterFood`](../FlutterFood) folder.

### Requirements

- .NET 8 SDK
- Node.js and npm

### Backend

The Development environment uses SQLite, so you can run the API without installing SQL Server. From `FoodOrdersApi`:

```bash
dotnet restore
dotnet run --launch-profile https
```

On first launch, the API creates `foodorders.dev.db` in the API project directory. This local database is ignored by git. Swagger is available at `https://localhost:7157/swagger`.

In Development with SQLite, the API also adds sample restaurants and meals, a demo organization/user, and a demo account with a saved address. Seeding is repeatable and does not overwrite existing records. Sign in to the Flutter app with:

- Username: `demo.user`
- Password: `Demo1234!`

These credentials are for local development only.

To use SQL Server instead, set `DatabaseProvider` to `SqlServer` and configure `ConnectionStrings:DataBaseConnection` in `appsettings.Development.json`. Restore the local EF tool and apply migrations before starting the API:

```bash
dotnet tool restore
dotnet ef database update
dotnet run --launch-profile https
```

The local development JWT key is only for local testing; configure a secure `Authentication__JwtKey` before deploying anywhere.

### Account and location API

Authenticated account settings and saved-location endpoints are available to the Flutter app:

- `GET` / `PUT` `api/account/profile` — view or update the signed-in account username; the profile response includes `totalSpent`, the sum of that account's order totals
- `PUT` `api/account/password` — change the signed-in account password
- `GET` `api/address/mine` and `POST` `api/address` — list or add saved delivery locations
- `PUT` `api/address/{id}` — edit a saved location
- `PUT` `api/address/{id}/default` — select the default location
- `DELETE` `api/address/{id}` — remove a saved location

Saved addresses belong to the authenticated account. Checkout copies the chosen address into the order's delivery details so later edits to a saved location do not change an existing order.

### Flutter customer app (Android emulator)

Install Flutter, Android Studio/Android SDK, and Java 17 or newer. Start an Android emulator, then from `FlutterFood`:

```bash
flutter doctor
flutter doctor --android-licenses
flutter pub get
flutter run
```

Start the backend first. The Flutter app connects to the host computer through `https://10.0.2.2:7157` when running in the Android emulator. A physical device requires a host address reachable from the device and a corresponding change in `FlutterFood/lib/core/api_constants.dart`.

### React web app (optional)

From `FoodOrdersApp`, install the dependencies and start the development server:

```bash
npm install
npm run dev
```

The frontend expects the API at `https://localhost:7157/api` and runs at `http://localhost:5173` by default.

## Project status

This is a local portfolio and learning project. It demonstrates the application's architecture and core workflows, but it is not presented as a production service.

## Author

Developed by [Michał Grochowski](https://github.com/grochochotowski).
