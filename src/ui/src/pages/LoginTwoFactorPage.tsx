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
  const state = location.state as { userId?: string } | null;

  const [code, setCode] = useState("");
  const [rememberDevice, setRememberDevice] = useState(false);
  const [error, setError] = useState("");
  const loginTwoFactor = useLoginTwoFactor();

  if (!state?.userId) {
    navigate("/auth/login", { replace: true });
    return null;
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    loginTwoFactor.mutate(
      { userId: state.userId!, code, rememberDevice },
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
            <div className="flex items-center gap-2">
              <input
                id="rememberDevice"
                type="checkbox"
                checked={rememberDevice}
                onChange={(e) => setRememberDevice(e.target.checked)}
                className="h-4 w-4 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
              />
              <Label htmlFor="rememberDevice" className="text-sm text-gray-600 cursor-pointer">
                Remember this device for 30 days
              </Label>
            </div>
            {error && <p className="text-sm text-red-500">{error}</p>}
            <Button type="submit" className="w-full" disabled={loginTwoFactor.isPending || code.length < 6}>
              {loginTwoFactor.isPending ? "Verifying..." : "Verify Code"}
            </Button>
          </form>
          <div className="mt-4 space-y-2 text-center text-sm">
            <Link to="/auth/login" className="text-indigo-600 hover:text-indigo-500">
              <ArrowLeft className="h-4 w-4 inline mr-1" />
              Back to Sign In
            </Link>
            <div>
              <Link
                to="/auth/login-recovery"
                state={{ userId: state.userId }}
                className="text-gray-500 hover:text-gray-700"
              >
                Lost your authenticator? Use a recovery code
              </Link>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
