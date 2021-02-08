import classNames from 'classnames';
import React from 'react';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
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
            <a href="#">la_care_leavers_accommodation.csv</a> (csv, 2 Mb)
          </SummaryListItem>
          <SummaryListItem term="Geographical levels">
            Local Authority; National; Regional
          </SummaryListItem>
          <SummaryListItem term="Time period">Time period</SummaryListItem>
          <SummaryListItem term="Content">
            <p>
              Local authority level data on care leavers aged 17 to 21, by
              accommodation type (as measured on or around their birthday).
            </p>
            <p>Footnotes:</p>
            <p>
              1. National and regional numbers have been rounded to the nearest
              10. Percentages rounded to the nearest whole number. Figures
              exclude young people who were looked after under an agreed series
              of short term placements, those who have died since leaving care,
              those who have returned home to parents or someone with parental
              responsibility for a continuous period of at least 6 months and
              those whose care was transferred to another local authority.
            </p>
            <p>
              2. 'Local authority not in touch' excludes young people where
              activity information is known, as a third party provided it even
              though the local authority is not directly in touch with the young
              person.
            </p>
            <p>
              3. Accommodated with parents or relatives is likely to be an under
              count - if a young person's former foster carer is a relative they
              should be recorded as accommodated with their former foster carer.
            </p>
            <p>
              4. For some local authorities, such as Kent, the figures may be
              impacted by significant numbers of unaccompanied asylum seeking
              children.
            </p>
          </SummaryListItem>
          <SummaryListItem term="Variable names and descriptions">
            Do we need the variables here?
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
                  'govuk-!-margin-bottom-6',
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
              <ButtonGroup>
                <Button variant="secondary">Previous step</Button>
                <Button>Next step</Button>
              </ButtonGroup>
            </fieldset>
          </div>
        </div>
      </div>
    </>
  );
};

export default PrototypePreviewSubjects;
