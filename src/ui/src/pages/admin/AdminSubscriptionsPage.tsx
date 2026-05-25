import { useAdminSubscriptions, useUpdateSubscriptionStatus } from "@/api/admin";
import { useToastStore } from "@/stores/toastStore";
import { extractErrorDetail } from "@/lib/errors";
import { ErrorDisplay } from "@/components/ErrorDisplay";
import { Skeleton } from "@/components/ui/skeleton";

const statusOptions = [
  { value: 0, label: "Active" },
  { value: 1, label: "Trialing" },
  { value: 2, label: "Past Due" },
  { value: 3, label: "Canceled" },
  { value: 4, label: "Expired" },
];

export function AdminSubscriptionsPage() {
  const { data: subscriptions, isLoading, error } = useAdminSubscriptions();
  const updateStatus = useUpdateSubscriptionStatus();
  const addToast = useToastStore((state) => state.addToast);

  function handleStatusChange(subscriptionId: string, newStatus: number) {
    updateStatus.mutate(
      { id: subscriptionId, newStatus },
      {
        onError: (err) => addToast(extractErrorDetail(err), "error"),
      },
    );
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Admin — Subscriptions</h1>
      </div>

      {isLoading && (
        <div className="space-y-3">
          {[...Array(5)].map((_, i) => (
            <Skeleton key={i} className="h-16 w-full" />
          ))}
        </div>
      )}
      {error && <ErrorDisplay error={error} className="mb-4" />}

      {subscriptions && subscriptions.length === 0 && (
        <div className="rounded-lg border border-dashed border-gray-300 p-8 text-center">
          <p className="text-sm text-gray-500">No subscriptions found.</p>
        </div>
      )}

      {subscriptions && subscriptions.length > 0 && (
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="text-left px-4 py-3 font-medium text-gray-500">Tenant</th>
                <th className="text-left px-4 py-3 font-medium text-gray-500">Plan</th>
                <th className="text-left px-4 py-3 font-medium text-gray-500">Status</th>
                <th className="text-left px-4 py-3 font-medium text-gray-500">Billing</th>
                <th className="text-left px-4 py-3 font-medium text-gray-500">Expires</th>
                <th className="text-left px-4 py-3 font-medium text-gray-500">Created</th>
              </tr>
            </thead>
            <tbody>
              {subscriptions.map((sub) => (
                <tr key={sub.id} className="border-b border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <span className="font-medium text-gray-900">{sub.tenantName}</span>
                    <p className="text-xs text-gray-400">{sub.tenantId.slice(0, 8)}...</p>
                  </td>
                  <td className="px-4 py-3">
                    <span className="text-gray-700">{sub.planName}</span>
                  </td>
                  <td className="px-4 py-3">
                    <select
                      value={statusOptions.find((o) => o.label === sub.status)?.value ?? -1}
                      onChange={(e) => {
                        const val = Number(e.target.value);
                        if (val >= 0) handleStatusChange(sub.id, val);
                      }}
                      disabled={updateStatus.isPending}
                      className="text-xs border border-gray-200 rounded px-2 py-1 bg-white"
                    >
                      {statusOptions.map((opt) => (
                        <option key={opt.value} value={opt.value}>
                          {opt.label}
                        </option>
                      ))}
                    </select>
                  </td>
                  <td className="px-4 py-3 text-gray-500">
                    {sub.billingPeriod !== "None" ? sub.billingPeriod : "—"}
                  </td>
                  <td className="px-4 py-3 text-gray-500">
                    {sub.expiresAt
                      ? new Date(sub.expiresAt).toLocaleDateString()
                      : "—"}
                  </td>
                  <td className="px-4 py-3 text-gray-500">
                    {new Date(sub.createdAt).toLocaleDateString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
