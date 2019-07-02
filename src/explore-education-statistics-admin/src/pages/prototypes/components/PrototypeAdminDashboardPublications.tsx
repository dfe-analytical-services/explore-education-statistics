import PrototypeDashboardRelease from '@admin/pages/prototypes/components/PrototypeDashboardRelease';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import {
  FormGroup,
  FormFieldset,
  FormTextInput,
  FormSelect,
  FormRadioGroup,
} from '@common/components/form';
import React from 'react';
import Link from '@admin/components/Link';

const PrototypeAdminDashboardPublications = () => {
  return (
    <>
      <form>
        <FormGroup>
          <FormSelect
            id="select-theme"
            label="Select theme"
            name="select-theme"
            options={[
              { label: 'Pupils and schools', value: 'pupils-schools' },
              {
                label: 'School and college outcomes and performance',
                value: 'school-college-performance',
              },
            ]}
          />
        </FormGroup>
        <FormGroup>
          <FormSelect
            id="select-theme"
            label="Select topic"
            name="select-topic"
            value="pupil-absence"
            options={[
              { label: 'Admission appeals', value: 'admission-appeals' },
              { label: 'Exclusions', value: 'exclusions' },
              { label: 'Pupil absence', value: 'pupil-absence' },
              {
                label: 'Parental responsibility measures',
                value: 'parental-repsonsibilty',
              },
              { label: 'Pupil projections', value: 'pupil-projections' },
              { label: 'View all topics', value: 'view-all-topics' },
            ]}
          />
        </FormGroup>
      </form>
      <hr />
      <h2 className="govuk-heading-l govuk-!-margin-bottom-0">
        Pupils and schools, pupil absence
      </h2>
      <p className="govuk-body">
        Edit an existing release or create a new release for current
        publications.
      </p>
      <Link to="/prototypes/publication-create-new" className="govuk-button">
        Create a new publication
      </Link>
      <h2 className="govuk-heading-m govuk-!-margin-bottom-0">Publications</h2>
      <Accordion id="pupil-absence">
        <AccordionSection
          heading="Pupil absence statistics and data for schools in England"
          caption=""
        >
          <dl className="govuk-summary-list govuk-!-margin-bottom-0">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key  dfe-summary-list__key--small">
                Methodology
              </dt>
              <dd className="govuk-summary-list__value">
                <Link to="#">A guide to absence statistics</Link>
              </dd>
              <dd className="govuk-summary-list__actions">
                <Link to="/prototypes/publication-assign-methodology">
                  Edit methodology
                </Link>
              </dd>
            </div>
          </dl>
          <dl className="govuk-summary-list">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key dfe-summary-list__key--small">
                Releases
              </dt>
              <dd className="govuk-summary-list__value">
                <ul className="govuk-list dfe-admin">
                  {window.location.search === '?status=readyApproval' && (
                    <li>
                      <PrototypeDashboardRelease
                        title="Academic year,"
                        years="2018 to 2019"
                        tag="Ready to review"
                        review
                        lastEdited={new Date('2019-03-20 17:37')}
                        lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                        published={new Date('2019-09-20 09:30')}
                        nextRelease={new Date('2020-09-20 09:30')}
                        dataType="Revised"
                        showComments
                      />
                    </li>
                  )}
                  {window.location.search === '?status=editNewRelease' && (
                    <li>
                      <PrototypeDashboardRelease
                        title="Academic year,"
                        years="2018 to 2019"
                        tag="New release in progress"
                        editing={
                          window.location.search === '?status=editNewRelease'
                        }
                        isNew
                        lastEdited={new Date('2019-03-20 17:37')}
                        lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                        published={new Date('2019-09-24 09:30')}
                        nextRelease={new Date('2020-09-25 09:30')}
                        dataType="Provisional"
                      />
                    </li>
                  )}
                  <li>
                    <PrototypeDashboardRelease
                      title="Academic year,"
                      years="2017 to 2018"
                      tag={
                        window.location.search === '?status=editLiveRelease'
                          ? 'Editing in progress'
                          : ''
                      }
                      isLatest
                      editing={
                        window.location.search === '?status=editLiveRelease'
                      }
                      lastEdited={new Date('2018-03-20 17:37')}
                      lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                      published={new Date('2018-09-24 09:30')}
                      nextRelease={new Date('2019-09-23 09:30')}
                      dataType="Final"
                    />
                  </li>
                  <li>
                    <PrototypeDashboardRelease
                      title="Academic year,"
                      years="2016 to 2017"
                      isLive
                      editing={
                        window.location.search === '?status=editLiveRelease'
                      }
                      lastEdited={new Date('2018-03-20 14:23')}
                      lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                      published={new Date('2017-09-25 09:30')}
                      dataType="Final"
                    />
                  </li>
                  <li>
                    <PrototypeDashboardRelease
                      title="Academic year,"
                      years="2015 to 2016"
                      isLive
                      editing={
                        window.location.search === '?status=editLiveRelease'
                      }
                      lastEdited={new Date('2017-03-20 16:15')}
                      lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                      published={new Date('2016-03-26 09:30')}
                      dataType="Final"
                    />
                  </li>
                </ul>
              </dd>
            </div>
          </dl>
          <Link to="/prototypes/release-create-new" className="govuk-button">
            Create a new release
          </Link>
        </AccordionSection>
        <AccordionSection
          heading=" Pupil absence statistics and data for schools in England: autumn term"
          caption=""
        >
          <dl className="govuk-summary-list govuk-!-margin-bottom-0">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key  dfe-summary-list__key--small">
                Methodology
              </dt>
              <dd className="govuk-summary-list__value">
                <Link to="#">A guide to absence statistics</Link>
              </dd>
              <dd className="govuk-summary-list__actions">
                <Link to="/prototypes/publication-assign-methodology">
                  Edit methodology
                </Link>
              </dd>
            </div>
          </dl>
          <dl className="govuk-summary-list">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key dfe-summary-list__key--small">
                Releases
              </dt>
              <dd className="govuk-summary-list__value">
                <ul className="govuk-list">
                  <li>
                    <PrototypeDashboardRelease
                      title="Autumn term, academic year, "
                      years="2017 to 2018"
                      isLatest
                      editing={
                        window.location.search === '?status=editLiveRelease'
                      }
                      lastEdited={new Date('2019-03-20 17:37')}
                      lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                      published={new Date('2018-01-08 09:30')}
                      nextRelease={new Date('2019-01-07 17:37')}
                    />
                  </li>

                  <li>
                    <PrototypeDashboardRelease
                      title="Autumn term, academic year, "
                      years="2016 to 2017"
                      isLive
                      lastEdited={new Date('2019-04-24 16:55')}
                      lastEditor={{
                        id: 'me',
                        name: 'Ann Evans',
                        permissions: [],
                      }}
                      published={new Date('2017-01-06 09:30')}
                      lead="Ann Evans"
                    />
                  </li>
                </ul>
              </dd>
            </div>
          </dl>
          <Link to="/prototypes/release-create-new" className="govuk-button">
            Create a new release
          </Link>
        </AccordionSection>
        <AccordionSection
          heading="Pupil absence statistics and data for schools in England: autumn and
        spring terms"
          caption=""
        >
          <dl className="govuk-summary-list govuk-!-margin-bottom-0">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key  dfe-summary-list__key--small">
                Methodology
              </dt>
              <dd className="govuk-summary-list__value">
                <Link to="#">A guide to absence statistics</Link>
              </dd>
              <dd className="govuk-summary-list__actions">
                <Link to="/prototypes/publication-assign-methodology">
                  Edit methodology
                </Link>
              </dd>
            </div>
          </dl>
          <dl className="govuk-summary-list">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key dfe-summary-list__key--small">
                Releases
              </dt>
              <dd className="govuk-summary-list__value">
                <ul className="govuk-list">
                  <li>
                    <PrototypeDashboardRelease
                      title="Autumn and spring terms, academic year, "
                      years="2017 to 2018"
                      isLatest
                      editing={
                        window.location.search === '?status=editLiveRelease'
                      }
                      lastEdited={new Date('2019-03-20 17:37')}
                      lastEditor={{ id: 'me', name: 'me', permissions: [] }}
                      published={new Date('2019-09-24 09:30')}
                      nextRelease={new Date('2020-09-25 09:30')}
                    />
                  </li>

                  <li>
                    <PrototypeDashboardRelease
                      title="Autumn and spring terms, academic year, "
                      years="2016 to 2017"
                      editing
                      isLive
                      lastEdited={new Date('2017-09-23 16:55')}
                      lastEditor={{
                        id: 'me',
                        name: 'Ann Evans',
                        permissions: [],
                      }}
                      published={new Date('2019-03-20 09:30')}
                      lead="Ann Evans"
                    />
                  </li>
                </ul>
              </dd>
            </div>
          </dl>
          <Link to="/prototypes/release-create-new" className="govuk-button">
            Create new release
          </Link>
        </AccordionSection>
        {window.location.search === '?status=newPublication' && (
          <AccordionSection
            heading="Pupil absence statistics and data for schools in England: summer term"
            caption="New publication, requires release adding"
          >
            <dl className="govuk-summary-list">
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key  dfe-summary-list__key--small">
                  Methodology
                </dt>
                <dd className="govuk-summary-list__value">
                  No methodology available
                </dd>
                <dd className="govuk-summary-list__actions">
                  <Link to="/prototypes/publication-assign-methodology">
                    Add methodology
                  </Link>
                </dd>
              </div>
            </dl>
            <Link to="/prototypes/release-create-new" className="govuk-button">
              Create new release
            </Link>
          </AccordionSection>
        )}
      </Accordion>
    </>
  );
};

export default PrototypeAdminDashboardPublications;
