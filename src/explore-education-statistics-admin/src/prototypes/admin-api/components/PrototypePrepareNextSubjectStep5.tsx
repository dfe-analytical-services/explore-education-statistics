import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useMounted from '@common/hooks/useMounted';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import { FormFieldset } from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import React, { useEffect, useState } from 'react';
import ChangelogExample from './PrototypeChangelogExamples';
import {
  VersionType,
  usePrototypeNextSubjectContext,
} from '../contexts/PrototypeNextSubjectContext';

interface FormValues {
  versionNotes?: string;
  versionType: VersionType;
}

const PrototypePrepareNextSubjectStep5 = ({
  ...stepProps
}: InjectedWizardProps) => {
  const { isMounted } = useMounted();
  const { isActive, goToNextStep } = stepProps;

  const {
    versionNotes,
    versionType,
    setVersionNotes,
    setVersionType,
    locations,
    filters,
    indicators,
  } = usePrototypeNextSubjectContext();

  const [initialVersionType, setInitialVersionType] =
    useState<VersionType>(versionType);

  useEffect(() => {
    if (
      locations.noMappingItems.length ||
      filters.noMappingItems.length ||
      indicators.noMappingItems.length
    ) {
      setInitialVersionType('major');
      setVersionType('major');
    } else {
      setInitialVersionType('minor');
      setVersionType('minor');
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [locations, filters, indicators]);

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading>
      Changelog
    </WizardStepHeading>
  );

  if (isActive && isMounted) {
    return (
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <FormProvider
            initialValues={{
              versionNotes,
              versionType: initialVersionType,
            }}
          >
            {() => (
              <Form
                id="form"
                onSubmit={values => {
                  setVersionNotes(values.versionNotes);
                  setVersionType(values.versionType);
                  goToNextStep();
                }}
              >
                <FormFieldset id="downloadFiles" legend={stepHeading}>
                  <>
                    <FormFieldTextArea<FormValues>
                      hint="Use the public guidance notes to highlight any extra information to your end users that may not
                      be apparent in the automated changelog below"
                      label="Public guidance notes"
                      name="versionNotes"
                      rows={3}
                    />

                    <fieldset className="govuk-fieldset govuk-!-margin-top-9 govuk-!-margin-bottom-9">
                      <FormFieldRadioGroup<FormValues>
                        legend="Changes on current live version (version 1.0)"
                        name="versionType"
                        onChange={event =>
                          setVersionType(event.target.value as VersionType)
                        }
                        order={[]}
                        options={[
                          {
                            label: 'Minor',
                            value: 'minor',
                            disabled: initialVersionType === 'major',
                          },
                          {
                            label: 'Major',
                            value: 'major',
                          },
                        ]}
                      />
                    </fieldset>

                    <h3>
                      {versionType === 'major'
                        ? 'Major update changelog'
                        : 'Minor update changelog'}
                    </h3>
                    <p className="govuk-!-margin-top-6 govuk-!-margin-bottom-0 govuk-heading-s">
                      New API data set version number
                    </p>
                    <p>{versionType === 'major' ? '2.0' : '1.1'}</p>

                    <ChangelogExample
                      changelog={{
                        locations,
                        filters,
                        indicators,
                        versionType,
                        versionNotes,
                      }}
                    />
                  </>
                </FormFieldset>
                <WizardStepFormActions
                  submitText="Next step - complete this API data set version"
                  {...stepProps}
                />
              </Form>
            )}
          </FormProvider>
        </div>
      </div>
    );
  }

  return (
    <WizardStepSummary {...stepProps} goToButtonText="Update version changelog">
      {stepHeading}

      <SummaryList noBorder>
        <SummaryListItem term="Dataset for next release">
          {versionType === 'minor' ? 'Minor update' : 'Major update'}
        </SummaryListItem>
        <SummaryListItem term="Next release version">
          {versionType === 'major' ? '2.0' : '1.1'}
        </SummaryListItem>
        <SummaryListItem term="Notes">
          <div style={{ whiteSpace: 'pre-wrap' }}>{versionNotes}</div>
        </SummaryListItem>
        <SummaryListItem term="Changelog">
          <ChangelogExample
            changelog={{
              locations,
              filters,
              indicators,
              versionType,
              versionNotes,
            }}
          />
        </SummaryListItem>
      </SummaryList>
    </WizardStepSummary>
  );
};

export default PrototypePrepareNextSubjectStep5;
