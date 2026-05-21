import { create } from "zustand";
import { persist } from "zustand/middleware";

interface TenantState {
  activeTenantId: string | null;
  setActiveTenant: (id: string) => void;
}

export const useTenantStore = create<TenantState>()(
  persist(
    (set) => ({
      activeTenantId: null,
      setActiveTenant: (id) => set({ activeTenantId: id }),
    }),
    { name: "tenant-storage" },
  ),
);
