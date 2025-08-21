import Tag from '@common/components/Tag';
import React from 'react';

interface Props {
  url: string;
}

export default function PhaseBanner({ url }: Props) {
  return (
    <div className="govuk-phase-banner" role="region" aria-label="phase-banner">
      <p className="govuk-phase-banner__content">
        <Tag className="govuk-phase-banner__content__tag">Beta</Tag>

        <span className="govuk-phase-banner__text">
          This is a new service – your{' '}
          <a
            href={url}
            rel="noopener noreferrer nofollow"
            target="_blank"
            data-testid="banner-feedback-link"
          >
            feedback (opens in new tab)
          </a>{' '}
          will help us to improve it.
        </span>
      </p>
    </div>
  );
}
