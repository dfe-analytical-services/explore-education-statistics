import ChangeList from '@common/modules/data-catalogue/components/ChangeList';
import { ChangeSet } from '@common/services/types/apiDataSetChanges';
import React from 'react';

interface Props {
  changes: ChangeSet;
}

export default function ChangeSection({ changes }: Props) {
  return (
    <>
      <ChangeList changes={changes.filters} metaType="filters" />

      {changes.filterOptions?.map(group => {
        return (
          <ChangeList
            key={group.filter.id}
            changes={group.options}
            metaType="filterOptions"
            metaTypeLabel={`${group.filter.label} filter options`}
            testId={`filterOptions-${group.filter.id}`}
          />
        );
      })}

      <ChangeList
        changes={changes.geographicLevels}
        metaType="geographicLevels"
      />

      <ChangeList changes={changes.indicators} metaType="indicators" />

      <ChangeList changes={changes.locationGroups} metaType="locationGroups" />

      {changes.locationOptions?.map(group => {
        return (
          <ChangeList
            key={group.level.code}
            changes={group.options}
            metaType="locationOptions"
            metaTypeLabel={`${group.level.label} location options`}
            testId={`locationOptions-${group.level.code}`}
          />
        );
      })}

      <ChangeList changes={changes.timePeriods} metaType="timePeriods" />
    </>
  );
}
