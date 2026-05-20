import { Routes, Route } from "react-router-dom";
import { Layout } from "@/pages/Layout";
import { Dashboard } from "@/pages/Dashboard";
import { TodosPage } from "@/pages/TodosPage";

export function AppRoutes() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<Dashboard />} />
        <Route path="todos" element={<TodosPage />} />
      </Route>
    </Routes>
  );
}
