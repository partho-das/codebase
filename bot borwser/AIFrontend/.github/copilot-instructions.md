# Copilot Instructions for AI Agents

## Project Overview
- This is an Angular frontend project using TypeScript, SCSS, and Tailwind CSS.
- The main app logic is in `src/app/`, with components in `components/`, services in `services/`, and pipes in `pipes/`.
- The app appears to be an AI-powered chat or assistant interface, with major UI components like `chat-panel`, `full-display-ai-chat`, and `card-list`.

## Architecture & Data Flow
- **Component-driven:** Each UI feature is a self-contained component (see `src/app/components/`).
- **Service-based state & logic:** Shared logic and data fetching are handled by Angular services (see `src/app/services/`).
  - Example: `ai.service.ts` likely handles AI communication; `data.service.ts` manages app data.
- **Pipes:** Custom pipes (e.g., `markdown.pipe.ts`) are used for data transformation in templates.
- **Routing:** App routes are defined in `app.routes.ts`.

## Developer Workflows
- **Start dev server:** `npm start` (runs Angular app with live reload)
- **Run tests:** `npm test`
- **Build for production:** `ng build` (see `angular.json` for config)
- **Tailwind CSS:** Configured via `tailwind.config.js` and used in `styles.scss`.
- **Proxy:** API requests may be proxied via `proxy.conf.json` during development.

## Project Conventions
- **Component naming:** Use kebab-case for folders, PascalCase for class names.
- **SCSS for styles:** Each component has its own `.scss` file.
- **Service injection:** Use Angular dependency injection for cross-component logic.
- **No direct DOM manipulation:** Use Angular templates and bindings.
- **Public assets:** Place static files in `public/`.

## Integration & Communication
- **AI/Backend integration:** Handled via services (see `ai.service.ts`, `event-source.service.ts`).
- **Event-driven UI:** Components communicate via Angular inputs/outputs and shared services.
- **Markdown rendering:** Use the custom `markdown` pipe in templates for rich text.

## Key Files & Directories
- `src/app/components/` — All major UI components
- `src/app/services/` — Shared logic, data, and AI integration
- `src/app/pipes/markdown.pipe.ts` — Markdown transformation
- `angular.json`, `package.json` — Project configuration
- `tailwind.config.js`, `styles.scss` — Styling setup

## Example Patterns
- To add a new feature, create a new component in `components/` and a service if shared logic is needed.
- Use observables in services for async data and subscribe in components.
- Use the `markdown` pipe in templates: `{{ message.text | markdown }}`

---

For more details, review the structure in `src/app/` and the configuration files at the project root.
