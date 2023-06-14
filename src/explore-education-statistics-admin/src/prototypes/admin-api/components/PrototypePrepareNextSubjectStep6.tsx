import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import Button from '@common/components/Button';
import WarningMessage from '@common/components/WarningMessage';
import React from 'react';
import { useHistory } from 'react-router-dom';

const PrototypePrepareNextSubjectStep6 = ({
  ...stepProps
}: InjectedWizardProps) => {
  const history = useHistory();

  const stepHeading = (
    <WizardStepHeading {...stepProps}>Complete linking</WizardStepHeading>
  );

  return (
    <>
      {stepHeading}
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p>
            By completing this step, this publication subject will be updated to
            use data from the next data set chosen above.
          </p>

          <p>
            All of the above mapping will be applied so that existing facets
            match to facets on the next data set. Any new facets introduced by
            the next data set will also be created.
          </p>

          <WarningMessage>
            Changes will not be made in the public API until the next data set's
            release has been published.
          </WarningMessage>

          <Button
            onClick={() => {
              history.push('/prototypes/admin-api/data/2022-23#subjects');
            }}
          >
            Complete data set linking
          </Button>
        </div>
      </div>
    </>
  );
};

export default PrototypePrepareNextSubjectStep6;
