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
      <td>
        {dataSetConfig.dataGrouping.type === 'Custom'
          ? dataGroupingTypes[dataSetConfig.dataGrouping.type]
          : `${dataSetConfig.dataGrouping.numberOfGroups} ${dataGroupingTypes[
              dataSetConfig.dataGrouping.type
            ].toLowerCase()}`}
      </td>
      <td className="govuk-!-text-align-right">
        <Modal
          open={open}
          triggerButton={<ButtonText onClick={toggleOpen.on}>Edit</ButtonText>}
          title="Edit groupings"
          onExit={toggleOpen.off}
        >
          <ChartDataGroupingForm
            dataSetConfig={dataSetConfig}
            dataSetConfigs={map.dataSetConfigs}
            meta={meta}
            unit={unit}
            onCancel={toggleOpen.off}
            onSubmit={values => {
              onChange({
                dataSetConfigs: map.dataSetConfigs.map(config => {
                  return isEqual(config.dataSet, values.dataSet)
                    ? values
                    : config;
                }),
              });

              toggleOpen.off();
            }}
          />
        </Modal>
      </td>
    </tr>
  );
}
