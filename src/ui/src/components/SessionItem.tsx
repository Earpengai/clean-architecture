import { Monitor, Smartphone, Laptop, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import type { UserSessionResponse } from "@/api/types";

function formatRelativeTime(dateStr: string): string {
  const date = new Date(dateStr);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);

  if (diffMins < 1) {
    return "Just now";
  }

  if (diffMins < 60) {
    return `${diffMins}m ago`;
  }

  const diffHours = Math.floor(diffMins / 60);

  if (diffHours < 24) {
    return `${diffHours}h ago`;
  }

  const diffDays = Math.floor(diffHours / 24);

  if (diffDays < 30) {
    return `${diffDays}d ago`;
  }

  return date.toLocaleDateString();
}

function getOsIcon(os: string) {
  switch (os.toLowerCase()) {
    case "android":
    case "ios":
      return <Smartphone className="h-4 w-4 text-gray-500" />;
    case "windows":
    case "macos":
    case "linux":
      return <Monitor className="h-4 w-4 text-gray-500" />;
    default:
      return <Laptop className="h-4 w-4 text-gray-500" />;
  }
}

interface SessionItemProps {
  session: UserSessionResponse;
  onRevoke: (id: string) => void;
  isRevoking: boolean;
}

export function SessionItem({ session, onRevoke, isRevoking }: SessionItemProps) {
  const label = `${session.browser} on ${session.operatingSystem}`;

  return (
    <div className="flex items-center justify-between py-3 border-b border-gray-100 last:border-0">
      <div className="flex items-center gap-3">
        {getOsIcon(session.operatingSystem)}
        <div>
          <p className="text-sm font-medium text-gray-900">{label}</p>
          <p className="text-xs text-gray-500">
            {session.ipAddress} &middot; {formatRelativeTime(session.lastActivityAt)}
          </p>
        </div>
      </div>
      <Button
        variant="ghost"
        size="icon"
        className="h-8 w-8 text-gray-400 hover:text-red-500"
        onClick={() => onRevoke(session.id)}
        disabled={isRevoking}
        title="Revoke session"
      >
        <X className="h-4 w-4" />
      </Button>
    </div>
  );
}
