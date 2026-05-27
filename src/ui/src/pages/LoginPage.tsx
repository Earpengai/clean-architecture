import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "@/hooks/useAuth";
import { useLogin, useRequestEmailVerification } from "@/api/auth";
import type { LoginResponse } from "@/api/types";
import type { ApiError } from "@/api/client";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { LogIn, Mail } from "lucide-react";

export function LoginPage() {
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();
  const login = useLogin();
  const requestVerification = useRequestEmailVerification();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [errorType, setErrorType] = useState<string | undefined>();
  const [verifyResent, setVerifyResent] = useState(false);

  if (isAuthenticated) {
    navigate("/app", { replace: true });
    return null;
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setErrorType(undefined);

    login.mutate(
      { email, password },
      {
        onSuccess: (data: LoginResponse) => {
          if (data.requiresTwoFactor) {
            navigate("/auth/login-2fa", {
              state: { userId: data.userId! },
              replace: true,
            });
          } else {
            navigate("/app", { replace: true });
          }
        },
        onError: (err) => {
          const apiError = err as ApiError;
          setError(apiError.message);
          setErrorType(apiError.problemType);
        },
      },
    );
  };

  return (
    <div className="flex min-h-[80vh] items-center justify-center">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mx-auto mb-2 flex h-12 w-12 items-center justify-center rounded-full bg-indigo-100">
            <LogIn className="h-6 w-6 text-indigo-600" />
          </div>
          <CardTitle>Sign In</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                placeholder="you@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Password</Label>
              <Input
                id="password"
                type="password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>
            {error && (
              <div className="space-y-2">
                <p className="text-sm text-red-500">{error}</p>
                {errorType === "Users.EmailNotVerified" && (
                  <div className="flex items-center gap-2">
                    {verifyResent ? (
                      <span className="text-xs text-green-600">Verification email sent.</span>
                    ) : (
                      <Button
                        variant="outline"
                        size="sm"
                        className="text-xs h-8"
                        onClick={() => {
                          requestVerification.mutate(undefined, {
                            onSuccess: () => setVerifyResent(true),
                          });
                        }}
                        disabled={requestVerification.isPending}
                      >
                        <Mail className="h-3 w-3 mr-1" />
                        {requestVerification.isPending ? "Sending..." : "Resend Verification Email"}
                      </Button>
                    )}
                  </div>
                )}
              </div>
            )}
            <Button type="submit" className="w-full" disabled={login.isPending}>
              {login.isPending ? "Signing in..." : "Sign In"}
            </Button>
          </form>
          <p className="mt-4 text-center text-sm text-gray-500">
            Don&apos;t have an account?{" "}
            <Link to="/auth/register" className="font-medium text-indigo-600 hover:text-indigo-500">
              Sign up
            </Link>
          </p>
          <p className="mt-1 text-center text-sm">
            <Link to="/auth/forgot-password" className="text-gray-500 hover:text-gray-700">
              Forgot password?
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
