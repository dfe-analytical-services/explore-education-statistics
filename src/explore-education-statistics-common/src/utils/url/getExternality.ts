export type Externality =
  | 'external'
  | 'external-admin'
  | 'external-trusted'
  | 'internal';

const PUBLIC_HOSTNAME = 'explore-education-statistics.service.gov.uk';
const ADMIN_HOSTNAME = 'admin.explore-education-statistics.service.gov.uk';

export default function getExternality(url: string): Externality {
  if ((url.startsWith('/') || url.startsWith('?')) && !url.includes('://')) {
    // Assume internal link if it's a relative URL
    return 'internal';
  }

  const { host } = new URL(url);

  switch (host.toLowerCase()) {
    case PUBLIC_HOSTNAME:
      return 'internal';
    case ADMIN_HOSTNAME:
      return 'external-admin';
    case 'www.gov.uk':
      return 'external-trusted';
    default:
      return 'external';
  }
}
