import ButtonLink from '@admin/components/ButtonLink';
import Link from '@admin/components/Link';
import ThemeAndTopicContext from '@admin/components/ThemeAndTopicContext';
import releaseRoutes, { summaryRoute } from '@admin/routes/edit-release/routes';
import {
  AdminDashboardPublication,
  AdminDashboardRelease,
} from '@admin/services/dashboard/types';
import service from '@admin/services/release/create-release/service';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React, { useContext, useEffect, useState } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';
import NonScheduledReleaseSummary from './NonScheduledReleaseSummary';
import CancelAmendmentModal from './CancelAmendmentModal';

export interface Props {
  initialPublication: AdminDashboardPublication;
}

const PublicationSummary = ({
  initialPublication,
  history,
}: Props & RouteComponentProps) => {
  const [publication, setPublication] = useState<AdminDashboardPublication>();

  useEffect(() => {
    setPublication(initialPublication);
  }, [initialPublication, setPublication]);

  const { selectedThemeAndTopic } = useContext(ThemeAndTopicContext);
  const [amendReleaseId, setAmendReleaseId] = useState<string>();
  const [cancelAmendmentReleaseId, setCancelAmendmentReleaseId] = useState<
    string
  >();
  const noAmendmentInProgressFilter = (release: AdminDashboardRelease) =>
    publication &&
    !publication.releases.some(r => r.amendment && r.originalId === release.id);

  return (
    <>
      {publication && (
        <>
          <SummaryList>
            <SummaryListItem term="Methodology" smallKey>
              {publication.methodology && (
                <Link to={`/methodologies/${publication.methodology.id}`}>
                  {publication.methodology.title}
                </Link>
              )}
              {publication.externalMethodology &&
                publication.externalMethodology.url && (
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
                )}
              {!publication.methodology &&
                (!publication.externalMethodology ||
                  !publication.externalMethodology.url) && (
                  <>No methodology assigned</>
                )}
            </SummaryListItem>
            <SummaryListItem term="Releases" smallKey>
              <ul className="govuk-list dfe-admin">
                {publication.releases
                  .filter(noAmendmentInProgressFilter)
                  .map(release => (
                    <li key={release.id}>
                      <NonScheduledReleaseSummary
                        onClickAmendRelease={setAmendReleaseId}
                        onClickCancelAmendment={setCancelAmendmentReleaseId}
                        release={release}
                      />
                    </li>
                  ))}
                {publication.releases.length < 1 && <>No releases created</>}
              </ul>
            </SummaryListItem>
          </SummaryList>
          <SummaryList>
            <SummaryListItem term="" smallKey>
              {publication.permissions.canCreateReleases && (
                <ButtonLink
                  to={releaseRoutes.createReleaseRoute.generateLink(
                    publication.id,
                  )}
                  className="govuk-!-margin-right-6"
                  testId={`Create new release link for ${publication.title}`}
                >
                  Create new release
                </ButtonLink>
              )}
              <ButtonLink
                to={`/theme/${selectedThemeAndTopic.theme.id}/topic/${selectedThemeAndTopic.topic.id}/publication/${publication.id}/assign-methodology`}
                className="govuk-button--secondary"
              >
                {!publication.methodology &&
                (!publication.externalMethodology ||
                  !publication.externalMethodology.url)
                  ? 'Add'
                  : 'Edit'}{' '}
                methodology
              </ButtonLink>
            </SummaryListItem>
          </SummaryList>

          {amendReleaseId && (
            <ModalConfirm
              title="Confirm you want to amend this live release"
              onConfirm={async () =>
                service
                  .createReleaseAmendment(amendReleaseId)
                  .then(amendment =>
                    history.push(
                      summaryRoute.generateLink(publication.id, amendment.id),
                    ),
                  )
              }
              onExit={() => setAmendReleaseId(undefined)}
              onCancel={() => setAmendReleaseId(undefined)}
              mounted
            >
              <p>
                Please note, any changes made to this live release must be
                approved before updates can be published.
              </p>
            </ModalConfirm>
          )}

          {cancelAmendmentReleaseId && (
            <CancelAmendmentModal
              onConfirm={async () =>
                service.deleteRelease(cancelAmendmentReleaseId).then(() => {
                  setPublication({
                    ...publication,
                    releases: publication.releases.filter(
                      r => r.id !== cancelAmendmentReleaseId,
                    ),
                  });
                  setCancelAmendmentReleaseId(undefined);
                })
              }
              onCancel={() => setCancelAmendmentReleaseId(undefined)}
            />
          )}
        </>
      )}
    </>
  );
};

export default withRouter(PublicationSummary);
