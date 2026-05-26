import { useState } from "react";
import { Outlet } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
import { AppHeader } from "@/components/AppHeader";
import { SidebarNav, type SidebarSection } from "@/components/SidebarNav";
import { useSidebar } from "@/components/SidebarProvider";
import { LayoutDashboard, X, AlertCircle, Mail } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/hooks/useAuth";
import { useRequestEmailVerification } from "@/api/auth";

interface AppShellProps {
  section: SidebarSection;
}

export function AppShell({ section }: AppShellProps) {
  const { t } = useTranslation();
  const { isOpen, close } = useSidebar();
  const { isEmailVerified } = useAuth();
  const requestVerification = useRequestEmailVerification();
  const [verifySent, setVerifySent] = useState(false);

  return (
    <div className="flex min-h-screen bg-gray-50">
      {/* Desktop sidebar */}
      <aside className="fixed left-0 top-0 hidden h-screen w-64 flex-col border-r border-gray-200 bg-white md:flex">
        <div className="flex h-14 items-center border-b border-gray-200 px-4">
          <Link to="/app" className="flex items-center gap-2 font-semibold text-gray-900">
            <LayoutDashboard className="h-5 w-5 text-indigo-600" />
            {t("app.title")}
          </Link>
        </div>
        <SidebarNav section={section} />
      </aside>

      {/* Mobile sidebar overlay */}
      {isOpen && (
        <>
          <div
            className="fixed inset-0 z-30 bg-black/50 md:hidden"
            onClick={close}
          />
          <aside className="fixed left-0 top-0 z-40 flex h-screen w-64 flex-col border-r border-gray-200 bg-white md:hidden">
            <div className="flex h-14 items-center justify-between border-b border-gray-200 px-4">
              <Link to="/app" className="flex items-center gap-2 font-semibold text-gray-900" onClick={close}>
                <LayoutDashboard className="h-5 w-5 text-indigo-600" />
                {t("app.title")}
              </Link>
              <Button variant="ghost" size="icon" onClick={close}>
                <X className="h-5 w-5" />
              </Button>
            </div>
            <div onClick={close}>
              <SidebarNav section={section} />
            </div>
          </aside>
        </>
      )}

      {/* Main content area */}
      <div className="flex flex-1 flex-col md:ml-64">
        <AppHeader section={section} />
        {!isEmailVerified && (
          <div className="border-b border-amber-200 bg-amber-50 px-4 py-2 md:px-8">
            <div className="flex items-center justify-between gap-3">
              <div className="flex items-center gap-2 text-sm text-amber-800">
                <AlertCircle className="h-4 w-4 flex-shrink-0" />
                <span>Please verify your email address to access all features.</span>
              </div>
              <div className="flex items-center gap-2">
                {verifySent ? (
                  <span className="text-xs text-green-600 font-medium">Verification email sent</span>
                ) : (
                  <Button
                    variant="outline"
                    size="sm"
                    className="border-amber-300 hover:bg-amber-100 text-xs h-8"
                    onClick={() => {
                      requestVerification.mutate(undefined, {
                        onSuccess: () => setVerifySent(true),
                      });
                    }}
                    disabled={requestVerification.isPending}
                  >
                    <Mail className="h-3 w-3 mr-1" />
                    {requestVerification.isPending ? "Sending..." : "Resend Email"}
                  </Button>
                )}
              </div>
            </div>
          </div>
        )}
        <main className="flex-1 p-4 md:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
