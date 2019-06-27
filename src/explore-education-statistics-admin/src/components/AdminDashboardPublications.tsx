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
    </>
  );
};

export default AdminDashboardPublications;
