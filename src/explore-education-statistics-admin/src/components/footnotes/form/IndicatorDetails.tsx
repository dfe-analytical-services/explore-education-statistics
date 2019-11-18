import React from 'react';
import { FormikProps } from 'formik';
import get from 'lodash/get';
import Details from '@common/components/Details';
import {
  FootnoteProps,
  FootnoteSubjectMeta,
} from '@admin/services/release/edit-release/footnotes/types';
import FieldCheckboxArray from '@common/components/form/FieldCheckboxArray';
import { FormCheckbox } from '@common/components/form';
import FieldSubjectCheckbox from './FieldSubjectCheckbox';

interface Props {
  summary: string;
  parentSelected: boolean;
  valuePath: string;
  indicator: FootnoteSubjectMeta['indicators'];
  form: FormikProps<FootnoteProps>;
}

const IndicatorDetails = ({
  summary,
  parentSelected = false,
  valuePath,
  indicator,
  form,
}: Props) => {
  return (
    <Details
      summary={`${summary} ${parentSelected ? '(All)' : ''}`}
      className="govuk-!-margin-bottom-2"
    >
      <div className="dfe-filter-overflow">
        {Object.entries(indicator).map(([indicatorGroupId, indicatorGroup]) => {
          const groupValue = get(
            form.values,
            `${valuePath}.indicatorGroups[${indicatorGroupId}].selected`,
          );
          const hideGrouping = indicatorGroup.label === 'Default';
          const indicators =
            get(
              form.values,
              `${valuePath}.indicatorGroups[${indicatorGroupId}].indicators`,
            ) || [];
          return (
            <>
              {!hideGrouping && (
                <FieldSubjectCheckbox
                  key={indicatorGroupId}
                  className="govuk-checkboxes--small"
                  id={`indicatorGroup-${indicatorGroupId}`}
                  name={`${valuePath}.indicatorGroups[${indicatorGroupId}].selected`}
                  label={
                    <>
                      <strong>{indicatorGroup.label}</strong>{' '}
                      {groupValue && '(All)'}
                    </>
                  }
                  disabled={parentSelected}
                />
              )}
              <div
                className={
                  !hideGrouping
                    ? 'govuk-!-margin-left-4 govuk-!-margin-bottom-3'
                    : ''
                }
              >
                {Object.entries(indicatorGroup.options).map(
                  ([indicatorItemId, indicatorItem]) => {
                    const checked =
                      (indicators &&
                        indicators.includes(Number(indicatorItem.value))) ||
                      false;
                    return (
                      <FormCheckbox
                        key={`indicatorItem-${indicatorItemId}`}
                        className="govuk-checkboxes--small"
                        name={`${valuePath}.indicatorGroups[${indicatorGroupId}].indicators`}
                        id={indicatorItemId}
                        {...indicatorItem}
                        disabled={parentSelected || groupValue}
                        checked={checked}
                        onChange={e => {
                          if (!checked) {
                            form.setFieldValue(
                              `${valuePath}.indicatorGroups[${indicatorGroupId}].indicators`,
                              [...indicators, Number(indicatorItem.value)],
                            );
                          } else {
                            form.setFieldValue(
                              `${valuePath}.indicatorGroups[${indicatorGroupId}].indicators`,
                              [
                                ...indicators.filter(
                                  (selectedItem: string) =>
                                    String(selectedItem) !==
                                    String(indicatorItemId),
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
            </>
          );
        })}
      </div>
    </Details>
  );
};

export default IndicatorDetails;
