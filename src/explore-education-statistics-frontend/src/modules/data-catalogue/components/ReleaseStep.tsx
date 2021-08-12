import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepEditButton from '@common/modules/table-tool/components//WizardStepEditButton';
import { Release } from '@common/services/publicationService';
import ReleaseForm, {
  ReleaseFormSubmitHandler,
} from '@frontend/modules/data-catalogue/components/ReleaseForm';
import useMounted from '@common/hooks/useMounted';
import React from 'react';

interface Props {
  releases: Release[];
  selectedRelease?: Release;
  onSubmit: ReleaseFormSubmitHandler;
}

const ReleaseStep = ({
  releases,
  selectedRelease,
  onSubmit,
  ...stepProps
}: Props & InjectedWizardProps) => {
  const { isActive, currentStep, stepNumber } = stepProps;
  const { isMounted } = useMounted();

  const stepEnabled = currentStep > stepNumber;
  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading stepEnabled={stepEnabled}>
      Choose a release
    </WizardStepHeading>
  );
  // isMounted check required as Formik context can be undefined if the step is active on page load.
  if (isActive && isMounted) {
    return (
      <ReleaseForm
        {...stepProps}
        initialValues={{
          releaseId: selectedRelease?.id || '',
        }}
        options={releases}
        onSubmit={onSubmit}
        legendSize="l"
        legend={stepHeading}
      />
    );
  }

  return (
    <div className="govuk-grid-row">
      <div className="govuk-grid-column-two-thirds">
        {stepHeading}
        <SummaryList noBorder>
          <SummaryListItem term="Release">
            {selectedRelease?.title}
          </SummaryListItem>
        </SummaryList>
      </div>
      <div className="govuk-grid-column-one-third dfe-align--right">
        {stepEnabled && (
          <WizardStepEditButton {...stepProps} editTitle="Change release" />
        )}
      </div>
    </div>
  );
};

export default ReleaseStep;
