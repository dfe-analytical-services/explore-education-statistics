import RelatedContent from '@common/components/RelatedContent';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  heading?: string;
  id?: string;
}

export default function RelatedInformation({
  children,
  heading = 'Related information',
  id = 'related-information',
}: Props) {
  return (
    <RelatedContent>
      <nav role="navigation" aria-labelledby={id}>
        <h2 className="govuk-heading-m" id={id}>
          {heading}
        </h2>
        {children}
      </nav>
    </RelatedContent>
  );
}
