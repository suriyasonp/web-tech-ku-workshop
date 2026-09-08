# Web Technology KU Workshop

Self-paced hands-on materials for the KU Web Technology Development workshop.

## Full-Stack Todo Self-Labs

Students build a complete Todo application step by step.

### Backend — .NET 10 Minimal API

- Visual Studio Code
- .NET 10 Minimal API
- REST and HTTP fundamentals
- DTOs
- CRUD endpoints
- Entity Framework Core
- SQLite
- JWT Bearer authentication
- VS Code REST Client testing

### Frontend — Vue 3 + TypeScript

- Vue 3 Composition API and Single-File Components
- TypeScript models
- Tailwind CSS
- Axios API client
- Login and JWT Bearer token handling
- Authenticated Todo CRUD integration
- CORS configuration in ASP.NET Core

The student-facing labs are:

- [`docs/index.html`](docs/index.html) — Backend lab
- [`docs/frontend.html`](docs/frontend.html) — Frontend and backend-integration lab

Complete the backend lab first, then continue with the frontend lab.

## Completed Project

The repository also contains the complete runnable project:

```text
backend/TodoApi/   .NET 10 Minimal API, EF Core, SQLite, JWT and OpenAPI
frontend/          Vue 3, TypeScript, Tailwind CSS and Axios
scripts/           Cross-platform development and screenshot scripts
```

### Run locally

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), a current [Node.js LTS](https://nodejs.org/en/download), and then run from the repository root:

```bash
npm install
npm run setup
npm run dev
```

Open:

- Frontend: <http://localhost:5173>
- API: <http://localhost:5000>
- Interactive API reference: <http://localhost:5000/scalar/v1>
- OpenAPI JSON: <http://localhost:5000/openapi/v1.json>

Sign in with `student` / `password`.

### Capture the application

With dependencies installed, run:

```bash
npm run capture
```

This starts both applications, signs in with Playwright using Chromium, creates a demonstration Todo, and saves `docs/images/todo-app-demo.png`.

For documentation-only screenshot generation on a machine without .NET, use the embedded mock API:

```bash
npm run capture:mock
```

### GitHub Pages

The repository includes `.github/workflows/pages.yml`, which publishes the `docs/` directory using GitHub Actions.

If Pages has not previously been configured for this repository, open **Repository Settings → Pages → Build and deployment → Source** and select **GitHub Actions** once. Subsequent pushes to `main` that change `docs/**` will deploy automatically.
