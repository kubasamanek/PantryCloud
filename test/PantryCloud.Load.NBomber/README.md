## PantryCloud.Load.NBomber

NBomber-based load testing suite for PantryCloud. It exercises key user flows and write-heavy workloads against the API Gateway to validate performance, rate limiting behaviour, and Horizontal Pod Autoscaler (HPA) scaling.

- **Entry point**: `Program.cs`
- **Configuration**: `appsettings.json` (overridable via `PANTRYCLOUD_` environment variables)
- **Scenario catalog**: `Scenarios/ScenarioCatalog.cs`

### What it covers

Scenarios are defined in `ScenarioCatalog` and selected by name via configuration (`Load:ScenarioName` or `PANTRYCLOUD_LOAD_SCENARIO_NAME`).

| Scenario name | Default profile | Description |
|---------------|-----------------|-------------|
| `UserRegistration` | `Smoke` | Registers new users via the Identity service using a single `register_user` step. Lightweight validation of the registration endpoint and rate limiting for unauthenticated traffic. |
| `AuthenticatedUserFlow` | `HpaLoad` | Full authenticated user flow: register, verify email, login, then repeat a household + pantry flow loop (create household if needed, get household, list pantry, create pantry item). Use this to stress the system and observe HPA behaviour. |
| `PantryWriteLoad` | `HpaLoad` | High-volume pantry item writes. Each virtual user registers, verifies, logs in, ensures a household, then repeatedly creates pantry items. Designed for write-heavy HPA validation of the Pantry service and backing database. |

### Load profiles

Profiles allow you to quickly switch between conservative smoke runs and heavier HPA-focused runs without manually tweaking all numeric settings.

- **Smoke** (default via `Load:Profile` or `PANTRYCLOUD_LOAD_PROFILE`):
  - Short duration, low concurrency for quick validation.
  - Currently configured as: 30s duration, 0s warmup, 1 concurrent copy, 1 authenticated loop, no delay between authenticated steps.

- **HpaLoad**:
  - Longer duration and higher concurrency intended to trigger HPA scaling.
  - Currently configured as: 600s duration, 60s warmup, 20 concurrent copies, 5 authenticated loops, 200ms delay between authenticated steps.

### Configuration

Settings can be provided via:

1. `appsettings.json`
2. `appsettings.Development.json` (optional)
3. Environment variables with the `PANTRYCLOUD_` prefix (override JSON)

Key configuration options:

- **Gateway**
  - JSON: `Gateway:BaseUrl`
  - Environment: `PANTRYCLOUD_GATEWAY_URL` (for example, `http://pantry.test`)

- **Scenario selection and profile**
  - JSON: `Load:ScenarioName`, `Load:Profile`
  - Environment:
    - `PANTRYCLOUD_LOAD_SCENARIO_NAME` (for example, `UserRegistration`, `AuthenticatedUserFlow`, `PantryWriteLoad`)
    - `PANTRYCLOUD_LOAD_PROFILE` (for example, `Smoke`, `HpaLoad`)

- **Load parameters (can be overridden per run)**
  - JSON: `Load:DurationSeconds`, `Load:WarmupSeconds`, `Load:ConcurrentCopies`, `Load:AuthenticatedLoopIterations`, `Load:DelayBetweenAuthenticatedStepsMs`
  - Environment (examples):
    - `PANTRYCLOUD_LOAD_DURATION`
    - `PANTRYCLOUD_LOAD_CONCURRENCY`
    - `PANTRYCLOUD_LOAD_AUTHENTICATED_LOOP_ITERATIONS`
    - `PANTRYCLOUD_LOAD_DELAY_BETWEEN_STEPS_MS`

- **Endpoint paths (optional overrides; defaults live in `appsettings.json`)**
  - `PANTRYCLOUD_REGISTRATION_PATH`, `PANTRYCLOUD_LOGIN_PATH`, `PANTRYCLOUD_VERIFY_EMAIL_PATH`
  - `PANTRYCLOUD_HOUSEHOLD_PATH`, `PANTRYCLOUD_PANTRY_PATH`

- **Reports**
  - JSON: `Reports:OutputDirectory` (default: `./nbomber-reports`)

### Running locally

From the repository root, after starting the cluster (for example, Kind) and deploying PantryCloud via the API Gateway:

```bash
dotnet run --project test/PantryCloud.Load.NBomber
```

If no scenario is specified explicitly, the settings (or defaults) determine which one runs. Typical runs:

- **UserRegistration** (default, `Smoke` profile):

  ```bash
  export PANTRYCLOUD_GATEWAY_URL=http://pantry.test
  export PANTRYCLOUD_LOAD_DURATION=120
  export PANTRYCLOUD_LOAD_CONCURRENCY=5

  dotnet run --project test/PantryCloud.Load.NBomber
  ```

- **AuthenticatedUserFlow** (end-to-end, HPA-oriented load with `HpaLoad` profile):

  ```bash
  export PANTRYCLOUD_GATEWAY_URL=http://pantry.test
  export PANTRYCLOUD_LOAD_SCENARIO_NAME=AuthenticatedUserFlow
  export PANTRYCLOUD_LOAD_PROFILE=HpaLoad
  # optional overrides:
  # export PANTRYCLOUD_LOAD_DURATION=120
  # export PANTRYCLOUD_LOAD_CONCURRENCY=30
  # export PANTRYCLOUD_LOAD_AUTHENTICATED_LOOP_ITERATIONS=5
  # export PANTRYCLOUD_LOAD_DELAY_BETWEEN_STEPS_MS=200

  dotnet run --project test/PantryCloud.Load.NBomber
  ```

- **PantryWriteLoad** (write-heavy pantry workload):

  ```bash
  export PANTRYCLOUD_GATEWAY_URL=http://pantry.test
  export PANTRYCLOUD_LOAD_SCENARIO_NAME=PantryWriteLoad
  export PANTRYCLOUD_LOAD_PROFILE=HpaLoad

  dotnet run --project test/PantryCloud.Load.NBomber
  ```

### Interpreting results

NBomber writes reports to the directory configured by `Reports:OutputDirectory` (default: `./nbomber-reports`). The report file name is derived from the scenario:

- `user_registration` for `UserRegistration`
- `authenticated_user_flow` for `AuthenticatedUserFlow`
- `pantry_write_load` (when configured accordingly in `Program.cs` / settings)

To validate a run:

- Open the **HTML report** in a browser and review.
- Check raw logs from NBomber (console output) for unexpected failures.

### HPA validation

Load scenarios do not assert pod scaling directly. To confirm that replicas scale:

1. Run a heavier scenario such as `AuthenticatedUserFlow` or `PantryWriteLoad` with the `HpaLoad` profile.
2. Observe the HPA and pod counts while the test is running:

   ```bash
   kubectl get hpa -A
   kubectl get pods -A
   ```

3. Correlate increased load (from NBomber reports) with HPA decisions and pod scaling to validate that autoscaling behaves as expected.
