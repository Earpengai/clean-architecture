import { Routes, Route, Navigate, useParams } from "react-router-dom";
import { AuthLayout } from "@/pages/AuthLayout";
import { AppShell } from "@/components/AppShell";
import { AdminGuard } from "@/components/AdminGuard";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { PermissionGuard } from "@/components/PermissionGuard";
import { Dashboard } from "@/pages/Dashboard";
import { TodosPage } from "@/pages/TodosPage";
import { TodoDetailPage } from "@/pages/TodoDetailPage";
import { TenantsPage } from "@/pages/TenantsPage";
import { TenantDetailPage } from "@/pages/TenantDetailPage";
import { RolesPage } from "@/pages/RolesPage";
import { UsersPage } from "@/pages/UsersPage";
import { UserDetailPage } from "@/pages/UserDetailPage";
import { InvitationsPage } from "@/pages/InvitationsPage";
import { AcceptInvitationPage } from "@/pages/AcceptInvitationPage";
import { LoginPage } from "@/pages/LoginPage";
import { RegisterPage } from "@/pages/RegisterPage";
import { ProfilePage } from "@/pages/ProfilePage";
import { ForgotPasswordPage } from "@/pages/ForgotPasswordPage";
import { ResetPasswordPage } from "@/pages/ResetPasswordPage";
import { VerifyEmailPage } from "@/pages/VerifyEmailPage";
import { LoginTwoFactorPage } from "@/pages/LoginTwoFactorPage";
import { OnboardingGuard } from "@/components/OnboardingGuard";
import { AdminTenantsPage } from "@/pages/admin/AdminTenantsPage";
import { AdminSubscriptionPage } from "@/pages/admin/AdminSubscriptionPage";
import { SubscriptionPage } from "@/pages/SubscriptionPage";
import { SubscriptionBillingPage } from "@/pages/SubscriptionBillingPage";

function LegacyTodoRedirect() {
  const { id } = useParams<{ id: string }>();
  return <Navigate to={`/app/todos/${id}`} replace />;
}

function LegacyTenantRedirect() {
  const { id } = useParams<{ id: string }>();
  return <Navigate to={`/tenant/tenants/${id}`} replace />;
}

function LegacyUserRedirect() {
  const { id } = useParams<{ id: string }>();
  return <Navigate to={`/tenant/users/${id}`} replace />;
}

export function AppRoutes() {
  return (
    <Routes>
      {/* === PUBLIC AUTH ROUTES === */}
      <Route element={<AuthLayout />}>
        <Route path="auth/login" element={<LoginPage />} />
        <Route path="auth/register" element={<RegisterPage />} />
        <Route path="auth/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="auth/reset-password" element={<ResetPasswordPage />} />
        <Route path="auth/verify-email" element={<VerifyEmailPage />} />
        <Route path="auth/login-2fa" element={<LoginTwoFactorPage />} />
      </Route>

      {/* === PUBLIC ACCEPT INVITATION === */}
      <Route path="invitations/:token/accept" element={<AcceptInvitationPage />} />

      {/* === PROTECTED ROUTES === */}
      <Route element={<ProtectedRoute />}>
        {/* Application Section */}
        <Route element={<OnboardingGuard />}>
          <Route path="app" element={<AppShell section="app" />}>
            <Route index element={<Dashboard />} />
            <Route path="todos" element={<TodosPage />} />
            <Route path="todos/:id" element={<TodoDetailPage />} />
            <Route path="profile" element={<ProfilePage />} />
          </Route>
        </Route>

        {/* Tenant Management Section */}
        <Route path="tenant" element={<AppShell section="tenant" />}>
          <Route path="tenants" element={<TenantsPage />} />
          <Route path="tenants/:id" element={<TenantDetailPage />} />
          <Route element={<PermissionGuard permissions={["roles:read"]} />}>
            <Route path="roles" element={<RolesPage />} />
          </Route>
          <Route element={<PermissionGuard permissions={["users:read"]} />}>
            <Route path="users" element={<UsersPage />} />
            <Route path="users/:id" element={<UserDetailPage />} />
            <Route path="invitations" element={<InvitationsPage />} />
          </Route>
          <Route element={<PermissionGuard permissions={["tenants:write"]} />}>
            <Route path="subscription" element={<SubscriptionPage />} />
            <Route path="billing" element={<SubscriptionBillingPage />} />
          </Route>
        </Route>

        {/* Admin Section */}
        <Route element={<AdminGuard />}>
          <Route path="admin" element={<AppShell section="admin" />}>
            <Route path="tenants" element={<AdminTenantsPage />} />
            <Route path="subscription" element={<AdminSubscriptionPage />} />
          </Route>
        </Route>
      </Route>

      {/* === LEGACY REDIRECTS === */}
      <Route path="login" element={<Navigate to="/auth/login" replace />} />
      <Route path="register" element={<Navigate to="/auth/register" replace />} />
      <Route path="forgot-password" element={<Navigate to="/auth/forgot-password" replace />} />
      <Route path="reset-password" element={<Navigate to="/auth/reset-password" replace />} />
      <Route path="verify-email" element={<Navigate to="/auth/verify-email" replace />} />
      <Route path="todos" element={<Navigate to="/app/todos" replace />} />
      <Route path="todos/:id" element={<LegacyTodoRedirect />} />
      <Route path="tenants" element={<Navigate to="/tenant/tenants" replace />} />
      <Route path="tenants/:id" element={<LegacyTenantRedirect />} />
      <Route path="roles" element={<Navigate to="/tenant/roles" replace />} />
      <Route path="users" element={<Navigate to="/tenant/users" replace />} />
      <Route path="users/:id" element={<LegacyUserRedirect />} />
      <Route path="invitations" element={<Navigate to="/tenant/invitations" replace />} />
      <Route path="profile" element={<Navigate to="/app/profile" replace />} />

      {/* === DEFAULT REDIRECTS === */}
      <Route path="/" element={<Navigate to="/app" replace />} />
      <Route path="*" element={<Navigate to="/app" replace />} />
    </Routes>
  );
}
