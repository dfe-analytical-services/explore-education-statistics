import { SelectOption } from '@common/components/form/FormSelect';
import styles from '@common/modules/charts/components/MapBlock.module.scss';
import createMapDataSetCategories, {
  MapDataSetCategory,
} from '@common/modules/charts/components/utils/createMapDataSetCategories';
import { LegendDataGroup } from '@common/modules/charts/components/utils/generateLegendDataGroups';
import MapGeoJSON from '@common/modules/charts/components/MapGeoJSON';
import MapControls from '@common/modules/charts/components/MapControls';
import MapLegend from '@common/modules/charts/components/MapLegend';
import MapSelectedItem from '@common/modules/charts/components/MapSelectedItem';
import generateFeaturesAndDataGroups from '@common/modules/charts/components/utils/generateFeaturesAndDataGroups';
import {
  AxisConfiguration,
  ChartDefinition,
  ChartProps,
  DataGroupingType,
  MapConfig,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import getDataSetCategoryConfigs, {
  DataSetCategoryConfig,
} from '@common/modules/charts/util/getDataSetCategoryConfigs';
import { GeoJsonFeatureProperties } from '@common/services/tableBuilderService';

import { Dictionary } from '@common/types';
import classNames from 'classnames';
import { Feature, FeatureCollection, Geometry } from 'geojson';
import { Layer, Path, Polyline } from 'leaflet';
import keyBy from 'lodash/keyBy';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useMemo, useState } from 'react';
import { MapContainer } from 'react-leaflet';

export interface MapFeatureProperties extends GeoJsonFeatureProperties {
  colour: string;
  data: number;
  dataSets: DataSetCategory['dataSets'];
  layer?: Layer & Path & Polyline;
}

export type MapFeature = Feature<Geometry, MapFeatureProperties>;

export type MapFeatureCollection = FeatureCollection<
  Geometry,
  MapFeatureProperties
>;

export interface MapBlockProps extends ChartProps {
  axes: {
    major: AxisConfiguration;
  };
  // dataGroups & dataClassification to be removed when
  // migrate old maps to use the new config
  // https://dfedigital.atlassian.net/browse/EES-4271
  dataGroups?: number;
  dataClassification?: DataGroupingType;
  id: string;
  legend: LegendConfiguration;
  map?: MapConfig;
  position?: { lat: number; lng: number };
}

export const mapBlockDefinition: ChartDefinition = {
  type: 'map',
  name: 'Geographic',
  capabilities: {
    canPositionLegendInline: false,
    canSize: true,
    canSort: false,
    hasGridLines: false,
    hasLegend: true,
    hasLegendPosition: false,
    hasLineStyle: false,
    hasReferenceLines: false,
    hasSymbols: false,
    requiresGeoJson: true,
    stackable: false,
  },
  options: {
    defaults: {
      height: 600,
    },
  },
  legend: {
    defaults: {},
  },
  axes: {
    major: {
      id: 'geojson',
      title: 'GeoJSON (major axis)',
      type: 'major',
      hide: true,
      capabilities: {
        canRotateLabel: false,
      },
      defaults: {
        groupBy: 'locations',
      },
      constants: {
        groupBy: 'locations',
      },
    },
  },
};

