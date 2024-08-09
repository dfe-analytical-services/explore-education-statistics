import ChangeList from '@common/modules/data-catalogue/components/ChangeList';
import { ChangeSet } from '@common/services/types/apiDataSetChanges';
import naturalOrderBy from '@common/utils/array/naturalOrderBy';
import orderBy from 'lodash/orderBy';
import React from 'react';

interface Props {
  changes: ChangeSet;
}

export default function ChangeSection({ changes }: Props) {
  const filters = naturalOrderBy(
    changes.filters ?? [],
    filter => filter.previousState?.label ?? filter.currentState?.label ?? '',
  );

  const filterOptions = naturalOrderBy(
    changes.filterOptions ?? [],
    group => group.filter.label,
  );

  const geographicLevels = orderBy(
    changes.geographicLevels ?? [],
    level => level.previousState?.label ?? level.currentState?.label,
  );

  const indicators = naturalOrderBy(
    changes.indicators ?? [],
    indicator =>
      indicator.previousState?.label ?? indicator.currentState?.label ?? '',
  );

  const locationGroups = orderBy(
    changes.locationGroups ?? [],
    group =>
      group.previousState?.level.label ?? group.currentState?.level.label,
  );

  const locationOptions = orderBy(
    changes.locationOptions ?? [],
    group => group.level.label,
  );

  const timePeriods = naturalOrderBy(
    changes.timePeriods ?? [],
    timePeriod =>
      timePeriod.previousState?.label ?? timePeriod.currentState?.label ?? '',
  );

  return (
    <>
      <ChangeList changes={filters} metaType="filters" />

      {filterOptions.map(group => {
        return (
          <ChangeList
            key={group.filter.id}
            changes={orderBy(
              group.options,
              option =>
                option.previousState?.label ?? option?.currentState?.label,
            )}
            metaType="filterOptions"
            metaTypeLabel={`${group.filter.label} filter options`}
            testId={`filterOptions-${group.filter.id}`}
          />
        );
      })}

      <ChangeList changes={geographicLevels} metaType="geographicLevels" />

      <ChangeList changes={indicators} metaType="indicators" />

      <ChangeList changes={locationGroups} metaType="locationGroups" />

      {locationOptions.map(group => {
        return (
          <ChangeList
            key={group.level.code}
            changes={orderBy(
              group.options,
              option =>
                option.previousState?.label ?? option?.currentState?.label,
            )}
            metaType="locationOptions"
            metaTypeLabel={`${group.level.label} location options`}
            testId={`locationOptions-${group.level.code}`}
          />
        );
      })}

      <ChangeList changes={timePeriods} metaType="timePeriods" />
    </>
  );
}
