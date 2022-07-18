import Link from '@admin/components/Link';
import { PublishedStatusGuidanceModal } from '@admin/pages/publication/components/PublicationGuidance';
import styles from '@admin/pages/publication/components/PublicationPublishedReleases.module.scss';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import releaseService from '@admin/services/releaseService';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import InfoIcon from '@common/components/InfoIcon';
import ModalConfirm from '@common/components/ModalConfirm';
import Tag from '@common/components/Tag';
import TagGroup from '@common/components/TagGroup';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import React, { useEffect, useMemo, useRef, useState } from 'react';
import { generatePath, useHistory } from 'react-router';

const PublicationPublishedReleases = () => {
  const history = useHistory();

  const focusRef = useRef<HTMLTableRowElement>(null);

  const { publicationId, publishedReleases } = usePublicationContext();

  const [amendReleaseId, setAmendReleaseId] = useState<string>();

  const [focusIndex, setFocusIndex] = useState<number | undefined>(undefined);

  const [
    showPublishedStatusGuidance,
    togglePublishedStatusGuidance,
  ] = useToggle(false);

  const [currentChunk, setCurrentChunk] = useState<number>(1);

  const chunkSize = 5;

  const currentReleases = useMemo(() => {
    return publishedReleases.slice(0, chunkSize * currentChunk);
  }, [publishedReleases, chunkSize, currentChunk]);

  const showMoreNumber =
    chunkSize < publishedReleases.length - currentReleases.length
      ? chunkSize
      : publishedReleases.length - currentReleases.length;

  useEffect(() => {
    if (focusIndex) {
      focusRef.current?.focus();
    }
  }, [focusIndex]);

  return (
    <>
      <table data-testid="publication-published-releases">
        <caption
          aria-live="polite"
          aria-atomic
          className="govuk-table__caption--m"
        >
          Published releases ({currentReleases.length} of{' '}
          {publishedReleases.length})
        </caption>
        <thead>
          <tr>
            <th className="govuk-!-width-one-quarter">Release period</th>
            <th>
              State{' '}
              <ButtonText onClick={togglePublishedStatusGuidance.on}>
                <InfoIcon description="Guidance on draft release issues" />
              </ButtonText>
            </th>
            <th>Published date</th>
            <th className="dfe-align--right">Actions</th>
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
              <td className="govuk-!-width-one-quarter">{release.title}</td>
              <td>
                <TagGroup>
                  <Tag colour="green">Published</Tag>
                </TagGroup>
              </td>
              <td>
                <FormattedDate>{release.published || ''}</FormattedDate>
              </td>
              <td className="dfe-align--right">
                {release.permissions.canMakeAmendmentOfRelease && (
                  <ButtonText onClick={() => setAmendReleaseId(release.id)}>
                    Amend<VisuallyHidden> {release.title}</VisuallyHidden>
                  </ButtonText>
                )}
                <Link
                  className="govuk-!-margin-left-4"
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
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {currentReleases.length < publishedReleases.length && (
        <ButtonText
          onClick={() => {
            setFocusIndex(chunkSize * currentChunk + 1);
            setCurrentChunk(currentChunk + 1);
          }}
        >
          Show next {showMoreNumber} published release
          {showMoreNumber > 1 && 's'}
        </ButtonText>
      )}

      <PublishedStatusGuidanceModal
        open={showPublishedStatusGuidance}
        onClose={togglePublishedStatusGuidance.off}
      />

      {amendReleaseId && (
        <ModalConfirm
          open={!!amendReleaseId}
          title="Confirm you want to amend this live release"
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
            Please note, any changes made to this live release must be approved
            before updates can be published.
          </p>
        </ModalConfirm>
      )}
    </>
  );
};

export default PublicationPublishedReleases;
