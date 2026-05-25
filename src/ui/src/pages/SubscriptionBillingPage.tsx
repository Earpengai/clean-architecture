import { useEffect, useRef, useState, useMemo } from "react";
import { useTranslation } from "react-i18next";
import QRCode from "qrcode";
import { useInitiatePayment, useCheckPayment, usePaymentHistory, usePricing } from "@/api/billing";
import { useTenantSubscription } from "@/api/subscription";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { Check, CreditCard, RefreshCw, History, Clock } from "lucide-react";

interface PlanOption {
  planId: string;
  planName: string;
  monthlyPrice: number;
  yearlyPrice: number;
  monthlyText: string;
  yearlyText: string;
  badgeColor: string;
}

function planBadgeColor(plan: string) {
  switch (plan) {
    case "Free": return "bg-gray-100 text-gray-700";
    case "Pro": return "bg-blue-100 text-blue-700";
    case "Enterprise": return "bg-purple-100 text-purple-700";
    default: return "bg-indigo-100 text-indigo-700";
  }
}

function periodLabel(period: string) {
  return period;
}

function PlanCard({
  option,
  selected,
  billingPeriod,
  isCurrent,
  onSelect,
}: {
  option: PlanOption;
  selected: boolean;
  billingPeriod: string;
  isCurrent: boolean;
  onSelect: (planId: string, period: string) => void;
}) {
  const isYearly = billingPeriod === "Yearly";
  const priceText = isYearly ? option.yearlyText : option.monthlyText;

  return (
    <Card
      className={`transition-all ${isCurrent ? "opacity-60 ring-1 ring-gray-200" : "cursor-pointer hover:border-gray-300"} ${
        selected && !isCurrent
          ? "ring-2 ring-indigo-500 border-indigo-200"
          : ""
      }`}
      onClick={() => !isCurrent && onSelect(option.planId, billingPeriod)}
    >
      <CardContent className="p-6">
        <div className="flex items-center justify-between mb-3">
          <Badge className={option.badgeColor}>{option.planName}</Badge>
          {isCurrent && (
            <Badge className="bg-gray-100 text-gray-600">Current</Badge>
          )}
          {selected && !isCurrent && (
            <Badge className="bg-indigo-100 text-indigo-700">
              <Check className="h-3 w-3 mr-1" />
              Selected
            </Badge>
          )}
        </div>
        <div className="text-3xl font-bold text-gray-900 mb-1">{priceText}</div>
        <div className="flex gap-2 mt-4">
          <button
            type="button"
            disabled={isCurrent}
            onClick={(e) => {
              e.stopPropagation();
              if (!isCurrent) onSelect(option.planId, "Monthly");
            }}
            className={`flex-1 px-3 py-1.5 text-xs font-medium rounded-md border transition-colors ${
              selected && billingPeriod === "Monthly" && !isCurrent
                ? "border-indigo-500 bg-indigo-50 text-indigo-700"
                : "border-gray-200 text-gray-500 hover:border-gray-300 disabled:opacity-40 disabled:cursor-not-allowed"
            }`}
          >
            Monthly
          </button>
          <button
            type="button"
            disabled={isCurrent}
            onClick={(e) => {
              e.stopPropagation();
              if (!isCurrent) onSelect(option.planId, "Yearly");
            }}
            className={`flex-1 px-3 py-1.5 text-xs font-medium rounded-md border transition-colors ${
              selected && billingPeriod === "Yearly" && !isCurrent
                ? "border-indigo-500 bg-indigo-50 text-indigo-700"
                : "border-gray-200 text-gray-500 hover:border-gray-300 disabled:opacity-40 disabled:cursor-not-allowed"
            }`}
          >
            Yearly
          </button>
        </div>
      </CardContent>
    </Card>
  );
}

const QR_EXPIRY_SECONDS = 120;

function formatCountdown(totalSeconds: number) {
  const min = Math.floor(totalSeconds / 60);
  const sec = totalSeconds % 60;
  return `${min.toString().padStart(2, "0")}:${sec.toString().padStart(2, "0")}`;
}

