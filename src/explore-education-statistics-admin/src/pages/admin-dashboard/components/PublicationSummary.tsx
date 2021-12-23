import ButtonLink from '@admin/components/ButtonLink';
import NonScheduledReleaseSummary from '@admin/pages/admin-dashboard/components/NonScheduledReleaseSummary';
import MethodologySummary from '@admin/pages/admin-dashboard/components/MethodologySummary';
import styles from '@admin/pages/admin-dashboard/components/PublicationSummary.module.scss';
import {
  publicationEditRoute,
  publicationManageTeamAccessRoute,
  PublicationRouteParams,
  releaseCreateRoute,
} from '@admin/routes/routes';
import { MyPublication } from '@admin/services/publicationService';
import { Release } from '@admin/services/releaseService';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import WarningMessage from '@common/components/WarningMessage';
import React from 'react';
import { generatePath } from 'react-router';
import classNames from 'classnames';

export interface Props {
  publication: MyPublication;
  topicId: string;
  onChangePublication: () => void;
  showManageTeamAccessButton?: boolean;
}

const PublicationSummary = ({
  publication,
  topicId,
  onChangePublication,
  showManageTeamAccessButton = true,
}: Props) => {
  const { contact, permissions, releases, id, title } = publication;

  const noAmendmentInProgressFilter = (release: Release) =>
    !releases.some(r => r.amendment && r.previousVersionId === release.id);

  return (
    <>
      <div className={styles.section}>
        <h5 className={`govuk-heading-s ${styles.sectionHeading}`}>Releases</h5>
        <div
          className={styles.sectionContent}
          data-testid={`Releases for ${publication.title}`}
        >
          {releases.length > 0 ? (
            <ul className="govuk-list govuk-!-margin-top-2">
              {releases.filter(noAmendmentInProgressFilter).map(release => (
                <li key={release.id}>
                  <NonScheduledReleaseSummary
                    includeCreateAmendmentControls
                    onAmendmentCancelled={onChangePublication}
                    release={release}
                  />
                </li>
              ))}
            </ul>
          ) : (
            <WarningMessage className="govuk-!-margin-bottom-2">
              No releases created
            </WarningMessage>
          )}
          {permissions.canCreateReleases && (
            <ButtonLink
              className="govuk-!-margin-bottom-0"
              to={generatePath<PublicationRouteParams>(
                releaseCreateRoute.path,
                {
                  publicationId: id,
                },
              )}
              data-testid={`Create new release link for ${title}`}
            >
              Create new release
            </ButtonLink>
          )}
        </div>
      </div>

      <div className={styles.section}>
        <h5 className={`govuk-heading-s ${styles.sectionHeading}`}>
          Methodologies
        </h5>
        <div
          className={styles.sectionContent}
          data-testid={`Methodology for ${publication.title}`}
        >
          <MethodologySummary
            publication={publication}
            topicId={topicId}
            onChangePublication={onChangePublication}
          />
        </div>
      </div>

      <div className={classNames(styles.section, styles.lastSection)}>
        <h5 className={`govuk-heading-s ${styles.sectionHeading}`}>
          Publication details
        </h5>
        <div
          className={styles.sectionContent}
          data-testid={`Details for ${publication.title}`}
        >
          <SummaryList className="govuk-!-margin-bottom-0">
            <SummaryListItem term="Team">
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
            </SummaryListItem>
          </SummaryList>

          <SummaryList>
            <SummaryListItem term="Contact">
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
            </SummaryListItem>
          </SummaryList>
        </div>

        {permissions.canUpdatePublication && (
          <div className={styles.sectionActions}>
            <ButtonLink
              className="govuk-!-width-full"
              data-testid={`Edit publication link for ${publication.title}`}
              to={generatePath<PublicationRouteParams>(
                publicationEditRoute.path,
                {
                  publicationId: publication.id,
                },
              )}
              variant="secondary"
            >
              Manage publication
            </ButtonLink>
            {showManageTeamAccessButton && (
              <ButtonLink
                className="govuk-!-width-full"
                data-testid={`Manage team access for publication ${publication.title}`}
                to={generatePath<PublicationRouteParams>(
                  publicationManageTeamAccessRoute.path,
                  {
                    publicationId: publication.id,
                  },
                )}
                variant="secondary"
              >
                Manage team access
              </ButtonLink>
            )}
          </div>
        )}
      </div>
    </>
  );
};

export default PublicationSummary;
