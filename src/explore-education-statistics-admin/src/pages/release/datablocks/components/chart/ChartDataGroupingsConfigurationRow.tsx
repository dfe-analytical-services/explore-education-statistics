import ChartDataGroupingForm from '@admin/pages/release/datablocks/components/chart/ChartDataGroupingForm';
import { MapDataGroupingConfig } from '@admin/pages/release/datablocks/components/chart/types/mapConfig';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import {
  dataGroupingTypes,
  MapConfig,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import isEqual from 'lodash/isEqual';
import React from 'react';

interface Props {
  dataSetConfig: MapDataSetConfig;
  label: string;
  map: MapConfig;
  meta: FullTableMeta;
  unit: string;
  onChange: (values: MapDataGroupingConfig) => void;
}

export default function ChartDataGroupingsConfigurationRow({
  dataSetConfig,
  label,
  map,
  meta,
  unit,
  onChange,
}: Props) {
  const [open, toggleOpen] = useToggle(false);

  return (
    <tr>
      <td>{label}</td>
      <td colSpan={dataSetConfig.categoricalDataConfig?.length ? 2 : 1}>
        {!dataSetConfig.categoricalDataConfig?.length ? (
          <>
            {dataSetConfig.dataGrouping.type === 'Custom'
              ? dataGroupingTypes[dataSetConfig.dataGrouping.type]
              : `${
                  dataSetConfig.dataGrouping.numberOfGroups
                } ${dataGroupingTypes[
                  dataSetConfig.dataGrouping.type
                ].toLowerCase()}`}
          </>
        ) : (
          'N/A for categorical data'
        )}
      </td>
      {!dataSetConfig.categoricalDataConfig?.length && (
        <td className="govuk-!-text-align-right">
          {!dataSetConfig.categoricalDataConfig?.length && (
            <Modal
              open={open}
              triggerButton={
                <ButtonText onClick={toggleOpen.on}>Edit</ButtonText>
              }
              title="Edit groupings"
              onExit={toggleOpen.off}
            >
              <ChartDataGroupingForm
                dataSetConfig={dataSetConfig}
                dataSetConfigs={map.dataSetConfigs}
                meta={meta}
                unit={unit}
                onCancel={toggleOpen.off}
                onSubmit={updatedConfig => {
                  onChange({
                    dataSetConfigs: map.dataSetConfigs.map(config => {
                      return isEqual(config.dataSet, updatedConfig.dataSet)
                        ? updatedConfig
                        : config;
                    }),
                  });

                  toggleOpen.off();
                }}
              />
            </Modal>
          )}
        </td>
      )}
    </tr>
  );
}
