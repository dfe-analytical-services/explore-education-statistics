import { FootnoteSubjectMeta } from '@admin/services/footnoteService';
import Details from '@common/components/Details';
import RHFFormFieldCheckboxGroup from '@common/components/form/rhf/RHFFormFieldCheckboxGroup';
import React from 'react';

interface Props {
  summary: string;
  valuePath: string;
  indicatorGroups: FootnoteSubjectMeta['indicators'];
}

export default function IndicatorDetails({
  summary,
  valuePath,
  indicatorGroups,
}: Props) {
  return (
    <Details summary={summary} className="govuk-!-margin-bottom-2">
      {Object.entries(indicatorGroups).map(
        ([indicatorGroupId, indicatorGroup]) => {
          const hideGrouping = indicatorGroup.label === 'Default';

          return (
            <div key={indicatorGroupId} className="govuk-!-margin-bottom-2 ">
              <div
                className={
                  !hideGrouping
                    ? 'govuk-!-margin-left-4 govuk-!-margin-bottom-3'
                    : ''
                }
              >
                <RHFFormFieldCheckboxGroup
                  name={`${valuePath}.indicatorGroups.${indicatorGroupId}.indicators`}
                  options={indicatorGroup.options}
                  legend={indicatorGroup.label}
                  legendSize="s"
                  legendHidden={hideGrouping}
                  small
                />
              </div>
            </div>
          );
        },
      )}
    </Details>
  );
}
