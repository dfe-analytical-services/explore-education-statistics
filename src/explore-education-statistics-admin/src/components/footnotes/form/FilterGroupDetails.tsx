import React from 'react';
import { FormikProps } from 'formik';
import get from 'lodash/get';
import Details from '@common/components/Details';
import {
  FootnoteProps,
  FootnoteSubjectMeta,
} from '@admin/services/release/edit-release/footnotes/types';
import { FormCheckbox } from '@common/components/form';
import FieldSubjectCheckbox from './FieldSubjectCheckbox';

interface Props {
  summary: string;
  parentSelected: boolean;
  selectAll?: boolean;
  valuePath: string;
  groupId?: number | string;
  filter: FootnoteSubjectMeta['filters'][0];
  value: boolean;
  form: FormikProps<FootnoteProps>;
}

const FilterGroupDetails = ({
  summary,
  parentSelected = false,
  selectAll = false,
  valuePath,
  groupId,
  filter,
  value,
  form,
}: Props) => {
  const groupPath = `${valuePath}.filters.${groupId}`;
  const groupIsSelected = parentSelected || value;

  return (
    <Details
      summary={`${summary} ${groupIsSelected ? '(All)' : ''}`}
      className="govuk-!-margin-bottom-2"
    >
      {selectAll && groupId && (
        <FieldSubjectCheckbox
          className="govuk-checkboxes--small"
          id={`select-all-${groupId}`}
          name={`${groupPath}.selected`}
          disabled={parentSelected}
          label="Select all groups"
        />
      )}
      <div className="dfe-filter-overflow">
        {Object.entries(filter.options).map(([filterGroupId, filterGroup]) => {
          const groupValue = get(
            form.values,
            `${groupPath}.filterGroups[${filterGroupId}].selected`,
          );
          const hideGrouping = filterGroup.label === 'Default';
          const filterItems =
            get(
              form.values,
              `${groupPath}.filterGroups[${filterGroupId}].filterItems`,
            ) || [];
          return (
            <div key={filterGroupId}>
              {!hideGrouping && (
                <FieldSubjectCheckbox
                  key={filterGroupId}
                  className="govuk-checkboxes--small"
                  id={`filterGroup-${filterGroupId}`}
                  name={`${groupPath}.filterGroups[${filterGroupId}].selected`}
                  label={
                    <>
                      <strong>{filterGroup.label}</strong>{' '}
                      {groupValue && '(All)'}
                    </>
                  }
                  disabled={groupIsSelected}
                />
              )}
              <div
                className={
                  !hideGrouping
                    ? 'govuk-!-margin-left-4 govuk-!-margin-bottom-3'
                    : ''
                }
              >
                {Object.entries(filterGroup.options).map(
                  ([filterItemId, filterItem]) => {
                    const checked =
                      (filterItems && filterItems.includes(filterItem.value)) ||
                      false;
                    return (
                      <FormCheckbox
                        key={`filterItem-${filterItemId}`}
                        className="govuk-checkboxes--small"
                        name={`${groupPath}.filterGroups[${filterGroupId}].filterItems`}
                        id={filterItemId}
                        {...filterItem}
                        disabled={groupIsSelected || groupValue}
                        checked={checked}
                        onChange={e => {
                          form.setFieldValue(
                            `${groupPath}.filterGroups[${filterGroupId}].selected`,
                            false,
                          );
                          if (!checked) {
                            form.setFieldValue(
                              `${groupPath}.filterGroups[${filterGroupId}].filterItems`,
                              [...filterItems, e.target.value],
                            );
                          } else {
                            form.setFieldValue(
                              `${groupPath}.filterGroups[${filterGroupId}].filterItems`,
                              [
                                ...filterItems.filter(
                                  (selectedItem: string) =>
                                    selectedItem !== e.target.value,
                                ),
                              ],
                            );
                          }
                        }}
                      />
                    );
                  },
                )}
              </div>
            </div>
          );
        })}
      </div>
    </Details>
  );
};

export default FilterGroupDetails;
