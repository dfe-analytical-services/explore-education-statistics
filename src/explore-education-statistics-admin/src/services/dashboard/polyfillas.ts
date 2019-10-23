import {
  AdminDashboardPublication,
  AdminDashboardRelease,
} from '@admin/services/dashboard/types';
import { Polyfilla } from '@admin/services/util/polyfilla';

export const releasePolyfilla: Polyfilla<AdminDashboardRelease> = (
  release: AdminDashboardRelease,
) => ({
  ...release,
  lastEditedUser: {
    id: 'TODO',
    name: 'TODO editor user',
  },
  lastEditedDateTime: '1971-01-01T00:00',
});

export const publicationPolyfilla: Polyfilla<AdminDashboardPublication> = (
  publication: AdminDashboardPublication,
) => ({
  ...publication,
  releases: publication.releases.map(releasePolyfilla),
});

export default {};
