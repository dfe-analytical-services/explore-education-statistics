import React from 'react';
import Details from '@common/components/Details';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';

interface Props {
  title?: string;
}

const PrototypeMoreDetails = ({ title }: Props) => {
  return (
    <>
      <Details
        className="govuk-!-margin-bottom-0"
        summary={title || 'Example file'}
      >
        <SummaryList>
          <SummaryListItem term="Download data">
            <a
              href="#"
              className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0 govuk-!-margin-right-3"
            >
              CSV (2Mb)
            </a>{' '}
            <a
              href="#"
              className="govuk-button govuk-button--secondary govuk-!-margin-bottom-0"
            >
              ODS (2Mb)
            </a>
          </SummaryListItem>
          <SummaryListItem term="Geographical levels">
            Local Authority; National; Regional
          </SummaryListItem>
          <SummaryListItem term="Time period">Time period</SummaryListItem>
          <SummaryListItem term="Content">
            <p>
              Local authority level data on care leavers aged 17 to 21, by
              accommodation type (as measured on or around their birthday).
            </p>
          </SummaryListItem>
        </SummaryList>
      </Details>
    </>
  );
};

export default PrototypeMoreDetails;