export default function MapBlockInternal({
  id,
  dataGroups: deprecatedDataGroups,
  dataClassification: deprecatedDataClassification,
  data,
  map,
  meta,
  legend,
  position = { lat: 53.00986, lng: -3.2524038 },
  width,
  height,
  axes,
}: MapBlockProps) {
  const axisMajor = useMemo<AxisConfiguration>(
    () => ({
      ...axes.major,
      // Enforce grouping by locations
      groupBy: 'locations',
    }),
    [axes.major],
  );

  const dataSetCategories = useMemo<MapDataSetCategory[]>(
    () => createMapDataSetCategories(axisMajor, data, meta),
    [axisMajor, data, meta],
  );

  const dataSetCategoryConfigs = useMemo<Dictionary<DataSetCategoryConfig>>(
    () =>
      keyBy(
        getDataSetCategoryConfigs({
          dataSetCategories,
          legendItems: legend.items,
          meta,
          dataSetConfigs: map?.dataSetConfigs,
          deprecatedDataGroups,
          deprecatedDataClassification,
        }),
        dataSetConfig => dataSetConfig.dataKey,
      ),
    [
      dataSetCategories,
      map?.dataSetConfigs,
      legend,
      meta,
      deprecatedDataGroups,
      deprecatedDataClassification,
    ],
  );

  const dataSetOptions = useMemo<SelectOption[]>(() => {
    return orderBy(
      Object.values(dataSetCategoryConfigs).map(dataSet => ({
        label: dataSet.config.label,
        value: dataSet.dataKey,
      })),
      ['label'],
    );
  }, [dataSetCategoryConfigs]);

  const [selectedDataSetKey, setSelectedDataSetKey] = useState<string>(
    (dataSetOptions[0]?.value as string) ?? '',
  );
  const [selectedFeature, setSelectedFeature] = useState<MapFeature>();

  const [features, setFeatures] = useState<MapFeatureCollection>();

  const [legendDataGroups, setLegendDataGroups] = useState<LegendDataGroup[]>(
    [],
  );

  const selectedDataSetConfig = dataSetCategoryConfigs[selectedDataSetKey];

  const selectedDataSet =
    selectedFeature?.properties?.dataSets[selectedDataSetKey];

  // Rebuild the geometry if the selection has changed
  useEffect(() => {
    if (dataSetCategories.length && selectedDataSetConfig) {
      const { features: newFeatures, dataGroups: newDataGroups } =
        generateFeaturesAndDataGroups({
          selectedDataSetConfig,
          dataSetCategories,
        });

      setFeatures(newFeatures);
      setLegendDataGroups(newDataGroups);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    dataSetCategories,
    dataSetCategoryConfigs,
    meta,
    selectedDataSetConfig,
    selectedDataSetKey,
  ]);

  if (
    data === undefined ||
    axisMajor === undefined ||
    axisMajor.dataSets === undefined
  ) {
    return <div>Unable to render map, map incorrectly configured</div>;
  }

  return (
    <>
      <MapControls
        dataSetCategories={dataSetCategories}
        dataSetOptions={dataSetOptions}
        id={id}
        selectedDataSetKey={selectedDataSetKey}
        selectedLocation={selectedFeature?.id?.toString()}
        onChangeDataSet={setSelectedDataSetKey}
        onChangeLocation={value => {
          const feature = features?.features.find(feat => feat.id === value);
          return feature
            ? setSelectedFeature(feature)
            : setSelectedFeature(undefined);
        }}
      />

      <div className="govuk-grid-row govuk-!-margin-bottom-4">
        <div className="govuk-grid-column-two-thirds">
          <MapContainer
            style={{
              width: (width && `${width}px`) || '100%',
              height: `${height || 600}px`,
            }}
            className={classNames(styles.map, 'dfe-print-break-avoid')}
            center={position}
            minZoom={5}
            zoom={5}
          >
            <MapGeoJSON
              dataSetCategoryConfigs={dataSetCategoryConfigs}
              features={features}
              width={width}
              height={height}
              selectedDataSetKey={selectedDataSetKey}
              selectedFeature={selectedFeature}
              onSelectFeature={setSelectedFeature}
            />
          </MapContainer>
        </div>
        {selectedDataSetConfig && (
          <div className="govuk-grid-column-one-third">
            <MapLegend
              heading={selectedDataSetConfig?.config?.label}
              legendDataGroups={legendDataGroups}
            />
            <MapSelectedItem
              decimalPlaces={
                selectedDataSetConfig.dataSet.indicator.decimalPlaces
              }
              heading={selectedFeature?.properties.name}
              title={selectedDataSetConfig.config.label}
              unit={selectedDataSetConfig.dataSet.indicator.unit}
              value={selectedDataSet?.value}
            />
          </div>
        )}
      </div>
    </>
  );
}
