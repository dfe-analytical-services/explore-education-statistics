export default function logEnvVars(page: string) {
  console.log(
    `%c ENVIRONMENT VARIABLES FETCHED AT ${new Date().toLocaleString()} ON PAGE ${page}`,
    'color: #00ff00; font-weight: bold; font-size: 16px;',
  );

  console.log(
    '%c ENVIRONMENT VARIABLES',
    'color: #00ff00; font-weight: bold; font-size: 16px;',
  );

  console.table(
    {
    NEXT_PUBLIC_URL: process.env.NEXT_PUBLIC_URL,
    NEXT_PUBLIC_CONTENT_API_BASE_URL:
      process.env.NEXT_PUBLIC_CONTENT_API_BASE_URL,
    NEXT_PUBLIC_DATA_API_BASE_URL: process.env.NEXT_PUBLIC_DATA_API_BASE_URL,
    NEXT_PUBLIC_NOTIFICATION_API_BASE_URL:
      process.env.NEXT_PUBLIC_NOTIFICATION_API_BASE_URL,
  });
}
