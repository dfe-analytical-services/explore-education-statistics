import formatPretty from '@common/utils/number/formatPretty';
import orderBy from 'lodash/orderBy';
import React from 'react';
import { TooltipProps } from 'recharts';
import styles from './CustomTooltip.module.scss';

const CustomTooltip = ({ payload, label }: TooltipProps) => {
  return (
    <div className={styles.tooltip}>
      <p className="govuk-!-font-weight-bold">{label}</p>

      {payload && (
        <ul className={styles.itemList}>
          {orderBy(payload, 'value').map((item, index) => {
            return (
              // eslint-disable-next-line react/no-array-index-key
              <li key={index}>
                <span
                  className={styles.itemColour}
                  style={{ backgroundColor: item.fill }}
                />

                <span>
                  {`${item.name} : `}

                  <strong>
                    {formatPretty(item.value.toString(), item.unit)}
                  </strong>
                </span>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
};

export default CustomTooltip;
