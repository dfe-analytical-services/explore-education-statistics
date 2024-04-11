import { FootnoteSubjectMeta } from '@admin/services/footnoteService';
import Details from '@common/components/Details';
import RHFFormFieldCheckbox from '@common/components/form/rhf/RHFFormFieldCheckbox';
import RHFFormFieldCheckboxGroup from '@common/components/form/rhf/RHFFormFieldCheckboxGroup';
import classNames from 'classnames';
import React from 'react';
import { useFormContext } from 'react-hook-form';

interface Props {
  summary: string;
  selectAll?: boolean;
  valuePath: string;
  groupId?: number | string;
  filter: FootnoteSubjectMeta['filters'][0];
}

export default function FilterGroupDetails({
  summary,
  selectAll = false,
  valuePath,
  groupId,
  filter,
}: Props) {
  const { watch } = useFormContext();
  const groupPath = `${valuePath}.filters.${groupId}`;
  const groupIsSelected = watch(`${groupPath}.selected`);

  return (
    <Details
      summary={`${summary}${groupIsSelected ? ' (All)' : ''}`}
      className="govuk-!-margin-bottom-2"
      testId={`filter-${groupId}`}
    >
      {selectAll && groupId && (
        <RHFFormFieldCheckbox
          name={`${groupPath}.selected`}
          label="Select all"
          small
          checked={groupIsSelected}
          boldLabel
          formGroup={false}
        />
      )}

      {Object.entries(filter.options).map(([filterGroupId, filterGroup]) => {
        const groupValue = watch(
          `${groupPath}.filterGroups[${filterGroupId}].selected`,
        );

        const hideGrouping = filterGroup.label === 'Default';

        return (
          <div key={filterGroupId}>
            {!hideGrouping && (
              <RHFFormFieldCheckbox
                name={`${groupPath}.filterGroups[${filterGroupId}].selected`}
                label={`${filterGroup.label}${groupValue ? ' (All)' : ''}`}
                small
                boldLabel
                formGroup={false}
                disabled={!!groupIsSelected}
              />
            )}

            <div
              className={classNames({
                'govuk-!-margin-left-4 govuk-!-margin-bottom-3': !hideGrouping,
              })}
            >
              <RHFFormFieldCheckboxGroup
                name={`${groupPath}.filterGroups[${filterGroupId}].filterItems`}
                options={filterGroup.options}
                legend={filterGroup.label}
                legendSize="s"
                legendHidden
                small
                disabled={!!(groupIsSelected || groupValue)}
              />
            </div>
          </div>
        );
      })}
    </Details>
  );
}
