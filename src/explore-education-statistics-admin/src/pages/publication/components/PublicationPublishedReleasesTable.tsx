import Link from '@admin/components/Link';
import styles from '@admin/pages/publication/PublicationReleasesPage.module.scss';
import { releaseSummaryRoute } from '@admin/routes/releaseRoutes';
import { releaseCreateRoute } from '@admin/routes/routes';
import { ReleaseVersionSummaryWithPermissions } from '@admin/services/releaseVersionService';
import appendQuery from '@common/utils/url/appendQuery';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import InsetText from '@common/components/InsetText';
import ModalConfirm from '@common/components/ModalConfirm';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React, { useEffect, useRef } from 'react';
import { generatePath } from 'react-router';
import { Publication } from '@admin/services/publicationService';
import { useConfig } from '@admin/contexts/ConfigContext';
import { PublishedStatusGuidanceModal } from './PublicationGuidance';
import ReleaseLabelEditModal, {
  ReleaseLabelFormValues,
} from './ReleaseLabelEditModal';

interface PublishedReleasesTableProps {
  focusReleaseId?: string;
  publication: Publication;
  releases: ReleaseVersionSummaryWithPermissions[];
  onAmend: (releaseVersionId: string) => void;
  onEdit: (
    releaseId: string,
    releaseDetailsFormValues: ReleaseLabelFormValues,
  ) => Promise<void>;
}

export default function PublicationPublishedReleasesTable({
  focusReleaseId,
  publication,
  releases,
  onAmend,
  onEdit,
}: PublishedReleasesTableProps) {
  const rowRef = useRef<HTMLTableRowElement>(null);
  const { publicAppUrl, enableReleaseRollover } = useConfig();

  useEffect(() => {
    rowRef.current?.focus();
  }, [focusReleaseId]);

  if (releases.length === 0) {
    return <InsetText>You have no published releases.</InsetText>;
  }

  return (
    <>
      <div className="table-container">
        <table
          className="dfe-hide-empty-cells"
          data-testid="publication-published-releases"
        >
          <caption className="govuk-visually-hidden">
            Table showing the published releases for this publication.
          </caption>
          <thead>
            <tr>
              <th className="govuk-!-width-one-third">Release period</th>
              <th className={`${styles.statusColumn} dfe-white-space--nowrap`}>
                Status
              </th>
              <th>Published date</th>
              <th>Last published</th>
              <th className="govuk-!-width-one-quarter">Actions</th>
            </tr>
          </thead>
          <tbody>
            {releases.map(releaseVersion => {
              const isFocused = focusReleaseId === releaseVersion.id;

              return (
                <tr
                  className={styles.row}
                  key={releaseVersion.id}
                  ref={isFocused ? rowRef : undefined}
                  tabIndex={isFocused ? -1 : undefined}
                >
                  <td>
                    <Link
                      to={`${publicAppUrl}/find-statistics/${publication.slug}/${releaseVersion.slug}`}
                      target="_blank"
                      rel="noopener noreferrer"
                    >
                      {releaseVersion.title} (opens in new tab)
                    </Link>
                  </td>
                  <td>
                    <Tag colour="green">Published</Tag>
                  </td>
                  <td>
                    {releaseVersion.publishedDisplayDate && (
                      <FormattedDate usingUkTime>
                        {releaseVersion.publishedDisplayDate}
                      </FormattedDate>
                    )}
                  </td>
                  <td>
                    {releaseVersion.lastPublished && (
                      <FormattedDate usingUkTime>
                        {releaseVersion.lastPublished}
                      </FormattedDate>
                    )}
                  </td>
                  <td>
                    <div className={styles.actionsColumn}>
                      {releaseVersion.permissions.canViewReleaseVersion ? (
                        <Link
                          to={generatePath(releaseSummaryRoute.fullPath, {
                            publicationId: publication.id,
                            releaseVersionId: releaseVersion.id,
                          })}
                        >
                          View
                          <VisuallyHidden>
                            {' '}
                            {releaseVersion.title}
                          </VisuallyHidden>
                        </Link>
                      ) : (
                        <>
                          No permission
                          <VisuallyHidden>
                            {' '}
                            {releaseVersion.title}
                          </VisuallyHidden>
                        </>
                      )}
                      {releaseVersion.permissions.canUpdateRelease && (
                        <ReleaseLabelEditModal
                          currentReleaseSlug={releaseVersion.slug}
                          publicationSlug={publication.slug}
                          initialValues={{ label: releaseVersion.label }}
                          triggerButton={
                            <ButtonText>
                              Edit release label
                              <VisuallyHidden>
                                {' '}
                                for {releaseVersion.title}
                              </VisuallyHidden>
                            </ButtonText>
                          }
                          onSubmit={formValues =>
                            onEdit(releaseVersion.releaseId, formValues)
                          }
                        />
                      )}
                      {releaseVersion.permissions
                        .canMakeAmendmentOfReleaseVersion && (
                        <ModalConfirm
                          title="Confirm you want to amend this published release"
                          triggerButton={
                            <ButtonText>
                              Amend
                              <VisuallyHidden>
                                {' '}
                                {releaseVersion.title}
                              </VisuallyHidden>
                            </ButtonText>
                          }
                          onConfirm={async () => onAmend(releaseVersion.id)}
                        >
                          <p>
                            Please note, any changes made to this published
                            release must be approved before updates can be
                            published.
                          </p>
                        </ModalConfirm>
                      )}
                      {enableReleaseRollover &&
                        publication.permissions?.canCreateReleases && (
                          <Link
                            to={appendQuery(
                              generatePath(releaseCreateRoute.fullPath, {
                                publicationId: publication.id,
                              }),
                              { sourceReleaseVersionId: releaseVersion.id },
                            )}
                          >
                            Create release rollover
                            <VisuallyHidden>
                              {' '}
                              of {releaseVersion.title}
                            </VisuallyHidden>
                          </Link>
                        )}
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
      <PublishedStatusGuidanceModal />
    </>
  );
}
