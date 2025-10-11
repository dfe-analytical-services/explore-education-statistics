import styles from '@common/modules/charts/components/MapBlock.module.scss';
import { MapLegendItem } from '@common/modules/charts/types/chart';
import React from 'react';
import ColorNamer from 'color-namer';

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
              </dt>
              <span className={styles.srOnly}>
              map colour {rgbaToHex(colour)} ({hexToColourName(colour)})
              </span>
              <dd>{value}</dd>
          </div>
        ))}
      </dl>
    </>
  );
}

function hexToColourName(rgba: string): string | false {
  const name = ColorNamer(rgba, { pick: ['ntc', 'pantone'] });
  const pantone = name.pantone[0] ? name.pantone[0].name : false;
  const ntc = name.ntc[0] ? name.ntc[0].name : false;
  if (ntc) return ntc;
  if (pantone) return pantone;
  return false;
}

function componentToHex(c: number): string {
  const hex = c.toString(16);
  return hex.length === 1 ? `0${hex}` : hex;
}

function rgbaToHex(rgba: string): string {
  const match = rgba.match(
    /^rgba?\(\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})(?:\s*,\s*([\d.]+))?\s*\)$/,
  );
  if (!match) {
    throw new Error('Invalid RGBA color format');
  }

  const [, r, g, b, a] = match;
  const rParsed = parseInt(r, 10);
  const gParsed = parseInt(g, 10);
  const bParsed = parseInt(b, 10);

  const clamp = (n: number, min: number, max: number) =>
    Math.min(max, Math.max(min, n));

  const red = clamp(Math.round(rParsed), 0, 255);
  const green = clamp(Math.round(gParsed), 0, 255);
  const blue = clamp(Math.round(bParsed), 0, 255);

  let alphaHex = '';

  if (a !== undefined) {
    const alpha = clamp(parseFloat(a), 0, 1);
    alphaHex = componentToHex(Math.round(alpha * 255));
  }

  return `#${componentToHex(red)}${componentToHex(green)}${componentToHex(
    blue,
  )}${alphaHex}`;
}
