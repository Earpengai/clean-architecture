export interface AppError {
  message: string;
  isTenantError: boolean;
  isPermissionError: boolean;
  isNotFoundError: boolean;
  isTenantBlocked: boolean;
  tenantBlockReason: "disabled" | "expired" | "trial" | null;
}

function makeBlockResult(reason: "disabled" | "expired" | "trial", message: string): AppError {
  return {
    message,
    isTenantError: false,
    isPermissionError: false,
    isNotFoundError: false,
    isTenantBlocked: true,
    tenantBlockReason: reason,
  };
}

function detectTenantBlock(
  status: number,
  detail: string,
  problemType?: string
): AppError | null {
  if (status !== 403) return null;

  if (problemType) {
    if (problemType.includes("tenant-disabled"))
      return makeBlockResult("disabled", detail);
    if (problemType.includes("tenant-expired"))
      return makeBlockResult("expired", detail);
    if (problemType.includes("trial-expired"))
      return makeBlockResult("trial", detail);
  }

  const lower = detail.toLowerCase();
  if (lower.includes("tenant disabled") || lower.includes("tenants.disabled"))
    return makeBlockResult("disabled", detail);
  if (lower.includes("subscription expired") || lower.includes("tenants.subscriptionexpired"))
    return makeBlockResult("expired", detail);
  if (lower.includes("trial expired") || lower.includes("tenants.trialexpired"))
    return makeBlockResult("trial", detail);

  return null;
}

export function parseErrorMessage(
  status: number,
  detail?: string,
  problemType?: string
): AppError {
  const tenantBlocked = detectTenantBlock(status, detail ?? "", problemType);
  if (tenantBlocked) return tenantBlocked;

  if (status === 403) {
    return {
      message: "You don't have permission to access this resource.",
      isPermissionError: true,
      isTenantError: false,
      isNotFoundError: false,
      isTenantBlocked: false,
      tenantBlockReason: null,
    };
  }

  if (status === 400) {
    const lower = (detail ?? "").toLowerCase();
    if (lower.includes("x-tenant-id") || lower.includes("tenant")) {
      return {
        message: "Please select a tenant from the dropdown above.",
        isTenantError: true,
        isPermissionError: false,
        isNotFoundError: false,
        isTenantBlocked: false,
        tenantBlockReason: null,
      };
    }
    return {
      message: detail ?? "The request was invalid.",
      isTenantError: false,
      isPermissionError: false,
      isNotFoundError: false,
      isTenantBlocked: false,
      tenantBlockReason: null,
    };
  }

  if (status === 404) {
    return {
      message: detail ?? "The requested resource was not found.",
      isNotFoundError: true,
      isTenantError: false,
      isPermissionError: false,
      isTenantBlocked: false,
      tenantBlockReason: null,
    };
  }

  if (status === 409) {
    return {
      message: detail ?? "A conflict occurred.",
      isTenantError: false,
      isPermissionError: false,
      isNotFoundError: false,
      isTenantBlocked: false,
      tenantBlockReason: null,
    };
  }

  if (status >= 500) {
    return {
      message: "An unexpected error occurred. Please try again later.",
      isTenantError: false,
      isPermissionError: false,
      isNotFoundError: false,
      isTenantBlocked: false,
      tenantBlockReason: null,
    };
  }

  return {
    message: detail ?? "An error occurred.",
    isTenantError: false,
    isPermissionError: false,
    isNotFoundError: false,
    isTenantBlocked: false,
    tenantBlockReason: null,
  };
}

export function extractErrorDetail(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }
  if (typeof error === "string") {
    return error;
  }
  return "An unknown error occurred.";
}
