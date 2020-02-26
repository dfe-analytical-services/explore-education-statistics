import React from 'react';
import { TooltipProps } from 'recharts';

const CustomTooltip = ({ active, payload, label }: TooltipProps) => {
  if (active) {
    return (
      <div className="graph-tooltip">
        <p className="govuk-!-font-weight-bold">{label}</p>
        {payload &&
          payload
            .sort((a, b) => {
              if (typeof b.value === 'number' && typeof a.value === 'number') {
                return b.value - a.value;
              }

              return 0;
            })
            .map((_, index) => {
              return (
                // eslint-disable-next-line react/no-array-index-key
                <p key={index}>
                  {payload[index].name} :{' '}
                  <strong> {payload[index].value}</strong>
                </p>
              );
            })}
      </div>
    );
  }

  return null;
};

export default CustomTooltip;
