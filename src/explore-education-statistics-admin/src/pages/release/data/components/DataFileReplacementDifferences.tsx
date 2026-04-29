import dataReplacementService, {
  PlanMappings,
  UpdateMappingPayloadMultiple,
} from '@admin/services/dataReplacementService';
import Tag from '@common/components/Tag';
import TagGroup from '@common/components/TagGroup';
import VisuallyHidden from '@common/components/VisuallyHidden';
import mapValues from 'lodash/mapValues';
import startCase from 'lodash/startCase';
import React, { useCallback, useEffect, useMemo } from 'react';
import { useImmer } from 'use-immer';
import DifferencesMappingTableRows from './DataFileReplacementDifferencesTableRows';

interface Props {
  releaseVersionId: string;
  fileId: string;
  replacementFileId: string;
  mapping: PlanMappings;
  reloadPlan: () => void;
}

export default function DataFileReplacementDifferences({
  releaseVersionId,
  fileId,
  replacementFileId,
  mapping,
  reloadPlan,
}: Props) {
  const [planMappings, updatePlanMappings] = useImmer<PlanMappings>(mapping);

  useEffect(() => {
    updatePlanMappings(mapping);
  }, [mapping]);

  const handleIndicatorsMappingUpdate = useCallback(
    async (payload: UpdateMappingPayloadMultiple) => {
      updatePlanMappings(draft => {
        payload.forEach(({ sourceKey, candidateKey }) => {
          draft.indicators.mappings[sourceKey].candidateKey = candidateKey;
          draft.indicators.mappings[sourceKey].type = 'ManuallySet';
          return draft;
        });
      });

      await dataReplacementService.updatePlanIndicatorMappings(
        releaseVersionId,
        fileId,
        replacementFileId,
        payload.map(({ sourceKey, candidateKey }) => ({
          originalColumnName: sourceKey,
          newReplacementColumnName: candidateKey,
        })),
      );

      reloadPlan();
    },
    [updatePlanMappings],
  );

  const tableId = 'replacements-differences-table';

  const mappingCounts: { indicators: { mapped: number; unmapped: number } } =
    useMemo(() => {
      return mapValues(planMappings, itemType => {
        const nonAutoMappedMappings = Object.values(itemType.mappings).filter(
          ({ type }) => type !== 'AutoSet',
        );
        const manuallyMappedCount = nonAutoMappedMappings.filter(
          ({ type }) => type === 'ManuallySet',
        ).length;
        const unmappedCount =
          nonAutoMappedMappings.length - manuallyMappedCount;
        return { mapped: manuallyMappedCount, unmapped: unmappedCount };
      });
    }, [planMappings]);

  return (
    <>
      <h3>Missing Dependencies</h3>

      <p>
        The following items were not found in the replacement data and were
        previously used in existing datablocks or footnotes.
        <br /> Please map these items to new items that appear in the
        replacement data or select "No mapping" for items that are no longer
        represented.
      </p>

      <table
        className="dfe-table--vertical-align-middle dfe-table--row-highlights"
        id={tableId}
        data-testid={tableId}
      >
        <caption className="govuk-!-margin-bottom-3 govuk-!-font-size-24">
          {Object.entries(mappingCounts).map(
            ([itemType, { mapped, unmapped }]) => (
              <span key={itemType}>
                {startCase(itemType)}{' '}
                <TagGroup className="govuk-!-margin-left-2">
                  {unmapped > 0 && (
                    <Tag colour="red">
                      {`${unmapped} unmapped ${
                        unmapped > 1 ? itemType : itemType.slice(0, -1)
                      } `}
                    </Tag>
                  )}
                  {mapped > 0 && (
                    <Tag colour="blue">
                      {`${mapped} mapped ${
                        mapped > 1 ? itemType : itemType.slice(0, -1)
                      }`}
                    </Tag>
                  )}
                </TagGroup>
              </span>
            ),
          )}
        </caption>
        <thead>
          <VisuallyHidden as="tr">
            <th className="govuk-!-width-one-quarter">Original Item</th>
            <th className="govuk-!-width-one-quarter">Mapping</th>
            <th className="govuk-!-text-align-right">Actions</th>
          </VisuallyHidden>
        </thead>
        <tbody data-testid={`${tableId}-body`}>
          <DifferencesMappingTableRows
            itemType="indicator"
            mappings={planMappings.indicators}
            onUpdate={handleIndicatorsMappingUpdate}
          />
        </tbody>
      </table>
    </>
  );
}
