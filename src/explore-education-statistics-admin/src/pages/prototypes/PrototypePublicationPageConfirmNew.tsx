import React from 'react';
import RelatedInformation from '@common/components/RelatedInformation';
import Link from '@admin/components/Link';
import PrototypePage from './components/PrototypePage';
import PrototypePublicationConfig from './components/PrototypePublicationPageConfig';

const PublicationPage = () => {
  const sectionId = 'confirmPublication';
  return (
    <PrototypePage
      wide
      breadcrumbs={[
        {
          link: '/prototypes/admin-dashboard',
          text: 'Administrator dashboard',
        },
        { text: 'Create new publication', link: '#' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">
            <span className="govuk-caption-l">
              Pupils and schools / Pupil absence
            </span>{' '}
            Create new publication
          </h1>
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link
                  to="/prototypes/documentation/create-new-publication"
                  target="blank"
                >
                  Creating a new publication{' '}
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>
      <PrototypePublicationConfig sectionId={sectionId} />
    </PrototypePage>
  );
};

export default PublicationPage;
