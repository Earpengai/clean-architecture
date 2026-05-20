import { Outlet, Link, NavLink, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "@/hooks/useAuth";
import { useLogout } from "@/api/auth";
import { TenantSwitcher } from "@/features/tenants/components/TenantSwitcher";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
} from "@/components/ui/dropdown-menu";
import { ListTodo, LayoutDashboard, Building2, Shield, Users, Settings as SettingsIcon, LogOut, Settings } from "lucide-react";

export function Layout() {
  const { t } = useTranslation();
  const { isAuthenticated } = useAuth();
  const logout = useLogout();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="border-b border-gray-200 bg-white">
        <div className="mx-auto flex h-14 max-w-6xl items-center justify-between px-6">
          <div className="flex items-center">
            <Link to="/" className="flex items-center gap-2 font-semibold text-gray-900">
              <LayoutDashboard className="h-5 w-5" />
              {t("app.title")}
            </Link>
            {isAuthenticated && (
              <nav className="ml-6 flex gap-1">
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
                <NavLink
                  to="/tenants"
                  className={({ isActive }) =>
                    `flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
                      isActive ? "bg-indigo-50 text-indigo-700" : "text-gray-600 hover:text-gray-900"
                    }`
                  }
                >
                  <Building2 className="h-4 w-4" />
                  Tenants
                </NavLink>
                <NavLink
                  to="/roles"
                  className={({ isActive }) =>
                    `flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
                      isActive ? "bg-indigo-50 text-indigo-700" : "text-gray-600 hover:text-gray-900"
                    }`
                  }
                >
                  <Shield className="h-4 w-4" />
                  Roles
                </NavLink>
                <NavLink
                  to="/users"
                  className={({ isActive }) =>
                    `flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
                      isActive ? "bg-indigo-50 text-indigo-700" : "text-gray-600 hover:text-gray-900"
                    }`
                  }
                >
                  <Users className="h-4 w-4" />
                  Users
                </NavLink>
              </nav>
            )}
          </div>
          <div className="flex items-center gap-3">
            {isAuthenticated && <TenantSwitcher />}
            <div className="flex items-center gap-2">
              {isAuthenticated ? (
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <Button variant="ghost" size="icon">
                      <SettingsIcon className="h-5 w-5" />
                    </Button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end" className="w-48">
                    <DropdownMenuItem onClick={() => navigate("/profile")}>
                      <Settings className="h-4 w-4" />
                      Settings
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => navigate("/admin/tenants")}>
                      <Settings className="h-4 w-4" />
                      Admin
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={handleLogout}>
                      <LogOut className="h-4 w-4" />
                      Sign Out
                    </DropdownMenuItem>
                  </DropdownMenuContent>
                </DropdownMenu>
              ) : (
                <>
                  <Button variant="ghost" size="sm" onClick={() => navigate("/login")}>
                    Sign In
                  </Button>
                  <Button size="sm" onClick={() => navigate("/register")}>
                    Sign Up
                  </Button>
                </>
              )}
            </div>
          </div>
        </div>
      </header>
      <main className="mx-auto max-w-6xl px-6 py-8">
        <Outlet />
      </main>
    </div>
  );
}
