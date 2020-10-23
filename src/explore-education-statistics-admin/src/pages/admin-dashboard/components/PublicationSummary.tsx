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
import { MyPublication } from '@admin/services/publicationService';
import releaseService, { Release } from '@admin/services/releaseService';
import ButtonGroup from '@common/components/ButtonGroup';
import ModalConfirm from '@common/components/ModalConfirm';
import React, { useState } from 'react';
import { generatePath, useHistory } from 'react-router';
import CancelAmendmentModal from './CancelAmendmentModal';
import NonScheduledReleaseSummary from './NonScheduledReleaseSummary';

export interface Props {
  publication: MyPublication;
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

  return (
    <>
      <table>
        <tbody>
          <tr>
            <th className="govuk-!-width-one-quarter">Team</th>
            <td className="govuk-!-width-one-half">
              <p
                className="govuk-!-margin-bottom-1"
                data-testid={`Team name for ${publication.title}`}
              >
                {contact?.teamName || 'No team name'}
              </p>

              {contact?.teamEmail && (
                <p className="govuk-!-margin-bottom-0">
                  <a
                    href={`mailto:${contact.teamEmail}`}
                    data-testid={`Team email for ${publication.title}`}
                  >
                    {contact.teamEmail}
                  </a>
                </p>
              )}
            </td>
            <td className="dfe-align--right govuk-!-width-one-quarter">
              {permissions.canUpdatePublication && (
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
              )}
            </td>
          </tr>
          <tr>
            <th>Contact</th>
            <td>
              <p
                className="govuk-!-margin-bottom-1"
                data-testid={`Contact name for ${publication.title}`}
              >
                {contact?.contactName || 'No contact name'}
              </p>

              {contact?.contactTelNo && (
                <p className="govuk-!-margin-bottom-0">
                  <a
                    href={`tel:${contact.contactTelNo}`}
                    data-testid={`Contact phone number for ${publication.title}`}
                  >
                    {contact.contactTelNo}
                  </a>
                </p>
              )}
            </td>
            <td className="dfe-align--right">
              {permissions.canUpdatePublication && (
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
              )}
            </td>
          </tr>
          <tr>
            <th>Methodology</th>
            <td data-testid={`Methodology for ${publication.title}`}>
              {methodology ? (
                <Link to={`/methodologies/${methodology.id}`}>
                  {methodology.title}
                </Link>
              ) : (
                <>
                  {externalMethodology?.url ? (
                    <Link to={externalMethodology.url} unvisited>
                      {externalMethodology.title} (external methodology)
                    </Link>
                  ) : (
                    'No methodology assigned'
                  )}
                </>
              )}
            </td>
            <td className="dfe-align--right">
              {permissions.canUpdatePublication && (
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
              )}
            </td>
          </tr>
          <tr>
            <th>Releases</th>
            <td colSpan={2} data-testid={`Releases for ${publication.title}`}>
              {releases.length > 0 ? (
                <ul className="govuk-list">
                  {releases.filter(noAmendmentInProgressFilter).map(release => (
                    <li key={release.id}>
                      <NonScheduledReleaseSummary
                        onClickAmendRelease={setAmendReleaseId}
                        onClickCancelAmendment={setCancelAmendmentReleaseId}
                        release={release}
                      />
                    </li>
                  ))}
                </ul>
              ) : (
                <p>No releases created</p>
              )}

              <ButtonGroup className="govuk-!-margin-bottom-2">
                {permissions.canCreateReleases && (
                  <ButtonLink
                    to={generatePath(releaseCreateRoute.path, {
                      publicationId: id,
                    })}
                    data-testid={`Create new release link for ${title}`}
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
                    data-testid={`Legacy releases link for ${title}`}
                  >
                    Manage legacy releases
                  </ButtonLink>
                )}
              </ButtonGroup>
            </td>
          </tr>
        </tbody>
      </table>

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
