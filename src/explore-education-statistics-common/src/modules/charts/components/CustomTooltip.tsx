import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import getCategoryLabel from '@common/modules/charts/util/getCategoryLabel';
import formatPretty from '@common/utils/number/formatPretty';
import orderBy from 'lodash/orderBy';
import React, { useMemo } from 'react';
import { TooltipProps } from 'recharts';
import styles from './CustomTooltip.module.scss';

interface CustomTooltipProps extends TooltipProps {
  dataSetCategories: DataSetCategory[];
}

const CustomTooltip = ({
  payload,
  label,
  dataSetCategories,
}: CustomTooltipProps) => {
  const tooltipLabel = useMemo(() => {
    if (typeof label !== 'string') {
      return label;
    }

    return getCategoryLabel(dataSetCategories)(label);
  }, [dataSetCategories, label]);

  return (
    <div className={styles.tooltip}>
      <p className="govuk-!-font-weight-bold">{tooltipLabel}</p>

      {payload && (
        <ul className={styles.itemList}>
          {orderBy(payload, item => Number(item.value), ['desc']).map(
            (item, index) => {
              return (
                // eslint-disable-next-line react/no-array-index-key
                <li key={index}>
                  <div className="govuk-!-margin-right-2">
                    <span
                      className={styles.itemColour}
                      style={{ backgroundColor: item.fill }}
                    />
                  </div>

                  <div>
                    {`${item.name} : `}

                    <strong>
                      {formatPretty(item.value.toString(), item.unit)}
                    </strong>
                  </div>
                </li>
              );
            },
          )}
        </ul>
      )}
    </div>
  );
};

export default CustomTooltip;
