import ChangeList from '@common/modules/data-catalogue/components/ChangeList';
import { ChangeSet } from '@common/services/types/apiDataSetChanges';
import React from 'react';

interface Props {
  majorChanges: ChangeSet;
  minorChanges: ChangeSet;
}

export default function ApiDataSetChangelog({
  majorChanges,
  minorChanges,
}: Props) {
  return (
    <>
      {Object.keys(majorChanges).length > 0 && (
        <>
          <h3>Major changes</h3>

          <ChangeSection {...majorChanges} />
        </>
      )}

      {Object.keys(minorChanges).length > 0 && (
        <>
          <h3>Minor changes</h3>

          <p>These are minor changes</p>

          <ChangeSection {...minorChanges} />
        </>
      )}
    </>
  );
}

function ChangeSection(changes: ChangeSet) {
  return (
    <section>
      <ChangeList changes={changes?.filters} metaType="filters" />

      {changes?.filterOptions?.map(group => {
        return (
          <ChangeList
            key={group.filter.id}
            changes={group.options}
            metaType="filterOptions"
            metaTypeLabel={`'${group.filter.label}' filter options`}
          />
        );
      })}

      <ChangeList
        changes={changes?.geographicLevels}
        metaType="geographicLevels"
      />

      <ChangeList changes={changes?.indicators} metaType="indicators" />

      <ChangeList changes={changes?.locationGroups} metaType="locationGroups" />

      {changes?.locationOptions?.map(group => {
        return (
          <ChangeList
            key={group.level.code}
            changes={group.options}
            metaType="locationOptions"
            metaTypeLabel={`${group.level.label} location options`}
          />
        );
      })}

      <ChangeList changes={changes?.timePeriods} metaType="timePeriods" />
    </section>
  );
}
