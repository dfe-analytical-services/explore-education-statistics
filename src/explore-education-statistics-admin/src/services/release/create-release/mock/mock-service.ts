import {
  AdminDashboardRelease,
  ReleaseApprovalStatus,
} from '@admin/services/dashboard/types';
import {CreateReleaseRequest} from '@admin/services/release/create-release/types';
import {ReleaseSetupDetails} from "@admin/services/release/types";
import {
  generateRandomIntegerString,
  getCaptureGroups,
} from '@admin/services/util/mock/mock-service';
import MockAdapter from 'axios-mock-adapter';
import { format } from 'date-fns';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ '@admin/services/release/edit-release/setup/mock/mock-data'
  )).default;

  const mockDashboardData = (await import(
    /* webpackChunkName: "mock-dashboard-data" */ '@admin/services/dashboard/mock/mock-data'
  )).default;

  const mockReferenceData = (await import(
    /* webpackChunkName: "mock-dashboard-data" */ '@admin/pages/DummyReferenceData'
  )).default;

  const createReleaseUrl = /\/publication\/(.*)\/releases/;

  mock.onPost(createReleaseUrl).reply(config => {
    const publicationId = getCaptureGroups(createReleaseUrl, config.url)[0];

    const createRequest = JSON.parse(config.data) as CreateReleaseRequest;

    const allPublications = Object.values(
      mockDashboardData.dashboardPublicationsByTopicId,
    ).flat();
    const matchingPublication = allPublications.find(
      publication => publication.id === publicationId,
    );

    if (matchingPublication) {
      const newReleaseDetails: ReleaseSetupDetails = {
        id: generateRandomIntegerString(),
        leadStatisticianName: 'Bob',
        publicationTitle: matchingPublication.title,
        releaseType: createRequest.releaseType,
        timePeriodCoverageStartYear: createRequest.timePeriodCoverageStartYear,
        timePeriodCoverageCode: createRequest.timePeriodCoverageCode,
        scheduledPublishDate: createRequest.scheduledPublishDate,
        nextReleaseExpectedDate: createRequest.nextReleaseExpectedDate,
      };

      const startYear = createRequest.timePeriodCoverageStartYear;

      const newRelease: AdminDashboardRelease = {
        id: newReleaseDetails.id,
        contact: matchingPublication.contact,
        nextReleaseExpectedDate: createRequest.nextReleaseExpectedDate,
        lastEditedUser: {
          id: '1234',
          name: 'John Smith',
        },
        lastEditedDateTime: format(new Date(), 'yyyy-MM-dd HH:mm:ss'),
        latestRelease: true,
        timePeriodCoverage: mockReferenceData.findTimePeriodCoverageOption(
          createRequest.timePeriodCoverageCode,
        ),
        live: false,
        publishScheduled: createRequest.scheduledPublishDate,
        releaseName: `${startYear} - ${startYear}`,
        status: ReleaseApprovalStatus.None,
      };

      matchingPublication.releases = matchingPublication.releases
        ? matchingPublication.releases.concat(newRelease)
        : [newRelease];

      mockData.setupByReleaseId[newReleaseDetails.id] = newReleaseDetails;

      return [200, newReleaseDetails];
    }

    return [404];
  });
};
