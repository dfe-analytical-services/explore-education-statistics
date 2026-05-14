import dataReplacementService, {
  DataReplacementPlan,
  PlanMappings,
  UpdateMappingPayload,
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
  plan: DataReplacementPlan;
  reloadPlan: () => void;
}

export default function DataFileReplacementDifferences({
  releaseVersionId,
  fileId,
  replacementFileId,
  plan,
  reloadPlan,
}: Props) {
  const referencedFilters = useMemo(() => {
    // collated list of items referenced by datablocks and footnotes

    // Indicators listed in dataBlocks
    const dataBlockIndicators = plan.dataBlocks.flatMap(block =>
      Object.values(block.indicatorGroups || {}).flatMap(group =>
        (group.indicators || []).map(indicator => indicator.name),
      ),
    );

    // Indicators listed in footnotes
    const footnoteIndicators = (plan.footnotes || [])
      .flatMap(footnote => Object.values(footnote.indicatorGroups) || [])
      .flatMap(({ indicators }) => indicators.map(({ name }) => name));

    // Combined unique list
    const indicators = new Set([...dataBlockIndicators, ...footnoteIndicators]);

    return { indicators };
  }, [plan.dataBlocks, plan.footnotes]);

  const filteredMappings: DataReplacementPlan['mapping'] = useMemo(() => {
    // filtered mappings that aren't used in datablocks or footnotes

    const filteredIndicatorMappings = {
      candidates: plan.mapping.indicators.candidates,
      mappings: Object.fromEntries(
        Object.entries(plan.mapping.indicators.mappings).filter(([key]) =>
          referencedFilters.indicators.has(key),
        ),
      ),
    };

    return { indicators: filteredIndicatorMappings };
  }, [plan.mapping, referencedFilters]);

  const [planMappings, updatePlanMappings] =
    useImmer<PlanMappings>(filteredMappings);

  useEffect(() => {
    updatePlanMappings(filteredMappings);
  }, [filteredMappings, updatePlanMappings]);

  const handleIndicatorsMappingUpdate = useCallback(
    async ({ sourceKey, candidateKey }: UpdateMappingPayload) => {
      updatePlanMappings(draft => {
        draft.indicators.mappings[sourceKey].candidateKey = candidateKey;
        draft.indicators.mappings[sourceKey].type = 'ManuallySet';
        return draft;
      });

      await dataReplacementService.updatePlanIndicatorMappings(
        releaseVersionId,
        fileId,
        replacementFileId,
        [
          {
            originalColumnName: sourceKey,
            newReplacementColumnName: candidateKey,
          },
        ],
      );

      reloadPlan();
    },
    [
      updatePlanMappings,
      fileId,
      releaseVersionId,
      reloadPlan,
      replacementFileId,
    ],
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
      <h3>Mapping Dependencies</h3>

      <p>
        The following items were not found in the replacement data and were
        previously used in existing datablocks or footnotes.
        <br /> Please map these items to new items that appear in the
        replacement data or select "No mapping" for items that are no longer
        represented.
      </p>
      <div className="table-container">
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
      </div>
    </>
  );
}
