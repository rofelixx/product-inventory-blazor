# Product Inventory (Blazor)

A small Product Inventory management app built with Blazor Web App (Interactive Server render mode), .NET 9. No database, no auth — an in-memory repository stands in for a real backing store, per the challenge brief.

## Running it

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) or newer

### Run

```bash
dotnet run --project src/ProductInventory.Web
```

Then open the URL printed in the console (e.g. `http://localhost:5126`). The app comes pre-seeded with 10 sample products so there's something to interact with immediately.

### Run the tests

```bash
dotnet test
```

## Project layout

```
src/ProductInventory.Web/
  Models/Product.cs                   The Product model, annotated with the required validation rules.
  Services/IProductRepository.cs      Repository abstraction (async CRUD + search).
  Services/InMemoryProductRepository.cs  In-memory implementation; every operation goes through
                                          Task.Delay(500) to simulate real network/database latency.
  Components/Pages/Home.razor         The single page: dashboard + search + table + dialogs.
  Components/Shared/                  Reusable components: DashboardStats, ProductTable,
                                       ProductFormDialog, ConfirmDialog, LoadingSpinner.
tests/ProductInventory.Tests/         xUnit tests for the repository and model validation.
```

## How the requirements are met

- **Table view** — `ProductTable.razor` renders Name, Price, Quantity, and an Active/Inactive badge; on narrow screens it collapses into stacked cards (see the `@media (max-width: 640px)` rules in `wwwroot/app.css`).
- **Search while typing** — the full product list is loaded once and cached client-side; `Home.razor`'s `OnSearchInput` debounces keystrokes by 250ms before re-filtering, so the UI stays responsive without hammering the repository on every keypress. The match is case-insensitive (`StringComparison.OrdinalIgnoreCase`).
- **Create/Edit with validation** — `ProductFormDialog.razor` wraps a single `EditForm` (shared between create and edit) using `DataAnnotationsValidator` against the rules declared directly on `Product` (`[Required]`, `[StringLength(100)]`, `[Range]` for price `> 0` and quantity `>= 0`).
- **Delete with confirmation** — `ConfirmDialog.razor` is a generic yes/no modal reused for delete confirmation.
- **Async + simulated latency** — every `IProductRepository` method awaits `Task.Delay(500)` before touching the in-memory list.
- **Loading indicators** — `Home.razor` tracks `_isLoading` / `_isSaving` / `_isDeleting` flags and swaps in `LoadingSpinner.razor` or disables buttons with an in-progress label while a repository call is in flight.
- **Dashboard** — `DashboardStats.razor` computes Total Products, Active Products, and Inventory Value (`Σ Price × Quantity`) from the currently loaded product list.
- **Dependency Injection** — `IProductRepository` is registered as a singleton in `Program.cs` and injected into `Home.razor` via `@inject`.
- **No third-party UI frameworks** — styling is hand-written CSS in `wwwroot/app.css` (the default Bootstrap template assets were removed); no component libraries like MudBlazor/Radzen are referenced.

## Bonus items implemented

- **Error handling** — repository calls in `Home.razor` are wrapped in try/catch; failures surface as a dismissible banner instead of crashing the page.
- **Responsive UI** — the product table reflows into stacked cards below 640px; the toolbar and dashboard grid wrap naturally on small screens.
- **Unit tests** — `InMemoryProductRepositoryTests` covers CRUD, search, latency, `ReplaceAllAsync`, and the clone-on-read isolation guarantee; `ProductValidationTests` covers every validation rule on `Product`.
- **Local storage persistence** — every create/edit/delete serializes the full product list to the browser's `localStorage` (key `productInventory.products`) via `IJSRuntime`, from `PersistToLocalStorageAsync()` in `Home.razor`. On the first client render after an app start, `OnAfterRenderAsync` reads that key back and, if present, replaces the repository's contents (`IProductRepository.ReplaceAllAsync`) before the page renders — so a reload keeps your data instead of resetting to the 10 seeded products. JS interop failures (storage disabled, corrupted JSON) are swallowed silently, falling back to the seeded data.

  **Known limitation**: `IProductRepository` is registered as a `Singleton`, so its state is shared across every browser/tab connecting to the app — not per-user — while `localStorage` is per-browser. `IProductRepository.IsHydrated` guards against every new tab re-importing its own local copy over the already-shared state: only the *first* tab to render after an app start triggers hydration. This keeps the shared-state model coherent, at the cost of later tabs not re-syncing their own separately-saved snapshot. The JS interop calls themselves aren't unit-tested (this project doesn't use bUnit); the underlying `ReplaceAllAsync`/`IsHydrated` repository logic is.
