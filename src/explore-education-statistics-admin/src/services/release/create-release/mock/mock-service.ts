import { dateToDayMonthYear } from '@admin/services/common/types';
import {
  AdminDashboardRelease,
  ReleaseApprovalStatus,
} from '@admin/services/dashboard/types';
import { CreateReleaseRequest } from '@admin/services/release/create-release/types';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import {
  generateRandomIntegerString,
  getCaptureGroups,
} from '@admin/services/util/mock/mock-service';
import MockAdapter from 'axios-mock-adapter';
import { format } from 'date-fns';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ '@admin/services/release/edit-release/summary/mock/mock-data'
  )).default;

  const mockDashboardData = (await import(
    /* webpackChunkName: "mock-dashboard-data" */ '@admin/services/dashboard/mock/mock-data'
  )).default;

  const mockReferenceData = (await import(
    /* webpackChunkName: "mock-dashboard-data" */ '@admin/pages/DummyReferenceData'
  )).default;

  const mockCommonData = (await import(
    /* webpackChunkName: "mock-dashboard-data" */ '@admin/services/common/mock/mock-data'
  )).default;

  const getReleasesUrl = /\/publications\/(.*)\/releases/;
  const createReleaseUrl = /\/publications\/(.*)\/releases/;

  // getTemplateRelease()
  mock.onGet(getReleasesUrl).reply(config => {
    const publicationId = getCaptureGroups(createReleaseUrl, config.url)[0];

    const allPublications = Object.values(
      mockDashboardData.dashboardPublicationsByTopicId,
    ).flat();

    const matchingPublication = allPublications.find(
      publication => publication.id === publicationId,
    );

    if (matchingPublication) {
      const latestRelease = matchingPublication.releases.map(release => ({
        id: release.id,
        title: release.releaseName,
        latestRelease: release.latestRelease ? 'true' : 'false',
      }));

      return [200, latestRelease];
    }

    return [200, []];
  });

  // createRelease()
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
      const newReleaseDetails: ReleaseSummaryDetails = {
        id: generateRandomIntegerString(),
        leadStatisticianName: 'Bob',
        publicationTitle: matchingPublication.title,
        releaseType:
          mockCommonData
            .getReleaseTypes()
            .find(type => type.id === createRequest.releaseTypeId) ||
          mockCommonData.getReleaseTypes()[0],
        timePeriodCoverageStartYear: createRequest.releaseName,
        timePeriodCoverageCode: createRequest.timePeriodCoverage.value,
        scheduledPublishDate: dateToDayMonthYear(
          createRequest.publishScheduled,
        ),
        nextReleaseExpectedDate: createRequest.nextReleaseExpected,
      };

      const startYear = createRequest.releaseName;

      const newRelease: AdminDashboardRelease = {
        id: newReleaseDetails.id,
        contact: matchingPublication.contact,
        nextReleaseExpectedDate: createRequest.nextReleaseExpected,
        lastEditedUser: {
          id: '1234',
          name: 'John Smith',
        },
        lastEditedDateTime: format(new Date(), 'yyyy-MM-dd HH:mm:ss'),
        latestRelease: true,
        timePeriodCoverage: mockReferenceData.findTimePeriodCoverageOption(
          createRequest.timePeriodCoverage.value,
        ),
        live: false,
        publishScheduled: dateToDayMonthYear(createRequest.publishScheduled),
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
