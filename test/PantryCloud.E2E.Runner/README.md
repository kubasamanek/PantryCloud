## PantryCloud E2E Runner

Lightweight, deterministic end-to-end scenario runner for PantryCloud. It executes a single run of each scenario (no load) against the API Gateway and is intended for local and CI verification (for example, Kind or docker-compose environments).

### What it covers

Scenarios are grouped by domain and executed via the API Gateway using real HTTP calls. Each scenario has a stable `Name` that you pass on the command line.

| Domain | Scenario name | Description |
|--------|---------------|-------------|
| **Identity** | `identity-token-lifecycle` | Register, verify email, login, refresh token, logout; refresh after logout must fail. |
| | `identity-password-reset` | Forgot password, reset password, login with old password (must fail) and new password (must succeed). |
| | `registration` | Register, verify email, and login once to validate the basic happy path. |
| **Household** | `household-create-and-get` | Create a household and retrieve the current household; asserts identifier and name consistency. |
| | `household-invite` | Owner creates a household, invites a member, member accepts; asserts the member’s current household. |
| | `household-transfer-and-leave` | Transfer ownership to a member, original owner leaves; asserts that ownership and membership are updated correctly. |
| **Pantry** | `pantry-crud` | Create, list, update, and delete a pantry item; validates list state after each operation. |
| | `pantry-deplete` | Create an item with quantity 1, update to 0, and assert depleted state. |
| **Recipe** | `recipe-search` | Register and authenticate a user, search recipes by ingredients, and fetch the first recipe by id. |
| **Shopping list** | `shopping-list-create-with-items` | Create a list, add multiple items, retrieve the list, and assert that all expected items are present. |
| | `shopping-list-completion` | Create a list with multiple items, complete all items, and assert that the list is fully checked off. |
| **Notification** | `notification-member-joined` | After invite/accept, asserts that the household owner receives a “member joined” notification. |
| | `notification-pantry-depletion` | After depleting a pantry item, asserts that a depletion notification is emitted. |
| **Audit** | `audit-entry` | Create a pantry item and query household audit entries; asserts that a corresponding create entry exists. |

If you pass an unknown scenario name, the runner prints the list of available names to the console (`Program.cs`).

### Running scenarios

- **Run all scenarios (default):**

  ```bash
  dotnet run --project test/PantryCloud.E2E.Runner
  ```

  or explicitly:

  ```bash
  dotnet run --project test/PantryCloud.E2E.Runner -- --all
  ```

- **Run a single scenario by name:**

  ```bash
  dotnet run --project test/PantryCloud.E2E.Runner -- --scenario <name>
  ```

  Example:

  ```bash
  dotnet run --project test/PantryCloud.E2E.Runner -- --scenario household-invite
  ```

The process exits with:

- `0` when all selected scenarios succeed.
- `1` when at least one selected scenario fails.

This makes the runner suitable for CI pipelines and other automated checks.

### Configuration

Configuration is loaded in the following order:

1. `appsettings.json`
2. `appsettings.Development.json` (optional)
3. Environment variables with the `PANTRYCLOUD_E2E_` prefix (which override JSON settings)

Key settings:

- **Gateway base URL**
  - JSON: `Gateway:BaseUrl`
  - Environment: `PANTRYCLOUD_E2E_GATEWAY_URL` (for example, `http://localhost:5050` or `http://pantry.test` for Kind)

- **Environment label**
  - JSON: `Environment:Name`
  - Environment: `PANTRYCLOUD_E2E_ENV` (for example, `kind`, `docker-compose`); used only in console output to identify the target environment.

### Requirements

- API Gateway and all downstream services (Identity, Household, Pantry, Recipe, ShoppingList, Notification, Audit) must be running and reachable at the configured gateway base URL.
- Notification and Audit scenarios depend on asynchronous event processing (RabbitMQ and consumers); a short delay is expected before notifications and audit entries become visible.

### Interpreting results

- Each scenario prints a header and a final line:
  - `--- Scenario: <name> ---`
  - `Result: SUCCESS` or `Result: FAILED - <reason>`
- When multiple scenarios are run, failures do not stop execution; all selected scenarios run, and the overall exit code reflects whether any scenario failed.
