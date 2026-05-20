import { Outlet, Link, NavLink } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { ListTodo, LayoutDashboard } from "lucide-react";

export function Layout() {
  const { t } = useTranslation();

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="border-b border-gray-200 bg-white">
        <div className="mx-auto flex h-14 max-w-6xl items-center px-6">
          <Link to="/" className="flex items-center gap-2 font-semibold text-gray-900">
            <LayoutDashboard className="h-5 w-5" />
            {t("app.title")}
          </Link>
          <nav className="ml-8 flex gap-1">
            <NavLink
              to="/"
              end
              className={({ isActive }) =>
                `flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
                  isActive ? "bg-indigo-50 text-indigo-700" : "text-gray-600 hover:text-gray-900"
                }`
              }
            >
              <LayoutDashboard className="h-4 w-4" />
              {t("nav.dashboard")}
            </NavLink>
            <NavLink
              to="/todos"
              className={({ isActive }) =>
                `flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
                  isActive ? "bg-indigo-50 text-indigo-700" : "text-gray-600 hover:text-gray-900"
                }`
              }
            >
              <ListTodo className="h-4 w-4" />
              {t("nav.todos")}
            </NavLink>
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-6xl px-6 py-8">
        <Outlet />
      </main>
    </div>
  );
}
