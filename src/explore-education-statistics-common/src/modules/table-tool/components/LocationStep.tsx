import CollapsibleList from '@common/components/CollapsibleList';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import LocationFiltersForm, {
  LocationFiltersFormProps,
} from '@common/modules/table-tool/components/LocationFiltersForm';
import { LocationOption } from '@common/services/tableBuilderService';
import sortBy from 'lodash/sortBy';
import React, { useMemo, useState } from 'react';

interface Props extends LocationFiltersFormProps {
  stepTitle: string;
}

export default function LocationStep(stepProps: Props) {
  const {
    initialValues = [],
    isActive,
    options,
    stepTitle,
    onSubmit,
  } = stepProps;

  const [selectedLocationIds, setSelectedLocationIds] =
    useState<string[]>(initialValues);

  const selectedLocations = useMemo<
    { legend: string; options: LocationOption[] }[]
  >(() => {
    return selectedLocationIds.length
      ? Object.values(options).reduce<
          { legend: string; options: LocationOption[] }[]
        >((acc, level) => {
          const opts = level.options[0].options
            ? level.options.flatMap(option => option.options ?? [])
            : level.options;
          const selectedOptions = opts.filter(
            opt => opt?.id && selectedLocationIds.includes(opt.id),
          );
          if (selectedOptions.length) {
            acc.push({
              ...level,
              options: selectedOptions,
            });
          }

          return acc;
        }, [])
      : [];
  }, [options, selectedLocationIds]);

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading>
      {stepTitle}
    </WizardStepHeading>
  );

  if (!isActive) {
    return (
      <WizardStepSummary {...stepProps} goToButtonText="Edit locations">
        {stepHeading}
        <SummaryList noBorder>
          {selectedLocations.map(level => {
            return (
              <SummaryListItem term={level.legend} key={level.legend}>
                <CollapsibleList
                  id="locationsList"
                  itemName="location"
                  itemNamePlural="locations"
                >
                  {sortBy(level.options, ['label']).map(option => (
                    <li key={option.value}>{option.label}</li>
                  ))}
                </CollapsibleList>
              </SummaryListItem>
            );
          })}
        </SummaryList>
      </WizardStepSummary>
    );
  }

  return (
    <LocationFiltersForm
      {...stepProps}
      stepHeading={stepHeading}
      onSubmit={values => {
        setSelectedLocationIds(values.locationIds);
        onSubmit(values);
      }}
    />
  );
}
