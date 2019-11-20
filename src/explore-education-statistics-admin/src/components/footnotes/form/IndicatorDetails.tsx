import React from 'react';
import { FormikProps } from 'formik';
import get from 'lodash/get';
import Details from '@common/components/Details';
import {
  FootnoteProps,
  FootnoteSubjectMeta,
} from '@admin/services/release/edit-release/footnotes/types';
import { FormCheckbox } from '@common/components/form';

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
            `${valuePath}.indicatorGroups.${indicatorGroupId}.selected`,
          );
          const hideGrouping = indicatorGroup.label === 'Default';
          const indicators =
            get(
              form.values,
              `${valuePath}.indicatorGroups.${indicatorGroupId}.indicators`,
            ) || [];
          return (
            <div key={indicatorGroupId} className="govuk-!-margin-bottom-2 ">
              {!hideGrouping && (
                <div className="govuk-!-margin-top-1 govuk-!-margin-bottom-1">
                  <strong>{indicatorGroup.label}</strong>
                </div>
                /* 
                 // Disabling indicatorGroup selection for now
                  <FieldSubjectCheckbox
                  key={indicatorGroupId}
                  className="govuk-checkboxes--small"
                  id={`indicatorGroup-${indicatorGroupId}`}
                  name={`${valuePath}.indicatorGroups[${indicatorGroupId}].selected`}
                  label={`${indicatorGroup.label} ${groupValue && '(All)' : ''}`}
                  boldLabel
                  disabled={parentSelected}
                /> */
              )}
              <div
                className={
                  !hideGrouping
                    ? 'govuk-!-margin-left-4 govuk-!-margin-bottom-3'
                    : ''
                }
              >
                {Object.entries(indicatorGroup.options)
                  .sort(function(a, b) {
                    const textA = a[1].label.toUpperCase();
                    const textB = b[1].label.toUpperCase();
                    return textA < textB ? -1 : 1;
                  })
                  .map(([indicatorItemId, indicatorItem]) => {
                    const checked =
                      (indicators &&
                        indicators.includes(indicatorItem.value)) ||
                      false;
                    return (
                      <FormCheckbox
                        key={`indicatorItem-${indicatorItemId}`}
                        className="govuk-checkboxes--small"
                        name={`${valuePath}.indicatorGroups.${indicatorGroupId}.indicators`}
                        id={indicatorItemId}
                        {...indicatorItem}
                        disabled={parentSelected || groupValue}
                        checked={checked}
                        onChange={e => {
                          form.setFieldValue(
                            `${valuePath}.indicatorGroups.${indicatorGroupId}.selected`,
                            false,
                          );
                          if (!checked) {
                            form.setFieldValue(
                              `${valuePath}.indicatorGroups.${indicatorGroupId}.indicators`,
                              [...indicators, e.target.value],
                            );
                          } else {
                            form.setFieldValue(
                              `${valuePath}.indicatorGroups.${indicatorGroupId}.indicators`,
                              [
                                ...indicators.filter(
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

export default IndicatorDetails;
