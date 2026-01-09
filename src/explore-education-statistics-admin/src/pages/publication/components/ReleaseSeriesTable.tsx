import Link from '@admin/components/Link';
import {
  publicationEditReleaseSeriesLegacyLinkRoute,
  PublicationEditReleaseSeriesLegacyLinkRouteParams,
} from '@admin/routes/publicationRoutes';
import { ReleaseSeriesTableEntry } from '@admin/services/publicationService';
import ReorderableList from '@common/components/ReorderableList';
import ButtonGroup from '@common/components/ButtonGroup';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import reorder from '@common/utils/reorder';
import ButtonText from '@common/components/ButtonText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useConfig } from '@admin/contexts/ConfigContext';
import Tag from '@common/components/Tag';
import React, { useEffect, useState } from 'react';
import { generatePath } from 'react-router';
import { useHistory } from 'react-router-dom';

interface Props {
  canManageReleaseSeries: boolean;
  isReordering: boolean;
  releaseSeries: ReleaseSeriesTableEntry[];
  publicationId: string;
  publicationSlug: string;
  onCancelReordering: () => void;
  onConfirmReordering: (nextSeries: ReleaseSeriesTableEntry[]) => void;
  onDelete: (id: string) => Promise<void> | void;
}
export default function ReleaseSeriesTable({
  canManageReleaseSeries,
  isReordering,
  releaseSeries: initialReleaseSeries,
  publicationId,
  publicationSlug,
  onCancelReordering,
  onConfirmReordering,
  onDelete,
}: Props) {
  const history = useHistory();
  const [releaseSeries, setReleaseSeries] = useState(initialReleaseSeries);

  useEffect(() => {
    setReleaseSeries(initialReleaseSeries);
  }, [initialReleaseSeries]);

  const config = useConfig();

  if (isReordering) {
    return (
      <ReorderableList
        heading="Reorder releases"
        id="releaseSeries"
        list={releaseSeries.map(seriesItem => ({
          id: seriesItem.id,
          label: seriesItem.description,
        }))}
        onCancel={() => {
          setReleaseSeries(initialReleaseSeries);
          onCancelReordering();
        }}
        onConfirm={() => onConfirmReordering(releaseSeries)}
        onMoveItem={({ prevIndex, nextIndex }) => {
          const reordered = reorder(releaseSeries, prevIndex, nextIndex);
          setReleaseSeries(reordered);
        }}
        onReverse={() => {
          setReleaseSeries(releaseSeries.toReversed());
        }}
      />
    );
  }

  return (
    <table data-testid="release-series">
      <caption className="govuk-visually-hidden">
        Table showing the ordered releases for this publication.
      </caption>
      <thead>
        <tr>
          <th>Description</th>
          <th className="govuk-!-width-one-half">URL</th>
          <th>Status</th>
          {canManageReleaseSeries && <th>Actions</th>}
        </tr>
      </thead>
      <tbody>
        {releaseSeries.map(seriesItem => (
          <tr key={seriesItem.id}>
            <td>
              <span className="govuk-!-display-block">
                {seriesItem.description}
              </span>
            </td>
            <td className="govuk-!-width-one-half">
              {seriesItem.isLegacyLink && (
                <a
                  className="govuk-link--no-visited-state"
                  href={seriesItem.legacyLinkUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  {seriesItem.legacyLinkUrl}
                </a>
              )}

              {!seriesItem.isLegacyLink && seriesItem.isPublished && (
                <Link
                  to={`${config.publicAppUrl}/find-statistics/${publicationSlug}/${seriesItem.releaseSlug}`}
                  className="govuk-link--no-visited-state"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  {`${config.publicAppUrl}/find-statistics/${publicationSlug}/${seriesItem.releaseSlug}`}
                </Link>
              )}
              {!seriesItem.isLegacyLink &&
                !seriesItem.isPublished &&
                `${config.publicAppUrl}/find-statistics/${publicationSlug}/${seriesItem.releaseSlug}`}
            </td>

            <td>
              {seriesItem.isLegacyLink ? (
                <Tag colour="grey">Legacy release</Tag>
              ) : (
                <>
                  {seriesItem.isLatest && <Tag>Latest release</Tag>}
                  {!seriesItem.isPublished && <Tag>Unpublished</Tag>}
                </>
              )}
            </td>

            {canManageReleaseSeries && (
              <td>
                {seriesItem.isLegacyLink && (
                  <ButtonGroup className="govuk-!-margin-bottom-0">
                    <ModalConfirm
                      confirmText="OK"
                      title="Edit legacy release"
                      triggerButton={
                        <ButtonText>
                          Edit
                          <VisuallyHidden>
                            {` ${seriesItem.description}`}
                          </VisuallyHidden>
                        </ButtonText>
                      }
                      onConfirm={() => {
                        history.push(
                          generatePath<PublicationEditReleaseSeriesLegacyLinkRouteParams>(
                            publicationEditReleaseSeriesLegacyLinkRoute.path,
                            {
                              publicationId,
                              releaseSeriesItemId: seriesItem.id,
                            },
                          ),
                        );
                      }}
                    >
                      <WarningMessage>
                        All changes made to legacy releases appear immediately
                        on the public website.
                      </WarningMessage>
                    </ModalConfirm>

                    <ModalConfirm
                      title="Delete legacy release"
                      triggerButton={
                        <ButtonText variant="warning">
                          Delete
                          <VisuallyHidden>
                            {` ${seriesItem.description}`}
                          </VisuallyHidden>
                        </ButtonText>
                      }
                      onConfirm={async () => {
                        await onDelete(seriesItem.id);
                      }}
                    >
                      <p>
                        Are you sure you want to delete this legacy release?
                      </p>
                      <p>
                        All changes made to legacy releases appear immediately
                        on the public website.
                      </p>
                    </ModalConfirm>
                  </ButtonGroup>
                )}
              </td>
            )}
          </tr>
        ))}
      </tbody>
    </table>
  );
}
