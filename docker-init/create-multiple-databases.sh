set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE "TaskDb";
    CREATE DATABASE "NotificationDb";
    CREATE DATABASE "AnalyticsDb";
EOSQL

echo "Multiple databases created successfully"