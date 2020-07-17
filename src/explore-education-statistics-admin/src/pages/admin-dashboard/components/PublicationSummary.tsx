import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import ThemeAndTopicContext from '@admin/components/ThemeAndTopicContext';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import { releaseCreateRoute } from '@admin/routes/routes';
import { AdminDashboardPublication } from '@admin/services/dashboardService';
import releaseService, { Release } from '@admin/services/releaseService';
import ButtonGroup from '@common/components/ButtonGroup';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React, { useContext, useState } from 'react';
import { generatePath, RouteComponentProps, withRouter } from 'react-router';
import CancelAmendmentModal from './CancelAmendmentModal';
import NonScheduledReleaseSummary from './NonScheduledReleaseSummary';

export interface Props {
  publication: AdminDashboardPublication;
  onChangePublication: () => void;
}

const PublicationSummary = ({
  publication,
  onChangePublication,
  history,
}: Props & RouteComponentProps) => {
  const { selectedThemeAndTopic } = useContext(ThemeAndTopicContext);
  const [amendReleaseId, setAmendReleaseId] = useState<string>();
  const [cancelAmendmentReleaseId, setCancelAmendmentReleaseId] = useState<
    string
  >();
  const noAmendmentInProgressFilter = (release: Release) =>
    publication &&
    !publication.releases.some(
      r => r.amendment && r.previousVersionId === release.id,
    );

  // BAU-404 - temporarily hide the Amend Release button completely until Release Versioning Phase 1 is complete
  const showAmendmentButton = () => true;

  return (
    <>
      <SummaryList>
        <SummaryListItem term="Methodology" smallKey>
          <p>
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
          </p>

          <ButtonGroup className="govuk-!-margin-bottom-2">
            <ButtonLink
              to={`/theme/${selectedThemeAndTopic.theme.id}/topic/${selectedThemeAndTopic.topic.id}/publication/${publication.id}/assign-methodology`}
              variant="secondary"
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
        <SummaryListItem term="Releases" smallKey>
          <ul className="govuk-list dfe-admin">
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

export default withRouter(PublicationSummary);
