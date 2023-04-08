import styles from '@admin/pages/release/footnotes/components/FootnoteForm.module.scss';
import {
  BaseFootnote,
  FootnoteSubjectMeta,
} from '@admin/services/footnoteService';
import Details from '@common/components/Details';
import { FormCheckbox } from '@common/components/form';
import { FormikProps } from 'formik';
import get from 'lodash/get';
import React from 'react';

interface Props {
  summary: string;
  valuePath: string;
  indicatorGroups: FootnoteSubjectMeta['indicators'];
  form: FormikProps<BaseFootnote>;
}

const IndicatorDetails = ({
  summary,
  valuePath,
  indicatorGroups,
  form,
}: Props) => {
  return (
    <Details summary={summary} className="govuk-!-margin-bottom-2">
      <div className={styles.filterOverflow}>
        {Object.entries(indicatorGroups).map(
          ([indicatorGroupId, indicatorGroup]) => {
            const groupValue = get(
              form.values,
              `${valuePath}.indicatorGroups.${indicatorGroupId}.selected`,
            );
            const hideGrouping = indicatorGroup.label === 'Default';
            const indicators: string[] =
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
                  label={`${indicatorGroup.label}${groupValue && ' (All)' : ''}`}
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
                  {indicatorGroup.options.map(indicatorItem => {
                    const checked =
                      indicators.includes(indicatorItem.value) || false;
                    return (
                      <FormCheckbox
                        {...indicatorItem}
                        key={`indicatorItem-${indicatorItem.value}`}
                        className="govuk-checkboxes--small"
                        name={`${valuePath}.indicatorGroups.${indicatorGroupId}.indicators`}
                        id={indicatorItem.value}
                        disabled={groupValue}
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
          },
        )}
      </div>
    </Details>
  );
};

export default IndicatorDetails;
