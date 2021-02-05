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
          <SummaryListItem term="Download CSV">
            <a href="#">{`${title}.csv`}</a> (2Mb)
          </SummaryListItem>
          <SummaryListItem term="Download ODS">
            <a href="#">{`${title}.ods`}</a> (2Mb)
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
