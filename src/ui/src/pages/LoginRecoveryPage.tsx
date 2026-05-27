import { useState } from "react";
import { useLocation, useNavigate, Link } from "react-router-dom";
import { useLoginRecovery } from "@/api/auth";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ShieldCheck, ArrowLeft } from "lucide-react";

export function LoginRecoveryPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const state = location.state as { userId?: string } | null;

  const [recoveryCode, setRecoveryCode] = useState("");
  const [error, setError] = useState("");
  const loginRecovery = useLoginRecovery();

  if (!state?.userId) {
    navigate("/auth/login", { replace: true });
    return null;
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    loginRecovery.mutate(
      { userId: state.userId!, recoveryCode },
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
          <div className="mx-auto mb-2 flex h-12 w-12 items-center justify-center rounded-full bg-amber-100">
            <ShieldCheck className="h-6 w-6 text-amber-600" />
          </div>
          <CardTitle>Recovery Code</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-gray-500 mb-4">
            Enter one of your recovery codes to sign in. Each code can only be used once.
          </p>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="recoveryCode">Recovery Code</Label>
              <Input
                id="recoveryCode"
                type="text"
                placeholder="XXXX-XXXX-XXXX-XXXX"
                value={recoveryCode}
                onChange={(e) => setRecoveryCode(e.target.value)}
                required
              />
            </div>
            {error && <p className="text-sm text-red-500">{error}</p>}
            <Button type="submit" className="w-full" disabled={loginRecovery.isPending || recoveryCode.length < 8}>
              {loginRecovery.isPending ? "Verifying..." : "Sign In"}
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
