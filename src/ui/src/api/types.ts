export interface TodoItem {
  id: string;
  userId: string;
  description: string;
  dueDate: string | null;
  labels: string[];
  isCompleted: boolean;
  createdAt: string;
  completedAt: string | null;
}

export interface CreateTodoPayload {
  userId: string;
  description: string;
  dueDate?: string | null;
  labels?: string[];
  priority?: number;
}

export interface TenantResponse {
  id: string;
  name: string;
  identifier: string;
  subscriptionPlan: number;
  subscriptionStatus: number;
  seatCount: number;
  role: string;
}

export interface CreateTenantPayload {
  name: string;
  identifier: string;
}

export interface RoleResponse {
  id: string;
  name: string;
  description: string | null;
  isSystem: boolean;
  permissions: string[];
}

export interface RoleFormPayload {
  name: string;
  description: string | null;
  permissions: string[];
}

export interface UserResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  emailConfirmed: boolean;
  createdAt: string;
  roleName: string;
  roleId: string | null;
}

export interface InvitationResponse {
  id: string;
  email: string;
  roleName: string;
  status: number;
  createdAt: string;
  tokenExpiry: string;
}

export interface TenantAdminResponse {
  id: string;
  name: string;
  identifier: string;
  subscriptionPlan: number;
  subscriptionStatus: number;
  seatCount: number;
  isActive: boolean;
  createdAt: string;
}

export interface LoginResponse {
  accessToken: string | null;
  refreshToken: string | null;
  requiresTwoFactor: boolean;
  twoFactorToken: string | null;
  userId: string | null;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
}

export interface CreateTenantResponse {
  tenantId: string;
  accessToken: string;
  refreshToken: string;
}

export interface AcceptInvitationResponse {
  userId: string;
  accessToken: string;
  refreshToken: string;
}

export interface EnableTwoFactorResponse {
  sharedKey: string;
  authenticatorUri: string;
}

export interface UserProfileResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  twoFactorEnabled: boolean;
  emailConfirmed: boolean;
}

export interface PlanFeatureResponse {
  plan: string;
  feature: string;
  isEnabled: boolean;
}

export interface PlanLimitResponse {
  plan: string;
  limit: string;
  value: number;
}

export interface FeatureState {
  feature: string;
  isEnabled: boolean;
}

export interface LimitState {
  limit: string;
  value: number;
}

export interface TenantFeaturesResponse {
  subscriptionPlan: string;
  subscriptionStatus: string;
  billingPeriod: string;
  subscriptionExpiresAt: string | null;
  features: FeatureState[];
  limits: LimitState[];
}

export interface PricingResponse {
  plan: string;
  billingPeriod: string;
  amount: number;
}

export interface InitiatePaymentResponse {
  paymentId: string;
  qr: string;
  md5: string;
}

export interface CheckPaymentResponse {
  isCompleted: boolean;
  transaction: TransactionCheckResult | null;
}

export interface TransactionCheckResult {
  hash: string;
  fromAccountId: string;
  toAccountId: string;
  currency: string;
  amount: number;
  description: string;
  createdDateMs: number;
  acknowledgedDateMs: number;
  externalRef: string;
}

export interface PaymentResponse {
  id: string;
  plan: string;
  billingPeriod: string;
  amount: number;
  currency: string;
  status: string;
  createdAt: string;
  completedAt: string | null;
}
