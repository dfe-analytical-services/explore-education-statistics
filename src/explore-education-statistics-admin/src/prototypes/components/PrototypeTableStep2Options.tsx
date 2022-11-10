import React from 'react';
import Button from '@common/components/Button';

interface Props {
  noFeatured?: boolean;
}

const TableOptions = ({ noFeatured }: Props) => {
  return (
    <>
      <hr />

      <h3>What would you like to do?</h3>

      <div className="dfe-flex dfe-align-items--center govuk-!-margin-bottom-9">
        <Button className="govuk-!-margin-bottom-0">
          Create your own table
        </Button>{' '}
        <span className="govuk-!-margin-left-2 govuk-!-margin-right-2">or</span>{' '}
        <a href="#">Download full dataset (.zip, 100mb)</a>
      </div>

      {!noFeatured && (
        <>
          <h4 className=" govuk-heading-m">View our featured tables </h4>
          <div className="govuk-width-container govuk-!-margin-0">
            <p>
              These featured tables have been created by our publication team,
              highlighting popular tables built from this dataset. These tables
              can be viewed, shared and customised to the data that you're
              interested in.
            </p>
          </div>
        </>
      )}
    </>
  );
};

export default TableOptions;
