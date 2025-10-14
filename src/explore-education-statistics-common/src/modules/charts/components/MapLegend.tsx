import styles from '@common/modules/charts/components/MapBlock.module.scss';
import { MapLegendItem } from '@common/modules/charts/types/chart';
import React from 'react';
import ColorNamer from 'color-namer';
import VisuallyHidden from '@common/components/VisuallyHidden';

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
      <dl className="govuk-list" data-testid="mapBlock-legend">
        {legendItems.map(({ value, colour }) => (
          <div
            key={value}
            className={styles.legend}
            data-testid="mapBlock-legend-item"
          >
            <dt>
              <div
                className={styles.legendIcon}
                data-testid="mapBlock-legend-colour"
                style={{
                  backgroundColor: colour,
                }}
              />
              <VisuallyHidden>
                map colour {colour} ({rgbaToColourName(colour)})
              </VisuallyHidden>
            </dt>
            <dd>{value}</dd>
          </div>
        ))}
      </dl>
    </>
  );
}

function rgbaToColourName(rgba: string): string | false {
  const names = ColorNamer(rgba, { pick: ['ntc', 'pantone'] });
  return names.ntc[0]?.name || names.pantone[0]?.name;
}
