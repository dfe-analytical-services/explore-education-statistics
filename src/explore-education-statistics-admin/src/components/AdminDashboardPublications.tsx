import DashboardRelease from '@admin/components/DashboardRelease';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import React from 'react';
import Link from '@admin/components/Link';
import { Publication } from '@common/services/publicationService';
import DashboardReleaseList from '@admin/components/DashboardReleaseList';

export interface AdminDashboardPublicationsProps {
  publication: Publication;
}

const AdminDashboardPublications = ({
  publication,
}: AdminDashboardPublicationsProps) => {
  return (
    <>
      <h2 className="govuk-heading-l govuk-!-margin-bottom-0">
        {publication.topic.theme.title}, {publication.topic.title}
      </h2>
      <p className="govuk-body">
        Edit an existing release or create a new release for current
        publications.
      </p>
      <Link to="/prototypes/publication-create-new" className="govuk-button">
        Create a new publication
      </Link>
      <Accordion id="pupil-absence">
        <AccordionSection heading={publication.title} caption="">
          <dl className="govuk-summary-list govuk-!-margin-bottom-0">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key  dfe-summary-list__key--small">
                Methodology
              </dt>
              <dd className="govuk-summary-list__value">
                <Link to="/methodology/{publication.methodology.id}">
                  {publication.methodology.title}
                </Link>
              </dd>
              <dd className="govuk-summary-list__actions">
                <Link to="/prototypes/publication-assign-methodology">
                  Edit methodology
                </Link>
              </dd>
            </div>
          </dl>
          <DashboardReleaseList releases={publication.releases} />
        </AccordionSection>
        <AccordionSection
          heading="Pupil absence statistics and data for schools in England: autumn term"
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
                    <DashboardRelease
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
                    <DashboardRelease
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
                    <DashboardRelease
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
                    <DashboardRelease
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

export default AdminDashboardPublications;
