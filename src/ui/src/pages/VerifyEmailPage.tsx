import { useEffect, useState } from "react";
import { useSearchParams, Link } from "react-router-dom";
import { useVerifyEmail } from "@/api/auth";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { CheckCircle, XCircle, ArrowLeft } from "lucide-react";

export function VerifyEmailPage() {
  const [searchParams] = useSearchParams();
  const userId = searchParams.get("userId") ?? "";
  const token = searchParams.get("token") ?? "";

  const [status, setStatus] = useState<"loading" | "success" | "error">("loading");
  const [errorMessage, setErrorMessage] = useState("");
  const verifyEmail = useVerifyEmail();

  useEffect(() => {
    if (!token || !userId) {
      setStatus("error");
      setErrorMessage("Invalid or missing verification link.");
      return;
    }

    verifyEmail.mutate(
      { userId, token },
      {
        onSuccess: () => setStatus("success"),
        onError: (err) => {
          setStatus("error");
          setErrorMessage(err.message);
        },
      },
    );
  }, [userId, token]);

  return (
    <div className="flex min-h-[80vh] items-center justify-center">
      <Card className="w-full max-w-md text-center">
        <CardContent className="pt-6">
          {status === "loading" && (
            <>
              <div className="mx-auto mb-3 h-12 w-12 animate-spin rounded-full border-4 border-indigo-200 border-t-indigo-600" />
              <h3 className="text-lg font-semibold">Verifying Email</h3>
              <p className="mt-1 text-sm text-gray-500">Please wait while we verify your email address...</p>
            </>
          )}

          {status === "success" && (
            <>
              <div className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-green-100">
                <CheckCircle className="h-6 w-6 text-green-600" />
              </div>
              <h3 className="text-lg font-semibold">Email Verified</h3>
              <p className="mt-1 text-sm text-gray-500">Your email has been verified successfully.</p>
              <Button className="mt-4" asChild>
                <Link to="/auth/login">
                  <ArrowLeft className="h-4 w-4 mr-1" />
                  Sign In
                </Link>
              </Button>
            </>
          )}

          {status === "error" && (
            <>
              <div className="mx-auto mb-3 flex h-12 w-12 items-center justify-center rounded-full bg-red-100">
                <XCircle className="h-6 w-6 text-red-600" />
              </div>
              <h3 className="text-lg font-semibold">Verification Failed</h3>
              <p className="mt-1 text-sm text-red-500">{errorMessage || "Unable to verify your email."}</p>
              <Button variant="link" className="mt-4" asChild>
                <Link to="/auth/login">
                  <ArrowLeft className="h-4 w-4 mr-1" />
                  Back to Sign In
                </Link>
              </Button>
            </>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
