import { useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import { useAuth } from "@/hooks/useAuth";
import { useAcceptInvitation, useAcceptInvitationAuthenticated } from "@/api/users";
import { decodeJwt } from "@/lib/jwt";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Mail, CheckCircle } from "lucide-react";

export function AcceptInvitationPage() {
  const { token } = useParams<{ token: string }>();
  const navigate = useNavigate();
  const { isAuthenticated, token: authToken } = useAuth();
  const acceptInvitation = useAcceptInvitation();
  const acceptAuthenticated = useAcceptInvitationAuthenticated();

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(false);

  const payload = authToken ? decodeJwt(authToken) : null;
  const userEmail = (payload?.email as string | undefined) ?? null;

  if (!token) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Card className="w-full max-w-md text-center">
          <CardContent className="pt-6">
            <p className="text-sm text-red-500">Invalid invitation link. No token provided.</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (success) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Card className="w-full max-w-md text-center">
          <CardContent className="pt-6">
            <div className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-green-100">
              <CheckCircle className="h-6 w-6 text-green-600" />
            </div>
            <h3 className="text-lg font-semibold">Invitation Accepted</h3>
            <p className="mt-1 text-sm text-gray-500">
              {isAuthenticated
                ? "Your invitations and tenants have been updated."
                : "You have been signed in successfully."}
            </p>
            <Button className="mt-4" onClick={() => navigate("/app")}>
              Go to Dashboard
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (isAuthenticated && userEmail) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Card className="w-full max-w-md text-center">
          <CardHeader className="text-center">
            <div className="mx-auto mb-2 flex h-12 w-12 items-center justify-center rounded-full bg-indigo-100">
              <Mail className="h-6 w-6 text-indigo-600" />
            </div>
            <CardTitle>Accept Invitation</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-gray-600">
              You are signed in as <strong>{userEmail}</strong>.
            </p>
            <p className="mt-2 text-sm text-gray-500">
              Click below to accept this invitation and join the tenant.
            </p>
            {error && <p className="mt-2 text-sm text-red-500">{error}</p>}
            <Button
              className="mt-4 w-full"
              disabled={acceptAuthenticated.isPending}
              onClick={() =>
                acceptAuthenticated.mutate(token, {
                  onSuccess: () => setSuccess(true),
                  onError: (err) => setError(err.message),
                })
              }
            >
              {acceptAuthenticated.isPending ? "Accepting..." : "Accept Invitation"}
            </Button>
            <p className="mt-4 text-sm text-gray-400">
              Not you?{" "}
              <Link to="/auth/login" className="font-medium text-indigo-600 hover:text-indigo-500">
                Switch account
              </Link>
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen items-center justify-center">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mx-auto mb-2 flex h-12 w-12 items-center justify-center rounded-full bg-indigo-100">
            <Mail className="h-6 w-6 text-indigo-600" />
          </div>
          <CardTitle>Accept Invitation</CardTitle>
          <p className="text-sm text-gray-500">Create your account to join</p>
        </CardHeader>
        <CardContent>
          <form
            onSubmit={(e) => {
              e.preventDefault();
              setError("");
              acceptInvitation.mutate(
                { token, firstName, lastName, password },
                {
                  onSuccess: () => setSuccess(true),
                  onError: (err) => setError(err.message),
                },
              );
            }}
            className="space-y-4"
          >
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-2">
                <Label htmlFor="firstName">First Name</Label>
                <Input
                  id="firstName"
                  placeholder="John"
                  value={firstName}
                  onChange={(e) => setFirstName(e.target.value)}
                  required
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="lastName">Last Name</Label>
                <Input
                  id="lastName"
                  placeholder="Doe"
                  value={lastName}
                  onChange={(e) => setLastName(e.target.value)}
                  required
                />
              </div>
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
            {error && <p className="text-sm text-red-500">{error}</p>}
            <Button type="submit" className="w-full" disabled={acceptInvitation.isPending}>
              {acceptInvitation.isPending ? "Creating account..." : "Create Account & Accept"}
            </Button>
          </form>
          <p className="mt-4 text-center text-sm text-gray-500">
            Already have an account?{" "}
            <Link to="/auth/login" className="font-medium text-indigo-600 hover:text-indigo-500">
              Sign in
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
