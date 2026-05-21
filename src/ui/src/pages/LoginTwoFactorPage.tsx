import { useState } from "react";
import { useLocation, useNavigate, Link } from "react-router-dom";
import { useLoginTwoFactor } from "@/api/auth";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ShieldCheck, ArrowLeft } from "lucide-react";

export function LoginTwoFactorPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const state = location.state as { userId?: string; twoFactorToken?: string } | null;

  const [code, setCode] = useState("");
  const [error, setError] = useState("");
  const loginTwoFactor = useLoginTwoFactor();

  if (!state?.userId || !state?.twoFactorToken) {
    navigate("/auth/login", { replace: true });
    return null;
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    loginTwoFactor.mutate(
      { userId: state.userId!, twoFactorToken: state.twoFactorToken!, code },
      {
        onSuccess: () => navigate("/app", { replace: true }),
        onError: (err) => setError(err.message),
      },
    );
  };

  return (
    <div className="flex min-h-[80vh] items-center justify-center">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mx-auto mb-2 flex h-12 w-12 items-center justify-center rounded-full bg-indigo-100">
            <ShieldCheck className="h-6 w-6 text-indigo-600" />
          </div>
          <CardTitle>Two-Factor Authentication</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="code">Authenticator Code</Label>
              <Input
                id="code"
                type="text"
                inputMode="numeric"
                autoComplete="one-time-code"
                placeholder="000000"
                value={code}
                onChange={(e) => setCode(e.target.value)}
                required
                maxLength={6}
              />
            </div>
            {error && <p className="text-sm text-red-500">{error}</p>}
            <Button type="submit" className="w-full" disabled={loginTwoFactor.isPending || code.length < 6}>
              {loginTwoFactor.isPending ? "Verifying..." : "Verify Code"}
            </Button>
          </form>
          <p className="mt-4 text-center text-sm">
            <Link to="/auth/login" className="text-indigo-600 hover:text-indigo-500">
              <ArrowLeft className="h-4 w-4 inline mr-1" />
              Back to Sign In
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
