import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { Ban, Clock, AlertTriangle, ArrowLeft, CreditCard } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";

interface TenantBlockedPageProps {
  reason: "disabled" | "expired" | "trial";
}

export function TenantBlockedPage({ reason }: TenantBlockedPageProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const config = {
    disabled: {
      icon: Ban,
      iconColor: "text-red-500",
      bgColor: "bg-red-50",
      title: t("tenant.disabledTitle"),
      message: t("tenant.disabledMessage"),
    },
    expired: {
      icon: Clock,
      iconColor: "text-amber-500",
      bgColor: "bg-amber-50",
      title: t("tenant.expiredTitle"),
      message: t("tenant.expiredMessage"),
    },
    trial: {
      icon: AlertTriangle,
      iconColor: "text-amber-500",
      bgColor: "bg-amber-50",
      title: t("tenant.trialExpiredTitle"),
      message: t("tenant.trialExpiredMessage"),
    },
  }[reason];

  const Icon = config.icon;

  return (
    <div className="flex items-center justify-center min-h-[60vh] p-4">
      <Card className="max-w-md w-full">
        <CardContent className="p-8 text-center">
          <div className={`inline-flex rounded-full p-3 ${config.bgColor} mb-4`}>
            <Icon className={`h-8 w-8 ${config.iconColor}`} />
          </div>
          <h2 className="text-xl font-semibold text-gray-900 mb-2">
            {config.title}
          </h2>
          <p className="text-sm text-gray-600 mb-6">
            {config.message}
          </p>
          <div className="flex flex-col gap-2">
            <Button
              variant="outline"
              onClick={() => navigate("/tenant/tenants")}
              className="w-full justify-center gap-2"
            >
              <ArrowLeft className="h-4 w-4" />
              {t("tenant.manageTenants")}
            </Button>
            {(reason === "expired" || reason === "trial") && (
              <Button
                onClick={() => navigate("/tenant/billing")}
                className="w-full justify-center gap-2"
              >
                <CreditCard className="h-4 w-4" />
                {t("tenant.upgradePlan")}
              </Button>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
