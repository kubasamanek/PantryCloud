# PantryCloud.Web – Frontend

Blazor WebAssembly (standalone) SPA for PantryCloud. It runs entirely in the browser and talks only to the API Gateway.

## What this app is

- **Blazor WebAssembly standalone** – C#/Razor UI is compiled to WebAssembly and runs in the browser. There is no .NET process running on the server in production.
- **Static output** – `dotnet publish` produces a set of static files: `index.html`, JavaScript, `.wasm` binaries, and assets. The browser downloads these and runs the app locally.
- **Single entry point** – All HTTP traffic from the app goes to the **API Gateway** (`http://localhost:5050` in dev). The frontend never calls backend services directly.

## Why nginx?

After publish, the frontend is **only static files**. Something still has to serve them over HTTP:

- **Local dev** – `dotnet run` uses the Blazor dev server (e.g. Kestrel) to serve the app and do hot reload. No nginx.
- **Production / Docker** – There is no long‑running .NET process for the frontend. We need a web server that:
  - Serves `index.html`, `_framework/`, `_content/` over HTTP.
  - Handles SPA routing (e.g. `try_files $uri $uri/ /index.html`) so refreshes on `/some/route` work.

**nginx** is used because it:

1. Serves static files with minimal config.
2. Supports the SPA fallback in one line (`try_files` in `nginx.conf`).
3. Keeps the Docker image small (`nginx:alpine` vs a full .NET runtime image).
4. Is a common, well-understood choice for hosting static/SPA sites.

So: nginx is not part of the “app logic”; it is the **static file host** for the published Blazor output when we run in Docker or any production environment.

## How the frontend works (high level)

```
┌─────────────────────────────────────────────────────────────────┐
│  Browser                                                        │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  PantryCloud.Web (Blazor WASM)                            │  │
│  │  – Loaded from nginx (or dev server): index.html + .wasm  │  │
│  │  – OIDC login/logout via redirects                        │  │
│  │  – API calls go to Gateway with Bearer token              │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
         │                                    │
         │  Static files (first load)          │  API + Auth (HTTPS/HTTP)
         ▼                                    ▼
┌─────────────────┐                 ┌─────────────────┐
│  nginx (or      │                 │  API Gateway     │
│  dev server)    │                 │  :5050           │
│  :3000 / :80    │                 │  → Identity,     │
└─────────────────┘                 │    Pantry, etc.  │
                                    └─────────────────┘
```

- **First load** – Browser requests the site (e.g. `http://localhost:3000`). nginx (or the dev server) returns `index.html` and the browser then loads `_framework/blazor.webassembly.js` and the .wasm runtime. No .NET runs on the server for this.
- **Auth** – The app uses OIDC (Authority at the Gateway). Login/logout are redirects to the Gateway → Identity; tokens are stored in the browser (e.g. session/local storage) and the auth library handles refresh.
- **API** – A named `HttpClient` with `CustomAuthorizationMessageHandler` sends the access token only to the Gateway base URL. Refit clients use this when you add them.

## Why the Gateway allows anonymous for `/api/identity`

The Blazor app talks to the **Authority** at the Gateway: `http://localhost:5050/api/identity`. That is where OIDC discovery and the login/token endpoints live (proxied to the Identity service).

**The problem:** When the user clicks “Login”, there is **no token yet**. The OIDC flow must:

1. **Fetch the discovery document** – `GET .../api/identity/.well-known/openid-configuration` (no auth).
2. **Redirect the user** to the authorization endpoint (still no token).
3. **After redirect** – Exchange the auth code for tokens at the token endpoint.

If the Gateway required a valid JWT for every request to `/api/identity/*`, step 1 would return 401 and the login flow could never start.

**The approach:** The Gateway is configured so that the **Identity route** uses `AuthorizationPolicy = "Anonymous"`. That means:

- The Gateway **does not** require a JWT to forward requests to `/api/identity/*`.
- Discovery, login, token, and other public Identity endpoints are reachable from the browser without a token.
- The **Identity service** still decides which of its own endpoints are public vs protected (e.g. `JwksController` and login/token are public; `SessionsController` is `[Authorize]`).

So “anonymous at the Gateway” only means: “don’t block unauthenticated traffic to the Identity service.” Security is still enforced by the Identity service on each endpoint. This is the usual setup when the Authority is behind an API gateway.

