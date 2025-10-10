import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import getCategoryLabel from '@common/modules/charts/util/getCategoryLabel';
import { DataSetCategoryConfig } from '@common/modules/charts/util/getDataSetCategoryConfigs';
import formatPretty from '@common/utils/number/formatPretty';
import keyBy from 'lodash/keyBy';
import React, { useMemo } from 'react';
import {
  NameType,
  ValueType,
} from 'recharts/types/component/DefaultTooltipContent';
import { TooltipContentProps } from 'recharts/types/component/Tooltip';
import orderBy from 'lodash/orderBy';
import styles from './CustomTooltip.module.scss';

interface CustomTooltipProps extends TooltipContentProps<ValueType, NameType> {
  dataSetCategories: DataSetCategory[];
  dataSetCategoryConfigs: DataSetCategoryConfig[];
  order?: 'default' | 'reverse' | 'value';
}

const CustomTooltip = ({
  payload = [],
  label,
  dataSetCategories,
  dataSetCategoryConfigs,
  order = 'default',
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

  const getPayloadOrder = () => {
    if (order === 'value') {
      return orderBy(payload, item => Number(item.value), ['desc']);
    }
    return order === 'reverse' ? [...payload]?.reverse() : payload;
  };

  return (
    <div
      className={styles.tooltip}
      aria-live="assertive"
      data-testid="chartTooltip"
      role="status"
    >
      <p className="govuk-!-font-weight-bold" data-testid="chartTooltip-label">
        {tooltipLabel}
      </p>

      {payload && !!payload.length && (
        <ul className={styles.itemList} data-testid="chartTooltip-items">
          {getPayloadOrder()?.map((item, index) => {
            const dataKey =
              typeof item.dataKey === 'string' ? item.dataKey : '';

            return (
              // eslint-disable-next-line react/no-array-index-key
              <li key={index}>
                <div className="govuk-!-margin-right-2">
                  <span
                    className={styles.itemColour}
                    style={{ backgroundColor: item.color }}
                  />
                </div>

                <div data-testid="chartTooltip-item-text">
                  {`${item.name}: `}

                  {typeof item.value !== 'undefined' && (
                    <strong>
                      {formatPretty(
                        item.value.toString(),
                        typeof item.unit === 'string' ? item.unit : '',
                        configsByDataKey[dataKey]?.dataSet.indicator
                          .decimalPlaces,
                      )}
                    </strong>
                  )}
                </div>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
};

export default CustomTooltip;
