import { AlertTriangle, ShieldAlert, Building2, AlertCircle, Ban, Clock } from "lucide-react";
import { parseErrorMessage, extractErrorDetail } from "@/lib/errors";
import { ApiError } from "@/api/client";
import { Button } from "@/components/ui/button";
import { useNavigate } from "react-router-dom";

interface ErrorDisplayProps {
  error: Error | null | undefined;
  className?: string;
}

export function ErrorDisplay({ error, className = "" }: ErrorDisplayProps) {
  const navigate = useNavigate();

  if (!error) {
    return null;
  }

  const detail = extractErrorDetail(error);
  const status = error instanceof ApiError ? error.status : 500;
  const problemType = error instanceof ApiError ? error.problemType : undefined;

  const parsed = parseErrorMessage(status, detail, problemType);

  const Icon = parsed.isTenantBlocked
    ? parsed.tenantBlockReason === "disabled" ? Ban : Clock
    : parsed.isPermissionError
      ? ShieldAlert
      : parsed.isTenantError
        ? Building2
        : parsed.isNotFoundError
          ? AlertCircle
          : AlertTriangle;

  const bgColor = parsed.isTenantBlocked
    ? "bg-red-50 border-red-200"
    : parsed.isPermissionError
      ? "bg-amber-50 border-amber-200"
      : parsed.isTenantError
        ? "bg-blue-50 border-blue-200"
        : parsed.isNotFoundError
          ? "bg-gray-50 border-gray-200"
          : "bg-red-50 border-red-200";

  const textColor = parsed.isTenantBlocked
    ? "text-red-800"
    : parsed.isPermissionError
      ? "text-amber-800"
      : parsed.isTenantError
        ? "text-blue-800"
        : parsed.isNotFoundError
          ? "text-gray-600"
          : "text-red-800";

  const iconColor = parsed.isTenantBlocked
    ? "text-red-500"
    : parsed.isPermissionError
      ? "text-amber-500"
      : parsed.isTenantError
        ? "text-blue-500"
        : parsed.isNotFoundError
          ? "text-gray-400"
          : "text-red-500";

  return (
    <div className={`flex items-start gap-3 rounded-lg border p-4 ${bgColor} ${className}`}>
      <Icon className={`h-5 w-5 shrink-0 ${iconColor}`} />
      <div className="flex-1 min-w-0">
        <p className={`text-sm font-medium ${textColor}`}>{parsed.message}</p>
        {parsed.isTenantBlocked && (
          <div className="mt-2 flex gap-2">
            <Button
              variant="link"
              size="sm"
              className="h-auto p-0 text-xs"
              onClick={() => navigate("/tenant/tenants")}
            >
              Manage Tenants
            </Button>
            {(parsed.tenantBlockReason === "expired" || parsed.tenantBlockReason === "trial") && (
              <Button
                variant="link"
                size="sm"
                className="h-auto p-0 text-xs"
                onClick={() => navigate("/tenant/billing")}
              >
                Upgrade Plan
              </Button>
            )}
          </div>
        )}
        {parsed.isTenantError && (
          <Button
            variant="link"
            size="sm"
            className="mt-1 h-auto p-0 text-xs"
            onClick={() => navigate("/tenant/tenants")}
          >
            Go to tenant selection
          </Button>
        )}
        {parsed.isPermissionError && (
          <Button
            variant="link"
            size="sm"
            className="mt-1 h-auto p-0 text-xs"
            onClick={() => navigate("/app")}
          >
            Back to Dashboard
          </Button>
        )}
      </div>
    </div>
  );
}
