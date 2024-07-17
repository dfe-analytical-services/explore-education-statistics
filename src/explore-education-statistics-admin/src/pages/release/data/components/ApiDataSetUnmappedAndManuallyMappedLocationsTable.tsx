import { UnmappedAndManuallyMappedLocation } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import styles from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage.module.scss';
import { MappingType } from '@admin/services/apiDataSetVersionService';
import Tag, { TagProps } from '@common/components/Tag';
import ButtonText from '@common/components/ButtonText';
import ButtonGroup from '@common/components/ButtonGroup';
import TagGroup from '@common/components/TagGroup';
import VisuallyHidden from '@common/components/VisuallyHidden';
import locationLevelsMap from '@common/utils/locationLevelsMap';
import React from 'react';
import camelCase from 'lodash/camelCase';
import classNames from 'classnames';

interface Props {
  level: string;
  locations: UnmappedAndManuallyMappedLocation[];
}

export default function ApiDataSetUnmappedAndManuallyMappedLocationsTable({
  level,
  locations,
}: Props) {
  const totalUnmapped = locations.filter(
    location => location.mapping.type === 'AutoNone',
  ).length;
  const totalManuallyMapped = locations.length - totalUnmapped;
  return (
    <table
      className={`${styles.table} hasRowHighlights`}
      id={`unmapped-${level}`}
      data-testid={`unmapped-${level}`}
    >
      <caption className="govuk-!-margin-bottom-3 govuk-!-font-size-24">
        {/* TODO remove camelCase when levels are camelCase in BE */}
        {locationLevelsMap[camelCase(level)]?.plural ?? level}{' '}
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
          <th>Actions</th>
          <th>Update</th>
        </tr>
      </thead>
      <tbody>
        {Object.values(locations).map(({ candidate, mapping }) => {
          return (
            <tr
              key={`mapping-${mapping.source.code}`}
              className={classNames({
                'rowHighlight--alert': mapping.type === 'AutoNone',
              })}
            >
              <td>
                {mapping.source.label}
                <br />
                <span className={styles.code}>{mapping.source.code}</span>
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
                    <span className={styles.code}>{candidate.code}</span>
                  </>
                )}
              </td>
              <td>
                <ButtonGroup className="govuk-!-margin-bottom-0">
                  {mapping.type !== 'ManualNone' && (
                    <ButtonText>
                      No mapping
                      <VisuallyHidden>
                        {' '}
                        for {mapping.source.label}
                      </VisuallyHidden>
                    </ButtonText>
                  )}
                  <ButtonText>
                    Edit
                    <VisuallyHidden>
                      {' '}
                      mapping for {mapping.source.label}
                    </VisuallyHidden>
                  </ButtonText>
                </ButtonGroup>
              </td>
              <td>
                <Tag colour={getUpdateTagColour(mapping.type)}>
                  {getUpdateTagText(mapping.type)}
                </Tag>
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
