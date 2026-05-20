import { useTranslation } from "react-i18next";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";

export function Dashboard() {
  const { t } = useTranslation();

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900">{t("app.title")}</h1>
      <p className="mt-1 text-sm text-gray-500">Welcome to the Clean Architecture dashboard.</p>
      <div className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle>Todos</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-gray-500">Manage your tasks and todo items.</p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
