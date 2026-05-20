# Clean Architecture UI

Frontend for the Clean Architecture application, built with React 19 + Vite + TypeScript + Tailwind v4.

## Tech Stack

- **React 19** — UI framework
- **Vite 6** — Build tool and dev server
- **TypeScript 5.8** — Type safety
- **Tailwind CSS v4** — Utility-first CSS
- **React Router 7** — Client-side routing
- **TanStack Query 5** — Server state management
- **Radix UI** — Accessible primitives (Button, Input, Dialog, etc.)
- **Lucide React** — Icon library
- **i18next** — Internationalization

## Getting Started

```powershell
# Ensure Node 22+ is active
nvm use 24

# Install dependencies
npm install

# Start dev server (proxies /api to backend)
npm run dev
```

The dev server runs at `http://localhost:5173` and proxies `/api` requests to the backend.

## Available Scripts

| Command | Description |
|---|---|
| `npm run dev` | Start Vite dev server |
| `npm run build` | Type-check and build for production |
| `npm run preview` | Preview production build |
| `npm run typecheck` | Run TypeScript type checking |
| `npm run test` | Run tests with Vitest |
| `npm run test:watch` | Run tests in watch mode |

## Project Structure

```
src/
├── api/                    # API client and TanStack Query hooks
│   ├── client.ts           # Fetch wrapper with JWT auth
│   ├── types.ts            # Shared API types
│   ├── todos.ts            # Todo endpoints (useTodos, useCreateTodo)
│   └── auth.ts             # Auth endpoints (useLogin, useRegister)
├── components/ui/          # Reusable Radix UI primitives
│   ├── button.tsx          # Button with cva variants
│   ├── input.tsx           # Styled input
│   └── card.tsx            # Card, CardHeader, CardTitle, CardContent
├── features/               # Feature-based domain modules
│   └── todos/
│       ├── components/     # TodoList, TodoItem, TodoForm
│       └── hooks/          # Feature-specific hooks
├── hooks/                  # Cross-cutting hooks
├── i18n/                   # i18next config and locale files
│   ├── index.ts
│   └── locales/en.json
├── lib/                    # Pure utility functions
│   └── cn.ts               # clsx + tailwind-merge
├── pages/                  # Route-level page components
│   ├── Layout.tsx          # App shell with nav
│   ├── Dashboard.tsx       # Home page
│   └── TodosPage.tsx       # Todo management page
├── App.tsx                 # Root component (providers)
├── main.tsx                # Entry point
├── routes.tsx              # Route definitions
└── index.css               # Tailwind import
```

## Environment Variables

Copy `.env.example` to `.env` and adjust as needed:

```powershell
cp .env.example .env
```

- `VITE_API_TARGET` — Backend API URL (default: `http://web-api:8080` for Docker, or `http://localhost:5000` for local dev)
