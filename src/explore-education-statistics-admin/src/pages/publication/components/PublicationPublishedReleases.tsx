import Link from '@admin/components/Link';
import { PublishedStatusGuidanceModal } from '@admin/pages/publication/components/PublicationGuidance';
import styles from '@admin/pages/publication//PublicationReleasesPage.module.scss';
import releaseService, { MyRelease } from '@admin/services/releaseService';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import InfoIcon from '@common/components/InfoIcon';
import ModalConfirm from '@common/components/ModalConfirm';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import React, { useEffect, useMemo, useRef, useState } from 'react';
import { generatePath, useHistory } from 'react-router';

const pageSize = 5;

interface Props {
  publicationId: string;
  releases: MyRelease[];
}

const PublicationPublishedReleases = ({ publicationId, releases }: Props) => {
  const history = useHistory();

  const focusRef = useRef<HTMLTableRowElement>(null);

  const [amendReleaseId, setAmendReleaseId] = useState<string>();

  const [focusIndex, setFocusIndex] = useState<number | undefined>(undefined);

  const [
    showPublishedStatusGuidance,
    togglePublishedStatusGuidance,
  ] = useToggle(false);

  const [currentPage, setCurrentPage] = useState<number>(1);

  const currentReleases = useMemo(() => {
    return releases.slice(0, pageSize * currentPage);
  }, [releases, currentPage]);

  const showMoreNumber =
    pageSize < releases.length - currentReleases.length
      ? pageSize
      : releases.length - currentReleases.length;

  useEffect(() => {
    if (focusIndex) {
      focusRef.current?.focus();
    }
  }, [focusIndex]);

  return (
    <>
      <table
        className="dfe-hide-empty-cells"
        data-testid="publication-published-releases"
      >
        <caption
          aria-live="polite"
          aria-atomic
          className="govuk-table__caption--m"
        >
          {`Published releases (${currentReleases.length} of ${releases.length})`}
        </caption>
        <thead>
          <tr>
            <th className="govuk-!-width-one-third">Release period</th>
            <th className={styles.statusColumn}>
              State{' '}
              <ButtonText onClick={togglePublishedStatusGuidance.on}>
                <InfoIcon description="Guidance on states" />
              </ButtonText>
            </th>
            <th>Published date</th>
            <th className={styles.actionsColumn}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {currentReleases.map((release, index) => (
            <tr
              className={styles.row}
              key={release.id}
              ref={focusIndex === index + 1 ? focusRef : undefined}
              tabIndex={focusIndex === index + 1 ? -1 : undefined}
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
                <Link
                  to={generatePath<ReleaseRouteParams>(
                    releaseSummaryRoute.path,
                    {
                      publicationId: release.publicationId,
                      releaseId: release.id,
                    },
                  )}
                >
                  View<VisuallyHidden> {release.title}</VisuallyHidden>
                </Link>
                {release.permissions.canMakeAmendmentOfRelease && (
                  <ButtonText
                    className="govuk-!-margin-left-4"
                    onClick={() => setAmendReleaseId(release.id)}
                  >
                    Amend<VisuallyHidden> {release.title}</VisuallyHidden>
                  </ButtonText>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {currentReleases.length < releases.length && (
        <ButtonText
          onClick={() => {
            setFocusIndex(pageSize * currentPage + 1);
            setCurrentPage(currentPage + 1);
          }}
        >
          {`Show ${showMoreNumber} more published release${
            showMoreNumber > 1 ? 's' : ''
          }`}
        </ButtonText>
      )}

      <PublishedStatusGuidanceModal
        open={showPublishedStatusGuidance}
        onClose={togglePublishedStatusGuidance.off}
      />

      {amendReleaseId && (
        <ModalConfirm
          open={!!amendReleaseId}
          title="Confirm you want to amend this published release"
          onCancel={() => setAmendReleaseId(undefined)}
          onConfirm={async () => {
            const amendment = await releaseService.createReleaseAmendment(
              amendReleaseId,
            );

            history.push(
              generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
                publicationId,
                releaseId: amendment.id,
              }),
            );
          }}
          onExit={() => setAmendReleaseId(undefined)}
        >
          <p>
            Please note, any changes made to this published release must be
            approved before updates can be published.
          </p>
        </ModalConfirm>
      )}
    </>
  );
};

export default PublicationPublishedReleases;
