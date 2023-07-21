import styles from '@admin/pages/release/footnotes/components/FootnoteForm.module.scss';
import {
  BaseFootnote,
  FootnoteSubjectMeta,
} from '@admin/services/footnoteService';
import Details from '@common/components/Details';
import { FormCheckbox, FormFieldCheckbox } from '@common/components/form';
import { FormikProps } from 'formik';
import get from 'lodash/get';
import React from 'react';

interface Props {
  summary: string;
  selectAll?: boolean;
  valuePath: string;
  groupId?: number | string;
  filter: FootnoteSubjectMeta['filters'][0];
  value: boolean;
  form: FormikProps<BaseFootnote>;
}

const FilterGroupDetails = ({
  summary,
  selectAll = false,
  valuePath,
  groupId,
  filter,
  value,
  form,
}: Props) => {
  const groupPath = `${valuePath}.filters.${groupId}`;
  const groupIsSelected = value;

  return (
    <Details
      summary={`${summary}${groupIsSelected ? ' (All)' : ''}`}
      className="govuk-!-margin-bottom-2"
    >
      <div className={styles.filterOverflow}>
        {selectAll && groupId && (
          <FormFieldCheckbox
            name={`${groupPath}.selected`}
            label="Select all"
            small
            checked={groupIsSelected}
            boldLabel
            formGroup={false}
          />
        )}

        {Object.entries(filter.options).map(([filterGroupId, filterGroup]) => {
          const groupValue = get(
            form.values,
            `${groupPath}.filterGroups[${filterGroupId}].selected`,
          );
          const hideGrouping = filterGroup.label === 'Default';
          const filterItems: string[] =
            get(
              form.values,
              `${groupPath}.filterGroups[${filterGroupId}].filterItems`,
            ) || [];
          return (
            <div key={filterGroupId}>
              {!hideGrouping && (
                <FormFieldCheckbox
                  name={`${groupPath}.filterGroups[${filterGroupId}].selected`}
                  label={`${filterGroup.label}${groupValue ? ' (All)' : ''}`}
                  small
                  boldLabel
                  formGroup={false}
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
                {filterGroup.options.map(filterItem => {
                  const checked =
                    filterItems.includes(filterItem.value) || false;
                  return (
                    <FormCheckbox
                      {...filterItem}
                      key={`filterItem-${filterItem.value}`}
                      className="govuk-checkboxes--small"
                      name={`${groupPath}.filterGroups[${filterGroupId}].filterItems`}
                      id={filterItem.value}
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
                })}
              </div>
            </div>
          );
        })}
      </div>
    </Details>
  );
};

export default FilterGroupDetails;
