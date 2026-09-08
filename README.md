# Web Technology KU Workshop

Self-paced hands-on materials for the KU Web Technology Development workshop.

## .NET 10 Minimal API Self-Lab

Students build a Todo API step by step with:

- Visual Studio Code
- .NET 10 Minimal API
- REST and HTTP fundamentals
- DTOs
- CRUD endpoints
- Entity Framework Core
- SQLite
- JWT Bearer authentication
- VS Code REST Client testing

The student-facing lab is in [`docs/index.html`](docs/index.html) and is deployed with GitHub Pages after changes are merged into `main`.

### GitHub Pages

The repository includes `.github/workflows/pages.yml`, which publishes the `docs/` directory using GitHub Actions.

If Pages has not previously been configured for this repository, open **Repository Settings → Pages → Build and deployment → Source** and select **GitHub Actions** once. Subsequent pushes to `main` that change `docs/**` will deploy automatically.
