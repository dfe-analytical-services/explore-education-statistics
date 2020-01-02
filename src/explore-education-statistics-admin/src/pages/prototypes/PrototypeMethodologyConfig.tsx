import React, { useState } from 'react';
import Link from '@admin/components/Link';
import PrototypeMethodologyNavigation from './components/PrototypeMethodologyNavigation';
import PrototypePage from './components/PrototypePage';
import PrototypeMethodologySummary from './components/PrototypeMethodologyPageSummary';
import PrototypeMethodologyConfig from './components/PrototypeMethodologyPageConfig';

const PublicationConfigPage = () => {
  const sectionId = 'setup';

  const [editSetup, setEditSetup] = useState(false);

  return (
    <PrototypePage
      wide
      breadcrumbs={[{ text: 'Create new methodology', link: '#' }]}
    >
      <PrototypeMethodologyNavigation sectionId={sectionId} />

      {!editSetup && (
        <>
          <PrototypeMethodologySummary />
          <div className="dfe-align--right">
            <a
              href="#methodology-tabs"
              onClick={() => {
                setEditSetup(true);
              }}
            >
              Edit methodology summary
            </a>
          </div>

          <div className="govuk-!-margin-top-9 dfe-align--right">
            <Link to="/prototypes/methodology-edit">
              <span className="govuk-heading-m govuk-!-margin-bottom-0">
                Next step
              </span>
              Manage content
            </Link>
          </div>
        </>
      )}

      {editSetup && (
        <PrototypeMethodologyConfig
          title="Example statistics: methodology"
          sectionId={sectionId}
        />
      )}
    </PrototypePage>
  );
};

export default PublicationConfigPage;
