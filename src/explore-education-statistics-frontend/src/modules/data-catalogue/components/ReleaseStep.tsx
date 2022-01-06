import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useMounted from '@common/hooks/useMounted';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import { ReleaseSummary } from '@common/services/publicationService';
import ReleaseForm, {
  ReleaseFormSubmitHandler,
} from '@frontend/modules/data-catalogue/components/ReleaseForm';
import React from 'react';

interface Props {
  releases: ReleaseSummary[];
  selectedRelease?: ReleaseSummary;
  onSubmit: ReleaseFormSubmitHandler;
}

const ReleaseStep = ({
  releases,
  selectedRelease,
  onSubmit,
  ...stepProps
}: Props & InjectedWizardProps) => {
  const { isActive } = stepProps;
  const { isMounted } = useMounted();

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading>
      Choose a release
    </WizardStepHeading>
  );

  // isMounted check required as Formik context can be
  // undefined if the step is active on page load.
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
    <WizardStepSummary {...stepProps} goToButtonText="Change release">
      {stepHeading}

      <SummaryList noBorder>
        <SummaryListItem term="Release">
          {selectedRelease?.title}
        </SummaryListItem>
      </SummaryList>
    </WizardStepSummary>
  );
};

export default ReleaseStep;
