import {
  FilterOptionSource,
  LocationCandidate,
  MappingType,
} from '@admin/services/apiDataSetVersionService';
import {
  FilterCandidateWithKey,
  MappableFilter,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import ApiDataSetMappingModal from '@admin/pages/release/data/components/ApiDataSetMappingModal';
import {
  LocationCandidateWithKey,
  LocationMappingWithKey,
  MappableLocation,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import { PendingMappingUpdate } from '@admin/pages/release/data/types/apiDataSetMappings';
import Tag, { TagProps } from '@common/components/Tag';
import ButtonText from '@common/components/ButtonText';
import TagGroup from '@common/components/TagGroup';
import VisuallyHidden from '@common/components/VisuallyHidden';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { ReactNode } from 'react';
import classNames from 'classnames';
import kebabCase from 'lodash/kebabCase';

interface Props {
  candidateHint?: (
    candidate: FilterCandidateWithKey | LocationCandidateWithKey,
  ) => string;
  candidateIsMajorMapping?: (
    candidate: LocationCandidateWithKey,
    mapping: LocationMappingWithKey,
  ) => boolean;
  groupKey: string;
  groupLabel: string;
  itemLabel: string;
  itemPluralLabel: string;
  mappableItems: MappableFilter[] | MappableLocation[];
  newItems?: FilterCandidateWithKey[] | LocationCandidateWithKey[];
  pendingUpdates?: PendingMappingUpdate[];
  renderCandidate: (
    candidate: LocationCandidateWithKey | FilterCandidateWithKey,
  ) => ReactNode;
  renderCaptionEnd?: ReactNode;
  renderSource: (source: LocationCandidate | FilterOptionSource) => ReactNode;
  renderSourceDetails?: (
    source: FilterOptionSource | LocationCandidate,
  ) => ReactNode;
  onUpdate: (update: PendingMappingUpdate) => Promise<void>;
}

export default function ApiDataSetMappableTable({
  candidateHint,
  candidateIsMajorMapping,
  groupKey,
  groupLabel,
  itemLabel,
  itemPluralLabel,
  mappableItems,
  newItems = [],
  pendingUpdates = [],
  renderCandidate,
  renderCaptionEnd,
  renderSource,
  renderSourceDetails,
  onUpdate,
}: Props) {
  const totalUnmapped = mappableItems.filter(
    item => item.mapping.type === 'AutoNone',
  ).length;
  const totalManuallyMapped = mappableItems.length - totalUnmapped;

  return (
    <table
      className="dfe-table--vertical-align-middle dfe-table--row-highlights"
      id={`mappable-filter-options-${kebabCase(groupKey)}`}
      data-testid={`mappable-table-${groupKey}`}
    >
      <caption className="govuk-!-margin-bottom-3 govuk-!-font-size-24">
        {groupLabel}{' '}
        <TagGroup className="govuk-!-margin-left-2">
          {totalUnmapped > 0 && (
            <Tag colour="red">
              {`${totalUnmapped} unmapped ${
                totalUnmapped > 1 ? itemPluralLabel : itemLabel
              } `}
            </Tag>
          )}
          {totalManuallyMapped > 0 && (
            <Tag colour="blue">
              {`${totalManuallyMapped} mapped ${
                totalManuallyMapped > 1 ? itemPluralLabel : itemLabel
              } `}
            </Tag>
          )}
        </TagGroup>
        {renderCaptionEnd}
      </caption>
      <thead>
        <tr>
          <th className="govuk-!-width-one-third">Current data set</th>
          <th className="govuk-!-width-one-third">New data set</th>
          <th>Type</th>
          <th className="govuk-!-text-align-right">Actions</th>
        </tr>
      </thead>
      <tbody>
        {Object.values(mappableItems).map(({ candidate, mapping }) => {
          const isPendingUpdate = pendingUpdates.some(
            update => update.sourceKey === mapping.sourceKey,
          );

          const isMajorMapping =
            candidate && candidateIsMajorMapping
              ? candidateIsMajorMapping(candidate, mapping)
              : false;

          return (
            <tr
              key={`mapping-${mapping.sourceKey}`}
              className={classNames({
                'rowHighlight--alert': mapping.type === 'AutoNone',
              })}
            >
              <td>{renderSource(mapping.source)}</td>
              <td>
                {!candidate ? (
                  <>
                    {mapping.type === 'AutoNone' ? (
                      <Tag colour="red">Unmapped</Tag>
                    ) : (
                      'No mapping available'
                    )}
                  </>
                ) : (
                  <>{renderCandidate(candidate)}</>
                )}
              </td>
              <td>
                <Tag colour={getUpdateTagColour(mapping.type, isMajorMapping)}>
                  {getUpdateTagText(mapping.type, isMajorMapping)}
                </Tag>
              </td>
              <td className="govuk-!-text-align-right">
                {isPendingUpdate ? (
                  <LoadingSpinner
                    alert
                    hideText
                    inline
                    size="sm"
                    text={`Updating mapping for ${mapping.source.label}`}
                  />
                ) : (
                  <>
                    {mapping.type !== 'ManualNone' && (
                      <ButtonText
                        onClick={async () => {
                          await onUpdate({
                            groupKey,
                            previousCandidate: candidate,
                            previousMapping: mapping,
                            sourceKey: mapping.sourceKey,
                            type: 'ManualNone',
                          });
                        }}
                      >
                        No mapping
                        <VisuallyHidden>
                          {' '}
                          for {mapping.source.label}
                        </VisuallyHidden>
                      </ButtonText>
                    )}

                    <ApiDataSetMappingModal
                      candidate={candidate}
                      candidateHint={candidateHint}
                      groupKey={groupKey}
                      itemLabel={itemLabel}
                      mapping={mapping}
                      newItems={newItems}
                      renderSourceDetails={renderSourceDetails}
                      onSubmit={onUpdate}
                    />
                  </>
                )}
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}

function getUpdateTagText(type: MappingType, isMajorMapping: boolean): string {
  switch (type) {
    case 'ManualMapped':
      return isMajorMapping ? 'Major' : 'Minor';
    case 'ManualNone':
      return 'Major';
    default:
      return 'N/A';
  }
}
function getUpdateTagColour(
  type: MappingType,
  isMajorMapping: boolean,
): TagProps['colour'] {
  switch (type) {
    case 'ManualMapped':
      return isMajorMapping ? 'blue' : 'grey';
    case 'ManualNone':
      return 'blue';
    default:
      return 'red';
  }
}
