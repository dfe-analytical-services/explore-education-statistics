import RelatedAside from '@common/components/RelatedAside';
import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
  heading?: string;
  id?: string;
}

const RelatedInformation = ({
  children,
  heading = 'Related information',
  id = 'related-information',
}: Props) => {
  return (
    <RelatedAside>
      <nav role="navigation" aria-labelledby={id}>
        <h2 className="govuk-heading-m" id={id}>
          {heading}
        </h2>
        {children}
      </nav>
    </RelatedAside>
  );
};

export default RelatedInformation;
