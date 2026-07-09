# React + Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend using TypeScript with type-aware lint rules enabled. Check out the [TS template](https://github.com/vitejs/vite/tree/main/packages/create-vite/template-react-ts) for information on how to integrate TypeScript and [`typescript-eslint`](https://typescript-eslint.io) in your project.

## Deploying to Render

Add a `Dockerfile` (already included) and `render.yaml` to the repo. To deploy:

- Push the repository to GitHub or another Git remote.
- On Render.com, create a new Web Service and connect your repo.
- Choose "Docker" as the environment — Render will use `render.yaml` if present.
- If needed set environment variables in the Render dashboard (e.g. `ConnectionStrings__DefaultConnection`).
- Render will build the Docker image and expose the app on port 5000 (ASPNETCORE_URLS).

Local build & run (optional):

```powershell
docker build -t backend-mboagreen:latest .
docker run -e ASPNETCORE_URLS="http://+:5000" -p 5000:5000 backend-mboagreen:latest
```

