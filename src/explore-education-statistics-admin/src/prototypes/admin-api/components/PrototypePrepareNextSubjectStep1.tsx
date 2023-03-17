import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useMounted from '@common/hooks/useMounted';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import { Form, FormFieldSelect, FormFieldset } from '@common/components/form';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React from 'react';
import { subjectsForRelease2 } from '../PrototypePublicationSubjects';

interface FormValues {
  subjectId: string;
}

interface Props extends InjectedWizardProps {
  onSubmit: (subjectId: string) => void;
}

const PrototypePrepareNextSubjectStep1 = ({
  onSubmit,
  ...stepProps
}: Props) => {
  const { isMounted } = useMounted();
  const { isActive, goToNextStep } = stepProps;

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading>
      Select new API dataset
    </WizardStepHeading>
  );

  if (isActive && isMounted) {
    return (
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <Formik<FormValues>
            initialValues={{
              subjectId: '',
            }}
            validationSchema={Yup.object({
              subjectId: Yup.string()
                .required('Choose a subject')
                .test({
                  name: 'whatevs',
                  message: `Time periods in the next dataset should be compatible with the current dataset.`,
                  test(values) {
                    if (values !== 'id4') {
                      return true;
                    }
                    return false;
                  },
                }),
            })}
            onSubmit={values => {
              onSubmit(values.subjectId);
              goToNextStep();
            }}
          >
            {() => (
              <Form id="form" showSubmitError>
                <FormFieldset id="downloadFiles" legend={stepHeading}>
                  <>
                    <p>
                      To make updates to this API dataset, a new dataset needs
                      to be selected to provide the underlying data. The chosen
                      dataset should be a continuation of the current dataset
                      and not have a drastically differing data structure.
                    </p>
                    <p>The following rules apply:</p>
                    <ul className="govuk-!-margin-bottom-8">
                      <li>
                        existing facets (column variables) should map to
                        equivalent facets in the new dataset
                      </li>
                      <li>
                        existing facets (column variables) should not have been
                        removed in the new dataset
                      </li>
                    </ul>
                    <FormFieldSelect<FormValues>
                      id="subjectId"
                      name="subjectId"
                      label="Available datasets"
                      options={subjectsForRelease2.map(s => ({
                        label: s.title,
                        value: s.id,
                      }))}
                      placeholder="Select a new dataset"
                    />
                  </>
                </FormFieldset>
                <WizardStepFormActions
                  submitText="Next step, locations"
                  {...stepProps}
                />
              </Form>
            )}
          </Formik>
        </div>
      </div>
    );
  }

  return (
    <WizardStepSummary
      {...stepProps}
      goToButtonText="Change dataset for next release"
    >
      {stepHeading}

      <SummaryList noBorder>
        <SummaryListItem term="Dataset for next release">
          {subjectsForRelease2[0].title}
        </SummaryListItem>
        <SummaryListItem term="Next release">
          {subjectsForRelease2[0].release}
        </SummaryListItem>
      </SummaryList>
    </WizardStepSummary>
  );
};

export default PrototypePrepareNextSubjectStep1;
