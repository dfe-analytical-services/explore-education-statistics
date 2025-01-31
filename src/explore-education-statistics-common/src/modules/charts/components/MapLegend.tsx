import styles from '@common/modules/charts/components/MapBlock.module.scss';
import { LegendDataGroup } from '@common/modules/charts/components/utils/generateLegendDataGroups';
import React from 'react';

interface Props {
  heading: string;
  legendDataGroups: LegendDataGroup[];
}

export default function MapLegend({ heading, legendDataGroups }: Props) {
  return (
    <>
      <h3 className="govuk-heading-s dfe-word-break--break-word">
        Key to {heading}
      </h3>
      <ul className="govuk-list" data-testid="mapBlock-legend">
        {legendDataGroups.map(({ min, max, colour }) => (
          <li
            key={`${min}-${max}-${colour}`}
            className={styles.legend}
            data-testid="mapBlock-legend-item"
          >
            <span
              className={styles.legendIcon}
              data-testid="mapBlock-legend-colour"
              style={{
                backgroundColor: colour,
              }}
            />
            {`${min} to ${max}`}
          </li>
        ))}
      </ul>
    </>
  );
}
