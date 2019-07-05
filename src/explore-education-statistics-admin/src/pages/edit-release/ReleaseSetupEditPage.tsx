import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import DummyReferenceData, {
  DateType,
  TimePeriodCoverageGroup,
} from '@admin/pages/DummyReferenceData';
import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';
import {
  FormFieldset,
  FormGroup,
  FormRadioGroup,
  FormTextInput,
} from '@common/components/form';
import { format } from 'date-fns';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import Link from '../../components/Link';
import { setupEditRoute } from '../../routes/releaseRoutes';
import {
  IdLabelPair,
  ReleaseSetupDetails,
} from '../../services/publicationService';

interface MatchProps {
  releaseId: string;
}

const ReleaseSetupEditPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const [releaseSetupDetails, setReleaseSetupDetails] = useState<
    ReleaseSetupDetails
  >();

  const [timePeriodCoverageGroups, setTimePeriodCoverageGroups] = useState<
    TimePeriodCoverageGroup[]
  >();

  const [releaseTypes, setReleaseTypes] = useState<IdLabelPair[]>();

  useEffect(() => {
    setReleaseSetupDetails(
      DummyPublicationsData.getReleaseSetupDetails(releaseId),
    );
    setTimePeriodCoverageGroups(DummyReferenceData.timePeriodCoverageGroups);
    setReleaseTypes(DummyReferenceData.releaseTypeOptions);
  }, [releaseId]);

  const selectedTimePeriodCoverageGroup =
    releaseSetupDetails && releaseSetupDetails.timePeriodCoverageCode
      ? DummyReferenceData.findTimePeriodCoverageGroup(
          releaseSetupDetails.timePeriodCoverageCode,
        )
      : null;

  return (
    <>
      <ReleasePageTemplate
        releaseId={releaseId}
        publicationTitle={
          releaseSetupDetails ? releaseSetupDetails.publicationTitle : ''
        }
      >
        <h2 className="govuk-heading-m">Edit release setup</h2>

        {releaseSetupDetails && timePeriodCoverageGroups && releaseTypes && (
          <form action={setupEditRoute.generateLink(releaseId)}>
            <FormFieldset id="test" legend="Select time period coverage">
              <FormGroup>
                <label htmlFor="time-period" className="govuk-label">
                  Type
                </label>
                {/* eslint-disable-next-line jsx-a11y/no-onchange */}
                <select
                  name="time-period"
                  id="time-period"
                  className="govuk-select"
                  value={releaseSetupDetails.timePeriodCoverageCode}
                  onChange={event => {
                    setReleaseSetupDetails({
                      ...releaseSetupDetails,
                      timePeriodCoverageCode: event.target.value,
                    });
                  }}
                >
                  {timePeriodCoverageGroups.map(group => (
                    <optgroup key={group.label} label={group.label}>
                      {group.options.map(option => (
                        <option key={option.id} value={option.id}>
                          {`${option.label} - ${option.id}`}
                        </option>
                      ))}
                    </optgroup>
                  ))}
                </select>
              </FormGroup>
              {selectedTimePeriodCoverageGroup &&
                DateType.Year ===
                  selectedTimePeriodCoverageGroup.startDateType && (
                  <FormGroup>
                    <FormTextInput
                      id="release-year"
                      name="release-year"
                      label={selectedTimePeriodCoverageGroup.startDateLabel}
                      value={format(
                        releaseSetupDetails.timePeriodCoverageStartDate,
                        'yyyy',
                      )}
                      width={4}
                    />
                  </FormGroup>
                )}
              {selectedTimePeriodCoverageGroup &&
                DateType.DayMonthYear ===
                  selectedTimePeriodCoverageGroup.startDateType && (
                  <FormGroup>
                    <fieldset className="govuk-fieldset">
                      <legend className="govuk-heading-s govuk-!-margin-bottom-3">
                        {selectedTimePeriodCoverageGroup.startDateLabel}
                      </legend>
                      <div className="govuk-form-group">
                        <fieldset className="govuk-fieldset">
                          <div
                            className="govuk-date-input"
                            id="financial-year-start-date"
                          >
                            <div className="govuk-date-input__item">
                              <div className="govuk-form-group">
                                <label
                                  htmlFor="financial-year-day"
                                  className="govuk-label govuk-date-input__label"
                                >
                                  Day
                                </label>
                                <input
                                  type="number"
                                  pattern="[0-9]*"
                                  name="financial-year-day"
                                  id="financial-year-day"
                                  className="govuk-input govuk-date-input__input govuk-input--width-2"
                                  value={format(
                                    releaseSetupDetails.timePeriodCoverageStartDate,
                                    'd',
                                  )}
                                />
                              </div>
                            </div>
                            <div className="govuk-date-input__item">
                              <div className="govuk-form-group">
                                <label
                                  htmlFor="financial-year-month"
                                  className="govuk-label govuk-date-input__label"
                                >
                                  Month
                                </label>
                                <input
                                  type="number"
                                  pattern="[0-9]*"
                                  name="financial-year-month"
                                  id="financial-year-month"
                                  className="govuk-input govuk-date-input__input govuk-input--width-2"
                                  value={format(
                                    releaseSetupDetails.timePeriodCoverageStartDate,
                                    'MM',
                                  )}
                                />
                              </div>
                            </div>
                            <div className="govuk-date-input__item">
                              <div className="govuk-form-group">
                                <label
                                  htmlFor="financial-year-year"
                                  className="govuk-label govuk-date-input__label"
                                >
                                  Year
                                </label>
                                <input
                                  type="number"
                                  pattern="[0-9]*"
                                  name="financial-year-year"
                                  id="financial-year-year"
                                  className="govuk-input govuk-date-input__input govuk-input--width-4"
                                  value={format(
                                    releaseSetupDetails.timePeriodCoverageStartDate,
                                    'yyyy',
                                  )}
                                />
                              </div>
                            </div>
                          </div>
                        </fieldset>
                      </div>
                    </fieldset>
                  </FormGroup>
                )}
            </FormFieldset>

            <fieldset className="govuk-fieldset">
              <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
                Schedule publish date
              </legend>
              <div className="govuk-date-input" id="schedule-publish-date">
                <div className="govuk-date-input__item">
                  <div className="govuk-form-group">
                    <label
                      htmlFor="schedule-day"
                      className="govuk-label govuk-date-input__label"
                    >
                      Day
                    </label>
                    <input
                      className="govuk-input govuk-date-input__inout govuk-input--width-2"
                      id="schedule-day"
                      name="schedule-day"
                      type="number"
                      pattern="[0-9]*"
                      value={format(
                        releaseSetupDetails.scheduledReleaseDate,
                        'd',
                      )}
                    />
                  </div>
                </div>
                <div className="govuk-date-input__item">
                  <div className="govuk-form-group">
                    <label
                      htmlFor="schedule-month"
                      className="govuk-label govuk-date-input__label"
                    >
                      Month
                    </label>
                    <input
                      className="govuk-input govuk-date-input__inout govuk-input--width-2"
                      id="schedule-month"
                      name="schedule-month"
                      type="number"
                      pattern="[0-9]*"
                      value={format(
                        releaseSetupDetails.scheduledReleaseDate,
                        'MM',
                      )}
                    />
                  </div>
                </div>
                <div className="govuk-date-input__item">
                  <div className="govuk-form-group">
                    <label
                      htmlFor="schedule-year"
                      className="govuk-label govuk-date-input__label"
                    >
                      Year
                    </label>
                    <input
                      className="govuk-input govuk-date-input__inout govuk-input--width-4"
                      id="schedule-year"
                      name="schedule-year"
                      type="number"
                      pattern="[0-9]*"
                      value={format(
                        releaseSetupDetails.scheduledReleaseDate,
                        'yyyy',
                      )}
                    />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset className="govuk-fieldset govuk-!-margin-top-6 govuk-!-margin-bottom-6">
              <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
                Next release expected (optional)
              </legend>
              <div className="govuk-date-input" id="schedule-publish-date">
                <div className="govuk-date-input__item">
                  <div className="govuk-form-group">
                    <label
                      htmlFor="schedule-day"
                      className="govuk-label govuk-date-input__label"
                    >
                      Day
                    </label>
                    <input
                      className="govuk-input govuk-date-input__inout govuk-input--width-2"
                      id="schedule-day"
                      name="schedule-day"
                      type="number"
                      pattern="[0-9]*"
                      value={
                        releaseSetupDetails.nextExpectedReleaseDate &&
                        format(releaseSetupDetails.nextExpectedReleaseDate, 'd')
                      }
                    />
                  </div>
                </div>
                <div className="govuk-date-input__item">
                  <div className="govuk-form-group">
                    <label
                      htmlFor="schedule-month"
                      className="govuk-label govuk-date-input__label"
                    >
                      Month
                    </label>
                    <input
                      className="govuk-input govuk-date-input__inout govuk-input--width-2"
                      id="schedule-month"
                      name="schedule-month"
                      type="number"
                      pattern="[0-9]*"
                      value={
                        releaseSetupDetails.nextExpectedReleaseDate &&
                        format(
                          releaseSetupDetails.nextExpectedReleaseDate,
                          'MM',
                        )
                      }
                    />
                  </div>
                </div>
                <div className="govuk-date-input__item">
                  <div className="govuk-form-group">
                    <label
                      htmlFor="schedule-year"
                      className="govuk-label govuk-date-input__label"
                    >
                      Year
                    </label>
                    <input
                      className="govuk-input govuk-date-input__inout govuk-input--width-4"
                      id="schedule-year"
                      name="schedule-year"
                      type="number"
                      pattern="[0-9]*"
                      value={
                        releaseSetupDetails.nextExpectedReleaseDate &&
                        format(
                          releaseSetupDetails.nextExpectedReleaseDate,
                          'yyyy',
                        )
                      }
                    />
                  </div>
                </div>
              </div>
            </fieldset>

            <FormGroup>
              <FormRadioGroup
                legend="Release type"
                id="release-type"
                name="release-type"
                value={releaseSetupDetails.releaseType.id}
                options={releaseTypes.map(option => ({
                  id: option.id,
                  label: option.label,
                  value: option.id,
                }))}
              />
            </FormGroup>
            <>
              <button
                type="submit"
                className="govuk-button govuk-!-margin-top-6"
              >
                Update release setup
              </button>

              <div className="govuk-!-margin-top-6">
                <Link to="/prototypes/publication-create-new-absence-config">
                  Cancel update
                </Link>
              </div>
            </>
          </form>
        )}
      </ReleasePageTemplate>
    </>
  );
};

export default ReleaseSetupEditPage;
