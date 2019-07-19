import {
  AdminDashboardPublication,
  AdminDashboardRelease,
} from '@admin/services/dashboard/types';
import { Polyfilla } from '@admin/services/util/polyfilla';

const publicationPollyfilla: Polyfilla<AdminDashboardPublication> = (
  publication: AdminDashboardPublication,
) => ({
  ...publication,
  releases: publication.releases.map(release => {
    const patchedRelease: AdminDashboardRelease = {
      ...release,
      lastEditedUser: {
        id: 'TODO',
        name: 'TODO editor user',
      },
      lastEditedDateTime: '1971-01-01 00:00',
      nextReleaseExpectedDate: {
        day: 2,
        month: 2,
        year: 1971,
      },
      contact: publication.contact,
    };
    return patchedRelease;
  }),
});

export default publicationPollyfilla;
