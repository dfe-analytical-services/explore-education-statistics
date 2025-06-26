export default function isPatchVersion(version?: string): boolean {
  if (!version) return false;
  const parts = version.split('.');
  return parts.length === 3 && parseInt(parts[2], 10) > 0;
}
