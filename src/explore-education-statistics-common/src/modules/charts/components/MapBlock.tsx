import { SelectOption } from '@common/components/form/FormSelect';
import styles from '@common/modules/charts/components/MapBlock.module.scss';
import MapControls from '@common/modules/charts/components/MapControls';
import MapGeoJSON from '@common/modules/charts/components/MapGeoJSON';
import MapLegend from '@common/modules/charts/components/MapLegend';
import MapSelectedItem from '@common/modules/charts/components/MapSelectedItem';
import createMapDataSetCategories, {
  MapDataSetCategory,
} from '@common/modules/charts/components/utils/createMapDataSetCategories';
import generateFeaturesAndDataGroups from '@common/modules/charts/components/utils/generateFeaturesAndDataGroups';
import { LegendDataGroup } from '@common/modules/charts/components/utils/generateLegendDataGroups';
import {
  AxisConfiguration,
  ChartDefinition,
  ChartProps,
  DataGroupingType,
  MapConfig,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import getMapDataSetCategoryConfigs, {
  MapDataSetCategoryConfig,
} from '@common/modules/charts/util/getMapDataSetCategoryConfigs';
import { GeoJsonFeatureProperties } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import { Feature, FeatureCollection, Geometry } from 'geojson';
import { Layer, Path, Polyline } from 'leaflet';
import keyBy from 'lodash/keyBy';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { MapContainer } from 'react-leaflet';
import useToggle from '@common/hooks/useToggle';
import LoadingSpinner from '@common/components/LoadingSpinner';
import generateDataSetKey from '../util/generateDataSetKey';

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
  boundaryLevel: number;
  onBoundaryLevelChange: (boundaryLevel: number) => Promise<void>;
}

export const mapBlockDefinition: ChartDefinition = {
  type: 'map',
  name: 'Geographic',
  capabilities: {
    canIncludeNonNumericData: false,
    canPositionLegendInline: false,
    canSetBarThickness: false,
    canSetDataLabelPosition: false,
    canShowDataLabels: false,
    canShowAllMajorAxisTicks: false,
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

export default function MapBlock({
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
  title,
  boundaryLevel,
  onBoundaryLevelChange,
}: MapBlockProps) {
  const [isBoundaryLevelChanging, toggleBoundaryLevelChanging] =
    useToggle(false);

  const axisMajor = useMemo<AxisConfiguration>(
    () => ({
      ...axes.major,
      // Enforce grouping by locations
      groupBy: 'locations',
    }),
    [axes.major],
  );

  const dataSetCategories = useMemo<MapDataSetCategory[]>(() => {
    return createMapDataSetCategories(axisMajor, data, meta);
  }, [axisMajor, data, meta]);

  const dataSetCategoryConfigs = useMemo<Dictionary<MapDataSetCategoryConfig>>(
    () =>
      keyBy(
        getMapDataSetCategoryConfigs({
          dataSetCategories,
          dataSetConfigs: map?.dataSetConfigs,
          legendItems: legend.items,
          meta,
          deprecatedDataGroups,
          deprecatedDataClassification,
        }),
        dataSetConfig => dataSetConfig.dataKey,
      ),
    [
      dataSetCategories,
      legend.items,
      meta,
      map,
      deprecatedDataGroups,
      deprecatedDataClassification,
    ],
  );

  const dataSetOptions = useMemo<SelectOption[]>(() => {
    const dataSetKeys = legend.items.map(({ dataSet }) =>
      generateDataSetKey(dataSet),
    );
    return dataSetKeys
      .map(key => {
        const dataSet: MapDataSetCategoryConfig | undefined =
          dataSetCategoryConfigs[key];
        return dataSet
          ? {
              label: dataSet.config.label,
              value: dataSet.dataKey,
            }
          : undefined;
      })
      .filter(item => item !== undefined);
  }, [dataSetCategoryConfigs, legend]);

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
  }, [
    dataSetCategories,
    dataSetCategoryConfigs,
    selectedDataSetConfig,
    selectedDataSetKey,
  ]);

  const handleLocationChange = useCallback(
    (value: string) => {
      setSelectedFeature(features?.features.find(feat => feat.id === value));
    },
    [features],
  );

  const handleDataSetChange = useCallback(
    async (value: string) => {
      setSelectedDataSetKey(value);

      const prevBoundaryLevel =
        selectedDataSetConfig?.boundaryLevel ?? boundaryLevel;
      const nextBoundaryLevel =
        dataSetCategoryConfigs[value].boundaryLevel ?? boundaryLevel;

      if (nextBoundaryLevel !== prevBoundaryLevel) {
        toggleBoundaryLevelChanging.on();
        await onBoundaryLevelChange(nextBoundaryLevel);
        toggleBoundaryLevelChanging.off();
      }
    },
    [
      dataSetCategoryConfigs,
      boundaryLevel,
      onBoundaryLevelChange,
      toggleBoundaryLevelChanging,
      selectedDataSetConfig,
    ],
  );

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
        title={title}
        onChangeDataSet={handleDataSetChange}
        onChangeLocation={handleLocationChange}
      />

      <div className="govuk-grid-row govuk-!-margin-bottom-4">
        <div
          className={classNames(
            'govuk-grid-column-two-thirds',
            styles.mapWrapper,
            {
              [styles.mapLoading]: isBoundaryLevelChanging,
            },
          )}
        >
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
          <LoadingSpinner
            className={styles.mapSpinner}
            loading={isBoundaryLevelChanging}
            text="Loading map"
            size="xl"
            hideText
            alert
          />
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
              heading={selectedFeature?.properties.Name}
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
