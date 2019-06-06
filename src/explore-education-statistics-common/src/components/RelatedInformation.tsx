import RelatedItems from '@common/components/RelatedItems';
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
    <RelatedItems>
      <nav role="navigation" aria-labelledby={id}>
        <h2 className="govuk-heading-m" id={id}>
          {heading}
        </h2>
        {children}
      </nav>
    </RelatedItems>
  );
};

export default RelatedInformation;
