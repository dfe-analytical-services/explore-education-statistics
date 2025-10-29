import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import formatPretty from '@common/utils/number/formatPretty';
import React from 'react';

interface Props {
  decimalPlaces?: number;
  heading?: string;
  title: string;
  unit: string;
  value?: number | string;
}

export default function MapSelectedItem({
  decimalPlaces,
  heading,
  title,
  unit,
  value,
}: Props) {
  return (
    <div
      aria-live="polite"
      className="govuk-!-margin-top-5"
      data-testid="mapBlock-indicator"
    >
      {heading && value && (
        <>
          <h3 className="govuk-heading-s">{heading}</h3>

          <KeyStatTile
            testId="mapBlock-indicatorTile"
            title={title}
            value={formatPretty(value, unit, decimalPlaces)}
          />
        </>
      )}
    </div>
  );
}
