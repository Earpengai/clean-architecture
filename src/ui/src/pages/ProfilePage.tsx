import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  useUpdateProfile,
  useChangePassword,
  useChangeEmail,
  useRequestEmailVerification,
} from "@/api/auth";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { User, Lock, Mail, ShieldCheck, ArrowLeft } from "lucide-react";

function useFormState() {
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const clear = () => { setError(""); setSuccess(""); };
  return { error, success, clear, setError, setSuccess };
}

export function ProfilePage() {
  const navigate = useNavigate();

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [newEmail, setNewEmail] = useState("");

  const updateProfile = useUpdateProfile();
  const changePassword = useChangePassword();
  const changeEmail = useChangeEmail();
  const requestVerification = useRequestEmailVerification();

  const profileForm = useFormState();
  const passwordForm = useFormState();
  const emailForm = useFormState();
  const [verifySent, setVerifySent] = useState(false);

  const handleUpdateProfile = (e: React.FormEvent) => {
    e.preventDefault();
    profileForm.clear();
    updateProfile.mutateAsync({ firstName, lastName })
      .then(() => profileForm.setSuccess("Saved successfully."))
      .catch((err: Error) => profileForm.setError(err.message));
  };

  const handleChangePassword = (e: React.FormEvent) => {
    e.preventDefault();
    passwordForm.clear();
    changePassword.mutateAsync({ currentPassword, newPassword })
      .then(() => passwordForm.setSuccess("Password changed successfully."))
      .catch((err: Error) => passwordForm.setError(err.message));
  };

  const handleChangeEmail = (e: React.FormEvent) => {
    e.preventDefault();
    emailForm.clear();
    changeEmail.mutateAsync({ newEmail })
      .then(() => emailForm.setSuccess("Email change requested. Check your inbox."))
      .catch((err: Error) => emailForm.setError(err.message));
  };

  const handleRequestVerification = () => {
    requestVerification.mutate(undefined, {
      onSuccess: () => setVerifySent(true),
    });
  };

  return (
    <div>
      <div className="flex items-center gap-3 mb-6">
        <Button variant="ghost" size="icon" onClick={() => navigate("/")}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <h1 className="text-2xl font-bold text-gray-900">Settings</h1>
      </div>

      <div className="space-y-6 max-w-lg">
        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <User className="h-5 w-5 text-indigo-600" />
              <CardTitle>Profile</CardTitle>
            </div>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleUpdateProfile} className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-2">
                  <Label htmlFor="firstName">First Name</Label>
                  <Input id="firstName" value={firstName} onChange={(e) => setFirstName(e.target.value)} placeholder="John" />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="lastName">Last Name</Label>
                  <Input id="lastName" value={lastName} onChange={(e) => setLastName(e.target.value)} placeholder="Doe" />
                </div>
              </div>
              {profileForm.error && <p className="text-sm text-red-500">{profileForm.error}</p>}
              {profileForm.success && <p className="text-sm text-green-600">{profileForm.success}</p>}
              <Button type="submit" disabled={updateProfile.isPending}>Update Profile</Button>
            </form>
          </CardContent>
        </Card>

        <Separator />

        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <Lock className="h-5 w-5 text-indigo-600" />
              <CardTitle>Password</CardTitle>
            </div>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleChangePassword} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="currentPassword">Current Password</Label>
                <Input id="currentPassword" type="password" value={currentPassword} onChange={(e) => setCurrentPassword(e.target.value)} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="newPassword">New Password</Label>
                <Input id="newPassword" type="password" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} />
              </div>
              {passwordForm.error && <p className="text-sm text-red-500">{passwordForm.error}</p>}
              {passwordForm.success && <p className="text-sm text-green-600">{passwordForm.success}</p>}
              <Button type="submit" disabled={changePassword.isPending}>Change Password</Button>
            </form>
          </CardContent>
        </Card>

        <Separator />

        <Card>
          <CardHeader>
            <div className="flex items-center gap-2">
              <Mail className="h-5 w-5 text-indigo-600" />
              <CardTitle>Email</CardTitle>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            <form onSubmit={handleChangeEmail} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="newEmail">New Email</Label>
                <Input id="newEmail" type="email" value={newEmail} onChange={(e) => setNewEmail(e.target.value)} placeholder="new@example.com" />
              </div>
              {emailForm.error && <p className="text-sm text-red-500">{emailForm.error}</p>}
              {emailForm.success && <p className="text-sm text-green-600">{emailForm.success}</p>}
              <Button type="submit" disabled={changeEmail.isPending}>Change Email</Button>
            </form>
            <Separator />
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <ShieldCheck className="h-4 w-4 text-gray-400" />
                <span className="text-sm text-gray-600">Email Verification</span>
              </div>
              {verifySent ? (
                <span className="text-sm text-green-600">Verification email sent.</span>
              ) : (
                <Button variant="outline" size="sm" onClick={handleRequestVerification} disabled={requestVerification.isPending}>
                  Request Verification
                </Button>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
