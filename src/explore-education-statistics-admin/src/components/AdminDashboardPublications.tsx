import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import React from 'react';
import Link from '@admin/components/Link';
import DashboardReleaseList from '@admin/components/DashboardReleaseList';
import { Publication } from '@admin/services/publicationService';

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
      </Accordion>
    </>
  );
};

export default AdminDashboardPublications;
