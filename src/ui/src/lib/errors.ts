export interface AppError {
  message: string;
  isTenantError: boolean;
  isPermissionError: boolean;
  isNotFoundError: boolean;
}

export function parseErrorMessage(status: number, detail?: string): AppError {
  if (status === 403) {
    return {
      message: "You don't have permission to access this resource.",
      isPermissionError: true,
      isTenantError: false,
      isNotFoundError: false,
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
      };
    }
    return {
      message: detail ?? "The request was invalid.",
      isTenantError: false,
      isPermissionError: false,
      isNotFoundError: false,
    };
  }

  if (status === 404) {
    return {
      message: detail ?? "The requested resource was not found.",
      isNotFoundError: true,
      isTenantError: false,
      isPermissionError: false,
    };
  }

  if (status === 409) {
    return {
      message: detail ?? "A conflict occurred.",
      isTenantError: false,
      isPermissionError: false,
      isNotFoundError: false,
    };
  }

  if (status >= 500) {
    return {
      message: "An unexpected error occurred. Please try again later.",
      isTenantError: false,
      isPermissionError: false,
      isNotFoundError: false,
    };
  }

  return {
    message: detail ?? "An error occurred.",
    isTenantError: false,
    isPermissionError: false,
    isNotFoundError: false,
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
