import { useState } from "react";
import { Link } from "react-router-dom";
import { useRequestPasswordReset } from "@/api/auth";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Mail, ArrowLeft } from "lucide-react";

export function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [sent, setSent] = useState(false);
  const [error, setError] = useState("");
  const requestReset = useRequestPasswordReset();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    requestReset.mutate(
      { email },
      {
        onSuccess: () => setSent(true),
        onError: (err) => setError(err.message),
      },
    );
  };

  if (sent) {
    return (
      <div className="flex min-h-[80vh] items-center justify-center">
        <Card className="w-full max-w-md text-center">
          <CardContent className="pt-6">
            <div className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-green-100">
              <Mail className="h-6 w-6 text-green-600" />
            </div>
            <h3 className="text-lg font-semibold">Check your email</h3>
            <p className="mt-1 text-sm text-gray-500">If an account exists, a reset link has been sent.</p>
            <Button variant="link" className="mt-4" asChild>
              <Link to="/auth/login">
                <ArrowLeft className="h-4 w-4 mr-1" />
                Back to Sign In
              </Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="flex min-h-[80vh] items-center justify-center">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mx-auto mb-2 flex h-12 w-12 items-center justify-center rounded-full bg-indigo-100">
            <Mail className="h-6 w-6 text-indigo-600" />
          </div>
          <CardTitle>Reset Password</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input id="email" type="email" placeholder="you@example.com" value={email} onChange={(e) => setEmail(e.target.value)} required />
            </div>
            {error && <p className="text-sm text-red-500">{error}</p>}
            <Button type="submit" className="w-full" disabled={requestReset.isPending}>
              {requestReset.isPending ? "Sending..." : "Send Reset Link"}
            </Button>
          </form>
          <p className="mt-4 text-center text-sm">
            <Link to="/auth/login" className="text-indigo-600 hover:text-indigo-500">
              Back to Sign In
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
