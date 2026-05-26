export interface TodoCardData {
  id: string;
  description: string;
  dueDate: string | null;
  labels: string[];
  isCompleted: boolean;
  priority: number;
}

export interface TodoItem extends TodoCardData {
  userId: string;
  parentId: string | null;
  createdAt: string;
  completedAt: string | null;
}

export interface CreateTodoPayload {
  userId: string;
  parentId?: string | null;
  description: string;
  dueDate?: string | null;
  labels?: string[];
  priority?: number;
}

export interface PaginatedList<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface KanbanColumn<TColumn, TItem> {
  key: TColumn;
  label: string;
  order: number;
  items: TItem[];
}

export interface KanbanList<TColumn, TItem> {
  columns: KanbanColumn<TColumn, TItem>[];
}

export interface TreeList<T> {
  roots: T[];
}

export interface TodoTreeItem extends TodoCardData {
  userId: string;
  parentId: string | null;
  createdAt: string;
  children: TodoTreeItem[];
}

export interface TenantResponse {
  id: string;
  name: string;
  identifier: string;
  subscriptionPlanName: string | null;
  subscriptionStatus: number | null;
  maxUsersOverride: number | null;
  role?: string;
  billingPeriod?: string;
  subscriptionExpiresAt?: string | null;
  isActive: boolean;
}

export interface AvailablePlanResponse {
  planId: string;
  name: string;
  description: string | null;
  priceMonthly: number;
  priceYearly: number;
  trialDays: number;
  remainingQuota: number;
  features: string[];
  limits: Record<string, number>;
}

export interface CreateTenantPayload {
  name: string;
  identifier: string;
  subscriptionPlanId?: string;
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

export interface MyInvitationResponse {
  id: string;
  tenantName: string;
  roleName: string;
  token: string;
  createdAt: string;
  tokenExpiry: string;
}

export interface TenantAdminResponse {
  id: string;
  name: string;
  identifier: string;
  subscriptionPlanName: string | null;
  subscriptionStatus: number | null;
  maxUsersOverride: number | null;
  isActive: boolean;
  createdAt: string;
}

export interface LoginResponse {
  accessToken: string | null;
  refreshToken: string | null;
  requiresTwoFactor: boolean;
  twoFactorToken: string | null;
  userId: string | null;
  emailConfirmed: boolean;
}

export interface RegisterResponse {
  userId: string;
  verificationEmailSent: boolean;
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
  subscriptionPlanId: string;
  plan: string;
  feature: string;
  isEnabled: boolean;
}

export interface PlanLimitResponse {
  subscriptionPlanId: string;
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
  planId: string;
  plan: string;
  billingPeriod: string;
  amount: number;
}

export interface InitiatePaymentPayload {
  subscriptionPlanId: string;
  billingPeriod: string;
}

export interface UpdateTenantSubscriptionPayload {
  subscriptionPlanId: string;
  subscriptionStatus: number;
  maxUsersOverride: number | null;
}

export interface SubscriptionPlanListItem {
  id: string;
  name: string;
  description: string | null;
  priceMonthly: number;
  priceYearly: number;
  trialDays: number;
  sortOrder: number;
  isActive: boolean;
  createdAt: string;
  subscriptionCount: number;
}

export interface PlanFeatureInfo {
  feature: string;
  isEnabled: boolean;
}

export interface PlanLimitInfo {
  limit: string;
  value: number;
}

export interface SubscriptionPlanDetail {
  id: string;
  name: string;
  description: string | null;
  priceMonthly: number;
  priceYearly: number;
  trialDays: number;
  sortOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
  features: PlanFeatureInfo[];
  limits: PlanLimitInfo[];
}

export interface SubscriptionResponse {
  id: string;
  tenantId: string;
  tenantName: string;
  planName: string;
  status: string;
  billingPeriod: string;
  expiresAt: string | null;
  createdAt: string;
}

export interface TenantSubscriptionResponse {
  subscriptionId: string;
  tenantId: string;
  tenantName: string;
  planName: string;
  status: string;
  billingPeriod: string;
  expiresAt: string | null;
  createdAt: string;
  updatedAt: string | null;
  features: FeatureState[];
  limits: LimitState[];
}

export interface CreateSubscriptionPlanPayload {
  name: string;
  description: string | null;
  priceMonthly: number;
  priceYearly: number;
  trialDays: number;
  sortOrder: number;
  isActive: boolean;
}

export interface UpdateSubscriptionStatusPayload {
  newStatus: number;
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

export interface MyPermissionsResponse {
  permissions: string[];
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
