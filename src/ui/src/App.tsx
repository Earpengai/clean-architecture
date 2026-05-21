import { useEffect } from "react";
import { BrowserRouter } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { queryClient } from "@/api/queryClient";
import { useTenantStore } from "@/stores/tenantStore";
import { ToastContainer } from "@/stores/ToastContainer";
import { AppRoutes } from "@/routes";

function TenantChangeInvalidator() {
  useEffect(() => {
    const unsubscribe = useTenantStore.subscribe((state, prev) => {
      if (state.activeTenantId !== prev.activeTenantId) {
        queryClient.invalidateQueries();
      }
    });
    return unsubscribe;
  }, []);

  return null;
}

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <TenantChangeInvalidator />
        <AppRoutes />
        <ToastContainer />
      </BrowserRouter>
    </QueryClientProvider>
  );
}
