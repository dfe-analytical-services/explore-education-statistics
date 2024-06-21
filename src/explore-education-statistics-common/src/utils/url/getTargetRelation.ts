type TargetRelation = 'internal-public' | 'internal-admin' | 'external';

const DEFAULT_PUBLIC_HOSTNAME = 'explore-education-statistics.service.gov.uk';
const DEFAULT_ADMIN_HOSTNAME =
  'admin.explore-education-statistics.service.gov.uk';

export default function getTargetRelation(
  url: string,
  publicUrl?: string,
  adminUrl?: string,
): TargetRelation {
  const { host } = new URL(url);
  const publicHost = publicUrl
    ? new URL(publicUrl).host
    : DEFAULT_PUBLIC_HOSTNAME;
  const adminHost = adminUrl ? new URL(adminUrl).host : DEFAULT_ADMIN_HOSTNAME;

  if (host === adminHost) {
    return 'internal-admin';
  }

  if (host === publicHost) {
    return 'internal-public';
  }

  return 'external';
}
