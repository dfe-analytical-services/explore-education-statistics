import styles from '@common/modules/charts/components/MapBlock.module.scss';
import { MapLegendItem } from '@common/modules/charts/types/chart';
import React from 'react';

interface Props {
  heading: string;
  legendItems: MapLegendItem[];
}

export default function MapLegend({ heading, legendItems }: Props) {
  return (
    <>
      <h3 className="govuk-heading-s dfe-word-break--break-word">
        Key to {heading}
      </h3>
      <ul className="govuk-list" data-testid="mapBlock-legend">
        {legendItems.map(({ value, colour }) => (
          <li
            key={value}
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
            {value}
          </li>
        ))}
      </ul>
    </>
  );
}
