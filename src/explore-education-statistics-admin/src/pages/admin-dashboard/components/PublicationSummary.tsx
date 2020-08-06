import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import {
  legacyReleasesRoute,
  publicationEditRoute,
  PublicationRouteParams,
  releaseCreateRoute,
} from '@admin/routes/routes';
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
  onChangePublication: () => void;
}

const PublicationSummary = ({ publication, onChangePublication }: Props) => {
  const history = useHistory();

  const [amendReleaseId, setAmendReleaseId] = useState<string>();
  const [cancelAmendmentReleaseId, setCancelAmendmentReleaseId] = useState<
    string
  >();

  const {
    contact,
    externalMethodology,
    methodology,
    permissions,
    releases,
    id,
    title,
  } = publication;

  const noAmendmentInProgressFilter = (release: Release) =>
    !releases.some(r => r.amendment && r.previousVersionId === release.id);

  // BAU-404 - temporarily hide the Amend Release button completely until Release Versioning Phase 1 is complete
  const showAmendmentButton = () => true;

  return (
    <>
      <SummaryList>
        <SummaryListItem
          term="Team"
          actions={
            permissions.canUpdatePublication && (
              <Link
                data-testid={`Edit publication link for ${publication.title}`}
                unvisited
                to={generatePath<PublicationRouteParams>(
                  publicationEditRoute.path,
                  {
                    publicationId: publication.id,
                  },
                )}
              >
                Change <span className="govuk-visually-hidden">team</span>
              </Link>
            )
          }
        >
          <p>{contact?.teamName || 'No team name'}</p>

          {contact?.teamEmail && (
            <p>
              <a href={`mailto:${contact.teamEmail}`}>{contact.teamEmail}</a>
            </p>
          )}
        </SummaryListItem>
        <SummaryListItem
          term="Contact"
          actions={
            permissions.canUpdatePublication && (
              <Link
                data-testid={`Edit publication link for ${publication.title}`}
                unvisited
                to={generatePath<PublicationRouteParams>(
                  publicationEditRoute.path,
                  {
                    publicationId: publication.id,
                  },
                )}
              >
                Change <span className="govuk-visually-hidden">contact</span>
              </Link>
            )
          }
        >
          <p>{contact?.contactName || 'No contact name'}</p>

          {contact?.contactTelNo && (
            <p>
              <a href={`tel:${contact.contactTelNo}`}>{contact.contactTelNo}</a>
            </p>
          )}
        </SummaryListItem>
        <SummaryListItem
          term="Methodology"
          actions={
            permissions.canUpdatePublication && (
              <Link
                data-testid={`Edit publication link for ${publication.title}`}
                unvisited
                to={generatePath<PublicationRouteParams>(
                  publicationEditRoute.path,
                  {
                    publicationId: publication.id,
                  },
                )}
              >
                Change{' '}
                <span className="govuk-visually-hidden">methodology</span>
              </Link>
            )
          }
        >
          {methodology ? (
            <Link to={`/methodologies/${methodology.id}`}>
              {methodology.title}
            </Link>
          ) : (
            <>
              {externalMethodology?.url ? (
                <>
                  {externalMethodology.title} (
                  <a
                    href={externalMethodology.url}
                    target="_blank"
                    rel="noopener noreferrer"
                  >
                    {externalMethodology.url}
                  </a>
                  )
                </>
              ) : (
                'No methodology assigned'
              )}
            </>
          )}
        </SummaryListItem>
        <SummaryListItem
          term="Releases"
          actions={permissions.canUpdatePublication && <></>}
        >
          <ul className="govuk-list">
            {releases.filter(noAmendmentInProgressFilter).map(release => (
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
            {releases.length < 1 && <>No releases created</>}
          </ul>

          <ButtonGroup className="govuk-!-margin-bottom-2">
            {permissions.canCreateReleases && (
              <ButtonLink
                to={generatePath(releaseCreateRoute.path, {
                  publicationId: id,
                })}
                testId={`Create new release link for ${title}`}
              >
                Create new release
              </ButtonLink>
            )}
            {permissions.canUpdatePublication && (
              <ButtonLink
                to={generatePath(legacyReleasesRoute.path, {
                  publicationId: id,
                })}
                variant="secondary"
                testId={`Legacy releases link for ${title}`}
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
                    publicationId: id,
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
