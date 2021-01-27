import classNames from 'classnames';
import React from 'react';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';

const PrototypeMoreDetails = () => {
  return (
    <>
      <Details
        className="govuk-!-margin-bottom-6 govuk-!-margin-left-3"
        summary="More details"
      >
        <SummaryList className="govuk-!-margin-bottom-9">
          <SummaryListItem term="Filename">
            Absence_3term201819_nat_reg_la_sch
          </SummaryListItem>
          <SummaryListItem term="Content">
            Absence information for all enrolments in state-funded primary,
            secondary and special schools including information on overall
            absence, persistent absence and reason for absence for pupils aged
            5-15, based on all 5 half terms data from 2006/07 to 2011/12
            inclusive and based on 6 half term data from 2012/13 onwards
          </SummaryListItem>
          <SummaryListItem term="Geographical levels">
            National; Regional; Local authority; School
          </SummaryListItem>
          <SummaryListItem term="Years">2006/07 to 2018/19</SummaryListItem>
          <SummaryListItem term="Variable names and descriptions">
            test
          </SummaryListItem>
        </SummaryList>
      </Details>
    </>
  );
};

const PrototypePreviewSubjects = () => {
  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-three-quarters">
          <div className="govuk-form-group">
            <fieldset className="govuk-fieldset">
              <legend
                className={classNames(
                  'govuk-fieldset__legend',
                  'govuk-fieldset__legend--m',
                )}
              >
                Choose a subject
              </legend>
              <div className="govuk-radios">
                <div className="govuk-radios__item">
                  <input
                    type="radio"
                    className="govuk-radios__input"
                    name="subject"
                    id="subject-1"
                  />
                  <label
                    className={classNames('govuk-label', 'govuk-radios__label')}
                    htmlFor="subject-1"
                  >
                    LA - Accommodation of care leavers
                  </label>
                  <PrototypeMoreDetails />
                </div>
                <div className="govuk-radios__item">
                  <input
                    type="radio"
                    className="govuk-radios__input"
                    name="subject"
                    id="subject-2"
                  />
                  <label
                    className={classNames('govuk-label', 'govuk-radios__label')}
                    htmlFor="subject-2"
                  >
                    LA - Activity of care leavers
                  </label>
                  <PrototypeMoreDetails />
                </div>
                <div className="govuk-radios__item">
                  <input
                    type="radio"
                    className="govuk-radios__input"
                    name="subject"
                    id="subject-3"
                  />
                  <label
                    className={classNames('govuk-label', 'govuk-radios__label')}
                    htmlFor="subject-3"
                  >
                    LA - Care leavers by whether their accommodation is suitable
                  </label>
                  <PrototypeMoreDetails />
                </div>
                <div className="govuk-radios__item">
                  <input
                    type="radio"
                    className="govuk-radios__input"
                    name="subject"
                    id="subject-4"
                  />
                  <label
                    className={classNames('govuk-label', 'govuk-radios__label')}
                    htmlFor="subject-4"
                  >
                    LA - Children looked after at 31 March with three or more
                    placements during the year, or aged under 16 at 31 March who
                    had been looked after continuously for at least 2.5 years
                    and who were living in the same placement for at least 2
                    years
                  </label>
                  <PrototypeMoreDetails />
                </div>
              </div>
              <Button>Next step</Button>
            </fieldset>
          </div>
        </div>
      </div>
    </>
  );
};

export default PrototypePreviewSubjects;
