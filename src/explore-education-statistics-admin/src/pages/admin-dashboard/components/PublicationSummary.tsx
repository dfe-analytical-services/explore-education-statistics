import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import { legacyReleasesRoute, releaseCreateRoute } from '@admin/routes/routes';
import {
  publicationAssignMethodologyRoute,
  ThemeTopicPublicationParams,
} from '@admin/routes/themeTopicRoutes';
import { AdminDashboardPublication } from '@admin/services/dashboardService';
import releaseService, { Release } from '@admin/services/releaseService';
import ButtonGroup from '@common/components/ButtonGroup';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React, { useState } from 'react';
import { generatePath, useHistory } from 'react-router';
import CancelAmendmentModal from './CancelAmendmentModal';
import NonScheduledReleaseSummary from './NonScheduledReleaseSummary';

export interface Props {
  publication: AdminDashboardPublication;
  themeId: string;
  topicId: string;
  onChangePublication: () => void;
}

const PublicationSummary = ({
  publication,
  themeId,
  topicId,
  onChangePublication,
}: Props) => {
  const history = useHistory();

  const [amendReleaseId, setAmendReleaseId] = useState<string>();
  const [cancelAmendmentReleaseId, setCancelAmendmentReleaseId] = useState<
    string
  >();
  const noAmendmentInProgressFilter = (release: Release) =>
    !publication.releases.some(
      r => r.amendment && r.previousVersionId === release.id,
    );

  // BAU-404 - temporarily hide the Amend Release button completely until Release Versioning Phase 1 is complete
  const showAmendmentButton = () => true;

  return (
    <>
      <SummaryList smallKey>
        <SummaryListItem term="Methodology">
          <div className="govuk-!-margin-bottom-4">
            {publication.methodology ? (
              <Link to={`/methodologies/${publication.methodology.id}`}>
                {publication.methodology.title}
              </Link>
            ) : (
              <>
                {publication.externalMethodology?.url ? (
                  <>
                    {publication.externalMethodology.title} (
                    <a
                      href={publication.externalMethodology.url}
                      target="_blank"
                      rel="noopener noreferrer"
                    >
                      {publication.externalMethodology.url}
                    </a>
                    )
                  </>
                ) : (
                  'No methodology assigned'
                )}
              </>
            )}
          </div>

          <ButtonGroup className="govuk-!-margin-bottom-2">
            <ButtonLink
              variant="secondary"
              to={generatePath<ThemeTopicPublicationParams>(
                publicationAssignMethodologyRoute.path,
                {
                  themeId,
                  topicId,
                  publicationId: publication.id,
                },
              )}
            >
              {!publication.methodology &&
              (!publication.externalMethodology ||
                !publication.externalMethodology.url)
                ? 'Add'
                : 'Edit'}{' '}
              methodology
            </ButtonLink>
          </ButtonGroup>
        </SummaryListItem>
        <SummaryListItem term="Releases">
          <ul className="govuk-list">
            {publication.releases
              .filter(noAmendmentInProgressFilter)
              .map(release => (
                <li key={release.id}>
                  <NonScheduledReleaseSummary
                    onClickAmendRelease={
                      showAmendmentButton() ? setAmendReleaseId : undefined
                    }
                    onClickCancelAmendment={setCancelAmendmentReleaseId}
                    release={release}
                  />
                </li>
              ))}
            {publication.releases.length < 1 && <>No releases created</>}
          </ul>

          <ButtonGroup className="govuk-!-margin-bottom-2">
            {publication.permissions.canCreateReleases && (
              <ButtonLink
                to={generatePath(releaseCreateRoute.path, {
                  publicationId: publication.id,
                })}
                testId={`Create new release link for ${publication.title}`}
              >
                Create new release
              </ButtonLink>
            )}
            {publication.permissions.canUpdatePublication && (
              <ButtonLink
                to={generatePath(legacyReleasesRoute.path, {
                  publicationId: publication.id,
                })}
                variant="secondary"
                testId={`Legacy releases link for ${publication.title}`}
              >
                Manage legacy releases
              </ButtonLink>
            )}
          </ButtonGroup>
        </SummaryListItem>
      </SummaryList>

      {amendReleaseId && (
        <ModalConfirm
          title="Confirm you want to amend this live release"
          onConfirm={async () =>
            releaseService
              .createReleaseAmendment(amendReleaseId)
              .then(amendment =>
                history.push(
                  generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
                    publicationId: publication.id,
                    releaseId: amendment.id,
                  }),
                ),
              )
          }
          onExit={() => setAmendReleaseId(undefined)}
          onCancel={() => setAmendReleaseId(undefined)}
          mounted
        >
          <p>
            Please note, any changes made to this live release must be approved
            before updates can be published.
          </p>
        </ModalConfirm>
      )}

      {cancelAmendmentReleaseId && (
        <CancelAmendmentModal
          onConfirm={async () => {
            await releaseService.deleteRelease(cancelAmendmentReleaseId);
            setCancelAmendmentReleaseId(undefined);
            onChangePublication();
          }}
          onCancel={() => setCancelAmendmentReleaseId(undefined)}
        />
      )}
    </>
  );
};

export default PublicationSummary;
