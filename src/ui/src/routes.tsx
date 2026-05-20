import { Routes, Route } from "react-router-dom";
import { Layout } from "@/pages/Layout";
import { Dashboard } from "@/pages/Dashboard";
import { TodosPage } from "@/pages/TodosPage";
import { TodoDetailPage } from "@/pages/TodoDetailPage";
import { TenantsPage } from "@/pages/TenantsPage";
import { TenantDetailPage } from "@/pages/TenantDetailPage";
import { RolesPage } from "@/pages/RolesPage";
import { UsersPage } from "@/pages/UsersPage";
import { UserDetailPage } from "@/pages/UserDetailPage";
import { InvitationsPage } from "@/pages/InvitationsPage";
import { LoginPage } from "@/pages/LoginPage";
import { RegisterPage } from "@/pages/RegisterPage";
import { ProfilePage } from "@/pages/ProfilePage";
import { ForgotPasswordPage } from "@/pages/ForgotPasswordPage";
import { ResetPasswordPage } from "@/pages/ResetPasswordPage";
import { VerifyEmailPage } from "@/pages/VerifyEmailPage";
import { AdminTenantsPage } from "@/pages/admin/AdminTenantsPage";
import { ProtectedRoute } from "@/components/ProtectedRoute";

export function AppRoutes() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="login" element={<LoginPage />} />
        <Route path="register" element={<RegisterPage />} />
        <Route path="forgot-password" element={<ForgotPasswordPage />} />
        <Route path="reset-password" element={<ResetPasswordPage />} />
        <Route path="verify-email" element={<VerifyEmailPage />} />

        <Route element={<ProtectedRoute />}>
          <Route index element={<Dashboard />} />
          <Route path="todos" element={<TodosPage />} />
          <Route path="todos/:id" element={<TodoDetailPage />} />
          <Route path="tenants" element={<TenantsPage />} />
          <Route path="tenants/:id" element={<TenantDetailPage />} />
          <Route path="roles" element={<RolesPage />} />
          <Route path="users" element={<UsersPage />} />
          <Route path="users/:id" element={<UserDetailPage />} />
          <Route path="invitations" element={<InvitationsPage />} />
          <Route path="profile" element={<ProfilePage />} />
          <Route path="admin/tenants" element={<AdminTenantsPage />} />
        </Route>
      </Route>
    </Routes>
  );
}
