import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "@/hooks/useAuth";
import { useIsAdmin } from "@/hooks/useIsAdmin";
import { useLogout } from "@/api/auth";
import { useSidebar } from "@/components/SidebarProvider";
import { TenantSwitcher } from "@/features/tenants/components/TenantSwitcher";
import { InvitationNotification } from "@/features/invitations/components/InvitationNotification";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
} from "@/components/ui/dropdown-menu";
import {
  LayoutDashboard,
  LogOut,
  Settings,
  Shield,
  Menu,
  ChevronDown,
  Building2,
  ListTodo,
} from "lucide-react";
import { cn } from "@/lib/cn";

export type HeaderSection = "app" | "tenant" | "admin";

interface AppHeaderProps {
  section: HeaderSection;
}

export function AppHeader({ section }: AppHeaderProps) {
  const { t } = useTranslation();
  const { isAuthenticated } = useAuth();
  const isAdmin = useIsAdmin();
  const logout = useLogout();
  const navigate = useNavigate();
  const { toggle } = useSidebar();

  const handleLogout = () => {
    logout();
    navigate("/auth/login");
  };

  const sectionLabel =
    section === "app"
      ? t("section.application")
      : section === "tenant"
        ? t("section.management")
        : t("section.administration");

  const sectionIcon =
    section === "app" ? <ListTodo className="h-4 w-4" /> : section === "tenant" ? <Building2 className="h-4 w-4" /> : <Shield className="h-4 w-4" />;

  return (
    <header className="sticky top-0 z-10 flex h-14 items-center gap-4 border-b border-gray-200 bg-white px-4 md:px-6">
      <Button variant="ghost" size="icon" className="md:hidden" onClick={toggle}>
        <Menu className="h-5 w-5" />
      </Button>

      <div className="flex items-center gap-2">
        <LayoutDashboard className="h-5 w-5 text-indigo-600" />
        <span className="hidden text-sm font-semibold text-gray-900 sm:inline">{t("app.title")}</span>
      </div>

      <div className="flex-1" />

      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" size="sm" className="gap-1.5">
            {sectionIcon}
            <span className="hidden text-sm font-medium sm:inline">{sectionLabel}</span>
            <ChevronDown className="h-3 w-3 text-gray-400" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="w-48">
          <DropdownMenuItem
            className={cn(section === "app" && "bg-indigo-50 text-indigo-700")}
            onClick={() => navigate("/app")}
          >
            <ListTodo className="h-4 w-4" />
            {t("section.application")}
          </DropdownMenuItem>
          <DropdownMenuItem
            className={cn(section === "tenant" && "bg-indigo-50 text-indigo-700")}
            onClick={() => navigate("/tenant/tenants")}
          >
            <Building2 className="h-4 w-4" />
            {t("section.management")}
          </DropdownMenuItem>
          {isAdmin && (
            <DropdownMenuItem
              className={cn(section === "admin" && "bg-indigo-50 text-indigo-700")}
              onClick={() => navigate("/admin/tenants")}
            >
              <Shield className="h-4 w-4" />
              {t("section.administration")}
            </DropdownMenuItem>
          )}
        </DropdownMenuContent>
      </DropdownMenu>

      {isAuthenticated && <TenantSwitcher />}

      {isAuthenticated && <InvitationNotification />}

      <div className="flex items-center gap-2">
        {isAuthenticated ? (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon">
                <Settings className="h-5 w-5" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-48">
              <DropdownMenuItem onClick={() => navigate("/app/profile")}>
                <Settings className="h-4 w-4" />
                Settings
              </DropdownMenuItem>
              {isAdmin && (
                <DropdownMenuItem onClick={() => navigate("/admin/tenants")}>
                  <Shield className="h-4 w-4" />
                  Admin
                </DropdownMenuItem>
              )}
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={handleLogout}>
                <LogOut className="h-4 w-4" />
                Sign Out
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        ) : (
          <>
            <Button variant="ghost" size="sm" onClick={() => navigate("/auth/login")}>
              Sign In
            </Button>
            <Button size="sm" onClick={() => navigate("/auth/register")}>
              Sign Up
            </Button>
          </>
        )}
      </div>
    </header>
  );
}
