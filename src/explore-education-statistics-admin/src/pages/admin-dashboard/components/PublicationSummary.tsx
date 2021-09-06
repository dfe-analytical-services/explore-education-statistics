import ButtonLink from '@admin/components/ButtonLink';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import {
  publicationEditRoute,
  PublicationRouteParams,
  releaseCreateRoute,
} from '@admin/routes/routes';
import { MyPublication } from '@admin/services/publicationService';
import releaseService, {
  DeleteReleasePlan,
  Release,
} from '@admin/services/releaseService';
import ButtonGroup from '@common/components/ButtonGroup';
import ModalConfirm from '@common/components/ModalConfirm';
import React, { useState } from 'react';
import { generatePath, useHistory } from 'react-router';
import CancelAmendmentModal from './CancelAmendmentModal';
import NonScheduledReleaseSummary from './NonScheduledReleaseSummary';
import MethodologySummary from './MethodologySummary';

export interface Props {
  publication: MyPublication;
  topicId: string;
  onChangePublication: () => void;
}

const PublicationSummary = ({
  publication,
  topicId,
  onChangePublication,
}: Props) => {
  const history = useHistory();

  const [amendReleaseId, setAmendReleaseId] = useState<string>();
  const { contact, permissions, releases, id, title } = publication;

  const [deleteReleasePlan, setDeleteReleasePlan] = useState<
    DeleteReleasePlan
  >();

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
          </tr>
          <tr>
            <th>Methodologies</th>
            <td data-testid={`Methodology for ${publication.title}`}>
              <MethodologySummary
                publication={publication}
                topicId={topicId}
                onChangePublication={onChangePublication}
              />
            </td>
          </tr>
          {permissions.canUpdatePublication && (
            <tr>
              <th>Manage</th>
              <td>
                <ButtonGroup className="govuk-!-margin-bottom-2">
                  <ButtonLink
                    data-testid={`Edit publication link for ${publication.title}`}
                    to={generatePath<PublicationRouteParams>(
                      publicationEditRoute.path,
                      {
                        publicationId: publication.id,
                      },
                    )}
                  >
                    Manage this publication
                  </ButtonLink>
                </ButtonGroup>
              </td>
            </tr>
          )}
          <tr>
            <th>Releases</th>
            <td colSpan={2} data-testid={`Releases for ${publication.title}`}>
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
              </ButtonGroup>
              {releases.length > 0 ? (
                <ul className="govuk-list">
                  {releases.filter(noAmendmentInProgressFilter).map(release => (
                    <li key={release.id}>
                      <NonScheduledReleaseSummary
                        onClickAmendRelease={setAmendReleaseId}
                        onClickCancelAmendment={async releaseId => {
                          setDeleteReleasePlan(
                            await releaseService.getDeleteReleasePlan(
                              releaseId,
                            ),
                          );
                        }}
                        release={release}
                      />
                    </li>
                  ))}
                </ul>
              ) : (
                <p>No releases created</p>
              )}
            </td>
          </tr>
        </tbody>
      </table>

      {amendReleaseId && (
        <ModalConfirm
          title="Confirm you want to amend this live release"
          onConfirm={async () => {
            const amendment = await releaseService.createReleaseAmendment(
              amendReleaseId,
            );

            history.push(
              generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
                publicationId: id,
                releaseId: amendment.id,
              }),
            );
          }}
          onExit={() => setAmendReleaseId(undefined)}
          onCancel={() => setAmendReleaseId(undefined)}
          open
        >
          <p>
            Please note, any changes made to this live release must be approved
            before updates can be published.
          </p>
        </ModalConfirm>
      )}

      {deleteReleasePlan && (
        <CancelAmendmentModal
          methodologiesScheduledWithRelease={
            deleteReleasePlan.methodologiesScheduledWithRelease
          }
          onConfirm={async () => {
            await releaseService.deleteRelease(deleteReleasePlan.releaseId);
            setDeleteReleasePlan(undefined);
            onChangePublication();
          }}
          onCancel={() => setDeleteReleasePlan(undefined)}
        />
      )}
    </>
  );
};

export default PublicationSummary;