**CORS:** The Gateway also enables CORS for the web origin (e.g. `http://localhost:3000`) so the browser can call the Gateway from the Blazor app. Without CORS, the browser would block the discovery and token requests even when the Gateway allows them.

## Running the frontend

### Local (dev server, no nginx)

From the repo root:

```bash
dotnet run --project src/Web/PantryCloud.Web/PantryCloud.Web.csproj
```

Open the URL shown (e.g. `https://localhost:7xxx`). The API Gateway and Identity service must be reachable at the URLs in `wwwroot/appsettings.json` / `appsettings.Development.json` (e.g. `http://localhost:5050`).

### Docker (nginx serves static files)

From the repo root:

```bash
cd deployments
docker compose up -d web
```

Then open **http://localhost:3000**. The image builds the Blazor app, then the final stage runs nginx and serves the published static files from `/usr/share/nginx/html`.

## Configuration

- **wwwroot/appsettings.json** – `Oidc` (Authority, ClientId, redirect URIs, scopes) and `Gateway:BaseUrl`. Authority is the Gateway URL where OIDC discovery is exposed (e.g. `http://localhost:5050/api/identity`).
- **Gateway:BaseUrl** – Base URL for API calls and for the auth handler (token is attached only to this host).

## Main pieces

| Piece | Role |
|-------|------|
| **Program.cs** | Registers MudBlazor, OIDC, `CustomAuthorizationMessageHandler`, and the named Gateway `HttpClient`. |
| **CustomAuthorizationMessageHandler** | Adds the Bearer token only to requests whose URL is under the Gateway base URL. |
| **Authentication.razor** | Handles OIDC redirects: `login-callback`, `login-failed`, `logout-callback`. |
| **MainLayout.razor** | MudBlazor layout and login/logout in the app bar. |
| **nginx.conf** | Serves static files and SPA fallback (`try_files ... /index.html`). |
| **Dockerfile** | Builds the Blazor app, then runs nginx and copies the published static files into nginx’s document root. |

## Debugging and logging

### Where the app runs

- **In the container (Docker):** Only **nginx** runs. It serves static files (`index.html`, `_framework/*`, `_content/*`). There is **no .NET process** and **no application logs** in the web container. Any logs you see there (e.g. `docker logs`) are nginx access/error logs (requests for `/`, `/css/site.css`, etc.), not Blazor or C# logs.
- **In the browser:** After the first load, **all** app logic runs in the user’s browser: .NET runtime (WASM), your C# code, rendering, API calls, auth. So all **application** debugging and logging for the frontend happens in the browser.

### How to debug

1. **Browser DevTools (F12)**  
   - **Console** – C# `ILogger` output and JavaScript errors. This is where Blazor WebAssembly sends log messages.  
   - **Network** – See requests to the Gateway (e.g. discovery, token, API calls), status codes, and CORS.  
   - **Sources** – Set breakpoints in the generated JavaScript that interacts with .NET; for C# breakpoints use the next option.

2. **Visual Studio / Rider / VS Code**  
   - Run the app with **`dotnet run`** (Blazor dev server), then attach the debugger to the browser or use the built-in “debug Blazor WebAssembly” launch profile. You can set **C# breakpoints** in your Razor and C# code and step through.

3. **Docker / production build**  
   - The same static files are served; there is no debugger in the container. Reproduce issues by opening the site in the browser and using **Console** and **Network** in DevTools. For C# breakpoints, run the app locally with `dotnet run` and the same backend (e.g. Gateway + Identity) if needed.

### Logging in the app

- **`ILogger`** in Blazor WebAssembly is wired to the **browser console**. `logger.LogInformation("…")` and similar show up in the DevTools Console (and in the browser’s native console API).
- **Production:** Logs stay in the user’s browser; you don’t get them on the server. Common approaches:
  - **Console only** – Rely on users or support to open DevTools and share console output when something goes wrong.
  - **Front-end monitoring** – Send errors or telemetry to a service (e.g. Application Insights, Sentry) from the client so you get reports and stack traces in production without needing access to the user’s machine.

So: the web container only serves files; all execution and logging for the SPA happen in the browser, and production debugging is done via browser DevTools and/or a client-side logging/monitoring service.

## Adding API calls

Use Refit interfaces under `Services/ApiClient/` and register them with the Gateway `HttpClient` (so they use the same auth handler). See `.cursor/rules/FRONTEND.md` for conventions.
