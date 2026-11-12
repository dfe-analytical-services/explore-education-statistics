import releaseDataPageTabs from '@admin/pages/release/data/utils/releaseDataPageTabs';
import { releaseDataRoute } from '@admin/routes/releaseRoutes';
import { IndicatorsMappingsPlan } from '@admin/services/dataReplacementService';
import { PairedIndicator } from '@admin/services/tableBuilderService';
import { generatePath } from 'react-router';

export interface ReplacementPairedIndicator {
  parentIndicatorId: string;
  parentLabel: string;
  childIndicatorId: string;
  childLabel: string;
  maintained: boolean;
}

/**
 * Works out which paired indicators a data replacement will maintain or drop.
 * A pair is only maintained if both of its indicators are mapped to the
 * replacement data.
 */
export function getReplacementPairedIndicators(
  pairedIndicators: PairedIndicator[],
  { mappings }: IndicatorsMappingsPlan,
): ReplacementPairedIndicator[] {
  const isMapped = (indicatorId: string) =>
    !!mappings[indicatorId]?.candidateKey;

  const getLabel = (indicatorId: string) =>
    mappings[indicatorId]?.source.label ?? indicatorId;

  return pairedIndicators.map(({ parentIndicatorId, childIndicatorId }) => ({
    parentIndicatorId,
    parentLabel: getLabel(parentIndicatorId),
    childIndicatorId,
    childLabel: getLabel(childIndicatorId),
    maintained: isMapped(parentIndicatorId) && isMapped(childIndicatorId),
  }));
}

/**
 * Path to the Manage filters and indicators tab, opening Manage indicators
 * for the data set if {@param subjectId} is given.
 */
export function getManageIndicatorsPath({
  publicationId,
  releaseVersionId,
  subjectId,
}: {
  publicationId: string;
  releaseVersionId: string;
  subjectId?: string;
}): string {
  const dataPath = generatePath(releaseDataRoute.fullPath, {
    publicationId,
    releaseVersionId,
  });
  const query = subjectId ? `?manageIndicators=${subjectId}` : '';

  return `${dataPath}${query}#${releaseDataPageTabs.reordering.id}`;
}
