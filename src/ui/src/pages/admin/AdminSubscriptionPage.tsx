import { useState } from "react";
import { useTranslation } from "react-i18next";
import { usePlanFeatures, useUpdatePlanFeature, usePlanLimits, useUpdatePlanLimit } from "@/api/subscription";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";

type TabKey = "features" | "limits";

function featureLabel(feature: string) {
  const name = feature.replace("feature:", "");
  return name
    .split("_")
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(" ");
}

function limitLabel(limit: string) {
  const name = limit.replace("limit:", "");
  return name
    .split("_")
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(" ");
}

const planOrder = ["Free", "Pro", "Enterprise"];

function planBadgeColor(plan: string) {
  switch (plan) {
    case "Free": return "bg-gray-100 text-gray-700";
    case "Pro": return "bg-blue-100 text-blue-700";
    case "Enterprise": return "bg-purple-100 text-purple-700";
    default: return "bg-gray-100 text-gray-700";
  }
}

function FeaturesTab() {
  const { t } = useTranslation();
  const { data: features, isLoading, error } = usePlanFeatures();
  const updateFeature = useUpdatePlanFeature();

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[...Array(5)].map((_, i) => (
          <Skeleton key={i} className="h-10 w-full" />
        ))}
      </div>
    );
  }

  if (error) {
    return <p className="text-sm text-red-500">{t("todos.error")}</p>;
  }

  if (!features || features.length === 0) {
    return <p className="text-sm text-gray-500">No features configured.</p>;
  }

  const featureNames = [...new Set(features.map((f) => f.feature))].sort();
  const plans = [...new Set(features.map((f) => f.plan))].sort(
    (a, b) => planOrder.indexOf(a) - planOrder.indexOf(b),
  );

  function isEnabled(plan: string, feature: string) {
    return features?.find((f) => f.plan === plan && f.feature === feature)?.isEnabled ?? false;
  }

  function handleToggle(plan: string, feature: string, current: boolean) {
    updateFeature.mutate({ plan, feature, isEnabled: !current });
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr>
            <th className="text-left px-3 py-2 font-medium text-gray-500">Feature</th>
            {plans.map((plan) => (
              <th key={plan} className="text-center px-3 py-2">
                <Badge className={planBadgeColor(plan)}>{plan}</Badge>
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {featureNames.map((feature) => (
            <tr key={feature} className="border-t border-gray-100">
              <td className="px-3 py-3 font-medium text-gray-900">
                {featureLabel(feature)}
              </td>
              {plans.map((plan) => {
                const enabled = isEnabled(plan, feature);
                const pending = updateFeature.isPending &&
                  updateFeature.variables?.plan === plan &&
                  updateFeature.variables?.feature === feature;
                return (
                  <td key={plan} className="px-3 py-3 text-center">
                    <button
                      type="button"
                      disabled={pending}
                      onClick={() => handleToggle(plan, feature, enabled)}
                      className={`inline-flex h-6 w-11 items-center rounded-full transition-colors focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-1 ${
                        enabled ? "bg-indigo-600" : "bg-gray-200"
                      } ${pending ? "opacity-50" : ""}`}
                    >
                      <span
                        className={`inline-block h-4 w-4 rounded-full bg-white shadow-sm transition-transform ${
                          enabled ? "translate-x-6" : "translate-x-1"
                        }`}
                      />
                    </button>
                  </td>
                );
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function LimitsTab() {
  const { t } = useTranslation();
  const { data: limits, isLoading, error } = usePlanLimits();
  const updateLimit = useUpdatePlanLimit();
  const [editingValues, setEditingValues] = useState<Record<string, number>>({});

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[...Array(5)].map((_, i) => (
          <Skeleton key={i} className="h-10 w-full" />
        ))}
      </div>
    );
  }

  if (error) {
    return <p className="text-sm text-red-500">{t("todos.error")}</p>;
  }

  if (!limits || limits.length === 0) {
    return <p className="text-sm text-gray-500">No limits configured.</p>;
  }

  const limitNames = [...new Set(limits.map((l) => l.limit))].sort();
  const plans = [...new Set(limits.map((l) => l.plan))].sort(
    (a, b) => planOrder.indexOf(a) - planOrder.indexOf(b),
  );

  function currentValue(plan: string, limit: string) {
    return limits?.find((l) => l.plan === plan && l.limit === limit)?.value;
  }

  function getEditKey(plan: string, limit: string) {
    return `${plan}::${limit}`;
  }

  function getDisplayValue(plan: string, limit: string) {
    const key = getEditKey(plan, limit);
    if (key in editingValues) {
      return editingValues[key];
    }
    return currentValue(plan, limit);
  }

  function handleBlur(plan: string, limit: string) {
    const key = getEditKey(plan, limit);
    const val = editingValues[key];
    if (val !== undefined && val !== currentValue(plan, limit)) {
      updateLimit.mutate({ plan, limit, value: val });
    }
    setEditingValues((prev) => {
      const next = { ...prev };
      delete next[key];
      return next;
    });
  }

  function handleChange(plan: string, limit: string, value: string) {
    const num = parseInt(value, 10);
    if (!isNaN(num)) {
      setEditingValues((prev) => ({ ...prev, [getEditKey(plan, limit)]: num }));
    }
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr>
            <th className="text-left px-3 py-2 font-medium text-gray-500">Limit</th>
            {plans.map((plan) => (
              <th key={plan} className="text-center px-3 py-2">
                <Badge className={planBadgeColor(plan)}>{plan}</Badge>
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {limitNames.map((limit) => (
            <tr key={limit} className="border-t border-gray-100">
              <td className="px-3 py-3 font-medium text-gray-900">
                {limitLabel(limit)}
              </td>
              {plans.map((plan) => {
                const val = getDisplayValue(plan, limit);
                const key = getEditKey(plan, limit);
                const isEditing = key in editingValues;
                const pending = updateLimit.isPending &&
                  updateLimit.variables?.plan === plan &&
                  updateLimit.variables?.limit === limit;

                if (isEditing || pending) {
                  return (
                    <td key={plan} className="px-3 py-3 text-center">
                      <Input
                        type="number"
                        min={-1}
                        value={val === -1 ? "" : val}
                        placeholder={val === -1 ? "Unlimited" : undefined}
                        className="mx-auto h-8 w-24 text-center text-sm"
                        onChange={(e) => handleChange(plan, limit, e.target.value)}
                        onBlur={() => handleBlur(plan, limit)}
                        onKeyDown={(e) => {
                          if (e.key === "Enter") {
                            handleBlur(plan, limit);
                          }
                        }}
                        autoFocus
                      />
                    </td>
                  );
                }

                return (
                  <td key={plan} className="px-3 py-3 text-center">
                    <button
                      type="button"
                      onClick={() =>
                        setEditingValues((prev) => ({
                          ...prev,
                          [key]: val ?? -1,
                        }))
                      }
                      className={`px-2 py-1 rounded text-xs font-medium hover:bg-gray-100 transition-colors ${
                        val === -1 ? "text-indigo-600" : "text-gray-700"
                      }`}
                    >
                      {val === -1 ? "Unlimited" : (val ?? "-")}
                    </button>
                  </td>
                );
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function AdminSubscriptionPage() {
  const { t } = useTranslation();
  const [tab, setTab] = useState<TabKey>("features");

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">
          {t("subscription.adminTitle")}
        </h1>
      </div>

      <Card>
        <CardHeader>
          <div className="flex gap-2">
            <button
              type="button"
              onClick={() => setTab("features")}
              className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${
                tab === "features"
                  ? "bg-indigo-100 text-indigo-700"
                  : "text-gray-500 hover:bg-gray-100 hover:text-gray-700"
              }`}
            >
              {t("subscription.features")}
            </button>
            <button
              type="button"
              onClick={() => setTab("limits")}
              className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${
                tab === "limits"
                  ? "bg-indigo-100 text-indigo-700"
                  : "text-gray-500 hover:bg-gray-100 hover:text-gray-700"
              }`}
            >
              {t("subscription.limits")}
            </button>
          </div>
        </CardHeader>
        <CardContent>
          {tab === "features" ? <FeaturesTab /> : <LimitsTab />}
        </CardContent>
      </Card>
    </div>
  );
}
