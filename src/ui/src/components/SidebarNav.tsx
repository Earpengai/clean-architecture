import { useTranslation } from "react-i18next";
import { NavLink } from "react-router-dom";
import { cn } from "@/lib/cn";
import {
  LayoutDashboard,
  ListTodo,
  Building2,
  Shield,
  Users,
  Mail,
  User,
  ArrowLeft,
  CreditCard,
  Settings2,
} from "lucide-react";

export type SidebarSection = "app" | "tenant" | "admin";

interface SidebarNavProps {
  section: SidebarSection;
}

interface NavItem {
  label: string;
  path: string;
  icon: React.ReactNode;
}

function NavItemLink({ item }: { item: NavItem }) {
  return (
    <NavLink
      to={item.path}
      end={item.path === "/app"}
      className={({ isActive }) =>
        cn(
          "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
          isActive
            ? "bg-indigo-50 text-indigo-700"
            : "text-gray-600 hover:bg-gray-100 hover:text-gray-900",
        )
      }
    >
      {item.icon}
      {item.label}
    </NavLink>
  );
}

export function SidebarNav({ section }: SidebarNavProps) {
  const { t } = useTranslation();

  const appItems: NavItem[] = [
    { label: t("nav.dashboard"), path: "/app", icon: <LayoutDashboard className="h-4 w-4" /> },
    { label: t("nav.todos"), path: "/app/todos", icon: <ListTodo className="h-4 w-4" /> },
  ];

  const tenantItems: NavItem[] = [
    { label: t("nav.tenants"), path: "/tenant/tenants", icon: <Building2 className="h-4 w-4" /> },
    { label: t("nav.roles"), path: "/tenant/roles", icon: <Shield className="h-4 w-4" /> },
    { label: t("nav.users"), path: "/tenant/users", icon: <Users className="h-4 w-4" /> },
    { label: t("nav.invitations"), path: "/tenant/invitations", icon: <Mail className="h-4 w-4" /> },
    { label: t("nav.subscription"), path: "/tenant/subscription", icon: <CreditCard className="h-4 w-4" /> },
  ];

  const adminItems: NavItem[] = [
    { label: t("nav.adminTenants"), path: "/admin/tenants", icon: <Building2 className="h-4 w-4" /> },
    { label: t("nav.subscription"), path: "/admin/subscription", icon: <Settings2 className="h-4 w-4" /> },
  ];

  const items =
    section === "app" ? appItems : section === "tenant" ? tenantItems : adminItems;

  const sectionTitle =
    section === "app"
      ? t("section.application")
      : section === "tenant"
        ? t("section.management")
        : t("section.administration");

  return (
    <nav className="flex flex-1 flex-col gap-1 px-3 py-4">
      <div className="mb-2 px-3 text-xs font-semibold uppercase tracking-wider text-gray-400">
        {sectionTitle}
      </div>
      {items.map((item) => (
        <NavItemLink key={item.path} item={item} />
      ))}

      {section !== "app" && (
        <>
          <div className="my-2 border-t border-gray-100" />
          <NavLink
            to="/app"
            className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-gray-600 transition-colors hover:bg-gray-100 hover:text-gray-900"
          >
            <ArrowLeft className="h-4 w-4" />
            {t("nav.backToApp")}
          </NavLink>
        </>
      )}

      {section === "app" && (
        <>
          <div className="my-2 border-t border-gray-100" />
          <NavItemLink
            item={{
              label: t("nav.profile"),
              path: "/app/profile",
              icon: <User className="h-4 w-4" />,
            }}
          />
        </>
      )}
    </nav>
  );
}
