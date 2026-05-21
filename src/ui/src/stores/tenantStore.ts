import { create } from "zustand";
import { persist } from "zustand/middleware";

interface TenantState {
  activeTenantId: string | null;
  activeTenantIdentifier: string | null;
  setActiveTenant: (id: string, identifier: string) => void;
  clearActiveTenant: () => void;
}

export const useTenantStore = create<TenantState>()(
  persist(
    (set) => ({
      activeTenantId: null,
      activeTenantIdentifier: null,
      setActiveTenant: (id, identifier) => set({ activeTenantId: id, activeTenantIdentifier: identifier }),
      clearActiveTenant: () => set({ activeTenantId: null, activeTenantIdentifier: null }),
    }),
    { name: "tenant-storage" },
  ),
);
