import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import getCategoryLabel from '@common/modules/charts/util/getCategoryLabel';
import { DataSetCategoryConfig } from '@common/modules/charts/util/getDataSetCategoryConfigs';
import formatPretty from '@common/utils/number/formatPretty';
import keyBy from 'lodash/keyBy';
import orderBy from 'lodash/orderBy';
import React, { useMemo } from 'react';
import { TooltipProps } from 'recharts';
import styles from './CustomTooltip.module.scss';

interface CustomTooltipProps extends TooltipProps {
  dataSetCategories: DataSetCategory[];
  dataSetCategoryConfigs: DataSetCategoryConfig[];
}

const CustomTooltip = ({
  payload,
  label,
  dataSetCategories,
  dataSetCategoryConfigs,
}: CustomTooltipProps) => {
  const tooltipLabel = useMemo(() => {
    if (typeof label !== 'string') {
      return label;
    }

    return getCategoryLabel(dataSetCategories)(label);
  }, [dataSetCategories, label]);

  const configsByDataKey = useMemo(
    () => keyBy(dataSetCategoryConfigs, config => config.dataKey),
    [dataSetCategoryConfigs],
  );

  return (
    <div className={styles.tooltip} data-testid="chartTooltip">
      <p className="govuk-!-font-weight-bold" data-testid="chartTooltip-label">
        {tooltipLabel}
      </p>

      {payload && (
        <ul className={styles.itemList} data-testid="chartTooltip-items">
          {orderBy(payload, item => Number(item.value), ['desc']).map(
            (item, index) => {
              const dataKey =
                typeof item.dataKey === 'string' ? item.dataKey : '';

              return (
                // eslint-disable-next-line react/no-array-index-key
                <li key={index}>
                  <div className="govuk-!-margin-right-2">
                    <span
                      className={styles.itemColour}
                      style={{ backgroundColor: item.fill }}
                    />
                  </div>

                  <div data-testid="chartTooltip-item-text">
                    {`${item.name}: `}

                    <strong>
                      {formatPretty(
                        item.value.toString(),
                        item.unit,
                        configsByDataKey[dataKey]?.dataSet.indicator
                          .decimalPlaces,
                      )}
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
