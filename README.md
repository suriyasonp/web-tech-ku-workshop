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

### GitHub Pages

The repository includes `.github/workflows/pages.yml`, which publishes the `docs/` directory using GitHub Actions.

If Pages has not previously been configured for this repository, open **Repository Settings → Pages → Build and deployment → Source** and select **GitHub Actions** once. Subsequent pushes to `main` that change `docs/**` will deploy automatically.
