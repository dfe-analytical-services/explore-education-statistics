import classNames from 'classnames';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useMounted from '@common/hooks/useMounted';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import { Form, FormFieldset } from '@common/components/form';
import { Formik } from 'formik';
import React, { useState } from 'react';
import ChangelogExample from './PrototypeChangelogExamples';

interface FormValues {
  subjectId: string;
}

const PrototypePrepareNextSubjectStep5 = ({
  ...stepProps
}: InjectedWizardProps) => {
  const { isMounted } = useMounted();
  const { isActive, goToNextStep } = stepProps;
  const [versionType, setVersionType] = useState(false);
  const [versionNotes, setVersionNotes] = useState('');

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading>
      Changelog
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
            onSubmit={() => {
              goToNextStep();
            }}
          >
            {() => (
              <Form id="form">
                <FormFieldset id="downloadFiles" legend={stepHeading}>
                  <>
                    <label htmlFor="deprecationNotes">
                      Version notes
                      <span className="govuk-hint">
                        Use notes to highlight any extra guidance that may not
                        be apparent in the automated changelog below
                      </span>
                    </label>
                    <textarea
                      className="govuk-textarea"
                      id="versionNotes"
                      onChange={e => {
                        setVersionNotes(e.target.value);
                      }}
                    />

                    <fieldset className="govuk-fieldset govuk-!-margin-top-9 govuk-!-margin-bottom-9">
                      <legend className="govuk-legend govuk-fieldset__legend">
                        <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                          Changes on current live version (version 1.0)
                        </h3>
                      </legend>
                      <div className="govuk-radios">
                        <div className="govuk-radios__item">
                          <input
                            type="radio"
                            className="govuk-radios__input"
                            name="version-type"
                            id="version-minor"
                            checked={!versionType}
                            onClick={() => {
                              setVersionType(false);
                            }}
                          />
                          <label
                            className={classNames(
                              'govuk-label',
                              'govuk-radios__label',
                            )}
                            htmlFor="version-minor"
                          >
                            Minor
                          </label>
                        </div>

                        <div className="govuk-radios__item">
                          <input
                            type="radio"
                            className="govuk-radios__input"
                            name="version-type"
                            id="version-major"
                            onClick={() => {
                              setVersionType(true);
                            }}
                          />
                          <label
                            className={classNames(
                              'govuk-label',
                              'govuk-radios__label',
                            )}
                            htmlFor="version-major"
                          >
                            Major
                          </label>
                        </div>
                      </div>
                    </fieldset>

                    {!versionType && (
                      <>
                        <h3>Minor update changelog</h3>
                        <h3 className="govuk-!-margin-top-6 govuk-!-margin-bottom-0 govuk-heading-s">
                          New API data set version number
                        </h3>
                        <p>1.1</p>
                        <ChangelogExample versionUpdate="Minor" />
                      </>
                    )}
                    {versionType && (
                      <>
                        <h3>Major update changelog</h3>
                        <h3 className="govuk-!-margin-top-6 govuk-!-margin-bottom-0 govuk-heading-s">
                          New API data set version number
                        </h3>
                        <p>2.0</p>
                        <ChangelogExample versionUpdate="Major" />
                      </>
                    )}
                  </>
                </FormFieldset>
                <WizardStepFormActions
                  submitText="Next step - complete this API dataset version"
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
    <WizardStepSummary {...stepProps} goToButtonText="Update version changelog">
      {stepHeading}

      <SummaryList noBorder>
        <SummaryListItem term="Dataset for next release">
          {!versionType ? 'Minor update' : 'Major update'}
        </SummaryListItem>
        <SummaryListItem term="Next release version">
          {versionType ? '2.0' : '1.1'}
        </SummaryListItem>
        <SummaryListItem term="Notes">
          <div style={{ whiteSpace: 'pre-wrap' }}>{versionNotes}</div>
        </SummaryListItem>
        <SummaryListItem term="Changelog">
          <ChangelogExample versionUpdate={!versionType ? 'Minor' : 'Major'} />
        </SummaryListItem>
      </SummaryList>
    </WizardStepSummary>
  );
};

export default PrototypePrepareNextSubjectStep5;
