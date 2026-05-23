import { useTranslation } from "react-i18next";
import { useTenantSubscription } from "@/api/subscription";
import { ErrorDisplay } from "@/components/ErrorDisplay";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Check, X, Infinity } from "lucide-react";

function planBadgeColor(plan: string) {
  switch (plan) {
    case "Free": return "bg-gray-100 text-gray-700";
    case "Pro": return "bg-blue-100 text-blue-700";
    case "Enterprise": return "bg-purple-100 text-purple-700";
    default: return "bg-gray-100 text-gray-700";
  }
}

function statusBadge(status: string) {
  switch (status) {
    case "Active": return { label: "Active", color: "bg-green-100 text-green-700" };
    case "Trialing": return { label: "Trialing", color: "bg-blue-100 text-blue-700" };
    case "PastDue": return { label: "Past Due", color: "bg-yellow-100 text-yellow-700" };
    case "Canceled": return { label: "Canceled", color: "bg-red-100 text-red-700" };
    case "Expired": return { label: "Expired", color: "bg-gray-100 text-gray-700" };
    default: return { label: status, color: "bg-gray-100 text-gray-700" };
  }
}

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

export function SubscriptionPage() {
  const { t } = useTranslation();
  const { data: sub, isLoading, error } = useTenantSubscription();

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-32 w-full" />
      </div>
    );
  }

  if (error) {
    return <ErrorDisplay error={error} />;
  }

  if (!sub) {
    return <p className="text-sm text-gray-500">No subscription data available.</p>;
  }

  const status = statusBadge(sub.subscriptionStatus);

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">
          {t("subscription.title")}
        </h1>
      </div>

      <div className="mb-6 flex items-center gap-3">
        <span className="text-sm text-gray-500">{t("subscription.plan")}:</span>
        <Badge className={planBadgeColor(sub.subscriptionPlan)}>
          {sub.subscriptionPlan}
        </Badge>
        <Badge className={status.color}>{status.label}</Badge>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">{t("subscription.features")}</CardTitle>
          </CardHeader>
          <CardContent>
            {sub.features.length === 0 ? (
              <p className="text-sm text-gray-400">{t("subscription.noFeatures")}</p>
            ) : (
              <ul className="space-y-2">
                {sub.features.map((f) => (
                  <li key={f.feature} className="flex items-center gap-3 text-sm">
                    {f.isEnabled ? (
                      <Check className="h-4 w-4 text-green-600 shrink-0" />
                    ) : (
                      <X className="h-4 w-4 text-red-400 shrink-0" />
                    )}
                    <span className={f.isEnabled ? "text-gray-900" : "text-gray-400"}>
                      {featureLabel(f.feature)}
                    </span>
                  </li>
                ))}
              </ul>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-lg">{t("subscription.limits")}</CardTitle>
          </CardHeader>
          <CardContent>
            {sub.limits.length === 0 ? (
              <p className="text-sm text-gray-400">{t("subscription.noLimits")}</p>
            ) : (
              <ul className="space-y-2">
                {sub.limits.map((l) => (
                  <li key={l.limit} className="flex items-center justify-between text-sm">
                    <span className="text-gray-900">{limitLabel(l.limit)}</span>
                    <span className="font-medium text-gray-500">
                      {l.value === -1 ? (
                        <span className="inline-flex items-center gap-1 text-indigo-600">
                          <Infinity className="h-3 w-3" />
                          {t("subscription.unlimited")}
                        </span>
                      ) : (
                        l.value
                      )}
                    </span>
                  </li>
                ))}
              </ul>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