function QrPayment({
  qr,
  md5,
  amount,
  currency,
  planLabel,
  onRegenerate,
}: {
  qr: string;
  md5: string;
  amount: number;
  currency: string;
  planLabel: string;
  onRegenerate: () => void;
}) {
  const { t } = useTranslation();
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [secondsLeft, setSecondsLeft] = useState(QR_EXPIRY_SECONDS);
  const { data: checkResult } = useCheckPayment(md5);

  useEffect(() => {
    if (canvasRef.current) {
      QRCode.toCanvas(canvasRef.current, qr, { width: 200, margin: 1 });
    }
  }, [qr]);

  useEffect(() => {
    if (checkResult?.isCompleted) return;
    const timer = setInterval(() => {
      setSecondsLeft((prev) => {
        if (prev <= 1) {
          clearInterval(timer);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(timer);
  }, [checkResult?.isCompleted]);

  if (checkResult?.isCompleted) {
    return (
      <div className="text-center py-6">
        <div className="inline-flex h-16 w-16 items-center justify-center rounded-full bg-green-100 mb-4">
          <Check className="h-8 w-8 text-green-600" />
        </div>
        <p className="text-lg font-semibold text-green-700">{t("billing.paymentSuccess")}</p>
        <p className="text-sm text-gray-500 mt-1">{t("billing.paymentSuccessDesc")}</p>
        {checkResult.transaction && (
          <div className="mt-4 p-3 bg-gray-50 rounded-md text-left text-xs text-gray-600 space-y-1">
            <p><span className="font-medium">Amount:</span> {checkResult.transaction.amount} {checkResult.transaction.currency}</p>
            <p><span className="font-medium">From:</span> {checkResult.transaction.fromAccountId}</p>
            <p><span className="font-medium">Ref:</span> {checkResult.transaction.externalRef}</p>
          </div>
        )}
      </div>
    );
  }

  if (secondsLeft <= 0) {
    return (
      <div className="text-center py-6">
        <p className="text-red-500 font-medium mb-1">{t("billing.expired")}</p>
        <p className="text-sm text-gray-400 mb-4">{t("billing.expiredDesc")}</p>
        <Button variant="outline" onClick={onRegenerate}>
          <RefreshCw className="h-4 w-4 mr-2" />
          {t("billing.generateNew")}
        </Button>
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center gap-4">
      <div className="w-48 h-48 flex items-center justify-center bg-white rounded-lg border border-gray-200 p-3">
        <canvas ref={canvasRef} className="w-full h-full" />
      </div>

      <div className="text-center w-full space-y-1">
        <p className="text-sm font-medium text-gray-900">
          {amount.toFixed(2)} {currency}
        </p>
        <p className="text-xs text-gray-500">
          {planLabel} Plan
        </p>
      </div>

      <div className="flex items-center gap-1.5 text-sm">
        <Clock className="h-3.5 w-3.5 text-amber-600" />
        <span className={secondsLeft <= 30 ? "text-red-500 font-medium" : "text-gray-500"}>
          {formatCountdown(secondsLeft)}
        </span>
      </div>

      <p className="text-xs text-gray-400 text-center">{t("billing.qrHint")}</p>
    </div>
  );
}

export function SubscriptionBillingPage() {
  const { t } = useTranslation();
  const [selectedPlanId, setSelectedPlanId] = useState<string | null>(null);
  const [billingPeriod, setBillingPeriod] = useState<string>("Monthly");
  const [paymentData, setPaymentData] = useState<{
    qr: string;
    md5: string;
    amount: number;
    currency: string;
    planLabel: string;
  } | null>(null);
  const [error, setError] = useState("");

  const initiatePayment = useInitiatePayment();
  const { data: history, isLoading: historyLoading } = usePaymentHistory();
  const { data: pricing, isLoading: pricingLoading } = usePricing();
  const { data: currentSub, isLoading: subLoading } = useTenantSubscription();

  const currentPlanName = currentSub?.subscriptionPlan ?? "";
  const currentBillingPeriod = currentSub?.billingPeriod ?? "";

  const plans: PlanOption[] = useMemo(() => {
    if (!pricing) return [];

    const grouped: Record<string, { planName: string; monthly: number; yearly: number }> = {};
    for (const p of pricing) {
      if (!grouped[p.planId]) {
        grouped[p.planId] = { planName: p.plan, monthly: 0, yearly: 0 };
      }
      const planPrices = grouped[p.planId]!;
      if (p.billingPeriod === "Monthly") planPrices.monthly = p.amount;
      if (p.billingPeriod === "Yearly") planPrices.yearly = p.amount;
    }

    return Object.entries(grouped)
      .filter(([, prices]) => prices.planName !== "Free")
      .map(([planId, prices]) => ({
        planId,
        planName: prices.planName,
        monthlyPrice: prices.monthly,
        yearlyPrice: prices.yearly,
        monthlyText: `$${prices.monthly.toFixed(2)}/mo`,
        yearlyText: `$${prices.yearly.toFixed(2)}/yr`,
        badgeColor: planBadgeColor(prices.planName),
      }));
  }, [pricing]);

  const isCurrentPlan = (planId: string, period: string) =>
    planId === currentPlanName && period === currentBillingPeriod;

  const handleSelectPlan = (planId: string, period: string) => {
    setSelectedPlanId(planId);
    setBillingPeriod(period);
    setPaymentData(null);
    setError("");
  };

  const handlePay = () => {
    if (selectedPlanId === null) return;
    setError("");

    initiatePayment.mutate(
      { subscriptionPlanId: selectedPlanId, billingPeriod },
      {
        onSuccess: (data) => {
          const planOption = plans.find((p) => p.planId === selectedPlanId);
          const isYearly = billingPeriod === "Yearly";
          const amount = isYearly ? (planOption?.yearlyPrice ?? 0) : (planOption?.monthlyPrice ?? 0);

          setPaymentData({
            qr: data.qr,
            md5: data.md5,
            amount,
            currency: "USD",
            planLabel: planOption?.planName ?? "",
          });
          window.scrollTo({ top: 0, behavior: "smooth" });
        },
        onError: (err) => {
          setError(err.message);
        },
      },
    );
  };

  const handleRegenerate = () => {
    setPaymentData(null);
    handlePay();
  };

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">{t("billing.title")}</h1>
      </div>

      {subLoading ? (
        <Skeleton className="h-10 w-64" />
      ) : currentSub && currentPlanName !== "Free" ? (
        <div className="flex items-center gap-3 p-3 rounded-md bg-gray-50 border border-gray-100">
          <span className="text-sm text-gray-500">{t("billing.currentPlan")}:</span>
          <Badge className={planBadgeColor(currentPlanName)}>
            {currentPlanName}
          </Badge>
          {currentBillingPeriod !== "None" && (
            <span className="text-xs text-gray-500">{periodLabel(currentBillingPeriod)}</span>
          )}
          {currentSub.subscriptionExpiresAt && (
            <span className="text-xs text-gray-400">
              {t("billing.expires")}: {new Date(currentSub.subscriptionExpiresAt).toLocaleDateString()}
            </span>
          )}
        </div>
      ) : null}

      {error && (
        <div className="p-3 rounded-md bg-red-50 border border-red-200 text-sm text-red-600">{error}</div>
      )}

      {!paymentData && (
        <div>
          {pricingLoading ? (
            <div className="grid gap-4 md:grid-cols-2">
              <Skeleton className="h-48 w-full" />
              <Skeleton className="h-48 w-full" />
            </div>
          ) : plans.length > 0 ? (
            <>
              <p className="text-sm text-gray-500 mb-4">{t("billing.selectPlan")}</p>
              <div className="grid gap-4 md:grid-cols-2">
                {plans.map((option) => (
                  <PlanCard
                    key={option.planId}
                    option={option}
                    selected={selectedPlanId === option.planId}
                    billingPeriod={selectedPlanId === option.planId ? billingPeriod : "Monthly"}
                    isCurrent={isCurrentPlan(option.planId, "Monthly") || isCurrentPlan(option.planId, "Yearly")}
                    onSelect={handleSelectPlan}
                  />
                ))}
              </div>
            </>
          ) : (
            <p className="text-sm text-gray-400">No plans available.</p>
          )}

          <Button
            className="mt-6 w-full"
            disabled={selectedPlanId === null || initiatePayment.isPending || pricingLoading}
            onClick={handlePay}
          >
            {initiatePayment.isPending ? (
              <>
                <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
                {t("billing.generating")}
              </>
            ) : (
              <>
                <CreditCard className="h-4 w-4 mr-2" />
                {t("billing.pay")}
              </>
            )}
          </Button>
        </div>
      )}

      {paymentData && (
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-lg">{t("billing.scanQr")}</CardTitle>
          </CardHeader>
          <CardContent>
            <QrPayment
              qr={paymentData.qr}
              md5={paymentData.md5}
              amount={paymentData.amount}
              currency={paymentData.currency}
              planLabel={paymentData.planLabel}
              onRegenerate={handleRegenerate}
            />
          </CardContent>
        </Card>
      )}

      <Card>
        <CardHeader>
          <CardTitle className="text-lg flex items-center gap-2">
            <History className="h-4 w-4" />
            {t("billing.history")}
          </CardTitle>
        </CardHeader>
        <CardContent>
          {historyLoading && (
            <div className="space-y-2">
              {[...Array(3)].map((_, i) => (
                <Skeleton key={i} className="h-10 w-full" />
              ))}
            </div>
          )}
          {history && history.length === 0 && (
            <p className="text-sm text-gray-400">{t("billing.noHistory")}</p>
          )}
          {history && history.length > 0 && (
            <div className="space-y-2">
              {history.map((p) => (
                <div
                  key={p.id}
                  className="flex items-center justify-between p-3 rounded-md border border-gray-100"
                >
                  <div>
                    <div className="flex items-center gap-2">
                      <Badge className={p.plan === "Pro" ? "bg-blue-100 text-blue-700" : p.plan === "Enterprise" ? "bg-purple-100 text-purple-700" : "bg-indigo-100 text-indigo-700"}>
                        {p.plan}
                      </Badge>
                      <span className="text-xs text-gray-500">{p.billingPeriod}</span>
                    </div>
                    <p className="text-xs text-gray-400 mt-1">
                      {new Date(p.createdAt).toLocaleDateString()}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-medium text-gray-900">
                      ${p.amount.toFixed(2)} {p.currency}
                    </p>
                    <Badge className={
                      p.status === "Completed"
                        ? "bg-green-100 text-green-700"
                        : p.status === "Failed"
                          ? "bg-red-100 text-red-700"
                          : "bg-yellow-100 text-yellow-700"
                    }>
                      {p.status}
                    </Badge>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
