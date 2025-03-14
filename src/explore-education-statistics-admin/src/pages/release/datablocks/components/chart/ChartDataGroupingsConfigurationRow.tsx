import ChartDataGroupingForm from '@admin/pages/release/datablocks/components/chart/ChartDataGroupingForm';
import { MapDataGroupingConfig } from '@admin/pages/release/datablocks/components/chart/types/mapConfig';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import {
  dataGroupingTypes,
  MapConfig,
  MapDataSetConfig,
} from '@common/modules/charts/types/chart';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import isEqual from 'lodash/isEqual';
import React, { useState } from 'react';

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
  const [editDataSetConfig, setEditDataSetConfig] = useState<{
    dataSetConfig: MapDataSetConfig;
    unit: string;
  }>();

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
          open={!!editDataSetConfig}
          triggerButton={
            <ButtonText
              onClick={() => setEditDataSetConfig({ dataSetConfig, unit })}
            >
              Edit
            </ButtonText>
          }
          title="Edit groupings"
          onExit={() => setEditDataSetConfig(undefined)}
        >
          {editDataSetConfig && (
            <ChartDataGroupingForm
              dataSetConfig={editDataSetConfig.dataSetConfig}
              dataSetConfigs={map.dataSetConfigs}
              meta={meta}
              unit={editDataSetConfig.unit}
              onCancel={() => setEditDataSetConfig(undefined)}
              onSubmit={values => {
                onChange({
                  dataSetConfigs: map.dataSetConfigs.map(config => {
                    return isEqual(config.dataSet, values.dataSet)
                      ? values
                      : config;
                  }),
                });

                setEditDataSetConfig(undefined);
              }}
            />
          )}
        </Modal>
      </td>
    </tr>
  );
}
