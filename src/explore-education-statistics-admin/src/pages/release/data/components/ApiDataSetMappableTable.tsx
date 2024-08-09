import {
  LocationCandidate,
  MappingType,
  PendingMappingUpdate,
} from '@admin/services/apiDataSetVersionService';
import {
  FilterCandidateWithKey,
  MappableFilter,
} from '@admin/pages/release/data/utils/getApiDataSetFilterMappings';
import ApiDataSetMappingModal from '@admin/pages/release/data/components/ApiDataSetMappingModal';
import {
  LocationCandidateWithKey,
  MappableLocation,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import ApiDataSetLocationCode from '@admin/pages/release/data/components/ApiDataSetLocationCode';
import Tag, { TagProps } from '@common/components/Tag';
import ButtonText from '@common/components/ButtonText';
import TagGroup from '@common/components/TagGroup';
import VisuallyHidden from '@common/components/VisuallyHidden';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React from 'react';
import classNames from 'classnames';
import omit from 'lodash/omit';

// Fields to omit from mapping diff.
const omittedDiffingFields: (keyof LocationCandidateWithKey)[] = [
  'key',
  'label',
];

interface Props {
  groupKey: string;
  groupLabel: string;
  label: string;
  mappableItems: MappableFilter[] | MappableLocation[];
  newItems?: FilterCandidateWithKey[] | LocationCandidateWithKey[];
  pendingUpdates?: PendingMappingUpdate[];
  showColumnName?: boolean;
  onUpdate: (update: PendingMappingUpdate) => Promise<void>;
}

export default function ApiDataSetMappableTable({
  groupKey,
  groupLabel,
  label,
  mappableItems,
  newItems = [],
  pendingUpdates = [],
  showColumnName = false,
  onUpdate,
}: Props) {
  const totalUnmapped = mappableItems.filter(
    option => option.mapping.type === 'AutoNone',
  ).length;
  const totalManuallyMapped = mappableItems.length - totalUnmapped;

  return (
    <table
      className="dfe-table-vertical-align-middle dfe-has-row-highlights"
      id={`mappable-${groupKey}`}
      data-testid={`mappable-table-${groupKey}`}
    >
      <caption className="govuk-!-margin-bottom-3 govuk-!-font-size-24">
        {groupLabel}{' '}
        <TagGroup className="govuk-!-margin-left-2">
          {totalUnmapped > 0 && (
            <Tag colour="red">
              {`${totalUnmapped} unmapped ${label}${
                totalUnmapped > 1 ? 's' : ''
              } `}
            </Tag>
          )}
          {totalManuallyMapped > 0 && (
            <Tag colour="blue">
              {`${totalManuallyMapped} mapped ${label}${
                totalManuallyMapped > 1 ? 's' : ''
              } `}
            </Tag>
          )}
        </TagGroup>
        <br />
        {showColumnName && (
          <div className="govuk-!-font-size-19 govuk-!-margin-top-4">
            Column:{' '}
            <span className="govuk-!-font-weight-regular">{groupKey}</span>
          </div>
        )}
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

          const isMajorMapping = candidate
            ? Object.entries(omit(mapping.source, omittedDiffingFields)).some(
                ([key, value]) =>
                  candidate[key as keyof LocationCandidate] !== value,
              )
            : true;

          return (
            <tr
              key={`mapping-${mapping.sourceKey}`}
              className={classNames({
                'rowHighlight--alert': mapping.type === 'AutoNone',
              })}
            >
              <td>
                {mapping.source.label}
                <ApiDataSetLocationCode location={mapping.source} />
              </td>
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
                  <>
                    {candidate.label}
                    <ApiDataSetLocationCode location={candidate} />
                  </>
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
                    text={`Updating ${label} mapping`}
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
                      groupKey={groupKey}
                      label={label}
                      mapping={mapping}
                      newItems={newItems}
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
