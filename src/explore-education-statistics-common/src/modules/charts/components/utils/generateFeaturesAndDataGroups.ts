import { MapDataSetCategory } from '@common/modules/charts/components/utils/createMapDataSetCategories';
import generateLegendDataGroups from '@common/modules/charts/components/utils/generateLegendDataGroups';
import { MapDataSetCategoryConfig } from '@common/modules/charts/util/getMapDataSetCategoryConfigs';
import generateHslColour from '@common/utils/colour/generateHslColour';
import {
  MapFeature,
  MapFeatureCollection,
} from '@common/modules/charts/components/MapBlock';
import {
  MapCategoricalDataConfig,
  MapLegendItem,
} from '@common/modules/charts/types/chart';
import { mapCategoricalDataColours } from '@common/modules/charts/util/chartUtils';
import uniq from 'lodash/uniq';

export default function generateFeaturesAndDataGroups({
  categoricalDataConfig,
  dataSetCategories,
  selectedDataSetConfig,
}: {
  categoricalDataConfig?: MapCategoricalDataConfig[];
  dataSetCategories: MapDataSetCategory[];
  selectedDataSetConfig: MapDataSetCategoryConfig;
}): {
  features: MapFeatureCollection;
  legendItems: MapLegendItem[];
  categoricalDataGroups?: MapCategoricalDataConfig[];
} {
  const selectedDataSetKey = selectedDataSetConfig.dataKey;

  const values = dataSetCategories.map(
    category => category.dataSets[selectedDataSetKey]?.value,
  );

  const isCategoricalData =
    categoricalDataConfig?.length ||
    values.every(value => !Number.isFinite(value));

  const { features, legendItems, categoricalDataGroups } = isCategoricalData
    ? getCategoricalDataFeatures({
        categoricalDataConfig,
        dataSetCategories,
        selectedDataSetKey,
      })
    : getNumericDataFeatures({
        dataSetCategories,
        selectedDataSetConfig,
        selectedDataSetKey,
      });

  return { features, legendItems, categoricalDataGroups };
}

function getCategoricalDataFeatures({
  categoricalDataConfig,
  dataSetCategories,
  selectedDataSetKey,
}: {
  categoricalDataConfig?: MapCategoricalDataConfig[];
  dataSetCategories: MapDataSetCategory[];
  selectedDataSetKey: string;
}) {
  const defaultColour = 'rgba(0,0,0,0)';

  const values = dataSetCategories.map(
    category => category.dataSets[selectedDataSetKey]?.value,
  );

  const categoricalDataGroups: MapCategoricalDataConfig[] =
    categoricalDataConfig?.length
      ? categoricalDataConfig
      : uniq(values).map((value, i) => {
          return {
            colour:
              mapCategoricalDataColours[i] ??
              `#${Math.floor(Math.random() * 16777215)
                .toString(16)
                .padStart(6, '0')}`,
            value: value.toString(),
          };
        });

  const features: MapFeatureCollection = {
    type: 'FeatureCollection',
    features: dataSetCategories.reduce<MapFeature[]>(
      (acc, { dataSets, filter, geoJson }) => {
        const value = dataSets?.[selectedDataSetKey]?.value;

        const matchingDataGroup = categoricalDataGroups.find(
          dataClass => dataClass.value === value,
        );

        acc.push({
          ...geoJson,
          id: filter.id,
          properties: {
            ...geoJson.properties,
            dataSets,
            colour: matchingDataGroup?.colour ?? defaultColour,
            data: value,
          },
        });

        return acc;
      },
      [],
    ),
  };

  return {
    features,
    categoricalDataGroups,
    legendItems: categoricalDataGroups,
  };
}

function getNumericDataFeatures({
  dataSetCategories,
  selectedDataSetConfig,
  selectedDataSetKey,
}: {
  dataSetCategories: MapDataSetCategory[];
  selectedDataSetConfig: MapDataSetCategoryConfig;
  selectedDataSetKey: string;
}) {
  const { unit, decimalPlaces } = selectedDataSetConfig.dataSet.indicator;

  const colour =
    selectedDataSetConfig.config.colour ??
    generateHslColour(selectedDataSetConfig.dataKey);

  // Default to white for areas not covered by custom data sets
  // to make it clearer which aren't covered by the groups.
  const defaultColour =
    selectedDataSetConfig.dataGrouping?.type === 'Custom'
      ? 'rgba(255, 255, 255, 1)'
      : 'rgba(0,0,0,0)';

  // Extract only the numeric values out of relevant data sets
  const values: number[] = dataSetCategories.reduce<number[]>(
    (acc, category) => {
      const value = category.dataSets[selectedDataSetKey]?.value;
      if (Number.isFinite(value)) {
        acc.push(Number(value));
      }

      return acc;
    },
    [],
  );

  const dataGroups = generateLegendDataGroups({
    colour,
    dataGrouping: selectedDataSetConfig.dataGrouping,
    decimalPlaces,
    values,
    unit,
  });

  const features: MapFeatureCollection = {
    type: 'FeatureCollection',
    features: dataSetCategories.reduce<MapFeature[]>(
      (acc, { dataSets, filter, geoJson }) => {
        const value = Number(dataSets?.[selectedDataSetKey]?.value);

        const matchingDataGroup = dataGroups.find(dataClass => {
          const roundedValue = Number(value.toFixed(dataClass.decimalPlaces));
          return (
            dataClass.minRaw <= roundedValue && roundedValue <= dataClass.maxRaw
          );
        });

        acc.push({
          ...geoJson,
          id: filter.id,
          properties: {
            ...geoJson.properties,
            dataSets,
            colour: matchingDataGroup?.colour ?? defaultColour,
            data: value,
          },
        });

        return acc;
      },
      [],
    ),
  };

  return {
    features,
    legendItems: dataGroups.map(group => {
      return {
        colour: group.colour,
        value: `${group.min} to ${group.max}`,
      };
    }),
    categoricalDataGroups: undefined,
  };
}
