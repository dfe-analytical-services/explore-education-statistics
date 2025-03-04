import Link from '@admin/components/Link';
import styles from '@admin/pages/publication/PublicationReleasesPage.module.scss';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import { ReleaseVersionSummaryWithPermissions } from '@admin/services/releaseVersionService';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import InsetText from '@common/components/InsetText';
import ModalConfirm from '@common/components/ModalConfirm';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React, { useEffect, useRef } from 'react';
import { generatePath } from 'react-router';
import { PublishedStatusGuidanceModal } from './PublicationGuidance';
import ReleaseLabelEditModal, {
  ReleaseLabelFormValues,
} from './ReleaseLabelEditModal';

interface PublishedReleasesTableProps {
  focusReleaseId?: string;
  publicationId: string;
  releases: ReleaseVersionSummaryWithPermissions[];
  onAmend: (releaseVersionId: string) => void;
  onEdit: (
    releaseId: string,
    releaseDetailsFormValues: ReleaseLabelFormValues,
  ) => Promise<void>;
}

export default function PublicationPublishedReleasesTable({
  focusReleaseId,
  publicationId,
  releases,
  onAmend,
  onEdit,
}: PublishedReleasesTableProps) {
  const rowRef = useRef<HTMLTableRowElement>(null);

  useEffect(() => {
    rowRef.current?.focus();
  }, [focusReleaseId]);

  if (releases.length === 0) {
    return <InsetText>You have no published releases.</InsetText>;
  }

  return (
    <table
      className="dfe-hide-empty-cells"
      data-testid="publication-published-releases"
    >
      <thead>
        <tr>
          <th className="govuk-!-width-one-third">Release period</th>
          <th className={`${styles.statusColumn} dfe-white-space--nowrap`}>
            Status <PublishedStatusGuidanceModal />
          </th>
          <th>Published date</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {releases.map(release => {
          const isFocused = focusReleaseId === release.id;

          return (
            <tr
              className={styles.row}
              key={release.id}
              ref={isFocused ? rowRef : undefined}
              tabIndex={isFocused ? -1 : undefined}
            >
              <td>{release.title}</td>
              <td>
                <Tag colour="green">Published</Tag>
              </td>
              <td>
                {release.published && (
                  <FormattedDate>{release.published}</FormattedDate>
                )}
              </td>
              <td>
                {release.permissions.canViewReleaseVersion ? (
                  <Link
                    className="govuk-!-margin-right-4 govuk-!-display-inline-block"
                    to={generatePath<ReleaseRouteParams>(
                      releaseSummaryRoute.path,
                      {
                        publicationId,
                        releaseVersionId: release.id,
                      },
                    )}
                  >
                    View<VisuallyHidden> {release.title}</VisuallyHidden>
                  </Link>
                ) : (
                  <>
                    No permission
                    <VisuallyHidden> {release.title}</VisuallyHidden>
                  </>
                )}
                {release.permissions.canUpdateRelease && (
                  <ReleaseLabelEditModal
                    triggerButton={
                      <ButtonText
                        className={`govuk-!-display-inline-block ${
                          release.permissions.canMakeAmendmentOfReleaseVersion
                            ? ' govuk-!-margin-right-4'
                            : ''
                        }`}
                      >
                        Edit details
                      </ButtonText>
                    }
                    initialValues={{ label: release.label }}
                    onSubmit={formValues =>
                      onEdit(release.releaseId, formValues)
                    }
                  />
                )}
                {release.permissions.canMakeAmendmentOfReleaseVersion && (
                  <ModalConfirm
                    title="Confirm you want to amend this published release"
                    triggerButton={
                      <ButtonText>
                        Amend<VisuallyHidden> {release.title}</VisuallyHidden>
                      </ButtonText>
                    }
                    onConfirm={async () => onAmend(release.id)}
                  >
                    <p>
                      Please note, any changes made to this published release
                      must be approved before updates can be published.
                    </p>
                  </ModalConfirm>
                )}
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
