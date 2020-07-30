import PageTitle from '@admin/components/PageTitle';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React from 'react';

const PrototypePreReleasePage = () => {
  return (
    <PrototypePage
      breadcrumbs={[
        { name: 'Find statistics and data', link: '#' },
        { name: 'An example publication', link: '#' },
        { name: 'Pre release access list', link: '#' },
      ]}
    >
      <PageTitle
        title="An example publication"
        caption="Academic year 2018/19"
      />

      <h2 className="govuk-heading-m">Pre-release access list</h2>

      <h3 className="govuk-heading-s">Published 23 July 2020</h3>

      <div className="govuk-!-margin-top-9 govuk-!-margin-bottom-9">
        <p>
          Beside Department for Education (DfE) professional and production
          staff the following post holders are given pre-release access up to 24
          hours before release.
        </p>
        <ul>
          <li>Secretary of State, DfE</li>
          <li>Prime Minister</li>
        </ul>
      </div>
    </PrototypePage>
  );
};

export default PrototypePreReleasePage;
