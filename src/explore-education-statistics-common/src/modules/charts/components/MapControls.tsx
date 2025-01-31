import { FormGroup, FormSelect } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { MapDataSetCategory } from '@common/modules/charts/components/utils/createMapDataSetCategories';
import { Dictionary } from '@common/types';
import locationLevelsMap, {
  LocationLevelKey,
} from '@common/utils/locationLevelsMap';
import orderBy from 'lodash/orderBy';
import uniq from 'lodash/uniq';
import React, { useMemo } from 'react';

interface Props {
  dataSetCategories: MapDataSetCategory[];
  dataSetOptions: SelectOption[];
  id: string;
  selectedDataSetKey: string;
  selectedLocation?: string;
  title?: string;
  onChangeDataSet: (value: string) => void;
  onChangeLocation: (value: string) => void;
}

export default function MapControls({
  dataSetCategories,
  dataSetOptions,
  id,
  selectedDataSetKey,
  selectedLocation,
  title,
  onChangeDataSet,
  onChangeLocation,
}: Props) {
  const shouldGroupLocationOptions = useMemo(() => {
    return dataSetCategories.some(
      element =>
        element.filter.level === 'localAuthority' ||
        element.filter.level === 'localAuthorityDistrict',
    );
  }, [dataSetCategories]);

  // If there are no LAs or LADs don't group the locations.
  const locationOptions = useMemo(() => {
    if (shouldGroupLocationOptions) {
      return undefined;
    }
    return orderBy(
      dataSetCategories.map(dataSetCategory => ({
        label: dataSetCategory.filter.label,
        value: dataSetCategory.filter.id,
      })),
      ['label'],
    );
  }, [dataSetCategories, shouldGroupLocationOptions]);

  // // If there are LAs or LADs, group them by region and group any others by level
  const groupedLocationOptions = useMemo(() => {
    if (!shouldGroupLocationOptions) {
      return undefined;
    }
    return dataSetCategories.reduce<Dictionary<SelectOption[]>>(
      (acc, { filter }) => {
        const groupLabel =
          filter.level === 'localAuthority' ||
          filter.level === 'localAuthorityDistrict'
            ? (filter.group as string)
            : locationLevelsMap[filter.level as LocationLevelKey].label;

        (acc[groupLabel] ??= []).push({
          label: filter.label,
          value: filter.id,
        });
        return acc;
      },
      {},
    );
  }, [dataSetCategories, shouldGroupLocationOptions]);

  const locationType = useMemo(() => {
    const levels = uniq(
      dataSetCategories.map(category => category.filter.level),
    );
    const levelKey = levels[0] as LocationLevelKey;
    return !levels.every(level => level === levels[0]) ||
      !locationLevelsMap[levelKey]
      ? { label: 'location', prefix: 'a' }
      : locationLevelsMap[levelKey];
  }, [dataSetCategories]);

  return (
    <form className="govuk-!-margin-bottom-2">
      <div className="govuk-grid-row">
        <FormGroup className="govuk-grid-column-two-thirds">
          <FormSelect
            name="selectedDataSet"
            id={`${id}-selectedDataSet`}
            className="govuk-!-width-full"
            label={
              <>
                1. Select data to view
                {title && <VisuallyHidden>{` for ${title}`}</VisuallyHidden>}
              </>
            }
            value={selectedDataSetKey}
            onChange={e => onChangeDataSet(e.currentTarget.value)}
            options={dataSetOptions}
            order={FormSelect.unordered}
          />
        </FormGroup>

        <FormGroup className="govuk-grid-column-one-third">
          <FormSelect
            name="selectedLocation"
            id={`${id}-selectedLocation`}
            label={
              <>
                {`2. Select ${locationType.prefix} ${locationType.label}`}
                {title && <VisuallyHidden>{` for ${title}`}</VisuallyHidden>}
              </>
            }
            value={selectedLocation}
            options={locationOptions}
            optGroups={groupedLocationOptions}
            order={FormSelect.unordered}
            placeholder="None selected"
            onChange={e => onChangeLocation(e.currentTarget.value)}
          />
        </FormGroup>
      </div>
    </form>
  );
}
