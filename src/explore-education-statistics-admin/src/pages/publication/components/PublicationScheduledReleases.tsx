import Link from '@admin/components/Link';
import ReleaseServiceStatus from '@admin/components/ReleaseServiceStatus';
import {
  ScheduledStagesGuidanceModal,
  ScheduledStatusGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import InfoIcon from '@common/components/InfoIcon';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import React from 'react';
import { generatePath } from 'react-router';

const PublicationScheduledReleases = () => {
  const { scheduledReleases } = usePublicationContext();

  const [
    showScheduledStatusGuidance,
    toggleScheduledStatusGuidance,
  ] = useToggle(false);
  const [
    showScheduledStagesGuidance,
    toggleScheduledStagesGuidance,
  ] = useToggle(false);

  return (
    <>
      <table
        className="govuk-!-margin-bottom-9"
        data-testid="publication-scheduled-releases"
      >
        <caption className="govuk-table__caption--m">
          Scheduled releases
        </caption>
        <thead>
          <tr>
            <th className="govuk-!-width-one-quarter">Release period</th>
            <th>
              State{' '}
              <ButtonText onClick={toggleScheduledStatusGuidance.on}>
                <InfoIcon description="Guidance on scheduled states" />
              </ButtonText>
            </th>
            <th className="govuk-!-width-one-quarter">
              Stages checklist{' '}
              <ButtonText onClick={toggleScheduledStagesGuidance.on}>
                <InfoIcon description="Guidance on publication stages" />
              </ButtonText>
            </th>
            <th>Publish date</th>
            <th className="dfe-align--right">Actions</th>
          </tr>
        </thead>
        <tbody>
          {scheduledReleases.map(release => (
            <tr key={release.id}>
              <td className="govuk-!-width-one-quarter">{release.title}</td>
              <td>
                <ReleaseServiceStatus
                  exclude="details"
                  releaseId={release.id}
                />
              </td>
              <td>
                {' '}
                <ReleaseServiceStatus
                  exclude="status"
                  releaseId={release.id}
                  newAdminStyle
                />
              </td>
              <td>
                <FormattedDate>{release.publishScheduled || ''}</FormattedDate>
              </td>
              <td className="dfe-align--right">
                <Link
                  to={generatePath<ReleaseRouteParams>(
                    releaseSummaryRoute.path,
                    {
                      publicationId: release.publicationId,
                      releaseId: release.id,
                    },
                  )}
                >
                  {release.permissions.canUpdateRelease ? 'Edit' : 'View'}
                  <VisuallyHidden> {release.title}</VisuallyHidden>
                </Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <ScheduledStagesGuidanceModal
        open={showScheduledStagesGuidance}
        onClose={toggleScheduledStagesGuidance.off}
      />
      <ScheduledStatusGuidanceModal
        open={showScheduledStatusGuidance}
        onClose={toggleScheduledStatusGuidance.off}
      />
    </>
  );
};

export default PublicationScheduledReleases;
