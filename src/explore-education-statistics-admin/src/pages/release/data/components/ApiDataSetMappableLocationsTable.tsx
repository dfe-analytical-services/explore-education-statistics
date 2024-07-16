import {
  MappableLocation,
  LocationCandidateWithKey,
} from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import ApiDataSetLocationMappingModal from '@admin/pages/release/data/components/ApiDataSetLocationMappingModal';
import styles from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage.module.scss';
import { PendingLocationMappingUpdate } from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage';
import getApiDataSetLocationCodes from '@admin/pages/release/data/utils/getApiDataSetLocationCodes';
import { MappingType } from '@admin/services/apiDataSetVersionService';
import Tag, { TagProps } from '@common/components/Tag';
import ButtonText from '@common/components/ButtonText';
import TagGroup from '@common/components/TagGroup';
import VisuallyHidden from '@common/components/VisuallyHidden';
import LoadingSpinner from '@common/components/LoadingSpinner';
import locationLevelsMap, {
  LocationLevelKey,
} from '@common/utils/locationLevelsMap';
import React from 'react';
import classNames from 'classnames';

interface Props {
  level: LocationLevelKey;
  locations: MappableLocation[];
  newLocations?: LocationCandidateWithKey[];
  pendingUpdates?: PendingLocationMappingUpdate[];
  onUpdate: (update: PendingLocationMappingUpdate) => Promise<void>;
}

export default function ApiDataSetMappableLocationsTable({
  level,
  locations,
  newLocations = [],
  pendingUpdates = [],
  onUpdate,
}: Props) {
  const totalUnmapped = locations.filter(
    location => location.mapping.type === 'AutoNone',
  ).length;
  const totalManuallyMapped = locations.length - totalUnmapped;

  return (
    <table
      className={`${styles.table} hasRowHighlights`}
      id={`mappable-${level}`}
      data-testid={`mappable-table-${level}`}
    >
      <caption className="govuk-!-margin-bottom-3 govuk-!-font-size-24">
        {locationLevelsMap[level]?.plural ?? level}{' '}
        <TagGroup className="govuk-!-margin-left-2">
          {totalUnmapped > 0 && (
            <Tag colour="red">
              {`${totalUnmapped} unmapped location${
                totalUnmapped > 1 ? 's' : ''
              } `}
            </Tag>
          )}
          {totalManuallyMapped > 0 && (
            <Tag colour="blue">
              {`${totalManuallyMapped} mapped location${
                totalManuallyMapped > 1 ? 's' : ''
              } `}
            </Tag>
          )}
        </TagGroup>
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
        {Object.values(locations).map(({ candidate, mapping }) => {
          const isPendingUpdate = pendingUpdates.some(
            update => update.sourceKey === mapping.sourceKey,
          );
          return (
            <tr
              key={`mapping-${mapping.sourceKey}`}
              className={classNames({
                'rowHighlight--alert': mapping.type === 'AutoNone',
              })}
            >
              <td>
                {mapping.source.label}
                <br />
                <span className={styles.code}>
                  {getApiDataSetLocationCodes(mapping.source)}
                </span>
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
                    <br />
                    <span className={styles.code}>
                      {getApiDataSetLocationCodes(candidate)}
                    </span>
                  </>
                )}
              </td>
              <td>
                <Tag colour={getUpdateTagColour(mapping.type)}>
                  {getUpdateTagText(mapping.type)}
                </Tag>
              </td>
              <td className="govuk-!-text-align-right">
                {isPendingUpdate ? (
                  <LoadingSpinner
                    alert
                    hideText
                    inline
                    size="sm"
                    text="Updating location mapping"
                  />
                ) : (
                  <>
                    {mapping.type !== 'ManualNone' && (
                      <ButtonText
                        onClick={async () => {
                          await onUpdate({
                            level,
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
                    <ApiDataSetLocationMappingModal
                      candidate={candidate}
                      level={level}
                      mapping={mapping}
                      newLocations={newLocations}
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

function getUpdateTagText(type: MappingType): string {
  switch (type) {
    case 'ManualMapped':
      return 'Minor';
    case 'ManualNone':
      return 'Major';
    default:
      return 'N/A';
  }
}

function getUpdateTagColour(type: MappingType): TagProps['colour'] {
  switch (type) {
    case 'ManualMapped':
      return 'grey';
    case 'ManualNone':
      return 'blue';
    default:
      return 'red';
  }
}
