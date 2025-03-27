import { FormCheckbox } from '@common/components/form';
import FormField from '@common/components/form/FormField';
import { SubjectMetaFilterHierarchy } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import React, { memo } from 'react';

export type FilterHierarchyOption2 = {
  value: string;
  label: string;
  options?: FilterHierarchyOption2[];
};

interface FilterHierarchyOptionsProps {
  optionLabelsMap: Dictionary<string>;
  filterHierarchy: SubjectMetaFilterHierarchy;
  tiersTotal: number;
  optionId: string;
  level: number;
  disabled?: boolean;
  name: string;
  selectedValues: string[];
  hideLowerTotals?: boolean;
}

function FilterHierarchyOptions({
  optionId,
  optionLabelsMap,
  filterHierarchy,
  tiersTotal,
  name,
  disabled,
  level,
  selectedValues = [],
  hideLowerTotals = true,
}: FilterHierarchyOptionsProps) {
  const optionLabel = optionLabelsMap[optionId];
  const childOptionIds = filterHierarchy[level]?.hierarchy[optionId] ?? [];

  return (
    <div
      className={classNames('govuk-checkboxes', {
        'govuk-checkboxes--small': true,
      })}
    >
      <div className="govuk-!-margin-left-8 govuk-!-margin-bottom-2">
        {hideLowerTotals &&
        level !== 0 &&
        optionLabel.toLocaleLowerCase() === 'total' &&
        hideLowerTotals ? null : (
          <FormField
            label={optionLabel}
            id={`${name}-${optionId}`}
            // @ts-expect-error  `value` required to make checkboxes unique
            value={optionId}
            name={name}
            as={FormCheckbox}
            checked={selectedValues.includes(optionId)}
            disabled={disabled}
          />
        )}
        {childOptionIds.map(childOptionId => (
          <FilterHierarchyOptions
            optionId={childOptionId}
            key={childOptionId}
            optionLabelsMap={optionLabelsMap}
            filterHierarchy={filterHierarchy}
            name={name}
            disabled={disabled}
            tiersTotal={tiersTotal}
            selectedValues={selectedValues}
            level={level + 1}
          />
        ))}
      </div>
    </div>
  );
}

export default memo(FilterHierarchyOptions);
