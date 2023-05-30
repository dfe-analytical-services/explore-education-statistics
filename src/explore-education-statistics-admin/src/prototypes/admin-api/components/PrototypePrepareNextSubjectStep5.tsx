import classNames from 'classnames';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import useMounted from '@common/hooks/useMounted';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import {
  Form,
  FormRadio,
  FormRadioGroup,
  FormFieldset,
} from '@common/components/form';
import FormEditor from '@admin/components/form/FormEditor';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { useState } from 'react';
import { subjectsForRelease2 } from '../PrototypePublicationSubjects';

interface FormValues {
  subjectId: string;
}

interface Props extends InjectedWizardProps {
  onSubmit: (subjectId: string) => void;
}

const PrototypePrepareNextSubjectStep5 = ({
  onSubmit,
  ...stepProps
}: Props) => {
  const { isMounted } = useMounted();
  const { isActive, goToNextStep } = stepProps;
  const [versionType, setVersionType] = useState(false);

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
            validationSchema={Yup.object({
              subjectId: Yup.string().test({
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
            onSubmit={() => {
              goToNextStep();
            }}
          >
            {() => (
              <Form id="form">
                <FormFieldset id="downloadFiles" legend={stepHeading}>
                  <>
                    <FormEditor
                      id="description"
                      label="Version notes"
                      value=""
                      hint="Use notes to highlight any extra guidance that may not be apparent in the automated changelog below."
                      onChange={() => {}}
                    />
                    <fieldset className="govuk-fieldset govuk-!-margin-top-9 govuk-!-margin-bottom-9">
                      <legend className="govuk-legend govuk-fieldset__legend">
                        <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                          Changes on current live version (v1.0)
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
                        <p>{versionType ? '2.0' : '1.1'}</p>
                        <h4 className="govuk-!-margin-bottom-0">
                          New locations
                        </h4>
                        <ul className="govuk-!-margin-bottom-6">
                          <li>Leeds</li>
                          <li>Doncaster</li>
                        </ul>
                        <h4 className="govuk-!-margin-bottom-0">
                          Mapped locations
                        </h4>
                        <ul className="govuk-!-margin-bottom-6">
                          <li>Darlington --{'>'} Darlington (new)</li>
                          <li>
                            Kingston upon Hull, City of --{'>'} Kingston upon
                            Hull, City of (new)
                          </li>
                          <li>Northumberland --{'>'} Nortumberland (new)</li>
                          <li>Sheffield--{'>'} Sheffield (new)</li>
                        </ul>
                        <h4 className="govuk-!-margin-bottom-0">New filters</h4>
                        <ul className="govuk-!-margin-bottom-6">
                          <li>Age 11</li>
                          <li>Age 12</li>
                        </ul>
                        <h4 className="govuk-!-margin-bottom-0">
                          Mapped filters
                        </h4>
                        <ul className="govuk-!-margin-bottom-6">
                          <li>Age 10 --{'>'} Age 10 (new)</li>
                          <li>
                            Ethnicity Major Asian Total --{'>'} Ethnicity Major
                            Asian Total (new)
                          </li>
                          <li>
                            Ethnicity Major Black Total --{'>'} Ethnicity Major
                            Black Total (new)
                          </li>
                          <li>Age 4 and under --{'>'} Age 4 and under (new)</li>
                        </ul>
                        <h4 className="govuk-!-margin-bottom-0">
                          New indicators
                        </h4>
                        <ul className="govuk-!-margin-bottom-6">
                          <li>Number of authorised other sessions</li>
                          <li>Number of authorised reasons sessions</li>
                        </ul>
                        <h4 className="govuk-!-margin-bottom-0">
                          Mapped indicators
                        </h4>
                        <ul className="govuk-!-margin-bottom-6">
                          <li>
                            Number of authorised holiday sessions --{'>'} Number
                            of authorised holiday sessions (new)
                          </li>
                          <li>
                            Authorised absence rate --{'>'} Number of Authorised
                            absence rate (new)
                          </li>
                        </ul>
                      </>
                    )}
                    {versionType && (
                      <>
                        <h3>Major update changelog</h3>
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
      </SummaryList>
    </WizardStepSummary>
  );
};

export default PrototypePrepareNextSubjectStep5;
