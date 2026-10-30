import Link from '@admin/components/Link';
import { releaseDataRoute } from '@admin/routes/releaseRoutes';
import releaseDataPageTabs from '@admin/pages/release/data/utils/releaseDataPageTabs';
import { IndicatorsMappingsPlan } from '@admin/services/dataReplacementService';
import tableBuilderService from '@admin/services/tableBuilderService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath } from 'react-router';

interface Props {
  indicatorMappings: IndicatorsMappingsPlan;
  publicationId: string;
  releaseVersionId: string;
  subjectId: string;
}

/**
 * Shows which paired indicators will be maintained or dropped by a data
 * replacement. A pair is only maintained if both of its indicators are
 * mapped to the replacement data.
 */
export default function DataFileReplacementPairedIndicators({
  indicatorMappings,
  publicationId,
  releaseVersionId,
  subjectId,
}: Props) {
  const { value: pairedIndicators = [], isLoading } = useAsyncHandledRetry(
    async () =>
      (
        await tableBuilderService.getPairedIndicators(
          releaseVersionId,
          subjectId,
        )
      ).pairedIndicators,
    [releaseVersionId, subjectId],
  );

  const { mappings } = indicatorMappings;

  const isMapped = (indicatorId: string) =>
    !!mappings[indicatorId]?.candidateKey;

  const getLabel = (indicatorId: string) =>
    mappings[indicatorId]?.source.label ?? indicatorId;

  const hasDroppedPairs = pairedIndicators.some(
    pair =>
      !isMapped(pair.parentIndicatorId) || !isMapped(pair.childIndicatorId),
  );

  const manageIndicatorsPath = `${generatePath(releaseDataRoute.fullPath, {
    publicationId,
    releaseVersionId,
  })}?manageIndicators=${subjectId}#${releaseDataPageTabs.reordering.id}`;

  return (
    <LoadingSpinner loading={isLoading}>
      <h3 className="govuk-heading-m govuk-!-padding-top-4">
        Paired indicators
      </h3>

      {pairedIndicators.length > 0 ? (
        <>
          <p>
            {hasDroppedPairs
              ? 'One or more paired indicators will be dropped by this data replacement because their parent or child indicator has not been mapped. Indicators from a dropped pair can be selected independently in the table tool.'
              : 'All paired indicators will be maintained by this data replacement.'}
          </p>

          <table data-testid="paired-indicators-table">
            <caption className="govuk-visually-hidden">
              Table showing whether each pair of indicators will be maintained
              or dropped by this data replacement.
            </caption>
            <thead>
              <tr>
                <th scope="col">Parent indicator</th>
                <th scope="col">Child indicator</th>
                <th scope="col">Status</th>
              </tr>
            </thead>
            <tbody>
              {pairedIndicators.map(pair => {
                const isMaintained =
                  isMapped(pair.parentIndicatorId) &&
                  isMapped(pair.childIndicatorId);

                return (
                  <tr
                    key={`${pair.parentIndicatorId}-${pair.childIndicatorId}`}
                  >
                    <td>{getLabel(pair.parentIndicatorId)}</td>
                    <td>{getLabel(pair.childIndicatorId)}</td>
                    <td>
                      <Tag colour={isMaintained ? 'green' : 'orange'}>
                        {isMaintained ? 'Maintained' : 'Dropped'}
                      </Tag>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </>
      ) : (
        <p>No indicators have been paired for this data set.</p>
      )}

      <p>
        <Link to={manageIndicatorsPath} unvisited>
          Manage paired indicators
        </Link>
      </p>
    </LoadingSpinner>
  );
}
