import React from 'react';
import Tag from '@common/components/Tag';

const PageBanner = () => {
  return (
    <div className="govuk-phase-banner">
      <p className="govuk-phase-banner__content">
        <Tag className="govuk-phase-banner__content__tag" strong>
          Beta
        </Tag>

        <span className="govuk-phase-banner__text">
          This is a new service – your{' '}
          <a
            href="https://forms.office.com/Pages/ResponsePage.aspx?id=yXfS-grGoU2187O4s0qC-VQ56HAfKLpBrG0LxbfxbVdUQjVJQVdMOFlSMURGQ1kyMzRNWlpKN1NMVy4u"
            target="_blank"
            rel="noopener noreferrer"
          >
            feedback
          </a>{' '}
          will help us to improve it.
        </span>
      </p>
    </div>
  );
};

export default PageBanner;
